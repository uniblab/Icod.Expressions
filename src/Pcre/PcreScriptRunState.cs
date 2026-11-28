namespace Icod.Expressions.Pcre;

// Shared Unicode/script-extension intersection for an already decoded scalar.
internal struct PcreScriptRunState {
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

	private RunState state;
	private ulong requiredA, requiredB, requiredC;
	private int digitSet; // Zero is unset; stored identities are offset by one.

	internal bool TryAdd( int scalar ) {
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
			if ( digitSet != 0 && digitSet != digits + 1 ) { return false; }
			digitSet = digits + 1;
		}
		return true;
	}
}
