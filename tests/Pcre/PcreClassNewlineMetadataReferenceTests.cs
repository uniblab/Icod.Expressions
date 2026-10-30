using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>UTF class source syntax, rather than membership, controls CRLF search continuation.</summary>
public sealed class PcreClassNewlineMetadataReferenceTests {
	[Theory]
	[InlineData( @"[\x{9}-\x{e}]A", 0, -1, 0, "" )]
	[InlineData( @"[\x{0}-\x{10ffff}]A", 0, -1, 0, "" )]
	[InlineData( @"(?i)[\x{9}-\x{e}]A", 0, -1, 0, "" )]
	[InlineData( @"(?xx)[ \x{9} - \x{e} ]A", 0, -1, 0, "" )]
	[InlineData( @"[\x{9}-\x{e}]A", 1, 1, 2, "1:2" )]
	[InlineData( @"[\n-\r]A", 0, 1, 2, "1:2" )]
	[InlineData( @"[\x{0}-\n]A", 0, 1, 2, "1:2" )]
	[InlineData( @"[\n]A", 0, 1, 2, "1:2" )]
	[InlineData( @"[^\r]A", 0, -1, 0, "" )]
	[InlineData( @"[^\r-\r]A", 0, -1, 0, "" )]
	[InlineData( @"[^\r\r]A", 0, 1, 2, "1:2" )]
	[InlineData( @"[^\rA]A", 0, 1, 2, "1:2" )]
	[InlineData( @"[^\r-\r\r-\r]A", 0, 1, 2, "1:2" )]
	[InlineData( @"(?xx)[^ \r \r ]A", 0, 1, 2, "1:2" )]
	[InlineData( "[^\\Q\r\r\\E]A", 0, 1, 2, "1:2" )]
	[InlineData( @"[^\n-\r]A", 0, -1, 0, "" )]
	[InlineData( @"[^\p{L}\r]A", 0, 1, 2, "1:2" )]
	[InlineData( @"[^\p{Xan}\r]A", 0, 1, 2, "1:2" )]
	[InlineData( @"[^\P{Any}\r]A", 0, 1, 2, "1:2" )]
	[InlineData( @"[\p{Any}\r]A", 0, 1, 2, "1:2" )]
	[InlineData( @"[\r\p{Any}]A", 0, 1, 2, "1:2" )]
	[InlineData( @"[\P{Any}\r]A", 0, -1, 0, "" )]
	[InlineData( @"[^\p{Any}\r]A", 0, -1, 0, "" )]
	[InlineData( @"[\p{Any}]A", 0, -1, 0, "" )]
	[InlineData( @"[^\P{Any}]A", 0, -1, 0, "" )]
	[InlineData( @"[\x{9}-\x{e}]{0}\p{Any}A", 0, -1, 0, "" )]
	[InlineData( @"[^\r\r]{0}\p{Any}A", 0, 1, 2, "1:2" )]
	[InlineData( @"(?(DEFINE)(?<x>[^\r\r]))\p{Any}A", 0, 1, 2, "1:2,-" )]
	public void LiteralTermsAndRangeEndpointsPreserveNativeUtfSearch( string pattern,
		int offset, int index, int length, string captures ) {
		var subject = "\r\nA"u8.ToArray();
		var reference = Reference( pattern, true );
		var expected = reference.Match( subject, offset );
		Assert.Equal( index, expected.Success ? expected.Index : -1 );
		if ( expected.Success ) {
			Assert.Equal( length, expected.Length );
			Assert.Equal( captures, string.Join( ",", expected.Groups.Select(
				group => group.Success ? $"{group.Index}:{group.Length}" : "-" ) ) );
		}
		Compare( reference, subject, offset, Compile( pattern, true ).Match( subject, offset ), pattern );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void SourceMetadataKeepsCallerOffsetsAndMalformedFragments( bool anyCrLf ) {
		foreach ( var pattern in new[] { @"[\x{9}-\x{e}]A", @"[^\r\r]A",
			@"[^\p{Xan}\r]A", @"\G[^\P{Any}\r]A", @"(?m)[\p{Any}\r]A$" } ) {
			var reference = Reference( pattern, anyCrLf, invalidUtf: true );
			var compiled = Compile( pattern, anyCrLf, invalidUtf: true );
			foreach ( var hex in new[] { "0D0A41", "FF0D0A41", "0D0AFF41" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
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

	private static PcreRegex8Bit Reference( string pattern, bool anyCrLf, bool invalidUtf = false ) => new(
		Encoding.UTF8.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), Encoding.UTF8,
		new PcreRegexSettings { Options = PcreOptions.Compiled | PcreOptions.Utf
			| ( invalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, bool anyCrLf, bool invalidUtf = false ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ),
			new PcreByteOptions { Utf8 = true, AnyCrLfNewline = anyCrLf, MatchInvalidUtf = invalidUtf } );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
