namespace Icod.Expressions.Pcre;

/// <summary>Tries source-ordered paths over authoritative bytes with bounded backtracking.</summary>
internal static class PcreByteMatcher {
	private const int MaximumMatchDepth = 256;

	internal static PcreByteMatchResult Match(
		PcreByteAlternatives root,
		int groupCount,
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
		var context = new Context( input, matchWorkLimit, workObserver, cancellationToken );
		try {
			for ( var start = startOffset; start <= input.Length; start++ ) {
				context.Step(); // Candidate start.
				var initial = new State( start, new PcreByteCapture?[groupCount + 1] );
				foreach ( var result in MatchAlternatives( root, initial, context, 0 ) ) {
					var captures = (PcreByteCapture?[])result.Captures.Clone();
					captures[0] = new PcreByteCapture( start, result.Position - start );
					return new(
						new PcreByteMatch( start, result.Position - start, Array.AsReadOnly( captures ) ),
						null
					);
				}
			}
			return new( null, null );
		} catch ( MatchLimitException error ) {
			return new( null, new( error.Code, 0, error.Message ) );
		}
	}

	private static IEnumerable<State> MatchAlternatives(
		PcreByteAlternatives alternatives, State state, Context context, int depth
	) {
		context.CheckDepth( depth );
		foreach ( var branch in alternatives.Branches ) {
			context.Step(); // Attempted alternative.
			foreach ( var result in MatchSequence( branch, 0, state, context, depth + 1 ) ) {
				yield return result;
			}
		}
	}

	private static IEnumerable<State> MatchSequence(
		IReadOnlyList<PcreByteNode> nodes, int index, State state, Context context, int depth
	) {
		context.CheckDepth( depth );
		if ( index >= nodes.Count ) {
			yield return state;
			yield break;
		}
		context.Step(); // Attempted node transition.
		if ( nodes[index] is PcreByteRepeat repeat ) {
			foreach ( var result in MatchRepeat( repeat, 0, state, nodes, index + 1, context, depth + 1 ) ) {
				yield return result;
			}
		} else {
			foreach ( var next in MatchNode( nodes[index], state, context, depth + 1 ) ) {
				foreach ( var result in MatchSequence( nodes, index + 1, next, context, depth + 1 ) ) {
					yield return result;
				}
			}
		}
	}

	private static IEnumerable<State> MatchNode( PcreByteNode node, State state, Context context, int depth ) {
		context.CheckDepth( depth );
		switch ( node ) {
			case PcreByteLiteral literal:
				context.Step(); // Attempted byte comparison.
				if ( state.Position < context.Length && context.ByteAt( state.Position ) == literal.Value ) {
					yield return state with { Position = state.Position + 1 };
				}
				break;
			case PcreByteAnchor anchor:
				if ( state.Position == ( anchor.IsStart ? 0 : context.Length ) ) {
					yield return state;
				}
				break;
			case PcreByteGroup group:
				foreach ( var next in MatchAlternatives( group.Body, state, context, depth + 1 ) ) {
					var captures = (PcreByteCapture?[])next.Captures.Clone();
					captures[group.Number] = new PcreByteCapture( state.Position, next.Position - state.Position );
					yield return new State( next.Position, captures );
				}
				break;
			default:
				throw new InvalidOperationException( "Compiler produced an unknown PCRE byte node." );
		}
	}

	private static IEnumerable<State> MatchRepeat(
		PcreByteRepeat repeat, int count, State state,
		IReadOnlyList<PcreByteNode> nodes, int continuation, Context context, int depth
	) {
		context.CheckDepth( depth );
		IEnumerable<State> Expand() {
			if ( count >= repeat.Maximum ) { yield break; }
			foreach ( var next in MatchNode( repeat.Child, state, context, depth + 1 ) ) {
				context.Step(); // Attempted repetition edge.
				if ( next.Position == state.Position ) {
					// One empty iteration may satisfy the minimum; never revisit this state.
					if ( count + 1 >= repeat.Minimum ) {
						foreach ( var result in MatchSequence( nodes, continuation, next, context, depth + 1 ) ) {
							yield return result;
						}
					}
					continue;
				}
				foreach ( var result in MatchRepeat( repeat, count + 1, next, nodes, continuation, context, depth + 1 ) ) {
					yield return result;
				}
			}
		}
		IEnumerable<State> Continue() {
			if ( count < repeat.Minimum ) { yield break; }
			foreach ( var result in MatchSequence( nodes, continuation, state, context, depth + 1 ) ) {
				yield return result;
			}
		}
		foreach ( var result in repeat.Greedy ? Expand() : Continue() ) { yield return result; }
		foreach ( var result in repeat.Greedy ? Continue() : Expand() ) { yield return result; }
	}

	private readonly record struct State( int Position, PcreByteCapture?[] Captures );

	private sealed class Context(
		ReadOnlyMemory<byte> input, int workLimit, Action<int>? workObserver, CancellationToken cancellationToken
	) {
		private int work;
		internal int Length => input.Length;
		internal byte ByteAt( int index ) => input.Span[index];

		internal void Step() {
			cancellationToken.ThrowIfCancellationRequested();
			if ( work >= workLimit ) {
				throw new MatchLimitException( PcreByteDiagnosticCode.MatchWorkLimitExceeded, "match work limit exceeded" );
			}
			work++;
			workObserver?.Invoke( work );
			cancellationToken.ThrowIfCancellationRequested();
		}

		internal void CheckDepth( int depth ) {
			if ( depth > MaximumMatchDepth ) {
				throw new MatchLimitException( PcreByteDiagnosticCode.MatchDepthLimitExceeded, "match depth limit exceeded" );
			}
		}
	}

	private sealed class MatchLimitException( PcreByteDiagnosticCode code, string message ) : Exception( message ) {
		internal PcreByteDiagnosticCode Code { get; } = code;
	}
}
