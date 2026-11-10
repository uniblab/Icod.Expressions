using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

// The catalogue/order is PCRE2 10.48's ucp_boolprop enum, not every UCD property.
string[] names = {
	"ASCII", "ASCII_Hex_Digit", "Alphabetic", "Bidi_Control", "Bidi_Mirrored",
	"Case_Ignorable", "Cased", "Changes_When_Casefolded", "Changes_When_Casemapped",
	"Changes_When_Lowercased", "Changes_When_Titlecased", "Changes_When_Uppercased",
	"Dash", "Default_Ignorable_Code_Point", "Deprecated", "Diacritic", "Emoji", "Emoji_Component",
	"Emoji_Modifier", "Emoji_Modifier_Base", "Emoji_Presentation", "Extended_Pictographic",
	"Extender", "Grapheme_Base", "Grapheme_Extend", "Grapheme_Link", "Hex_Digit",
	"IDS_Binary_Operator", "IDS_Trinary_Operator", "IDS_Unary_Operator",
	"ID_Compat_Math_Continue", "ID_Compat_Math_Start", "ID_Continue", "ID_Start",
	"Ideographic", "InCB", "Join_Control", "Logical_Order_Exception", "Lowercase", "Math",
	"Modifier_Combining_Mark", "Noncharacter_Code_Point", "Pattern_Syntax",
	"Pattern_White_Space", "Prepended_Concatenation_Mark", "Quotation_Mark", "Radical",
	"Regional_Indicator", "Sentence_Terminal", "Soft_Dotted", "Terminal_Punctuation",
	"Unified_Ideograph", "Uppercase", "Variation_Selector", "White_Space", "XID_Continue", "XID_Start"
};
string[] hashes = {
	"130DCDDCAADAF071008BDFCE1E7743E04FDFBC910886F017D9F9AC931D8C64DD",
	"24C7FED1195C482FAAEFD5C1E7EB821C5EE1FB6DE07ECDBAA64B56A99DA22C08",
	"2CB2BB9455CDA83E8481541ECF5B6DFDA66A3BB89EFA3FA7C5297ECCF607B72B",
	"A2F16FB873AB4FCDF3221CB1A8A85A134DDD6ED03603181823FF5206AF3741CE",
	"4867B4B7F0731ED1BFCD34CC6251211FF1542541FCE0734B6FBDA139EE80B3A4",
	"4441F573CAF952FFECE1D7C892E7715BD7136DFC26F96EB6F268BF1E474715FB",
	"64E9A5F76F7A1E8B5A47D6A1F9A26522A251208F5276BDFA1559DAC7CF2E827A"
};
const string nativeHash = "774FE3490BF3F2331C3A2FE2452622EFE159B700070F90AD3B08131A7597A440";
var verify = args.Length > 0 && args[0] == "--verify";
var paths = verify ? args[1..] : args;
if ( paths.Length != 8 ) {
	Console.Error.WriteLine( "Usage: GeneratePcreUnicodeProperties [--verify] PropList.txt DerivedCoreProperties.txt emoji-data.txt BidiMirroring.txt DerivedBidiClass.txt PropertyAliases.txt PropertyValueAliases.txt PcreUnicodeProperties.Data.cs" );
	return 2;
}
var sources = new string[7];
for ( var i = 0; i < sources.Length; i++ ) {
	var bytes = File.ReadAllBytes( paths[i] );
	if ( Convert.ToHexString( SHA256.HashData( bytes ) ) != hashes[i] ) {
		Console.Error.WriteLine( "Expected the pinned Unicode17 source SHA256: " + paths[i] );
		return 1;
	}
	sources[i] = Encoding.UTF8.GetString( bytes );
}
var ids = names.Select( (name, id) => (name, id) ).ToDictionary( item => item.name, item => item.id, StringComparer.Ordinal );
var flags = new ulong[0x110000];
for ( var i = 0; i < 3; i++ ) {
	foreach ( var fields in Fields( sources[i] ) ) {
		if ( !ids.TryGetValue( fields[1], out var id ) ) { continue; }
		var (first, last) = Limits( fields[0] );
		// Native InCB is the union of all non-None values (ignore the third field).
		for ( var scalar = first; scalar <= last; scalar++ ) { flags[scalar] |= 1UL << id; }
	}
}
for ( var scalar = 0; scalar < 128; scalar++ ) { flags[scalar] |= 1UL << ids["ASCII"]; }
// Native mirrored membership follows listed BidiMirroring entries, not every Y in UnicodeData.
foreach ( var fields in Fields( sources[3] ) ) {
	var (first, last) = Limits( fields[0] );
	if ( first != last ) { throw new InvalidDataException( "Unexpected mirrored range." ); }
	flags[first] |= 1UL << ids["Bidi_Mirrored"];
}
var bidiRows = Fields( sources[6] ).Where( fields => fields[0] == "bc" )
	.OrderBy( fields => fields[1], StringComparer.Ordinal ).ToArray();
if ( bidiRows.Length != 23 ) { throw new InvalidDataException( "Unexpected native Bidi class count." ); }
var bidiNames = bidiRows.Select( fields => fields[1] ).ToArray();
var bidiIds = new Dictionary<string, byte>( StringComparer.Ordinal );
for ( var id = 0; id < bidiRows.Length; id++ ) {
	foreach ( var value in bidiRows[id].Skip( 1 ).Where( value => value.Length > 0 ) ) { bidiIds.Add( value, (byte)id ); }
}
var bidi = new byte[flags.Length];
Array.Fill( bidi, bidiIds["L"] );
// @missing entries include narrower R/AL/ET defaults for unassigned code points.
// Apply them in file order before the explicit rows, as in the pinned native generator.
foreach ( var raw in sources[4].Split( '\n' ) ) {
	var line = raw.StartsWith( "# @missing: ", StringComparison.Ordinal ) ? raw[12..] : raw;
	foreach ( var fields in Fields( line ) ) {
		var (first, last) = Limits( fields[0] );
		Array.Fill( bidi, bidiIds[fields[1]], first, last - first + 1 );
	}
}
var aliases = new SortedDictionary<string, byte>( StringComparer.Ordinal ) {
	["ASCII"] = (byte)ids["ASCII"], ["INCB"] = (byte)ids["InCB"]
};
foreach ( var fields in Fields( sources[5] ) ) {
	if ( !ids.TryGetValue( fields[1], out var id ) ) { continue; }
	foreach ( var alias in fields.Where( value => value.Length > 0 ) ) {
		var name = Normalize( alias );
		if ( aliases.TryGetValue( name, out var previous ) && previous != id ) { throw new InvalidDataException( "Colliding binary aliases." ); }
		aliases[name] = (byte)id;
	}
}
if ( aliases.Count != 106 ) { throw new InvalidDataException( "Unexpected native binary alias count." ); }
var fingerprint = new byte[( 0x110000 - 0x800 ) * 9];
for ( var scalar = 0; scalar < flags.Length; scalar++ ) {
	if ( scalar is >= 0xD800 and <= 0xDFFF ) { continue; }
	var at = ( scalar - ( scalar > 0xDFFF ? 0x800 : 0 ) ) * 9;
	BinaryPrimitives.WriteUInt64LittleEndian( fingerprint.AsSpan( at, 8 ), flags[scalar] );
	fingerprint[at + 8] = bidi[scalar];
}
if ( Convert.ToHexString( SHA256.HashData( fingerprint ) ) != nativeHash ) {
	throw new InvalidDataException( "Unicode source membership differs from the pinned native fingerprint." );
}
var output = new StringBuilder();
void Line( string value = "" ) => output.Append( value ).Append( '\n' );
Line( "// <auto-generated />" );
Line( "// Unicode 17.0.0 binary/Bidi properties; tools/GeneratePcreUnicodeProperties." );
foreach ( var hash in hashes ) { Line( "// Source SHA256: " + hash ); }
Line( "// Native scalar membership SHA256: " + nativeHash );
Line( "// Copyright © 2025 Unicode, Inc. See THIRD-PARTY-NOTICES.txt (Unicode License V3)." );
Line();
Line( "namespace Icod.Expressions.Pcre;" );
Line();
Line( "internal static partial class PcreUnicodeProperties {" );
Line( "\tinternal const int BinaryCount = 57;" );
Line( "\tinternal const int BidiCount = 23;" );
void EmitNames( string field, IEnumerable<string> source ) {
	Line( "\tprivate static readonly string[] " + field + " = {" );
	var entries = source.Select( name => "\"" + name + "\"" ).ToArray();
	for ( var first = 0; first < entries.Length; first += 4 ) { Line( "\t\t" + string.Join( ", ", entries.Skip( first ).Take( 4 ) ) + "," ); }
	Line( "\t};" );
}
EmitNames( "BinaryNames", aliases.Keys );
Line( "\tprivate static ReadOnlySpan<byte> BinaryIds => [" );
var aliasIds = aliases.Values.Select( value => value.ToString( CultureInfo.InvariantCulture ) ).ToArray();
for ( var first = 0; first < aliasIds.Length; first += 16 ) { Line( "\t\t" + string.Join( ", ", aliasIds.Skip( first ).Take( 16 ) ) + "," ); }
Line( "\t];" );
EmitNames( "BidiNames", bidiNames );
void EmitRanges( string prefix, int count, Func<int, int, bool> contains ) {
	var offsets = new List<string> { "0" };
	var entries = new List<string>();
	for ( var id = 0; id < count; id++ ) {
		var start = -1;
		for ( var scalar = 0; scalar <= flags.Length; scalar++ ) {
			if ( scalar < flags.Length && contains( scalar, id ) ) {
				if ( start < 0 ) { start = scalar; }
			} else if ( start >= 0 ) {
				entries.Add( "0x" + ( ( (ulong)start << 21 ) | (uint)( scalar - 1 ) ).ToString( "X16", CultureInfo.InvariantCulture ) + "UL" );
				start = -1;
			}
		}
		offsets.Add( checked((ushort)entries.Count).ToString( CultureInfo.InvariantCulture ) );
	}
	Line();
	Line( "\tprivate static ReadOnlySpan<ushort> " + prefix + "Offsets => [" );
	for ( var first = 0; first < offsets.Count; first += 12 ) { Line( "\t\t" + string.Join( ", ", offsets.Skip( first ).Take( 12 ) ) + "," ); }
	Line( "\t];" );
	Line( "\tprivate static ReadOnlySpan<ulong> " + prefix + "Ranges => [" );
	for ( var first = 0; first < entries.Count; first += 4 ) { Line( "\t\t" + string.Join( ", ", entries.Skip( first ).Take( 4 ) ) + "," ); }
	Line( "\t];" );
}
EmitRanges( "Binary", names.Length, (scalar, id) => ( flags[scalar] & ( 1UL << id ) ) != 0 );
EmitRanges( "Bidi", bidiNames.Length, (scalar, id) => bidi[scalar] == id );
Line( "}" );
var generated = new UTF8Encoding( false ).GetBytes( output.ToString() );
if ( verify ) {
	if ( !File.ReadAllBytes( paths[7] ).AsSpan().SequenceEqual( generated ) ) {
		Console.Error.WriteLine( "Generated Unicode binary/Bidi properties differ from committed data." );
		return 1;
	}
	Console.WriteLine( $"Verified Unicode17 properties: {names.Length} binary, {aliases.Count} aliases, {bidiNames.Length} Bidi classes, native fingerprint {nativeHash}." );
	return 0;
}
File.WriteAllBytes( paths[7], generated );
return 0;

static string Normalize( string value ) => new( value.Where( c => c is not ( '_' or '-' or ' ' ) )
	.Select( c => c is >= 'a' and <= 'z' ? (char)( c - 32 ) : c ).ToArray() );

static IEnumerable<string[]> Fields( string source ) {
	foreach ( var line in source.Split( '\n' ) ) {
		var text = line.Split( '#' )[0].Trim();
		if ( text.Length == 0 ) { continue; }
		var fields = text.Split( ';' ).Select( field => field.Trim() ).ToArray();
		if ( fields.Length < 2 ) { throw new InvalidDataException( "Malformed Unicode source row." ); }
		yield return fields;
	}
}

static (int First, int Last) Limits( string text ) {
	var limits = text.Split( "..", StringSplitOptions.None );
	var first = int.Parse( limits[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture );
	var last = int.Parse( limits[^1], NumberStyles.HexNumber, CultureInfo.InvariantCulture );
	if ( first < 0 || first > last || last > 0x10FFFF ) { throw new InvalidDataException( "Invalid Unicode range." ); }
	return (first, last);
}
