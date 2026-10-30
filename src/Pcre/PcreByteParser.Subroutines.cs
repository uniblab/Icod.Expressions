namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private bool hasSubroutines;
	private readonly Dictionary<int, PcreByteGroup> subroutineTargets = new();
	internal IReadOnlyDictionary<int, PcreByteGroup> SubroutineTargets => subroutineTargets;

	private sealed record ParsedSubroutineCall( int Number, string? Name, int Offset,
		ReadOnlyMemory<ReturnedCaptureReference> ReturnedCaptures ) : PcreByteNode;

	private PcreByteNode ParseSubroutineCall( byte terminator, int offset, bool named = false ) {
		hasSubroutines = true;
		if ( index >= source.Length ) { throw Invalid( offset, "unclosed subroutine call" ); }
		var first = source[index];
		if ( !named && first is >= (byte)'0' and <= (byte)'9' or (byte)'+' or (byte)'-' ) {
			var number = ParseSubroutineNumber( offset );
			var returned = FinishSubroutineHeader( terminator, offset );
			if ( number == 0 ) { throw Unsupported( offset ); } // Whole-pattern recursion.
			return new ParsedSubroutineCall( number, null, offset, returned );
		}
		var name = ParseCaptureName( terminator, offset, allowSubroutineArguments: terminator == (byte)')' );
		// Parenthesized names leave either ')' or '(' for the exact header parser;
		// g angle/quoted names still consume their ordinary terminator.
		var selection = terminator == (byte)')' ? FinishSubroutineHeader( terminator, offset ) : default;
		return new ParsedSubroutineCall( 0, name, offset, selection );
	}

	private ReadOnlyMemory<ReturnedCaptureReference> FinishSubroutineHeader( byte terminator, int offset ) {
		var returned = terminator == (byte)')' && index < source.Length && source[index] == (byte)'('
			? ParseReturnedCaptures( offset ) : default;
		if ( index >= source.Length || source[index] != terminator ) { throw Invalid( offset, "invalid subroutine header" ); }
		index++;
		return returned;
	}

	private Dictionary<int, PcreByteGroup> CollectSubroutineGroups( PcreByteAlternatives root ) {
		var groups = new Dictionary<int, PcreByteGroup>();
		var pending = new Stack<(PcreByteNode Node, bool Ready)>();
		PushTreeBody( pending, root );
		while ( pending.TryPop( out var item ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			// Native selects the first syntactic definition, not the participating slot.
			if ( item.Node is PcreByteGroup group ) { groups.TryAdd( group.Number, group ); }
			PushTreeChildren( pending, item.Node );
		}
		return groups;
	}

	private PcreByteAlternatives ResolveSubroutines( PcreByteAlternatives root ) {
		root = RewriteTree( root, node => {
			if ( node is not ParsedSubroutineCall call ) { return node; }
			var number = call.Number;
			if ( call.Name is { } name ) {
				if ( !namedCaptures.TryGetValue( name, out var numbers ) ) { throw Invalid( call.Offset, "subroutine has no capture name" ); }
				number = numbers[0];
			}
			if ( number <= 0 || number > groupCount ) { throw Invalid( call.Offset, "subroutine has no capture group" ); }
			return new PcreByteSubroutineCall( number, call.Offset, ResolveReturnedCaptures( call.ReturnedCaptures, call.Offset ) );
		} );
		var groups = CollectSubroutineGroups( root );
		var edges = new Dictionary<int, List<PcreByteSubroutineCall>>();
		var scan = new Stack<(PcreByteNode Node, bool Ready)>();
		foreach ( var group in groups.Values ) {
			var calls = new List<PcreByteSubroutineCall>();
			PushTreeBody( scan, group.Body );
			while ( scan.TryPop( out var item ) ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( item.Node is PcreByteSubroutineCall call ) { calls.Add( call ); }
				PushTreeChildren( scan, item.Node );
			}
			edges.Add( group.Number, calls );
		}
		var complete = new HashSet<int>();
		var active = new Dictionary<int, int>();
		var pending = new Stack<(int Number, bool Ready, int Offset)>();
		foreach ( var number in groups.Keys ) {
			pending.Push( (number, false, int.MaxValue) );
			while ( pending.TryPop( out var item ) ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( complete.Contains( item.Number ) ) { continue; }
				if ( item.Ready ) { active.Remove( item.Number ); complete.Add( item.Number ); continue; }
				if ( !active.TryAdd( item.Number, item.Offset ) ) {
					var offset = item.Offset;
					foreach ( var opening in active.Values ) { offset = Math.Min( offset, opening ); }
					throw Unsupported( offset );
				}
				pending.Push( (item.Number, true, item.Offset) );
				var calls = edges[item.Number];
				for ( var index = calls.Count - 1; index >= 0; index-- ) {
					pending.Push( (calls[index].Number, false, calls[index].Offset) );
				}
			}
		}
		return root;
	}

	private void FinishSubroutineTargets( PcreByteAlternatives root ) {
		var groups = CollectSubroutineGroups( root );
		var pending = new Stack<(PcreByteNode Node, bool Ready)>();
		var scan = new Stack<(PcreByteNode Node, bool Ready)>();
		PushTreeBody( pending, root );
		while ( pending.TryPop( out var item ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( item.Node is PcreByteSubroutineCall call && !subroutineTargets.ContainsKey( call.Number ) ) {
				var group = groups[call.Number];
				PushTreeBody( scan, group.Body );
				while ( scan.TryPop( out var child ) ) {
					cancellationToken.ThrowIfCancellationRequested();
					if ( child.Node is PcreByteLookbehind { BranchLengths.IsEmpty: true } ) { throw Unsupported( call.Offset ); }
					PushTreeChildren( scan, child.Node );
				}
				subroutineTargets.Add( call.Number, group );
			}
			PushTreeChildren( pending, item.Node );
		}
	}
}
