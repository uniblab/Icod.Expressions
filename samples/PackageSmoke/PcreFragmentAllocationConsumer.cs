using System.Globalization;
using System.Text;
using Icod.Expressions.Pcre;

namespace Icod.Expressions.PackageSmoke;

internal static class PcreFragmentAllocationConsumer {
	internal static PcreAllocationEvidence Run( PcreFragmentAllocationProfile profile, int path = 0 ) {
		var small = PcreFragmentAllocationCases.Input( PcreAllocationCases.SmallSize );
		var large = PcreFragmentAllocationCases.Input( PcreAllocationCases.LargeSize );
		using var source = new CancellationTokenSource();
		var token = path == 2 ? source.Token : default;
		var compiled = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( profile.Pattern ), new PcreByteOptions {
			MatchInvalidUtf = true, Anchored = profile.Anchored, WorkObserver = path == 1 ? _ => { } : null
		} );
		if ( !compiled.IsSuccess ) { throw new InvalidOperationException( $"Fragment profile {profile.Name} did not compile." ); }
		var metrics = PcreAllocationProbe.Measure( small.AsMemory( 1, PcreAllocationCases.SmallSize ),
			large.AsMemory( 1, PcreAllocationCases.LargeSize ), Verify, PcreAllocationCases.Iterations );
		return new( profile.Name, PcreAllocationCases.SmallSize, PcreAllocationCases.LargeSize, metrics );

		void Verify( ReadOnlyMemory<byte> input ) {
			var result = compiled.Pattern!.Match( input, PcreFragmentAllocationCases.Offset( profile, input.Length ), token );
			if ( !result.IsSuccess ) { throw new InvalidOperationException( $"Fragment profile {profile.Name}: {result.Diagnostic?.Code}." ); }
			var captures = result.Match is { } match ? String.Join( ",", match.Captures.Select( c => c is { } value
				? String.Concat( value.ByteIndex.ToString( CultureInfo.InvariantCulture ), ":", value.ByteLength.ToString( CultureInfo.InvariantCulture ) )
				: "-" ) ) : "";
			var expected = input.Length == PcreAllocationCases.SmallSize ? profile.SmallCaptures : profile.LargeCaptures;
			if ( captures != expected ) { throw new InvalidOperationException( $"Fragment profile {profile.Name} changed captures: {captures}." ); }
		}
	}
}
