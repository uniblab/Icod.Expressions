using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Representative valid PCRE2 families deliberately outside the internal subset.</summary>
public sealed class PcreSyntaxAcceptanceTests {
	[Theory]
	[InlineData( "(?C2)", 0, false )]
	[InlineData( "(?C2)(*FAIL)", 0, false )]
	[InlineData( "(*LIMIT_DEPTH=10)a", 0, false )]
	[InlineData( "(*LIMIT_MATCH=10)a", 0, false )]
	[InlineData( "(*LIMIT_HEAP=10)a", 0, false )]
	[InlineData( "(*NO_JIT)a", 0, false )]
	[InlineData( "(*NO_START_OPT)a", 0, true )]
	[InlineData( "a(?C2)", 1, false )]
	[InlineData( "(?i)(?C2)", 4, false )]
	[InlineData( "(?C3)", 0, false )]
	[InlineData( "(?C1)a", 0, false )]
	[InlineData( @"\C", 0, true )]
	[InlineData( "(?C2)a", 0, true )]
	public void PendingConstructHasExplicitOpeningByteDiagnostic( string pattern, int expectedOffset, bool utf8 ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions {
			Utf8 = utf8, Ucp = utf8, AsciiBsD = utf8, MatchInvalidUtf = utf8
		} );
		Assert.Null( result.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( expectedOffset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void PinnedReferenceAcceptsPendingCalloutWhichInternalCompilerRejects() {
		const string pattern = "a(?C2)";
		var bytes = Encoding.ASCII.GetBytes( pattern );
		var reference = new PcreRegex8Bit( bytes, Encoding.Latin1, new PcreRegexSettings {
			Options = PcreOptions.Compiled
		} ).Match( "a"u8.ToArray() );
		Assert.True( reference.Success );
		Assert.Equal( 0, reference.Index );
		Assert.Equal( 1, reference.Length );
		var result = PcreByteCompiler.Compile( bytes );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 1, result.Diagnostic?.ByteOffset );
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
