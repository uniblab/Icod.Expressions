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
	) where T : unmanaged {
		ArgumentNullException.ThrowIfNull( reproduces );
		if ( pattern.Length > 4096 ) { throw new ArgumentOutOfRangeException( nameof( pattern ) ); }
		if ( subject.Length > 4096 ) { throw new ArgumentOutOfRangeException( nameof( subject ) ); }
		if ( startOffset < 0 || startOffset > subject.Length ) { throw new ArgumentOutOfRangeException( nameof( startOffset ) ); }
		if ( maximumEvaluations is <= 0 or > 65536 ) { throw new ArgumentOutOfRangeException( nameof( maximumEvaluations ) ); }
		cancellationToken.ThrowIfCancellationRequested();
		var currentPattern = pattern.ToArray();
		var currentSubject = subject.ToArray();
		var evaluations = 0;

		bool Evaluate( T[] p, T[] s ) {
			cancellationToken.ThrowIfCancellationRequested();
			evaluations++;
			var accepted = reproduces( p, s, startOffset );
			cancellationToken.ThrowIfCancellationRequested();
			return accepted;
		}
		PcreReduction<T> Finish( PcreReductionStatus status ) =>
			new( currentPattern, currentSubject, startOffset, evaluations, status );

		if ( !Evaluate( currentPattern, currentSubject ) ) { return Finish( PcreReductionStatus.NotReproduced ); }
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			var changed = false;
			for ( var field = 0; field < 2; field++ ) {
				var values = field == 0 ? currentPattern : currentSubject;
				// Keep checking the same position after acceptance: the following
				// chunk has moved here. Finer chunks finish with every single unit.
				for ( var chunk = values.Length; chunk > 0; chunk = chunk == 1 ? 0 : ( chunk + 1 ) / 2 ) {
					var offset = 0;
					while ( offset < values.Length ) {
						cancellationToken.ThrowIfCancellationRequested();
						var count = Math.Min( chunk, values.Length - offset );
						if ( field == 1 && values.Length - count < startOffset ) { offset += chunk; continue; }
						if ( evaluations >= maximumEvaluations ) { return Finish( PcreReductionStatus.EvaluationLimit ); }
						var candidate = Remove( values, offset, count );
						var accepted = field == 0 ? Evaluate( candidate, currentSubject ) : Evaluate( currentPattern, candidate );
						if ( !accepted ) { offset += chunk; continue; }
						values = candidate;
						if ( field == 0 ) { currentPattern = candidate; }
						else { currentSubject = candidate; }
						changed = true;
					}
				}
			}
			// Changing the subject can unlock a pattern reduction (and vice versa).
			// A complete unchanged pass proves one-deletion local minimality.
			if ( !changed ) { return Finish( PcreReductionStatus.Completed ); }
		}
	}

	private static T[] Remove<T>( T[] values, int offset, int count ) {
		var result = new T[values.Length - count];
		values.AsSpan( 0, offset ).CopyTo( result );
		values.AsSpan( offset + count ).CopyTo( result.AsSpan( offset ) );
		return result;
	}
}
