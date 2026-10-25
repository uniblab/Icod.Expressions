using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 invalid UTF subject barriers over the original source bytes.</summary>
public sealed class PcreUtf8InvalidSubjectTests {
	private static readonly PcreByteOptions InvalidUtf = new() {
		Utf8 = true, Ucp = true, AsciiBsD = true, MatchInvalidUtf = true
	};

	[Theory]
	[InlineData( "x", "78FF79", 0, 1 )]
	[InlineData( "y", "78FF79", 2, 1 )]
	[InlineData( "é", "FFC3A9FF", 1, 2 )]
	[InlineData( "A", "E28241", 2, 1 )]
	[InlineData( ".", "FFC3A9", 1, 2 )]
	[InlineData( "\\p{L}", "FFC3A9", 1, 2 )]
	[InlineData( "\\W", "FF20", 1, 1 )]
	[InlineData( "[^[:digit:]]", "FF79", 1, 1 )]
	[InlineData( ".", "F09F9880FF", 0, 4 )]
	public void SearchesOnlyValidScalarIslands( string pattern, string hex, int offset, int length ) {
		var result = Compile( pattern ).Match( Convert.FromHexString( hex ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( offset, length ), result.Match?.Captures[0] );
	}

	[Theory]
	[InlineData( "x.y", "78FF79" )]
	[InlineData( "\\P{L}", "FF" )]
	[InlineData( "\\W", "FF" )]
	[InlineData( "[^[:digit:]]", "FF" )]
	[InlineData( ".", "C0AF" )]
	[InlineData( ".", "EDB080" )]
	[InlineData( ".", "F4908080" )]
	[InlineData( "(x)\\1", "78FF78" )]
	public void NoConsumingAtomOrBackreferenceCrossesBarrier( string pattern, string hex ) {
		var compilation = PcreByteCompiler.Compile( System.Text.Encoding.UTF8.GetBytes( pattern ), InvalidUtf );
		Assert.True( compilation.IsSuccess, compilation.Diagnostic?.Message );
		var result = compilation.Pattern!.Match( Convert.FromHexString( hex ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Null( result.Match );
	}

	[Fact]
	public void CapturesAndBackreferencesRemainWithinOneIsland() {
		var compiled = Compile( @"(é+)\1" );
		var result = compiled.Match( Convert.FromHexString( "FFC3A9C3A9FFC3A9" ) );
		Assert.Equal( new PcreByteCapture( 1, 4 ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 1, 2 ), result.Match?.Captures[1] );
		Assert.Null( compiled.Match( Convert.FromHexString( "C3A9FFC3A9" ) ).Match );
	}

	[Theory]
	[InlineData( "\\b", "61FF62", 0 )]
	[InlineData( "a\\b", "61FF62", 0 )]
	[InlineData( "\\b", "FF61", 1 )]
	[InlineData( "\\B", "FF", 0 )]
	[InlineData( "\\z", "61FF62", 1 )]
	[InlineData( "\\Z", "61FF62", 1 )]
	[InlineData( "a\\z", "61FF62", 0 )]
	[InlineData( "b\\z", "61FF62", 2 )]
	[InlineData( "^", "FF61", 0 )]
	[InlineData( "$", "FF61", 2 )]
	[InlineData( "", "FF", 0 )]
	[InlineData( "", "80", 1 )]
	[InlineData( "\\z", "80FF41", 1 )]
	[InlineData( "\\B", "80", 1 )]
	public void ZeroWidthAssertionsRespectFragmentAndWholeSubjectEdges( string pattern, string hex, int offset ) {
		var result = Compile( pattern ).Match( Convert.FromHexString( hex ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( offset, pattern is "a\\z" or "b\\z" or "a\\b" ? 1 : 0 ),
			result.Match?.Captures[0] );
	}

	[Theory]
	[InlineData( "^b", "61FF62" )]
	[InlineData( "a$", "61FF62" )]
	[InlineData( "a.b", "61FF62" )]
	[InlineData( "^", "8061" )]
	[InlineData( "\\B", "61FF" )]
	[InlineData( "\\B", "61FFFF" )]
	[InlineData( "\\B", "61FFFF62" )]
	[InlineData( "$", "61FF" )]
	public void InvalidSequenceDoesNotBecomeLineOrCharacter( string pattern, string hex ) {
		Assert.Null( Compile( pattern ).Match( Convert.FromHexString( hex ) ).Match );
	}

	[Fact]
	public void MultilineStartDoesNotTreatInvalidFragmentEdgeAsNextLine() {
		var compiled = Compile( "(?m)^" );
		Assert.Null( compiled.Match( Convert.FromHexString( "0AFF" ), startOffset: 1 ).Match );
		Assert.Equal( 1, compiled.Match( Convert.FromHexString( "0A61" ), startOffset: 1 ).Match?.ByteIndex );
	}

	[Fact]
	public void StartOffsetsSkipContinuationAndMalformedBytes() {
		var subject = Convert.FromHexString( "C3A9FFE28241" );
		Assert.Equal( 5, Compile( "A" ).Match( subject, startOffset: 1 ).Match?.ByteIndex );
		Assert.Equal( 5, Compile( "A" ).Match( subject, startOffset: 3 ).Match?.ByteIndex );
		Assert.Equal( 2, Compile( "\\z" ).Match( subject, startOffset: 2 ).Match?.ByteIndex );
		Assert.Equal( 3, Compile( "" ).Match( subject, startOffset: 3 ).Match?.ByteIndex );
		Assert.Equal( 5, Compile( "" ).Match( subject, startOffset: 4 ).Match?.ByteIndex );
		Assert.Equal( 2, Compile( "\\z" ).Match( Convert.FromHexString( "6180" ), startOffset: 1 ).Match?.ByteIndex );
		Assert.Equal( 2, Compile( "\\B" ).Match( Convert.FromHexString( "6180" ), startOffset: 1 ).Match?.ByteIndex );
		Assert.Equal( 2, Compile( "\\B" ).Match( Convert.FromHexString( "61FF" ), startOffset: 2 ).Match?.ByteIndex );
		Assert.Equal( 2, Compile( "\\B" ).Match( Convert.FromHexString( "61FFFF" ), startOffset: 2 ).Match?.ByteIndex );
		Assert.Equal( 2, Compile( "\\B" ).Match( Convert.FromHexString( "6180FF" ), startOffset: 1 ).Match?.ByteIndex );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset,
			Compile( "A" ).Match( subject, startOffset: 7 ).Diagnostic?.Code );
	}

	[Fact]
	public void MatchInvalidUtfImpliesUtfButPatternMustStillBeValid() {
		var implied = PcreByteCompiler.Compile( "é"u8.ToArray(), new PcreByteOptions { MatchInvalidUtf = true } );
		Assert.True( implied.IsSuccess, implied.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( 1, 2 ),
			implied.Pattern!.Match( Convert.FromHexString( "FFC3A9" ) ).Match?.Captures[0] );
		var invalid = PcreByteCompiler.Compile( new byte[] { 0xFF }, InvalidUtf );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, invalid.Diagnostic?.Code );
	}

	[Fact]
	public void ScanHonorsWorkBudgetAndCancellation() {
		var limited = Compile( "A", InvalidUtf with { MatchWorkLimit = 2 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			limited.Match( Convert.FromHexString( "FFFF41" ) ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( "A", InvalidUtf with {
			WorkObserver = count => { if ( count == 2 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () =>
			cancellable.Match( Convert.FromHexString( "FFFF41" ), cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.UTF8.GetBytes( pattern ), options ?? InvalidUtf );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
