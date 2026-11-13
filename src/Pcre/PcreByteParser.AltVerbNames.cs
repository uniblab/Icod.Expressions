namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private ReadOnlyMemory<byte> ReadAlternateControlVerbName( string verb, bool requireName ) {
		var start = index;
		var name = new List<byte>();
		var quoting = false;
		while ( index < source.Length ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( !quoting ) { SkipIgnoredPatternText( allowInlineComments: false ); }
			if ( index >= source.Length ) { break; }
			var value = source[index];
			if ( value == (byte)'\\' && index + 1 < source.Length ) {
				if ( source[index + 1] == (byte)'E' ) { index += 2; quoting = false; continue; }
				if ( !quoting && source[index + 1] == (byte)'Q' ) { index += 2; quoting = true; continue; }
			}
			if ( !quoting && value == (byte)')' ) { break; }
			if ( !quoting && value == (byte)'\\' ) {
				var opening = index++;
				AppendControlVerbEscape( name, opening );
				continue;
			}
			var width = 1;
			if ( utf8 && value >= 0x80 ) { PcreUtf8.TryRead( source, index, out _, out width ); }
			for ( var count = 0; count < width; count++ ) { name.Add( source[index++] ); }
		}
		if ( index == source.Length ) { throw Invalid( index, String.Concat( "unterminated ", verb, " name" ) ); }
		var rawLength = index - start;
		if ( rawLength > 255 || requireName && rawLength == 0 ) {
			throw Invalid( index, String.Concat( verb, requireName ? " name must contain 1 to 255 bytes" : " name must contain at most 255 bytes" ) );
		}
		index++;
		if ( rawLength == 0 ) { return ReadOnlyMemory<byte>.Empty; }
		// An empty decoded name remains a named argument, like a raw NUL name.
		return name.Count == 0 ? new byte[] { 0 } : name.ToArray();
	}

	private void AppendControlVerbEscape( List<byte> name, int offset ) {
		if ( index >= source.Length ) { throw Invalid( offset, "trailing backslash in control-verb name" ); }
		var value = source[index];
		if ( utf8 && value >= 0x80 ) {
			PcreUtf8.TryRead( source, index, out var scalar, out var width );
			index += width;
			AppendControlVerbScalar( name, scalar );
			return;
		}
		var letter = value is >= (byte)'a' and <= (byte)'z' or >= (byte)'A' and <= (byte)'Z';
		var literalLetter = value is (byte)'a' or (byte)'c' or (byte)'e' or (byte)'f' or (byte)'n'
			or (byte)'r' or (byte)'t' or (byte)'o' or (byte)'x' or (byte)'N' or (byte)'u' or (byte)'U';
		if ( letter && !literalLetter ) { throw Invalid( offset, "control-verb name escapes must be literal" ); }
		PcreByteNode escaped;
		try { escaped = ParseEscape( offset ); }
		catch ( PcreByteParseException error ) when ( error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax ) {
			throw Invalid( offset, "control-verb name escape is not recognized" );
		}
		if ( escaped is PcreByteLiteral literal ) { AppendControlVerbScalar( name, literal.Value ); }
		else if ( escaped is ParsedAltBsuxLiteralBrace ) { name.Add( (byte)'u' ); name.Add( (byte)'{' ); }
		else { throw Invalid( offset, "control-verb name escapes must be literal" ); }
	}

	private void AppendControlVerbScalar( List<byte> name, int scalar ) {
		if ( !utf8 ) { name.Add( (byte)scalar ); return; }
		Span<byte> encoded = stackalloc byte[4];
		var width = PcreUtf8.EncodeCodePoint( scalar, encoded );
		for ( var count = 0; count < width; count++ ) { name.Add( encoded[count] ); }
	}
}
