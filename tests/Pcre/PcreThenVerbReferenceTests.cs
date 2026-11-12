using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pinned PCRE2 10.48 JIT THEN alternation cuts, scope catches and byte captures.</summary>
public sealed class PcreThenVerbReferenceTests {
	[Theory]
	[InlineData( "a(*THEN)b", "7878616162", "3:2", 0 )]
	[InlineData( "a+(*THEN)b", "616163616162", "3:3", 0 )]
	[InlineData( "\\A(?:a(*THEN)b|ac)", "6163", "0:2", 0 )]
	[InlineData( "(?:a(*THEN)b|ac)", "6163", "0:2", 0 )]
	[InlineData( "(?:a(*THEN)b|ac)", "61636162", "0:2", 0 )]
	[InlineData( "(?:a(*THEN)b|ac)", "6162", "0:2", 0 )]
	[InlineData( "(a|ab)(*THEN)c", "616263", "", 0 )]
	[InlineData( "(a|ab)c", "616263", "0:3,0:2", 0 )]
	[InlineData( "a(*THEN)(b|bc)c", "61626363", "0:3,1:1", 0 )]
	[InlineData( "(a)(*THEN)X|(b)", "6162", "1:1,-,1:1", 0 )]
	[InlineData( "\\Q(*THEN)\\E", "282A5448454E29", "0:7", 0 )]
	[InlineData( "[(*THEN)]", "48", "0:1", 0 )]
	[InlineData( "(?x)a (*THEN) # tail\n b", "6162", "0:2", 0 )]
	[InlineData( "(?i)a(*THEN)b", "4142", "0:2", 0 )]
	[InlineData( "(?i)(é)(*THEN)X|é", "C3A9C3A9", "0:2,-", 1 )]
	[InlineData( "a\\K(*THEN)X|(b)", "6162", "1:1,1:1", 0 )]
	[InlineData( "(*NOTEMPTY)(*THEN)|a", "61", "0:1", 0 )]
	[InlineData( "(*THEN)(*ACCEPT)|a", "61", "0:0", 0 )]
	[InlineData( "(*NOTEMPTY)(*THEN)(*ACCEPT)|a", "61", "0:1", 0 )]
	[InlineData( "\\A(?:(?>a(*THEN))b|ac)", "6163", "0:2", 0 )]
	[InlineData( "\\A(?:(?>a(*THEN)b)|ac)", "6163", "0:2", 0 )]
	[InlineData( "\\A(?=(a(*THEN)))ac|ac", "6163", "0:2,0:1", 0 )]
	[InlineData( "\\A(?=(a(*THEN)b|a))a|a", "61", "0:1,0:1", 0 )]
	[InlineData( "\\A(?!a(*THEN)b|a)a", "61", "", 0 )]
	[InlineData( "\\A(?(?=a(*THEN)b|a)X|a)", "61", "", 0 )]
	[InlineData( "\\A(?(?!a(*THEN)b|a)a|X)", "61", "", 0 )]
	[InlineData( "\\A(?(?!(a(*THEN)b))a|X)", "61", "0:1,-", 0 )]
	[InlineData( "\\A(?!(?=a(*THEN)b|a)a)a", "61", "", 0 )]
	[InlineData( "\\A(?=(?!a(*THEN)b|a)a)a", "61", "", 0 )]
	[InlineData( "\\A(?*(a(*THEN)|ab))\\1c", "616263", "0:3,0:2", 0 )]
	[InlineData( "\\A(?*(a|ab)(*THEN))\\1c", "616263", "", 0 )]
	[InlineData( "\\A(?*(a(*THEN)b|ab))ab", "6162", "0:2,0:2", 0 )]
	[InlineData( "(?<=(a(*THEN)b))c", "616263", "2:1,0:2", 0 )]
	[InlineData( "(?<=(a(*THEN)b|ab))c|c", "616263", "2:1,0:2", 0 )]
	[InlineData( "(?<=(a{1,2}(*THEN)b))c", "61616263", "3:1,0:3", 0 )]
	[InlineData( "(?<!(a(*THEN)b|a))c", "6163", "", 0 )]
	[InlineData( "(?(?<=(a(*THEN)b))c|X)", "616263", "2:1,0:2", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN)b|ac))(?&x)d|acd", "616364", "0:3,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN)b|ac))(?&x)d", "616364", "0:3,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a|ab))(?&x)(*THEN)c|abc", "616263", "0:3,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN)))(?&x)b|ac", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN)b))(?&x)c|aac", "616163", "0:3,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*THEN)b))(?&x)c|aabc", "61616263", "0:4,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&y)c)(?<y>b(*THEN)X|b))(?&x)d|abcd", "61626364", "0:4,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=a(*THEN)b|a)a))(?&x)X|a", "61", "0:1,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN)b|a))(?!(?&x))a", "61", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN)b|a))(?(?=(?&x))X|a)", "61", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN)b))(?:(?&x(2))X|ac)(?(v)Y|d)", "616364", "", 0 )]
	[InlineData( "\\A(?:(a(*THEN)b)?ac|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*THEN)b)*ac|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*THEN)b){0}ac|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*THEN))?b|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*THEN))??b|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*THEN))?+b|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*THEN))++b|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*THEN)b)+|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*THEN)){2}b|aa)", "6161", "0:2,-", 0 )]
	[InlineData( "\\A(*sr:(a(*THEN)b|ac))|ac", "6163", "0:2,0:2", 0 )]
	[InlineData( "\\A(*asr:(a(*THEN)))b|ac", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?J)(?<x>a)(*THEN)X|(?<x>ab)", "6162", "0:2,-,0:2", 0 )]
	[InlineData( "\\A(?|(?<x>a(*THEN)b)|(?<x>ac))", "6163", "0:2,0:2", 0 )]
	[InlineData( "\\A(?=(a(*THEN)b))a|a", "61", "0:1,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN)(?<v>b)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?<u>b)(*THEN)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,0:1,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN)b))(?!(?&x(2)))ac(?(v)d|X)", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN)b))(?(?=(?&x(2)))X|ac)(?(v)d|X)", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN)b))(?:(?&x)X|ac)d", "616364", "0:3,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN)(?<v>b)X|abc))(?:(?&x(2))Y|abc)d", "61626364", "0:4,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN)b))(?:(?&x(2))X|ac(*FAIL))|acd", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?!(?<v>a)(*THEN)b)a))(?&x(2))", "61", "0:1,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?(?=(?<v>a)(*THEN)b)X|a)))(?&x(2))", "61", "0:1,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=(?<v>a)(*THEN)b)a))(?:(?&x(2))X|a)", "61", "0:1,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*THEN)c))(?:(?&x(2))X|ab)d", "616264", "0:3,-,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*THEN)c))(?:(?&x)X|ab)d", "616264", "0:3,-,-,-", 0 )]
	[InlineData( "(*THEN)", "61", "0:0", 0 )]
	[InlineData( "(*THEN)(*FAIL)|a", "61", "0:1", 0 )]
	[InlineData( "(*THEN)abc", "78797A616263", "3:3", 0 )]
	[InlineData( "(*THEN)abc", "7861616263", "2:3", 0 )]
	[InlineData( "\\A(?:a+(*THEN)X|a+)b", "61616162", "0:4", 0 )]
	[InlineData( "\\A(?:(?:a+(*THEN)X)|a+)b", "61616162", "0:4", 0 )]
	[InlineData( "\\A(?:a+(?:a(*THEN)X)|a+)b", "61616162", "0:4", 0 )]
	[InlineData( "\\Aa+(?:a(*THEN)X|(*F))b|aaab", "61616162", "0:4", 0 )]
	[InlineData( "\\Aa+(?:a(*THEN)X|(*F))aab", "6161616162", "", 0 )]
	[InlineData( "\\Aa+(?:a(*THEN)X)aab", "6161616162", "", 0 )]
	[InlineData( "\\A(?:a+X|a+(*THEN)Y|a+)b", "61616162", "0:4", 0 )]
	[InlineData( "\\A(?:a+X|a+(*THEN)Y)ab|aaaab", "6161616162", "0:5", 0 )]
	[InlineData( "\\Aa+(?:a+X|a(*THEN)Y)aab", "6161616162", "", 0 )]
	[InlineData( "\\A(?:a+(*THEN)b|a+(*THEN)c|a+d)", "61616164", "0:4", 0 )]
	[InlineData( "\\A(?:a+(*THEN)b|a+(*THEN)c|a+(*THEN)d|a+e)", "61616165", "0:4", 0 )]
	[InlineData( "\\A(?:a(*THEN)b|ac)d", "616364", "0:3", 0 )]
	[InlineData( "\\A(?:(a)(*THEN)b|(ac))d", "616364", "0:3,-,0:2", 0 )]
	[InlineData( "\\A(?:a\\K(*THEN)b|ac)d", "616364", "0:3", 0 )]
	[InlineData( "\\A(?=a(*THEN)b|a)a|X", "61", "0:1", 0 )]
	[InlineData( "\\A(?=a(*THEN)b)a|a", "61", "0:1", 0 )]
	[InlineData( "\\A(?!a(*THEN)b|a)a|a", "61", "0:1", 0 )]
	[InlineData( "\\A(?*(a|ab)(*THEN))\\1c|abc", "616263", "0:3,-", 0 )]
	[InlineData( "\\A(?=(a+(*THEN)X|a))\\1b", "616162", "", 0 )]
	[InlineData( "\\A(?(?=a(*THEN)b|a)a|X)", "61", "0:1", 0 )]
	[InlineData( "\\A(?(?!a(*THEN)b|a)X|a)", "61", "0:1", 0 )]
	[InlineData( "\\A(?(?=a(*THEN)b)X|a)", "61", "0:1", 0 )]
	[InlineData( "\\A.*?(?(?=a)a|b(*THEN)c)", "6261", "", 0 )]
	[InlineData( "\\A(?:.*?(?(?=a)a|b(*THEN)c)|ba)", "6261", "0:2", 0 )]
	[InlineData( "\\A(?:a(?(?=c)c|(*THEN)X)|ab)", "6162", "0:2", 0 )]
	[InlineData( "\\A(?:a(?(1)(*THEN)X|b)|ac)()", "6163", "0:2,2:0", 0 )]
	[InlineData( "\\A(?:a(?(R)(*THEN)X|b)|ac)", "6163", "0:2", 0 )]
	[InlineData( "\\A(?:a(?(VERSION>=10.48)(*THEN)X|b)|ac)", "6163", "0:2", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a+(*THEN)X|a))(?&x)b", "616162", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a+(*THEN)X))(?&x)b|aaab", "61616162", "0:4,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a+(?:a(*THEN)X|a)))(?&x)b", "61616162", "0:4,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a+(?:a(*THEN)X)))(?&x)b|aaab", "61616162", "0:4,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN)b|ac))(?:(?&x(2))X|ac)d", "616364", "0:3,-,-", 0 )]
	[InlineData( "\\A(?:a(*COMMIT)b(*THEN)c|abd)", "616264", "0:3", 0 )]
	[InlineData( "\\A(?:a(*THEN)b(*COMMIT)c|abd)", "616264", "", 0 )]
	[InlineData( "\\A(?:a(*PRUNE)b(*THEN)c|abd)", "616264", "0:3", 0 )]
	[InlineData( "\\A(?:a(*THEN)b(*PRUNE)c|abd)", "616264", "", 0 )]
	[InlineData( "\\A(?:a(*SKIP)b(*THEN)c|abd)", "616264", "0:3", 0 )]
	[InlineData( "\\A(?:a(*THEN)b(*SKIP)c|abd)", "616264", "", 0 )]
	[InlineData( "\\A(?:a(*COMMIT)(?:b(*THEN)c|bX)|abd)", "616264", "", 0 )]
	[InlineData( "\\A(?:a(*COMMIT)(?:b(*THEN)c)|abd)", "616264", "0:3", 0 )]
	[InlineData( "\\A(?:a(*COMMIT)(?:bX|b(*THEN)c)|abd)", "616264", "", 0 )]
	[InlineData( "\\A(?:(?>a(*THEN)b|ac))d", "616364", "0:3", 0 )]
	[InlineData( "\\A(?:a(*THEN)b|ac)+d", "6163616364", "0:5", 0 )]
	[InlineData( "\\A(?:a(*THEN)b|ac)+?d", "6163616364", "0:5", 0 )]
	[InlineData( "\\A(?:a(*THEN)b|ac)++d", "6163616364", "0:5", 0 )]
	[InlineData( "\\A(?:a(*THEN)b|ac){2}d", "6163616364", "0:5", 0 )]
	[InlineData( "\\A(?:a(*THEN)b|ac){0}d", "64", "0:1", 0 )]
	[InlineData( "\\A(?:a(*THEN)b|ac)?d", "616364", "0:3", 0 )]
	[InlineData( "\\A(?:a+(*THEN)X|a+)+b", "61616162", "0:4", 0 )]
	[InlineData( "\\A(?:a+X|a+(*THEN)Y)+b|aaab", "61616162", "0:4", 0 )]
	[InlineData( "(?<=(a(*THEN)b|ac))d", "616364", "2:1,0:2", 0 )]
	[InlineData( "(?<=(a{1,2}(*THEN)X|a))b", "616162", "2:1,1:1", 0 )]
	[InlineData( "(?(?<=(a(*THEN)b|a))c|X)", "6163", "1:1,0:1", 0 )]
	[InlineData( "\\A(*sr:(a(*THEN)b|ac))d", "616364", "0:3,0:2", 0 )]
	[InlineData( "\\A(*asr:(a(*THEN)b|ac))d", "616364", "0:3,0:2", 0 )]
	[InlineData( "\\A(*sr:(a(*THEN)b))d|acd", "616364", "0:3,-", 0 )]
	[InlineData( "é+(*THEN)(*F)|(.)", "C3A9C3A9E6BCA2", "0:2,0:2", 1 )]
	[InlineData( "(*UTF)(*UCP)\\X(*THEN)X|(.)", "C3A961", "0:2,0:2", 1 )]
	[InlineData( "\\A(?:a(*THEN)(*ACCEPT)|ac)d", "6163", "0:1", 0 )]
	[InlineData( "(*ANYCRLF)(?s:.)\\B(*THEN)(*F)|(?s:(.))", "0D0A78", "0:1,0:1", 0 )]
	[InlineData( "(*ANYCRLF)a+(*THEN)X|(b)", "61610D0A62", "4:1,4:1", 0 )]
	[InlineData( "\\A(?|(?<v>a)(*THEN)X|(?<v>ab))\\k<v>", "61626162", "0:4,0:2", 0 )]
	[InlineData( "\\A(?J)(?<v>a)(*THEN)X|(?<v>ab)", "6162", "0:2,-,0:2", 0 )]
	[InlineData( "(*THEN)(*COMMIT)abc", "78797A616263", "3:3", 0 )]
	[InlineData( "(*COMMIT)(*THEN)abc", "78797A616263", "3:3", 0 )]
	[InlineData( "\\Aa+(?:a(*THEN)a*|(*F))ab", "6161616162", "0:5", 0 )]
	[InlineData( "\\Aa+(?:a(*THEN)a*)ab", "6161616162", "", 0 )]
	[InlineData( "\\Aa+(?:X|a(*THEN)a*)ab", "6161616162", "0:5", 0 )]
	[InlineData( "\\A(?:a+(*THEN)X|a)+b", "61616162", "0:4", 0 )]
	public void ThenRetriesTheOwningAlternativeAndPreservesScopeCaptureTransactions( string pattern, string hex, string spans, int mode ) {
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
	public void EverySelectedOffsetRetainsScopedThenAndMalformedUtfBoundaries( bool anyCrLf ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf };
			foreach ( var pattern in new[] { "a+(*THEN)(*F)|(b)", "(?:a(*THEN)X|ab)", "(?:(*THEN)a|b)", "(?=a(*THEN)X)a|b", "(*THEN)(*COMMIT)a", "(?=a+(*THEN)X)(a)|(b)", "(?<=a(*THEN)b)X|(b)",
				"(*THEN)(*F)|()", "(*THEN)(*ACCEPT)", @"a+\K(*THEN)(*F)|()", @"(?s:.)\B(*THEN)(*F)|(?s:(.))" } ) {
				var reference = Reference( pattern, options ); var compiled = Compile( pattern, options );
				foreach ( var hex in new[] { "", "FF", "FFFF", "FF616162FF", "C3A9FF616162", "0D0A62", "616162FF0D0A62", "F09F9880FF62", "C378C3A9", "FFC378C3A9", "FF80C3A9", "C3AAC3A9" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, utf={utf}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( "(*THEN)*", 7 )]
	[InlineData( "(*THEN)?", 7 )]
	[InlineData( "(*THEN){0}", 7 )]
	[InlineData( "(*THEN){1,2}", 7 )]
	[InlineData( "(*THEN)(?#x)+", 12 )]
	[InlineData( "é(*THEN){65536}", 9 )]
	[InlineData( @"(*THEN)\k<missing>", 7 )]
	public void DirectRepeatsAndGlobalErrorsRetainTheirOriginalByte( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(*THEN :a)" )]
	[InlineData( "(*THENX:a)" )]
	[InlineData( "(*then)" )]
	[InlineData( "(*THENx)" )]
	public void NonExactHeadersRemainExplicitlyUnsupported( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void ThenDoesNotHideUnboundedLookbehindTails() {
		const string pattern = "(?<=(*THEN)a*)x";
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void SavedThenFramesShareThe1024FrameLimitAndExpansionBudget() {
		foreach ( var count in new[] { 1024, 1025 } ) {
			var pattern = $"\\A(?:a(*THEN)){{{count}}}"; var subject = Enumerable.Repeat( (byte)'a', count ).ToArray();
			Assert.True( Reference( pattern, new() ).Match( subject ).Success );
			var actual = Compile( pattern, new() ).Match( subject );
			if ( count == 1024 ) { Assert.True( actual.IsSuccess ); Assert.Equal( count, actual.Match?.ByteLength ); }
			else { Assert.Null( actual.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code ); }
		}
		var expanded = PcreByteCompiler.Compile( "(?:a(*THEN)){2000}"u8.ToArray() );
		Assert.Null( expanded.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, expanded.Diagnostic?.Code );
	}

	[Fact]
	public void AllFourCutKindsConsumeOneSharedFrameBudget() {
		foreach ( var count in new[] { 256, 257 } ) {
			var pattern = $"\\A(?:a(*PRUNE)(*SKIP)(*COMMIT)(*THEN)){{{count}}}"; var subject = Enumerable.Repeat( (byte)'a', count ).ToArray();
			Assert.True( Reference( pattern, new() ).Match( subject ).Success );
			var actual = Compile( pattern, new() ).Match( subject );
			if ( count == 256 ) { Assert.True( actual.IsSuccess ); Assert.Equal( count, actual.Match?.ByteLength ); }
			else { Assert.Null( actual.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code ); }
		}
	}

	[Fact]
	public void AlternationCutsUseTheSharedWorkAndCancellationBudget() {
		const string pattern = @"\A(?=(?:a?){20}(*THEN)z)|a";
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

	[Fact]
	public void AlternationCheckpointsShareThe256MarksWithScriptRuns() {
		foreach ( var mixed in new[] { false, true } ) {
			foreach ( var over in new[] { false, true } ) {
				var count = ( mixed ? 128 : 256 ) + ( over ? 1 : 0 );
				var body = mixed ? "(?:a|b)(*sr:a)" : "(?:a|b)";
				var pattern = String.Concat( @"\A", String.Concat( Enumerable.Repeat( body, count ) ), "(*THEN)" );
				var subject = Enumerable.Repeat( (byte)'a', count * ( mixed ? 2 : 1 ) ).ToArray();
				Assert.True( Reference( pattern, new() ).Match( subject ).Success );
				var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
				if ( over ) { Assert.Null( result.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, result.Diagnostic?.Code ); }
				else { Assert.True( result.IsSuccess ); Assert.Equal( subject.Length, result.Pattern!.Match( subject ).Match?.ByteLength ); }
			}
		}
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
