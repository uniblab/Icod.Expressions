using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordGeneralCategoryTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( @"\p{L}", "é", 0, false ), ( @"\p{L}", "é", 0, true ),
				( @"\p{L}", "😀中", 0, false ), ( @"\p{L}", "😀中", 0, true ),
				( @"\P{L}", "é😀", 0, false ), ( @"\P{L}", "é😀", 0, true ),
				( @"\p{Nd}", "x٣", 0, false ), ( @"\p{Nd}", "x٣", 0, true ),
				( @"\P{Nd}", "٣😀", 0, false ), ( @"\P{Nd}", "٣😀", 0, true ),
				( @"\pN", "Ⅻ", 0, false ), ( @"\pN", "Ⅻ", 0, true ),
				( @"\PN", "Ⅻ中", 0, false ), ( @"\PN", "Ⅻ中", 0, true ),
				( @"\p{Zs}", "\u00A0", 0, false ), ( @"\p{Zs}", "\u00A0", 0, true ),
				( @"\p{So}", "😀", 0, false ), ( @"\p{So}", "😀", 0, true ),
				( @"\P{Cs}", "😀", 0, false ), ( @"\P{Cs}", "😀", 0, true ),
				( @"[\p{L}X]", "é", 0, false ), ( @"[\p{L}X]", "é", 0, true ),
				( @"[^\P{L}]", "中", 0, false ), ( @"[^\P{L}]", "中", 0, true ),
				( @"[\P{Zs}A]", "\u00A0A", 0, false ), ( @"[\P{Zs}A]", "\u00A0A", 0, true ),
				( @"\p{Lu}", "Ω", 0, false ), ( @"\p{Lu}", "Ω", 0, true ),
				( @"[\p{Ll}\d]", "٣é", 0, false ), ( @"[\p{Ll}\d]", "٣é", 0, true ),
				( @"[\p{M}]", "\u0301", 0, false ), ( @"[\p{M}]", "\u0301", 0, true ),
				( @"\p{C}", "\u200D", 0, false ), ( @"\p{C}", "\u200D", 0, true ),
				( @"\p{S}", "😀", 0, false ), ( @"\p{S}", "😀", 0, true ),
				( @"\p{P}", "。", 0, false ), ( @"\p{P}", "。", 0, true ),
				( @"[^\P{Nl}A]", "Ⅻ", 0, false ), ( @"[^\P{Nl}A]", "Ⅻ", 0, true ),
				( @"\p{Z}", "\u3000", 0, false ), ( @"\p{Z}", "\u3000", 0, true ),
				( @"\p{L&}", "ǅ", 0, false ), ( @"\p{L&}", "ǅ", 0, true ),
				( @"\p{Cs}", "\uD800", 0, false ), ( @"\p{Cs}", "😀", 0, false ),
				( @"\p{So}", "😀", 1, false )
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
	public void GeneralCategoriesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"category {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( @"\p{L}", "é" )]
	[InlineData( @"\p{So}", "😀" )]
	[InlineData( @"\P{Cs}", "😀" )]
	[InlineData( @"[\p{Ll}\d]", "٣é" )]
	public void ValidUnicodeCategoriesAgreeAcrossManagedWidths( string source, string subject ) {
		var subjectBytes = Encoding.UTF8.GetBytes( subject );
		var bytes = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true } );
		var words = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.True( bytes.IsSuccess ); Assert.True( words.IsSuccess );
		var byteMatch = bytes.Pattern!.Match( subjectBytes );
		var wordMatch = words.Pattern!.Match( subject.AsMemory() );
		Assert.True( byteMatch.IsSuccess ); Assert.True( wordMatch.IsSuccess );
		var coordinates = new PcreDualWidthCoordinates( subjectBytes, subject );
		Assert.Equal( coordinates.ByteCaptures( byteMatch.Match is { } b ? $"{b.ByteIndex}:{b.ByteLength}" : "" ),
			coordinates.WordCaptures( wordMatch.Match is { } w ? $"{w.CodeUnitIndex}:{w.CodeUnitLength}" : "" ) );
	}

	[Theory]
	[InlineData( "C", "\u200D" )]
	[InlineData( "Cc", "\u0001" )]
	[InlineData( "Cf", "\u200D" )]
	[InlineData( "Cn", "\u0378" )]
	[InlineData( "Co", "\uE000" )]
	[InlineData( "L", "中" )]
	[InlineData( "Lc", "ǅ" )]
	[InlineData( "L&", "ǅ" )]
	[InlineData( "Ll", "é" )]
	[InlineData( "Lm", "ᵃ" )]
	[InlineData( "Lo", "中" )]
	[InlineData( "Lt", "ǅ" )]
	[InlineData( "Lu", "Ω" )]
	[InlineData( "M", "\u0301" )]
	[InlineData( "Mc", "\u093E" )]
	[InlineData( "Me", "\u20DD" )]
	[InlineData( "Mn", "\u0301" )]
	[InlineData( "N", "Ⅻ" )]
	[InlineData( "Nd", "٣" )]
	[InlineData( "Nl", "Ⅻ" )]
	[InlineData( "No", "²" )]
	[InlineData( "P", "。" )]
	[InlineData( "Pc", "_" )]
	[InlineData( "Pd", "‐" )]
	[InlineData( "Pe", ")" )]
	[InlineData( "Pf", "”" )]
	[InlineData( "Pi", "“" )]
	[InlineData( "Po", "!" )]
	[InlineData( "Ps", "(" )]
	[InlineData( "S", "😀" )]
	[InlineData( "Sc", "£" )]
	[InlineData( "Sk", "^" )]
	[InlineData( "Sm", "+" )]
	[InlineData( "So", "😀" )]
	[InlineData( "Z", "\u00A0" )]
	[InlineData( "Zl", "\u2028" )]
	[InlineData( "Zp", "\u2029" )]
	[InlineData( "Zs", "\u00A0" )]
	public void EachCategoryNameUsesNativeWordMembership( string category, string subject ) {
		var source = String.Concat( @"\p{", category, "}" );
		foreach ( var utf in new[] { false, true } ) {
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf } );
			Assert.True( compiled.IsSuccess, $"{category}/{utf}: {compiled.Diagnostic}" );
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, 0 );
			Assert.True( native.Kind is "M" or "N", $"native {category}/{utf}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory() );
			Assert.True( match.IsSuccess );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( false, 3 )]
	[InlineData( true, 4 )]
	public void GeneralCategoryConsumesOneBoundedStep( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( @"[\p{Zs}]".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var match = pattern.Match( "\u00A0".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), match.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void GeneralCategoryBoundAndOtherPropertyAdmissionStayExplicit() {
		var allowed = "[" + new string( 'a', 63 ) + @"\p{L}]";
		var rejected = "[" + new string( 'a', 64 ) + @"\p{L}]";
		Assert.Equal( new PcreWordCapture( 0, 1 ),
			PcreWordSequenceCompiler.CompileSequence( allowed.AsMemory() ).Pattern!.Match( "é".AsMemory() ).Match );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( rejected.AsMemory() ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( @"\p{XWD}".AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
