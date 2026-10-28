using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Characterizes the internal saved-path ceiling independently of match work.</summary>
public sealed class PcreResourceContractTests {
	[Theory]
	[InlineData( 1024, true )]
	[InlineData( 1025, false )]
	public void BoundsLiveSavedPathsAtTheConfiguredCeiling( int choices, bool succeeds ) {
		var compiled = CompileLazyChoices( choices, new PcreByteOptions { MatchWorkLimit = 100_000 } );
		var result = compiled.Match( ReadOnlyMemory<byte>.Empty );

		Assert.Equal( succeeds, result.IsSuccess );
		if ( succeeds ) {
			Assert.Equal( 0, result.Match?.ByteLength );
			Assert.Null( result.Diagnostic );
		} else {
			Assert.Null( result.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code );
		}
	}

	[Fact]
	public void ReportsWorkExhaustionBeforeTheSavedPathCeiling() {
		var compiled = CompileLazyChoices( 1025, new PcreByteOptions { MatchWorkLimit = 100 } );
		var result = compiled.Match( ReadOnlyMemory<byte>.Empty );

		Assert.False( result.IsSuccess );
		Assert.Null( result.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code );
	}

	[Fact]
	public void CancellationInterruptsBeforeTheSavedPathCeiling() {
		using var cancellation = new CancellationTokenSource();
		var compiled = CompileLazyChoices( 1025, new PcreByteOptions {
			MatchWorkLimit = 100_000,
			WorkObserver = count => { if ( count == 100 ) cancellation.Cancel(); }
		} );

		Assert.Throws<OperationCanceledException>( () =>
			compiled.Match( ReadOnlyMemory<byte>.Empty, cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern CompileLazyChoices( int choices, PcreByteOptions options ) {
		var pattern = Encoding.ASCII.GetBytes( string.Concat( Enumerable.Repeat( "a??", choices ) ) );
		var compilation = PcreByteCompiler.Compile( pattern, options );
		Assert.True( compilation.IsSuccess, compilation.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( compilation.Pattern );
	}
}
