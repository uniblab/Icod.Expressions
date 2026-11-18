using System.Buffers;
using System.Globalization;
using System.Text;

namespace Icod.Expressions.Tests.Pcre;

// Test infrastructure only: never used by a production matcher or API.
internal sealed class PcreDualWidthCoordinates {
	private readonly int[] byteScalars;
	private readonly int[] wordScalars;

	internal PcreDualWidthCoordinates( byte[] utf8, string utf16 ) {
		ArgumentNullException.ThrowIfNull( utf8 );
		ArgumentNullException.ThrowIfNull( utf16 );
		byteScalars = new int[checked( utf8.Length + 1 )];
		wordScalars = new int[checked( utf16.Length + 1 )];
		Array.Fill( byteScalars, -1 );
		Array.Fill( wordScalars, -1 );
		var byteOffset = 0;
		var wordOffset = 0;
		var scalar = 0;
		while ( byteOffset < utf8.Length && wordOffset < utf16.Length ) {
			var byteStatus = Rune.DecodeFromUtf8( utf8.AsSpan( byteOffset ), out var byteRune, out var byteCount );
			var wordStatus = Rune.DecodeFromUtf16( utf16.AsSpan( wordOffset ), out var wordRune, out var wordCount );
			if ( byteStatus != OperationStatus.Done || wordStatus != OperationStatus.Done || byteRune != wordRune ) {
				throw new ArgumentException( "Both inputs must contain the same valid Unicode scalar sequence." );
			}
			byteScalars[byteOffset] = scalar;
			wordScalars[wordOffset] = scalar;
			byteOffset += byteCount;
			wordOffset += wordCount;
			scalar++;
		}
		if ( byteOffset != utf8.Length || wordOffset != utf16.Length ) {
			throw new ArgumentException( "Both inputs must contain the same valid Unicode scalar sequence." );
		}
		byteScalars[byteOffset] = scalar;
		wordScalars[wordOffset] = scalar;
		ScalarCount = scalar;
	}

	internal int ScalarCount { get; }
	internal string ByteCaptures( string captures ) => Normalize( captures, byteScalars );
	internal string WordCaptures( string captures ) => Normalize( captures, wordScalars );

	private static string Normalize( string captures, int[] boundaries ) {
		ArgumentNullException.ThrowIfNull( captures );
		if ( captures.Length == 0 ) { return ""; }
		return String.Join( ",", captures.Split( ',' ).Select( Capture ) );

		string Capture( string value ) {
			if ( value == "-" ) { return "-"; }
			var colon = value.IndexOf( ':' );
			if ( colon <= 0 || colon != value.LastIndexOf( ':' )
				|| !Int32.TryParse( value.AsSpan( 0, colon ), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var start )
				|| !Int32.TryParse( value.AsSpan( colon + 1 ), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var length ) ) {
				throw new FormatException( "Expected an unset marker or signed native start:length pair." );
			}
			var end = checked( start + length );
			var scalarStart = Scalar( start );
			var scalarEnd = Scalar( end );
			return String.Concat( scalarStart.ToString( CultureInfo.InvariantCulture ), ":",
				( scalarEnd - scalarStart ).ToString( CultureInfo.InvariantCulture ) );
		}

		int Scalar( int offset ) {
			if ( offset < 0 || offset >= boundaries.Length ) { throw new ArgumentOutOfRangeException( nameof( captures ) ); }
			if ( boundaries[offset] < 0 ) { throw new ArgumentException( "A native endpoint splits a scalar.", nameof( captures ) ); }
			return boundaries[offset];
		}
	}
}
