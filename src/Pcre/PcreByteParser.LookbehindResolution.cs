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
		var suppressed = SuppressedWidthBehinds( root );
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

	private HashSet<int> SuppressedWidthBehinds( PcreByteAlternatives root ) {
		// Width mode skips DEFINE and the rest of a branch after failure,
		// including lookahead descendants there. Global mode still scans them.
		var suppressed = new HashSet<int>();
		var pending = new Stack<(PcreByteNode Node, bool Width, bool Suppressed)>();
		var children = new Stack<(PcreByteNode Node, bool Ready)>();
		void PushBody( PcreByteAlternatives body, bool width, bool skip ) {
			foreach ( var branch in body.Branches ) {
				var terminated = false;
				foreach ( var node in branch ) {
					cancellationToken.ThrowIfCancellationRequested();
					pending.Push( (node, width, skip || terminated) );
					if ( width && node is PcreByteFailure ) { terminated = true; }
				}
			}
		}
		PushBody( root, false, false );
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
			var body = item.Node switch {
				PcreByteGroup group => group.Body,
				PcreByteNoncapturingGroup noncapturing => noncapturing.Body,
				ParsedCaptureConditional parsedCondition => parsedCondition.Body,
				PcreByteCaptureConditional captureCondition => captureCondition.Body,
				PcreByteRecursionConditional recursionCondition => recursionCondition.Body,
				PcreByteStaticConditional staticCondition => staticCondition.Body,
				PcreByteAssertionConditional assertionBranches => assertionBranches.Body,
				PcreByteLookahead ahead => ahead.Body,
				PcreByteLookbehind behind => behind.Body,
				_ => null
			};
			if ( body is not null ) {
				PushBody( body, width, skip );
				if ( item.Node is PcreByteAssertionConditional assertionConditional ) {
					pending.Push( (assertionConditional.Assertion, width, skip) );
				}
			} else {
				PushTreeChildren( children, item.Node );
				while ( children.TryPop( out var descendant ) ) { pending.Push( (descendant.Node, width, skip) ); }
			}
		}
		return suppressed;
	}
}
