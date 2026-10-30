using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Nonrecursive returned capture lists against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreReturnedCaptureReferenceTests {
	[Theory]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)(?<y>b)))(?&g(<x>))c$", "abc", 0, 3, "0:3,-,0:1,-" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)(?<y>b)))(?1(2,3))c$", "abc", 0, 3, "0:3,-,0:1,1:1" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)))(?P>g(1,2))$", "a", 0, 1, "0:1,-,0:1" )]
	[InlineData( @"^(?<g>(?<x>a|b))(?&g(1,2))$", "ab", 0, 2, "0:2,0:1,1:1" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>)(?<y>a)?))(?&g('x','y'))$", "", 0, 0, "0:0,-,0:0,-" )]
	[InlineData( @"^(?<g>(?<x>a)?b)(?&g(<x>))$", "abb", 0, 3, "0:3,0:2,0:1" )]
	[InlineData( @"^(?<g>(?<x>a|b)(?<y>a|b))(?&g(2))$", "aabb", 0, 4, "0:4,0:2,2:1,1:1" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a|b)+))(?&g(2))$", "ab", 0, 2, "0:2,-,1:1" )]
	[InlineData( @"^(?(DEFINE)(?<g>(a)))(?01(002,2,002))$", "a", 0, 1, "0:1,-,0:1" )]
	[InlineData( @"^(?(DEFINE)(?<g>(a)))(?-2(-1))$", "a", 0, 1, "0:1,-,0:1" )]
	[InlineData( @"^(?+1(+2))(?<g>(a))$", "aa", 0, 2, "0:2,1:1,1:1" )]
	[InlineData( @"^(?+1(+2))\2(?<g>(a)){0}$", "aa", 0, 2, "0:2,-,0:1" )]
	[InlineData( @"^(?&g(<x>))(?<g>(?<x>a))$", "aa", 0, 2, "0:2,1:1,1:1" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)))(?&g(+1))(?<later>b)$", "ab", 0, 2, "0:2,-,-,1:1" )]
	[InlineData( @"(?J)^(?(DEFINE)(?<g>(?:(?<x>a)|(?<x>b))))(?&g(<x>))$", "b", 0, 1, "0:1,-,-,0:1" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?|(?<x>a)|(?<x>b))))(?&g(<x>,2))$", "b", 0, 1, "0:1,-,0:1" )]
	[InlineData( @"^(?|(?<g>(?<x>a))b|(?&g(+2))c)$", "ac", 0, 2, "0:2,-,0:1" )]
	[InlineData( @"^(?(DEFINE)(?<字٢>(?<値>é)))(?&字٢(<値>))$", "é", 0, 2, "0:2,-,0:2" )]
	public void ListsResolveGloballyAndReturnOnlySelectedFinalNumberedCaptures( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Theory]
	[InlineData( @"^(?(DEFINE)(?<a>(?&b(<x>)))(?<b>(?<x>c)))(?&a(<x>))$", "c", 0, 1, "0:1,-,-,0:1" )]
	[InlineData( @"^(?(DEFINE)(?<a>(?&b(<x>)))(?<b>(?<x>c)))(?&a)$", "c", 0, 1, "0:1,-,-,-" )]
	[InlineData( @"^(?(DEFINE)(?<a>(?&b))(?<b>(?<x>c)))(?&a(<x>))$", "c", 0, 1, "0:1,-,-,-" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)|(?<y>ab)))(?&g(2,3))c$", "abc", 0, 3, "0:3,-,-,0:2" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)))(?:(?&g(2))b|ac)(?(x)X|d)$", "acd", 0, 3, "0:3,-,-" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)\Kb))(?&g(2))c$", "abc", 1, 2, "1:2,-,0:1" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)\Kb))(?:(?&g(2))c|abd)$", "abd", 0, 3, "0:3,-,-" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)|(?<y>ab)))(?>(?&g(2,3)))b$", "ab", 0, 2, "0:2,-,0:1,-" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)|(?<y>ab)))(?>(?&g(2,3)))c$", "abc", -1, 0, "" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)))(?=(?&g(2)))\2$", "a", 0, 1, "0:1,-,0:1" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)))(?!(?&g(2))b)ac(?(x)X|)$", "ac", 0, 2, "0:2,-,-" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a|ab)))(?*(?&g(2)))\2c$", "abc", 0, 3, "0:3,-,0:2" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a|ab)))(?=(?&g(2)))\2c$", "abc", -1, 0, "" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a)))(?(?=(?&g(2)))\2|X)$", "a", 0, 1, "0:1,-,0:1" )]
	[InlineData( @"^a(?(DEFINE)(?<g>(?<=(?<x>a))))(?&g(2))b$", "ab", 0, 2, "0:2,-,0:1" )]
	[InlineData( @"^c(?(DEFINE)(?<g>(?<!(?<x>a))))(?&g(2))b$", "cb", 0, 2, "0:2,-,-" )]
	[InlineData( @"^(?<outer>a(?&g(1,3))b)(?(DEFINE)(?<g>(?<x>c)))$", "acb", 0, 3, "0:3,0:3,-,1:1" )]
	[InlineData( @"^(?|(a(b))|(((?1(2)))c))$", "abc", 0, 3, "0:3,0:3,0:2" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a?)))(?&g(2))*b$", "aab", 0, 3, "0:3,-,2:0" )]
	[InlineData( @"^(?(DEFINE)(?<g>(?<x>a|ab)))(?:(?*(?&g(2)))\2d|abc)(?(x)X|)$", "abc", 0, 3, "0:3,-,-" )]
	public void ReturnedCapturesUseTheSharedJournalAcrossCallsRetriesAssertionsAndPruning( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Theory]
	[InlineData( @"^ab(?<=(?=(?&g(<x>)))ab)$(?(DEFINE)(?<g>(?<x>ab)))", "ab", 0, 2, "0:2,-,0:2" )]
	[InlineData( @"^ab(?<=(?!(?&g(<x>))c)ab)$(?(DEFINE)(?<g>(?<x>ab)))", "ab", 0, 2, "0:2,-,-" )]
	[InlineData( @"^ab(?<=(?(?=(?&g(<x>)))ab|cd))$(?(DEFINE)(?<g>(?<x>ab)))", "ab", 0, 2, "0:2,-,0:2" )]
	[InlineData( @"^ab(?<=(?(DEFINE)(?<q>(?&g(<x>))))ab)$(?(DEFINE)(?<g>(?<x>ab)))", "ab", 0, 2, "0:2,-,-,-" )]
	[InlineData( @"^ab(?<=(?&outer))$(?(DEFINE)(?<outer>(?=(?&g(<x>)))ab)(?<g>(?<x>ab)))", "ab", 0, 2, "0:2,-,-,-" )]
	[InlineData( @"^ab(?<=(?&outer))$(?(DEFINE)(?<outer>(?(DEFINE)(?<q>(?&g(<x>))))ab)(?<g>(?<x>ab)))", "ab", 0, 2, "0:2,-,-,-,-" )]
	public void BehindWidthSkipsAheadAssertionsAndDefineBodiesIncludingThroughOrdinaryCalls( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Theory]
	[InlineData( "(?1())" )]
	[InlineData( "(?1(,1))" )]
	[InlineData( "(?1(1,))" )]
	[InlineData( "(?1(1,,1))" )]
	[InlineData( "(?1(0))" )]
	[InlineData( "(?1(+0))" )]
	[InlineData( "(?1(-0))" )]
	[InlineData( "(?1(2))" )]
	[InlineData( "(?1(-2))" )]
	[InlineData( "(?1(65536))" )]
	[InlineData( "(?1(2147483648))" )]
	[InlineData( "(?1(١))" )]
	[InlineData( "(?1( 1))" )]
	[InlineData( "(?1(1 ))" )]
	[InlineData( "(?1(1, 1))" )]
	[InlineData( "(?1(1#comment\n))" )]
	[InlineData( "(?1(1) )" )]
	[InlineData( "(?1(1)" )]
	[InlineData( "(?1(<missing>))" )]
	[InlineData( "(?1('missing'))" )]
	[InlineData( "(?1(<>))" )]
	[InlineData( "(?1(''))" )]
	[InlineData( "(?1(<1>))" )]
	[InlineData( "(?1(<g))" )]
	[InlineData( "(?1('g))" )]
	[InlineData( "(?&g (1))" )]
	[InlineData( "(?P>g(1x))" )]
	[InlineData( @"\g<g(1)>" )]
	[InlineData( @"\g'g(1)'" )]
	public void InvalidListsReportTheOriginalCallOpeningEvenInExtendedMode( string header ) {
		foreach ( var extended in new[] { "", "(?x)" } ) {
			var prefix = extended + "é(?<g>a)";
			var pattern = prefix + header;
			Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
			Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, Encoding.UTF8.GetByteCount( prefix ) );
		}
	}

	[Theory]
	[InlineData( @"(?<=(?(DEFINE)(?<g>(?<=a*))))(?&g(1))", "(?&g" )]
	public void SuppressedBehindRemainsUnsupportedAtTheCall( string pattern, string opener ) {
		_ = Reference( pattern );
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, pattern.IndexOf( opener, StringComparison.Ordinal ) );
	}

	[Theory]
	[InlineData( @"(?(DEFINE)(?<g>(?<x>a)))(?<=(?&g(2)))b", "(?<=" )]
	[InlineData( @"(?(DEFINE)(?<g>(?<x>a)))(?<!(?&g(2)))b", "(?<!" )]
	[InlineData( @"(?(DEFINE)(?<g>(?<x>a)))(?<=(?&g(2)){0})b", "(?<=" )]
	[InlineData( @"^ab(?<=(?&outer))$(?(DEFINE)(?<outer>(?&g(<x>)))(?<g>(?<x>ab)))", "(?<=" )]
	[InlineData( @"^ab(?<=(?(VERSION>=99.0)(?&g(<x>))|ab))$(?(DEFINE)(?<g>(?<x>ab)))", "(?<=" )]
	public void ReturnedListsInsideBehindRemainUnsupportedAtTheOwningAssertion( string pattern, string opener ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, pattern.IndexOf( opener, StringComparison.Ordinal ) );
	}

	[Fact]
	public void OrdinaryCallsInsideBehindRetainTheirSupportedWidth() =>
		Check( @"(?(DEFINE)(?<g>(?<x>a)))(?<=(?&g))b", "ab", 1, 1, "1:1,-,-" );

	[Fact]
	public void DuplicateListsRetainAll256ResolvedSlotsWithoutDuplicateState() {
		var pattern = "(?J)(?(DEFINE)(?<g>" + string.Concat( Enumerable.Repeat( "(?<x>)", 255 ) )
			+ "))(?&g(<x>,256,<x>,1))";
		Check( pattern, "", 0, 0, "0:0,-," + string.Join( ",", Enumerable.Repeat( "0:0", 255 ) ) );
	}

	[Fact]
	public void ErasedAndUnselectedCallsStillResolveEveryReturnedReference() {
		foreach ( var pattern in new[] { "é(a)(?1(<missing>)){0}", "é(a)(?(VERSION>=99.0)(?1(2))|b)" } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
			var opening = pattern.IndexOf( "(?1", StringComparison.Ordinal );
			Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax,
				Encoding.UTF8.GetByteCount( pattern[..opening] ) );
		}
	}

	[Fact]
	public void ListsKeepTheNameCaptureDepthAndInstructionLimits() {
		var name = new string( '字', 42 ) + "aa";
		Check( "(?(DEFINE)(?<g>(?<" + name + ">a)))(?&g(<" + name + ">))", "a", 0, 1, "0:1,-,0:1" );
		var tooLong = "é(?<g>a)(?&g(<" + name + "x>))";
		Assert.Throws<PcrePatternException>( () => Reference( tooLong ) );
		Diagnostic( tooLong, PcreByteDiagnosticCode.InvalidSyntax, 9 );
		var declarations = "(?(DEFINE)" + string.Concat( Enumerable.Repeat( "()", 256 ) );
		Check( declarations + ")(?256(1,256))", "", 0, 0,
			"0:0," + string.Join( ",", Enumerable.Repeat( "-", 256 ) ) );
		_ = Reference( declarations + "())(?257(1))" );
		Diagnostic( declarations + "())(?257(1))", PcreByteDiagnosticCode.PatternCaptureLimitExceeded, declarations.Length );
		var nested = string.Concat( Enumerable.Repeat( "(?:", 255 ) ) + "(a)" + new string( ')', 255 ) + "(?1(1))";
		Check( nested, "aa", 0, 2, "0:2,0:1" );
		_ = Reference( "(?:" + nested + ")" );
		Diagnostic( "(?:" + nested + ")", PcreByteDiagnosticCode.PatternDepthLimitExceeded, 768 );
		var instructions = "(?(DEFINE)(?<g>" + string.Concat( Enumerable.Repeat( "[a]", 4096 ) ) + "))(?&g(1))";
		_ = Reference( instructions );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( instructions ), new PcreByteOptions { Utf8 = true } ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( "", 1024, true )]
	[InlineData( "", 1025, false )]
	[InlineData( "a|b", 512, true )]
	[InlineData( "a|b", 513, false )]
	public void ListsShareReturnedGuardsAndCalleeChoiceFrameLimits( string body, int count, bool succeeds ) {
		var pattern = "(?(DEFINE)(?<g>" + body + "))" + string.Concat( Enumerable.Repeat( "(?&g(1))", count ) );
		var subject = Encoding.ASCII.GetBytes( body.Length == 0 ? "" : new string( 'a', count ) );
		var reference = Reference( pattern );
		AssertNative( reference, subject, 0, 0, subject.Length, $"0:{subject.Length},-" );
		var actual = Compile( pattern ).Match( subject );
		Assert.Equal( succeeds, actual.IsSuccess );
		Assert.Equal( succeeds ? null : PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code );
		if ( succeeds ) { Compare( reference, subject, 0, actual, pattern ); }
	}

	[Fact]
	public void AtomicPruningReclaimsListReturnGuards() =>
		Check( @"^(?(DEFINE)(?<g>(?<x>a|b)))(?>(?&g(2)))+$", new string( 'a', 513 ), 0, 513, "0:513,-,512:1" );

	[Fact]
	public void ListCallsSpendSharedWorkAndHonorCancellation() {
		const string pattern = "^(?(DEFINE)(?<g>(?<x>a|aa)*))(?&g(2))b$";
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 16 ) );
		AssertNative( Reference( pattern ), subject, 0, -1, 0, "" );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, new PcreByteOptions { Utf8 = true, MatchWorkLimit = 40 } ).Match( subject ).Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions {
			Utf8 = true, WorkObserver = work => { if ( work == 40 ) { cancelled.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancelled.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.UTF8.GetBytes( pattern ), cancellationToken: cancelled.Token ) );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ListsKeepMalformedUtfCallerOffsetsGAndCrLfContinuationSemantics( bool anyCrLf ) {
		foreach ( var pattern in new[] { @"(?(DEFINE)(?<g>(?<x>.?)))(?&g(2))",
			@"(?(DEFINE)(?<g>(?<x>\G.?)))(?&g(2))", @"(?m)(?(DEFINE)(?<g>(?<x>.*)))(?&g(2))$",
			@"(?(DEFINE)(?<g>(?<x>\n)))(?s:(?&g(2))|.)\z" } ) {
			var reference = Reference( pattern, true, anyCrLf );
			var compiled = Compile( pattern, new PcreByteOptions { Utf8 = true, MatchInvalidUtf = true, AnyCrLfNewline = anyCrLf } );
			foreach ( var hex in new[] { "", "61FF62", "FFC3A9", "FFFF", "0D0A610D0A" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, {anyCrLf}" );
				}
			}
		}
	}

	private static void Diagnostic( string pattern, PcreByteDiagnosticCode code, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void Check( string pattern, string text, int index, int length, string captures ) {
		foreach ( var utf8 in pattern.All( character => character <= 127 ) ? new[] { false, true } : new[] { true } ) {
			var subject = ( utf8 ? Encoding.UTF8 : Encoding.Latin1 ).GetBytes( text );
			var reference = Reference( pattern, utf8: utf8 );
			AssertNative( reference, subject, 0, index, length, captures );
			Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions { Utf8 = utf8 } ).Match( subject ), pattern );
		}
	}

	private static void AssertNative( PcreRegex8Bit reference, byte[] subject, int offset, int index, int length, string captures ) {
		var native = reference.Match( subject, offset );
		Assert.Equal( index, native.Success ? native.Index : -1 );
		if ( !native.Success ) { return; }
		Assert.Equal( length, native.Length );
		var spans = captures.Split( ',' );
		Assert.Equal( spans.Length, native.Groups.Count );
		for ( var group = 0; group < spans.Length; group++ ) {
			var capture = native.Groups[group];
			Assert.Equal( spans[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
		}
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

	private static PcreRegex8Bit Reference( string pattern, bool invalidUtf = false, bool anyCrLf = false, bool utf8 = true ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = PcreOptions.Compiled | ( utf8 ? PcreOptions.Utf : PcreOptions.None )
				| ( invalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options ?? new PcreByteOptions { Utf8 = true } );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
