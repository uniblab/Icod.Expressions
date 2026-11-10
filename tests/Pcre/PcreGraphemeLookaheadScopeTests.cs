using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>The pinned native matcher bounds nested forward assertions in variable lookbehind.</summary>
public sealed class PcreGraphemeLookaheadScopeTests {
	[Theory]
	[InlineData( @"(?<=a{1,2}(?=(.)))", false )]
	[InlineData( @"(?<=a{1,2}(?!(b)))", true )]
	[InlineData( @"(?<=a{1,2}(?=(\X)))", false )]
	public void VariableBehindRetainsItsNativeForwardAssertionLimit( string pattern, bool success ) {
		var subject = "ab́"u8.ToArray();
		var native = new PcreRegex8Bit( Encoding.UTF8.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | PcreOptions.Utf
		} ).Match( subject );
		Assert.Equal( success, native.Success );
		var compiled = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.True( compiled.IsSuccess, compiled.Diagnostic?.Message );
		var actual = compiled.Pattern!.Match( subject );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		Assert.Equal( success, actual.Match is not null );
		if ( !success ) { return; }
		Assert.Equal( native.Index, actual.Match!.ByteIndex );
		Assert.Equal( native.Length, actual.Match.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var i = 0; i < native.Groups.Count; i++ ) {
			var capture = native.Groups[i];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[i] );
		}
	}
}
