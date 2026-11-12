namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	// Native empty-class recognition precedes ordinary/UTS class parsing. Perl
	// algebra deliberately uses its own reader and never calls this helper.
	private bool TryReadEmptyClass( out bool negated ) {
		negated = false;
		if ( !allowEmptyClass ) { return false; }
		var savedIndex = index;
		negated = ParseClassNegation();
		if ( index < source.Length && source[index] == (byte)']' ) {
			index++;
			return true;
		}
		index = savedIndex;
		return false;
	}
}
