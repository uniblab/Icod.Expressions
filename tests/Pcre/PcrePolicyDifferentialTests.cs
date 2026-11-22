using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcrePolicyDifferentialTests {
	private static readonly string[] Patterns = {
		"a?", @"(?:a|b){0,2}", @"(a?)\1", @"(?=(a))\1", @"(?<=a)b",
		"(a)?(?(1)b|a)", "(?&x)b(?(DEFINE)(?<x>a))", @"(?=a\K(*ACCEPT))a",
		"(*MARK:M)(?(?=a(*ACCEPT:N))a|b)",
		@"(*MARK:M)(?:\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))",
		@"(?:a\K(*ACCEPT)|ab)", "(?:a(*SKIP:missing)b|ac)"
	};
	private static readonly (string Text, int Start)[] Subjects = {
		("", 0), ("a", 0), ("b", 0), ("ab", 0), ("ac", 0), ("abc", 0), ("ba", 1), ("xabc", 1)
	};
	private const string FaultPattern = @"(*NO_JIT)(?=a\K(*ACCEPT))a";

	public static IEnumerable<object[]> Profiles {
		get {
			for ( var index = 0; index < Patterns.Length; index++ ) {
				foreach ( var mode in new[] { 8, 9, 11, 12 } ) {
					foreach ( var noJit in new[] { false, true } ) { yield return new object[] { index, mode, noJit }; }
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Profiles ) )]
	public void BoundedGrammarSweepComparesEveryPolicyAndReducesAnyMismatch( int index, int mode, bool noJit ) {
		var pattern = Bytes( String.Concat( noJit ? "(*NO_JIT)" : "", Patterns[index] ) );
		var probe = new PcrePolicyDifferentialProbe( pattern, mode );
		var comparisons = 0;
		foreach ( var item in Subjects ) {
			var subject = Bytes( item.Text );
			for ( var mask = 0; mask < 64; mask++ ) {
				var mismatch = probe.Compare( subject, mask, item.Start );
				if ( mismatch is not null ) {
					var reduced = PcrePolicyDifferentialProbe.Reduce( pattern, subject, mode, mask, item.Start, mismatch.Kind );
					Assert.Fail( String.Concat( "Original: ", mismatch, "; reduced: ", reduced.Mismatch,
						"; status: ", reduced.Reduction.Status, "; evaluations: ", reduced.Reduction.Evaluations,
						"; replay: ", reduced.Mismatch?.Replay ) );
				}
				comparisons++;
			}
		}
		Assert.Equal( 512, comparisons );
	}

	[Fact]
	public void DroppedInterpreterDispatchIsDetectedAndReducedAgainstLiveNativeMatching() {
		var original = Bytes( FaultPattern );
		var subject = Bytes( "zza" );
		var mismatch = new PcrePolicyDifferentialProbe( original, 8, DropNoJit ).Compare( subject, 0 );
		Assert.Equal( PcrePolicyMismatchKind.Captures, mismatch?.Kind );
		var result = PcrePolicyDifferentialProbe.Reduce( original, subject, 8, 0, 0,
			PcrePolicyMismatchKind.Captures, DropNoJit );
		Assert.Equal( PcreReductionStatus.Completed, result.Reduction.Status );
		Assert.InRange( result.Reduction.Evaluations, 1, 1024 );
		Assert.True( result.Reduction.Subject.Length < subject.Length );
		var reduced = Assert.IsType<PcrePolicyMismatch>( result.Mismatch );
		Assert.Equal( PcrePolicyMismatchKind.Captures, reduced.Kind );
		Assert.NotEqual( reduced.Native, reduced.Managed );
		Assert.Equal( reduced, new PcrePolicyDifferentialProbe( result.Reduction.Pattern, 8, DropNoJit )
			.Compare( result.Reduction.Subject, 0 ) );
	}

	[Fact]
	public void MarkFaultCannotReduceIntoAnUnrelatedCaptureMismatch() {
		var result = PcrePolicyDifferentialProbe.Reduce( Bytes( "z?(*MARK:M)a" ), Bytes( "xxa" ), 8, 0, 0,
			PcrePolicyMismatchKind.Mark, ChangeMark );
		Assert.Equal( PcreReductionStatus.Completed, result.Reduction.Status );
		var mismatch = Assert.IsType<PcrePolicyMismatch>( result.Mismatch );
		Assert.Equal( PcrePolicyMismatchKind.Mark, mismatch.Kind );
		Assert.EndsWith( "/4D", mismatch.Native );
		Assert.EndsWith( "/4E", mismatch.Managed );
		Assert.Equal( mismatch.Native.Split( '/' )[0], mismatch.Managed.Split( '/' )[0] );
	}

	[Fact]
	public void EvaluationLimitKeepsTheLastConfirmedMismatchWithoutAnExtraOracleCall() {
		var calls = 0;
		var result = PcrePolicyDifferentialProbe.Reduce( Bytes( FaultPattern ), Bytes( "zza" ), 8, 0, 0,
			PcrePolicyMismatchKind.Captures, ( source, options ) => { calls++; return DropNoJit( source, options ); },
			maximumEvaluations: 1 );
		Assert.Equal( PcreReductionStatus.EvaluationLimit, result.Reduction.Status );
		Assert.Equal( 1, result.Reduction.Evaluations );
		Assert.Equal( 1, calls );
		Assert.Equal( PcrePolicyMismatchKind.Captures, result.Mismatch?.Kind );
	}

	[Fact]
	public void NonReproducingInputDoesNotAcquireAMismatch() {
		var result = PcrePolicyDifferentialProbe.Reduce( Bytes( "a" ), Bytes( "a" ), 8, 0, 0, PcrePolicyMismatchKind.Captures );
		Assert.Equal( PcreReductionStatus.NotReproduced, result.Reduction.Status );
		Assert.Equal( 1, result.Reduction.Evaluations );
		Assert.Null( result.Mismatch );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void CancellationBeforeOrDuringReductionPropagates( bool during ) {
		using var source = new CancellationTokenSource();
		var calls = 0;
		if ( !during ) { source.Cancel(); }
		Assert.Throws<OperationCanceledException>( () => PcrePolicyDifferentialProbe.Reduce(
			Bytes( FaultPattern ), Bytes( "a" ), 8, 0, 0, PcrePolicyMismatchKind.Captures,
			( pattern, options ) => { calls++; source.Cancel(); return DropNoJit( pattern, options ); },
			cancellationToken: source.Token ) );
		Assert.Equal( during ? 1 : 0, calls );
	}

	[Fact]
	public void CandidateExceptionsPropagateInsteadOfBecomingNoMatch() {
		var error = new InvalidOperationException( "candidate failed" );
		var actual = Assert.Throws<InvalidOperationException>( () => PcrePolicyDifferentialProbe.Reduce(
			Bytes( "a" ), Bytes( "a" ), 8, 0, 0, PcrePolicyMismatchKind.Captures, ( _, _ ) => throw error ) );
		Assert.Same( error, actual );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	public void PresenceCompilationAndResourceFaultsKeepDistinctClasses( int fault ) {
		var probe = new PcrePolicyDifferentialProbe( Bytes( "a" ), 8, ( pattern, options ) => {
			if ( fault == 0 ) { return PcreByteCompiler.Compile( "b"u8.ToArray(), options ); }
			if ( fault == 1 ) { return PcreByteCompiler.Compile( "(?C2)"u8.ToArray(), options ); }
			return PcreByteCompiler.Compile( pattern, options with { MatchWorkLimit = 1 } );
		} );
		var mismatch = Assert.IsType<PcrePolicyMismatch>( probe.Compare( Bytes( "a" ), 0 ) );
		Assert.Equal( new[] { PcrePolicyMismatchKind.Presence, PcrePolicyMismatchKind.Compilation,
			PcrePolicyMismatchKind.Diagnostic }[fault], mismatch.Kind );
		Assert.Equal( "0:1/", mismatch.Native );
	}

	[Fact]
	public void ReplayOwnsOriginalByteViewsAndPreservesTheStartOffset() {
		var pattern = Bytes( String.Concat( "X", FaultPattern, "Y" ) );
		var subject = Bytes( "XzzaY" );
		var probe = new PcrePolicyDifferentialProbe( pattern.AsMemory( 1, pattern.Length - 2 ), 8, DropNoJit );
		Array.Fill( pattern, (byte)'Z' );
		var mismatch = Assert.IsType<PcrePolicyMismatch>( probe.Compare( subject.AsMemory( 1, 3 ), 0, 1 ) );
		Array.Fill( subject, (byte)'Z' );
		Assert.Equal( Convert.ToHexString( Bytes( FaultPattern ) ), mismatch.Pattern );
		Assert.Equal( "7A7A61", mismatch.Subject );
		Assert.Equal( 1, mismatch.Start );
		Assert.Equal( String.Concat( "0|8|0|0|", mismatch.Pattern, "|7A7A61|1" ), mismatch.Replay );
	}

	private static byte[] Bytes( string text ) => Encoding.ASCII.GetBytes( text );
	private static PcreByteCompileResult DropNoJit( ReadOnlyMemory<byte> pattern, PcreByteOptions options ) =>
		PcreByteCompiler.Compile( pattern.Span.StartsWith( "(*NO_JIT)"u8 ) ? pattern[9..] : pattern, options );
	private static PcreByteCompileResult ChangeMark( ReadOnlyMemory<byte> pattern, PcreByteOptions options ) =>
		PcreByteCompiler.Compile( Bytes( Encoding.ASCII.GetString( pattern.Span ).Replace( "(*MARK:M)", "(*MARK:N)", StringComparison.Ordinal ) ), options );
}
