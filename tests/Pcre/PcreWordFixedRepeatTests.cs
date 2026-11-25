using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordFixedRepeatTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "a{0}", "", 0, false, false ), ( "a{0}", "a", 0, false, false ),
				( "a{0}", "za", 1, true, false ), ( "a{1}", "za", 0, false, false ),
				( "a{2}", "zaab", 0, false, false ), ( "a{2}", "a", 0, true, false ),
				( "a{2,2}", "aaa", 0, false, false ), ( "a{2}?", "aa", 0, true, false ),
				( "a{3}", "baaa", 0, false, false ), ( "a{4}", "aaaa", 0, true, false ),
				( "a{2}b", "aab", 0, false, false ), ( "ab{2}c", "abbc", 0, true, false ),
				( "a{0}b", "xb", 0, false, false ), ( "a{0}b", "ab", 0, true, false ),
				( "a{2}b{0}c", "aac", 0, false, false ), ( "a{2}b{2}", "aabb", 0, true, false ),
				( "a{0}|ab", "ab", 0, false, false ), ( "a{0}|ab", "ab", 0, true, false ),
				( "a{2}|a", "a", 0, false, false ), ( "a{2}|a", "aa", 0, true, false ),
				( "a|a{2}", "aa", 0, false, false ), ( "a{2}|ab", "ab", 0, true, false ),
				( @"\Aa{2}\z", "aa", 0, false, false ), ( @"\Ga{2}|b", "xaa", 1, true, false ),
				( @"\Aa{0}\z", "", 0, true, false ), ( @"a{2}\z|a", "aaab", 0, false, false ),
				( ".{2}", "ab", 0, false, false ), ( ".{2}", "a\nb", 0, true, false ),
				( "(?s).{2}", "a\n", 0, true, false ), ( @"[a-c]{2}", "xbc", 0, false, false ),
				( @"[^a]{2}", "abb", 0, true, false ), ( @"[[:digit:]]{2}", "٥٦", 0, true, true ),
				( @"\d{2}", "٥٦", 0, true, true ), ( @"\w{2}", "Ωé", 0, true, true ),
				( @"\p{Greek}{2}", "Ωα", 0, true, false ), ( @"\h{2}", " \t", 0, true, false ),
				( @"\x{E9}{2}", "éé", 0, true, false ), ( "(?i)é{2}", "Éé", 0, true, false ),
				( "(?i)[é]{2}", "Éé", 0, true, false ), ( "(?i)a{2}", "Aa", 0, false, false ),
				( "😀{2}", "x😀😀", 0, true, false ), ( "😀{2}", "😀", 0, true, false ),
				( @"\x{1F600}{2}", "😀😀", 0, true, true ),
				( @"\x{D800}{2}", "\uD800\uD800", 0, false, false ),
				( "a{2}|😀{1}", "😀", 0, true, false ),
				( "a{0}|😀", "😀", 0, true, false ),
				( "a{2}", "😀aa", 0, true, false ),
				( "a{2}", "😀aa", 0, false, false ),
				( "a{2}", "zaa", 1, true, false ),
				( "a{0}", "😀", 0, true, false )
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
	public void ExactRepeatsAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"repeat {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "a{2}", "aa" )]
	[InlineData( @"\p{Greek}{2}", "Ωα" )]
	[InlineData( "😀{2}", "😀😀" )]
	[InlineData( "a{0}|😀", "😀" )]
	public void ValidUnicodeRepeatsAgreeAcrossManagedWidths( string source, string subject ) {
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
	[InlineData( false, 4 )]
	[InlineData( true, 6 )]
	public void ExpandedComparisonsConsumeExactWork( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( "a{2}".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var match = pattern.Match( "aa".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 2 ), match.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void RepetitionAndExpandedNodeBoundsAreExplicit() {
		Assert.Equal( new PcreWordCapture( 0, 64 ),
			PcreWordSequenceCompiler.CompileSequence( "a{64}".AsMemory() ).Pattern!.Match( new string( 'a', 64 ).AsMemory() ).Match );
		Assert.True( PcreWordSequenceCompiler.CompileSequence(
			String.Concat( Enumerable.Repeat( "a{64}", 64 ) ).AsMemory() ).IsSuccess );
		foreach ( var source in new[] { "a{65}", String.Concat( Enumerable.Repeat( "a{64}", 64 ).Append( "a" ) ),
			"a*b*", "a+b+", "a{2,}b*", "(a){0}", "(?:a){2}",
			"a{2}+", "(?>a){2}" } ) {
			var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory() );
			Assert.True( result.Diagnostic?.Code == PcreWordDiagnosticCode.UnsupportedSyntax,
				$"{source}: {result.Diagnostic}" );
		}
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
