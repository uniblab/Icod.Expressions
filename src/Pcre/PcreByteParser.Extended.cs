namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private enum ExtendedMode { None, Extended, More }

	// Only call at token boundaries. Compound token readers keep their own grammar.
	private void SkipPatternTrivia( ref bool quoting ) {
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( TryConsumeQuoteDelimiter( ref quoting ) ) { continue; }
			if ( quoting ) { return; }
			var before = index;
			SkipIgnoredPatternText();
			if ( index == before ) { return; }
		}
	}

	private void SkipIgnoredPatternText() {
		while ( index < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( extended != ExtendedMode.None ) {
				if ( TryConsumePatternWhiteSpace() ) { continue; }
				if ( source[index] == (byte)'#' ) {
					index++;
					while ( index < source.Length ) {
						cancellationToken.ThrowIfCancellationRequested();
						var value = source[index++];
						if ( value == (byte)'\n' ) { break; }
						if ( anyCrLfNewline && value == (byte)'\r' ) {
							if ( index < source.Length && source[index] == (byte)'\n' ) { index++; }
							break;
						}
					}
					continue;
				}
			}
			if ( source.AsSpan( index ).StartsWith( "(?#"u8 ) ) {
				var opening = index;
				index += 3;
				while ( index < source.Length && source[index] != (byte)')' ) {
					cancellationToken.ThrowIfCancellationRequested();
					index++;
				}
				if ( index >= source.Length ) { throw Invalid( opening, "unclosed comment" ); }
				index++;
				continue;
			}
			return;
		}
	}

	private bool TryConsumePatternWhiteSpace() {
		var scalar = (int)source[index];
		var width = 1;
		if ( utf8 && scalar >= 0x80 ) {
			PcreUtf8.TryRead( source, index, out scalar, out width ); // Validated at compile entry.
		}
		// PCRE2 10.48 Pattern_White_Space, including raw byte NEL outside UTF mode.
		if ( scalar is >= 0x09 and <= 0x0d or 0x20 or 0x85 or 0x200e or 0x200f or 0x2028 or 0x2029 ) {
			index += width;
			return true;
		}
		return false;
	}

	private void SkipClassSpace() {
		while ( extended == ExtendedMode.More && index < source.Length && source[index] is (byte)' ' or (byte)'\t' ) {
			cancellationToken.ThrowIfCancellationRequested();
			index++;
		}
	}

	private void SkipClassTrivia( ref bool quoting ) {
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( TryConsumeQuoteDelimiter( ref quoting ) ) { continue; }
			if ( quoting ) { return; }
			var before = index;
			SkipClassSpace();
			if ( index == before ) { return; }
		}
	}

	private bool TryStartBracketRange( ref bool quoting, out int hyphenOffset ) {
		hyphenOffset = index;
		if ( quoting || index >= source.Length || source[index] != (byte)'-' ) { return false; }
		index++;
		SkipClassTrivia( ref quoting );
		if ( !quoting && index < source.Length && source[index] == (byte)']' ) {
			// Leave a trailing hyphen for the next literal item, including its trivia.
			index = hyphenOffset;
			return false;
		}
		return true;
	}
}
