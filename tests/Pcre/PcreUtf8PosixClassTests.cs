using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 ordinary POSIX class membership over one UTF-8 scalar.</summary>
public sealed class PcreUtf8PosixClassTests {
	[Theory]
	[InlineData( "alnum", "é", true )]
	[InlineData( "alnum", "Ⅻ", true )]
	[InlineData( "alnum", "٣", true )]
	[InlineData( "alnum", "_", false )]
	[InlineData( "alpha", "é", true )]
	[InlineData( "alpha", "Ⅻ", false )]
	[InlineData( "ascii", "\u007f", true )]
	[InlineData( "ascii", "é", false )]
	[InlineData( "blank", "\u00a0", true )]
	[InlineData( "blank", "\u180e", true )]
	[InlineData( "blank", "\n", false )]
	[InlineData( "cntrl", "\u0085", true )]
	[InlineData( "cntrl", "\u180e", false )]
	[InlineData( "graph", "\u200d", true )]
	[InlineData( "graph", "\u061c", false )]
	[InlineData( "graph", "\u180e", false )]
	[InlineData( "graph", "\u2066", false )]
	[InlineData( "graph", "\u2069", false )]
	[InlineData( "graph", "\u00a0", false )]
	[InlineData( "lower", "ß", true )]
	[InlineData( "lower", "Σ", false )]
	[InlineData( "print", "\u00a0", true )]
	[InlineData( "print", "\u2028", false )]
	[InlineData( "punct", "£", true )]
	[InlineData( "punct", "€", false )]
	[InlineData( "punct", "。", true )]
	[InlineData( "upper", "Σ", true )]
	[InlineData( "upper", "ß", false )]
	[InlineData( "xdigit", "Ｆ", true )]
	[InlineData( "xdigit", "９", true )]
	[InlineData( "xdigit", "Ｇ", false )]
	[InlineData( "xdigit", "٣", false )]
	public void UcpClassUsesPcre2PropertyRules( string name, string subject, bool expected ) {
		var result = Compile( $"^[[:{name}:]]$", ucp: true ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( expected, result.Match is not null );
		if ( expected ) { Assert.Equal( Encoding.UTF8.GetByteCount( subject ), result.Match?.ByteLength ); }
	}

	[Theory]
	[InlineData( "alnum", "7", true )]
	[InlineData( "alpha", "B", true )]
	[InlineData( "ascii", "\0", true )]
	[InlineData( "blank", "\t", true )]
	[InlineData( "cntrl", "\u007f", true )]
	[InlineData( "graph", "!", true )]
	[InlineData( "lower", "b", true )]
	[InlineData( "print", " ", true )]
	[InlineData( "punct", "!", true )]
	[InlineData( "upper", "B", true )]
	[InlineData( "xdigit", "F", true )]
	[InlineData( "alnum", "Ⅻ", false )]
	[InlineData( "alpha", "é", false )]
	[InlineData( "ascii", "é", false )]
	[InlineData( "blank", "\u00a0", false )]
	[InlineData( "cntrl", "\u0085", false )]
	[InlineData( "graph", "£", false )]
	[InlineData( "lower", "ß", false )]
	[InlineData( "print", "£", false )]
	[InlineData( "punct", "£", false )]
	[InlineData( "upper", "Σ", false )]
	[InlineData( "xdigit", "Ｆ", false )]
	public void NonUcpClassUsesAsciiTables( string name, string subject, bool expected ) {
		var result = Compile( $"^[[:{name}:]]$", ucp: false ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( expected, result.Match is not null );
	}

	[Theory]
	[InlineData( "[a-z[:alpha:]]", "é", 0, 2 )]
	[InlineData( "[[:^alpha:]é]", "é", 0, 2 )]
	[InlineData( "[[:^alpha:]é]", "٣", 0, 2 )]
	[InlineData( "[[:^alpha:]é]", "à", -1, 0 )]
	[InlineData( "[^[:^alpha:]é]", "à", 0, 2 )]
	[InlineData( "[^[:^alpha:]é]", "é", -1, 0 )]
	[InlineData( "[[:ascii:]😀]", "😀", 0, 4 )]
	public void BracketUnionAndNegationRetainSourceByteSpans( string pattern, string subject,
		int offset, int length ) {
		var result = Compile( pattern, ucp: true ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( offset < 0 ? null : new PcreByteCapture( offset, length ), result.Match?.Captures[0] );
	}

	[Fact]
	public void NegatedClassesDoNotMatchMalformedBytesOrEof() {
		var pattern = Compile( "[^[:^alpha:]]", ucp: true, invalidUtf: true );
		foreach ( var bytes in new[] { Array.Empty<byte>(), Convert.FromHexString( "FF" ) } ) {
			var result = pattern.Match( bytes );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.Null( result.Match );
		}
		Assert.Equal( new PcreByteCapture( 1, 2 ), pattern.Match( Convert.FromHexString( "FFC3A9" ) ).Match?.Captures[0] );
	}

	[Theory]
	[InlineData( "[[:nosuch:]]", PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "[[.ch.]]", PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "[[:alpha:]-a]", PcreByteDiagnosticCode.InvalidSyntax )]
	[InlineData( "[a-[:alpha:]]", PcreByteDiagnosticCode.InvalidSyntax )]
	public void UnsupportedAndInvalidFormsRemainDiagnostics( string pattern, PcreByteDiagnosticCode code ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true, Ucp = true } );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void NewPredicateTermsAreBudgetedAndCancellable() {
		var limited = Compile( "[a[:alpha:][:xdigit:]]", ucp: true, limit: 7 );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			limited.Match( "Ｆ"u8.ToArray() ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = PcreByteCompiler.Compile( "[a[:alpha:][:xdigit:]]"u8.ToArray(),
			new PcreByteOptions { Utf8 = true, Ucp = true,
				WorkObserver = count => { if ( count == 7 ) { cancellation.Cancel(); } }
			} );
		Assert.True( compiled.IsSuccess, compiled.Diagnostic?.Message );
		Assert.Throws<OperationCanceledException>( () =>
			compiled.Pattern!.Match( "Ｆ"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, bool ucp, bool invalidUtf = false,
		int limit = int.MaxValue ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions {
			Utf8 = true, Ucp = ucp, MatchInvalidUtf = invalidUtf, MatchWorkLimit = limit
		} );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
