using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreUtfCaseFoldRangeWorkTests {
	[Theory]
	[InlineData( @"(?i)[zA]", "41", 0 )]
	[InlineData( @"(?i)[zA]", "5A", 0 )]
	[InlineData( @"(?i)[zA]", "62", -1 )]
	[InlineData( @"[zA]", "41", 0 )]
	[InlineData( @"(?i)[z\p{L}]", "41", 0 )]
	[InlineData( @"(?i)[^zA]", "62", 0 )]
	[InlineData( @"(?i)[^zA]", "5A", -1 )]
	public void FoldedRangeSearchRetainsRawOrderPredicatesAndNegation( string pattern, string hex, int index ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var tolerant in new[] { false, true } ) {
				var expected = Reference( pattern, ucp, tolerant ).Match( subject );
				Assert.Equal( index, expected.Success ? expected.Index : -1 );
				var actual = Compile( pattern, new() { Utf8 = true, Ucp = ucp, MatchInvalidUtf = tolerant } ).Match( subject );
				Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
				Assert.Equal( index, actual.Match?.ByteIndex ?? -1 );
				if ( expected.Success ) {
					Assert.Equal( expected.Length, actual.Match!.ByteLength );
					Assert.Equal( new PcreByteCapture( expected.Index, expected.Length ), actual.Match.Captures[0] );
				}
			}
		}
	}

	[Fact]
	public void ScalarBelowFoldedRangeMinimumRetainsChargedExactAndExhaustedWork() {
		const string pattern = @"(?i)[\x{100}-\x{10ffff}]";
		var subject = new byte[] { 0x0D };
		Assert.False( Reference( pattern, false, false ).Match( subject ).Success );
		var steps = new List<int>();
		var result = Compile( pattern, new() { Utf8 = true, MatchWorkLimit = 16, WorkObserver = steps.Add } ).Match( subject );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Null( result.Match );
		Assert.InRange( steps.Count, 1, 16 );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		foreach ( var observed in new[] { false, true } ) {
			var exact = Compile( pattern, new() { Utf8 = true, MatchWorkLimit = steps.Count,
				WorkObserver = observed ? _ => { } : null } ).Match( subject );
			Assert.True( exact.IsSuccess, exact.Diagnostic?.Message );
			Assert.Null( exact.Match );
			var exhausted = Compile( pattern, new() { Utf8 = true, MatchWorkLimit = steps.Count - 1,
				WorkObserver = observed ? _ => { } : null } ).Match( subject );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool ucp, bool tolerant ) => new(
		Encoding.ASCII.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | PcreOptions.Utf | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ) } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
