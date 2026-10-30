using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Abbreviated PCRE2 Unicode general categories over UTF-8 scalars.</summary>
public sealed class PcreUtf8GeneralCategoryTests {
	[Theory]
	[InlineData( "C", "\u200d" )]
	[InlineData( "Cc", "\u0001" )]
	[InlineData( "Cf", "\u200d" )]
	[InlineData( "Cn", "\u0378" )]
	[InlineData( "Co", "\ue000" )]
	[InlineData( "L", "中" )]
	[InlineData( "Lc", "ǅ" )]
	[InlineData( "L&", "ǅ" )]
	[InlineData( "Ll", "é" )]
	[InlineData( "Lm", "ᵃ" )]
	[InlineData( "Lo", "中" )]
	[InlineData( "Lt", "ǅ" )]
	[InlineData( "Lu", "Ω" )]
	[InlineData( "M", "\u0301" )]
	[InlineData( "Mc", "\u093e" )]
	[InlineData( "Me", "\u20dd" )]
	[InlineData( "Mn", "\u0301" )]
	[InlineData( "N", "Ⅻ" )]
	[InlineData( "Nd", "٣" )]
	[InlineData( "Nl", "Ⅻ" )]
	[InlineData( "No", "²" )]
	[InlineData( "P", "。" )]
	[InlineData( "Pc", "_" )]
	[InlineData( "Pd", "‐" )]
	[InlineData( "Pe", ")" )]
	[InlineData( "Pf", "”" )]
	[InlineData( "Pi", "“" )]
	[InlineData( "Po", "!" )]
	[InlineData( "Ps", "(" )]
	[InlineData( "S", "😀" )]
	[InlineData( "Sc", "£" )]
	[InlineData( "Sk", "^" )]
	[InlineData( "Sm", "+" )]
	[InlineData( "So", "😀" )]
	[InlineData( "Z", "\u00a0" )]
	[InlineData( "Zl", "\u2028" )]
	[InlineData( "Zp", "\u2029" )]
	[InlineData( "Zs", "\u00a0" )]
	public void EachGeneralCategoryConsumesOneScalar( string category, string subject ) {
		var result = Compile( $"^\\p{{{category}}}$" ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( 0, Encoding.UTF8.GetByteCount( subject ) ), result.Match?.Captures[0] );
	}

	[Theory]
	[InlineData( @"\pN", "Ⅻ", true )]
	[InlineData( @"\PN", "Ⅻ", false )]
	[InlineData( @"\p{^N}", "Ⅻ", false )]
	[InlineData( @"\P{^N}", "Ⅻ", true )]
	[InlineData( @"\p{ ^ N}", "Ⅻ", false )]
	[InlineData( @"\P{ ^ N}", "Ⅻ", true )]
	[InlineData( @"\p{n-l}", "Ⅻ", true )]
	[InlineData( @"\p{G C = l u}", "Ω", true )]
	[InlineData( @"\p{General_Category: p o}", "!", true )]
	[InlineData( @"\p{L&}", "ᵃ", false )]
	[InlineData( @"\p{Cs}", "😀", false )]
	[InlineData( @"\P{Cs}", "😀", true )]
	[InlineData( @"\p{N}", "é", false )]
	[InlineData( @"\p{P}", "£", false )]
	[InlineData( @"\p{Z}", "\n", false )]
	[InlineData( @"\s", "\n", true )]
	public void NamesComplementsAndExistingPredicatesStayDistinct( string pattern, string subject, bool expected ) {
		foreach ( var ucp in new[] { false, true } ) {
			var result = Compile( pattern, new PcreByteOptions { Utf8 = true, Ucp = ucp } )
				.Match( Encoding.UTF8.GetBytes( subject ) );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.Equal( expected, result.Match is not null );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void Latin1CurrencySymbolIsNotPosixOrPropertyPunctuation( bool ucp ) {
		foreach ( var pattern in new[] { "[[:punct:]]", @"\p{P}" } ) {
			var result = Compile( pattern, new PcreByteOptions { Utf8 = true, Ucp = ucp } )
				.Match( "£"u8.ToArray() );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.Null( result.Match );
		}
	}

	[Theory]
	[InlineData( @"[\p{N}A]", "Ⅻ", true )]
	[InlineData( @"[\p{N}A]", "A", true )]
	[InlineData( @"[\p{N}A]", "😀", false )]
	[InlineData( @"[\p{Nl}]", "Ⅻ", true )]
	[InlineData( @"[\P{Nl}]", "Ⅻ", false )]
	[InlineData( @"[^\P{Nl}A]", "Ⅻ", true )]
	[InlineData( @"[^\P{Nl}A]", "A", false )]
	[InlineData( @"[^\P{ ^ N}A]", "😀", true )]
	[InlineData( @"[^\P{N}A]", "٣", true )]
	[InlineData( @"[^\P{N}A]", "A", false )]
	[InlineData( @"[^\P{N}A]", "😀", false )]
	public void BracketUnionAppliesBothComplementsInOrder( string pattern, string subject, bool expected ) {
		var result = Compile( pattern ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( expected, result.Match is not null );
	}

	[Fact]
	public void CapturesAndMalformedIslandsRetainSourceBytePositions() {
		var pattern = Compile( @"(\p{N}+)\1", new PcreByteOptions { MatchInvalidUtf = true } );
		var result = pattern.Match( Convert.FromHexString( "FF" ).Concat( "ⅫⅫ"u8.ToArray() ).ToArray() );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( 1, 6 ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 1, 3 ), result.Match?.Captures[1] );
		var barrier = Compile( @"\P{N}", new PcreByteOptions { MatchInvalidUtf = true } );
		Assert.Null( barrier.Match( Convert.FromHexString( "FF" ) ).Match );
		Assert.Null( barrier.Match( Array.Empty<byte>() ).Match );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset,
			Compile( @"\p{N}" ).Match( "Ⅻ"u8.ToArray(), startOffset: 1 ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( @"\p{Greek}", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( @"\p{gc:Greek}", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( @"\p{Script=Greek}", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( @"\p{Cq}", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( @"\p{Letter}", (int)PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( @"[a-\p{N}]", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( @"[\p{N}-a]", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( @"\p{_ - }", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( @"\p{gc:}", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( @"\p{General_Category= }", (int)PcreByteDiagnosticCode.InvalidSyntax )]
	public void InvalidAndOtherPropertiesRemainDiagnosed( string pattern, int code ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( (PcreByteDiagnosticCode)code, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void PropertyChecksObserveWorkAndCancellation() {
		var limited = Compile( @"\p{Nl}", new PcreByteOptions { Utf8 = true, MatchWorkLimit = 4 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			limited.Match( "Ⅻ"u8.ToArray() ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( @"\p{Nl}", new PcreByteOptions {
			Utf8 = true, WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () =>
			cancellable.Match( "Ⅻ"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void BracketSpecificCategoryChargesEachTerm() {
		var limited = Compile( @"[a\p{Nl}]", new PcreByteOptions { Utf8 = true, MatchWorkLimit = 6 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			limited.Match( "Ⅻ"u8.ToArray() ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( @"[a\p{Nl}]", new PcreByteOptions {
			Utf8 = true, WorkObserver = count => { if ( count == 6 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () =>
			cancellable.Match( "Ⅻ"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options ?? new PcreByteOptions { Utf8 = true } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
