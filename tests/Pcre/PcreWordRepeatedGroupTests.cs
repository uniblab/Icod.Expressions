using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordRepeatedGroupTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "(?:ab)*c", "ababc", 0, false, false ),
				( "(?:ab)*?c", "ababc", 0, false, false ),
				( "(?:ab)+c", "ababc", 0, false, false ),
				( "(?:ab)+?c", "ababc", 0, false, false ),
				( "(?:ab){2}c", "ababc", 0, false, false ),
				( "(?:ab){1,3}c", "abababc", 0, false, false ),
				( "(?:ab){1,3}?c", "abababc", 0, false, false ),
				( "(?:ab){2,}c", "abababc", 0, false, false ),
				( "(?:ab){2,}?c", "abababc", 0, false, false ),
				( "(?:ab)?c", "abc", 0, false, false ),
				( "(?:ab)??c", "abc", 0, false, false ),
				( "(?:ab){0}c", "c", 0, false, false ),
				( "(?:a|ab)*c?", "ababc", 0, false, false ),
				( "(?:a|ab)*?c?", "ababc", 0, false, false ),
				( "(?:a|ab)+c?", "ababc", 0, false, false ),
				( "(?:a|ab)+?c?", "ababc", 0, false, false ),
				( "(?:a|ab){1,3}c?", "ababc", 0, false, false ),
				( "(?:a|ab){1,3}?c?", "ababc", 0, false, false ),
				( "a?(?:a|ab)*c?", "aababc", 0, false, false ),
				( "a??(?:a|ab)*?c??", "aababc", 0, false, false ),
				( "(?:ab|a)*b", "aab", 0, false, false ),
				( "(?:ab|a)*?b", "aab", 0, false, false ),
				( "(?:a*b?)*c", "aabbc", 0, false, false ),
				( "(?:a*?b??)*?c", "aabbc", 0, false, false ),
				( "(?:(?:ab|a)b?)*c", "abbabc", 0, false, false ),
				( "(?:(?:a|ab)c?){2}d?", "abcd", 0, false, false ),
				( "(?:a|b)*(?:bc|c)*d", "abcbcd", 0, false, false ),
				( "(?:a|b)*?(?:bc|c)*?d", "abcbcd", 0, false, false ),
				( "(?:a+)+b", "aaaab", 0, false, false ),
				( "(?:a+?)+?b", "aaaab", 0, false, false ),
				( "(?:a?)*b", "aaab", 0, false, false ),
				( "(?:a??)*?b", "aaab", 0, false, false ),
				( "(?:(?:a*)*)*b", "aaab", 0, false, false ),
				( "(?:(?:a*?)*)*?b", "aaab", 0, false, false ),
				( "(?:|a)*", "aa", 0, false, false ),
				( "(?:|a)*?", "aa", 0, false, false ),
				( "(?:|a)+", "aa", 0, false, false ),
				( "(?:|a)+?", "aa", 0, false, false ),
				( "(?:|a){1,}", "aa", 0, false, false ),
				( "(?:|a){1,}?", "aa", 0, false, false ),
				( "(?:|a){2,}", "aa", 0, false, false ),
				( "(?:|a){2,}?", "aa", 0, false, false ),
				( "(?:|a){0,2}", "aa", 0, false, false ),
				( "(?:|a){0,2}?", "aa", 0, false, false ),
				( "(?:|a){2}", "aa", 0, false, false ),
				( "(?:a|)*", "aa", 0, false, false ),
				( "(?:a|)*?", "aa", 0, false, false ),
				( "(?:)*", "", 0, false, false ),
				( "(?:)*?", "", 0, false, false ),
				( "(?:)+", "", 0, false, false ),
				( "(?:){2,}", "", 0, false, false ),
				( "(?:){0,3}", "", 0, false, false ),
				( "(?:\\b)*a", "a", 0, false, false ),
				( "(?:\\A)+a", "a", 0, false, false ),
				( "(?:\\B|a)*b", "aab", 0, false, false ),
				( "(?:\\G|a)*b", "zaab", 1, false, false ),
				( "(?m:(?:^a$\\n?)+)", "a\na\n", 0, false, false ),
				( "(*CR)(?m:(?:^a$\\r?)+)", "a\ra\r", 0, false, false ),
				( "(*NUL)(?:a.)*b", "a\u0000b", 0, false, false ),
				( "(?i:é|x)+y?", "Ééxy", 0, true, false ),
				( "(?:😀|x)*y?", "😀😀xy", 0, true, false ),
				( "(?:😀|x)*?y?", "😀😀xy", 0, true, false ),
				( "(?:\\p{Greek}|x)+y?", "αβxy", 0, true, true ),
				( "(?aW:\\w)*x", "éx", 0, true, true ),
				( "(?:\\R)*\\r\\nx", "\r\n\r\nx", 0, true, false ),
				( "(?:\\R)*?\\r\\nx", "\r\n\r\nx", 0, true, false ),
				( "(*BSR_ANYCRLF)(?:\\R|a)+x", "\r\nax", 0, true, false ),
				( "(?:\\R|\\r)*\\n", "\r\n", 0, true, false ),
				( "(?:\\x{D800}|x)*y", "\ud800xy", 0, false, false ),
				( "(?:a|ab)*c?", "zababc", 1, false, false ),
				( "(?:a|ab)*x|b", "b", 0, false, false ),
				( "(?:(?:|a)*)*", "aa", 0, false, false ),
				( "(?:(?:|a)*?)*?", "aa", 0, false, false ),
				( "(?:a{0,2}|b?){0,3}c", "aabc", 0, false, false ),
				( "(?:a|ab){0,64}c?", "ababc", 0, false, false ),
				( "(?:a){64}", new string( 'a', 64 ), 0, false, false ),
				( "(?:a){64,}b", String.Concat( new string( 'a', 65 ), "b" ), 0, false, false ),
				( "(?:a?b?c?d?e?f?)*x", "abcdefx", 0, false, false ),
				( String.Concat( NestedRepeat( "a", 8 ), "b" ), "aab", 0, true, false ),
				( String.Concat( NestedRepeat( "a", 64 ), "b" ), "b", 0, true, false )
			};
			for ( var id = 0; id < cases.Length; id++ ) {
				var (source, subject, start, utf, ucp) = cases[id];
				foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 16, 32, 63 } ) {
					yield return new object[] { id, source, subject, start, utf, ucp, defaults };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void RepeatedGroupsAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"repeated group {id}: {compiled.Diagnostic}" );
		var nativePolicies = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start, defaults, ucp );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = nativePolicies[mask];
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			Assert.Equal( native.Captures, result.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "(?:ab|a)*b", "aab" )]
	[InlineData( "(?:|a){2,}", "aa" )]
	[InlineData( "(?:(?:a*)*)*b", "aaab" )]
	[InlineData( "(?i:é|x)+y?", "Ééxy" )]
	[InlineData( "(?:😀|x)*y?", "😀😀xy" )]
	[InlineData( @"(?:\p{Greek}|x)+y?", "αβxy" )]
	[InlineData( @"(?:\R)*\r\nx", "\r\n\r\nx" )]
	public void RepeatedGroupsAgreeAcrossManagedWidths( string source, string subject ) {
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
	[InlineData( "(?:)*", 6 )]
	[InlineData( "(?:)*?", 3 )]
	[InlineData( "(?:)+", 6 )]
	[InlineData( "(?:){1,}", 6 )]
	[InlineData( "(?:){2}", 9 )]
	[InlineData( "(?:){0}", 3 )]
	[InlineData( "(?:){0,3}", 12 )]
	public void EmptyIterationsHaveExactWorkBoundaries( string source, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add
			} );
			Assert.True( compiled.IsSuccess );
			var result = compiled.Pattern!.Match( ReadOnlyMemory<char>.Empty );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 0 ), result.Match?.Captures[0] ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void BacktrackFrameCeilingIsIndependentOfWorkAndResetsBetweenCalls() {
		var compiled = PcreWordSequenceCompiler.CompileSequence( @"\A(?:a)*\z".AsMemory() );
		Assert.True( compiled.IsSuccess );
		var pattern = compiled.Pattern!;
		Assert.Equal( new PcreWordCapture( 0, 1023 ), pattern.Match( new string( 'a', 1023 ).AsMemory() ).Match?.Captures[0] );
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded,
			pattern.Match( new string( 'a', 1024 ).AsMemory() ).Diagnostic?.Code );
		Assert.Equal( new PcreWordCapture( 0, 2 ), pattern.Match( "aa".AsMemory() ).Match?.Captures[0] );
		var lazy = PcreWordSequenceCompiler.CompileSequence( @"\A(?:a)*?\z".AsMemory() ).Pattern!;
		Assert.Equal( new PcreWordCapture( 0, 2048 ), lazy.Match( new string( 'a', 2048 ).AsMemory() ).Match?.Captures[0] );
	}

	[Fact]
	public void AmbiguousIterationsStopAtWorkQuota() {
		var trace = new List<int>();
		var compiled = PcreWordSequenceCompiler.CompileSequence( @"\A(?:a|aa)+x".AsMemory(), new() {
			MatchWorkLimit = 10_000, WorkObserver = trace.Add
		} );
		Assert.True( compiled.IsSuccess );
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded,
			compiled.Pattern!.Match( new string( 'a', 20 ).AsMemory() ).Diagnostic?.Code );
		Assert.Equal( Enumerable.Range( 1, 10_000 ), trace );
	}

	[Fact]
	public void IterationReturnCancellationAndStateReset() {
		using var cancellation = new CancellationTokenSource();
		var pattern = PcreWordSequenceCompiler.CompileSequence( "(?:a)*".AsMemory(), new() {
			WorkObserver = work => { if ( work == 6 ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => pattern.Match( "aaaa".AsMemory(), cancellationToken: cancellation.Token ) );
		Assert.Equal( new PcreWordCapture( 0, 4 ), pattern.Match( "aaaa".AsMemory() ).Match?.Captures[0] );
		Assert.Throws<OperationCanceledException>( () => PcreWordSequenceCompiler.CompileSequence(
			NestedRepeat( "a", 64 ).AsMemory(), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void CompileBoundsApplyToRetainedBodiesAndRuntimeCounts() {
		var choices = "(?:a?b?c?d?e?f?)*";
		foreach ( var source in new[] { choices, "(?:a|b){0,64}", "(?:a){64,}", NestedRepeat( "a", 64 ) } ) {
			Assert.True( PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).IsSuccess );
		}
		foreach ( var source in new[] { String.Concat( choices, "g?" ), String.Concat( choices, "|x" ),
			"(?:a?b?c?d?e?f?g?)*", "(?:a){65}", "(?:a){65,}", "(?:a){0,65}", NestedRepeat( "a", 65 ),
			String.Concat( "(?:", Repeated( "a*", 64 ), ")*" ),
			String.Concat( "(?:", Repeated( "a{64}", 64 ), ")*" ),
			"(?:a){65,}+", "(?>(*SKIP:N)a)*", "(?=a){65}", "(?:a(*SKIP:N))*" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	[Fact]
	public void ZeroCountBodiesKeepNativeValidationLookbehind() {
		var subject = "\uD800a";
		foreach ( var source in new[] { @"(?:\b){0}a", @"(?:\A){0}a" } ) {
			Assert.Equal( "X", PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 1 ).Kind );
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
			Assert.True( compiled.IsSuccess );
			Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 0 ),
				compiled.Pattern!.Match( subject.AsMemory(), 1 ).Diagnostic );
		}
		var plain = PcreWordSequenceCompiler.CompileSequence( "(?:a){0}a".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( new PcreWordCapture( 1, 1 ), plain.Match( subject.AsMemory(), 1 ).Match?.Captures[0] );
	}

	[Fact]
	public void RepeatedChoiceStateResetsAcrossCandidatesBranchesAndOwnedSources() {
		var source = "(?:a|ab)*c|b".ToCharArray();
		var compiled = PcreWordSequenceCompiler.CompileSequence( source );
		Assert.True( compiled.IsSuccess );
		Array.Fill( source, 'x' );
		foreach ( var subject in new[] { "ababc", "b", "zzabc", "x", "ababc" } ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( "(?:a|ab)*c|b" ), Words( subject ), false, 0 );
			Assert.True( native.Kind is "M" or "N" );
			var result = compiled.Pattern!.Match( subject.AsMemory() );
			Assert.True( result.IsSuccess );
			Assert.Equal( native.Captures, result.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	private static string Repeated( string source, int count ) => String.Concat( Enumerable.Repeat( source, count ) );
	private static string NestedRepeat( string source, int depth ) => String.Concat( Repeated( "(?:", depth ), source, Repeated( ")*", depth ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
