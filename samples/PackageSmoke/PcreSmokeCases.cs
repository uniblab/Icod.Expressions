namespace Icod.Expressions.PackageSmoke;

internal sealed record PcreSmokeProtocol( string Kind, string Captures = "", string Mark = "", string Diagnostic = "", int DiagnosticOffset = 0 );

internal sealed record PcreSmokeCase( int Id, string Pattern, string Subject, int Mode, int Start, PcreSmokeProtocol Expected, int WorkLimit = 1_000_000 );

// Literals reuse the frozen native-first byte, dual-width and Perl-overlap contracts.
internal static class PcreSmokeCases {
	internal static IReadOnlyList<PcreSmokeCase> All { get; } = new PcreSmokeCase[] {
		new( 1, "5C43", "", 0, 0, new( "N", "", "" ) ),
		new( 2, "5C41285C43295C7A", "00", 0, 0, new( "M", "0:1,0:1", "" ) ),
		new( 3, "5C41285C43295C7A", "FF", 0, 0, new( "M", "0:1,0:1", "" ) ),
		new( 4, "5C41285C43295C7A", "0A", 0, 0, new( "M", "0:1,0:1", "" ) ),
		new( 5, "5C41285C43295C7A", "0D", 0, 0, new( "M", "0:1,0:1", "" ) ),
		new( 6, "5C41285C43295C7A", "85", 0, 0, new( "M", "0:1,0:1", "" ) ),
		new( 7, "5C43", "C3A9", 0, 0, new( "M", "0:1", "" ) ),
		new( 8, "5C41285C437B327D295C7A", "C3A9", 0, 0, new( "M", "0:2,0:2", "" ) ),
		new( 9, "5C41285C437B337D295C7A", "EDA080", 0, 0, new( "M", "0:3,0:3", "" ) ),
		new( 10, "283F69295C41285C4329285C6343295C7A", "FF03", 0, 0, new( "M", "0:2,0:1,1:1", "" ) ),
		new( 11, "282A55435029283F6961295C41285C43295C7A", "E9", 0, 0, new( "M", "0:1,0:1", "" ) ),
		new( 12, "283F2D73295C41285C43295C7A", "0A", 0, 0, new( "M", "0:1,0:1", "" ) ),
		new( 13, "", "", 1, 0, new( "M", "0:0", "" ) ),
		new( 14, "28C3A92928F09F988029", "78C3A9F09F988079", 1, 1, new( "M", "1:6,1:2,3:4", "" ) ),
		new( 15, "28C3A92928F09F988029", "78C3A9F09F988079", 1, 8, new( "N", "", "" ) ),
		new( 16, "283F7C28C3A9297C28F09F98802929284229", "F09F988042", 1, 0, new( "M", "0:5,0:4,4:1", "" ) ),
		new( 17, "28C3A9293F2829F09F9880", "F09F9880", 1, 0, new( "M", "0:4,-,0:0", "" ) ),
		new( 18, "283F3C3DC3A929F09F9880", "C3A9F09F9880", 1, 0, new( "M", "2:4", "" ) ),
		new( 19, "283F3C3DC3A97B312C327D29F09F9880", "C3A9C3A9F09F9880", 1, 0, new( "M", "4:4", "" ) ),
		new( 20, "283F3D28C3A9F09F9880292928C3A929", "C3A9F09F9880", 1, 0, new( "M", "0:2,0:6,0:2", "" ) ),
		new( 21, "283F3EC3A97CC3A9F09F98802942", "C3A9F09F988042", 1, 0, new( "N", "", "" ) ),
		new( 22, "C3A92B3F", "C3A9C3A9", 1, 0, new( "M", "0:2", "" ) ),
		new( 23, "283F692928C3A9295C31", "C389C3A9", 1, 0, new( "M", "0:4,0:2", "" ) ),
		new( 24, "282A4D41524B3ACEBB2928C3A929282A4D41524B3AF09F988029F09F9880", "C3A9F09F9880", 1, 0, new( "M", "0:6,0:2", "F09F9880" ) ),
		new( 25, "285C707B4C7D2B295C73285C5829", "CEBBC3A920F09F9880", 3, 0, new( "M", "0:9,0:4,5:4", "" ) ),
		new( 26, "285C707B4C7D2B295C73285C5829", "CEBBC3A920F09F9880", 3, 2, new( "M", "2:7,2:2,5:4", "" ) ),
		new( 27, "285C707B4C7D2B295C73285C5829", "CEBBC3A920F09F9880", 3, 4, new( "N", "", "" ) ),
		new( 28, "285C707B4C7D2B295C73285C5829", "CEBBC3A920F09F9880", 3, 5, new( "N", "", "" ) ),
		new( 29, "285C707B4C7D2B295C73285C5829", "CEBBC3A920F09F9880", 3, 9, new( "N", "", "" ) ),
		new( 30, "5C62C3A95C62", "C3A9", 3, 0, new( "M", "0:2", "" ) ),
		new( 31, "5C62C3A95C62", "C3A9", 3, 2, new( "N", "", "" ) ),
		new( 32, "5C622841295C62", "204120", 3, 0, new( "M", "1:1,1:1", "" ) ),
		new( 33, "41", "41", 0, 0, new( "C", "", "", "InvalidWorkLimit", 0 ), 0 ),
		new( 34, "41", "41", 0, 0, new( "C", "", "", "InvalidWorkLimit", 0 ), -1 ),
		new( 35, "41", "41", 0, -1, new( "X", "", "", "InvalidStartOffset", -1 ), 1000000 ),
		new( 36, "41", "41", 0, 2, new( "X", "", "", "InvalidStartOffset", 2 ), 1000000 ),
		new( 37, "FF", "", 1, 0, new( "C", "", "", "InvalidUtf8Pattern", 0 ), 1000000 ),
		new( 38, "41FF", "", 1, 0, new( "C", "", "", "InvalidUtf8Pattern", 1 ), 1000000 ),
		new( 39, "2E", "FF", 1, 0, new( "X", "", "", "InvalidUtf8Subject", 0 ), 1000000 ),
		new( 40, "2E", "41FF", 1, 0, new( "X", "", "", "InvalidUtf8Subject", 1 ), 1000000 ),
		new( 41, "41", "41", 0, 0, new( "X", "", "", "MatchWorkLimitExceeded", 0 ), 1 ),
		new( 42, "5C43", "41", 1, 0, new( "C", "", "", "UnsupportedSyntax", 0 ), 1000000 ),
	};
}
