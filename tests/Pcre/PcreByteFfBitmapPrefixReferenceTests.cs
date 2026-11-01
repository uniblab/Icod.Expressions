using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>The effective FF bit broadens native bitmap scanning independently of set cardinality.</summary>
public sealed class PcreByteFfBitmapPrefixReferenceTests {
	[Theory]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}A", "0D0AFF41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}?A", "0D0AFF41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}?A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}+A", "0D0AFF41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}+A", "0D0A4141", "0:3" )]
	[InlineData( @"(?<x>[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2})(A)", "0D0AFF41", "1:3,1:2,3:1" )]
	[InlineData( @"(?<x>[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2})(A)", "0D0A4141", "0:3,0:2,2:1" )]
	[InlineData( @"(?>[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2})A", "0D0AFF41", "1:3" )]
	[InlineData( @"(?>[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2})A", "0D0A4141", "0:3" )]
	[InlineData( @"(?:[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2})A", "0D0AFF41", "1:3" )]
	[InlineData( @"(?:[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2})A", "0D0A4141", "0:3" )]
	[InlineData( @"([^\x00-\x09\x0b-\x0c\x0e-\xfe]){2}A", "0D0AFF41", "" )]
	[InlineData( @"([^\x00-\x09\x0b-\x0c\x0e-\xfe]){2}A", "0D0A4141", "0:3,1:1" )]
	[InlineData( @"(?:[^\x00-\x09\x0b-\x0c\x0e-\xfe]){2}A", "0D0AFF41", "" )]
	[InlineData( @"(?:[^\x00-\x09\x0b-\x0c\x0e-\xfe]){2}A", "0D0A4141", "0:3" )]
	[InlineData( @"(?>[^\x00-\x09\x0b-\x0c\x0e-\xfe]){2}A", "0D0AFF41", "" )]
	[InlineData( @"(?>[^\x00-\x09\x0b-\x0c\x0e-\xfe]){2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{1}A", "0D0AFF41", "2:2" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{1}A", "0D0A4141", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]A", "0D0AFF41", "2:2" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]A", "0D0A4141", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{0}A", "0D0AFF41", "3:1" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{0}A", "0D0A4141", "2:1" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{1,2}A", "0D0AFF41", "2:2" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{1,2}A", "0D0A4141", "0:3" )]
	[InlineData( @"(?i)[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}a", "0D0AFF41", "1:3" )]
	[InlineData( @"(?i)[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}a", "0D0A4141", "0:3" )]
	[InlineData( @"(?ir)[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}a", "0D0AFF41", "1:3" )]
	[InlineData( @"(?ir)[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}a", "0D0A4141", "0:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0f-\xfe]{2}A", "0D0AFF41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0f-\xfe]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x10-\xfe]{2}A", "0D0AFF41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x10-\xfe]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x11-\xfe]{2}A", "0D0AFF41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x11-\xfe]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]A", "0D0A41", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{1}A", "0D0A41", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A", "0D0AFF41", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{11}A", "0D0AFFFFFFFFFFFFFFFFFFFF41", "1:12" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{12}A", "0D0AFFFFFFFFFFFFFFFFFFFFFF41", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}\C{9}A", "0D0AFFFFFFFFFFFFFFFFFFFF41", "1:12" )]
	[InlineData( @"[^\r][^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}\xFF", "0D0AFFFFFF", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}\x00", "0D0AFF00", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}\xFF", "0D0AFFFF", "0:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}\n", "0D0AFF0A", "1:3" )]
	public void EffectiveFfBitPreservesPinnedCaptureSpans( string body, string hex, string spans ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern );
		var expected = reference.Match( subject );
		Assert.Equal( spans.Length > 0, expected.Success );
		if ( expected.Success ) {
			var groups = spans.Split( ',' );
			Assert.Equal( groups.Length, expected.Groups.Count );
			for ( var group = 0; group < groups.Length; group++ ) {
				var capture = expected.Groups[group];
				Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
			}
		}
		Compare( reference, subject, 0, Compile( pattern ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( "(*LF)" )]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CR)" )]
	[InlineData( "(*NUL)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void EveryOffsetRetainsEffectiveFfAndGroupBoundaries( string newline ) {
		string[] bodies = { @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}?A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}+A", @"(?<x>[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2})(A)", @"(?>[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2})A", @"(?:[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2})A", @"([^\x00-\x09\x0b-\x0c\x0e-\xfe]){2}A", @"(?:[^\x00-\x09\x0b-\x0c\x0e-\xfe]){2}A", @"(?>[^\x00-\x09\x0b-\x0c\x0e-\xfe]){2}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{1}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{0}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{1,2}A", @"(?i)[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}a", @"(?ir)[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}a", @"[^\x00-\x09\x0b-\x0c\x0f-\xfe]{2}A", @"[^\x00-\x09\x0b-\x0c\x10-\xfe]{2}A", @"[^\x00-\x09\x0b-\x0c\x11-\xfe]{2}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{11}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{12}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}\C{9}A", @"[^\r][^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}\xFF" };
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var body in bodies ) {
				var pattern = String.Concat( newline, body );
				var reference = Reference( pattern, ucp );
				var compiled = Compile( pattern, new() { Ucp = ucp } );
				foreach ( var hex in Subjects() ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}/{ucp}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void UtfRepeatControlsRetainScalarAndInvalidFragmentRules( string newline ) {
		foreach ( var body in new[] { @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}A", @"(?:[^\x00-\x09\x0b-\x0c\x0e-\xfe]){2}A", @"[^\x00-\x09\x0b-\x0c\x0f-\xfe]{2}A", @"[^\x00-\x09\x0b-\x0c\x10-\xfe]{2}A" } ) {
			var pattern = String.Concat( newline, body );
			var reference = Reference( pattern, true, true );
			var compiled = Compile( pattern, new() { Utf8 = true, Ucp = true, MatchInvalidUtf = true } );
			foreach ( var hex in new[] { "0D0A4241", "0D0A4141", "C3A90D0A4241", "FF0D0A4241", "0D0AFF41", "EDA0800D0A41", "0D0AC3BF41", "0D0AC48041" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}A", "0D0AFF41" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0f-\xfe]{2}A", "0D0AFF41" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x10-\xfe]{2}A", "0D0AFF41" )]
	[InlineData( @"(?<x>[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2})(A)", "0D0AFF41" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{11}A", "0D0AFFFFFFFFFFFFFFFFFFFF41" )]
	[InlineData( @"(?>[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2})A", "0D0AFF41" )]
	public void RepeatContinuationSharesObservedExactAndExhaustedBudgets( string body, string hex ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		var steps = new List<int>();
		var result = Compile( pattern, new() { WorkObserver = steps.Add } ).Match( subject );
		Assert.True( result.IsSuccess );
		Assert.Equal( 1, result.Match?.ByteIndex );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		foreach ( var observe in new[] { false, true } ) {
			Compare( Reference( pattern ), subject, 0, Compile( pattern, new() { MatchWorkLimit = steps.Count,
				WorkObserver = observe ? _ => { } : null } ).Match( subject ), pattern );
			var exhausted = Compile( pattern, new() { MatchWorkLimit = steps.Count - 1,
				WorkObserver = observe ? _ => { } : null } ).Match( subject );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
	}

	[Fact]
	public void FfBitmapSelectorUsesExistingChargeAndCancellation() {
		const string pattern = @"(*ANYCRLF)[^\r][^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}\xFF";
		var subject = Convert.FromHexString( "0D0AFFFFFF" );
		// CR:3 VM+1 selector; remaining FF/FF/FF/end candidates:9+7+5+3.
		foreach ( var observed in new[] { false, true } ) {
			var steps = new List<int>();
			var exact = Compile( pattern, new() { MatchWorkLimit = 28, WorkObserver = observed ? steps.Add : null } ).Match( subject );
			Assert.True( exact.IsSuccess );
			Assert.Null( exact.Match );
			if ( observed ) { Assert.Equal( Enumerable.Range( 1, 28 ), steps ); }
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
				Compile( pattern, new() { MatchWorkLimit = 27, WorkObserver = observed ? _ => { } : null } ).Match( subject ).Diagnostic?.Code );
		}
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void DeepFfWrappersAndRepeatCeilingsRemainShared() {
		var wrapped = String.Concat( "(*ANYCRLF)", String.Concat( Enumerable.Repeat( "(?>", 200 ) ), @"(?<x>[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2})(A)", new string( ')', 200 ) );
		var subject = Convert.FromHexString( "0D0AFF41" );
		Compare( Reference( wrapped ), subject, 0, Compile( wrapped ).Match( subject ), wrapped );
		var depth = String.Concat( String.Concat( Enumerable.Repeat( "(?>", 257 ) ), @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{2}A", new string( ')', 257 ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( depth ) ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{4096}A" ) ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]A", "0D0A41", 11 )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfe]{1}A", "0D0A41", 11 )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A", "0D0AFF41", 22 )]
	public void BitmapEligibilityPreservesChargedWork( string body, string hex, int budget ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		foreach ( var observed in new[] { false, true } ) {
			var steps = new List<int>();
			var result = Compile( pattern, new() { MatchWorkLimit = budget, WorkObserver = observed ? steps.Add : null } ).Match( subject );
			Assert.True( result.IsSuccess );
			Assert.Null( result.Match );
			if ( observed ) { Assert.Equal( Enumerable.Range( 1, budget ), steps ); }
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
				Compile( pattern, new() { MatchWorkLimit = budget - 1, WorkObserver = observed ? _ => { } : null } ).Match( subject ).Diagnostic?.Code );
		}
	}

	private static string[] Subjects() => new[] { "", "00", "0D0A", "0D0A41", "0D0AFF41", "0D0A4141", "0D0AFE41", "FF0D0AFF41", "0D0AFFFFFF", "0D0AFF00", "C3BF0D0AFF41", "0D0AFF410D0A4141", "0D0AFFFFFFFFFFFFFFFFFFFF41", "0D0AFFFFFFFFFFFFFFFFFFFFFF41" };

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}: native={( expected.Success ? expected.Index : -1 )}, managed={actual.Match?.ByteIndex ?? -1}" );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool ucp = false, bool utf = false ) => new(
		Encoding.ASCII.GetBytes( pattern ), Encoding.Latin1, new PcreRegexSettings {
			Options = PcreOptions.Compiled | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( utf ? PcreOptions.Utf | PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
