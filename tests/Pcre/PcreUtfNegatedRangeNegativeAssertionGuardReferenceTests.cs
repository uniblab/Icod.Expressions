using System.Collections.Generic;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins negative assertion guards when a negated range still admits CR or LF.</summary>
public sealed class PcreUtfNegatedRangeNegativeAssertionGuardReferenceTests {
	public static IEnumerable<object[]> NativeCases() {
		var bodies = new[] { @"[^\x0b-\x0f]", @"[^\x09-\x0c]", "[^B-D]" };
		foreach ( var caseless in new[] { false, true } ) {
			foreach ( var body in bodies ) {
				foreach ( var behind in new[] { false, true } ) {
					foreach ( var middle in new[] { false, true } ) {
						var assertion = String.Concat( behind ? "(?<!" : "(?!", body, ")" );
						var pattern = String.Concat( "(*ANYCRLF)", caseless ? "(?i)" : "", "(?:",
							middle ? "(?=B)" : "", assertion, @"(?:B|C)*)?([\x09-\x0c\x{100}])A" );
						yield return new object[] { pattern, "0D0A41" };
						yield return new object[] { pattern, "0B41" };
					}
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( NativeCases ) )]
	public void NewlineAdmittingNegatedRangesPreserveNativeSearch( string pattern, string hex ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var tolerant in new[] { false, true } ) {
			var reference = new PcreRegex8Bit( Encoding.ASCII.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
				Options = PcreOptions.Compiled | PcreOptions.Utf
					| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ) } );
			var expected = reference.Match( subject );
			var compiled = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new() {
				Utf8 = true, MatchInvalidUtf = tolerant } );
			Assert.True( compiled.IsSuccess, compiled.Diagnostic?.Message );
			var actual = Assert.IsType<PcreCompiledBytePattern>( compiled.Pattern ).Match( subject );
			Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
			Assert.Equal( expected.Success, actual.Match is not null );
			if ( !expected.Success ) { continue; }
			Assert.Equal( expected.Index, actual.Match!.ByteIndex );
			Assert.Equal( expected.Length, actual.Match.ByteLength );
			Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
			for ( var group = 0; group < expected.Groups.Count; group++ ) {
				var capture = expected.Groups[group];
				Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null,
					actual.Match.Captures[group] );
			}
		}
	}
}
