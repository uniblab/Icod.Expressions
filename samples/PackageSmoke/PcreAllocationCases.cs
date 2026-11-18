namespace Icod.Expressions.PackageSmoke;

internal sealed record PcreAllocationProfile( string Name, string Pattern, int Mode, string Seed, string Captures );

internal static class PcreAllocationCases {
	internal const int SmallSize = 1024;
	internal const int LargeSize = 65536;
	internal const int Iterations = 8;
	internal const long FixedAllowance = 4096;
	internal static IReadOnlyList<PcreAllocationProfile> All { get; } = new PcreAllocationProfile[] {
		new( "raw-code-unit", @"\A(\C)", 0, "FF00", "0:1,0:1" ),
		new( "utf8-dot", @"\A(.)", 1, "CEBB", "0:2,0:2" ),
		new( "ucp-letter", @"\A(\p{L})", 3, "CEBB", "0:2,0:2" ),
		new( "utf8-no-match", @"\A(Z)", 1, "CEBB", "" )
	};

	internal static byte[] Input( PcreAllocationProfile profile, int size ) {
		var seed = Convert.FromHexString( profile.Seed );
		if ( size < 0 || size % seed.Length != 0 ) { throw new ArgumentOutOfRangeException( nameof( size ) ); }
		var result = new byte[size];
		for ( var offset = 0; offset < size; offset += seed.Length ) { seed.AsSpan().CopyTo( result.AsSpan( offset ) ); }
		return result;
	}
}
