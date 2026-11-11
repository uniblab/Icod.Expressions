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
		return new( ReadControlVerbName( "MARK", requireName: true ) );
	}

	private PcreByteNode? TryParseNamedSkip() {
		if ( source.AsSpan( index ).SequenceEqual( "*SKIP"u8 ) ) {
			throw Invalid( source.Length, "unterminated SKIP header" );
		}
		if ( !TryConsumeGroupAlias( "*SKIP:"u8 ) ) { return null; }
		var name = ReadControlVerbName( "SKIP", requireName: false );
		if ( name.IsEmpty ) { return new PcreByteSkip(); }
		// A nonempty raw argument beginning with NUL is still named SKIP.
		HasNamedSkip = true;
		return new PcreByteNamedSkip( name );
	}

	private ReadOnlyMemory<byte> ReadControlVerbName( string verb, bool requireName ) {
		var start = index;
		// Without ALT_VERBNAMES, backslashes and extended-mode trivia are raw
		// name bytes. Only the first closing parenthesis terminates this token.
		while ( index < source.Length && source[index] != (byte)')' ) {
			cancellationToken.ThrowIfCancellationRequested();
			index++;
		}
		if ( index == source.Length ) { throw Invalid( index, String.Concat( "unterminated ", verb, " name" ) ); }
		var length = index - start;
		if ( length > 255 || requireName && length == 0 ) {
			throw Invalid( index, String.Concat( verb, requireName ? " name must contain 1 to 255 bytes" : " name must contain at most 255 bytes" ) );
		}
		var name = source.AsMemory( start, length );
		index++;
		return name;
	}
}
