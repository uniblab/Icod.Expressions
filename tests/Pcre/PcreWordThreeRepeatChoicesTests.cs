using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordThreeRepeatChoicesTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "a*b?c*d*", "aaabccddd", 0, false, false ),
				( "a*b??c*d*", "aaabccddd", 0, true, false ),
				( "a*?b?c*d*", "aaabccddd", 0, false, false ),
				( "a*b?c*?d*", "aaabccddd", 0, true, false ),
				( "a*b?c*d*?", "aaabccddd", 0, false, false ),
				( "a*?b??c*?d*?", "aaabccddd", 0, true, false ),
				( "a*b?c*d*x", "aaabccdddx", 0, false, false ),
				( "a*?b??c*?d*?x", "aaabccdddx", 0, true, false ),
				( "a*b{0,2}c*d*x", "aaabbccdddx", 0, false, false ),
				( "a*b{0,2}?c*?d*?x", "aaabbccdddx", 0, true, false ),
				( "a*b*c?d*", "aaabccddd", 0, false, false ),
				( "a*b*c??d*", "aaabccddd", 0, true, false ),
				( "a*?b*?c?d*", "aaabccddd", 0, false, false ),
				( "a*b*c{0,2}d*x", "aaabccdddx", 0, true, false ),
				( "a*b*?c{0,2}?d*?x", "aaabccdddx", 0, false, false ),
				( "a*b*c*d?", "aaabccddd", 0, true, false ),
				( "a*b*c*d??", "aaabccddd", 0, false, false ),
				( "a*?b*?c*?d?", "aaabccddd", 0, true, false ),
				( "a*b*c*d{0,2}x", "aaabccddx", 0, false, false ),
				( "a*?b*?c*?d{0,2}?x", "aaabccddx", 0, true, false ),
				( "a*b?c*d?e*f?x", "aaabccddeefx", 0, false, false ),
				( "a*?b??c*?d??e*?f??x", "aaabccddeefx", 0, true, false ),
				( "a*a?a*a?a*", "aaa", 0, false, false ),
				( "a*?a??a*?a??a*?", "aaa", 0, true, false ),
				( "a*a?a*a?x", "aaax", 0, false, false ),
				( "a*?a??a*?a??x", "aaax", 0, true, false ),
				( "a*a?b*b?c*c?x", "aaabbccx", 0, false, false ),
				( "a*?a??b*?b??c*?c??x", "aaabbccx", 0, true, false ),
				( "x?a*b?c*d?e*f?y", "xaaabccddeefy", 0, false, false ),
				( "x??a*?b??c*?d??e*?f??y", "xaaabccddeefy", 0, true, false ),
				( "x{0,2}a*b{0,2}c*d?e*f", "xxaabbccdeef", 0, true, false ),
				( "a*b?c*d?e*|x", "x", 0, false, false ),
				( "x|a*b?c*d?e*", "aaabcde", 0, true, false ),
				( @"\Aa*b?c*d?e*\z", "aaabcde", 0, true, false ),
				( @"\Ga*b?c*d?e*|x", "zaabcde", 1, false, false ),
				( @"a*b?c*d?e*\z|c", "aaabcde", 0, true, false ),
				( ".*b?c*d?e*", "a\nbccde", 0, true, false ),
				( "(?s).*b?c*d?e*", "a\nbccde", 0, true, false ),
				( "[a-c]*d?e*f?g*x", "abcdeefgx", 0, false, false ),
				( @"\d*x?\w*y?\p{Greek}*z?", "٥٦xΩyαz", 0, true, true ),
				( "(?i)é*b?c*d?e*", "ÉéBcDee", 0, true, false ),
				( "😀*x?😀*y?😀*z?", "😀😀x😀y😀z", 0, true, false ),
				( "😀*?x??😀*?y??😀*?z??", "😀😀x😀y😀z", 0, true, false ),
				( @"\x{D800}*x?\x{D800}*y?\x{D800}*z?", "\uD800x\uD800y\uD800z", 0, false, false ),
				( "a*b?c*d?e*", "😀aaabcde", 0, true, false ),
				( "a*b?c*d?e*", "😀aaabcde", 0, false, false ),
				( "a*b?c*d?e*", "zaaabcde", 1, true, false ),
				( "a*b?c*d?e*", "", 0, true, false ),
				( "a*b?c*d?e*", "b", 0, false, false ),
				( "a*b?c*d?e*", "aaax", 0, true, false )
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
	public void ChoicesAtThreeRepeatDepthsAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"three-repeat choices {id}: {compiled.Diagnostic}" );
		var nativePolicies = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start, defaults, ucp );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = nativePolicies[mask];
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "a*?a??a*?a??x", "aaax" )]
	[InlineData( @"\d*x?\w*y?\p{Greek}*z?", "٥٦xΩyαz" )]
	[InlineData( "😀*x?😀*y?😀*z?", "😀😀x😀y😀z" )]
	public void ValidUnicodeChoicesAgreeAcrossManagedWidths( string source, string subject ) {
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
	public void FourChoiceProductsStayWithinTheGlobalBound() {
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a*b{0,63}c*d*".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a?b*c{0,3}d*e{0,3}f*g?".AsMemory() ).IsSuccess );
		foreach ( var source in new[] { "a*b{0,64}c*d*", "a?b*c{0,3}d*e{0,3}f*g?|x",
			"a*b?c?d?e*f?g?h?i*j?", "(?>(*THEN)a*)b?c*d*" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	[Fact]
	public void FourChoiceLevelsShareCancellationAwareWorkQuota() {
		var source = "a*?a??b*?b??c*?c??x";
		var subject = "aab";
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
		Assert.Throws<OperationCanceledException>( () => pattern.Match( subject.AsMemory(), cancellationToken: cancelled.Token ) );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
