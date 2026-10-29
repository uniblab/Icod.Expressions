namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private readonly record struct LookbehindCapture( PcreByteGroup Group, int Start, int End );

	private sealed class LookbehindContext {
		internal Dictionary<int, LookbehindCapture> Captures { get; } = new();
		internal Dictionary<int, PcreByteGroup> CallTargets { get; } = new();
		internal Dictionary<PcreByteNode, int> Positions { get; } = new( ReferenceEqualityComparer.Instance );
	}

	private PcreByteLookbehind DeferLookbehind( PcreByteAlternatives body, bool positive, int openingOffset,
		bool nonAtomic = false ) {
		hasGeneralLookbehinds = true;
		return new( body, positive, ReadOnlyMemory<int>.Empty, OpeningOffset: openingOffset, NonAtomic: nonAtomic );
	}

	private PcreByteAlternatives ResolveLookbehindLengths( PcreByteAlternatives root ) {
		var suppressed = SuppressedDefinitionBehinds( root );
		// Collect definitions and lexical spans from the same resolved node identities.
		// Assertions and erased children still belong to their lexical capture scope.
		var context = new LookbehindContext();
		var pending = new Stack<(PcreByteNode Node, bool Ready)>();
		var sequence = 0;
		PushTreeBody( pending, root );
		while ( pending.TryPop( out var item ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( !item.Ready ) {
				context.Positions[item.Node] = sequence++;
				// Calls use the first syntactic definition even for duplicate slots/names.
				// Keep backreference targets and lexical intervals independent.
				if ( item.Node is PcreByteGroup first ) { context.CallTargets.TryAdd( first.Number, first ); }
				pending.Push( (item.Node, true) );
				PushTreeChildren( pending, item.Node );
			} else if ( item.Node is PcreByteGroup group ) {
				context.Captures[group.Number] = new( group, context.Positions[group], sequence );
			}
		}
		return RewriteTree( root, node => node is PcreByteLookbehind assertion && !suppressed.Contains( assertion.OpeningOffset )
			? FinishLookbehind( assertion.Body, assertion.IsPositive, assertion.OpeningOffset, context )
				with { Caseless = assertion.Caseless, CaselessRestricted = assertion.CaselessRestricted,
					NonAtomic = assertion.NonAtomic }
			: node );
	}

	private HashSet<int> SuppressedDefinitionBehinds( PcreByteAlternatives root ) {
		// Native switches between a global scan and consumed-width analysis. A
		// behind enters width mode; an ahead reenters global mode. DEFINE reached
		// in width mode suppresses ALL descendants. Keep source offsets because
		// RewriteTree rebuilds node identities before invoking its callback.
		var suppressed = new HashSet<int>();
		var pending = new Stack<(PcreByteNode Node, bool Width, bool Suppressed)>();
		var children = new Stack<(PcreByteNode Node, bool Ready)>();
		PushTreeBody( children, root );
		while ( children.TryPop( out var rootChild ) ) { pending.Push( (rootChild.Node, false, false) ); }
		while ( pending.TryPop( out var item ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			var width = item.Width;
			var skip = item.Suppressed;
			if ( item.Node is PcreByteLookbehind assertion ) {
				if ( skip ) { suppressed.Add( assertion.OpeningOffset ); }
				width = true;
			} else if ( item.Node is PcreByteLookahead ) {
				width = false;
			} else if ( width && item.Node is PcreByteStaticConditional { IsDefinition: true } ) {
				skip = true;
			}
			PushTreeChildren( children, item.Node );
			while ( children.TryPop( out var descendant ) ) { pending.Push( (descendant.Node, width, skip) ); }
		}
		return suppressed;
	}
}
