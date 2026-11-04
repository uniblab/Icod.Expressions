using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins root-star ownership and its endpoint before plain UTF starter classes.</summary>
public sealed class PcreUtfRootStarStartClassReferenceTests {
	[Theory]
	[InlineData( @"B*([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"B*([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"B*([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "" )]
	[InlineData( @"B*([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "" )]
	[InlineData( @"B*([\x09-\x0c\x{100}])A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"B*([\x09-\x0c\x{100}])A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"B*([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, false, "3:2,3:1" )]
	[InlineData( @"B*([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, true, "3:2,3:1" )]
	[InlineData( @"B*?([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"B*?([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"B*?([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "" )]
	[InlineData( @"B*?([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "" )]
	[InlineData( @"B*?([\x09-\x0c\x{100}])A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"B*?([\x09-\x0c\x{100}])A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"B*?([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, false, "3:2,3:1" )]
	[InlineData( @"B*?([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, true, "3:2,3:1" )]
	[InlineData( @"B*+([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"B*+([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"B*+([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "" )]
	[InlineData( @"B*+([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "" )]
	[InlineData( @"B*+([\x09-\x0c\x{100}])A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"B*+([\x09-\x0c\x{100}])A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"B*+([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, false, "3:2,3:1" )]
	[InlineData( @"B*+([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, true, "3:2,3:1" )]
	[InlineData( @"[B-D]*([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"[B-D]*([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"[B-D]*([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "" )]
	[InlineData( @"[B-D]*([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "" )]
	[InlineData( @"[B-D]*([\x09-\x0c\x{100}])A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"[B-D]*([\x09-\x0c\x{100}])A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"[B-D]*([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, false, "3:2,3:1" )]
	[InlineData( @"[B-D]*([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, true, "3:2,3:1" )]
	[InlineData( @"[B-D]*?([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"[B-D]*?([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"[B-D]*?([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "" )]
	[InlineData( @"[B-D]*?([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "" )]
	[InlineData( @"[B-D]*?([\x09-\x0c\x{100}])A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"[B-D]*?([\x09-\x0c\x{100}])A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"[B-D]*?([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, false, "3:2,3:1" )]
	[InlineData( @"[B-D]*?([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, true, "3:2,3:1" )]
	[InlineData( @"[B-D]*+([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"[B-D]*+([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"[B-D]*+([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "" )]
	[InlineData( @"[B-D]*+([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "" )]
	[InlineData( @"[B-D]*+([\x09-\x0c\x{100}])A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"[B-D]*+([\x09-\x0c\x{100}])A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"[B-D]*+([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, false, "3:2,3:1" )]
	[InlineData( @"[B-D]*+([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, true, "3:2,3:1" )]
	[InlineData( @"\d*([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"\d*([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "" )]
	[InlineData( @"\d*([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"\d*([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "" )]
	[InlineData( @"\d*([\x09-\x0c\x{100}])A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"\d*([\x09-\x0c\x{100}])A", "0B0D0A41", 0, true, "" )]
	[InlineData( @"\d*([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, false, "3:2,3:1" )]
	[InlineData( @"\d*([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, true, "3:2,3:1" )]
	[InlineData( @"\h*([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"\h*([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"\h*([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"\h*([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"\h*([\x09-\x0c\x{100}])A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"\h*([\x09-\x0c\x{100}])A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"\h*([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, false, "3:2,3:1" )]
	[InlineData( @"\h*([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, true, "3:2,3:1" )]
	[InlineData( @"\S*([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"\S*([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "" )]
	[InlineData( @"\S*([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "" )]
	[InlineData( @"\S*([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "" )]
	[InlineData( @"\S*([\x09-\x0c\x{100}])A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"\S*([\x09-\x0c\x{100}])A", "0B0D0A41", 0, true, "" )]
	[InlineData( @"\S*([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, false, "3:2,3:1" )]
	[InlineData( @"\S*([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, true, "3:2,3:1" )]
	[InlineData( @"[\x09-\x0c\x{100}]*([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"[\x09-\x0c\x{100}]*([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"[\x09-\x0c\x{100}]*([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"[\x09-\x0c\x{100}]*([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"[\x09-\x0c\x{100}]*([\x09-\x0c\x{100}])A", "0B0D0A41", 0, false, "" )]
	[InlineData( @"[\x09-\x0c\x{100}]*([\x09-\x0c\x{100}])A", "0B0D0A41", 0, true, "" )]
	[InlineData( @"[\x09-\x0c\x{100}]*([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, false, "3:2,3:1" )]
	[InlineData( @"[\x09-\x0c\x{100}]*([\x09-\x0c\x{100}])A", "0B0D0A0B41", 0, true, "3:2,3:1" )]
	public void RootStarStartClassesPreservePinnedNativeCaptureSpans( string body, string hex, int offset, bool ucp, string spans ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		foreach ( var tolerant in new[] { false, true } ) {
			var reference = Reference( pattern, ucp, tolerant );
			var expected = reference.Match( subject, offset );
			Assert.Equal( spans.Length > 0, expected.Success );
			if ( expected.Success ) {
				var groups = spans.Split( ',' );
				Assert.Equal( groups.Length, expected.Groups.Count );
				for ( var group = 0; group < groups.Length; group++ ) {
					var capture = expected.Groups[group];
					Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
				}
			}
			Compare( reference, subject, offset, Compile( pattern, new() { Utf8 = true, Ucp = ucp,
				MatchInvalidUtf = tolerant } ).Match( subject, offset ), pattern );
		}
	}


	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}: native={( expected.Success ? expected.Index : -1 )}, managed={actual.Match?.ByteIndex ?? -1}" );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool ucp = false, bool tolerant = false ) => new(
		Encoding.ASCII.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | PcreOptions.Utf | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
