using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordDotTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( ".", "a", 0, false ), ( ".", "x😀y", 1, true ), ( ".", "x😀y", 1, false ),
				( ".", "\n", 0, false ), ( ".", "\n", 0, true ),
				( "(?s).", "\n", 0, false ), ( "(?s).", "\n", 0, true ),
				( ".", "\r", 0, false ), ( ".", "\r\n", 0, true ),
				( "(?s)..", "\r\n", 0, true ),
				( @"\A.\z", "😀", 0, true ), ( @"\A.\z", "😀", 0, false ),
				( @"\A..\z", "😀", 0, false ), ( @"\A..\z", "😀", 0, true ),
				( "a.b", "aéb", 0, true ), ( "a.b", "a😀b", 0, true ),
				( "a(?s).", "a\n", 0, false ), ( "a(?s).", "a\n", 0, true ),
				( "(?s).", "\uD800", 0, false ), ( ".", "\uD800", 0, false ),
				( ".", "", 0, true ), ( "(?s)a.b", "a\nb", 0, false )
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
	public void ScalarAndUnitDotAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"dot {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( ".", "😀" )]
	[InlineData( @"\A.\z", "😀" )]
	[InlineData( "a.b", "aéb" )]
	[InlineData( "(?s).", "\n" )]
	public void ValidUnicodeDotHasEquivalentManagedScalarResults( string source, string subject ) {
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
	public void DotConsumesOneWorkStepAfterCandidateAdmission( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( ".".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var result = pattern.Match( "a".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), result.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void RawCodeUnitEscapeRemainsExplicitlyUnsupported() {
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( @"\C".AsMemory() ).Diagnostic?.Code );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
