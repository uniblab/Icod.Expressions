namespace Icod.Expressions.PackageSmoke;

internal sealed record PcreAllocationEvidence( string Name, int SmallInputBytes, int LargeInputBytes, PcreAllocationMetrics Metrics );

internal static class PcreAllocationConsumer {
	internal static PcreAllocationEvidence Run( PcreAllocationProfile profile ) =>
		throw new NotImplementedException( "PCRE allocation consumer is not implemented yet." );
}
