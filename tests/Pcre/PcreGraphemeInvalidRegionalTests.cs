using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pinned native JIT backward-context behavior in invalid-UTF mode.</summary>
public sealed class PcreGraphemeInvalidRegionalTests {
	[Theory]
	[InlineData( "F09F87A6F09F87A7F09F87A8", 0, 0, 12 )]
	[InlineData( "F0A6879FF09F87A6F09F87A7F09F87A8", 4, 4, 4 )]
	[InlineData( "F0A6879FF0A6879FF09F87A6F09F87A7F09F87A8", 8, 8, 12 )]
	[InlineData( "F0A6879FF09F87A6F09F87A7F09F87A8", 8, 8, 8 )]
	[InlineData( "F0A687A0F09F87A6F09F87A7F09F87A8", 4, 4, 12 )]
	[InlineData( "FFF0A6879FF09F87A6F09F87A7F09F87A8", 5, 5, 4 )]
	[InlineData( "F0A6879FFFF09F87A6F09F87A7F09F87A8", 5, 5, 12 )]
	[InlineData( "F09F87A6F09F87A7F09F87A8", 1, 4, 8 )]
	public void TolerantJitRegionalContextKeepsPinnedSpans( string hex, int offset, int index, int length ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var ucp in new[] { false, true } ) {
			var options = PcreOptions.Compiled | PcreOptions.Utf | PcreOptions.MatchInvalidUtf | ( ucp ? PcreOptions.Ucp : PcreOptions.None );
			var native = new PcreRegex8Bit( "(\\X)"u8.ToArray(), Encoding.UTF8, new PcreRegexSettings { Options = options } ).Match( subject, offset );
			Assert.True( native.Success );
			Assert.Equal( index, native.Index );
			Assert.Equal( length, native.Length );
			var compiled = PcreByteCompiler.Compile( "(\\X)"u8.ToArray(), new() { Utf8 = true, Ucp = ucp, MatchInvalidUtf = true } );
			Assert.True( compiled.IsSuccess, compiled.Diagnostic?.Message );
			var actual = compiled.Pattern!.Match( subject, offset );
			Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
			Assert.NotNull( actual.Match );
			Assert.Equal( index, actual.Match!.ByteIndex );
			Assert.Equal( length, actual.Match.ByteLength );
			Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
			for ( var i = 0; i < native.Groups.Count; i++ ) {
				Assert.Equal( new PcreByteCapture( native.Groups[i].Index, native.Groups[i].Length ), actual.Match.Captures[i] );
			}
		}
	}
}
