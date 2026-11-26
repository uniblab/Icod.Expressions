using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordAlternativeTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "a|ab", "ab", 0, false, false ), ( "a|ab", "ab", 0, true, false ),
				( "ab|a", "ab", 0, false, false ), ( "ab|a", "ab", 0, true, false ),
				( "a|ab", "zab", 0, false, false ), ( "a|ab", "zab", 0, true, false ),
				( "a|a", "a", 0, false, false ), ( "a|a", "a", 0, true, false ),
				( "a|b", "z", 0, false, false ), ( "a|b", "z", 0, true, false ),
				( "a|b", "xb", 1, false, false ), ( "a|b", "xb", 1, true, false ),
				( "|a", "a", 0, false, false ), ( "|a", "a", 0, true, false ),
				( "a|", "a", 0, false, false ), ( "a|", "a", 0, true, false ),
				( "|a", "x", 0, false, false ), ( "|a", "x", 0, true, false ),
				( "a|", "x", 0, false, false ), ( "a|", "x", 0, true, false ),
				( "a||b", "b", 0, false, false ), ( "a||b", "b", 0, true, false ),
				( "|", "", 0, false, false ), ( "|", "", 0, true, false ),
				( @"a\z|ab", "ab", 0, false, false ), ( @"a\z|ab", "ab", 0, true, false ),
				( @"\Aab|b", "xab", 0, false, false ), ( @"\Aab|b", "xab", 0, true, false ),
				( @"\Ga|a", "xa", 1, false, false ), ( @"\Ga|a", "xa", 1, true, false ),
				( @"\p{Greek}|a", "Ω", 0, false, false ), ( @"\p{Greek}|a", "Ω", 0, true, false ),
				( @"\d|Ω", "٥", 0, false, true ), ( @"\d|Ω", "٥", 0, true, true ),
				( "[[:digit:]]|Ω", "٥", 0, false, true ), ( "[[:digit:]]|Ω", "٥", 0, true, true ),
				( "(?i)a|b", "B", 0, false, false ), ( "(?i)a|b", "B", 0, true, true ),
				( "a|(?i)b", "B", 0, false, false ), ( "a|(?i)b", "B", 0, true, true ),
				( "(?i)a|(?-i)b", "B", 0, false, false ), ( "(?i)a|(?-i)b", "B", 0, true, true ),
				( "(?i)[é]|É", "É", 0, false, false ), ( "(?i)[é]|É", "É", 0, true, true ),
				( "😀|😃", "😃", 0, false, false ), ( "😀|😃", "😃", 0, true, true ),
				( "a|😀", "😀", 0, false, false ), ( "a|😀", "😀", 0, true, true ),
				( "a|😀", "x😀", 0, false, false ), ( "a|😀", "x😀", 0, true, true ),
				( @"\x{D800}|A", "\uD800", 0, false, false ),
				( @"\x{D800}|A", "\uD800", 0, false, true ),
				( "a|b", "😀b", 1, false, false ), ( "a|b", "😀b", 0, true, false )
			};
			for ( var index = 0; index < cases.Length; index++ ) {
				var (source, subject, start, utf, ucp) = cases[index];
				foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 63 } ) {
					yield return new object[] { index, source, subject, start, utf, ucp, defaults };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void OrderedBranchesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"branch {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "a|ab", "ab" )]
	[InlineData( "ab|a", "ab" )]
	[InlineData( "a|😀", "😀" )]
	[InlineData( @"\p{Greek}|é", "Ω" )]
	public void ValidUnicodeBranchesAgreeAcrossManagedWidths( string source, string subject ) {
		var subjectBytes = Encoding.UTF8.GetBytes( subject );
		var bytes = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true, Ucp = true } );
		var words = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true, Ucp = true } );
		Assert.True( bytes.IsSuccess ); Assert.True( words.IsSuccess );
		var byteMatch = bytes.Pattern!.Match( subjectBytes );
		var wordMatch = words.Pattern!.Match( subject.AsMemory() );
		Assert.True( byteMatch.IsSuccess ); Assert.True( wordMatch.IsSuccess );
		var coordinates = new PcreDualWidthCoordinates( subjectBytes, subject );
		Assert.Equal( coordinates.ByteCaptures( byteMatch.Match is { } b ? $"{b.ByteIndex}:{b.ByteLength}" : "" ),
			coordinates.WordCaptures( wordMatch.Match is { } w ? $"{w.CodeUnitIndex}:{w.CodeUnitLength}" : "" ) );
	}

	[Theory]
	[InlineData( false, 6 )]
	[InlineData( true, 7 )]
	public void BranchRetriesConsumeExactWork( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( "x|a".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var match = pattern.Match( "a".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), match.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void BranchBoundAndUnsupportedNestedExecutionStayExplicit() {
		var allowed = String.Join( '|', Enumerable.Repeat( "x", 63 ).Append( "a" ) );
		var rejected = String.Concat( allowed, "|a" );
		Assert.Equal( new PcreWordCapture( 0, 1 ),
			PcreWordSequenceCompiler.CompileSequence( allowed.AsMemory() ).Pattern!.Match( "a".AsMemory() ).Match );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( rejected.AsMemory() ).Diagnostic?.Code );
		foreach ( var source in new[] { "(?:a|b)", "(a|b)", "(*CRLF)a|^b" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
