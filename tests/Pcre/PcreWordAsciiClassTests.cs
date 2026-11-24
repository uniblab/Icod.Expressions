using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordAsciiClassTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( @"\d", "x5y", 0, false ), ( @"\d", "x5y", 0, true ),
				( @"\D", "5Ā", 0, false ), ( @"\D", "5Ā", 0, true ),
				( @"\w", "é_", 0, false ), ( @"\w", "é_", 0, true ),
				( @"\W", "é_", 0, false ), ( @"\W", "é_", 0, true ),
				( @"\s", "x\ty", 0, false ), ( @"\s", "x\ty", 0, true ),
				( @"\S", " \u00A0", 0, false ), ( @"\S", " \u00A0", 0, true ),
				( @"[\dX]", "X", 0, false ), ( @"[\dX]", "X", 0, true ),
				( @"[^\W]", "é_", 0, false ), ( @"[^\W]", "é_", 0, true ),
				( @"[\s\d]", "Ā8", 0, false ), ( @"[\s\d]", "Ā8", 0, true ),
				( "[[:alpha:]]", "éB", 0, false ), ( "[[:alpha:]]", "éB", 0, true ),
				( "[[:^digit:]]", "8Ā", 0, false ), ( "[[:^digit:]]", "8Ā", 0, true ),
				( "[[:blank:]]", "\u00A0 ", 0, false ), ( "[[:blank:]]", "\u00A0 ", 0, true ),
				( "[[:word:]]", "é_", 0, false ), ( "[[:word:]]", "é_", 0, true ),
				( "[[:xdigit:]]", "gF", 0, false ), ( "[[:xdigit:]]", "gF", 0, true ),
				( @"[\D[:digit:]]", "é5", 0, false ), ( @"[\D[:digit:]]", "é5", 0, true ),
				( @"\d\w", "5_", 0, false ), ( @"\d\w", "5_", 0, true ),
				( "[^[:space:]]", "\tĀ", 0, false ), ( "[^[:space:]]", "\tĀ", 0, true ),
				( @"[\d]", "😀1", 1, false ), ( @"[\d]", "😀1", 0, true )
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
	public void AsciiClassesAgreeWithNativePolicies( int id, string source, string subject,
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
	[InlineData( @"\D", "é" )]
	[InlineData( @"[\w]", "x_" )]
	[InlineData( "[[:alpha:]]", "éB" )]
	[InlineData( @"[^\W]", "é_" )]
	public void ValidUnicodeAsciiClassesAgreeAcrossManagedWidths( string source, string subject ) {
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
	[InlineData( "alnum", "A" )]
	[InlineData( "alpha", "A" )]
	[InlineData( "ascii", "A" )]
	[InlineData( "blank", " " )]
	[InlineData( "cntrl", "\u0001" )]
	[InlineData( "digit", "2" )]
	[InlineData( "graph", "!" )]
	[InlineData( "lower", "a" )]
	[InlineData( "print", " " )]
	[InlineData( "punct", "!" )]
	[InlineData( "space", "\t" )]
	[InlineData( "upper", "A" )]
	[InlineData( "word", "_" )]
	[InlineData( "xdigit", "F" )]
	public void EachDefaultPosixNameUsesNativeAsciiMembership( string name, string member ) {
		var source = String.Concat( "[[:", name, ":]]" );
		foreach ( var utf in new[] { false, true } ) {
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf } );
			Assert.True( compiled.IsSuccess, $"{name}/{utf}: {compiled.Diagnostic}" );
			var pattern = compiled.Pattern!;
			foreach ( var subject in new[] { member, "é" } ) {
				var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, 0 );
				Assert.True( native.Kind is "M" or "N", $"native {name}/{utf}: {native}" );
				var match = pattern.Match( subject.AsMemory() );
				Assert.True( match.IsSuccess );
				Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
			}
		}
	}

	[Theory]
	[InlineData( false, 3 )]
	[InlineData( true, 4 )]
	public void StandaloneClassUsesOneBoundedComparison( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var steps = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( @"\w".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = steps.Add
			} ).Pattern!;
			var match = pattern.Match( "_".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), match.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), steps );
		}
	}

	[Fact]
	public void PredicateRangeBoundAndOtherPropertyAdmissionRemainExplicit() {
		var allowed = "[" + new string( 'a', 63 ) + @"\d]";
		var rejected = "[" + new string( 'a', 64 ) + @"\d]";
		Assert.Equal( new PcreWordCapture( 0, 1 ),
			PcreWordSequenceCompiler.CompileSequence( allowed.AsMemory() ).Pattern!.Match( "5".AsMemory() ).Match );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( rejected.AsMemory() ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( @"[\p{XUC}]".AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
