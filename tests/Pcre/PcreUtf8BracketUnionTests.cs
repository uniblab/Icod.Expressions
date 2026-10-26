using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Union and complement semantics for supported UTF scalar bracket members.</summary>
public sealed class PcreUtf8BracketUnionTests {
	private static readonly PcreByteOptions GrepOptions = new() {
		Utf8 = true, Ucp = true, AsciiBsD = true, MatchInvalidUtf = true
	};

	[Theory]
	[InlineData( @"[é\d]", "é", 0, 2 )]
	[InlineData( @"[é\d]", "7", 0, 1 )]
	[InlineData( @"[é\d]", "٣", -1, 0 )]
	[InlineData( "[[:digit:]_]", "٣", 0, 2 )]
	[InlineData( "[[:digit:]_]", "_", 0, 1 )]
	[InlineData( "[a-z[:digit:]]", "٣", 0, 2 )]
	[InlineData( "[[:word:]0]", "é", 0, 2 )]
	[InlineData( @"[\p{Nd}A]", "٣", 0, 2 )]
	[InlineData( @"[\p{Nd}A]", "A", 0, 1 )]
	[InlineData( @"[\p{Nd}A]", "²", -1, 0 )]
	[InlineData( @"[\p{L}0]", "é", 0, 2 )]
	[InlineData( @"[\p{L}0]", "😀", -1, 0 )]
	[InlineData( @"[\P{L}é]", "7", 0, 1 )]
	[InlineData( @"[\P{L}é]", "é", 0, 2 )]
	[InlineData( @"[\P{L}é]", "à", -1, 0 )]
	[InlineData( @"[^\W_]", "é", 0, 2 )]
	[InlineData( @"[^\W_]", "_", -1, 0 )]
	[InlineData( @"[^\W_]", "😀", -1, 0 )]
	[InlineData( "[[:^digit:]_]", "😀", 0, 4 )]
	[InlineData( "[[:^digit:]_]", "٣", -1, 0 )]
	[InlineData( "[[:^digit:]_]", "_", 0, 1 )]
	[InlineData( "[^[:^digit:]_]", "٣", 0, 2 )]
	[InlineData( "[^[:^digit:]_]", "_", -1, 0 )]
	[InlineData( "[^[:^digit:]_]", "😀", -1, 0 )]
	[InlineData( @"[[:space:]\xFF]", "\u00a0", 0, 2 )]
	[InlineData( @"[[:space:]\xFF]", "ÿ", 0, 2 )]
	[InlineData( @"[\D]", "٣", 0, 2 )]
	[InlineData( @"[^\D]", "٣", -1, 0 )]
	[InlineData( @"[^\D]", "3", 0, 1 )]
	[InlineData( @"[\sA]", "\u00a0", 0, 2 )]
	public void UnionMatchesExactlyOneScalar( string pattern, string subject, int offset, int length ) {
		var result = Compile( pattern ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( offset < 0 ? null : new PcreByteCapture( offset, length ), result.Match?.Captures[0] );
	}

	[Theory]
	[InlineData( false, false, @"[\d_]", "٣", -1, 0 )]
	[InlineData( true, true, @"[\d_]", "٣", -1, 0 )]
	[InlineData( true, false, @"[\d_]", "٣", 0, 2 )]
	[InlineData( true, true, @"(?-aD)[\d_]", "٣", 0, 2 )]
	[InlineData( true, true, "[[:digit:]_]", "٣", 0, 2 )]
	[InlineData( false, true, "[[:digit:]_]", "٣", -1, 0 )]
	[InlineData( false, true, @"[\p{Nd}_]", "٣", 0, 2 )]
	[InlineData( false, true, @"[\w]", "é", -1, 0 )]
	[InlineData( true, true, @"[\w]", "é", 0, 2 )]
	public void OptionsApplyIndependentlyToEachPredicate(
		bool ucp, bool asciiBsD, string pattern, string subject, int offset, int length
	) {
		var result = Compile( pattern, new PcreByteOptions {
			Utf8 = true, Ucp = ucp, AsciiBsD = asciiBsD
		} ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( offset < 0 ? null : new PcreByteCapture( offset, length ), result.Match?.Captures[0] );
	}

	[Fact]
	public void PredicateTermsPreserveCaptureRollbackAndByteSpans() {
		var result = Compile( @"([[:digit:]]+)x|([\p{Nd}]+)y" ).Match( "٣y"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 0, 3 ), result.Match?.Captures[0] );
		Assert.Null( result.Match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 2 ), result.Match?.Captures[2] );
	}

	[Fact]
	public void NoPredicateMatchesMalformedByteOrCrossesFragment() {
		var malformed = Compile( @"[^\p{L}]" ).Match( Convert.FromHexString( "FF" ) );
		Assert.True( malformed.IsSuccess, malformed.Diagnostic?.Message );
		Assert.Null( malformed.Match );
		Assert.Equal( new PcreByteCapture( 1, 1 ),
			Compile( @"[^\p{L}]" ).Match( Convert.FromHexString( "FF37" ) ).Match?.Captures[0] );
		var crossed = Compile( @"[\p{L}]A" ).Match( Convert.FromHexString( "C3A9FF41" ) );
		Assert.True( crossed.IsSuccess, crossed.Diagnostic?.Message );
		Assert.Null( crossed.Match );
		var twiceNegated = Compile( "[^[:^digit:]_]" );
		foreach ( var subject in new[] { Convert.FromHexString( "FF" ), Array.Empty<byte>() } ) {
			var result = twiceNegated.Match( subject );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.Null( result.Match );
		}
		Assert.Equal( new PcreByteCapture( 1, 2 ),
			Compile( @"[\p{L}]" ).Match( Convert.FromHexString( "FFC3A9" ) ).Match?.Captures[0] );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset,
			Compile( @"[\p{L}]", new PcreByteOptions { Utf8 = true } )
				.Match( "é"u8.ToArray(), startOffset: 1 ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( @"[a-\w]", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( @"[\w-a]", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( "[[:digit:]-a]", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( @"[\p{L}-a]", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( "[[:nosuch:]_]", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "[[.ch.]]", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( @"[\p{Greek}]", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( @"[\B]", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	public void InvalidOrUnimplementedItemsAreDiagnosed( string pattern, int code ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), GrepOptions );
		Assert.Equal( (PcreByteDiagnosticCode)code, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void EachUnionTermObservesWorkAndCancellation() {
		var limited = Compile( @"[a\d\p{Nd}]", GrepOptions with { MatchWorkLimit = 7 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			limited.Match( "٣"u8.ToArray() ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( @"[a\d\p{Nd}]", GrepOptions with {
			WorkObserver = count => { if ( count == 7 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () =>
			cancellable.Match( "٣"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options ?? GrepOptions );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
