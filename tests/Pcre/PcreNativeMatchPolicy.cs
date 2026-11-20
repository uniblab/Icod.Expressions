namespace Icod.Expressions.Tests.Pcre;

internal static class PcreNativeMatchPolicy {
	internal static PCRE.PcreMatchOptions Options( int defaults, int mask ) {
		// Anchor defaults belong to compilation. Reapplying them as call flags
		// changes native dispatch. The other defaults are match-only restrictions.
		var effective = mask | ( defaults & ~3 );
		return (PCRE.PcreMatchOptions)(
			( ( effective & 1 ) != 0 ? 0x80000000u : 0 )
			| ( ( effective & 2 ) != 0 ? 0x20000000u : 0 )
			| ( ( effective & 4 ) != 0 ? 4u : 0 )
			| ( ( effective & 8 ) != 0 ? 8u : 0 )
			| ( ( effective & 16 ) != 0 ? 1u : 0 )
			| ( ( effective & 32 ) != 0 ? 2u : 0 )
		);
	}
}
