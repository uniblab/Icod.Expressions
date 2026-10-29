using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Bounded variable character-length lookbehind against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreVariableLookbehindReferenceTests {
	[Theory]
	[InlineData( "(?<=[a]?)b", "62", 0, 1, "0:1" )]
	[InlineData( "(?<=[a]?)b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<![a]?)b", "62", -1, 0, "" )]
	[InlineData( "(?<=[a]{1,2})b", "616162", 2, 1, "2:1" )]
	[InlineData( "(?<=[a]{1,2})b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<=[a]{1,2})b", "62", -1, 0, "" )]
	[InlineData( "(?<![a]{1,2})b", "7862", 1, 1, "1:1" )]
	[InlineData( "(?<![a]{1,2})b", "6162", -1, 0, "" )]
	[InlineData( "(?<=[a]{,2})b", "62", 0, 1, "0:1" )]
	[InlineData( "(?<=[a]{,2}?)b", "616162", 2, 1, "2:1" )]
	[InlineData( "(?<=(?:a|bc))d", "626364", 2, 1, "2:1" )]
	[InlineData( "(?<=(?>a|bc))d", "626364", 2, 1, "2:1" )]
	[InlineData( "(?<!(?:a|bc))d", "786364", 2, 1, "2:1" )]
	[InlineData( "(?<=(?:a|)){1,2}b", "62", 0, 1, "0:1" )]
	[InlineData( "(?<=(?:|aa))b", "616162", 2, 1, "2:1" )]
	[InlineData( @"(?<=(\R))b", "0D0A62", 2, 1, "2:1,0:2" )]
	[InlineData( @"(?<=(\R))b", "0A62", 1, 1, "1:1,0:1" )]
	[InlineData( @"(?<!\R)b", "7862", 1, 1, "1:1" )]
	[InlineData( @"(?<=\R?)b", "62", 0, 1, "0:1" )]
	[InlineData( "(?<=(?i)[a]{1,2})b", "414162", 2, 1, "2:1" )]
	[InlineData( "(?<=(?i)[a]{1,2})b", "414142", -1, 0, "" )]
	[InlineData( "(?<=(?x) [a]{1,2} # bounded\n (?# ignored) [b]? )c", "616263", 2, 1, "2:1" )]
	public void BoundedBodiesUseCharacterRangesAndPreserveOptions( string pattern, string hex,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, false, 0, index, length, captures );

	[Theory]
	[InlineData( "(?<=([a]{1,3}?))b", "61616162", 3, 1, "3:1,0:3" )]
	[InlineData( "(?<=([a]{1,3}))b", "61616162", 3, 1, "3:1,0:3" )]
	[InlineData( "(?<=([a]{1,3}?)([a]{0,2}))b", "61616162", 3, 1, "3:1,0:1,1:2" )]
	[InlineData( "(?<=([a]{1,3})([a]{0,2}))b", "61616162", 3, 1, "3:1,0:3,3:0" )]
	[InlineData( "(?<=([a]{1,3}+)[a])b", "61616162", -1, 0, "" )]
	[InlineData( "(?<=([a]{1,2})[a])b", "61616162", 3, 1, "3:1,0:2" )]
	[InlineData( "(?<=([a]{1,2}?)[a])b", "61616162", 3, 1, "3:1,0:2" )]
	[InlineData( "(?<=((?:a|aa))([a]?))b", "616162", 2, 1, "2:1,0:1,1:1" )]
	[InlineData( "(?<=((?:aa|a))([a]?))b", "616162", 2, 1, "2:1,0:2,2:0" )]
	[InlineData( "(?<=([a])|([a]{1,3}))b", "61616162", 3, 1, "3:1,2:1,-" )]
	[InlineData( "(?<=([a]{1,3})|([a]))b", "61616162", 3, 1, "3:1,0:3,-" )]
	[InlineData( "(?<=([a]{1,2})[x]|([a]{1,3}))b", "61616162", 3, 1, "3:1,-,0:3" )]
	[InlineData( "(?<=(([a]{1,2})[x]|([a]{1,2})))b", "616162", 2, 1, "2:1,0:2,-,0:2" )]
	[InlineData( "(?<=(a)?(?:bc|b))c", "61626263", 3, 1, "3:1,-" )]
	[InlineData( "(?<=([a]{0,2}))b", "62", 0, 1, "0:1,0:0" )]
	[InlineData( "(?<=(a|))b", "62", 0, 1, "0:1,0:0" )]
	[InlineData( @"^aaa(?<=([a]{1,3}?))\1$", "61616161", -1, 0, "" )]
	[InlineData( @"^aaa(?<=([a]{1,3}?)([a]{0,2}))\1$", "61616161", 0, 4, "0:4,0:1,1:2" )]
	[InlineData( @"(?<=(?<left>[ab]{1,2}))\k<left>", "61626162", 2, 2, "2:2,0:2" )]
	[InlineData( @"(?<=(?|(?<x>a)|(?<x>bb)))\g{x}", "62626262", 2, 2, "2:2,0:2" )]
	[InlineData( "(?n)(?<=(a?)(?<x>b?))c", "616263", 2, 1, "2:1,1:1" )]
	[InlineData( "(?<!([a]{1,2}))b", "7862", 1, 1, "1:1,-" )]
	[InlineData( "(?:(?<=([a]{1,2}))c|(b))", "6162", 1, 1, "1:1,-,1:1" )]
	public void ReverseStartsPrecedeBodyQuantifiersAndJournalAllCaptures( string pattern, string hex,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, false, 0, index, length, captures );

	[Theory]
	// PCRE2 10.48 JIT caps the subject end for the entire assertion if it emits any VREVERSE.
	// In the mixed case the first branch's fixed REVERSE equals the global minimum; the cap still applies.
	[InlineData( "(?<=[a]{1,2}(?=b))b", "6162", -1, 0, "" )]
	[InlineData( "(?<=a{1,2}(?<=a(?=b)))b", "6162", -1, 0, "" )]
	[InlineData( "(?<=[a](?=b))b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<=[a](?=b)|[x]{1,2})b", "6162", -1, 0, "" )]
	[InlineData( @"(?<=[a]{1,2}\z)b", "6162", 1, 1, "1:1" )]
	[InlineData( @"(?<=[a]\z)b", "6162", -1, 0, "" )]
	[InlineData( "(?<=[a]{1,2}$)b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<=[a]{1,2}(?!b))b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<![a]{1,2}(?=b))b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<=[a]{1,2}(?=(b?)))b", "6162", 1, 1, "1:1,1:0" )]
	[InlineData( "(?<=[a]{1,2})(b)", "6162", 1, 1, "1:1,1:1" )]
	[InlineData( "(?<![x]{1,2})(b)", "6162", 1, 1, "1:1,1:1" )]
	[InlineData( "(?:(?<=[a]{1,2}(?=b))b|(b))", "6162", 1, 1, "1:1,1:1" )]
	[InlineData( "(?<=[a](?<=[a]{1,2})(?=b))b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<=[a]{1,2}(?<=[a]{1,2})(?!b))b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<=[a]{1,2}(?!(?<=[a]{1,2})b))b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<=[a]{1,2}(?<=[a]){1,2})b", "6162", 1, 1, "1:1" )]
	public void JitScopedSubjectEndIncludesAllBranchesAndRestoresOnExit( string pattern, string hex,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, false, 0, index, length, captures );

	[Theory]
	[InlineData( "(?<=([é😀]{1,2}))b", "C3A9F09F988062", 6, 1, "6:1,0:6" )]
	[InlineData( "(?<=([é😀]{1,2}?))b", "F09F9880C3A962", 6, 1, "6:1,0:6" )]
	[InlineData( "(?<=([é😀]?))b", "F09F988062", 4, 1, "4:1,0:4" )]
	[InlineData( "(?<=(é|😀a))b", "F09F98806162", 5, 1, "5:1,0:5" )]
	[InlineData( "(?i)(?<=([k]{1,2}))b", "E284AA6B62", 4, 1, "4:1,0:4" )]
	[InlineData( "(?i)(?<=([K]{1,2}?))b", "6BE284AA62", 4, 1, "4:1,0:4" )]
	[InlineData( "(?i)(?<=([ſ]{1,2}))b", "53C5BF62", 3, 1, "3:1,0:3" )]
	[InlineData( "(?<=(?<字>[😀]{1,2}))b", "F09F9880F09F988062", 8, 1, "8:1,0:8" )]
	public void UtfReverseRangesCountScalarsAndKeepByteCaptureSpans( string pattern, string hex,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, true, 0, index, length, captures );

	[Theory]
	[InlineData( @"(?<=\G[a]{1,2})b", "616162", 0, 2, 1, "2:1" )]
	[InlineData( @"(?<=\G[a]{1,2})b", "616162", 1, 2, 1, "2:1" )]
	[InlineData( @"(?<=\G[a]{1,2})b", "616162", 2, -1, 0, "" )]
	[InlineData( @"(?<=[a]{1,2}\G)b", "616162", 2, 2, 1, "2:1" )]
	[InlineData( @"(?<=[a]{1,2}\G)b", "616162", 0, -1, 0, "" )]
	[InlineData( "a(?<=([a]{0,2})){2,3}b", "6162", 0, 0, 2, "0:2,0:1" )]
	[InlineData( "a(?<=([a]{0,2})){2,3}?b", "6162", 0, 0, 2, "0:2,0:1" )]
	[InlineData( "a(?<=([a]{0,2}))++b", "6162", 0, 0, 2, "0:2,0:1" )]
	[InlineData( "a(?<!([a]{1,2}))*b", "6162", 0, 0, 2, "0:2,-" )]
	public void CallerStartAndDirectAssertionRepeatsSurviveReverseSearch( string pattern, string hex, int offset,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, false, offset, index, length, captures );

	[Theory]
	// With tolerant UTF, JIT's maximum rewind cannot cross a malformed scalar even when the minimum fits.
	[InlineData( "(?<=[a]{1,2})b", "FF6162", -1, 0, "" )]
	[InlineData( "(?<=[a]{1,2})b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<=[a]{0,1})b", "FF62", -1, 0, "" )]
	[InlineData( "(?<=[a]{0,1})b", "62", 0, 1, "0:1" )]
	[InlineData( "(?<=([a]))b", "FF6162", 2, 1, "2:1,1:1" )]
	[InlineData( "(?<=())b", "FF62", 1, 1, "1:1,1:0" )]
	public void TolerantUtfMaximumRewindStillHonorsMalformedLeftBarriers( string pattern, string hex,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, true, 0, index, length, captures, true );

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void EverySelectedByteOffsetPreservesMalformedFragmentsAndCrLf( bool utf8, bool anyCrLf ) {
		foreach ( var pattern in new[] { @"(?<=[a]?)", @"(?<![a]?)", @"(?<=([a]{1,2}))", @"(?<!([a]{1,2}))",
			@"(?<=([ab]{,2}?))", @"(?<=((?:a|bb)))", @"(?<!(?:a|bb))", @"(?<=\R)", @"(?<!\R)",
			@"(?<=\R?)", @"(?<=([a]{1,2})(?=(b?)))b", @"(?<=[a]{1,2}(?!b))b", @"(?<=[a](?=b)|[x]{1,2})b",
			@"(?m)(?<=[a]{1,2}$)", @"(?<=[a]{1,2}\z)", @"(?<=\G[a]{1,2})", @"(?<=[a]{1,2}\G)",
			@"(?<=([a]{0,2}))+", @"(?<!([a]{1,2}))*?", @"(?<=[a]{1,2}(?<=[a]{1,2}))" } ) {
			var reference = Reference( pattern, utf8, utf8, anyCrLf );
			var compiled = Compile( pattern, new PcreByteOptions {
				Utf8 = utf8, MatchInvalidUtf = utf8, AnyCrLfNewline = anyCrLf
			} );
			foreach ( var hex in new[] { "", "61", "6162", "61616162", "6262", "FF", "FF6162", "61FF62", "FF62",
				"80806162", "61C3A962", "F09F98806162", "C3FF6162", "E2826162", "0D0A62", "610D0A62", "610D62", "0A610A62" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ),
						$"{pattern}, {hex}, offset {offset}, UTF={utf8}, ANYCRLF={anyCrLf}" );
				}
			}
		}
	}

	[Fact]
	public void VariableMaximumIs255AndAppliesToEveryTopLevelBranch() {
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 255 ) + "b" );
		AssertOutcome( "(?<=([a]{1,255}))b", Convert.ToHexString( subject ), false, 0, 255, 1, "255:1,0:255" );
		// Isolate the compile boundary from unanchored quadratic/cubic search work.
		AssertOutcome( "(?<=[a]{255}|[x]{1,2})b", Convert.ToHexString( subject ), false, 255, 255, 1, "255:1" );
		// Intermediate bounded width is erased before the assertion-wide variable cap.
		AssertOutcome( "(?<=(?:[a]{1,256}){0})b", "62", false, 0, 0, 1, "0:1" );
		foreach ( var pattern in new[] { "(?<=[a]{1,256})b", "(?<=[a]{256}|[x]{1,2})b", "(?<=[x]{1,2}|[a]{256})b",
			"(?<=[a]{1,255}[b])c", "(?<=(?:[a]{1,128}){2})b", "(?<=[a]{1,256}){0}b",
			"(?<=[a]{256}|[x]{1,2}){0}b" } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
			AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, 0 );
		}
	}

	[Theory]
	[InlineData( "(?<=[a]{256}|[b]{255}){0}c" )]
	[InlineData( "(?<=[a]{65535}|[b]{2}){0}c" )]
	[InlineData( "(?<=[a]{65535}){0}c" )]
	public void AllFixedBranchesRetainThe65535Policy( string pattern ) {
		// The erased assertion validates widths without asking JIT to generate 65535 copies.
		AssertOutcome( pattern, "63", false, 0, 0, 1, "0:1" );
	}

	[Theory]
	[InlineData( "(?<=[a]*)b", 0 )]
	[InlineData( "x(?<=[a]+)b", 1 )]
	[InlineData( "(?<=(?:a+))b", 0 )]
	[InlineData( "(?<=(?:^)*[a])b", 0 )]
	[InlineData( @"(a)(?<=\1)b", 3 )]
	[InlineData( @"(?<x>a)(?<=\k<x>)b", 7 )]
	[InlineData( @"(?<=(a)\g{-1})b", 0 )]
	[InlineData( @"x(?<=\X)b", 1 )]
	[InlineData( "(?<=[a]*){0}b", 0 )]
	[InlineData( "(?<=(?:[a]*){0})b", 0 )]
	[InlineData( "(?<=(?<=a)*)b", 0 )]
	[InlineData( "(?<=(?<=a)+)b", 0 )]
	[InlineData( "(?<=(?<=[a]{1,2})*)b", 0 )]
	public void UnboundedUnknownAndConsumedBackreferencesRemainUnsupportedAtTheOpening( string pattern, int offset ) =>
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, offset );

	[Theory]
	[InlineData( 1021, true )]
	[InlineData( 1022, false )]
	public void ReverseRetrySharesTheExistingSavedPathCeiling( int choices, bool succeeds ) {
		// Outer lazy choices + assertion guard + reverse retry + body Split = choices + 3.
		var pattern = @"\Aa" + string.Concat( Enumerable.Repeat( "a??", choices ) ) + "(?<=[a]?)";
		var result = Compile( pattern ).Match( "a"u8.ToArray() );
		Assert.Equal( succeeds, result.IsSuccess );
		if ( succeeds ) {
			Assert.Null( result.Diagnostic );
			Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match?.Captures[0] );
		} else {
			Assert.Null( result.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code );
		}
	}

	[Theory]
	[InlineData( "(?<=[a]{1,254}[x])b" )]
	[InlineData( "(?<![a]{1,254}[x])b" )]
	[InlineData( "a(?<=([a]{0,2})){30}b" )]
	public void ReverseExplorationSpendsWorkAndHonorsCancellation( string pattern ) {
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 254 ) + "b" );
		var limited = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 40 } ).Match( subject );
		Assert.Null( limited.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions {
			WorkObserver = work => { if ( work == 40 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
	}

	private static void AssertDiagnostic( string pattern, PcreByteDiagnosticCode code, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.Latin1.GetBytes( pattern ) );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void AssertOutcome( string pattern, string hex, bool utf8, int offset,
		int expectedIndex, int expectedLength, string captures, bool invalidUtf = false ) {
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern, utf8, invalidUtf );
		var native = reference.Match( subject, offset );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) {
			Assert.Equal( expectedLength, native.Length );
			var spans = captures.Split( ',' );
			Assert.Equal( spans.Length, native.Groups.Count );
			for ( var group = 0; group < spans.Length; group++ ) {
				var capture = native.Groups[group];
				Assert.Equal( spans[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
			}
		}
		Compare( reference, subject, offset, Compile( pattern, new PcreByteOptions {
			Utf8 = utf8, MatchInvalidUtf = invalidUtf
		} ).Match( subject, offset ), pattern );
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset,
		PcreByteMatchResult actual, string context ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( native.Success ? native.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}; native index {( native.Success ? native.Index : -1 )}, managed index {actual.Match?.ByteIndex ?? -1}" );
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
