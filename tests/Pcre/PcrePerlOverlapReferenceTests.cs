using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreDualWidthReferenceTests {
	[Theory]
	[MemberData( nameof( PcrePerlOverlapCases.Matching ), MemberType = typeof( PcrePerlOverlapCases ) )]
	public void PinnedPerlScalarCapturesAgreeWithBothNativeWidthsAndManagedBytes(
		int id, string pattern8, string pattern16, string subject8, string subject16, int mode,
		int offset8, int offset16, string spans8, string spans16, string perlScalars,
		string mark8, string mark16, string names ) {
		AssertProtocol( id, pattern8, pattern16, subject8, subject16, mode, offset8, offset16,
			spans8, spans16, perlScalars, mark8, mark16, names );
		var direct = PcreNative16Oracle.Execute( Units( pattern16 ), Units( subject16 ), mode, (uint)offset16 );
		Assert.Equal( spans16.Length == 0 ? "N" : "M", direct.Kind );
		Assert.Equal( spans16, direct.Captures );
		Assert.True( direct.JitSize > 0 );
		Assert.NotEqual( 0u, direct.Options & 0x80000u );
	}

	private static ushort[] Units( string hex ) => Words( hex ).Select( word => (ushort)word ).ToArray();
}
