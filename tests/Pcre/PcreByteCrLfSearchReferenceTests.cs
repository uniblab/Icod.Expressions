using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins source class metadata and the bounded native byte prefix search consequence.</summary>
public sealed class PcreByteCrLfSearchReferenceTests {
	[Theory]
	[InlineData( @"[^\r]A", 0, "1:2" )]
	[InlineData( @"[^\r-\r]A", 0, "1:2" )]
	[InlineData( @"[^\x00]A", 0, "1:2" )]
	[InlineData( @"[\x00-\xff]A", 0, "1:2" )]
	[InlineData( @"[\s\S]A", 0, "1:2" )]
	[InlineData( @"[\D\d]A", 0, "1:2" )]
	[InlineData( @"(?i)[^\r]a", 0, "1:2" )]
	[InlineData( @"(?xx)[^ \r - \r ] A", 0, "1:2" )]
	[InlineData( "[^\\Q\r\\E]A", 0, "1:2" )]
	[InlineData( @"(?<x>[^\r])A", 0, "1:2,1:1" )]
	[InlineData( @"(?<x>[^\r]A)", 0, "1:2,1:2" )]
	[InlineData( @"(?:[^\r])(?:A)", 0, "1:2" )]
	[InlineData( @"(?<x>[^\r])(?<y>A)", 0, "1:2,1:1,2:1" )]
	[InlineData( @"[^\r\r]A", 0, "1:2" )]
	[InlineData( @"[^\r-\r\r-\r]A", 0, "1:2" )]
	[InlineData( @"[^\r\d]A", 0, "1:2" )]
	[InlineData( @"[^\d\r]A", 0, "1:2" )]
	[InlineData( @"[^\rA]A", 0, "1:2" )]
	[InlineData( @"[^[:digit:]\r]A", 0, "1:2" )]
	[InlineData( @"(?xx)[^ \r \r ] A", 0, "1:2" )]
	[InlineData( "[^\\Q\r\r\\E]A", 0, "1:2" )]
	[InlineData( @"[\x09-\x0e]A", 1, "1:2" )]
	[InlineData( @"[^\x00\x01]A", 0, "" )]
	[InlineData( @"[^\x00\x01]A", 1, "1:2" )]
	[InlineData( @"[^\r]\w", 0, "" )]
	[InlineData( @"[^\r]+A", 0, "" )]
	[InlineData( @"[^\r]{1,2}A", 0, "" )]
	[InlineData( @"[^\r]{2}A", 0, "" )]
	[InlineData( @"[\n-\r]A", 0, "1:2" )]
	[InlineData( @"[\x00-\n]A", 0, "1:2" )]
	[InlineData( @"[^\n-\r]A", 0, "" )]
	[InlineData( @"[^\r\r]{0}[\x09-\x0e]A", 0, "1:2" )]
	[InlineData( @"(?(DEFINE)(?<x>[^\r\r]))[\x09-\x0e]A", 0, "1:2,-" )]
	public void ByteClassSourceAndPrefixShapesPreserveLiteralSpans( string pattern, int offset, string spans ) {
		var subject = "\r\nA"u8.ToArray();
		var reference = Reference( pattern, true );
		AssertLiteral( reference, subject, offset, spans );
		Compare( reference, subject, offset, Compile( pattern, new() { AnyCrLfNewline = true } ).Match( subject, offset ), pattern );
	}

	[Theory]
	[InlineData( @"[\x09-\x0c]", "0D0A", "1:1" )]
	[InlineData( @"[\x09-\x0c]A", "0D0A41", "1:2" )]
	[InlineData( @"[\x09-\x0e]{2}A", "0D0A0B41", "1:3" )]
	[InlineData( @"[\x00-\xff]AA", "0D0A4141", "1:3" )]
	[InlineData( @"(?>[\x00-\xff]A)", "0D0A41", "1:2" )]
	public void InteriorRangesRetainPreviouslyCorrectNativeMatches( string pattern, string hex, string spans ) {
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern, true );
		AssertLiteral( reference, subject, 0, spans );
		Compare( reference, subject, 0, Compile( pattern, new() { AnyCrLfNewline = true } ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryCallerOffsetPreservesByteAndUtfControls( bool anyCrLf ) {
		foreach ( var utf in new[] { false, true } ) {
			foreach ( var pattern in new[] { @"[^\r]A", @"[^\r-\r]A", @"[^\x00]A", @"[\x00-\xff]A",
				@"[\s\S]A", @"[\x09-\x0c]A", @"[^\r\r]A", @"[^\d\r]A", @"(?<x>[^\r])(?<y>A)",
				@"\G[^\r]A", @"(?m)[^\r\r]A$", @"(?<x>[^\r\r]A)\k<x>", @"[^\r]+A", @"[^\r]\w" } ) {
				var reference = Reference( pattern, anyCrLf, utf, utf );
				var compiled = Compile( pattern, new() { AnyCrLfNewline = anyCrLf, Utf8 = utf, MatchInvalidUtf = utf } );
				foreach ( var hex in new[] { "0D0A41", "FF0D0A41", "0D0AFF41", "0D0A410D0A41", "C3A90D0A41610D0A41" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, utf={utf}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( @"[^\r]A" )]
	[InlineData( @"(?<x>[^\r])(?<y>A)" )]
	[InlineData( @"[^\r\d]A" )]
	[InlineData( @"[\x09-\x0c]A" )]
	public void ObservedAndCancellableSearchPreservesTheSameNativeResult( string pattern ) {
		var subject = "\r\nA"u8.ToArray();
		var reference = Reference( pattern, true );
		foreach ( var observed in new[] { false, true } ) {
			using var cancellation = new CancellationTokenSource();
			var steps = new List<int>();
			var compiled = Compile( pattern, new() { AnyCrLfNewline = true,
				WorkObserver = observed ? steps.Add : null } );
			Compare( reference, subject, 0, compiled.Match( subject, cancellationToken: cancellation.Token ), pattern );
			if ( observed ) { Assert.Equal( Enumerable.Range( 1, steps.Count ), steps ); }
		}
	}

	[Fact]
	public void PrefixContinuationSharesWorkExhaustionAndCancellation() {
		const string pattern = @"(?<x>[^\r])(?<y>A)";
		var subject = "\r\nA"u8.ToArray();
		var steps = new List<int>();
		var observed = Compile( pattern, new() { AnyCrLfNewline = true, WorkObserver = steps.Add } );
		var result = observed.Match( subject );
		Assert.True( result.IsSuccess );
		Assert.Equal( 1, result.Match?.ByteIndex );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		foreach ( var observe in new[] { false, true } ) {
			var exhausted = Compile( pattern, new() { AnyCrLfNewline = true, MatchWorkLimit = steps.Count - 1,
				WorkObserver = observe ? _ => { } : null } ).Match( subject );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { AnyCrLfNewline = true,
			WorkObserver = count => { if ( count == 3 ) cancellation.Cancel(); } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => observed.Match( subject, cancellationToken: cancellation.Token ) );
	}

	private static void AssertLiteral( PcreRegex8Bit reference, byte[] subject, int offset, string spans ) {
		var match = reference.Match( subject, offset );
		Assert.Equal( spans.Length > 0, match.Success );
		if ( !match.Success ) { return; }
		var groups = spans.Split( ',' );
		Assert.Equal( groups.Length, match.Groups.Count );
		for ( var group = 0; group < groups.Length; group++ ) {
			var capture = match.Groups[group];
			Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
		}
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}: native={ ( expected.Success ? expected.Index : -1 ) }, managed={actual.Match?.ByteIndex ?? -1}" );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool anyCrLf, bool utf = false, bool invalidUtf = false ) => new(
		Encoding.UTF8.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), Encoding.UTF8,
		new PcreRegexSettings { Options = PcreOptions.Compiled | ( utf ? PcreOptions.Utf : PcreOptions.None )
			| ( invalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
