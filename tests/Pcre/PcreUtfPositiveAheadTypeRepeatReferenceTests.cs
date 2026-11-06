using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins native LF entry and captures through mandatory supported-type repeats after positive ahead.</summary>
public sealed class PcreUtfPositiveAheadTypeRepeatReferenceTests {
	[Theory]
	[InlineData( @"(*ANYCRLF)(?:(?=\d)\d+)?([\x09-\x0c\x{100}])A", "0D0A41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\d)\d+)?([\x09-\x0c\x{100}])A", "310B41", "0:3,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\d)\d+)?([\x09-\x0c\x{100}])A", "31320B41", "0:4,2:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\d)\d+)?([\x09-\x0c\x{100}])A", "420B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\d)\d+)?([\x09-\x0c\x{100}])A", "42430B41", "2:2,2:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\d)\d+)?([\x09-\x0c\x{100}])A", "090B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\d)\d+)?([\x09-\x0c\x{100}])A", "210B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\d)\d+)?([\x09-\x0c\x{100}])A", "0D0A31320B41", "2:4,4:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\d)\d+)?([\x09-\x0c\x{100}])A", "0B41", "0:2,0:1" )]
	[InlineData( @"(*ANYCRLF)((?=\w)\w+)?([\x09-\x0c\x{100}])A", "0D0A41", "1:2,-,1:1" )]
	[InlineData( @"(*ANYCRLF)((?=\w)\w+)?([\x09-\x0c\x{100}])A", "310B41", "0:3,0:1,1:1" )]
	[InlineData( @"(*ANYCRLF)((?=\w)\w+)?([\x09-\x0c\x{100}])A", "31320B41", "0:4,0:2,2:1" )]
	[InlineData( @"(*ANYCRLF)((?=\w)\w+)?([\x09-\x0c\x{100}])A", "420B41", "0:3,0:1,1:1" )]
	[InlineData( @"(*ANYCRLF)((?=\w)\w+)?([\x09-\x0c\x{100}])A", "42430B41", "0:4,0:2,2:1" )]
	[InlineData( @"(*ANYCRLF)((?=\w)\w+)?([\x09-\x0c\x{100}])A", "090B41", "1:2,-,1:1" )]
	[InlineData( @"(*ANYCRLF)((?=\w)\w+)?([\x09-\x0c\x{100}])A", "210B41", "1:2,-,1:1" )]
	[InlineData( @"(*ANYCRLF)((?=\w)\w+)?([\x09-\x0c\x{100}])A", "0D0A31320B41", "2:4,2:2,4:1" )]
	[InlineData( @"(*ANYCRLF)((?=\w)\w+)?([\x09-\x0c\x{100}])A", "0B41", "0:2,-,0:1" )]
	[InlineData( @"(*ANYCRLF)(?>(?=\h)\h++)?([\x09-\x0c\x{100}])A", "0D0A41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?>(?=\h)\h++)?([\x09-\x0c\x{100}])A", "310B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?>(?=\h)\h++)?([\x09-\x0c\x{100}])A", "31320B41", "2:2,2:1" )]
	[InlineData( @"(*ANYCRLF)(?>(?=\h)\h++)?([\x09-\x0c\x{100}])A", "420B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?>(?=\h)\h++)?([\x09-\x0c\x{100}])A", "42430B41", "2:2,2:1" )]
	[InlineData( @"(*ANYCRLF)(?>(?=\h)\h++)?([\x09-\x0c\x{100}])A", "090B41", "0:3,1:1" )]
	[InlineData( @"(*ANYCRLF)(?>(?=\h)\h++)?([\x09-\x0c\x{100}])A", "210B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?>(?=\h)\h++)?([\x09-\x0c\x{100}])A", "0D0A31320B41", "4:2,4:1" )]
	[InlineData( @"(*ANYCRLF)(?>(?=\h)\h++)?([\x09-\x0c\x{100}])A", "0B41", "0:2,0:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S{2})?([\x09-\x0c\x{100}])A", "0D0A41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S{2})?([\x09-\x0c\x{100}])A", "310B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S{2})?([\x09-\x0c\x{100}])A", "31320B41", "0:4,2:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S{2})?([\x09-\x0c\x{100}])A", "420B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S{2})?([\x09-\x0c\x{100}])A", "42430B41", "0:4,2:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S{2})?([\x09-\x0c\x{100}])A", "090B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S{2})?([\x09-\x0c\x{100}])A", "210B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S{2})?([\x09-\x0c\x{100}])A", "0D0A31320B41", "2:4,4:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S{2})?([\x09-\x0c\x{100}])A", "0B41", "0:2,0:1" )]
	[InlineData( @"(*ANYCRLF)((?=\d)(?!Z)\d{1,3}?){0,3}?([\x09-\x0c\x{100}])A", "0D0A41", "1:2,-,1:1" )]
	[InlineData( @"(*ANYCRLF)((?=\d)(?!Z)\d{1,3}?){0,3}?([\x09-\x0c\x{100}])A", "310B41", "0:3,0:1,1:1" )]
	[InlineData( @"(*ANYCRLF)((?=\d)(?!Z)\d{1,3}?){0,3}?([\x09-\x0c\x{100}])A", "31320B41", "0:4,1:1,2:1" )]
	[InlineData( @"(*ANYCRLF)((?=\d)(?!Z)\d{1,3}?){0,3}?([\x09-\x0c\x{100}])A", "420B41", "1:2,-,1:1" )]
	[InlineData( @"(*ANYCRLF)((?=\d)(?!Z)\d{1,3}?){0,3}?([\x09-\x0c\x{100}])A", "42430B41", "2:2,-,2:1" )]
	[InlineData( @"(*ANYCRLF)((?=\d)(?!Z)\d{1,3}?){0,3}?([\x09-\x0c\x{100}])A", "090B41", "1:2,-,1:1" )]
	[InlineData( @"(*ANYCRLF)((?=\d)(?!Z)\d{1,3}?){0,3}?([\x09-\x0c\x{100}])A", "210B41", "1:2,-,1:1" )]
	[InlineData( @"(*ANYCRLF)((?=\d)(?!Z)\d{1,3}?){0,3}?([\x09-\x0c\x{100}])A", "0D0A31320B41", "2:4,3:1,4:1" )]
	[InlineData( @"(*ANYCRLF)((?=\d)(?!Z)\d{1,3}?){0,3}?([\x09-\x0c\x{100}])A", "0B41", "0:2,-,0:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\w)\w{1,3}?)?([\x09-\x0c\x{100}])A", "0D0A41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\w)\w{1,3}?)?([\x09-\x0c\x{100}])A", "310B41", "0:3,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\w)\w{1,3}?)?([\x09-\x0c\x{100}])A", "31320B41", "0:4,2:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\w)\w{1,3}?)?([\x09-\x0c\x{100}])A", "420B41", "0:3,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\w)\w{1,3}?)?([\x09-\x0c\x{100}])A", "42430B41", "0:4,2:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\w)\w{1,3}?)?([\x09-\x0c\x{100}])A", "090B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\w)\w{1,3}?)?([\x09-\x0c\x{100}])A", "210B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\w)\w{1,3}?)?([\x09-\x0c\x{100}])A", "0D0A31320B41", "2:4,4:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\w)\w{1,3}?)?([\x09-\x0c\x{100}])A", "0B41", "0:2,0:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\h)\h+)?([\x09-\x0c\x{100}])A", "0D0A41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\h)\h+)?([\x09-\x0c\x{100}])A", "310B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\h)\h+)?([\x09-\x0c\x{100}])A", "31320B41", "2:2,2:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\h)\h+)?([\x09-\x0c\x{100}])A", "420B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\h)\h+)?([\x09-\x0c\x{100}])A", "42430B41", "2:2,2:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\h)\h+)?([\x09-\x0c\x{100}])A", "090B41", "0:3,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\h)\h+)?([\x09-\x0c\x{100}])A", "210B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\h)\h+)?([\x09-\x0c\x{100}])A", "0D0A31320B41", "4:2,4:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\h)\h+)?([\x09-\x0c\x{100}])A", "0B41", "0:2,0:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S+)?([\x09-\x0c\x{100}])A", "0D0A41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S+)?([\x09-\x0c\x{100}])A", "310B41", "0:3,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S+)?([\x09-\x0c\x{100}])A", "31320B41", "0:4,2:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S+)?([\x09-\x0c\x{100}])A", "420B41", "0:3,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S+)?([\x09-\x0c\x{100}])A", "42430B41", "0:4,2:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S+)?([\x09-\x0c\x{100}])A", "090B41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S+)?([\x09-\x0c\x{100}])A", "210B41", "0:3,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S+)?([\x09-\x0c\x{100}])A", "0D0A31320B41", "2:4,4:1" )]
	[InlineData( @"(*ANYCRLF)(?:(?=\S)\S+)?([\x09-\x0c\x{100}])A", "0B41", "0:2,0:1" )]
	public void PositiveAheadBeforeRepeatedTypePreservesCaptures( string pattern, string hex, string spans ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var tolerant in new[] { false, true } ) {
			var reference = new PcreRegex8Bit( Encoding.ASCII.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
				Options = PcreOptions.Compiled | PcreOptions.Utf
					| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ) } );
			var expected = reference.Match( subject );
			Assert.True( expected.Success );
			var groups = spans.Split( ',' );
			Assert.Equal( groups.Length, expected.Groups.Count );
			for ( var group = 0; group < groups.Length; group++ ) {
				var capture = expected.Groups[group];
				Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
			}
			var compiled = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new() {
				Utf8 = true, MatchInvalidUtf = tolerant } );
			Assert.True( compiled.IsSuccess, compiled.Diagnostic?.Message );
			var actual = Assert.IsType<PcreCompiledBytePattern>( compiled.Pattern ).Match( subject );
			Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
			Assert.Equal( expected.Index, actual.Match?.ByteIndex ?? -1 );
			Assert.Equal( expected.Length, actual.Match?.ByteLength );
			Assert.Equal( expected.Groups.Count, actual.Match!.Captures.Count );
			for ( var group = 0; group < expected.Groups.Count; group++ ) {
				var capture = expected.Groups[group];
				Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null,
					actual.Match.Captures[group] );
			}
		}
	}
}
