namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private readonly record struct ReturnedCaptureReference( int Number, string? Name );

	// Calls and list entries use the source capture count, before global resolution
	// and before later branch-reset arms can change it.
	private int ParseSubroutineNumber( int offset ) {
		var first = source[index];
		var relative = first is (byte)'+' or (byte)'-';
		if ( relative ) { index++; }
		var start = index;
		var number = 0;
		while ( index < source.Length && source[index] is >= (byte)'0' and <= (byte)'9' ) {
			cancellationToken.ThrowIfCancellationRequested();
			var digit = source[index++] - (byte)'0';
			if ( number > ( MaximumCaptureCount - digit ) / 10 ) {
				throw Invalid( offset, "subroutine reference exceeds capture limit" );
			}
			number = number * 10 + digit;
		}
		if ( index == start || relative && number == 0 ) { throw Invalid( offset, "invalid subroutine reference" ); }
		if ( relative ) {
			number = first == (byte)'+' ? groupCount + number : groupCount + 1 - number;
			if ( number <= 0 ) { throw Invalid( offset, "relative subroutine reference has no group" ); }
		}
		return number;
	}

	private ReadOnlyMemory<ReturnedCaptureReference> ParseReturnedCaptures( int offset ) {
		var references = new List<ReturnedCaptureReference>();
		do {
			cancellationToken.ThrowIfCancellationRequested();
			index++; // Opening '(' or comma; no header trivia is ignored.
			if ( index >= source.Length ) { throw Invalid( offset, "unclosed returned capture list" ); }
			if ( source[index] is (byte)'<' or (byte)'\'' ) {
				var terminator = source[index++] == (byte)'<' ? (byte)'>' : (byte)'\'';
				references.Add( new( 0, ParseCaptureName( terminator, offset ) ) );
			} else {
				var number = ParseSubroutineNumber( offset );
				if ( number <= 0 ) { throw Invalid( offset, "returned capture must be positive" ); }
				references.Add( new( number, null ) );
			}
		} while ( index < source.Length && source[index] == (byte)',' );
		if ( index >= source.Length || source[index] != (byte)')' ) {
			throw Invalid( offset, "invalid returned capture list" );
		}
		index++;
		return references.ToArray();
	}

	private ReadOnlyMemory<int> ResolveReturnedCaptures( ReadOnlyMemory<ReturnedCaptureReference> references, int offset ) {
		if ( references.IsEmpty ) { return default; }
		var selected = new HashSet<int>();
		foreach ( var reference in references.Span ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( reference.Name is { } name ) {
				if ( !namedCaptures.TryGetValue( name, out var numbers ) ) {
					throw Invalid( offset, "returned capture has no name" );
				}
				foreach ( var number in numbers ) {
					cancellationToken.ThrowIfCancellationRequested();
					selected.Add( number );
				}
			} else {
				if ( reference.Number > groupCount ) { throw Invalid( offset, "returned capture has no group" ); }
				selected.Add( reference.Number );
			}
		}
		var result = selected.ToArray();
		Array.Sort( result ); // At most256 unique capture slots.
		return result;
	}
}
