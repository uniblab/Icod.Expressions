using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Reference spans observed with PCRE2 8-bit non-UTF, default LF newline.</summary>
public sealed class PcreByteAtomTests {
	[Theory]
	[InlineData( ".", "0A", -1, 0 )]
	[InlineData( ".", "0D", 0, 1 )]
	[InlineData( ".", "00", 0, 1 )]
	[InlineData( "\\n", "780A79", 1, 1 )]
	[InlineData( "\\t", "09", 0, 1 )]
	[InlineData( "\\r", "0D", 0, 1 )]
	[InlineData( "\\f", "0C", 0, 1 )]
	[InlineData( "\\a", "07", 0, 1 )]
	[InlineData( "\\e", "1B", 0, 1 )]
	[InlineData( "\\\\", "5C", 0, 1 )]
	[InlineData( "\\.", "2E", 0, 1 )]
	[InlineData( "\\|", "7C", 0, 1 )]
	[InlineData( "\\x", "00", 0, 1 )]
	[InlineData( "\\xF", "0F", 0, 1 )]
	[InlineData( "\\xFF", "FF", 0, 1 )]
	[InlineData( "\\xFG", "0F47", 0, 2 )]
	public void MatchesByteAtomWithPcre2Span( string pattern, string inputHex, int index, int length ) {
		var compiled = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		var result = compiled.Match( Convert.FromHexString( inputHex ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		if ( index < 0 ) {
			Assert.Null( result.Match );
		} else {
			Assert.Equal( index, result.Match?.ByteIndex );
			Assert.Equal( length, result.Match?.ByteLength );
		}
	}

	[Fact]
	public void QuantifiedDotLeavesNewlineAndCapturesLastByte() {
		var result = Compile( "(.)+"u8.ToArray() ).Match( "ab\n"u8.ToArray() );
		Assert.Equal( 2, result.Match?.ByteLength );
		Assert.Equal( new PcreByteCapture( 1, 1 ), result.Match?.Captures[1] );
	}

	[Fact]
	public void EscapedHighByteRemainsOneLiteralByte() {
		var result = Compile( new byte[] { (byte)'\\', 0xFF } ).Match( new byte[] { 0xFF } );
		Assert.Equal( 0, result.Match?.ByteIndex );
		Assert.Equal( 1, result.Match?.ByteLength );
	}

	[Fact]
	public void TrailingBackslashReportsItsByteOffset() {
		var result = PcreByteCompiler.Compile( "a\\"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( 1, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "\\x{FF}" )]
	[InlineData( "\\q" )]
	[InlineData( "\\p{L}" )]
	[InlineData( "\\1" )]
	public void RejectsPendingEscapesInsteadOfLiteralizingThem( string pattern ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern ) {
		var result = PcreByteCompiler.Compile( pattern );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
