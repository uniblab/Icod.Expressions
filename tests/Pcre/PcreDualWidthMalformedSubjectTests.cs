using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreDualWidthReferenceTests {
	[Theory]
	[InlineData( 8, "C3", -3 )]
	[InlineData( 8, "80", -22 )]
	[InlineData( 8, "C0AF", -17 )]
	[InlineData( 8, "EDA080", -16 )]
	[InlineData( 8, "F4908080", -15 )]
	[InlineData( 16, "D800", -24 )]
	[InlineData( 16, "D8000041", -25 )]
	[InlineData( 16, "DC00", -26 )]
	public void WidthSpecificMalformedSubjectsKeepNativeErrors( int width, string hex, int error ) {
		PcreMatchException actual;
		if ( width == 8 ) {
			var subject = Convert.FromHexString( hex );
			var native = new PcreRegex8Bit( "."u8, StrictUtf8, Settings( 1 ) );
			actual = Assert.Throws<PcreMatchException>( () => { native.Match( subject ); } );
			var pattern = PcreByteCompiler.Compile( "."u8.ToArray(), new PcreByteOptions { Utf8 = true } );
			Assert.True( pattern.IsSuccess );
			Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, pattern.Pattern!.Match( subject ).Diagnostic?.Code );
		}
		else {
			var subject = Words( hex );
			var native = new PcreRegex( ".", Settings( 1 ) );
			actual = Assert.Throws<PcreMatchException>( () => { native.Match( subject ); } );
		}
		Assert.Equal( error, (int)actual.ErrorCode );
	}
}
