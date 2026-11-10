using System.Buffers;
using System.Text;

namespace Icod.Expressions.Pcre;

internal static partial class PcreByteMatcher {
	// Input-derived memo only: no capture, scope or backtracking state is cached.
	private readonly record struct RegionalRun( int Start, int End );

	private static int ConsumeGrapheme( ReadOnlyMemory<byte> input, int start, int end, int fragmentStart,
		bool utf8, bool matchInvalidUtf, ref int work, int matchWorkLimit, Action<int>? workObserver, CancellationToken cancellationToken,
		ref RegionalRun regionalRun ) {
		ChargeWork( ref work, matchWorkLimit, workObserver, cancellationToken );
		if ( !TryReadGrapheme( input, start, end, utf8, out var left, out var width ) ) { return -1; }
		var cursor = start + width;
		var leftStart = start;
		var pictographBeforeZwj = false;
		while ( cursor < end ) {
			ChargeWork( ref work, matchWorkLimit, workObserver, cancellationToken ); // Lookahead is charged even when it discovers a break.
			if ( !TryReadGrapheme( input, cursor, end, utf8, out var right, out width )
				|| !GraphemeJoins( left, right, pictographBeforeZwj ) ) { break; }
			if ( left == PcreGraphemeBreak.RegionalIndicator && right == PcreGraphemeBreak.RegionalIndicator ) {
				var runStart = FindRegionalRunStart( input, leftStart, cursor, fragmentStart, matchInvalidUtf, ref work, matchWorkLimit, workObserver, cancellationToken, ref regionalRun );
				// Both native RI and its invalid-mode prefix view use four-byte scalars.
				if ( ( ( leftStart - runStart ) / 4 & 1 ) != 0 ) { break; }
			}
			pictographBeforeZwj = left == PcreGraphemeBreak.ExtendedPictographic && right == PcreGraphemeBreak.Zwj;
			// Preserve EP through Extend so EP Extend* ZWJ retains its context.
			if ( left != PcreGraphemeBreak.ExtendedPictographic || right != PcreGraphemeBreak.Extend ) { left = right; }
			leftStart = cursor;
			cursor += width;
		}
		return cursor; // No internal backtrack edge: the whole cluster is atomic.
	}

	private static bool TryReadGrapheme( ReadOnlyMemory<byte> input, int offset, int end, bool utf8,
		out PcreGraphemeBreak kind, out int width ) {
		kind = default;
		width = 1;
		if ( offset >= end ) { return false; }
		var scalar = (int)input.Span[offset];
		if ( utf8 && ( !PcreUtf8.TryRead( input.Span, offset, out scalar, out width ) || width > end - offset ) ) { return false; }
		kind = PcreUnicodeGraphemes.GetBreak( scalar );
		return true;
	}

	private static bool GraphemeJoins( PcreGraphemeBreak left, PcreGraphemeBreak right, bool pictographBeforeZwj ) {
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

	private static int FindRegionalRunStart( ReadOnlyMemory<byte> input, int leftStart, int rightStart,
		int fragmentStart, bool matchInvalidUtf, ref int work, int matchWorkLimit, Action<int>? workObserver, CancellationToken cancellationToken,
		ref RegionalRun memo ) {
		var cursor = leftStart;
		while ( cursor > fragmentStart ) {
			if ( !matchInvalidUtf && memo.Start >= fragmentStart && cursor >= memo.Start && cursor <= memo.End ) {
				cursor = memo.Start;
				break;
			}
			ChargeWork( ref work, matchWorkLimit, workObserver, cancellationToken ); // Backward context inspection is bounded, observable and cancellable.
			if ( Rune.DecodeLastFromUtf8( input.Span.Slice( fragmentStart, cursor - fragmentStart ), out var previous, out var width )
				!= OperationStatus.Done ) { break; }
			var scalar = previous.Value;
			if ( matchInvalidUtf ) {
				// PCRE2 10.48 JIT's invalid-mode backward view exchanges these
				// four-byte payload chunks. Literal RI prefixes stop the count;
				// 26 mapped CJK prefixes can contribute. Forward decoding stays normal.
				if ( width != 4 ) { break; }
				scalar = ( scalar & ~0x3FFFF ) | ( ( scalar & 63 ) << 12 ) | ( scalar & 0xFC0 ) | ( ( scalar >> 12 ) & 63 );
				if ( scalar is < 0x10000 or > 0x10FFFF ) { break; }
			}
			if ( PcreUnicodeGraphemes.GetBreak( scalar ) != PcreGraphemeBreak.RegionalIndicator ) { break; }
			cursor -= width;
		}
		if ( !matchInvalidUtf ) {
			var end = memo.Start == cursor ? Math.Max( memo.End, rightStart ) : rightStart;
			memo = new( cursor, end );
		}
		return cursor;
	}
}
