using System.Text;

namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private const int MaximumNameBytes = 128;
	private readonly Dictionary<string, int> namedCaptures = new( StringComparer.Ordinal );
	private readonly Dictionary<int, string> captureNames = new();
	private bool hasNamedReferences;

	// Temporary parser node only: resolved before the numbered program compiler runs.
	private sealed record NamedReference( string Name, int Offset ) : PcreByteNode;

	private void RegisterCaptureName( string name, int number, int offset ) {
		if ( namedCaptures.TryGetValue( name, out var previousNumber ) && previousNumber != number ) {
			throw Invalid( offset, "capture name identifies different group numbers" );
		}
		if ( captureNames.TryGetValue( number, out var previousName ) && previousName != name ) {
			throw Invalid( offset, "capture group number has different names" );
		}
		namedCaptures[name] = number;
		captureNames[number] = name;
	}

	private NamedReference ParseNamedReference( byte terminator, int offset ) {
		var name = ParseCaptureName( terminator, offset );
		hasNamedReferences = true;
		return new( name, offset );
	}

	private string ParseCaptureName( byte terminator, int offset ) {
		var braced = terminator == (byte)'}';
		if ( braced ) { SkipBraceHorizontalSpace(); }
		var start = index;
		while ( index < source.Length && source[index] != terminator ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( braced && source[index] is (byte)' ' or (byte)'\t' ) { break; }
			var scalar = (int)source[index];
			var width = 1;
			if ( utf8 ) { PcreUtf8.TryRead( source, index, out scalar, out width ); } // Validated at compile entry.
			var letter = scalar == '_' || ( utf8 ? Rune.IsLetter( new Rune( scalar ) )
				: scalar is >= 'A' and <= 'Z' or >= 'a' and <= 'z' );
			var digit = utf8 ? Rune.IsDigit( new Rune( scalar ) ) : scalar is >= '0' and <= '9';
			if ( !letter && ( index == start || !digit ) ) {
				throw Invalid( offset, "invalid capture name" );
			}
			if ( index - start + width > MaximumNameBytes ) {
				throw Invalid( offset, "capture name exceeds 128 bytes" );
			}
			index += width;
		}
		var length = index - start;
		if ( length == 0 ) { throw Invalid( offset, "capture name must not be empty" ); }
		if ( braced ) { SkipBraceHorizontalSpace(); }
		if ( index >= source.Length || source[index] != terminator ) {
			throw Invalid( offset, "unclosed capture name" );
		}
		index++;
		return ( utf8 ? Encoding.UTF8 : Encoding.ASCII ).GetString( source, start, length );
	}

	private PcreByteAlternatives ResolveNamedReferences( PcreByteAlternatives alternatives ) {
		var branches = new IReadOnlyList<PcreByteNode>[alternatives.Branches.Count];
		for ( var branch = 0; branch < branches.Length; branch++ ) {
			var original = alternatives.Branches[branch];
			var nodes = new PcreByteNode[original.Count];
			for ( var n = 0; n < nodes.Length; n++ ) { nodes[n] = ResolveNamedReference( original[n] ); }
			branches[branch] = nodes;
		}
		return new( branches );
	}

	private PcreByteNode ResolveNamedReference( PcreByteNode node ) {
		cancellationToken.ThrowIfCancellationRequested();
		return node switch {
			NamedReference reference => namedCaptures.TryGetValue( reference.Name, out var number )
				? new PcreByteBackreference( number ) { Caseless = reference.Caseless }
				: throw Invalid( reference.Offset, "backreference has no capture name" ),
			PcreByteGroup group => group with { Body = ResolveNamedReferences( group.Body ) },
			PcreByteNoncapturingGroup group => group with { Body = ResolveNamedReferences( group.Body ) },
			PcreByteLookahead assertion => assertion with { Body = ResolveNamedReferences( assertion.Body ) },
			PcreByteLookbehind assertion => assertion with { Body = ResolveNamedReferences( assertion.Body ) },
			PcreByteRepeat repeat => repeat with { Child = ResolveNamedReference( repeat.Child ) },
			PcreByteAtomic atomic => atomic with { Child = ResolveNamedReference( atomic.Child ) },
			_ => node
		};
	}
}
