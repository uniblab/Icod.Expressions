namespace Icod.Expressions.Pcre;

/// <summary>Parses only implemented byte-mode constructs, retaining syntax order.</summary>
internal sealed class PcreByteParser {
	private readonly byte[] source;
	private readonly CancellationToken cancellationToken;
	private int index;
	private int groupCount;

	internal PcreByteParser( byte[] source, CancellationToken cancellationToken ) {
		this.source = source;
		this.cancellationToken = cancellationToken;
	}

	internal int GroupCount => groupCount;

	internal PcreByteAlternatives Parse() {
		var root = ParseAlternatives( false );
		if ( index != source.Length ) {
			throw Invalid( index, "unmatched closing parenthesis" );
		}
		return root;
	}

	private PcreByteAlternatives ParseAlternatives( bool inGroup ) {
		var branches = new List<IReadOnlyList<PcreByteNode>>();
		while ( true ) {
			var nodes = new List<PcreByteNode>();
			while ( index < source.Length && source[index] != (byte)'|' && source[index] != (byte)')' ) {
				cancellationToken.ThrowIfCancellationRequested();
				var atom = ParseAtom();
				if ( index < source.Length && source[index] is (byte)'*' or (byte)'+' or (byte)'?' ) {
					if ( atom is PcreByteAnchor ) {
						throw Invalid( index, "an anchor cannot be repeated" );
					}
					var quantifier = source[index++];
					var minimum = quantifier == (byte)'+' ? 1 : 0;
					var maximum = quantifier == (byte)'?' ? 1 : int.MaxValue;
					var greedy = index >= source.Length || source[index] != (byte)'?';
					if ( !greedy ) { index++; }
					atom = new PcreByteRepeat( atom, minimum, maximum, greedy );
					if ( index < source.Length && source[index] is (byte)'*' or (byte)'+' or (byte)'?' ) {
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

	private PcreByteNode ParseAtom() {
		var offset = index;
		var value = source[index++];
		if ( value == (byte)'(' ) {
			if ( index < source.Length && source[index] == (byte)'?' ) {
				throw Unsupported( offset );
			}
			var number = ++groupCount;
			PcreByteAlternatives body;
			try {
				body = ParseAlternatives( true );
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
		if ( value is (byte)'*' or (byte)'+' or (byte)'?' ) {
			throw Invalid( offset, "quantifier without an atom" );
		}
		if ( value is (byte)'\\' or (byte)'.' or (byte)'{' or (byte)'}' or (byte)'[' or (byte)']' ) {
			throw Unsupported( offset );
		}
		return new PcreByteLiteral( value );
	}

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
