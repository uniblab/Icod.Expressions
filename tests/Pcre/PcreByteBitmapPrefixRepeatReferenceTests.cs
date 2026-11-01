using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Broad byte bitmap exact repeats expose a final literal without admitting ordinary atoms.</summary>
public sealed class PcreByteBitmapPrefixRepeatReferenceTests {
	[Theory]
	[InlineData( @"[\D]{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"[\D]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[\H]{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"[\H]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\x00\x01]{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"[^\x00\x01]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[\D]{2}?A", "0D0A4241", "1:3" )]
	[InlineData( @"[\D]{2}?A", "0D0A4141", "0:3" )]
	[InlineData( @"[\D]{2}+A", "0D0A4241", "1:3" )]
	[InlineData( @"[\D]{2}+A", "0D0A4141", "0:3" )]
	[InlineData( @"(?<x>[\D]{2})(A)", "0D0A4241", "1:3,1:2,3:1" )]
	[InlineData( @"(?<x>[\D]{2})(A)", "0D0A4141", "0:3,0:2,2:1" )]
	[InlineData( @"(?>[\D]{2})A", "0D0A4241", "1:3" )]
	[InlineData( @"(?>[\D]{2})A", "0D0A4141", "0:3" )]
	[InlineData( @"(?:[\D]{2})A", "0D0A4241", "1:3" )]
	[InlineData( @"(?:[\D]{2})A", "0D0A4141", "0:3" )]
	[InlineData( @"([\D]){2}A", "0D0A4241", "" )]
	[InlineData( @"([\D]){2}A", "0D0A4141", "0:3,1:1" )]
	[InlineData( @"(?:[\D]){2}A", "0D0A4241", "" )]
	[InlineData( @"(?:[\D]){2}A", "0D0A4141", "0:3" )]
	[InlineData( @"(?>[\D]){2}A", "0D0A4241", "" )]
	[InlineData( @"(?>[\D]){2}A", "0D0A4141", "0:3" )]
	[InlineData( @"\H{2}A", "0D0A4241", "" )]
	[InlineData( @"\H{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[\D]{1}A", "0D0A4241", "2:2" )]
	[InlineData( @"[\D]{1}A", "0D0A4141", "2:2" )]
	[InlineData( @"[\D]A", "0D0A4241", "2:2" )]
	[InlineData( @"[\D]A", "0D0A4141", "2:2" )]
	[InlineData( @"[\D]{0}A", "0D0A4241", "3:1" )]
	[InlineData( @"[\D]{0}A", "0D0A4141", "2:1" )]
	[InlineData( @"[\D]{1,2}A", "0D0A4241", "2:2" )]
	[InlineData( @"[\D]{1,2}A", "0D0A4141", "0:3" )]
	[InlineData( @"(?i)[\D]{2}a", "0D0A4241", "1:3" )]
	[InlineData( @"(?i)[\D]{2}a", "0D0A4141", "0:3" )]
	[InlineData( @"(?ir)[\D]{2}a", "0D0A4241", "1:3" )]
	[InlineData( @"(?ir)[\D]{2}a", "0D0A4141", "0:3" )]
	[InlineData( @"[^[:digit:]]{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"[^[:digit:]]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[\s]{2}A", "0D0A0941", "1:3" )]
	[InlineData( @"[[:space:]]{2}A", "0D0A0941", "1:3" )]
	[InlineData( @"[\W]{2}A", "0D0A2341", "1:3" )]
	[InlineData( @"[\v]{2}A", "0D0A4241", "" )]
	[InlineData( @"[\D]A", "0D0A41", "" )]
	[InlineData( @"[\D]{1}A", "0D0A41", "" )]
	[InlineData( @"[\D]{11}A", "0D0A4242424242424242424241", "1:12" )]
	[InlineData( @"[\D]{12}A", "0D0A424242424242424242424241", "" )]
	[InlineData( @"[\D]{2}\C{9}A", "0D0A4242424242424242424241", "1:12" )]
	[InlineData( @"[^\r][\D]{2}A", "0D0A414141", "" )]
	[InlineData( @"[^\r][\D]{2}A", "0D0A424241", "1:4" )]
	[InlineData( @"[\D]{2}\x00", "0D0A4200", "1:3" )]
	[InlineData( @"[\D]{2}\xFF", "0D0A42FF", "1:3" )]
	[InlineData( @"[\D]{2}\n", "0D0A420A", "1:3" )]
	public void BitmapRepeatSourceShapesPreservePinnedCaptureSpans( string body, string hex, string spans ) {
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
	public void EveryOffsetRetainsBitmapTypeAndGroupBoundaries( string newline ) {
		string[] bodies = { @"[\D]{2}A", @"[\H]{2}A", @"[^\x00\x01]{2}A", @"[\D]{2}?A", @"[\D]{2}+A", @"(?<x>[\D]{2})(A)", @"(?>[\D]{2})A", @"(?:[\D]{2})A", @"([\D]){2}A", @"(?:[\D]){2}A", @"(?>[\D]){2}A", @"\H{2}A", @"[\D]{1}A", @"[\D]A", @"[\D]{0}A", @"[\D]{1,2}A", @"(?i)[\D]{2}a", @"(?ir)[\D]{2}a", @"[^[:digit:]]{2}A", @"[\s]{2}A", @"[[:space:]]{2}A", @"[\W]{2}A", @"[\v]{2}A", @"[\D]{11}A", @"[\D]{12}A", @"[^\r][\D]{2}A", @"(?a)[\D]{2}A", @"(?aP)[^[:digit:]]{2}A" };
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
		foreach ( var body in new[] { @"[\D]{2}A", @"[\s]{2}A", @"[^\x00\x01]{2}A", @"(?:[\D]){2}A" } ) {
			var pattern = String.Concat( newline, body );
			var reference = Reference( pattern, true, true );
			var compiled = Compile( pattern, new() { Utf8 = true, Ucp = true, MatchInvalidUtf = true } );
			foreach ( var hex in new[] { "0D0A4241", "0D0A4141", "C3A90D0A4241", "FF0D0A4241", "0D0AFF41", "EDA0800D0A41" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"[\D]{2}A", "0D0A4241" )]
	[InlineData( @"[\H]{2}A", "0D0A4241" )]
	[InlineData( @"[^\x00\x01]{2}A", "0D0A4241" )]
	[InlineData( @"(?<x>[\D]{2})(A)", "0D0A4241" )]
	[InlineData( @"[\D]{11}A", "0D0A4242424242424242424241" )]
	[InlineData( @"[\s]{2}A", "0D0A0941" )]
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
	public void BitmapRepeatSelectorUsesExistingChargeAndCancellation() {
		const string pattern = @"(*ANYCRLF)[^\r][\D]{2}A";
		var subject = "\r\nAAA"u8.ToArray();
		// CR:3 VM+1 selector; remaining A/A/A/end candidates:9+7+5+3.
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
	public void DeepBitmapWrappersAndRepeatCeilingsRemainShared() {
		var wrapped = String.Concat( "(*ANYCRLF)", String.Concat( Enumerable.Repeat( "(?>", 200 ) ), @"(?<x>[\D]{2})(A)", new string( ')', 200 ) );
		var subject = "\r\nBA"u8.ToArray();
		Compare( Reference( wrapped ), subject, 0, Compile( wrapped ).Match( subject ), wrapped );
		var depth = String.Concat( String.Concat( Enumerable.Repeat( "(?>", 257 ) ), @"[\D]{2}A", new string( ')', 257 ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( depth ) ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( @"[\D]{4096}A" ) ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( @"[\D]A", "0D0A41", 13 )]
	[InlineData( @"[\D]{1}A", "0D0A41", 13 )]
	[InlineData( @"[\v]{2}A", "0D0A4241", 22 )]
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

	private static string[] Subjects() => new[] { "", "00", "0D0A", "0D0A41", "0D0A4241", "0D0A4141", "0D0A4161",
		"FF0D0A4241", "0D0AFF41", "0D0A2341", "0D0A0941", "C3A90D0A4241", "0D0A42410D0A4141",
		"0D0A4242424242424242424241", "0D0A424242424242424242424241" };

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
