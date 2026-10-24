using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Characterizes the internal PCRE byte-mode foundation without switching any consumer.</summary>
public sealed class PcreByteFoundationTests {
	[Fact]
	public void FindsLiteralAtAuthoritativeByteOffset() {
		var result = Compile( "ab"u8.ToArray() ).Match( "zab"u8.ToArray() );

		Assert.True( result.IsSuccess );
		Assert.Equal( 1, result.Match?.ByteIndex );
		Assert.Equal( 2, result.Match?.ByteLength );
	}

	[Theory]
	[InlineData( "a|ab", 1 )]
	[InlineData( "ab|a", 2 )]
	public void TriesAlternativesInSourceOrder( string pattern, int expectedLength ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) ).Match( "ab"u8.ToArray() );

		Assert.True( result.IsSuccess );
		Assert.Equal( 0, result.Match?.ByteIndex );
		Assert.Equal( expectedLength, result.Match?.ByteLength );
	}

	[Theory]
	[InlineData( "", 0 )]
	[InlineData( "ab|", 0 )]
	public void AllowsEmptyPatternAndBranch( string pattern, int expectedLength ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) ).Match( "z"u8.ToArray() );

		Assert.True( result.IsSuccess );
		Assert.Equal( 0, result.Match?.ByteIndex );
		Assert.Equal( expectedLength, result.Match?.ByteLength );
	}

	[Fact]
	public void PreservesNulAndHighByteWithoutTranscoding() {
		var pattern = new byte[] { 0xFF, 0x00, (byte)'b' };
		var input = new byte[] { (byte)'z', 0xFF, 0x00, (byte)'b' };
		var result = Compile( pattern ).Match( input );

		Assert.True( result.IsSuccess );
		Assert.Equal( 1, result.Match?.ByteIndex );
		Assert.Equal( 3, result.Match?.ByteLength );
	}

	[Fact]
	public void CopiesMutableCallerPattern() {
		var source = "ab"u8.ToArray();
		var compiled = Compile( source );
		source[0] = (byte)'x';

		var result = compiled.Match( "ab"u8.ToArray() );
		Assert.Equal( 0, result.Match?.ByteIndex );
		Assert.Equal( 2, result.Match?.ByteLength );
	}

	[Theory]
	[InlineData( "(?<=x)" )]
	[InlineData( "\\xFF" )]
	public void RejectsUnsupportedSyntaxAtCompileTime( string pattern ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );

		Assert.False( result.IsSuccess );
		Assert.Null( result.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( -1 )]
	[InlineData( 3 )]
	public void RejectsStartOutsideInput( int offset ) {
		var result = Compile( "a"u8.ToArray() ).Match( "ab"u8.ToArray(), offset );

		Assert.False( result.IsSuccess );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, result.Diagnostic?.Code );
	}

	[Fact]
	public void RejectsNonpositiveWorkLimit() {
		var result = PcreByteCompiler.Compile( "a"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 0 } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit, result.Diagnostic?.Code );
	}

	[Fact]
	public void LimitsWorkAcrossCandidateStarts() {
		var compiled = Compile( "zz"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 3 } );
		var result = compiled.Match( "aaaaaa"u8.ToArray() );

		Assert.False( result.IsSuccess );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code );
	}

	[Fact]
	public void ResetsWorkBudgetForEachMatch() {
		var compiled = Compile( "a"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 3 } );
		Assert.Equal( 0, compiled.Match( "a"u8.ToArray() ).Match?.ByteIndex );
		Assert.Equal( 0, compiled.Match( "a"u8.ToArray() ).Match?.ByteIndex );
	}

	[Fact]
	public void ReportsNoMatchSeparatelyFromResourceFailure() {
		var result = Compile( "z"u8.ToArray() ).Match( "a"u8.ToArray() );
		Assert.True( result.IsSuccess );
		Assert.Null( result.Match );
	}

	[Fact]
	public void StartsSearchAtRequestedByteOffset() {
		var result = Compile( "a"u8.ToArray() ).Match( "aaa"u8.ToArray(), 2 );
		Assert.Equal( 2, result.Match?.ByteIndex );
	}

	[Fact]
	public void ObservesAlreadyCanceledCompilationAndMatch() {
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "a"u8.ToArray() );
		cancellation.Cancel();

		Assert.Throws<OperationCanceledException>(
			() => PcreByteCompiler.Compile( "a"u8.ToArray(), cancellationToken: cancellation.Token )
		);
		Assert.Throws<OperationCanceledException>(
			() => compiled.Match( "a"u8.ToArray(), cancellationToken: cancellation.Token )
		);
	}

	[Fact]
	public void RollsBackGroupCaptureWhenAnEarlierBranchFails() {
		var result = Compile( "(a|ab)c"u8.ToArray() ).Match( "abc"u8.ToArray() );
		Assert.Equal( 0, result.Match?.ByteIndex );
		Assert.Equal( 3, result.Match?.ByteLength );
		Assert.Equal( new PcreByteCapture( 0, 3 ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 2 ), result.Match?.Captures[1] );
	}

	[Fact]
	public void NumbersCapturesByOpeningParenthesis() {
		var result = Compile( "(a)(b)"u8.ToArray() ).Match( "zab"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 1, 2 ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 1, 1 ), result.Match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 2, 1 ), result.Match?.Captures[2] );
	}

	[Theory]
	[InlineData( "a+", 3 )]
	[InlineData( "a+?", 1 )]
	public void OrdersGreedyAndLazyRepeatPaths( string pattern, int length ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) ).Match( "aaab"u8.ToArray() );
		Assert.Equal( length, result.Match?.ByteLength );
	}

	[Fact]
	public void EmptyRepeatTerminates() {
		var result = Compile( "(a?)*"u8.ToArray() ).Match( ReadOnlyMemory<byte>.Empty );
		Assert.Equal( 0, result.Match?.ByteLength );
	}

	[Theory]
	[InlineData( "^a", "ba", null )]
	[InlineData( "a$", "ba", 1 )]
	[InlineData( "^a$", "a", 0 )]
	public void AnchorsUseWholeInputBoundaries( string pattern, string input, int? start ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) ).Match( System.Text.Encoding.ASCII.GetBytes( input ) );
		Assert.True( result.IsSuccess );
		Assert.Equal( start, result.Match?.ByteIndex );
	}

	[Theory]
	[InlineData( "*a", 0 )]
	[InlineData( "a++", 2 )]
	[InlineData( "(a", 0 )]
	public void DiagnosesMalformedRepeatOrGroup( string pattern, int offset ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void BoundsNestedRepeats() {
		var compiled = Compile( "(a+)+$"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 100 } );
		var result = compiled.Match( "aaaaaaaaaaaaaaaaaaaaab"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code );
	}

	[Fact]
	public void CancelsDuringBacktrackingWithoutAClock() {
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "(a+)+$"u8.ToArray(), new PcreByteOptions {
			WorkObserver = count => { if ( count == 40 ) cancellation.Cancel(); }
		} );
		Assert.Throws<OperationCanceledException>(
			() => compiled.Match( "aaaaaaaaaaaaaaaaaaaaab"u8.ToArray(), cancellationToken: cancellation.Token )
		);
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( pattern, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
