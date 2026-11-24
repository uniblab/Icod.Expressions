using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordPatternSourceTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "é", "xéy", 0, false ), ( "é", "xéy", 0, true ),
				( "Ā", "xĀ", 0, false ), ( "Ā", "xĀ", 0, true ),
				( "\uD800", "x\uD800y", 0, false ), ( "\uDC00", "x\uDC00y", 0, false ),
				( "😀", "x😀y", 0, false ), ( "😀", "x😀y", 0, true ),
				( "😀a", "x😀ay", 0, false ), ( @"\Aé😀\z", "é😀", 0, true ),
				( @"\Qé😀.*\E", "xé😀.*y", 0, true ),
				( "\\Q\uD800\\E", "\uD800", 0, false ),
				( @"\😀", "😀", 0, true ), ( @"\é", "é", 0, true ),
				( @"é\x{1F600}", "xé😀y", 0, true ),
				( "🙂a", "🙂a", 0, false ), ( "🙂a", "🙂a", 0, true ),
				( "é", "xéy", 1, true )
			};
			for ( var index = 0; index < cases.Length; index++ ) {
				var (source, subject, start, utf) = cases[index];
				foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 63 } ) {
					yield return new object[] { index, source, subject, start, utf, defaults };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void NativeWordSourceAgreesWithEveryCallPolicy( int id, string source, string subject,
		int start, bool utf, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"source {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			Assert.Equal( native.Captures, result.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "é😀", "xé😀y" )]
	[InlineData( @"\Aé😀\z", "é😀" )]
	[InlineData( @"\é😀", "é😀" )]
	[InlineData( @"\Qé😀.*\E", "xé😀.*y" )]
	public void ValidUnicodeSourceHasTheSameScalarResultInBothWidths( string source, string subject ) {
		var patternBytes = Encoding.UTF8.GetBytes( source );
		var subjectBytes = Encoding.UTF8.GetBytes( subject );
		var bytes = PcreByteCompiler.Compile( patternBytes, new() { Utf8 = true } );
		var words = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.True( bytes.IsSuccess ); Assert.True( words.IsSuccess );
		var byteMatch = bytes.Pattern!.Match( subjectBytes );
		var wordMatch = words.Pattern!.Match( subject.AsMemory() );
		Assert.True( byteMatch.IsSuccess ); Assert.True( wordMatch.IsSuccess );
		var positions = new PcreDualWidthCoordinates( subjectBytes, subject );
		Assert.Equal( positions.ByteCaptures( byteMatch.Match is { } b ? $"{b.ByteIndex}:{b.ByteLength}" : "" ),
			positions.WordCaptures( wordMatch.Match is { } w ? $"{w.CodeUnitIndex}:{w.CodeUnitLength}" : "" ) );
	}

	[Theory]
	[InlineData( "😀\\", 2, false )]
	[InlineData( "😀\\", 2, true )]
	[InlineData( "a😀\\", 3, true )]
	public void SyntaxDiagnosticsUseOriginalPatternUnits( string source, int slashOffset, bool utf ) {
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Array.Empty<ushort>(), utf, 0 );
		Assert.Equal( "E", native.Kind );
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf } );
		Assert.Equal( PcreWordDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( slashOffset, result.Diagnostic?.CodeUnitOffset );
	}

	[Fact]
	public void WordSourceIsOwnedAndOtherPropertiesStayExplicit() {
		var source = "é😀".ToCharArray();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source, new() { Utf16 = true } ).Pattern!;
		Array.Fill( source, 'x' );
		Assert.Equal( new PcreWordCapture( 0, 3 ), pattern.Match( "é😀".AsMemory() ).Match );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( @"[\p{XWD}]".AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Pattern,
			PcreWordSequenceCompiler.CompileSequence( "😀\uD800".AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
		Assert.Equal( new PcreWordCapture( 0, 1 ),
			PcreWordSequenceCompiler.CompileSequence( "\uD800".AsMemory() ).Pattern!.Match( "\uD800".AsMemory() ).Match );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ExtendedModeSkipsWidePatternWhitespace( bool utf ) {
		const string source = "(?x)\u200Eé";
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( "é" ), utf, 0 );
		Assert.Equal( "M", native.Kind );
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf } );
		Assert.True( compiled.IsSuccess );
		Assert.Equal( new PcreWordCapture( 0, 1 ), compiled.Pattern!.Match( "é".AsMemory() ).Match );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
