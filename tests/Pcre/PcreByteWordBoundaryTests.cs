using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 8-bit non-UCP word-boundary behavior over authoritative bytes.</summary>
public sealed class PcreByteWordBoundaryTests {
	[Theory]
	[InlineData( "\\bcat\\b", "cat!", 0, 3 )]
	[InlineData( "\\bcat\\b", "scat", -1, 0 )]
	[InlineData( "\\Boo\\B", "foobar", 1, 2 )]
	[InlineData( "\\b", "_", 0, 0 )]
	[InlineData( "\\b", "", -1, 0 )]
	[InlineData( "\\B", "", 0, 0 )]
	[InlineData( "\\B", "!", 0, 0 )]
	[InlineData( "\\b", "!", -1, 0 )]
	[InlineData( "\\bA", " A", 1, 1 )]
	[InlineData( "\\BA", "BA", 1, 1 )]
	public void MatchesAsciiWordTransitionsWithoutConsumingBytes( string pattern, string subject, int index, int length ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : index, result.Match?.ByteIndex );
		if ( index >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void NonzeroStartDoesNotHidePreviousByte() {
		var compiled = Compile( "\\bbar"u8.ToArray() );
		Assert.Null( compiled.Match( "foobar"u8.ToArray(), startOffset: 3 ).Match );
		Assert.Equal( 4, compiled.Match( "foo bar"u8.ToArray(), startOffset: 4 ).Match?.ByteIndex );
	}

	[Fact]
	public void AbandonedBranchRollsBackCaptureBeforeBoundary() {
		var match = Compile( "(a)\\bX|(a)\\B\\2"u8.ToArray() ).Match( "aa"u8.ToArray() ).Match;
		Assert.Equal( 0, match?.ByteIndex );
		Assert.Equal( 2, match?.ByteLength );
		Assert.Equal( 3, match?.Captures.Count );
		Assert.Null( match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 1 ), match?.Captures[2] );
	}

	[Theory]
	[InlineData( "ff41" )]
	[InlineData( "0041" )]
	public void HighBytesAndNulAreNonWord( string inputHex ) {
		var match = Compile( "\\bA"u8.ToArray() ).Match( Convert.FromHexString( inputHex ) ).Match;
		Assert.Equal( 1, match?.ByteIndex );
		Assert.Equal( 1, match?.ByteLength );
	}

	[Fact]
	public void BracketBackspaceRemainsLiteralWhileBracketCapitalBIsUnsupported() {
		var backspace = Compile( "[\\b]"u8.ToArray() ).Match( new byte[] { 0x08 } ).Match;
		Assert.Equal( 1, backspace?.ByteLength );
		var unsupported = PcreByteCompiler.Compile( "[\\B]"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, unsupported.Diagnostic?.Code );
		Assert.Equal( 1, unsupported.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "\\b+" )]
	[InlineData( "\\B*" )]
	public void RejectsRepetitionOfZeroWidthBoundary( string pattern ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( 2, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void BoundaryChecksRespectWorkAndCancellationLimits() {
		var input = System.Text.Encoding.ASCII.GetBytes( new string( '!', 40 ) );
		var limited = Compile( "\\bA"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 12 } ).Match( input );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );

		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "\\bA"u8.ToArray(), new PcreByteOptions {
			WorkObserver = count => { if ( count == 8 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( input, cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( pattern, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
