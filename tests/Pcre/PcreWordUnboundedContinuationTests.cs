using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordUnboundedContinuationTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "a*b", "aaab", 0, false, false ), ( "a*b", "b", 0, true, false ),
				( "a*b", "aaac", 0, false, false ), ( "a*?b", "aaab", 0, true, false ),
				( "a*?b", "b", 0, false, false ), ( "a+b", "aaab", 0, true, false ),
				( "a+b", "b", 0, false, false ), ( "a+?b", "aaab", 0, true, false ),
				( "a{2,}b", "aaaab", 0, false, false ),
				( "a{2,}?b", "aaaab", 0, true, false ),
				( "a{2,}b", "ab", 0, false, false ),
				( "a*a", "aaa", 0, false, false ), ( "a*?a", "aaa", 0, true, false ),
				( "a+a", "aaa", 0, false, false ), ( "a+?a", "aaa", 0, true, false ),
				( "a{2,}a", "aaaa", 0, false, false ),
				( "a{2,}?a", "aaaa", 0, true, false ),
				( "a*b{2}c", "aaabbc", 0, false, false ),
				( "a*?b{2}c", "aaabbc", 0, true, false ),
				( "a?b*c", "abbbc", 0, false, false ),
				( "a??b*?c", "abbbc", 0, true, false ),
				( "a{0,2}b*c", "aabbbc", 0, false, false ),
				( "a{0,2}?b*?c", "aabbbc", 0, true, false ),
				( "a*b|c", "c", 0, false, false ),
				( "a*?b|c", "c", 0, true, false ),
				( "a*b|ab", "ab", 0, false, false ),
				( @"\Aa*\z", "aaa", 0, true, false ),
				( @"\Aa*?\z", "aaa", 0, false, false ),
				( @"a*\z|b", "ab", 0, true, false ),
				( @"\Ga*b|b", "xaab", 1, false, false ),
				( ".*b", "a\nb", 0, true, false ),
				( "(?s).*b", "a\nb", 0, true, false ),
				( "(?s).*?b", "a\nbb", 0, false, false ),
				( "[a-c]*d", "abcd", 0, false, false ),
				( "[a-c]*?d", "abcd", 0, true, false ),
				( @"\d+x", "٥٦x", 0, true, true ),
				( @"\w*?x", "Ωéx", 0, true, true ),
				( @"\p{Greek}+x", "Ωαx", 0, true, false ),
				( "(?i)é+x", "Ééx", 0, true, false ),
				( "(?i)[é]*?x", "Ééx", 0, true, false ),
				( "😀+x", "😀😀x", 0, true, false ),
				( "😀*?😀", "😀😀", 0, true, false ),
				( @"\x{D800}+x", "\uD800\uD800x", 0, false, false ),
				( "a*b", "😀aaab", 0, true, false ),
				( "a*b", "😀aaab", 0, false, false ),
				( "a*b", "zaaab", 1, true, false )
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
	public void UnboundedContinuationAgreesWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"continuation {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "a*b", "aaab" )]
	[InlineData( @"\p{Greek}+x", "Ωαx" )]
	[InlineData( "😀+x", "😀😀x" )]
	[InlineData( "a??b*?c", "abbbc" )]
	public void ValidUnicodeContinuationsAgreeAcrossManagedWidths( string source, string subject ) {
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

	[Theory]
	[InlineData( "a*b", "aab", false, 7 )]
	[InlineData( "a*b", "aab", true, 10 )]
	[InlineData( "a*?b", "ab", false, 6 )]
	[InlineData( "a*?b", "ab", true, 8 )]
	[InlineData( "a*a", "aa", false, 9 )]
	[InlineData( "a*a", "aa", true, 11 )]
	public void ContinuationAndRewindChargeExactWork( string source, string subject, bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var match = pattern.Match( subject.AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, subject.Length ), match.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void LongSuffixRetriesStayWithinWorkQuota() {
		var subject = String.Concat( new string( 'a', 10_000 ), "b" );
		var trace = new List<int>();
		var pattern = PcreWordSequenceCompiler.CompileSequence( "a*b".AsMemory(), new() {
			MatchWorkLimit = 20, WorkObserver = trace.Add
		} ).Pattern!;
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, pattern.Match( subject.AsMemory() ).Diagnostic?.Code );
		Assert.Equal( Enumerable.Range( 1, 20 ), trace );
	}

	[Fact]
	public void ContinuationAdmissionKeepsSuffixOrderingExplicit() {
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a{64,}b".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a?b*c".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a*b{2}c".AsMemory() ).IsSuccess );
		foreach ( var source in new[] { "a{65,}b", "a*b*", "a*b+c", "a+b*", "(?:a)*b", "a*+b" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
