using System.Collections.Generic;
using System.Linq;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins native LF entry through bounded deep mandatory groups after positive ahead.</summary>
public sealed class PcreUtfPositiveAheadDeepGroupReferenceTests {
	public static IEnumerable<object[]> NativeCases() {
		foreach ( var depth in new[] { 2, 4, 8, 16, 64, 128 } ) {
			var pattern = String.Concat( @"(*ANYCRLF)(?:(?=B)", String.Concat( Enumerable.Repeat( "(?:", depth ) ),
				"B+", new string( ')', depth ), @")?([\x09-\x0c\x{100}])A" );
			yield return new object[] { pattern, "0D0A41", "1:2,1:1" };
			yield return new object[] { pattern, "42420B41", "0:4,2:1" };
			yield return new object[] { pattern, "0B41", "0:2,0:1" };
		}
		foreach ( var depth in new[] { 2, 4, 8 } ) {
			var pattern = String.Concat( @"(*ANYCRLF)(?:(?=B)", new string( '(', depth ),
				"B+", new string( ')', depth ), @")?([\x09-\x0c\x{100}])A" );
			yield return new object[] { pattern, "0D0A41",
				String.Concat( "1:2,", String.Join( ",", Enumerable.Repeat( "-", depth ) ), ",1:1" ) };
			yield return new object[] { pattern, "420B41",
				String.Concat( "0:3,", String.Join( ",", Enumerable.Repeat( "0:1", depth ) ), ",1:1" ) };
			yield return new object[] { pattern, "42420B41",
				String.Concat( "0:4,", String.Join( ",", Enumerable.Repeat( "0:2", depth ) ), ",2:1" ) };
		}
	}
	[Theory]
	[MemberData( nameof( NativeCases ) )]
	public void PositiveAheadBeforeDeepGroupPreservesCaptures( string pattern, string hex, string spans ) {
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
