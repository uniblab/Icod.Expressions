using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Resumable assertions against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreNonAtomicAssertionReferenceTests {
	[Theory]
	[InlineData( "(?*", false )]
	[InlineData( "(*napla:", false )]
	[InlineData( "(*non_atomic_positive_lookahead:", false )]
	[InlineData( "(?<*", true )]
	[InlineData( "(*naplb:", true )]
	[InlineData( "(*non_atomic_positive_lookbehind:", true )]
	public void EverySpellingCanRetryWhereTheAtomicFormCommits( string prefix, bool behind ) {
		foreach ( var utf8 in new[] { false, true } ) {
			Check( behind ? "^ab" + prefix + @"(b)|(ab))(?:\1|\2)c$" : prefix + @"(a|ab))\1c",
				behind ? "ababc" : "abc", 0, behind ? 5 : 3, behind ? 3 : 2, utf8 );
			Check( prefix + ")", "", 0, 0, 1, utf8 );
			Check( prefix + @"())\1", "", 0, 0, 2, utf8 );
		}
		Check( behind ? @"^ab(?<=(b)|(ab))(?:\1|\2)c$" : @"(?=(a|ab))\1c",
			behind ? "ababc" : "abc", -1, 0, 0 );
	}

	[Theory]
	[InlineData( @"(?*(a+))\1a", "aaa", 0, 3, 2 )]
	[InlineData( @"(?*(a)?b)\1", "b", -1, 0, 0 )]
	[InlineData( @"(?*(?*(a|ab)))\1c", "abc", 0, 3, 2 )]
	[InlineData( @"(?*(?=(a|ab)))\1c", "abc", -1, 0, 0 )]
	[InlineData( @"(?=(?*(a|ab)))\1c", "abc", -1, 0, 0 )]
	[InlineData( @"(?*(a|ab))(?>\1)c", "abc", 0, 3, 2 )]
	[InlineData( @"(?>(?*(a|ab))\1)c", "abc", -1, 0, 0 )]
	[InlineData( @"(?*(a|ab))(?*(a|ab))\1c", "abc", 0, 3, 3 )]
	[InlineData( @"(?*(a|ab))(?<*(a?))\1c", "abc", 0, 3, 3 )]
	[InlineData( @"(?*(a|ab))(?>(?*(a|ab)))\1c", "abc", 0, 3, 3 )]
	[InlineData( @"(?!(?*(a|ab))\1c)abc", "abc", -1, 0, 0 )]
	[InlineData( @"(?!(?*(a|ab))\1d)abc", "abc", 0, 3, 2 )]
	[InlineData( @"(?*(?!(a)c)(a|ab))\2c", "abc", 0, 3, 3 )]
	[InlineData( @"(?:(?*(a|ab))\1d|a(b)c)\2", "abcb", 0, 4, 3 )]
	[InlineData( @"(?J)(?*(?<x>a)|(?<x>ab))\k<x>c", "abc", 0, 3, 3 )]
	[InlineData( @"(?n)(?*(?<x>a|ab))\k<x>c", "abc", 0, 3, 2 )]
	[InlineData( @"(?x)(?* (?<x>a|ab) )\k<x> c", "abc", 0, 3, 2 )]
	[InlineData( @"(?*(?<x>k|K)).(?i:\k<x>)", "kK", 0, 4, 2 )]
	[InlineData( @"(?*(?<x>k|K)).(?ir:\k<x>)", "kK", -1, 0, 0 )]
	[InlineData( @"(?*(é|é😀))\1x", "é😀x", 0, 7, 2 )]
	[InlineData( @"(?*(a|ab)){2}\1c", "abc", 0, 3, 2 )]
	[InlineData( @"(?*(a|ab)){1,2}?\1c", "abc", 0, 3, 2 )]
	[InlineData( @"(?U)(?*(a|ab))+\1c", "abc", 0, 3, 2 )]
	[InlineData( @"(?*(a|ab))++\1c", "abc", -1, 0, 0 )]
	[InlineData( @"(?*(a|ab)){0}abc", "abc", 0, 3, 2 )]
	[InlineData( @"(?<x>a)(?<*\k<x>)b", "ab", 0, 2, 2 )]
	[InlineData( @"(?J)(?<x>a)(?<x>b)(?<*(?*\k<x>)b)c", "abc", -1, 0, 0 )]
	public void RetainedChoicesRestoreSiblingScopesCapturesAndAtomicBoundaries( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( @"^aa(?<*(a{1,2}))\1b$", "aaab", 1, 1 )]
	[InlineData( @"^aa(?<*(a{1,2}))(?=a)\1b$", "aaab", 1, 1 )]
	[InlineData( @"^aa(?<*(?*(a{1,2}))a{1,2})\1b$", "aaab", 0, 1 )]
	[InlineData( @"^aa(?<*(?=(a{1,2}))a{1,2})\1b$", "aaab", 1, 1 )]
	[InlineData( @"^aa(?<*(?*(a{1,3}))a{1,2})\1b$", "aaaab", 0, 2 )]
	[InlineData( @"^aa(?<*(?*(a{1,3}))aa)\1b$", "aaaaab", 0, 3 )]
	[InlineData( @"^éé(?<*(é{1,2}))\1x$", "éééx", 2, 2 )]
	public void InnerRetriesAndReverseStartsRestoreTheCorrectJitEndCap( string pattern,
		string text, int captureIndex, int captureLength ) {
		var subject = Encoding.UTF8.GetBytes( text );
		var reference = Reference( pattern );
		var native = reference.Match( subject );
		Assert.True( native.Success );
		Assert.Equal( captureIndex, native.Groups[1].Index );
		Assert.Equal( captureLength, native.Groups[1].Length );
		Check( pattern, text, 0, subject.Length, 2 );
	}

	[Fact]
	public void AtomicVariableBehindDoesNotRetryAndWidthBoundariesAreUnchanged() {
		Check( @"^aa(?<=(a{1,2}))\1b$", "aaab", -1, 0, 0 );
		Check( "(?<*a{1,255})b", "ab", 1, 1, 1 );
		Check( "(?<*a{256})b", new string( 'a', 256 ) + "b", 256, 1, 1 );
	}

	[Theory]
	[InlineData( "(*NAPLA:a)" )]
	[InlineData( "(*naplaa:a)" )]
	[InlineData( "(*naplb)" )]
	[InlineData( "(*napla :a)" )]
	[InlineData( "(?x)(*napla\t:a)" )]
	[InlineData( "(?i)(*non_atomic_positive_Lookahead:a)" )]
	public void AlphaHeadersRemainExactEvenUnderPatternOptions( string pattern ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax,
			pattern.IndexOf( "(*", StringComparison.Ordinal ) );
	}

	[Theory]
	[InlineData( "é(?*a", 2 )]
	[InlineData( "é(?<*a", 2 )]
	[InlineData( "é(*napla:a", 2 )]
	[InlineData( "é(*non_atomic_positive_lookbehind:a", 2 )]
	[InlineData( "((?*a", 1 )]
	[InlineData( @"é(?*\K)", 2 )]
	[InlineData( @"é(?<*\K)", 2 )]
	[InlineData( @"é(?*(?>a\K))", 2 )]
	[InlineData( @"é(?>(?*\K))", 5 )]
	public void SourceByteOpenersAndEnclosingAssertionContextSurviveHeaders( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, offset );
	}

	[Theory]
	[InlineData( @"(?<*a*)b" )]
	[InlineData( @"(?<*a{1,256})b" )]
	[InlineData( @"(?J)(?<x>a)(?<x>b)(?<*\k<x>)c" )]
	[InlineData( @"(?|(a)|(b))(?<*\1)c" )]
	[InlineData( @"(?<x>(?<*\k<x>)a)" )]
	public void ConsumedBehindRestrictionsRemainControlled( string pattern ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax,
			pattern.IndexOf( "(?<*", StringComparison.Ordinal ) );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void MalformedUtfCrLfAndCallerOffsetsRetainAllCaptureSpans( bool anyCrLf ) {
		foreach ( var pattern in new[] {
			@"(?*(a|ab))\1c", @"(?<*(a{1,2}))b", @"(?*(?<x>.|..))\k<x>",
			@"(?*(a?))(?<*(a?))\1", @"(?m)(?*([a\r\n]*))\1$", @"(?*\G(a?))\1"
		} ) {
			var reference = Reference( pattern, invalidUtf: true, anyCrLf: anyCrLf );
			var compiled = Compile( pattern, Options( invalidUtf: true, anyCrLf: anyCrLf ) );
			foreach ( var hex in new[] { "", "616263", "61FF62", "FFC3A9C3A9", "FFFF", "0D0A61610D0A" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( "(?*" )]
	[InlineData( "(*napla:" )]
	[InlineData( "(*non_atomic_positive_lookahead:" )]
	[InlineData( "(?<*" )]
	[InlineData( "(*naplb:" )]
	[InlineData( "(*non_atomic_positive_lookbehind:" )]
	public void AssertionFormsUseOneDepthLevelAndNoImplicitCapture( string header ) {
		var nested = string.Concat( Enumerable.Repeat( header, 256 ) ) + "[a]" + new string( ')', 256 );
		Assert.True( Compile( nested ).Match( "a"u8.ToArray() ).IsSuccess );
		Diagnostic( header + nested + ")", PcreByteDiagnosticCode.PatternDepthLimitExceeded, 256 * header.Length );
		var captures = header + string.Concat( Enumerable.Repeat( "()", 256 ) ) + ")";
		Assert.Equal( 257, Compile( captures ).Match( Array.Empty<byte>() ).Match?.Captures.Count );
		Diagnostic( captures[..^1] + "())", PcreByteDiagnosticCode.PatternCaptureLimitExceeded, header.Length + 512 );
	}

	[Theory]
	[InlineData( 1024, true )]
	[InlineData( 1025, false )]
	public void SuccessfulInactiveGuardsStillCountTowardSavedPathLimit( int count, bool succeeds ) {
		var pattern = string.Concat( Enumerable.Repeat( "(?*[a])", count ) );
		var result = Compile( pattern ).Match( "a"u8.ToArray() );
		Assert.Equal( succeeds, result.IsSuccess );
		Assert.Equal( succeeds ? null : PcreByteDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code );
		if ( succeeds ) { Assert.Equal( new PcreByteCapture( 0, 0 ), result.Match?.Captures[0] ); }
	}

	[Fact]
	public void AtomicPruningReclaimsSiblingContextsAcrossManySuccesses() {
		Check( @"^(?>(?*[a])(?*[a])a)+$", new string( 'a', 800 ), 0, 800, 1 );
	}

	[Fact]
	public void RetriedBodiesRemainBoundedBySharedWorkAndCancellation() {
		const string pattern = @"^(?*(a|aa)*)(?:a|aa)*[b]$";
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 16 ) );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, Options() with { MatchWorkLimit = 400 } ).Match( subject ).Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( pattern, Options() with { WorkObserver = work => { if ( work == 400 ) { cancelled.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancelled.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), Options(), cancelled.Token ) );
	}

	private static void Diagnostic( string pattern, PcreByteDiagnosticCode code, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void Check( string pattern, string text, int index, int length, int groups, bool utf8 = true ) {
		var subject = ( utf8 ? Encoding.UTF8 : Encoding.Latin1 ).GetBytes( text );
		var reference = Reference( pattern, utf8: utf8 );
		var native = reference.Match( subject );
		Assert.Equal( index, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( length, native.Length ); Assert.Equal( groups, native.Groups.Count ); }
		Compare( reference, subject, 0, Compile( pattern, Options( utf8: utf8 ) ).Match( subject ), pattern );
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( native.Success ? native.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, actual.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var capture = native.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreByteOptions Options( bool invalidUtf = false, bool anyCrLf = false, bool utf8 = true ) => new() {
		Utf8 = utf8, MatchInvalidUtf = invalidUtf, AnyCrLfNewline = anyCrLf
	};

	private static PcreRegex8Bit Reference( string pattern, bool invalidUtf = false, bool anyCrLf = false, bool utf8 = true ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = PcreOptions.Compiled | ( utf8 ? PcreOptions.Utf : PcreOptions.None )
				| ( invalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		options ??= Options();
		var encoding = options.Utf8 || options.MatchInvalidUtf ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
