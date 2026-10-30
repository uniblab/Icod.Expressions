using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Source-byte engine fixtures from Grep's current PCRE consumer tests.</summary>
public sealed class PcreGrepConsumerFixtureTests {
	[Theory]
	[InlineData( "utf-lookbehind", @"(?<=foo)bar", "666F6F626172", true, false, 0, 3, 3 )]
	[InlineData( "utf-backreference", @"^(\w+)\s+\1$", "73616D652073616D65", true, false, 0, 0, 9 )]
	[InlineData( "utf-backreference-miss", @"^(\w+)\s+\1$", "6E6F742073616D65", true, false, 0, -1, 0 )]
	[InlineData( "utf-ascii-digit-miss", @"^\d$", "D9A3", true, false, 0, -1, 0 )]
	[InlineData( "utf-posix-digit", "^[[:digit:]]$", "D9A3", true, false, 0, 0, 2 )]
	[InlineData( "utf-digit-override", @"(?-aD)^\d$", "D9A3", true, false, 0, 0, 2 )]
	[InlineData( "utf-letter-property", @"^\p{L}+$", "C3A9", true, false, 0, 0, 2 )]
	[InlineData( "byte-high", @"\xFF", "FF0A", false, false, 0, 0, 1 )]
	[InlineData( "byte-nul-pattern", "a\0b", "610062", false, false, 0, 0, 3 )]
	[InlineData( "utf-dotall-embedded-lf", "(?s)a.b", "610A62", true, false, 0, 0, 3 )]
	[InlineData( "utf-malformed-subject", "x", "78FF79", true, false, 0, 0, 1 )]
	[InlineData( "utf-caseless-lookbehind", @"(?<=foo)bar", "7878464F4F6241727A7A", true, true, 0, 5, 3 )]
	[InlineData( "byte-subsequent-offset", "bar", "626172626172", false, false, 3, 3, 3 )]
	public void GrepPatternReturnsExpectedSourceByteSpan( string name, string pattern,
		string subjectHex, bool utf8, bool caseless, int startOffset, int expectedIndex, int expectedLength ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var patternBytes = encoding.GetBytes( pattern );
		var subject = Convert.FromHexString( subjectHex );
		var nativeOptions = PcreOptions.Compiled;
		if ( utf8 ) { nativeOptions |= PcreOptions.Utf | PcreOptions.Ucp | PcreOptions.MatchInvalidUtf; }
		if ( caseless ) { nativeOptions |= PcreOptions.Caseless; }
		var reference = new PcreRegex8Bit( patternBytes, encoding, new PcreRegexSettings {
			Options = nativeOptions,
			ExtraCompileOptions = utf8 ? PcreExtraCompileOptions.AsciiBsD : PcreExtraCompileOptions.None
		} ).Match( subject, startOffset );
		Assert.Equal( expectedIndex, reference.Success ? reference.Index : -1 );
		if ( reference.Success ) { Assert.Equal( expectedLength, reference.Length ); }

		var compiled = PcreByteCompiler.Compile( patternBytes, new PcreByteOptions {
			Utf8 = utf8, Ucp = utf8, AsciiBsD = utf8, MatchInvalidUtf = utf8, Caseless = caseless
		} );
		Assert.True( compiled.IsSuccess, $"{name}: {compiled.Diagnostic?.Message}" );
		var managed = compiled.Pattern!.Match( subject, startOffset );
		Assert.True( managed.IsSuccess, $"{name}: {managed.Diagnostic?.Message}" );
		Assert.Equal( expectedIndex, managed.Match?.ByteIndex ?? -1 );
		if ( expectedIndex >= 0 ) { Assert.Equal( expectedLength, managed.Match?.ByteLength ); }
	}

	[Theory]
	[InlineData( "(?<=a+)b" )]
	public void PendingSyntaxIsExplicitlyRejected( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Null( result.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
	}

	[Fact]
	public void GrepInvalidGroupHasControlledCompileDiagnostic() {
		var result = PcreByteCompiler.Compile( "("u8.ToArray(), new PcreByteOptions {
			Utf8 = true, Ucp = true, AsciiBsD = true, MatchInvalidUtf = true
		} );
		Assert.Null( result.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
	}
}
