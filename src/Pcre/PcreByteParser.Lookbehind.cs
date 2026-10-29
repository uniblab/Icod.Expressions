namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private const int MaximumLookbehindLength = 65535;

	private int[] GetFixedLookbehindLengths( PcreByteAlternatives body, int openingOffset ) {
		// Reference keys avoid recursively hashing the record tree. Neither width
		// evaluation nor parser source nesting consumes the native call stack.
		var widths = new Dictionary<PcreByteNode, int>( ReferenceEqualityComparer.Instance );
		var pending = new Stack<(PcreByteNode Node, bool Ready)>();
		void ScheduleBody( PcreByteAlternatives alternatives ) {
			foreach ( var branch in alternatives.Branches ) {
				foreach ( var child in branch ) {
					cancellationToken.ThrowIfCancellationRequested();
					pending.Push( (child, false) );
				}
			}
		}
		int SequenceWidth( IReadOnlyList<PcreByteNode> nodes ) {
			var width = 0;
			foreach ( var node in nodes ) {
				cancellationToken.ThrowIfCancellationRequested();
				width += widths[node];
				if ( width > MaximumLookbehindLength ) { throw Unsupported( openingOffset ); }
			}
			return width;
		}
		int GroupWidth( PcreByteAlternatives alternatives ) {
			int? width = null;
			foreach ( var branch in alternatives.Branches ) {
				var current = SequenceWidth( branch );
				if ( width.HasValue && width.Value != current ) { throw Unsupported( openingOffset ); }
				width = current;
			}
			return width ?? 0;
		}
		int RepeatWidth( PcreByteRepeat repeat ) {
			if ( repeat.Maximum == int.MaxValue
				|| repeat.UnboundedAssertion && repeat.Child is PcreByteLookbehind or PcreByteLiteralAssertion { IsBehind: true } ) {
				throw Unsupported( openingOffset );
			}
			if ( repeat.Maximum == 0 ) { return 0; }
			var child = widths[repeat.Child];
			if ( child != 0 && repeat.Minimum != repeat.Maximum ) { throw Unsupported( openingOffset ); }
			var width = (long)child * repeat.Minimum;
			if ( width > MaximumLookbehindLength ) { throw Unsupported( openingOffset ); }
			return (int)width;
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
				PcreByteLiteral or PcreByteAny or PcreByteClass or PcreByteScalarClass or PcreByteScalarSet => 1,
				PcreByteAnchor or PcreByteSearchStart or PcreByteLineAnchor or PcreByteEndBeforeFinalLf
					or PcreByteWordBoundary or PcreByteLiteralAssertion or PcreByteLookahead or PcreByteLookbehind => 0,
				PcreByteGroup group => GroupWidth( group.Body ),
				PcreByteNoncapturingGroup group => GroupWidth( group.Body ),
				PcreByteAtomic atomic => widths[atomic.Child],
				PcreByteRepeat repeat => RepeatWidth( repeat ),
				_ => throw Unsupported( openingOffset )
			};
		}
		var lengths = new int[body.Branches.Count];
		for ( var branch = 0; branch < lengths.Length; branch++ ) {
			lengths[branch] = SequenceWidth( body.Branches[branch] );
		}
		return lengths;
	}
}
