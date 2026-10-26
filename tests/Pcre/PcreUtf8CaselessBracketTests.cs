using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Case closure of ordinary UTF-8 bracket members in the internal matcher.</summary>
public sealed class PcreUtf8CaselessBracketTests {
	[Theory]
	[InlineData( "(?i)[a-z]", "K", 0, 3 )]
	[InlineData( "(?i)[K]", "K", 0, 3 )]
	[InlineData( "(?i)[K]", "xK", 1, 1 )]
	[InlineData( "(?i)[S]", "ſ", 0, 2 )]
	[InlineData( "(?i)[Σ]", "ς", 0, 2 )]
	[InlineData( "(?i)[ǅ]", "ǆ", 0, 2 )]
	[InlineData( "(?i)[W-c]", "w", 0, 1 )]
	[InlineData( "(?i)[W-c]", "Z", 0, 1 )]
	[InlineData( "(?i)[W-c]", "d", -1, 0 )]
	[InlineData( "(?i)[I]", "ı", -1, 0 )]
	[InlineData( "(?i)[İ]", "i", -1, 0 )]
	[InlineData( "(?i)[^k]", "K", -1, 0 )]
	[InlineData( "(?i)[^k]", "!", 0, 1 )]
	[InlineData( "[k]", "K", -1, 0 )]
	public void FoldsLiteralAndRangeMembersButKeepsOriginalByteSpan(
		string pattern, string subject, int offset, int length
	) {
		var result = Compile( pattern ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( offset < 0 ? null : new PcreByteCapture( offset, length ), result.Match?.Captures[0] );
	}

	[Fact]
	public void CallerCaselessOptionAndSurrogateSpanningRangeKeepScalarBoundaries() {
		var caller = Compile( "[k]", new PcreByteOptions { Utf8 = true, Caseless = true } );
		Assert.Equal( new PcreByteCapture( 1, 3 ), caller.Match( "xK"u8.ToArray() ).Match?.Captures[0] );
		var range = Compile( "(?i)[\uD7FF-\uE000]" );
		Assert.Equal( 3, range.Match( Encoding.UTF8.GetBytes( "\uE000" ) ).Match?.ByteLength );
		Assert.Equal( 3, range.Match( Encoding.UTF8.GetBytes( "\uD7FF" ) ).Match?.ByteLength );
		Assert.Null( range.Match( "x"u8.ToArray() ).Match );
	}

	[Fact]
	public void OverlappingRangesAndFullSpaceUnionKeepTheirMembers() {
		var overlap = Compile( "(?i)[a-mh-z]" );
		Assert.Equal( 3, overlap.Match( "K"u8.ToArray() ).Match?.ByteLength );
		Assert.Null( overlap.Match( "!"u8.ToArray() ).Match );
		var full = Compile( "(?i)[\0-\U0010FFFF\0-\U0010FFFF]" );
		Assert.Equal( 4, full.Match( "\U0010FFFF"u8.ToArray() ).Match?.ByteLength );
	}

	[Fact]
	public void WideRangeCompilationHonorsCancellation() {
		using var cancellation = new CancellationTokenSource();
		cancellation.CancelAfter( TimeSpan.FromMilliseconds( 1 ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.UTF8.GetBytes( "(?i)[A-\U0010FFFF]" ),
			new PcreByteOptions { Utf8 = true }, cancellation.Token
		) );
	}

	[Theory]
	[InlineData( "(?i)[\\p{Lu}]", "a", false, 0, 1 )]
	[InlineData( "(?i)[\\p{Ll}]", "ǅ", false, 0, 2 )]
	[InlineData( "(?i)[\\p{Lt}]", "ǆ", false, 0, 2 )]
	[InlineData( "(?i)[\\P{Lu}]", "a", false, -1, 0 )]
	[InlineData( "(?i)[^\\p{Ll}]", "A", false, -1, 0 )]
	[InlineData( "(?i)[^\\P{Lu}]", "a", false, 0, 1 )]
	[InlineData( "(?i)[[:lower:]]", "A", false, 0, 1 )]
	[InlineData( "(?i)[[:upper:]]", "a", false, 0, 1 )]
	[InlineData( "(?i)[[:lower:]]", "K", false, -1, 0 )]
	[InlineData( "(?i)[[:lower:]]", "K", true, 0, 3 )]
	[InlineData( "(?i)[[:upper:]]", "ǆ", true, 0, 2 )]
	[InlineData( "(?i)[[:^lower:]]", "A", false, -1, 0 )]
	[InlineData( "(?i)[[:^lower:]]", "K", false, 0, 3 )]
	[InlineData( "(?i)[[:^upper:]]", "ǆ", true, -1, 0 )]
	[InlineData( "(?i)[^[:^upper:]]", "a", true, 0, 1 )]
	[InlineData( "(?i)[\\w]", "é", false, -1, 0 )]
	[InlineData( "(?i)[\\w]", "é", true, 0, 2 )]
	[InlineData( "(?i)[\\dK]", "K", false, 0, 3 )]
	[InlineData( "(?i)[\\d]", "٣", false, -1, 0 )]
	[InlineData( "(?i)[\\d]", "٣", true, 0, 2 )]
	public void PredicatesPreserveTheirUcpAndComplementPolicies(
		string pattern, string subject, bool ucp, int offset, int length
	) {
		var result = Compile( pattern, new PcreByteOptions { Utf8 = true, Ucp = ucp } )
			.Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( offset < 0 ? null : new PcreByteCapture( offset, length ), result.Match?.Captures[0] );
	}

	[Fact]
	public void CannotConsumeMalformedUtf8AcrossOrWithinAFragment() {
		var options = new PcreByteOptions { MatchInvalidUtf = true, Caseless = true };
		var input = new byte[] { (byte)'k', 0xFF, 0xE2, 0x84, 0xAA };
		var repeated = Compile( "[k]+", options );
		Assert.Equal( new PcreByteCapture( 0, 1 ), repeated.Match( input ).Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 2, 3 ), repeated.Match( input, startOffset: 2 ).Match?.Captures[0] );
		Assert.Null( Compile( "[k][k]", options ).Match( input ).Match );
	}

	[Fact]
	public void EachAttemptedFoldedIntervalSpendsWorkAndCanBeCancelled() {
		var subject = Encoding.UTF8.GetBytes( new string( 'z', 30 ) );
		var pattern = "(?i)[acegikmoqsuwy]";
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, new PcreByteOptions { Utf8 = true, MatchWorkLimit = 45 } )
				.Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions {
			Utf8 = true, WorkObserver = count => { if ( count == 45 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () =>
			compiled.Match( subject, cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ),
			options ?? new PcreByteOptions { Utf8 = true } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
