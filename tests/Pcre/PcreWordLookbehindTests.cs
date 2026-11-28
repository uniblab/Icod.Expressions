using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordLookbehindTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new (string Source, string Subject, int Start, bool Utf, bool Ucp)[] {
				("(?<=a)b", "ab", 0, true, false),
				("(?<=a)b", "zb", 0, true, false),
				("(?<!a)b", "zb", 0, true, false),
				("(?<!a)b", "ab", 0, true, false),
				("(?<=ab)c", "abc", 0, true, false),
				("(?<=ab)c", "bc", 0, true, false),
				("(?<=)a", "a", 0, true, false),
				("(?<!)a|a", "a", 0, true, false),
				("(?<=)", "", 0, true, false),
				("(?<!)", "", 0, true, false),
				("(?<=a)", "a", 0, true, false),
				("(?<!a)", "a", 0, true, false),
				("(?<=(a))b", "ab", 0, true, false),
				("(?<=(ab))c", "abc", 0, true, false),
				("(?<=(a))\\1", "aa", 0, true, false),
				("(?<=(a))", "a", 0, true, false),
				("(?<=(a|ab))c", "abc", 0, true, false),
				("(?<=(ab|a))c", "abc", 0, true, false),
				("(?<=a|ab)c", "abc", 0, true, false),
				("(?<=ab|a)c", "ac", 0, true, false),
				("(?<=(a{1,3}))b", "aaab", 0, true, false),
				("(?<=(a{1,3}?))b", "aaab", 0, true, false),
				("(?<=(a{0,3}))b", "b", 0, true, false),
				("(?<=(a{0,3}))b", "aab", 0, true, false),
				("(?<=(a{0,3}?))b", "aab", 0, true, false),
				("(?<!a{1,3})b", "aaab", 0, true, false),
				("(?<!a{1,3})b", "b", 0, true, false),
				("(?<=(ba{1,3}))c", "baaac", 0, true, false),
				("(?<=(ba{1,3}))c", "baac", 0, true, false),
				("(?<=(ba{1,3}))c", "bac", 0, true, false),
				("(?<=(ba{1,3}))c", "aac", 0, true, false),
				("(?<=((a)|ba))c", "bac", 0, true, false),
				("(?<=(a(b)?))c", "abc", 0, true, false),
				("(?<=(a(b)?))c", "ac", 0, true, false),
				("(?<=(a{1,3}))b\\1", "aaabaa", 0, true, false),
				("(?<=(a{1,3}))b\\1", "aaabaaa", 0, true, false),
				("(?<=(a|aa))b\\1", "aabaa", 0, true, false),
				("(?<=(aa|a))b\\1", "aabaa", 0, true, false),
				("(?<=(a))bX|b", "ab", 0, true, false),
				("(?<!(a)c)b", "ab", 0, true, false),
				("(?<!(a)b)c|c", "abc", 0, true, false),
				("(?<!(a|b)c).", "bcd", 0, true, false),
				("(?<!(a{1,3}))b|b", "aaab", 0, true, false),
				("(?<!(a?))b|b", "b", 0, true, false),
				("a(?<=a)b", "ab", 0, true, false),
				("(a)(?<=\\1)b", "ab", 0, true, false),
				("(ab)(?<=\\1)c", "abc", 0, true, false),
				("(a{1,3})(?<=\\1)b", "aaab", 0, true, false),
				("(?<x>ab)(?<=\\k<x>)c", "abc", 0, true, false),
				("(?<=(a)\\1)b", "aab", 0, true, false),
				("(?<=(?<x>a)\\k<x>)b", "aab", 0, true, false),
				("(?<=(?<x>ab))c\\k<x>", "abcab", 0, true, false),
				("(?<=(a)?)b", "b", 0, true, false),
				("(?<=(a)?)b", "ab", 0, true, false),
				("(?<=(a){2})b", "aab", 0, true, false),
				("(?<=(a){1,3})b", "aaab", 0, true, false),
				("(?<=(a){1,3}?)b", "aaab", 0, true, false),
				("(?<=(?:ab|a){1,2})c", "abac", 0, true, false),
				("(?<=(?:ab|a){1,2}?)c", "abac", 0, true, false),
				("(?<=a(?=b))b", "ab", 0, true, false),
				("(?<=a(?!c))b", "ab", 0, true, false),
				("(?<=(?=a)a)b", "ab", 0, true, false),
				("(?<=a(?=(b)))\\1", "ab", 0, true, false),
				("(?=(?<=(a))b)b", "ab", 0, true, false),
				("(?<=(?!(a)c)a)b", "ab", 0, true, false),
				("(?<!(?=(a))a)b", "ab", 0, true, false),
				("(?<=(?<=a)b)c", "abc", 0, true, false),
				("(?<=(?<!x)b)c", "abc", 0, true, false),
				("(?<=(?<!a)b)c", "abc", 0, true, false),
				("(?<!(?<=a)b)c", "abc", 0, true, false),
				("(?<=(?<=(a))b)c", "abc", 0, true, false),
				("(?<=(?<=a{1,2})b)c", "aabc", 0, true, false),
				("(?<=(?<=a)b{1,2})c", "abbc", 0, true, false),
				("(?<=(?<=a)b)c", "abc", 2, true, false),
				("(?<=(?<=ab)c)d", "abcd", 3, true, false),
				("(?<=a)b?(c)", "abc", 0, true, false),
				("a?(?<=a)b?", "ab", 0, true, false),
				("a??(?<=a)b??", "ab", 0, true, false),
				("a{0,2}(?<=(a?))a{0,2}\\1", "aaaa", 0, true, false),
				("a{0,2}?(?<!(a)b)a{0,2}?b", "aaab", 0, true, false),
				("(?:(?<=a)b|a)+", "abab", 0, true, false),
				("(?:(?<!b)a|b)+", "aabb", 0, true, false),
				("((?<=(a?))a){1,3}", "aaa", 0, true, false),
				("((?<=(a?))){2}b", "ab", 0, true, false),
				("((?<=(a?)))*b", "ab", 0, true, false),
				("(?<=é)Σ", "éΣ", 0, true, false),
				("(?<=😀)a", "😀a", 0, true, false),
				("(?<=(😀))a\\1", "😀a😀", 0, true, false),
				("(?<=😀|é)a", "éa", 0, true, false),
				("(?<=(😀{1,3}))a", "😀😀a", 0, true, false),
				("(?<=(😀{1,3}?))a", "😀😀a", 0, true, false),
				("(?<=.)a", "😀a", 0, true, false),
				("(?<=..)a", "😀a", 0, true, false),
				("(?i)(?<=é)a", "Éa", 0, true, false),
				("(?i)(?<=𐐀)a", "𐐨a", 0, true, false),
				("(?i)(?<=k)a", "Ka", 0, true, false),
				("(?ir)(?<=k)a", "Ka", 0, true, false),
				("(?<=\\p{Greek})a", "Σa", 0, true, false),
				("(?<=\\w)a", "éa", 0, true, true),
				("(?<=\\d)a", "١a", 0, true, true),
				("(?<=\\b)a", " a", 0, true, false),
				("(?<=\\b)a", "ba", 0, true, false),
				("(?<=\\A)a", "a", 0, true, false),
				("(?<=\\G)a", "za", 1, true, false),
				("(?<=^a)b", "ab", 0, true, false),
				("(?m)(?<=^a)b", "x\nab", 0, true, false),
				("(*CR)(?m)(?<=^a)b", "x\rab", 0, true, false),
				("(*NUL)(?m)(?<=^a)b", "x\u0000ab", 0, true, false),
				("(?<=a$)", "a\n", 0, true, false),
				("(?<=\\R)a", "\r\na", 0, true, false),
				("(?<=(\\R))a", "\r\na", 0, true, false),
				("(?<=(\\R))a", "\na", 0, true, false),
				("(?<!\\R)a", "\r\na", 0, true, false),
				("(*BSR_ANYCRLF)(?<=\\R)a", "\u0085a", 0, true, false),
				("(?<=(\\R){1,2})a", "\r\n\r\na", 0, true, false),
				("(*plb:(a))b", "ab", 0, true, false),
				("(*positive_lookbehind:(a))b", "ab", 0, true, false),
				("(*nlb:(a))b", "zb", 0, true, false),
				("(*negative_lookbehind:(a))b", "zb", 0, true, false),
				("(?<=ab)c", "zabc", 3, true, false),
				("(?<!ab)c", "zac", 2, true, false),
				("(?<=😀)a", "x😀a", 3, true, false),
				("(?<=😀)a", "😀a", 0, false, false),
				("(?<=.)a", "😀a", 0, false, false),
				("(?<=..)a", "😀a", 0, false, false),
				("(?i)(?<=é)a", "Éa", 0, false, false),
				("(?i)(?<=é)a", "Éa", 0, false, true),
				("(?<=\\x{0})a", "\u0000a", 0, false, false),
				("(?<=a{64})b", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaab", 64, true, false),
				("(?<=(?:a{64}){4})b", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaab", 256, true, false),
				("(?<=(?:a{64}){3}a{61}a{0,2})b", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaab", 255, true, false),
				("(?<=\ud800)a", "\ud800a", 0, false, false),
				("(?<!(.\udc00))a", "𐀀a", 2, false, false),
				("(?<=\\ba)b", "aab", 2, true, false),
				("(?<=\\Ba)b", "aab", 2, true, false),
				("(?<!(?<=a)b)c", "abc", 2, true, false),
				("(?<=(?<!a)b)c", "abc", 2, true, false),
				("(?<=\\ba)b", "aab", 2, false, false),
				("(?<=\\Ba)b", "aab", 2, false, false),
				("(?<=(?<=a)b)c", "abc", 2, false, false),
				("(?<!(?<=a)b)c", "abc", 2, false, false),
				("(?<=(?<=(a?))b)c", "abc", 2, true, false),
				("(?<=(?<=(a{1,2}))b)c", "aabc", 3, true, false)
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
	public void LookbehindsAgreeWithEveryNativePolicy( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"lookbehind {id}: {compiled.Diagnostic}" );
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
	[InlineData( @"(?<=(?<x>😀é))a\k<x>", "😀éa😀é" )]
	[InlineData( @"(?<=(😀{1,3}?))a", "😀😀a" )]
	[InlineData( @"(?<!(😀)x)(a)", "😀a" )]
	[InlineData( @"(?<=(?<=(𐐀))😀)a", "𐐀😀a" )]
	[InlineData( @"(?i)(?<=(Σ{1,3}))a", "Σςσa" )]
	[InlineData( @"(?<=(a|aa))b\1", "aabaa" )]
	[InlineData( @"((?<=(a?))a){1,3}", "aaa" )]
	[InlineData( @"(?<=(\R){1,2})a", "\r\n\r\na" )]
	public void LookbehindCapturesKeepCrossWidthSpansAndValues( string source, string subject ) {
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
	[InlineData( "(?<=a)b", "ab", 1, 10, true )]
	[InlineData( "(?<!a)b", "zb", 1, 9, true )]
	[InlineData( "(?<=ab)c", "c", 0, 6, false )]
	[InlineData( "(?<!ab)c", "c", 0, 8, true )]
	[InlineData( "(?<=)", "", 0, 7, true )]
	[InlineData( "(?<!)", "", 0, 6, false )]
	[InlineData( "(?<=(a))b", "ab", 1, 12, true )]
	[InlineData( "(?<!(a))b", "ab", 1, 11, false )]
	[InlineData( "(?<!(a)b)c", "axc", 2, 14, true )]
	[InlineData( "(?<=a|ba)c", "bac", 2, 12, true )]
	[InlineData( "(?<=a{1,2})b", "ab", 1, 15, true )]
	[InlineData( "(?<=a{1,2})b", "aab", 2, 15, true )]
	[InlineData( "(?<=(ba{1,2}))c", "xbac", 3, 26, true )]
	public void RewindingAndResolutionConsumeExactWork( string source, string subject, int start, int required, bool matched ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add, Policy = new() { Anchored = true }
			} ).Pattern!;
			var result = pattern.Match( subject.AsMemory(), start );
			if ( budget < required ) {
				Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); Assert.Null( result.Match );
			} else {
				Assert.True( result.IsSuccess ); Assert.Equal( matched, result.Match is not null );
				Assert.Equal( PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), false, (uint)start, 1 ).Captures, Spans( result.Match ) );
			}
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void RewindingAndExtendedValidationObserveCancellationAndQuotas() {
		var subject = String.Concat( new string( 'a', 64 ), "b" );
		foreach ( var utf in new[] { false, true } ) {
			using var cancellation = new CancellationTokenSource();
			var pattern = PcreWordSequenceCompiler.CompileSequence( "(?<=a{64})b".AsMemory(), new() {
				Utf16 = utf, WorkObserver = work => { if ( work == 40 ) { cancellation.Cancel(); } }
			} ).Pattern!;
			Assert.Throws<OperationCanceledException>( () => pattern.Match( subject.AsMemory(), 64, cancellation.Token ) );
			Assert.Equal( "64:1", Spans( pattern.Match( subject.AsMemory(), 64 ).Match ) );
			var bounded = PcreWordSequenceCompiler.CompileSequence( "(?<=a{64})b".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = 40
			} ).Pattern!;
			var failed = bounded.Match( subject.AsMemory(), 64 );
			Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, failed.Diagnostic?.Code ); Assert.Null( failed.Match );
		}
	}

	[Fact]
	public void AShorterRewindRetryCanCancelAndBeReused() {
		using var cancellation = new CancellationTokenSource();
		var pattern = PcreWordSequenceCompiler.CompileSequence( "(?<=(ba{1,2}))c".AsMemory(), new() {
			WorkObserver = work => { if ( work == 13 ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => pattern.Match( "xbac".AsMemory(), 3, cancellation.Token ) );
		Assert.Equal( "3:1,1:2", Spans( pattern.Match( "xbac".AsMemory(), 3 ).Match ) );
	}

	[Fact]
	public void CapturesBeforeGroupZeroRemainOwnedAcrossCallsAndRetries() {
		var source = @"(?<=(?<x>a{1,3}))b\k<x>|(?<!(?<y>a)c)b".ToCharArray();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source ).Pattern!;
		Array.Fill( source, 'z' );
		var first = pattern.Match( "!aaabaaa!".AsMemory( 1, 7 ), 3 ).Match!;
		Assert.Equal( "3:4,0:3,-", Spans( first ) );
		foreach ( var subject in new[] { "b", "acb", "aabaa", "aaabaa", "aaabaaa", "" } ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( @"(?<=(?<x>a{1,3}))b\k<x>|(?<!(?<y>a)c)b" ), Words( subject ), false, 0 );
			var result = pattern.Match( subject.AsMemory() );
			Assert.True( result.IsSuccess ); Assert.Equal( native.Captures, Spans( result.Match ) );
		}
		Assert.Equal( new PcreWordCapture( 0, 3 ), first.GetCapture( "x" ) );
		Assert.Null( first.GetCapture( "y" ) ); Assert.Equal( "3:4,0:3,-", Spans( first ) );
	}

	[Fact]
	public void NestedBehindGuardsShareLimitsAndNegativeAssertionsPropagateFailures() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( @"\A(?<=(?=(?:a)*\z))a*".AsMemory() ).Pattern!;
		Assert.Equal( "0:1021", Spans( pattern.Match( new string( 'a', 1021 ).AsMemory() ).Match ) );
		var failed = pattern.Match( new string( 'a', 1022 ).AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded, failed.Diagnostic?.Code ); Assert.Null( failed.Match );
		Assert.Equal( "0:2", Spans( pattern.Match( "aa".AsMemory() ).Match ) );
		var negative = PcreWordSequenceCompiler.CompileSequence( @"\A(?<!(?=(?:a)*x))".AsMemory() ).Pattern!;
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded,
			negative.Match( new string( 'a', 1022 ).AsMemory() ).Diagnostic?.Code );
		var bounded = PcreWordSequenceCompiler.CompileSequence( @"(?<!(a|aa){1,8}x)b".AsMemory(), new() {
			MatchWorkLimit = 100, Policy = new() { Anchored = true }
		} ).Pattern!;
		var exhausted = bounded.Match( String.Concat( new string( 'a', 20 ), "b" ).AsMemory(), 20 );
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code ); Assert.Null( exhausted.Match );
		Assert.Equal( "0:1,-", Spans( bounded.Match( "b".AsMemory() ).Match ) );
	}

	[Fact]
	public void LookbehindWidthsAndBodiesRetainAdmissionBounds() {
		var nested = String.Concat( String.Concat( Enumerable.Repeat( "(?<=", 64 ) ), new string( ')', 64 ) );
		foreach ( var source in new[] { nested, "(?<=a{1,64})", "(?<=(?:a{64}){4})",
			"(?<=(?:a{64}){3}a{61}a{0,2})", "(?<=a|ab)", "(?<=a?b?c?d?e?f?)" } ) {
			Assert.True( PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).IsSuccess, source );
		}
		Assert.Equal( "0:0", Spans( PcreWordSequenceCompiler.CompileSequence( nested.AsMemory() ).Pattern!.Match( ReadOnlyMemory<char>.Empty ).Match ) );
		foreach ( var source in new[] { String.Concat( "(?<=", nested, ")" ), "(?<=a{0,64})",
			"(?<=(?:a{64}){3}a{62}a{0,2})", "(?<=(?:(?:a{64}){64}){16})",
			"(?<=a?b?c?d?e?f?)g?", "(?<=a*)", "(?<=a){65}", "(?<=a){64,}", "(?<=(?>(*sr:(?:a){65}a)a)){0}",
			"(?<*a){65}", "(?<!a(*asr:(?:a){65}a))", "(a)(?<=a)(?1)(*sr:(?:a){65}a)" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	public static IEnumerable<object[]> ValidationCases {
		get {
			yield return new object[] { "(?<=ab)c", "\uD800abc", 3, -1 };
			yield return new object[] { "(?<=ab)c", "\uD800bc", 2, 0 };
			yield return new object[] { "(?<!ab)c", "\uD800bc", 2, 0 };
			yield return new object[] { "(?<=😀a)b", "\uD800😀ab", 4, -1 };
			yield return new object[] { "(?<=😀a)b", "\uD800ab", 2, 0 };
			yield return new object[] { "(?<=😀a)b", "\uD800\uDC00ab", 3, -1 };
			yield return new object[] { "(?<=(?:a){0,3})b", "\uD800aab", 3, 0 };
			yield return new object[] { "(?<=abc)x|b", "\uD800aab", 3, 0 };
			yield return new object[] { "(?:(?<=abc)){0}b", "\uD800aab", 3, 0 };
			yield return new object[] { "(?<=(?<=a)b)c", "\uD800abc", 3, -1 };
		}
	}

	[Theory]
	[MemberData( nameof( ValidationCases ) )]
	public void NativeValidationWindowsKeepOriginalCodeUnitOffsets( string source, string subject, int start, int invalid ) {
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, (uint)start );
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.True( compiled.IsSuccess, $"{source}: {compiled.Diagnostic}" );
		var result = compiled.Pattern!.Match( subject.AsMemory(), start );
		if ( invalid >= 0 ) {
			Assert.Equal( "X", native.Kind );
			Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, invalid ), result.Diagnostic );
		} else {
			Assert.True( native.Kind is "M" or "N" );
			Assert.True( result.IsSuccess ); Assert.Equal( native.Captures, Spans( result.Match ) );
		}
	}
	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( c => c is { } span ? $"{span.CodeUnitIndex}:{span.CodeUnitLength}" : "-" ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
