using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordUnboundedSuffixChoicesTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "a*b?", "aaa", 0, false, false ), ( "a*b?", "aaab", 0, true, false ),
				( "a*b?", "b", 0, false, false ), ( "a*b?", "zab", 1, true, false ),
				( "a*?b?", "aaa", 0, false, false ), ( "a*?b?", "ab", 0, true, false ),
				( "a+b?", "aab", 0, false, false ), ( "a+?b?", "aab", 0, true, false ),
				( "a{2,}b?", "aab", 0, true, false ), ( "a{2,}?b?", "aab", 0, false, false ),
				( "a*a?", "aaa", 0, false, false ), ( "a*?a?", "aaa", 0, true, false ),
				( "a+a?", "aaa", 0, false, false ), ( "a+?a?", "aaa", 0, true, false ),
				( "a*a??", "aaa", 0, true, false ), ( "a*?a??", "aaa", 0, false, false ),
				( "a*b{0,2}c", "aaabbc", 0, false, false ),
				( "a*b{0,2}?c", "aaabbc", 0, true, false ),
				( "a*?b{0,2}c", "aaabbc", 0, false, false ),
				( "a*?b{0,2}?c", "aaabbc", 0, true, false ),
				( "a*b?c?d", "aaabcd", 0, true, false ),
				( "a*b??c?d", "aaabcd", 0, false, false ),
				( "a*?b?c??d", "aaabcd", 0, true, false ),
				( "a*b?c?d", "aaad", 0, false, false ),
				( "a?b*c?", "abbbc", 0, false, false ),
				( "a??b*?c?", "abbbc", 0, true, false ),
				( "a{0,2}b*c?", "aabbbc", 0, false, false ),
				( "a{0,2}?b*?c??", "aabbbc", 0, true, false ),
				( "a?b*c?|x", "abbbc", 0, false, false ),
				( "a*b?|ab", "ab", 0, true, false ),
				( "a*?b?|ab", "ab", 0, false, false ),
				( "x|a*b?", "ab", 0, true, false ),
				( @"\Aa*b?\z", "aaab", 0, true, false ),
				( @"\Ga*?b?c|b", "zaabc", 1, true, false ),
				( @"a*b?\z|b", "aaab", 0, false, false ),
				( ".*b?c", "a\nbc", 0, true, false ),
				( "(?s).*b?c", "a\nbc", 0, true, false ),
				( "[a-c]*d?x", "abcx", 0, false, false ),
				( @"\d+b?x", "٥٦bx", 0, true, true ),
				( @"\w*?x?", "Ωéx", 0, true, true ),
				( @"\p{Greek}+x?", "Ωαx", 0, true, false ),
				( "(?i)é+b?", "ÉéB", 0, true, false ),
				( "(?i)[é]*?b?", "ÉéB", 0, true, false ),
				( "😀*😀?", "😀😀", 0, true, false ),
				( "😀*?😀?", "😀😀", 0, true, false ),
				( @"\x{D800}*x?", "\uD800\uD800x", 0, false, false ),
				( "a*b?", "😀aaab", 0, true, false ),
				( "a*b?", "😀aaab", 0, false, false ),
				( "a*b?", "zaaab", 1, true, false )
			};
			for ( var index = 0; index < cases.Length; index++ ) {
				var (source, subject, start, utf, ucp) = cases[index];
				foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 63 } ) {
					yield return new object[] { index, source, subject, start, utf, ucp, defaults };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void SuffixCountRetriesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"suffix choices {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "a*b?x", "aaabx" )]
	[InlineData( "a*?a?x", "aaax" )]
	[InlineData( @"\p{Greek}+x?", "Ωαx" )]
	[InlineData( "😀*😀?x", "😀😀x" )]
	public void ValidUnicodeSuffixChoicesAgreeAcrossManagedWidths( string source, string subject ) {
		var subjectBytes = Encoding.UTF8.GetBytes( subject );
		var bytes = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true, Ucp = true } );
		var words = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true, Ucp = true } );
		Assert.True( bytes.IsSuccess ); Assert.True( words.IsSuccess );
		var byteMatch = bytes.Pattern!.Match( subjectBytes );
		var wordMatch = words.Pattern!.Match( subject.AsMemory() );
		Assert.True( byteMatch.IsSuccess ); Assert.True( wordMatch.IsSuccess );
		var coordinates = new PcreDualWidthCoordinates( subjectBytes, subject );
		Assert.Equal( coordinates.ByteCaptures( byteMatch.Match is { } b ? $"{b.ByteIndex}:{b.ByteLength}" : "" ),
			coordinates.WordCaptures( wordMatch.Match is { } w ? $"{w.CodeUnitIndex}:{w.CodeUnitLength}" : "" ) );
	}

	[Fact]
	public void SuffixChoicesChargeOrderedAdmissionAndHonorQuota() {
		var source = "a*b?c";
		var subject = "aaac";
		var completeTrace = new List<int>();
		var complete = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			MatchWorkLimit = 100, WorkObserver = completeTrace.Add
		} ).Pattern!.Match( subject.AsMemory() );
		Assert.Equal( new PcreWordCapture( 0, 4 ), complete.Match );
		Assert.True( completeTrace.Count > 0 );
		foreach ( var budget in new[] { 1, completeTrace.Count - 1 } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, pattern.Match( subject.AsMemory() ).Diagnostic?.Code );
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
		Assert.Equal( Enumerable.Range( 1, completeTrace.Count ), completeTrace );
	}

	[Fact]
	public void SuffixChoiceProductsRemainBoundedAcrossAlternatives() {
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a*b{0,63}".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a?b*c{0,31}".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a*b{0,62}|x".AsMemory() ).IsSuccess );
		foreach ( var source in new[] { "a*b{0,64}", "a?b*c{0,32}", "a*b{0,63}|x",
			"a?b*c{0,31}|x", "a*b?c?d?e?f?g?h?", "a*b*c*d?", "(a*)b?" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
