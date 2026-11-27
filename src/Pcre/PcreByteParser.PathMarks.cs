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
		var name = ReadControlVerbName( "MARK", requireName: true, out var wordName );
		return new( name, wordName );
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

	private PcreByteNode? TryParseNamedImmediateVerb() {
		if ( TryConsumeGroupAlias( "*ACCEPT:"u8 ) ) { return new PcreByteAccept( ReadControlVerbName( "ACCEPT", requireName: false ) ); }
		if ( TryConsumeGroupAlias( "*FAIL:"u8 ) ) { return ReadNamedFailure( "FAIL" ); }
		if ( TryConsumeGroupAlias( "*F:"u8 ) ) { return ReadNamedFailure( "F" ); }
		return null; // Preserve the existing malformed-group byte for bare unfinished headers.
	}

	private PcreByteFailure ReadNamedFailure( string verb ) {
		var name = ReadControlVerbName( verb, requireName: false, out var wordName );
		return new( name, wordName );
	}

	private PcreByteNode? TryParseNamedCut() {
		if ( TryConsumeGroupAlias( "*PRUNE:"u8 ) ) { return new PcreBytePrune( ReadControlVerbName( "PRUNE", requireName: false ) ); }
		if ( TryConsumeGroupAlias( "*COMMIT:"u8 ) ) { return new PcreByteCommit( ReadControlVerbName( "COMMIT", requireName: false ) ); }
		if ( TryConsumeGroupAlias( "*THEN:"u8 ) ) {
			HasThen = true;
			return new PcreByteThen( ReadControlVerbName( "THEN", requireName: false ) );
		}
		var remainder = source.AsSpan( index );
		if ( remainder.SequenceEqual( "*PRUNE"u8 ) || remainder.SequenceEqual( "*COMMIT"u8 ) || remainder.SequenceEqual( "*THEN"u8 ) ) {
			throw Invalid( source.Length, "unterminated control verb header" );
		}
		return null;
	}

	private ReadOnlyMemory<byte> ReadControlVerbName( string verb, bool requireName ) =>
		ReadControlVerbName( verb, requireName, out _ );

	private ReadOnlyMemory<byte> ReadControlVerbName( string verb, bool requireName, out string? wordName ) {
		wordName = null;
		if ( altVerbNames ) {
			// Alternate-name escapes are not yet part of the word options profile.
			if ( wordSource is not null ) { throw Unsupported( index ); }
			return ReadAlternateControlVerbName( verb, requireName );
		}
		var start = index;
		// Without ALT_VERBNAMES, backslashes and extended-mode trivia are raw
		// name bytes. Only the first closing parenthesis terminates this token.
		while ( index < source.Length && source[index] != (byte)')' ) {
			cancellationToken.ThrowIfCancellationRequested();
			index++;
		}
		if ( index == source.Length ) { throw Invalid( index, String.Concat( "unterminated ", verb, " name" ) ); }
		var length = index - start;
		// Native stores the length in one code unit: 255 bytes or 65,535 words.
		// The word compiler's smaller whole-pattern budget is checked before parsing.
		var maximum = wordSource is null ? 255 : 65535;
		if ( length > maximum || requireName && length == 0 ) {
			throw Invalid( index, String.Concat( verb, requireName ? " name must contain 1 to " : " name must contain at most ",
				maximum.ToString( System.Globalization.CultureInfo.InvariantCulture ), wordSource is null ? " bytes" : " code units" ) );
		}
		var name = source.AsMemory( start, length );
		if ( wordSource is not null ) { wordName = new string( wordSource, start, length ); }
		index++;
		return name;
	}
}
