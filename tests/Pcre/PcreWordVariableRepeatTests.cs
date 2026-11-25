using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordVariableRepeatTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "a?", "", 0, false, false ), ( "a?", "a", 0, false, false ),
				( "a??", "a", 0, false, false ), ( "a??", "ba", 0, true, false ),
				( "a{0,1}", "a", 0, true, false ), ( "a{0,1}?", "a", 0, false, false ),
				( "a{0,2}", "aa", 0, false, false ), ( "a{0,2}?", "aa", 0, true, false ),
				( "a{1,3}", "aaaa", 0, false, false ), ( "a{1,3}?", "aaaa", 0, true, false ),
				( "a{2,4}", "baaaac", 0, false, false ), ( "a{2,4}?", "baaaac", 0, true, false ),
				( "a{0,2}b", "ab", 0, false, false ), ( "a{0,2}?b", "ab", 0, true, false ),
				( "a{0,2}b", "aab", 0, true, false ), ( "a{0,2}?b", "aab", 0, false, false ),
				( "a{0,2}a", "aa", 0, false, false ), ( "a{0,2}?a", "aa", 0, true, false ),
				( "a{1,3}a", "aaa", 0, false, false ), ( "a{1,3}?a", "aaa", 0, true, false ),
				( "a{2}b{0,2}c", "aabbc", 0, false, false ),
				( "a{2}b{0,2}?c", "aabbc", 0, true, false ),
				( "a{0,2}|ab", "ab", 0, false, false ),
				( "a{0,2}?|ab", "ab", 0, true, false ),
				( "x|a{0,2}", "aa", 0, false, false ),
				( "x|a{0,2}?", "aa", 0, true, false ),
				( @"\Aa{0,2}\z", "aa", 0, true, false ),
				( @"\Aa{0,2}?\z", "aa", 0, false, false ),
				( @"\Ga{0,2}b|b", "xaab", 1, true, false ),
				( @"a{0,2}\z|a", "aaa", 0, false, false ),
				( ".{0,2}b", "a\nb", 0, true, false ),
				( "(?s).{0,2}b", "a\nb", 0, true, false ),
				( "[a-c]{1,3}d", "abcd", 0, false, false ),
				( "[a-c]{1,3}?d", "abcd", 0, true, false ),
				( @"\d{1,3}x", "٥٦x", 0, true, true ),
				( @"\w{0,2}x", "Ωéx", 0, true, true ),
				( @"\p{Greek}{1,2}x", "Ωαx", 0, true, false ),
				( "(?i)é{1,2}x", "Ééx", 0, true, false ),
				( "(?i)[é]{1,2}?x", "Ééx", 0, true, false ),
				( "😀{1,2}x", "😀😀x", 0, true, false ),
				( "😀{1,2}?x", "😀😀x", 0, true, false ),
				( @"\x{D800}{1,2}x", "\uD800\uD800x", 0, false, false ),
				( "a{0,2}?|😀", "😀", 0, true, false ),
				( "a{1,2}", "😀aa", 0, true, false ),
				( "a{1,2}", "😀aa", 0, false, false ),
				( "a{1,2}", "zaa", 1, true, false ),
				( "a{0,2}?", "😀", 0, true, false )
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
	public void OrderedCountChoicesAgreeWithNativePolicies( int id, string source, string subject,
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
	[InlineData( "a{0,2}b", "aab" )]
	[InlineData( "a{0,2}?b", "aab" )]
	[InlineData( @"\p{Greek}{1,2}x", "Ωαx" )]
	[InlineData( "😀{1,2}?x", "😀😀x" )]
	public void ValidUnicodeCountChoicesAgreeAcrossManagedWidths( string source, string subject ) {
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
	[InlineData( "a{0,2}b", false, 8 )]
	[InlineData( "a{0,2}b", true, 10 )]
	[InlineData( "a{0,2}?b", false, 7 )]
	[InlineData( "a{0,2}?b", true, 9 )]
	public void CountRetriesConsumeExactWork( string source, bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var match = pattern.Match( "ab".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 2 ), match.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void CountAndBranchBoundsStayExplicit() {
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a{0,63}".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a{1,64}".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a{0,62}|x".AsMemory() ).IsSuccess );
		foreach ( var source in new[] { "a{0,64}", "a{0,63}|x", "a{1,65}",
			"a*b", "a+b", "a{2,}b", "(?:a){1,2}", "a{0,2}+" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
