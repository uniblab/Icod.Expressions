using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Capture participation against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreCaptureConditionReferenceTests {
	[Theory]
	[InlineData( "x" )]
	[InlineData( "<x>" )]
	[InlineData( "'x'" )]
	public void EveryNameSpellingResolvesForwardAndTestsEmptyCaptures( string name ) {
		foreach ( var utf8 in new[] { false, true } ) {
			Check( "^(?<x>a)?(?(" + name + ")b|c)$", "ab", 0, 2, 2, utf8 );
			Check( "^(?<x>a)?(?(" + name + ")b|c)$", "c", 0, 1, 2, utf8 );
			Check( "^(?(" + name + ")b|c)(?<x>a)$", "ca", 0, 2, 2, utf8 );
			Check( "^(?<x>)(?(" + name + ")a|b)$", "a", 0, 1, 2, utf8 );
		}
	}

	[Theory]
	[InlineData( @"^(a)?(?(1)b|c)$", "ab", 0, 2, 2 )]
	[InlineData( @"^(a)?(?(1)b|c)$", "c", 0, 1, 2 )]
	[InlineData( @"^(a)?(?(1)b|c)$", "ac", -1, 0, 0 )]
	[InlineData( @"^(a)?(?(1)b)$", "", 0, 0, 2 )]
	[InlineData( @"^()(?(1)a|b)$", "a", 0, 1, 2 )]
	[InlineData( @"^(a)(?(1)x|b)$", "ab", -1, 0, 0 )]
	[InlineData( @"^(?(1)a|b)(a)$", "ba", 0, 2, 2 )]
	[InlineData( @"^(?(+1)a|b)(a)$", "ba", 0, 2, 2 )]
	[InlineData( @"^(a)?(?(-1)b|c)$", "ab", 0, 2, 2 )]
	[InlineData( @"^(a)?(?(-1)b|c)$", "c", 0, 1, 2 )]
	[InlineData( @"^((?(-1)a|b))$", "b", 0, 1, 2 )]
	[InlineData( @"(a)(?(+01)b|c)(d)", "acd", 0, 3, 3 )]
	[InlineData( @"(a)(?(-01)b|c)", "ab", 0, 2, 2 )]
	[InlineData( @"(a)(?(01)b|c)", "ab", 0, 2, 2 )]
	[InlineData( @"(a)(?(1)|)", "a", 0, 1, 2 )]
	[InlineData( @"(?J)^(?:(?<x>a)|(?<x>b))(?(x)c|d)$", "bc", 0, 2, 3 )]
	[InlineData( @"(?J)^(?<x>a)?(?<x>b)?(?(x)c|d)$", "bc", 0, 2, 3 )]
	[InlineData( @"(?J)^(?<x>)?(?<x>b)?(?(x)c|d)$", "c", 0, 1, 3 )]
	[InlineData( @"(?J)^(?<x>a)?(?<x>b)(?(1)c|d)$", "bd", 0, 2, 3 )]
	[InlineData( @"(?J)^(?<x>a)?(?<x>b)(?(1)c|d)$", "bc", -1, 0, 0 )]
	[InlineData( @"(?J)^(?<x>a)(?<x>b)(?(x)\k<x>|b)$", "aba", 0, 3, 3 )]
	[InlineData( @"(?J)^(?<x>a)(?<x>b)(?(x)\k<x>|b)$", "abb", -1, 0, 0 )]
	[InlineData( @"^(a)?(?(1)b|a)c$", "ac", 0, 2, 2 )]
	[InlineData( @"^((?(1)b|a))+$", "ab", 0, 2, 2 )]
	[InlineData( @"^(?*(a)|ab)(?(1)\1x|abc)$", "abc", 0, 3, 2 )]
	[InlineData( @"^(?=(a)|ab)(?(1)\1x|abc)$", "abc", -1, 0, 0 )]
	[InlineData( @"^(?!(a)b)(?(1)b|a)$", "a", 0, 1, 2 )]
	[InlineData( @"^(?:(?>(a))x|ab)(?(1)c|d)$", "abd", 0, 3, 2 )]
	[InlineData( @"(?n)(a)(?<x>b)(?(x)c|d)", "abc", 0, 3, 2 )]
	[InlineData( @"(?|(a)|(b))(?(-1)c|d)", "bc", 0, 2, 2 )]
	[InlineData( @"(a)(?(1)(?i)b|c)d", "aBd", 0, 3, 2 )]
	[InlineData( @"(a)(?(1)(?i)b|c)d", "aBD", -1, 0, 0 )]
	[InlineData( @"(a)?(?(1)(?i)b|c)d", "Cd", 0, 2, 2 )]
	[InlineData( @"(?i)(a)(?(1)(?r:k)|x)", "aK", -1, 0, 0 )]
	[InlineData( @"(?i)(a)(?(1)k|x)", "aK", 0, 4, 2 )]
	[InlineData( @"(?ir)(a)(?(1)(?-r:k)|x)", "aK", 0, 4, 2 )]
	[InlineData( @"(?U)^(a)?(?(1)b|a)+$", "aaa", 0, 3, 2 )]
	[InlineData( @"(?<字٢>é)(?(字٢)😀|x)", "é😀", 0, 6, 2 )]
	[InlineData( @"^(a)(?(1)\K)b$", "ab", 1, 1, 2 )]
	public void SelectionAndRetriesShareTheCaptureJournalAndOptionScopes( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( @"^(?<R>a)(?(R)b|c)$", "ab", 0, 2, 2 )]
	[InlineData( @"^(?(R)b|c)(?<R>a)$", "ca", 0, 2, 2 )]
	[InlineData( @"^(?<R01>a)(?(R01)b|c)$", "ab", 0, 2, 2 )]
	[InlineData( @"^(?(R1)b|c)(?<R1>a)$", "ca", 0, 2, 2 )]
	[InlineData( @"(?<R١>a)(?(R١)b|c)", "ab", 0, 2, 2 )]
	[InlineData( @"(?<DEFINE>a)(?(<DEFINE>)b|c)", "ab", 0, 2, 2 )]
	[InlineData( @"(?<DEFINE>a)(?('DEFINE')b|c)", "ab", 0, 2, 2 )]
	[InlineData( @"(?<VERSION>a)(?(VERSION)b|c)", "ab", 0, 2, 2 )]
	[InlineData( @"(?<VERSIONX>a)(?(<VERSIONX>)b|c)", "ab", 0, 2, 2 )]
	[InlineData( @"(?<versionx>a)(?(versionx)b|c)", "ab", 0, 2, 2 )]
	public void ReservedBareHeadersRespectNativeNamePriority( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( @"(a)?(?<=(?(1)b))x", "x", -1, 0, 0 )]
	[InlineData( @"(a)?(?<=(?(1)b))x", "bx", 1, 1, 2 )]
	[InlineData( @"(a)?(?<=(?(1)b|))x", "x", 0, 1, 2 )]
	[InlineData( @"(a)(?<=(?(1)a|bb))x", "ax", 0, 2, 2 )]
	[InlineData( @"(?<=(?<x>(?(x)b|a)))x", "ax", 1, 1, 2 )]
	public void BehindWidthsUseExplicitBranchesWithoutCaptureBodyDependencies( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Fact]
	public void OmittedElseIsFixedWidthButExplicitEmptyElseUsesTheVariableLimit() {
		Check( @"(a)?(?<=(?(1)b{256}))x", new string( 'b', 256 ) + "x", 256, 1, 2 );
		Check( @"(a)?(?<=(?(1)b{256}))x", "x", -1, 0, 0 );
		Check( @"(a)?(?<=(?(1)b{255}|))x", "x", 0, 1, 2 );
		const string tooWide = @"(a)?(?<=(?(1)b{256}|))x";
		Assert.Throws<PcrePatternException>( () => Reference( tooWide ) );
		Diagnostic( tooWide, PcreByteDiagnosticCode.UnsupportedSyntax, 4 );
	}

	[Theory]
	[InlineData( "(a)(?(0)b|c)" )]
	[InlineData( "(a)(?(+0)b|c)" )]
	[InlineData( "(a)(?(-00)b|c)" )]
	[InlineData( "(a)(?(-2)b|c)" )]
	[InlineData( "(a)(?(2)b|c)" )]
	[InlineData( "(a)(?(+2)b|c)" )]
	[InlineData( "(?(2147483648)a)" )]
	[InlineData( "(?(missing)a){0}" )]
	[InlineData( "(a)(?(1)b|c|d){0}" )]
	[InlineData( "(a)(?(<1>)b|c)" )]
	[InlineData( "(a)(?('1')b|c)" )]
	[InlineData( "(?<x>a)(?({x})b|c)" )]
	[InlineData( "(?x)(a)(?( 1)b|c)" )]
	[InlineData( "(?xx)(a)(?(1 )b|c)" )]
	[InlineData( "(?x)(?<x>a)(?(<x> )b|c)" )]
	[InlineData( "é()(?(1)a" )]
	public void InvalidHeadersTargetsAndBranchesReportTheConditionByteOpener( string pattern ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		ConditionDiagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax );
	}

	[Theory]
	[InlineData( "(?(R)a|b)" )]
	[InlineData( "(a)(?(R1)b|c)" )]
	[InlineData( "(?(DEFINE)a)" )]
	[InlineData( "(?<DEFINE>a)(?(DEFINE)b)" )]
	[InlineData( "(?(VERSION>=10.0)a|b)" )]
	public void OtherConditionFamiliesRemainPending( string pattern ) {
		_ = Reference( pattern );
		ConditionDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax );
	}

	[Fact]
	public void LongerBareVersionPrefixesRemainReservedEvenWhenNamed() {
		const string pattern = "(?<VERSIONX>a)(?(VERSIONX)b|c)";
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		ConditionDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax );
		const string assertion = @"é(?=()(?(1)\K))";
		Assert.Throws<PcrePatternException>( () => Reference( assertion ) );
		Diagnostic( assertion, PcreByteDiagnosticCode.InvalidSyntax, 2 );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void InvalidUtfCrLfAndOffsetsPreserveImplicitEmptyContinuations( bool anyCrLf ) {
		foreach ( var pattern in new[] { @"(a)?(?(1)b)", @"(a)?(?(1)b|)",
			@"(?m)([\r\n])?(?(1)$|.)", @"(?<x>.?)(?(x)\k<x>|)" } ) {
			var reference = Reference( pattern, invalidUtf: true, anyCrLf: anyCrLf );
			var compiled = Compile( pattern, Options( invalidUtf: true, anyCrLf: anyCrLf ) );
			foreach ( var hex in new[] { "", "6162", "61FF62", "FFC3A9", "FFFF", "0D0A610D0A" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Fact]
	public void ConditionsUseOneDepthLevelAndNoImplicitCaptureAndRespectInstructionLimits() {
		const string header = "(?(1)";
		var nested = "()" + string.Concat( Enumerable.Repeat( header, 256 ) ) + "a" + new string( ')', 256 );
		Assert.Equal( 2, Compile( nested ).Match( "a"u8.ToArray() ).Match?.Captures.Count );
		Diagnostic( "()" + header + nested[2..] + ")", PcreByteDiagnosticCode.PatternDepthLimitExceeded, 2 + 256 * header.Length );
		var captures = string.Concat( Enumerable.Repeat( "()", 256 ) );
		Check( captures + "(?(256)a|b)", "a", 0, 1, 257 );
		Diagnostic( captures + "()(?(256)a|b)", PcreByteDiagnosticCode.PatternCaptureLimitExceeded, 512 );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( "()(?(1)a|b){2048}"u8.ToArray(), Options() ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( 1024, true )]
	[InlineData( 1025, false )]
	public void CaptureTestsDoNotAddAnAlternativeSavedPath( int count, bool succeeds ) {
		var pattern = "()(?(1)" + string.Concat( Enumerable.Repeat( "a??", count ) ) + "|b)";
		var result = Compile( pattern ).Match( Array.Empty<byte>() );
		Assert.Equal( succeeds, result.IsSuccess );
		Assert.Equal( succeeds ? null : PcreByteDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code );
		if ( succeeds ) { Assert.Equal( new PcreByteCapture( 0, 0 ), result.Match?.Captures[0] ); }
	}

	[Fact]
	public void DuplicateParticipationSearchPollsSharedWorkAndCancellation() {
		var pattern = "(?J)" + string.Concat( Enumerable.Repeat( "(?<x>a){0}", 255 ) ) + "(?<x>)(?(x)b|c)";
		Check( pattern, "b", 0, 1, 257 );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, Options() with { MatchWorkLimit = 40 } ).Match( "b"u8.ToArray() ).Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( pattern, Options() with { WorkObserver = work => { if ( work == 40 ) { cancelled.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( "b"u8.ToArray(), cancellationToken: cancelled.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options(), cancelled.Token ) );
	}

	private static void ConditionDiagnostic( string pattern, PcreByteDiagnosticCode code ) => Diagnostic( pattern, code,
		Encoding.UTF8.GetByteCount( pattern.AsSpan( 0, pattern.IndexOf( "(?(", StringComparison.Ordinal ) ) ) );

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
		Assert.True( ( native.Success ? native.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}: native index {( native.Success ? native.Index : -1 )}, managed index {actual.Match?.ByteIndex ?? -1}" );
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
