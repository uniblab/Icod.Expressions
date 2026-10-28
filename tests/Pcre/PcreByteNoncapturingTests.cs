using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 8-bit default matching for groups that do not capture.</summary>
public sealed class PcreByteNoncapturingTests {
	[Theory]
	[InlineData( "(?:a|ab)c", "abc", 3 )]
	[InlineData( "(?:a|ab)c", "ac", 2 )]
	[InlineData( "(?:a|ab)", "ab", 1 )]
	[InlineData( "(?:ab)+", "abab", 4 )]
	[InlineData( "(?:a)?b", "b", 1 )]
	[InlineData( "(?:|a)b", "b", 1 )]
	[InlineData( "(?:a|)b", "b", 1 )]
	[InlineData( "(?:)", "", 0 )]
	[InlineData( "(?:a+?)b", "aaab", 4 )]
	public void GroupsWithoutCapturing( string pattern, string subject, int length ) {
		var match = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) ).Match;
		Assert.Equal( length, match?.ByteLength );
		Assert.Single( match!.Captures );
	}

	[Fact]
	public void NumbersOnlyTheCapturingGroupsIncludingNestedOnes() {
		var match = Compile( "(a)(?:b(c))\\2"u8.ToArray() ).Match( "abcc"u8.ToArray() ).Match;
		Assert.Equal( 4, match?.ByteLength );
		Assert.Equal( 3, match?.Captures.Count );
		Assert.Equal( new PcreByteCapture( 0, 1 ), match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 2, 1 ), match?.Captures[2] );
	}

	[Fact]
	public void RepeatedNoncapturingGroupRetainsLastNestedCapture() {
		var match = Compile( "(?:(a)|b)+\\1"u8.ToArray() ).Match( "aba"u8.ToArray() ).Match;
		Assert.Equal( 3, match?.ByteLength );
		Assert.Equal( new PcreByteCapture( 0, 1 ), match?.Captures[1] );
	}

	[Fact]
	public void AbandonedAlternativeRollsBackNestedCapture() {
		var match = Compile( "(?:(a)x|(ab)c)"u8.ToArray() ).Match( "abc"u8.ToArray() ).Match;
		Assert.Equal( 3, match?.ByteLength );
		Assert.Equal( 3, match?.Captures.Count );
		Assert.Null( match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 2 ), match?.Captures[2] );
	}

	[Fact]
	public void NoncapturingGroupsDoNotSpendCaptureSlots() {
		var pattern = System.Text.Encoding.ASCII.GetBytes(
			string.Concat( Enumerable.Repeat( "(?:a)", 256 ) ) + "(b)"
		);
		var match = Compile( pattern ).Match(
			System.Text.Encoding.ASCII.GetBytes( new string( 'a', 256 ) + "b" )
		).Match;
		Assert.Equal( 257, match?.ByteLength );
		Assert.Equal( 2, match?.Captures.Count );
		Assert.Equal( new PcreByteCapture( 256, 1 ), match?.Captures[1] );
	}

	[Fact]
	public void NoncapturingGroupsStillCountTowardNestingLimit() {
		var pattern = System.Text.Encoding.ASCII.GetBytes(
			string.Concat( Enumerable.Repeat( "(?:", 257 ) ) + "a" + new string( ')', 257 )
		);
		var result = PcreByteCompiler.Compile( pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, result.Diagnostic?.Code );
		Assert.Equal( 3 * 256, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void DoesNotTreatAnUnmatchedNestedCaptureAsASetBackreference() {
		Assert.Null( Compile( "(?:a|(b))\\1"u8.ToArray() ).Match( "a"u8.ToArray() ).Match );
	}

	[Fact]
	public void PreservesRawNulAndHighBytesAcrossRepeatedAlternatives() {
		var pattern = new byte[] { (byte)'(', (byte)'?', (byte)':', 0xFF, (byte)'|', 0x00, (byte)')', (byte)'+' };
		var match = Compile( pattern ).Match( new byte[] { 0xFF, 0x00, 0xFF } ).Match;
		Assert.Equal( 3, match?.ByteLength );
		Assert.Single( match!.Captures );
	}

	[Theory]
	[InlineData( "(?:a" )]
	[InlineData( "a(?:b" )]
	public void DiagnosesUnclosedGroupAtOpeningParenthesis( string pattern ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( pattern.IndexOf( '(' ), result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?=a|b)" )]
	public void OtherSpecialGroupsRemainUnsupported( string pattern ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void GroupAlternativesRemainWithinTheWorkLimit() {
		var compiled = Compile( "(?:a|aa)*b"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 40 } );
		var result = compiled.Match( System.Text.Encoding.ASCII.GetBytes( new string( 'a', 100 ) ) );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code );
	}

	[Fact]
	public void CancelsWhileMatchingGroupedAlternatives() {
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "(?:a|aa)*b"u8.ToArray(), new PcreByteOptions {
			WorkObserver = count => { if ( count == 40 ) cancellation.Cancel(); }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match(
			System.Text.Encoding.ASCII.GetBytes( new string( 'a', 100 ) ), cancellationToken: cancellation.Token
		) );
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( pattern, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
