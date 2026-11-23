using System.Globalization;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreUtf16NavigationTests {
	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EverySingleCodeUnitRetainsItsRawOrStrictInterpretation( bool utf ) {
		var input = new char[1];
		for ( var unit = 0; unit <= UInt16.MaxValue; unit++ ) {
			input[0] = (char)unit;
			var valid = !utf || unit is < 0xD800 or > 0xDFFF;
			Assert.Equal( valid, PcreUtf16.TryRead( input, 0, utf, out var value, out var width ) );
			Assert.Equal( valid ? unit : 0, value );
			Assert.Equal( valid ? 1 : 0, width );
			Assert.Equal( valid, PcreUtf16.TryReadPrevious( input, 1, utf, out value, out width ) );
			Assert.Equal( valid ? unit : 0, value );
			Assert.Equal( valid ? 1 : 0, width );
		}
	}

	[Theory]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	[InlineData( 4 )]
	[InlineData( 5 )]
	[InlineData( 6 )]
	[InlineData( 7 )]
	[InlineData( 8 )]
	[InlineData( 9 )]
	[InlineData( 10 )]
	[InlineData( 11 )]
	[InlineData( 12 )]
	[InlineData( 13 )]
	[InlineData( 14 )]
	[InlineData( 15 )]
	[InlineData( 16 )]
	public void EverySupplementaryScalarRoundTripsInBothDirections( int plane ) {
		var storage = new[] { 'X', '\0', '\0', 'Y' };
		for ( var low = 0; low <= UInt16.MaxValue; low++ ) {
			var scalar = ( plane << 16 ) | low;
			Assert.Equal( 2, PcreUtf16.EncodeCodePoint( scalar, storage.AsSpan( 1, 2 ) ) );
			Assert.Equal( (char)( 0xD800 + ( ( scalar - 0x10000 ) >> 10 ) ), storage[1] );
			Assert.Equal( (char)( 0xDC00 + ( ( scalar - 0x10000 ) & 1023 ) ), storage[2] );
			Assert.True( PcreUtf16.TryRead( storage.AsSpan( 1, 2 ), 0, true, out var value, out var width ) );
			Assert.Equal( scalar, value ); Assert.Equal( 2, width );
			Assert.True( PcreUtf16.TryReadPrevious( storage.AsSpan( 1, 2 ), 2, true, out value, out width ) );
			Assert.Equal( scalar, value ); Assert.Equal( 2, width );
		}
		Assert.Equal( 'X', storage[0] ); Assert.Equal( 'Y', storage[3] );
	}

	[Theory]
	[InlineData( -1 )]
	[InlineData( Int32.MinValue )]
	[InlineData( Int32.MaxValue )]
	[InlineData( 3 )]
	public void InvalidOffsetsReturnClearedOutputs( int offset ) {
		foreach ( var utf in new[] { false, true } ) {
			Assert.False( PcreUtf16.TryRead( "ab", offset, utf, out var value, out var width ) );
			Assert.Equal( 0, value ); Assert.Equal( 0, width );
			Assert.False( PcreUtf16.TryReadPrevious( "ab", offset, utf, out value, out width ) );
			Assert.Equal( 0, value ); Assert.Equal( 0, width );
		}
	}

	[Fact]
	public void EmptyViewsAndTruncatedPairsDoNotReadSurroundingStorage() {
		var input = new[] { 'X', '\uD800', '\uDC00', 'Y' };
		Assert.False( PcreUtf16.TryRead( input.AsSpan( 1, 1 ), 0, true, out _, out _ ) );
		Assert.False( PcreUtf16.TryReadPrevious( input.AsSpan( 2, 1 ), 1, true, out _, out _ ) );
		Assert.False( PcreUtf16.TryRead( input, 2, true, out _, out _ ) );
		Assert.False( PcreUtf16.TryReadPrevious( input, 2, true, out _, out _ ) );
		foreach ( var utf in new[] { false, true } ) {
			Assert.False( PcreUtf16.TryRead( ReadOnlySpan<char>.Empty, 0, utf, out _, out _ ) );
			Assert.False( PcreUtf16.TryReadPrevious( ReadOnlySpan<char>.Empty, 0, utf, out _, out _ ) );
			Assert.False( PcreUtf16.TryRead( input, input.Length, utf, out _, out _ ) );
			Assert.False( PcreUtf16.TryReadPrevious( input, 0, utf, out _, out _ ) );
		}
		Assert.Equal( new[] { 'X', '\uD800', '\uDC00', 'Y' }, input );
	}

	[Theory]
	[InlineData( -1 )]
	[InlineData( 0x110000 )]
	public void InvalidCodePointsDoNotWriteToTheDestination( int value ) {
		var output = new[] { 'X', 'Y' };
		Assert.Throws<ArgumentOutOfRangeException>( () => PcreUtf16.EncodeCodePoint( value, output ) );
		Assert.Equal( new[] { 'X', 'Y' }, output );
	}

	[Theory]
	[InlineData( 0, 0 )]
	[InlineData( 0xD800, 0 )]
	[InlineData( 0x10000, 1 )]
	public void ShortDestinationsAreRejectedBeforeWriting( int value, int capacity ) {
		var output = new[] { 'X', 'Y' };
		Assert.Throws<ArgumentException>( () => PcreUtf16.EncodeCodePoint( value, output.AsSpan( 0, capacity ) ) );
		Assert.Equal( new[] { 'X', 'Y' }, output );
	}

	[Fact]
	public void EncodingEveryBmpCodePointPreservesRawSurrogates() {
		var output = new char[2];
		for ( var value = 0; value <= UInt16.MaxValue; value++ ) {
			output[1] = 'X';
			Assert.Equal( 1, PcreUtf16.EncodeCodePoint( value, output ) );
			Assert.Equal( (char)value, output[0] );
			Assert.Equal( 'X', output[1] );
		}
	}

	[Theory]
	[InlineData( "0000" )]
	[InlineData( "000A" )]
	[InlineData( "D7FF" )]
	[InlineData( "D800" )]
	[InlineData( "DBFF" )]
	[InlineData( "DC00" )]
	[InlineData( "DFFF" )]
	[InlineData( "E000" )]
	[InlineData( "FFFF" )]
	[InlineData( "D800DC00" )]
	[InlineData( "DBFFDFFF" )]
	public void NavigationAgreesWithPinnedNative16Consumption( string hex ) {
		var words = Enumerable.Range( 0, hex.Length / 4 ).Select( index =>
			UInt16.Parse( hex.AsSpan( index * 4, 4 ), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture ) ).ToArray();
		var text = new string( words.Select( word => (char)word ).ToArray() );
		var forward = @"(?s)\G.".Select( unit => (ushort)unit ).ToArray();
		var reverse = @"(?s)(?<=(.))\G".Select( unit => (ushort)unit ).ToArray();
		foreach ( var utf in new[] { false, true } ) {
			var mode = utf ? 1 : 0;
			var first = PcreNative16Oracle.Execute( forward, words, mode, 0 );
			var last = PcreNative16Oracle.Execute( reverse, words, mode, (uint)words.Length );
			Assert.True( first.JitSize > 0 ); Assert.True( last.JitSize > 0 );
			Assert.Equal( utf, ( first.Options & 0x80000u ) != 0 );
			Assert.Equal( utf, ( last.Options & 0x80000u ) != 0 );
			var valid = PcreUtf16.TryRead( text, 0, utf, out _, out var width );
			Assert.Equal( valid ? "M" : "X", first.Kind );
			if ( valid ) { Assert.Equal( String.Concat( "0:", width ), first.Captures ); }
			else { Assert.Equal( words[0] <= 0xDBFF ? -24 : -26, first.ErrorCode ); }
			valid = PcreUtf16.TryReadPrevious( text, text.Length, utf, out _, out width );
			Assert.Equal( valid ? "M" : "X", last.Kind );
			if ( valid ) { Assert.Equal( String.Concat( words.Length, ":0,", words.Length - width, ":", width ), last.Captures ); }
			else { Assert.Equal( words[0] <= 0xDBFF ? -24 : -26, last.ErrorCode ); }
		}
	}
}
