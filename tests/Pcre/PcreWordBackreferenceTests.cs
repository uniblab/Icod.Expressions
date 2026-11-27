using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordBackreferenceTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new (string Source, string Subject, int Start, bool Utf, bool Ucp)[] {
				("(a)\\1", "zaa", 0, true, false),
				("(ab)\\1", "zabab", 0, true, false),
				("(ab)\\1", "abax", 0, true, false),
				("(ab)\\1", "aba", 0, true, false),
				("(a|ab)\\1", "abab", 0, true, false),
				("((a)|ab)\\1", "abab", 0, true, false),
				("(a(b)?)+\\2", "abab", 0, true, false),
				("(a(b)?)+\\1", "abaa", 0, true, false),
				("(a|b\\1)+", "aba", 0, true, false),
				("^(\\1a|a)+$", "aaa", 0, true, false),
				("(a\\1|b)+", "bab", 0, true, false),
				("(\\1)", "", 0, true, false),
				("\\1(a)", "aa", 0, true, false),
				("(a)?\\1", "", 0, true, false),
				("(a)?\\1", "a", 0, true, false),
				("(a)?\\1", "aa", 0, true, false),
				("()\\1", "", 0, true, false),
				("()\\1a", "a", 0, true, false),
				("(a|)\\1b", "aab", 0, true, false),
				("(a|)\\1b", "b", 0, true, false),
				("(a)?\\1*", "", 0, true, false),
				("(a)?\\1+", "", 0, true, false),
				("(a){0}\\1{0}", "", 0, true, false),
				("(a){0}\\1", "", 0, true, false),
				("(a)\\g1", "aa", 0, true, false),
				("(a)\\g{1}", "aa", 0, true, false),
				("(a)(b)\\g{-2}\\g{-1}", "abab", 0, true, false),
				("\\g{+1}(a)", "aa", 0, true, false),
				("(a)\\g{-1}", "aa", 0, true, false),
				("(a)(b)(c)(d)(e)(f)(g)(h)(i)(j)\\10", "abcdefghijj", 0, true, false),
				("(?<x>a)\\k<x>", "aa", 0, true, false),
				("(?<x>a)\\k'x'", "aa", 0, true, false),
				("(?<x>a)\\k{x}", "aa", 0, true, false),
				("(?<x>a)\\g{x}", "aa", 0, true, false),
				("(?P<x>a)(?P=x)", "aa", 0, true, false),
				("(?<é>a)\\k<é>", "aa", 0, true, false),
				("(?<𐐀>😀)\\k<𐐀>", "😀😀", 0, true, false),
				("(?J)(?:(?<x>a)|(?<x>b))\\k<x>", "bb", 0, true, false),
				("(?J)(?<x>a)(?<x>b)\\k<x>", "aba", 0, true, false),
				("(?J)(?<x>a)(?<x>b)\\k<x>", "abb", 0, true, false),
				("(?J)(?<x>)(?<x>b)\\k<x>", "b", 0, true, false),
				("(?J)(?<x>a)?(?<x>b)\\k<x>", "bb", 0, true, false),
				("(?J)(?<x>a)?(?<x>b)?\\k<x>", "", 0, true, false),
				("(?J)(?:(?<x>a)|(?<x>b))+\\k<x>", "aba", 0, true, false),
				("(?J)(?:(?<x>a)|(?<x>b))+\\k<x>", "abb", 0, true, false),
				("(?|(a)|(b))\\1", "bb", 0, true, false),
				("(?J)(?|(a)(?<x>b)|(?<x>c))\\k<x>", "abb", 0, true, false),
				("(?J)(?|(a)(?<x>b)|(?<x>c))\\k<x>", "aba", 0, true, false),
				("(?|(?<x>a)|(?<x>b))+\\k<x>", "abb", 0, true, false),
				("(?n)(a)(?<x>b)\\1", "abb", 0, true, false),
				("(a)\\1?b", "aab", 0, true, false),
				("(a)\\1??b", "aab", 0, true, false),
				("(a)\\1*b", "aaaab", 0, true, false),
				("(a)\\1*?b", "aaaab", 0, true, false),
				("(a)\\1+b", "aaaab", 0, true, false),
				("(a)\\1+?b", "aaaab", 0, true, false),
				("(ab)\\1{2}c", "abababc", 0, true, false),
				("(ab)\\1{0,3}abc", "abababc", 0, true, false),
				("(ab)\\1{0,3}?abc", "abababc", 0, true, false),
				("(ab)\\1{1,}abc", "abababc", 0, true, false),
				("(a|ab)\\1*c", "abababc", 0, true, false),
				("(a|ab)\\1*?c", "abababc", 0, true, false),
				("((a|ab)\\2)+c", "abababc", 0, true, false),
				("(a*)\\1*b", "aaaab", 0, true, false),
				("(a*)\\1*?b", "aaaab", 0, true, false),
				("(a*?)\\1+b", "aaaab", 0, true, false),
				("()\\1*", "", 0, true, false),
				("()\\1*?", "", 0, true, false),
				("()\\1+", "", 0, true, false),
				("()\\1{1,}", "", 0, true, false),
				("()\\1{2}", "", 0, true, false),
				("()\\1{0,3}", "", 0, true, false),
				("()\\1{0,3}?", "", 0, true, false),
				("(()\\2)*a", "a", 0, true, false),
				("(?i)(a)\\1", "aA", 0, true, false),
				("(a)(?i:\\1)", "aA", 0, true, false),
				("(?i:(a))\\1", "Aa", 0, true, false),
				("(?i:(a))\\1", "AA", 0, true, false),
				("(?i)(é)\\1", "éÉ", 0, true, false),
				("(?i)(𐐀)\\1", "𐐨𐐀", 0, true, false),
				("(?i)(k)\\1", "kK", 0, true, false),
				("(k)(?ir:\\1)", "kK", 0, true, false),
				("(k)(?ir:\\1)", "kK", 0, true, false),
				("(?i)(Σ)\\1+", "Σςσ", 0, true, false),
				("(😀|x)\\1+", "😀😀😀", 0, true, false),
				("(😀|x)\\1+?x", "😀😀😀x", 0, true, false),
				("(\\R)\\1+", "\r\n\r\n\r\n", 0, true, false),
				("(\\R)\\1*\\r\\nx", "\r\n\r\n\r\nx", 0, true, false),
				("(\\w+)\\s+\\1", "same same", 0, true, false),
				("(?m)^(a+)\\1$", "aa\naaaa", 0, true, false),
				("(*CR)(?m)^(a+)\\1$", "aa\raaaa", 0, true, false),
				("\\A(a|ab)\\1\\z", "abab", 0, true, false),
				("\\G(a)\\1", "zaa", 0, true, false),
				("(a)\\1|b", "abb", 0, true, false),
				("((a)\\2)*c|b", "zzb", 0, true, false),
				("((a)|(b))*\\2", "aba", 0, true, false),
				("\\G(a)\\1", "zaa", 1, true, false),
				("(😀)\\1", "x😀😀", 1, true, false),
				("(?i)(é)\\1", "éÉ", 0, false, false),
				("(?i)(é)\\1", "éÉ", 0, false, true),
				("(?i)(k)\\1", "kK", 0, false, true),
				("(k)(?ir:\\1)", "kK", 0, false, true),
				("(\ud800)\\1", "\ud800\ud800", 0, false, false),
				("(\udc00)\\1", "x\udc00\udc00", 1, false, false),
				("(.)\\1", "😀😀", 0, false, false),
				("(..)\\1", "😀😀", 0, false, false),
				("(\\x{0})\\1+", "\u0000\u0000", 0, false, false)
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
	public void ReferencesAgreeWithEveryNativePolicy( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"reference {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			Assert.Equal( native.Kind == "M", result.Match is not null );
			Assert.Equal( native.Captures, Spans( result.Match ) );
		}
	}

	[Theory]
	[InlineData( @"(a|ab)\1", "abab" )]
	[InlineData( @"(a|b\1)+", "aba" )]
	[InlineData( @"(a(b)?)+\2", "abab" )]
	[InlineData( @"(ab)\1{0,3}?abc", "abababc" )]
	[InlineData( @"()\1{2,}", "" )]
	[InlineData( @"(?J)(?|(a)(?<x>b)|(?<x>c))\k<x>", "abb" )]
	[InlineData( @"(?<𐐀>😀)\k<𐐀>", "😀😀" )]
	[InlineData( @"(?i)(Σ)\1+", "Σςσ" )]
	[InlineData( @"(\R)\1*\r\nx", "\r\n\r\n\r\nx" )]
	public void ReferencesKeepAllCrossWidthSpansAndValues( string source, string subject ) {
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
	[InlineData( @"(ab)\1", "abab", false, 9, true )]
	[InlineData( @"(ab)\1", "aba", false, 8, false )]
	[InlineData( @"(ab)\1", "abab", true, 13, true )]
	[InlineData( @"(😀)\1", "😀😀", true, 9, true )]
	[InlineData( @"()\1", "", false, 5, true )]
	[InlineData( @"()\1*", "", false, 9, true )]
	[InlineData( @"(?J)(?<x>a)(?<x>b)\k<x>", "aba", false, 11, true )]
	public void LookupAndComparisonConsumeExactWork( string source, string subject, bool utf, int required, bool matched ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add, Policy = new() { Anchored = true }
			} ).Pattern!;
			var result = pattern.Match( subject.AsMemory() );
			if ( budget < required ) {
				Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); Assert.Null( result.Match );
			} else {
				Assert.True( result.IsSuccess ); Assert.Equal( matched, result.Match is not null );
				Assert.Equal( PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, 0, 1 ).Captures, Spans( result.Match ) );
			}
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void LongReferenceComparisonCancelsAndExhaustsWithoutPartialResults() {
		const string source = @"(a{64})(?i:\1)";
		var subject = new string( 'a', 128 );
		using var cancellation = new CancellationTokenSource();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			WorkObserver = work => { if ( work == 100 ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => pattern.Match( subject.AsMemory(), cancellationToken: cancellation.Token ) );
		Assert.Equal( "0:128,0:64", Spans( pattern.Match( subject.AsMemory() ).Match ) );
		var bounded = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { MatchWorkLimit = 100 } ).Pattern!;
		var failed = bounded.Match( subject.AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, failed.Diagnostic?.Code ); Assert.Null( failed.Match );
		Assert.True( bounded.Match( ReadOnlyMemory<char>.Empty ).IsSuccess );
	}

	[Fact]
	public void ReferencesObserveRollbackAndKeepReturnedCapturesStable() {
		var source = @"(?<x>a|ab)\k<x>c|(?<y>b)".ToCharArray();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source ).Pattern!;
		Array.Fill( source, 'z' );
		var first = pattern.Match( "!zzababc!".AsMemory( 1, 7 ) ).Match!;
		Assert.Equal( "2:5,2:2,-", Spans( first ) );
		foreach ( var subject in new[] { "b", "aba", "x", "aabc", "ababc" } ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( @"(?<x>a|ab)\k<x>c|(?<y>b)" ), Words( subject ), false, 0 );
			var result = pattern.Match( subject.AsMemory() );
			Assert.True( result.IsSuccess ); Assert.Equal( native.Captures, Spans( result.Match ) );
		}
		Assert.Equal( new PcreWordCapture( 2, 2 ), first.GetCapture( "x" ) );
		Assert.Null( first.GetCapture( "y" ) ); Assert.Equal( "2:5,2:2,-", Spans( first ) );
	}

	[Fact]
	public void RepeatedReferencesRetainFrameAndAdmissionBounds() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( @"\A(a)\1*\z".AsMemory() ).Pattern!;
		Assert.Equal( "0:1024,0:1", Spans( pattern.Match( new string( 'a', 1024 ).AsMemory() ).Match ) );
		var failed = pattern.Match( new string( 'a', 1025 ).AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded, failed.Diagnostic?.Code ); Assert.Null( failed.Match );
		Assert.Equal( "0:2,0:1", Spans( pattern.Match( "aa".AsMemory() ).Match ) );
		var lazy = PcreWordSequenceCompiler.CompileSequence( @"\A(a)\1*?\z".AsMemory() ).Pattern!;
		Assert.Equal( "0:2048,0:1", Spans( lazy.Match( new string( 'a', 2048 ).AsMemory() ).Match ) );
		foreach ( var source in new[] { @"(a)\1{0,64}", @"(a)\1{64,}", @"(a)\g<1>", @"(?<x>a)\g'x'",
			String.Concat( "(a)", String.Concat( Enumerable.Repeat( @"\1*", 64 ) ) ) } ) {
			Assert.True( PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).IsSuccess );
		}
		foreach ( var source in new[] { @"(a)\1{0,65}", @"(a)\1{65,}", String.Concat( "(a)", String.Concat( Enumerable.Repeat( @"\1*", 65 ) ) ),
			@"(a)\1{65,}+", @"(?*(a)){65}\1" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	[Theory]
	[InlineData( @"\1" )]
	[InlineData( @"(a)\2" )]
	[InlineData( @"(a)\g{0}" )]
	[InlineData( @"(a)\g{-2}" )]
	[InlineData( @"(a)\k<missing>" )]
	[InlineData( @"😀(a)\2" )]
	public void InvalidReferencesKeepOriginalEscapeOffsets( string source ) {
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Array.Empty<ushort>(), true, 0 );
		Assert.Equal( "E", native.Kind );
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidSyntax, source.IndexOf( '\\' ) ), compiled.Diagnostic );
	}

	[Fact]
	public void StrictReferencesKeepValidationAndStartOffsetDiagnostics() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( @"(😀)\1".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidStartOffset, 1 ), pattern.Match( "😀😀".AsMemory(), 1 ).Diagnostic );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 2 ), pattern.Match( "😀\uD800".AsMemory() ).Diagnostic );
		Assert.Equal( "1:4,1:2", Spans( pattern.Match( "\uD800😀😀".AsMemory(), 1 ).Match ) );
		using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => pattern.Match( "😀😀".AsMemory(), cancellationToken: cancellation.Token ) );
	}

	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( c => c is { } span ? $"{span.CodeUnitIndex}:{span.CodeUnitLength}" : "-" ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
