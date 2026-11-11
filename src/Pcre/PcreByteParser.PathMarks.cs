namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private PcreBytePathMark? TryParsePathMark() {
		if ( source.AsSpan( index ).SequenceEqual( "*MARK"u8 ) ) {
			throw Invalid( source.Length, "unterminated MARK header" );
		}
		if ( TryConsumeGroupAlias( "*MARK)"u8 ) ) {
			throw Invalid( index - 1, "MARK requires a name" );
		}
		if ( !TryConsumeGroupAlias( "*MARK:"u8 ) && !TryConsumeGroupAlias( "*:"u8 ) ) { return null; }
		var start = index;
		// Without ALT_VERBNAMES, backslashes and extended-mode trivia are raw
		// name bytes. Only the first closing parenthesis terminates this token.
		while ( index < source.Length && source[index] != (byte)')' ) {
			cancellationToken.ThrowIfCancellationRequested();
			index++;
		}
		if ( index == source.Length ) { throw Invalid( index, "unterminated MARK name" ); }
		var length = index - start;
		if ( length is 0 or > 255 ) { throw Invalid( index, "MARK name must contain 1 to 255 bytes" ); }
		var name = source.AsMemory( start, length );
		index++;
		return new( name );
	}
}
