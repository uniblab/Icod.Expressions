using System.Buffers;
using System.Text;

namespace Icod.Expressions.Pcre;

/// <summary>Strict UTF-8 scalar decoding over the original authoritative byte buffer.</summary>
internal static class PcreUtf8 {
	// Compiled pattern data may contain explicitly allowed surrogate escapes.
	// This encoder never participates in source or subject validation.
	internal static int EncodeCodePoint( int value, Span<byte> destination ) {
		if ( value is < 0xD800 or > 0xDFFF ) { return new Rune( value ).EncodeToUtf8( destination ); }
		destination[0] = (byte)( 0xE0 | ( value >> 12 ) );
		destination[1] = (byte)( 0x80 | ( ( value >> 6 ) & 0x3F ) );
		destination[2] = (byte)( 0x80 | ( value & 0x3F ) );
		return 3;
	}

	internal static bool TryRead( ReadOnlySpan<byte> source, int offset, out int scalar, out int width ) {
		scalar = 0;
		width = 0;
		if ( offset < 0 || offset >= source.Length ) { return false; }
		if ( source[offset] < 0x80 ) {
			scalar = source[offset];
			width = 1;
			return true;
		}
		if ( Rune.DecodeFromUtf8( source[offset..], out var rune, out width ) != OperationStatus.Done ) {
			return false;
		}
		scalar = rune.Value;
		return true;
	}
}
