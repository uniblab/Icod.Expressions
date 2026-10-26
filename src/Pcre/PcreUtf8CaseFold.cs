using System.Text;

namespace Icod.Expressions.Pcre;

/// <summary>Provisional Unicode scalar equivalence for the internal UTF-8 matcher.</summary>
internal static class PcreUtf8CaseFold {
	internal static bool Equals( int left, int right ) => left == right || Fold( left ) == Fold( right );

	internal static PcreByteScalarRange[] CloseRanges(
		IReadOnlyList<PcreByteScalarRange> ranges, CancellationToken cancellationToken
	) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( ranges.Count == 0 ) { return Array.Empty<PcreByteScalarRange>(); }
		if ( ranges.Count == 1 && ranges[0] is { Start: 0, End: 0x10FFFF } ) {
			return [ranges[0]]; // Every valid subject scalar is already in this interval.
		}
		const int scalarCount = 0x110000;
		var bitmap = new uint[scalarCount / 32]; // 136 KiB, independent of range width.
		var minimum = scalarCount;
		var maximum = -1;
		foreach ( var range in ranges ) {
			for ( var scalar = range.Start; scalar <= range.End; scalar++ ) {
				if ( ( scalar & 1023 ) == 0 ) { cancellationToken.ThrowIfCancellationRequested(); }
				if ( !Rune.IsValid( scalar ) ) { continue; } // A range may straddle the surrogate gap.
				var key = Fold( scalar );
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

	internal static int Fold( int scalar ) {
		// PCRE2's default mode keeps dotted and dotless Turkish I separate from ASCII I/i.
		if ( scalar is 0x0130 or 0x0131 ) { return scalar; }
		// These three simple folds have more than the usual two case equivalents.
		if ( scalar == 0x212A ) { return 'k'; } // Kelvin sign
		if ( scalar == 0x017F ) { return 's'; } // Long s
		if ( scalar == 0x03C2 ) { return 0x03C3; } // Greek final sigma
		return Rune.ToLowerInvariant( Rune.ToUpperInvariant( new Rune( scalar ) ) ).Value;
	}
}
