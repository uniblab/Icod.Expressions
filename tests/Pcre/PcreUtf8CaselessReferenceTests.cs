using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Variable-width Unicode case equivalence in captures and zero-width assertions.</summary>
public sealed class PcreUtf8CaselessReferenceTests {
	[Theory]
	[InlineData( "(?i)^(k)\\1$", "KK", 4, 1 )]
	[InlineData( "(?i)^(K)\\1$", "Kk", 4, 3 )]
	[InlineData( "(?i)^(É)\\1$", "éÉ", 4, 2 )]
	[InlineData( "(?i)^(Σ)\\1$", "σς", 4, 2 )]
	[InlineData( "(?i)^(s)\\1$", "Sſ", 3, 1 )]
	[InlineData( "(?i)^(k|K)\\1$", "KK", 4, 3 )]
	public void ReferenceConsumesSubjectScalarWidthsAndKeepsCaptureBytes(
		string pattern, string subject, int matchLength, int captureLength
	) {
		var result = Compile( pattern ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( 0, matchLength ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, captureLength ), result.Match?.Captures[1] );
	}

	[Fact]
	public void CallerCaselessOptionAppliesToReferenceWithoutLeadingFlag() {
		var result = Compile( "^(K)\\1$", new PcreByteOptions { Utf8 = true, Caseless = true } )
			.Match( "KK"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 0, 4 ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 3 ), result.Match?.Captures[1] );
	}

	[Theory]
	[InlineData( "(?i)^(I)\\1$", "Iı" )]
	[InlineData( "(?i)^(İ)\\1$", "İi" )]
	[InlineData( "(?i)^(k)\\1$", "Ks" )]
	[InlineData( "^(k)\\1$", "KK" )]
	public void ReferenceRejectsUnequalScalarsAndPreservesCasefulBehavior( string pattern, string subject ) {
		var result = Compile( pattern ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Null( result.Match );
	}

	[Theory]
	[InlineData( "(?i)(?<=k)A", "Ka", 3, 1 )]
	[InlineData( "(?i)(?<=K)A", "ka", 1, 1 )]
	[InlineData( "(?i)(?<=Ék)A", "éKa", 5, 1 )]
	[InlineData( "(?i)(?<=\\x4B)A", "Ka", 3, 1 )]
	[InlineData( "(?i)(?<!K)A", "ka", -1, 0 )]
	[InlineData( "(?i)(?<!K)A", "sa", 1, 1 )]
	[InlineData( "(?i)(?=K)k", "K", 0, 1 )]
	[InlineData( "(?i)(?!K)k", "K", -1, 0 )]
	[InlineData( "(?i)(?!K)s", "S", 0, 1 )]
	[InlineData( "(?i)(?=)", "K", 0, 0 )]
	[InlineData( "(?i)(?<=)", "K", 0, 0 )]
	[InlineData( "(?i)(?!)", "K", -1, 0 )]
	[InlineData( "(?i)(?<!)", "K", -1, 0 )]
	public void LiteralAssertionsCompareScalarCountsAndRemainZeroWidth(
		string pattern, string subject, int expectedOffset, int expectedLength
	) {
		var result = Compile( pattern ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( expectedOffset < 0 ? null : new PcreByteCapture( expectedOffset, expectedLength ),
			result.Match?.Captures[0] );
	}

	[Fact]
	public void BacktrackingRestoresCaptureAcrossCaselessReferenceAlternatives() {
		var result = Compile( "(?i)(Ka|k)\\1" ).Match( "kK"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 0, 4 ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match?.Captures[1] );
	}

	[Fact]
	public void NegativeAssertionsAndReferencesRespectMalformedUtfBarriers() {
		var subject = new byte[] { (byte)'k', 0xFF, (byte)'a' };
		var options = new PcreByteOptions { MatchInvalidUtf = true };
		Assert.Null( Compile( "(?i)(k)\\1", options ).Match( new byte[] {
			(byte)'k', 0xFF, 0xE2, 0x84, 0xAA
		} ).Match );
		Assert.Null( Compile( "(?i)(?<=K)A", options ).Match( subject ).Match );
		Assert.Equal( 2, Compile( "(?i)(?<!K)A", options ).Match( subject ).Match?.ByteIndex );
		Assert.Null( Compile( "(?i)k(?=K)", options ).Match( new byte[] {
			(byte)'k', 0xFF, 0xE2, 0x84, 0xAA
		} ).Match );
	}

	[Fact]
	public void ReferenceAndAssertionScalarComparisonsObserveWorkAndCancellation() {
		var repeated = new string( 'k', 80 );
		var captured = Encoding.UTF8.GetBytes( new string( 'K', 80 ) + new string( 'K', 80 ) );
		var referencePattern = "(?i)^(" + repeated + ")\\1$";
		var limit = new PcreByteOptions { Utf8 = true, MatchWorkLimit = 350 };
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( referencePattern, limit ).Match( captured ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( referencePattern, new PcreByteOptions {
			Utf8 = true, WorkObserver = count => { if ( count == 350 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () =>
			cancellable.Match( captured, cancellationToken: cancellation.Token ) );

		var assertionPattern = "(?i)(?=" + repeated + ")";
		var asserted = Encoding.UTF8.GetBytes( new string( 'K', 80 ) );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( assertionPattern, new PcreByteOptions { Utf8 = true, MatchWorkLimit = 125 } )
				.Match( asserted ).Diagnostic?.Code );
		using var assertionCancellation = new CancellationTokenSource();
		var cancellingAssertion = Compile( assertionPattern, new PcreByteOptions {
			Utf8 = true, WorkObserver = count => { if ( count == 125 ) { assertionCancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () =>
			cancellingAssertion.Match( asserted, cancellationToken: assertionCancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ),
			options ?? new PcreByteOptions { Utf8 = true } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
