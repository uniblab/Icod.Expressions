using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreBracedHexTests {
	[Theory]
	[InlineData( "\\x{41}", "7A41", 1, 1 )]
	[InlineData( "\\x{ff}", "FF", 0, 1 )]
	[InlineData( "\\x{00000000}", "00", 0, 1 )]
	[InlineData( "\\x{ 41 }", "41", 0, 1 )]
	public void ByteEscapesProduceOneAuthoritativeByte( string pattern, string inputHex,
		int index, int length ) {
		var match = Compile( pattern ).Match( Convert.FromHexString( inputHex ) );
		Assert.True( match.IsSuccess, match.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( index, length ), match.Match?.Captures[0] );
	}

	[Theory]
	[InlineData( "[\\x{41}-\\x{43}]", "B", 0, 1 )]
	[InlineData( "[^\\x{41}]", "B", 0, 1 )]
	[InlineData( "[\\x{41}-\\x{43}]", "D", -1, 0 )]
	public void ByteBracketMembersAndRangeEndpointsUseEscapedValues(
		string pattern, string subject, int index, int length ) {
		var match = Compile( pattern ).Match( Encoding.ASCII.GetBytes( subject ) );
		Assert.Equal( index < 0 ? null : new PcreByteCapture( index, length ), match.Match?.Captures[0] );
	}

	[Theory]
	[InlineData( "\\x{E9}", "zé", 1, 2 )]
	[InlineData( "\\x{1F600}", "z😀", 1, 4 )]
	[InlineData( "[\\x{1F600}-\\x{1F601}]", "😁", 0, 4 )]
	[InlineData( "(?i)[\\x{4B}]", "K", 0, 3 )]
	public void UtfEscapesConsumeScalarsWithSourceByteSpans(
		string pattern, string subject, int index, int length ) {
		var match = Compile( pattern, new PcreByteOptions { Utf8 = true } )
			.Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( match.IsSuccess, match.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( index, length ), match.Match?.Captures[0] );
	}

	[Fact]
	public void EscapedScalarWorksInFixedLiteralLookbehind() {
		var match = Compile( "(?<=\\x{1F600})x", new PcreByteOptions { Utf8 = true } )
			.Match( "😀x"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 4, 1 ), match.Match?.Captures[0] );
	}

	[Fact]
	public void EscapedScalarCanBeCountedButNotMatchAcrossInvalidUtf() {
		var match = Compile( "\\x{1F600}{2}", new PcreByteOptions { MatchInvalidUtf = true } )
			.Match( new byte[] { 0xF0, 0x9F, 0x98, 0x80, 0xFF, 0xF0, 0x9F, 0x98, 0x80 } );
		Assert.Null( match.Match );
		Assert.True( match.IsSuccess, match.Diagnostic?.Message );
	}

	[Theory]
	[InlineData( "\\x{}", false )]
	[InlineData( "\\x{G}", false )]
	[InlineData( "\\x{41", false )]
	[InlineData( "\\x{4 1}", false )]
	[InlineData( "\\x{100}", false )]
	[InlineData( "[\\x{100}]", false )]
	[InlineData( "\\x{D800}", true )]
	[InlineData( "\\x{110000}", true )]
	[InlineData( "[\\x{DFFF}]", true )]
	public void InvalidHexOrOutOfRangeValueFailsAtEscapeStart( string pattern, bool utf8 ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ),
			new PcreByteOptions { Utf8 = utf8 } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( pattern[0] == '[' ? 1 : 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void ManyLeadingZerosStayBoundedAndCanBeCancelled() {
		var pattern = "\\x{" + new string( '0', 10000 ) + "41}";
		Assert.Equal( 1, Compile( pattern ).Match( "A"u8.ToArray() ).Match?.ByteLength );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
