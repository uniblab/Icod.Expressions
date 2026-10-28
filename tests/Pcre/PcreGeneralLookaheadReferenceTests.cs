using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Atomic assertion transactions against test-only PCRE.NET 1.6.0 / PCRE2 10.48.</summary>
public sealed class PcreGeneralLookaheadReferenceTests {
	[Theory]
	[InlineData( "^(?=[ab]+c)[a-z]+$", "616263", 0, 3 )]
	[InlineData( "^(?=[ab]+c)[a-z]+$", "616264", -1, 0 )]
	[InlineData( "^(?![ab]+c)[a-z]+$", "616264", 0, 3 )]
	[InlineData( "^(?![ab]+c)[a-z]+$", "616263", -1, 0 )]
	[InlineData( "^(?=a{2,3}b)a+b$", "61616162", 0, 4 )]
	[InlineData( "^(?=a+?b)a+b$", "61616162", 0, 4 )]
	[InlineData( "^(?=a++b)a+b$", "61616162", 0, 4 )]
	[InlineData( "^(?=(?>a|ab)c)abc$", "616263", -1, 0 )]
	[InlineData( "^(?=(?>ab|a)c)abc$", "616263", 0, 3 )]
	[InlineData( "^(?=(?:a|ab)c)abc$", "616263", 0, 3 )]
	[InlineData( "^(?=(?![b])[a])a$", "61", 0, 1 )]
	[InlineData( "^(?!(?=[a])[b])a$", "61", 0, 1 )]
	[InlineData( "^(?!(?=[a])[a])a$", "61", -1, 0 )]
	[InlineData( "^(?=(?=[a])[a])a$", "61", 0, 1 )]
	[InlineData( @"^(?=\b[a]\b)a$", "61", 0, 1 )]
	[InlineData( "^(?![a]?)b$", "62", -1, 0 )]
	[InlineData( "^(?![a]+)$", "", 0, 0 )]
	[InlineData( "^(?:(?=[a])a)+$", "616161", 0, 3 )]
	[InlineData( "^(?:(?![b])a){2,3}$", "616161", 0, 3 )]
	[InlineData( "^(?:(?=[a]))*a$", "61", 0, 1 )]
	[InlineData( "^(?:(?![b]))+a$", "61", 0, 1 )]
	[InlineData( "^(?=(?i)[a]+)a+$", "4141", -1, 0 )]
	[InlineData( "^(?=(?i)[a]+)[A]+$", "4141", 0, 2 )]
	[InlineData( "^(?i:(?=(?-i:[a]))a)$", "41", -1, 0 )]
	[InlineData( "^(?=(?s).+)..$", "610A", -1, 0 )]
	[InlineData( "^(?=(?s).+)(?s:..)$", "610A", 0, 2 )]
	[InlineData( "(?=(?m)^[a]$)a", "0A610A", 1, 1 )]
	[InlineData( "^(?=(?x) [a] # body comment\n b )ab$", "6162", 0, 2 )]
	[InlineData( "^(?=(?xx)[ a ] (?# ignored) b)ab$", "6162", 0, 2 )]
	[InlineData( @"^(?=[\x00\xFF]+)\x00\xFF$", "00FF", 0, 2 )]
	public void GeneralBodiesKeepOrderedAtomicMatchOutcomes( string pattern, string hex,
		int expectedIndex, int expectedLength ) =>
		AssertOutcome( pattern, hex, false, 0, expectedIndex, expectedLength );

	[Theory]
	[InlineData( @"^(?=(a|ab))\1c$", "616263", -1, 0, "" )]
	[InlineData( @"^(?=(ab|a))\1c$", "616263", 0, 3, "0:3,0:2" )]
	[InlineData( @"^(?=(a|ab)c)\1c$", "616263", 0, 3, "0:3,0:2" )]
	[InlineData( @"^(?=(a+))a\1$", "6161", -1, 0, "" )]
	[InlineData( @"^(?=(a+?))a\1$", "6161", 0, 2, "0:2,0:1" )]
	[InlineData( "(?=(abc))a", "616263", 0, 1, "0:1,0:3" )]
	[InlineData( @"^(?=(?!(a)b)(a))\2$", "61", 0, 1, "0:1,-,0:1" )]
	[InlineData( "^(?!(?=(a))b)a$", "61", 0, 1, "0:1,-" )]
	[InlineData( "^(?=(a))b|a$", "61", 0, 1, "0:1,-" )]
	[InlineData( "^(?:(?=(a)b)a|a)$", "61", 0, 1, "0:1,-" )]
	[InlineData( "^(?!(a)b|(a)c)a$", "61", 0, 1, "0:1,-,-" )]
	[InlineData( "^(?:(?!(a))b|a)$", "61", 0, 1, "0:1,-" )]
	[InlineData( "^(?:(?=(?>(a)))b|a)$", "61", 0, 1, "0:1,-" )]
	[InlineData( @"^(?:(?=(.))a|b)+\1$", "61626261", 0, 4, "0:4,0:1" )]
	[InlineData( @"^(?|(?=(a))a|(?!(b)c)b)+\1$", "616261", 0, 3, "0:3,0:1" )]
	[InlineData( @"^(?|(?=(a))a|(?!(?=(b))c)b)+\1$", "616261", 0, 3, "0:3,0:1" )]
	[InlineData( @"^(?|(?=(a))a|(?!(?>(b))c)b)+\1$", "616261", 0, 3, "0:3,0:1" )]
	[InlineData( @"^(?:(?=(a))a)+a\1$", "616161", 0, 3, "0:3,0:1" )]
	[InlineData( @"^(?=(?<x>a+))\k<x>$", "6161", 0, 2, "0:2,0:2" )]
	[InlineData( @"^(?<x>a)(?=\k<x>[b])ab$", "616162", 0, 3, "0:3,0:1" )]
	[InlineData( @"^(?=(?|(a)|(b)))\g{1}$", "62", 0, 1, "0:1,0:1" )]
	[InlineData( @"^(?=(?|(?<x>a)|(?<x>b)))\g{x}$", "62", 0, 1, "0:1,0:1" )]
	[InlineData( @"(?n)^(?=(a)(?<x>b))ab\k<x>$", "616262", 0, 3, "0:3,1:1" )]
	[InlineData( @"^(?=(?n)(a)(?<x>b))ab\k<x>(c)$", "61626263", 0, 4, "0:4,1:1,3:1" )]
	[InlineData( @"^(?=(a)\g{-1})aa$", "6161", 0, 2, "0:2,0:1" )]
	[InlineData( @"^(?=(\\K))\\K$", "5C4B", 0, 2, "0:2,0:2" )]
	[InlineData( @"^(?=(\Q\K\E))\\K$", "5C4B", 0, 2, "0:2,0:2" )]
	[InlineData( @"^(?!(\Q\K\E))a$", "61", 0, 1, "0:1,-" )]
	public void AssertionsKeepOrRestoreEveryCaptureTransaction( string pattern, string hex,
		int expectedIndex, int expectedLength, string expectedCaptures ) =>
		AssertOutcome( pattern, hex, false, 0, expectedIndex, expectedLength, expectedCaptures );

	[Theory]
	[InlineData( @"^(?=([é😀]+))\1$", "C3A9F09F9880", 0, 6, "0:6,0:6" )]
	[InlineData( @"^(?i)(?=([é]+))\1$", "C389C3A9", 0, 4, "0:4,0:4" )]
	[InlineData( @"^(?i)(?=(K))k$", "6B", 0, 1, "0:1,0:1" )]
	[InlineData( @"^(?i)(?=(𐐀))𐐨$", "F09090A8", 0, 4, "0:4,0:4" )]
	[InlineData( "(?=(é😀))é", "C3A9F09F9880", 0, 2, "0:2,0:6" )]
	[InlineData( "^(?![é😀]+$).+$", "C3A961", 0, 3, "0:3" )]
	[InlineData( @"^(?=(?<字>😀))\k<字>$", "F09F9880", 0, 4, "0:4,0:4" )]
	public void UtfScalarsAndCaselessAssertionsCaptureSourceByteSpans( string pattern, string hex,
		int expectedIndex, int expectedLength, string expectedCaptures ) =>
		AssertOutcome( pattern, hex, true, 0, expectedIndex, expectedLength, expectedCaptures );

	[Theory]
	[InlineData( @"(?=\G[a])a", "6261", 0, -1, 0 )]
	[InlineData( @"(?=\G[a])a", "6261", 1, 1, 1 )]
	[InlineData( @"(?=a\G)a", "61", 0, -1, 0 )]
	[InlineData( @"(?!\G[a])a", "6261", 0, 1, 1 )]
	[InlineData( @"(?!\G[a])a", "6261", 1, -1, 0 )]
	[InlineData( @"(?=(\G[a]))a", "7861", 1, 1, 1 )]
	public void AssertionsRetainTheOriginalCallerStart( string pattern, string hex, int offset,
		int expectedIndex, int expectedLength ) =>
		AssertOutcome( pattern, hex, false, offset, expectedIndex, expectedLength );

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void EverySelectedOffsetKeepsOriginalFragmentsAndNewlineConvention( bool utf8, bool anyCrLf ) {
		// Empty branches and successful negatives must remain eligible at malformed/continuation bytes.
		// Consuming bodies and references still cannot cross a malformed UTF fragment barrier.
		foreach ( var pattern in new[] { @"(?=[a]?)", @"(?![a])", @"(?=([a]?))\1", @"(?=([a]+))\1",
			@"(?=(.))\1", @"(?=a.b)a", @"(?!a.b)a", @"(?=(a?))", @"(?!(a))",
			@"(?=(?:[a]|))", @"(?=\G[a]?)", @"(?!\G[a])", @"(?=[a]$)a", @"(?m)(?=^([a]))a",
			@"(?=(\R))\1", @"(?=\N+).", @"(?=(?<=a)[b])b", @"(?=())", @"(?!a)", @"(?!(?=a))",
			@"(?=(?:a|))", @"(?=(a))", @"(?=\G)(?!a)" } ) {
			var reference = Reference( pattern, utf8, utf8, anyCrLf );
			var compiled = Compile( pattern, new PcreByteOptions {
				Utf8 = utf8, MatchInvalidUtf = utf8, AnyCrLfNewline = anyCrLf
			} );
			foreach ( var hex in new[] { "", "61", "6162", "FF", "80FF", "61FF62", "61FF61", "C3A9",
				"61C3A962", "C3FF61", "F09F9880FF61", "0D0A61", "610D0A", "61FF", "61FFFF62", "C3A961",
				"8061", "E28261" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ),
						$"{pattern}, {hex}, offset {offset}, UTF={utf8}, ANYCRLF={anyCrLf}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"(?=\K)" )]
	[InlineData( @"(?!\K)" )]
	[InlineData( @"(?=(?:[a]\K))" )]
	[InlineData( @"(?!(?>[a]\K))" )]
	[InlineData( @"(?=(?=[a])\K)" )]
	[InlineData( @"(?!(?=[a])(?:\K))" )]
	public void ResetStartIsInvalidThroughoutTheActiveAssertion( string pattern ) {
		foreach ( var utf8 in new[] { false, true } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern, utf8 ) );
			AssertDiagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, 0, utf8 );
		}
	}

	[Theory]
	[InlineData( "(?=[a])*" )]
	[InlineData( "(?=[a])+" )]
	[InlineData( "(?=[a])?" )]
	[InlineData( "(?=[a]){0}" )]
	[InlineData( "(?![a]){1,2}" )]
	public void DirectAssertionQuantifiersRemainUnsupportedAtTheQuantifier( string pattern ) {
		Reference( pattern ); // PCRE2 gives these special rules, outside the ordinary repeat contract.
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, 7 );
	}

	[Theory]
	[InlineData( "(?<=[a])b" )]
	[InlineData( "(?<!a+)b" )]
	[InlineData( "(*pla:[a])" )]
	[InlineData( "(?*[a])" )]
	[InlineData( "(?(?=[a])a|b)" )]
	public void BroaderLookbehindAliasesAndConditionsRemainPending( string pattern ) =>
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, 0 );

	[Fact]
	public void GeneralAssertionsKeepTheHeapParserDepthBoundary() {
		var atLimit = string.Concat( Enumerable.Repeat( "(?=", 256 ) ) + "[a]" + new string( ')', 256 );
		var match = Compile( atLimit ).Match( "a"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 0, 0 ), match.Match?.Captures[0] );
		Assert.Null( match.Diagnostic );
		AssertDiagnostic( "(?=" + atLimit + ")", PcreByteDiagnosticCode.PatternDepthLimitExceeded, 768 );
	}

	[Fact]
	public void DeepNamedResolutionVisitsAssertionBodiesWithoutExceedingDepth() {
		// 255 assertions plus the named capture use exactly 256 group frames.
		var pattern = string.Concat( Enumerable.Repeat( "(?=", 255 ) ) + @"(?<x>[a])\k<x>"
			+ new string( ')', 255 );
		var match = Compile( pattern ).Match( "aa"u8.ToArray() );
		Assert.Null( match.Diagnostic );
		Assert.Equal( new PcreByteCapture( 0, 0 ), match.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 1 ), match.Match?.Captures[1] );
		Assert.Equal( 2, match.Match?.Captures.Count );
	}

	[Theory]
	[InlineData( 1, 1023, true )]
	[InlineData( 1, 1024, false )]
	[InlineData( 2, 1022, true )]
	[InlineData( 2, 1023, false )]
	public void AssertionGuardsCountTowardTheExistingSavedPathCeiling( int guards, int choices, bool succeeds ) {
		var pattern = string.Concat( Enumerable.Repeat( "(?=", guards ) )
			+ string.Concat( Enumerable.Repeat( "a??", choices ) ) + new string( ')', guards );
		var result = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 100_000 } ).Match( Array.Empty<byte>() );
		Assert.Equal( succeeds, result.IsSuccess );
		if ( succeeds ) {
			Assert.Null( result.Diagnostic );
			Assert.Equal( new PcreByteCapture( 0, 0 ), result.Match?.Captures[0] );
		} else {
			Assert.Null( result.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code );
		}
	}

	[Theory]
	[InlineData( "^(?=(?:a|aa)*b)a+$" )]
	[InlineData( "^(?!(?:a|aa)*b)a+$" )]
	[InlineData( "^(?:(?![b])a)+$" )]
	public void BodyExplorationSpendsWorkAndHonorsCancellation( string pattern ) {
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 40 ) );
		var limited = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 100 } ).Match( subject );
		Assert.Null( limited.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions {
			WorkObserver = count => { if ( count == 100 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
	}

	private static void AssertDiagnostic( string pattern, PcreByteDiagnosticCode code, int offset, bool utf8 = false ) {
		var result = PcreByteCompiler.Compile( ( utf8 ? Encoding.UTF8 : Encoding.Latin1 ).GetBytes( pattern ),
			new PcreByteOptions { Utf8 = utf8 } );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void AssertOutcome( string pattern, string hex, bool utf8, int offset,
		int expectedIndex, int expectedLength, string? expectedCaptures = null ) {
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern, utf8 );
		var native = reference.Match( subject, offset );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) {
			Assert.Equal( expectedLength, native.Length );
			if ( expectedCaptures is not null ) {
				var spans = expectedCaptures.Split( ',' );
				Assert.Equal( spans.Length, native.Groups.Count );
				for ( var group = 0; group < spans.Length; group++ ) {
					var capture = native.Groups[group];
					Assert.Equal( spans[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
				}
			}
		}
		Compare( reference, subject, offset, Compile( pattern, new PcreByteOptions { Utf8 = utf8 } ).Match( subject, offset ), pattern );
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset,
		PcreByteMatchResult actual, string context ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( native.Success ? native.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, actual.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var capture = native.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null,
				actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8 = false, bool invalidUtf = false, bool anyCrLf = false ) {
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = options } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var encoding = options?.Utf8 == true || options?.MatchInvalidUtf == true ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
