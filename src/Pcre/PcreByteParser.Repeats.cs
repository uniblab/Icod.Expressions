namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private bool IsRepeatQuantifier( int offset ) => offset < source.Length
		&& ( source[offset] is (byte)'*' or (byte)'+' or (byte)'?'
			|| source[offset] == (byte)'{' && IsCountedQuantifier( offset ) );

	// Classify the complete shape before reading numeric values. In particular,
	// huge digits in a malformed shape are literal text, not count overflow.
	// This scan is transactional: only the local cursor moves, with no allocations.
	private bool IsCountedQuantifier( int offset ) {
		var cursor = offset + 1;
		SkipCountedSpace( ref cursor );
		var minimum = ScanCountedDigits( ref cursor );
		SkipCountedSpace( ref cursor );
		if ( cursor >= source.Length ) { return false; }
		if ( source[cursor] == (byte)'}' ) { return minimum; }
		if ( source[cursor++] != (byte)',' ) { return false; }
		SkipCountedSpace( ref cursor );
		var maximum = ScanCountedDigits( ref cursor );
		SkipCountedSpace( ref cursor );
		return ( minimum || maximum ) && cursor < source.Length && source[cursor] == (byte)'}';
	}

	private bool ScanCountedDigits( ref int cursor ) {
		var start = cursor;
		while ( cursor < source.Length && source[cursor] is >= (byte)'0' and <= (byte)'9' ) {
			cancellationToken.ThrowIfCancellationRequested();
			cursor++;
		}
		return cursor > start;
	}

	private void SkipCountedSpace( ref int cursor ) {
		while ( cursor < source.Length && source[cursor] is (byte)' ' or (byte)'\t' ) {
			cancellationToken.ThrowIfCancellationRequested();
			cursor++;
		}
	}

	private (int Minimum, int Maximum) ParseCountedBounds( int offset ) {
		if ( !IsCountedQuantifier( index ) ) { throw Unsupported( offset ); }
		index++; // Opening brace.
		SkipBraceHorizontalSpace();
		var hasMinimum = ReadCount( offset, out var minimum );
		SkipBraceHorizontalSpace();
		var comma = index < source.Length && source[index] == (byte)',';
		var hasMaximum = false;
		var maximum = minimum;
		if ( comma ) {
			index++;
			SkipBraceHorizontalSpace();
			hasMaximum = ReadCount( offset, out maximum );
			SkipBraceHorizontalSpace();
			if ( !hasMinimum && !hasMaximum ) { throw Unsupported( offset ); }
			if ( !hasMinimum ) { minimum = 0; }
			if ( !hasMaximum ) { maximum = int.MaxValue; }
		} else if ( !hasMinimum ) { throw Unsupported( offset ); }
		if ( index >= source.Length || source[index] != (byte)'}' ) {
			throw Invalid( offset, "unclosed counted quantifier" );
		}
		index++;
		if ( maximum < minimum ) { throw Invalid( offset, "repeat maximum is less than minimum" ); }
		return (minimum, maximum);
	}

	private void SkipBraceHorizontalSpace() {
		while ( index < source.Length && source[index] is (byte)' ' or (byte)'\t' ) {
			cancellationToken.ThrowIfCancellationRequested();
			index++;
		}
	}

	private bool ReadCount( int offset, out int count ) {
		count = 0;
		var start = index;
		while ( index < source.Length && source[index] is >= (byte)'0' and <= (byte)'9' ) {
			cancellationToken.ThrowIfCancellationRequested();
			var digit = source[index++] - (byte)'0';
			if ( count > ( 65535 - digit ) / 10 ) {
				throw Invalid( offset, "repeat count exceeds 65535" );
			}
			count = count * 10 + digit;
		}
		return index > start;
	}

}
