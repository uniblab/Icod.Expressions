namespace Icod.Expressions.Pcre;

/// <summary>Global newline conventions currently implemented by the shared engine.</summary>
internal enum PcreByteNewline : byte { Lf, AnyCrLf, Cr, Nul }

internal static class PcreByteNewlineExtensions {
	// ANYCRLF retains its additional CR/pair handling at the existing call sites.
	internal static byte Character( this PcreByteNewline newline ) => newline switch {
		PcreByteNewline.Cr => (byte)'\r',
		PcreByteNewline.Nul => 0,
		_ => (byte)'\n'
	};
}
