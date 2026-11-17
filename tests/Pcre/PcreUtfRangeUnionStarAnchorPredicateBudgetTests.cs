using System.Text;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreUtfRangeUnionStarAnchorReferenceTests {
	[Theory]
	[InlineData( 4093, false )]
	[InlineData( 4093, true )]
	[InlineData( 4094, false )]
	[InlineData( 4094, true )]
	public void RangeUnionSuccessPreservesExistingPredicateCharges( int firstCount, bool disabled ) {
		var first = String.Concat( @"[\x00-\xFF\x{100}-\x{10FFFF}",
			String.Concat( Enumerable.Repeat( @"\P{Any}", firstCount - 1 ) ), @"\p{Any}]" );
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?:", first, @"*?Z|[\P{Any}\P{Any}\p{Any}]*?Z)" ) );
		AssertAdmissionAndWork( source, firstCount == 4093, disabled );
	}
}
