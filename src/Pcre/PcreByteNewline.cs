namespace Icod.Expressions.Pcre;

/// <summary>Global newline conventions currently implemented by the shared engine.</summary>
internal enum PcreByteNewline : byte { Lf, AnyCrLf, Cr, Nul, CrLf, Any }

internal static class PcreByteNewlineExtensions {
	// Sequence-aware conventions retain their explicit pair handling at call sites.
	internal static byte Character( this PcreByteNewline newline ) => newline switch {
		PcreByteNewline.Cr => (byte)'\r',
		PcreByteNewline.Nul => 0,
		_ => (byte)'\n'
	};

	internal static bool IsCrLf( ReadOnlySpan<byte> input, int position, int end ) =>
		position >= 0 && position < end - 1 && input[position] == (byte)'\r' && input[position + 1] == (byte)'\n';

	internal static int AnyLengthAt( ReadOnlySpan<byte> input, int position, int end, bool utf8 ) {
		if ( position < 0 || position >= end ) { return 0; }
		var scalar = (int)input[position];
		var width = 1;
		if ( utf8 && ( !PcreUtf8.TryRead( input, position, out scalar, out width ) || width > end - position ) ) { return 0; }
		if ( !PcreSpaceCharacters.IsVertical( scalar ) ) { return 0; }
		return scalar == '\r' && IsCrLf( input, position, end ) ? 2 : width;
	}

	internal static bool IsAnyBefore( ReadOnlySpan<byte> input, int position, int start, bool utf8 ) {
		var previous = position - 1;
		if ( previous < start ) { return false; }
		if ( utf8 ) {
			// A UTF scalar has at most three continuation bytes; never cross a fragment.
			for ( var count = 0; count < 3 && previous > start && ( input[previous] & 0xC0 ) == 0x80; count++ ) {
				previous--;
			}
		}
		var length = AnyLengthAt( input, previous, position, utf8 );
		return length > 0 && length == position - previous;
	}
}
