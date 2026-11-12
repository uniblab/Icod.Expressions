namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private PcreByteAlternatives WrapWholeMatch( PcreByteAlternatives root, GroupOptions entry ) {
		if ( !matchWord && !matchLine ) { return root; }
		PcreByteNode start;
		PcreByteNode end;
		if ( matchLine ) {
			start = CreateStartAnchor( entry.Multiline );
			end = CreateEndAnchor( entry.Multiline );
		} else {
			var unicode = ucp && ( entry.AsciiRestrictions & AsciiRestriction.BackslashW ) == AsciiRestriction.None;
			start = new PcreByteWordBoundary( true, unicode );
			end = new PcreByteWordBoundary( true, unicode );
		}
		// Guards belong to the entry scope, before source inline options. The
		// wrapper becomes part of root0 before recursive call targets are built.
		start = start with { Caseless = entry.Caseless, CaselessRestricted = entry.CaselessRestricted };
		end = end with { Caseless = entry.Caseless, CaselessRestricted = entry.CaselessRestricted };
		var body = new PcreByteNoncapturingGroup( root ) {
			Caseless = entry.Caseless, CaselessRestricted = entry.CaselessRestricted
		};
		return new( [new PcreByteNode[] { start, body, end }] );
	}
}
