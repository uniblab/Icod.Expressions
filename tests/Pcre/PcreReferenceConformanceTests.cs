using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Whole-match byte spans against Grep's pinned PCRE.NET reference, in tests only.</summary>
public sealed class PcreReferenceConformanceTests {
	[Theory]
	[InlineData( "byte-braced-space", "\\x{ 41 }", "41", false, false, false, 0, 0, 1 )]
	[InlineData( "byte-high", "\\x{ff}", "00FF", false, false, false, 0, 1, 1 )]
	[InlineData( "byte-range-count", "[\\x{41}-\\x{43}]{2}", "5A4243", false, false, false, 0, 1, 2 )]
	[InlineData( "byte-lookbehind", "(?<=\\x{41})B", "4142", false, false, false, 0, 1, 1 )]
	[InlineData( "byte-lazy-count", "a{2,4}?a", "61616161", false, false, false, 0, 0, 3 )]
	[InlineData( "byte-caseless", "[k]", "4B", false, true, false, 0, 0, 1 )]
	[InlineData( "byte-backref", "^(a)\\1$", "6161", false, false, false, 0, 0, 2 )]
	[InlineData( "byte-g-plain", "(a)\\g1", "6161", false, false, false, 0, 0, 2 )]
	[InlineData( "byte-g-delimited-digit", "(a)\\g{1}2", "616132", false, false, false, 0, 0, 3 )]
	[InlineData( "byte-g-tenth", "(a)(b)(c)(d)(e)(f)(g)(h)(i)(j)\\g{10}", "6162636465666768696A6A", false, false, false, 0, 0, 11 )]
	[InlineData( "byte-g-unset", "(a)?b\\g{1}", "62", false, false, false, 0, -1, 0 )]
	[InlineData( "byte-g-forward", "\\g{1}(a)", "61", false, false, false, 0, -1, 0 )]
	[InlineData( "utf-g-caseless", "(?i)(é)\\g{1}", "C3A9C389", true, false, true, 0, 0, 4 )]
	[InlineData( "utf-braced-scalar", "\\x{1F600}", "7AF09F9880", true, false, true, 0, 1, 4 )]
	[InlineData( "utf-braced-range", "[\\x{1F600}-\\x{1F601}]", "F09F9881", true, false, true, 0, 0, 4 )]
	[InlineData( "utf-lookbehind", "(?<=\\x{1F600})x", "F09F988078", true, false, true, 0, 4, 1 )]
	[InlineData( "utf-kelvin", "[k]", "E284AA", true, true, true, 0, 0, 3 )]
	[InlineData( "utf-long-s", "[s]", "C5BF", true, true, true, 0, 0, 2 )]
	[InlineData( "utf-sigma", "Σ", "CF82", true, true, true, 0, 0, 2 )]
	[InlineData( "utf-ascii-backslash-d", "^\\d$", "D9A3", true, false, true, 0, -1, 0 )]
	[InlineData( "utf-posix-digit", "[[:digit:]]", "D9A3", true, false, true, 0, 0, 2 )]
	[InlineData( "utf-invalid-start", "a", "61FF61", true, false, true, 2, 2, 1 )]
	[InlineData( "utf-invalid-barrier", "aa", "61FF61", true, false, true, 0, -1, 0 )]
	[InlineData( "byte-atomic-alt", "^(?>a|ab)c$", "616263", false, false, false, 0, -1, 0 )]
	[InlineData( "byte-atomic-prior", "^(?:a|ab)(?>b)c$", "61626263", false, false, false, 0, 0, 4 )]
	[InlineData( "byte-possessive-star", "^a*+a$", "6161", false, false, false, 0, -1, 0 )]
	[InlineData( "byte-possessive-count", "^a{1,2}+a$", "616161", false, false, false, 0, 0, 3 )]
	[InlineData( "byte-possessive-empty", "^(?:)*+a$", "61", false, false, false, 0, 0, 1 )]
	[InlineData( "utf-atomic-scalar", "^(?>é|éé)x$", "C3A9C3A978", true, false, true, 0, -1, 0 )]
	public void MatchesPinnedReferenceWholeByteSpan( string name, string pattern, string subjectHex,
		bool utf8, bool caseless, bool invalidUtf, int startOffset, int expectedIndex, int expectedLength ) {
		var patternBytes = ( utf8 ? Encoding.UTF8 : Encoding.Latin1 ).GetBytes( pattern );
		var subject = Convert.FromHexString( subjectHex );
		var nativeOptions = PcreOptions.Compiled;
		if ( caseless ) { nativeOptions |= PcreOptions.Caseless; }
		if ( utf8 ) { nativeOptions |= PcreOptions.Utf | PcreOptions.Ucp; }
		if ( invalidUtf ) { nativeOptions |= PcreOptions.MatchInvalidUtf; }
		var reference = new PcreRegex8Bit( patternBytes, utf8 ? Encoding.UTF8 : Encoding.Latin1,
			new PcreRegexSettings {
				Options = nativeOptions,
				ExtraCompileOptions = utf8 ? PcreExtraCompileOptions.AsciiBsD : PcreExtraCompileOptions.None
			} );
		var native = reference.Match( subject, startOffset );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( expectedLength, native.Length ); }

		var compiled = PcreByteCompiler.Compile( patternBytes, new PcreByteOptions {
			Utf8 = utf8, Ucp = utf8, AsciiBsD = utf8, MatchInvalidUtf = invalidUtf,
			Caseless = caseless
		} );
		Assert.True( compiled.IsSuccess, $"{name}: {compiled.Diagnostic?.Message}" );
		var managed = compiled.Pattern!.Match( subject, startOffset );
		Assert.True( managed.IsSuccess, $"{name}: {managed.Diagnostic?.Message}" );
		Assert.Equal( native.Success ? native.Index : -1, managed.Match?.ByteIndex ?? -1 );
		if ( native.Success ) { Assert.Equal( native.Length, managed.Match?.ByteLength ); }
	}
}
