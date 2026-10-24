namespace Icod.Expressions.Pcre;

/// <summary>Searches byte literals in candidate-start and alternative order.</summary>
internal static class PcreByteMatcher {
	internal static PcreByteMatchResult Match(
		byte[][] alternatives,
		ReadOnlyMemory<byte> input,
		int startOffset,
		int matchWorkLimit,
		Action<int>? workObserver,
		CancellationToken cancellationToken
	) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( startOffset < 0 || startOffset > input.Length ) {
			return new( null, new(
				PcreByteDiagnosticCode.InvalidStartOffset,
				startOffset,
				"start byte offset is outside the input"
			) );
		}
		var source = input.Span;
		var work = 0;
		bool TryStep() {
			cancellationToken.ThrowIfCancellationRequested();
			if ( work >= matchWorkLimit ) {
				return false;
			}
			work++;
			workObserver?.Invoke( work );
			cancellationToken.ThrowIfCancellationRequested();
			return true;
		}
		PcreByteMatchResult LimitExceeded() => new( null, new(
			PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			0,
			"match work limit exceeded"
		) );

		for ( var start = startOffset; start <= source.Length; start++ ) {
			if ( !TryStep() ) {
				return LimitExceeded();
			}
			foreach ( var alternative in alternatives ) {
				if ( !TryStep() ) {
					return LimitExceeded();
				}
				if ( alternative.Length > source.Length - start ) {
					continue;
				}
				var matches = true;
				for ( var index = 0; index < alternative.Length; index++ ) {
					if ( !TryStep() ) {
						return LimitExceeded();
					}
					if ( source[start + index] == alternative[index] ) {
						continue;
					}
					matches = false;
					break;
				}
				if ( matches ) {
					return new( new PcreByteMatch( start, alternative.Length ), null );
				}
			}
		}
		return new( null, null );
	}
}
