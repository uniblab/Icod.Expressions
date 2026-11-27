using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordNestedAlternativeTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "(?:a|ab)c?", "abc", 0, false, false ),
				( "(?:ab|a)c?", "abc", 0, false, false ),
				( "(?:a|ab)c??", "abc", 0, false, false ),
				( "(?:a|ab)c{0,2}", "abcc", 0, false, false ),
				( "a*(?:ab|b)c", "aabc", 0, false, false ),
				( "a*?(?:ab|b)c", "aabc", 0, false, false ),
				( "(?:a*|ab)b", "aab", 0, false, false ),
				( "(?:a*?|ab)b", "aab", 0, false, false ),
				( "(?:a*|ab)c?", "aabc", 0, false, false ),
				( "(?:a*?|ab)c?", "aabc", 0, false, false ),
				( "a?(?:a|ab)c?", "aabc", 0, false, false ),
				( "a??(?:a|ab)c??", "aabc", 0, false, false ),
				( "(?:ab|a)(?:c|bc)d?", "abcd", 0, false, false ),
				( "(?:a(?:b|bc)|ab)c?", "abcc", 0, false, false ),
				( "(?:(?:a|ab)c?|abc)d?", "abcd", 0, false, false ),
				( "(?:a|(?:ab|abc))d?", "abcd", 0, false, false ),
				( "(?:|a)b?", "ab", 0, false, false ),
				( "(?:a|)b?", "ab", 0, false, false ),
				( "(?:|a)b?", "", 0, false, false ),
				( "(?:)a*", "aa", 0, false, false ),
				( "(?:(?:)|a)", "a", 0, false, false ),
				( "(?:a*|b*)c", "bbbc", 0, false, false ),
				( "(?:a*?b?|a)b*?c", "aabbc", 0, false, false ),
				( "(?:a?b*|ab?)c?", "abbc", 0, false, false ),
				( "(?:a*|b)c|b", "b", 0, false, false ),
				( "(?:a*|b)c", "zaabc", 0, false, false ),
				( "(?:a|ab)c?", "zabc", 1, false, false ),
				( "(?:a*|aa)(?:b*|bb)c", "aabbc", 0, false, false ),
				( "(?:a*?|aa)(?:b*?|bb)c", "aabbc", 0, false, false ),
				( "(?:(?:a*)b*)c", "aabbc", 0, false, false ),
				( "(?:(?:a*?)b*?)c", "aabbc", 0, false, false ),
				( "(?:(?:a|ab)c?)d*e", "abcddde", 0, false, false ),
				( "(?i:é|éx)y?", "ÉxY", 0, true, false ),
				( "(?i:a|ab)(?-i)C?", "AbC", 0, false, false ),
				( "(?s:.*|a)\nx", "a\nx", 0, false, false ),
				( "(?m:^a|b)$", "a\nb", 0, false, false ),
				( "(?:\\Ga|b)c?", "zac", 1, false, false ),
				( "(?:\\A|b)c?", "bc", 0, false, false ),
				( "(?:\\b\\w*|x)y?", "αβy", 0, true, true ),
				( "(?:\\B|a)b?", "ab", 0, false, false ),
				( "(?:😀*|😀x)y?", "😀😀xy", 0, true, false ),
				( "(?:😀*?|😀x)y?", "😀😀xy", 0, true, false ),
				( "(?:\\p{Greek}*|x)y?", "αβy", 0, true, true ),
				( "(?i:[a-z]*|x)y?", "AbY", 0, true, true ),
				( "(?:\\R*)\\r\\nx", "\r\nx", 0, true, false ),
				( "(?:\\R*?)\\r\\nx", "\r\nx", 0, true, false ),
				( "(*BSR_ANYCRLF)(?:\\R*|a)\\r\\nx", "\r\n\r\nx", 0, true, false ),
				( "(*CR)(?m:(?:^a|b)$)", "a\rb", 0, false, false ),
				( "(*NUL)(?:(?:a.)|a)$", "a\u0000", 0, false, false ),
				( "(?:\\x{D800}*|x)y?", "\ud800y", 0, false, false ),
				( "(?:a{2}|ab)c?", "abc", 0, false, false ),
				( "(?:(?:ab|a)b*)c", "abbc", 0, false, false ),
				( "a*(?:(?:ab|b)c?)d?", "aabcd", 0, false, false ),
				( "(?:a(?:b|bc)c?|ab)d?", "abccd", 0, false, false ),
				( String.Concat( Nested( "a*", 8 ), "ab" ), "aaab", 0, false, false ),
				( String.Concat( Nested( "a*?|ab", 8 ), "c?" ), "abc", 0, true, false ),
				( String.Concat( Nested( "a*", 32 ), "ab" ), "aaab", 0, false, false ),
				( String.Concat( Nested( "a*?|ab", 32 ), "c?" ), "abc", 0, true, false ),
				( String.Concat( Nested( "a*", 64 ), "ab" ), "aaab", 0, false, false ),
				( String.Concat( Nested( "a*?|ab", 64 ), "c?" ), "abc", 0, true, false )
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
	public void NestedAlternativesAndContinuationsAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"nested alternatives {id}: {compiled.Diagnostic}" );
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
	[InlineData( "(?:a|ab)c?", "abc" )]
	[InlineData( "(?:(?:a*)b*)c", "aabbc" )]
	[InlineData( "(?:😀*|😀x)y?", "😀😀xy" )]
	[InlineData( "(?i:é|éx)y?", "ÉxY" )]
	[InlineData( @"(?:\p{Greek}*|x)y?", "αβy" )]
	[InlineData( @"(?:\R*)\r\nx", "\r\nx" )]
	public void NestedUnicodePathsAgreeAcrossManagedWidths( string source, string subject ) {
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
	[InlineData( 1 )]
	[InlineData( 8 )]
	[InlineData( 32 )]
	[InlineData( 64 )]
	public void GroupEntryAndReturnHaveExactWorkAndCancellationBoundaries( int depth ) {
		var source = Nested( "a", depth );
		var required = 2 * depth + 3;
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add
			} );
			Assert.True( compiled.IsSuccess );
			var result = compiled.Pattern!.Match( "a".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), result.Match?.Captures[0] ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
		using var cancellation = new CancellationTokenSource();
		var cancelled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			WorkObserver = work => { if ( work == depth + 3 ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => cancelled.Match( "a".AsMemory(), cancellationToken: cancellation.Token ) );
		Assert.Equal( new PcreWordCapture( 0, 1 ), cancelled.Match( "a".AsMemory() ).Match?.Captures[0] );
	}

	[Theory]
	[InlineData( "(?:a*?|aa)(?:a??|a)x", "aaaa" )]
	[InlineData( "(?:a|ab)c?", "abc" )]
	[InlineData( "(?:(?:a*)b*)c", "aabbc" )]
	public void BacktrackingAcrossReturnsPreservesExactQuota( string source, string subject ) {
		var completeTrace = new List<int>();
		var complete = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			WorkObserver = completeTrace.Add, Policy = new() { EndAnchored = true }
		} ).Pattern!.Match( subject.AsMemory() );
		Assert.True( complete.IsSuccess );
		var required = completeTrace.Count;
		Assert.True( required > 1 );
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add, Policy = new() { EndAnchored = true }
			} ).Pattern!;
			var result = pattern.Match( subject.AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); }
			else {
				Assert.Equal( complete.Diagnostic, result.Diagnostic );
				Assert.Equal( complete.Match?.Captures[0], result.Match?.Captures[0] );
			}
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void NestedBoundsIncludeEnclosingAndSiblingPaths() {
		var choices = Repeated( "(?:a|b)", 6 );
		var repeats = String.Concat( "(?:", Repeated( "a*", 32 ), ")(?:", Repeated( "b*", 32 ), ")" );
		var expanded = String.Concat( "(?:", Repeated( "a{64}", 63 ), "a{63})" );
		foreach ( var source in new[] { Nested( "a", 64 ), choices, repeats, expanded,
			String.Concat( "(?:", String.Join( '|', Enumerable.Repeat( "a", 64 ) ), ")" ) } ) {
			Assert.True( PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).IsSuccess );
		}
		foreach ( var source in new[] { Nested( "a", 65 ), String.Concat( choices, "(?:a|b)" ),
			String.Concat( choices, "|x" ), String.Concat( choices, "a?" ), String.Concat( repeats, "a*" ),
			String.Concat( "a", expanded ),
			"(?>(*SKIP:N)a|b)", "(?*a){65}b", "(?<*a){65}b", "a{65,}+", "(?:a(*SKIP:N)|b)" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	[Fact]
	public void ReturnsResetAcrossBranchesCandidatesAndRepeatedCalls() {
		foreach ( var source in new[] { "(?:(?:a*?|ab)c?|b)d", "(?:(?:a*?|ab)c?|b)d|x" } ) {
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory() );
			Assert.True( compiled.IsSuccess );
			foreach ( var subject in new[] { "abd", "bd", "zabd", "x", "abd", "" } ) {
				var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), false, 0 );
				Assert.True( native.Kind is "M" or "N" );
				var result = compiled.Pattern!.Match( subject.AsMemory() );
				Assert.True( result.IsSuccess );
				Assert.Equal( native.Captures, result.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
			}
		}
	}

	[Fact]
	public void NestedAssertionsRetainStrictValidationWindow() {
		var subject = "\uD800a";
		foreach ( var source in new[] { @"(?:\ba|b)", @"(?:\A|a)", @"(?:a|(?:\Bb))" } ) {
			Assert.Equal( "X", PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 1 ).Kind );
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
			Assert.True( compiled.IsSuccess );
			Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 0 ),
				compiled.Pattern!.Match( subject.AsMemory(), 1 ).Diagnostic );
		}
		var plain = "(?:(?:a)|b)";
		Assert.Equal( "M", PcreNative16Oracle.ExecuteSequence( Words( plain ), Words( subject ), true, 1 ).Kind );
		var pattern = PcreWordSequenceCompiler.CompileSequence( plain.AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( new PcreWordCapture( 1, 1 ), pattern.Match( subject.AsMemory(), 1 ).Match?.Captures[0] );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidStartOffset, 1 ),
			pattern.Match( "😀a".AsMemory(), 1 ).Diagnostic );
	}

	[Fact]
	public void NestedCompilationOwnsPatternAndHonorsCancellation() {
		var source = "(?:a|ab)c?".ToCharArray();
		var compiled = PcreWordSequenceCompiler.CompileSequence( source );
		Assert.True( compiled.IsSuccess );
		Array.Fill( source, 'x' );
		Assert.Equal( new PcreWordCapture( 0, 1 ), compiled.Pattern!.Match( "abc".AsMemory() ).Match?.Captures[0] );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreWordSequenceCompiler.CompileSequence(
			Nested( "a", 64 ).AsMemory(), cancellationToken: cancellation.Token ) );
	}

	private static string Repeated( string source, int count ) => String.Concat( Enumerable.Repeat( source, count ) );
	private static string Nested( string source, int depth ) => String.Concat( Repeated( "(?:", depth ), source, new string( ')', depth ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
