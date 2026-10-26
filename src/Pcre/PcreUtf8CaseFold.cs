using System.Text;

namespace Icod.Expressions.Pcre;

/// <summary>Provisional simple Unicode casing for valid UTF-8 scalar literals.</summary>
internal static class PcreUtf8CaseFold {
	internal static bool Equals( int left, int right ) => left == right || Fold( left ) == Fold( right );

	private static int Fold( int scalar ) {
		// PCRE2's default mode keeps dotted and dotless Turkish I separate from ASCII I/i.
		if ( scalar is 0x0130 or 0x0131 ) { return scalar; }
		// These three simple folds have more than the usual two case equivalents.
		if ( scalar == 0x212A ) { return 'k'; } // Kelvin sign
		if ( scalar == 0x017F ) { return 's'; } // Long s
		if ( scalar == 0x03C2 ) { return 0x03C3; } // Greek final sigma
		return Rune.ToLowerInvariant( Rune.ToUpperInvariant( new Rune( scalar ) ) ).Value;
	}
}
