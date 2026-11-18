using System.Globalization;
using System.Text;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreRaw16OracleReferenceTests {
	private static readonly UTF8Encoding StrictUtf8 = new( false, true );

	[Theory]
	[MemberData( nameof( PcreRaw16Cases.Native ), MemberType = typeof( PcreRaw16Cases ) )]
	public void RawUnitsKeepPinnedNativeProtocols( int id, string pattern, string subject, int mode, uint offset,
		string kind, int error, uint errorOffset, string captures, string mark, string names, bool utf ) {
		var result = PcreNative16Oracle.Execute( Units( pattern ), Units( subject ), mode, offset );
		Assert.True( result.Kind == kind, $"fixture {id}: expected {kind}, actual {result.Kind}" );
		Assert.Equal( error, result.ErrorCode );
		Assert.Equal( errorOffset, result.ErrorOffset );
		Assert.Equal( captures, result.Captures );
		Assert.Equal( mark, result.Mark );
		Assert.Equal( names, result.Names );
		if ( kind == "E" ) { Assert.Equal( (nuint)0, result.JitSize ); }
		else {
			Assert.True( result.JitSize > 0 );
			Assert.Equal( utf, ( result.Options & 0x80000u ) != 0 );
		}
	}

	[Theory]
	[MemberData( nameof( PcreDualWidthCases.Matching ), MemberType = typeof( PcreDualWidthCases ) )]
	public void ExplicitUtfBridgeReusesTheFrozenSharedCatalog( int id, string pattern8, string pattern16,
		string subject8, string subject16, int mode, int offset8, int offset16,
		string spans8, string spans16, string scalars, string mark8, string mark16, string names ) {
		Assert.Equal( Convert.FromHexString( pattern8 ), StrictUtf8.GetBytes( Text( Units( pattern16 ) ) ) );
		var bytes = Convert.FromHexString( subject8 );
		var words = Text( Units( subject16 ) );
		var result = PcreNative16Oracle.Execute( Units( pattern16 ), Units( subject16 ), mode, (uint)offset16 );
		Assert.True( result.Kind == ( spans16.Length == 0 ? "N" : "M" ), $"UTF fixture {id}" );
		Assert.Equal( 0, result.ErrorCode );
		Assert.Equal( spans16, result.Captures );
		Assert.Equal( mark16, result.Mark );
		Assert.Equal( mark8, Convert.ToHexString( StrictUtf8.GetBytes( Text( Units( result.Mark ) ) ) ) );
		Assert.Equal( names, UtfNames( result.Names ) );
		Assert.True( result.JitSize > 0 );
		Assert.NotEqual( 0u, result.Options & 0x80000u );
		var coordinates = new PcreDualWidthCoordinates( bytes, words );
		Assert.Equal( scalars, coordinates.WordCaptures( result.Captures ) );
		Assert.Equal( scalars, coordinates.ByteCaptures( spans8 ) );
		Assert.Equal( coordinates.ByteCaptures( $"{offset8}:0" ), coordinates.WordCaptures( $"{offset16}:0" ) );
	}

	[Theory]
	[MemberData( nameof( PcreDualWidthCases.Diagnostics ), MemberType = typeof( PcreDualWidthCases ) )]
	public void ExplicitUtfErrorsKeepFrozenSourceOffsets( int id, string pattern8, string pattern16,
		int code, int offset8, int offset16, int scalarOffset ) {
		var bytes = Convert.FromHexString( pattern8 );
		var words = Text( Units( pattern16 ) );
		var result = PcreNative16Oracle.Execute( Units( pattern16 ), Array.Empty<ushort>(), 1, 0 );
		Assert.True( result.Kind == "E", $"diagnostic {id}" );
		Assert.Equal( code, result.ErrorCode );
		Assert.Equal( (uint)offset16, result.ErrorOffset );
		Assert.Equal( (nuint)0, result.JitSize );
		var coordinates = new PcreDualWidthCoordinates( bytes, words );
		Assert.Equal( $"{scalarOffset}:0", coordinates.WordCaptures( $"{result.ErrorOffset}:0" ) );
		Assert.Equal( $"{scalarOffset}:0", coordinates.ByteCaptures( $"{offset8}:0" ) );
	}

	[Fact]
	public void ManagedLayoutMatchesTheIndependentPinnedC64Layout() {
		Assert.Equal( new[] { 64, 40, 80, 16, 24, 56, 32, 32, 56, 64, 8 }, PcreNative16Oracle.AbiLayout() );
	}

	[Fact]
	public void ReturnedDataOutlivesNativeProgramsAndInputArrays() {
		var pattern = Units( "0028003F003C0078003E002E00290028002A004D00410052004B003A004F0057004E0029" );
		var subject = new ushort[] { 0xD800 };
		var first = PcreNative16Oracle.Execute( pattern, subject, 0, 0 );
		Assert.Equal( "M", first.Kind );
		Array.Fill( pattern, (ushort)'X' );
		Array.Fill( subject, (ushort)'Y' );
		for ( var i = 0; i < 32; i++ ) {
			Assert.Equal( "E", PcreNative16Oracle.Execute( new ushort[] { (ushort)'(' }, Array.Empty<ushort>(), 0, 0 ).Kind );
			Assert.Equal( "N", PcreNative16Oracle.Execute( new ushort[] { (ushort)'A' }, new ushort[] { (ushort)'B' }, 0, 0 ).Kind );
		}
		Assert.Equal( "0:1,0:1", first.Captures );
		Assert.Equal( "004F0057004E", first.Mark );
		Assert.Equal( "0078=1", first.Names );
		Assert.Equal( 0u, first.Options & 0x80000u );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	[InlineData( 4 )]
	[InlineData( 5 )]
	public void InvalidManagedInputsDoNotReachTheNativeAbi( int selection ) {
		if ( selection == 0 ) { Assert.Throws<ArgumentNullException>( () => PcreNative16Oracle.Execute( null!, Array.Empty<ushort>(), 0, 0 ) ); }
		else if ( selection == 1 ) { Assert.Throws<ArgumentNullException>( () => PcreNative16Oracle.Execute( Array.Empty<ushort>(), null!, 0, 0 ) ); }
		else if ( selection == 2 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreNative16Oracle.Execute( Array.Empty<ushort>(), Array.Empty<ushort>(), -1, 0 ) ); }
		else if ( selection == 3 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreNative16Oracle.Execute( Array.Empty<ushort>(), Array.Empty<ushort>(), 8, 0 ) ); }
		else if ( selection == 4 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreNative16Oracle.Execute( new ushort[4097], Array.Empty<ushort>(), 0, 0 ) ); }
		else { Assert.Throws<ArgumentOutOfRangeException>( () => PcreNative16Oracle.Execute( Array.Empty<ushort>(), new ushort[4097], 0, 0 ) ); }
	}

	private static ushort[] Units( string hex ) {
		if ( hex.Length % 4 != 0 ) { throw new FormatException( "Expected four digits per native word." ); }
		var result = new ushort[hex.Length / 4];
		for ( var i = 0; i < result.Length; i++ ) {
			result[i] = UInt16.Parse( hex.AsSpan( i * 4, 4 ), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture );
		}
		return result;
	}
	private static string Text( ushort[] units ) => new( units.Select( unit => (char)unit ).ToArray() );
	private static string UtfNames( string native ) {
		if ( native.Length == 0 ) { return ""; }
		var map = new SortedDictionary<string, List<int>>( StringComparer.Ordinal );
		foreach ( var entry in native.Split( ';' ) ) {
			var parts = entry.Split( '=' );
			var name = Text( Units( parts[0] ) );
			if ( !map.TryGetValue( name, out var numbers ) ) { numbers = new List<int>(); map.Add( name, numbers ); }
			numbers.Add( Int32.Parse( parts[1], CultureInfo.InvariantCulture ) );
		}
		return String.Join( ";", map.Select( entry => String.Concat( Convert.ToHexString( StrictUtf8.GetBytes( entry.Key ) ), "=", String.Join( ",", entry.Value.OrderBy( n => n ) ) ) ) );
	}
}
