using System.Text;

namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private const int MaximumNameBytes = 128;
	private readonly Dictionary<string, List<int>> namedCaptures = new( StringComparer.Ordinal );
	private readonly List<(string Name, int Number)> captureNames = new();
	private bool hasNamedReferences;

	// Temporary parser node only: resolved before the numbered program compiler runs.
	private sealed record NamedReference( string Name, int Offset ) : PcreByteNode;

	private void RegisterCaptureName( string name, int number, int offset ) {
		foreach ( var previous in captureNames ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( previous.Name == name ) {
				if ( previous.Number == number ) { return; }
				if ( !duplicateNames ) { throw Invalid( offset, "capture name identifies different group numbers" ); }
				// Native stops collision checks at the first matching name. A later
				// different-name number collision must not preempt this declaration.
				break;
			}
			if ( previous.Number == number ) { throw Invalid( offset, "capture group number has different names" ); }
		}
		if ( !namedCaptures.TryGetValue( name, out var numbers ) ) {
			numbers = new();
			namedCaptures.Add( name, numbers );
		}
		// Check the first historical number before deduplicating aliases. Under
		// branch reset, declaration order need not be numerical order.
		foreach ( var previousNumber in numbers ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( previousNumber == number ) { return; }
		}
		numbers.Add( number );
		captureNames.Add( (name, number) );
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
		var duplicates = new Dictionary<string, ReadOnlyMemory<int>>( StringComparer.Ordinal );
		return RewriteTree( alternatives, node => {
			if ( node is not NamedReference reference ) { return node; }
			if ( !namedCaptures.TryGetValue( reference.Name, out var numbers ) ) {
				throw Invalid( reference.Offset, "backreference has no capture name" );
			}
			ReadOnlyMemory<int> candidates = default;
			if ( numbers.Count > 1 && !duplicates.TryGetValue( reference.Name, out candidates ) ) {
				candidates = numbers.ToArray();
				duplicates.Add( reference.Name, candidates );
			}
			return new PcreByteBackreference( numbers[0], candidates ) {
				Caseless = reference.Caseless, CaselessRestricted = reference.CaselessRestricted
			};
		} );
	}
}
