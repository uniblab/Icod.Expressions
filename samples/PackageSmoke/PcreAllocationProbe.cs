namespace Icod.Expressions.PackageSmoke;

internal sealed record PcreAllocationMetrics( int Iterations, long SmallAllocatedBytes, long LargeAllocatedBytes );

internal static class PcreAllocationProbe {
	internal static PcreAllocationMetrics Measure(
		ReadOnlyMemory<byte> small, ReadOnlyMemory<byte> large, Action<ReadOnlyMemory<byte>> visit,
		int iterations = 8, CancellationToken cancellationToken = default
	) => throw new NotImplementedException( "Paired allocation measurement is not implemented yet." );
}
