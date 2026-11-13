using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins standard ALT_BSUX token recognition without source rewriting.</summary>
public sealed class PcreAltBsuxReferenceTests {
	[Theory]
	[InlineData( "5C7530313058", "7530313058", true, 0, "0:5", "" )]
	[InlineData( "5C7530313058", "7530313058", true, 2, "0:5", "" )]
	[InlineData( "5C7530313058", "7530313058", true, 3, "0:5", "" )]
	[InlineData( "5C7530313058", "7530313058", true, 7, "0:5", "" )]
	[InlineData( "5C7530313058", "7530313058", true, 11, "0:5", "" )]
	[InlineData( "5C7530313058", "7530313058", true, 19, "0:5", "" )]
	[InlineData( "5C75303035437530303431", "5C7530303431", true, 0, "0:6", "" )]
	[InlineData( "5C75303035437530303431", "5C7530303431", true, 2, "0:6", "" )]
	[InlineData( "5C75303035437530303431", "5C7530303431", true, 3, "0:6", "" )]
	[InlineData( "5C75303035437530303431", "5C7530303431", true, 7, "0:6", "" )]
	[InlineData( "5C75303035437530303431", "5C7530303431", true, 11, "0:6", "" )]
	[InlineData( "5C75303035437530303431", "5C7530303431", true, 19, "0:6", "" )]
	[InlineData( "5C7835437530303431", "5C7530303431", false, 0, "0:6", "" )]
	[InlineData( "5C7835437530303431", "5C7530303431", false, 2, "0:6", "" )]
	[InlineData( "5C7835437530303431", "5C7530303431", false, 3, "0:6", "" )]
	[InlineData( "5C7835437530303431", "5C7530303431", false, 7, "0:6", "" )]
	[InlineData( "5C7835437530303431", "5C7530303431", false, 11, "0:6", "" )]
	[InlineData( "5C7835437530303431", "5C7530303431", false, 19, "0:6", "" )]
	[InlineData( "5C7835437530303431", "5C7530303431", true, 0, "0:6", "" )]
	[InlineData( "5C7835437530303431", "5C7530303431", true, 2, "0:6", "" )]
	[InlineData( "5C7835437530303431", "5C7530303431", true, 3, "0:6", "" )]
	[InlineData( "5C7835437530303431", "5C7530303431", true, 7, "0:6", "" )]
	[InlineData( "5C7835437530303431", "5C7530303431", true, 11, "0:6", "" )]
	[InlineData( "5C7835437530303431", "5C7530303431", true, 19, "0:6", "" )]
	[InlineData( "5C7530303431", "41", true, 0, "0:1", "" )]
	[InlineData( "5C7530303431", "61", true, 0, "", "" )]
	[InlineData( "5C7530303431", "7530303431", true, 0, "", "" )]
	[InlineData( "5C7530303431", "41", true, 2, "0:1", "" )]
	[InlineData( "5C7530303431", "61", true, 2, "", "" )]
	[InlineData( "5C7530303431", "7530303431", true, 2, "", "" )]
	[InlineData( "5C7530303431", "41", true, 3, "0:1", "" )]
	[InlineData( "5C7530303431", "61", true, 3, "", "" )]
	[InlineData( "5C7530303431", "7530303431", true, 3, "", "" )]
	[InlineData( "5C7530303431", "41", true, 7, "0:1", "" )]
	[InlineData( "5C7530303431", "61", true, 7, "", "" )]
	[InlineData( "5C7530303431", "7530303431", true, 7, "", "" )]
	[InlineData( "5C7530303431", "41", true, 11, "0:1", "" )]
	[InlineData( "5C7530303431", "61", true, 11, "0:1", "" )]
	[InlineData( "5C7530303431", "7530303431", true, 11, "", "" )]
	[InlineData( "5C7530303431", "41", true, 19, "0:1", "" )]
	[InlineData( "5C7530303431", "61", true, 19, "", "" )]
	[InlineData( "5C7530303431", "7530303431", true, 19, "", "" )]
	[InlineData( "5C7530306539", "E9", true, 0, "0:1", "" )]
	[InlineData( "5C7530306539", "C3A9", true, 0, "", "" )]
	[InlineData( "5C7530306539", "E9", true, 2, "0:1", "" )]
	[InlineData( "5C7530306539", "C3A9", true, 2, "", "" )]
	[InlineData( "5C7530306539", "E9", true, 3, "", "" )]
	[InlineData( "5C7530306539", "C3A9", true, 3, "0:2", "" )]
	[InlineData( "5C7530306539", "E9", true, 7, "", "" )]
	[InlineData( "5C7530306539", "C3A9", true, 7, "0:2", "" )]
	[InlineData( "5C7530306539", "E9", true, 11, "", "" )]
	[InlineData( "5C7530306539", "C3A9", true, 11, "0:2", "" )]
	[InlineData( "5C7530306539", "E9", true, 19, "", "" )]
	[InlineData( "5C7530306539", "C3A9", true, 19, "0:2", "" )]
	[InlineData( "5C7530334232", "CEB2", true, 3, "0:2", "" )]
	[InlineData( "5C7530334232", "CEB2", true, 7, "0:2", "" )]
	[InlineData( "5C7530334232", "CEB2", true, 11, "0:2", "" )]
	[InlineData( "5C7530334232", "CEB2", true, 19, "0:2", "" )]
	[InlineData( "5C7532313241", "4B", true, 3, "", "" )]
	[InlineData( "5C7532313241", "6B", true, 3, "", "" )]
	[InlineData( "5C7532313241", "E284AA", true, 3, "0:3", "" )]
	[InlineData( "5C7532313241", "4B", true, 7, "", "" )]
	[InlineData( "5C7532313241", "6B", true, 7, "", "" )]
	[InlineData( "5C7532313241", "E284AA", true, 7, "0:3", "" )]
	[InlineData( "5C7532313241", "4B", true, 11, "0:1", "" )]
	[InlineData( "5C7532313241", "6B", true, 11, "0:1", "" )]
	[InlineData( "5C7532313241", "E284AA", true, 11, "0:3", "" )]
	[InlineData( "5C7532313241", "4B", true, 19, "", "" )]
	[InlineData( "5C7532313241", "6B", true, 19, "", "" )]
	[InlineData( "5C7532313241", "E284AA", true, 19, "0:3", "" )]
	[InlineData( "5C7546464646", "EFBFBF", true, 3, "0:3", "" )]
	[InlineData( "5C7546464646", "EFBFBF", true, 7, "0:3", "" )]
	[InlineData( "5C7546464646", "EFBFBF", true, 11, "0:3", "" )]
	[InlineData( "5C7546464646", "EFBFBF", true, 19, "0:3", "" )]
	[InlineData( "5C75", "75", true, 0, "0:1", "" )]
	[InlineData( "5C75", "75", true, 2, "0:1", "" )]
	[InlineData( "5C75", "75", true, 3, "0:1", "" )]
	[InlineData( "5C75", "75", true, 7, "0:1", "" )]
	[InlineData( "5C75", "75", true, 11, "0:1", "" )]
	[InlineData( "5C75", "75", true, 19, "0:1", "" )]
	[InlineData( "5C7531", "7531", true, 0, "0:2", "" )]
	[InlineData( "5C7531", "7531", true, 2, "0:2", "" )]
	[InlineData( "5C7531", "7531", true, 3, "0:2", "" )]
	[InlineData( "5C7531", "7531", true, 7, "0:2", "" )]
	[InlineData( "5C7531", "7531", true, 11, "0:2", "" )]
	[InlineData( "5C7531", "7531", true, 19, "0:2", "" )]
	[InlineData( "5C753132", "753132", true, 0, "0:3", "" )]
	[InlineData( "5C753132", "753132", true, 2, "0:3", "" )]
	[InlineData( "5C753132", "753132", true, 3, "0:3", "" )]
	[InlineData( "5C753132", "753132", true, 7, "0:3", "" )]
	[InlineData( "5C753132", "753132", true, 11, "0:3", "" )]
	[InlineData( "5C753132", "753132", true, 19, "0:3", "" )]
	[InlineData( "5C75313233", "75313233", true, 0, "0:4", "" )]
	[InlineData( "5C75313233", "75313233", true, 2, "0:4", "" )]
	[InlineData( "5C75313233", "75313233", true, 3, "0:4", "" )]
	[InlineData( "5C75313233", "75313233", true, 7, "0:4", "" )]
	[InlineData( "5C75313233", "75313233", true, 11, "0:4", "" )]
	[InlineData( "5C75313233", "75313233", true, 19, "0:4", "" )]
	[InlineData( "5C7530305131", "7530305131", true, 0, "0:5", "" )]
	[InlineData( "5C7530305131", "7530305131", true, 2, "0:5", "" )]
	[InlineData( "5C7530305131", "7530305131", true, 3, "0:5", "" )]
	[InlineData( "5C7530305131", "7530305131", true, 7, "0:5", "" )]
	[InlineData( "5C7530305131", "7530305131", true, 11, "0:5", "" )]
	[InlineData( "5C7530305131", "7530305131", true, 19, "0:5", "" )]
	[InlineData( "5C753030343131", "4131", true, 0, "0:2", "" )]
	[InlineData( "5C753030343131", "4131", true, 2, "0:2", "" )]
	[InlineData( "5C753030343131", "4131", true, 3, "0:2", "" )]
	[InlineData( "5C753030343131", "4131", true, 7, "0:2", "" )]
	[InlineData( "5C753030343131", "4131", true, 11, "0:2", "" )]
	[InlineData( "5C753030343131", "4131", true, 19, "0:2", "" )]
	[InlineData( "5C55", "55", true, 0, "0:1", "" )]
	[InlineData( "5C55", "55", true, 2, "0:1", "" )]
	[InlineData( "5C55", "55", true, 3, "0:1", "" )]
	[InlineData( "5C55", "55", true, 7, "0:1", "" )]
	[InlineData( "5C55", "55", true, 11, "0:1", "" )]
	[InlineData( "5C55", "55", true, 19, "0:1", "" )]
	[InlineData( "5C553030303030303431", "553030303030303431", true, 0, "0:9", "" )]
	[InlineData( "5C553030303030303431", "553030303030303431", true, 2, "0:9", "" )]
	[InlineData( "5C553030303030303431", "553030303030303431", true, 3, "0:9", "" )]
	[InlineData( "5C553030303030303431", "553030303030303431", true, 7, "0:9", "" )]
	[InlineData( "5C553030303030303431", "553030303030303431", true, 11, "0:9", "" )]
	[InlineData( "5C553030303030303431", "553030303030303431", true, 19, "0:9", "" )]
	[InlineData( "5C757B327D", "7575", true, 0, "0:2", "" )]
	[InlineData( "5C757B327D", "02", true, 0, "", "" )]
	[InlineData( "5C757B327D", "7575", true, 2, "0:2", "" )]
	[InlineData( "5C757B327D", "02", true, 2, "", "" )]
	[InlineData( "5C757B327D", "7575", true, 3, "0:2", "" )]
	[InlineData( "5C757B327D", "02", true, 3, "", "" )]
	[InlineData( "5C757B327D", "7575", true, 7, "0:2", "" )]
	[InlineData( "5C757B327D", "02", true, 7, "", "" )]
	[InlineData( "5C757B327D", "7575", true, 11, "0:2", "" )]
	[InlineData( "5C757B327D", "02", true, 11, "", "" )]
	[InlineData( "5C757B327D", "7575", true, 19, "0:2", "" )]
	[InlineData( "5C757B327D", "02", true, 19, "", "" )]
	[InlineData( "5C757B20327D", "7575", true, 0, "0:2", "" )]
	[InlineData( "5C757B20327D", "7575", true, 2, "0:2", "" )]
	[InlineData( "5C757B20327D", "7575", true, 3, "0:2", "" )]
	[InlineData( "5C757B20327D", "7575", true, 7, "0:2", "" )]
	[InlineData( "5C757B20327D", "7575", true, 11, "0:2", "" )]
	[InlineData( "5C757B20327D", "7575", true, 19, "0:2", "" )]
	[InlineData( "5C757B322C337D", "757575", true, 0, "0:3", "" )]
	[InlineData( "5C757B322C337D", "757575", true, 2, "0:3", "" )]
	[InlineData( "5C757B322C337D", "757575", true, 3, "0:3", "" )]
	[InlineData( "5C757B322C337D", "757575", true, 7, "0:3", "" )]
	[InlineData( "5C757B322C337D", "757575", true, 11, "0:3", "" )]
	[InlineData( "5C757B322C337D", "757575", true, 19, "0:3", "" )]
	[InlineData( "5C757B477D", "757B477D", true, 0, "0:4", "" )]
	[InlineData( "5C757B477D", "757B477D", true, 2, "0:4", "" )]
	[InlineData( "5C757B477D", "757B477D", true, 3, "0:4", "" )]
	[InlineData( "5C757B477D", "757B477D", true, 7, "0:4", "" )]
	[InlineData( "5C757B477D", "757B477D", true, 11, "0:4", "" )]
	[InlineData( "5C757B477D", "757B477D", true, 19, "0:4", "" )]
	[InlineData( "5C78", "78", true, 0, "0:1", "" )]
	[InlineData( "5C78", "78", true, 2, "0:1", "" )]
	[InlineData( "5C78", "78", true, 3, "0:1", "" )]
	[InlineData( "5C78", "78", true, 7, "0:1", "" )]
	[InlineData( "5C78", "78", true, 11, "0:1", "" )]
	[InlineData( "5C78", "78", true, 19, "0:1", "" )]
	[InlineData( "5C7841", "7841", false, 0, "", "" )]
	[InlineData( "5C7841", "7841", false, 2, "", "" )]
	[InlineData( "5C7841", "7841", false, 3, "", "" )]
	[InlineData( "5C7841", "7841", false, 7, "", "" )]
	[InlineData( "5C7841", "7841", false, 11, "", "" )]
	[InlineData( "5C7841", "7841", false, 19, "", "" )]
	[InlineData( "5C7841", "7841", true, 0, "0:2", "" )]
	[InlineData( "5C7841", "7841", true, 2, "0:2", "" )]
	[InlineData( "5C7841", "7841", true, 3, "0:2", "" )]
	[InlineData( "5C7841", "7841", true, 7, "0:2", "" )]
	[InlineData( "5C7841", "7841", true, 11, "0:2", "" )]
	[InlineData( "5C7841", "7841", true, 19, "0:2", "" )]
	[InlineData( "5C7830", "7830", false, 0, "", "" )]
	[InlineData( "5C7830", "7830", false, 2, "", "" )]
	[InlineData( "5C7830", "7830", false, 3, "", "" )]
	[InlineData( "5C7830", "7830", false, 7, "", "" )]
	[InlineData( "5C7830", "7830", false, 11, "", "" )]
	[InlineData( "5C7830", "7830", false, 19, "", "" )]
	[InlineData( "5C7830", "7830", true, 0, "0:2", "" )]
	[InlineData( "5C7830", "7830", true, 2, "0:2", "" )]
	[InlineData( "5C7830", "7830", true, 3, "0:2", "" )]
	[InlineData( "5C7830", "7830", true, 7, "0:2", "" )]
	[InlineData( "5C7830", "7830", true, 11, "0:2", "" )]
	[InlineData( "5C7830", "7830", true, 19, "0:2", "" )]
	[InlineData( "5C783451", "783451", false, 0, "", "" )]
	[InlineData( "5C783451", "783451", false, 2, "", "" )]
	[InlineData( "5C783451", "783451", false, 3, "", "" )]
	[InlineData( "5C783451", "783451", false, 7, "", "" )]
	[InlineData( "5C783451", "783451", false, 11, "", "" )]
	[InlineData( "5C783451", "783451", false, 19, "", "" )]
	[InlineData( "5C783451", "783451", true, 0, "0:3", "" )]
	[InlineData( "5C783451", "783451", true, 2, "0:3", "" )]
	[InlineData( "5C783451", "783451", true, 3, "0:3", "" )]
	[InlineData( "5C783451", "783451", true, 7, "0:3", "" )]
	[InlineData( "5C783451", "783451", true, 11, "0:3", "" )]
	[InlineData( "5C783451", "783451", true, 19, "0:3", "" )]
	[InlineData( "5C78343134", "4134", false, 0, "0:2", "" )]
	[InlineData( "5C78343134", "4134", false, 2, "0:2", "" )]
	[InlineData( "5C78343134", "4134", false, 3, "0:2", "" )]
	[InlineData( "5C78343134", "4134", false, 7, "0:2", "" )]
	[InlineData( "5C78343134", "4134", false, 11, "0:2", "" )]
	[InlineData( "5C78343134", "4134", false, 19, "0:2", "" )]
	[InlineData( "5C78343134", "4134", true, 0, "0:2", "" )]
	[InlineData( "5C78343134", "4134", true, 2, "0:2", "" )]
	[InlineData( "5C78343134", "4134", true, 3, "0:2", "" )]
	[InlineData( "5C78343134", "4134", true, 7, "0:2", "" )]
	[InlineData( "5C78343134", "4134", true, 11, "0:2", "" )]
	[InlineData( "5C78343134", "4134", true, 19, "0:2", "" )]
	[InlineData( "5C78412B", "78414141", false, 0, "", "" )]
	[InlineData( "5C78412B", "78414141", false, 2, "", "" )]
	[InlineData( "5C78412B", "78414141", false, 3, "", "" )]
	[InlineData( "5C78412B", "78414141", false, 7, "", "" )]
	[InlineData( "5C78412B", "78414141", false, 11, "", "" )]
	[InlineData( "5C78412B", "78414141", false, 19, "", "" )]
	[InlineData( "5C78412B", "78414141", true, 0, "0:4", "" )]
	[InlineData( "5C78412B", "78414141", true, 2, "0:4", "" )]
	[InlineData( "5C78412B", "78414141", true, 3, "0:4", "" )]
	[InlineData( "5C78412B", "78414141", true, 7, "0:4", "" )]
	[InlineData( "5C78412B", "78414141", true, 11, "0:4", "" )]
	[InlineData( "5C78412B", "78414141", true, 19, "0:4", "" )]
	[InlineData( "5C787B477D", "787B477D", true, 0, "0:4", "" )]
	[InlineData( "5C787B477D", "787B477D", true, 2, "0:4", "" )]
	[InlineData( "5C787B477D", "787B477D", true, 3, "0:4", "" )]
	[InlineData( "5C787B477D", "787B477D", true, 7, "0:4", "" )]
	[InlineData( "5C787B477D", "787B477D", true, 11, "0:4", "" )]
	[InlineData( "5C787B477D", "787B477D", true, 19, "0:4", "" )]
	[InlineData( "5C787B327D", "7878", false, 0, "", "" )]
	[InlineData( "5C787B327D", "7878", false, 2, "", "" )]
	[InlineData( "5C787B327D", "7878", false, 3, "", "" )]
	[InlineData( "5C787B327D", "7878", false, 7, "", "" )]
	[InlineData( "5C787B327D", "7878", false, 11, "", "" )]
	[InlineData( "5C787B327D", "7878", false, 19, "", "" )]
	[InlineData( "5C787B327D", "7878", true, 0, "0:2", "" )]
	[InlineData( "5C787B327D", "7878", true, 2, "0:2", "" )]
	[InlineData( "5C787B327D", "7878", true, 3, "0:2", "" )]
	[InlineData( "5C787B327D", "7878", true, 7, "0:2", "" )]
	[InlineData( "5C787B327D", "7878", true, 11, "0:2", "" )]
	[InlineData( "5C787B327D", "7878", true, 19, "0:2", "" )]
	[InlineData( "5C787B34317D", "7878787878787878787878787878787878787878787878787878787878787878787878787878787878", false, 0, "", "" )]
	[InlineData( "5C787B34317D", "7878787878787878787878787878787878787878787878787878787878787878787878787878787878", false, 2, "", "" )]
	[InlineData( "5C787B34317D", "7878787878787878787878787878787878787878787878787878787878787878787878787878787878", false, 3, "", "" )]
	[InlineData( "5C787B34317D", "7878787878787878787878787878787878787878787878787878787878787878787878787878787878", false, 7, "", "" )]
	[InlineData( "5C787B34317D", "7878787878787878787878787878787878787878787878787878787878787878787878787878787878", false, 11, "", "" )]
	[InlineData( "5C787B34317D", "7878787878787878787878787878787878787878787878787878787878787878787878787878787878", false, 19, "", "" )]
	[InlineData( "5C787B34317D", "7878787878787878787878787878787878787878787878787878787878787878787878787878787878", true, 0, "0:41", "" )]
	[InlineData( "5C787B34317D", "7878787878787878787878787878787878787878787878787878787878787878787878787878787878", true, 2, "0:41", "" )]
	[InlineData( "5C787B34317D", "7878787878787878787878787878787878787878787878787878787878787878787878787878787878", true, 3, "0:41", "" )]
	[InlineData( "5C787B34317D", "7878787878787878787878787878787878787878787878787878787878787878787878787878787878", true, 7, "0:41", "" )]
	[InlineData( "5C787B34317D", "7878787878787878787878787878787878787878787878787878787878787878787878787878787878", true, 11, "0:41", "" )]
	[InlineData( "5C787B34317D", "7878787878787878787878787878787878787878787878787878787878787878787878787878787878", true, 19, "0:41", "" )]
	[InlineData( "5C783431", "41", false, 0, "0:1", "" )]
	[InlineData( "5C783431", "61", false, 0, "", "" )]
	[InlineData( "5C783431", "41", false, 2, "0:1", "" )]
	[InlineData( "5C783431", "61", false, 2, "", "" )]
	[InlineData( "5C783431", "41", false, 3, "0:1", "" )]
	[InlineData( "5C783431", "61", false, 3, "", "" )]
	[InlineData( "5C783431", "41", false, 7, "0:1", "" )]
	[InlineData( "5C783431", "61", false, 7, "", "" )]
	[InlineData( "5C783431", "41", false, 11, "0:1", "" )]
	[InlineData( "5C783431", "61", false, 11, "0:1", "" )]
	[InlineData( "5C783431", "41", false, 19, "0:1", "" )]
	[InlineData( "5C783431", "61", false, 19, "", "" )]
	[InlineData( "5C783431", "41", true, 0, "0:1", "" )]
	[InlineData( "5C783431", "61", true, 0, "", "" )]
	[InlineData( "5C783431", "41", true, 2, "0:1", "" )]
	[InlineData( "5C783431", "61", true, 2, "", "" )]
	[InlineData( "5C783431", "41", true, 3, "0:1", "" )]
	[InlineData( "5C783431", "61", true, 3, "", "" )]
	[InlineData( "5C783431", "41", true, 7, "0:1", "" )]
	[InlineData( "5C783431", "61", true, 7, "", "" )]
	[InlineData( "5C783431", "41", true, 11, "0:1", "" )]
	[InlineData( "5C783431", "61", true, 11, "0:1", "" )]
	[InlineData( "5C783431", "41", true, 19, "0:1", "" )]
	[InlineData( "5C783431", "61", true, 19, "", "" )]
	[InlineData( "5C783041", "0A", false, 0, "0:1", "" )]
	[InlineData( "5C783041", "0A", false, 2, "0:1", "" )]
	[InlineData( "5C783041", "0A", false, 3, "0:1", "" )]
	[InlineData( "5C783041", "0A", false, 7, "0:1", "" )]
	[InlineData( "5C783041", "0A", false, 11, "0:1", "" )]
	[InlineData( "5C783041", "0A", false, 19, "0:1", "" )]
	[InlineData( "5C783041", "0A", true, 0, "0:1", "" )]
	[InlineData( "5C783041", "0A", true, 2, "0:1", "" )]
	[InlineData( "5C783041", "0A", true, 3, "0:1", "" )]
	[InlineData( "5C783041", "0A", true, 7, "0:1", "" )]
	[InlineData( "5C783041", "0A", true, 11, "0:1", "" )]
	[InlineData( "5C783041", "0A", true, 19, "0:1", "" )]
	[InlineData( "5C785C6341", "7801", true, 0, "0:2", "" )]
	[InlineData( "5C785C6341", "7801", true, 2, "0:2", "" )]
	[InlineData( "5C785C6341", "7801", true, 3, "0:2", "" )]
	[InlineData( "5C785C6341", "7801", true, 7, "0:2", "" )]
	[InlineData( "5C785C6341", "7801", true, 11, "0:2", "" )]
	[InlineData( "5C785C6341", "7801", true, 19, "0:2", "" )]
	[InlineData( "285C75303034312B295C31", "41414141", true, 0, "0:4,0:2", "" )]
	[InlineData( "285C75303034312B295C31", "41414141", true, 2, "0:4,0:2", "" )]
	[InlineData( "285C75303034312B295C31", "41414141", true, 3, "0:4,0:2", "" )]
	[InlineData( "285C75303034312B295C31", "41414141", true, 7, "0:4,0:2", "" )]
	[InlineData( "285C75303034312B295C31", "41414141", true, 11, "0:4,0:2", "" )]
	[InlineData( "285C75303034312B295C31", "41414141", true, 19, "0:4,0:2", "" )]
	[InlineData( "283F3C783E5C7530303431295C6B3C783E", "4141", true, 0, "0:2,0:1", "" )]
	[InlineData( "283F3C783E5C7530303431295C6B3C783E", "4161", true, 0, "", "" )]
	[InlineData( "283F3C783E5C7530303431295C6B3C783E", "4141", true, 2, "0:2,0:1", "" )]
	[InlineData( "283F3C783E5C7530303431295C6B3C783E", "4161", true, 2, "", "" )]
	[InlineData( "283F3C783E5C7530303431295C6B3C783E", "4141", true, 3, "0:2,0:1", "" )]
	[InlineData( "283F3C783E5C7530303431295C6B3C783E", "4161", true, 3, "", "" )]
	[InlineData( "283F3C783E5C7530303431295C6B3C783E", "4141", true, 7, "0:2,0:1", "" )]
	[InlineData( "283F3C783E5C7530303431295C6B3C783E", "4161", true, 7, "", "" )]
	[InlineData( "283F3C783E5C7530303431295C6B3C783E", "4141", true, 11, "0:2,0:1", "" )]
	[InlineData( "283F3C783E5C7530303431295C6B3C783E", "4161", true, 11, "0:2,0:1", "" )]
	[InlineData( "283F3C783E5C7530303431295C6B3C783E", "4141", true, 19, "0:2,0:1", "" )]
	[InlineData( "283F3C783E5C7530303431295C6B3C783E", "4161", true, 19, "", "" )]
	[InlineData( "5C415B5C75303034312D5C75303035415D2B5C7A", "41425A", true, 0, "0:3", "" )]
	[InlineData( "5C415B5C75303034312D5C75303035415D2B5C7A", "61427A", true, 0, "", "" )]
	[InlineData( "5C415B5C75303034312D5C75303035415D2B5C7A", "41425A", true, 2, "0:3", "" )]
	[InlineData( "5C415B5C75303034312D5C75303035415D2B5C7A", "61427A", true, 2, "", "" )]
	[InlineData( "5C415B5C75303034312D5C75303035415D2B5C7A", "41425A", true, 3, "0:3", "" )]
	[InlineData( "5C415B5C75303034312D5C75303035415D2B5C7A", "61427A", true, 3, "", "" )]
	[InlineData( "5C415B5C75303034312D5C75303035415D2B5C7A", "41425A", true, 7, "0:3", "" )]
	[InlineData( "5C415B5C75303034312D5C75303035415D2B5C7A", "61427A", true, 7, "", "" )]
	[InlineData( "5C415B5C75303034312D5C75303035415D2B5C7A", "41425A", true, 11, "0:3", "" )]
	[InlineData( "5C415B5C75303034312D5C75303035415D2B5C7A", "61427A", true, 11, "0:3", "" )]
	[InlineData( "5C415B5C75303034312D5C75303035415D2B5C7A", "41425A", true, 19, "0:3", "" )]
	[InlineData( "5C415B5C75303034312D5C75303035415D2B5C7A", "61427A", true, 19, "", "" )]
	[InlineData( "5C415B5E5C75303034315D5C7A", "42", true, 0, "0:1", "" )]
	[InlineData( "5C415B5E5C75303034315D5C7A", "41", true, 0, "", "" )]
	[InlineData( "5C415B5E5C75303034315D5C7A", "42", true, 2, "0:1", "" )]
	[InlineData( "5C415B5E5C75303034315D5C7A", "41", true, 2, "", "" )]
	[InlineData( "5C415B5E5C75303034315D5C7A", "42", true, 3, "0:1", "" )]
	[InlineData( "5C415B5E5C75303034315D5C7A", "41", true, 3, "", "" )]
	[InlineData( "5C415B5E5C75303034315D5C7A", "42", true, 7, "0:1", "" )]
	[InlineData( "5C415B5E5C75303034315D5C7A", "41", true, 7, "", "" )]
	[InlineData( "5C415B5E5C75303034315D5C7A", "42", true, 11, "0:1", "" )]
	[InlineData( "5C415B5E5C75303034315D5C7A", "41", true, 11, "", "" )]
	[InlineData( "5C415B5E5C75303034315D5C7A", "42", true, 19, "0:1", "" )]
	[InlineData( "5C415B5E5C75303034315D5C7A", "41", true, 19, "", "" )]
	[InlineData( "5C415B5C7834515D2B5C7A", "783451", false, 0, "", "" )]
	[InlineData( "5C415B5C7834515D2B5C7A", "783451", false, 2, "", "" )]
	[InlineData( "5C415B5C7834515D2B5C7A", "783451", false, 3, "", "" )]
	[InlineData( "5C415B5C7834515D2B5C7A", "783451", false, 7, "", "" )]
	[InlineData( "5C415B5C7834515D2B5C7A", "783451", false, 11, "", "" )]
	[InlineData( "5C415B5C7834515D2B5C7A", "783451", false, 19, "", "" )]
	[InlineData( "5C415B5C7834515D2B5C7A", "783451", true, 0, "0:3", "" )]
	[InlineData( "5C415B5C7834515D2B5C7A", "783451", true, 2, "0:3", "" )]
	[InlineData( "5C415B5C7834515D2B5C7A", "783451", true, 3, "0:3", "" )]
	[InlineData( "5C415B5C7834515D2B5C7A", "783451", true, 7, "0:3", "" )]
	[InlineData( "5C415B5C7834515D2B5C7A", "783451", true, 11, "0:3", "" )]
	[InlineData( "5C415B5C7834515D2B5C7A", "783451", true, 19, "0:3", "" )]
	[InlineData( "283F5B5C75303034315D29", "41", true, 0, "0:1", "" )]
	[InlineData( "283F5B5C75303034315D29", "61", true, 0, "", "" )]
	[InlineData( "283F5B5C75303034315D29", "41", true, 2, "0:1", "" )]
	[InlineData( "283F5B5C75303034315D29", "61", true, 2, "", "" )]
	[InlineData( "283F5B5C75303034315D29", "41", true, 3, "0:1", "" )]
	[InlineData( "283F5B5C75303034315D29", "61", true, 3, "", "" )]
	[InlineData( "283F5B5C75303034315D29", "41", true, 7, "0:1", "" )]
	[InlineData( "283F5B5C75303034315D29", "61", true, 7, "", "" )]
	[InlineData( "283F5B5C75303034315D29", "41", true, 11, "0:1", "" )]
	[InlineData( "283F5B5C75303034315D29", "61", true, 11, "0:1", "" )]
	[InlineData( "283F5B5C75303034315D29", "41", true, 19, "0:1", "" )]
	[InlineData( "283F5B5C75303034315D29", "61", true, 19, "", "" )]
	[InlineData( "283F5B5B5C75303034312D5C75303035415D2026205B5E425D5D29", "41", true, 0, "0:1", "" )]
	[InlineData( "283F5B5B5C75303034312D5C75303035415D2026205B5E425D5D29", "42", true, 0, "", "" )]
	[InlineData( "283F5B5B5C75303034312D5C75303035415D2026205B5E425D5D29", "41", true, 2, "0:1", "" )]
	[InlineData( "283F5B5B5C75303034312D5C75303035415D2026205B5E425D5D29", "42", true, 2, "", "" )]
	[InlineData( "283F5B5B5C75303034312D5C75303035415D2026205B5E425D5D29", "41", true, 3, "0:1", "" )]
	[InlineData( "283F5B5B5C75303034312D5C75303035415D2026205B5E425D5D29", "42", true, 3, "", "" )]
	[InlineData( "283F5B5B5C75303034312D5C75303035415D2026205B5E425D5D29", "41", true, 7, "0:1", "" )]
	[InlineData( "283F5B5B5C75303034312D5C75303035415D2026205B5E425D5D29", "42", true, 7, "", "" )]
	[InlineData( "283F5B5B5C75303034312D5C75303035415D2026205B5E425D5D29", "41", true, 11, "0:1", "" )]
	[InlineData( "283F5B5B5C75303034312D5C75303035415D2026205B5E425D5D29", "42", true, 11, "", "" )]
	[InlineData( "283F5B5B5C75303034312D5C75303035415D2026205B5E425D5D29", "41", true, 19, "0:1", "" )]
	[InlineData( "283F5B5B5C75303034312D5C75303035415D2026205B5E425D5D29", "42", true, 19, "", "" )]
	[InlineData( "283F3C3D5C753030343129285C753030343229", "4142", true, 0, "1:1,1:1", "" )]
	[InlineData( "283F3C3D5C753030343129285C753030343229", "4142", true, 2, "1:1,1:1", "" )]
	[InlineData( "283F3C3D5C753030343129285C753030343229", "4142", true, 3, "1:1,1:1", "" )]
	[InlineData( "283F3C3D5C753030343129285C753030343229", "4142", true, 7, "1:1,1:1", "" )]
	[InlineData( "283F3C3D5C753030343129285C753030343229", "4142", true, 11, "1:1,1:1", "" )]
	[InlineData( "283F3C3D5C753030343129285C753030343229", "4142", true, 19, "1:1,1:1", "" )]
	[InlineData( "283F3D5C753030343129285C753030343129", "41", true, 0, "0:1,0:1", "" )]
	[InlineData( "283F3D5C753030343129285C753030343129", "41", true, 2, "0:1,0:1", "" )]
	[InlineData( "283F3D5C753030343129285C753030343129", "41", true, 3, "0:1,0:1", "" )]
	[InlineData( "283F3D5C753030343129285C753030343129", "41", true, 7, "0:1,0:1", "" )]
	[InlineData( "283F3D5C753030343129285C753030343129", "41", true, 11, "0:1,0:1", "" )]
	[InlineData( "283F3D5C753030343129285C753030343129", "41", true, 19, "0:1,0:1", "" )]
	[InlineData( "5C75303034315C4B285C753030343229", "4142", true, 0, "1:1,1:1", "" )]
	[InlineData( "5C75303034315C4B285C753030343229", "4142", true, 2, "1:1,1:1", "" )]
	[InlineData( "5C75303034315C4B285C753030343229", "4142", true, 3, "1:1,1:1", "" )]
	[InlineData( "5C75303034315C4B285C753030343229", "4142", true, 7, "1:1,1:1", "" )]
	[InlineData( "5C75303034315C4B285C753030343229", "4142", true, 11, "1:1,1:1", "" )]
	[InlineData( "5C75303034315C4B285C753030343229", "4142", true, 19, "1:1,1:1", "" )]
	[InlineData( "283F3E5C75303034312B295C7530303432", "41414142", true, 0, "0:4", "" )]
	[InlineData( "283F3E5C75303034312B295C7530303432", "41414142", true, 2, "0:4", "" )]
	[InlineData( "283F3E5C75303034312B295C7530303432", "41414142", true, 3, "0:4", "" )]
	[InlineData( "283F3E5C75303034312B295C7530303432", "41414142", true, 7, "0:4", "" )]
	[InlineData( "283F3E5C75303034312B295C7530303432", "41414142", true, 11, "0:4", "" )]
	[InlineData( "283F3E5C75303034312B295C7530303432", "41414142", true, 19, "0:4", "" )]
	[InlineData( "283F3C783E5C7530303431283F2678293F4229", "41414242", true, 0, "0:4,0:4", "" )]
	[InlineData( "283F3C783E5C7530303431283F2678293F4229", "41414242", true, 2, "0:4,0:4", "" )]
	[InlineData( "283F3C783E5C7530303431283F2678293F4229", "41414242", true, 3, "0:4,0:4", "" )]
	[InlineData( "283F3C783E5C7530303431283F2678293F4229", "41414242", true, 7, "0:4,0:4", "" )]
	[InlineData( "283F3C783E5C7530303431283F2678293F4229", "41414242", true, 11, "0:4,0:4", "" )]
	[InlineData( "283F3C783E5C7530303431283F2678293F4229", "41414242", true, 19, "0:4,0:4", "" )]
	[InlineData( "283F7C5C7530303431285C7530303432297C5C7530303433285C753030343429295C31", "414242", true, 0, "0:3,1:1", "" )]
	[InlineData( "283F7C5C7530303431285C7530303432297C5C7530303433285C753030343429295C31", "434444", true, 0, "0:3,1:1", "" )]
	[InlineData( "283F7C5C7530303431285C7530303432297C5C7530303433285C753030343429295C31", "414242", true, 2, "0:3,1:1", "" )]
	[InlineData( "283F7C5C7530303431285C7530303432297C5C7530303433285C753030343429295C31", "434444", true, 2, "0:3,1:1", "" )]
	[InlineData( "283F7C5C7530303431285C7530303432297C5C7530303433285C753030343429295C31", "414242", true, 3, "0:3,1:1", "" )]
	[InlineData( "283F7C5C7530303431285C7530303432297C5C7530303433285C753030343429295C31", "434444", true, 3, "0:3,1:1", "" )]
	[InlineData( "283F7C5C7530303431285C7530303432297C5C7530303433285C753030343429295C31", "414242", true, 7, "0:3,1:1", "" )]
	[InlineData( "283F7C5C7530303431285C7530303432297C5C7530303433285C753030343429295C31", "434444", true, 7, "0:3,1:1", "" )]
	[InlineData( "283F7C5C7530303431285C7530303432297C5C7530303433285C753030343429295C31", "414242", true, 11, "0:3,1:1", "" )]
	[InlineData( "283F7C5C7530303431285C7530303432297C5C7530303433285C753030343429295C31", "434444", true, 11, "0:3,1:1", "" )]
	[InlineData( "283F7C5C7530303431285C7530303432297C5C7530303433285C753030343429295C31", "414242", true, 19, "0:3,1:1", "" )]
	[InlineData( "283F7C5C7530303431285C7530303432297C5C7530303433285C753030343429295C31", "434444", true, 19, "0:3,1:1", "" )]
	[InlineData( "283F5E29285C753030343129", "41", true, 0, "0:1,0:1", "" )]
	[InlineData( "283F5E29285C753030343129", "61", true, 0, "", "" )]
	[InlineData( "283F5E29285C753030343129", "41", true, 2, "0:1,0:1", "" )]
	[InlineData( "283F5E29285C753030343129", "61", true, 2, "", "" )]
	[InlineData( "283F5E29285C753030343129", "41", true, 3, "0:1,0:1", "" )]
	[InlineData( "283F5E29285C753030343129", "61", true, 3, "", "" )]
	[InlineData( "283F5E29285C753030343129", "41", true, 7, "0:1,0:1", "" )]
	[InlineData( "283F5E29285C753030343129", "61", true, 7, "", "" )]
	[InlineData( "283F5E29285C753030343129", "41", true, 11, "0:1,0:1", "" )]
	[InlineData( "283F5E29285C753030343129", "61", true, 11, "", "" )]
	[InlineData( "283F5E29285C753030343129", "41", true, 19, "0:1,0:1", "" )]
	[InlineData( "283F5E29285C753030343129", "61", true, 19, "", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "6142", true, 0, "0:2", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "4142", true, 0, "0:2", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "4162", true, 0, "", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "6142", true, 2, "0:2", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "4142", true, 2, "0:2", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "4162", true, 2, "", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "6142", true, 3, "0:2", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "4142", true, 3, "0:2", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "4162", true, 3, "", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "6142", true, 7, "0:2", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "4142", true, 7, "0:2", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "4162", true, 7, "", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "6142", true, 11, "0:2", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "4142", true, 11, "0:2", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "4162", true, 11, "", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "6142", true, 19, "0:2", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "4142", true, 19, "0:2", "" )]
	[InlineData( "283F693A5C753030363129283F2D693A5C753030343229", "4162", true, 19, "", "" )]
	[InlineData( "283F783A5C75303020343129", "7530303431", true, 0, "0:5", "" )]
	[InlineData( "283F783A5C75303020343129", "41", true, 0, "", "" )]
	[InlineData( "283F783A5C75303020343129", "7530303431", true, 2, "0:5", "" )]
	[InlineData( "283F783A5C75303020343129", "41", true, 2, "", "" )]
	[InlineData( "283F783A5C75303020343129", "7530303431", true, 3, "0:5", "" )]
	[InlineData( "283F783A5C75303020343129", "41", true, 3, "", "" )]
	[InlineData( "283F783A5C75303020343129", "7530303431", true, 7, "0:5", "" )]
	[InlineData( "283F783A5C75303020343129", "41", true, 7, "", "" )]
	[InlineData( "283F783A5C75303020343129", "7530303431", true, 11, "0:5", "" )]
	[InlineData( "283F783A5C75303020343129", "41", true, 11, "", "" )]
	[InlineData( "283F783A5C75303020343129", "7530303431", true, 19, "0:5", "" )]
	[InlineData( "283F783A5C75303020343129", "41", true, 19, "", "" )]
	[InlineData( "282A4D41524B3A5C7530303431295C7530303431", "41", true, 0, "0:1", "5C7530303431" )]
	[InlineData( "282A4D41524B3A5C7530303431295C7530303431", "41", true, 2, "0:1", "5C7530303431" )]
	[InlineData( "282A4D41524B3A5C7530303431295C7530303431", "41", true, 3, "0:1", "5C7530303431" )]
	[InlineData( "282A4D41524B3A5C7530303431295C7530303431", "41", true, 7, "0:1", "5C7530303431" )]
	[InlineData( "282A4D41524B3A5C7530303431295C7530303431", "41", true, 11, "0:1", "5C7530303431" )]
	[InlineData( "282A4D41524B3A5C7530303431295C7530303431", "41", true, 19, "0:1", "5C7530303431" )]
	[InlineData( "5C515C75303034315C7834315C555C45", "5c75303034315c7834315c55", false, 0, "0:12", "" )]
	[InlineData( "5C515C75303034315C7834315C555C45", "5c75303034315c7834315c55", false, 2, "0:12", "" )]
	[InlineData( "5C515C75303034315C7834315C555C45", "5c75303034315c7834315c55", false, 3, "0:12", "" )]
	[InlineData( "5C515C75303034315C7834315C555C45", "5c75303034315c7834315c55", false, 7, "0:12", "" )]
	[InlineData( "5C515C75303034315C7834315C555C45", "5c75303034315c7834315c55", false, 11, "0:12", "" )]
	[InlineData( "5C515C75303034315C7834315C555C45", "5c75303034315c7834315c55", false, 19, "0:12", "" )]
	[InlineData( "5C515C75303034315C7834315C555C45", "5c75303034315c7834315c55", true, 0, "0:12", "" )]
	[InlineData( "5C515C75303034315C7834315C555C45", "5c75303034315c7834315c55", true, 2, "0:12", "" )]
	[InlineData( "5C515C75303034315C7834315C555C45", "5c75303034315c7834315c55", true, 3, "0:12", "" )]
	[InlineData( "5C515C75303034315C7834315C555C45", "5c75303034315c7834315c55", true, 7, "0:12", "" )]
	[InlineData( "5C515C75303034315C7834315C555C45", "5c75303034315c7834315c55", true, 11, "0:12", "" )]
	[InlineData( "5C515C75303034315C7834315C555C45", "5c75303034315c7834315c55", true, 19, "0:12", "" )]
	[InlineData( "283F782923205C75303034315C7834315C550A41", "41", false, 0, "0:1", "" )]
	[InlineData( "283F782923205C75303034315C7834315C550A41", "41", false, 2, "0:1", "" )]
	[InlineData( "283F782923205C75303034315C7834315C550A41", "41", false, 3, "0:1", "" )]
	[InlineData( "283F782923205C75303034315C7834315C550A41", "41", false, 7, "0:1", "" )]
	[InlineData( "283F782923205C75303034315C7834315C550A41", "41", false, 11, "0:1", "" )]
	[InlineData( "283F782923205C75303034315C7834315C550A41", "41", false, 19, "0:1", "" )]
	[InlineData( "283F782923205C75303034315C7834315C550A41", "41", true, 0, "0:1", "" )]
	[InlineData( "283F782923205C75303034315C7834315C550A41", "41", true, 2, "0:1", "" )]
	[InlineData( "283F782923205C75303034315C7834315C550A41", "41", true, 3, "0:1", "" )]
	[InlineData( "283F782923205C75303034315C7834315C550A41", "41", true, 7, "0:1", "" )]
	[InlineData( "283F782923205C75303034315C7834315C550A41", "41", true, 11, "0:1", "" )]
	[InlineData( "283F782923205C75303034315C7834315C550A41", "41", true, 19, "0:1", "" )]
	[InlineData( "5C7530313030", "", true, 3, "", "" )]
	[InlineData( "5C7530313030", "", true, 7, "", "" )]
	[InlineData( "5C7530313030", "", true, 11, "", "" )]
	[InlineData( "5C7530313030", "", true, 19, "", "" )]
	[InlineData( "5B5C75303130305D", "", true, 3, "", "" )]
	[InlineData( "5B5C75303130305D", "", true, 7, "", "" )]
	[InlineData( "5B5C75303130305D", "", true, 11, "", "" )]
	[InlineData( "5B5C75303130305D", "", true, 19, "", "" )]
	[InlineData( "283F5B5C75303130305D29", "", true, 3, "", "" )]
	[InlineData( "283F5B5C75303130305D29", "", true, 7, "", "" )]
	[InlineData( "283F5B5C75303130305D29", "", true, 11, "", "" )]
	[InlineData( "283F5B5C75303130305D29", "", true, 19, "", "" )]
	public void AlternateTokensRetainPinnedNativeCapturesAndLabels( string hex, string subjectHex, bool enabled, int mode, string spans, string mark ) {
		var source = Convert.FromHexString( hex );
		var subject = Convert.FromHexString( subjectHex );
		var reference = Reference( source, enabled, mode );
		AssertLiteral( reference, subject, spans, mark );
		Compare( reference, subject, 0, Compile( source, Options( enabled, mode ) ).Match( subject ), hex );
	}

	[Theory]
	[InlineData( "5C75443833445C7544453030", true, 0, 177, 6, 0 )]
	[InlineData( "5C75443833445C7544453030", true, 2, 177, 6, 0 )]
	[InlineData( "5C75443833445C7544453030", true, 3, 173, 6, 0 )]
	[InlineData( "5C75443833445C7544453030", true, 7, 173, 6, 0 )]
	[InlineData( "5C75443833445C7544453030", true, 11, 173, 6, 0 )]
	[InlineData( "5C75443833445C7544453030", true, 19, 173, 6, 0 )]
	[InlineData( "5C7530334232", true, 0, 177, 6, 0 )]
	[InlineData( "5C7530334232", true, 2, 177, 6, 0 )]
	[InlineData( "5C7532313241", true, 0, 177, 6, 0 )]
	[InlineData( "5C7532313241", true, 2, 177, 6, 0 )]
	[InlineData( "5C7546464646", true, 0, 177, 6, 0 )]
	[InlineData( "5C7546464646", true, 2, 177, 6, 0 )]
	[InlineData( "5C78", false, 0, 178, 2, 0 )]
	[InlineData( "5C78", false, 2, 178, 2, 0 )]
	[InlineData( "5C78", false, 3, 178, 2, 0 )]
	[InlineData( "5C78", false, 7, 178, 2, 0 )]
	[InlineData( "5C78", false, 11, 178, 2, 0 )]
	[InlineData( "5C78", false, 19, 178, 2, 0 )]
	[InlineData( "5C787B477D", false, 0, 167, 4, 0 )]
	[InlineData( "5C787B477D", false, 2, 167, 4, 0 )]
	[InlineData( "5C787B477D", false, 3, 167, 4, 0 )]
	[InlineData( "5C787B477D", false, 7, 167, 4, 0 )]
	[InlineData( "5C787B477D", false, 11, 167, 4, 0 )]
	[InlineData( "5C787B477D", false, 19, 167, 4, 0 )]
	[InlineData( "5C785C6341", false, 0, 178, 2, 0 )]
	[InlineData( "5C785C6341", false, 2, 178, 2, 0 )]
	[InlineData( "5C785C6341", false, 3, 178, 2, 0 )]
	[InlineData( "5C785C6341", false, 7, 178, 2, 0 )]
	[InlineData( "5C785C6341", false, 11, 178, 2, 0 )]
	[InlineData( "5C785C6341", false, 19, 178, 2, 0 )]
	[InlineData( "5C7530313030", true, 0, 177, 6, 0 )]
	[InlineData( "5C7530313030", true, 2, 177, 6, 0 )]
	[InlineData( "5C7544383030", true, 0, 177, 6, 0 )]
	[InlineData( "5C7544383030", true, 2, 177, 6, 0 )]
	[InlineData( "5C7544383030", true, 3, 173, 6, 0 )]
	[InlineData( "5C7544383030", true, 7, 173, 6, 0 )]
	[InlineData( "5C7544383030", true, 11, 173, 6, 0 )]
	[InlineData( "5C7544383030", true, 19, 173, 6, 0 )]
	[InlineData( "5C7544464646", true, 0, 177, 6, 0 )]
	[InlineData( "5C7544464646", true, 2, 177, 6, 0 )]
	[InlineData( "5C7544464646", true, 3, 173, 6, 0 )]
	[InlineData( "5C7544464646", true, 7, 173, 6, 0 )]
	[InlineData( "5C7544464646", true, 11, 173, 6, 0 )]
	[InlineData( "5C7544464646", true, 19, 173, 6, 0 )]
	[InlineData( "5B5C75303130305D", true, 0, 177, 7, 1 )]
	[InlineData( "5B5C75303130305D", true, 2, 177, 7, 1 )]
	[InlineData( "5B5C75443830305D", true, 0, 177, 7, 1 )]
	[InlineData( "5B5C75443830305D", true, 2, 177, 7, 1 )]
	[InlineData( "5B5C75443830305D", true, 3, 173, 7, 1 )]
	[InlineData( "5B5C75443830305D", true, 7, 173, 7, 1 )]
	[InlineData( "5B5C75443830305D", true, 11, 173, 7, 1 )]
	[InlineData( "5B5C75443830305D", true, 19, 173, 7, 1 )]
	[InlineData( "283F5B5C75303130305D29", true, 0, 177, 9, 0 )]
	[InlineData( "283F5B5C75303130305D29", true, 2, 177, 9, 0 )]
	[InlineData( "283F3C3D5C75443830302941", true, 0, 177, 10, 4 )]
	[InlineData( "283F3C3D5C75443830302941", true, 2, 177, 10, 4 )]
	[InlineData( "283F3C3D5C75443830302941", true, 3, 173, 10, 4 )]
	[InlineData( "283F3C3D5C75443830302941", true, 7, 173, 10, 4 )]
	[InlineData( "283F3C3D5C75443830302941", true, 11, 173, 10, 4 )]
	[InlineData( "283F3C3D5C75443830302941", true, 19, 173, 10, 4 )]
	public void CompleteAlternateValuesRetainNativeBoundsAndDiagnosticOwnership( string hex, bool enabled, int mode, int nativeCode, int nativeOffset, int offset ) {
		var source = Convert.FromHexString( hex );
		var native = Assert.Throws<PcrePatternException>( () => Reference( source, enabled, mode ) );
		Assert.Equal( nativeCode, (int)native.ErrorCode );
		Assert.EndsWith( $" at offset {nativeOffset}.", native.Message );
		var result = PcreByteCompiler.Compile( source, Options( enabled, mode ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
		Assert.Null( result.Pattern );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 3 )]
	[InlineData( 7 )]
	[InlineData( 11 )]
	public void All256ValuesUseCanonicalByteOrScalarLiterals( int mode ) {
		var patterns = Enumerable.Range( 0, 256 ).Select( value => Encoding.ASCII.GetBytes( $"\\A(\\u{value:X4})\\z" ) ).ToArray();
		var references = patterns.Select( source => Reference( source, true, mode ) ).ToArray();
		for ( var value = 0; value < patterns.Length; value++ ) {
			var subject = ( mode & 5 ) != 0 ? Encoding.UTF8.GetBytes( ( (char)value ).ToString() ) : new[] { (byte)value };
			var length = subject.Length;
			AssertLiteral( references[value], subject, $"0:{length},0:{length}", "" );
			Compare( references[value], subject, 0, Compile( patterns[value], Options( true, mode ) ).Match( subject ), $"alternate scalar {value}" );
		}
	}

	[Theory]
	[InlineData( "(*LF)", 0 )]
	[InlineData( "(*LF)", 7 )]
	[InlineData( "(*ANYCRLF)", 0 )]
	[InlineData( "(*ANYCRLF)", 7 )]
	[InlineData( "(*CR)", 0 )]
	[InlineData( "(*CR)", 7 )]
	[InlineData( "(*NUL)", 0 )]
	[InlineData( "(*NUL)", 7 )]
	[InlineData( "(*CRLF)", 0 )]
	[InlineData( "(*CRLF)", 7 )]
	[InlineData( "(*ANY)", 0 )]
	[InlineData( "(*ANY)", 7 )]
	public void EveryOffsetRetainsLiteralWidthNewlineMetadataAndFragmentBounds( string newline, int mode ) {
		var sources = OffsetBodies.Select( body => Encoding.ASCII.GetBytes( String.Concat( newline, body ) ) ).ToArray();
		var references = sources.Select( source => Reference( source, true, mode ) ).ToArray();
		for ( var index = 0; index < sources.Length; index++ ) {
			var compiled = Compile( sources[index], Options( true, mode ) );
			foreach ( var hex in new[] { "", "41", "4142", "0D0A41", "0D0A4142", "0B0D0A41", "0A41", "E280A841", "C2850A41", "FF41", "E280FF41", "E280A841FF42", "410D0A", "00FF41" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( references[index], subject, offset, compiled.Match( subject, offset ), $"{newline}/{index}/{hex}/{offset}/{mode}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( "\\u0041", 0, 0 )]
	[InlineData( "\\u0041", 3, 0 )]
	[InlineData( "\\U", 0, 0 )]
	[InlineData( "\\U", 3, 0 )]
	[InlineData( "[\\u0041]", 0, 1 )]
	[InlineData( "[\\u0041]", 3, 0 )]
	[InlineData( "(?<=\\u0041)A", 0, 0 )]
	[InlineData( "(?<=\\u0041)A", 3, 0 )]
	[InlineData( "(?[\\u0041])", 0, 0 )]
	[InlineData( "(?[\\u0041])", 3, 0 )]
	public void DefaultUnicodeEscapeDiagnosticsRemainUnchanged( string pattern, int mode, int offset ) {
		var source = Encoding.ASCII.GetBytes( pattern );
		var native = Assert.Throws<PcrePatternException>( () => Reference( source, false, mode ) );
		Assert.Equal( 137, (int)native.ErrorCode );
		var result = PcreByteCompiler.Compile( source, Options( false, mode ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
		Assert.Null( result.Pattern );
	}

	[Fact]
	public void AlternateLiteralsKeepCanonicalWorkAndCancellation() {
		var source = Encoding.ASCII.GetBytes( @"(\u0041|\u0041\u0041)+B" );
		var subject = "AAA"u8.ToArray();
		var reference = Reference( source, true, 0 );
		AssertLiteral( reference, subject, "", "" );
		var steps = new List<int>();
		var options = Options( true, 0 ) with { WorkObserver = steps.Add };
		Compare( reference, subject, 0, Compile( source, options ).Match( subject ), "alternate work" );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		Assert.True( steps.Count > 2 );
		var limited = Compile( source, options with { MatchWorkLimit = steps.Count - 1 } ).Match( subject );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		Assert.Null( limited.Match );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( source, options with { WorkObserver = count => { if ( count == 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, 0, cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( source, options, cancellation.Token ) );
	}

	[Fact]
	public void CompiledAlternateLiteralsOwnSourceAndOptionState() {
		var source = Encoding.ASCII.GetBytes( @"(?<x>\u0041)\k<x>" );
		var subject = "AA"u8.ToArray();
		var reference = Reference( source, true, 3 );
		AssertLiteral( reference, subject, "0:2,0:1", "" );
		var options = Options( true, 3 );
		var compiled = Compile( source, options );
		Array.Fill( source, (byte)'x' );
		typeof( PcreByteOptions ).GetProperty( "AltBsux" )?.SetValue( options, false );
		Compare( reference, subject, 0, compiled.Match( subject ), "owned alternate source" );
	}

	[Fact]
	public void CanonicalLoweringRetainsStructuralQuotas() {
		var patterns = new[] {
			String.Concat( String.Concat( Enumerable.Repeat( "()", 256 ) ), @"\u0041" ),
			String.Concat( String.Concat( Enumerable.Repeat( "()", 257 ) ), @"\u0041" ),
			String.Concat( new string( '(', 257 ), @"\u0041", new string( ')', 257 ) ), @"\u0041{4096}"
		};
		var references = patterns.Select( pattern => Reference( Encoding.ASCII.GetBytes( pattern ), true, 0 ) ).ToArray();
		Compare( references[0], "A"u8.ToArray(), 0, Compile( Encoding.ASCII.GetBytes( patterns[0] ), Options( true, 0 ) ).Match( "A"u8.ToArray() ), "alternate full capture capacity" );
		var codes = new[] { PcreByteDiagnosticCode.PatternCaptureLimitExceeded, PcreByteDiagnosticCode.PatternDepthLimitExceeded, PcreByteDiagnosticCode.PatternRepeatLimitExceeded };
		for ( var index = 1; index < patterns.Length; index++ ) {
			var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( patterns[index] ), Options( true, 0 ) );
			Assert.Equal( codes[index - 1], result.Diagnostic?.Code );
			Assert.Null( result.Pattern );
		}
	}

	[Fact]
	public void ExistingCallerProhibitionsKeepPrecedenceWithoutRelexingLiterals() {
		var sources = new[] { Encoding.ASCII.GetBytes( @"\u0041" ), Encoding.ASCII.GetBytes( @"(*UCP)\u0041" ), Encoding.ASCII.GetBytes( @"\u0041\C" ) };
		var modes = new[] { 1, 0, 0 };
		var flags = new uint[] { 0x1000, 0x800, 0x100000 };
		var nativeCodes = new[] { 174, 175, 183 };
		var nativeOffsets = new[] { 0, 6, 8 };
		var offsets = new[] { 0, 6, 6 };
		var native = sources.Select( ( source, index ) => Assert.Throws<PcrePatternException>( () => Reference( source, true, modes[index], flags[index] ) ) ).ToArray();
		for ( var index = 0; index < sources.Length; index++ ) {
			Assert.Equal( nativeCodes[index], (int)native[index].ErrorCode );
			Assert.EndsWith( $" at offset {nativeOffsets[index]}.", native[index].Message );
		}
		var options = new[] { Options( true, 1 ) with { NeverUtf = true }, Options( true, 0 ) with { NeverUcp = true }, Options( true, 0 ) with { NeverBackslashC = true } };
		for ( var index = 0; index < sources.Length; index++ ) {
			var result = PcreByteCompiler.Compile( sources[index], options[index] );
			Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
			Assert.Equal( offsets[index], result.Diagnostic?.ByteOffset );
			Assert.Contains( "disabled by the caller", result.Diagnostic!.Message );
			Assert.Null( result.Pattern );
		}
		var literalSource = Encoding.ASCII.GetBytes( @"\u005CC" );
		var reference = Reference( literalSource, true, 0, 0x100000 );
		var subject = "\\C"u8.ToArray();
		AssertLiteral( reference, subject, "0:2", "" );
		Compare( reference, subject, 0, Compile( literalSource, Options( true, 0 ) with { NeverBackslashC = true } ).Match( subject ), "produced slash is not syntax" );
	}

	[Fact]
	public void WholePatternUtfValidationStillPrecedesAlternateParsing() {
		var source = Encoding.ASCII.GetBytes( @"\u0041" ).Concat( new byte[] { 0xFF } ).ToArray();
		foreach ( var enabled in new[] { false, true } ) {
			var native = Assert.Throws<PcrePatternException>( () => Reference( source, enabled, 3 ) );
			Assert.Equal( -23, (int)native.ErrorCode );
			Assert.EndsWith( " at offset 6.", native.Message );
			var result = PcreByteCompiler.Compile( source, Options( enabled, 3 ) );
			Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, result.Diagnostic?.Code );
			Assert.Equal( 6, result.Diagnostic?.ByteOffset );
			Assert.Null( result.Pattern );
		}
	}

	[Fact]
	public void AlternateGrammarDoesNotCreateAPatternDirective() {
		var source = "(*ALT_BSUX)A"u8.ToArray();
		foreach ( var enabled in new[] { false, true } ) {
			Assert.Throws<PcrePatternException>( () => Reference( source, enabled, 0 ) );
			var result = PcreByteCompiler.Compile( source, Options( enabled, 0 ) );
			Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
			Assert.Equal( 0, result.Diagnostic?.ByteOffset );
			Assert.Null( result.Pattern );
		}
	}

	private static readonly string[] OffsetBodies = [@"\u0041", @"(\u0041)\u0042", @"(?<=\u0041)\u0042", @"\G(\u0041)", @"[\u0009-\u000c]A", @"[\u000a]A", @"[\u000d\u000a]A"];
	private static PcreByteOptions Options( bool enabled, int mode ) {
		var options = new PcreByteOptions { Utf8 = ( mode & 1 ) != 0, Ucp = ( mode & 2 ) != 0, MatchInvalidUtf = ( mode & 4 ) != 0,
			Caseless = ( mode & 8 ) != 0, Extended = ( mode & 16 ) != 0 };
		// Keep tests-only RED executable before the new caller property exists.
		if ( enabled ) { typeof( PcreByteOptions ).GetProperty( "AltBsux" )?.SetValue( options, true ); }
		return options;
	}
	private static PcreRegex8Bit Reference( byte[] source, bool enabled, int mode, uint flags = 0 ) => new(
		source, Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | (PcreOptions)( flags | ( enabled ? 2u : 0u )
				| ( ( mode & 1 ) != 0 ? 0x80000u : 0u ) | ( ( mode & 2 ) != 0 ? 0x20000u : 0u )
				| ( ( mode & 4 ) != 0 ? 0x4000000u : 0u ) | ( ( mode & 8 ) != 0 ? 8u : 0u ) | ( ( mode & 16 ) != 0 ? 0x80u : 0u ) ),
			ParensLimit = 512
		} );
	private static PcreCompiledBytePattern Compile( byte[] source, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( source, options );
		Assert.True( result.IsSuccess, $"{Encoding.UTF8.GetString( source )}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
	private static void AssertLiteral( PcreRegex8Bit reference, byte[] subject, string spans, string mark ) {
		var match = reference.Match( subject );
		Assert.Equal( spans.Length > 0, match.Success );
		if ( !match.Success ) { return; }
		Assert.Equal( mark, Convert.ToHexString( match.Mark.ToArray() ) );
		var groups = spans.Split( ',' );
		Assert.Equal( groups.Length, match.Groups.Count );
		for ( var group = 0; group < groups.Length; group++ ) {
			var capture = match.Groups[group];
			Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
		}
	}
	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Mark.ToArray(), actual.Match.MarkName.ToArray() );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}
}
