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

	internal static bool Joins( PcreGraphemeBreak left, PcreGraphemeBreak right, bool pictographBeforeZwj ) {
		if ( left == PcreGraphemeBreak.Cr ) { return right == PcreGraphemeBreak.Lf; }
		if ( left is PcreGraphemeBreak.Lf or PcreGraphemeBreak.Control ) { return false; }
		// Native RI does not take Extend/SpacingMark/ZWJ; preserve that table exception.
		if ( left == PcreGraphemeBreak.RegionalIndicator ) { return right == PcreGraphemeBreak.RegionalIndicator; }
		if ( right is PcreGraphemeBreak.Extend or PcreGraphemeBreak.SpacingMark or PcreGraphemeBreak.Zwj ) { return true; }
		return left switch {
			PcreGraphemeBreak.Prepend => right is not ( PcreGraphemeBreak.Cr or PcreGraphemeBreak.Lf
				or PcreGraphemeBreak.Control or PcreGraphemeBreak.ExtendedPictographic ),
			PcreGraphemeBreak.L => right is PcreGraphemeBreak.L or PcreGraphemeBreak.V or PcreGraphemeBreak.Lv or PcreGraphemeBreak.Lvt,
			PcreGraphemeBreak.V or PcreGraphemeBreak.Lv => right is PcreGraphemeBreak.V or PcreGraphemeBreak.T,
			PcreGraphemeBreak.T or PcreGraphemeBreak.Lvt => right == PcreGraphemeBreak.T,
			PcreGraphemeBreak.Zwj => pictographBeforeZwj && right == PcreGraphemeBreak.ExtendedPictographic,
			_ => false
		};
	}
}
