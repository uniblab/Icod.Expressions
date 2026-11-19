namespace Icod.Expressions.PackageSmoke;

internal sealed record PcreFragmentAllocationProfile( string Name, string Pattern, bool Anchored, bool FromEnd, int Start, string SmallCaptures, string LargeCaptures );

internal static class PcreFragmentAllocationCases {
	internal static IReadOnlyList<PcreFragmentAllocationProfile> All { get; } = new PcreFragmentAllocationProfile[] {
		new( "caller-match", "(A)", true, false, 0, "0:1,0:1", "0:1,0:1" ),
		new( "caller-miss", "(Z)", true, false, 0, "", "" ),
		new( "caller-late", "(A)", true, true, 0, "1022:1,1022:1", "65534:1,65534:1" ),
		new( "caller-invalid-empty", "(A?)", true, false, 1, "1:0,1:0", "1:0,1:0" ),
		new( "dotstar-match", "(?s:.*)(A)", false, false, 0, "0:1,0:1", "0:1,0:1" ),
		new( "dotstar-miss", "(?s:.*)(Z)", false, false, 0, "", "" )
	};

	internal static byte[] Input( int size ) {
		var result = new byte[checked( size + 2 )];
		result[0] = 0xFE;
		result[^1] = 0xFE;
		for ( var i = 0; i < size; i += 2 ) { result[i + 1] = 0x41; result[i + 2] = 0xFF; }
		return result;
	}

	internal static int Offset( PcreFragmentAllocationProfile profile, int size ) => profile.FromEnd ? size - 2 : profile.Start;
}
