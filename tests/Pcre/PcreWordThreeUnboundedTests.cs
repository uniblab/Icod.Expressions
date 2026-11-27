using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordThreeUnboundedTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "a*b*c*", "aaabbbccc", 0, false, false ),
				( "a*b*c*", "", 0, true, false ),
				( "a*b*c*", "bbb", 0, false, false ),
				( "a*b*c*", "aaax", 0, true, false ),
				( "a*b*c*d", "aaabbbcccd", 0, false, false ),
				( "a*b*c*d", "d", 0, true, false ),
				( "a*b*c*d", "aaad", 0, false, false ),
				( "a*b*c*d", "aaabbbx", 0, true, false ),
				( "a*?b*c*d", "aaabbbcccd", 0, false, false ),
				( "a*b*?c*d", "aaabbbcccd", 0, true, false ),
				( "a*b*c*?d", "aaabbbcccd", 0, false, false ),
				( "a*?b*?c*d", "aaabbbcccd", 0, true, false ),
				( "a*?b*c*?d", "aaabbbcccd", 0, false, false ),
				( "a*b*?c*?d", "aaabbbcccd", 0, true, false ),
				( "a*?b*?c*?d", "aaabbbcccd", 0, false, false ),
				( "a+b+c+d", "aaabbbcccd", 0, true, false ),
				( "a+?b+?c+?d", "aaabbbcccd", 0, false, false ),
				( "a{2,}b{2,}c{2,}d", "aaabbbcccd", 0, true, false ),
				( "a{2,}?b{2,}?c{2,}?d", "aaabbbcccd", 0, false, false ),
				( "a{2,}b{2,}c{2,}d", "abbcccd", 0, false, false ),
				( "a*a*a*", "aaa", 0, true, false ),
				( "a*?a*a*", "aaa", 0, false, false ),
				( "a*a*?a*", "aaa", 0, true, false ),
				( "a*a*a*?", "aaa", 0, false, false ),
				( "a*?a*?a*?", "aaa", 0, true, false ),
				( "a+a+a+", "aaaa", 0, false, false ),
				( "a+?a+?a+?", "aaaa", 0, true, false ),
				( "a*a*a*b", "aaab", 0, false, false ),
				( "a*?a*?a*?b", "aaab", 0, true, false ),
				( "a*ba*ca*d", "aaabaacad", 0, false, false ),
				( "a*?ba*?ca*?d", "aaabaacad", 0, true, false ),
				( "a*b{2}c*d{2}e*f", "aaabbccddeef", 0, true, false ),
				( "x?a*b*c*d", "xaaabbbcccd", 0, false, false ),
				( "x??a*?b*?c*?d", "xaaabbbcccd", 0, true, false ),
				( "x{0,2}a*b*c*d", "xxaaabbbcccd", 0, false, false ),
				( "a*b*c*d|x", "x", 0, true, false ),
				( "x|a*b*c*d", "aaabbbcccd", 0, false, false ),
				( @"\Aa*b*c*\z", "aaabbbccc", 0, true, false ),
				( @"\Ga*b*c*d|x", "zaabbccd", 1, false, false ),
				( @"a*b*c*\z|c", "aaabbbccc", 0, true, false ),
				( ".*b*c*d", "a\nbbcd", 0, true, false ),
				( "(?s).*b*c*d", "a\nbbcd", 0, true, false ),
				( "[a-c]*d*e*x", "abcddeex", 0, false, false ),
				( @"\d*\w*\p{Greek}*x", "٥٦Ωαx", 0, true, true ),
				( "(?i)é*b*c*d", "ÉéBcD", 0, true, false ),
				( "😀*😀*😀*x", "😀😀😀x", 0, true, false ),
				( "😀*?😀*?😀*?x", "😀😀😀x", 0, true, false ),
				( @"\x{D800}*\x{D800}*\x{D800}*x", "\uD800\uD800x", 0, false, false ),
				( "a*b*c*d", "😀aaabbbcccd", 0, true, false ),
				( "a*b*c*d", "😀aaabbbcccd", 0, false, false ),
				( "a*b*c*d", "zaaabbbcccd", 1, true, false )
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
	public void ThirdCountRetriesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"three repeats {id}: {compiled.Diagnostic}" );
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
	[InlineData( "a*?a*?a*?x", "aaax" )]
	[InlineData( @"\d*\w*\p{Greek}*x", "٥٦Ωαx" )]
	[InlineData( "😀*😀*😀*x", "😀😀😀x" )]
	public void ValidUnicodeThreeRepeatsAgreeAcrossManagedWidths( string source, string subject ) {
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
	public void ThirdDepthSharesCancellationAwareWorkQuota() {
		var source = "a*?a*?a*?b";
		var subject = "aaaa";
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

	[Fact]
	public void ThirdDepthKeepsBoundsAndUnsupportedWrappers() {
		Assert.True( PcreWordSequenceCompiler.CompileSequence( "x?a{64,}bc{64,}de{64,}f".AsMemory() ).IsSuccess );
		foreach ( var source in new[] { "a{65,}b*c*d", "a*b{65,}c*d", "a*b*c{65,}d", "(?>(*THEN)a*)b*c*d", "a{65,}+b*c*d" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
