using System.Globalization;
using System.Text;

namespace Icod.Expressions.Pcre;

/// <summary>Parses only implemented byte-mode constructs, retaining syntax order.</summary>
internal sealed partial class PcreByteParser {
	[Flags]
	private enum AsciiRestriction : byte {
		None = 0, BackslashD = 1, BackslashS = 2, BackslashW = 4, Posix = 8, PosixDigit = 16,
		All = BackslashD | BackslashS | BackslashW | Posix | PosixDigit
	}

	private const int MaximumPatternDepth = 256;
	private const int MaximumCaptureCount = 256;
	private const int MaximumRepeatMarks = 256;
	private readonly byte[] source;
	private readonly CancellationToken cancellationToken;
	private readonly bool utf8;
	private readonly bool ucp;
	private readonly PcreByteNewline newline;
	private readonly List<(int Number, int Offset)> backreferences = new();
	private int index;
	private int groupCount;
	private int repeatMarks;
	private bool dotAll;
	private bool caseless;
	private bool caselessRestricted;
	private bool duplicateNames;
	private bool multiline;
	private AsciiRestriction asciiRestrictions;
	private bool noAutoCapture;
	private bool ungreedy;
	private bool hasBranchReset;
	private bool hasGeneralLookbehinds;
	private ExtendedMode extended;

	internal PcreByteParser( byte[] source, bool caseless, bool asciiBsD,
		PcreByteStartSettings settings, CancellationToken cancellationToken ) {
		this.source = source;
		this.caseless = caseless;
		utf8 = settings.Utf8;
		ucp = settings.Ucp;
		asciiRestrictions = asciiBsD ? AsciiRestriction.BackslashD : AsciiRestriction.None;
		newline = settings.Newline;
		index = settings.BodyOffset;
		this.cancellationToken = cancellationToken;
	}

	internal int GroupCount => groupCount;

	private bool HasAsciiRestriction( AsciiRestriction restriction ) =>
		( asciiRestrictions & restriction ) != AsciiRestriction.None;

	internal PcreByteAlternatives Parse() {
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			SkipIgnoredPatternText();
			if ( !TryParseOptions( index, scoped: false ) ) {
				break;
			}
		}
		var root = ParseAlternatives();
		if ( index != source.Length ) {
			throw Invalid( index, "unmatched closing parenthesis" );
		}
		foreach ( var reference in backreferences ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( reference.Number > groupCount ) {
				throw Invalid( reference.Offset, "backreference has no capture group" );
			}
		}
		if ( hasNamedReferences ) { root = ResolveNamedReferences( root ); }
		if ( hasCaptureConditions ) { root = ResolveCaptureConditions( root ); }
		if ( hasSubroutines ) { root = ResolveSubroutines( root ); }
		if ( hasGeneralLookbehinds ) { root = ResolveLookbehindLengths( root ); }
		if ( hasSubroutines ) { FinishSubroutineTargets( root ); }
		return root;
	}

	private bool TryParseOptions( int openingOffset, bool scoped ) {
		if ( openingOffset + 2 >= source.Length || source[openingOffset] != (byte)'('
			|| source[openingOffset + 1] != (byte)'?' ) { return false; }
		var cursor = openingOffset + 2;
		var reset = source[cursor] == (byte)'^';
		if ( reset ) { cursor++; }
		var unsetting = false;
		var setI = false;
		var setR = false;
		var setJ = false;
		var setM = false;
		var setS = false;
		var setN = false;
		var setU = false;
		var setAscii = AsciiRestriction.None;
		var setX = false;
		var setXx = false;
		var unsetX = false;
		var unsetI = false;
		var unsetR = false;
		var unsetJ = false;
		var unsetM = false;
		var unsetS = false;
		var unsetN = false;
		var unsetU = false;
		var unsetAscii = AsciiRestriction.None;
		while ( cursor < source.Length && source[cursor] is not (byte)')' and not (byte)':' ) {
			cancellationToken.ThrowIfCancellationRequested();
			var value = source[cursor++];
			if ( value == (byte)'-' && !unsetting && !reset ) {
				unsetting = true;
				continue;
			}
			if ( value is not (byte)'i' and not (byte)'m' and not (byte)'s' and not (byte)'n' and not (byte)'U' and not (byte)'x' and not (byte)'a' and not (byte)'r' and not (byte)'J' ) { return false; }
			switch ( value ) {
				case (byte)'a':
					var restriction = cursor < source.Length ? source[cursor] switch {
						(byte)'D' => AsciiRestriction.BackslashD,
						(byte)'S' => AsciiRestriction.BackslashS,
						(byte)'W' => AsciiRestriction.BackslashW,
						(byte)'P' => AsciiRestriction.Posix | AsciiRestriction.PosixDigit,
						(byte)'T' => AsciiRestriction.PosixDigit,
						_ => AsciiRestriction.None
					} : AsciiRestriction.None;
					if ( restriction == AsciiRestriction.None ) { restriction = AsciiRestriction.All; }
					else { cursor++; }
					if ( unsetting ) { unsetAscii |= restriction; } else { setAscii |= restriction; }
					break;
				case (byte)'i': if ( unsetting ) { unsetI = true; } else { setI = true; } break;
				case (byte)'r': if ( unsetting ) { unsetR = true; } else { setR = true; } break;
				case (byte)'J': if ( unsetting ) { unsetJ = true; } else { setJ = true; } break;
				case (byte)'m': if ( unsetting ) { unsetM = true; } else { setM = true; } break;
				case (byte)'s': if ( unsetting ) { unsetS = true; } else { setS = true; } break;
				case (byte)'n': if ( unsetting ) { unsetN = true; } else { setN = true; } break;
				case (byte)'U': if ( unsetting ) { unsetU = true; } else { setU = true; } break;
				case (byte)'x':
					if ( unsetting ) { unsetX = true; } else { setX = true; }
					if ( cursor < source.Length && source[cursor] == (byte)'x' ) {
						cursor++;
						if ( !unsetting ) { setXx = true; }
					}
					break;
			}
		}
		if ( cursor >= source.Length
			|| source[cursor] != ( scoped ? (byte)':' : (byte)')' ) ) { return false; }
		index = cursor + 1;
		if ( reset ) {
			// PCRE's reset keeps J, U and ASCII restrictions.
			caseless = caselessRestricted = multiline = dotAll = noAutoCapture = false;
			extended = ExtendedMode.None;
		}
		caseless = ( caseless || setI ) && !unsetI;
		caselessRestricted = ( caselessRestricted || setR ) && !unsetR;
		duplicateNames = ( duplicateNames || setJ ) && !unsetJ;
		multiline = ( multiline || setM ) && !unsetM;
		dotAll = ( dotAll || setS ) && !unsetS;
		noAutoCapture = ( noAutoCapture || setN ) && !unsetN;
		ungreedy = ( ungreedy || setU ) && !unsetU;
		asciiRestrictions = ( asciiRestrictions | setAscii ) & ~unsetAscii;
		extended = unsetX ? ExtendedMode.None : setX ? setXx ? ExtendedMode.More : ExtendedMode.Extended : extended;
		if ( !scoped ) { SkipIgnoredPatternText(); }
		if ( !scoped && IsRepeatQuantifier( index ) ) {
			var repeatOffset = index;
			if ( source[index] == (byte)'{' ) { ParseCountedBounds( repeatOffset ); }
			throw Invalid( repeatOffset, "an option setting cannot be repeated" );
		}
		return true;
	}

	private bool TryConsumeQuoteDelimiter( ref bool quoting ) {
		if ( index + 1 >= source.Length || source[index] != (byte)'\\' ) { return false; }
		if ( source[index + 1] == (byte)'E' ) {
			index += 2;
			quoting = false;
			return true;
		}
		if ( !quoting && source[index + 1] == (byte)'Q' ) {
			index += 2;
			quoting = true;
			return true;
		}
		return false;
	}

	private PcreByteLiteral ParseQuotedLiteral() {
		var offset = index;
		if ( utf8 && source[index] >= 0x80 ) {
			PcreUtf8.TryRead( source, offset, out var scalar, out var width ); // Validated at compile entry.
			index += width;
			return new( scalar );
		}
		return new( source[index++] );
	}

	private PcreByteNode? ParseAtom( int depth, int assertionOpeningContext, out GroupFrame? group ) {
		group = null;
		var offset = index;
		var value = source[index++];
		if ( value == (byte)'(' ) { return ParseGroupOpening( offset, depth, assertionOpeningContext, out group ); }
		if ( value == (byte)'^' ) {
			return multiline ? new PcreByteLineAnchor( true ) : new PcreByteAnchor( true );
		}
		if ( value == (byte)'$' ) {
			return multiline ? new PcreByteLineAnchor( false ) : new PcreByteEndBeforeFinalLf();
		}
		if ( value == (byte)'.' ) {
			return CreateAnyCharacter( dotAll );
		}
		if ( value == (byte)'[' ) {
			if ( utf8 ) { return ParseUtf8Bracket( offset ); }
			return ParseBracket( offset );
		}
		if ( value == (byte)'\\' ) {
			var escaped = ParseEscape( offset );
			if ( escaped is PcreByteMatchStartReset && assertionOpeningContext >= 0 ) {
				throw Invalid( assertionOpeningContext, "a match-start reset is not allowed in an assertion" );
			}
			return escaped;
		}
		if ( value is (byte)'*' or (byte)'+' or (byte)'?' ) {
			throw Invalid( offset, "quantifier without an atom" );
		}
		if ( value == (byte)'{' && IsCountedQuantifier( offset ) ) {
			index = offset;
			ParseCountedBounds( offset ); // Preserve numeric errors before no-atom errors.
			throw Invalid( offset, "quantifier without an atom" );
		}
		if ( utf8 && value >= 0x80 ) {
			PcreUtf8.TryRead( source, offset, out var scalar, out var width ); // Validated at compile entry.
			index = offset + width;
			return new PcreByteLiteral( scalar );
		}
		return new PcreByteLiteral( value );
	}

	private PcreByteNode CreateAnyCharacter( bool includeNewline ) =>
		utf8 || newline == PcreByteNewline.CrLf || newline == PcreByteNewline.Any && includeNewline ? new PcreByteAny( includeNewline )
			: new PcreByteClass( includeNewline ? PcreByteSet.AllBytes
				: newline switch {
					PcreByteNewline.AnyCrLf => PcreByteSet.DotAnyCrLf,
					PcreByteNewline.Cr => PcreByteSet.DotCr,
					PcreByteNewline.Nul => PcreByteSet.DotNul,
					PcreByteNewline.Any => PcreByteSet.DotAny,
					_ => PcreByteSet.Dot
				} );

	private PcreByteScalarSet ParseUtf8Bracket( int openingOffset ) {
		var negated = ParseClassNegation();
		var quoting = false;
		var sourceTermCount = 0;
		var soleLiteral = false;
		var explicitNewline = false;
		var ranges = new List<PcreByteScalarRange>();
		var predicates = new List<PcreByteScalarPredicate>();
		while ( index < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			SkipClassTrivia( ref quoting );
			if ( index >= source.Length ) { break; }
			if ( !quoting && source[index] == (byte)']' && ( ranges.Count > 0 || predicates.Count > 0 ) ) {
				index++;
				return new( caseless ? PcreUtf8CaseFold.CloseRanges( ranges, cancellationToken, caselessRestricted )
					: ranges.ToArray(), predicates.ToArray(), negated,
					explicitNewline && !( negated && sourceTermCount == 1 && soleLiteral ) );
			}
			var first = quoting ? ParseQuotedLiteral() : ParseUtf8BracketItem( openingOffset );
			SkipClassTrivia( ref quoting );
			soleLiteral = first is PcreByteLiteral;
			if ( first is PcreByteLiteral { Value: '\r' or '\n' } ) { explicitNewline = true; }
			if ( TryStartBracketRange( ref quoting, out _ ) ) {
				if ( first is not PcreByteLiteral start ) {
					throw Invalid( openingOffset, "a character class cannot start a range" );
				}
				if ( index >= source.Length ) { throw Invalid( openingOffset, "unclosed bracket class" ); }
				var last = quoting ? ParseQuotedLiteral() : ParseUtf8BracketItem( openingOffset );
				SkipClassTrivia( ref quoting );
				if ( last is not PcreByteLiteral end || end.Value < start.Value ) {
					throw Invalid( openingOffset, "range endpoints must be ordered literal scalars" );
				}
				soleLiteral = start.Value == end.Value;
				if ( end.Value is '\r' or '\n' ) { explicitNewline = true; }
				ranges.Add( new( start.Value, end.Value ) );
			} else if ( first is PcreByteLiteral literal ) {
				ranges.Add( new( literal.Value, literal.Value ) );
			} else if ( first is PcreByteScalarClass scalarClass ) {
				predicates.Add( new( scalarClass.Kind, scalarClass.Negated, scalarClass.Category ) );
			} else { throw Unsupported( openingOffset ); }
			// Native metadata uses original operand shape, before folding/deduplication.
			// A duplicate literal or predicate prevents the negated-singleton shortcut.
			sourceTermCount++;
		}
		throw Invalid( openingOffset, "unclosed bracket class" );
	}

	private PcreByteNode ParseUtf8BracketItem( int openingOffset ) {
		var offset = index;
		var value = source[index++];
		if ( value == (byte)'[' && index < source.Length
			&& source[index] is (byte)':' or (byte)'.' or (byte)'='
			&& HasCompleteUtf8SpecialClassToken( index ) ) {
			if ( source[index] != (byte)':' ) { throw Unsupported( openingOffset ); }
			return ParseUtf8PosixItem( openingOffset );
		}
		if ( value == (byte)'\\' ) {
			if ( index < source.Length && source[index] == (byte)'b' ) {
				index++;
				return new PcreByteLiteral( 0x08 );
			}
			PcreByteNode escaped;
			try { escaped = ParseEscape( offset, inBracket: true ); }
			catch ( PcreByteParseException error ) when (
				error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax
			) { throw Unsupported( openingOffset ); }
			return escaped is PcreByteLiteral or PcreByteScalarClass
				? escaped : throw Unsupported( openingOffset );
		}
		if ( value >= 0x80 ) {
			PcreUtf8.TryRead( source, offset, out var scalar, out var width ); // Validated at compile entry.
			index = offset + width;
			return new PcreByteLiteral( scalar );
		}
		return new PcreByteLiteral( value );
	}

	private PcreByteScalarClass ParseUtf8PosixItem( int openingOffset ) {
		index++; // The ':' after '['.
		var negated = index < source.Length && source[index] == (byte)'^';
		if ( negated ) { index++; }
		var nameStart = index;
		while ( index < source.Length && source[index] is >= (byte)'a' and <= (byte)'z' ) {
			cancellationToken.ThrowIfCancellationRequested();
			index++;
		}
		if ( index == nameStart || index + 1 >= source.Length
			|| source[index] != (byte)':' || source[index + 1] != (byte)']' ) {
			throw Unsupported( openingOffset );
		}
		var name = Encoding.ASCII.GetString( source, nameStart, index - nameStart );
		var unicodePosix = ucp && !HasAsciiRestriction( AsciiRestriction.Posix );
		var unicodeDigits = unicodePosix && !HasAsciiRestriction( AsciiRestriction.PosixDigit );
		var kind = name switch {
			"alnum" => unicodePosix ? PcreByteScalarClassKind.UnicodeAlnum : PcreByteScalarClassKind.AsciiAlnum,
			"alpha" => unicodePosix ? PcreByteScalarClassKind.Letter : PcreByteScalarClassKind.AsciiAlpha,
			"ascii" => PcreByteScalarClassKind.Ascii,
			"blank" => unicodePosix ? PcreByteScalarClassKind.UnicodeBlank : PcreByteScalarClassKind.AsciiBlank,
			"cntrl" => unicodePosix ? PcreByteScalarClassKind.UnicodeControl : PcreByteScalarClassKind.AsciiControl,
			"digit" => unicodeDigits ? PcreByteScalarClassKind.DecimalDigit : PcreByteScalarClassKind.AsciiDigit,
			"graph" => unicodePosix ? PcreByteScalarClassKind.UnicodeGraph : PcreByteScalarClassKind.AsciiGraph,
			"lower" => unicodePosix ? PcreByteScalarClassKind.UnicodeLower : PcreByteScalarClassKind.AsciiLower,
			"print" => unicodePosix ? PcreByteScalarClassKind.UnicodePrintable : PcreByteScalarClassKind.AsciiPrintable,
			"punct" => unicodePosix ? PcreByteScalarClassKind.UnicodePunctuation : PcreByteScalarClassKind.AsciiPunctuation,
			"word" => unicodePosix ? PcreByteScalarClassKind.UnicodeWord : PcreByteScalarClassKind.AsciiWord,
			"space" => unicodePosix ? PcreByteScalarClassKind.UnicodeSpace : PcreByteScalarClassKind.AsciiSpace,
			"upper" => unicodePosix ? PcreByteScalarClassKind.UnicodeUpper : PcreByteScalarClassKind.AsciiUpper,
			"xdigit" => unicodeDigits ? PcreByteScalarClassKind.UnicodeHexDigit : PcreByteScalarClassKind.AsciiHexDigit,
			_ => throw Unsupported( openingOffset )
		};
		index += 2; // POSIX ':]' belongs to this member; the outer class has its own ']'.
		return new( kind, negated );
	}

	private bool HasCompleteUtf8SpecialClassToken( int markerOffset ) {
		var marker = source[markerOffset];
		for ( var scan = markerOffset + 1; scan < source.Length; scan++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( source[scan] == (byte)'\\' ) { scan++; continue; }
			if ( source[scan] == (byte)']' ) { return false; }
			if ( scan > markerOffset + 1 && source[scan] == marker
				&& scan + 1 < source.Length && source[scan + 1] == (byte)']' ) {
				return true;
			}
		}
		return false;
	}

	private PcreByteNode ParseBracket( int openingOffset ) {
		var negated = ParseClassNegation();
		var quoting = false;
		var hasItem = false;
		var sourceTermCount = 0;
		var soleLiteral = false;
		var explicitNewline = false;
		var set = default(PcreByteSet);
		var specialScanCursor = index;
		var nextClosingBracket = -1;
		while ( index < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			SkipClassTrivia( ref quoting );
			if ( index >= source.Length ) { break; }
			if ( !quoting && source[index] == (byte)']' && hasItem ) {
				index++;
				var singletonNegation = negated && sourceTermCount == 1 && soleLiteral;
				var members = negated ? set.Complement() : set;
				return new PcreByteClass( members, explicitNewline && !singletonNegation,
					singletonNegation || members == PcreByteSet.AllBytes );
			}
			var item = quoting ? QuotedBracketLiteral() : ParseBracketItem( ref specialScanCursor, ref nextClosingBracket );
			SkipClassTrivia( ref quoting );
			soleLiteral = item.Value.HasValue;
			if ( item.Value is (byte)'\r' or (byte)'\n' ) { explicitNewline = true; }
			if ( ( !item.Value.HasValue || item.MayStartRange ) && TryStartBracketRange( ref quoting, out var hyphenOffset ) ) {
				if ( !item.Value.HasValue ) {
					throw Invalid( hyphenOffset, "a character class cannot start a range" );
				}
				if ( index >= source.Length ) { throw Invalid( openingOffset, "unclosed bracket class" ); }
				var end = quoting ? QuotedBracketLiteral() : ParseBracketItem( ref specialScanCursor, ref nextClosingBracket );
				SkipClassTrivia( ref quoting );
				if ( !end.Value.HasValue || end.Value.Value < item.Value.Value ) {
					throw Invalid( hyphenOffset, "range endpoints must be ordered literal bytes" );
				}
				soleLiteral = item.Value.Value == end.Value.Value;
				if ( end.Value is (byte)'\r' or (byte)'\n' ) { explicitNewline = true; }
				// Preserve existing positive interior-range continuation until the
				// broader native start-bitmap/prefix behavior is implemented (#99).
				if ( !negated && ( item.Value.Value <= (byte)'\n' && end.Value.Value >= (byte)'\n'
					|| item.Value.Value <= (byte)'\r' && end.Value.Value >= (byte)'\r' ) ) {
					explicitNewline = true;
				}
				var range = PcreByteSet.Range( item.Value.Value, end.Value.Value );
				set = set.Union( caseless ? range.AsciiCaseClosure() : range );
			} else {
				set = set.Union( item.Set );
			}
			hasItem = true;
			sourceTermCount++;
		}
		throw Invalid( openingOffset, "unclosed bracket class" );
	}

	private (PcreByteSet Set, byte? Value, bool MayStartRange) QuotedBracketLiteral() {
		var value = source[index++];
		return (BracketLiteral( value ), value, true);
	}

	private (PcreByteSet Set, byte? Value, bool MayStartRange) ParseBracketItem(
		ref int specialScanCursor, ref int nextClosingBracket
	) {
		var offset = index;
		var value = source[index++];
		if ( value == (byte)'[' && index < source.Length
			&& ( source[index] is (byte)':' or (byte)'.' or (byte)'=' )
			&& HasCompleteSpecialClassToken( ref specialScanCursor, ref nextClosingBracket ) ) {
			return ParsePosixItem( offset );
		}
		if ( value == (byte)'\\' ) {
			if ( index < source.Length && source[index] == (byte)'b' ) {
				index++;
				return (PcreByteSet.Singleton( 0x08 ), 0x08, true);
			}
			var escape = ParseEscape( offset, inBracket: true );
			return escape switch {
				PcreByteLiteral literal => (BracketLiteral( (byte)literal.Value ), (byte)literal.Value, true),
				PcreByteClass characterClass => (characterClass.Set, null, false),
				_ => throw Unsupported( offset )
			};
		}
		return (BracketLiteral( value ), value, true);
	}

	private PcreByteSet BracketLiteral( byte value ) {
		var set = PcreByteSet.Singleton( value );
		return caseless ? set.AsciiCaseClosure() : set;
	}

	private bool HasCompleteSpecialClassToken( ref int scanCursor, ref int nextClosingBracket ) {
		var marker = source[index];
		while ( nextClosingBracket < index && scanCursor < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( source[scanCursor] == (byte)'\\' ) { scanCursor = Math.Min( scanCursor + 2, source.Length ); continue; }
			if ( source[scanCursor++] == (byte)']' ) { nextClosingBracket = scanCursor - 1; }
		}
		return nextClosingBracket > index + 1 && source[nextClosingBracket - 1] == marker;
	}

	private (PcreByteSet Set, byte? Value, bool MayStartRange) ParsePosixItem( int offset ) {
		if ( source[index] != (byte)':' ) { throw Unsupported( offset ); } // Collating/equivalence elements.
		index++;
		var negated = index < source.Length && source[index] == (byte)'^';
		if ( negated ) { index++; }
		var nameStart = index;
		while ( index < source.Length && source[index] is >= (byte)'a' and <= (byte)'z' ) {
			cancellationToken.ThrowIfCancellationRequested();
			index++;
		}
		if ( index == nameStart || index + 1 >= source.Length
			|| source[index] != (byte)':' || source[index + 1] != (byte)']' ) {
			throw Invalid( offset, "malformed POSIX character class" );
		}
		var name = System.Text.Encoding.ASCII.GetString( source, nameStart, index - nameStart );
		var set = name switch {
			"alnum" => PcreByteSet.Alnum,
			"alpha" => PcreByteSet.Alpha,
			"ascii" => PcreByteSet.Ascii,
			"blank" => PcreByteSet.Blank,
			"cntrl" => PcreByteSet.Control,
			"digit" => PcreByteSet.Digit,
			"graph" => PcreByteSet.Graph,
			"lower" => PcreByteSet.Lower,
			"print" => PcreByteSet.Printable,
			"punct" => PcreByteSet.Punctuation,
			"space" => PcreByteSet.WhiteSpace,
			"upper" => PcreByteSet.Upper,
			"word" => PcreByteSet.Word,
			"xdigit" => PcreByteSet.HexDigit,
			_ => throw Unsupported( offset )
		};
		index += 2; // The POSIX ':]' belongs to this item; the outer ']' is separate.
		if ( caseless ) { set = set.AsciiCaseClosure(); }
		return (negated ? set.Complement() : set, null, false);
	}

	private PcreByteNode ParseEscape( int offset, bool inBracket = false ) {
		if ( index >= source.Length ) {
			throw Invalid( offset, "trailing backslash" );
		}
		var value = source[index++];
		if ( value == (byte)'c' ) {
			if ( index >= source.Length || source[index] is < 0x20 or > 0x7e ) {
				throw Invalid( offset, "control escape requires a printable ASCII operand" );
			}
			var control = source[index++];
			if ( control is >= (byte)'a' and <= (byte)'z' ) { control -= 0x20; }
			return new PcreByteLiteral( control ^ 0x40 );
		}
		if ( value == (byte)'R' ) {
			if ( inBracket ) { throw Invalid( offset, "a newline sequence is not a character class member" ); }
			return new PcreByteNewlineSequence();
		}
		if ( value is (byte)'G' or (byte)'K' ) {
			if ( inBracket ) { throw Invalid( offset, "a match-position escape is not a character class member" ); }
			return value == (byte)'G' ? new PcreByteSearchStart() : new PcreByteMatchStartReset();
		}
		if ( value == (byte)'N' ) { return ParseNonNewlineEscape( offset, inBracket ); }
		if ( value == (byte)'A' ) { return new PcreByteAnchor( true ); }
		if ( value == (byte)'z' ) { return new PcreByteAnchor( false ); }
		if ( value == (byte)'Z' ) { return new PcreByteEndBeforeFinalLf(); }
		if ( value is (byte)'p' or (byte)'P' ) {
			return ParseUnicodeProperty( offset, value == (byte)'P' );
		}
		if ( value == (byte)'b' || value == (byte)'B' ) {
			return new PcreByteWordBoundary( value == (byte)'b', ucp && !HasAsciiRestriction( AsciiRestriction.BackslashW ) );
		}
		if ( value is >= (byte)'0' and <= (byte)'9' ) { return ParseDecimalOrOctalEscape( value, offset, inBracket ); }
		if ( value == (byte)'o' ) { return ParseBracedOctal( offset ); }
		if ( value == (byte)'k' ) {
			if ( inBracket ) { return new PcreByteLiteral( value ); }
			if ( index >= source.Length || source[index] is not (byte)'<' and not (byte)'\'' and not (byte)'{' ) {
				throw Invalid( offset, "named backreference requires a delimiter" );
			}
			var delimiter = source[index++];
			return ParseNamedReference( delimiter == (byte)'<' ? (byte)'>'
				: delimiter == (byte)'{' ? (byte)'}' : (byte)'\'', offset );
		}
		if ( value == (byte)'g' ) {
			if ( inBracket ) { return new PcreByteLiteral( value ); }
			if ( index < source.Length && source[index] is (byte)'<' or (byte)'\'' ) {
				var delimiter = source[index++];
				return ParseSubroutineCall( delimiter == (byte)'<' ? (byte)'>' : (byte)'\'', offset );
			}
			if ( index < source.Length && source[index] == (byte)'{' ) {
				var opening = index++;
				SkipBraceHorizontalSpace();
				if ( index < source.Length && source[index] is not (>= (byte)'0' and <= (byte)'9')
					and not (byte)'+' and not (byte)'-' ) {
					return ParseNamedReference( (byte)'}', offset );
				}
				index = opening;
			}
			return ParseNumericBackreference( offset );
		}
		if ( value == (byte)'x' ) {
			if ( index < source.Length && source[index] == (byte)'{' ) {
				return new PcreByteLiteral( ParseBracedHex( offset ) );
			}
			var result = 0;
			var count = 0;
			for ( ; count < 2 && index < source.Length; count++ ) {
				var digit = HexDigit( source[index] );
				if ( digit < 0 ) { break; }
				result = ( result << 4 ) | digit;
				index++;
			}
			if ( count == 0 ) { throw Invalid( offset, "hex escape requires at least one digit" ); }
			return new PcreByteLiteral( result );
		}
		if ( value is (byte)'h' or (byte)'H' or (byte)'v' or (byte)'V' ) {
			var horizontal = value is (byte)'h' or (byte)'H';
			var negated = value is (byte)'H' or (byte)'V';
			if ( utf8 ) {
				return new PcreByteScalarClass( horizontal ? PcreByteScalarClassKind.HorizontalSpace
					: PcreByteScalarClassKind.VerticalSpace, negated );
			}
			var set = horizontal ? PcreByteSet.HorizontalSpace : PcreByteSet.VerticalSpace;
			return new PcreByteClass( negated ? set.Complement() : set );
		}
		if ( utf8 && ( value is (byte)'d' or (byte)'D' ) ) {
			return new PcreByteScalarClass( ucp && !HasAsciiRestriction( AsciiRestriction.BackslashD )
				? PcreByteScalarClassKind.DecimalDigit : PcreByteScalarClassKind.AsciiDigit,
				value == (byte)'D' );
		}
		if ( utf8 && ( value is (byte)'w' or (byte)'W' ) ) {
			return new PcreByteScalarClass( ucp && !HasAsciiRestriction( AsciiRestriction.BackslashW ) ? PcreByteScalarClassKind.UnicodeWord
				: PcreByteScalarClassKind.AsciiWord, value == (byte)'W' );
		}
		if ( utf8 && ( value is (byte)'s' or (byte)'S' ) ) {
			return new PcreByteScalarClass( ucp && !HasAsciiRestriction( AsciiRestriction.BackslashS ) ? PcreByteScalarClassKind.UnicodeSpace
				: PcreByteScalarClassKind.AsciiSpace, value == (byte)'S' );
		}
		PcreByteSet? characterClass = value switch {
			(byte)'d' => PcreByteSet.Digit,
			(byte)'D' => PcreByteSet.Digit.Complement(),
			(byte)'s' => PcreByteSet.WhiteSpace,
			(byte)'S' => PcreByteSet.WhiteSpace.Complement(),
			(byte)'w' => PcreByteSet.Word,
			(byte)'W' => PcreByteSet.Word.Complement(),
			_ => (PcreByteSet?)null
		};
		if ( characterClass.HasValue ) {
			if ( utf8 ) { throw Unsupported( offset ); }
			return new PcreByteClass( characterClass.Value );
		}
		var escaped = value switch {
			(byte)'a' => 0x07,
			(byte)'e' => 0x1B,
			(byte)'f' => 0x0C,
			(byte)'n' => 0x0A,
			(byte)'r' => 0x0D,
			(byte)'t' => 0x09,
			_ => -1
		};
		if ( escaped >= 0 ) { return new PcreByteLiteral( (byte)escaped ); }
		if ( value is >= (byte)'0' and <= (byte)'9'
			or >= (byte)'A' and <= (byte)'Z'
			or >= (byte)'a' and <= (byte)'z' ) {
			throw Unsupported( offset );
		}
		if ( utf8 && value >= 0x80 ) { throw Unsupported( offset ); }
		return new PcreByteLiteral( value );
	}

	private PcreByteBackreference ParseNumericBackreference( int offset ) {
		var braced = index < source.Length && source[index] == (byte)'{';
		if ( braced ) { index++; SkipBraceHorizontalSpace(); }
		var sign = 0;
		if ( index < source.Length && source[index] is (byte)'+' or (byte)'-' ) {
			sign = source[index++] == (byte)'+' ? 1 : -1;
		}
		if ( index >= source.Length || source[index] is < (byte)'0' or > (byte)'9' ) {
			if ( sign != 0 || braced && index < source.Length && source[index] == (byte)'}' ) {
				throw Invalid( offset, "numeric backreference requires a group number" );
			}
			throw Unsupported( offset ); // Named and subroutine forms are pending.
		}
		var magnitude = 0;
		do {
			cancellationToken.ThrowIfCancellationRequested();
			var digit = source[index++] - (byte)'0';
			if ( magnitude > ( MaximumCaptureCount - digit ) / 10 ) {
				throw Invalid( offset, "backreference has no capture group" );
			}
			magnitude = magnitude * 10 + digit;
		} while ( index < source.Length && source[index] is >= (byte)'0' and <= (byte)'9' );
		if ( braced ) {
			SkipBraceHorizontalSpace();
			if ( index >= source.Length || source[index] != (byte)'}' ) {
				throw Invalid( offset, "unclosed numeric backreference" );
			}
			index++;
		}
		if ( magnitude == 0 ) { throw Invalid( offset, "backreference must be positive" ); }
		var number = sign switch {
			-1 => groupCount + 1 - magnitude,
			1 => groupCount + magnitude,
			_ => magnitude
		};
		if ( number <= 0 || number > MaximumCaptureCount ) {
			throw Invalid( offset, "backreference has no capture group" );
		}
		backreferences.Add( (number, offset) );
		return new( number );
	}

	private PcreByteNode ParseNonNewlineEscape( int offset, bool inBracket ) {
		if ( index < source.Length && source[index] == (byte)'{' ) {
			var opening = index++;
			SkipBraceHorizontalSpace();
			if ( source.AsSpan( index ).StartsWith( "U+"u8 ) ) {
				if ( !utf8 ) { throw Invalid( offset, "a Unicode codepoint escape requires UTF mode" ); }
				index += 2;
				return new PcreByteLiteral( ParseBracedHexValue( offset ) );
			}
			index = opening;
			if ( inBracket ) { throw Invalid( offset, "a non-newline escape is not a character class member" ); }
			// Validate braces here, but leave valid counts to the normal quantifier path.
			try { ParseCountedBounds( opening ); }
			catch ( PcreByteParseException error ) when ( error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax ) {
				throw Invalid( offset, "non-newline braces require a count or a Unicode codepoint" );
			} finally { index = opening; }
		}
		if ( inBracket ) { throw Invalid( offset, "a non-newline escape is not a character class member" ); }
		return CreateAnyCharacter( false );
	}

	private int ParseBracedHex( int offset ) {
		index++; // Opening brace.
		SkipBraceHorizontalSpace();
		return ParseBracedHexValue( offset );
	}

	private int ParseBracedHexValue( int offset ) => ParseBracedCharacterValue( offset, 16, "hex" );

	private int ParseBracedCharacterValue( int offset, int radix, string kind ) {
		var maximum = utf8 ? 0x10FFFF : 0xFF;
		var scalar = 0;
		var hasDigits = false;
		while ( index < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			var digit = HexDigit( source[index] );
			if ( digit < 0 || digit >= radix ) { break; }
			if ( scalar > ( maximum - digit ) / radix ) {
				throw Invalid( offset, $"{kind} escape exceeds the supported character range" );
			}
			scalar = scalar * radix + digit;
			index++;
			hasDigits = true;
		}
		if ( !hasDigits ) { throw Invalid( offset, $"{kind} escape requires at least one digit" ); }
		SkipBraceHorizontalSpace();
		if ( index >= source.Length || source[index] != (byte)'}' ) {
			throw Invalid( offset, $"unclosed {kind} escape or non{kind} character" );
		}
		index++;
		if ( utf8 && scalar is >= 0xD800 and <= 0xDFFF ) {
			throw Invalid( offset, $"{kind} escape is a surrogate code point" );
		}
		return scalar;
	}

	private PcreByteNode ParseUnicodeProperty( int offset, bool negated ) {
		if ( index >= source.Length ) { throw Invalid( offset, "property escape requires a name" ); }
		if ( source[index] != (byte)'{' ) {
			var shortName = NormalizePropertyName( source.AsSpan( index++, 1 ) );
			return CreatePropertyClass( GeneralCategory( shortName, negated ) ?? throw Unsupported( offset ) );
		}
		index++;
		var nameStart = index;
		while ( index < source.Length && source[index] != (byte)'}' ) {
			cancellationToken.ThrowIfCancellationRequested();
			index++;
		}
		if ( index >= source.Length || index == nameStart ) {
			throw Invalid( offset, "malformed property escape" );
		}
		var name = NormalizePropertyName( source.AsSpan( nameStart, index - nameStart ) );
		index++;
		if ( name.StartsWith( '^' ) ) { negated = !negated; name = name[1..]; }
		if ( name.Length == 0 ) { throw Invalid( offset, "property escape requires a name" ); }
		var categoryOnly = false;
		if ( name.StartsWith( "GC:" ) || name.StartsWith( "GC=" ) ) { name = name[3..]; categoryOnly = true; }
		else if ( name.StartsWith( "GENERALCATEGORY:" ) || name.StartsWith( "GENERALCATEGORY=" ) ) {
			name = name[16..];
			categoryOnly = true;
		}
		if ( name.Length == 0 ) { throw Invalid( offset, "property escape requires a name" ); }
		return CreatePropertyClass( GeneralCategory( name, negated ) ?? ( categoryOnly ? null : SpecialProperty( name, negated ) )
			?? throw Unsupported( offset ) );
	}

	private PcreByteNode CreatePropertyClass( PcreByteScalarClass property ) {
		if ( utf8 ) { return property; }
		var members = PcreByteSet.FromPredicate( value => {
			cancellationToken.ThrowIfCancellationRequested();
			return PcreScalarClassMembership.Contains( property.Kind, value, property.Category, caseless ) != property.Negated;
		} );
		return new PcreByteClass( members );
	}

	private static PcreByteScalarClass? SpecialProperty( string name, bool negated ) {
		PcreByteScalarClassKind? kind = name switch {
			"ANY" => PcreByteScalarClassKind.AnyScalar,
			"XAN" => PcreByteScalarClassKind.UnicodeAlnum,
			"XPS" or "XSP" => PcreByteScalarClassKind.UnicodeSpace,
			"XWD" => PcreByteScalarClassKind.UnicodeWord,
			"XUC" => PcreByteScalarClassKind.UniversalCharacterName,
			_ => null
		};
		return kind.HasValue ? new( kind.Value, negated ) : null;
	}

	private static string NormalizePropertyName( ReadOnlySpan<byte> sourceName ) {
		var normalized = new StringBuilder( sourceName.Length );
		foreach ( var value in sourceName ) {
			if ( value is >= 0x09 and <= 0x0D or (byte)' ' or (byte)'-' or (byte)'_' ) { continue; }
			normalized.Append( (char)( value is >= (byte)'a' and <= (byte)'z' ? value - 32 : value ) );
		}
		return normalized.ToString();
	}

	private static PcreByteScalarClass? GeneralCategory( string name, bool negated ) {
		UnicodeCategory? specific = name switch {
			"CC" => UnicodeCategory.Control,
			"CF" => UnicodeCategory.Format,
			"CN" => UnicodeCategory.OtherNotAssigned,
			"CO" => UnicodeCategory.PrivateUse,
			"CS" => UnicodeCategory.Surrogate,
			"LL" => UnicodeCategory.LowercaseLetter,
			"LM" => UnicodeCategory.ModifierLetter,
			"LO" => UnicodeCategory.OtherLetter,
			"LT" => UnicodeCategory.TitlecaseLetter,
			"LU" => UnicodeCategory.UppercaseLetter,
			"MC" => UnicodeCategory.SpacingCombiningMark,
			"ME" => UnicodeCategory.EnclosingMark,
			"MN" => UnicodeCategory.NonSpacingMark,
			"ND" => UnicodeCategory.DecimalDigitNumber,
			"NL" => UnicodeCategory.LetterNumber,
			"NO" => UnicodeCategory.OtherNumber,
			"PC" => UnicodeCategory.ConnectorPunctuation,
			"PD" => UnicodeCategory.DashPunctuation,
			"PE" => UnicodeCategory.ClosePunctuation,
			"PF" => UnicodeCategory.FinalQuotePunctuation,
			"PI" => UnicodeCategory.InitialQuotePunctuation,
			"PO" => UnicodeCategory.OtherPunctuation,
			"PS" => UnicodeCategory.OpenPunctuation,
			"SC" => UnicodeCategory.CurrencySymbol,
			"SK" => UnicodeCategory.ModifierSymbol,
			"SM" => UnicodeCategory.MathSymbol,
			"SO" => UnicodeCategory.OtherSymbol,
			"ZL" => UnicodeCategory.LineSeparator,
			"ZP" => UnicodeCategory.ParagraphSeparator,
			"ZS" => UnicodeCategory.SpaceSeparator,
			_ => null
		};
		if ( specific.HasValue ) {
			return new( PcreByteScalarClassKind.SpecificCategory, negated, specific );
		}
		PcreByteScalarClassKind? group = name switch {
			"C" => PcreByteScalarClassKind.CategoryOther,
			"L" => PcreByteScalarClassKind.Letter,
			"LC" or "L&" => PcreByteScalarClassKind.CasedLetter,
			"M" => PcreByteScalarClassKind.CategoryMark,
			"N" => PcreByteScalarClassKind.CategoryNumber,
			"P" => PcreByteScalarClassKind.CategoryPunctuation,
			"S" => PcreByteScalarClassKind.CategorySymbol,
			"Z" => PcreByteScalarClassKind.CategorySeparator,
			_ => null
		};
		return group.HasValue ? new( group.Value, negated ) : null;
	}

	private static int HexDigit( byte value ) => value switch {
		>= (byte)'0' and <= (byte)'9' => value - (byte)'0',
		>= (byte)'A' and <= (byte)'F' => value - (byte)'A' + 10,
		>= (byte)'a' and <= (byte)'f' => value - (byte)'a' + 10,
		_ => -1
	};

	private static PcreByteParseException Invalid( int offset, string message ) => new(
		new( PcreByteDiagnosticCode.InvalidSyntax, offset, message )
	);

	private static PcreByteParseException Unsupported( int offset ) => new(
		new( PcreByteDiagnosticCode.UnsupportedSyntax, offset, "PCRE syntax is not implemented by the internal byte foundation" )
	);
}

internal sealed class PcreByteParseException( PcreByteDiagnostic diagnostic ) : Exception( diagnostic.Message ) {
	internal PcreByteDiagnostic Diagnostic { get; } = diagnostic;
}
