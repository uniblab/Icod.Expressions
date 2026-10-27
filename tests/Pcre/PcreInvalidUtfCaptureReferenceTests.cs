using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Source-byte capture participation and spans under Grep's pinned invalid-UTF reference.</summary>
public sealed class PcreInvalidUtfCaptureReferenceTests {
	private static readonly string[] Patterns = [
		"()", "(a)", "(a)?", "(a|ab)", "(?:(a)|b)", @"(a)\1", "((a)?b)+",
		"(?<=a)(b)", "(é)", "(😀)", "(?m)^()", @"()\Z", @"(\p{L}+)",
		@"(\w+)", @"([^\r]a)", @"(\s?a)"
	];
	private static readonly string[] SelectedSubjects = [
		"", "6162", "6161", "616261", "6261", "61FF61", "61FF62", "61FFFF62",
		"0D0A61", "610D0A62", "0A80", "0DFF", "C3A9", "C3A9FF61",
		"F09F9880FF61", "61E28262", "E282AC", "61C3A9", "F09F9880"
	];

	[Fact]
	public void OneByteSweepAndSelectedFragmentsPreserveEveryNumberedCapture() {
		var mismatchCount = 0;
		var examples = new List<string>();
		void Report( string caseName, string expected, string found ) {
			mismatchCount++;
			if ( examples.Count < 25 ) { examples.Add( $"{caseName}: expected {expected}, found {found}" ); }
		}

		foreach ( var pattern in Patterns ) {
			var source = Encoding.UTF8.GetBytes( pattern );
			var reference = new PcreRegex8Bit( source, Encoding.UTF8, new PcreRegexSettings {
				Options = PcreOptions.Compiled | PcreOptions.Utf | PcreOptions.Ucp | PcreOptions.MatchInvalidUtf,
				ExtraCompileOptions = PcreExtraCompileOptions.AsciiBsD
			} );
			var compiled = PcreByteCompiler.Compile( source, new PcreByteOptions {
				Utf8 = true, Ucp = true, AsciiBsD = true, MatchInvalidUtf = true,
				AnyCrLfNewline = true
			} );
			Assert.True( compiled.IsSuccess, $"{pattern}: {compiled.Diagnostic?.Message}" );
			foreach ( var subject in Subjects() ) {
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					var expected = reference.Match( subject, offset );
					var actual = compiled.Pattern!.Match( subject, offset );
					var caseName = $"{pattern} on {Convert.ToHexString( subject )} from {offset}";
					if ( !actual.IsSuccess || expected.Success != ( actual.Match is not null ) ) {
						Report( caseName, expected.Success ? "match" : "no match",
							actual.IsSuccess ? actual.Match is null ? "no match" : "match" : $"{actual.Diagnostic?.Code}" );
						continue;
					}
					if ( !expected.Success ) { continue; }
					var captures = actual.Match!.Captures;
					if ( expected.Groups.Count != captures.Count ) {
						Report( caseName, $"{expected.Groups.Count} groups", $"{captures.Count} groups" );
						continue;
					}
					for ( var groupIndex = 0; groupIndex < captures.Count; groupIndex++ ) {
						var referenceGroup = expected.Groups[groupIndex];
						var capture = captures[groupIndex];
						if ( referenceGroup.Success != capture.HasValue
							|| referenceGroup.Success && ( referenceGroup.Index != capture!.Value.ByteIndex
								|| referenceGroup.Length != capture.Value.ByteLength ) ) {
							Report( $"{caseName} group {groupIndex}",
								referenceGroup.Success ? $"{referenceGroup.Index}/{referenceGroup.Length}" : "unset",
								capture.HasValue ? $"{capture.Value.ByteIndex}/{capture.Value.ByteLength}" : "unset" );
						}
					}
				}
			}
		}
		Assert.True( mismatchCount == 0, $"{mismatchCount} invalid-UTF capture mismatches"
			+ Environment.NewLine + String.Join( Environment.NewLine, examples ) );
	}

	private static IEnumerable<byte[]> Subjects() {
		for ( var value = 0; value <= byte.MaxValue; value++ ) { yield return [ (byte)value ]; }
		foreach ( var hex in SelectedSubjects ) { yield return Convert.FromHexString( hex ); }
	}
}
