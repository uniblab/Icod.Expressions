namespace Icod.Expressions.Pcre;

/// <summary>Pinned Unicode17 primary and native primary-or-extension script membership.</summary>
internal static partial class PcreUnicodeScripts {
	// The parser supplies existing loose-normalized ASCII names.
	internal static bool TryGetScript( string normalizedName, out int script ) {
		var index = Array.BinarySearch( NormalizedNames, normalizedName, StringComparer.Ordinal );
		script = index >= 0 ? NameScripts[index] : -1;
		return index >= 0;
	}

	internal static bool Contains( int scalar, int script, bool extensions ) {
		if ( (uint)script >= ScriptCount ) { throw new ArgumentOutOfRangeException( nameof( script ) ); }
		var offsets = extensions ? ExtendedOffsets : PrimaryOffsets;
		var ranges = extensions ? ExtendedRanges : PrimaryRanges;
		var low = (int)offsets[script];
		var high = offsets[script + 1] - 1;
		if ( low > high || scalar < (int)( ranges[low] >> 21 ) || scalar > (int)( ranges[high] & 0x1FFFFFUL ) ) { return false; }
		while ( low <= high ) {
			var middle = low + ( high - low ) / 2;
			var range = ranges[middle];
			if ( scalar < (int)( range >> 21 ) ) { high = middle - 1; }
			else if ( scalar > (int)( range & 0x1FFFFFUL ) ) { low = middle + 1; }
			else { return true; }
		}
		return false;
	}
}
