using System.Text;

namespace Icod.Expressions.Pcre;

/// <summary>Unicode17 simple-case equivalence shared by UTF-8 and byte-UCP matching.</summary>
internal static partial class PcreUtf8CaseFold {
	internal static bool Equals( int left, int right, bool restricted = false ) =>
		left == right || Fold( left, restricted ) == Fold( right, restricted );

	internal static PcreByteScalarRange[] CloseRanges(
		IReadOnlyList<PcreByteScalarRange> ranges, CancellationToken cancellationToken, bool restricted = false
	) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( ranges.Count == 0 ) { return Array.Empty<PcreByteScalarRange>(); }
		var ordered = ranges.ToArray();
		Array.Sort( ordered, static ( left, right ) => left.Start.CompareTo( right.Start ) );
		var merged = new List<PcreByteScalarRange>();
		var current = ordered[0];
		for ( var index = 1; index < ordered.Length; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			var next = ordered[index];
			if ( next.Start <= current.End + 1 ) {
				current = current with { End = Math.Max( current.End, next.End ) };
			} else {
				merged.Add( current );
				current = next;
			}
		}
		merged.Add( current );
		if ( merged.Count == 1 && merged[0] is { Start: 0, End: 0x10FFFF } ) {
			return [merged[0]]; // Every valid subject scalar is already in this interval.
		}
		const int scalarCount = 0x110000;
		var bitmap = new uint[scalarCount / 32]; // 136 KiB, independent of range width.
		var minimum = scalarCount;
		var maximum = -1;
		foreach ( var range in merged ) {
			for ( var scalar = range.Start; scalar <= range.End; scalar++ ) {
				if ( ( scalar & 1023 ) == 0 ) { cancellationToken.ThrowIfCancellationRequested(); }
				if ( !Rune.IsValid( scalar ) ) { continue; } // A range may straddle the surrogate gap.
				var key = Fold( scalar, restricted );
				bitmap[key >> 5] |= 1u << ( key & 31 );
				minimum = Math.Min( minimum, key );
				maximum = Math.Max( maximum, key );
			}
		}
		cancellationToken.ThrowIfCancellationRequested();
		var intervals = new List<PcreByteScalarRange>();
		var intervalStart = -1;
		for ( var scalar = minimum; scalar <= maximum; scalar++ ) {
			if ( ( scalar & 1023 ) == 0 ) { cancellationToken.ThrowIfCancellationRequested(); }
			var present = ( bitmap[scalar >> 5] & ( 1u << ( scalar & 31 ) ) ) != 0;
			if ( present && intervalStart < 0 ) { intervalStart = scalar; }
			if ( !present && intervalStart >= 0 ) {
				intervals.Add( new( intervalStart, scalar - 1 ) );
				intervalStart = -1;
			}
		}
		if ( intervalStart >= 0 ) { intervals.Add( new( intervalStart, maximum ) ); }
		return intervals.ToArray();
	}

	internal static int Fold( int scalar, bool restricted = false ) {
		var key = FoldUnrestricted( scalar );
		// The pinned default cross-boundary sets each have one non-ASCII member.
		return restricted && scalar >= 0x80 && key < 0x80 ? scalar : key;
	}

	private static int FoldUnrestricted( int scalar ) {
		if ( (uint)scalar < 128 ) { return scalar is >= 'A' and <= 'Z' ? scalar + 32 : scalar; }
		if ( !Rune.IsValid( scalar ) ) { throw new ArgumentOutOfRangeException( nameof( scalar ) ); }
		var mappings = UnicodeMappings;
		// Most of the scalar domain has no case mapping. Avoid searching beyond
		// the sorted source bounds, especially during broad range closure.
		if ( scalar < (int)( mappings[0] >> 21 ) || scalar > (int)( mappings[^1] >> 21 ) ) { return scalar; }
		var low = 0;
		var high = mappings.Length - 1;
		while ( low <= high ) {
			var middle = low + ( high - low ) / 2;
			var mapping = mappings[middle];
			var source = (int)( mapping >> 21 );
			if ( source < scalar ) { low = middle + 1; }
			else if ( source > scalar ) { high = middle - 1; }
			else { return (int)( mapping & 0x1FFFFFUL ); }
		}
		return scalar; // Includes default dotted/dotless Turkish I; T mappings are not admitted.
	}
}
