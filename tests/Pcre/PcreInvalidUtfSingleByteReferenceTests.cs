using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>All one-byte subjects at both offsets against Grep's pinned UTF-8 PCRE2 wrapper.</summary>
public sealed class PcreInvalidUtfSingleByteReferenceTests {
	private static readonly string[] Patterns = [
		"", ".", @"\b", @"\B", "^", "$", @"\A", @"\z", @"\Z", @"\p{L}", @"\d",
		"(?m)^", "(?m)$"
	];

	[Fact]
	public void EverySingleByteAndStartOffsetPreservesReferenceSpan() {
		var mismatches = new List<string>();
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
			for ( var value = 0; value <= byte.MaxValue; value++ ) {
				var subject = new[] { (byte)value };
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					var expected = reference.Match( subject, offset );
					var actual = compiled.Pattern!.Match( subject, offset );
					if ( !actual.IsSuccess
						|| ( expected.Success ? expected.Index : -1 ) != ( actual.Match?.ByteIndex ?? -1 )
						|| expected.Success && expected.Length != actual.Match?.ByteLength ) {
						mismatches.Add( $"{pattern} on {value:X2} from {offset}: expected "
							+ $"{( expected.Success ? expected.Index : -1 )}/{( expected.Success ? expected.Length : 0 )}, "
							+ $"found {actual.Match?.ByteIndex ?? -1}/{actual.Match?.ByteLength ?? 0} "
							+ $"({actual.Diagnostic?.Code})" );
					}
				}
			}
		}
		Assert.True( mismatches.Count == 0, $"{mismatches.Count} one-byte mismatches"
			+ Environment.NewLine + String.Join( Environment.NewLine, mismatches.Take( 25 ) ) );
	}

	[Theory]
	[InlineData( "0D0A" )]
	[InlineData( "610D0A62" )]
	[InlineData( "610D62" )]
	[InlineData( "610A62" )]
	public void CrLfBoundaryPreservesReferenceSpanAtEveryOffset( string subjectHex ) {
		var subject = Convert.FromHexString( subjectHex );
		var mismatches = new List<string>();
		foreach ( var pattern in new[] { ".", "$", @"\Z", "(?m)^", "(?m)$" } ) {
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
			for ( var offset = 0; offset <= subject.Length; offset++ ) {
				var expected = reference.Match( subject, offset );
				var actual = compiled.Pattern!.Match( subject, offset );
				if ( !actual.IsSuccess || ( expected.Success ? expected.Index : -1 ) != ( actual.Match?.ByteIndex ?? -1 )
					|| expected.Success && expected.Length != actual.Match?.ByteLength ) {
					mismatches.Add( $"{pattern} on {subjectHex} from {offset}: expected "
						+ $"{( expected.Success ? expected.Index : -1 )}/{( expected.Success ? expected.Length : 0 )}, "
						+ $"found {actual.Match?.ByteIndex ?? -1}/{actual.Match?.ByteLength ?? 0} "
						+ $"({actual.Diagnostic?.Code})" );
				}
			}
		}
		Assert.True( mismatches.Count == 0, $"{mismatches.Count} CRLF mismatches"
			+ Environment.NewLine + String.Join( Environment.NewLine, mismatches.Take( 25 ) ) );
	}
}
