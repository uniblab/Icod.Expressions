using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreNamedSkipDispatchReferenceTests {
	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ByteNamedSkipDispatchAndAcceptedLabelsAgreeWithNative( bool utf ) {
		var cases = new List<(string Pattern, string Subject)> {
			("(*MARK:X)(?=a(*COMMIT:Y)(*SKIP:X)X)|(.)", "abc"),
			("a(*MARK:X)(?=b(*COMMIT:Y)(*SKIP:X)X)|(.)", "abc"),
			("a(*MARK:X)(?=b(*MARK:Y)(*SKIP:X)X)|(.)", "abc"),
			("a(*MARK:X)(?=b(*THEN:Y)(*SKIP:X)X)|(.)", "abc"),
			("a(*MARK:X)(?=b(*PRUNE:Y)(*SKIP:X)X)|(.)", "abc"),
			("a(*MARK:X)(?<=a(*COMMIT:Y)(*SKIP:X))X|(.)", "abc"),
			("a(*MARK:X)(?*b(*COMMIT:Y)(*SKIP:X)X)|(.)", "abc"),
			("a(*MARK:X)(?=(?(DEFINE)(*MARK:Y))b(*SKIP:X)X)|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*MARK:X)))(?!(?&x)(*SKIP:X)b)a", "a"),
			("(?(DEFINE)(?<x>a(*MARK:X)))(?(?=(?&x)(*SKIP:X)b)X|a)", "a"),
			("(?(DEFINE)(?<x>(?&y))(?<y>a(*MARK:X)))(?!(?&x)(*SKIP:X)b)a", "a"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)bX|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)b(*SKIP:X)X|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)b(*PRUNE)X|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)b(*SKIP)X|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)b(*THEN)X|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(.)|(?&x)b(*THEN)X", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)(*MARK:Y)bX|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(*MARK:Z)(?&x)bX|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?:(?&x)bX)|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)bX|(*MARK:Z)(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)(*PRUNE)))(?&x)bX|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)(*SKIP:missing)))(?&x)bX|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)b(*ACCEPT)|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)bX|(.)", "abcabc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)b(*SKIP:missing)X|(.)", "abcabc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)b(*PRUNE)X|(.)", "abcabc"),
			("(?=a(*ACCEPT:X))bX|(.)", "abc"),
			("(?=a(*MARK:X))bX|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)bX(*THEN)|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)bX(?:q(*THEN)){0}|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)(?:b(*THEN)X|bX)|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)(?:bX(*THEN)|bX)|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)bX|(.)|(*THEN)X", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)bX|(*THEN)(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(*MARK:Z)(?:(?&x)b(*THEN)X|bX)|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)(*MARK:Z)(?:b(*THEN)X|bX)|(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)bX|(*MARK:Y)(*THEN)(.)", "abc"),
			("(?(DEFINE)(?<x>a(*ACCEPT:X)))(?&x)bX|(*THEN:Y)(.)", "abc"),
		};
		foreach ( var (pattern, subject) in cases ) {
			var source = Encoding.UTF8.GetBytes( pattern ); var input = Encoding.UTF8.GetBytes( subject );
			foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 16, 32, 63 } ) {
				var flags = ( utf ? 0xA0000u : 0u ) | ( ( defaults & 1 ) != 0 ? 0x80000000u : 0u )
					| ( ( defaults & 2 ) != 0 ? 0x20000000u : 0u );
				var native = new PcreRegex8Bit( source, Encoding.UTF8, new PcreRegexSettings {
					Options = PcreOptions.Compiled | (PcreOptions)flags,
					NewLine = PcreNewLine.Lf, BackslashR = PcreBackslashR.Unicode, ParensLimit = 512
				} );
				var compiled = PcreByteCompiler.Compile( source, new() {
					Utf8 = utf, Ucp = utf, Anchored = ( defaults & 1 ) != 0, EndAnchored = ( defaults & 2 ) != 0,
					NotEmpty = ( defaults & 4 ) != 0, NotEmptyAtStart = ( defaults & 8 ) != 0,
					NotBol = ( defaults & 16 ) != 0, NotEol = ( defaults & 32 ) != 0
				} );
				Assert.True( compiled.IsSuccess, $"{pattern}: {compiled.Diagnostic}" );
				for ( var mask = 0; mask < 64; mask++ ) {
					var expected = native.Match( input, 0, PcreNativeMatchPolicy.Options( defaults, mask ) );
					var captures = new List<string>();
					if ( expected.Success ) {
						for ( var slot = 0; slot < expected.Groups.Count; slot++ ) {
							var capture = expected.Groups[slot];
							captures.Add( capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
						}
					}
					var protocol = String.Concat( String.Join( ",", captures ), "/",
						expected.Success ? Convert.ToHexString( expected.Mark.ToArray() ) : "" );
					var actual = PcreMatchPolicyConsumer.Project( compiled.Pattern!.Match( input, PcreMatchPolicyConsumer.Policy( mask ) ) );
					Assert.True( protocol == actual, $"{pattern}/{utf}/{defaults}/{mask}: expected {protocol}, actual {actual}" );
				}
			}
		}
	}
}
