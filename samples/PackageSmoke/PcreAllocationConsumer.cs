using System.Globalization;
using System.Text;
using Icod.Expressions.Pcre;

namespace Icod.Expressions.PackageSmoke;

internal sealed record PcreAllocationEvidence( string Name, int SmallInputBytes, int LargeInputBytes, PcreAllocationMetrics Metrics );

internal static class PcreAllocationConsumer {
	internal static PcreAllocationEvidence Run( PcreAllocationProfile profile ) {
		var small = Padded( PcreAllocationCases.Input( profile, PcreAllocationCases.SmallSize ) );
		var large = Padded( PcreAllocationCases.Input( profile, PcreAllocationCases.LargeSize ) );
		var compiled = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( profile.Pattern ), new PcreByteOptions {
			Utf8 = ( profile.Mode & 1 ) != 0, Ucp = ( profile.Mode & 2 ) != 0, Anchored = true
		} );
		if ( !compiled.IsSuccess ) { throw new InvalidOperationException( $"Allocation profile {profile.Name} did not compile." ); }
		var metrics = PcreAllocationProbe.Measure( small.AsMemory( 1, PcreAllocationCases.SmallSize ),
			large.AsMemory( 1, PcreAllocationCases.LargeSize ), Verify, PcreAllocationCases.Iterations );
		if ( metrics.LargeAllocatedBytes - metrics.SmallAllocatedBytes > PcreAllocationCases.FixedAllowance * metrics.Iterations ) {
			throw new InvalidOperationException( $"Allocation profile {profile.Name} exceeded the fixed growth allowance." );
		}
		return new( profile.Name, PcreAllocationCases.SmallSize, PcreAllocationCases.LargeSize, metrics );

		void Verify( ReadOnlyMemory<byte> input ) {
			var result = compiled.Pattern!.Match( input );
			if ( !result.IsSuccess ) { throw new InvalidOperationException( $"Allocation profile {profile.Name} returned a diagnostic." ); }
			var captures = result.Match is { } match ? String.Join( ",", match.Captures.Select( c => c is { } value
				? String.Concat( value.ByteIndex.ToString( CultureInfo.InvariantCulture ), ":", value.ByteLength.ToString( CultureInfo.InvariantCulture ) )
				: "-" ) ) : "";
			if ( captures != profile.Captures ) { throw new InvalidOperationException( $"Allocation profile {profile.Name} changed captures." ); }
		}
	}

	private static byte[] Padded( byte[] input ) {
		var result = new byte[checked( input.Length + 2 )];
		result[0] = 0xFF;
		result[^1] = 0xFE;
		input.AsSpan().CopyTo( result.AsSpan( 1 ) );
		return result;
	}
}
