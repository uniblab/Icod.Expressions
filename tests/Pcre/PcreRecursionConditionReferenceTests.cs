using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Innermost nonrecursive call conditions against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreRecursionConditionReferenceTests {
	[Theory]
	[InlineData( @"^(?(R)a|b)(?(R0)c|d)(?(R000)e|f)$", "bdf", 0, 3, "0:3" )]
	[InlineData( @"^((?(R1)a|b))$", "b", 0, 1, "0:1,0:1" )]
	[InlineData( @"^(?(DEFINE)(?<a>(?(R)a|x)(?(R0)b|x)(?(R000)c|x)(?(R01)d|x)))(?&a)$", "abcd", 0, 4, "0:4,-" )]
	[InlineData( @"^(?(DEFINE)(?<a>(?&b)(?(R1)f|X))(?<b>(?(R1)X|b)(?(R2)c|X)(?(R&a)X|d)(?(R&b)e|X)))(?&a)$", "bcdef", 0, 5, "0:5,-,-" )]
	[InlineData( @"^(?(DEFINE)(?<a>a|ab))(?&a)(?(R)X|c)$", "abc", 0, 3, "0:3,-" )]
	[InlineData( @"^(?(DEFINE)(?<a>(?*(a|ab))(?(R1)\2|X)))(?&a)c$", "abc", 0, 3, "0:3,-,-" )]
	[InlineData( @"^(?(DEFINE)(?<a>(?=(a|ab))(?(R1)\2|X)))(?&a)c$", "abc", -1, 0, "" )]
	[InlineData( @"^(?(DEFINE)(?<a>(?>(a|ab))(?(R1)|X)))(?&a)c$", "abc", -1, 0, "" )]
	[InlineData( @"^(?(DEFINE)(?<a>(?<x>a)(?(R1)\Kx|X)|ab))(?&a)c(?(R)X|d)(?(x)X|e)$", "abcde", 0, 5, "0:5,-,-" )]
	[InlineData( @"^(?(DEFINE)(?<a>(?(R1)a\Kb|X)))(?&a)c$", "abc", 1, 2, "1:2,-" )]
	[InlineData( @"^(?(DEFINE)(?<a>a))(?!(?&a)b)(?(R)X|ac)$", "ac", 0, 2, "0:2,-" )]
	[InlineData( @"^(?(DEFINE)(?<a>a))(?=(?&a))(?(R)X|a)$", "a", 0, 1, "0:1,-" )]
	[InlineData( @"^(?(DEFINE)(?<a>a))(?:(?>(?&a))x|a)(?(R)X|b)$", "ab", 0, 2, "0:2,-" )]
	[InlineData( @"^(?(DEFINE)(?<a>a))(?(?=(?&a))a|X)(?(R)X|b)$", "ab", 0, 2, "0:2,-" )]
	[InlineData( @"^(?(DEFINE)(?<a>(?(R)b|a)))(?&a)$", "a", -1, 0, "" )]
	[InlineData( @"^(?(R)b|a)c$", "bc", -1, 0, "" )]
	public void OnlyCurrentCallSurvivesReturnRetryAndAssertionPruning( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Theory]
	[InlineData( @"(?J)^(?(DEFINE)(?<x>X)(?<x>(?(R&x)a|X)))(?2)$", "a", 0, 1, "0:1,-,-" )]
	[InlineData( @"(?J)^(?<R>a)?(?<R>b)(?(R)c|d)$", "bc", 0, 2, "0:2,-,0:1" )]
	[InlineData( @"^(?(R65536)b|c)(?<R65536>a)$", "ca", 0, 2, "0:2,1:1" )]
	[InlineData( @"^(?(R1)b|c)(?<R1>a)$", "ca", 0, 2, "0:2,1:1" )]
	[InlineData( @"^(?(DEFINE)(?<a>(?(R1)X|b))(?<R1>a))(?&a)$", "b", 0, 1, "0:1,-,-" )]
	[InlineData( @"^(?<R>a)(?(DEFINE)(?<g>(?(R&g)b|X)(?(<R>)c|X)(?('R')d|X)))(?&g)$", "abcd", 0, 4, "0:4,0:1,-" )]
	[InlineData( @"^(?<R>(?(R&R)a|b))(?&R)$", "ba", 0, 2, "0:2,0:1" )]
	[InlineData( @"^(?(DEFINE)(?<字٢>(?(R&字٢)é|x)))(?&字٢)$", "é", 0, 2, "0:2,-" )]
	[InlineData( @"^(?<R١>a)(?(R١)b|c)$", "ab", 0, 2, "0:2,0:1" )]
	[InlineData( @"^(?<r>a)(?(r)b|c)$", "ab", 0, 2, "0:2,0:1" )]
	[InlineData( @"^(?(R)(?<x>a)|(?<y>b))(?(y)c|X)$", "bc", 0, 2, "0:2,-,0:1" )]
	[InlineData( @"^(?(R)(?&x)|b)(?(DEFINE)(?<x>a))$", "b", 0, 1, "0:1,-" )]
	public void GlobalNamesWinBeforeNumericParsingAndForcedNamesUseEverySlot( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Theory]
	[InlineData( @"(?<=(?(R)a))b", "b", -1, 0, "" )]
	[InlineData( @"(?<=(?(R)a))b", "ab", 1, 1, "1:1" )]
	[InlineData( @"(?<=(?(R)a|))b", "b", 0, 1, "0:1" )]
	[InlineData( @"(?(DEFINE)(?<g>(?<=(?(R1)a|bb))b))(?&g)", "ab", 1, 1, "1:1,-" )]
	[InlineData( @"(?(DEFINE)(?<g>(?(R1)a|bb)))(?<=(?&g))c", "ac", 1, 1, "1:1,-" )]
	[InlineData( @"(?(DEFINE)(?<g>(?(R1)a|bb)))(?<=(?&g))c", "bbc", -1, 0, "" )]
	public void BehindWidthsUseAllExplicitBranchesWithoutFoldingTheCallTest( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Fact]
	public void OmittedElseAndExplicitEmptyElseRetainDifferentBehindLimits() {
		Check( @"(?<=(?(R)a{256}))b", new string( 'a', 256 ) + "b", 256, 1, "256:1" );
		Check( @"(?<=(?(R)a{255}|))b", "b", 0, 1, "0:1" );
		foreach ( var pattern in new[] { @"(?<=(?(R)a{256}|))b", @"(?(R)(?<=a*)|b){0}" } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
			Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, pattern.IndexOf( "(?<=", StringComparison.Ordinal ) );
		}
		const string missing = "é(?(R)(?&missing)|a){0}";
		Assert.Throws<PcrePatternException>( () => Reference( missing ) );
		Diagnostic( missing, PcreByteDiagnosticCode.InvalidSyntax, 7 );
	}

	[Theory]
	[InlineData( "R1" )]
	[InlineData( "R257" )]
	[InlineData( "R65536" )]
	[InlineData( "R2147483648" )]
	[InlineData( "R+1" )]
	[InlineData( "R-1" )]
	[InlineData( " R" )]
	[InlineData( "R " )]
	[InlineData( "R&" )]
	[InlineData( "R&missing" )]
	[InlineData( "R&1" )]
	[InlineData( "R&x " )]
	[InlineData( "R١" )]
	[InlineData( "<R>" )]
	[InlineData( "'R'" )]
	public void InvalidHeadersReportTheOriginalUtfConditionalOpening( string header ) {
		var pattern = "é(?(" + header + ")a|b)";
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, 2 );
	}

	[Fact]
	public void NamesUseTheExisting128UtfByteLimit() {
		var name = new string( '字', 42 ) + "aa";
		Check( "(?(DEFINE)(?<" + name + ">(?(R&" + name + ")a|b)))(?1)", "a", 0, 1, "0:1,-" );
		var pattern = "é(?(R&" + name + "x)a|b)";
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, 2 );
	}

	[Fact]
	public void ConditionsShareDepthCaptureAndInstructionCeilingsAcrossBothBodies() {
		const string header = "(?(R)";
		var nested = string.Concat( Enumerable.Repeat( header, 256 ) ) + "a" + new string( ')', 256 );
		Check( nested, "", 0, 0, "0:0" );
		Diagnostic( header + nested + ")", PcreByteDiagnosticCode.PatternDepthLimitExceeded, 1280 );
		var captures = string.Concat( Enumerable.Repeat( "()", 256 ) );
		Check( captures + "(?(R256)a|b)", "b", 0, 1, "0:1," + string.Join( ",", Enumerable.Repeat( "0:0", 256 ) ) );
		Diagnostic( captures + "()(?(R256)a|b)", PcreByteDiagnosticCode.PatternCaptureLimitExceeded, 512 );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( "(?(R)a{4096}|b)"u8.ToArray(), new PcreByteOptions { Utf8 = true } ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( 1024, true )]
	[InlineData( 1025, false )]
	public void ACommittedConditionAddsNoAlternativeToTheSavedPathBudget( int count, bool succeeds ) {
		var pattern = "(?(DEFINE)(?<g>(?(R))))" + string.Concat( Enumerable.Repeat( "(?&g)", count ) );
		var subject = Array.Empty<byte>();
		var reference = Reference( pattern );
		AssertNative( reference, subject, 0, 0, 0, "0:0,-" );
		var actual = Compile( pattern ).Match( subject );
		Assert.Equal( succeeds, actual.IsSuccess );
		Assert.Equal( succeeds ? null : PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code );
		if ( succeeds ) { Compare( reference, subject, 0, actual, pattern ); }
	}

	[Fact]
	public void DuplicateCallTargetsSpendSharedWorkAndHonorCancellation() {
		var pattern = "(?J)" + string.Concat( Enumerable.Repeat( "(?<x>){0}", 255 ) )
			+ "(?<x>(?(R&x)b|c)){0}(?256)";
		Check( pattern, "b", 0, 1, "0:1," + string.Join( ",", Enumerable.Repeat( "-", 256 ) ) );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, new PcreByteOptions { Utf8 = true, MatchWorkLimit = 40 } ).Match( "b"u8.ToArray() ).Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions {
			Utf8 = true, WorkObserver = work => { if ( work == 40 ) { cancelled.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( "b"u8.ToArray(), cancellationToken: cancelled.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.UTF8.GetBytes( pattern ), cancellationToken: cancelled.Token ) );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void MalformedUtfCallerOffsetsGAndCrLfKeepSharedContinuationSemantics( bool anyCrLf ) {
		foreach ( var pattern in new[] { @"(?(DEFINE)(?<g>(?(R)\G.?|X)))(?&g)(?(R)X|)",
			@"(?m)(?(DEFINE)(?<g>(?(R).?|X)))(?&g)(?(R)X|$)", @"(?(R)\n|.)\z", @"(?(R)a)*b" } ) {
			var reference = Reference( pattern, true, anyCrLf );
			var compiled = Compile( pattern, new PcreByteOptions { Utf8 = true, MatchInvalidUtf = true, AnyCrLfNewline = anyCrLf } );
			foreach ( var hex in new[] { "61FF62", "0D0A62" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, {anyCrLf}" );
				}
			}
		}
	}

	[Fact]
	public void InactiveConditionalBodiesStillContributeExplicitNewlineMetadata() {
		const string pattern = @"(?(R)\n)(?s:.)\z";
		var subject = "\r\n"u8.ToArray();
		var reference = Reference( pattern, anyCrLf: true );
		AssertNative( reference, subject, 0, 1, 1, "1:1" );
		Compare( reference, subject, 0,
			Compile( pattern, new PcreByteOptions { Utf8 = true, AnyCrLfNewline = true } ).Match( subject ), pattern );
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
