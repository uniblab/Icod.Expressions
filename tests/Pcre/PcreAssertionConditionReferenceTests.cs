using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Atomic assertion conditions against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreAssertionConditionReferenceTests {
	[Theory]
	[InlineData( "(?=", false, true )]
	[InlineData( "(?!", false, false )]
	[InlineData( "(?<=", true, true )]
	[InlineData( "(?<!", true, false )]
	[InlineData( "(*pla:", false, true )]
	[InlineData( "(*positive_lookahead:", false, true )]
	[InlineData( "(*nla:", false, false )]
	[InlineData( "(*negative_lookahead:", false, false )]
	[InlineData( "(*plb:", true, true )]
	[InlineData( "(*positive_lookbehind:", true, true )]
	[InlineData( "(*nlb:", true, false )]
	[InlineData( "(*negative_lookbehind:", true, false )]
	public void EveryAtomicSpellingSelectsWithoutConsumingOrAllocatingACapture( string header, bool behind, bool positive ) {
		foreach ( var utf8 in new[] { false, true } ) {
			var prefix = behind ? "^a" : "^";
			var matched = positive ? "a|b" : "b|a";
			Check( prefix + "(?" + header + "(a))" + matched + ")$", behind ? "aa" : "a", 0, behind ? 2 : 1, 2, utf8 );
			var missed = positive ? "b|a" : "a|b";
			Check( prefix + "(?" + header + "(z))" + missed + ")$", behind ? "aa" : "a", 0, behind ? 2 : 1, 2, utf8 );
			Check( "^(?" + header + ")" + ( positive ? "a|b" : "b|a" ) + ")$", "a", 0, 1, 1, utf8 );
		}
	}

	[Theory]
	[InlineData( @"^(?(?=(a))\1|X)$", "a", 0, 1 )]
	[InlineData( @"^(?(?!(a))X|\1)$", "a", 0, 1 )]
	[InlineData( @"^a(?(?<=(a))\1|X)$", "aa", 0, 1 )]
	[InlineData( @"^a(?(?<!(a))X|\1)$", "aa", 0, 1 )]
	[InlineData( @"^(?(?=())a|X)$", "a", 0, 0 )]
	[InlineData( @"^(?(?!())X|a)$", "a", 0, 0 )]
	[InlineData( @"^(?(?=(a)b)X|(?(1)Y|a))$", "a", -1, 0 )]
	[InlineData( @"^(?(?!(a)b)(?(1)Y|a)|X)$", "a", -1, 0 )]
	public void MatchingBodiesCommitCapturesForBothSignsAndExhaustedBodiesRollback( string pattern, string text,
		int captureIndex, int captureLength ) {
		Check( pattern, text, 0, Encoding.UTF8.GetByteCount( text ), 2 );
		var capture = Reference( pattern ).Match( Encoding.UTF8.GetBytes( text ) ).Groups[1];
		Assert.Equal( captureIndex >= 0, capture.Success );
		if ( capture.Success ) { Assert.Equal( captureIndex, capture.Index ); Assert.Equal( captureLength, capture.Length ); }
	}

	[Theory]
	[InlineData( @"^(?(?=(a|ab))\1c|X)$", "abc", -1, 0, 0 )]
	[InlineData( @"^(?(?!(a|ab))X|\1c)$", "ac", 0, 2, 2 )]
	[InlineData( @"^(?(?!(a|ab))X|\1c)$", "abc", -1, 0, 0 )]
	[InlineData( @"^(?(?=(a))b|a)$", "a", -1, 0, 0 )]
	[InlineData( @"^(?(?!b)b|a)$", "a", -1, 0, 0 )]
	[InlineData( @"^(?:(a)|ab)(?(?=c)(?(1)X|c)|Y)$", "abc", 0, 3, 2 )]
	[InlineData( @"^(?*(a)|ab)(?(?=a)(?(1)X|abc)|Y)$", "abc", 0, 3, 2 )]
	[InlineData( @"^(?(?=(?*(a|ab)))\1c|X)$", "abc", -1, 0, 0 )]
	[InlineData( @"^(?(?=(?*(a|ab))\1c)abc|X)$", "abc", 0, 3, 2 )]
	[InlineData( @"^(?:(?(?=(?>(a)))X|Y)|a)(?(1)X|b)$", "ab", 0, 2, 2 )]
	[InlineData( @"^(?(?=(?(?=(a))a|X))\1|Y)$", "a", 0, 1, 2 )]
	[InlineData( @"^(?(?=(?!(a)b))a|X)$", "a", 0, 1, 2 )]
	[InlineData( @"^(?>(?(?=(a))a|b))+c$", "aabc", 0, 4, 2 )]
	[InlineData( @"^(?(?=a)a|b){2,3}?c$", "abbc", 0, 4, 1 )]
	[InlineData( @"^(?(?=a)a|b)++c$", "abbc", 0, 4, 1 )]
	[InlineData( @"^(?(?=z)X)$", "", 0, 0, 1 )]
	[InlineData( @"^(?(?!z))$", "", 0, 0, 1 )]
	[InlineData( @"^(?(?=a)|)a$", "a", 0, 1, 1 )]
	[InlineData( @"(?(?=a)a){0}b", "b", 0, 1, 1 )]
	[InlineData( @"^(?(?=a)a\K|b\K)c$", "ac", 1, 1, 1 )]
	[InlineData( @"^(?(?=a)a\K|b\K)c$", "bc", 1, 1, 1 )]
	public void SelectedBranchesCommitButOuterRetriesRestoreTheSharedTransactions( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( @"^(?(?=(?i)a)A|B)c$", "Ac", 0, 2, 1 )]
	[InlineData( @"^(?(?=(?i)a)A|B)c$", "AC", -1, 0, 0 )]
	[InlineData( @"^(?(?=a)(?i)a|b)c$", "Bc", 0, 2, 1 )]
	[InlineData( @"^(?(?=a)(?i)a|b)c$", "BC", -1, 0, 0 )]
	[InlineData( @"(?n)^(?(?=(a)(?<x>b))ab|X)\k<x>$", "abb", 0, 3, 2 )]
	[InlineData( @"(?J)^(?(?=(?<x>a)|(?<x>b))\k<x>|X)$", "b", 0, 1, 3 )]
	[InlineData( @"^(?(?=(?|(a)|(b)))\g{-1}|X)$", "b", 0, 1, 2 )]
	[InlineData( @"^(?(?=a)(b)|(c))(d)$", "cd", 0, 2, 4 )]
	[InlineData( @"^(?(?=(?(+1)X|a))(a)|X)$", "a", 0, 1, 2 )]
	[InlineData( @"(?x)^(?(?= (a) ) \1 | X )$", "a", 0, 1, 2 )]
	[InlineData( @"^(?(?=(?<字٢>é😀))\k<字٢>|X)$", "é😀", 0, 6, 2 )]
	public void OptionsAndDeferredReferencesVisitConditionAndBothBranches( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( @"(?(?<=(a{1,2}))\1b|X)", "aaaab", 2, 3, 2 )]
	[InlineData( @"^aa(?(?<=(a{1,2}))\1b|X)$", "aaab", -1, 0, 0 )]
	[InlineData( @"^aa(?(?<!(a{1,2}))X|\1b)$", "aaab", -1, 0, 0 )]
	[InlineData( @"(?<=(?(?=b)a|xx))b", "ab", -1, 0, 0 )]
	[InlineData( @"(?<=(?(?=a)a|xx))b", "ab", 1, 1, 1 )]
	[InlineData( @"(?<=(?(?=z)a))x", "x", -1, 0, 0 )]
	[InlineData( @"(?<=(?(?=z)a))x", "ax", 1, 1, 1 )]
	[InlineData( @"(?<=(?(?=z)a|))x", "x", 0, 1, 1 )]
	[InlineData( @"(?(?<=(é|😀a))b|X)", "😀ab", 5, 1, 2 )]
	public void BehindUsesExplicitBranchWidthsAndOriginalSubjectByteSpans( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Fact]
	public void OmittedElseIsFixedWidthAndNestedAssertionsStillValidateWhenErased() {
		Check( @"(?<=(?(?=z)a{256}))x", new string( 'a', 256 ) + "x", 256, 1, 1 );
		Check( @"(?<=(?(?=z)a{255}|))x", "x", 0, 1, 1 );
		foreach ( var pattern in new[] { @"(?<=(?(?=z)a{256}|))x", @"(?(?<=a*)a|b){0}", @"(?(?=a)a|(?(?<=b*)b|c)){0}" } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
			Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax,
				Encoding.UTF8.GetByteCount( pattern.AsSpan( 0, pattern.IndexOf( "(?<=", StringComparison.Ordinal ) ) ) );
		}
	}

	[Theory]
	[InlineData( "(?*" )]
	[InlineData( "(?<*" )]
	[InlineData( "(*napla:" )]
	[InlineData( "(*non_atomic_positive_lookahead:" )]
	[InlineData( "(*naplb:" )]
	[InlineData( "(*non_atomic_positive_lookbehind:" )]
	public void NonAtomicConditionsAreInvalidAtTheirInnerSourceByteOpener( string header ) {
		var pattern = "é(?" + header + "a)a|b){0}";
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, 4 );
	}

	[Theory]
	[InlineData( @"é(?(?=a)?a|b)", 9 )]
	[InlineData( @"é(?(?=a)*a|b)", 9 )]
	[InlineData( @"é(?(?=a)+a|b)", 9 )]
	[InlineData( @"é(?(?=a){0}a|b)", 9 )]
	[InlineData( @"é(?(?=a){1}a|b)", 9 )]
	[InlineData( @"é(?(?=a){0,1}a|b)", 9 )]
	[InlineData( @"é(?(?=a){2}a|b)", 9 )]
	[InlineData( @"é(?(?=a){1,2}?a|b)", 9 )]
	[InlineData( @"é(?(?=a)a|b|c){0}", 2 )]
	[InlineData( @"é(?(?=a)a", 2 )]
	[InlineData( @"é(?(?=\K)a|b)", 4 )]
	[InlineData( @"é(?(?=(?>a\K))a|b)", 4 )]
	[InlineData( @"é(?=(?(?=a)\K|b))", 2 )]
	[InlineData( "(?x)é(?( ?=a)a|b)", 6 )]
	public void InvalidSupportedSyntaxKeepsQuantifierConditionAndAssertionByteDiagnostics( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, offset );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void MalformedUtfCrLfAndEveryCallerOffsetRetainNumberedByteCaptures( bool anyCrLf ) {
		foreach ( var pattern in new[] { @"(?(?=(a?))\1|b)", @"(?(?!(a))b|\1)", @"(?(?<=(a{1,2}))b|a)",
			@"(?m)(?(?=([\r\n]))$|.)", @"(?(?=\G(a?))\1|b)", @"(?(?<=\G[a]{1,2})b|a)", @"(?(?=a)a)" } ) {
			var reference = Reference( pattern, invalidUtf: true, anyCrLf: anyCrLf );
			var compiled = Compile( pattern, Options( invalidUtf: true, anyCrLf: anyCrLf ) );
			foreach ( var hex in new[] { "", "616162", "61FF62", "FFC3A9", "FFFF", "0D0A61610D0A" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Fact]
	public void ConditionAndAssertionUseTwoDepthLevelsAndNoImplicitCapture() {
		var nested = "[a]";
		for ( var depth = 0; depth < 128; depth++ ) { nested = "(?(?=" + nested + ")[a]|[b])"; }
		Assert.Equal( 1, Compile( nested ).Match( "a"u8.ToArray() ).Match?.Captures.Count );
		Assert.Equal( 1, Compile( string.Concat( Enumerable.Repeat( "(?:", 254 ) ) + "(?(?=a)a|b)" + new string( ')', 254 ) )
			.Match( "a"u8.ToArray() ).Match?.Captures.Count );
		var wrappers = string.Concat( Enumerable.Repeat( "(?:", 255 ) );
		Diagnostic( wrappers + "(?(?=a)a|b)" + new string( ')', 255 ), PcreByteDiagnosticCode.PatternDepthLimitExceeded, 767 );
		var captures = "(?(?=" + string.Concat( Enumerable.Repeat( "()", 256 ) ) + ")a|b)";
		Check( captures, "a", 0, 1, 257 );
		Diagnostic( captures[..^5] + "())a|b)", PcreByteDiagnosticCode.PatternCaptureLimitExceeded, 517 );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( "(?(?=a)a|b){2048}"u8.ToArray(), Options() ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( 1024, true )]
	[InlineData( 1025, false )]
	public void CompletedConditionReleasesItsGuardBeforeSelectedBranchSavedPaths( int count, bool succeeds ) {
		var pattern = "(?(?=[a])" + string.Concat( Enumerable.Repeat( "a??", count ) ) + "|b)";
		var result = Compile( pattern ).Match( "a"u8.ToArray() );
		Assert.Equal( succeeds, result.IsSuccess );
		Assert.Equal( succeeds ? null : PcreByteDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code );
		if ( succeeds ) { Assert.Equal( new PcreByteCapture( 0, 0 ), result.Match?.Captures[0] ); }
	}

	[Theory]
	[InlineData( 1023, true )]
	[InlineData( 1024, false )]
	public void ActiveConditionGuardCountsTowardItsBodySavedPathLimit( int count, bool succeeds ) {
		var pattern = "(?(?=" + string.Concat( Enumerable.Repeat( "a??", count ) ) + "[a])a|b)";
		var result = Compile( pattern ).Match( "a"u8.ToArray() );
		Assert.Equal( succeeds, result.IsSuccess );
		Assert.Equal( succeeds ? null : PcreByteDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code );
		if ( succeeds ) { Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match?.Captures[0] ); }
	}

	[Fact]
	public void RepeatedTransactionsReclaimScopesAndPollSharedWorkAndCancellation() {
		Check( @"^(?>(?(?=a)a|b)(?(?=a)a|b))+$", new string( 'a', 1600 ), 0, 1600, 1 );
		const string pattern = @"^(?(?=(a|aa)*[b])X|a*)$";
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 16 ) );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, Options() with { MatchWorkLimit = 400 } ).Match( subject ).Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( pattern, Options() with { WorkObserver = work => { if ( work == 400 ) { cancelled.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancelled.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options(), cancelled.Token ) );
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
