using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreThenPruneScanReferenceTests {
	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ByteScanReuseMatchesNativeDispatchAndRootWrapperDepth( bool utf ) {
		var cases = new List<(string Pattern, string Subject)> {
			("a+(*THEN)(*PRUNE)X|a", "aaa"),
			("a+(*THEN:N)(*PRUNE:P)X|(a)", "aaa"),
			("a+(*THEN)(*PRUNE)(*F)|a", "aaa"),
			("a+(*THEN)(*PRUNE)a|a", "aaa"),
			("a+(*PRUNE)(*THEN)X|a", "aaa"),
			("a+(*THEN)(*COMMIT)X|a", "aaa"),
			("é+(*THEN:é)(*PRUNE:P)X|(é)", "ééé"),
			("(*NO_JIT)a+(*THEN)(*PRUNE)X|a", "aaa"),
			("(*NO_JIT)a+(*PRUNE)X|a", "aaa"),
			("(*NO_START_OPT)a+(*THEN)(*PRUNE)X|a", "aaa")
		};
		for ( var depth = 1; depth <= 6; depth++ ) {
			foreach ( var capture in new[] { false, true } ) {
				foreach ( var cut in new[] { "(*THEN)(*PRUNE)", "(*PRUNE)" } ) {
					cases.Add( (String.Concat( String.Concat( Enumerable.Repeat( capture ? "(" : "(?:", depth ) ),
						"a+", cut, "X|a", new string( ')', depth ) ), "aaa") );
				}
			}
		}
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
