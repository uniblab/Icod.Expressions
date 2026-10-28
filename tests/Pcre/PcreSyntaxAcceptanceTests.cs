using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Representative valid PCRE2 families deliberately outside the internal subset.</summary>
public sealed class PcreSyntaxAcceptanceTests {
	[Theory]
	[InlineData( "(*ACCEPT)", 0, false )]
	[InlineData( "(*SKIP)(*FAIL)", 0, false )]
	[InlineData( "(*ANYCRLF)a", 0, false )]
	[InlineData( "(*LIMIT_MATCH=10)a", 0, false )]
	[InlineData( "(*BSR_ANYCRLF)a", 0, false )]
	[InlineData( "(*atomic:a)", 0, false )]
	[InlineData( "(*pla:a)", 0, false )]
	[InlineData( "(*UTF)a", 0, false )]
	[InlineData( "(*UCP)a", 0, true )]
	[InlineData( "a(*SKIP)", 1, false )]
	[InlineData( "(?i)(*FAIL)", 4, false )]
	[InlineData( "(?i:a)", 0, false )]
	[InlineData( "(?#comment)a", 0, false )]
	[InlineData( "(?[a|b])", 0, false )]
	[InlineData( "(a)(?(1)b|c)", 3, false )]
	[InlineData( "(?R)", 0, false )]
	[InlineData( "(?C1)a", 0, false )]
	[InlineData( "(?<=[ab])c", 0, false )]
	[InlineData( @"(a)\10", 3, false )]
	[InlineData( @"(a)\g<1>", 3, false )]
	[InlineData( @"\C", 0, false )]
	[InlineData( @"\p{Greek}", 0, true )]
	public void PendingConstructHasExplicitOpeningByteDiagnostic( string pattern, int expectedOffset, bool utf8 ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions {
			Utf8 = utf8, Ucp = utf8, AsciiBsD = utf8, MatchInvalidUtf = utf8
		} );
		Assert.Null( result.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( expectedOffset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void PinnedReferenceAcceptsBacktrackingVerbWhichInternalCompilerRejects() {
		const string pattern = "(*FAIL)|a";
		var bytes = Encoding.ASCII.GetBytes( pattern );
		var reference = new PcreRegex8Bit( bytes, Encoding.Latin1, new PcreRegexSettings {
			Options = PcreOptions.Compiled
		} ).Match( "a"u8.ToArray() );
		Assert.True( reference.Success );
		Assert.Equal( 0, reference.Index );
		Assert.Equal( 1, reference.Length );
		var result = PcreByteCompiler.Compile( bytes );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "a(", 1 )]
	[InlineData( "a\\", 1 )]
	[InlineData( "(*", 1 )]
	public void MalformedPatternRetainsInvalidSyntaxDiagnostic( string pattern, int expectedOffset ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( expectedOffset, result.Diagnostic?.ByteOffset );
	}
}
