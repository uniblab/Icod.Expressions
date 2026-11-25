using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordTwoUnboundedTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "a*b*", "aaabbb", 0, false, false ), ( "a*b*", "", 0, false, false ),
				( "a*b*", "bbb", 0, true, false ), ( "a*b*", "aaac", 0, true, false ),
				( "a*b*c", "aaabbbc", 0, false, false ), ( "a*b*c", "ac", 0, true, false ),
				( "a*b*c", "bbbc", 0, false, false ), ( "a*b*c", "aaaac", 0, true, false ),
				( "a*b*c", "aaabbbx", 0, false, false ),
				( "a*?b*c", "aaabbbc", 0, true, false ),
				( "a*b*?c", "aaabbbc", 0, false, false ),
				( "a*?b*?c", "aaabbbc", 0, true, false ),
				( "a+b+c", "aaabbbc", 0, false, false ),
				( "a+?b+?c", "aaabbbc", 0, true, false ),
				( "a{2,}b{2,}c", "aaabbbc", 0, true, false ),
				( "a{2,}?b{2,}?c", "aaabbbc", 0, false, false ),
				( "a{2,}b{2,}c", "abbc", 0, false, false ),
				( "a*a*", "aaa", 0, false, false ), ( "a*?a*", "aaa", 0, true, false ),
				( "a*a*?", "aaa", 0, false, false ), ( "a*?a*?", "aaa", 0, true, false ),
				( "a+a+", "aaa", 0, false, false ), ( "a+?a+?", "aaa", 0, true, false ),
				( "a*a*b", "aaab", 0, true, false ),
				( "a*?a*b", "aaab", 0, false, false ),
				( "a*a*?b", "aaab", 0, true, false ),
				( "a*?a*?b", "aaab", 0, false, false ),
				( "a*ba*c", "aaabaac", 0, true, false ),
				( "a*?ba*?c", "aaabaac", 0, false, false ),
				( "x?a*b*c", "xaaabbbc", 0, false, false ),
				( "x??a*b*c", "xaaabbbc", 0, true, false ),
				( "x{0,2}a*b*c", "xxaaabbbc", 0, true, false ),
				( "a*b*c|x", "x", 0, false, false ),
				( "a*a*b|a", "aaa", 0, true, false ),
				( @"\Aa*b*\z", "aaabbb", 0, true, false ),
				( @"\Ga*b*c|x", "zaabbc", 1, false, false ),
				( @"a*b*\z|b", "aabb", 0, true, false ),
				( ".*b*c", "a\nbbc", 0, true, false ),
				( "(?s).*b*c", "a\nbbc", 0, true, false ),
				( "[a-c]*d*c", "abcdc", 0, false, false ),
				( @"\d*\w*c", "٥٦Ωc", 0, true, true ),
				( @"\p{Greek}*\w*c", "Ωαβc", 0, true, true ),
				( "(?i)é*b*c", "ÉéBc", 0, true, false ),
				( "😀*😀*x", "😀😀x", 0, true, false ),
				( "😀*?😀*?x", "😀😀x", 0, true, false ),
				( @"\x{D800}*\x{D800}*x", "\uD800\uD800x", 0, false, false ),
				( "a*b*c", "😀aaabbbc", 0, true, false ),
				( "a*b*c", "😀aaabbbc", 0, false, false ),
				( "a*b*c", "zaaabbbc", 1, true, false )
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
	public void TwoCountRetriesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"two repeats {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "a*?a*x", "aaax" )]
	[InlineData( @"\p{Greek}*\w*x", "Ωαβx" )]
	[InlineData( "😀*😀*x", "😀😀x" )]
	public void ValidUnicodeTwoRepeatsAgreeAcrossManagedWidths( string source, string subject ) {
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
	public void NestedRetriesChargeWorkAndHonorCancellation() {
		var subject = new string( 'a', 1000 );
		var trace = new List<int>();
		var pattern = PcreWordSequenceCompiler.CompileSequence( "a*a*b".AsMemory(), new() {
			MatchWorkLimit = 30, WorkObserver = trace.Add
		} ).Pattern!;
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, pattern.Match( subject.AsMemory() ).Diagnostic?.Code );
		Assert.Equal( Enumerable.Range( 1, 30 ), trace );
		using var cancelled = new CancellationTokenSource();
		cancelled.Cancel();
		Assert.Throws<OperationCanceledException>( () => pattern.Match( subject.AsMemory(), cancellationToken: cancelled.Token ) );
	}

	[Fact]
	public void OnlyTwoUnboundedRepeatsAndStraightContinuationsAreAdmitted() {
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a{64,}bc{64,}d".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "x?a*b*c".AsMemory() ).IsSuccess );
		foreach ( var source in new[] { "a*b*c*", "a*b?c*", "a*b*c?", "a*b{0,2}c*",
			"a{65,}b*c", "a*b{65,}c", "(a*)b*c", "a*+b*c" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
