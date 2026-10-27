using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Numbered source-byte capture spans against Grep's pinned test-only reference.</summary>
public sealed class PcreReferenceCaptureTests {
	[Theory]
	[InlineData( "no-capture", "a", "7A61", false, false, 0, "1:1" )]
	[InlineData( "backreference-rollback", @"^(a|ab)\1$", "61626162", false, false, 0, "0:4;0:2" )]
	[InlineData( "nested-unset", "^((a)|ab)c$", "616263", false, false, 0, "0:3;0:2;-" )]
	[InlineData( "optional-unset", "^(a)?b$", "62", false, false, 0, "0:1;-" )]
	[InlineData( "optional-set", "^(a)?b$", "6162", false, false, 0, "0:2;0:1" )]
	[InlineData( "participating-empty", "^()a$", "61", false, false, 0, "0:1;0:0" )]
	[InlineData( "repeated-nested", "^((a)?b)+$", "616262", false, false, 0, "0:3;2:1;0:1" )]
	[InlineData( "repeat-backtrack", "(a){1,3}a", "616161", false, false, 0, "0:3;1:1" )]
	[InlineData( "unanchored-byte-start", "(a|ab)c", "7A616263", false, false, 0, "1:3;1:2" )]
	[InlineData( "prior-alternative", "^(?:a|ab)(b)c$", "61626263", false, false, 0, "0:4;2:1" )]
	[InlineData( "atomic-capture", "^(?>(ab|a))(c)$", "616263", false, false, 0, "0:3;0:2;2:1" )]
	[InlineData( "atomic-outer-rollback", "^(?:(?>(a|ab))d|abc)$", "616263", false, false, 0, "0:3;-" )]
	[InlineData( "byte-high-nul", @"^(\x{ff}\x{00})\1$", "FF00FF00", false, false, 0, "0:4;0:2" )]
	[InlineData( "utf-multibyte", "^(😀)(x)$", "F09F988078", true, false, 0, "0:5;0:4;4:1" )]
	[InlineData( "utf-lookbehind-start", "(?<=😀)(x)", "7AF09F988078", true, false, 1, "5:1;5:1" )]
	[InlineData( "utf-invalid-fragment", "(a)", "61FF61", true, true, 2, "2:1;2:1" )]
	public void MatchesPinnedReferenceNumberedByteCaptures( string name, string pattern,
		string subjectHex, bool utf8, bool invalidUtf, int startOffset, string expectedSpans ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var patternBytes = encoding.GetBytes( pattern );
		var subject = Convert.FromHexString( subjectHex );
		var nativeOptions = PcreOptions.Compiled;
		if ( utf8 ) { nativeOptions |= PcreOptions.Utf | PcreOptions.Ucp; }
		if ( invalidUtf ) { nativeOptions |= PcreOptions.MatchInvalidUtf; }
		var reference = new PcreRegex8Bit( patternBytes, encoding, new PcreRegexSettings {
			Options = nativeOptions,
			ExtraCompileOptions = utf8 ? PcreExtraCompileOptions.AsciiBsD : PcreExtraCompileOptions.None
		} );
		var native = reference.Match( subject, startOffset );
		Assert.True( native.Success, $"{name}: pinned reference did not match" );

		var compiled = PcreByteCompiler.Compile( patternBytes, new PcreByteOptions {
			Utf8 = utf8, Ucp = utf8, AsciiBsD = utf8, MatchInvalidUtf = invalidUtf
		} );
		Assert.True( compiled.IsSuccess, $"{name}: {compiled.Diagnostic?.Message}" );
		var managed = compiled.Pattern!.Match( subject, startOffset );
		Assert.True( managed.IsSuccess, $"{name}: {managed.Diagnostic?.Message}" );
		Assert.NotNull( managed.Match );

		var expected = expectedSpans.Split( ';' );
		Assert.Equal( expected.Length, native.Groups.Count );
		Assert.Equal( expected.Length, managed.Match.Captures.Count );
		for ( var groupIndex = 0; groupIndex < expected.Length; groupIndex++ ) {
			var referenceGroup = native.Groups[groupIndex];
			if ( expected[groupIndex] == "-" ) {
				Assert.False( referenceGroup.Success );
				Assert.Null( managed.Match.Captures[groupIndex] );
				continue;
			}
			var separator = expected[groupIndex].IndexOf( ':' );
			var start = int.Parse( expected[groupIndex].AsSpan( 0, separator ) );
			var length = int.Parse( expected[groupIndex].AsSpan( separator + 1 ) );
			Assert.True( referenceGroup.Success, $"{name}: native group {groupIndex} is unset" );
			Assert.Equal( start, referenceGroup.Index );
			Assert.Equal( length, referenceGroup.Length );
			Assert.Equal( new PcreByteCapture( start, length ), managed.Match.Captures[groupIndex] );
		}
	}
}
