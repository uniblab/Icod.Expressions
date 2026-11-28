namespace Icod.Expressions.Pcre;

internal readonly record struct PcreWordPrefix( int Width, ReadOnlyMemory<PcreByteLiteral> Literals, bool RequiresComparison );

// Bounded source proofs for native width-16 search entry around CRLF.
internal static class PcreWordStartStudy {
	private const int MaximumVisits = 1000;

	private readonly record struct First( bool Known, bool Empty, bool Cr, bool Lf );

	internal static bool HasLfStartClass( PcreByteAlternatives root, IEnumerable<PcreByteAlternatives> bodies,
		CancellationToken cancellationToken ) {
		var known = new Dictionary<PcreByteAlternatives, First>( ReferenceEqualityComparer.Instance );
		var visits = 0;
		foreach ( var body in bodies ) {
			var complete = new First( true, false, false, false );
			foreach ( var branch in body.Branches ) {
				var first = new First( true, true, false, false );
				foreach ( var node in branch ) {
					cancellationToken.ThrowIfCancellationRequested();
					if ( ++visits > MaximumVisits ) { return false; }
					var next = FirstNode( node );
					first = new( first.Known && next.Known, next.Empty, first.Cr || next.Cr, first.Lf || next.Lf );
					if ( !first.Empty ) { break; }
				}
				complete = new( complete.Known && first.Known, complete.Empty || first.Empty,
					complete.Cr || first.Cr, complete.Lf || first.Lf );
			}
			known.Add( body, complete );
		}
		var result = known[root];
		return result.Known && !result.Empty && !result.Cr && result.Lf;

		First FirstNode( PcreByteNode node ) {
			var optional = false;
			while ( node is PcreByteAtomic or PcreByteRepeat or PcreByteScriptRun ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( ++visits > MaximumVisits ) { return default; }
				if ( node is PcreByteRepeat repeat ) {
					if ( repeat.Maximum == 0 ) { return new( true, true, false, false ); }
					optional |= repeat.Minimum == 0;
					node = repeat.Child;
				} else { node = node is PcreByteAtomic atomic ? atomic.Child : ( (PcreByteScriptRun)node ).Child; }
			}
			var first = node switch {
				PcreByteLiteral value => new First( true, false, value.Value == '\r', value.Value == '\n' ),
				PcreByteGroup group => known[group.Body],
				PcreByteNoncapturingGroup group => known[group.Body],
				PcreByteScalarSet { Negated: false, Predicates.IsEmpty: true } set => FirstSet( set ),
				_ => default
			};
			return first with { Empty = first.Empty || optional };
		}

		First FirstSet( PcreByteScalarSet set ) {
			var cr = false;
			var lf = false;
			foreach ( var range in set.Ranges.Span ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( ++visits > MaximumVisits ) { return default; }
				cr |= range.Start <= '\r' && range.End >= '\r';
				lf |= range.Start <= '\n' && range.End >= '\n';
			}
			return new( true, false, cr, lf );
		}
	}

	internal static PcreWordPrefix? FindFixedPrefix( PcreByteAlternatives root, bool utf, CancellationToken cancellationToken ) {
		var pending = new Stack<PcreByteNode>();
		var visits = 0;
		var width = 0;
		PcreByteLiteral? literal = null;
		var restricted = false;
		if ( !AddBody( root ) ) { return null; }
		while ( pending.TryPop( out var node ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( ++visits > MaximumVisits ) { return null; }
			switch ( node ) {
				case PcreByteGroup group when width > 0 && literal is null && group.Body.Branches.Count > 1 && pending.Count == 0:
					return FinishAlternatives( group.Body );
				case PcreByteGroup group:
					if ( !AddBody( group.Body ) ) { return null; }
					break;
				case PcreByteNoncapturingGroup group when width > 0 && literal is null && group.Body.Branches.Count > 1 && pending.Count == 0:
					return FinishAlternatives( group.Body );
				case PcreByteNoncapturingGroup group:
					if ( !AddBody( group.Body ) ) { return null; }
					break;
				case PcreByteAtomic atomic: pending.Push( atomic.Child ); break;
				case PcreByteRepeat { Minimum: > 0 } repeat when repeat.Minimum == repeat.Maximum && repeat.Minimum < 12:
					for ( var copy = 0; copy < repeat.Minimum; copy++ ) { pending.Push( repeat.Child ); }
					break;
				case PcreByteLiteral value when width > 0 && literal is null: literal = value; break;
				case PcreByteAny any when !utf && literal is null: width++; restricted |= !any.DotAll; break;
				case PcreByteScalarClass { Kind: PcreByteScalarClassKind.AsciiSpace, Negated: false } when literal is null:
					width++; restricted = true; break;
				case PcreByteScalarClass { Kind: PcreByteScalarClassKind.UnicodeSpace, Negated: false } when !utf && literal is null:
					width++; restricted = true; break;
				case PcreByteScalarSet { Negated: false, Ranges.IsEmpty: true, Predicates.Length: 1 } set
					when literal is null && set.Predicates.Span[0] is { Negated: false } predicate
						&& ( predicate.Kind == PcreByteScalarClassKind.AsciiSpace
							|| !utf && predicate.Kind == PcreByteScalarClassKind.UnicodeSpace ):
					width++; restricted = true;
					break;
				case PcreByteScalarSet { Negated: true, Ranges.Length: 1, Predicates.IsEmpty: true, SourceLiteral: >= 0 } when !utf && literal is null:
					width++; restricted = true;
					break;
				default: return null;
			}
			if ( width >= 12 || pending.Count > MaximumVisits - visits ) { return null; }
		}
		return literal is null ? null : new( width, new[] { literal }, restricted && width > 1 );

		PcreWordPrefix? FinishAlternatives( PcreByteAlternatives body ) {
			var literals = new List<PcreByteLiteral>();
			foreach ( var branch in body.Branches ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( ++visits > MaximumVisits || branch.Count != 1 || branch[0] is not PcreByteLiteral { Caseless: false } value ) { return null; }
				literals.Add( value );
			}
			return new( width, literals.ToArray(), restricted && width > 1 );
		}

		bool AddBody( PcreByteAlternatives body ) {
			if ( body.Branches.Count != 1 || body.Branches[0].Count > MaximumVisits - pending.Count - visits ) { return false; }
			var branch = body.Branches[0];
			for ( var index = branch.Count - 1; index >= 0; index-- ) { pending.Push( branch[index] ); }
			return true;
		}
	}
}
