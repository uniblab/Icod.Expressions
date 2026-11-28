namespace Icod.Expressions.Pcre;

internal sealed partial class PcreCompiledWordPattern {
	// Negative: exhausted; zero: invalid run; positive: valid run.
	private int CheckScriptRun( ReadOnlySpan<char> source, int start, int end,
		ref int work, CancellationToken cancellationToken ) {
		if ( start >= end ) { return 1; }
		if ( !ChargeWork( ref work, cancellationToken ) ) { return -1; }
		if ( !PcreUtf16.TryRead( source[..end], start, options.Utf16, out var scalar, out var width ) ) { return 0; }
		var cursor = start + width;
		if ( cursor == end ) { return 1; } // Native permits Unknown only in a singleton.
		var state = new PcreScriptRunState();
		while ( true ) {
			if ( !ChargeWork( ref work, cancellationToken ) ) { return -1; }
			if ( !state.TryAdd( scalar ) ) { return 0; }
			if ( cursor >= end ) { return 1; }
			if ( !ChargeWork( ref work, cancellationToken ) ) { return -1; }
			if ( !PcreUtf16.TryRead( source[..end], cursor, options.Utf16, out scalar, out width ) ) { return 0; }
			cursor += width;
		}
	}
}
