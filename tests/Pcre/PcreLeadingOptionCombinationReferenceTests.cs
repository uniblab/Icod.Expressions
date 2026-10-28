using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Leading option combinations and source-byte spans against the pinned reference.</summary>
public sealed class PcreLeadingOptionCombinationReferenceTests {
	[Theory]
	[InlineData( "byte-combined-captures", "(?is)^(a).(b)$", "410A42", false, false, 0, 0, 3 )]
	[InlineData( "byte-multiline-dotall", "(?ms)^a.b$", "780A610A62", false, false, 0, 2, 3 )]
	[InlineData( "byte-multiline-caseless", "(?im)^a$", "780A410A", false, false, 0, 2, 1 )]
	[InlineData( "utf-multiline-caseless", "(?im)^(é)$", "780AC3890A", true, false, 0, 2, 2 )]
	[InlineData( "utf-unset-dotall", "(?ims-s)^a.b$", "610A62", true, false, 0, -1, 0 )]
	[InlineData( "byte-repeated-prefixes", "(?s)(?i)^a.b$", "410A42", false, false, 0, 0, 3 )]
	[InlineData( "byte-unset-caller-caseless", "(?-i)^a$", "41", false, true, 0, -1, 0 )]
	[InlineData( "byte-unset-previous-caseless", "(?i)(?-i)^a$", "41", false, false, 0, -1, 0 )]
	[InlineData( "byte-reenable-caseless", "(?i)(?-i)(?i)^a$", "41", false, false, 0, 0, 1 )]
	[InlineData( "byte-repeated-flag-unset-wins", "(?i-i)a", "41", false, false, 0, -1, 0 )]
	[InlineData( "byte-unset-dotall", "(?s)(?-s)^a.b$", "610A62", false, false, 0, -1, 0 )]
	[InlineData( "byte-unset-multiline", "(?m)(?-m)^b", "610A62", false, false, 0, -1, 0 )]
	[InlineData( "byte-alternative-flags", "(?i-m)a|^b", "42", false, false, 0, 0, 1 )]
	[InlineData( "utf-aD-before-options", "(?-aD)(?i)^\\d$", "D9A3", true, false, 0, 0, 2 )]
	[InlineData( "utf-aD-after-options", "(?i)(?aD)^\\d$", "D9A3", true, false, 0, -1, 0 )]
	[InlineData( "utf-multiline-before-aD", "(?m)(?-aD)^\\d$", "780AD9A3", true, false, 0, 2, 2 )]
	[InlineData( "utf-aD-override", "(?aD)(?-aD)^\\d$", "D9A3", true, false, 0, 0, 2 )]
	public void LeadingOptionsMatchPinnedReference( string name, string pattern, string subjectHex,
		bool utf8, bool callerCaseless, int startOffset, int expectedIndex, int expectedLength ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var patternBytes = encoding.GetBytes( pattern );
		var subject = Convert.FromHexString( subjectHex );
		var nativeOptions = PcreOptions.Compiled;
		if ( callerCaseless ) { nativeOptions |= PcreOptions.Caseless; }
		if ( utf8 ) { nativeOptions |= PcreOptions.Utf | PcreOptions.Ucp | PcreOptions.MatchInvalidUtf; }
		var reference = new PcreRegex8Bit( patternBytes, encoding, new PcreRegexSettings {
			Options = nativeOptions,
			ExtraCompileOptions = utf8 ? PcreExtraCompileOptions.AsciiBsD : PcreExtraCompileOptions.None
		} ).Match( subject, startOffset );
		Assert.Equal( expectedIndex, reference.Success ? reference.Index : -1 );
		if ( reference.Success ) { Assert.Equal( expectedLength, reference.Length ); }

		var compiled = PcreByteCompiler.Compile( patternBytes, new PcreByteOptions {
			Caseless = callerCaseless, Utf8 = utf8, Ucp = utf8,
			AsciiBsD = utf8, MatchInvalidUtf = utf8
		} );
		Assert.True( compiled.IsSuccess, $"{name}: {compiled.Diagnostic?.Message}" );
		var managed = compiled.Pattern!.Match( subject, startOffset );
		Assert.True( managed.IsSuccess, $"{name}: {managed.Diagnostic?.Message}" );
		Assert.Equal( expectedIndex, managed.Match?.ByteIndex ?? -1 );
		if ( !reference.Success ) { Assert.Null( managed.Match ); return; }
		Assert.Equal( expectedLength, managed.Match?.ByteLength );
		Assert.Equal( reference.Groups.Count, managed.Match!.Captures.Count );
		for ( var group = 0; group < reference.Groups.Count; group++ ) {
			var native = reference.Groups[group];
			Assert.Equal( native.Success ? new PcreByteCapture( native.Index, native.Length ) : null,
				managed.Match.Captures[group] );
		}
	}

	[Theory]
	[InlineData( "(?im-x)a", 0 )]
	[InlineData( "(?i)(?x)a", 4 )]
	[InlineData( "(?im-aD)a", 0 )]
	public void OtherOptionSyntaxRemainsExplicitlyUnsupported( string pattern, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}
}
