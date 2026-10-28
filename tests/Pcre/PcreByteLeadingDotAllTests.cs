using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 8-bit DotAll behavior for a leading inline option.</summary>
public sealed class PcreByteLeadingDotAllTests {
	[Theory]
	[InlineData( "a.b", "a\nb", -1, 0 )]
	[InlineData( "(?s)a.b", "a\nb", 0, 3 )]
	[InlineData( "(?s)^a\\nb$", "a\nb", 0, 3 )]
	[InlineData( "(?s)a.b|x.y", "x\ny", 0, 3 )]
	[InlineData( "(?s)", "", 0, 0 )]
	[InlineData( "(?s)\\.", "\n", -1, 0 )]
	[InlineData( "(?s)[.]", "\n", -1, 0 )]
	public void LeadingOptionChangesDotOnly( string pattern, string subject, int index, int length ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : index, result.Match?.ByteIndex );
		if ( index >= 0 ) {
			Assert.Equal( length, result.Match?.ByteLength );
			Assert.Single( result.Match!.Captures );
		}
	}

	[Fact]
	public void LeadingOptionPreservesCaptureNumbering() {
		var match = Compile( "(?s)(a).(b)"u8.ToArray() ).Match( "a\nb"u8.ToArray() ).Match;
		Assert.Equal( 3, match?.ByteLength );
		Assert.Equal( 3, match?.Captures.Count );
		Assert.Equal( new PcreByteCapture( 0, 1 ), match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 2, 1 ), match?.Captures[2] );
	}

	[Theory]
	[InlineData( "(?x).", 0 )]
	public void UnsupportedFlagPlacementIsDiagnosed( string pattern, int offset ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?s)\\.", "." )]
	[InlineData( "(?s)[.]", "." )]
	public void EscapedAndBracketDotsRemainLiteral( string pattern, string subject ) {
		var match = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) ).Match;
		Assert.Equal( 1, match?.ByteLength );
	}

	[Fact]
	public void DotAllStillMatchesRawNulAndHighBytes() {
		var match = Compile( "(?s).."u8.ToArray() ).Match( new byte[] { 0, 0xff } ).Match;
		Assert.Equal( 0, match?.ByteIndex );
		Assert.Equal( 2, match?.ByteLength );
	}

	[Fact]
	public void DotAllRetainsWorkLimitAndCancellation() {
		var limited = Compile( "(?s)a.b"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 4 } )
			.Match( "a\nb"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );

		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "(?s)a.b"u8.ToArray(), new PcreByteOptions {
			WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match(
			"a\nb"u8.ToArray(), cancellationToken: cancellation.Token
		) );
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( pattern, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
