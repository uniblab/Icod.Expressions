namespace Icod.Expressions.Pcre;

internal sealed partial class PcreCompiledWordPattern {
	// Match-local, input-derived context. No capture or retry state is memoized.
	private readonly record struct RegionalRun( int Start, int End );

	private bool ChargeWork( ref int work, CancellationToken cancellationToken ) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( work >= options.MatchWorkLimit ) { return false; }
		work++;
		options.WorkObserver?.Invoke( work );
		cancellationToken.ThrowIfCancellationRequested();
		return true;
	}

	// Positive: consumed units; zero: no atom; negative: work exhausted.
	private int ConsumeGrapheme( ReadOnlySpan<char> source, int start, int reverseStart,
		ref RegionalRun memo, ref int work, CancellationToken cancellationToken ) {
		if ( !ChargeWork( ref work, cancellationToken ) ) { return -1; }
		if ( !PcreUtf16.TryRead( source, start, options.Utf16, out var scalar, out var width ) ) { return 0; }
		var left = PcreUnicodeGraphemes.GetBreak( scalar );
		var leftStart = start;
		var cursor = start + width;
		var pictographBeforeZwj = false;
		while ( cursor < source.Length ) {
			if ( !ChargeWork( ref work, cancellationToken ) ) { return -1; } // Charge lookahead even at a break.
			if ( !PcreUtf16.TryRead( source, cursor, options.Utf16, out scalar, out width ) ) { break; }
			var right = PcreUnicodeGraphemes.GetBreak( scalar );
			if ( !PcreUnicodeGraphemes.Joins( left, right, pictographBeforeZwj ) ) { break; }
			if ( left == PcreGraphemeBreak.RegionalIndicator && right == PcreGraphemeBreak.RegionalIndicator ) {
				var runStart = FindRegionalRunStart( source, leftStart, cursor, reverseStart, ref memo, ref work, cancellationToken );
				if ( runStart < 0 ) { return -1; }
				// Regional indicators occupy two units in UTF-16 and cannot occur in raw words.
				if ( ( ( leftStart - runStart ) / 2 & 1 ) != 0 ) { break; }
			}
			pictographBeforeZwj = left == PcreGraphemeBreak.ExtendedPictographic && right == PcreGraphemeBreak.Zwj;
			if ( left != PcreGraphemeBreak.ExtendedPictographic || right != PcreGraphemeBreak.Extend ) { left = right; }
			leftStart = cursor;
			cursor += width;
		}
		return cursor - start; // A cluster has no internal backtrack edge.
	}

	private int FindRegionalRunStart( ReadOnlySpan<char> source, int leftStart, int rightStart, int reverseStart,
		ref RegionalRun memo, ref int work, CancellationToken cancellationToken ) {
		var cursor = leftStart;
		while ( cursor > reverseStart ) {
			if ( memo.Start >= reverseStart && cursor >= memo.Start && cursor <= memo.End ) { cursor = memo.Start; break; }
			if ( !ChargeWork( ref work, cancellationToken ) ) { return -1; }
			if ( !PcreUtf16.TryReadPrevious( source, cursor, options.Utf16, out var scalar, out var width )
				|| width > cursor - reverseStart || PcreUnicodeGraphemes.GetBreak( scalar ) != PcreGraphemeBreak.RegionalIndicator ) { break; }
			cursor -= width;
		}
		memo = new( cursor, memo.Start == cursor ? Math.Max( memo.End, rightStart ) : rightStart );
		return cursor;
	}

	private int RewindGrapheme( ReadOnlySpan<char> source, int position, int minimumEnd,
		ref int work, CancellationToken cancellationToken ) {
		if ( !ChargeWork( ref work, cancellationToken ) ) { return -1; }
		if ( !PcreUtf16.TryReadPrevious( source, position, options.Utf16, out var scalar, out var width ) ) {
			throw new InvalidOperationException( "A grapheme retry lost its original character boundary." );
		}
		var cursor = position - width;
		var right = PcreUnicodeGraphemes.GetBreak( scalar );
		while ( cursor > minimumEnd ) {
			if ( !ChargeWork( ref work, cancellationToken ) ) { return -1; }
			if ( !PcreUtf16.TryReadPrevious( source, cursor, options.Utf16, out scalar, out width ) ) {
				throw new InvalidOperationException( "A grapheme retry lost its original character boundary." );
			}
			var left = PcreUnicodeGraphemes.GetBreak( scalar );
			// The pinned interpreter reverses its pair table without forward RI
			// parity or the additional EP/ZWJ context check.
			if ( !PcreUnicodeGraphemes.Joins( left, right, pictographBeforeZwj: true ) ) { break; }
			cursor -= width;
			right = left;
		}
		return cursor;
	}
}
