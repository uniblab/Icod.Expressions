using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordNonAtomicAssertionTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new (string Source, string Subject, int Start, bool Utf, bool Ucp)[] {
				("(?*a)a", "za", 0, true, false),
				("(?*a)b", "ab", 0, true, false),
				("(?*)", "", 0, true, false),
				("(?*())\\1", "", 0, true, false),
				("(?*(a))", "a", 0, true, false),
				("(?*(ab))a", "ab", 0, true, false),
				("(?*(a|ab))\\1c", "abc", 0, true, false),
				("(?=(a|ab))\\1c", "abc", 0, true, false),
				("(?*(ab|a))\\1c", "ac", 0, true, false),
				("(?*(a+))\\1a", "aaa", 0, true, false),
				("(?*(a+?))\\1a", "aaa", 0, true, false),
				("(?*(a*))\\1b", "aaab", 0, true, false),
				("(?*(a*?))\\1b", "aaab", 0, true, false),
				("(?*(a)?b)\\1", "b", 0, true, false),
				("(?*(a|))\\1a", "a", 0, true, false),
				("(?*(a?))\\1", "a", 0, true, false),
				("(?*(a?))\\1", "", 0, true, false),
				("(?*(?*(a|ab)))\\1c", "abc", 0, true, false),
				("(?*(?=(a|ab)))\\1c", "abc", 0, true, false),
				("(?=(?*(a|ab)))\\1c", "abc", 0, true, false),
				("(?*(a|ab))(?*(a|ab))\\1c", "abc", 0, true, false),
				("(?*(a|ab))(?<*(a?))\\1c", "abc", 0, true, false),
				("(?!(?*(a|ab))\\1c)abc", "abc", 0, true, false),
				("(?!(?*(a|ab))\\1d)abc", "abc", 0, true, false),
				("(?*(?!(a)c)(a|ab))\\2c", "abc", 0, true, false),
				("(?:(?*(a|ab))\\1d|a(b)c)\\2", "abcb", 0, true, false),
				("(?J)(?*(?<x>a)|(?<x>ab))\\k<x>c", "abc", 0, true, false),
				("(?n)(?*(?<x>a|ab))\\k<x>c", "abc", 0, true, false),
				("(?x)(?* (?<x>a|ab) )\\k<x> c", "abc", 0, true, false),
				("(?*(?<x>k|K)).(?i:\\k<x>)", "kK", 0, true, false),
				("(?*(?<x>k|K)).(?ir:\\k<x>)", "kK", 0, true, false),
				("(?*(é|é😀))\\1x", "é😀x", 0, true, false),
				("(?*(a))b|a", "a", 0, true, false),
				("(?*(a|ab))\\1d|a(b)c", "abc", 0, true, false),
				("a(?*(b|bc))\\1d", "abcd", 0, true, false),
				("a?(?*(a|ab))a?\\1c", "aabc", 0, true, false),
				("a??(?*(a|ab))a??\\1c", "aabc", 0, true, false),
				("(?*(a|ab))a{0,2}\\1c", "aabc", 0, true, false),
				("(?*(a|ab))a{0,2}?\\1c", "aabc", 0, true, false),
				("(?:(?*(a|ab))\\1)+c", "aabc", 0, true, false),
				("(?:(?*(a|ab))\\1)+?c", "aabc", 0, true, false),
				("((?*(a?)))*a", "a", 0, true, false),
				("((?*(a?))){2}a", "a", 0, true, false),
				("((?*(a?))){0,3}?a", "a", 0, true, false),
				("(?*(a))a(?*(b))b\\1\\2", "abab", 0, true, false),
				("(?|(?*(a))a|(?*(b))b)\\1", "bb", 0, true, false),
				("(*napla:(a|ab))\\1c", "abc", 0, true, false),
				("(*non_atomic_positive_lookahead:(a|ab))\\1c", "abc", 0, true, false),
				("(?<*a)b", "ab", 0, true, false),
				("(?<*a)b", "b", 0, true, false),
				("(?<*)a", "a", 0, true, false),
				("(?<*)", "", 0, true, false),
				("(?<*(a))b\\1", "aba", 0, true, false),
				("(?<*(a{1,3}))b\\1", "aaabaa", 0, true, false),
				("(?<=(a{1,3}))b\\1", "aaabaa", 0, true, false),
				("(?<*(a{1,3}?))b\\1", "aaabaa", 0, true, false),
				("(?<*(a|aa))b\\1", "aabaa", 0, true, false),
				("(?<*(b)|(ab))(?:\\1|\\2)c", "ababc", 0, true, false),
				("(?<=(b)|(ab))(?:\\1|\\2)c", "ababc", 0, true, false),
				("^aa(?<*(a{1,2}))\\1b$", "aaab", 0, true, false),
				("^aa(?<*(a{1,2}))(?=a)\\1b$", "aaab", 0, true, false),
				("^aa(?<*(?*(a{1,2}))a{1,2})\\1b$", "aaab", 0, true, false),
				("^aa(?<*(?=(a{1,2}))a{1,2})\\1b$", "aaab", 0, true, false),
				("^aa(?<*(?*(a{1,3}))a{1,2})\\1b$", "aaaab", 0, true, false),
				("^aa(?<*(?*(a{1,3}))aa)\\1b$", "aaaaab", 0, true, false),
				("^éé(?<*(é{1,2}))\\1x$", "éééx", 0, true, false),
				("(?<x>a)(?<*\\k<x>)b", "ab", 0, true, false),
				("(a{1,3})(?<*\\1)b", "aaab", 0, true, false),
				("(?<*(a)\\1)b", "aab", 0, true, false),
				("(?<*(?<x>ab))c\\k<x>", "abcab", 0, true, false),
				("(?<*(a{0,2}))b", "b", 0, true, false),
				("(?<*(a{0,2}))b", "aab", 0, true, false),
				("(?<*(a)?b)c\\1", "abc", 0, true, false),
				("(?<*(a)?b)c\\1", "bc", 0, true, false),
				("(?<*(?*a)a)b", "ab", 0, true, false),
				("(?<*(?<=a)b)c", "abc", 0, true, false),
				("(?<*(?<=a)b)c", "abc", 2, true, false),
				("(?<*(?<!a)b)c", "abc", 2, true, false),
				("(?<*(?<*a)b)c", "abc", 2, true, false),
				("(?*(?<*(a{1,2}))b\\1)b\\1", "aabaa", 0, true, false),
				("(?!(?<*(a{1,2}))b\\1)b", "aabaa", 0, true, false),
				("(?<*(a|aa))b(?*\\1)\\1", "aabaa", 0, true, false),
				("a{0,2}(?<*(a?))a{0,2}\\1", "aaaa", 0, true, false),
				("(?:(?<*(a?))a){1,3}", "aaa", 0, true, false),
				("(?:(?<*(a?))){2}b", "ab", 0, true, false),
				("(?<*😀)a", "😀a", 0, true, false),
				("(?<*(😀{1,3}))a\\1", "😀😀a😀", 0, true, false),
				("(?i)(?<*(é{1,2}))a\\1", "éÉaÉ", 0, true, false),
				("(*naplb:(a|aa))b\\1", "aabaa", 0, true, false),
				("(*non_atomic_positive_lookbehind:(a|aa))b\\1", "aabaa", 0, true, false),
				("(?<*a{1,2}(?=(b)))\\1", "aab", 2, true, false),
				("(?<=a{1,2}(?=(b)))\\1", "aab", 2, true, false),
				("(?<*a{1,2}(?!b))b", "aab", 2, true, false),
				("(?<*a{1,2}\\b)b", "aab", 2, true, false),
				("(?<*a{1,2}\\B)b", "aab", 2, true, false),
				("(?<*a{1,2}\\z)b", "aab", 2, true, false),
				("(?<*a{1,2}$)b", "aab", 2, true, false),
				("(?<*a{1,2}\\Z)b", "aab", 2, true, false),
				("(?<*a{1,2}(?*.))b", "aab", 2, true, false),
				("(?<*a{1,2}(?*\\w))b", "aab", 2, true, true),
				("(?<*a{1,2}(?*\\R))\\R", "aa\r\n", 2, true, false),
				("(?<*a{1,2}(?*\\R))\\R", "aa\n", 2, true, false),
				("(?<*a{1,2}(?*b+))b", "aab", 2, true, false),
				("(?<*a{1,2}(?*b*?))b", "aab", 2, true, false),
				("(?=..(?<x>a))a(?<*(?:\\k<x>|aa))b", "aba", 0, true, false),
				("(?<*(a{1,2}))(?*b)b\\1", "aabaa", 2, true, false),
				("(?<*a{1,2}(?=(b)))\\1", "aab", 2, false, false),
				("(?<*a{1,2}\\b)b", "aab", 2, false, false),
				("(?<*a{1,2}\\z)b", "aab", 2, false, false),
				("(?<*(?<=a)b)c", "abc", 2, false, false),
				("(?*(😀|😀a))\\1b", "😀ab", 0, true, false),
				("(?i)(?*(𐐀|𐐀a))\\1b", "𐐨ab", 0, true, false),
				("(?i)(?*(Σ{1,3}))\\1σ", "Σςσ", 0, true, false),
				("(?*(\\p{Greek}{1,3}))\\1σ", "Σςσ", 0, true, false),
				("(?*(\\w{1,3}))\\1x", "éΣx", 0, true, true),
				("(?*(\\R|\\R\\R))\\1x", "\r\n\r\nx", 0, true, false),
				("(*BSR_ANYCRLF)(?*(\\R|\\R\\R))\\1x", "\r\n\r\nx", 0, true, false),
				("(?m)(?*(^a+$))a+", "x\naaa\nx", 0, true, false),
				("(*CR)(?m)(?*(^a+$))a+", "x\raaa\rx", 0, true, false),
				("(*NUL)(?m)(?*(^a+$))a+", "x\u0000aaa\u0000x", 0, true, false),
				("(?*\\G(a|ab))\\1c", "zabc", 1, true, false),
				("(?*\\b(a|ab))\\1c", " abc", 1, true, false),
				("(?<*\\ba)b", "aab", 2, true, false),
				("(?<*\\Ba)b", "aab", 2, true, false),
				("(?*(a))a", "za", 1, true, false),
				("(?<*(😀))a", "x😀a", 3, true, false),
				("(?*(😀|😀a))\\1b", "😀ab", 0, false, false),
				("(?i)(?*(é|éa))\\1b", "Éab", 0, false, true),
				("(?i)(?*(é|éa))\\1b", "Éab", 0, false, false),
				("(?*(\\x{0}|\\x{0}a))\\1b", "\u0000ab", 0, false, false),
				("(?*(\ud800|\ud800a))\\1b", "\ud800ab", 0, false, false),
				("(?<*(\udc00{1,2}))a\\1", "\udc00\udc00a\udc00", 0, false, false)
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
	public void RetainedChoicesAgreeWithEveryNativePolicy( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"non-atomic {id}: {compiled.Diagnostic}" );
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
	[InlineData( @"(?*(a|ab))\1c", "abc" )]
	[InlineData( @"(?*(é|é😀))\1x", "é😀x" )]
	[InlineData( @"(?i)(?*(𐐀|𐐀a))\1b", "𐐨ab" )]
	[InlineData( @"(?<*(😀{1,3}))a\1", "😀😀a😀" )]
	[InlineData( @"^éé(?<*(é{1,2}))\1x$", "éééx" )]
	[InlineData( @"^aa(?<*(?*(a{1,3}))a{1,2})\1b$", "aaaab" )]
	[InlineData( @"(?=..(?<x>a))a(?<*(?:\k<x>|aa))b", "aba" )]
	[InlineData( @"(?*(\R|\R\R))\1x", "\r\n\r\nx" )]
	public void ResumedAssertionsKeepCrossWidthCaptureSpansAndValues( string source, string subject ) {
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
	[InlineData( "(?*)", "", 0, 6 )]
	[InlineData( "(?*a)a", "a", 0, 8 )]
	[InlineData( "(?*(a))a", "a", 0, 10 )]
	[InlineData( "(?*(a|ab))a", "abc", 0, 11 )]
	[InlineData( @"(?*(a|ab))\1c", "abc", 0, 24 )]
	[InlineData( @"(?*(a+))\1a", "aaa", 0, 27 )]
	[InlineData( "(?*(a))b|a", "a", 0, 15 )]
	[InlineData( "(?<*a)b", "ab", 1, 10 )]
	[InlineData( "(?<*(a))b", "ab", 1, 12 )]
	public void ReturnsAndBodyRetriesConsumeExactWork( string source, string subject, int start, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add, Policy = new() { Anchored = true }
			} ).Pattern!;
			var result = pattern.Match( subject.AsMemory(), start );
			if ( budget < required ) {
				Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); Assert.Null( result.Match );
			} else {
				Assert.True( result.IsSuccess ); Assert.NotNull( result.Match );
				Assert.Equal( PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), false, (uint)start, 1 ).Captures, Spans( result.Match ) );
			}
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Theory]
	[InlineData( @"(?*(a|ab))\1c", "abc", 13, "0:3,0:2" )]
	[InlineData( @"(?*(a+))\1a", "aaa", 19, "0:3,0:2" )]
	public void CancellationDuringReentryPublishesNoPartialCaptures( string source, string subject, int cancelAt, string expected ) {
		using var cancellation = new CancellationTokenSource();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			WorkObserver = work => { if ( work == cancelAt ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => pattern.Match( subject.AsMemory(), cancellationToken: cancellation.Token ) );
		Assert.Equal( expected, Spans( pattern.Match( subject.AsMemory() ).Match ) );
	}

	[Fact]
	public void RetainedGuardsShareTheFrameCeiling() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( @"\A(?:(?*a)a)*\z".AsMemory() ).Pattern!;
		Assert.Equal( "0:511", Spans( pattern.Match( new string( 'a', 511 ).AsMemory() ).Match ) );
		var failed = pattern.Match( new string( 'a', 512 ).AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded, failed.Diagnostic?.Code ); Assert.Null( failed.Match );
		Assert.Equal( "0:2", Spans( pattern.Match( "aa".AsMemory() ).Match ) );
		var atomic = PcreWordSequenceCompiler.CompileSequence( @"\A(?:(?=a)a)*\z".AsMemory() ).Pattern!;
		Assert.Equal( "0:512", Spans( atomic.Match( new string( 'a', 512 ).AsMemory() ).Match ) );
	}

	[Fact]
	public void SourceOwnershipAndReturnedCapturesSurviveLaterRetries() {
		var source = @"(?*(?<x>a|ab))\k<x>c|(?<y>b)".ToCharArray();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source ).Pattern!;
		Array.Fill( source, 'z' );
		var first = pattern.Match( "!zabc!".AsMemory( 1, 4 ) ).Match!;
		Assert.Equal( "1:3,1:2,-", Spans( first ) );
		foreach ( var subject in new[] { "b", "x", "abc", "ac", "", "ab" } ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( @"(?*(?<x>a|ab))\k<x>c|(?<y>b)" ), Words( subject ), false, 0 );
			var result = pattern.Match( subject.AsMemory() );
			Assert.True( result.IsSuccess ); Assert.Equal( native.Captures, Spans( result.Match ) );
		}
		Assert.Equal( new PcreWordCapture( 1, 2 ), first.GetCapture( "x" ) );
		Assert.Null( first.GetCapture( "y" ) ); Assert.Equal( "1:3,1:2,-", Spans( first ) );
	}

	[Fact]
	public void NonAtomicBodiesRetainAdmissionBounds() {
		foreach ( var opener in new[] { "(?*", "(?<*" } ) {
			var nested = String.Concat( String.Concat( Enumerable.Repeat( opener, 64 ) ), new string( ')', 64 ) );
			Assert.True( PcreWordSequenceCompiler.CompileSequence( nested.AsMemory() ).IsSuccess );
			Assert.Equal( "0:0", Spans( PcreWordSequenceCompiler.CompileSequence( nested.AsMemory() ).Pattern!.Match( ReadOnlyMemory<char>.Empty ).Match ) );
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( String.Concat( opener, nested, ")" ).AsMemory() ).Diagnostic?.Code );
		}
		foreach ( var source in new[] { "(?*a?b?c?d?e?f?)", "(?<*a?b?c?d?e?f?)",
			"(?<*(?:a{64}){3}a{61}a{0,2})" } ) {
			Assert.True( PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).IsSuccess );
		}
		foreach ( var source in new[] { "(?*a?b?c?d?e?f?)g?", "(?<*a?b?c?d?e?f?)g?",
			"(?<*a*)", "(?<*(?:a{64}){3}a{62}a{0,2})",
			"(?*a){65}", "(?*a){64,}", "(?*(?>(*ACCEPT)a)){0}", "(?<*a){65}", "(?*(?>(*ACCEPT)a))", "(?*a(*ACCEPT))" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	[Theory]
	[InlineData( "é(?*a", 1 )]
	[InlineData( "😀(?<*a", 2 )]
	[InlineData( "é(*napla:a", 1 )]
	[InlineData( @"é(?*\K)", 1 )]
	[InlineData( @"é(?<*\K)", 1 )]
	public void InvalidAssertionsKeepOriginalOpeningOffsets( string source, int offset ) {
		Assert.Equal( "E", PcreNative16Oracle.ExecuteSequence( Words( source ), Array.Empty<ushort>(), true, 0 ).Kind );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidSyntax, offset ),
			PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Diagnostic );
	}

	[Fact]
	public void ValidationWindowsRemainNativeForNonAtomicBodies() {
		foreach ( var source in new[] { @"(?*\b)a", @"(?*a(?<=abc))", @"(?<*ab)c" } ) {
			var subject = "\uD800bc";
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 1 );
			Assert.Equal( "X", native.Kind );
			var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Pattern!.Match( subject.AsMemory(), 1 );
			Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 0 ), result.Diagnostic );
		}
		var pattern = PcreWordSequenceCompiler.CompileSequence( "(?<*ab)c".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( "3:1", Spans( pattern.Match( "\uD800abc".AsMemory(), 3 ).Match ) );
		var scalar = PcreWordSequenceCompiler.CompileSequence( "(?*😀)😀".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidStartOffset, 1 ), scalar.Match( "😀".AsMemory(), 1 ).Diagnostic );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 2 ), scalar.Match( "😀\uD800".AsMemory() ).Diagnostic );
	}

	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( c => c is { } span ? $"{span.CodeUnitIndex}:{span.CodeUnitLength}" : "-" ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
