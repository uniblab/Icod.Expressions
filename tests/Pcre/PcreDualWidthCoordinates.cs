namespace Icod.Expressions.Tests.Pcre;

// Test infrastructure only: never used by a production matcher or API.
internal sealed class PcreDualWidthCoordinates {
	internal PcreDualWidthCoordinates( byte[] utf8, string utf16 ) {
		throw new NotImplementedException( "Dual-width coordinate contract is not implemented yet." );
	}
	internal int ScalarCount => throw new NotImplementedException();
	internal string ByteCaptures( string captures ) => throw new NotImplementedException();
	internal string WordCaptures( string captures ) => throw new NotImplementedException();
}
