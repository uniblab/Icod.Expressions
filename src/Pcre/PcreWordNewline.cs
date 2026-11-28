namespace Icod.Expressions.Pcre;

internal static class PcreWordNewline {
	internal static bool IsCrLf( ReadOnlySpan<char> source, int at ) =>
		at >= 0 && at < source.Length - 1 && source[at] == '\r' && source[at + 1] == '\n';

	internal static int LengthAt( PcreByteNewline newline, ReadOnlySpan<char> source, int at ) {
		if ( (uint)at >= (uint)source.Length ) { return 0; }
		var pair = IsCrLf( source, at );
		if ( newline == PcreByteNewline.CrLf ) { return pair ? 2 : 0; }
		var matches = newline switch {
			PcreByteNewline.Any => PcreSpaceCharacters.IsVertical( source[at] ),
			PcreByteNewline.AnyCrLf => source[at] is '\r' or '\n',
			_ => source[at] == newline.Character()
		};
		if ( !matches ) { return 0; }
		return pair && newline is PcreByteNewline.Any or PcreByteNewline.AnyCrLf ? 2 : 1;
	}

	internal static bool IsBefore( PcreByteNewline newline, ReadOnlySpan<char> source, int at ) =>
		newline == PcreByteNewline.CrLf ? at >= 2 && IsCrLf( source[..at], at - 2 )
			: at > 0 && LengthAt( newline, source[..at], at - 1 ) > 0;
}
