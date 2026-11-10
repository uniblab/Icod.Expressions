using System.Collections.Generic;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins native LF entry through negative assertions over endpoint-implicit CR/LF-excluding ranges.</summary>
public sealed class PcreUtfNegatedRangeNegativeAssertionReferenceTests {
	public static IEnumerable<object[]> NativeCases() {
		var shapes = new[] {
			(Pattern: "(?:B|C)*", Captured: false),
			(Pattern: "(B|C)*", Captured: true),
			(Pattern: "(?:B|C){0,2}", Captured: false),
			(Pattern: "(?>(?:B|C))*", Captured: false),
			(Pattern: "(?:B|C)*+", Captured: false)
		};
		var bodies = new[] { @"[^\x09-\x0e]", @"[^\x08-\x0f]", @"[^\x09-\x0eB]" };
		var middles = new[] { "", "(?!Z)", "(?=B)" };
		foreach ( var caseless in new[] { false, true } ) {
			foreach ( var body in bodies ) {
				foreach ( var behind in new[] { false, true } ) {
					var assertion = String.Concat( behind ? "(?<!" : "(?!", body, ")" );
					foreach ( var middle in middles ) {
						foreach ( var shape in shapes ) {
							var pattern = String.Concat( "(*ANYCRLF)", caseless ? "(?i)" : "", "(?:",
								assertion, middle, shape.Pattern, @")?([\x09-\x0c\x{100}])A" );
							foreach ( var entry in Cases( pattern, shape.Captured ) ) { yield return entry; }
						}
					}
				}
			}
			foreach ( var lead in new[] { "(?=B)", "(?<=B)", "(?!B)", "(?![BC])" } ) {
				foreach ( var body in bodies ) {
					foreach ( var behind in new[] { false, true } ) {
						var assertion = String.Concat( behind ? "(?<!" : "(?!", body, ")" );
						foreach ( var shape in shapes ) {
							var pattern = String.Concat( "(*ANYCRLF)", caseless ? "(?i)" : "", "(?:",
								lead, assertion, shape.Pattern, @")?([\x09-\x0c\x{100}])A" );
							foreach ( var entry in Cases( pattern, shape.Captured ) ) { yield return entry; }
						}
					}
				}
			}
		}
	}

	private static IEnumerable<object[]> Cases( string pattern, bool captured ) {
		yield return new object[] { pattern, "0D0A41", captured ? "1:2,-,1:1" : "1:2,1:1" };
		yield return new object[] { pattern, "0B41", captured ? "0:2,-,0:1" : "0:2,0:1" };
	}

	[Theory]
	[MemberData( nameof( NativeCases ) )]
	public void NegativeNegatedSetBeforeNullableGroupRepeatPreservesCaptures( string pattern, string hex, string spans ) {
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
