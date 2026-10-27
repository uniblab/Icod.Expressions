using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Every start offset on selected malformed byte subjects against Grep's pinned PCRE2 wrapper.</summary>
public sealed class PcreInvalidUtfOffsetReferenceTests {
	private static readonly string[] Patterns = [
		"", ".", "a", "é", "a.b", @"(?<=a)b", @"\b", @"\z", @"(a)\1"
	];

	[Theory]
	[InlineData( "" )]
	[InlineData( "61" )]
	[InlineData( "C3A9" )]
	[InlineData( "61FF62" )]
	[InlineData( "C3A9FF61" )]
	[InlineData( "61E28262" )]
	[InlineData( "6180FF62" )]
	[InlineData( "C0AF61" )]
	[InlineData( "F09F9880FF61" )]
	public void EveryStartOffsetPreservesReferenceByteSpan( string subjectHex ) {
		var subject = Convert.FromHexString( subjectHex );
		foreach ( var pattern in Patterns ) {
			var source = Encoding.UTF8.GetBytes( pattern );
			var reference = new PcreRegex8Bit( source, Encoding.UTF8, new PcreRegexSettings {
				Options = PcreOptions.Compiled | PcreOptions.Utf | PcreOptions.Ucp | PcreOptions.MatchInvalidUtf,
				ExtraCompileOptions = PcreExtraCompileOptions.AsciiBsD
			} );
			var compiled = PcreByteCompiler.Compile( source, new PcreByteOptions {
				Utf8 = true, Ucp = true, AsciiBsD = true, MatchInvalidUtf = true
			} );
			Assert.True( compiled.IsSuccess, $"{pattern} on {subjectHex}: {compiled.Diagnostic?.Message}" );
			for ( var offset = 0; offset <= subject.Length; offset++ ) {
				var expected = reference.Match( subject, offset );
				var actual = compiled.Pattern!.Match( subject, offset );
				var caseName = $"{pattern} on {subjectHex} from {offset}";
				Assert.True( actual.IsSuccess, $"{caseName}: {actual.Diagnostic?.Message}" );
				Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
					$"{caseName}: expected index {( expected.Success ? expected.Index : -1 )}, "
					+ $"found {actual.Match?.ByteIndex ?? -1}" );
				if ( expected.Success ) {
					Assert.True( expected.Length == actual.Match?.ByteLength,
						$"{caseName}: expected length {expected.Length}, found {actual.Match?.ByteLength}" );
				}
			}
		}
	}
}
