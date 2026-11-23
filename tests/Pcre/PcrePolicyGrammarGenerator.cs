using System.Globalization;

namespace Icod.Expressions.Tests.Pcre;

internal sealed record PcreGeneratedPolicyCase( string Pattern, byte[] Subject, int Start );

// Test-only bounded grammar. xorshift32 and explicit byte chunks keep the
// corpus independent of System.Random versions, cultures and UTF fallback.
internal static class PcrePolicyGrammarGenerator {
	internal static IReadOnlyList<PcreGeneratedPolicyCase> Create( uint seed, int mode, int count = 128 ) {
		if ( seed == 0 ) { throw new ArgumentOutOfRangeException( nameof( seed ) ); }
		if ( mode is not 8 and not 15 ) { throw new ArgumentOutOfRangeException( nameof( mode ) ); }
		if ( count is < 1 or > 256 ) { throw new ArgumentOutOfRangeException( nameof( count ) ); }
		var generator = new Generator( seed, mode );
		var cases = new List<PcreGeneratedPolicyCase>( count );
		for ( var index = 0; index < count; index++ ) { cases.Add( generator.Sample() ); }
		return cases;
	}

	private sealed class Generator( uint seed, int mode ) {
		private static readonly string[] CommonAtoms = { "", "a", "b", ".", @"\d", @"\w", @"\b", @"\x00", "[ab]", "[^a]", @"\p{L}" };
		private static readonly string[] ByteAtoms = { @"\x80", @"\xFF" };
		private static readonly string[] UtfAtoms = { @"\x{E9}", @"\x{1F600}" };
		private static readonly string[] Chunks = { "", "61", "62", "00", "0A", "0D0A", "C3A9", "F09F9880", "FF", "80", "E282", "EDA080" };
		private uint state = seed;
		private int groups;

		private int Next( int bound ) {
			state ^= state << 13;
			state ^= state >> 17;
			state ^= state << 5;
			return (int)( state % (uint)bound );
		}

		internal PcreGeneratedPolicyCase Sample() {
			groups = 0;
			var pattern = String.Concat( "(*MARK:S)", Expression( 3 ) );
			var subject = new List<byte>();
			var chunks = Next( 5 );
			for ( var index = 0; index < chunks; index++ ) { subject.AddRange( Convert.FromHexString( Chunks[Next( Chunks.Length )] ) ); }
			return new( pattern, subject.ToArray(), Next( subject.Count + 1 ) );
		}

		private string Atom() {
			var index = Next( CommonAtoms.Length + 2 );
			return index < CommonAtoms.Length ? CommonAtoms[index] : ( mode == 8 ? ByteAtoms : UtfAtoms )[index - CommonAtoms.Length];
		}

		private string GroupName() => String.Concat( "g", ( ++groups ).ToString( CultureInfo.InvariantCulture ) );

		private string Expression( int depth ) {
			if ( depth == 0 ) { return Atom(); }
			switch ( Next( 9 ) ) {
				case 0: return Atom();
				case 1: return String.Concat( Expression( depth - 1 ), Expression( depth - 1 ) );
				case 2: return String.Concat( "(?:", Expression( depth - 1 ), "|", Expression( depth - 1 ), ")" );
				case 3: {
					var body = Expression( depth - 1 );
					return String.Concat( "(?:", body, "){0,2}", new[] { "", "?", "+" }[Next( 3 )] );
				}
				case 4: return String.Concat( Next( 2 ) == 0 ? "(?=" : "(?!", Expression( depth - 1 ), ")" );
				case 5: return String.Concat( "(?>", Expression( depth - 1 ), ")" );
				case 6: {
					var name = GroupName();
					return String.Concat( "(?<", name, ">", Expression( depth - 1 ), ")\\k<", name, ">" );
				}
				case 7: {
					var name = GroupName();
					return String.Concat( "(?<", name, ">", Atom(), ")?(?(<", name, ">)",
						Expression( depth - 1 ), "|", Expression( depth - 1 ), ")" );
				}
				default: return String.Concat( "(?i:", Expression( depth - 1 ), ")" );
			}
		}
	}
}
