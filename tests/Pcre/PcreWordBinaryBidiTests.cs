using System.Globalization;
using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordBinaryBidiTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( @"\p{Emoji}", "#", 0, false ), ( @"\p{Emoji}", "#", 0, true ),
				( @"\p{Emoji}", "😀", 0, false ), ( @"\p{Emoji}", "😀", 0, true ),
				( @"\P{Emoji}", "😀A", 0, false ), ( @"\P{Emoji}", "😀A", 0, true ),
				( @"\p{Alphabetic}", "Ω", 0, false ), ( @"\p{Alphabetic}", "Ω", 0, true ),
				( @"\p{Alpha}", "\u0345", 0, false ), ( @"\p{Alpha}", "\u0345", 0, true ),
				( @"\p{White_Space}", "x\u0085", 0, false ), ( @"\p{White_Space}", "x\u0085", 0, true ),
				( @"\P{White_Space}", "\u0085\u180E", 0, false ), ( @"\P{White_Space}", "\u0085\u180E", 0, true ),
				( @"\p{BidiC}", "\u200E", 0, false ), ( @"\p{BidiC}", "\u200E", 0, true ),
				( @"\p{BidiM}", "(", 0, false ), ( @"\p{BidiM}", "(", 0, true ),
				( @"\p{Lowercase}", "aA", 0, false ), ( @"\p{Lowercase}", "aA", 0, true ),
				( @"\p{Lower}", "aA", 0, false ), ( @"\p{Lower}", "aA", 0, true ),
				( @"\p{Uppercase}", "A", 0, false ), ( @"\p{Uppercase}", "A", 0, true ),
				( @"\p{Hex_Digit}", "０", 0, false ), ( @"\p{Hex_Digit}", "０", 0, true ),
				( @"\p{ASCII_Hex_Digit}", "０F", 0, false ), ( @"\p{ASCII_Hex_Digit}", "０F", 0, true ),
				( @"\p{Emoji_Modifier}", "🏻", 0, false ), ( @"\p{Emoji_Modifier}", "🏻", 0, true ),
				( @"\p{Variation_Selector}", "\uFE0F", 0, false ), ( @"\p{Variation_Selector}", "\uFE0F", 0, true ),
				( @"\p{BC=R}", "א", 0, false ), ( @"\p{BC=R}", "א", 0, true ),
				( @"\p{BC=AL}", "ا", 0, false ), ( @"\p{BC=AL}", "ا", 0, true ),
				( @"\p{Bidi_Class=NSM}", "\u0300", 0, false ), ( @"\p{Bidi_Class=NSM}", "\u0300", 0, true ),
				( @"\P{BC=R}", "אA", 0, false ), ( @"\P{BC=R}", "אA", 0, true ),
				( @"[\p{Emoji}\p{BC=R}]", "#א", 0, false ), ( @"[\p{Emoji}\p{BC=R}]", "#א", 0, true ),
				( @"[^\P{Alphabetic}]", "Ω", 0, false ), ( @"[^\P{Alphabetic}]", "Ω", 0, true ),
				( @"[\p{BC=R}\p{Alpha}]", "Ωא", 0, false ), ( @"[\p{BC=R}\p{Alpha}]", "Ωא", 0, true ),
				( @"a\p{Emoji}b", "a#b", 0, false ), ( @"a\p{Emoji}b", "a#b", 0, true ),
				( @"\p{BC=ON}", "!", 0, false ), ( @"\p{BC=ON}", "!", 0, true ),
				( @"\P{Emoji}", "😀", 1, false )
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
	public void BinaryAndBidiPropertiesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"property {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Fact]
	public void EveryPinnedBinaryAndBidiNameUsesNativeMembership() {
		void Check( string name, string subject ) {
			var source = String.Concat( @"\p{", name, "}" );
			foreach ( var utf in new[] { false, true } ) {
				var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf } );
				Assert.True( compiled.IsSuccess, $"{name}/{utf}: {compiled.Diagnostic}" );
				var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, 0 );
				Assert.True( native.Kind is "M" or "N", $"native {name}/{utf}: {native}" );
				var match = compiled.Pattern!.Match( subject.AsMemory() );
				Assert.True( match.IsSuccess );
				Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
			}
		}
		foreach ( var row in PcreBinaryPropertyFixtureData.Binary.Split( '\n', StringSplitOptions.RemoveEmptyEntries ) ) {
			var fields = row.Split( ';' );
			Check( fields[1], char.ConvertFromUtf32( int.Parse( fields[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture ) ) );
		}
		foreach ( var row in PcreBinaryPropertyFixtureData.Bidi.Split( '\n', StringSplitOptions.RemoveEmptyEntries ) ) {
			var fields = row.Split( ';' );
			Check( String.Concat( "BC=", fields[1] ),
				char.ConvertFromUtf32( int.Parse( fields[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture ) ) );
		}
	}

	[Theory]
	[InlineData( @"\p{Emoji}", "😀" )]
	[InlineData( @"\P{Emoji}", "😀" )]
	[InlineData( @"\p{BC=R}", "א" )]
	[InlineData( @"[\p{Emoji}\p{BC=R}]", "#א" )]
	public void ValidUnicodePropertiesAgreeAcrossManagedWidths( string source, string subject ) {
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
	public void BinaryPredicateConsumesOneBoundedStep( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( @"[\p{Emoji}]".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var match = pattern.Match( "#".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), match.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void BinaryBoundAndSpecialPropertiesRemainExplicit() {
		var allowed = "[" + new string( 'a', 63 ) + @"\p{Emoji}]";
		var rejected = "[" + new string( 'a', 64 ) + @"\p{Emoji}]";
		Assert.Equal( new PcreWordCapture( 0, 1 ),
			PcreWordSequenceCompiler.CompileSequence( allowed.AsMemory() ).Pattern!.Match( "#".AsMemory() ).Match );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( rejected.AsMemory() ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( @"\p{XWD}".AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
