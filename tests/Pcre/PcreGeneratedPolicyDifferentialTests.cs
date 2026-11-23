using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreGeneratedPolicyDifferentialTests {
	private static readonly uint[] Seeds = { 1, 0x12345678, 0x9E3779B9, 0xDEADBEEF };

	public static IEnumerable<object[]> Profiles {
		get {
			foreach ( var seed in Seeds ) {
				foreach ( var mode in new[] { 8, 15 } ) {
					foreach ( var noJit in new[] { false, true } ) { yield return new object[] { seed, mode, noJit }; }
				}
			}
		}
	}

	[Theory]
	[InlineData( 1u, 8, "94286F3419A73909AA06C6D6E63F8ECA39A4A5FD4A170412037B41BA20560272" )]
	[InlineData( 1u, 15, "A029A199B083C79F81768A82A128712F6AAE2EA9D82A2F662943A1196FFA0F59" )]
	[InlineData( 0x12345678u, 8, "CBC9A9C83C528A3A930F8ACF0101D78AC64ED537052AF871A47CA85DB73A7031" )]
	[InlineData( 0x12345678u, 15, "7ECAB61D733342CAB351B893CD3335E97AF99302AB4464DB6A5007C9139A1B4D" )]
	[InlineData( 0x9E3779B9u, 8, "1E0ABF39745285554BC77970C37CA146CBDCA84FFFFC22DEA749AB5D19C4ABD2" )]
	[InlineData( 0x9E3779B9u, 15, "91C12A9FA782A77BE338D3FAA0253526066A4D38ED46394AEF57D3DE7688B9B7" )]
	[InlineData( 0xDEADBEEFu, 8, "7EA881928C902D914DDFF8B5FBBD1D78CBFEE663AF218A1988BD4A8944C34C0B" )]
	[InlineData( 0xDEADBEEFu, 15, "00F2DDB9F722ED029E6454D48AF15E591F9179F3EE0CC6B7D44ACADDF16230C3" )]
	public void GeneratedCorpusRetainsItsIndependentPreflightDigest( uint seed, int mode, string expected ) {
		var samples = PcrePolicyGrammarGenerator.Create( seed, mode );
		// These are protocol bytes: LF is explicit on every operating system.
		var rows = String.Concat( samples.Select( sample => String.Concat(
			Convert.ToHexString( Encoding.ASCII.GetBytes( sample.Pattern ) ), "|",
			Convert.ToHexString( sample.Subject ), "|", sample.Start.ToString( CultureInfo.InvariantCulture ), "\n" ) ) );
		Assert.Equal( expected, Convert.ToHexString( SHA256.HashData( Encoding.ASCII.GetBytes( rows ) ) ) );
	}

	[Fact]
	public void GeneratedMarkFaultIsDetectedReducedAndReproducedAtItsOriginalOffset() {
		var sample = PcrePolicyGrammarGenerator.Create( 1, 15 )[2];
		var pattern = Encoding.ASCII.GetBytes( sample.Pattern );
		Assert.Equal( 2, sample.Start );
		var result = PcrePolicyDifferentialProbe.Reduce( pattern, sample.Subject, 15, 0, sample.Start,
			PcrePolicyMismatchKind.Mark, ChangeMark );
		Assert.Equal( PcreReductionStatus.Completed, result.Reduction.Status );
		Assert.InRange( result.Reduction.Evaluations, 1, 1024 );
		Assert.True( result.Reduction.Pattern.Length < pattern.Length );
		Assert.Equal( sample.Start, result.Reduction.StartOffset );
		var mismatch = Assert.IsType<PcrePolicyMismatch>( result.Mismatch );
		Assert.Equal( PcrePolicyMismatchKind.Mark, mismatch.Kind );
		Assert.EndsWith( "/53", mismatch.Native );
		Assert.EndsWith( "/54", mismatch.Managed );
		Assert.Equal( mismatch.Native.Split( '/' )[0], mismatch.Managed.Split( '/' )[0] );
		Assert.Equal( mismatch, new PcrePolicyDifferentialProbe( result.Reduction.Pattern, 15, ChangeMark )
			.Compare( result.Reduction.Subject, 0, sample.Start ) );
		Assert.EndsWith( "|2", mismatch.Replay );
	}

	private static PcreByteCompileResult ChangeMark( ReadOnlyMemory<byte> source, PcreByteOptions options ) =>
		PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( Encoding.ASCII.GetString( source.Span )
			.Replace( "(*MARK:S)", "(*MARK:T)", StringComparison.Ordinal ) ), options );

	[Theory]
	[MemberData( nameof( Profiles ) )]
	public void SeededGrammarPreservesNativePoliciesAndReducesAnyMismatch( uint seed, int mode, bool noJit ) {
		var samples = PcrePolicyGrammarGenerator.Create( seed, mode );
		Assert.Equal( 128, samples.Count );
		for ( var index = 0; index < samples.Count; index++ ) {
			var sample = samples[index];
			var pattern = Encoding.ASCII.GetBytes( String.Concat( noJit ? "(*NO_JIT)" : "", sample.Pattern ) );
			var probe = new PcrePolicyDifferentialProbe( pattern, mode );
			for ( var mask = 0; mask < 64; mask++ ) {
				var mismatch = probe.Compare( sample.Subject, mask, sample.Start );
				if ( mismatch is null ) { continue; }
				var reduced = PcrePolicyDifferentialProbe.Reduce( pattern, sample.Subject, mode, mask, sample.Start, mismatch.Kind );
				Assert.Fail( String.Concat( $"Seed: {seed:X8}; sample: {index}; original: ", mismatch,
					"; reduced: ", reduced.Mismatch, "; status: ", reduced.Reduction.Status,
					"; evaluations: ", reduced.Reduction.Evaluations, "; replay: ", reduced.Mismatch?.Replay ) );
			}
		}
	}

	[Theory]
	[InlineData( 0u, 8, 1 )]
	[InlineData( 1u, 0, 1 )]
	[InlineData( 1u, 9, 1 )]
	[InlineData( 1u, 8, 0 )]
	[InlineData( 1u, 8, 257 )]
	public void InvalidGenerationRequestsAreRejected( uint seed, int mode, int count ) =>
		Assert.Throws<ArgumentOutOfRangeException>( () => PcrePolicyGrammarGenerator.Create( seed, mode, count ) );
}
