namespace Icod.Expressions.Pcre;

/// <summary>Global newline conventions currently implemented by the shared engine.</summary>
internal enum PcreByteNewline : byte { Lf, AnyCrLf, Cr, Nul, CrLf }

internal static class PcreByteNewlineExtensions {
	// Sequence-aware conventions retain their explicit pair handling at call sites.
	internal static byte Character( this PcreByteNewline newline ) => newline switch {
		PcreByteNewline.Cr => (byte)'\r',
		PcreByteNewline.Nul => 0,
		_ => (byte)'\n'
	};

	internal static bool IsCrLf( ReadOnlySpan<byte> input, int position, int end ) =>
		position >= 0 && position < end - 1 && input[position] == (byte)'\r' && input[position + 1] == (byte)'\n';
}
