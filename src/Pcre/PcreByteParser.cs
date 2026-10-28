using System.Globalization;
using System.Text;

namespace Icod.Expressions.Pcre;

/// <summary>Parses only implemented byte-mode constructs, retaining syntax order.</summary>
internal sealed partial class PcreByteParser {
	private const int MaximumPatternDepth = 256;
	private const int MaximumCaptureCount = 256;
	private const int MaximumRepeatMarks = 256;
	private readonly byte[] source;
	private readonly CancellationToken cancellationToken;
	private readonly bool utf8;
	private readonly bool ucp;
	private readonly bool anyCrLfNewline;
	private readonly List<(int Number, int Offset)> backreferences = new();
	private int index;
	private int groupCount;
	private int repeatMarks;
	private bool dotAll;
	private bool caseless;
	private bool multiline;
	private bool asciiBsD;

	internal PcreByteParser( byte[] source, bool caseless, bool utf8, bool ucp,
		bool asciiBsD, bool anyCrLfNewline, CancellationToken cancellationToken ) {
		this.source = source;
		this.caseless = caseless;
		this.utf8 = utf8;
		this.ucp = ucp;
		this.asciiBsD = asciiBsD;
		this.anyCrLfNewline = anyCrLfNewline;
		this.cancellationToken = cancellationToken;
	}

	internal int GroupCount => groupCount;
	internal bool Caseless => caseless;

	internal PcreByteAlternatives Parse() {
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( utf8 && source.AsSpan( index ).StartsWith( "(?-aD)"u8 ) ) {
				index += 6;
				asciiBsD = false;
			} else if ( utf8 && source.AsSpan( index ).StartsWith( "(?aD)"u8 ) ) {
				index += 5;
				asciiBsD = true;
			} else if ( !TryParseLeadingOptions() ) {
				break;
			}
		}
		var root = ParseAlternatives( false, 0 );
		if ( index != source.Length ) {
			throw Invalid( index, "unmatched closing parenthesis" );
		}
		foreach ( var reference in backreferences ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( reference.Number > groupCount ) {
				throw Invalid( reference.Offset, "backreference has no capture group" );
			}
		}
		return hasNamedReferences ? ResolveNamedReferences( root ) : root;
	}

	private bool TryParseLeadingOptions() {
		if ( index + 3 >= source.Length || source[index] != (byte)'('
			|| source[index + 1] != (byte)'?' ) { return false; }
		var cursor = index + 2;
		var unsetting = false;
		var sawFlag = false;
		var sawUnsetFlag = false;
		var setI = false;
		var setM = false;
		var setS = false;
		var unsetI = false;
		var unsetM = false;
		var unsetS = false;
		while ( cursor < source.Length && source[cursor] != (byte)')' ) {
			cancellationToken.ThrowIfCancellationRequested();
			var value = source[cursor++];
			if ( value == (byte)'-' && !unsetting ) {
				unsetting = true;
				continue;
			}
			if ( value is not (byte)'i' and not (byte)'m' and not (byte)'s' ) { return false; }
			sawFlag = true;
			if ( unsetting ) { sawUnsetFlag = true; }
			switch ( value ) {
				case (byte)'i': if ( unsetting ) { unsetI = true; } else { setI = true; } break;
				case (byte)'m': if ( unsetting ) { unsetM = true; } else { setM = true; } break;
				case (byte)'s': if ( unsetting ) { unsetS = true; } else { setS = true; } break;
			}
		}
		if ( cursor >= source.Length || !sawFlag || unsetting && !sawUnsetFlag ) { return false; }
		index = cursor + 1;
		caseless = ( caseless || setI ) && !unsetI;
		multiline = ( multiline || setM ) && !unsetM;
		dotAll = ( dotAll || setS ) && !unsetS;
		return true;
	}

	private PcreByteAlternatives ParseAlternatives( bool inGroup, int depth, bool branchReset = false ) {
		if ( depth > MaximumPatternDepth ) {
			throw new PcreByteParseException( new(
				PcreByteDiagnosticCode.PatternDepthLimitExceeded, index, "pattern nesting limit exceeded"
			) );
		}
		var entryGroupCount = groupCount;
		var maximumGroupCount = groupCount;
		var branches = new List<IReadOnlyList<PcreByteNode>>();
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			var nodes = new List<PcreByteNode>();
			var quoting = false;
			while ( index < source.Length ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( TryConsumeQuoteDelimiter( ref quoting ) ) { continue; }
				if ( !quoting && source[index] is (byte)'|' or (byte)')' ) { break; }
				var atom = quoting ? ParseQuotedLiteral() : ParseAtom( depth );
				while ( TryConsumeQuoteDelimiter( ref quoting ) ) { }
				if ( !quoting && index < source.Length && source[index] is (byte)'*' or (byte)'+' or (byte)'?' or (byte)'{' ) {
					if ( atom is PcreByteLiteralAssertion ) {
						throw Unsupported( index );
					}
					if ( atom is PcreByteMatchStartReset ) {
						throw Invalid( index, "a match-start reset cannot be repeated" );
					}
					if ( atom is PcreByteAnchor or PcreByteSearchStart or PcreByteLineAnchor or PcreByteEndBeforeFinalLf or PcreByteWordBoundary ) {
						throw Invalid( index, "an anchor cannot be repeated" );
					}
					var quantifierOffset = index;
					var counted = source[index] == (byte)'{';
					var quantifier = counted ? (byte)0 : source[index++];
					var bounds = counted ? ParseCountedBounds( quantifierOffset )
						: (quantifier == (byte)'+' ? 1 : 0,
							quantifier == (byte)'?' ? 1 : int.MaxValue);
					var minimum = bounds.Item1;
					var maximum = bounds.Item2;
					repeatMarks += counted ? maximum == int.MaxValue ? 1 : 0
						: quantifier == (byte)'+' ? 2 : quantifier == (byte)'*' ? 1 : 0;
					if ( repeatMarks > MaximumRepeatMarks ) {
						throw new PcreByteParseException( new(
							PcreByteDiagnosticCode.PatternRepeatLimitExceeded, quantifierOffset, "repeat state limit exceeded"
						) );
					}
					var greedy = index >= source.Length || source[index] != (byte)'?';
					if ( !greedy ) { index++; }
					atom = new PcreByteRepeat( atom, minimum, maximum, greedy,
						counted ? quantifierOffset : -1 );
					if ( greedy && index < source.Length && source[index] == (byte)'+' ) {
						index++;
						atom = new PcreByteAtomic( atom );
					}
					if ( index < source.Length && source[index] is (byte)'*' or (byte)'+' or (byte)'?' or (byte)'{' ) {
						throw Invalid( index, "repeated quantifier" );
					}
				}
				nodes.Add( atom );
			}
			branches.Add( nodes );
			if ( branchReset ) { maximumGroupCount = Math.Max( maximumGroupCount, groupCount ); }
			if ( index >= source.Length || source[index] == (byte)')' ) {
				if ( inGroup && index >= source.Length ) {
					throw Invalid( source.Length, "unclosed group" );
				}
				if ( branchReset ) { groupCount = maximumGroupCount; }
				return new( branches );
			}
			index++; // Ordered alternative.
			if ( branchReset ) { groupCount = entryGroupCount; }
		}
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

	private (int Minimum, int Maximum) ParseCountedBounds( int offset ) {
		index++; // Opening brace.
		SkipBraceHorizontalSpace();
		var hasMinimum = ReadCount( offset, out var minimum );
		SkipBraceHorizontalSpace();
		var comma = index < source.Length && source[index] == (byte)',';
		var hasMaximum = false;
		var maximum = minimum;
		if ( comma ) {
			index++;
			SkipBraceHorizontalSpace();
			hasMaximum = ReadCount( offset, out maximum );
			SkipBraceHorizontalSpace();
			if ( !hasMinimum && !hasMaximum ) { throw Unsupported( offset ); }
			if ( !hasMinimum ) { minimum = 0; }
			if ( !hasMaximum ) { maximum = int.MaxValue; }
		} else if ( !hasMinimum ) { throw Unsupported( offset ); }
		if ( index >= source.Length || source[index] != (byte)'}' ) {
			throw Invalid( offset, "unclosed counted quantifier" );
		}
		index++;
		if ( maximum < minimum ) { throw Invalid( offset, "repeat maximum is less than minimum" ); }
		return (minimum, maximum);
	}

	private void SkipBraceHorizontalSpace() {
		while ( index < source.Length && source[index] is (byte)' ' or (byte)'\t' ) {
			cancellationToken.ThrowIfCancellationRequested();
			index++;
		}
	}

	private bool ReadCount( int offset, out int count ) {
		count = 0;
		var start = index;
		while ( index < source.Length && source[index] is >= (byte)'0' and <= (byte)'9' ) {
			cancellationToken.ThrowIfCancellationRequested();
			var digit = source[index++] - (byte)'0';
			if ( count > ( 65535 - digit ) / 10 ) {
				throw Invalid( offset, "repeat count exceeds 65535" );
			}
			count = count * 10 + digit;
		}
		return index > start;
	}

	private PcreByteNode ParseAtom( int depth ) {
		var offset = index;
		var value = source[index++];
		if ( value == (byte)'(' ) {
			// PCRE2's (*...) directives and aliases are pending, not repeated empty groups.
			if ( index < source.Length && source[index] == (byte)'*'
				&& source.AsSpan( index + 1 ).IndexOf( (byte)')' ) >= 0 ) {
				throw Unsupported( offset );
			}
			var capturing = true;
			var atomic = false;
			var branchReset = false;
			string? name = null;
			if ( index < source.Length && source[index] == (byte)'?' ) {
				if ( index + 1 < source.Length && source[index + 1] == (byte)':' ) {
					index += 2;
					capturing = false;
				} else if ( index + 1 < source.Length && source[index + 1] == (byte)'|' ) {
					index += 2;
					capturing = false;
					branchReset = true;
				} else if ( index + 1 < source.Length && source[index + 1] == (byte)'>' ) {
					index += 2;
					capturing = false;
					atomic = true;
				} else if ( source.AsSpan( index ).StartsWith( "?P="u8 ) ) {
					index += 3;
					return ParseNamedReference( (byte)')', offset );
				} else if ( source.AsSpan( index ).StartsWith( "?P<"u8 ) ) {
					index += 3;
					name = ParseCaptureName( (byte)'>', offset );
				} else if ( source.AsSpan( index ).StartsWith( "?'"u8 ) ) {
					index += 2;
					name = ParseCaptureName( (byte)'\'', offset );
				} else if ( index + 1 < source.Length && source[index + 1] == (byte)'<'
					&& ( index + 2 >= source.Length || source[index + 2] is not (byte)'=' and not (byte)'!' and not (byte)'*' ) ) {
					index += 2;
					name = ParseCaptureName( (byte)'>', offset );
				} else {
					var behind = index + 2 < source.Length && source[index + 1] == (byte)'<'
						&& source[index + 2] is (byte)'=' or (byte)'!';
					var ahead = index + 1 < source.Length && source[index + 1] is (byte)'=' or (byte)'!';
					if ( !behind && !ahead ) { throw Unsupported( offset ); }
					if ( depth >= MaximumPatternDepth ) {
						throw new PcreByteParseException( new(
							PcreByteDiagnosticCode.PatternDepthLimitExceeded, offset, "pattern nesting limit exceeded"
						) );
					}
					var positive = source[index + (behind ? 2 : 1)] == (byte)'=';
					index += behind ? 3 : 2;
					return ParseLiteralAssertion( offset, positive, behind );
				}
			}
			if ( depth >= MaximumPatternDepth ) {
				throw new PcreByteParseException( new(
					PcreByteDiagnosticCode.PatternDepthLimitExceeded, offset, "pattern nesting limit exceeded"
				) );
			}
			var number = capturing ? ++groupCount : 0;
			if ( groupCount > MaximumCaptureCount ) {
				throw new PcreByteParseException( new(
					PcreByteDiagnosticCode.PatternCaptureLimitExceeded, offset, "capture count limit exceeded"
				) );
			}
			if ( name is not null ) { RegisterCaptureName( name, number, offset ); }
			PcreByteAlternatives body;
			try {
				body = ParseAlternatives( true, depth + 1, branchReset );
			} catch ( PcreByteParseException error ) when ( error.Diagnostic.Code == PcreByteDiagnosticCode.InvalidSyntax && error.Diagnostic.Message == "unclosed group" ) {
				throw Invalid( offset, "unclosed group" );
			}
			if ( index >= source.Length || source[index] != (byte)')' ) {
				throw Invalid( offset, "unclosed group" );
			}
			index++;
			return capturing ? new PcreByteGroup( number, body )
				: atomic ? new PcreByteAtomic( new PcreByteNoncapturingGroup( body ) )
				: new PcreByteNoncapturingGroup( body );
		}
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
			return ParseEscape( offset );
		}
		if ( value is (byte)'*' or (byte)'+' or (byte)'?' ) {
			throw Invalid( offset, "quantifier without an atom" );
		}
		if ( value is (byte)'{' or (byte)'}' ) {
			throw Unsupported( offset );
		}
		if ( utf8 && value >= 0x80 ) {
			PcreUtf8.TryRead( source, offset, out var scalar, out var width ); // Validated at compile entry.
			index = offset + width;
			return new PcreByteLiteral( scalar );
		}
		return new PcreByteLiteral( value );
	}

	private PcreByteNode CreateAnyCharacter( bool includeNewline ) =>
		utf8 ? new PcreByteAny( includeNewline )
			: new PcreByteClass( includeNewline ? PcreByteSet.AllBytes
				: anyCrLfNewline ? PcreByteSet.DotAnyCrLf : PcreByteSet.Dot );

	private PcreByteScalarSet ParseUtf8Bracket( int openingOffset ) {
		var negated = index < source.Length && source[index] == (byte)'^';
		if ( negated ) { index++; }
		var quoting = false;
		var ranges = new List<PcreByteScalarRange>();
		var predicates = new List<PcreByteScalarPredicate>();
		while ( index < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( TryConsumeQuoteDelimiter( ref quoting ) ) { continue; }
			if ( !quoting && source[index] == (byte)']' && ( ranges.Count > 0 || predicates.Count > 0 ) ) {
				index++;
				return new( caseless ? PcreUtf8CaseFold.CloseRanges( ranges, cancellationToken )
					: ranges.ToArray(), predicates.ToArray(), negated );
			}
			var first = quoting ? ParseQuotedLiteral() : ParseUtf8BracketItem( openingOffset );
			while ( TryConsumeQuoteDelimiter( ref quoting ) ) { }
			if ( !quoting && index + 1 < source.Length && source[index] == (byte)'-'
				&& source[index + 1] != (byte)']' ) {
				if ( first is not PcreByteLiteral start ) {
					throw Invalid( openingOffset, "a character class cannot start a range" );
				}
				index++;
				while ( TryConsumeQuoteDelimiter( ref quoting ) ) { }
				if ( index >= source.Length ) { throw Invalid( openingOffset, "unclosed bracket class" ); }
				var last = quoting ? ParseQuotedLiteral() : ParseUtf8BracketItem( openingOffset );
				while ( TryConsumeQuoteDelimiter( ref quoting ) ) { }
				if ( last is not PcreByteLiteral end || end.Value < start.Value ) {
					throw Invalid( openingOffset, "range endpoints must be ordered literal scalars" );
				}
				ranges.Add( new( start.Value, end.Value ) );
			} else if ( first is PcreByteLiteral literal ) {
				ranges.Add( new( literal.Value, literal.Value ) );
			} else if ( first is PcreByteScalarClass scalarClass ) {
				predicates.Add( new( scalarClass.Kind, scalarClass.Negated, scalarClass.Category ) );
			} else { throw Unsupported( openingOffset ); }
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
		var kind = name switch {
			"alnum" => ucp ? PcreByteScalarClassKind.UnicodeAlnum : PcreByteScalarClassKind.AsciiAlnum,
			"alpha" => ucp ? PcreByteScalarClassKind.Letter : PcreByteScalarClassKind.AsciiAlpha,
			"ascii" => PcreByteScalarClassKind.Ascii,
			"blank" => ucp ? PcreByteScalarClassKind.UnicodeBlank : PcreByteScalarClassKind.AsciiBlank,
			"cntrl" => ucp ? PcreByteScalarClassKind.UnicodeControl : PcreByteScalarClassKind.AsciiControl,
			"digit" => ucp ? PcreByteScalarClassKind.DecimalDigit : PcreByteScalarClassKind.AsciiDigit,
			"graph" => ucp ? PcreByteScalarClassKind.UnicodeGraph : PcreByteScalarClassKind.AsciiGraph,
			"lower" => ucp ? PcreByteScalarClassKind.UnicodeLower : PcreByteScalarClassKind.AsciiLower,
			"print" => ucp ? PcreByteScalarClassKind.UnicodePrintable : PcreByteScalarClassKind.AsciiPrintable,
			"punct" => ucp ? PcreByteScalarClassKind.UnicodePunctuation : PcreByteScalarClassKind.AsciiPunctuation,
			"word" => ucp ? PcreByteScalarClassKind.UnicodeWord : PcreByteScalarClassKind.AsciiWord,
			"space" => ucp ? PcreByteScalarClassKind.UnicodeSpace : PcreByteScalarClassKind.AsciiSpace,
			"upper" => ucp ? PcreByteScalarClassKind.UnicodeUpper : PcreByteScalarClassKind.AsciiUpper,
			"xdigit" => ucp ? PcreByteScalarClassKind.UnicodeHexDigit : PcreByteScalarClassKind.AsciiHexDigit,
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

	private PcreByteLiteralAssertion ParseLiteralAssertion( int openingOffset, bool positive, bool behind ) {
		var bytes = new List<byte>();
		Span<byte> encoded = stackalloc byte[4];
		var quoting = false;
		while ( index < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( TryConsumeQuoteDelimiter( ref quoting ) ) { continue; }
			if ( !quoting && source[index] == (byte)')' ) {
				index++;
				return new( bytes.ToArray(), positive, behind );
			}
			var value = source[index++];
			if ( quoting ) { bytes.Add( value ); }
			else if ( value == (byte)'\\' ) {
				PcreByteNode escaped;
				try {
					escaped = ParseEscape( index - 1 );
				} catch ( PcreByteParseException error ) when ( error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax ) {
					throw Unsupported( openingOffset );
				}
				if ( escaped is PcreByteMatchStartReset ) { throw Invalid( openingOffset, "a match-start reset is not allowed in an assertion" ); }
				if ( escaped is not PcreByteLiteral literal ) { throw Unsupported( openingOffset ); }
				if ( utf8 ) {
					var width = new Rune( literal.Value ).EncodeToUtf8( encoded );
					for ( var byteIndex = 0; byteIndex < width; byteIndex++ ) { bytes.Add( encoded[byteIndex] ); }
				} else { bytes.Add( (byte)literal.Value ); }
			} else if ( value is (byte)'|' or (byte)'(' or (byte)'[' or (byte)'.'
				or (byte)'^' or (byte)'$' or (byte)'*' or (byte)'+' or (byte)'?'
				or (byte)'{' or (byte)'}' ) {
				throw Unsupported( openingOffset );
			} else {
				bytes.Add( value );
			}
		}
		throw Invalid( openingOffset, "unclosed assertion" );
	}

	private PcreByteNode ParseBracket( int openingOffset ) {
		var negated = index < source.Length && source[index] == (byte)'^';
		if ( negated ) { index++; }
		var quoting = false;
		var hasItem = false;
		var explicitNewline = false;
		var set = default(PcreByteSet);
		var specialScanCursor = index;
		var nextClosingBracket = -1;
		while ( index < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( TryConsumeQuoteDelimiter( ref quoting ) ) { continue; }
			if ( !quoting && source[index] == (byte)']' && hasItem ) {
				index++;
				return new PcreByteClass( negated ? set.Complement() : set, !negated && explicitNewline );
			}
			var item = quoting ? QuotedBracketLiteral() : ParseBracketItem( ref specialScanCursor, ref nextClosingBracket );
			while ( TryConsumeQuoteDelimiter( ref quoting ) ) { }
			if ( item.Value is (byte)'\r' or (byte)'\n' ) { explicitNewline = true; }
			if ( !item.Value.HasValue && index < source.Length && source[index] == (byte)'-'
				&& index + 1 < source.Length && source[index + 1] != (byte)']' ) {
				throw Invalid( index, "a character class cannot start a range" );
			}
			if ( !quoting && item.Value.HasValue && item.MayStartRange && index < source.Length
				&& source[index] == (byte)'-' && index + 1 < source.Length && source[index + 1] != (byte)']' ) {
				var hyphenOffset = index++;
				while ( TryConsumeQuoteDelimiter( ref quoting ) ) { }
				if ( index >= source.Length ) { throw Invalid( openingOffset, "unclosed bracket class" ); }
				var end = quoting ? QuotedBracketLiteral() : ParseBracketItem( ref specialScanCursor, ref nextClosingBracket );
				while ( TryConsumeQuoteDelimiter( ref quoting ) ) { }
				if ( !end.Value.HasValue || end.Value.Value < item.Value.Value ) {
					throw Invalid( hyphenOffset, "range endpoints must be ordered literal bytes" );
				}
				if ( item.Value.Value <= (byte)'\n' && end.Value.Value >= (byte)'\n'
					|| item.Value.Value <= (byte)'\r' && end.Value.Value >= (byte)'\r' ) {
					explicitNewline = true;
				}
				var range = PcreByteSet.Range( item.Value.Value, end.Value.Value );
				set = set.Union( caseless ? range.AsciiCaseClosure() : range );
			} else {
				set = set.Union( item.Set );
			}
			hasItem = true;
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
			return new PcreByteWordBoundary( value == (byte)'b', ucp );
		}
		if ( value is >= (byte)'0' and <= (byte)'9' ) { return ParseDecimalOrOctalEscape( value, offset, inBracket ); }
		if ( value == (byte)'o' ) { return ParseBracedOctal( offset ); }
		if ( value == (byte)'k' ) {
			if ( inBracket ) { throw Unsupported( offset ); } // Literal bracket interpretation is pending.
			if ( index >= source.Length || source[index] is not (byte)'<' and not (byte)'\'' and not (byte)'{' ) {
				throw Invalid( offset, "named backreference requires a delimiter" );
			}
			var delimiter = source[index++];
			return ParseNamedReference( delimiter == (byte)'<' ? (byte)'>'
				: delimiter == (byte)'{' ? (byte)'}' : (byte)'\'', offset );
		}
		if ( value == (byte)'g' ) {
			if ( inBracket ) { throw Unsupported( offset ); } // g is a literal in a PCRE2 bracket.
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
			return new PcreByteScalarClass( ucp && !asciiBsD
				? PcreByteScalarClassKind.DecimalDigit : PcreByteScalarClassKind.AsciiDigit,
				value == (byte)'D' );
		}
		if ( utf8 && ( value is (byte)'w' or (byte)'W' ) ) {
			return new PcreByteScalarClass( ucp ? PcreByteScalarClassKind.UnicodeWord
				: PcreByteScalarClassKind.AsciiWord, value == (byte)'W' );
		}
		if ( utf8 && ( value is (byte)'s' or (byte)'S' ) ) {
			return new PcreByteScalarClass( ucp ? PcreByteScalarClassKind.UnicodeSpace
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
		if ( !utf8 ) { throw Unsupported( offset ); }
		if ( index >= source.Length ) { throw Invalid( offset, "property escape requires a name" ); }
		if ( source[index] != (byte)'{' ) {
			var shortName = NormalizePropertyName( source.AsSpan( index++, 1 ) );
			return GeneralCategory( shortName, negated ) ?? throw Unsupported( offset );
		}
		index++;
		while ( index < source.Length && source[index] is >= 0x09 and <= 0x0D or (byte)' ' ) {
			cancellationToken.ThrowIfCancellationRequested();
			index++;
		}
		if ( index < source.Length && source[index] == (byte)'^' ) {
			negated = !negated;
			index++;
		}
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
		if ( name.Length == 0 ) { throw Invalid( offset, "property escape requires a name" ); }
		if ( name.StartsWith( "GC:" ) || name.StartsWith( "GC=" ) ) { name = name[3..]; }
		else if ( name.StartsWith( "GENERALCATEGORY:" ) || name.StartsWith( "GENERALCATEGORY=" ) ) {
			name = name[16..];
		}
		if ( name.Length == 0 ) { throw Invalid( offset, "property escape requires a name" ); }
		return GeneralCategory( name, negated ) ?? throw Unsupported( offset );
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
