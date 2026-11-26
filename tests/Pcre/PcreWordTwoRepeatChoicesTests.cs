using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordTwoRepeatChoicesTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "a*b?c*", "aaabccc", 0, false, false ),
				( "a*b?c*", "aaaccc", 0, true, false ),
				( "a*b??c*", "aaabccc", 0, false, false ),
				( "a*?b?c*", "aaabccc", 0, true, false ),
				( "a*b?c*?", "aaabccc", 0, false, false ),
				( "a*?b??c*?", "aaabccc", 0, true, false ),
				( "a*b?c*d", "aaabcccd", 0, true, false ),
				( "a*?b??c*?d", "aaabcccd", 0, false, false ),
				( "a*b{0,2}c*d", "aaabbcccd", 0, false, false ),
				( "a*b{0,2}?c*?d", "aaabbcccd", 0, true, false ),
				( "a*b?c*", "b", 0, false, false ),
				( "a*b?c*", "", 0, true, false ),
				( "a*b*c?", "aaabbc", 0, false, false ),
				( "a*b*c??", "aaabbc", 0, true, false ),
				( "a*?b*?c?", "aaabbc", 0, false, false ),
				( "a*b*c?d", "aaabbcd", 0, true, false ),
				( "a*b*?c??d", "aaabbcd", 0, false, false ),
				( "a*b*c{0,2}d", "aaabbccd", 0, true, false ),
				( "a*?b*?c{0,2}?d", "aaabbccd", 0, false, false ),
				( "a*a*a?", "aaa", 0, false, false ),
				( "a*?a*a??", "aaa", 0, true, false ),
				( "a*a*?a?", "aaa", 0, false, false ),
				( "a*?a*?a??", "aaa", 0, true, false ),
				( "a*a?a*b", "aaab", 0, false, false ),
				( "a*?a??a*?b", "aaab", 0, true, false ),
				( "a*a*a?b", "aaab", 0, false, false ),
				( "a*?a*?a??b", "aaab", 0, true, false ),
				( "a*a?a*a?b", "aaab", 0, false, false ),
				( "a*?a??a*?a??b", "aaab", 0, true, false ),
				( "x?a*b?c*d?", "xaaabcd", 0, false, false ),
				( "x??a*?b??c*?d??", "xaaabcd", 0, true, false ),
				( "x{0,2}a*b{0,2}c*d?e", "xxaabbccde", 0, true, false ),
				( "a*b?c*d?|x", "aaabcd", 0, false, false ),
				( "x|a*b?c*d?", "aaabcd", 0, true, false ),
				( @"\Aa*b?c*d?\z", "aaabcd", 0, true, false ),
				( @"\Ga*b?c*d?|x", "zaabcd", 1, false, false ),
				( @"a*b?c*d?\z|b", "aabcd", 0, true, false ),
				( ".*b?c*d?", "a\nbccd", 0, true, false ),
				( "(?s).*b?c*d?", "a\nbccd", 0, true, false ),
				( "[a-c]*d?e*f?x", "abcdeefx", 0, false, false ),
				( @"\d*b?\w*x?", "٥٦bΩx", 0, true, true ),
				( @"\p{Greek}*x?\w*y?", "Ωαxβy", 0, true, true ),
				( "(?i)é*b?c*d?", "ÉéBccD", 0, true, false ),
				( "😀*x?😀*y?", "😀😀x😀y", 0, true, false ),
				( "😀*?x??😀*?y??", "😀😀x😀y", 0, true, false ),
				( @"\x{D800}*x?\x{D800}*y?", "\uD800x\uD800y", 0, false, false ),
				( "a*b?c*d?", "😀aaabcd", 0, true, false ),
				( "a*b?c*d?", "😀aaabcd", 0, false, false ),
				( "a*b?c*d?", "zaaabcd", 1, true, false )
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
	public void NestedFiniteChoicesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"nested choice {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "a*?a??a*?x", "aaax" )]
	[InlineData( @"\p{Greek}*x?\w*y?", "Ωαxβy" )]
	[InlineData( "😀*x?😀*y?", "😀😀x😀y" )]
	public void ValidUnicodeNestedChoicesAgreeAcrossManagedWidths( string source, string subject ) {
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
	public void ChoiceProductsAreBoundedAcrossAllThreeDepths() {
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a*b{0,63}c*".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a*b{0,31}c*d?".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a?b*c{0,15}d*e?".AsMemory() ).IsSuccess );
		foreach ( var source in new[] { "a*b{0,64}c*", "a*b{0,63}c*d?",
			"a*b{0,31}c*d?|x", "a?b*c{0,15}d*e?|x", "a*b?c*d?e?f?g?h?i?",
			"(a*)b?c*" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	[Fact]
	public void NestedChoicesShareCancellationAwareWorkQuota() {
		var source = "a*?a??b*?b??c";
		var subject = "aabb";
		var completeTrace = new List<int>();
		var completePattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			MatchWorkLimit = 10_000, WorkObserver = completeTrace.Add
		} ).Pattern!;
		var complete = completePattern.Match( subject.AsMemory() );
		Assert.True( complete.IsSuccess );
		Assert.Null( complete.Match );
		Assert.True( completeTrace.Count > 1 );
		var trace = new List<int>();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			MatchWorkLimit = completeTrace.Count - 1, WorkObserver = trace.Add
		} ).Pattern!;
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded,
			pattern.Match( subject.AsMemory() ).Diagnostic?.Code );
		Assert.Equal( Enumerable.Range( 1, completeTrace.Count - 1 ), trace );
		Assert.Equal( Enumerable.Range( 1, completeTrace.Count ), completeTrace );
		using var cancelled = new CancellationTokenSource();
		cancelled.Cancel();
		Assert.Throws<OperationCanceledException>( () => pattern.Match( "aaaa".AsMemory(), cancellationToken: cancelled.Token ) );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
