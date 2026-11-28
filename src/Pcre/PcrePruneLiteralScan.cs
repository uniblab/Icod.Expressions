namespace Icod.Expressions.Pcre;

// The native JIT can reuse a leading literal-plus scan on retry after PRUNE.
internal static class PcrePruneLiteralScan {
	internal static (PcreByteAlternatives Body, PcreByteRepeat Repeat)? Find(
		PcreByteAlternatives root, CancellationToken cancellationToken
	) {
		cancellationToken.ThrowIfCancellationRequested();
		for ( var depth = 0; depth < 4 && root.Branches.Count == 1; depth++ ) {
			var nested = SingleNonReset( root.Branches[0], cancellationToken ) switch {
				PcreByteGroup group => group.Body,
				PcreByteNoncapturingGroup group => group.Body,
				_ => null
			};
			if ( nested is null ) { break; }
			root = nested;
		}
		if ( root.Branches.Count != 2 ) { return null; }

		// Native early-failure analysis skips match-start resets as zero-width nodes.
		// They still execute normally. Only the leading repeat uses the cached scan.
		var first = new List<PcreByteNode>( 4 );
		foreach ( var node in root.Branches[0] ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( node is PcreByteMatchStartReset ) { continue; }
			if ( first.Count == 4 ) { return null; }
			first.Add( node );
		}
		if ( first.Count is not ( 3 or 4 )
			|| first[0] is not PcreByteRepeat { Minimum: 1, Maximum: int.MaxValue,
				Greedy: true, Offset: -1, Child: PcreByteLiteral { Caseless: false } literal } repeat
			|| first.Count == 4 && first[1] is not ( PcreByteCommit or PcreByteThen or PcreByteSkip or PcreByteNamedSkip or PcreBytePathMark )
			|| first[^2] is not PcreBytePrune
			|| first[^1] is not ( PcreByteFailure or PcreByteLiteral { Caseless: false } ) ) { return null; }
		var fallback = SingleNonReset( root.Branches[1], cancellationToken );
		if ( fallback is PcreByteGroup { Body.Branches.Count: 1 } fallbackGroup ) {
			fallback = SingleNonReset( fallbackGroup.Body.Branches[0], cancellationToken );
		}
		return fallback is PcreByteLiteral { Caseless: false } other && other.Value == literal.Value ? (root, repeat) : null;
	}

	private static PcreByteNode? SingleNonReset( IReadOnlyList<PcreByteNode> nodes, CancellationToken cancellationToken ) {
		PcreByteNode? result = null;
		foreach ( var node in nodes ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( node is PcreByteMatchStartReset ) { continue; }
			if ( result is not null ) { return null; }
			result = node;
		}
		return result;
	}
}
