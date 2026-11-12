namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private PcreByteNode CreateStartAnchor( bool multiline ) {
		if ( !multiline ) { return new PcreByteAnchor( true ); }
		return new PcreByteLineAnchor( true ) { AllowSubjectEnd = altCircumflex };
	}

	private PcreByteNode CreateEndAnchor( bool multiline ) {
		if ( multiline ) { return new PcreByteLineAnchor( false ); }
		return dollarEndOnly ? new PcreByteAnchor( false ) : new PcreByteEndBeforeFinalLf();
	}
}
