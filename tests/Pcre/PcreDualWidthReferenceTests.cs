using System.Globalization;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreDualWidthReferenceTests {
	private static readonly UTF8Encoding StrictUtf8 = new( false, true );

	private static void AssertProtocol( int id, string pattern8, string pattern16, string subject8, string subject16,
		int mode, int offset8, int offset16, string spans8, string spans16, string scalars, string mark8, string mark16, string names ) {
		var source = Convert.FromHexString( pattern8 );
		var text = Words( pattern16 );
		var bytes = Convert.FromHexString( subject8 );
		var words = Words( subject16 );
		Assert.Equal( source, StrictUtf8.GetBytes( text ) );
		Assert.Equal( bytes, StrictUtf8.GetBytes( words ) );
		var regex8 = new PcreRegex8Bit( source, StrictUtf8, Settings( mode ) );
		var regex16 = new PcreRegex( text, Settings( mode ) );
		var match8 = regex8.Match( bytes, offset8 );
		var match16 = regex16.Match( words, offset16 );
		var groups8 = new List<string>();
		if ( match8.Success ) {
			for ( var i = 0; i < match8.Groups.Count; i++ ) {
				var capture = match8.Groups[i];
				groups8.Add( capture.Success ? $"{capture.Index}:{capture.EndIndex - capture.Index}" : "-" );
			}
		}
		var groups16 = new List<string>();
		if ( match16.Success ) {
			for ( var i = 0; i < match16.Groups.Count; i++ ) {
				var capture = match16.Groups[i];
				groups16.Add( capture.Success ? $"{capture.Index}:{capture.EndIndex - capture.Index}" : "-" );
			}
		}
		Assert.Equal( spans8, String.Join( ",", groups8 ) );
		Assert.Equal( spans16, String.Join( ",", groups16 ) );
		Assert.Equal( mark8, match8.Success ? Convert.ToHexString( match8.Mark ) : "" );
		Assert.Equal( mark16, match16.Success ? WordHex( match16.Mark ?? "" ) : "" );
		Assert.Equal( names, Names( regex8.PatternInfo ) );
		Assert.Equal( names, Names( regex16.PatternInfo ) );
		if ( match8.Success ) { Assert.Equal( StrictUtf8.GetString( match8.Mark ), match16.Mark ?? "" ); }

		var compiled = PcreByteCompiler.Compile( source, new PcreByteOptions {
			Utf8 = true, Ucp = ( mode & 2 ) != 0, AllowLookaroundBackslashK = ( mode & 4 ) != 0
		} );
		Assert.True( compiled.IsSuccess, $"fixture {id}: {compiled.Diagnostic?.Message}" );
		var actual = compiled.Pattern!.Match( bytes, offset8 );
		Assert.True( actual.IsSuccess, $"fixture {id}: {actual.Diagnostic?.Message}" );
		Assert.Equal( spans8, actual.Match is null ? "" : String.Join( ",", actual.Match.Captures.Select(
			capture => capture is { } value ? $"{value.ByteIndex}:{value.ByteLength}" : "-" ) ) );
		Assert.Equal( mark8, actual.Match is null ? "" : Convert.ToHexString( actual.Match.MarkName.Span ) );
		if ( actual.Match is { } found ) {
			Assert.Equal( match8.Index, found.ByteIndex );
			Assert.Equal( match8.EndIndex - match8.Index, found.ByteLength );
		}

		var coordinates = new PcreDualWidthCoordinates( bytes, words );
		Assert.Equal( scalars, coordinates.ByteCaptures( spans8 ) );
		Assert.Equal( scalars, coordinates.WordCaptures( spans16 ) );
		Assert.Equal( coordinates.ByteCaptures( $"{offset8}:0" ), coordinates.WordCaptures( $"{offset16}:0" ) );
	}

	private static void AssertCompileError( int id, string pattern8, string pattern16,
		int code, int offset8, int offset16, int scalarOffset ) {
		var bytes = Convert.FromHexString( pattern8 );
		var words = Words( pattern16 );
		Assert.Equal( bytes, StrictUtf8.GetBytes( words ) );
		var error8 = Assert.Throws<PcrePatternException>( () => new PcreRegex8Bit( bytes, StrictUtf8, Settings( 1 ) ) );
		var error16 = Assert.Throws<PcrePatternException>( () => new PcreRegex( words, Settings( 1 ) ) );
		Assert.Equal( code, (int)error8.ErrorCode );
		Assert.Equal( code, (int)error16.ErrorCode );
		Assert.EndsWith( $" at offset {offset8}.", error8.Message );
		Assert.EndsWith( $" at offset {offset16}.", error16.Message );
		var managed = PcreByteCompiler.Compile( bytes, new PcreByteOptions { Utf8 = true } );
		Assert.False( managed.IsSuccess, $"invalid fixture {id}" );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, managed.Diagnostic?.Code );
		var coordinates = new PcreDualWidthCoordinates( bytes, words );
		Assert.Equal( $"{scalarOffset}:0", coordinates.ByteCaptures( $"{offset8}:0" ) );
		Assert.Equal( $"{scalarOffset}:0", coordinates.WordCaptures( $"{offset16}:0" ) );
	}

	private static PcreRegexSettings Settings( int mode ) => new() {
		Options = PcreOptions.Compiled | PcreOptions.Utf | ( ( mode & 2 ) != 0 ? PcreOptions.Ucp : PcreOptions.None ),
		ExtraCompileOptions = (PcreExtraCompileOptions)( ( mode & 4 ) != 0 ? 64 : 0 ),
		NewLine = (PcreNewLine)2, BackslashR = (PcreBackslashR)1, ParensLimit = 512
	};

	private static string Names( PcrePatternInfo info ) => String.Join( ";", info.GroupNames.OrderBy( name => name, StringComparer.Ordinal )
		.Select( name => String.Concat( Convert.ToHexString( StrictUtf8.GetBytes( name ) ), "=",
			String.Join( ",", info.GetGroupIndexesByName( name ).OrderBy( number => number ) ) ) ) );

	private static string Words( string hex ) {
		if ( hex.Length % 4 != 0 ) { throw new FormatException( "Expected four hex digits per UTF-16 code unit." ); }
		var words = new char[hex.Length / 4];
		for ( var i = 0; i < words.Length; i++ ) {
			words[i] = (char)UInt16.Parse( hex.AsSpan( i * 4, 4 ), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture );
		}
		return new string( words );
	}
	private static string WordHex( string words ) => String.Concat( words.Select( word => ( (int)word ).ToString( "X4", CultureInfo.InvariantCulture ) ) );
}
