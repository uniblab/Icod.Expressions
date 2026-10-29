namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private const int MaximumLookbehindLength = 65535;

	private readonly record struct LookbehindWidth( int Minimum, int Maximum );

	private PcreByteLookbehind FinishLookbehind( PcreByteAlternatives body, bool positive, int openingOffset,
		LookbehindContext context ) {
		// Width edges and nested assertion validation share the active dependency
		// chain. Reference keys avoid recursively hashing the record tree.
		var widths = new Dictionary<PcreByteNode, LookbehindWidth>( ReferenceEqualityComparer.Instance );
		var pending = new Stack<(PcreByteNode Node, bool Ready, int Opening)>();
		var active = new HashSet<PcreByteNode>( ReferenceEqualityComparer.Instance );
		void ScheduleBody( PcreByteAlternatives alternatives, int opening ) {
			for ( var branch = alternatives.Branches.Count - 1; branch >= 0; branch-- ) {
				var nodes = alternatives.Branches[branch];
				for ( var index = nodes.Count - 1; index >= 0; index-- ) {
					cancellationToken.ThrowIfCancellationRequested();
					pending.Push( (nodes[index], false, opening) );
				}
			}
		}
		void ScheduleNestedBehinds( PcreByteAlternatives alternatives ) {
			// Native lookahead width analysis ignores ordinary references but
			// checks every nested behind with the current capture dependency chain.
			var scan = new Stack<(PcreByteNode Node, bool Ready)>();
			PushTreeBody( scan, alternatives );
			while ( scan.TryPop( out var item ) ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( item.Node is PcreByteLookbehind assertion ) {
					pending.Push( (assertion, false, assertion.OpeningOffset) );
				} else { PushTreeChildren( scan, item.Node ); }
			}
		}
		PcreByteGroup ReferenceTarget( PcreByteBackreference reference, int opening ) {
			if ( hasBranchReset || !reference.DuplicateNumbers.IsEmpty
				|| !context.Captures.TryGetValue( reference.Number, out var capture ) ) {
				throw Unsupported( opening );
			}
			var position = context.Positions[reference];
			if ( position > capture.Start && position < capture.End ) { throw Unsupported( opening ); }
			return capture.Group;
		}
		LookbehindWidth CheckedWidth( long minimum, long maximum, int opening ) {
			if ( maximum > MaximumLookbehindLength ) { throw Unsupported( opening ); }
			return new( (int)minimum, (int)maximum );
		}
		LookbehindWidth SequenceWidth( IReadOnlyList<PcreByteNode> nodes, int opening ) {
			var width = new LookbehindWidth( 0, 0 );
			foreach ( var node in nodes ) {
				cancellationToken.ThrowIfCancellationRequested();
				var child = widths[node];
				width = CheckedWidth( (long)width.Minimum + child.Minimum, (long)width.Maximum + child.Maximum, opening );
			}
			return width;
		}
		LookbehindWidth GroupWidth( PcreByteAlternatives alternatives, int opening ) {
			var minimum = int.MaxValue;
			var maximum = 0;
			foreach ( var branch in alternatives.Branches ) {
				var current = SequenceWidth( branch, opening );
				minimum = Math.Min( minimum, current.Minimum );
				maximum = Math.Max( maximum, current.Maximum );
			}
			return new( minimum == int.MaxValue ? 0 : minimum, maximum );
		}
		LookbehindWidth RepeatWidth( PcreByteRepeat repeat, int opening ) {
			if ( repeat.Maximum == int.MaxValue
				|| repeat.UnboundedAssertion && repeat.Child is PcreByteLookbehind or PcreByteLiteralAssertion { IsBehind: true } ) {
				throw Unsupported( opening );
			}
			var child = widths[repeat.Child];
			return CheckedWidth( (long)child.Minimum * repeat.Minimum, (long)child.Maximum * repeat.Maximum, opening );
		}
		(int[] Lengths, int Minimum) BranchBounds( PcreByteAlternatives alternatives, int opening ) {
			var lengths = new int[alternatives.Branches.Count];
			var minimum = int.MaxValue;
			var maximum = 0;
			var variable = false;
			for ( var branch = 0; branch < lengths.Length; branch++ ) {
				var width = SequenceWidth( alternatives.Branches[branch], opening );
				lengths[branch] = width.Maximum;
				minimum = Math.Min( minimum, width.Minimum );
				maximum = Math.Max( maximum, width.Maximum );
				variable |= width.Minimum != width.Maximum;
			}
			if ( variable && maximum > 255 ) { throw Unsupported( opening ); }
			return (lengths, variable ? minimum : -1);
		}
		LookbehindWidth ValidateNestedBehind( PcreByteLookbehind assertion ) {
			BranchBounds( assertion.Body, assertion.OpeningOffset );
			return new( 0, 0 );
		}

		ScheduleBody( body, openingOffset );
		while ( pending.TryPop( out var item ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( widths.ContainsKey( item.Node ) ) { continue; }
			if ( !item.Ready ) {
				if ( !active.Add( item.Node ) ) { throw Unsupported( item.Opening ); }
				pending.Push( (item.Node, true, item.Opening) );
				switch ( item.Node ) {
					case PcreByteGroup group: ScheduleBody( group.Body, item.Opening ); break;
					case PcreByteBackreference reference:
						pending.Push( (ReferenceTarget( reference, item.Opening ), false, item.Opening) ); break;
					case PcreByteSubroutineCall call:
						pending.Push( (context.CallTargets[call.Number], false, item.Opening) ); break;
					case PcreByteNoncapturingGroup group: ScheduleBody( group.Body, item.Opening ); break;
					case PcreByteCaptureConditional conditional: ScheduleBody( conditional.Body, item.Opening ); break;
					case PcreByteRecursionConditional conditional: ScheduleBody( conditional.Body, item.Opening ); break;
					// Native consumed-width scans skip DEFINE completely. A lookahead's
					// global nested-behind scan still traverses it through PushTreeChildren.
					case PcreByteStaticConditional { IsDefinition: true }: break;
					case PcreByteStaticConditional conditional: ScheduleBody( conditional.Body, item.Opening ); break;
					case PcreByteAssertionConditional conditional:
						ScheduleBody( conditional.Body, item.Opening );
						pending.Push( (conditional.Assertion, false, item.Opening) );
						break;
					case PcreByteLookbehind assertion: ScheduleBody( assertion.Body, assertion.OpeningOffset ); break;
					case PcreByteLookahead assertion: ScheduleNestedBehinds( assertion.Body ); break;
					case PcreByteAtomic atomic: pending.Push( (atomic.Child, false, item.Opening) ); break;
					// Erased children still validate before applying their zero count.
					case PcreByteRepeat repeat: pending.Push( (repeat.Child, false, item.Opening) ); break;
				}
				continue;
			}
			widths[item.Node] = item.Node switch {
				PcreByteLiteral or PcreByteAny or PcreByteClass or PcreByteScalarClass or PcreByteScalarSet => new( 1, 1 ),
				PcreByteNewlineSequence => new( 1, 2 ),
				PcreByteAnchor or PcreByteSearchStart or PcreByteMatchStartReset or PcreByteLineAnchor or PcreByteEndBeforeFinalLf
					or PcreByteWordBoundary or PcreByteLiteralAssertion or PcreByteLookahead => new( 0, 0 ),
				PcreByteLookbehind assertion => ValidateNestedBehind( assertion ),
				PcreByteGroup group => GroupWidth( group.Body, item.Opening ),
				PcreByteBackreference reference => widths[ReferenceTarget( reference, item.Opening )],
				PcreByteSubroutineCall call => widths[context.CallTargets[call.Number]],
				PcreByteNoncapturingGroup group => GroupWidth( group.Body, item.Opening ),
				// Native width analysis sees explicit branches only, even without an else.
				PcreByteCaptureConditional conditional => GroupWidth( conditional.Body, item.Opening ),
				PcreByteRecursionConditional conditional => GroupWidth( conditional.Body, item.Opening ),
				PcreByteStaticConditional { IsDefinition: true } => new( 0, 0 ),
				PcreByteStaticConditional conditional => GroupWidth( conditional.Body, item.Opening ),
				PcreByteAssertionConditional conditional => GroupWidth( conditional.Body, item.Opening ),
				PcreByteAtomic atomic => widths[atomic.Child],
				PcreByteRepeat repeat => RepeatWidth( repeat, item.Opening ),
				_ => throw Unsupported( item.Opening )
			};
			active.Remove( item.Node );
		}
		var bounds = BranchBounds( body, openingOffset );
		return new( body, positive, bounds.Lengths, bounds.Minimum, openingOffset );
	}
}
