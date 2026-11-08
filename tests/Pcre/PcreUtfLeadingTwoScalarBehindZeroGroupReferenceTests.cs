using System.Collections.Generic;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins native LF entry through positive two-scalar lookbehind before nullable group repeats.</summary>
public sealed class PcreUtfLeadingTwoScalarBehindZeroGroupReferenceTests {
	public static IEnumerable<object[]> NativeCases() {
		var shapes = new[] {
			(Pattern: "(?:B|C)*", Captured: false),
			(Pattern: "(B|C)*", Captured: true),
			(Pattern: "(?:B|C){0,2}", Captured: false),
			(Pattern: "(?>(?:B|C))*", Captured: false),
			(Pattern: "(?:B|C)*+", Captured: false)
		};
		var leads = new[] {
			(Pattern: "(?<=BC)", Positive: "4243420B41", Start: 2),
			(Pattern: "(?<=BD)", Positive: "4244420B41", Start: 2),
			(Pattern: @"(?<=B\x{100})", Positive: "42C480420B41", Start: 3),
			(Pattern: @"(?<=\x{100}B)", Positive: "C48042420B41", Start: 3)
		};
		var middles = new[] { "", "(?!Z)", "(?=B)" };
		foreach ( var lead in leads ) {
			foreach ( var middle in middles ) {
				foreach ( var shape in shapes ) {
					var pattern = String.Concat( "(*ANYCRLF)(?:", lead.Pattern, middle, shape.Pattern,
						@")?([\x09-\x0c\x{100}])A" );
					yield return new object[] { pattern, "0D0A41", shape.Captured ? "1:2,-,1:1" : "1:2,1:1" };
					yield return new object[] { pattern, "420B41", shape.Captured ? "1:2,-,1:1" : "1:2,1:1" };
					var start = lead.Start;
					var spans = shape.Captured ? $"{start}:3,{start}:1,{start + 1}:1"
						: $"{start}:3,{start + 1}:1";
					yield return new object[] { pattern, lead.Positive, spans };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( NativeCases ) )]
	public void LeadingPositiveTwoScalarLookbehindBeforeNullableGroupRepeatPreservesCaptures( string pattern, string hex, string spans ) {
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
