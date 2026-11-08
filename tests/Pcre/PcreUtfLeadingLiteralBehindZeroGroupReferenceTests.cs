using System.Collections.Generic;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins native LF entry through positive literal lookbehind before nullable group repeats.</summary>
public sealed class PcreUtfLeadingLiteralBehindZeroGroupReferenceTests {
	public static IEnumerable<object[]> NativeCases() {
		var shapes = new[] {
			(Pattern: "(?:B|C)*", CrLf: "1:2,1:1", B: "1:2,1:1", Positive: "1:3,2:1", Wide: "2:3,3:1"),
			(Pattern: "(B|C)*", CrLf: "1:2,-,1:1", B: "1:2,-,1:1", Positive: "1:3,1:1,2:1", Wide: "2:3,2:1,3:1"),
			(Pattern: "(?:B|C){0,2}", CrLf: "1:2,1:1", B: "1:2,1:1", Positive: "1:3,2:1", Wide: "2:3,3:1"),
			(Pattern: "(?>(?:B|C))*", CrLf: "1:2,1:1", B: "1:2,1:1", Positive: "1:3,2:1", Wide: "2:3,3:1"),
			(Pattern: "(?:B|C)*+", CrLf: "1:2,1:1", B: "1:2,1:1", Positive: "1:3,2:1", Wide: "2:3,3:1")
		};
		var leads = new[] {
			(Pattern: "(?<=B)", Positive: "42420B41", Wide: false),
			(Pattern: "(?<=C)", Positive: "43420B41", Wide: false),
			(Pattern: "(?<=D)", Positive: "44420B41", Wide: false),
			(Pattern: @"(?<=\x{100})", Positive: "C480420B41", Wide: true)
		};
		var middles = new[] { "", "(?!Z)", "(?=B)" };
		foreach ( var lead in leads ) {
			foreach ( var middle in middles ) {
				foreach ( var shape in shapes ) {
					var pattern = String.Concat( "(*ANYCRLF)(?:", lead.Pattern, middle, shape.Pattern,
						@")?([\x09-\x0c\x{100}])A" );
					yield return new object[] { pattern, "0D0A41", shape.CrLf };
					yield return new object[] { pattern, "420B41", shape.B };
					yield return new object[] { pattern, lead.Positive, lead.Wide ? shape.Wide : shape.Positive };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( NativeCases ) )]
	public void LeadingPositiveLiteralLookbehindBeforeNullableGroupRepeatPreservesCaptures( string pattern, string hex, string spans ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var tolerant in new[] { false, true } ) {
			var reference = new PcreRegex8Bit( Encoding.ASCII.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
				Options = PcreOptions.Compiled | PcreOptions.Utf
					| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ) } );
			var expected = reference.Match( subject );
			Assert.True( expected.Success );
			var groups = spans.Split( ',' );
			Assert.Equal( groups.Length, expected.Groups.Count );
			for ( var group = 0; group < groups.Length; group++ ) {
				var capture = expected.Groups[group];
				Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
			}
			var compiled = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new() {
				Utf8 = true, MatchInvalidUtf = tolerant } );
			Assert.True( compiled.IsSuccess, compiled.Diagnostic?.Message );
			var actual = Assert.IsType<PcreCompiledBytePattern>( compiled.Pattern ).Match( subject );
			Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
			Assert.Equal( expected.Index, actual.Match?.ByteIndex ?? -1 );
			Assert.Equal( expected.Length, actual.Match?.ByteLength );
			Assert.Equal( expected.Groups.Count, actual.Match!.Captures.Count );
			for ( var group = 0; group < expected.Groups.Count; group++ ) {
				var capture = expected.Groups[group];
				Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null,
					actual.Match.Captures[group] );
			}
		}
	}
}
