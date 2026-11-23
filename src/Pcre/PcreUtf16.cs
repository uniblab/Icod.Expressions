using System.Buffers;
using System.Text;

namespace Icod.Expressions.Pcre;

/// <summary>Width-16 code-unit and scalar navigation over the original input view.</summary>
internal static class PcreUtf16 {
	internal static bool TryRead( ReadOnlySpan<char> source, int offset, bool utf, out int value, out int width ) {
		value = width = 0;
		if ( (uint)offset >= (uint)source.Length ) { return false; }
		if ( !utf ) { value = source[offset]; width = 1; return true; }
		if ( Rune.DecodeFromUtf16( source[offset..], out var rune, out var consumed ) != OperationStatus.Done ) { return false; }
		value = rune.Value;
		width = consumed;
		return true;
	}

	// end is exclusive, in the supplied view's native code units. A caller can
	// pass a bounded fragment without exposing its surrounding storage.
	internal static bool TryReadPrevious( ReadOnlySpan<char> source, int end, bool utf, out int value, out int width ) {
		value = width = 0;
		if ( end == 0 || (uint)end > (uint)source.Length ) { return false; }
		if ( !utf ) { value = source[end - 1]; width = 1; return true; }
		if ( Rune.DecodeLastFromUtf16( source[..end], out var rune, out var consumed ) != OperationStatus.Done ) { return false; }
		value = rune.Value;
		width = consumed;
		return true;
	}

	// Compiled literal data may explicitly admit surrogate code points. Writing
	// one here does not make it valid strict UTF source or subject text.
	internal static int EncodeCodePoint( int value, Span<char> destination ) {
		if ( (uint)value > 0x10FFFF ) { throw new ArgumentOutOfRangeException( nameof( value ) ); }
		var width = value <= 0xFFFF ? 1 : 2;
		if ( destination.Length < width ) { throw new ArgumentException( "Destination is too short for the code point.", nameof( destination ) ); }
		if ( width == 1 ) { destination[0] = (char)value; return 1; }
		return new Rune( value ).EncodeToUtf16( destination );
	}
}
