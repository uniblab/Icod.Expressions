namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private PcreByteAlternatives ParseLiteralPattern() {
		var nodes = new List<PcreByteNode>();
		while ( index < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			nodes.Add( ParseQuotedLiteral() with {
				Caseless = caseless, CaselessRestricted = caselessRestricted
			} );
		}
		return new( [nodes] );
	}
}
