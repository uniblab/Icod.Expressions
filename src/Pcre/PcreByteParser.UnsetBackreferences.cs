namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private PcreByteAlternatives EnableUnsetBackreferences( PcreByteAlternatives root ) =>
		RewriteTree( root, node => node is PcreByteBackreference reference
			? reference with { UnsetMatchesEmpty = true } : node );
}
