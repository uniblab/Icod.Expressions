namespace Icod.Expressions.Pcre;

// Shared source proof used only to stop interpreted named-SKIP replay.
internal static class PcreSourceStartAnchor {
	private const int MaximumInstructions = 4096;

	internal static bool HasAlternativeStartAnchor( PcreByteAlternatives root, CancellationToken cancellationToken ) {
		cancellationToken.ThrowIfCancellationRequested();
		var entries = new Stack<(PcreByteAlternatives Body, int First, int Count)>();
		entries.Push( (root, 0, root.Branches.Count) );
		var visits = 0;
		while ( entries.TryPop( out var entry ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			// Each leading node already consumes a visit during compilation.
			if ( entry.Count == 0 || entry.Count > MaximumInstructions - visits - entries.Count ) { return false; }
			for ( var index = entry.First; index < entry.First + entry.Count; index++ ) {
				var branch = entry.Body.Branches[index];
				var first = 0;
				PcreByteNode leading;
				// Native advances past these prefixes to find the first significant opcode.
				do {
					cancellationToken.ThrowIfCancellationRequested();
					if ( first == branch.Count || visits >= MaximumInstructions - entries.Count ) { return false; }
					visits++;
					leading = branch[first++];
				} while ( leading is PcreByteRepeat { Maximum: 0 }
					or PcreByteAtomic { Child: PcreByteRepeat { Maximum: 0 } }
					or PcreByteStaticConditional { Value: false, Body.Branches.Count: 1 }
					or PcreByteRepeat { Child: PcreByteStaticConditional { Value: false, Body.Branches.Count: 1 } }
					or PcreBytePathMark or PcreByteNamedSkip
					or PcreByteCommit { Name.IsEmpty: false }
					or PcreBytePrune { Name.IsEmpty: false }
					or PcreByteThen { Name.IsEmpty: false } );
				if ( leading is PcreByteAtomic { Child: PcreByteRepeat { Minimum: > 0 } possessive } ) { leading = possessive; }
				if ( leading is PcreByteRepeat { Minimum: > 0 } repeat ) { leading = repeat.Child; }
				if ( leading is PcreByteAssertionConditional {
					Body.Branches.Count: 2,
					Assertion: PcreByteLookahead { IsPositive: true, NonAtomic: false } guard
				} assertionCondition ) {
					if ( entries.Count > MaximumInstructions - visits - 2 ) { return false; }
					// Native analyzes the guard and else; the then body does not establish this evidence.
					entries.Push( (assertionCondition.Body, 1, 1) );
					entries.Push( (guard.Body, 0, guard.Body.Branches.Count) );
					continue;
				}
				var nested = leading switch {
					PcreByteGroup { Number: > 0 } capture => capture.Body,
					PcreByteNoncapturingGroup group => group.Body,
					PcreByteAtomic { Child: PcreByteNoncapturingGroup group } => group.Body,
					PcreByteLookahead { IsPositive: true } assertion => assertion.Body,
					PcreByteCaptureConditional { Body.Branches.Count: 2 } condition => condition.Body,
					PcreByteRecursionConditional { Body.Branches.Count: 2 } condition => condition.Body,
					PcreByteStaticConditional { IsDefinition: false, Body.Branches.Count: 2 } condition => condition.Body,
					_ => null
				};
				if ( nested is not null ) {
					if ( entries.Count >= MaximumInstructions - visits ) { return false; }
					entries.Push( (nested, 0, nested.Branches.Count) );
					continue;
				}
				// NotBol affects execution. Native retains caret's source anchoring.
				if ( leading is not PcreByteAnchor { IsStart: true } && leading is not PcreByteSearchStart ) { return false; }
			}
		}
		return true;
	}

}
