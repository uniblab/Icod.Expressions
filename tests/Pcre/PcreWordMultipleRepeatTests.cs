using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordMultipleRepeatTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "a?b?", "", 0, false, false ), ( "a?b?", "ab", 0, false, false ),
				( "a?b?", "a", 0, true, false ), ( "a?b?", "b", 0, true, false ),
				( "a??b?", "ab", 0, false, false ), ( "a?b??", "ab", 0, true, false ),
				( "a??b??", "ab", 0, false, false ), ( "a??b??", "zab", 1, true, false ),
				( "a?a?", "a", 0, false, false ), ( "a??a?", "a", 0, true, false ),
				( "a?a??", "a", 0, false, false ), ( "a??a??", "aa", 0, true, false ),
				( "a{0,2}a{0,2}", "aaa", 0, false, false ),
				( "a{0,2}?a{0,2}", "aaa", 0, true, false ),
				( "a{0,2}a{0,2}?", "aaa", 0, false, false ),
				( "a{0,2}?a{0,2}?", "aaa", 0, true, false ),
				( "a{1,2}a{0,2}b", "aaab", 0, false, false ),
				( "a{1,2}?a{0,2}b", "aaab", 0, true, false ),
				( "a{1,2}a{0,2}?b", "aaab", 0, false, false ),
				( "a{0,2}b{0,2}c", "aabbc", 0, true, false ),
				( "a{0,2}?b{0,2}?c", "aabbc", 0, false, false ),
				( "a{2}b?c?d", "aabcd", 0, true, false ),
				( "a?b{2}c?d", "abbcd", 0, false, false ),
				( "a?b?c?d", "abcd", 0, true, false ),
				( "a??b??c??d", "abcd", 0, false, false ),
				( "a?a?a?a?b", "aaaab", 0, true, false ),
				( "a??a?a??a?b", "aaaab", 0, false, false ),
				( "a?b?|ab", "ab", 0, true, false ),
				( "a??b??|ab", "ab", 0, false, false ),
				( @"\Aa?b?\z", "ab", 0, true, false ),
				( @"\Ga??b?x|b", "zabx", 1, false, false ),
				( @"a?b?\z|a", "ab", 0, true, false ),
				( ".?b?x", "a\nbx", 0, true, false ),
				( "(?s).?b?x", "\nbx", 0, true, false ),
				( "[a-c]?\\d?x", "b٥x", 0, true, true ),
				( @"\p{Greek}?\w?x", "Ωéx", 0, true, true ),
				( "(?i)é?b?x", "ÉBx", 0, true, false ),
				( "😀?a?x", "😀ax", 0, true, false ),
				( "😀??a??x", "😀ax", 0, true, false ),
				( @"\x{D800}?a?x", "\uD800ax", 0, false, false ),
				( "a?b?", "😀ab", 0, true, false ),
				( "a?b?", "😀ab", 0, false, false ),
				( "a?b?", "zab", 1, true, false )
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
	public void OrderedCountProductsAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"repeat product {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "a?b?x", "abx" )]
	[InlineData( "a??b??x", "abx" )]
	[InlineData( @"\p{Greek}?\w?x", "Ωéx" )]
	[InlineData( "😀?a?x", "😀ax" )]
	public void ValidUnicodeRepeatProductsAgreeAcrossManagedWidths( string source, string subject ) {
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
	[InlineData( false, 7 )]
	[InlineData( true, 8 )]
	public void AdjacentRetryChargesEachChoice( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( "a?a?".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var match = pattern.Match( "a".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), match.Match?.Captures[0] ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void ProductAndGlobalBranchBoundsStayExplicit() {
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a{0,7}b{0,7}".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a?b?c?d?e?f?".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a{0,6}b{0,8}|x".AsMemory() ).IsSuccess );
		foreach ( var source in new[] { "a{0,7}b{0,7}|x", "a{0,8}b{0,7}",
			"a?b?c?d?e?f?g?", "a{0,6}b{0,8}|x|y", "(?>(*ACCEPT)a?)b?" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
