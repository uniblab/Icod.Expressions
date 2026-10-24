using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 8-bit, non-UTF and non-UCP bracket fixtures.</summary>
public sealed class PcreByteBracketTests {
	[Theory]
	[InlineData( "[a-c]+", "7A61626364", 1, 3 )]
	[InlineData( "[a-c]", "64", -1, 0 )]
	[InlineData( "[A-Z0-9_]", "5F", 0, 1 )]
	[InlineData( "[\\x00-\\x02]", "0002", 0, 1 )]
	[InlineData( "[\\xFE-\\xFF]", "FF", 0, 1 )]
	[InlineData( "[\\xFF]", "FF", 0, 1 )]
	[InlineData( "[\\n]", "0A", 0, 1 )]
	[InlineData( "[\\b]", "08", 0, 1 )]
	[InlineData( "[\\]]", "5D", 0, 1 )]
	[InlineData( "[]]", "5D", 0, 1 )]
	[InlineData( "[-a]", "2D", 0, 1 )]
	[InlineData( "[a-]", "2D", 0, 1 )]
	[InlineData( "[a\\-c]", "2D", 0, 1 )]
	[InlineData( "[\\dA]", "35", 0, 1 )]
	[InlineData( "[\\dA]", "41", 0, 1 )]
	[InlineData( "[\\D]", "FF", 0, 1 )]
	[InlineData( "[\\s]", "0A", 0, 1 )]
	[InlineData( "[\\w]", "5F", 0, 1 )]
	[InlineData( "]", "5D", 0, 1 )]
	public void MatchesOrdinaryByteClassWithPcre2Span( string pattern, string inputHex, int start, int length ) {
		var match = Compile( pattern ).Match( Convert.FromHexString( inputHex ) );
		Assert.True( match.IsSuccess, match.Diagnostic?.Message );
		int? expectedStart = start < 0 ? null : start;
		Assert.Equal( expectedStart, match.Match?.ByteIndex );
		if ( start >= 0 ) { Assert.Equal( length, match.Match?.ByteLength ); }
	}

	[Theory]
	[InlineData( "[^a]", "0A" )]
	[InlineData( "[^a]", "00" )]
	[InlineData( "[^a]", "FF" )]
	[InlineData( "[^\\d]", "FF" )]
	public void NegationIncludesAllOtherBytes( string pattern, string inputHex ) {
		Assert.Equal( 0, Compile( pattern ).Match( Convert.FromHexString( inputHex ) ).Match?.ByteIndex );
	}

	[Fact]
	public void DotAndNegatedClassHaveDifferentNewlineBehavior() {
		Assert.Null( Compile( "." ).Match( "\n"u8.ToArray() ).Match );
		Assert.Equal( 0, Compile( "[^a]" ).Match( "\n"u8.ToArray() ).Match?.ByteIndex );
	}

	[Fact]
	public void BacktracksAcrossClassRepeatAndRestoresCapture() {
		var match = Compile( "([a-c]+)c" ).Match( "abcc"u8.ToArray() ).Match;
		Assert.Equal( 4, match?.ByteLength );
		Assert.Equal( new PcreByteCapture( 0, 3 ), match?.Captures[1] );
	}

	[Fact]
	public void CopiesBracketPatternBeforeCallerMutation() {
		var bytes = "[a]"u8.ToArray();
		var compiled = Compile( bytes );
		bytes[1] = (byte)'b';
		Assert.Equal( 0, compiled.Match( "a"u8.ToArray() ).Match?.ByteIndex );
		Assert.Null( compiled.Match( "b"u8.ToArray() ).Match );
	}

	[Fact]
	public void CountsClassTransitionsAgainstWorkLimit() {
		var result = PcreByteCompiler.Compile( "[a]+"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 6 } );
		Assert.True( result.IsSuccess );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			result.Pattern?.Match( "aaaaaa"u8.ToArray() ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( "[", 0 )]
	[InlineData( "[]", 0 )]
	[InlineData( "[^]", 0 )]
	[InlineData( "[a", 0 )]
	[InlineData( "[a\\", 2 )]
	[InlineData( "[z-a]", 2 )]
	[InlineData( "[a-\\d]", 2 )]
	[InlineData( "[\\d-a]", 3 )]
	public void RejectsMalformedOrdinaryClass( string pattern, int offset ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "[\\p{L}]" )]
	[InlineData( "[\\x{FF}]" )]
	public void RetainsPendingEscapesAsUnsupportedInClass( string pattern ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 1, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "alnum", "41", "FF" )]
	[InlineData( "alpha", "7A", "37" )]
	[InlineData( "ascii", "7F", "80" )]
	[InlineData( "blank", "09", "0A" )]
	[InlineData( "cntrl", "7F", "20" )]
	[InlineData( "digit", "39", "B2" )]
	[InlineData( "graph", "21", "20" )]
	[InlineData( "lower", "7A", "5A" )]
	[InlineData( "print", "20", "1F" )]
	[InlineData( "punct", "21", "41" )]
	[InlineData( "space", "0B", "A0" )]
	[InlineData( "upper", "5A", "7A" )]
	[InlineData( "word", "5F", "2D" )]
	[InlineData( "xdigit", "46", "47" )]
	public void PosixNamesHaveAsciiNonUcpBoundaries( string name, string yesHex, string noHex ) {
		var compiled = Compile( "[[:" + name + ":]]" );
		Assert.Equal( 0, compiled.Match( Convert.FromHexString( yesHex ) ).Match?.ByteIndex );
		Assert.Null( compiled.Match( Convert.FromHexString( noHex ) ).Match );
	}

	[Theory]
	[InlineData( "[[:^digit:]]", "FF", 0 )]
	[InlineData( "[[:^digit:]]", "35", -1 )]
	[InlineData( "[[:digit:]_]", "5F", 0 )]
	[InlineData( "[[:digit:]_]", "35", 0 )]
	[InlineData( "[^[:digit:]]", "FF", 0 )]
	[InlineData( "[^[:digit:]]", "35", -1 )]
	[InlineData( "[[:digit:]\\w]", "41", 0 )]
	[InlineData( "[[:digit:]-]", "2D", 0 )]
	public void CombinesPosixSetsAndOuterNegation( string pattern, string subjectHex, int expectedStart ) {
		var match = Compile( pattern ).Match( Convert.FromHexString( subjectHex ) );
		int? start = expectedStart < 0 ? null : expectedStart;
		Assert.Equal( start, match.Match?.ByteIndex );
	}

	[Theory]
	[InlineData( "[[:bogus:]]", 1, (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "[[.ch.]]", 1, (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "[[=a=]]", 1, (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "[[:digit]]", 1, (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( "[[:digit:]", 0, (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( "[a-[:digit:]]", 2, (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( "[[:digit:]-a]", 10, (int)PcreByteDiagnosticCode.InvalidSyntax )]
	public void RejectsUnknownOrMalformedPosixItemOrRange( string pattern, int offset, int code ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( (PcreByteDiagnosticCode)code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static PcreCompiledBytePattern Compile( string pattern ) => Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );

	private static PcreCompiledBytePattern Compile( byte[] pattern ) {
		var result = PcreByteCompiler.Compile( pattern );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
