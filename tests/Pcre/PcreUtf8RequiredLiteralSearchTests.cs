using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreUtf8RequiredLiteralSearchTests {
	[Fact]
	public void MissingUnicodeLiteralSuffixAvoidsPerRecordVmState() {
		var pattern = Compile( @"\p{L}+ 世界", new PcreByteOptions {
			Utf8 = true, Ucp = true, MatchInvalidUtf = true, AsciiBsD = true
		} );
		var record = "Καλημέρα ordinary payload"u8.ToArray();
		Assert.Equal( 0, pattern.Match( "Καλημέρα 世界 TARGET payload"u8.ToArray() ).Match?.ByteIndex );
		Assert.Null( pattern.Match( record ).Match );
		var before = GC.GetAllocatedBytesForCurrentThread();
		for ( var index = 0; index < 4096; index++ ) {
			Assert.Null( pattern.Match( record ).Match );
		}
		var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.InRange( allocated, 0L, 300_000L );
	}

	[Fact]
	public void InvalidUtfFragmentsCannotSupplyTheMissingSuffix() {
		var pattern = Compile( @"\p{L}+ 世界", new PcreByteOptions {
			Utf8 = true, Ucp = true, MatchInvalidUtf = true
		} );
		var result = pattern.Match( new byte[] { 0xCE, 0xBA, 0xFF, (byte)' ', (byte)'x' } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Null( result.Match );
	}

	[Fact]
	public void TolerantNoMatchRetainsLowWorkLimitAndObserverAccounting() {
		var input = new byte[] { 0xFF, (byte)'x' };
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( @"\p{L}+ 世界", new PcreByteOptions {
				Utf8 = true, Ucp = true, MatchInvalidUtf = true, MatchWorkLimit = 1
			} ).Match( input ).Diagnostic?.Code );
		var observed = new List<int>();
		var result = Compile( @"\p{L}+ 世界", new PcreByteOptions {
			Utf8 = true, Ucp = true, MatchInvalidUtf = true, WorkObserver = observed.Add
		} ).Match( input );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Null( result.Match );
		Assert.NotEmpty( observed );
	}

	[Fact]
	public void PresentSuffixStillUsesTheVmAndHonorsStartOffset() {
		var pattern = Compile( @"\p{L}+ 世界" );
		var input = "α Καλημέρα 世界 TARGET"u8.ToArray();
		var result = pattern.Match( input, startOffset: 3 );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( 3, result.Match?.ByteIndex );
		Assert.Equal( "Καλημέρα 世界"u8.Length, result.Match?.ByteLength );
		Assert.Null( pattern.Match( "Καλημέρα ordinary"u8.ToArray(), startOffset: 2 ).Match );
	}

	[Fact]
	public void AbsentSuffixStillValidatesUtf8AndHonorsWorkLimit() {
		var pattern = Compile( @"\p{L}+ 世界" );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject,
			pattern.Match( new byte[] { 0xCE, 0xBA, 0xFF } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset,
			pattern.Match( "Καλημέρα ordinary"u8.ToArray(), startOffset: 1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( @"\p{L}+ 世界", new PcreByteOptions { Utf8 = true, Ucp = true,
				MatchWorkLimit = 400 } ).Match( "Καλημέρα ordinary payload"u8.ToArray() ).Diagnostic?.Code );
	}

	[Fact]
	public void ObserverAndAlternationRemainAccountedFor() {
		using var cancellation = new CancellationTokenSource();
		var observed = Compile( @"\p{L}+ 世界", new PcreByteOptions { Utf8 = true, Ucp = true,
			WorkObserver = count => { if ( count == 20 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => observed.Match(
			"Καλημέρα ordinary payload"u8.ToArray(), cancellationToken: cancellation.Token ) );
		Assert.Equal( 0, Compile( @"\p{L}+ 世界|Καλημέρα" )
			.Match( "Καλημέρα ordinary"u8.ToArray() ).Match?.ByteIndex );
	}

	private static PcreCompiledBytePattern Compile( string source, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.UTF8.GetBytes( source ),
			options ?? new PcreByteOptions { Utf8 = true, Ucp = true } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
