using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreUtfOptionalSetCaseFoldReferenceTests {
	[Theory]
	[InlineData( @"(*ANYCRLF)(?i)[k]?([\x09-\x0c\x{100}]+)A", "0D0A41", "" )]
	[InlineData( @"(*ANYCRLF)(?i)[k]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", "" )]
	[InlineData( @"(*ANYCRLF)(?i)[k]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", "3:2,3:1" )]
	[InlineData( @"(*ANYCRLF)(?i)[s]?([\x09-\x0c\x{100}]+)A", "0D0A41", "" )]
	[InlineData( @"(*ANYCRLF)(?i)[s]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", "" )]
	[InlineData( @"(*ANYCRLF)(?i)[s]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", "3:2,3:1" )]
	[InlineData( @"(*ANYCRLF)(?i)[\x{3c3}]?([\x09-\x0c\x{100}]+)A", "0D0A41", "" )]
	[InlineData( @"(*ANYCRLF)(?i)[\x{3c3}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", "" )]
	[InlineData( @"(*ANYCRLF)(?i)[\x{3c3}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", "3:2,3:1" )]
	[InlineData( @"(*ANYCRLF)(?i)[\x{212a}]?([\x09-\x0c\x{100}]+)A", "0D0A41", "" )]
	[InlineData( @"(*ANYCRLF)(?i)[\x{212a}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", "" )]
	[InlineData( @"(*ANYCRLF)(?i)[\x{212a}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", "3:2,3:1" )]
	[InlineData( @"(*ANYCRLF)(?i)[k-l]?([\x09-\x0c\x{100}]+)A", "0D0A41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?i)[k-l]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", "3:2,3:1" )]
	[InlineData( @"(*ANYCRLF)(?i)[k-l]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", "2:3,2:2" )]
	[InlineData( @"(*ANYCRLF)(?i)[s-t]?([\x09-\x0c\x{100}]+)A", "0D0A41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?i)[s-t]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", "3:2,3:1" )]
	[InlineData( @"(*ANYCRLF)(?i)[s-t]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", "2:3,2:2" )]
	[InlineData( @"(*ANYCRLF)(?i)[\x{100}k]?([\x09-\x0c\x{100}]+)A", "0D0A41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?i)[\x{100}k]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", "3:2,3:1" )]
	[InlineData( @"(*ANYCRLF)(?i)[\x{100}k]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", "2:3,2:2" )]
	public void OptionalSetCaseFoldingRetainsNativeOpcodeAvailability( string pattern, string hex, string spans ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var tolerant in new[] { false, true } ) {
				var reference = Reference( pattern, ucp, tolerant );
				var expected = reference.Match( subject );
				Assert.Equal( spans.Length > 0, expected.Success );
				if ( expected.Success ) {
					var groups = spans.Split( ',' );
					Assert.Equal( groups.Length, expected.Groups.Count );
					for ( var group = 0; group < groups.Length; group++ ) {
						var capture = expected.Groups[group];
						Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
					}
				}
				Compare( reference, subject, 0, Compile( pattern, ucp, tolerant ), pattern );
			}
		}
	}

	[Theory]
	[InlineData( "(*LF)" )]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CR)" )]
	[InlineData( "(*NUL)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void OptionalSetCaseFoldingRetainsEveryCallerOffset( string newline ) {
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var body in Bodies() ) {
				var pattern = String.Concat( newline, body );
				var reference = Reference( pattern, ucp, true );
				var compiled = Compile( pattern, ucp, true );
				foreach ( var hex in Subjects() ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled, $"{pattern}/{hex}/{offset}/{ucp}" );
					}
				}
			}
		}
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreCompiledBytePattern compiled, string context ) {
		var expected = reference.Match( subject, offset );
		var actual = compiled.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.Equal( expected.Success ? expected.Index : -1, actual.Match?.ByteIndex ?? -1 );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool ucp, bool tolerant ) => new(
		Encoding.ASCII.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | PcreOptions.Utf | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ) } );

	private static PcreCompiledBytePattern Compile( string pattern, bool ucp, bool tolerant ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new() { Utf8 = true, Ucp = ucp, MatchInvalidUtf = tolerant } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}

	private static string[] Bodies() => new[] {
		@"(?i)[k]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[s]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[\x{3c3}]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[\x{212a}]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[k-l]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[s-t]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[\x{100}k]?([\x09-\x0c\x{100}]+)A"
	};
	private static string[] Subjects() => new[] { "", "0D0A41", "C4800D0A41", "0B0D0A0B41", "FF0B0D0A41", "0BFF0D0A41", "EDA0800B0D0A41", "6B0B41" };
}
