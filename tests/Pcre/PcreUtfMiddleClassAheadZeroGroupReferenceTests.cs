using System.Collections.Generic;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins native LF entry through positive class assertions between a leading assertion and nullable group repeats.</summary>
public sealed class PcreUtfMiddleClassAheadZeroGroupReferenceTests {
	public static IEnumerable<object[]> NativeCases() {
		var shapes = new[] {
			(Pattern: "(?:B|C)*", CrLf: "1:2,1:1", B: "0:3,1:1", Bc: "0:4,2:1", SkipB: "1:2,1:1", SkipBc: "2:2,2:1"),
			(Pattern: "(B|C)*", CrLf: "1:2,-,1:1", B: "0:3,0:1,1:1", Bc: "0:4,1:1,2:1", SkipB: "1:2,-,1:1", SkipBc: "2:2,-,2:1"),
			(Pattern: "(?:B|C){0,2}", CrLf: "1:2,1:1", B: "0:3,1:1", Bc: "0:4,2:1", SkipB: "1:2,1:1", SkipBc: "2:2,2:1"),
			(Pattern: "(?>(?:B|C))*", CrLf: "1:2,1:1", B: "0:3,1:1", Bc: "0:4,2:1", SkipB: "1:2,1:1", SkipBc: "2:2,2:1"),
			(Pattern: "(?:B|C)*+", CrLf: "1:2,1:1", B: "0:3,1:1", Bc: "0:4,2:1", SkipB: "1:2,1:1", SkipBc: "2:2,2:1")
		};
		var leads = new[] { "(?=B)", "(?=[BC])" };
		var middles = new[] { "(?=[BC])", "(?=[B-D])", @"(?=[B\x{100}])",
			@"(?=\w)", @"(?=\d)", @"(?=\h)", @"(?=[BC])\b(?=[B-D])" };
		foreach ( var lead in leads ) {
			foreach ( var middle in middles ) {
				foreach ( var shape in shapes ) {
					var pattern = String.Concat( "(*ANYCRLF)(?:", lead, middle, shape.Pattern,
						@")?([\x09-\x0c\x{100}])A" );
					var takesGroup = middle is not ( @"(?=\d)" or @"(?=\h)" );
					yield return new object[] { pattern, "0D0A41", shape.CrLf };
					yield return new object[] { pattern, "420B41", takesGroup ? shape.B : shape.SkipB };
					yield return new object[] { pattern, "42430B41", takesGroup ? shape.Bc : shape.SkipBc };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( NativeCases ) )]
	public void MiddlePositiveClassLookaheadBeforeNullableGroupRepeatPreservesCaptures( string pattern, string hex, string spans ) {
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
