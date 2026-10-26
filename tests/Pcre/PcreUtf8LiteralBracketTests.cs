using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Case-sensitive UTF-8 bracket literals and scalar ranges.</summary>
public sealed class PcreUtf8LiteralBracketTests {
	private static readonly PcreByteOptions GrepOptions = new() {
		Utf8 = true, Ucp = true, AsciiBsD = true, MatchInvalidUtf = true
	};

	[Theory]
	[InlineData( "[é😀]", "x😀", 1, 4 )]
	[InlineData( "[é😀]", "é", 0, 2 )]
	[InlineData( "[é😀]", "x", -1, 0 )]
	[InlineData( "[à-ö]", "é", 0, 2 )]
	[InlineData( "[0-9]", "3", 0, 1 )]
	[InlineData( "[0-9]", "٣", -1, 0 )]
	[InlineData( "[à-ö]", "ö", 0, 2 )]
	[InlineData( "[à-ö]", "÷", -1, 0 )]
	[InlineData( "[😀-🙏]", "😁", 0, 4 )]
	[InlineData( "[😀-🙏]", "🛐", -1, 0 )]
	[InlineData( @"[\xFF]", "ÿ", 0, 2 )]
	[InlineData( @"[\x80-\xFF]", "é", 0, 2 )]
	[InlineData( @"[\x80-\xFF]", "٣", -1, 0 )]
	[InlineData( "[^é]", "😀", 0, 4 )]
	[InlineData( "[^é]", "é", -1, 0 )]
	[InlineData( "[^é]", "", -1, 0 )]
	[InlineData( "[]]", "]", 0, 1 )]
	[InlineData( "[^]]", "]", -1, 0 )]
	[InlineData( "[[]", "[", 0, 1 )]
	[InlineData( "[-a]", "-", 0, 1 )]
	[InlineData( "[a-]", "-", 0, 1 )]
	[InlineData( "[a-c-z]", "-", 0, 1 )]
	[InlineData( "[a-c-z]", "z", 0, 1 )]
	[InlineData( @"[\[\]\-]", "-", 0, 1 )]
	[InlineData( @"[\b]", "\b", 0, 1 )]
	public void BracketConsumesOneWholeScalar(
		string pattern, string subject, int index, int length
	) {
		var result = Compile( pattern ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : new PcreByteCapture( index, length ), result.Match?.Captures[0] );
	}

	[Fact]
	public void RepeatedClassCapturesOriginalBytesAndRestoresFailedAlternative() {
		var repeated = Compile( "([é😀]+)" ).Match( "é😀x"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 0, 6 ), repeated.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 6 ), repeated.Match?.Captures[1] );
		var fallback = Compile( "([é]+)x|([à-ö]+)y" ).Match( "éy"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 0, 3 ), fallback.Match?.Captures[0] );
		Assert.Null( fallback.Match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 2 ), fallback.Match?.Captures[2] );
	}

	[Fact]
	public void NegationCannotConsumeMalformedByteOrCrossBarrier() {
		var compiled = Compile( "[^é]" );
		Assert.Equal( new PcreByteCapture( 1, 1 ),
			compiled.Match( Convert.FromHexString( "FF41" ) ).Match?.Captures[0] );
		Assert.Null( compiled.Match( Convert.FromHexString( "FF" ) ).Match );
		Assert.Null( Compile( "[é]A" ).Match( Convert.FromHexString( "C3A9FF41" ) ).Match );
		Assert.Equal( new PcreByteCapture( 1, 2 ),
			Compile( "[é]" ).Match( Convert.FromHexString( "FFC3A9" ) ).Match?.Captures[0] );
		Assert.Null( Compile( @"[\xFF]" ).Match( Convert.FromHexString( "FF" ) ).Match );
	}

	[Fact]
	public void StartOffsetRespectsUtf8Boundaries() {
		var strict = Compile( "[é]", new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset,
			strict.Match( "é"u8.ToArray(), startOffset: 1 ).Diagnostic?.Code );
		Assert.Equal( 2, Compile( "[😀]" ).Match( "é😀"u8.ToArray(), startOffset: 1 ).Match?.ByteIndex );
	}

	[Theory]
	[InlineData( @"[\w]", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( @"[\p{L}]", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( @"[a-\w]", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "[a[:digit:]]", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "[[:digit:]a]", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "[z-a]", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( "[é", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( "[a-", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	public void UnsupportedOrMalformedBracketIsDiagnosedAtOpeningByte( string pattern, int code ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), GrepOptions );
		Assert.Equal( (PcreByteDiagnosticCode)code, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void ScalarSetEvaluationIsBudgetedAndCancellable() {
		var limited = Compile( "[é]", GrepOptions with { MatchWorkLimit = 4 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			limited.Match( "é"u8.ToArray() ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( "[é]", GrepOptions with {
			WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () =>
			cancellable.Match( "é"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void EachRangeInspectionObservesWorkAndCancellation() {
		var limited = Compile( "[abcde]", GrepOptions with { MatchWorkLimit = 9 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			limited.Match( "e"u8.ToArray() ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( "[abcde]", GrepOptions with {
			WorkObserver = count => { if ( count == 7 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () =>
			cancellable.Match( "e"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options ?? GrepOptions );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
