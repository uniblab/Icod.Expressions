namespace Icod.Expressions.Pcre;

internal enum PcreGraphemeBreak : byte {
	Cr, Lf, Control, Extend, Prepend, SpacingMark, L, V, T, Lv, Lvt,
	RegionalIndicator, Other, Zwj, ExtendedPictographic
}

/// <summary>Pinned native grapheme break classification, independent of host Unicode services.</summary>
internal static partial class PcreUnicodeGraphemes {
	internal static PcreGraphemeBreak GetBreak( int scalar ) {
		if ( (uint)scalar > 0x10FFFF ) { throw new ArgumentOutOfRangeException( nameof( scalar ) ); }
		if ( scalar < 128 ) { return (PcreGraphemeBreak)AsciiKinds[scalar]; }
		var ranges = EndKinds;
		var low = 0;
		var high = ranges.Length - 1;
		while ( low < high ) {
			var middle = low + ( high - low ) / 2;
			if ( ( ranges[middle] >> 4 ) < scalar ) { low = middle + 1; }
			else { high = middle; }
		}
		return (PcreGraphemeBreak)( ranges[low] & 15 );
	}
}
