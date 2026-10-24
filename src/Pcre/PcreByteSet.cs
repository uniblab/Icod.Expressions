namespace Icod.Expressions.Pcre;

/// <summary>An immutable set over all 256 byte values.</summary>
internal readonly record struct PcreByteSet( ulong A, ulong B, ulong C, ulong D ) {
	internal static PcreByteSet Dot { get; } = FromPredicate( value => value != (byte)'\n' );

	internal bool Contains( byte value ) {
		var word = value >> 6;
		var bits = word switch { 0 => A, 1 => B, 2 => C, _ => D };
		return ( bits & ( 1UL << ( value & 63 ) ) ) != 0;
	}

	internal PcreByteSet Complement() => new( ~A, ~B, ~C, ~D );

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
