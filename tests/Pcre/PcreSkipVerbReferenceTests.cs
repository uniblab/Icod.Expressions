using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pinned PCRE2 10.48 JIT SKIP restart, scope catches and byte captures.</summary>
public sealed class PcreSkipVerbReferenceTests {
	[Theory]
	[InlineData( "a(*SKIP)b", "7878616162", "3:2", 0 )]
	[InlineData( "a+(*SKIP)b", "616163616162", "3:3", 0 )]
	[InlineData( "\\A(?:a(*SKIP)b|ac)", "6163", "", 0 )]
	[InlineData( "(?:a(*SKIP)b|ac)", "6163", "", 0 )]
	[InlineData( "(?:a(*SKIP)b|ac)", "61636162", "2:2", 0 )]
	[InlineData( "(?:a(*SKIP)b|ac)", "6162", "0:2", 0 )]
	[InlineData( "(a|ab)(*SKIP)c", "616263", "", 0 )]
	[InlineData( "(a|ab)c", "616263", "0:3,0:2", 0 )]
	[InlineData( "a(*SKIP)(b|bc)c", "61626363", "0:3,1:1", 0 )]
	[InlineData( "(a)(*SKIP)X|(b)", "6162", "1:1,-,1:1", 0 )]
	[InlineData( "\\Q(*SKIP)\\E", "282A5052554E4529", "", 0 )]
	[InlineData( "[(*SKIP)]", "50", "0:1", 0 )]
	[InlineData( "(?x)a (*SKIP) # tail\n b", "6162", "0:2", 0 )]
	[InlineData( "(?i)a(*SKIP)b", "4142", "0:2", 0 )]
	[InlineData( "(?i)(é)(*SKIP)X|é", "C3A9C3A9", "", 1 )]
	[InlineData( "a\\K(*SKIP)X|(b)", "6162", "1:1,1:1", 0 )]
	[InlineData( "(*NOTEMPTY)(*SKIP)|a", "61", "", 0 )]
	[InlineData( "(*SKIP)(*ACCEPT)|a", "61", "0:0", 0 )]
	[InlineData( "(*NOTEMPTY)(*SKIP)(*ACCEPT)|a", "61", "", 0 )]
	[InlineData( "\\A(?:(?>a(*SKIP))b|ac)", "6163", "0:2", 0 )]
	[InlineData( "\\A(?:(?>a(*SKIP)b)|ac)", "6163", "", 0 )]
	[InlineData( "\\A(?=(a(*SKIP)))ac|ac", "6163", "0:2,0:1", 0 )]
	[InlineData( "\\A(?=(a(*SKIP)b|a))a|a", "61", "", 0 )]
	[InlineData( "\\A(?!a(*SKIP)b|a)a", "61", "0:1", 0 )]
	[InlineData( "\\A(?(?=a(*SKIP)b|a)X|a)", "61", "0:1", 0 )]
	[InlineData( "\\A(?(?!a(*SKIP)b|a)a|X)", "61", "0:1", 0 )]
	[InlineData( "\\A(?(?!(a(*SKIP)b))a|X)", "61", "0:1,-", 0 )]
	[InlineData( "\\A(?!(?=a(*SKIP)b|a)a)a", "61", "0:1", 0 )]
	[InlineData( "\\A(?=(?!a(*SKIP)b|a)a)a", "61", "0:1", 0 )]
	[InlineData( "\\A(?*(a(*SKIP)|ab))\\1c", "616263", "", 0 )]
	[InlineData( "\\A(?*(a|ab)(*SKIP))\\1c", "616263", "", 0 )]
	[InlineData( "\\A(?*(a(*SKIP)b|ab))ab", "6162", "0:2,0:2", 0 )]
	[InlineData( "(?<=(a(*SKIP)b))c", "616263", "2:1,0:2", 0 )]
	[InlineData( "(?<=(a(*SKIP)b|ab))c|c", "616263", "2:1,0:2", 0 )]
	[InlineData( "(?<=(a{1,2}(*SKIP)b))c", "61616263", "3:1,0:3", 0 )]
	[InlineData( "(?<!(a(*SKIP)b|a))c", "6163", "1:1,-", 0 )]
	[InlineData( "(?(?<=(a(*SKIP)b))c|X)", "616263", "2:1,0:2", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*SKIP)b|ac))(?&x)d|acd", "616364", "0:3,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*SKIP)b|ac))(?&x)d", "616364", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a|ab))(?&x)(*SKIP)c|abc", "616263", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*SKIP)))(?&x)b|ac", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*SKIP)b))(?&x)c|aac", "616163", "0:3,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*SKIP)b))(?&x)c|aabc", "61616263", "0:4,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&y)c)(?<y>b(*SKIP)X|b))(?&x)d|abcd", "61626364", "0:4,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=a(*SKIP)b|a)a))(?&x)X|a", "61", "0:1,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*SKIP)b|a))(?!(?&x))a", "61", "0:1,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*SKIP)b|a))(?(?=(?&x))X|a)", "61", "0:1,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*SKIP)b))(?:(?&x(2))X|ac)(?(v)Y|d)", "616364", "", 0 )]
	[InlineData( "\\A(?:(a(*SKIP)b)?ac|ac)", "6163", "", 0 )]
	[InlineData( "\\A(?:(a(*SKIP)b)*ac|ac)", "6163", "", 0 )]
	[InlineData( "\\A(?:(a(*SKIP)b){0}ac|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*SKIP))?b|ac)", "6163", "", 0 )]
	[InlineData( "\\A(?:(a(*SKIP))??b|ac)", "6163", "", 0 )]
	[InlineData( "\\A(?:(a(*SKIP))?+b|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*SKIP))++b|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*SKIP)b)+|ac)", "6163", "", 0 )]
	[InlineData( "\\A(?:(a(*SKIP)){2}b|aa)", "6161", "", 0 )]
	[InlineData( "\\A(*sr:(a(*SKIP)b|ac))|ac", "6163", "", 0 )]
	[InlineData( "\\A(*asr:(a(*SKIP)))b|ac", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?J)(?<x>a)(*SKIP)X|(?<x>ab)", "6162", "", 0 )]
	[InlineData( "\\A(?|(?<x>a(*SKIP)b)|(?<x>ac))", "6163", "", 0 )]
	[InlineData( "\\A(?=(a(*SKIP)b))a|a", "61", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*SKIP)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*SKIP)(?<v>b)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?<u>b)(*SKIP)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,0:1,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*SKIP)b))(?!(?&x(2)))ac(?(v)d|X)", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*SKIP)b))(?(?=(?&x(2)))X|ac)(?(v)d|X)", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*SKIP)b))(?:(?&x)X|ac)d", "616364", "0:3,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*SKIP)(?<v>b)X|abc))(?:(?&x(2))Y|abc)d", "61626364", "0:4,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*SKIP)b))(?:(?&x(2))X|ac(*FAIL))|acd", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?!(?<v>a)(*SKIP)b)a))(?&x(2))", "61", "0:1,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?(?=(?<v>a)(*SKIP)b)X|a)))(?&x(2))", "61", "0:1,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=(?<v>a)(*SKIP)b)a))(?:(?&x(2))X|a)", "61", "0:1,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*SKIP)c))(?:(?&x(2))X|ab)d", "616264", "0:3,-,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*SKIP)c))(?:(?&x)X|ab)d", "616264", "0:3,-,-,-", 0 )]
	[InlineData( "a+(*SKIP)(*FAIL)|(a)", "61616162", "", 0 )]
	[InlineData( "a+(*PRUNE)(*FAIL)|(a)", "61616162", "1:1,1:1", 0 )]
	[InlineData( "aa(*SKIP)(*FAIL)|(a)", "616161", "2:1,2:1", 0 )]
	[InlineData( "aa(*SKIP)b|(a)", "6161616361616162", "2:1,2:1", 0 )]
	[InlineData( "\"[^\"]*\"(*SKIP)(*FAIL)|(\\w+)", "2268696464656E222073686F776E", "9:5,9:5", 0 )]
	[InlineData( "(?s:<.*?>)(*SKIP)(*F)|(\\w+)", "3C736B69703E20776F7264", "7:4,7:4", 0 )]
	[InlineData( "(a+)(*SKIP)X|(b)", "61616162", "3:1,-,3:1", 0 )]
	[InlineData( "(a+)\\K(*SKIP)X|(b)", "61616162", "3:1,-,3:1", 0 )]
	[InlineData( "(*SKIP)(*FAIL)|a", "6161", "", 0 )]
	[InlineData( "(*SKIP)(*FAIL)|()", "6161", "", 0 )]
	[InlineData( "(*NOTEMPTY_ATSTART)(*SKIP)(*ACCEPT)|a", "6161", "1:0", 0 )]
	[InlineData( "a+(*SKIP)(*FAIL)|()", "616161", "3:0,3:0", 0 )]
	[InlineData( "(?=a+(*SKIP)X)(.)|(b)", "61616162", "3:1,-,3:1", 0 )]
	[InlineData( "(?=a+(*SKIP)(*F))()", "616161", "", 0 )]
	[InlineData( "(?=a+(*SKIP)(*F))|(.)", "61616162", "3:1,3:1", 0 )]
	[InlineData( "(?*(a+(*SKIP)))\\1X|(b)", "61616162", "3:1,-,3:1", 0 )]
	[InlineData( "(?:(?>a+(*SKIP)))X|(a)", "616161", "0:1,0:1", 0 )]
	[InlineData( "(?:(?>a+(*SKIP)X))|(a)", "616161", "", 0 )]
	[InlineData( "(?<=(a)(*SKIP)b)X|(b)", "6162", "1:1,-,1:1", 0 )]
	[InlineData( "(?<=(a(*SKIP)b))X|(b)", "6162", "1:1,-,1:1", 0 )]
	[InlineData( "(?<=a(*SKIP))X|(b)", "616162", "2:1,2:1", 0 )]
	[InlineData( "(?<=(*SKIP)a)X|(a)", "6161", "0:1,0:1", 0 )]
	[InlineData( "(?<=a(*SKIP)b)X|(a)", "616261", "0:1,0:1", 0 )]
	[InlineData( "(?<=(a{1,2}(*SKIP)b))X|(b)", "616162", "", 0 )]
	[InlineData( "(?(?=a+(*SKIP)X)Y|a)", "616161", "0:1", 0 )]
	[InlineData( "(?!a+(*SKIP)X)a", "616161", "0:1", 0 )]
	[InlineData( "(?<=(?(?=a(*SKIP)X)a|b))c", "6263", "1:1", 0 )]
	[InlineData( "(a+(*SKIP)|b+(*PRUNE))X|(c)", "616162626263", "5:1,-,5:1", 0 )]
	[InlineData( "(a+(*PRUNE)|b+(*SKIP))X|(c)", "616162626263", "5:1,-,5:1", 0 )]
	[InlineData( "a+(*SKIP)(*F)|b+(*SKIP)(*F)|(c)", "61616162626263", "6:1,6:1", 0 )]
	[InlineData( "é+(*SKIP)(*F)|(.)", "C3A9C3A9E6BCA2", "4:3,4:3", 1 )]
	[InlineData( "(?=é+(*SKIP)X)(.)|(漢)", "C3A9C3A9E6BCA2", "4:3,-,4:3", 1 )]
	[InlineData( "(?<=é(*SKIP))X|(漢)", "C3A9E6BCA2", "2:3,2:3", 1 )]
	[InlineData( "é+\\K(*SKIP)(*F)|(.)", "C3A9C3A9E6BCA2", "4:3,4:3", 1 )]
	[InlineData( "(*UTF)(*UCP)\\X(*SKIP)X|(.)", "C3A961", "", 1 )]
	[InlineData( "(*sr:漢+)(*SKIP)(*F)|(.)", "E6BCA2E6BCA261", "6:1,6:1", 1 )]
	[InlineData( "(*UTF)(*UCP)é+(*SKIP)(*F)|()", "C3A9C3A9", "4:0,4:0", 1 )]
	[InlineData( "(*ANYCRLF)\\C(?<!\\R\\C)(*SKIP)(*FAIL)|(\\C)", "0D0A78", "2:1,2:1", 2 )]
	[InlineData( "(*CRLF)\\C(?<!\\R\\C)(*SKIP)(*FAIL)|(\\C)", "0D0A78", "2:1,2:1", 2 )]
	[InlineData( "(*ANYCRLF)(?s:.)(?<!\\R.)(*SKIP)(*FAIL)|(?s:(.))", "0D0A78", "2:1,2:1", 0 )]
	[InlineData( "(*ANYCRLF)\\r(*SKIP)(*FAIL)|(\\C)", "0D0A78", "1:1,1:1", 2 )]
	[InlineData( "(*ANYCRLF)\\C(*SKIP)(*FAIL)|()", "0D0A78", "3:0,3:0", 2 )]
	[InlineData( "(*ANYCRLF)\\C(?<!\\R\\C)(*PRUNE)(*FAIL)|(\\C)", "0D0A78", "2:1,2:1", 2 )]
	[InlineData( "(*ANYCRLF)(?s:.)(?<!\\R.)(*PRUNE)(*FAIL)|(?s:(.))", "0D0A78", "2:1,2:1", 0 )]
	[InlineData( "(*ANYCRLF)(?s:.)\\B(*SKIP)(*F)|(?s:(.))", "0D0A78", "1:1,1:1", 0 )]
	[InlineData( "(*ANYCRLF)(?s:.)\\B(*PRUNE)(*F)|(?s:(.))", "0D0A78", "2:1,2:1", 0 )]
	[InlineData( "(*CRLF)(?s:.)\\B(*SKIP)(*F)|(?s:(.))", "0D0A78", "1:1,1:1", 0 )]
	[InlineData( "(*ANY)(?s:.)\\B(*SKIP)(*F)|(?s:(.))", "0D0A78", "1:1,1:1", 0 )]
	public void SkipResumesAtThePinnedCandidateAndPreservesScopeCaptureTransactions( string pattern, string hex, string spans, int mode ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var utf in mode == 0 ? new[] { false, true } : new[] { mode == 1 } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var reference = Reference( pattern, options ); AssertLiteral( reference, subject, spans );
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EverySelectedOffsetRetainsForwardCrLfAndMalformedUtfRestartBoundaries( bool anyCrLf ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf };
			foreach ( var pattern in new[] { "a+(*SKIP)(*F)|(b)", "(?=a+(*SKIP)X)(a)|(b)", "(?<=a(*SKIP)b)X|(b)",
				"(*SKIP)(*F)|()", "(*SKIP)(*ACCEPT)", @"a+\K(*SKIP)(*F)|()", @"(?s:.)\B(*SKIP)(*F)|(?s:(.))" } ) {
				var reference = Reference( pattern, options ); var compiled = Compile( pattern, options );
				foreach ( var hex in new[] { "", "FF", "FFFF", "FF616162FF", "C3A9FF616162", "0D0A62", "616162FF0D0A62", "F09F9880FF62" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, utf={utf}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( "(*SKIP)*", 7 )]
	[InlineData( "(*SKIP)?", 7 )]
	[InlineData( "(*SKIP){0}", 7 )]
	[InlineData( "(*SKIP){1,2}", 7 )]
	[InlineData( "(*SKIP)(?#x)+", 12 )]
	[InlineData( "é(*SKIP){65536}", 9 )]
	[InlineData( @"(*SKIP)\k<missing>", 7 )]
	public void DirectRepeatsAndGlobalErrorsRetainTheirOriginalByte( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(*SKIP:a)" )]
	[InlineData( "(*SKIP:)" )]
	[InlineData( "(*skip)" )]
	[InlineData( "(*SKIPx)" )]
	public void NamedAndNonExactHeadersRemainExplicitlyUnsupported( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void SkipDoesNotHideUnboundedLookbehindTails() {
		const string pattern = "(?<=(*SKIP)a*)x";
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void SavedSkipFramesShareThe1024FrameLimitAndExpansionBudget() {
		foreach ( var count in new[] { 1024, 1025 } ) {
			var pattern = $"\\A(?:a(*SKIP)){{{count}}}"; var subject = Enumerable.Repeat( (byte)'a', count ).ToArray();
			Assert.True( Reference( pattern, new() ).Match( subject ).Success );
			var actual = Compile( pattern, new() ).Match( subject );
			if ( count == 1024 ) { Assert.True( actual.IsSuccess ); Assert.Equal( count, actual.Match?.ByteLength ); }
			else { Assert.Null( actual.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code ); }
		}
		var expanded = PcreByteCompiler.Compile( "(?:a(*SKIP)){2000}"u8.ToArray() );
		Assert.Null( expanded.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, expanded.Diagnostic?.Code );
	}

	[Fact]
	public void MixedSkipAndPruneFramesConsumeOneSharedBudget() {
		foreach ( var count in new[] { 512, 513 } ) {
			var pattern = $"\\A(?:a(*SKIP)(*PRUNE)){{{count}}}"; var subject = Enumerable.Repeat( (byte)'a', count ).ToArray();
			Assert.True( Reference( pattern, new() ).Match( subject ).Success );
			var actual = Compile( pattern, new() ).Match( subject );
			if ( count == 512 ) { Assert.True( actual.IsSuccess ); Assert.Equal( count, actual.Match?.ByteLength ); }
			else { Assert.Null( actual.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code ); }
		}
	}

	[Fact]
	public void ScopeWalkingAndRestartUseTheSharedWorkAndCancellationBudget() {
		const string pattern = @"\A(?=(?:a?){20}(*SKIP)z)|a";
		var subject = "a"u8.ToArray(); var steps = new List<int>();
		Compare( Reference( pattern, new() ), subject, 0, Compile( pattern, new() { WorkObserver = steps.Add } ).Match( subject ), pattern );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		var limited = Compile( pattern, new() { MatchWorkLimit = steps.Count - 1 } ).Match( subject );
		Assert.Null( limited.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { WorkObserver = count => { if ( count == 10 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	private static void AssertLiteral( PcreRegex8Bit reference, byte[] subject, string spans ) {
		var match = reference.Match( subject );
		Assert.Equal( spans.Length > 0, match.Success );
		if ( !match.Success ) { return; }
		var groups = spans.Split( ',' );
		Assert.Equal( groups.Length, match.Groups.Count );
		for ( var group = 0; group < groups.Length; group++ ) {
			var capture = match.Groups[group];
			Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
		}
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, PcreByteOptions options ) => Reference( Encoding.UTF8.GetBytes( pattern ), options );
	private static PcreRegex8Bit Reference( byte[] pattern, PcreByteOptions options ) => new( pattern, Encoding.UTF8, new PcreRegexSettings {
		Options = PcreOptions.Compiled | ( options.Caseless ? PcreOptions.Caseless : PcreOptions.None ) | ( options.Utf8 || options.MatchInvalidUtf ? PcreOptions.Utf : PcreOptions.None )
			| ( options.Ucp ? PcreOptions.Ucp : PcreOptions.None ) | ( options.MatchInvalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ),
		NewLine = options.AnyCrLfNewline ? PcreNewLine.AnyCrLf : PcreNewLine.Lf,
		ExtraCompileOptions = options.AsciiBsD ? PcreExtraCompileOptions.AsciiBsD : PcreExtraCompileOptions.None,
		BackslashR = options.BsrAnyCrLf ? PcreBackslashR.AnyCrLf : PcreBackslashR.Unicode,
		ParensLimit = 512
	} );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
