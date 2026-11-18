using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreDualWidthReferenceTests {
	[Theory]
	[MemberData( nameof( PcreDualWidthCases.Matching ), MemberType = typeof( PcreDualWidthCases ) )]
	public void NativeWidthsAndManagedBytesPreserveScalarProtocols( int id, string pattern8, string pattern16, string subject8, string subject16, int mode, int offset8, int offset16, string spans8, string spans16, string scalars, string mark8, string mark16, string names ) =>
		AssertProtocol( id, pattern8, pattern16, subject8, subject16, mode, offset8, offset16, spans8, spans16, scalars, mark8, mark16, names );

	[Theory]
	[MemberData( nameof( PcreDualWidthCases.Diagnostics ), MemberType = typeof( PcreDualWidthCases ) )]
	public void NativeCompileErrorsKeepBothOffsetUnits( int id, string pattern8, string pattern16, int code, int offset8, int offset16, int scalarOffset ) =>
		AssertCompileError( id, pattern8, pattern16, code, offset8, offset16, scalarOffset );
}
