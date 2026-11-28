using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordLookaheadTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new (string Source, string Subject, int Start, bool Utf, bool Ucp)[] {
				("(?=a)a", "za", 0, true, false),
				("(?!b)a", "za", 0, true, false),
				("(?=a)b", "ab", 0, true, false),
				("(?!a)a", "ab", 0, true, false),
				("(?=)", "", 0, true, false),
				("(?!)", "", 0, true, false),
				("(?=)a", "a", 0, true, false),
				("(?!)a|a", "a", 0, true, false),
				("(?=ab)a", "ab", 0, true, false),
				("(?=ab)a", "ac", 0, true, false),
				("(?=é)é", "zé", 0, true, false),
				("(?!é).", "éa", 0, true, false),
				("(?=😀)😀", "x😀", 0, true, false),
				("(?!😀).", "😀a", 0, true, false),
				("(?=(a))a", "a", 0, true, false),
				("(?=(ab))a", "ab", 0, true, false),
				("(?=(a))", "a", 0, true, false),
				("(?=(a|ab))\\1c", "abc", 0, true, false),
				("(?=(ab|a))\\1c", "abc", 0, true, false),
				("(?=(a+))a*b\\1", "baabac", 0, true, false),
				("(?=(a+?))a*b\\1", "baabac", 0, true, false),
				("(?=(a*))\\1b", "aaab", 0, true, false),
				("(?=(a*?))\\1b", "aaab", 0, true, false),
				("(?=(a|ab)c)\\1c", "abc", 0, true, false),
				("(?!(a)c)a", "a", 0, true, false),
				("(?!(a))b|a", "a", 0, true, false),
				("(?!(a|b)c).", "bcx", 0, true, false),
				("(?!(a|b)c).", "ab", 0, true, false),
				("(?!(a)b|a)c|a", "a", 0, true, false),
				("(?=(a))b|a", "a", 0, true, false),
				("(?=(a))aX|a", "a", 0, true, false),
				("(?=(a))a\\1", "aa", 0, true, false),
				("(?=(?<x>ab))\\k<x>", "ab", 0, true, false),
				("(a)(?=\\1)\\1", "aa", 0, true, false),
				("(a)(?!\\1)b", "ab", 0, true, false),
				("(a)?(?!\\1)b", "b", 0, true, false),
				("(?J)(?=(?<x>a))(?<x>a)\\k<x>", "aa", 0, true, false),
				("(?J)(?!(?<x>b))(?<x>a)\\k<x>", "aa", 0, true, false),
				("(?|(?=(a))a|(?=(b))b)\\1", "bb", 0, true, false),
				("(?=(?=(a))a)\\1", "a", 0, true, false),
				("(?=(?!(a)b)(a))\\2", "a", 0, true, false),
				("(?!(?=(a))b)a", "a", 0, true, false),
				("(?!(?!(a)))a", "a", 0, true, false),
				("(?!(?!(a)))b|a", "a", 0, true, false),
				("(?=(a)(?!b))\\1", "a", 0, true, false),
				("(?=(a)(?!b))\\1", "ab", 0, true, false),
				("(?!(a)(?=b))a", "ab", 0, true, false),
				("(?!(a)(?!b))a", "ab", 0, true, false),
				("(?=(?:(a)|ab)c)\\1|abc", "abc", 0, true, false),
				("a?(?=(a?))a?\\1", "aaa", 0, true, false),
				("a??(?=(a?))a??\\1", "aaa", 0, true, false),
				("a{0,2}(?=(a|ab))a{0,2}\\1", "aaa", 0, true, false),
				("a{0,2}?(?!(a)b)a{0,2}?b", "aaab", 0, true, false),
				("(?=(a?b?))a?b?", "ab", 0, true, false),
				("(?:(?=(a))a)+", "aaa", 0, true, false),
				("(?:(?!b)(a))+", "aaab", 0, true, false),
				("((?=(a|ab))\\2)+c", "aabc", 0, true, false),
				("((?=(a|ab))\\2)+?c", "aabc", 0, true, false),
				("((?=(a*))a)*b", "aaab", 0, true, false),
				("((?=(a?)))*a", "a", 0, true, false),
				("((?=(a?)))+a", "a", 0, true, false),
				("((?!(b))){2}a", "a", 0, true, false),
				("((?=(a?))){0,3}a", "a", 0, true, false),
				("((?=(a?))){0,3}?a", "a", 0, true, false),
				("(?=(a))a(?=(b))b\\1\\2", "abab", 0, true, false),
				("(?=(a))a(?!\\1)(b)", "ab", 0, true, false),
				("(?=(a))(?:b|a)\\1", "aa", 0, true, false),
				("(?:(?=(a))b|(?=(a))a)\\2", "aa", 0, true, false),
				("(?:(?=(a))a|b)*\\1", "aba", 0, true, false),
				("(a)(?:(?=(b))bX|b)\\1", "aba", 0, true, false),
				("(?=(?<x>😀))\\k<x>", "x😀", 0, true, false),
				("(?i)(?=(é))\\1", "É", 0, true, false),
				("(?i)(?=(𐐀))\\1", "𐐨", 0, true, false),
				("(?=(k))(?i:\\1)", "k", 0, true, false),
				("(?i)(?=k).", "K", 0, true, false),
				("(?ir)(?=k).", "K", 0, true, false),
				("(?=(\\w+))\\1", "éΣ", 0, true, true),
				("(?=\\b)\\w+", "éΣ", 0, true, true),
				("(?!\\B)\\w+", "éΣ", 0, true, true),
				("(?=\\p{Greek}+)(\\w+)", "Σσ", 0, true, false),
				("(?=[\\p{L}]+).+", "é😀", 0, true, false),
				("(?=\\A)a", "za", 0, true, false),
				("(?=\\G)a", "za", 0, true, false),
				("(?=\\G)a", "za", 1, true, false),
				("(?=^a)a", "a", 0, true, false),
				("(?=a$)a", "a\n", 0, true, false),
				("(?m)(?=^a$)a", "x\na\ny", 0, true, false),
				("(*CR)(?m)(?=^a$)a", "x\ra\ry", 0, true, false),
				("(*NUL)(?m)(?=^a$)a", "x\u0000a\u0000y", 0, true, false),
				("(?=(\\R))\\1", "\r\n", 0, true, false),
				("(?!\\R).", "\r\nx", 0, true, false),
				("(*BSR_ANYCRLF)(?!\\R).", "\u0085", 0, true, false),
				("(?=\\Z)", "a\n", 0, true, false),
				("(?!\\z).", "😀", 0, true, false),
				("(?=(a))a", "za", 1, true, false),
				("(*pla:(a))\\1", "a", 0, true, false),
				("(*positive_lookahead:(a))\\1", "a", 0, true, false),
				("(*nla:(a))b", "b", 0, true, false),
				("(*negative_lookahead:(a))b", "b", 0, true, false),
				("(?=.)", "x", 0, true, false),
				("(?!.)", "x", 0, true, false),
				("(?=(a?))", "a", 0, true, false),
				("(?=(a?))", "", 0, true, false),
				("(?=😀)..", "😀", 0, false, false),
				("(?=(.))\\1", "😀", 0, false, false),
				("(?i)(?=é).", "É", 0, false, false),
				("(?i)(?=é).", "É", 0, false, true),
				("(?=(\\x{0}))\\1", "\u0000", 0, false, false),
				("(?=\\b)a", " a", 1, true, false),
				("(?=\ud800)\ud800", "\ud800", 0, false, false),
				("(?!\udc00).", "\udc00a", 0, false, false)
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
	public void LookaheadsAgreeWithEveryNativePolicy( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"assertion {id}: {compiled.Diagnostic}" );
		var nativePolicies = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start, defaults, ucp );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = nativePolicies[mask];
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			Assert.Equal( native.Kind == "M", result.Match is not null );
			Assert.Equal( native.Captures, Spans( result.Match ) );
		}
	}

	[Theory]
	[InlineData( @"(?=(?<x>😀é))\k<x>", "x😀é" )]
	[InlineData( @"(?!(😀)x)(😀)", "😀" )]
	[InlineData( @"(?=(?=(𐐀|😀))\1)\1", "😀" )]
	[InlineData( @"(?i)(?=(Σ+))\1", "Σςσ" )]
	[InlineData( @"(?=(a|ab))\1c|abc", "abc" )]
	[InlineData( @"((?=(a*))a)*b", "aaab" )]
	[InlineData( @"(?=(\R))\1", "\r\n" )]
	public void LookaheadCapturesKeepCrossWidthSpansAndValues( string source, string subject ) {
		var subjectBytes = Encoding.UTF8.GetBytes( subject );
		var bytes = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true, Ucp = true } );
		var words = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true, Ucp = true } );
		Assert.True( bytes.IsSuccess ); Assert.True( words.IsSuccess );
		var byteResult = bytes.Pattern!.Match( subjectBytes );
		var wordResult = words.Pattern!.Match( subject.AsMemory() );
		Assert.True( byteResult.IsSuccess ); Assert.True( wordResult.IsSuccess );
		var b = Assert.IsType<PcreByteMatch>( byteResult.Match );
		var w = Assert.IsType<PcreWordMatch>( wordResult.Match );
		var coordinates = new PcreDualWidthCoordinates( subjectBytes, subject );
		Assert.Equal( coordinates.ByteCaptures( String.Join( ",", b.Captures.Select( c => c is { } span
			? $"{span.ByteIndex}:{span.ByteLength}" : "-" ) ) ), coordinates.WordCaptures( Spans( w ) ) );
		Assert.Equal( b.Captures.Count, w.Captures.Count );
		for ( var number = 0; number < b.Captures.Count; number++ ) {
			if ( b.Captures[number] is not { } bc ) { Assert.Null( w.Captures[number] ); continue; }
			var wc = Assert.IsType<PcreWordCapture>( w.Captures[number] );
			Assert.Equal( Encoding.UTF8.GetString( subjectBytes.AsSpan( bc.ByteIndex, bc.ByteLength ) ),
				subject.Substring( wc.CodeUnitIndex, wc.CodeUnitLength ) );
		}
	}

	[Theory]
	[InlineData( "(?=a)a", "a", 8, true )]
	[InlineData( "(?!b)a", "a", 7, true )]
	[InlineData( "(?=)", "", 6, true )]
	[InlineData( "(?!)", "", 5, false )]
	[InlineData( "(?=(a))a", "a", 10, true )]
	[InlineData( "(?!(a))b", "a", 9, false )]
	[InlineData( "(?!(a)c)a", "a", 11, true )]
	[InlineData( "(?=(a|ab))a", "abc", 12, true )]
	public void EntryResolutionPruningAndRollbackConsumeExactWork( string source, string subject, int required, bool matched ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add, Policy = new() { Anchored = true }
			} ).Pattern!;
			var result = pattern.Match( subject.AsMemory() );
			if ( budget < required ) {
				Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); Assert.Null( result.Match );
			} else {
				Assert.True( result.IsSuccess ); Assert.Equal( matched, result.Match is not null );
				Assert.Equal( PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), false, 0, 1 ).Captures, Spans( result.Match ) );
			}
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Theory]
	[InlineData( "(?=(a|ab))a", "abc", 10, "0:1,0:1" )]
	[InlineData( "(?!(a))b|a", "a", 10, "0:1,-" )]
	[InlineData( "(?!(a)c)a", "a", 8, "0:1,-" )]
	public void CancellationDuringAtomicPruningOrRollbackPublishesNoPartialResult(
		string source, string subject, int cancelAt, string expected ) {
		using var cancellation = new CancellationTokenSource();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			WorkObserver = work => { if ( work == cancelAt ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => pattern.Match( subject.AsMemory(), cancellationToken: cancellation.Token ) );
		Assert.Equal( expected, Spans( pattern.Match( subject.AsMemory() ).Match ) );
	}

	[Fact]
	public void AssertionCapturesBeyondGroupZeroRemainOwnedAcrossCalls() {
		var source = @"(?=(?<x>ab))a|(?!(?<y>b)c)b".ToCharArray();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source ).Pattern!;
		Array.Fill( source, 'z' );
		var first = pattern.Match( "!zab!".AsMemory( 1, 3 ) ).Match!;
		Assert.Equal( "1:1,1:2,-", Spans( first ) );
		Assert.Equal( new PcreWordCapture( 1, 2 ), first.GetCapture( "x" ) );
		foreach ( var subject in new[] { "b", "bc", "x", "ab", "" } ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( @"(?=(?<x>ab))a|(?!(?<y>b)c)b" ), Words( subject ), false, 0 );
			var result = pattern.Match( subject.AsMemory() );
			Assert.True( result.IsSuccess ); Assert.Equal( native.Captures, Spans( result.Match ) );
		}
		Assert.Null( first.GetCapture( "y" ) ); Assert.Equal( "1:1,1:2,-", Spans( first ) );
	}

	[Fact]
	public void AssertionGuardsShareTheFrameCeilingAndDoNotSwallowResourceFailures() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( @"\A(?=(?:a)*\z)a*\z".AsMemory() ).Pattern!;
		Assert.Equal( "0:1022", Spans( pattern.Match( new string( 'a', 1022 ).AsMemory() ).Match ) );
		var failed = pattern.Match( new string( 'a', 1023 ).AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded, failed.Diagnostic?.Code ); Assert.Null( failed.Match );
		Assert.Equal( "0:2", Spans( pattern.Match( "aa".AsMemory() ).Match ) );
		var negative = PcreWordSequenceCompiler.CompileSequence( @"\A(?!(?:a)*x)".AsMemory() ).Pattern!;
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded,
			negative.Match( new string( 'a', 1023 ).AsMemory() ).Diagnostic?.Code );
		var bounded = PcreWordSequenceCompiler.CompileSequence( @"(?!((a|aa)+x))".AsMemory(), new() {
			MatchWorkLimit = 100, Policy = new() { Anchored = true }
		} ).Pattern!;
		var exhausted = bounded.Match( new string( 'a', 20 ).AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code ); Assert.Null( exhausted.Match );
		Assert.True( bounded.Match( ReadOnlyMemory<char>.Empty ).IsSuccess );
	}

	[Fact]
	public void AssertionBodiesRetainStaticAdmissionBounds() {
		var nested = String.Concat( String.Concat( Enumerable.Repeat( "(?=", 64 ) ), "a", new string( ')', 64 ) );
		foreach ( var source in new[] { nested, "(?=a?b?c?d?e?f?)",
			String.Concat( "(?=", String.Concat( Enumerable.Repeat( "a*", 64 ) ), ")" ) } ) {
			Assert.True( PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).IsSuccess );
		}
		var deep = PcreWordSequenceCompiler.CompileSequence( nested.AsMemory() ).Pattern!;
		Assert.Equal( "0:0", Spans( deep.Match( "a".AsMemory() ).Match ) );
		foreach ( var source in new[] { String.Concat( "(?=", nested, ")" ), "(?=a?b?c?d?e?f?)g?",
			String.Concat( "(?=", String.Concat( Enumerable.Repeat( "a*", 65 ) ), ")" ),
			"(?=a){65}", "(?=a){64,}", "(?=(?>(*sr:a)a)){0}", "(?*a){65}", "(?<=a){65}", "(?<!a){65}", "(?>(*sr:a)a)", "(?=a(*asr:a))" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	[Fact]
	public void StrictAssertionsPreserveValidationWindowsAndOriginalOffsets() {
		var simple = PcreWordSequenceCompiler.CompileSequence( "(?=a)a".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( "1:1", Spans( simple.Match( "\uD800a".AsMemory(), 1 ).Match ) );
		foreach ( var source in new[] { @"(?=\b)a", @"(?!\B)a", @"(?=\A)a|a" } ) {
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Pattern!;
			Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 0 ),
				pattern.Match( "\uD800a".AsMemory(), 1 ).Diagnostic );
		}
		var scalar = PcreWordSequenceCompiler.CompileSequence( "(?=😀)😀".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidStartOffset, 1 ), scalar.Match( "😀".AsMemory(), 1 ).Diagnostic );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 2 ), scalar.Match( "😀\uD800".AsMemory() ).Diagnostic );
	}

	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( c => c is { } span ? $"{span.CodeUnitIndex}:{span.CodeUnitLength}" : "-" ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
