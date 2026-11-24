using System.Globalization;
using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordScriptTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( @"\p{Greek}", "·Ω", 0, false ), ( @"\p{Greek}", "·Ω", 0, true ),
				( @"\p{sc=Greek}", "·Ω", 0, false ), ( @"\p{sc=Greek}", "·Ω", 0, true ),
				( @"\p{scx=Greek}", "·Ω", 0, false ), ( @"\p{scx=Greek}", "·Ω", 0, true ),
				( @"\P{Greek}", "ΩA", 0, false ), ( @"\P{Greek}", "ΩA", 0, true ),
				( @"\p{sc=Hira}", "ーあ", 0, false ), ( @"\p{sc=Hira}", "ーあ", 0, true ),
				( @"\p{Hira}", "ーあ", 0, false ), ( @"\p{Hira}", "ーあ", 0, true ),
				( @"\p{Latn}", "é", 0, false ), ( @"\p{Latn}", "é", 0, true ),
				( @"\p{sc=Adlm}", "𞤀", 0, false ), ( @"\p{sc=Adlm}", "𞤀", 0, true ),
				( @"\p{scx=Adlm}", "𞤀", 0, false ), ( @"\p{scx=Adlm}", "𞤀", 0, true ),
				( @"[\p{Greek}A]", "·", 0, false ), ( @"[\p{Greek}A]", "·", 0, true ),
				( @"[\P{sc=Greek}Ω]", "·Ω", 0, false ), ( @"[\P{sc=Greek}Ω]", "·Ω", 0, true ),
				( @"[^\P{scx=Greek}A]", "·", 0, false ), ( @"[^\P{scx=Greek}A]", "·", 0, true ),
				( @"[\p{scx=Greek}\p{Latn}]", "A", 0, false ), ( @"[\p{scx=Greek}\p{Latn}]", "A", 0, true ),
				( @"[\p{scx=Greek}\d]", "5", 0, false ), ( @"[\p{scx=Greek}\d]", "5", 0, true ),
				( @"a\p{sc=Greek}b", "aΩb", 0, false ), ( @"a\p{sc=Greek}b", "aΩb", 0, true ),
				( @"\p{sc=Unknown}", "\u0378", 0, false ), ( @"\p{sc=Unknown}", "\u0378", 0, true ),
				( @"\p{scx=Zinh}", "\u0300", 0, false ), ( @"\p{scx=Zinh}", "\u0300", 0, true ),
				( @"\p{sc=Zyyy}", "ー", 0, false ), ( @"\p{sc=Zyyy}", "ー", 0, true ),
				( @"\P{sc=Greek}", "😀", 0, false ), ( @"\P{sc=Greek}", "😀", 0, true ),
				( @"[^\P{scx=Greek}]", "Ω", 0, false ), ( @"[^\P{scx=Greek}]", "Ω", 0, true ),
				( @"\P{sc=Greek}", "😀", 1, false )
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
	public void ScriptPropertiesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"script {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Fact]
	public void NativeScriptNamesRemainAvailableInBothWordModes() {
		foreach ( var row in PcreScriptPropertyFixtureData.Scripts.Split( '\n' ) ) {
			var fields = row.Split( ';' );
			var scalar = int.Parse( fields[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture );
			var subject = char.ConvertFromUtf32( scalar );
			foreach ( var name in new[] { "sc=", "scx=" } ) {
				var source = String.Concat( @"\p{", name, fields[0], "}" );
				foreach ( var utf in new[] { false, true } ) {
					var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf } );
					Assert.True( compiled.IsSuccess, $"{source}/{utf}: {compiled.Diagnostic}" );
					var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, 0 );
					Assert.True( native.Kind is "M" or "N", $"native {source}/{utf}: {native}" );
					var match = compiled.Pattern!.Match( subject.AsMemory() );
					Assert.True( match.IsSuccess );
					Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"\p{Greek}", "·Ω" )]
	[InlineData( @"\p{sc=Greek}", "·Ω" )]
	[InlineData( @"\p{sc=Adlm}", "𞤀" )]
	[InlineData( @"[^\P{scx=Greek}]", "Ω" )]
	public void ValidUnicodeScriptsAgreeAcrossManagedWidths( string source, string subject ) {
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
	[InlineData( false, 3 )]
	[InlineData( true, 4 )]
	public void ScriptPredicateConsumesOneBoundedStep( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( @"[\p{Greek}]".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var match = pattern.Match( "Ω".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), match.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void ScriptBoundAndOtherPropertiesStayExplicit() {
		var allowed = "[" + new string( 'a', 63 ) + @"\p{Greek}]";
		var rejected = "[" + new string( 'a', 64 ) + @"\p{Greek}]";
		Assert.Equal( new PcreWordCapture( 0, 1 ),
			PcreWordSequenceCompiler.CompileSequence( allowed.AsMemory() ).Pattern!.Match( "Ω".AsMemory() ).Match );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( rejected.AsMemory() ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( @"\p{XUC}".AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
