using System.Text;

namespace Icod.Expressions.Pcre;

/// <summary>Parses only implemented byte-mode constructs, retaining syntax order.</summary>
internal sealed class PcreByteParser {
	private const int MaximumPatternDepth = 256;
	private const int MaximumCaptureCount = 256;
	private const int MaximumRepeatMarks = 256;
	private readonly byte[] source;
	private readonly CancellationToken cancellationToken;
	private readonly bool utf8;
	private readonly bool ucp;
	private readonly List<(int Number, int Offset)> backreferences = new();
	private int index;
	private int groupCount;
	private int repeatMarks;
	private bool dotAll;
	private bool caseless;
	private bool multiline;
	private bool asciiBsD;

	internal PcreByteParser( byte[] source, bool caseless, bool utf8, bool ucp,
		bool asciiBsD, CancellationToken cancellationToken ) {
		this.source = source;
		this.caseless = caseless;
		this.utf8 = utf8;
		this.ucp = ucp;
		this.asciiBsD = asciiBsD;
		this.cancellationToken = cancellationToken;
	}

	internal int GroupCount => groupCount;
	internal bool Caseless => caseless;

	internal PcreByteAlternatives Parse() {
		if ( utf8 && source.AsSpan().StartsWith( "(?-aD)"u8 ) ) {
			index = 6;
			asciiBsD = false;
		} else if ( utf8 && source.AsSpan().StartsWith( "(?aD)"u8 ) ) {
			index = 5;
			asciiBsD = true;
		} else if ( source.Length >= 4 && source[0] == (byte)'(' && source[1] == (byte)'?'
			&& ( source[2] is (byte)'s' or (byte)'i' or (byte)'m' ) && source[3] == (byte)')' ) {
			if ( utf8 && source[2] == (byte)'i' ) { throw Unsupported( 0 ); }
			index = 4;
			dotAll = source[2] == (byte)'s';
			if ( source[2] == (byte)'i' ) { caseless = true; }
			multiline = source[2] == (byte)'m';
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
		return root;
	}

	private PcreByteAlternatives ParseAlternatives( bool inGroup, int depth ) {
		if ( depth > MaximumPatternDepth ) {
			throw new PcreByteParseException( new(
				PcreByteDiagnosticCode.PatternDepthLimitExceeded, index, "pattern nesting limit exceeded"
			) );
		}
		var branches = new List<IReadOnlyList<PcreByteNode>>();
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			var nodes = new List<PcreByteNode>();
			while ( index < source.Length && source[index] != (byte)'|' && source[index] != (byte)')' ) {
				cancellationToken.ThrowIfCancellationRequested();
				var atom = ParseAtom( depth );
				if ( index < source.Length && source[index] is (byte)'*' or (byte)'+' or (byte)'?' ) {
					if ( atom is PcreByteLiteralAssertion ) {
						throw Unsupported( index );
					}
					if ( atom is PcreByteAnchor or PcreByteLineAnchor or PcreByteEndBeforeFinalLf or PcreByteWordBoundary ) {
						throw Invalid( index, "an anchor cannot be repeated" );
					}
					var quantifierOffset = index;
					var quantifier = source[index++];
					repeatMarks += quantifier == (byte)'+' ? 2 : quantifier == (byte)'*' ? 1 : 0;
					if ( repeatMarks > MaximumRepeatMarks ) {
						throw new PcreByteParseException( new(
							PcreByteDiagnosticCode.PatternRepeatLimitExceeded, quantifierOffset, "repeat state limit exceeded"
						) );
					}
					var minimum = quantifier == (byte)'+' ? 1 : 0;
					var maximum = quantifier == (byte)'?' ? 1 : int.MaxValue;
					var greedy = index >= source.Length || source[index] != (byte)'?';
					if ( !greedy ) { index++; }
					atom = new PcreByteRepeat( atom, minimum, maximum, greedy );
					if ( index < source.Length && source[index] is (byte)'*' or (byte)'+' or (byte)'?' ) {
						if ( greedy && source[index] == (byte)'+' ) {
							throw Unsupported( index ); // Valid possessive suffix, pending implementation.
						}
						throw Invalid( index, "repeated quantifier" );
					}
				}
				nodes.Add( atom );
			}
			branches.Add( nodes );
			if ( index >= source.Length || source[index] == (byte)')' ) {
				if ( inGroup && index >= source.Length ) {
					throw Invalid( source.Length, "unclosed group" );
				}
				return new( branches );
			}
			index++; // Ordered alternative.
		}
	}

	private PcreByteNode ParseAtom( int depth ) {
		var offset = index;
		var value = source[index++];
		if ( value == (byte)'(' ) {
			var capturing = true;
			if ( index < source.Length && source[index] == (byte)'?' ) {
				if ( index + 1 < source.Length && source[index + 1] == (byte)':' ) {
					index += 2;
					capturing = false;
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
			PcreByteAlternatives body;
			try {
				body = ParseAlternatives( true, depth + 1 );
			} catch ( PcreByteParseException error ) when ( error.Diagnostic.Code == PcreByteDiagnosticCode.InvalidSyntax && error.Diagnostic.Message == "unclosed group" ) {
				throw Invalid( offset, "unclosed group" );
			}
			if ( index >= source.Length || source[index] != (byte)')' ) {
				throw Invalid( offset, "unclosed group" );
			}
			index++;
			return capturing ? new PcreByteGroup( number, body ) : new PcreByteNoncapturingGroup( body );
		}
		if ( value == (byte)'^' ) {
			return multiline ? new PcreByteLineAnchor( true ) : new PcreByteAnchor( true );
		}
		if ( value == (byte)'$' ) {
			return multiline ? new PcreByteLineAnchor( false ) : new PcreByteEndBeforeFinalLf();
		}
		if ( value == (byte)'.' ) {
			return utf8 ? new PcreByteAny( dotAll )
				: new PcreByteClass( dotAll ? PcreByteSet.AllBytes : PcreByteSet.Dot );
		}
		if ( value == (byte)'[' ) {
			if ( utf8 ) {
				var content = index < source.Length && source[index] == (byte)'^' ? index + 1 : index;
				return source.AsSpan( content ).StartsWith( "[:"u8 )
					? ParseUtf8StandalonePosixBracket( offset ) : ParseUtf8LiteralBracket( offset );
			}
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

	private PcreByteNode ParseUtf8StandalonePosixBracket( int openingOffset ) {
		var outerNegated = index < source.Length && source[index] == (byte)'^';
		if ( outerNegated ) { index++; }
		var posixNegated = source.AsSpan( index ).StartsWith( "[:^"u8 );
		if ( !posixNegated && !source.AsSpan( index ).StartsWith( "[:"u8 ) ) {
			throw Unsupported( openingOffset );
		}
		index += posixNegated ? 3 : 2;
		var suffix = source.AsSpan( index );
		PcreByteScalarClassKind kind;
		if ( suffix.StartsWith( "digit:]]"u8 ) ) {
			kind = ucp ? PcreByteScalarClassKind.DecimalDigit : PcreByteScalarClassKind.AsciiDigit;
			index += "digit:]]"u8.Length;
		} else if ( suffix.StartsWith( "word:]]"u8 ) ) {
			kind = ucp ? PcreByteScalarClassKind.UnicodeWord : PcreByteScalarClassKind.AsciiWord;
			index += "word:]]"u8.Length;
		} else if ( suffix.StartsWith( "space:]]"u8 ) ) {
			kind = ucp ? PcreByteScalarClassKind.UnicodeSpace : PcreByteScalarClassKind.AsciiSpace;
			index += "space:]]"u8.Length;
		} else { throw Unsupported( openingOffset ); }
		return new PcreByteScalarClass( kind, outerNegated != posixNegated );
	}

	private PcreByteScalarSet ParseUtf8LiteralBracket( int openingOffset ) {
		var negated = index < source.Length && source[index] == (byte)'^';
		if ( negated ) { index++; }
		var ranges = new List<PcreByteScalarRange>();
		while ( index < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( source[index] == (byte)']' && ranges.Count > 0 ) {
				index++;
				return new( ranges.ToArray(), negated );
			}
			var firstOffset = index;
			var first = ParseUtf8BracketLiteral( openingOffset );
			if ( source[firstOffset] != (byte)'-' && index + 1 < source.Length
				&& source[index] == (byte)'-' && source[index + 1] != (byte)']' ) {
				index++;
				var last = ParseUtf8BracketLiteral( openingOffset );
				if ( last < first ) { throw Invalid( openingOffset, "range endpoints must be ordered scalars" ); }
				ranges.Add( new( first, last ) );
			} else { ranges.Add( new( first, first ) ); }
		}
		throw Invalid( openingOffset, "unclosed bracket class" );
	}

	private int ParseUtf8BracketLiteral( int openingOffset ) {
		var offset = index;
		var value = source[index++];
		if ( value == (byte)'[' && index < source.Length
			&& source[index] is (byte)':' or (byte)'.' or (byte)'=' ) {
			throw Unsupported( openingOffset );
		}
		if ( value == (byte)'\\' ) {
			if ( index < source.Length && source[index] == (byte)'b' ) { index++; return 0x08; }
			if ( index < source.Length && source[index] is >= (byte)'1' and <= (byte)'9' ) {
				throw Unsupported( openingOffset ); // Numeric escapes in brackets are octal.
			}
			PcreByteNode escaped;
			try { escaped = ParseEscape( offset ); }
			catch ( PcreByteParseException error ) when (
				error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax
			) { throw Unsupported( openingOffset ); }
			return escaped is PcreByteLiteral literal ? literal.Value : throw Unsupported( openingOffset );
		}
		if ( value >= 0x80 ) {
			PcreUtf8.TryRead( source, offset, out var scalar, out var width ); // Validated at compile entry.
			index = offset + width;
			return scalar;
		}
		return value;
	}

	private PcreByteLiteralAssertion ParseLiteralAssertion( int openingOffset, bool positive, bool behind ) {
		var bytes = new List<byte>();
		Span<byte> encoded = stackalloc byte[4];
		while ( index < source.Length && source[index] != (byte)')' ) {
			cancellationToken.ThrowIfCancellationRequested();
			var value = source[index++];
			if ( value == (byte)'\\' ) {
				PcreByteNode escaped;
				try {
					escaped = ParseEscape( index - 1 );
				} catch ( PcreByteParseException error ) when ( error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax ) {
					throw Unsupported( openingOffset );
				}
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
		if ( index >= source.Length ) { throw Invalid( openingOffset, "unclosed assertion" ); }
		index++;
		return new( bytes.ToArray(), positive, behind );
	}

	private PcreByteNode ParseBracket( int openingOffset ) {
		var negated = index < source.Length && source[index] == (byte)'^';
		if ( negated ) { index++; }
		var hasItem = false;
		var set = default(PcreByteSet);
		var specialScanCursor = index;
		var nextClosingBracket = -1;
		while ( index < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( source[index] == (byte)']' && hasItem ) {
				index++;
				return new PcreByteClass( negated ? set.Complement() : set );
			}
			var item = ParseBracketItem( ref specialScanCursor, ref nextClosingBracket );
			if ( !item.Value.HasValue && index < source.Length && source[index] == (byte)'-'
				&& index + 1 < source.Length && source[index + 1] != (byte)']' ) {
				throw Invalid( index, "a character class cannot start a range" );
			}
			if ( item.Value.HasValue && item.MayStartRange && index < source.Length
				&& source[index] == (byte)'-' && index + 1 < source.Length && source[index + 1] != (byte)']' ) {
				var hyphenOffset = index++;
				var end = ParseBracketItem( ref specialScanCursor, ref nextClosingBracket );
				if ( !end.Value.HasValue || end.Value.Value < item.Value.Value ) {
					throw Invalid( hyphenOffset, "range endpoints must be ordered literal bytes" );
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
			if ( index < source.Length && source[index] is >= (byte)'1' and <= (byte)'9' ) {
				throw Unsupported( offset ); // In a class, numeric escapes are octal characters.
			}
			var escape = ParseEscape( offset );
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

	private PcreByteNode ParseEscape( int offset ) {
		if ( index >= source.Length ) {
			throw Invalid( offset, "trailing backslash" );
		}
		var value = source[index++];
		if ( value == (byte)'A' ) { return new PcreByteAnchor( true ); }
		if ( value == (byte)'z' ) { return new PcreByteAnchor( false ); }
		if ( value == (byte)'Z' ) { return new PcreByteEndBeforeFinalLf( true ); }
		if ( value is (byte)'p' or (byte)'P' ) {
			return ParseUnicodeProperty( offset, value == (byte)'P' );
		}
		if ( value == (byte)'b' || value == (byte)'B' ) {
			return new PcreByteWordBoundary( value == (byte)'b', ucp );
		}
		if ( value is >= (byte)'1' and <= (byte)'9' ) {
			if ( index < source.Length && source[index] is >= (byte)'0' and <= (byte)'9' ) {
				throw Unsupported( offset ); // Multi-digit references and octal ambiguity are pending.
			}
			var number = value - (byte)'0';
			backreferences.Add( (number, offset) );
			return new PcreByteBackreference( number );
		}
		if ( value == (byte)'x' ) {
			if ( index < source.Length && source[index] == (byte)'{' ) { throw Unsupported( offset ); }
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

	private PcreByteNode ParseUnicodeProperty( int offset, bool negated ) {
		if ( !utf8 ) { throw Unsupported( offset ); }
		if ( index >= source.Length ) { throw Invalid( offset, "property escape requires a name" ); }
		if ( source[index] != (byte)'{' ) {
			if ( source[index] != (byte)'L' ) { throw Unsupported( offset ); }
			index++;
			return new PcreByteScalarClass( PcreByteScalarClassKind.Letter, negated );
		}
		index++;
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
		var kind = index == nameStart + 1 && source[nameStart] == (byte)'L'
			? PcreByteScalarClassKind.Letter
			: index == nameStart + 2 && source[nameStart] == (byte)'N' && source[nameStart + 1] == (byte)'d'
				? PcreByteScalarClassKind.DecimalDigit
				: (PcreByteScalarClassKind?)null;
		index++;
		if ( !kind.HasValue ) { throw Unsupported( offset ); }
		return new PcreByteScalarClass( kind.Value, negated );
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
