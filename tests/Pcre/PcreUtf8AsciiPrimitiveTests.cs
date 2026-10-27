using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreUtf8AsciiPrimitiveTests {
	[Fact]
	public void DecoderKeepsAsciiAndMultibyteSourceWidths() {
		var source = new byte[] { 0, 0x7f, (byte)'A', 0xce, 0xa9, 0xff };
		AssertScalar( source, 0, 0, 1 );
		AssertScalar( source, 1, 0x7f, 1 );
		AssertScalar( source, 2, 'A', 1 );
		AssertScalar( source, 3, 'Ω', 2 );
		Assert.False( PcreUtf8.TryRead( source, 4, out _, out _ ) );
		Assert.False( PcreUtf8.TryRead( source, 5, out _, out _ ) );
	}

	[Theory]
	[InlineData( "A", 0, 1 )]
	[InlineData( "z", 0, 1 )]
	[InlineData( "9A", 1, 1 )]
	[InlineData( "Ⅻ", -1, 0 )]
	[InlineData( "éA", 0, 2 )]
	public void UnicodeLetterRecognizesAsciiAndNonAsciiWithoutMovingByteOffsets(
		string subject, int index, int length
	) {
		var compiled = PcreByteCompiler.Compile( "\\p{L}"u8.ToArray(), new PcreByteOptions { Utf8 = true } );
		Assert.True( compiled.IsSuccess, compiled.Diagnostic?.Message );
		var result = compiled.Pattern!.Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : index, result.Match?.ByteIndex );
		if ( index >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void AsciiPropertyStillConsumesTheSameWorkBudget() {
		var limited = PcreByteCompiler.Compile( "\\p{L}"u8.ToArray(),
			new PcreByteOptions { Utf8 = true, MatchWorkLimit = 4 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			limited.Pattern!.Match( "A"u8.ToArray() ).Diagnostic?.Code );
		var exact = PcreByteCompiler.Compile( "\\p{L}"u8.ToArray(),
			new PcreByteOptions { Utf8 = true, MatchWorkLimit = 5 } );
		Assert.Equal( 1, exact.Pattern!.Match( "A"u8.ToArray() ).Match?.ByteLength );
	}

	private static void AssertScalar( byte[] source, int offset, int expected, int expectedWidth ) {
		Assert.True( PcreUtf8.TryRead( source, offset, out var scalar, out var width ) );
		Assert.Equal( expected, scalar );
		Assert.Equal( expectedWidth, width );
	}
}
