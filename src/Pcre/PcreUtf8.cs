using System.Buffers;
using System.Text;

namespace Icod.Expressions.Pcre;

/// <summary>Strict UTF-8 scalar decoding over the original authoritative byte buffer.</summary>
internal static class PcreUtf8 {
	internal static bool TryRead( ReadOnlySpan<byte> source, int offset, out int scalar, out int width ) {
		scalar = 0;
		width = 0;
		if ( offset < 0 || offset >= source.Length ) { return false; }
		if ( Rune.DecodeFromUtf8( source[offset..], out var rune, out width ) != OperationStatus.Done ) {
			return false;
		}
		scalar = rune.Value;
		return true;
	}
}
