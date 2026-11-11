namespace Icod.Expressions.Pcre;

internal static partial class PcreByteMatcher {
	private enum RunState { Unset, Intersection, Han, Japanese, Mandarin, Korean }

	private static bool RunHas( ulong a, ulong b, ulong c, int script ) =>
		( ( script < 64 ? a : script < 128 ? b : c ) & ( 1UL << ( script & 63 ) ) ) != 0;

	private static RunState FirstRunState( int script ) => script switch {
		PcreUnicodeScripts.RunHan => RunState.Han,
		PcreUnicodeScripts.RunHiragana or PcreUnicodeScripts.RunKatakana => RunState.Japanese,
		PcreUnicodeScripts.RunBopomofo => RunState.Mandarin,
		PcreUnicodeScripts.RunHangul => RunState.Korean,
		_ => RunState.Intersection
	};

	private static bool IsScriptRun( ReadOnlyMemory<byte> input, int start, int end, bool utf8,
		ref int work, int matchWorkLimit, Action<int>? observer, CancellationToken cancellationToken ) {
		if ( start >= end ) { return true; }
		ChargeWork( ref work, matchWorkLimit, observer, cancellationToken );
		var scalar = (int)input.Span[start];
		var width = 1;
		if ( utf8 && !PcreUtf8.TryRead( input.Span, start, out scalar, out width ) || width > end - start ) { return false; }
		var cursor = start + width;
		if ( cursor == end ) { return true; } // Native permits Unknown only in a singleton.
		var state = RunState.Unset;
		ulong requiredA = 0, requiredB = 0, requiredC = 0;
		var requiredDigits = -1;
		while ( true ) {
			ChargeWork( ref work, matchWorkLimit, observer, cancellationToken );
			PcreUnicodeScripts.GetRunScripts( scalar, out var primary, out var a, out var b, out var c );
			if ( primary == PcreUnicodeScripts.RunUnknown ) { return false; }
			if ( ( a | b | c ) != 0 ) {
				switch ( state ) {
					case RunState.Unset:
						state = FirstRunState( primary );
						requiredA = a; requiredB = b; requiredC = c;
						break;
					case RunState.Intersection:
						if ( ( ( requiredA & a ) | ( requiredB & b ) | ( requiredC & c ) ) == 0 ) { return false; }
						state = FirstRunState( primary );
						if ( state == RunState.Intersection ) { requiredA &= a; requiredB &= b; requiredC &= c; }
						break;
					case RunState.Han:
						if ( primary == PcreUnicodeScripts.RunHan ) { break; }
						var special = ( RunHas( a, b, c, PcreUnicodeScripts.RunBopomofo ) ? 1 : 0 )
							| ( RunHas( a, b, c, PcreUnicodeScripts.RunHiragana ) ? 2 : 0 )
							| ( RunHas( a, b, c, PcreUnicodeScripts.RunKatakana ) ? 4 : 0 )
							| ( RunHas( a, b, c, PcreUnicodeScripts.RunHangul ) ? 8 : 0 );
						if ( special == 0 ) { return false; }
						// Preserve native directional transitions, including the pending
						// state for singleton Hiragana/Katakana/Hangul maps after Han.
						if ( special == 1 ) { state = RunState.Mandarin; }
						else if ( special == 6 ) { state = RunState.Japanese; }
						break;
					case RunState.Japanese:
						if ( !RunHas( a, b, c, PcreUnicodeScripts.RunHan ) && !RunHas( a, b, c, PcreUnicodeScripts.RunHiragana )
							&& !RunHas( a, b, c, PcreUnicodeScripts.RunKatakana ) ) { return false; }
						break;
					case RunState.Mandarin:
						if ( !RunHas( a, b, c, PcreUnicodeScripts.RunHan ) && !RunHas( a, b, c, PcreUnicodeScripts.RunBopomofo ) ) { return false; }
						break;
					case RunState.Korean:
						if ( !RunHas( a, b, c, PcreUnicodeScripts.RunHan ) && !RunHas( a, b, c, PcreUnicodeScripts.RunHangul ) ) { return false; }
						break;
				}
			}
			var digits = PcreUnicodeCategories.GetDecimalDigitSet( scalar );
			if ( digits >= 0 ) {
				if ( requiredDigits >= 0 && requiredDigits != digits ) { return false; }
				requiredDigits = digits;
			}
			if ( cursor >= end ) { return true; }
			ChargeWork( ref work, matchWorkLimit, observer, cancellationToken );
			scalar = input.Span[cursor]; width = 1;
			if ( utf8 && !PcreUtf8.TryRead( input.Span, cursor, out scalar, out width ) || width > end - cursor ) { return false; }
			cursor += width;
		}
	}
}
