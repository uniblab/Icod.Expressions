using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Selected three-byte shapes with every middle byte against Grep's pinned UTF-8 PCRE2 wrapper.</summary>
public sealed class PcreInvalidUtfThreeByteReferenceTests {
	private static readonly string[] Patterns = [
		"", ".", "a", "é", "a.b", @"(?<=a)b", @"\b", @"\B", "$", @"\Z", @"\z",
		@"\p{L}", "(?m)^", "(?m)$"
	];
	private static readonly byte[] LeadingBytes = [
		0x61, 0x0A, 0x0D, 0x80, 0xC2, 0xE2, 0xF0, 0xFF
	];
	private static readonly byte[] TrailingBytes = [
		0x00, 0x0A, 0x0D, 0x61, 0x62, 0x80, 0xBF, 0xFF
	];

	[Fact]
	public void EveryMiddleByteAndStartOffsetPreservesReferenceSpan() {
		var mismatchCount = 0;
		var examples = new List<string>();
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
			foreach ( var first in LeadingBytes ) {
				for ( var middle = 0; middle <= byte.MaxValue; middle++ ) {
					foreach ( var last in TrailingBytes ) {
						var subject = new[] { first, (byte)middle, last };
						for ( var offset = 0; offset <= subject.Length; offset++ ) {
							var expected = reference.Match( subject, offset );
							var actual = compiled.Pattern!.Match( subject, offset );
							if ( !actual.IsSuccess
								|| ( expected.Success ? expected.Index : -1 ) != ( actual.Match?.ByteIndex ?? -1 )
								|| expected.Success && expected.Length != actual.Match?.ByteLength ) {
								mismatchCount++;
								if ( examples.Count < 25 ) {
									examples.Add( $"{pattern} on {first:X2}{middle:X2}{last:X2} from {offset}: expected "
										+ $"{( expected.Success ? expected.Index : -1 )}/{( expected.Success ? expected.Length : 0 )}, "
										+ $"found {actual.Match?.ByteIndex ?? -1}/{actual.Match?.ByteLength ?? 0} "
										+ $"({actual.Diagnostic?.Code})" );
								}
							}
						}
					}
				}
			}
		}
		Assert.True( mismatchCount == 0, $"{mismatchCount} three-byte mismatches"
			+ Environment.NewLine + String.Join( Environment.NewLine, examples ) );
	}
}
