using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordMatchStartResetTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new (string Source, string Subject, int Start, bool Lookaround)[] {
				( @"ab\Kc", "abc", 0, false ),
				( @"(?:ab\KX|ab)c", "abc", 0, false ),
				( @"(a)(?<x>b\Kc)(d)", "xabcd", 1, false ),
				( @"\Ga\Kb", "zab", 1, false ),
				( @"a\K\G|b", "ab", 0, false ),
				( @"a\K\Kb\K", "zab", 0, false ),
				( @"a\K|ab", "ab", 0, false ),
				( @"a\K|ab", "zab", 1, false ),
				( @"\K|b", "ab", 0, false ),
				( @"\K", "", 0, false ),
				( @"\K\z", "ab", 2, false ),
				( @"(?:a\K)+a", "aaa", 0, false ),
				( @"(?:a\K){1,3}?a", "aaa", 0, false ),
				( @"(?:a\K){1,3}+b", "aaab", 0, false ),
				( @"a{1,3}\Kb{0,2}b", "aaabb", 0, false ),
				( @"a\K(?:b{1,2}\KX|bb)", "abb", 0, false ),
				( @"(?:a\K){0}b", "b", 0, false ),
				( @"(?:(?>a\K)bX|ab)c", "abc", 0, false ),
				( @"(?>a\K)b", "ab", 0, false ),
				( @"(?<x>a\K)\k<x>", "aa", 0, false ),
				( @"(?<x>a)?(?(x)\Kb|c)", "ab", 0, false ),
				( @"(?(DEFINE)(?<x>a\K))(?&x)b", "zab", 0, false ),
				( @"(?(DEFINE)(?<x>a\K))(?&x)X|ab", "ab", 0, false ),
				( @"(?(DEFINE)(?<x>a\K(?&x)?b))(?&x)c", "aabbc", 0, false ),
				( @"(?(DEFINE)(?<x>(?<v>a)\K))(?&x(2))b", "ab", 0, false ),
				( @"(?(DEFINE)(?<x>a\K(*ACCEPT:M)))(?&x)b", "ab", 0, false ),
				( @"a\K(*ACCEPT:M)|ab", "ab", 0, false ),
				( @"(a\K(*ACCEPT:M))b", "ab", 0, false ),
				( @"a\K(*THEN:M)X|ab", "ab", 0, false ),
				( @"a\K(*PRUNE:M)X|b", "ab", 0, false ),
				( @"(?:é+\K(*PRUNE)X|(é))", "ééé", 0, false ),
				( @"(?:a+(*PRUNE)\KX|(a))", "aaa", 0, false ),
				( @"a+\K(*PRUNE)X\K|a", "aaa", 0, false ),
				( @"a+\K(*THEN)(*PRUNE)X|a", "aaa", 0, false ),
				( @"a+(*PRUNE)\KX|a", "aaa", 0, false ),
				( @"\Ka+(*PRUNE)X|a", "aaa", 0, false ),
				( @"a+\K(*PRUNE)X|a", "aaa", 0, false ),
				( @"a+(*MARK:M)\K(*PRUNE)X|a", "aaa", 0, false ),
				( @"a+(*PRUNE)X|(a\K)", "aaa", 0, false ),
				( @"\K(?:a+(*PRUNE)X|a)", "aaa", 0, false ),
				( @"\Ka+(*PRUNE)X|a\K", "aaa", 0, false ),
				( @"(?:a+(*PRUNE)X|a)\K", "aaa", 0, false ),
				( @"a+(*PRUNE)X|\Ka", "aaa", 0, false ),
				( @"\K(*COMMIT)a", "zab", 0, false ),
				( @"a\K(*MARK:M)b(*SKIP:M)X|(b)", "ab", 0, false ),
				( @"a\K(*SKIP:missing)X|(ab)", "ab", 0, false ),
				( @"(😀)\K(x)", "z😀x", 1, false ),
				( @"é\Ké", "éé", 0, false ),
				( @"(?=a\K)", "a", 0, true ),
				( @"(?=a\K)a", "a", 0, true ),
				( @"(?!a\Kb)a", "a", 0, true ),
				( @"(?:(?!a\K)b|a)", "a", 0, true ),
				( @"(?=(?<x>a)\K)(*MARK:M).", "a", 0, true ),
				( @"(?=(?<x>a)\K)X|a", "a", 0, true ),
				( @"(?*(a\K|ab\K))\1c", "abc", 0, true ),
				( @"(?=(?:a\KX|ab))ab", "ab", 0, true ),
				( @"(?<=\Ka)b", "ab", 1, true ),
				( @"(?<=\K😀)x", "😀x", 2, true ),
				( @"(?<=a{1,2}\K)b", "aab", 2, true ),
				( @"(?(?=a\K)b|c)|a", "a", 0, true ),
				( @"(?(?=a\K)a|c)", "a", 0, true ),
				( @"(?(?!a\K)c|a)", "a", 0, true ),
				( @"a\K(?=b\K(*ACCEPT:M))b", "ab", 0, true ),
				( @"a\K(?(?=b\K(*ACCEPT:M))b|c)", "ab", 0, true ),
				( @"(?*a\K(*ACCEPT:M))a", "a", 0, true ),
				( @"(?=a\K){2}a", "a", 0, true ),
				( @"(?(DEFINE)(?<x>a\K))(?=(?&x))a", "a", 0, false ),
			};
			foreach ( var (source, subject, start, lookaround) in cases ) {
				foreach ( var utf in new[] { false, true } ) {
					foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 16, 32, 63 } ) {
						yield return new object[] { source, subject, start, lookaround, utf, defaults };
					}
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void FullCaptureVectorsAndLabelsAgreeWithNativePolicies(
		string source, string subject, int start, bool lookaround, bool utf, int defaults
	) {
		var native = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start,
			defaults, ucp: utf, allowLookaroundBackslashK: lookaround );
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = utf, AllowLookaroundBackslashK = lookaround, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, compiled.Diagnostic?.ToString() );
		for ( var mask = 0; mask < native.Length; mask++ ) {
			var expected = native[mask];
			Assert.True( expected.Kind is "M" or "N", $"Native policy {mask}: {expected}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"Managed policy {mask}: {result.Diagnostic}" );
			Assert.True( expected.Kind == ( result.Match is null ? "N" : "M" )
				&& expected.Captures == Spans( result.Match ) && expected.Mark == MarkHex( result.Match ),
				$"Policy {mask}: expected {expected.Kind} {expected.Captures}|{expected.Mark}, " +
				$"actual {( result.Match is null ? "N" : "M" )} {Spans( result.Match )}|{MarkHex( result.Match )}" );
		}
	}

	[Theory]
	[InlineData( @"(?=a\K)", false, 0 )]
	[InlineData( @"(?<=a\K)", false, 0 )]
	[InlineData( @"😀(?=a\K)", false, 2 )]
	[InlineData( @"a\K+", true, 3 )]
	public void ParserPreservesOriginalUnitDiagnostics( string source, bool lookaround, int offset ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = true, AllowLookaroundBackslashK = lookaround
		} );
		Assert.Equal( new( PcreWordDiagnosticCode.InvalidSyntax, offset ), compiled.Diagnostic );
	}

	[Theory]
	[InlineData( @"a\K(?(?=(b)\K)b|c)", "ab", "2:0,1:1", "1:1,1:1" )]
	[InlineData( @"(?(?!(a)\K)c|a)", "a", "1:0,0:1", "0:1,0:1" )]
	[InlineData( @"a\K(?(?=b\K)bX|c)|ab", "ab", "0:2", "0:2" )]
	public void ConditionalAssertionsKeepTheirDispatchAcrossWidths( string source, string subject, string jit, string interpreted ) {
		var word = Compile( source, new() { AllowLookaroundBackslashK = true } );
		var bytes = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( source ), new() { AllowLookaroundBackslashK = true } );
		Assert.True( bytes.IsSuccess );
		var byteInput = Encoding.ASCII.GetBytes( subject );
		var native = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), false, 0,
			allowLookaroundBackslashK: true );
		foreach ( var (mask, spans) in new[] { (0, jit), (1, interpreted) } ) {
			var policy = PcreMatchPolicyConsumer.Policy( mask );
			Assert.Equal( spans, native[mask].Captures );
			Assert.Equal( spans, Spans( word.Match( subject.AsMemory(), policy ).Match ) );
			Assert.Equal( String.Concat( spans, "/" ), PcreMatchPolicyConsumer.Project( bytes.Pattern!.Match( byteInput, policy ) ) );
		}
	}

	[Theory]
	[InlineData( @"a+\K(*PRUNE)X|a" )]
	[InlineData( @"\Ka+(*PRUNE)X|a" )]
	[InlineData( @"a+(*PRUNE)\KX|a" )]
	[InlineData( @"a+\K(*THEN)(*PRUNE)X|a" )]
	public void PruneScanSkipsAnExhaustedCandidateAcrossWidths( string source ) {
		const string subject = "aaa";
		var bytes = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( source ) );
		Assert.True( bytes.IsSuccess );
		var native = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), false, 0 );
		var byteInput = Encoding.ASCII.GetBytes( subject );
		for ( var mask = 0; mask < native.Length; mask++ ) {
			var expected = native[mask];
			Assert.True( expected.Kind is "M" or "N" );
			var result = bytes.Pattern!.Match( byteInput, PcreMatchPolicyConsumer.Policy( mask ) );
			Assert.Equal( String.Concat( expected.Captures, "/", expected.Mark ), PcreMatchPolicyConsumer.Project( result ) );
		}
	}

	[Fact]
	public void SupplementaryAndRawSurrogateSpansUseOriginalUnits() {
		const string pattern = @"(😀)\K(x)";
		const string subject = "__😀x__";
		var word = Compile( pattern, new() { Utf16 = true } ).Match( subject.AsMemory( 2, 3 ) ).Match;
		Assert.Equal( "2:1,0:2,2:1", Spans( word ) );
		var bytes = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.True( bytes.IsSuccess );
		var byteMatch = bytes.Pattern!.Match( Encoding.UTF8.GetBytes( subject ).AsMemory( 2, 5 ) ).Match;
		Assert.Equal( 4, byteMatch?.ByteIndex );
		Assert.Equal( 1, byteMatch?.ByteLength );
		Assert.Equal( "1:1", Spans( Compile( "\ud800\\Kx", new() ).Match( "\ud800x".AsMemory() ).Match ) );
	}

	[Fact]
	public void ResetAndRollbackShareWorkAndCancellation() {
		const string source = @"(?:a\KX|a)b\Kc";
		const string subject = "abc";
		var work = 0;
		var result = Compile( source, new() { WorkObserver = step => work = step } ).Match( subject.AsMemory() );
		Assert.Equal( "2:1", Spans( result.Match ) );
		Assert.Equal( "2:1", Spans( Compile( source, new() { MatchWorkLimit = work } ).Match( subject.AsMemory() ).Match ) );
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded,
			Compile( source, new() { MatchWorkLimit = work - 1 } ).Match( subject.AsMemory() ).Diagnostic?.Code );
		for ( var step = 1; step <= work; step++ ) {
			using var canceled = new CancellationTokenSource();
			var stop = step;
			var compiled = Compile( source, new() { WorkObserver = current => { if ( current == stop ) { canceled.Cancel(); } } } );
			Assert.Throws<OperationCanceledException>( () => compiled.Match( subject.AsMemory(), cancellationToken: canceled.Token ) );
			Assert.Equal( "2:1", Spans( compiled.Match( subject.AsMemory() ).Match ) );
		}
	}

	private static PcreCompiledWordPattern Compile( string source, PcreWordOptions options ) {
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.ToString() );
		return result.Pattern!;
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( span => span is { } actual ? FormattableString.Invariant( $"{actual.CodeUnitIndex}:{actual.CodeUnitLength}" ) : "-" ) );
	private static string MarkHex( PcreWordMatch? match ) => match is null ? "" : String.Concat(
		match.MarkName.Select( unit => FormattableString.Invariant( $"{(int)unit:X4}" ) ) );
}
