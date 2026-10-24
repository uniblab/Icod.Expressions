namespace Icod.Expressions.Pcre;

/// <summary>Parses only implemented byte-mode constructs, retaining syntax order.</summary>
internal sealed class PcreByteParser {
	private const int MaximumPatternDepth = 256;
	private const int MaximumCaptureCount = 256;
	private const int MaximumRepeatMarks = 256;
	private readonly byte[] source;
	private readonly CancellationToken cancellationToken;
	private readonly List<(int Number, int Offset)> backreferences = new();
	private int index;
	private int groupCount;
	private int repeatMarks;
	private bool dotAll;

	internal PcreByteParser( byte[] source, CancellationToken cancellationToken ) {
		this.source = source;
		this.cancellationToken = cancellationToken;
	}

	internal int GroupCount => groupCount;

	internal PcreByteAlternatives Parse() {
		if ( source.Length >= 4 && source[0] == (byte)'(' && source[1] == (byte)'?'
			&& source[2] == (byte)'s' && source[3] == (byte)')' ) {
			index = 4;
			dotAll = true;
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
					if ( atom is PcreByteAnchor ) {
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
		if ( value == (byte)'^' || value == (byte)'$' ) {
			return new PcreByteAnchor( value == (byte)'^' );
		}
		if ( value == (byte)'.' ) {
			return new PcreByteClass( dotAll ? PcreByteSet.AllBytes : PcreByteSet.Dot );
		}
		if ( value == (byte)'[' ) {
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
		return new PcreByteLiteral( value );
	}

	private PcreByteLiteralAssertion ParseLiteralAssertion( int openingOffset, bool positive, bool behind ) {
		var bytes = new List<byte>();
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
				bytes.Add( literal.Value );
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
				set = set.Union( PcreByteSet.Range( item.Value.Value, end.Value.Value ) );
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
				PcreByteLiteral literal => (PcreByteSet.Singleton( literal.Value ), literal.Value, true),
				PcreByteClass characterClass => (characterClass.Set, null, false),
				_ => throw Unsupported( offset )
			};
		}
		return (PcreByteSet.Singleton( value ), value, true);
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
		return (negated ? set.Complement() : set, null, false);
	}

	private PcreByteNode ParseEscape( int offset ) {
		if ( index >= source.Length ) {
			throw Invalid( offset, "trailing backslash" );
		}
		var value = source[index++];
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
			return new PcreByteLiteral( (byte)result );
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
		if ( characterClass.HasValue ) { return new PcreByteClass( characterClass.Value ); }
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
		return new PcreByteLiteral( value );
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
