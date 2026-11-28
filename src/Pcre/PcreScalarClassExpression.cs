namespace Icod.Expressions.Pcre;

// Iterative postfix evaluation over a decoded scalar or raw word unit.
internal static class PcreScalarClassExpression {
	// Scratch belongs to a Match call. Each evaluation overwrites its live
	// prefix, so candidates, frames and backtracking retain no Boolean state.
	// Negative: work exhausted; zero: excluded; positive: included.
	internal static int Evaluate( ReadOnlyMemory<PcreScalarSetTerm> terms, int scalar, bool[] stack,
		bool unicodeFolding, bool turkishCasing, ref int work, int matchWorkLimit,
		Action<int>? observer, CancellationToken cancellationToken ) {
		var count = 0;
		foreach ( var term in terms.Span ) {
			if ( !Charge( ref work, matchWorkLimit, observer, cancellationToken ) ) { return -1; }
			if ( term.Operand is { } operand ) {
				var contained = false;
				var folded = operand.Caseless && unicodeFolding ? PcreUtf8CaseFold.Fold( scalar, operand.CaselessRestricted, turkishCasing ) : scalar;
				var opposite = operand.Caseless && !unicodeFolding ? AsciiOpposite( scalar ) : folded;
				foreach ( var range in operand.Ranges.Span ) {
					if ( !Charge( ref work, matchWorkLimit, observer, cancellationToken ) ) { return -1; }
					if ( operand.Caseless && unicodeFolding && folded < range.Start ) { break; }
					if ( folded >= range.Start && folded <= range.End || opposite >= range.Start && opposite <= range.End ) { contained = true; break; }
				}
				if ( !contained ) {
					foreach ( var predicate in operand.Predicates.Span ) {
						if ( !Charge( ref work, matchWorkLimit, observer, cancellationToken ) ) { return -1; }
						if ( PcreScalarClassMembership.Contains( predicate.Kind, scalar, predicate.Category, operand.Caseless, predicate.PropertyValue ) != predicate.Negated ) {
							contained = true; break;
						}
					}
				}
				stack[count++] = contained != operand.Negated;
				continue;
			}
			if ( term.Operation == PcreScalarSetOperation.Not ) { stack[count - 1] = !stack[count - 1]; continue; }
			var right = stack[--count];
			var left = stack[count - 1];
			stack[count - 1] = term.Operation switch {
				PcreScalarSetOperation.And => left && right,
				PcreScalarSetOperation.Or => left || right,
				PcreScalarSetOperation.Subtract => left && !right,
				PcreScalarSetOperation.Xor => left != right,
				_ => throw new InvalidOperationException( "Unknown scalar class operator." )
			};
		}
		return stack[0] ? 1 : 0;
	}

	private static int AsciiOpposite( int value ) => value is >= 'A' and <= 'Z' ? value + 32
		: value is >= 'a' and <= 'z' ? value - 32 : value;

	private static bool Charge( ref int work, int limit, Action<int>? observer, CancellationToken cancellationToken ) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( work >= limit ) { return false; }
		work++;
		observer?.Invoke( work );
		cancellationToken.ThrowIfCancellationRequested();
		return true;
	}
}
