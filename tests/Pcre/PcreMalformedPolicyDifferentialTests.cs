using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreMalformedPolicyDifferentialTests {
	private static readonly string[] Patterns = {
		"", ".", ".?", @"\X", @"(\p{L}?)\1", @"(?=(.))\1", @"(?<=a|\x{E9})b",
		@"\b.", @"\B.", @"\A.*\z", @"(?:a|\x{E9}|\x{1F600})+",
		"(?&x)b(?(DEFINE)(?<x>\\X))", "(?:a(*SKIP)(*FAIL)|.)",
		@"(?=a\K(*ACCEPT))a", "(*MARK:M)(?(?=a(*ACCEPT:N))a|b)",
		@"(?:a\K(*ACCEPT)|ab)"
	};
	// Byte values are authoritative: never decode these through a replacement fallback.
	private static readonly string[] SubjectHex = {
		"", "61", "6162", "C3A962", "F09F988062", "00", "FF", "80", "C0AF", "C2",
		"E282", "E080AF", "EDA080", "F09F98", "F08080AF", "F4908080", "F5808080",
		"61FF62", "61FFFF62", "80FF6162", "C3A9FF6162", "61E28262", "F09F9880FF6162", "610D0AFF62"
	};

	public static IEnumerable<object[]> Profiles {
		get {
			for ( var index = 0; index < Patterns.Length; index++ ) {
				foreach ( var mode in new[] { 12, 15 } ) {
					foreach ( var noJit in new[] { false, true } ) { yield return new object[] { index, mode, noJit }; }
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Profiles ) )]
	public void EveryByteOffsetAndCallPolicyPreservesMalformedFragments( int index, int mode, bool noJit ) {
		var pattern = Encoding.ASCII.GetBytes( String.Concat( noJit ? "(*NO_JIT)" : "", Patterns[index] ) );
		var probe = new PcrePolicyDifferentialProbe( pattern, mode );
		var comparisons = 0;
		foreach ( var hex in SubjectHex ) {
			var subject = Convert.FromHexString( hex );
			for ( var start = 0; start <= subject.Length; start++ ) {
				for ( var mask = 0; mask < 64; mask++ ) {
					var mismatch = probe.Compare( subject, mask, start );
					if ( mismatch is not null ) {
						var reduced = PcrePolicyDifferentialProbe.Reduce( pattern, subject, mode, mask, start, mismatch.Kind );
						Assert.Fail( String.Concat( "Original: ", mismatch, "; reduced: ", reduced.Mismatch,
							"; status: ", reduced.Reduction.Status, "; evaluations: ", reduced.Reduction.Evaluations,
							"; replay: ", reduced.Mismatch?.Replay ) );
					}
					comparisons++;
				}
			}
		}
		Assert.Equal( 64 * SubjectHex.Sum( hex => hex.Length / 2 + 1 ), comparisons );
	}

	[Theory]
	[InlineData( 12 )]
	[InlineData( 15 )]
	public void LostTolerantModeReducesToTheOriginalMalformedByte( int mode ) {
		var source = "(*NO_JIT)(?:a|.)"u8.ToArray();
		var subject = Convert.FromHexString( "78FF61" );
		var result = PcrePolicyDifferentialProbe.Reduce( source, subject, mode, 0, 0,
			PcrePolicyMismatchKind.Diagnostic, StrictCandidate );
		Assert.Equal( PcreReductionStatus.Completed, result.Reduction.Status );
		Assert.InRange( result.Reduction.Evaluations, 1, 1024 );
		Assert.Equal( new byte[] { 0xFF }, result.Reduction.Subject.ToArray() );
		var mismatch = Assert.IsType<PcrePolicyMismatch>( result.Mismatch );
		Assert.Equal( PcrePolicyMismatchKind.Diagnostic, mismatch.Kind );
		Assert.Equal( "InvalidUtf8Subject", mismatch.Managed );
		Assert.Equal( "FF", mismatch.Subject );
		Assert.Equal( mismatch, new PcrePolicyDifferentialProbe( result.Reduction.Pattern, mode, StrictCandidate )
			.Compare( result.Reduction.Subject, 0 ) );
		Assert.Equal( String.Concat( "0|", mode, "|0|0|", mismatch.Pattern.Length == 0 ? "-" : mismatch.Pattern, "|FF|0" ), mismatch.Replay );
	}

	private static PcreByteCompileResult StrictCandidate( ReadOnlyMemory<byte> pattern, PcreByteOptions options ) =>
		PcreByteCompiler.Compile( pattern, options with { Utf8 = true, MatchInvalidUtf = false } );
}
