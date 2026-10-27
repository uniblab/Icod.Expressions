using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreByteCapturedLeadingLiteralSearchTests {
	[Fact]
	public void MissingFirstCaptureLiteralAvoidsPerRecordVmState() {
		var pattern = Compile( "(TARGET)\\1"u8.ToArray() );
		var record = "record ordinary payload"u8.ToArray();
		Assert.Null( pattern.Match( record ).Match );
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
	public void SkipsFailedCandidatesBeforeARealCaptureAndHonorsStartOffset() {
		var pattern = Compile( "(T)\\1"u8.ToArray() );
		var input = "xxTTxxTT"u8.ToArray();
		Assert.Equal( 2, pattern.Match( input ).Match?.ByteIndex );
		var later = pattern.Match( input, startOffset: 3 ).Match;
		Assert.Equal( 6, later?.ByteIndex );
		Assert.Equal( new PcreByteCapture( 6, 1 ), later?.Captures[1] );
	}

	[Fact]
	public void ChargesCaptureStartAtEveryFailedCandidateIncludingTheEmptyOne() {
		var input = "aa"u8.ToArray();
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( "(T)\\1"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 11 } )
				.Match( input ).Diagnostic?.Code );
		var exact = Compile( "(T)\\1"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 12 } )
			.Match( input );
		Assert.True( exact.IsSuccess, exact.Diagnostic?.Message );
		Assert.Null( exact.Match );
		var observed = new List<int>();
		Assert.Null( Compile( "(T)\\1"u8.ToArray(), new PcreByteOptions {
			MatchWorkLimit = 12, WorkObserver = observed.Add
		} ).Match( input ).Match );
		Assert.Equal( Enumerable.Range( 1, 12 ), observed );
	}

	[Fact]
	public void AlternativesAndOptionalCapturesStillUseAllBranches() {
		Assert.Equal( 0, Compile( "(T|a)\\1"u8.ToArray() ).Match( "aa"u8.ToArray() ).Match?.ByteIndex );
		Assert.Equal( 0, Compile( "(T)?a"u8.ToArray() ).Match( "a"u8.ToArray() ).Match?.ByteIndex );
	}

	private static PcreCompiledBytePattern Compile( byte[] source, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( source, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
