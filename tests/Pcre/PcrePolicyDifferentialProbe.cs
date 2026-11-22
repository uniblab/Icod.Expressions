using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;

namespace Icod.Expressions.Tests.Pcre;

internal enum PcrePolicyMismatchKind { Compilation, Diagnostic, Presence, Captures, Mark }

internal sealed record PcrePolicyMismatch(
	PcrePolicyMismatchKind Kind, int Mode, int Mask, int Start, string Pattern, string Subject,
	string Native, string Managed
) {
	// This is an input row for Native/match-policy-oracle.c, not decoded text.
	internal string Replay => String.Join( "|", "0", Mode, "0", Mask,
		Pattern.Length == 0 ? "-" : Pattern, Subject.Length == 0 ? "-" : Subject, Start );
}

internal sealed record PcreReducedPolicyMismatch( PcreReduction<byte> Reduction, PcrePolicyMismatch? Mismatch );

// Test-only native differential adapter. Fault controls replace the candidate
// compiler; reference compilation, matching and projection remain independent.
internal sealed class PcrePolicyDifferentialProbe {
	private sealed class NativePatternRejectedException( PcrePatternException error ) : Exception( "Native pattern rejected", error ) { }
	internal delegate PcreByteCompileResult CandidateCompiler( ReadOnlyMemory<byte> source, PcreByteOptions options );
	private readonly byte[] source;
	private readonly int mode;
	private readonly PcreRegex8Bit reference;
	private readonly PcreByteCompileResult candidate;

	internal PcrePolicyDifferentialProbe( ReadOnlyMemory<byte> pattern, int mode, CandidateCompiler? compiler = null ) {
		if ( ( mode & ~15 ) != 0 ) { throw new ArgumentOutOfRangeException( nameof( mode ) ); }
		source = pattern.ToArray();
		this.mode = mode;
		var flags = ( ( mode & 1 ) != 0 ? 0x80000u : 0 ) | ( ( mode & 2 ) != 0 ? 0x20000u : 0 )
			| ( ( mode & 4 ) != 0 ? 0x4000000u : 0 );
		try { reference = new PcreRegex8Bit( source, Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | (PcreOptions)flags,
			ExtraCompileOptions = (PcreExtraCompileOptions)( ( mode & 8 ) != 0 ? 64 : 0 ),
			NewLine = (PcreNewLine)2, BackslashR = (PcreBackslashR)1, ParensLimit = 512
		} ); } catch ( PcrePatternException error ) { throw new NativePatternRejectedException( error ); }
		var options = new PcreByteOptions {
			Utf8 = ( mode & 1 ) != 0, Ucp = ( mode & 2 ) != 0,
			MatchInvalidUtf = ( mode & 4 ) != 0, AllowLookaroundBackslashK = ( mode & 8 ) != 0
		};
		candidate = compiler is null ? PcreByteCompiler.Compile( source, options ) : compiler( source.ToArray(), options );
	}

	internal PcrePolicyMismatch? Compare( ReadOnlyMemory<byte> input, int mask, int start = 0,
		CancellationToken cancellationToken = default ) {
		if ( mask is < 0 or > 63 ) { throw new ArgumentOutOfRangeException( nameof( mask ) ); }
		if ( start < 0 || start > input.Length ) { throw new ArgumentOutOfRangeException( nameof( start ) ); }
		cancellationToken.ThrowIfCancellationRequested();
		var expected = reference.Match( input.ToArray(), start, PcreNativeMatchPolicy.Options( 0, mask ) );
		cancellationToken.ThrowIfCancellationRequested();
		var captures = new List<string>();
		if ( expected.Success ) {
			for ( var index = 0; index < expected.Groups.Count; index++ ) {
				var group = expected.Groups[index];
				captures.Add( group.Success ? $"{group.Index}:{group.EndIndex - group.Index}" : "-" );
			}
		}
		var spans = String.Join( ",", captures );
		var mark = expected.Success ? Convert.ToHexString( expected.Mark.ToArray() ) : "";
		var native = String.Concat( spans, "/", mark );
		PcrePolicyMismatch Difference( PcrePolicyMismatchKind kind, string actual ) =>
			new( kind, mode, mask, start, Convert.ToHexString( source ), Convert.ToHexString( input.Span ), native, actual );
		if ( !candidate.IsSuccess ) { return Difference( PcrePolicyMismatchKind.Compilation, candidate.Diagnostic!.Code.ToString() ); }
		var result = candidate.Pattern!.Match( input, PcreMatchPolicyConsumer.Policy( mask ), start, cancellationToken );
		if ( result.Diagnostic is { } diagnostic ) { return Difference( PcrePolicyMismatchKind.Diagnostic, diagnostic.Code.ToString() ); }
		var managed = PcreMatchPolicyConsumer.Project( result );
		if ( expected.Success != ( result.Match is not null ) ) { return Difference( PcrePolicyMismatchKind.Presence, managed ); }
		if ( !expected.Success || native == managed ) { return null; }
		var actualSpans = managed[..managed.IndexOf( '/' )];
		return Difference( spans != actualSpans ? PcrePolicyMismatchKind.Captures : PcrePolicyMismatchKind.Mark, managed );
	}

	internal static PcreReducedPolicyMismatch Reduce(
		ReadOnlyMemory<byte> pattern, ReadOnlyMemory<byte> subject, int mode, int mask, int start,
		PcrePolicyMismatchKind kind, CandidateCompiler? compiler = null, int maximumEvaluations = 1024,
		CancellationToken cancellationToken = default
	) {
		PcrePolicyMismatch? confirmed = null;
		PcrePolicyMismatch? Evaluate( ReadOnlyMemory<byte> p, ReadOnlyMemory<byte> s, int offset ) {
			cancellationToken.ThrowIfCancellationRequested();
			PcrePolicyDifferentialProbe probe;
			try { probe = new( p, mode, compiler ); }
			catch ( NativePatternRejectedException ) { return null; } // Native-invalid reductions cannot preserve this mismatch.
			return probe.Compare( s, mask, offset, cancellationToken );
		}
		var reduction = PcreFixtureReducer.Reduce<byte>( pattern, subject, start,
			( p, s, offset ) => {
				var result = Evaluate( p, s, offset );
				if ( result?.Kind != kind ) { return false; }
				confirmed = result;
				return true;
			},
			maximumEvaluations, cancellationToken );
		// The reducer already evaluated this exact candidate. Keep the confirmed
		// result from that evaluation rather than spending an uncounted oracle call.
		return new( reduction, confirmed );
	}
}
