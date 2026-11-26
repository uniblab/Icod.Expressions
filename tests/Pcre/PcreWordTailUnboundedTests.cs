using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordTailUnboundedTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "a*", "", 0, false, false ), ( "a*", "aaa", 0, false, false ),
				( "a*?", "aaa", 0, false, false ), ( "a*", "baaa", 0, true, false ),
				( "a*?", "baaa", 0, true, false ), ( "a+", "aaa", 0, false, false ),
				( "a+?", "aaa", 0, true, false ), ( "a+", "bbb", 0, false, false ),
				( "a+?", "baaa", 0, true, false ), ( "a{2,}", "aaaa", 0, false, false ),
				( "a{2,}?", "aaaa", 0, true, false ), ( "a{2,}", "a", 0, false, false ),
				( "a{0,}", "aaa", 0, true, false ), ( "a{1,}", "aaa", 0, false, false ),
				( "ab*", "abbb", 0, false, false ), ( "ab*?", "abbb", 0, true, false ),
				( "a{2}b*", "aabbb", 0, false, false ),
				( "a{2}b*?", "aabbb", 0, true, false ),
				( "a?b*", "abbb", 0, false, false ),
				( "a??b*?", "abbb", 0, true, false ),
				( "a{0,2}b*", "aabbb", 0, false, false ),
				( "a{0,2}?b*?", "aabbb", 0, true, false ),
				( "a*|b", "b", 0, false, false ),
				( "a*?|b", "b", 0, true, false ),
				( "b|a*", "aaa", 0, false, false ),
				( @"\Aa*", "aaa", 0, true, false ),
				( @"\Aa*?", "aaa", 0, false, false ),
				( @"\Ga*|b", "baaa", 1, true, false ),
				( ".*", "a\nb", 0, true, false ),
				( ".*?", "a\nb", 0, false, false ),
				( "(?s).*", "a\nb", 0, true, false ),
				( "[a-c]*", "bca", 0, false, false ),
				( "[^a]*?", "bbb", 0, true, false ),
				( @"\d+", "٥٦", 0, true, true ),
				( @"\w*?", "Ωé", 0, true, true ),
				( @"\p{Greek}+", "Ωα", 0, true, false ),
				( "(?i)é+", "Éé", 0, true, false ),
				( "(?i)[é]*?", "Éé", 0, true, false ),
				( "😀+", "😀😀", 0, true, false ),
				( "😀*?", "😀", 0, true, false ),
				( @"\x{D800}+", "\uD800\uD800", 0, false, false ),
				( "a+", "😀aaa", 0, true, false ),
				( "a+", "😀aaa", 0, false, false ),
				( "a+", "zaaa", 1, true, false )
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
	public void UnboundedTailAgreesWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"tail {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "a*", "aaa" )]
	[InlineData( @"\p{Greek}+", "Ωα" )]
	[InlineData( "😀+", "😀😀" )]
	[InlineData( "a?b*?", "abbb" )]
	public void ValidUnicodeTailsAgreeAcrossManagedWidths( string source, string subject ) {
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
	[InlineData( "a*", false, 6 )]
	[InlineData( "a*", true, 8 )]
	[InlineData( "a*?", false, 3 )]
	[InlineData( "a*?", true, 5 )]
	public void TailWorkIsChargedAndLazyCanStopEarly( string source, bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var match = pattern.Match( "aa".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, source.EndsWith( '?' ) ? 0 : 2 ), match.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void LongTailRemainsSubjectDrivenAndBudgeted() {
		var subject = new string( 'a', 10_000 );
		var trace = new List<int>();
		var greedy = PcreWordSequenceCompiler.CompileSequence( "a*".AsMemory(), new() {
			MatchWorkLimit = 20, WorkObserver = trace.Add
		} ).Pattern!;
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, greedy.Match( subject.AsMemory() ).Diagnostic?.Code );
		Assert.Equal( Enumerable.Range( 1, 20 ), trace );
		var lazy = PcreWordSequenceCompiler.CompileSequence( "a*?".AsMemory(), new() {
			MatchWorkLimit = 3
		} ).Pattern!;
		Assert.Equal( new PcreWordCapture( 0, 0 ), lazy.Match( subject.AsMemory() ).Match );
	}

	[Fact]
	public void TailAdmissionAndContinuationRemainExplicit() {
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a{64,}".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a?b*".AsMemory() ).IsSuccess );
		foreach ( var source in new[] { "a{65,}", "a*+" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
