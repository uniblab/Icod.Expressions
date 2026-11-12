namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private PcreByteNode ParseDecimalOrOctalEscape( byte first, int offset, bool inBracket ) {
		if ( first == (byte)'0' || inBracket ) { return ParseUnbracedOctal( first, offset ); }
		// Python decides from the first three octal characters, independently
		// of capture numbering. Any following digits stay ordinary source.
		if ( pythonOctal && first < (byte)'8' && source.Length - index >= 2
			&& source[index] is >= (byte)'0' and <= (byte)'7'
			&& source[index + 1] is >= (byte)'0' and <= (byte)'7' ) {
			return ParseUnbracedOctal( first, offset );
		}
		var number = first - (byte)'0';
		var cursor = index;
		while ( cursor < source.Length && source[cursor] is >= (byte)'0' and <= (byte)'9' ) {
			cancellationToken.ThrowIfCancellationRequested();
			// Beyond the library's capture ceiling, only the first digit can still
			// force reference syntax. Saturation preserves that decision without overflow.
			number = Math.Min( MaximumCaptureCount + 1, number * 10 + source[cursor++] - (byte)'0' );
		}
		if ( pythonOctal || number < 10 || first >= (byte)'8' || number <= groupCount ) {
			if ( number > MaximumCaptureCount ) { throw Invalid( offset, "backreference has no capture group" ); }
			index = cursor;
			backreferences.Add( (number, offset) );
			return new PcreByteBackreference( number );
		}
		// The decimal run was inspected, not consumed. Only up to three octal
		// digits belong to this literal; later digits return to ordinary parsing.
		return ParseUnbracedOctal( first, offset );
	}

	private PcreByteLiteral ParseUnbracedOctal( byte first, int offset ) {
		if ( first >= (byte)'8' ) { return new( first ); } // Literal 8/9 in a bracket.
		var value = first - (byte)'0';
		var digits = 1;
		for ( ; digits < 3 && index < source.Length
			&& source[index] is >= (byte)'0' and <= (byte)'7'; digits++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			value = value * 8 + source[index++] - (byte)'0';
		}
		if ( value > 255 && ( !utf8 || pythonOctal ) ) { throw Invalid( offset, "octal escape exceeds the supported character range" ); }
		if ( noBackslashZero && value == 0 && digits == 1 ) { throw Invalid( offset, "bare backslash-zero escape is disabled" ); }
		return new( value );
	}

	private PcreByteLiteral ParseBracedOctal( int offset ) {
		if ( index >= source.Length || source[index] != (byte)'{' ) {
			throw Invalid( offset, "octal escape requires braces" );
		}
		index++;
		SkipBraceHorizontalSpace();
		return new( ParseBracedCharacterValue( offset, 8, "octal" ) );
	}
}
