using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 8-bit default matching for single-digit numeric references.</summary>
public sealed class PcreByteBackreferenceTests {
	[Theory]
	[InlineData( @"^(\w+)\s+\1$", "same same", 9 )]
	[InlineData( @"(a|ab)\1", "abab", 4 )]
	[InlineData( @"(a|b\1)+", "aba", 3 )]
	[InlineData( @"(a)\1", "aa", 2 )]
	[InlineData( @"()\1", "", 0 )]
	public void MatchesLastCapturedBytesWithPcre2Span( string pattern, string subject, int length ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( 0, result.Match?.ByteIndex );
		Assert.Equal( length, result.Match?.ByteLength );
	}

	[Theory]
	[InlineData( @"^(\w+)\s+\1$", "not same" )]
	[InlineData( @"(a)?b\1", "b" )]
	[InlineData( @"(a)\1", "ab" )]
	[InlineData( @"\1(a)", "a" )]
	public void UnsetOrDifferentCaptureDoesNotMatch( string pattern, string subject ) {
		Assert.Null( Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) ).Match );
	}

	[Fact]
	public void BacktracksCaptureBeforeComparingReference() {
		var match = Compile( "(a|ab)\\1"u8.ToArray() ).Match( "abab"u8.ToArray() ).Match;
		Assert.Equal( new PcreByteCapture( 0, 2 ), match?.Captures[1] );
	}

	[Fact]
	public void PreservesNulAndHighBytesInCaptureComparison() {
		var compiled = Compile( new byte[] { (byte)'(', 0xFF, 0x00, (byte)')', (byte)'\\', (byte)'1' } );
		var match = compiled.Match( new byte[] { 0xFF, 0x00, 0xFF, 0x00 } ).Match;
		Assert.Equal( 4, match?.ByteLength );
		Assert.Equal( new PcreByteCapture( 0, 2 ), match?.Captures[1] );
	}

	[Fact]
	public void AllowsForwardSingleDigitReferenceWhenGroupExists() {
		var result = PcreByteCompiler.Compile( "\\8(a)(b)(c)(d)(e)(f)(g)(h)"u8.ToArray() );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Null( result.Pattern?.Match( "abcdefgh"u8.ToArray() ).Match );
	}

	[Theory]
	[InlineData( @"\1" )]
	[InlineData( @"(a)\2" )]
	[InlineData( @"\7(a)" )]
	public void RejectsReferenceToNonexistentGroup( string pattern ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( pattern.IndexOf( '\\' ), result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"(a)\10" )]
	[InlineData( @"\0" )]
	[InlineData( @"[\1]" )]
	public void RetainsAmbiguousOrInClassNumbersAsUnsupported( string pattern ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( pattern.IndexOf( '\\' ), result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void CountsEachComparedReferenceByteAgainstWorkLimit() {
		var pattern = System.Text.Encoding.ASCII.GetBytes( "(" + new string( 'a', 128 ) + @")\1" );
		var compiled = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 300 } );
		var result = compiled.Match( System.Text.Encoding.ASCII.GetBytes( new string( 'a', 256 ) ) );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code );
	}

	[Fact]
	public void CancelsDuringReferenceComparison() {
		using var cancellation = new CancellationTokenSource();
		var pattern = System.Text.Encoding.ASCII.GetBytes( "(" + new string( 'a', 128 ) + @")\1" );
		var compiled = Compile( pattern, new PcreByteOptions {
			WorkObserver = count => { if ( count == 300 ) cancellation.Cancel(); }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match(
			System.Text.Encoding.ASCII.GetBytes( new string( 'a', 256 ) ), cancellationToken: cancellation.Token
		) );
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( pattern, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
