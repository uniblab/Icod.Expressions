namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private PcreByteNode CreateStartAnchor( bool multiline ) {
		if ( !multiline ) { return new PcreByteAnchor( true ) { IsDisabled = notBol }; }
		return new PcreByteLineAnchor( true ) {
			AllowSubjectEnd = altCircumflex, SuppressSubjectBoundary = notBol
		};
	}

	private PcreByteNode CreateEndAnchor( bool multiline ) {
		if ( multiline ) { return new PcreByteLineAnchor( false ) { SuppressSubjectBoundary = notEol }; }
		return dollarEndOnly ? new PcreByteAnchor( false ) { IsDisabled = notEol }
			: new PcreByteEndBeforeFinalLf() { IsDisabled = notEol };
	}
}
