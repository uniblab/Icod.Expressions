using System.Globalization;

namespace Icod.Expressions.Pcre;

/// <summary>Unicode17 general categories, independent of the host runtime and OS.</summary>
internal static partial class PcreUnicodeCategories {
	// Callers supply decoded scalars or same-valued bytes. Surrogate entries in
	// the source table are retained for reproducibility, never consumed as UTF.
	internal static UnicodeCategory GetCategory( int scalar ) {
		if ( (uint)scalar < 128 ) { return (UnicodeCategory)AsciiCategories[scalar]; }
		if ( (uint)scalar > 0x10FFFF ) { throw new ArgumentOutOfRangeException( nameof( scalar ) ); }
		var ranges = CategoryRanges;
		var low = 0;
		var high = ranges.Length;
		// Find the last range start <= scalar. Range zero starts at U+0080.
		while ( low + 1 < high ) {
			var middle = low + ( high - low ) / 2;
			if ( ( ranges[middle] >> 5 ) <= (uint)scalar ) { low = middle; }
			else { high = middle; }
		}
		return (UnicodeCategory)( ranges[low] & 31 );
	}

	// Adjacent decimal families share one category interval (for example the
	// mathematical digits). Each family still occupies exactly ten code points.
	internal static int GetDecimalDigitSet( int scalar ) {
		if ( (uint)scalar < 128 ) { return scalar is >= '0' and <= '9' ? '0' : -1; }
		if ( (uint)scalar > 0x10FFFF ) { throw new ArgumentOutOfRangeException( nameof( scalar ) ); }
		var ranges = CategoryRanges;
		var low = 0;
		var high = ranges.Length;
		while ( low + 1 < high ) {
			var middle = low + ( high - low ) / 2;
			if ( ( ranges[middle] >> 5 ) <= (uint)scalar ) { low = middle; }
			else { high = middle; }
		}
		if ( (UnicodeCategory)( ranges[low] & 31 ) != UnicodeCategory.DecimalDigitNumber ) { return -1; }
		return scalar - ( scalar - (int)( ranges[low] >> 5 ) ) % 10;
	}
}
