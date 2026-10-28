using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Leading ASCII caseless option in the provisional 8-bit, non-UTF matcher.</summary>
public sealed class PcreByteLeadingCaselessTests {
	[Theory]
	[InlineData( "abc", "xAbC", -1, 0 )]
	[InlineData( "(?i)abc", "xAbC", 1, 3 )]
	[InlineData( "(?i)a+", "aAA", 0, 3 )]
	[InlineData( "(?i)foo|bar", "BaR", 0, 3 )]
	[InlineData( "(?i)\\x41", "a", 0, 1 )]
	[InlineData( "(?i)", "", 0, 0 )]
	[InlineData( "(?i)[a-z]+", "ABC", 0, 3 )]
	[InlineData( "(?i)[^a]", "A", -1, 0 )]
	[InlineData( "(?i)[^a]", "B", 0, 1 )]
	[InlineData( "(?i)[[:lower:]]+", "ABC", 0, 3 )]
	[InlineData( "(?i)[[:^lower:]]", "A", -1, 0 )]
	[InlineData( "(?i)[[:^lower:]]", "!", 0, 1 )]
	[InlineData( "(?i)[\\W]", "A", -1, 0 )]
	[InlineData( "(?i)[\\W]", "!", 0, 1 )]
	[InlineData( "(?i)\\d", "A", -1, 0 )]
	public void AppliesAsciiCaseToLiteralsAndBracketComponents( string pattern, string subject, int index, int length ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : index, result.Match?.ByteIndex );
		if ( index >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Theory]
	[InlineData( "(?i)(a)\\1", "aA", 2 )]
	[InlineData( "(?i)(a)\\1", "Aa", 2 )]
	[InlineData( "(a)\\1", "aA", -1 )]
	public void ReferencesCompareSourceBytesInTheActiveCaseMode( string pattern, string subject, int length ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		var match = result.Match;
		Assert.Equal( length < 0 ? null : length, match?.ByteLength );
		if ( length >= 0 ) { Assert.Equal( new PcreByteCapture( 0, 1 ), match?.Captures[1] ); }
	}

	[Theory]
	[InlineData( "(?i)(?<=ab)CD", "aBcD", 2, 2 )]
	[InlineData( "(?i)(?=ab)AB", "aB", 0, 2 )]
	[InlineData( "(?i)(?!ab)AB", "aB", -1, 0 )]
	public void FixedLiteralAssertionsCompareAsciiCase( string pattern, string subject, int index, int length ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		var match = result.Match;
		Assert.Equal( index < 0 ? null : index, match?.ByteIndex );
		if ( index >= 0 ) { Assert.Equal( length, match?.ByteLength ); }
	}

	[Fact]
	public void LeavesHighBytesAndNulExactAndPreservesOffsets() {
		var pattern = new byte[] { (byte)'(', (byte)'?', (byte)'i', (byte)')', 0xff, 0, (byte)'a' };
		var match = Compile( pattern ).Match( new byte[] { 0xc0, 0xff, 0, (byte)'A' } ).Match;
		Assert.Equal( 1, match?.ByteIndex );
		Assert.Equal( 3, match?.ByteLength );
		var highByteResult = Compile( new byte[] { (byte)'(', (byte)'?', (byte)'i', (byte)')', 0xc0 } )
			.Match( new byte[] { 0xe0 } );
		Assert.True( highByteResult.IsSuccess, highByteResult.Diagnostic?.Message );
		Assert.Null( highByteResult.Match );
	}

	[Fact]
	public void CaselessReferenceComparisonRemainsBoundedAndCancellable() {
		var pattern = System.Text.Encoding.ASCII.GetBytes( "(?i)(" + new string( 'a', 128 ) + @")\1" );
		var input = System.Text.Encoding.ASCII.GetBytes( new string( 'a', 128 ) + new string( 'A', 128 ) );
		var limited = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 300 } ).Match( input );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );

		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions {
			WorkObserver = count => { if ( count == 300 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( input, cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( pattern, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
