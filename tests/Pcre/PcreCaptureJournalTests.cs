using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreCaptureJournalTests {
	[Fact]
	public void FailedNestedBranchRestoresAllCaptureSlots() {
		var match = Compile( "^(?:(a)(b)|a(c))d$" ).Match( "acd"u8.ToArray() ).Match;
		Assert.Equal( new PcreByteCapture( 0, 3 ), match?.Captures[0] );
		Assert.Null( match?.Captures[1] );
		Assert.Null( match?.Captures[2] );
		Assert.Equal( new PcreByteCapture( 1, 1 ), match?.Captures[3] );
	}

	[Fact]
	public void RepeatRollbackRestoresPriorParticipatingNestedCapture() {
		var match = Compile( "^((a)?b){1,3}c$" ).Match( "abbc"u8.ToArray() ).Match;
		Assert.Equal( new PcreByteCapture( 0, 4 ), match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 2, 1 ), match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 1 ), match?.Captures[2] );
	}

	[Fact]
	public void AtomicPruningPreservesOuterFallbackAndDropsInnerCapture() {
		var match = Compile( "^(?:(?>(a|ab))c|a(b)c)$" ).Match( "abc"u8.ToArray() ).Match;
		Assert.Equal( new PcreByteCapture( 0, 3 ), match?.Captures[0] );
		Assert.Null( match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 1, 1 ), match?.Captures[2] );
	}

	[Fact]
	public void LaterCandidateStartsWithUnsetCapture() {
		var match = Compile( "(?:(a)b|a)c" ).Match( "abac"u8.ToArray() ).Match;
		Assert.Equal( new PcreByteCapture( 2, 2 ), match?.Captures[0] );
		Assert.Null( match?.Captures[1] );
	}

	[Fact]
	public void ManySavedPathsDoNotCopyEveryCaptureArray() {
		var pattern = Compile( "^" + string.Concat( Enumerable.Repeat( "()", 96 ) ) + "a{0,500}b$" );
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 500 ) + "b" );
		Assert.NotNull( pattern.Match( subject ).Match ); // Warm up JIT and the matching path.
		var before = GC.GetAllocatedBytesForCurrentThread();
		var result = pattern.Match( subject );
		var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.NotNull( result.Match );
		Assert.InRange( allocated, 0L, 350_000L );
	}

	private static PcreCompiledBytePattern Compile( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
