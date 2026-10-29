using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Nonrecursive calls against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreSubroutineReferenceTests {
	[Theory]
	[InlineData( @"^(a|ab)(?1)c$", "aabc", 0, 4, 2 )]
	[InlineData( @"^(a)(?01)\1$", "aaa", 0, 3, 2 )]
	[InlineData( @"^(?+1)(a)$", "aa", 0, 2, 2 )]
	[InlineData( @"^((?+1)(b))$", "bb", 0, 2, 3 )]
	[InlineData( @"^(a)(b)(?-2)$", "aba", 0, 3, 3 )]
	[InlineData( @"^(a)(?+01)(b)$", "abb", 0, 3, 3 )]
	[InlineData( @"^(?<a>a)(?P>a)$", "aa", 0, 2, 2 )]
	[InlineData( @"^\g<+1>(a)$", "aa", 0, 2, 2 )]
	[InlineData( @"^(a)\g<01>$", "aa", 0, 2, 2 )]
	[InlineData( @"^(a)(b)\g<-2>$", "aba", 0, 3, 3 )]
	[InlineData( @"^(?<a>a)\g<a>$", "aa", 0, 2, 2 )]
	[InlineData( @"^\g'a'(?'a'x)$", "xx", 0, 2, 2 )]
	[InlineData( @"^(a)\g'1'$", "aa", 0, 2, 2 )]
	[InlineData( @"^(a)\g'+1'(b)$", "abb", 0, 3, 3 )]
	[InlineData( @"^(a)\g'-1'$", "aa", 0, 2, 2 )]
	[InlineData( @"^(?|(a)|(b))(?1)$", "ba", 0, 2, 2 )]
	[InlineData( @"^(?|(a)|(b))(?-1)$", "ba", 0, 2, 2 )]
	[InlineData( @"^(?J)(?:(?<x>a)|(?<x>b))(?&x)$", "ba", 0, 2, 3 )]
	[InlineData( @"^(?J)(?|(?<x>a)|(?<x>b))(?&x)$", "ba", 0, 2, 2 )]
	[InlineData( @"^(abc)(?i:(?-1))$", "abcABC", -1, 0, 0 )]
	[InlineData( @"^(?i:(abc))(?-1)$", "ABCABC", 0, 6, 2 )]
	[InlineData( @"^(?(DEFINE)(?i:(?<x>a)))(?-i:(?&x))$", "A", 0, 1, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>(?i)a))(?&x)b$", "Ab", 0, 2, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>(?i)a))(?&x)b$", "AB", -1, 0, 0 )]
	public void SpellingsForwardResolutionFirstDefinitionsAndDefinitionOptions( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( @"^(?(DEFINE)(?<x>(?<y>a)))(?&x)(?(y)b|c)$", "ac", 0, 2, 3 )]
	[InlineData( @"^(?(DEFINE)(?<x>(?(x)a|b)))(?&x)(?(x)c|d)$", "bd", 0, 2, 2 )]
	[InlineData( @"^(?<x>(?(x)a|b))(?&x)\k<x>$", "bab", 0, 3, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>()a))(?&x)(?(2)x|c)$", "ac", 0, 2, 3 )]
	[InlineData( @"^(?(DEFINE)(?<x>(a)?b))(?&x)(?(2)x|c)$", "bc", 0, 2, 3 )]
	[InlineData( @"^(?(DEFINE)(?<x>(?<y>a)\k<y>))(?&x)$", "aa", 0, 2, 3 )]
	[InlineData( @"^(?<b>x)(?(DEFINE)(?<a>\k<b>))(?&a)$", "xx", 0, 2, 3 )]
	[InlineData( @"^(?<x>(a)?b)(?&x)(?(2)a|c)$", "abba", 0, 4, 3 )]
	[InlineData( @"^(?<x>()a)(?&x)(?(2)b|c)$", "aab", 0, 3, 3 )]
	[InlineData( @"^(?<x>(?<y>a|b)\k<y>)(?&x)\k<y>$", "aabba", 0, 5, 3 )]
	[InlineData( @"^(?:(?(DEFINE)(?<a>(?<b>a)))(?&a)b|a)(?(b)x|c)$", "ac", 0, 2, 3 )]
	[InlineData( @"^(?(DEFINE)(?<x>a|ab))(?&x)c$", "abc", 0, 3, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>(a|ab)\2))(?&x)c$", "ababc", 0, 5, 3 )]
	[InlineData( @"^(?(DEFINE)(?<x>(?<y>a)|(?<z>ab)))(?&x)c(?(y)y|n)(?(z)z|n)$", "abcnn", 0, 5, 4 )]
	[InlineData( @"^(?(DEFINE)(?<x>a|ab))(?>(?&x))c$", "abc", -1, 0, 0 )]
	[InlineData( @"^(?(DEFINE)(?<x>a|ab))(?&x)++c$", "abc", -1, 0, 0 )]
	[InlineData( @"^(?(DEFINE)(?<x>a?))(?&x){2}b$", "aab", 0, 3, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>a?))(?&x)*b$", "aab", 0, 3, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>(?&y)+)(?<y>a?))(?&x)b$", "aab", 0, 3, 3 )]
	[InlineData( @"^(?(DEFINE)(?<x>(?&y))(?<y>(?&z))(?<z>a))(?&x)$", "a", 0, 1, 4 )]
	[InlineData( @"^(?(DEFINE)(?<x>))(?&x)*$", "", 0, 0, 2 )]
	[InlineData( @"^(?<x>a){0}(?&x)$", "a", 0, 1, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>a\Kb))(?&x)c$", "abc", 1, 2, 2 )]
	[InlineData( @"^(?<x>a\Kb)(?&x)c$", "ababc", 3, 2, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>a\Kb|ac))(?&x)d$", "acd", 0, 3, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>a\Kb))(?:(?&x)c|abd)$", "abd", 0, 3, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>a))(?<=(?=(?&x)))a$", "a", 0, 1, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>a|ab))(?*((?&x)))\2c$", "abc", 0, 3, 3 )]
	[InlineData( @"^(?(DEFINE)(?<x>a|ab))(?=((?&x)))\2c$", "abc", -1, 0, 0 )]
	[InlineData( @"^(?(DEFINE)(?<x>(?<y>a)))(?!(?&x)b)ac(?(y)x|)$", "ac", 0, 2, 3 )]
	[InlineData( @"^(?(DEFINE)(?<x>a|ab))(?(?=(?&x)c)abc|d)$", "abc", 0, 3, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>(?*(a|ab))\2))(?&x)c$", "abc", 0, 3, 3 )]
	[InlineData( @"^(?(DEFINE)(?<x>a|ab))(?:(?>(?&x))d|abc)$", "abc", 0, 3, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>a|ab))(?:(?*((?&x)))\2d|abc)$", "abc", 0, 3, 3 )]
	public void ReturnsRestoreCapturesAndRepeatMarksWhileChoicesAndResetStartPersist( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( "(?2)" )]
	[InlineData( "(?+2)" )]
	[InlineData( "(?-2)" )]
	[InlineData( "(?257)" )]
	[InlineData( "(?&missing)" )]
	[InlineData( "(?P>missing)" )]
	[InlineData( @"\g<missing>" )]
	[InlineData( "(?+0)" )]
	[InlineData( "(?-0)" )]
	[InlineData( @"\g<+0>" )]
	[InlineData( "(?1 )" )]
	[InlineData( "(?&1)" )]
	[InlineData( "(?P>x )" )]
	[InlineData( "(?+01x)" )]
	[InlineData( @"\g<1a>" )]
	[InlineData( @"\g< 1>" )]
	[InlineData( @"\g'1" )]
	[InlineData( "(?１)" )]
	[InlineData( @"\g<１>" )]
	public void MissingTargetsAndMalformedHeadersReportTheirOriginalUtfByteOpener( string header ) {
		var pattern = "é(a)" + header;
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, 5 );
	}

	[Theory]
	[InlineData( "(a)(?R)", "(?R)" )]
	[InlineData( "(a)(?000)", "(?000)" )]
	[InlineData( @"(a)\g<0>", @"\g<0>" )]
	[InlineData( @"(a)\g'0'", @"\g'0'" )]
	[InlineData( "(?(DEFINE)(?<x>(?&x)))a", "(?&x)" )]
	[InlineData( "(?(DEFINE)(?<x>(?&y))(?<y>(?&x)))a", "(?&y)" )]
	[InlineData( "(?(DEFINE)(?<x>a))(?&x(<x>))", "(?&x" )]
	[InlineData( "(a)(?1(1))", "(?1" )]
	public void NativeAcceptedPendingFamiliesStayUnsupportedAtTheirCallOpener( string pattern, string opener ) {
		_ = Reference( pattern );
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, pattern.IndexOf( opener, StringComparison.Ordinal ) );
	}

	[Theory]
	[InlineData( @"(?<x>a)(?<=\g<x>)b", 7 )]
	[InlineData( @"(?(DEFINE)(?<x>a{1,2}))aa(?<=(?&x))b", 25 )]
	[InlineData( @"(?(DEFINE)(?<x>a))(?<*(?&x))b", 18 )]
	public void ConsumedCallsRemainUnsupportedAtTheOwningBehind( string pattern, int offset ) {
		_ = Reference( pattern );
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, offset );
	}

	[Fact]
	public void CalledDefinitionsCannotReuseSkippedGeneralBehindMetadata() {
		const string pattern = "(?<=(?(DEFINE)(?<x>(?<=a*))))(?&x)";
		_ = Reference( pattern ); // Native accepts this DEFINE-in-behind exception.
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, pattern.LastIndexOf( "(?&", StringComparison.Ordinal ) );
	}

	[Fact]
	public void NamesCaptureSlotsDepthAndInstructionsRetainTheirSourceLimits() {
		var name = new string( 'x', 128 );
		Check( "(?(DEFINE)(?<" + name + ">a))(?&" + name + ")", "a", 0, 1, 2 );
		var tooLong = "é(a)(?&" + name + "x)";
		Assert.Throws<PcrePatternException>( () => Reference( tooLong ) );
		Diagnostic( tooLong, PcreByteDiagnosticCode.InvalidSyntax, 5 );
		var declarations = "(?(DEFINE)" + string.Concat( Enumerable.Repeat( "()", 256 ) );
		Check( declarations + ")(?256)", "", 0, 0, 257 );
		_ = Reference( declarations + "())(?257)" );
		Diagnostic( declarations + "())(?257)", PcreByteDiagnosticCode.PatternCaptureLimitExceeded, declarations.Length );
		var nested = string.Concat( Enumerable.Repeat( "(?:", 255 ) ) + "(a)" + new string( ')', 255 ) + "(?1)";
		Check( nested, "aa", 0, 2, 2 );
		_ = Reference( "(?:" + nested + ")" );
		Diagnostic( "(?:" + nested + ")", PcreByteDiagnosticCode.PatternDepthLimitExceeded, 768 );
		foreach ( var count in new[] { 2048, 4096 } ) {
			var instructions = "(?(DEFINE)(?<g>" + string.Concat( Enumerable.Repeat( "[a]", count ) ) + "))(?&g)";
			_ = Reference( instructions );
			Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
				PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( instructions ), Options() ).Diagnostic?.Code );
		}
	}

	[Theory]
	[InlineData( "", 1024, true )]
	[InlineData( "", 1025, false )]
	[InlineData( "a|b", 512, true )]
	[InlineData( "a|b", 513, false )]
	public void ReturnedGuardsAndCalleeChoicesShareTheSavedPathCeiling( string body, int count, bool succeeds ) {
		var pattern = "(?(DEFINE)(?<g>" + body + "))" + string.Concat( Enumerable.Repeat( "(?&g)", count ) );
		var subject = Encoding.ASCII.GetBytes( body.Length == 0 ? "" : new string( 'a', count ) );
		var native = Reference( pattern ).Match( subject );
		Assert.True( native.Success );
		Assert.Equal( subject.Length, native.Length );
		Assert.Equal( 2, native.Groups.Count );
		Assert.False( native.Groups[1].Success );
		var result = Compile( pattern ).Match( subject );
		Assert.Equal( succeeds, result.IsSuccess );
		Assert.Equal( succeeds ? null : PcreByteDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code );
		if ( succeeds ) { Compare( Reference( pattern ), subject, 0, result, pattern ); }
	}

	[Fact]
	public void AtomicPruningReclaimsReturnedGuardsAndCalleeChoices() =>
		Check( @"^(?(DEFINE)(?<g>a|b))(?>(?&g))+$", new string( 'a', 800 ), 0, 800, 2 );

	[Fact]
	public void MalformedUtfCallerOffsetsCrLfAndGUseTheSharedMatcher() {
		foreach ( var anyCrLf in new[] { false, true } ) {
			foreach ( var pattern in new[] { @"(?(DEFINE)(?<g>.?))(?&g)", @"(?(DEFINE)(?<g>\G.?))(?&g)",
				@"(?m)(?(DEFINE)(?<g>.*))(?&g)$", @"(?(DEFINE)(?<g>\n))(?s:(?&g)|.)\z" } ) {
				var reference = Reference( pattern, invalidUtf: true, anyCrLf: anyCrLf );
				var compiled = Compile( pattern, Options( invalidUtf: true, anyCrLf: anyCrLf ) );
				foreach ( var hex in new[] { "", "61FF62", "FFC3A9", "FFFF", "0D0A610D0A" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
					}
				}
			}
		}
	}

	[Fact]
	public void NestedCallRestorationDoesNotDuplicateJournalHistoryExponentially() {
		var pattern = "^(?(DEFINE)";
		for ( var index = 0; index < 20; index++ ) {
			pattern += "(?<g" + index + ">()" + ( index == 19 ? "a" : "(?&g" + ( index + 1 ) + ")" ) + ")";
		}
		Check( pattern + ")(?&g0)$", "a", 0, 1, 41 );
	}

	[Fact]
	public void CallsRemainBoundedBySharedWorkAndCancellation() {
		const string pattern = "^(?(DEFINE)(?<g>(a|aa)*))(?&g)b$";
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 16 ) );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, Options() with { MatchWorkLimit = 40 } ).Match( subject ).Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( pattern, Options() with { WorkObserver = work => { if ( work == 40 ) { cancelled.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancelled.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options(), cancelled.Token ) );
	}

	private static void Diagnostic( string pattern, PcreByteDiagnosticCode code, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void Check( string pattern, string text, int index, int length, int groups ) {
		foreach ( var utf8 in new[] { false, true } ) {
			var subject = ( utf8 ? Encoding.UTF8 : Encoding.Latin1 ).GetBytes( text );
			var reference = Reference( pattern, utf8: utf8 );
			var native = reference.Match( subject );
			Assert.Equal( index, native.Success ? native.Index : -1 );
			if ( native.Success ) { Assert.Equal( length, native.Length ); Assert.Equal( groups, native.Groups.Count ); }
			Compare( reference, subject, 0, Compile( pattern, Options( utf8: utf8 ) ).Match( subject ), pattern );
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
