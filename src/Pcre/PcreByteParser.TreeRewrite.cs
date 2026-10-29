namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private void PushTreeBody( Stack<(PcreByteNode Node, bool Ready)> pending, PcreByteAlternatives body ) {
		for ( var branch = body.Branches.Count - 1; branch >= 0; branch-- ) {
			var nodes = body.Branches[branch];
			for ( var index = nodes.Count - 1; index >= 0; index-- ) {
				cancellationToken.ThrowIfCancellationRequested();
				pending.Push( (nodes[index], false) );
			}
		}
	}

	private void PushTreeChildren( Stack<(PcreByteNode Node, bool Ready)> pending, PcreByteNode node ) {
		switch ( node ) {
			case PcreByteGroup group: PushTreeBody( pending, group.Body ); break;
			case PcreByteNoncapturingGroup group: PushTreeBody( pending, group.Body ); break;
			case ParsedCaptureConditional conditional: PushTreeBody( pending, conditional.Body ); break;
			case PcreByteCaptureConditional conditional: PushTreeBody( pending, conditional.Body ); break;
			case PcreByteRecursionConditional conditional: PushTreeBody( pending, conditional.Body ); break;
			case PcreByteStaticConditional conditional: PushTreeBody( pending, conditional.Body ); break;
			case PcreByteAssertionConditional conditional:
				PushTreeBody( pending, conditional.Body );
				pending.Push( (conditional.Assertion, false) );
				break;
			case PcreByteLookahead assertion: PushTreeBody( pending, assertion.Body ); break;
			case PcreByteLookbehind assertion: PushTreeBody( pending, assertion.Body ); break;
			case PcreByteAtomic atomic: pending.Push( (atomic.Child, false) ); break;
			case PcreByteRepeat repeat: pending.Push( (repeat.Child, false) ); break;
		}
	}

	private PcreByteAlternatives RewriteTreeBody( PcreByteAlternatives body,
		Dictionary<PcreByteNode, PcreByteNode> rewritten ) {
		var branches = new IReadOnlyList<PcreByteNode>[body.Branches.Count];
		for ( var branch = 0; branch < branches.Length; branch++ ) {
			var original = body.Branches[branch];
			var nodes = new PcreByteNode[original.Count];
			for ( var index = 0; index < nodes.Length; index++ ) {
				cancellationToken.ThrowIfCancellationRequested();
				nodes[index] = rewritten[original[index]];
			}
			branches[branch] = nodes;
		}
		return new( branches );
	}

	private PcreByteAlternatives RewriteTree( PcreByteAlternatives root, Func<PcreByteNode, PcreByteNode> rewrite ) {
		var rewritten = new Dictionary<PcreByteNode, PcreByteNode>( ReferenceEqualityComparer.Instance );
		var pending = new Stack<(PcreByteNode Node, bool Ready)>();
		PushTreeBody( pending, root );
		while ( pending.TryPop( out var item ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( rewritten.ContainsKey( item.Node ) ) { continue; }
			if ( !item.Ready ) {
				pending.Push( (item.Node, true) );
				PushTreeChildren( pending, item.Node );
				continue;
			}
			var node = item.Node switch {
				PcreByteGroup group => group with { Body = RewriteTreeBody( group.Body, rewritten ) },
				PcreByteNoncapturingGroup group => group with { Body = RewriteTreeBody( group.Body, rewritten ) },
				ParsedCaptureConditional conditional => conditional with { Body = RewriteTreeBody( conditional.Body, rewritten ) },
				PcreByteCaptureConditional conditional => conditional with { Body = RewriteTreeBody( conditional.Body, rewritten ) },
				PcreByteRecursionConditional conditional => conditional with { Body = RewriteTreeBody( conditional.Body, rewritten ) },
				PcreByteStaticConditional conditional => conditional with { Body = RewriteTreeBody( conditional.Body, rewritten ) },
				PcreByteAssertionConditional conditional => conditional with {
					Assertion = rewritten[conditional.Assertion], Body = RewriteTreeBody( conditional.Body, rewritten )
				},
				PcreByteLookahead assertion => assertion with { Body = RewriteTreeBody( assertion.Body, rewritten ) },
				PcreByteLookbehind assertion => assertion with { Body = RewriteTreeBody( assertion.Body, rewritten ) },
				PcreByteAtomic atomic => atomic with { Child = rewritten[atomic.Child] },
				PcreByteRepeat repeat => repeat with { Child = rewritten[repeat.Child] },
				_ => item.Node
			};
			rewritten[item.Node] = rewrite( node );
		}
		return RewriteTreeBody( root, rewritten );
	}
}
