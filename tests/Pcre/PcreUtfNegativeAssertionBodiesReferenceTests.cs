using System.Collections.Generic;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins native LF entry past bounded regular negative assertion bodies.</summary>
public sealed class PcreUtfNegativeAssertionBodiesReferenceTests {
	public static IEnumerable<object[]> NativeCases() {
		var shapes = new[] {
			(Pattern: "(?:B|C)*", Captured: false),
			(Pattern: "(B|C)*", Captured: true),
			(Pattern: "(?:B|C){0,2}", Captured: false),
			(Pattern: "(?>(?:B|C))*", Captured: false),
			(Pattern: "(?:B|C)*+", Captured: false)
		};
		var deep = new StringBuilder();
		for ( var depth = 0; depth < 200; depth++ ) { deep.Append( "(?:" ); }
		deep.Append( "B|C" ).Append( ')', 200 );
		var alternatives = new string[64];
		Array.Fill( alternatives, "B[CD]" );
		var bodies = new[] {
			(Pattern: "B[CD]", Behind: true, Captures: 0),
			(Pattern: "[BC].", Behind: true, Captures: 0),
			(Pattern: "B|C", Behind: true, Captures: 0),
			(Pattern: "(?:B|C)", Behind: true, Captures: 0),
			(Pattern: "(B|C)", Behind: true, Captures: 1),
			(Pattern: "(?>B|C)", Behind: true, Captures: 0),
			(Pattern: "(?:B|C){1,2}", Behind: true, Captures: 0),
			(Pattern: "(?:B|C)*", Behind: false, Captures: 0),
			(Pattern: "B?", Behind: true, Captures: 0),
			(Pattern: @"\bB", Behind: true, Captures: 0),
			(Pattern: "B(?!D)", Behind: true, Captures: 0),
			(Pattern: "(?:B[CD]|D[EF])", Behind: true, Captures: 0),
			(Pattern: @"[\p{L}]\d", Behind: true, Captures: 0),
			(Pattern: "(?i:B[CD])", Behind: true, Captures: 0),
			(Pattern: "(?=B)B", Behind: true, Captures: 0),
			(Pattern: "B*", Behind: false, Captures: 0),
			(Pattern: deep.ToString(), Behind: true, Captures: 0),
			(Pattern: String.Join( "|", alternatives ), Behind: true, Captures: 0)
		};
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var caseless in new[] { false, true } ) {
				foreach ( var body in bodies ) {
					foreach ( var behind in new[] { false, true } ) {
						if ( behind && !body.Behind ) { continue; }
						foreach ( var middle in new[] { false, true } ) {
							var assertion = String.Concat( behind ? "(?<!" : "(?!", body.Pattern, ")" );
							foreach ( var shape in shapes ) {
								var pattern = String.Concat( "(*ANYCRLF)", ucp ? "(*UCP)" : "",
									caseless ? "(?i)" : "", "(?:", middle ? "(?=B)" : "",
									assertion, shape.Pattern, @")?([\x09-\x0c\x{100}])A" );
								foreach ( var entry in Cases( pattern, body.Captures + ( shape.Captured ? 1 : 0 ) ) ) { yield return entry; }
							}
						}
					}
				}
			}
		}
	}

	private static IEnumerable<object[]> Cases( string pattern, int unsetCaptures ) {
		foreach ( var crLf in new[] { true, false } ) {
			var start = crLf ? 1 : 0;
			var spans = new StringBuilder().Append( start ).Append( ":2" );
			for ( var capture = 0; capture < unsetCaptures; capture++ ) { spans.Append( ",-" ); }
			spans.Append( ',' ).Append( start ).Append( ":1" );
			yield return new object[] { pattern, crLf ? "0D0A41" : "0B41", spans.ToString() };
		}
	}

	[Theory]
	[MemberData( nameof( NativeCases ) )]
	public void RegularNegativeAssertionBeforeNullableGroupRepeatPreservesCaptures( string pattern, string hex, string spans ) {
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
