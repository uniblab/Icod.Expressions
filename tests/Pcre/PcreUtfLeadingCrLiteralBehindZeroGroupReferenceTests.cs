using System.Collections.Generic;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins native LF entry through CR-containing leading literal lookbehind before nullable group repeats.</summary>
public sealed class PcreUtfLeadingCrLiteralBehindZeroGroupReferenceTests {
	public static IEnumerable<object[]> NativeCases() {
		var shapes = new[] {
			(Pattern: "(?:B|C)*", Captured: false),
			(Pattern: "(B|C)*", Captured: true),
			(Pattern: "(?:B|C){0,2}", Captured: false),
			(Pattern: "(?>(?:B|C))*", Captured: false),
			(Pattern: "(?:B|C)*+", Captured: false)
		};
		var leads = new[] {
			(Pattern: @"(?<=\r)", PrefixHex: "0D", Start: 1),
			(Pattern: @"(?<=\n)", PrefixHex: "0A", Start: 1),
			(Pattern: @"(?<=B\r)", PrefixHex: "420D", Start: 2),
			(Pattern: @"(?<=\rB)", PrefixHex: "0D42", Start: 2),
			(Pattern: @"(?<=B\n)", PrefixHex: "420A", Start: 2),
			(Pattern: @"(?<=\nB)", PrefixHex: "0A42", Start: 2),
			(Pattern: @"(?<=\r\n)", PrefixHex: "0D0A", Start: 2),
			(Pattern: @"(?<=\n\r)", PrefixHex: "0A0D", Start: 2),
			(Pattern: @"(?<=\rB\r)", PrefixHex: "0D420D", Start: 3)
		};
		var middles = new[] { "", "(?!Z)", "(?=B)" };
		foreach ( var lead in leads ) {
			foreach ( var middle in middles ) {
				foreach ( var shape in shapes ) {
					var pattern = String.Concat( "(*ANYCRLF)(?:", lead.Pattern, middle, shape.Pattern,
						@")?([\x09-\x0c\x{100}])A" );
					var ordinary = shape.Captured ? "1:2,-,1:1" : "1:2,1:1";
					yield return new object[] { pattern, "0D0A41", ordinary };
					yield return new object[] { pattern, "420B41", ordinary };
					var start = lead.Start;
					var spans = shape.Captured ? $"{start}:2,-,{start}:1" : $"{start}:2,{start}:1";
					yield return new object[] { pattern, String.Concat( lead.PrefixHex, "0A41" ), spans };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( NativeCases ) )]
	public void LeadingCrLiteralLookbehindBeforeNullableGroupRepeatPreservesCaptures( string pattern, string hex, string spans ) {
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
