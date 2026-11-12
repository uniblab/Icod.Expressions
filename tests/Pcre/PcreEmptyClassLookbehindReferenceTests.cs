using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Distinguishes native empty-class source from a set with no members.</summary>
public sealed class PcreEmptyClassLookbehindReferenceTests {
	[Theory]
	[InlineData( "(?<=[])A", 1, 0 )]
	[InlineData( "(?<=[])A", 3, 0 )]
	[InlineData( "(?<=[])A", 5, 0 )]
	[InlineData( "(?<=[])A", 7, 0 )]
	[InlineData( "(?<![])A", 1, 0 )]
	[InlineData( "(?<![])A", 3, 0 )]
	[InlineData( "(?<![])A", 5, 0 )]
	[InlineData( "(?<![])A", 7, 0 )]
	[InlineData( "(?<=[]{0})A", 1, 0 )]
	[InlineData( "(?<=[]{0})A", 3, 0 )]
	[InlineData( "(?<=[]{0})A", 5, 0 )]
	[InlineData( "(?<=[]{0})A", 7, 0 )]
	[InlineData( "(?<![]{0})A", 1, 0 )]
	[InlineData( "(?<![]{0})A", 3, 0 )]
	[InlineData( "(?<![]{0})A", 5, 0 )]
	[InlineData( "(?<![]{0})A", 7, 0 )]
	[InlineData( "(?<x>[])(?<=\\k<x>)A", 1, 8 )]
	[InlineData( "(?<x>[])(?<=\\k<x>)A", 3, 8 )]
	[InlineData( "(?<x>[])(?<=\\k<x>)A", 5, 8 )]
	[InlineData( "(?<x>[])(?<=\\k<x>)A", 7, 8 )]
	[InlineData( "(?<x>[])(?<= (?&x))A", 1, 8 )]
	[InlineData( "(?<x>[])(?<= (?&x))A", 3, 8 )]
	[InlineData( "(?<x>[])(?<= (?&x))A", 5, 8 )]
	[InlineData( "(?<x>[])(?<= (?&x))A", 7, 8 )]
	public void ConsumedEmptyClassSourceHasNoNativeLookbehindLength( string pattern, int mode, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, mode ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options( mode ) );
		Assert.Null( result.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?<=[^\\s\\S])A", 1, "" )]
	[InlineData( "(?<=[^\\s\\S])A", 3, "" )]
	[InlineData( "(?<=[^\\s\\S])A", 5, "" )]
	[InlineData( "(?<=[^\\s\\S])A", 7, "" )]
	[InlineData( "(?<![^\\s\\S])A", 1, "0:1" )]
	[InlineData( "(?<![^\\s\\S])A", 3, "0:1" )]
	[InlineData( "(?<![^\\s\\S])A", 5, "0:1" )]
	[InlineData( "(?<![^\\s\\S])A", 7, "0:1" )]
	[InlineData( "(?<=[[]])A", 1, "" )]
	[InlineData( "(?<=[[]])A", 3, "" )]
	[InlineData( "(?<=[[]])A", 5, "" )]
	[InlineData( "(?<=[[]])A", 7, "" )]
	[InlineData( "(?<![[]])A", 1, "0:1" )]
	[InlineData( "(?<![[]])A", 3, "0:1" )]
	[InlineData( "(?<![[]])A", 5, "0:1" )]
	[InlineData( "(?<![[]])A", 7, "0:1" )]
	[InlineData( "(?<=[[]]{0})A", 1, "" )]
	[InlineData( "(?<=[[]]{0})A", 3, "" )]
	[InlineData( "(?<=[[]]{0})A", 5, "0:1" )]
	[InlineData( "(?<=[[]]{0})A", 7, "0:1" )]
	[InlineData( "(?<![[]]{0})A", 1, "0:1" )]
	[InlineData( "(?<![[]]{0})A", 3, "0:1" )]
	[InlineData( "(?<![[]]{0})A", 5, "" )]
	[InlineData( "(?<![[]]{0})A", 7, "" )]
	[InlineData( "(?<=(?=[])A)B", 1, "" )]
	[InlineData( "(?<=(?=[])A)B", 3, "" )]
	[InlineData( "(?<=(?=[])A)B", 5, "" )]
	[InlineData( "(?<=(?=[])A)B", 7, "" )]
	[InlineData( "(?<=(?![])A)B", 1, "1:1" )]
	[InlineData( "(?<=(?![])A)B", 3, "1:1" )]
	[InlineData( "(?<=(?![])A)B", 5, "1:1" )]
	[InlineData( "(?<=(?![])A)B", 7, "1:1" )]
	[InlineData( "(?<=(?(DEFINE)(?<x>[]))A)B", 1, "1:1,-" )]
	[InlineData( "(?<=(?(DEFINE)(?<x>[]))A)B", 3, "1:1,-" )]
	[InlineData( "(?<=(?(DEFINE)(?<x>[]))A)B", 5, "1:1,-" )]
	[InlineData( "(?<=(?(DEFINE)(?<x>[]))A)B", 7, "1:1,-" )]
	[InlineData( "(?<=(*FAIL)[])A", 1, "" )]
	[InlineData( "(?<=(*FAIL)[])A", 3, "" )]
	[InlineData( "(?<=(*FAIL)[])A", 5, "" )]
	[InlineData( "(?<=(*FAIL)[])A", 7, "" )]
	public void ComputedSetsAndUnconsumedEmptyClassesKeepTheirNativeWidths( string pattern, int mode, string spans ) {
		var subject = new byte[] { (byte)'A', (byte)'B' };
		var reference = Reference( pattern, mode );
		AssertLiteral( reference, subject, spans );
		Compare( reference, subject, 0, Compile( pattern, Options( mode ) ).Match( subject ), pattern );
	}

	private static PcreByteOptions Options( int mode ) {
		var options = new PcreByteOptions { Utf8 = ( mode & 2 ) != 0, Ucp = ( mode & 2 ) != 0, AltExtendedClass = ( mode & 4 ) != 0 };
		typeof( PcreByteOptions ).GetProperty( "AllowEmptyClass" )?.SetValue( options, true );
		return options;
	}
	private static ReadOnlyMemory<byte> Mark( PcreByteMatch match ) => match.MarkName;

	private static void AssertLiteral( PcreRegex8Bit reference, byte[] subject, string spans ) {
		var match = reference.Match( subject );
		Assert.Equal( spans.Length > 0, match.Success );
		if ( !match.Success ) { return; }
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
		Assert.Equal( expected.Mark.ToArray(), Mark( actual.Match! ).ToArray() );
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, int mode ) => new( Encoding.UTF8.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
		Options = PcreOptions.Compiled | (PcreOptions)0x1 | ( ( mode & 2 ) != 0 ? PcreOptions.Utf | PcreOptions.Ucp : PcreOptions.None ) | ( ( mode & 4 ) != 0 ? (PcreOptions)0x08000000 : PcreOptions.None ), ParensLimit = 512
	} );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
