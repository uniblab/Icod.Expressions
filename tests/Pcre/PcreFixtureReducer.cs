namespace Icod.Expressions.Tests.Pcre;

internal enum PcreReductionStatus { Completed, NotReproduced, EvaluationLimit }

internal sealed record PcreReduction<T>(
	ReadOnlyMemory<T> Pattern, ReadOnlyMemory<T> Subject, int StartOffset, int Evaluations, PcreReductionStatus Status
) where T : unmanaged;

// Test infrastructure only. The supplied oracle decides the mismatch class.
internal static class PcreFixtureReducer {
	internal static PcreReduction<T> Reduce<T>(
		ReadOnlyMemory<T> pattern, ReadOnlyMemory<T> subject, int startOffset,
		Func<ReadOnlyMemory<T>, ReadOnlyMemory<T>, int, bool> reproduces,
		int maximumEvaluations = 1024, CancellationToken cancellationToken = default
	) where T : unmanaged => throw new NotImplementedException( "Bounded differential fixture reducer is not implemented yet." );
}
