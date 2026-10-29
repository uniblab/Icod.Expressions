namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private const int MaximumLookbehindLength = 65535;

	private readonly record struct LookbehindWidth( int Minimum, int Maximum );

	private PcreByteLookbehind FinishLookbehind( PcreByteAlternatives body, bool positive, int openingOffset ) {
		// Reference keys avoid recursively hashing the record tree. Neither width
		// evaluation nor parser source nesting consumes the native call stack.
		var widths = new Dictionary<PcreByteNode, LookbehindWidth>( ReferenceEqualityComparer.Instance );
		var pending = new Stack<(PcreByteNode Node, bool Ready)>();
		void ScheduleBody( PcreByteAlternatives alternatives ) {
			foreach ( var branch in alternatives.Branches ) {
				foreach ( var child in branch ) {
					cancellationToken.ThrowIfCancellationRequested();
					pending.Push( (child, false) );
				}
			}
		}
		LookbehindWidth CheckedWidth( long minimum, long maximum ) {
			if ( maximum > MaximumLookbehindLength ) { throw Unsupported( openingOffset ); }
			return new( (int)minimum, (int)maximum );
		}
		LookbehindWidth SequenceWidth( IReadOnlyList<PcreByteNode> nodes ) {
			var width = new LookbehindWidth( 0, 0 );
			foreach ( var node in nodes ) {
				cancellationToken.ThrowIfCancellationRequested();
				var child = widths[node];
				width = CheckedWidth( (long)width.Minimum + child.Minimum, (long)width.Maximum + child.Maximum );
			}
			return width;
		}
		LookbehindWidth GroupWidth( PcreByteAlternatives alternatives ) {
			var minimum = int.MaxValue;
			var maximum = 0;
			foreach ( var branch in alternatives.Branches ) {
				var current = SequenceWidth( branch );
				minimum = Math.Min( minimum, current.Minimum );
				maximum = Math.Max( maximum, current.Maximum );
			}
			return new( minimum == int.MaxValue ? 0 : minimum, maximum );
		}
		LookbehindWidth RepeatWidth( PcreByteRepeat repeat ) {
			if ( repeat.Maximum == int.MaxValue
				|| repeat.UnboundedAssertion && repeat.Child is PcreByteLookbehind or PcreByteLiteralAssertion { IsBehind: true } ) {
				throw Unsupported( openingOffset );
			}
			var child = widths[repeat.Child];
			return CheckedWidth( (long)child.Minimum * repeat.Minimum, (long)child.Maximum * repeat.Maximum );
		}

		ScheduleBody( body );
		while ( pending.TryPop( out var item ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( widths.ContainsKey( item.Node ) ) { continue; }
			if ( !item.Ready ) {
				pending.Push( (item.Node, true) );
				switch ( item.Node ) {
					case PcreByteGroup group: ScheduleBody( group.Body ); break;
					case PcreByteNoncapturingGroup group: ScheduleBody( group.Body ); break;
					case PcreByteAtomic atomic: pending.Push( (atomic.Child, false) ); break;
					// Validate erased children too: an outer {0} must not hide
					// an unbounded group from the native-compatible width policy.
					case PcreByteRepeat repeat: pending.Push( (repeat.Child, false) ); break;
				}
				continue;
			}
			widths[item.Node] = item.Node switch {
				PcreByteLiteral or PcreByteAny or PcreByteClass or PcreByteScalarClass or PcreByteScalarSet => new( 1, 1 ),
				PcreByteNewlineSequence => new( 1, 2 ),
				PcreByteAnchor or PcreByteSearchStart or PcreByteLineAnchor or PcreByteEndBeforeFinalLf
					or PcreByteWordBoundary or PcreByteLiteralAssertion or PcreByteLookahead or PcreByteLookbehind => new( 0, 0 ),
				PcreByteGroup group => GroupWidth( group.Body ),
				PcreByteNoncapturingGroup group => GroupWidth( group.Body ),
				PcreByteAtomic atomic => widths[atomic.Child],
				PcreByteRepeat repeat => RepeatWidth( repeat ),
				_ => throw Unsupported( openingOffset )
			};
		}
		var lengths = new int[body.Branches.Count];
		var minimum = int.MaxValue;
		var maximum = 0;
		var variable = false;
		for ( var branch = 0; branch < lengths.Length; branch++ ) {
			var width = SequenceWidth( body.Branches[branch] );
			lengths[branch] = width.Maximum;
			minimum = Math.Min( minimum, width.Minimum );
			maximum = Math.Max( maximum, width.Maximum );
			variable |= width.Minimum != width.Maximum;
		}
		if ( variable && maximum > 255 ) { throw Unsupported( openingOffset ); }
		return new( body, positive, lengths, variable ? minimum : -1 );
	}
}
