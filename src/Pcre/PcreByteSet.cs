namespace Icod.Expressions.Pcre;

/// <summary>An immutable set over all 256 byte values.</summary>
internal readonly record struct PcreByteSet( ulong A, ulong B, ulong C, ulong D ) {
	internal static PcreByteSet AllBytes { get; } = new( ulong.MaxValue, ulong.MaxValue, ulong.MaxValue, ulong.MaxValue );
	internal static PcreByteSet Dot { get; } = FromPredicate( value => value != (byte)'\n' );
	internal static PcreByteSet DotAnyCrLf { get; } = FromPredicate( value => value != (byte)'\n' && value != (byte)'\r' );
	internal static PcreByteSet Digit { get; } = FromPredicate(
		value => value is >= (byte)'0' and <= (byte)'9'
	);
	internal static PcreByteSet WhiteSpace { get; } = FromPredicate(
		value => value is >= 0x09 and <= 0x0D or (byte)' '
	);
	internal static PcreByteSet HorizontalSpace { get; } = FromPredicate( value => PcreSpaceCharacters.IsHorizontal( value ) );
	internal static PcreByteSet VerticalSpace { get; } = FromPredicate( value => PcreSpaceCharacters.IsVertical( value ) );
	internal static PcreByteSet Word { get; } = FromPredicate(
		value => value is >= (byte)'0' and <= (byte)'9'
			or >= (byte)'A' and <= (byte)'Z'
			or >= (byte)'a' and <= (byte)'z'
			or (byte)'_'
	);
	internal static PcreByteSet Upper { get; } = Range( (byte)'A', (byte)'Z' );
	internal static PcreByteSet Lower { get; } = Range( (byte)'a', (byte)'z' );
	internal static PcreByteSet Alpha { get; } = Upper.Union( Lower );
	internal static PcreByteSet Alnum { get; } = Alpha.Union( Digit );
	internal static PcreByteSet Ascii { get; } = Range( 0x00, 0x7F );
	internal static PcreByteSet Blank { get; } = Singleton( 0x09 ).Union( Singleton( (byte)' ' ) );
	internal static PcreByteSet Control { get; } = Range( 0x00, 0x1F ).Union( Singleton( 0x7F ) );
	internal static PcreByteSet Graph { get; } = Range( (byte)'!', (byte)'~' );
	internal static PcreByteSet Printable { get; } = Range( (byte)' ', (byte)'~' );
	internal static PcreByteSet Punctuation { get; } = FromPredicate(
		value => Graph.Contains( value ) && !Alnum.Contains( value )
	);
	internal static PcreByteSet HexDigit { get; } = Digit.Union( Range( (byte)'A', (byte)'F' ) )
		.Union( Range( (byte)'a', (byte)'f' ) );

	internal bool Contains( byte value ) {
		var word = value >> 6;
		var bits = word switch { 0 => A, 1 => B, 2 => C, _ => D };
		return ( bits & ( 1UL << ( value & 63 ) ) ) != 0;
	}

	internal PcreByteSet Complement() => new( ~A, ~B, ~C, ~D );
	internal PcreByteSet Union( PcreByteSet other ) => new( A | other.A, B | other.B, C | other.C, D | other.D );
	internal PcreByteSet AsciiCaseClosure() {
		var original = this;
		return FromPredicate( value => original.Contains( value )
			|| ( value is >= (byte)'A' and <= (byte)'Z' && original.Contains( (byte)( value + 32 ) ) )
			|| ( value is >= (byte)'a' and <= (byte)'z' && original.Contains( (byte)( value - 32 ) ) )
		);
	}

	internal static PcreByteSet Singleton( byte value ) => FromPredicate( candidate => candidate == value );
	internal static PcreByteSet Range( byte lower, byte upper ) => FromPredicate(
		candidate => candidate >= lower && candidate <= upper
	);

	internal static PcreByteSet FromPredicate( Func<byte, bool> contains ) {
		var words = new ulong[4];
		for ( var index = 0; index < 256; index++ ) {
			if ( contains( (byte)index ) ) {
				words[index >> 6] |= 1UL << ( index & 63 );
			}
		}
		return new( words[0], words[1], words[2], words[3] );
	}
}
