using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreByteLookbehindSearchTests {
	[Fact]
	public void RequiredLiteralAbsentAfterLookbehindAvoidsPerRecordVmState() {
		var pattern = Compile( "(?<=prefix-)TARGET"u8.ToArray() );
		var record = "record ordinary payload"u8.ToArray();
		Assert.Null( pattern.Match( record ).Match ); // Warm up the matching path.
		var before = GC.GetAllocatedBytesForCurrentThread();
		var matches = 0;
		for ( var index = 0; index < 4096; index++ ) {
			if ( pattern.Match( record ).Match is not null ) { matches++; }
		}
		var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.Equal( 0, matches );
		Assert.InRange( allocated, 0L, 900_000L );
	}

	[Fact]
	public void LowWorkLimitStillCountsAReachedLookbehindAndLiteral() {
		var input = "x"u8.ToArray();
		var pattern = "(?<=x)Z"u8.ToArray();
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, new PcreByteOptions { MatchWorkLimit = 6 } ).Match( input ).Diagnostic?.Code );
		foreach ( var limit in new[] { 7, 10 } ) {
			var result = Compile( pattern, new PcreByteOptions { MatchWorkLimit = limit } ).Match( input );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.Null( result.Match );
		}
	}

	[Fact]
	public void OtherAlternativeAndPresentLiteralStillUseTheVm() {
		Assert.Equal( 0, Compile( "(?<=x)Z|Q"u8.ToArray() ).Match( "Q"u8.ToArray() ).Match?.ByteIndex );
		Assert.Null( Compile( "(?<=x)Z"u8.ToArray() ).Match( "aZ"u8.ToArray() ).Match );
		Assert.Equal( 1, Compile( "(?<=x)Z"u8.ToArray() ).Match( "xZ"u8.ToArray() ).Match?.ByteIndex );
	}

	[Fact]
	public void ObservedSearchWithMissingLiteralStillSupportsCancellation() {
		using var cancellation = new CancellationTokenSource();
		var pattern = Compile( "(?<=x)Z"u8.ToArray(), new PcreByteOptions {
			WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>(
			() => pattern.Match( "x"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( byte[] source, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( source, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
