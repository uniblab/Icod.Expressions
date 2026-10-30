namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private void ParseStartDirectives() {
		// PCRE processes only an uninterrupted prefix before any ordinary
		// options or trivia. Keep the original array for body diagnostics.
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( TryConsumeStartDirective( "(*LF)"u8 ) ) { anyCrLfNewline = false; }
			else if ( TryConsumeStartDirective( "(*ANYCRLF)"u8 ) ) { anyCrLfNewline = true; }
			else if ( TryConsumeStartDirective( "(*BSR_UNICODE)"u8 ) ) { BsrAnyCrLf = false; }
			else if ( TryConsumeStartDirective( "(*BSR_ANYCRLF)"u8 ) ) { BsrAnyCrLf = true; }
			else { break; }
		}
	}

	private bool TryConsumeStartDirective( ReadOnlySpan<byte> directive ) {
		if ( !source.AsSpan( index ).StartsWith( directive ) ) { return false; }
		index += directive.Length;
		return true;
	}
}
