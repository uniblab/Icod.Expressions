using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Byte class repeat scanning follows native source encoding, not just membership.</summary>
public sealed class PcreByteClassPrefixRepeatReferenceTests {
	[Theory]
	[InlineData( @"[^\r]{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"[^\r]{2}A", "0D0A4141", "" )]
	[InlineData( @"[^\r-\r]{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"[^\r-\r]{2}A", "0D0A4141", "" )]
	[InlineData( @"[^\r]{2}?A", "0D0A4241", "1:3" )]
	[InlineData( @"[^\r]{2}?A", "0D0A4141", "" )]
	[InlineData( @"[^\r]{2}+A", "0D0A4241", "1:3" )]
	[InlineData( @"[^\r]{2}+A", "0D0A4141", "" )]
	[InlineData( @"(?>[^\r]{2}A)", "0D0A4241", "1:3" )]
	[InlineData( @"(?>[^\r]{2}A)", "0D0A4141", "" )]
	[InlineData( @"(?<x>[^\r]{2})(A)", "0D0A4241", "1:3,1:2,3:1" )]
	[InlineData( @"(?<x>[^\r]{2})(A)", "0D0A4141", "" )]
	[InlineData( @"(?>([^\r]{2}))(A)", "0D0A4241", "1:3,1:2,3:1" )]
	[InlineData( @"(?>([^\r]{2}))(A)", "0D0A4141", "" )]
	[InlineData( @"[\s\S]{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"[\s\S]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"\p{Any}{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"\p{Any}{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"(?s).{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"(?s).{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"(?s:.{2})A", "0D0A4241", "1:3" )]
	[InlineData( @"(?s:.{2})A", "0D0A4141", "0:3" )]
	[InlineData( @"\D{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"\D{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"\D{2}?A", "0D0A4241", "1:3" )]
	[InlineData( @"\D{2}?A", "0D0A4141", "0:3" )]
	[InlineData( @"\D{2}+A", "0D0A4241", "1:3" )]
	[InlineData( @"\D{2}+A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\r\r]{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"[^\r\r]{2}A", "0D0A4141", "1:3" )]
	[InlineData( @"[\p{L}\P{L}]{2}A", "0D0A4241", "" )]
	[InlineData( @"[\p{L}\P{L}]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"(?i)[\p{L}\P{L}]{2}a", "0D0A4241", "" )]
	[InlineData( @"(?i)[\p{L}\P{L}]{2}a", "0D0A4141", "0:3" )]
	[InlineData( @"[^\r]{1}A", "0D0A41", "1:2" )]
	[InlineData( @"[\p{L}\P{L}]{1}A", "0D0A41", "1:2" )]
	[InlineData( @"[\s\S]{11}A", "0D0A4242424242424242424241", "1:12" )]
	[InlineData( @"[^\r]{11}A", "0D0A4242424242424242424241", "1:12" )]
	[InlineData( @"[^\r]{11}A", "0D0A4242424242424242424141", "" )]
	[InlineData( @"[^\r]{12}A", "0D0A424242424242424242424241", "" )]
	[InlineData( @"[^\r]{2}\C{9}A", "0D0A4242424242424242424241", "1:12" )]
	[InlineData( @"\P{L}{2}A", "0D0A2341", "1:3" )]
	[InlineData( @"\P{L}{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"\s{2}A", "0D0A0941", "1:3" )]
	[InlineData( @"\S{2}A", "0D0A4241", "" )]
	[InlineData( @"\W{2}A", "0D0A2341", "1:3" )]
	[InlineData( @"\w{2}A", "0D0A4241", "" )]
	[InlineData( @"(?a)\D{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"(*UCP)(?a)[\s\S]{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"(*UCP)[\s\S]{2}A", "0D0A4241", "" )]
	[InlineData( @"(?i)[^\r]{2}a", "0D0A6141", "" )]
	[InlineData( @"(?ir)[^\r]{2}a", "0D0A6141", "" )]
	[InlineData( @"(?i:[^\r]{2})a", "0D0A4161", "1:3" )]
	[InlineData( @"[^\r]{2}(?i:a)", "0D0A4161", "" )]
	[InlineData( @"[^\r]{2}\x00", "0D0A4200", "1:3" )]
	[InlineData( @"[^\r]{2}\xFF", "0D0A42FF", "1:3" )]
	[InlineData( @"[^\r]{2}\n", "0D0A420A", "1:3" )]
	[InlineData( @"[^\r]{0}A", "0D0A4241", "3:1" )]
	[InlineData( @"[^\r]{1,2}A", "0D0A4241", "2:2" )]
	[InlineData( @"[\P{L}]{2}A", "0D0A2341", "" )]
	[InlineData( @"[^\x00\x01]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"[^\r]{2}A(?!)", "0D0A4241", "" )]
	[InlineData( @"(*CRLF)\N{2}A", "0D0A4241", "1:3" )]
	public void SourceProvenancePreservesPinnedRepeatCaptureSpans( string body, string hex, string spans ) {
		var pattern = "(*ANYCRLF)" + body;
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
	public void EveryOffsetPreservesDirectTypesAndPropertyBracketRepeatBoundaries( string newline ) {
		string[] bodies = { @"[^\r]{2}A", @"[^\r-\r]{2}A", @"(?>[^\r]{2}A)", @"(?<x>[^\r]{2})(A)",
			@"[^\r]{2}?A", @"[^\r]{2}+A", @"[\s\S]{2}A", @"\p{Any}{2}A", @"\P{L}{2}A", @"\D{2}A",
			@"\s{2}A", @"\W{2}A", @"(?a)\D{2}A", @"(?a)[\s\S]{2}A", @"(?s).{2}A", @"\N{2}A",
			@"[^\r]{11}A", @"[^\r]{12}A", @"[^\r]{2}\C{9}A", @"[\p{L}\P{L}]{1}A",
			@"[\p{L}\P{L}]{2}A", @"[^\r\r]{2}A", @"(?i:[^\r]{2})a", @"[^\r]{2}(?i:a)" };
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var body in bodies ) {
				var pattern = newline + body;
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
		foreach ( var body in new[] { @"[^\r]{2}A", @"(?>[^\r]{2}A)", @"\p{Any}{2}A", @"(?s).{2}A", @"\D{2}A" } ) {
			var pattern = newline + body;
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
	[InlineData( @"[^\r]{2}A", "0D0A4241" )]
	[InlineData( @"(?>[^\r]{2}A)", "0D0A4241" )]
	[InlineData( @"(?<x>[^\r]{2})(A)", "0D0A4241" )]
	[InlineData( @"[^\r]{11}A", "0D0A4242424242424242424241" )]
	[InlineData( @"\p{Any}{2}A", "0D0A4241" )]
	[InlineData( @"(?s).{2}A", "0D0A4241" )]
	public void RepeatContinuationSharesObservedExactAndExhaustedBudgets( string body, string hex ) {
		var pattern = "(*ANYCRLF)" + body;
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
	public void RepeatedClassSelectorUsesExistingChargeAndCancellation() {
		const string pattern = @"(*ANYCRLF)[^\r]{2}A";
		var subject = "\r\nAA"u8.ToArray();
		// CR:3 VM+1 selector; remaining A/A/end candidates:7+5+3.
		foreach ( var observed in new[] { false, true } ) {
			var steps = new List<int>();
			var exact = Compile( pattern, new() { MatchWorkLimit = 19, WorkObserver = observed ? steps.Add : null } ).Match( subject );
			Assert.True( exact.IsSuccess );
			Assert.Null( exact.Match );
			if ( observed ) { Assert.Equal( Enumerable.Range( 1, 19 ), steps ); }
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
				Compile( pattern, new() { MatchWorkLimit = 18, WorkObserver = observed ? _ => { } : null } ).Match( subject ).Diagnostic?.Code );
		}
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void DeepWrappersAndClassRepeatCeilingsRemainShared() {
		var wrapped = "(*ANYCRLF)" + String.Concat( Enumerable.Repeat( "(?>", 200 ) ) + @"(?<x>[^\r]{2})(A)" + new string( ')', 200 );
		var subject = "\r\nBA"u8.ToArray();
		Compare( Reference( wrapped ), subject, 0, Compile( wrapped ).Match( subject ), wrapped );
		var depth = String.Concat( Enumerable.Repeat( "(?>", 257 ) ) + @"[^\r]{2}A" + new string( ')', 257 );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( depth ) ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( @"[^\r]{4096}A" ) ).Diagnostic?.Code );
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
