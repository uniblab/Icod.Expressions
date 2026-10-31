namespace Icod.Expressions.Pcre;

/// <summary>Global prefix choices resolved once before whole-pattern validation.</summary>
internal readonly record struct PcreByteStartSettings(
	bool Utf8, bool Ucp, PcreByteNewline Newline, bool BsrAnyCrLf, bool NotEmpty, bool NotEmptyAtStart, int BodyOffset
) {
	internal static PcreByteStartSettings Parse( ReadOnlySpan<byte> source, PcreByteOptions options,
		CancellationToken cancellationToken ) {
		var utf8 = options.Utf8 || options.MatchInvalidUtf;
		var ucp = options.Ucp;
		var newline = options.AnyCrLfNewline ? PcreByteNewline.AnyCrLf : PcreByteNewline.Lf;
		var bsrAnyCrLf = options.BsrAnyCrLf;
		var notEmpty = false;
		var notEmptyAtStart = false;
		var offset = 0;
		// PCRE processes an uninterrupted prefix before ordinary options/trivia.
		// Keep the original source and body index for validation and diagnostics.
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( TryConsume( source, ref offset, "(*UTF)"u8 )
				|| TryConsume( source, ref offset, "(*UTF8)"u8 ) ) { utf8 = true; }
			else if ( TryConsume( source, ref offset, "(*UCP)"u8 ) ) { ucp = true; }
			else if ( TryConsume( source, ref offset, "(*LF)"u8 ) ) { newline = PcreByteNewline.Lf; }
			else if ( TryConsume( source, ref offset, "(*ANYCRLF)"u8 ) ) { newline = PcreByteNewline.AnyCrLf; }
			else if ( TryConsume( source, ref offset, "(*CR)"u8 ) ) { newline = PcreByteNewline.Cr; }
			else if ( TryConsume( source, ref offset, "(*NUL)"u8 ) ) { newline = PcreByteNewline.Nul; }
			else if ( TryConsume( source, ref offset, "(*CRLF)"u8 ) ) { newline = PcreByteNewline.CrLf; }
			else if ( TryConsume( source, ref offset, "(*ANY)"u8 ) ) { newline = PcreByteNewline.Any; }
			else if ( TryConsume( source, ref offset, "(*BSR_UNICODE)"u8 ) ) { bsrAnyCrLf = false; }
			else if ( TryConsume( source, ref offset, "(*BSR_ANYCRLF)"u8 ) ) { bsrAnyCrLf = true; }
			else if ( TryConsume( source, ref offset, "(*NOTEMPTY)"u8 ) ) { notEmpty = true; }
			else if ( TryConsume( source, ref offset, "(*NOTEMPTY_ATSTART)"u8 ) ) { notEmptyAtStart = true; }
			else { break; }
		}
		return new( utf8, ucp, newline, bsrAnyCrLf, notEmpty, notEmptyAtStart, offset );
	}

	private static bool TryConsume( ReadOnlySpan<byte> source, ref int offset, ReadOnlySpan<byte> directive ) {
		if ( !source[offset..].StartsWith( directive ) ) { return false; }
		offset += directive.Length;
		return true;
	}
}
