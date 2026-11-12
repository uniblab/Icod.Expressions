using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 10.48 JIT scope catches, PRUNE retries and byte captures.</summary>
public sealed class PcrePruneVerbReferenceTests {
	[Theory]
	[InlineData( "a(*PRUNE)b", "7878616162", "3:2", false )]
	[InlineData( "a+(*PRUNE)b", "616163616162", "3:3", false )]
	[InlineData( "\\A(?:a(*PRUNE)b|ac)", "6163", "", false )]
	[InlineData( "(?:a(*PRUNE)b|ac)", "6163", "", false )]
	[InlineData( "(?:a(*PRUNE)b|ac)", "61636162", "2:2", false )]
	[InlineData( "(?:a(*PRUNE)b|ac)", "6162", "0:2", false )]
	[InlineData( "(a|ab)(*PRUNE)c", "616263", "", false )]
	[InlineData( "(a|ab)c", "616263", "0:3,0:2", false )]
	[InlineData( "a(*PRUNE)(b|bc)c", "61626363", "0:3,1:1", false )]
	[InlineData( "(a)(*PRUNE)X|(b)", "6162", "1:1,-,1:1", false )]
	[InlineData( "\\Q(*PRUNE)\\E", "282A5052554E4529", "0:8", false )]
	[InlineData( "[(*PRUNE)]", "50", "0:1", false )]
	[InlineData( "(?x)a (*PRUNE) # tail\n b", "6162", "0:2", false )]
	[InlineData( "(?i)a(*PRUNE)b", "4142", "0:2", false )]
	[InlineData( "(?i)(é)(*PRUNE)X|é", "C3A9C3A9", "", true )]
	[InlineData( "a\\K(*PRUNE)X|(b)", "6162", "1:1,1:1", false )]
	[InlineData( "(*NOTEMPTY)(*PRUNE)|a", "61", "", false )]
	[InlineData( "(*PRUNE)(*ACCEPT)|a", "61", "0:0", false )]
	[InlineData( "(*NOTEMPTY)(*PRUNE)(*ACCEPT)|a", "61", "", false )]
	[InlineData( "\\A(?:(?>a(*PRUNE))b|ac)", "6163", "0:2", false )]
	[InlineData( "\\A(?:(?>a(*PRUNE)b)|ac)", "6163", "", false )]
	[InlineData( "\\A(?=(a(*PRUNE)))ac|ac", "6163", "0:2,0:1", false )]
	[InlineData( "\\A(?=(a(*PRUNE)b|a))a|a", "61", "", false )]
	[InlineData( "\\A(?!a(*PRUNE)b|a)a", "61", "0:1", false )]
	[InlineData( "\\A(?(?=a(*PRUNE)b|a)X|a)", "61", "0:1", false )]
	[InlineData( "\\A(?(?!a(*PRUNE)b|a)a|X)", "61", "0:1", false )]
	[InlineData( "\\A(?(?!(a(*PRUNE)b))a|X)", "61", "0:1,-", false )]
	[InlineData( "\\A(?!(?=a(*PRUNE)b|a)a)a", "61", "0:1", false )]
	[InlineData( "\\A(?=(?!a(*PRUNE)b|a)a)a", "61", "0:1", false )]
	[InlineData( "\\A(?*(a(*PRUNE)|ab))\\1c", "616263", "", false )]
	[InlineData( "\\A(?*(a|ab)(*PRUNE))\\1c", "616263", "", false )]
	[InlineData( "\\A(?*(a(*PRUNE)b|ab))ab", "6162", "0:2,0:2", false )]
	[InlineData( "(?<=(a(*PRUNE)b))c", "616263", "2:1,0:2", false )]
	[InlineData( "(?<=(a(*PRUNE)b|ab))c|c", "616263", "2:1,0:2", false )]
	[InlineData( "(?<=(a{1,2}(*PRUNE)b))c", "61616263", "3:1,0:3", false )]
	[InlineData( "(?<!(a(*PRUNE)b|a))c", "6163", "1:1,-", false )]
	[InlineData( "(?(?<=(a(*PRUNE)b))c|X)", "616263", "2:1,0:2", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE)b|ac))(?&x)d|acd", "616364", "0:3,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE)b|ac))(?&x)d", "616364", "", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a|ab))(?&x)(*PRUNE)c|abc", "616263", "", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE)))(?&x)b|ac", "6163", "0:2,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE)b))(?&x)c|aac", "616163", "0:3,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*PRUNE)b))(?&x)c|aabc", "61616263", "0:4,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&y)c)(?<y>b(*PRUNE)X|b))(?&x)d|abcd", "61626364", "0:4,-,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=a(*PRUNE)b|a)a))(?&x)X|a", "61", "0:1,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE)b|a))(?!(?&x))a", "61", "0:1,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE)b|a))(?(?=(?&x))X|a)", "61", "0:1,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE)b))(?:(?&x(2))X|ac)(?(v)Y|d)", "616364", "", false )]
	[InlineData( "\\A(?:(a(*PRUNE)b)?ac|ac)", "6163", "", false )]
	[InlineData( "\\A(?:(a(*PRUNE)b)*ac|ac)", "6163", "", false )]
	[InlineData( "\\A(?:(a(*PRUNE)b){0}ac|ac)", "6163", "0:2,-", false )]
	[InlineData( "\\A(?:(a(*PRUNE))?b|ac)", "6163", "", false )]
	[InlineData( "\\A(?:(a(*PRUNE))??b|ac)", "6163", "", false )]
	[InlineData( "\\A(?:(a(*PRUNE))?+b|ac)", "6163", "0:2,-", false )]
	[InlineData( "\\A(?:(a(*PRUNE))++b|ac)", "6163", "0:2,-", false )]
	[InlineData( "\\A(?:(a(*PRUNE)b)+|ac)", "6163", "", false )]
	[InlineData( "\\A(?:(a(*PRUNE)){2}b|aa)", "6161", "", false )]
	[InlineData( "\\A(*sr:(a(*PRUNE)b|ac))|ac", "6163", "", false )]
	[InlineData( "\\A(*asr:(a(*PRUNE)))b|ac", "6163", "0:2,-", false )]
	[InlineData( "\\A(?J)(?<x>a)(*PRUNE)X|(?<x>ab)", "6162", "", false )]
	[InlineData( "\\A(?|(?<x>a(*PRUNE)b)|(?<x>ac))", "6163", "", false )]
	[InlineData( "\\A(?=(a(*PRUNE)b))a|a", "61", "", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE)(?<v>b)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?<u>b)(*PRUNE)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,0:1,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE)b))(?!(?&x(2)))ac(?(v)d|X)", "616364", "0:3,-,0:1", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE)b))(?(?=(?&x(2)))X|ac)(?(v)d|X)", "616364", "0:3,-,0:1", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE)b))(?:(?&x)X|ac)d", "616364", "0:3,-,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE)(?<v>b)X|abc))(?:(?&x(2))Y|abc)d", "61626364", "0:4,-,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE)b))(?:(?&x(2))X|ac(*FAIL))|acd", "616364", "0:3,-,0:1", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?!(?<v>a)(*PRUNE)b)a))(?&x(2))", "61", "0:1,-,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?(?=(?<v>a)(*PRUNE)b)X|a)))(?&x(2))", "61", "0:1,-,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=(?<v>a)(*PRUNE)b)a))(?:(?&x(2))X|a)", "61", "0:1,-,0:1", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*PRUNE)c))(?:(?&x(2))X|ab)d", "616264", "0:3,-,-,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*PRUNE)c))(?:(?&x)X|ab)d", "616264", "0:3,-,-,-", false )]
	public void BacktrackingCutsOnlyTheNativeScopeAndPinsSelectedCalleeValues( string pattern, string hex, string spans, bool utfOnly ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var utf in utfOnly ? new[] { true } : new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var reference = Reference( pattern, options );
			AssertLiteral( reference, subject, spans );
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EverySelectedOffsetPreservesCrLfAndMalformedUtfCandidateBoundaries( bool anyCrLf ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf };
			foreach ( var pattern in new[] { "(*PRUNE)", "(a)(*PRUNE)X|(b)", "(*PRUNE)(*ACCEPT)", "(*NOTEMPTY)(*PRUNE)|b",
				@"[a\n](*PRUNE)X|b", "(?!(a(*PRUNE)X|a))b", "(?(?=a(*PRUNE)X|a)X|b)", @"a\K(*PRUNE)X|b" } ) {
				var reference = Reference( pattern, options ); var compiled = Compile( pattern, options );
				foreach ( var hex in new[] { "", "FF", "FFFF", "FF6162FF", "C3A9FF6162", "0D0A62", "6162FF0D0A62", "F09F9880FF62" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, utf={utf}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( "(*PRUNE)*", 8 )]
	[InlineData( "(*PRUNE)?", 8 )]
	[InlineData( "(*PRUNE){0}", 8 )]
	[InlineData( "(*PRUNE){1,2}", 8 )]
	[InlineData( "(*PRUNE)(?#x)+", 13 )]
	[InlineData( "é(*PRUNE){65536}", 10 )]
	[InlineData( @"(*PRUNE)\k<missing>", 8 )]
	public void DirectRepeatsAndGlobalErrorsRetainTheirOriginalByte( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(*PRUNE :a)" )]
	[InlineData( "(*PRUNEX:a)" )]
	[InlineData( "(*prune)" )]
	[InlineData( "(*PRUNEx)" )]
	public void NonExactHeadersRemainExplicitlyUnsupported( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void PruneDoesNotHideUnboundedLookbehindTails() {
		const string pattern = "(?<=(*PRUNE)a*)x";
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void SavedPruneFramesShareThe1024FrameLimitAndExpansionBudget() {
		foreach ( var count in new[] { 1024, 1025 } ) {
			var pattern = $"\\A(?:a(*PRUNE)){{{count}}}"; var subject = Enumerable.Repeat( (byte)'a', count ).ToArray();
			Assert.True( Reference( pattern, new() ).Match( subject ).Success );
			var actual = Compile( pattern, new() ).Match( subject );
			if ( count == 1024 ) { Assert.True( actual.IsSuccess ); Assert.Equal( count, actual.Match?.ByteLength ); }
			else { Assert.Null( actual.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code ); }
		}
		var expanded = PcreByteCompiler.Compile( "(?:a(*PRUNE)){2000}"u8.ToArray() );
		Assert.Null( expanded.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, expanded.Diagnostic?.Code );
	}

	[Fact]
	public void ScopeWalkingAndDiscardingUseTheSharedWorkAndCancellationBudget() {
		const string pattern = @"\A(?:(?:a?){20}(*PRUNE)z|a)";
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
