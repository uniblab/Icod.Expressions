using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pinned PCRE2 10.48 JIT COMMIT termination, scope catches and byte captures.</summary>
public sealed class PcreCommitVerbReferenceTests {
	[Theory]
	[InlineData( "a(*COMMIT)b", "7878616162", "", 0 )]
	[InlineData( "a+(*COMMIT)b", "616163616162", "", 0 )]
	[InlineData( "\\A(?:a(*COMMIT)b|ac)", "6163", "", 0 )]
	[InlineData( "(?:a(*COMMIT)b|ac)", "6163", "", 0 )]
	[InlineData( "(?:a(*COMMIT)b|ac)", "61636162", "", 0 )]
	[InlineData( "(?:a(*COMMIT)b|ac)", "6162", "0:2", 0 )]
	[InlineData( "(a|ab)(*COMMIT)c", "616263", "", 0 )]
	[InlineData( "(a|ab)c", "616263", "0:3,0:2", 0 )]
	[InlineData( "a(*COMMIT)(b|bc)c", "61626363", "0:3,1:1", 0 )]
	[InlineData( "(a)(*COMMIT)X|(b)", "6162", "", 0 )]
	[InlineData( "\\Q(*COMMIT)\\E", "282A5052554E4529", "", 0 )]
	[InlineData( "[(*COMMIT)]", "50", "", 0 )]
	[InlineData( "(?x)a (*COMMIT) # tail\n b", "6162", "0:2", 0 )]
	[InlineData( "(?i)a(*COMMIT)b", "4142", "0:2", 0 )]
	[InlineData( "(?i)(é)(*COMMIT)X|é", "C3A9C3A9", "", 1 )]
	[InlineData( "a\\K(*COMMIT)X|(b)", "6162", "", 0 )]
	[InlineData( "(*NOTEMPTY)(*COMMIT)|a", "61", "", 0 )]
	[InlineData( "(*COMMIT)(*ACCEPT)|a", "61", "0:0", 0 )]
	[InlineData( "(*NOTEMPTY)(*COMMIT)(*ACCEPT)|a", "61", "", 0 )]
	[InlineData( "\\A(?:(?>a(*COMMIT))b|ac)", "6163", "0:2", 0 )]
	[InlineData( "\\A(?:(?>a(*COMMIT)b)|ac)", "6163", "", 0 )]
	[InlineData( "\\A(?=(a(*COMMIT)))ac|ac", "6163", "0:2,0:1", 0 )]
	[InlineData( "\\A(?=(a(*COMMIT)b|a))a|a", "61", "", 0 )]
	[InlineData( "\\A(?!a(*COMMIT)b|a)a", "61", "0:1", 0 )]
	[InlineData( "\\A(?(?=a(*COMMIT)b|a)X|a)", "61", "0:1", 0 )]
	[InlineData( "\\A(?(?!a(*COMMIT)b|a)a|X)", "61", "0:1", 0 )]
	[InlineData( "\\A(?(?!(a(*COMMIT)b))a|X)", "61", "0:1,-", 0 )]
	[InlineData( "\\A(?!(?=a(*COMMIT)b|a)a)a", "61", "0:1", 0 )]
	[InlineData( "\\A(?=(?!a(*COMMIT)b|a)a)a", "61", "0:1", 0 )]
	[InlineData( "\\A(?*(a(*COMMIT)|ab))\\1c", "616263", "", 0 )]
	[InlineData( "\\A(?*(a|ab)(*COMMIT))\\1c", "616263", "", 0 )]
	[InlineData( "\\A(?*(a(*COMMIT)b|ab))ab", "6162", "0:2,0:2", 0 )]
	[InlineData( "(?<=(a(*COMMIT)b))c", "616263", "2:1,0:2", 0 )]
	[InlineData( "(?<=(a(*COMMIT)b|ab))c|c", "616263", "2:1,0:2", 0 )]
	[InlineData( "(?<=(a{1,2}(*COMMIT)b))c", "61616263", "3:1,0:3", 0 )]
	[InlineData( "(?<!(a(*COMMIT)b|a))c", "6163", "1:1,-", 0 )]
	[InlineData( "(?(?<=(a(*COMMIT)b))c|X)", "616263", "2:1,0:2", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT)b|ac))(?&x)d|acd", "616364", "0:3,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT)b|ac))(?&x)d", "616364", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a|ab))(?&x)(*COMMIT)c|abc", "616263", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT)))(?&x)b|ac", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT)b))(?&x)c|aac", "616163", "0:3,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*COMMIT)b))(?&x)c|aabc", "61616263", "0:4,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&y)c)(?<y>b(*COMMIT)X|b))(?&x)d|abcd", "61626364", "0:4,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=a(*COMMIT)b|a)a))(?&x)X|a", "61", "0:1,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT)b|a))(?!(?&x))a", "61", "0:1,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT)b|a))(?(?=(?&x))X|a)", "61", "0:1,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT)b))(?:(?&x(2))X|ac)(?(v)Y|d)", "616364", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT)b)?ac|ac)", "6163", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT)b)*ac|ac)", "6163", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT)b){0}ac|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT))?b|ac)", "6163", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT))??b|ac)", "6163", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT))?+b|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT))++b|ac)", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT)b)+|ac)", "6163", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT)){2}b|aa)", "6161", "", 0 )]
	[InlineData( "\\A(*sr:(a(*COMMIT)b|ac))|ac", "6163", "", 0 )]
	[InlineData( "\\A(*asr:(a(*COMMIT)))b|ac", "6163", "0:2,-", 0 )]
	[InlineData( "\\A(?J)(?<x>a)(*COMMIT)X|(?<x>ab)", "6162", "", 0 )]
	[InlineData( "\\A(?|(?<x>a(*COMMIT)b)|(?<x>ac))", "6163", "", 0 )]
	[InlineData( "\\A(?=(a(*COMMIT)b))a|a", "61", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT)(?<v>b)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?<u>b)(*COMMIT)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,0:1,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT)b))(?!(?&x(2)))ac(?(v)d|X)", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT)b))(?(?=(?&x(2)))X|ac)(?(v)d|X)", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT)b))(?:(?&x)X|ac)d", "616364", "0:3,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT)(?<v>b)X|abc))(?:(?&x(2))Y|abc)d", "61626364", "0:4,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT)b))(?:(?&x(2))X|ac(*FAIL))|acd", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?!(?<v>a)(*COMMIT)b)a))(?&x(2))", "61", "0:1,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?(?=(?<v>a)(*COMMIT)b)X|a)))(?&x(2))", "61", "0:1,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=(?<v>a)(*COMMIT)b)a))(?:(?&x(2))X|a)", "61", "0:1,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*COMMIT)c))(?:(?&x(2))X|ab)d", "616264", "0:3,-,-,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*COMMIT)c))(?:(?&x)X|ab)d", "616264", "0:3,-,-,-", 0 )]
	[InlineData( "(*COMMIT)", "61", "0:0", 0 )]
	[InlineData( "(*COMMIT)abc", "78797A616263", "3:3", 0 )]
	[InlineData( "(*COMMIT)abc", "7861616263", "", 0 )]
	[InlineData( "(*COMMIT)a|b", "7861", "", 0 )]
	[InlineData( "(*COMMIT)a|b", "7862", "", 0 )]
	[InlineData( "(*COMMIT)(a|b)", "7861", "", 0 )]
	[InlineData( "(*COMMIT)[ab]", "7861", "", 0 )]
	[InlineData( "(*COMMIT).", "0A61", "", 0 )]
	[InlineData( "(*COMMIT)(a)", "7861", "1:1,1:1", 0 )]
	[InlineData( "(?:(*COMMIT)a)", "7861", "1:1", 0 )]
	[InlineData( "(*COMMIT)a+", "786161", "1:2", 0 )]
	[InlineData( "(*COMMIT)a{1,3}", "786161", "1:2", 0 )]
	[InlineData( "(*COMMIT)a+?", "786161", "1:1", 0 )]
	[InlineData( "(*COMMIT)a*", "7861", "0:0", 0 )]
	[InlineData( "(*COMMIT)(a*)b", "786162", "", 0 )]
	[InlineData( "(*COMMIT)(?=a)a", "7861", "1:1", 0 )]
	[InlineData( "(?=(*COMMIT)a)a", "7861", "1:1", 0 )]
	[InlineData( "(?=(*COMMIT))a", "7861", "1:1", 0 )]
	[InlineData( "(*COMMIT)(?:)(a)", "7861", "1:1,1:1", 0 )]
	[InlineData( "(*COMMIT)\\Ka", "7861", "1:1", 0 )]
	[InlineData( "(?i)(*COMMIT)a", "7841", "1:1", 0 )]
	[InlineData( "(*COMMIT)(?i:a)", "7841", "1:1", 0 )]
	[InlineData( "(?i)(*COMMIT)k", "784B", "1:1", 2 )]
	[InlineData( "(?i)(*COMMIT)k", "784B", "", 1 )]
	[InlineData( "(?i)(*COMMIT)s", "7853", "1:1", 2 )]
	[InlineData( "(?i)(*COMMIT)s", "7853", "", 1 )]
	[InlineData( "(?ir)(*COMMIT)k", "784B", "1:1", 0 )]
	[InlineData( "(?ir)(*COMMIT)s", "7853", "1:1", 0 )]
	[InlineData( "(*COMMIT)é", "78C3A9", "1:2", 0 )]
	[InlineData( "(*COMMIT)é", "C3AAC3A9", "", 0 )]
	[InlineData( "(*COMMIT)éa", "C3AA61C3A961", "", 0 )]
	[InlineData( "(?i)(*COMMIT)σ", "78CEA3", "", 0 )]
	[InlineData( "a+(*COMMIT)b", "7878616162", "2:3", 0 )]
	[InlineData( "a+(*COMMIT)(*F)|(a)", "61616162", "", 0 )]
	[InlineData( "a+(*COMMIT)(*PRUNE)X|(a)", "616161", "1:1,1:1", 0 )]
	[InlineData( "a+(*PRUNE)(*COMMIT)X|(a)", "616161", "", 0 )]
	[InlineData( "a+(*COMMIT)b(*SKIP)X|(b)", "6161616262", "4:1,4:1", 0 )]
	[InlineData( "a+(*COMMIT)b(*PRUNE)X|(b)", "6161616262", "3:1,3:1", 0 )]
	[InlineData( "a+(*SKIP)(*COMMIT)X|(a)", "616161", "", 0 )]
	[InlineData( "(*COMMIT)(*SKIP)(*F)|()", "61", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a\\K(*COMMIT)b))(?:(?&x)X|ac)d", "616364", "0:3,-", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT)))(?:(?&x(2))b|ac)d", "616364", "0:3,-,0:1", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT)))(?:(?&x(2))b|ac)d", "616264", "0:3,-,0:1", 0 )]
	[InlineData( "(?<=a(*COMMIT))X|(b)", "616162", "2:1,2:1", 0 )]
	[InlineData( "(?<=(*COMMIT)a)X|(a)", "6161", "0:1,0:1", 0 )]
	[InlineData( "(?<=(a{1,2}(*COMMIT)b))X|(b)", "616162", "", 0 )]
	[InlineData( "(?(?=a+(*COMMIT)X)Y|a)", "616161", "0:1", 0 )]
	[InlineData( "(?!a+(*COMMIT)X)a", "616161", "0:1", 0 )]
	[InlineData( "(?*(a+(*COMMIT)))\\1X|(b)", "61616162", "", 0 )]
	[InlineData( "é+(*COMMIT)(*F)|(.)", "C3A9C3A9E6BCA2", "", 1 )]
	[InlineData( "(?<=é(*COMMIT))X|(漢)", "C3A9E6BCA2", "2:3,2:3", 1 )]
	[InlineData( "(*UTF)(*UCP)\\X(*COMMIT)X|(.)", "C3A961", "", 1 )]
	[InlineData( "(*sr:漢+)(*COMMIT)(*F)|(.)", "E6BCA2E6BCA261", "", 1 )]
	[InlineData( "(*ANYCRLF)(*COMMIT)\\n", "0D0A", "1:1", 0 )]
	[InlineData( "(*CRLF)(*COMMIT)(?s:.)", "0D0A78", "0:1", 0 )]
	[InlineData( "(*ANYCRLF)(?s:.)\\B(*COMMIT)(*F)|(?s:(.))", "0D0A78", "", 0 )]
	[InlineData( "(*COMMIT)(a)(?:(*PRUNE)b|ac)", "616162", "1:2,1:1", 0 )]
	public void CommitTerminatesRootSearchAndPreservesScopeCaptureTransactions( string pattern, string hex, string spans, int mode ) {
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
	public void EverySelectedOffsetRetainsFirstByteAndMalformedUtfCommitBoundaries( bool anyCrLf ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf };
			foreach ( var pattern in new[] { "(*COMMIT)a", "(*COMMIT)é", "(*COMMIT)(?i:a)", "a+(*COMMIT)(*F)|(b)", "(?=a+(*COMMIT)X)(a)|(b)", "(?<=a(*COMMIT)b)X|(b)",
				"(*COMMIT)(*F)|()", "(*COMMIT)(*ACCEPT)", @"a+\K(*COMMIT)(*F)|()", @"(?s:.)\B(*COMMIT)(*F)|(?s:(.))" } ) {
				var reference = Reference( pattern, options ); var compiled = Compile( pattern, options );
				foreach ( var hex in new[] { "", "FF", "FFFF", "FF616162FF", "C3A9FF616162", "0D0A62", "616162FF0D0A62", "F09F9880FF62", "C378C3A9", "FFC378C3A9", "FF80C3A9", "C3AAC3A9", "FF80C361" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, utf={utf}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( "(*COMMIT)*", 9 )]
	[InlineData( "(*COMMIT)?", 9 )]
	[InlineData( "(*COMMIT){0}", 9 )]
	[InlineData( "(*COMMIT){1,2}", 9 )]
	[InlineData( "(*COMMIT)(?#x)+", 14 )]
	[InlineData( "é(*COMMIT){65536}", 11 )]
	[InlineData( @"(*COMMIT)\k<missing>", 9 )]
	public void DirectRepeatsAndGlobalErrorsRetainTheirOriginalByte( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(*COMMIT :a)" )]
	[InlineData( "(*COMMITX:a)" )]
	[InlineData( "(*skip)" )]
	[InlineData( "(*COMMITx)" )]
	public void NonExactHeadersRemainExplicitlyUnsupported( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void CommitDoesNotHideUnboundedLookbehindTails() {
		const string pattern = "(?<=(*COMMIT)a*)x";
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void SavedCommitFramesShareThe1024FrameLimitAndExpansionBudget() {
		foreach ( var count in new[] { 1024, 1025 } ) {
			var pattern = $"\\A(?:a(*COMMIT)){{{count}}}"; var subject = Enumerable.Repeat( (byte)'a', count ).ToArray();
			Assert.True( Reference( pattern, new() ).Match( subject ).Success );
			var actual = Compile( pattern, new() ).Match( subject );
			if ( count == 1024 ) { Assert.True( actual.IsSuccess ); Assert.Equal( count, actual.Match?.ByteLength ); }
			else { Assert.Null( actual.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code ); }
		}
		var expanded = PcreByteCompiler.Compile( "(?:a(*COMMIT)){2000}"u8.ToArray() );
		Assert.Null( expanded.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, expanded.Diagnostic?.Code );
	}

	[Fact]
	public void AllThreeCutKindsConsumeOneSharedFrameBudget() {
		foreach ( var count in new[] { 341, 342 } ) {
			var pattern = $"\\A(?:a(*PRUNE)(*SKIP)(*COMMIT)){{{count}}}"; var subject = Enumerable.Repeat( (byte)'a', count ).ToArray();
			Assert.True( Reference( pattern, new() ).Match( subject ).Success );
			var actual = Compile( pattern, new() ).Match( subject );
			if ( count == 341 ) { Assert.True( actual.IsSuccess ); Assert.Equal( count, actual.Match?.ByteLength ); }
			else { Assert.Null( actual.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code ); }
		}
	}

	[Fact]
	public void ScopeWalkingUsesTheSharedWorkAndCancellationBudget() {
		const string pattern = @"\A(?=(?:a?){20}(*COMMIT)z)|a";
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
	public void LeadingCommitSearchChargesAndCancelsEveryFirstByteCheck() {
		const string pattern = "(*COMMIT)a"; var subject = "xxxxa"u8.ToArray(); var steps = new List<int>();
		Compare( Reference( pattern, new() ), subject, 0, Compile( pattern, new() { WorkObserver = steps.Add } ).Match( subject ), pattern );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		var limited = Compile( pattern, new() { MatchWorkLimit = steps.Count - 1 } ).Match( subject );
		Assert.Null( limited.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { WorkObserver = count => { if ( count == 6 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
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
