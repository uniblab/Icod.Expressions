namespace Icod.Expressions.Pcre;

/// <summary>Parses only implemented byte-mode constructs, retaining syntax order.</summary>
internal sealed class PcreByteParser {
	private const int MaximumPatternDepth = 256;
	private const int MaximumCaptureCount = 256;
	private const int MaximumRepeatMarks = 256;
	private readonly byte[] source;
	private readonly CancellationToken cancellationToken;
	private int index;
	private int groupCount;
	private int repeatMarks;

	internal PcreByteParser( byte[] source, CancellationToken cancellationToken ) {
		this.source = source;
		this.cancellationToken = cancellationToken;
	}

	internal int GroupCount => groupCount;

	internal PcreByteAlternatives Parse() {
		var root = ParseAlternatives( false, 0 );
		if ( index != source.Length ) {
			throw Invalid( index, "unmatched closing parenthesis" );
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
			if ( index < source.Length && source[index] == (byte)'?' ) {
				throw Unsupported( offset );
			}
			if ( depth >= MaximumPatternDepth ) {
				throw new PcreByteParseException( new(
					PcreByteDiagnosticCode.PatternDepthLimitExceeded, offset, "pattern nesting limit exceeded"
				) );
			}
			var number = ++groupCount;
			if ( number > MaximumCaptureCount ) {
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
			return new PcreByteGroup( number, body );
		}
		if ( value == (byte)'^' || value == (byte)'$' ) {
			return new PcreByteAnchor( value == (byte)'^' );
		}
		if ( value == (byte)'.' ) {
			return new PcreByteClass( PcreByteSet.Dot );
		}
		if ( value == (byte)'\\' ) {
			return ParseEscape( offset );
		}
		if ( value is (byte)'*' or (byte)'+' or (byte)'?' ) {
			throw Invalid( offset, "quantifier without an atom" );
		}
		if ( value is (byte)'{' or (byte)'}' or (byte)'[' or (byte)']' ) {
			throw Unsupported( offset );
		}
		return new PcreByteLiteral( value );
	}

	private PcreByteNode ParseEscape( int offset ) {
		if ( index >= source.Length ) {
			throw Invalid( offset, "trailing backslash" );
		}
		var value = source[index++];
		if ( value == (byte)'x' ) {
			if ( index < source.Length && source[index] == (byte)'{' ) { throw Unsupported( offset ); }
			var result = 0;
			for ( var count = 0; count < 2 && index < source.Length; count++ ) {
				var digit = HexDigit( source[index] );
				if ( digit < 0 ) { break; }
				result = ( result << 4 ) | digit;
				index++;
			}
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
