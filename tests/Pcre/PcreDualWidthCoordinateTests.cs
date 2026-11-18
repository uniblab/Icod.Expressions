using System.Text;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreDualWidthReferenceTests {
	[Theory]
	[InlineData( "00", "0000" )]
	[InlineData( "7F", "007F" )]
	[InlineData( "C280", "0080" )]
	[InlineData( "DFBF", "07FF" )]
	[InlineData( "E0A080", "0800" )]
	[InlineData( "ED9FBF", "D7FF" )]
	[InlineData( "EE8080", "E000" )]
	[InlineData( "EFBFBF", "FFFF" )]
	[InlineData( "F0908080", "D800DC00" )]
	[InlineData( "F48FBFBF", "DBFFDFFF" )]
	public void ScalarBoundariesKeepTheirNativeWidths( string hex8, string hex16 ) {
		var bytes = Convert.FromHexString( hex8 );
		var words = Words( hex16 );
		var map = new PcreDualWidthCoordinates( bytes, words );
		Assert.Equal( 1, map.ScalarCount );
		Assert.Equal( "0:1", map.ByteCaptures( $"0:{bytes.Length}" ) );
		Assert.Equal( "0:1", map.WordCaptures( $"0:{words.Length}" ) );
		Assert.Equal( "1:0", map.ByteCaptures( $"{bytes.Length}:0" ) );
		Assert.Equal( "1:0", map.WordCaptures( $"{words.Length}:0" ) );
	}

	[Theory]
	[InlineData( "80", "" )]
	[InlineData( "C0AF", "" )]
	[InlineData( "E08080", "" )]
	[InlineData( "EDA080", "" )]
	[InlineData( "F4908080", "" )]
	[InlineData( "C3", "" )]
	[InlineData( "F09F98", "" )]
	[InlineData( "F5808080", "" )]
	[InlineData( "", "D800" )]
	[InlineData( "", "DC00" )]
	[InlineData( "", "D8000041" )]
	[InlineData( "", "D800D800" )]
	[InlineData( "", "DBFF" )]
	[InlineData( "80", "FFFD" )]
	[InlineData( "EFBFBD", "D800" )]
	[InlineData( "61", "0062" )]
	[InlineData( "6162", "0061" )]
	public void MalformedOrDifferentScalarSequencesAreNotComparable( string hex8, string hex16 ) {
		Assert.Throws<ArgumentException>( () => new PcreDualWidthCoordinates( Convert.FromHexString( hex8 ), Words( hex16 ) ) );
	}

	[Theory]
	[InlineData( false, "2:0", 0 )]
	[InlineData( false, "4:0", 0 )]
	[InlineData( false, "5:0", 0 )]
	[InlineData( false, "6:0", 0 )]
	[InlineData( false, "1:1", 0 )]
	[InlineData( true, "3:0", 0 )]
	[InlineData( true, "2:1", 0 )]
	[InlineData( false, "-1:0", 1 )]
	[InlineData( false, "9:0", 1 )]
	[InlineData( true, "6:0", 1 )]
	[InlineData( false, "0:-1", 1 )]
	[InlineData( false, "a:0", 2 )]
	[InlineData( false, "0", 2 )]
	[InlineData( true, "0:0:0", 2 )]
	[InlineData( false, "0:0,", 2 )]
	[InlineData( false, "1:2147483647", 3 )]
	public void InvalidCaptureProtocolsNeverSnapToNearbyBoundaries( bool wordUnits, string protocol, int kind ) {
		var map = new PcreDualWidthCoordinates( Encoding.UTF8.GetBytes( "Aé😀B" ), "Aé😀B" );
		void Normalize() { if ( wordUnits ) { map.WordCaptures( protocol ); } else { map.ByteCaptures( protocol ); } }
		switch ( kind ) {
			case 0: Assert.Throws<ArgumentException>( Normalize ); break;
			case 1: Assert.Throws<ArgumentOutOfRangeException>( Normalize ); break;
			case 2: Assert.Throws<FormatException>( Normalize ); break;
			default: Assert.Throws<OverflowException>( Normalize ); break;
		}
	}

	[Fact]
	public void EmptyUnsetAndReversedRangesRemainDistinct() {
		var map = new PcreDualWidthCoordinates( Encoding.UTF8.GetBytes( "Aé😀B" ), "Aé😀B" );
		Assert.Equal( 4, map.ScalarCount );
		Assert.Equal( "", map.ByteCaptures( "" ) );
		Assert.Equal( "", map.WordCaptures( "" ) );
		Assert.Equal( "0:0,-,3:-1,4:0", map.ByteCaptures( "0:0,-,7:-4,8:0" ) );
		Assert.Equal( "0:0,-,3:-1,4:0", map.WordCaptures( "0:0,-,4:-2,5:0" ) );
		var empty = new PcreDualWidthCoordinates( Array.Empty<byte>(), "" );
		Assert.Equal( 0, empty.ScalarCount );
		Assert.Equal( "0:0,-", empty.ByteCaptures( "0:0,-" ) );
		Assert.Equal( "0:0,-", empty.WordCaptures( "0:0,-" ) );
	}
}
