using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Every leading byte with selected second bytes under Grep's pinned UTF-8 PCRE2 wrapper.</summary>
public sealed class PcreInvalidUtfTwoByteReferenceTests {
	private static readonly string[] Patterns = [
		"", ".", "a", @"\b", @"\B", "$", @"\Z", @"\z", @"\p{L}", "(?m)^", "(?m)$"
	];
	private static readonly byte[] TrailingBytes = [
		0x00, 0x09, 0x0A, 0x0D, 0x20, 0x30, 0x41, 0x61, 0x7F, 0x80, 0x85, 0x9F,
		0xA0, 0xBF, 0xC0, 0xC1, 0xC2, 0xDF, 0xE0, 0xED, 0xF0, 0xF4, 0xF5, 0xFF
	];

	[Fact]
	public void EveryLeadingByteSelectedTrailingByteAndStartOffsetPreservesReferenceSpan() {
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
			for ( var first = 0; first <= byte.MaxValue; first++ ) {
				foreach ( var second in TrailingBytes ) {
					var subject = new[] { (byte)first, second };
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						var expected = reference.Match( subject, offset );
						var actual = compiled.Pattern!.Match( subject, offset );
						if ( !actual.IsSuccess
							|| ( expected.Success ? expected.Index : -1 ) != ( actual.Match?.ByteIndex ?? -1 )
							|| expected.Success && expected.Length != actual.Match?.ByteLength ) {
							mismatchCount++;
							if ( examples.Count < 25 ) {
								examples.Add( $"{pattern} on {first:X2}{second:X2} from {offset}: expected "
									+ $"{( expected.Success ? expected.Index : -1 )}/{( expected.Success ? expected.Length : 0 )}, "
									+ $"found {actual.Match?.ByteIndex ?? -1}/{actual.Match?.ByteLength ?? 0} "
									+ $"({actual.Diagnostic?.Code})" );
							}
						}
					}
				}
			}
		}
		Assert.True( mismatchCount == 0, $"{mismatchCount} two-byte mismatches"
			+ Environment.NewLine + String.Join( Environment.NewLine, examples ) );
	}
}
