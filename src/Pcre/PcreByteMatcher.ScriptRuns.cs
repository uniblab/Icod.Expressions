namespace Icod.Expressions.Pcre;

internal static partial class PcreByteMatcher {
	private static bool IsScriptRun( ReadOnlyMemory<byte> input, int start, int end, bool utf8,
		ref int work, int matchWorkLimit, Action<int>? observer, CancellationToken cancellationToken ) {
		if ( start >= end ) { return true; }
		ChargeWork( ref work, matchWorkLimit, observer, cancellationToken );
		var scalar = (int)input.Span[start];
		var width = 1;
		if ( utf8 && !PcreUtf8.TryRead( input.Span, start, out scalar, out width ) || width > end - start ) { return false; }
		var cursor = start + width;
		if ( cursor == end ) { return true; } // Native permits Unknown only in a singleton.
		var state = new PcreScriptRunState();
		while ( true ) {
			ChargeWork( ref work, matchWorkLimit, observer, cancellationToken );
			if ( !state.TryAdd( scalar ) ) { return false; }
			if ( cursor >= end ) { return true; }
			ChargeWork( ref work, matchWorkLimit, observer, cancellationToken );
			scalar = input.Span[cursor]; width = 1;
			if ( utf8 && !PcreUtf8.TryRead( input.Span, cursor, out scalar, out width ) || width > end - cursor ) { return false; }
			cursor += width;
		}
	}
}
