namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private PcreByteNode ParseAltBsuxEscape( byte value, int offset ) {
		if ( value == (byte)'U' ) { return new PcreByteLiteral( value ); }
		var digitCount = value == (byte)'u' ? 4 : 2;
		var cursor = index;
		var scalar = 0;
		for ( var count = 0; count < digitCount; count++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( cursor >= source.Length ) { return new PcreByteLiteral( value ); }
			var digit = HexDigit( source[cursor++] );
			if ( digit < 0 ) { return new PcreByteLiteral( value ); }
			scalar = ( scalar << 4 ) | digit;
		}
		// Incomplete tokens leave every following byte to the ordinary grammar.
		index = cursor;
		if ( !utf8 && scalar > 0xFF ) {
			throw Invalid( offset, "alternate hex escape exceeds the supported character range" );
		}
		if ( utf8 && scalar is >= 0xD800 and <= 0xDFFF ) {
			throw Invalid( offset, "alternate hex escape is a surrogate code point" );
		}
		return new PcreByteLiteral( scalar );
	}
}
