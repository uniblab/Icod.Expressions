namespace Icod.Expressions.Pcre;

internal static partial class PcreUnicodeScripts {
	internal static void GetRunScripts( int scalar, out int primary, out ulong a, out ulong b, out ulong c ) {
		if ( (uint)scalar > 0x10FFFF ) { throw new ArgumentOutOfRangeException( nameof( scalar ) ); }
		var ranges = RunRanges;
		var low = 0;
		var high = ranges.Length;
		while ( low + 1 < high ) {
			var middle = low + ( high - low ) / 2;
			if ( ( ranges[middle] >> 16 ) <= (uint)scalar ) { low = middle; }
			else { high = middle; }
		}
		var id = (int)( ranges[low] & 0xFFFF );
		primary = RunPrimary[id]; a = RunA[id]; b = RunB[id]; c = RunC[id];
	}
}
