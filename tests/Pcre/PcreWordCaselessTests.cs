using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordCaselessTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( @"(?i)a", "A", 0, false, false ), ( @"(?i)a", "A", 0, true, false ),
				( @"(?i)a", "A", 0, false, true ), ( @"(?i)a", "A", 0, true, true ),
				( @"(?i)é", "É", 0, false, false ), ( @"(?i)é", "É", 0, true, false ),
				( @"(?i)é", "É", 0, false, true ), ( @"(?i)é", "É", 0, true, true ),
				( @"(?i)K", "K", 0, false, false ), ( @"(?i)K", "K", 0, true, false ),
				( @"(?i)K", "K", 0, false, true ), ( @"(?i)K", "K", 0, true, true ),
				( @"(?i)ſ", "s", 0, false, false ), ( @"(?i)ſ", "s", 0, true, false ),
				( @"(?i)ſ", "s", 0, false, true ), ( @"(?i)ſ", "s", 0, true, true ),
				( @"(?ir)K", "K", 0, false, true ), ( @"(?ir)K", "K", 0, true, true ),
				( @"(?i)[a-z]", "K", 0, false, false ), ( @"(?i)[a-z]", "K", 0, true, false ),
				( @"(?i)[a-z]", "K", 0, false, true ), ( @"(?i)[a-z]", "K", 0, true, true ),
				( @"(?i)[é]", "É", 0, false, false ), ( @"(?i)[é]", "É", 0, true, false ),
				( @"(?i)[é]", "É", 0, false, true ), ( @"(?i)[é]", "É", 0, true, true ),
				( @"(?i)[A-Z]", "k", 0, false, false ), ( @"(?i)[A-Z]", "k", 0, true, false ),
				( @"(?i)[A-Z]", "k", 0, false, true ), ( @"(?i)[A-Z]", "k", 0, true, true ),
				( @"(?i)[^a]", "A!", 0, false, false ), ( @"(?i)[^a]", "A!", 0, true, true ),
				( @"(?i)[\p{Lu}x]", "a", 0, false, false ), ( @"(?i)[\p{Lu}x]", "a", 0, true, true ),
				( @"(?i)\p{Lu}", "a", 0, false, false ), ( @"(?i)\p{Lu}", "a", 0, true, true ),
				( "(?i)[[:lower:]]", "A", 0, false, false ), ( "(?i)[[:lower:]]", "A", 0, true, false ),
				( "(?i)[[:lower:]]", "A", 0, false, true ), ( "(?i)[[:lower:]]", "A", 0, true, true ),
				( @"(?i)A(?-i)b", "aB", 0, false, false ), ( @"(?i)A(?-i)b", "aB", 0, true, true ),
				( @"a(?i)b", "aB", 0, false, false ), ( @"a(?i)b", "aB", 0, true, true ),
				( @"(?i)𐐀", "𐐨", 0, false, true ), ( @"(?i)𐐀", "𐐨", 0, true, false ),
				( @"(?i)𐐀", "𐐨", 0, true, true ),
				( @"(?i)[\x{D800}]", "\uD800", 0, false, false ),
				( @"(?i)[\x{D800}]", "\uD800", 0, false, true ),
				( @"(?i)[\p{Cs}]", "\uD800", 0, false, true ),
				( @"(?i)[a]", "😀A", 1, false, false ),
				( @"(?i)[a]", "😀A", 0, true, true )
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
	public void CaselessLiteralsAndSetsAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"case {id}: {compiled.Diagnostic}" );
		var nativePolicies = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start, defaults, ucp );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = nativePolicies[mask];
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "é", "É", false, false )]
	[InlineData( "é", "É", false, true )]
	[InlineData( "é", "É", true, false )]
	[InlineData( "K", "K", true, true )]
	[InlineData( "[a-z]", "K", true, true )]
	public void CallerCaselessOptionAgreesWithInlineNative( string source, string subject, bool utf, bool ucp ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf, Ucp = ucp, Caseless = true } );
		Assert.True( compiled.IsSuccess );
		var native = PcreNative16Oracle.ExecuteSequence( Words( "(?i)" + source ), Words( subject ), utf, 0, ucp: ucp );
		Assert.True( native.Kind is "M" or "N" );
		var match = compiled.Pattern!.Match( subject.AsMemory() );
		Assert.True( match.IsSuccess );
		Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
	}

	[Theory]
	[InlineData( "K", "K", false )]
	[InlineData( "K", "K", true )]
	public void RestrictedCaselessOptionAgreesWithInlineNative( string source, string subject, bool utf ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = true, Caseless = true, CaselessRestricted = true
		} );
		Assert.True( compiled.IsSuccess );
		var native = PcreNative16Oracle.ExecuteSequence( Words( "(?ir)" + source ), Words( subject ), utf, 0, ucp: true );
		Assert.True( native.Kind is "M" or "N" );
		var match = compiled.Pattern!.Match( subject.AsMemory() );
		Assert.True( match.IsSuccess );
		Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
	}

	[Theory]
	[InlineData( "(?i)é", "É" )]
	[InlineData( "(?i)[a-z]", "K" )]
	[InlineData( "(?i)𐐀", "𐐨" )]
	[InlineData( "(?i)[[:lower:]]", "A" )]
	public void ValidUnicodeCaselessMatchesAgreeAcrossManagedWidths( string source, string subject ) {
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
	[InlineData( false, 3 )]
	[InlineData( true, 4 )]
	public void FoldedBracketConsumesOneBoundedStep( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( "(?i)[k]".AsMemory(), new() {
				Utf16 = utf, Ucp = true, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var match = pattern.Match( "K".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), match.Match?.Captures[0] ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void FoldedSourceAndExpandedRangeBoundsRemainExplicit() {
		var allowed = "(?i)[" + new string( 'a', 63 ) + @"\p{Lu}]";
		var rejected = "(?i)[" + new string( 'a', 64 ) + @"\p{Lu}]";
		Assert.Equal( new PcreWordCapture( 0, 1 ),
			PcreWordSequenceCompiler.CompileSequence( allowed.AsMemory(), new() { Utf16 = true } ).Pattern!.Match( "A".AsMemory() ).Match?.Captures[0] );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( rejected.AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
