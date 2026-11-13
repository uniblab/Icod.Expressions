namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private sealed record ParsedAltBsuxLiteralBrace : PcreByteNode;

	private PcreByteNode ParseAltBsuxEscape( byte value, int offset, bool inBracket ) {
		if ( value == (byte)'U' ) { return new PcreByteLiteral( value ); }
		if ( extraAltBsux && value == (byte)'u' && index < source.Length && source[index] == (byte)'{' ) {
			return ParseBracedAltBsuxEscape( offset, inBracket );
		}
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
		if ( utf8 && !allowSurrogateEscapes && scalar is >= 0xD800 and <= 0xDFFF ) {
			throw Invalid( offset, "alternate hex escape is a surrogate code point" );
		}
		return new PcreByteLiteral( scalar );
	}

	private PcreByteNode ParseBracedAltBsuxEscape( int offset, bool inBracket ) {
		var cursor = index + 1;
		uint scalar = 0;
		while ( cursor < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			var digit = HexDigit( source[cursor] );
			if ( digit < 0 ) { break; }
			if ( ( scalar & 0xF0000000u ) != 0 ) {
				throw Invalid( offset, "alternate hex escape exceeds the supported character range" );
			}
			scalar = ( scalar << 4 ) | (uint)digit;
			cursor++;
		}
		if ( cursor == index + 1 || cursor >= source.Length || source[cursor] != (byte)'}' ) {
			if ( inBracket ) { return new PcreByteLiteral( 'u' ); }
			index++; // Consume the literal brace; only it may own a following repeat.
			return new ParsedAltBsuxLiteralBrace();
		}
		index = cursor + 1;
		if ( scalar > ( utf8 ? 0x10FFFFu : 0xFFu ) ) {
			throw Invalid( offset, "alternate hex escape exceeds the supported character range" );
		}
		if ( utf8 && !allowSurrogateEscapes && scalar is >= 0xD800u and <= 0xDFFFu ) {
			throw Invalid( offset, "alternate hex escape is a surrogate code point" );
		}
		return new PcreByteLiteral( (int)scalar );
	}
}
