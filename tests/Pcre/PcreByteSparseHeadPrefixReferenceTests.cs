using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>A complete homogeneous sparse bitmap head uses the shared final-literal selector.</summary>
public sealed class PcreByteSparseHeadPrefixReferenceTests {
	[Theory]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{2}A", "0D0A0A41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{3}A", "0D0A0A0A41", "1:4" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{4}A", "0D0A0A0A0A41", "1:5" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{11}A", "0D0A0A0A0A0A0A0A0A0A0A0A41", "1:12" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{12}A", "0D0A0A0A0A0A0A0A0A0A0A0A0A41", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{2}?A", "0D0A0A41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{2}+A", "0D0A0A41", "1:3" )]
	[InlineData( @"(?<x>[^\x00-\x09\x0b-\x0c\x0e-\xff]{3})(A)", "0D0A0A0A41", "1:4,1:3,4:1" )]
	[InlineData( @"(?>[^\x00-\x09\x0b-\x0c\x0e-\xff]{3})A", "0D0A0A0A41", "1:4" )]
	[InlineData( @"([^\x00-\x09\x0b-\x0c\x0e-\xff]){2}A", "0D0A0A41", "" )]
	[InlineData( @"(?:[^\x00-\x09\x0b-\x0c\x0e-\xff]){2}A", "0D0A0A41", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{1}A", "0D0A41", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A", "0D0AFE41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{3}A", "0D0AFEFE41", "1:4" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{4}A", "0D0AFEFEFE41", "1:5" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{11}A", "0D0AFEFEFEFEFEFEFEFEFEFE41", "1:12" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{12}A", "0D0AFEFEFEFEFEFEFEFEFEFEFE41", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}?A", "0D0AFE41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}+A", "0D0AFE41", "1:3" )]
	[InlineData( @"(?<x>[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{3})(A)", "0D0AFEFE41", "1:4,1:3,4:1" )]
	[InlineData( @"(?>[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{3})A", "0D0AFEFE41", "1:4" )]
	[InlineData( @"([^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]){2}A", "0D0AFE41", "" )]
	[InlineData( @"(?:[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]){2}A", "0D0AFE41", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{1}A", "0D0A41", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\x00-\x09\x0e-\xff]{2}A", "0D0A0B41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0e-\xff]{3}A", "0D0A0B0B41", "1:4" )]
	[InlineData( @"[^\x00-\x09\x0e-\xff]{4}A", "0D0A0B0B0B41", "1:5" )]
	[InlineData( @"[^\x00-\x09\x0e-\xff]{11}A", "0D0A0B0B0B0B0B0B0B0B0B0B41", "1:12" )]
	[InlineData( @"[^\x00-\x09\x0e-\xff]{12}A", "0D0A0B0B0B0B0B0B0B0B0B0B0B41", "" )]
	[InlineData( @"[^\x00-\x09\x0e-\xff]{2}?A", "0D0A0B41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0e-\xff]{2}+A", "0D0A0B41", "1:3" )]
	[InlineData( @"(?<x>[^\x00-\x09\x0e-\xff]{3})(A)", "0D0A0B0B41", "1:4,1:3,4:1" )]
	[InlineData( @"(?>[^\x00-\x09\x0e-\xff]{3})A", "0D0A0B0B41", "1:4" )]
	[InlineData( @"([^\x00-\x09\x0e-\xff]){2}A", "0D0A0B41", "" )]
	[InlineData( @"(?:[^\x00-\x09\x0e-\xff]){2}A", "0D0A0B41", "" )]
	[InlineData( @"[^\x00-\x09\x0e-\xff]{1}A", "0D0A41", "" )]
	[InlineData( @"[^\x00-\x09\x0e-\xff]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[\v]{2}A", "0D0A0B41", "1:3" )]
	[InlineData( @"[\v]{3}A", "0D0A0B0B41", "1:4" )]
	[InlineData( @"[\v]{4}A", "0D0A0B0B0B41", "1:5" )]
	[InlineData( @"[\v]{11}A", "0D0A0B0B0B0B0B0B0B0B0B0B41", "1:12" )]
	[InlineData( @"[\v]{12}A", "0D0A0B0B0B0B0B0B0B0B0B0B0B41", "" )]
	[InlineData( @"[\v]{2}?A", "0D0A0B41", "1:3" )]
	[InlineData( @"[\v]{2}+A", "0D0A0B41", "1:3" )]
	[InlineData( @"(?<x>[\v]{3})(A)", "0D0A0B0B41", "1:4,1:3,4:1" )]
	[InlineData( @"(?>[\v]{3})A", "0D0A0B0B41", "1:4" )]
	[InlineData( @"([\v]){2}A", "0D0A0B41", "" )]
	[InlineData( @"(?:[\v]){2}A", "0D0A0B41", "" )]
	[InlineData( @"[\v]{1}A", "0D0A41", "" )]
	[InlineData( @"[\v]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{3}\n", "0D0A0A0A0A", "0:4" )]
	[InlineData( @"(?i)[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}a", "0D0AFE41", "1:3" )]
	[InlineData( @"(?ir)[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}a", "0D0AFE41", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}\x00", "0D0AFE00", "1:3" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}\xFF", "0D0AFEFF", "1:3" )]
	[InlineData( @"[^\r][^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A", "0D0A4241", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}\CA", "0D0A424241", "" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A", "0D0A4241", "" )]
	public void HomogeneousSparseHeadsPreservePinnedCaptureSpans( string body, string hex, string spans ) {
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
	public void EveryOffsetRetainsCompleteSparseHeadBoundaries( string newline ) {
		string[] bodies = { @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{2}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{3}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{4}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{11}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{12}A", @"(?<x>[^\x00-\x09\x0b-\x0c\x0e-\xff]{3})(A)", @"(?:[^\x00-\x09\x0b-\x0c\x0e-\xff]){2}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{1}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{3}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{4}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{11}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{12}A", @"(?<x>[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{3})(A)", @"(?:[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]){2}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{1}A", @"[^\x00-\x09\x0e-\xff]{2}A", @"[^\x00-\x09\x0e-\xff]{3}A", @"[^\x00-\x09\x0e-\xff]{4}A", @"[^\x00-\x09\x0e-\xff]{11}A", @"[^\x00-\x09\x0e-\xff]{12}A", @"(?<x>[^\x00-\x09\x0e-\xff]{3})(A)", @"(?:[^\x00-\x09\x0e-\xff]){2}A", @"[^\x00-\x09\x0e-\xff]{1}A", @"[\v]{2}A", @"[\v]{3}A", @"[\v]{4}A", @"[\v]{11}A", @"[\v]{12}A", @"(?<x>[\v]{3})(A)", @"(?:[\v]){2}A", @"[\v]{1}A" };
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
		foreach ( var body in new[] { @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{2}A", @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{3}A", @"[^\x00-\x09\x0e-\xff]{2}A", @"[\v]{2}A" } ) {
			var pattern = String.Concat( newline, body );
			var reference = Reference( pattern, true, true );
			var compiled = Compile( pattern, new() { Utf8 = true, Ucp = true, MatchInvalidUtf = true } );
			foreach ( var hex in new[] { "0D0A4241", "0D0A4141", "C3A90D0A4241", "FF0D0A4241", "0D0AFF41", "EDA0800D0A41", "0D0AC3BE41", "0D0AC48041" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xff]{2}A", "0D0A0A41" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A", "0D0AFE41" )]
	[InlineData( @"[^\x00-\x09\x0e-\xff]{3}A", "0D0A0B0B41" )]
	[InlineData( @"[\v]{4}A", "0D0A0B0B0B41" )]
	[InlineData( @"(?<x>[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{3})(A)", "0D0AFEFE41" )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{11}A", "0D0AFEFEFEFEFEFEFEFEFEFE41" )]
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
	public void SparseHeadSelectorSharesExactBudgetAndCancellation() {
		const string pattern = @"(*ANYCRLF)[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A";
		var subject = Convert.FromHexString( "0D0AFE41" );
		// Failed CR:7 VM+1 selector; successful LF candidate:8.
		foreach ( var observed in new[] { false, true } ) {
			var steps = new List<int>();
			var result = Compile( pattern, new() { MatchWorkLimit = 16, WorkObserver = observed ? steps.Add : null } ).Match( subject );
			Compare( Reference( pattern ), subject, 0, result, pattern );
			Assert.Equal( 1, result.Match?.ByteIndex );
			if ( observed ) { Assert.Equal( Enumerable.Range( 1, 16 ), steps ); }
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
				Compile( pattern, new() { MatchWorkLimit = 15, WorkObserver = observed ? _ => { } : null } ).Match( subject ).Diagnostic?.Code );
		}
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { WorkObserver = count => { if ( count == 8 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void DeepSparseHeadWrappersAndRepeatCeilingsRemainShared() {
		var wrapped = String.Concat( "(*ANYCRLF)", String.Concat( Enumerable.Repeat( "(?>", 200 ) ), @"(?<x>[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2})(A)", new string( ')', 200 ) );
		var subject = Convert.FromHexString( "0D0AFE41" );
		Compare( Reference( wrapped ), subject, 0, Compile( wrapped ).Match( subject ), wrapped );
		var depth = String.Concat( String.Concat( Enumerable.Repeat( "(?>", 257 ) ), @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A", new string( ')', 257 ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( depth ) ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{4096}A" ) ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( @"[^\r][^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A", "0D0A4241", 16 )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}\CA", "0D0A424241", 21 )]
	[InlineData( @"[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}[^\x00-\x09\x0b-\x0c\x0e-\xfd\xff]{2}A", "0D0A4241", 16 )]
	public void MixedSparsePrefixesPreservePriorWork( string body, string hex, int budget ) {
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

	private static string[] Subjects() => new[] { "", "00", "0D0A", "0D0A41", "0D0A0A41", "0D0AFE41", "0D0A0B41", "0D0A4141", "0D0A0A0A41", "0D0A0A0A0A41", "0D0AFEFE41", "FF0D0AFE41", "C3BE0D0AFE41", "0D0AFEFEFEFEFEFEFEFEFEFE41", "0D0AFEFEFEFEFEFEFEFEFEFEFE41" };

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
