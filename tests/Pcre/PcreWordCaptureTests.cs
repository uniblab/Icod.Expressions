using System.Globalization;
using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordCaptureTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new (string Source, string Subject, int Start, bool Utf, bool Ucp)[] {
				("(a)", "za", 0, true, false),
				("(a)(b)", "ab", 0, true, false),
				("((a)b)", "zab", 0, true, false),
				("()", "", 0, true, false),
				("(a)?", "", 0, true, false),
				("(a){0}", "a", 0, true, false),
				("(a)|(b)", "b", 0, true, false),
				("(a)|(b)", "za", 0, true, false),
				("(a|ab)c?", "abc", 0, true, false),
				("(a|ab)c", "abc", 0, true, false),
				("((a)|ab)c", "abc", 0, true, false),
				("(a(b)|ac)d", "acd", 0, true, false),
				("((a)c|ab)d", "abd", 0, true, false),
				("(a)c|(b)", "b", 0, true, false),
				("(a)c|b", "ab", 0, true, false),
				("(a)?a", "a", 0, true, false),
				("((a)?b)c|b", "b", 0, true, false),
				("(a)?b", "ab", 0, true, false),
				("(a)??a", "aa", 0, true, false),
				("(a*)a", "aaa", 0, true, false),
				("(a*?)a", "aaa", 0, true, false),
				("(a+)(a*)", "aaaa", 0, true, false),
				("((a|ab)*)c", "aabc", 0, true, false),
				("((a)|(b))*", "ab", 0, true, false),
				("((a)|(b))*a", "aba", 0, true, false),
				("((a)|(b))*?b", "abb", 0, true, false),
				("(a(b)?)+", "aba", 0, true, false),
				("(a(b)?)+b", "abab", 0, true, false),
				("(a(b)?)+?b", "abab", 0, true, false),
				("(a(b)?){2}", "aba", 0, true, false),
				("(a(b)?){2,4}b", "ababab", 0, true, false),
				("(a|ab)*b", "aab", 0, true, false),
				("(ab|a)*b", "aab", 0, true, false),
				("(a|ab)+?c", "aabc", 0, true, false),
				("(a|ab){0,3}b", "aab", 0, true, false),
				("(a|ab){0,3}?b", "aab", 0, true, false),
				("()*", "", 0, true, false),
				("()*?", "", 0, true, false),
				("()+", "", 0, true, false),
				("(){1,}", "", 0, true, false),
				("(){2}", "", 0, true, false),
				("(){0}", "", 0, true, false),
				("(){0,3}", "", 0, true, false),
				("(){0,3}?", "", 0, true, false),
				("(|a)*", "aa", 0, true, false),
				("(a|)*", "aa", 0, true, false),
				("(|a)+", "aa", 0, true, false),
				("(|a){1,}", "aa", 0, true, false),
				("(|a){2,}", "aa", 0, true, false),
				("(a*)*", "aa", 0, true, false),
				("(a*?)*?", "aa", 0, true, false),
				("((a?)*)b", "aab", 0, true, false),
				("((a?)*)b", "b", 0, true, false),
				("((a)?)*b", "ab", 0, true, false),
				("((a)?)*b", "b", 0, true, false),
				("((a?)*)b", "x", 0, true, false),
				("(a)?(b)?", "ab", 0, true, false),
				("((a)?)(b)?", "b", 0, true, false),
				("((a)?)(b)?", "", 0, true, false),
				("((a)|)(b)?", "ab", 0, true, false),
				("(a?)b?", "a", 0, true, false),
				("a?(b|bc)d?", "abcd", 0, true, false),
				("((a|ab)c?)?d", "abcd", 0, true, false),
				("(a{0,2})(a?)", "aaa", 0, true, false),
				("((a){0,2})(a?)", "aaa", 0, true, false),
				("((a)+)+b", "aaab", 0, true, false),
				("(?<x>a)", "za", 0, true, false),
				("(?'x'a)", "a", 0, true, false),
				("(?P<x>a)", "a", 0, true, false),
				("(?<outer>(?<inner>a)b)", "ab", 0, true, false),
				("(?J)(?<x>a)|(?<x>b)", "b", 0, true, false),
				("(?J)(?<x>a)?(?<x>b)", "ab", 0, true, false),
				("(?J)(?<x>a)?(?<x>b)", "b", 0, true, false),
				("(?J)(?<x>)|(?<x>a)", "a", 0, true, false),
				("(?J)(?<x>a)c|(?<x>b)", "b", 0, true, false),
				("(?|(a)|(b)(c))", "bc", 0, true, false),
				("(?|(a)|(b)(c))", "a", 0, true, false),
				("(?|(?<x>a)|(?<x>b))", "b", 0, true, false),
				("(?J)(?|(a)(?<x>b)|(?<x>c))", "c", 0, true, false),
				("(?n)(a)(?<x>b)", "ab", 0, true, false),
				("(?n:(a))(?<x>b)", "ab", 0, true, false),
				("(?i:(é|x))+y?", "Ééxy", 0, true, false),
				("(😀|x)+y?", "😀😀xy", 0, true, false),
				("(𐐀|x)+y?", "𐐀xy", 0, true, false),
				("(?i:(𐐀))+", "𐐨𐐀", 0, true, false),
				("(\u0000|x)+", "\u0000x", 0, true, false),
				("(\\R)*\\r\\nx", "\r\n\r\nx", 0, true, false),
				("(\\R)*?(x)", "\r\n\rx", 0, true, false),
				("(\\p{Greek})+(x)?", "αβx", 0, true, false),
				("(\\w)+", "é😀", 0, true, false),
				("(\\b)(a)(\\b)", "a", 0, true, false),
				("(?m)^(a*)$", "a\na", 0, true, false),
				("(*CR)(?m)^(a*)$", "a\ra", 0, true, false),
				("(\\G)(a)", "za", 0, true, false),
				("(a)(\\z)", "za", 0, true, false),
				("(a)|(bc)", "zzbc", 0, true, false),
				("(a(b)?)+", "xaba", 0, true, false),
				("(😀)(x)?", "x😀x", 0, true, false),
				("(a)|(b)", "zab", 1, true, false),
				("(\\G)(a)", "za", 1, true, false),
				("(😀|x)+", "z😀x", 1, true, false),
				("(\\p{Cs})+", "\ud800x\udc00", 0, false, true),
				("(\ud800|x)+", "\ud800x", 0, false, false),
				("(\udc00)+", "x\udc00", 0, false, false),
				("(😀)", "😀", 0, false, false),
				("(.)+", "😀", 0, false, false),
				("(\\w)+", "éx", 0, false, true),
				("(?<é>a)", "a", 0, true, false),
				("(?<𐐀>😀)+", "😀😀", 0, true, false),
				("(?<x١>a)", "a", 0, true, false),
				("(?J)(?<é>a)|(?<é>b)", "b", 0, true, false),
				("(?|(?<𐐀>a)|(?<𐐀>b))", "b", 0, true, false),
				("(?J)(?|(a)(?<x>b)|(?<x>c))", "ab", 0, true, false),
				("(?|(?<x>a)|(?<x>b))+", "ab", 0, true, false),
				("(?J)(?<x>a)(?<x>b)", "ab", 0, true, false),
				("(?J)(?<x>a)?(?<x>)", "", 0, true, false)
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
	public void EveryCaptureAndNameAgreesWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"capture {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			Assert.Equal( native.Kind == "M", result.Match is not null );
			Assert.Equal( native.Captures, Spans( result.Match ) );
			CompareNames( native, compiled.Pattern.CaptureInfo, result.Match );
		}
	}

	[Theory]
	[InlineData( "((a)|(b))*a", "aba" )]
	[InlineData( "(a(b)?)+", "aba" )]
	[InlineData( "(|a){2,}", "aa" )]
	[InlineData( "(?J)(?<x>a)|(?<x>b)", "b" )]
	[InlineData( "(?J)(?|(a)(?<x>b)|(?<x>c))", "c" )]
	[InlineData( "(?i:(é|x))+y?", "Ééxy" )]
	[InlineData( "(😀|x)+y?", "😀😀xy" )]
	[InlineData( @"(\R)*\r\nx", "\r\n\r\nx" )]
	public void AllCaptureSpansAndValuesAgreeAcrossManagedWidths( string source, string subject ) {
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
		Assert.Equal( b.CaptureInfo.Names, w.CaptureInfo.Names );
	}

	[Theory]
	[InlineData( "(a)", "a", 5 )]
	[InlineData( "()", "", 4 )]
	[InlineData( "()*", "", 6 )]
	[InlineData( "()*?", "", 3 )]
	[InlineData( "(a)b|a", "a", 9 )]
	[InlineData( @"\A(a|ab)c\z", "abc", 15 )]
	public void CaptureCompletionAndRollbackHaveExactWorkBoundaries( string source, string subject, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var result = pattern.Match( subject.AsMemory() );
			if ( budget < required ) {
				Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code );
				Assert.Null( result.Match );
			} else {
				Assert.True( result.IsSuccess ); Assert.NotNull( result.Match );
				Assert.Equal( PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), false, 0 ).Captures, Spans( result.Match ) );
			}
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void RollbackCancellationDoesNotLeakStateIntoLaterCalls() {
		using var cancellation = new CancellationTokenSource();
		var pattern = PcreWordSequenceCompiler.CompileSequence( @"\A(a|ab)c\z".AsMemory(), new() {
			WorkObserver = work => { if ( work == 8 ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => pattern.Match( "abc".AsMemory(), cancellationToken: cancellation.Token ) );
		Assert.Equal( "0:3,0:2", Spans( pattern.Match( "abc".AsMemory() ).Match ) );
		Assert.Throws<OperationCanceledException>( () => PcreWordSequenceCompiler.CompileSequence(
			"(?<x>a)".AsMemory(), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void ResultsAndMetadataOwnTheirStateAndUseSliceCoordinates() {
		var source = "(?J)(?<x>a)|(?<x>b)".ToCharArray();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source ).Pattern!;
		Array.Fill( source, 'z' );
		var subject = "!zzb!".ToCharArray();
		var first = pattern.Match( subject.AsMemory( 1, 3 ) ).Match!;
		Assert.Equal( "2:1,-,2:1", Spans( first ) );
		Assert.Same( pattern.CaptureInfo, first.CaptureInfo );
		Assert.Equal( new PcreWordCapture( 2, 1 ), first.GetCapture( "x" ) );
		Array.Fill( subject, 'z' );
		Assert.Equal( "0:1,0:1,-", Spans( pattern.Match( "a".AsMemory() ).Match ) );
		Assert.Null( pattern.Match( "z".AsMemory() ).Match );
		Assert.Equal( "2:1,-,2:1", Spans( first ) );
		Assert.Equal( new[] { 1, 2 }, first.CaptureInfo.GetGroupNumbers( "x" ) );
		Assert.Throws<NotSupportedException>( () => ( (IList<PcreWordCapture?>)first.Captures )[0] = null );
		Assert.Throws<NotSupportedException>( () => ( (IList<string>)first.CaptureInfo.Names )[0] = "changed" );
		Assert.Throws<NotSupportedException>( () => ( (IList<int>)first.CaptureInfo.GetGroupNumbers( "x" ) )[0] = 99 );
		foreach ( var name in new[] { "", "X", "x\0", "missing" } ) { Assert.Null( first.GetCapture( name ) ); }
		Assert.Throws<ArgumentNullException>( () => first.GetCapture( null! ) );
	}

	[Fact]
	public async Task ConcurrentCallsKeepIndependentCaptureJournals() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( "(a(b)?)+c|(?<x>b)".AsMemory() ).Pattern!;
		var subjects = new[] { "abac", "b", "zzabc", "z", "aabc" };
		var tasks = Enumerable.Range( 0, 30 ).Select( index => Task.Run( () => {
			var subject = subjects[index % subjects.Length];
			var native = PcreNative16Oracle.ExecuteSequence( Words( "(a(b)?)+c|(?<x>b)" ), Words( subject ), false, 0 );
			var result = pattern.Match( subject.AsMemory() );
			Assert.True( result.IsSuccess ); Assert.Equal( native.Captures, Spans( result.Match ) );
		} ) );
		await Task.WhenAll( tasks );
	}

	[Fact]
	public void CaptureCapacityAndNestingStayBounded() {
		var source = String.Concat( String.Concat( Enumerable.Repeat( "()", 256 ) ), "a" );
		var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Pattern!;
		Assert.Equal( 256, pattern.CaptureInfo.CaptureCount );
		var result = pattern.Match( "a".AsMemory() );
		Assert.True( result.IsSuccess ); Assert.Equal( 257, result.Match!.Captures.Count );
		Assert.All( result.Match.Captures.Skip( 1 ), c => Assert.Equal( new PcreWordCapture( 0, 0 ), c ) );
		Assert.Equal( PcreWordDiagnosticCode.InvalidSyntax,
			PcreWordSequenceCompiler.CompileSequence( String.Concat( "()", source ).AsMemory() ).Diagnostic?.Code );
		var nested = String.Concat( new string( '(', 64 ), "a", new string( ')', 64 ) );
		Assert.Equal( 65, PcreWordSequenceCompiler.CompileSequence( nested.AsMemory() ).Pattern!.Match( "a".AsMemory() ).Match!.Captures.Count );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( String.Concat( "(", nested, ")" ).AsMemory() ).Diagnostic?.Code );
	}

	[Fact]
	public void CaptureNameLimitsUseNativeCodeUnits() {
		foreach ( var name in new[] { new string( 'x', 128 ), new string( 'é', 128 ),
			String.Concat( Enumerable.Repeat( "𐐀", 64 ) ), new string( 'x', 129 ),
			new string( 'é', 129 ), String.Concat( Enumerable.Repeat( "𐐀", 65 ) ), "😀", "é", "x١" } ) {
			foreach ( var utf in new[] { false, true } ) {
				var source = String.Concat( "(?<", name, ">a)" );
				var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( "a" ), utf, 0 );
				var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf } );
				Assert.Equal( native.Kind == "M", compiled.IsSuccess );
				if ( compiled.IsSuccess ) {
					Assert.Equal( new[] { name }, compiled.Pattern!.CaptureInfo.Names );
					Assert.Equal( new PcreWordCapture( 0, 1 ), compiled.Pattern.Match( "a".AsMemory() ).Match!.GetCapture( name ) );
				} else {
					Assert.Equal( "E", native.Kind ); Assert.Equal( PcreWordDiagnosticCode.InvalidSyntax, compiled.Diagnostic?.Code );
				}
			}
		}
	}

	[Fact]
	public void DepthAndWorkFailuresDiscardPartialCaptures() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( @"\A(a)*\z".AsMemory() ).Pattern!;
		Assert.Equal( new PcreWordCapture( 1022, 1 ), pattern.Match( new string( 'a', 1023 ).AsMemory() ).Match!.Captures[1] );
		var failed = pattern.Match( new string( 'a', 1024 ).AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded, failed.Diagnostic?.Code ); Assert.Null( failed.Match );
		Assert.Equal( "0:2,1:1", Spans( pattern.Match( "aa".AsMemory() ).Match ) );
		var limited = PcreWordSequenceCompiler.CompileSequence( @"\A(a|aa)+x".AsMemory(), new() { MatchWorkLimit = 10_000 } ).Pattern!;
		failed = limited.Match( new string( 'a', 20 ).AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, failed.Diagnostic?.Code ); Assert.Null( failed.Match );
		Assert.Equal( "0:2,0:1", Spans( limited.Match( "ax".AsMemory() ).Match ) );
	}

	[Fact]
	public void CaptureFreeAndZeroCountPatternsStillBindMetadata() {
		var literal = PcreWordLiteralCompiler.CompileLiteral( "(a)".AsMemory() ).Pattern!;
		var match = literal.Match( "(a)".AsMemory() ).Match!;
		Assert.Single( match.Captures ); Assert.Equal( 0, match.CaptureInfo.CaptureCount );
		Assert.Same( literal.CaptureInfo, match.CaptureInfo ); Assert.Null( match.GetCapture( "a" ) );
		var zero = PcreWordSequenceCompiler.CompileSequence( "(?<x>a){0}".AsMemory() ).Pattern!;
		match = zero.Match( ReadOnlyMemory<char>.Empty ).Match!;
		Assert.Equal( new[] { "x" }, match.CaptureInfo.Names ); Assert.Null( match.Captures[1] );
		Assert.Null( match.GetCapture( "x" ) );
		var invalid = PcreWordSequenceCompiler.CompileSequence( @"(\b){0}a".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 0 ),
			invalid.Match( "\uD800a".AsMemory(), 1 ).Diagnostic );
	}

	[Theory]
	[InlineData( "(a)(?1)" )]
	[InlineData( "(?<*(a))b" )]
	[InlineData( "(?>a)" )]
	[InlineData( "(a)*+" )]
	[InlineData( "(a)(*ACCEPT)" )]
	public void CaptureDependentExecutionStillRequiresExplicitAdmission( string source ) {
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
	}

	private static void CompareNames( PcreNative16Protocol native, PcreCaptureInfo info, PcreWordMatch? match ) {
		var entries = native.Names.Split( ';', StringSplitOptions.RemoveEmptyEntries ).Select( entry => {
			var parts = entry.Split( '=' );
			var name = new string( Enumerable.Range( 0, parts[0].Length / 4 ).Select( index =>
				(char)UInt16.Parse( parts[0].AsSpan( index * 4, 4 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture ) ).ToArray() );
			return (Name: name, Number: Int32.Parse( parts[1], CultureInfo.InvariantCulture ));
		} ).ToArray();
		Assert.Equal( entries.Select( entry => entry.Name ).Distinct().OrderBy( name => name, StringComparer.Ordinal ), info.Names );
		foreach ( var name in info.Names ) {
			Assert.Equal( entries.Where( entry => entry.Name == name ).Select( entry => entry.Number ).OrderBy( n => n ), info.GetGroupNumbers( name ) );
			if ( match is null ) { continue; }
			var spans = native.Captures.Split( ',' );
			var expected = entries.Where( entry => entry.Name == name ).Select( entry => spans[entry.Number] ).FirstOrDefault( span => span != "-" ) ?? "-";
			Assert.Equal( expected, match.GetCapture( name ) is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "-" );
		}
	}

	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( c => c is { } span ? $"{span.CodeUnitIndex}:{span.CodeUnitLength}" : "-" ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
