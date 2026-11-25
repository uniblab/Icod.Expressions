using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordBoundaryTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( @"\b", "", 0, false, false ),
				( @"\B", "", 0, true, false ),
				( @"\b", "a", 0, false, false ),
				( @"\B", "a", 0, true, false ),
				( @"\b", "!", 0, true, false ),
				( @"\B", "!", 0, false, false ),
				( @"\ba\b", " a ", 0, false, false ),
				( @"\ba\B", "ab", 0, true, false ),
				( @"a\Bb", "ab", 0, false, false ),
				( @"a\bb", "ab", 0, true, false ),
				( @"\b_9\b", "!_9!", 0, false, false ),
				( @"\b", "a", 1, true, false ),
				( @"\B", "!", 1, false, false ),
				( @"\bX", "aX", 1, true, false ),
				( @"\BX", "aX", 1, false, false ),
				( @"\bX", "!X", 1, true, false ),
				( @"\bX", "\0X", 0, false, false ),
				( @"\bé\b", "é", 0, true, false ),
				( @"\bé\b", "é", 0, true, true ),
				( @"\bé\b", "é", 0, false, true ),
				( @"\Ba\B", "éaé", 1, true, true ),
				( @"\b\w+\b", " α\u0301_٥ ", 0, true, true ),
				( @"a\B\x{301}\b", "a\u0301", 0, true, true ),
				( @"\b\x{203F}\b", "\u203F", 0, true, true ),
				( @"\b𐐀\b", "!𐐀!", 0, true, true ),
				( @"\B𐐀\B", "!𐐀!", 0, true, false ),
				( @"\BX", "𐐀X", 2, true, true ),
				( @"\bX", "𐐀X", 2, true, false ),
				( @"\B😀\B", "😀", 0, true, true ),
				( @"\B\x{D800}\B", "\uD800", 0, false, true ),
				( @"(?aW)\bé\b", "é", 0, true, true ),
				( @"(?i)\bé\b", "É", 0, true, true ),
				( @"\b|a", "a", 0, true, false ),
				( @"\B|!", "!", 0, false, false ),
				( @"a*\ba", "aaa", 0, true, false ),
				( @"a*?\Ba", "aaa", 0, false, false ),
				( @"a*b?\Bc*d*e*\b", "aaabccdde", 0, true, false ),
				( @"a*?b??\Bc*?d*?e*?\b", "aaabccdde", 0, false, false ),
				( @"\Aa?\ba*\z", "aaa", 0, true, false ),
				( @"[\b]\B", "\b!", 0, false, false )
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
	public void BoundariesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"boundary {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( @"\b\w+\b", "!𐐀α\u0301!" )]
	[InlineData( @"a*?\Ba", "aaa" )]
	[InlineData( @"a*b?\Bc*d*e*\b", "aaabccdde" )]
	public void ValidUnicodeBoundariesAgreeAcrossManagedWidths( string source, string subject ) {
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
	[InlineData( @"\bX", "\uD800X", 1, 0 )]
	[InlineData( @"\BX", "\uDC00X", 1, 0 )]
	[InlineData( @"X|\b", "\uD800X", 1, 0 )]
	[InlineData( @"\AX|X", "\uD800X", 1, 0 )]
	[InlineData( @"\bX", "\uDC00\uDC00X", 2, 0 )]
	[InlineData( @"\b", "\uD800", 1, 0 )]
	public void StrictValidationIncludesNativeAssertionLookbehind( string source, string subject, int start, int invalid ) {
		Assert.Equal( "X", PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, (uint)start ).Kind );
		var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, invalid ),
			pattern.Match( subject.AsMemory(), start ).Diagnostic );
	}

	[Theory]
	[InlineData( @"\BX", "\uD800aX", 2 )]
	[InlineData( @"X|\A", "\uD800aX", 2 )]
	[InlineData( "X", "\uD800X", 1 )]
	public void EarlierMalformedPrefixStaysOutsideValidationWindow( string source, string subject, int start ) {
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, (uint)start );
		Assert.Equal( "M", native.Kind );
		var match = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } )
			.Pattern!.Match( subject.AsMemory(), start );
		Assert.True( match.IsSuccess );
		Assert.Equal( new PcreWordCapture( start, 1 ), match.Match );
	}

	[Theory]
	[InlineData( false, 8 )]
	[InlineData( true, 10 )]
	public void BoundaryReadsShareWorkQuotaAndCancellation( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( @"\ba\B".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add,
				Policy = new() { Anchored = true }
			} ).Pattern!;
			var match = pattern.Match( "ab".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), match.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
		using var cancellation = new CancellationTokenSource();
		var cancelled = PcreWordSequenceCompiler.CompileSequence( @"\ba\B".AsMemory(), new() {
			Utf16 = utf, WorkObserver = work => { if ( work == required - 1 ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => cancelled.Match( "ab".AsMemory(), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void CallerAsciiWordRestrictionMatchesNativeInlineRestriction() {
		foreach ( var source in new[] { @"\bé\b", @"\Bé\B", @"\bX", @"\BX" } ) {
			var subject = "éX";
			var native = PcreNative16Oracle.ExecuteSequence( Words( "(?aW)" + source ), Words( subject ), true, 0, ucp: true );
			Assert.True( native.Kind is "M" or "N" );
			var match = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				Utf16 = true, Ucp = true, AsciiBsW = true
			} ).Pattern!.Match( subject.AsMemory() );
			Assert.True( match.IsSuccess );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Fact]
	public void MalformedValidationRewindIsBudgeted() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( @"\bX".AsMemory(), new() {
			Utf16 = true, MatchWorkLimit = 2
		} ).Pattern!;
		var subject = String.Concat( new string( '\uDC00', 1000 ), "X" );
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded,
			pattern.Match( subject.AsMemory(), 1000 ).Diagnostic?.Code );
	}

	[Fact]
	public void BoundaryUsesTheSuppliedViewAndRejectsSplitScalarStarts() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( @"\bX".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( new PcreWordCapture( 0, 1 ), pattern.Match( "aXb".AsMemory( 1, 1 ) ).Match );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidStartOffset, 1 ),
			pattern.Match( "😀X".AsMemory(), 1 ).Diagnostic );
		Assert.Equal( "X", PcreNative16Oracle.ExecuteSequence( Words( @"\bX" ), Words( "😀X" ), true, 1 ).Kind );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
