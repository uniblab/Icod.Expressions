using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Duplicate names and source-local J against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreDuplicateNameReferenceTests {
	[Fact]
	public void EveryDeclarationAndReferenceSpellingSelectsTheParticipatingNumberedSlot() {
		foreach ( var declaration in new[] { "(?<x>{0})", "(?'x'{0})", "(?P<x>{0})" } ) {
			foreach ( var reference in new[] { @"\k<x>", @"\k'x'", @"\k{x}", @"\g{x}", "(?P=x)" } ) {
				var pattern = "(?J)^(?:" + string.Format( declaration, "a" ) + "|"
					+ string.Format( declaration, "b" ) + ")" + reference + "$";
				Check( pattern, "bb", 0, 2, 3, utf8: false );
				Check( pattern, "bb", 0, 2, 3 );
			}
		}
	}

	[Theory]
	[InlineData( "(?J)^(?<x>a)(?<x>b)\\k<x>$", "aba", 0, 3, 3 )]
	[InlineData( "(?J)^(?<x>a)(?<x>b)\\k<x>$", "abb", -1, 0, 0 )]
	[InlineData( "(?J)^(?<x>)(?<x>b)\\k<x>$", "b", 0, 1, 3 )]
	[InlineData( "(?J)^(?<x>a)?(?<x>b)?\\k<x>$", "", -1, 0, 0 )]
	[InlineData( "(?J)^(?<x>a)?(?<x>b)\\k<x>$", "bb", 0, 2, 3 )]
	[InlineData( "(?J)^(?<x>a){0}(?<x>b)\\k<x>$", "bb", 0, 2, 3 )]
	[InlineData( "(?J)^(?:(?<x>a)|(?<x>b))\\k<x>$", "aa", 0, 2, 3 )]
	[InlineData( "(?J)^(?:(?<x>a)|(?<x>b))\\k<x>$", "bb", 0, 2, 3 )]
	[InlineData( "(?J)^(?:(?<x>a)|(?<x>b))\\k<x>$", "ab", -1, 0, 0 )]
	[InlineData( "(?J)^(?:\\k<x>|(?<x>a)|(?<x>b)){2}$", "bb", 0, 2, 3 )]
	[InlineData( "(?J)^(?:(?<x>a)|(?<x>b))+\\k<x>$", "aba", 0, 3, 3 )]
	[InlineData( "(?J)^(?:(?<x>a)|(?<x>b))+\\k<x>$", "abb", -1, 0, 0 )]
	[InlineData( "(?J)^(?:(?<x>a)c|a(?<x>b))\\k<x>$", "abb", 0, 3, 3 )]
	[InlineData( "(?J)^(?:(?>(?<x>a))c|a(?<x>b))\\k<x>$", "abb", 0, 3, 3 )]
	[InlineData( "(?J)^(?=(?<x>a))(?<x>a)\\k<x>$", "aa", 0, 2, 3 )]
	[InlineData( "(?J)^(?!(?<x>a)c)(?<x>a)\\k<x>$", "aa", 0, 2, 3 )]
	[InlineData( "(?J)(?<=(?<x>a))(?<x>b)\\k<x>", "aba", 1, 2, 3 )]
	[InlineData( "(?J)^(?<x>a)(?<x>b)\\2$", "abb", 0, 3, 3 )]
	[InlineData( "(?J)^(?<x>a)(?<x>b)(?<=\\2)c$", "abc", 0, 3, 3 )]
	[InlineData( "(?J)(?<x>a)(?<=\\k<x>)b", "ab", 0, 2, 2 )]
	[InlineData( "(?J)(?<x>a)(?<x>b)(?<=(?!\\k<x>)b)c", "abc", 0, 3, 3 )]
	[InlineData( "(?J)(?|(?<x>a)|(?<x>b))\\k<x>", "bb", 0, 2, 2 )]
	[InlineData( "(?J)(?|(a)(?<x>b)|(?<x>c)(d))\\k<x>", "cdd", 0, 3, 3 )]
	[InlineData( "(?J)(?|(a)(?<x>b)|(?<x>c)(d))\\k<x>", "cdc", -1, 0, 0 )]
	[InlineData( "(?J)(?|(a)(?<x>b)|(?<x>c)(d)|(?-J)(f)(?<x>g))\\k<x>", "fgg", 0, 3, 3 )]
	[InlineData( "(?J)(?<x>a)(?<X>b)\\k<x>\\k<X>", "abab", 0, 4, 3 )]
	[InlineData( "(?J)^(?:(?<字>é)|(?<字>😀))\\k<字>$", "😀😀", 0, 8, 3 )]
	[InlineData( "(?J)^(?<x>é)(?<x>😀)\\k<x>$", "é😀é", 0, 8, 3 )]
	[InlineData( "(?J)^(?<x>k)(?<x>a)(?i:\\k<x>)$", "kaK", 0, 5, 3 )]
	[InlineData( "(?J)^(?<x>k)(?<x>a)(?ir:\\k<x>)$", "kaK", -1, 0, 0 )]
	[InlineData( "(?J)^(?<x>Σ)(?<x>a)(?ir:\\k<x>)$", "Σaς", 0, 5, 3 )]
	[InlineData( "(?Ji)^(?<x>k)(?<x>a)(?-i:\\k<x>)$", "kaK", -1, 0, 0 )]
	[InlineData( "(?J)^(?<x>k)(?<x>a)(?i-r:\\k<x>)$", "kaK", 0, 5, 3 )]
	public void FirstParticipatingCaptureWinsWithoutComparisonFallback( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( "(?im-J)a", "A", 0, 1, 1 )]
	[InlineData( "(?i)(?J)a", "A", 0, 1, 1 )]
	[InlineData( "a(?J)b", "ab", 0, 2, 1 )]
	[InlineData( "(?nU:a(?J)b)", "ab", 0, 2, 1 )]
	[InlineData( "(?nJ)(a)", "a", 0, 1, 1 )]
	[InlineData( "(?<x>a)(?J)(?<x>b)\\k<x>", "aba", 0, 3, 3 )]
	[InlineData( "(?J:(?<x>a)(?<x>b))\\k<x>", "aba", 0, 3, 3 )]
	[InlineData( "(?<x>a)(?J:(?<x>b))\\k<x>", "aba", 0, 3, 3 )]
	[InlineData( "(?J)(?<x>a)(?^)(?<x>b)\\k<x>", "aba", 0, 3, 3 )]
	[InlineData( "(?^J)(?<x>a)(?<x>b)\\k<x>", "aba", 0, 3, 3 )]
	[InlineData( "(?JJ)(?<x>a)(?<x>b)(?-J)\\k<x>", "aba", 0, 3, 3 )]
	[InlineData( "(?J)(?<x>a)(?<x>b)(?-J:\\k<x>)", "aba", 0, 3, 3 )]
	[InlineData( "(?:(?<x>a)(?J)|(?<x>b))\\k<x>", "bb", 0, 2, 3 )]
	[InlineData( "(?nJ)(a)(?<x>b)(?<x>c)\\k<x>", "abcb", 0, 4, 3 )]
	[InlineData( "(?xJ) (?<x>a) (?<x>b) \\k<x>", "aba", 0, 3, 3 )]
	[InlineData( "(?JU)(?^)(?<x>a+)(?<x>a+)\\k<x>", "aaaa", 0, 3, 3 )]
	[InlineData( "(?aJ)(?<x>a)(?<x>b)\\k<x>", "aba", 0, 3, 3 )]
	[InlineData( "(?J-J)(?<x>a)\\k<x>", "aa", 0, 2, 2 )]
	[InlineData( "(?-J:J)(?<x>a)", "Ja", 0, 2, 2 )]
	public void HeadersRestoreScopesPropagateAcrossBranchesAndPreserveJAtReset( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( @"(?J)(?<x>a)(?<x>b).(?<=(?=\k<x>)a)", "aba", 0, 3, 3 )]
	[InlineData( @"(?J)(?<x>a)(?<x>b)(?<=(?=\k<x>)b)c", "abc", -1, 0, 0 )]
	public void DuplicateReferencesInsideAheadDoNotContributeConsumedBehindWidth( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( @"(?J)(?<x>a)(?|(?<y>b)|(?<x>c))\k<y>", "acc" )]
	[InlineData( @"(?J)(?<x>a)(?|(?<y>b)|(?<x>c))\k<x>", "aca" )]
	[InlineData( @"(?J)(?<x>a)(?|(?<y>b)|(?<x>c))\k<y>", "abb" )]
	[InlineData( @"(?J)(?<x>a)(?|(?<y>b)|(?<x>c))\k<x>", "aba" )]
	public void EarlierMatchingNameCanPreemptALaterNumberConflict( string pattern, string text ) =>
		Check( pattern, text, 0, 3, 3 );

	[Theory]
	[InlineData( @"(?<x>a)(?<x>b)" )]
	[InlineData( @"(?<x>a)(?<x>b)(?J)" )]
	[InlineData( @"(?J:(?<x>a))(?<x>b)" )]
	[InlineData( @"(?J)(?<x>a)(?-J)(?<x>b)" )]
	[InlineData( @"(?J-J)(?<x>a)(?<x>b)" )]
	[InlineData( @"(?JJ-J)(?<x>a)(?<x>b)" )]
	[InlineData( @"(?J)(?|(?<x>a)|(?<y>b))" )]
	[InlineData( @"(?J)(?|(?<y>a)(?<x>b)|(?<x>c))" )]
	[InlineData( @"(?J)(?|(a)(?<x>b)|(?<x>c)(d)|(?-J)(?<x>e))" )]
	[InlineData( @"é(?J:(?<x>a))(?<x>b)" )]
	public void DuplicatePermissionIsCheckedLocallyAndEachSlotHasAtMostOneName( string pattern ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var declaration = pattern.LastIndexOf( "(?<", StringComparison.Ordinal );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( Encoding.UTF8.GetByteCount( pattern.AsSpan( 0, declaration ) ), result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"(?J)(?<x>a)(?<x>b)(?<=\k<x>)c" )]
	[InlineData( @"(?J)(?<x>a)(?<x>a)(?<=\k<x>)b" )]
	[InlineData( @"(?J)(?<x>a)(?<x>b)(?<!\k<x>)c" )]
	[InlineData( @"(?J)(?<=\k<x>)(?<x>a)(?<x>b)" )]
	[InlineData( @"(?J)(?<x>a)(?<x>b)(?<=\k<x>{0})c" )]
	[InlineData( @"(?J)(?<x>a)(?<x>b)(?-J)(?<=\k<x>)c" )]
	[InlineData( @"(?J)(?<x>a)(?<x>b)(?<=(?=.(?<=\k<x>))b)c" )]
	public void ConsumedAmbiguousNamesAreRejectedEvenWhenTheyHaveEqualOrErasedWidths( string pattern ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var behind = Math.Max( pattern.LastIndexOf( "(?<=", StringComparison.Ordinal ),
			pattern.LastIndexOf( "(?<!", StringComparison.Ordinal ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( behind, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?J)*", 4 )]
	[InlineData( "a(?J){2}", 5 )]
	public void BareJSettingsRemainUnrepeatable( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?J-J-J)a", 0 )]
	[InlineData( "(?^J-J)a", 0 )]
	[InlineData( "(?J J)a", 0 )]
	[InlineData( "é(?J", 2 )]
	public void MalformedHeadersKeepOriginalByteOffsets( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void SelectedMalformedUtfAndCrLfOffsetsPreserveAllNumberedSpans( bool anyCrLf ) {
		foreach ( var pattern in new[] {
			@"(?J)(?:(?<x>a?)|(?<x>.))\k<x>",
			@"(?Ji)(?:(?<x>k)|(?<x>K))\k<x>",
			@"(?J)(?=(?<x>.))(?<x>.)\k<x>",
			@"(?Jm)^(?:(?<x>a*)|(?<x>k*))\k<x>$"
		} ) {
			var reference = Reference( pattern, invalidUtf: true, anyCrLf: anyCrLf );
			var compiled = Compile( pattern, Options( invalidUtf: true, anyCrLf: anyCrLf ) );
			foreach ( var hex in new[] { "", "6161", "FFE284AAE284AA", "61FF61", "C3FF61", "FFFF", "0D0A61610D0A" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Fact]
	public void DuplicateNamesRetainCaptureAndDepthLimitsWithoutGrowingNativeStacks() {
		var nested = string.Concat( Enumerable.Repeat( "(?J:", 256 ) ) + "a" + new string( ')', 256 );
		Assert.Equal( 1, Compile( nested ).Match( "a"u8.ToArray() ).Match?.ByteLength );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( "(?J:" + nested + ")" ), Options() ).Diagnostic?.Code );
		var captures = "(?J)" + string.Concat( Enumerable.Repeat( "(?<x>)", 256 ) );
		Check( captures + @"\k<x>", "", 0, 0, 257 );
		Assert.Equal( PcreByteDiagnosticCode.PatternCaptureLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( captures + "(?<x>)" ), Options() ).Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource();
		cancelled.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( captures ), Options(), cancelled.Token ) );
	}

	[Fact]
	public void SelectionWorkAndCancellationAreChargedWithinTheExistingMatchBudget() {
		var pattern = "(?J)" + string.Concat( Enumerable.Repeat( "(?<x>a){0}", 255 ) ) + @"(?<x>b)\k<x>";
		var subject = "bb"u8.ToArray();
		Check( pattern, "bb", 0, 2, 257 );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, Options() with { MatchWorkLimit = 40 } ).Match( subject ).Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( pattern, Options() with {
			WorkObserver = work => { if ( work == 40 ) { cancelled.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancelled.Token ) );
	}

	private static void Check( string pattern, string text, int index, int length, int groups, bool utf8 = true ) {
		var subject = ( utf8 ? Encoding.UTF8 : Encoding.Latin1 ).GetBytes( text );
		var reference = Reference( pattern, utf8 );
		var native = reference.Match( subject );
		Assert.Equal( index, native.Success ? native.Index : -1 );
		if ( native.Success ) {
			Assert.Equal( length, native.Length );
			Assert.Equal( groups, native.Groups.Count );
		}
		Compare( reference, subject, 0, Compile( pattern, Options( utf8 ) ).Match( subject ), pattern );
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

	private static PcreByteOptions Options( bool utf8 = true, bool invalidUtf = false, bool anyCrLf = false ) => new() {
		Utf8 = utf8, MatchInvalidUtf = invalidUtf, AnyCrLfNewline = anyCrLf
	};

	private static PcreRegex8Bit Reference( string pattern, bool utf8 = true, bool invalidUtf = false, bool anyCrLf = false ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = options, ParensLimit = 512 } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		options ??= Options();
		var encoding = options.Utf8 || options.MatchInvalidUtf ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
