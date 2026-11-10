namespace Icod.Expressions.Pcre;

/// <summary>Pinned Unicode17 native binary predicates and bidirectional classes.</summary>
internal static partial class PcreUnicodeProperties {
	internal static bool TryGetBinary( string normalizedName, out int property ) {
		var index = Array.BinarySearch( BinaryNames, normalizedName, StringComparer.Ordinal );
		property = index >= 0 ? BinaryIds[index] : -1;
		return index >= 0;
	}

	internal static bool TryGetBidi( string normalizedValue, out int property ) {
		property = Array.BinarySearch( BidiNames, normalizedValue, StringComparer.Ordinal );
		return property >= 0;
	}

	internal static bool Contains( int scalar, int property, bool bidi ) {
		if ( (uint)property >= ( bidi ? BidiCount : BinaryCount ) ) { throw new ArgumentOutOfRangeException( nameof( property ) ); }
		var offsets = bidi ? BidiOffsets : BinaryOffsets;
		var ranges = bidi ? BidiRanges : BinaryRanges;
		var low = (int)offsets[property];
		var high = offsets[property + 1] - 1;
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
