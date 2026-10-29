using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 8-bit literal assertions retain the current byte position.</summary>
public sealed class PcreByteLookaroundTests {
	[Theory]
	[InlineData( "(?<=foo)bar", "foobar", 3, 3 )]
	[InlineData( "(?=foo)foo", "xfoo", 1, 3 )]
	[InlineData( "foo(?=bar)", "foobar", 0, 3 )]
	[InlineData( "foo(?!bar)", "foobaz", 0, 3 )]
	[InlineData( "(?<!foo)bar", "xxbar", 2, 3 )]
	[InlineData( "(?<!foo)bar", "bar", 0, 3 )]
	[InlineData( "(?!)", "", -1, 0 )]
	[InlineData( "(?<=)", "", 0, 0 )]
	[InlineData( "(?=)", "", 0, 0 )]
	[InlineData( "(?<!)", "", -1, 0 )]
	[InlineData( "(?=a)b", "b", -1, 0 )]
	[InlineData( "(?<=foo)bar", "bar", -1, 0 )]
	[InlineData( "(?<!foo)bar", "foobar", -1, 0 )]
	[InlineData( "foo(?!bar)", "foobar", -1, 0 )]
	public void LiteralAssertionsRetainBytePosition( string pattern, string subject, int index, int length ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : index, result.Match?.ByteIndex );
		if ( index >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void AssertionDoesNotAllocateCaptureAndAlternativeCanRetry() {
		var match = Compile( "(a)(?<=ab)c|(a)(?<=a)\\2"u8.ToArray() )
			.Match( "aa"u8.ToArray() ).Match;
		Assert.Equal( 0, match?.ByteIndex );
		Assert.Equal( 2, match?.ByteLength );
		Assert.Equal( 3, match?.Captures.Count );
		Assert.Null( match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 1 ), match?.Captures[2] );
	}

	[Fact]
	public void AssertionsCompareAuthoritativeNulAndHighBytes() {
		var pattern = new byte[] { (byte)'(', (byte)'?', (byte)'<', (byte)'=', 0, 0xff, (byte)')', (byte)'b' };
		var match = Compile( pattern ).Match( new byte[] { 0, 0xff, (byte)'b' } ).Match;
		Assert.Equal( 2, match?.ByteIndex );
		Assert.Equal( 1, match?.ByteLength );
		Assert.Single( match!.Captures );
	}

	[Fact]
	public void LiteralEscapesInsideAssertionDecodeToBytes() {
		var match = Compile( "(?<=\\x00\\xFF)b"u8.ToArray() )
			.Match( new byte[] { 0, 0xff, (byte)'b' } ).Match;
		Assert.Equal( 2, match?.ByteIndex );
		Assert.Equal( 1, match?.ByteLength );
	}

	[Theory]
	[InlineData( "(?<=a+)b", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "(?<=a|b)c", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "(?<=a(b))c", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "(?<=\\w)b", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "(?<=\\b)", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "(?<=\\k<x>)", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "(?<=a", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( "(?=a", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	public void DiagnosesBodiesOutsideTheLiteralSubset( string pattern, int code ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( (PcreByteDiagnosticCode)code, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void LiteralComparisonUsesExistingWorkBudget() {
		var result = Compile( "(?=abcdefghijklmnopqrst)"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 12 } )
			.Match( "abcdefghijklmnopqrst"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code );
	}

	[Fact]
	public void CancellationInterruptsLiteralComparison() {
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "(?=abcdefghijklmnopqrst)"u8.ToArray(), new PcreByteOptions {
			WorkObserver = count => { if ( count == 8 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match(
			"abcdefghijklmnopqrst"u8.ToArray(), cancellationToken: cancellation.Token
		) );
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( pattern, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
