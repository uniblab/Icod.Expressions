using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreNative16BatchTests {
	[Theory]
	[InlineData( "(?<x>a)?(*MARK:M)|b", "b", true )]
	[InlineData( "(*MARK:before)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:inside)(*PRUNE)))", "abc", true )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT)b))(?:(?&x(2))X|ac)d", "acd", true )]
	[InlineData( "(?*(a|ab))\\1c", "abc", false )]
	[InlineData( "(*MARK:M)(?(?=(a)(*ACCEPT:N))a|b)(?*(*ACCEPT)){0}", "a", true )]
	[InlineData( "(", "", true )]
	[InlineData( "a", "\ud800", true )]
	[InlineData( "(?<x>😀)(*MARK:é😀)", "😀", true )]
	public void BatchPoliciesRetainFreshCompileCapturesLabelsAndErrors( string source, string subject, bool utf ) {
		var patternUnits = Words( source ); var subjectUnits = Words( subject );
		foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 16, 32, 63 } ) {
			var batch = PcreNative16Oracle.ExecuteSequencePolicies( patternUnits, subjectUnits, utf, 0, defaults );
			Assert.Equal( 64, batch.Length );
			for ( var mask = 0; mask < batch.Length; mask++ ) {
				var fresh = PcreNative16Oracle.ExecuteSequence( patternUnits, subjectUnits, utf, 0, defaults, mask );
				Compare( fresh, batch[mask] );
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void LiteralBatchPoliciesRetainFreshCompileResults( bool utf ) {
		foreach ( var source in new[] { "", "a+b", "😀", "\ud800" } ) {
			var patternUnits = Words( source ); var subjectUnits = Words( String.Concat( "x", source ) );
			foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 16, 32, 63 } ) {
				var batch = PcreNative16Oracle.ExecuteLiteralPolicies( patternUnits, subjectUnits, utf, 0, defaults );
				Assert.Equal( 64, batch.Length );
				for ( var mask = 0; mask < batch.Length; mask++ ) {
					Compare( PcreNative16Oracle.ExecuteLiteral( patternUnits, subjectUnits, utf, 0, defaults, mask ), batch[mask] );
				}
			}
		}
	}

	private static void Compare( PcreNative16Protocol expected, PcreNative16Protocol actual ) {
		Assert.Equal( expected.Kind, actual.Kind );
		Assert.Equal( expected.ErrorCode, actual.ErrorCode ); Assert.Equal( expected.ErrorOffset, actual.ErrorOffset );
		Assert.Equal( expected.Captures, actual.Captures ); Assert.Equal( expected.Mark, actual.Mark );
		Assert.Equal( expected.Names, actual.Names ); Assert.Equal( expected.Options, actual.Options );
		Assert.Equal( expected.JitSize == 0, actual.JitSize == 0 );
	}

	private static ushort[] Words( string value ) => value.Select( unit => (ushort)unit ).ToArray();
}
