using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordClassesTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "[ab]", "xxby", 0, false ), ( "[ab]", "xxby", 0, true ),
				( "[a-z]", "9m", 0, false ), ( "[a-z]", "9m", 0, true ),
				( "[^a]", "a😀", 0, false ), ( "[^a]", "a😀", 0, true ),
				( "[éĀ]", "xĀy", 0, false ), ( "[éĀ]", "xĀy", 0, true ),
				( @"[\x{100}-\x{1FF}]", "ŀ", 0, false ), ( @"[\x{100}-\x{1FF}]", "ŀ", 0, true ),
				( "[😀]", "😀", 0, false ), ( "[😀]", "😀", 0, true ),
				( "[😀]", "😀", 1, false ),
				( "[^😀]", "😀", 0, false ), ( "[^😀]", "😀", 0, true ),
				( @"[\x{D800}]", "\uD800", 0, false ), ( @"[\x{1F600}]", "😀", 0, true ),
				( @"a[^\n]b", "a\rb", 0, false ), ( @"a[^\n]b", "a\nb", 0, true ),
				( "[0-9]", "😀4", 0, true ), ( @"[\-]", "-", 0, false ),
				( "[]a]", "x]y", 0, true )
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
	public void LiteralWordClassesAgreeWithNativePolicies( int id, string source, string subject,
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
	[InlineData( "[😀]", "x😀y" )]
	[InlineData( "[éĀ]", "Ā" )]
	[InlineData( "[^a]", "a😀" )]
	[InlineData( @"[\x{100}-\x{1FF}]", "ŀ" )]
	public void ValidUnicodeClassesHaveEquivalentManagedScalarResults( string source, string subject ) {
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
	public void ClassConsumesOneStepAfterCandidateAdmission( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( "[a]".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var result = pattern.Match( "a".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), result.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void ClassAdmissionBoundsRangeScanningAndRejectsUnicodePredicates() {
		var sixtyFour = "[" + new string( 'a', 64 ) + "]";
		var sixtyFive = "[" + new string( 'a', 65 ) + "]";
		Assert.Equal( new PcreWordCapture( 0, 1 ),
			PcreWordSequenceCompiler.CompileSequence( sixtyFour.AsMemory() ).Pattern!.Match( "a".AsMemory() ).Match );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( sixtyFive.AsMemory() ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( @"[\p{L}]".AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
