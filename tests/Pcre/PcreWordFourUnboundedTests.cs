using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordFourUnboundedTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "a*b*c*d*", "aaabbbcccddd", 0, false, false ),
				( "a*b*c*d*", "", 0, true, false ),
				( "a*b*c*d*x", "aaabbbcccdddx", 0, false, false ),
				( "a*?b*c*d*x", "aaabbbcccdddx", 0, true, false ),
				( "a*b*?c*d*x", "aaabbbcccdddx", 0, false, false ),
				( "a*b*c*?d*x", "aaabbbcccdddx", 0, true, false ),
				( "a*b*c*d*?x", "aaabbbcccdddx", 0, false, false ),
				( "a*?b*?c*?d*?x", "aaabbbcccdddx", 0, true, false ),
				( "a+b+c+d+x", "aaabbbcccdddx", 0, false, false ),
				( "a+?b+?c+?d+?x", "aaabbbcccdddx", 0, true, false ),
				( "a{2,}b{2,}c{2,}d{2,}x", "aaabbbcccdddx", 0, false, false ),
				( "a{2,}?b{2,}?c{2,}?d{2,}?x", "abbcccdddx", 0, true, false ),
				( "a*a*a*a*x", "aaaax", 0, false, false ),
				( "a*?a*?a*?a*?x", "aaaax", 0, true, false ),
				( "a+a+a+a+x", "aaaaax", 0, false, false ),
				( "a+?a+?a+?a+?x", "aaaaax", 0, true, false ),
				( "a*b?c*d*e*x", "aaabcccdddeex", 0, false, false ),
				( "a*b*c?d*e*x", "aaabbbcddeex", 0, true, false ),
				( "a*b*c*d?e*x", "aaabbbccdeex", 0, false, false ),
				( "a*b*c*d*e?x", "aaabbbcccddex", 0, true, false ),
				( "q?a*b?c*d?e*f?g*h?x", "qaaabccdeefgghx", 0, false, false ),
				( "q??a*?b??c*?d??e*?f??g*?h??x", "qaaabccdeefgghx", 0, true, false ),
				( "q{0,2}a*b?c*d?e*f?g*h?x", "qqaaabccdeefgghx", 0, false, false ),
				( "a*a?a*a?a*a?a*a?x", "aaaax", 0, true, false ),
				( "a*?a??a*?a??a*?a??a*?a??x", "aaaax", 0, false, false ),
				( "a*b?c*d?e*f?g*x|y", "y", 0, true, false ),
				( "y|a*b?c*d?e*f?g*x", "aaabccdeefggx", 0, false, false ),
				( @"\Aa*b?c*d?e*f?g*x\z", "aaabccdeefggx", 0, true, false ),
				( @"\Ga*b?c*d?e*f?g*x|y", "zaabccdeefggx", 1, false, false ),
				( @"a*b?c*d?e*f?g*x\z|c", "aaabccdeefggx", 0, true, false ),
				( ".*b?c*d?e*f?g*x", "a\nbccdeefgx", 0, true, false ),
				( "(?s).*b?c*d?e*f?g*x", "a\nbccdeefgx", 0, true, false ),
				( "[a-c]*d?e*f?g*h?i*x", "abcdeefghiix", 0, false, false ),
				( @"\d*x?\w*y?\p{Greek}*z?α*", "٥٦xΩyαzα", 0, true, true ),
				( "(?i)é*b?c*d?e*f?g*x", "ÉéBcDeFgX", 0, true, false ),
				( "😀*x?😀*y?😀*z?😀*w", "😀😀x😀y😀z😀w", 0, true, false ),
				( "😀*?x??😀*?y??😀*?z??😀*?w", "😀😀x😀y😀z😀w", 0, true, false ),
				( @"\x{D800}*x?\x{D800}*y?\x{D800}*z?\x{D800}*w", "\uD800x\uD800y\uD800z\uD800w", 0, false, false ),
				( "a*b?c*d?e*f?g*x", "😀aaabccdeefggx", 0, true, false ),
				( "a*b?c*d?e*f?g*x", "zaaabccdeefggx", 1, false, false ),
				( "a*b?c*d?e*f?g*", "", 0, true, false ),
				( "a*b?c*d?e*f?g*", "b", 0, false, false ),
				( "a*b?c*d?e*f?g*", "aaax", 0, true, false )
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
	public void FourthRepeatAndFiveChoiceDepthsAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"four repeats {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "a*?a*?a*?a*?x", "aaaax" )]
	[InlineData( @"\d*x?\w*y?\p{Greek}*z?α*", "٥٦xΩyαzα" )]
	[InlineData( "😀*x?😀*y?😀*z?😀*w", "😀😀x😀y😀z😀w" )]
	public void ValidUnicodeFourRepeatsAgreeAcrossManagedWidths( string source, string subject ) {
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
	public void FiveChoiceProductsStayWithinTheGlobalBound() {
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "a*b{0,63}c*d*e*".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "q?a*b?c*d?e*f?g*h?".AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "x?a{64,}bc{64,}de{64,}fg{64,}h".AsMemory() ).IsSuccess );
		foreach ( var source in new[] { "a*b{0,64}c*d*e*", "q{0,3}a*b?c*d?e*f?g*h?|x",
			"a{65,}b*c*d*", "(?>(*THEN)a*)b*c*d*", "a{65,}+b*c*d*" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	[Fact]
	public void FourthDepthSharesCancellationAwareWorkQuota() {
		var source = "a*?a??a*?a??a*?a??a*?x";
		var subject = "aa";
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
