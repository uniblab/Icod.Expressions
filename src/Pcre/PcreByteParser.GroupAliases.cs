namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	// index is immediately after '('. Match the colon too, so partial names,
	// different case and header trivia cannot become implemented aliases.
	private bool TryParseGroupAlias( out bool atomic, out int assertionKind, out bool nonAtomic ) {
		atomic = false;
		assertionKind = 0;
		nonAtomic = false;
		if ( index >= source.Length || source[index] != (byte)'*' ) { return false; }
		if ( TryConsumeGroupAlias( "*atomic:"u8 ) ) {
			atomic = true;
			return true;
		}
		if ( TryConsumeGroupAlias( "*pla:"u8 ) || TryConsumeGroupAlias( "*positive_lookahead:"u8 ) ) {
			assertionKind = 1;
			return true;
		}
		if ( TryConsumeGroupAlias( "*nla:"u8 ) || TryConsumeGroupAlias( "*negative_lookahead:"u8 ) ) {
			assertionKind = -1;
			return true;
		}
		if ( TryConsumeGroupAlias( "*plb:"u8 ) || TryConsumeGroupAlias( "*positive_lookbehind:"u8 ) ) {
			assertionKind = 2;
			return true;
		}
		if ( TryConsumeGroupAlias( "*nlb:"u8 ) || TryConsumeGroupAlias( "*negative_lookbehind:"u8 ) ) {
			assertionKind = -2;
			return true;
		}
		if ( TryConsumeGroupAlias( "*napla:"u8 ) || TryConsumeGroupAlias( "*non_atomic_positive_lookahead:"u8 ) ) {
			assertionKind = 1;
			nonAtomic = true;
			return true;
		}
		if ( TryConsumeGroupAlias( "*naplb:"u8 ) || TryConsumeGroupAlias( "*non_atomic_positive_lookbehind:"u8 ) ) {
			assertionKind = 2;
			nonAtomic = true;
			return true;
		}
		return false;
	}

	private bool TryConsumeGroupAlias( ReadOnlySpan<byte> header ) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( !source.AsSpan( index ).StartsWith( header ) ) { return false; }
		index += header.Length;
		return true;
	}
}
