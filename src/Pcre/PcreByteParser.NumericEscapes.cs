namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private PcreByteNode ParseDecimalOrOctalEscape( byte first, int offset, bool inBracket ) {
		if ( first == (byte)'0' || inBracket ) { return ParseUnbracedOctal( first, offset ); }
		var number = first - (byte)'0';
		var cursor = index;
		while ( cursor < source.Length && source[cursor] is >= (byte)'0' and <= (byte)'9' ) {
			cancellationToken.ThrowIfCancellationRequested();
			// Beyond the library's capture ceiling, only the first digit can still
			// force reference syntax. Saturation preserves that decision without overflow.
			number = Math.Min( MaximumCaptureCount + 1, number * 10 + source[cursor++] - (byte)'0' );
		}
		if ( number < 10 || first >= (byte)'8' || number <= groupCount ) {
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
		for ( var digits = 1; digits < 3 && index < source.Length
			&& source[index] is >= (byte)'0' and <= (byte)'7'; digits++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			value = value * 8 + source[index++] - (byte)'0';
		}
		if ( !utf8 && value > 255 ) { throw Invalid( offset, "octal escape exceeds the supported character range" ); }
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
