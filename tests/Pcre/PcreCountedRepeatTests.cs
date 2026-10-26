using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreCountedRepeatTests {
	[Theory]
	[InlineData( "a{2}", "zaaa", 1, 2 )]
	[InlineData( "a{0}", "aaa", 0, 0 )]
	[InlineData( "a{2,4}", "aaaaa", 0, 4 )]
	[InlineData( "a{2,4}?", "aaaaa", 0, 2 )]
	[InlineData( "a{2,}", "aaaaa", 0, 5 )]
	[InlineData( "a{2,}?", "aaaaa", 0, 2 )]
	[InlineData( "a{,3}", "aaaaa", 0, 3 )]
	[InlineData( "a{,3}?", "aaaaa", 0, 0 )]
	[InlineData( "a{ 2 , 4 }", "aaaaa", 0, 4 )]
	[InlineData( "a{2,4}a", "aaaa", 0, 4 )]
	[InlineData( "a{2,4}?a", "aaaa", 0, 3 )]
	public void CountedRepeatsKeepOrderAndBounds( string pattern, string subject, int start, int length ) {
		var result = Compile( pattern ).Match( Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( start, length ), result.Match?.Captures[0] );
	}

	[Theory]
	[InlineData( "a{2,1}", "InvalidSyntax", 1 )]
	[InlineData( "a{65536}", "InvalidSyntax", 1 )]
	[InlineData( "a{999999999999999999999}", "InvalidSyntax", 1 )]
	[InlineData( "a{2,3", "InvalidSyntax", 1 )]
	[InlineData( "a{2}*", "InvalidSyntax", 4 )]
	[InlineData( "^{2}", "InvalidSyntax", 1 )]
	[InlineData( "a{foo}", "UnsupportedSyntax", 1 )]
	[InlineData( "a{2,3}+", "UnsupportedSyntax", 6 )]
	public void RejectsInvalidOrPendingBraceSyntax( string pattern, string code, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( code, result.Diagnostic?.Code.ToString() );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void BoundedGroupCanBacktrackAndPreserveLastCapture() {
		var result = Compile( "(ab|a){2}b" ).Match( "aabb"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 0, 4 ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 1, 2 ), result.Match?.Captures[1] );
	}

	[Fact]
	public void ZeroAndEmptyGroupIterationsPreserveCaptureSemantics() {
		Assert.Null( Compile( "(a){0}b" ).Match( "b"u8.ToArray() ).Match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 0 ),
			Compile( "(){2,}" ).Match( ReadOnlyMemory<byte>.Empty ).Match?.Captures[1] );
	}

	[Fact]
	public void NestedExpansionIsBoundedAtCompileTime() {
		var result = PcreByteCompiler.Compile( "(?:a{100}){100}"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, result.Diagnostic?.Code );
		Assert.Equal( 10, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void EmptyGroupExpansionHasACompileWorkLimit() {
		var result = PcreByteCompiler.Compile( "(?:){5000}"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, result.Diagnostic?.Code );
		Assert.Equal( 4, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void InstructionCapAtFinalAcceptReportsCountedQuantifier() {
		var result = PcreByteCompiler.Compile( "a{4096}"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, result.Diagnostic?.Code );
		Assert.Equal( 1, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void Utf8RepeatCountsScalarsAndKeepsByteSpans() {
		var result = Compile( "é{2,3}?x", new PcreByteOptions { Utf8 = true } )
			.Match( "zéééx"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 1, 7 ), result.Match?.Captures[0] );
	}

	[Fact]
	public void CountedRepetitionCannotCrossMalformedUtf8Barrier() {
		var compiled = Compile( "a{2}", new PcreByteOptions { MatchInvalidUtf = true } );
		Assert.Null( compiled.Match( new byte[] { (byte)'a', 0xFF, (byte)'a' } ).Match );
	}

	[Fact]
	public void ExpandedInstructionsStillSpendMatchWork() {
		var compiled = Compile( "a{20}z", new PcreByteOptions { MatchWorkLimit = 10 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			compiled.Match( Encoding.ASCII.GetBytes( new string( 'a', 20 ) ) ).Diagnostic?.Code );
	}

	[Fact]
	public void CompilationObservesCancellationInsideExpansion() {
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () =>
			PcreByteCompiler.Compile( "a{200}"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
