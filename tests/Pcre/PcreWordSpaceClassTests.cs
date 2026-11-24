using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordSpaceClassTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( @"\h", "x\u00A0y", 0, false ), ( @"\h", "x\u00A0y", 0, true ),
				( @"\H", " \tA", 0, false ), ( @"\H", " \tA", 0, true ),
				( @"\v", "x\u2028y", 0, false ), ( @"\v", "x\u2028y", 0, true ),
				( @"\V", "\n😀", 0, false ), ( @"\V", "\n😀", 0, true ),
				( @"[\hX]", "x\u2002", 0, false ), ( @"[\hX]", "x\u2002", 0, true ),
				( @"[^\h]", "\u2002Ā", 0, false ), ( @"[^\h]", "\u2002Ā", 0, true ),
				( @"[\vA]", "\u0085", 0, false ), ( @"[\vA]", "\u0085", 0, true ),
				( @"[^\V]", "\u2029", 0, false ), ( @"[^\V]", "\u2029", 0, true ),
				( @"a\hb", "a\u3000b", 0, false ), ( @"a\hb", "a\u3000b", 0, true ),
				( @"a\vb", "a\rb", 0, false ), ( @"a\vb", "a\rb", 0, true ),
				( @"[\h\v]", "x\u2028", 0, false ), ( @"[\h\v]", "x\u2028", 0, true ),
				( @"\h", "\u180E", 0, false ), ( @"\h", "\u180E", 0, true ),
				( @"\H", "😀", 0, false ), ( @"\H", "😀", 0, true ),
				( @"\V", "😀", 0, false ), ( @"\V", "😀", 0, true ),
				( @"\h", "x\u200B", 0, false ), ( @"\h", "x\u200B", 0, true ),
				( @"\v", "\u202F", 0, false ), ( @"\v", "\u202F", 0, true ),
				( @"[\H]", "😀", 0, false ), ( @"[\H]", "😀", 0, true ),
				( @"[^\V]", "\r\u0085", 0, false ), ( @"[^\V]", "\r\u0085", 0, true ),
				( @"[\h0-9]", "4", 0, false ), ( @"[\h0-9]", "4", 0, true ),
				( @"[\vX]", "X", 0, false ), ( @"[\vX]", "X", 0, true ),
				( @"\H", "😀", 1, false )
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
	public void FixedWhitespaceClassesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"class {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( @"\h", "x\u00A0" )]
	[InlineData( @"\v", "x\u2028" )]
	[InlineData( @"\H", "😀" )]
	[InlineData( @"[\h\v]", "\u2029" )]
	public void ValidUnicodeWhitespaceAgreesAcrossManagedWidths( string source, string subject ) {
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
	public void WhitespacePredicateConsumesOneBoundedStep( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( @"[\h]".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var match = pattern.Match( "\u00A0".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), match.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void PredicateSetsKeepTheirCombinedBoundAndOtherPropertiesRemainExplicit() {
		var allowed = "[" + new string( 'a', 63 ) + @"\h]";
		var rejected = "[" + new string( 'a', 64 ) + @"\h]";
		Assert.Equal( new PcreWordCapture( 0, 1 ),
			PcreWordSequenceCompiler.CompileSequence( allowed.AsMemory() ).Pattern!.Match( "\u00A0".AsMemory() ).Match );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( rejected.AsMemory() ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( @"\p{XUC}".AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
