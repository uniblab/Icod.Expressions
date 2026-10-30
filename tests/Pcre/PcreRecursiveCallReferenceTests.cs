using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Recursive calls against PCRE.NET 1.6.0 / PCRE2 10.48 JIT (not the interpreter's loop heuristic).</summary>
public sealed class PcreRecursiveCallReferenceTests {
	[Theory]
	[InlineData( @"(?(DEFINE)(?R))", "", 0, 0, "0:0" )]
	[InlineData( @"(?(DEFINE)(?<a>(?&a)))a", "a", 0, 1, "0:1,-" )]
	[InlineData( @"(?(DEFINE)(?<a>(?&b))(?<b>(?&a)))a", "a", 0, 1, "0:1,-,-" )]
	[InlineData( @"(?R){0}a", "a", 0, 1, "0:1" )]
	[InlineData( @"(?(VERSION>=99.0)(?0)|a)", "a", 0, 1, "0:1" )]
	public void UnexecutedCyclesCompileWithoutStartingRecursion( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Theory]
	[InlineData( @"\A(?<a>a(?1)?b)\z", "aaabbb", 0, 6, "0:6,0:6" )]
	[InlineData( @"\A(?<a>a(?01)?b)\z", "aabb", 0, 4, "0:4,0:4" )]
	[InlineData( @"\A(?<a>a(?-1)?b)\z", "aabb", 0, 4, "0:4,0:4" )]
	[InlineData( @"\A(?<a>a(?&a)?b)\z", "aabb", 0, 4, "0:4,0:4" )]
	[InlineData( @"\A(?<a>a(?P>a)?b)\z", "aabb", 0, 4, "0:4,0:4" )]
	[InlineData( @"\A(?<a>a\g<1>?b)\z", "aabb", 0, 4, "0:4,0:4" )]
	[InlineData( @"\A(?<a>a\g<a>?b)\z", "aabb", 0, 4, "0:4,0:4" )]
	[InlineData( @"\A(?<a>a\g'1'?b)\z", "aabb", 0, 4, "0:4,0:4" )]
	[InlineData( @"\A(?<a>a\g'a'?b)\z", "aabb", 0, 4, "0:4,0:4" )]
	[InlineData( @"\A(?<a>a\g<-1>?b)\z", "aabb", 0, 4, "0:4,0:4" )]
	[InlineData( @"\A(?<a>a\g'-1'?b)\z", "aabb", 0, 4, "0:4,0:4" )]
	[InlineData( @"\A(?&a)\z(?(DEFINE)(?<a>a(?+1)?b)(?<b>c(?-2)?d))", "acabbdb", -1, 0, "" )]
	[InlineData( @"\A(?+1)\z(?(DEFINE)(?<a>a(?+1)?b)(?<b>c(?-2)?d))", "acabdb", 0, 6, "0:6,-,-" )]
	[InlineData( @"\A\g<+1>\z(?(DEFINE)(?<a>a\g<+1>?b)(?<b>c\g<-2>?d))", "acabdb", 0, 6, "0:6,-,-" )]
	[InlineData( @"\A\g'+1'\z(?(DEFINE)(?<a>a\g'+1'?b)(?<b>c\g'-2'?d))", "acabdb", 0, 6, "0:6,-,-" )]
	[InlineData( @"\A(?&a)\z(?(DEFINE)(?<a>a(?&b)?b)(?<b>c(?&a)?d))", "acabdb", 0, 6, "0:6,-,-" )]
	[InlineData( @"(?J)\A(?&a)\z(?(DEFINE)(?<a>a(?&a)?b)(?<a>z))", "aabb", 0, 4, "0:4,-,-" )]
	[InlineData( @"(?J)\A(?&a)\z(?(DEFINE)(?<a>a(?&a)?b)(?<a>z))", "z", -1, 0, "" )]
	[InlineData( @"\A(?|(?<a>a(?1)?b)|(?<a>z))(?1)\z", "zaabb", 0, 5, "0:5,0:1" )]
	[InlineData( @"\A(?|(?<a>a(?&a)?b)|(?<a>z))(?&a)\z", "zz", -1, 0, "" )]
	[InlineData( @"\A(?&字٢)\z(?(DEFINE)(?<字٢>é(?&字٢)?x))", "ééxx", 0, 6, "0:6,-" )]
	[InlineData( @"\A\g<R>\z(?(DEFINE)(?<R>a\g<R>?b))", "aabb", 0, 4, "0:4,-" )]
	public void EveryExistingCallSpellingResolvesCyclesAndUsesTheFirstDefinition( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Theory]
	[InlineData( @"\((?:[^()]++|(?R))*\)", "(ab(cd)ef)", 0, 10, "0:10" )]
	[InlineData( @"\((?:[^()]++|(?0))*\)", "(ab(cd)ef)", 0, 10, "0:10" )]
	[InlineData( @"\((?:[^()]++|(?000))*\)", "(ab(cd)ef)", 0, 10, "0:10" )]
	[InlineData( @"\((?:[^()]++|\g<0>)*\)", "(ab(cd)ef)", 0, 10, "0:10" )]
	[InlineData( @"\((?:[^()]++|\g'0')*\)", "(ab(cd)ef)", 0, 10, "0:10" )]
	[InlineData( @"\((?:[^()]++|\g'000')*\)", "(ab(cd)ef)", 0, 10, "0:10" )]
	[InlineData( @"\((?R)?\)", "((()))", 0, 6, "0:6" )]
	[InlineData( @"\((?R)*\)", "(()())", 0, 6, "0:6" )]
	[InlineData( @"^a(?R)?b$", "aabb", -1, 0, "" )]
	[InlineData( @"<(?:(?<x>[a-z])|(?R))*>", "<a<b>>", 0, 6, "0:6,1:1" )]
	[InlineData( @"<(?:(?<x>[a-z])|(?R(<x>)))*>", "<a<b>>", 0, 6, "0:6,3:1" )]
	[InlineData( @"<(?:(?<x>[a-z])|(?0('x')))*>", "<a<b>>", 0, 6, "0:6,3:1" )]
	[InlineData( @"<(?:(?<x>[a-z])|(?000(1,1)))*>", "<a<b>>", 0, 6, "0:6,3:1" )]
	[InlineData( @"a(?R)?\Kb", "aabb", 3, 1, "3:1" )]
	public void WholePatternCallsReenterTheRootAndRestoreOrReturnSelectedCaptures( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Theory]
	[InlineData( @"^((.)(?1)\2|.?)$", "abba", 0, 4, "0:4,0:4,0:1" )]
	[InlineData( @"^((.)(?1)\2|.?)$", "abcba", 0, 5, "0:5,0:5,0:1" )]
	[InlineData( @"^((.)(?1)\2|.?)$", "abca", -1, 0, "" )]
	[InlineData( @"\A(.)(\1|a(?2))\z", "bab", 0, 3, "0:3,0:1,1:2" )]
	[InlineData( @"\A(?<a>a(?:(?<x>b)|(?&a))c)\z", "aabcc", 0, 5, "0:5,0:5,-" )]
	[InlineData( @"\A(?<a>a(?:(?<x>b)|(?&a(<x>)))c)\z", "aabcc", 0, 5, "0:5,0:5,2:1" )]
	[InlineData( @"\A(?<a>a(?:(?<x>b)|(?&a(<x>))|d)c)\z", "aadcc", 0, 5, "0:5,0:5,-" )]
	[InlineData( @"\A(?&a(<x>))\z(?(DEFINE)(?<a>a(?:(?<x>b)|(?&a(<x>)))c))", "aaabccc", 0, 7, "0:7,-,3:1" )]
	[InlineData( @"\A(?&a(<x>))\z(?(DEFINE)(?<a>a(?:(?<x>b)|(?&a))c))", "aabcc", 0, 5, "0:5,-,-" )]
	[InlineData( @"\A(?&a(<x>))\z(?(DEFINE)(?<a>a(?:(?<x>b)|(?&a(<x>)))\Kc))", "aabcc", 4, 1, "4:1,-,2:1" )]
	[InlineData( @"\A(?(DEFINE)(?<a>a(?:(?<x>b)|(?&a(<x>)))\Kc))(?:(?&a(<x>))d|aabcce)(?(x)X|)\z", "aabcce", 0, 6, "0:6,-,-" )]
	[InlineData( @"\A(?<a>a(?&a)?\Kb)\z", "aabb", 3, 1, "3:1,0:4" )]
	[InlineData( @"\A(?:(?<a>a(?&a)?\Kb)c|aabbd)\z", "aabbd", 0, 5, "0:5,-" )]
	[InlineData( @"\A(?&a)\z(?(DEFINE)(?i:(?<a>a(?&a)?b)))", "aABb", 0, 4, "0:4,-" )]
	[InlineData( @"\A(?i:(?&a))\z(?(DEFINE)(?<a>a(?&a)?b))", "aABb", -1, 0, "" )]
	[InlineData( @"\A(?&a)\z(?(DEFINE)(?<a>a(?&a)?b|ab))", "aabb", 0, 4, "0:4,-" )]
	public void NonAtomicRecursionRetriesAndUsesTheSharedCaptureAndResetJournal( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Theory]
	[InlineData( @"\A(?&a)\z(?(DEFINE)(?<a>(?(<x>)a|(?<x>)(?&a))))", "a", 0, 1, "0:1,-,-" )]
	[InlineData( @"\A(?&a(<x>))\z(?(DEFINE)(?<a>(?(<x>)a|(?<x>)(?&a))))", "a", 0, 1, "0:1,-,0:0" )]
	[InlineData( @"\A(?&a)\z(?(DEFINE)(?<a>(?(R&a)a|X)(?&b)?(?(R1)b|X))(?<b>(?(R&a)X|c)(?(R&b)d|X)))", "acdb", 0, 4, "0:4,-,-" )]
	[InlineData( @"\A(?&a)\z(?(DEFINE)(?<a>a(?&a)?(?(R0)b|X)(?(R1)c|X)))", "aabcbc", 0, 6, "0:6,-" )]
	[InlineData( @"(?(R)a|b(?R)c)", "bac", 0, 3, "0:3" )]
	[InlineData( @"(?(R000)a|b(?0)c)", "bac", 0, 3, "0:3" )]
	[InlineData( @"(?(R1)X|(?(R0)a|b(?R)c))(?<x>){0}", "bac", 0, 3, "0:3,-" )]
	[InlineData( @"\A(?&a)(?(R)X|z)\z(?(DEFINE)(?<a>a(?&a)?b))", "aabbz", 0, 5, "0:5,-" )]
	public void CaptureChangingSamePositionRecursionIsValidAndConditionsUseTheInnermostCall( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Theory]
	[InlineData( @"\A(?=(?<a>a(?&a)?b))aabb\z", "aabb", 0, 4, "0:4,0:4" )]
	[InlineData( @"\A(?!(?<a>a(?&a)?b)c)aabbd\z", "aabbd", 0, 5, "0:5,-" )]
	[InlineData( @"\A(?(DEFINE)(?<a>a(?:(?<x>b)|(?&a(<x>)))c))(?=(?&a(<x>)))aabcc(?(x)d|X)\z", "aabccd", 0, 6, "0:6,-,2:1" )]
	[InlineData( @"\A(?(DEFINE)(?<a>a(?:(?<x>b)|(?&a(<x>)))c))(?!(?&a(<x>))d)aabcce(?(x)X|)\z", "aabcce", 0, 6, "0:6,-,-" )]
	[InlineData( @"\A(?>(?<a>a(?&a)?b))c\z", "aabbc", 0, 5, "0:5,0:4" )]
	[InlineData( @"\A(?*(?<a>a(?&a)?b|aabbc))\1d\z", "aabbcd", 0, 6, "0:6,0:5" )]
	[InlineData( @"\A(?=(?<a>a(?&a)?b|aabbc))\1d\z", "aabbcd", -1, 0, "" )]
	[InlineData( @"\A(?>(?<a>a(?&a)?b|aabbc))d\z", "aabbcd", -1, 0, "" )]
	[InlineData( @"\A(?:(?*(?<a>a(?&a)?b|aabbc))\1d|aabbce)(?(a)X|)\z", "aabbce", 0, 6, "0:6,-" )]
	[InlineData( @"\A(?&a)\z(?(DEFINE)(?<a>a(?<=a)(?&a)?b))", "aabb", 0, 4, "0:4,-" )]
	[InlineData( @"\Aaabb(?<=(?=(?&a))aabb)\z(?(DEFINE)(?<a>a(?&a)?b))", "aabb", 0, 4, "0:4,-" )]
	[InlineData( @"(?<=(?=(?<a>a(?&a)?b))a)b", "ab", 1, 1, "1:1,0:2" )]
	[InlineData( @"(?<=(?(DEFINE)(?<a>(?R)))a)x", "ax", 1, 1, "1:1,-" )]
	public void RecursionUsesSharedAtomicAssertionAndFixedBehindContinuationRules( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Theory]
	[InlineData( @"(?<=(?<a>a(?&a)?b))x", "(?<=" )]
	[InlineData( @"(?<=(?R))x", "(?<=" )]
	[InlineData( @"(?<=(?0){0})x", "(?<=" )]
	[InlineData( @"(?(DEFINE)(?<a>a(?&a)?b))(?<=(?&a))x", "(?<=" )]
	[InlineData( @"(?(DEFINE)(?<a>(?&b))(?<b>a(?&b)?b))(?<!(?&a))x", "(?<!" )]
	public void WidthCyclesRemainUnsupportedAtTheirOwningBehind( string pattern, string opener ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, pattern.IndexOf( opener, StringComparison.Ordinal ) );
	}

	[Theory]
	[InlineData( "(?R0)" )]
	[InlineData( "(?R00)" )]
	[InlineData( "(?Rr)" )]
	[InlineData( "(?R )" )]
	[InlineData( "(?R(0))" )]
	[InlineData( "(?0())" )]
	[InlineData( "(?0(+0))" )]
	[InlineData( "(?+0)" )]
	[InlineData( "(?-0)" )]
	[InlineData( @"\g<+0>" )]
	[InlineData( @"\g'-0'" )]
	public void OnlyExactRAndAbsoluteZeroHeadersAreValid( string header ) {
		var pattern = "é(a)" + header;
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, 5 );
	}

	[Theory]
	[InlineData( @"\A(?<a>(?&a)|a)\z" )]
	[InlineData( @"(?R)|a" )]
	[InlineData( @"\A(?&a)\z(?(DEFINE)(?<a>(?&b)|a)(?<b>(?&a)|b))" )]
	public void UnproductiveCyclesReportNativeResourceErrorsAndProductDepthLimits( string pattern ) {
		var subject = "a"u8.ToArray();
		var native = Assert.Throws<PcreMatchException>( () => Reference( pattern ).Match( subject ) );
		Assert.Equal( PcreErrorCode.JitStackLimit, native.ErrorCode );
		var actual = Compile( pattern ).Match( subject );
		Assert.False( actual.IsSuccess );
		Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code );
		Assert.Null( actual.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, new PcreByteOptions { Utf8 = true, MatchWorkLimit = 40 } ).Match( subject ).Diagnostic?.Code );
	}

	[Fact]
	public void RecursiveCallsKeepSharedSourceCaptureAndInstructionLimits() {
		var name = new string( '字', 42 ) + "aa";
		Check( "\\A(?&" + name + ")\\z(?(DEFINE)(?<" + name + ">a(?&" + name + ")?b))",
			"aabb", 0, 4, "0:4,-" );
		var tooLong = "é(?<a>a)(?&" + name + "x)";
		Assert.Throws<PcrePatternException>( () => Reference( tooLong ) );
		Diagnostic( tooLong, PcreByteDiagnosticCode.InvalidSyntax, 9 );
		var declarations = "(?(DEFINE)" + string.Concat( Enumerable.Repeat( "()", 255 ) );
		Check( declarations + "(?<a>a(?256)?b))(?256)", "aabb", 0, 4,
			"0:4," + string.Join( ",", Enumerable.Repeat( "-", 256 ) ) );
		var tooMany = declarations + "()(?<a>a(?257)?b))(?257)";
		_ = Reference( tooMany );
		Diagnostic( tooMany, PcreByteDiagnosticCode.PatternCaptureLimitExceeded, declarations.Length + 2 );
		var nested = string.Concat( Enumerable.Repeat( "(?:", 255 ) ) + "(a(?1)?b)" + new string( ')', 255 );
		Check( nested, "aabb", 0, 4, "0:4,0:4" );
		_ = Reference( "(?:" + nested + ")" );
		Diagnostic( "(?:" + nested + ")", PcreByteDiagnosticCode.PatternDepthLimitExceeded, 768 );
		var instructions = "(?(DEFINE)(?<a>" + string.Concat( Enumerable.Repeat( "[a]", 4096 ) ) + "(?&a)?b))(?&a)";
		_ = Reference( instructions );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( instructions ) ).Diagnostic?.Code );
	}

	[Fact]
	public void ModestSuccessfulNativeDepthAndProductOnlyLargeDepthUseBoundedSavedFrames() {
		const string pattern = @"\A(?&a)\z(?(DEFINE)(?<a>a(?&a)?b))";
		Check( pattern, new string( 'a', 32 ) + new string( 'b', 32 ), 0, 64, "0:64,-" );
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 1200 ) + new string( 'b', 1200 ) );
		Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, Compile( pattern ).Match( subject ).Diagnostic?.Code );
	}

	[Fact]
	public void RecursionHonorsCancellationDuringMatchingAndCompilation() {
		const string pattern = @"(?R)|a";
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions {
			Utf8 = true, WorkObserver = work => { if ( work == 40 ) { cancelled.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( "a"u8.ToArray(), cancellationToken: cancelled.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.UTF8.GetBytes( pattern ), cancellationToken: cancelled.Token ) );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void RecursiveCallsKeepMalformedUtfCallerOffsetsGAndCrLfContinuationSemantics( bool anyCrLf ) {
		foreach ( var pattern in new[] { @"(?(DEFINE)(?<a>.(?&a)?))(?&a)",
			@"(?(DEFINE)(?<a>\G.(?&a)?))(?&a)", @"(?m)(?(DEFINE)(?<a>.(?&a)?))(?&a)$",
			@"(?(DEFINE)(?<a>\n(?&a)?))(?s:(?&a)|.)\z", @"(?:a(?R)?b|.)\z" } ) {
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
		options ??= new PcreByteOptions { Utf8 = true };
		var encoding = options.Utf8 || options.MatchInvalidUtf ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
