namespace Icod.Expressions.Pcre;

internal static partial class PcreByteMatcher {
	// Scratch belongs to this Match call, not a candidate, frame or atom. Every
	// evaluation overwrites its live prefix, so backtracking needs no saved stack.
	private static bool ContainsScalarExpression( ReadOnlyMemory<PcreScalarSetTerm> terms, int scalar, bool[] stack,
		ref int work, int matchWorkLimit, Action<int>? observer, bool turkishCasing, CancellationToken cancellationToken ) {
		var count = 0;
		foreach ( var term in terms.Span ) {
			ChargeWork( ref work, matchWorkLimit, observer, cancellationToken );
			if ( term.Operand is { } operand ) {
				var contained = false;
				var folded = operand.Caseless ? PcreUtf8CaseFold.Fold( scalar, operand.CaselessRestricted, turkishCasing ) : scalar;
				foreach ( var range in operand.Ranges.Span ) {
					ChargeWork( ref work, matchWorkLimit, observer, cancellationToken );
					if ( operand.Caseless && folded < range.Start ) { break; }
					if ( folded >= range.Start && folded <= range.End ) { contained = true; break; }
				}
				if ( !contained ) {
					foreach ( var predicate in operand.Predicates.Span ) {
						ChargeWork( ref work, matchWorkLimit, observer, cancellationToken );
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
		return stack[0];
	}
}
