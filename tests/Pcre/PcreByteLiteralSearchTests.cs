using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreByteLiteralSearchTests {
	[Fact]
	public void FindsLaterLiteralAfterAnEarlierPartialMatchAndHonorsStartOffset() {
		var pattern = Compile( "TARGET"u8.ToArray() );
		var input = "TAXxxTARGETxxTARGET"u8.ToArray();
		Assert.Equal( 5, pattern.Match( input ).Match?.ByteIndex );
		Assert.Equal( 13, pattern.Match( input, startOffset: 6 ).Match?.ByteIndex );
	}

	[Fact]
	public void ChargesFailedLiteralCandidatesIncludingTheFinalEmptyPosition() {
		var input = "aa"u8.ToArray();
		var limited = Compile( "z"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 8 } ).Match( input );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		var exact = Compile( "z"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 9 } ).Match( input );
		Assert.True( exact.IsSuccess, exact.Diagnostic?.Message );
		Assert.Null( exact.Match );
	}

	[Fact]
	public void ChargesSkippedCandidatesBeforeARealMatch() {
		var input = "xxT"u8.ToArray();
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( "T"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 9 } )
				.Match( input ).Diagnostic?.Code );
		Assert.Equal( 2, Compile( "T"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 10 } )
			.Match( input ).Match?.ByteIndex );
	}

	[Fact]
	public void ReportsEveryStepToAnObserver() {
		var observed = new List<int>();
		var pattern = Compile( "z"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 9,
			WorkObserver = observed.Add } );
		Assert.Null( pattern.Match( "aa"u8.ToArray() ).Match );
		Assert.Equal( Enumerable.Range( 1, 9 ), observed );
	}

	[Fact]
	public void SearchesForRawHighByteLiteral() {
		var pattern = Compile( new byte[] { 0xff, (byte)'X' } );
		var result = pattern.Match( new byte[] { 0x80, 0xfe, 0xff, (byte)'a', 0xff, (byte)'X' } );
		Assert.Equal( 4, result.Match?.ByteIndex );
	}

	[Fact]
	public void NoPrefixFromStartOffsetUsesTheSameWorkBudget() {
		var input = "aaa"u8.ToArray();
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( "z"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 5 } )
				.Match( input, startOffset: 2 ).Diagnostic?.Code );
		var exact = Compile( "z"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 6 } )
			.Match( input, startOffset: 2 );
		Assert.True( exact.IsSuccess, exact.Diagnostic?.Message );
		Assert.Null( exact.Match );
	}

	[Fact]
	public void RecordsWithoutTheFirstLiteralDoNotAllocateVmState() {
		var pattern = Compile( "TARGET"u8.ToArray() );
		var record = "ordinary payload"u8.ToArray();
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

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( pattern, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
