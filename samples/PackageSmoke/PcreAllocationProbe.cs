namespace Icod.Expressions.PackageSmoke;

internal sealed record PcreAllocationMetrics( int Iterations, long SmallAllocatedBytes, long LargeAllocatedBytes );

internal static class PcreAllocationProbe {
	internal static PcreAllocationMetrics Measure(
		ReadOnlyMemory<byte> small, ReadOnlyMemory<byte> large, Action<ReadOnlyMemory<byte>> visit,
		int iterations = 8, CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( visit );
		if ( iterations is < 1 or > 32 ) { throw new ArgumentOutOfRangeException( nameof( iterations ) ); }
		if ( large.Length <= small.Length || large.Length > 262144 ) { throw new ArgumentOutOfRangeException( nameof( large ) ); }
		cancellationToken.ThrowIfCancellationRequested();
		for ( var warmup = 0; warmup < 8; warmup++ ) { Warm( small ); Warm( large ); }
		var smallBytes = 0L;
		var largeBytes = 0L;
		for ( var round = 0; round < iterations; round++ ) {
			if ( round % 2 == 0 ) { smallBytes = checked( smallBytes + Count( small ) ); largeBytes = checked( largeBytes + Count( large ) ); }
			else { largeBytes = checked( largeBytes + Count( large ) ); smallBytes = checked( smallBytes + Count( small ) ); }
		}
		return new( iterations, smallBytes, largeBytes );

		void Warm( ReadOnlyMemory<byte> input ) {
			cancellationToken.ThrowIfCancellationRequested();
			visit( input );
			cancellationToken.ThrowIfCancellationRequested();
		}
		long Count( ReadOnlyMemory<byte> input ) {
			cancellationToken.ThrowIfCancellationRequested();
			var before = GC.GetAllocatedBytesForCurrentThread();
			visit( input );
			var after = GC.GetAllocatedBytesForCurrentThread();
			cancellationToken.ThrowIfCancellationRequested();
			return checked( after - before );
		}
	}
}
