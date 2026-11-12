using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pinned PCRE2 10.48 successful termination, closes, scopes and byte spans.</summary>
public sealed class PcreAcceptVerbReferenceTests {
	[Theory]
	[InlineData( "(*ACCEPT)b", "61", "0:0", false )]
	[InlineData( "a(*ACCEPT)b", "6162", "0:1", false )]
	[InlineData( "A((?:A|B(*ACCEPT)|C)D)", "4142", "0:2,1:1", false )]
	[InlineData( "A((?:A|B(*ACCEPT)|C)D)", "414144", "0:3,1:2", false )]
	[InlineData( "(a(b(*ACCEPT)c)d)e", "6162", "0:2,0:2,1:1", false )]
	[InlineData( "(?|(a(*ACCEPT)b)|(c))d", "61", "0:1,0:1", false )]
	[InlineData( "(?n)(a)(?<x>b(*ACCEPT)c)", "6162", "0:2,1:1", false )]
	[InlineData( "(?i)(?<x>É(*ACCEPT)z)", "C3A9", "0:2,0:2", true )]
	[InlineData( "(?x)(a (*ACCEPT) # tail\n b)", "61", "0:1,0:1", false )]
	[InlineData( "\\Q(*ACCEPT)\\E", "282A41434345505429", "0:9", false )]
	[InlineData( "[(*ACCEPT)]", "41", "0:1", false )]
	[InlineData( "(a(*ACCEPT)??b)c", "616263", "0:3,0:2", false )]
	[InlineData( "(a(*ACCEPT)??b)c", "6178", "0:1,0:1", false )]
	[InlineData( "(a(*ACCEPT)??b)c", "616278", "0:1,0:1", false )]
	[InlineData( "(a(*ACCEPT)?b)c", "616263", "0:1,0:1", false )]
	[InlineData( "(?U)(a(*ACCEPT)?b)c", "616263", "0:3,0:2", false )]
	[InlineData( "(?U)(a(*ACCEPT)??b)c", "616263", "0:1,0:1", false )]
	[InlineData( "a(*ACCEPT){0}b", "6162", "0:2", false )]
	[InlineData( "a(*ACCEPT){0,2}?b", "6178", "0:1", false )]
	[InlineData( "a(*ACCEPT){2}b", "61", "0:1", false )]
	[InlineData( "a(*ACCEPT)*b", "61", "0:1", false )]
	[InlineData( "a(*ACCEPT)++b", "61", "0:1", false )]
	[InlineData( "a(*ACCEPT)?+b", "61", "0:1", false )]
	[InlineData( "(a(*ACCEPT)??b)c|abd", "616264", "0:1,0:1", false )]
	[InlineData( "a\\K(b(*ACCEPT)c)", "6162", "1:1,1:1", false )]
	[InlineData( "(a(*ACCEPT)b)|c", "63", "0:1,-", false )]
	[InlineData( "(*NOTEMPTY)(*ACCEPT)|a", "61", "0:1", false )]
	[InlineData( "(*NOTEMPTY)((*ACCEPT))|a", "61", "0:1,-", false )]
	[InlineData( "(*NOTEMPTY_ATSTART)(*ACCEPT)|b", "61", "1:0", false )]
	[InlineData( "(*NOTEMPTY)a\\K(*ACCEPT)|ab", "6162", "0:2", false )]
	[InlineData( "(?=(a(*ACCEPT)b))\\1", "61", "0:1,0:1", false )]
	[InlineData( "(?!(a(*ACCEPT)b))a|a", "61", "0:1,-", false )]
	[InlineData( "(a(?=(b(*ACCEPT)c))b)", "6162", "0:2,0:2,1:1", false )]
	[InlineData( "(?=(a(?=(b(*ACCEPT)c))b(*ACCEPT)c))ab", "6162", "0:2,0:2,1:1", false )]
	[InlineData( "(?(?=(a(*ACCEPT)b))\\1|x)", "61", "0:1,0:1", false )]
	[InlineData( "(?(?!(a(*ACCEPT)b))x|\\1)", "61", "0:1,0:1", false )]
	[InlineData( "(?(?=(a(*ACCEPT)b))X|a)|a", "61", "0:1,-", false )]
	[InlineData( "(?=(a(*ACCEPT)b))X|a", "61", "0:1,-", false )]
	[InlineData( "(?*(a(*ACCEPT)|ab))\\1c", "616263", "", false )]
	[InlineData( "(?*(a|ab)(*ACCEPT))\\1c", "616263", "", false )]
	[InlineData( "(?<=(a(*ACCEPT)z))x", "6178", "1:1,0:1", false )]
	[InlineData( "(?<=(a(*ACCEPT)b))x", "616278", "", false )]
	[InlineData( "(?<=(*ACCEPT)a*)x", "78", "0:1", false )]
	[InlineData( "(?<=(?:a(*ACCEPT))b)x", "616278", "2:1", false )]
	[InlineData( "(?<!(a(*ACCEPT)b))x|x", "6178", "1:1,-", false )]
	[InlineData( "(?<=(a{1,2}(*ACCEPT)b))x", "616178", "2:1,0:2", false )]
	[InlineData( "(?<=(a(*ACCEPT)(?<=b*)))x", "6178", "1:1,0:1", false )]
	[InlineData( "\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT)|ab))", "616263", "", false )]
	[InlineData( "\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT)|ab))", "6163", "0:2,-", false )]
	[InlineData( "\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT)|ab))", "616263", "0:3,-", false )]
	[InlineData( "\\A(?=(?&x))ac(?(DEFINE)(?<x>a(*ACCEPT)b))", "6163", "0:2,-", false )]
	[InlineData( "\\A(?&x)c(?(DEFINE)(?<x>(?=(a(*ACCEPT)b))a))", "6163", "0:2,-,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(a(*ACCEPT)b)))(?&x(2))\\2", "6161", "", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>(a)(*ACCEPT)b))(?&x(2))\\2", "6161", "0:2,-,0:1", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a\\K(*ACCEPT)b))(?&x)c", "6163", "1:1,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*ACCEPT)b))(?&x)c", "616163", "0:3,-", false )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&y)z)(?<y>b(*ACCEPT)c))(?&x)d", "61627A64", "0:4,-,-", false )]
	[InlineData( "(*sr:あ漢가(*ACCEPT))X", "E38182E6BCA2EAB080", "0:9", true )]
	[InlineData( "(?=(*sr:あ漢가(*ACCEPT))X).+", "E38182E6BCA2EAB080", "0:9", true )]
	[InlineData( "(?>(*ACCEPT))X", "61", "0:0", false )]
	[InlineData( "(?=(?>(*ACCEPT))X)a", "61", "0:1", false )]
	[InlineData( "(?<x>a(*sr:b(*ACCEPT)c)d)", "6162", "0:2,0:2", false )]
	public void ImmediateSuccessHasPinnedCapturesAndCommitsOnlyTheAcceptedScope( string pattern, string hex, string spans, bool utfOnly ) {
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
	public void EveryOffsetRetainsNullableInvalidFragmentsAndCrLfEntry( bool anyCrLf ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf };
			foreach ( var pattern in new[] { "(*ACCEPT)X", "((*ACCEPT))X", "a(*ACCEPT)X", @"a\K((*ACCEPT))X|b",
				"(?=(a(*ACCEPT)b))a", "(?!(a(*ACCEPT)b))b", "(*NOTEMPTY)(*ACCEPT)|b", @"[a\n](*ACCEPT)X", "(?<=(*ACCEPT))b" } ) {
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
	[InlineData( @"(?<=a*(*ACCEPT))x", 0, true )]
	[InlineData( @"(*ACCEPT)\k<missing>", 9, false )]
	[InlineData( @"(*ACCEPT)\2", 9, false )]
	[InlineData( @"(*ACCEPT)**", 10, false )]
	[InlineData( @"é(*ACCEPT){65536}", 11, false )]
	public void AcceptanceDoesNotHideGlobalReferencesRepeatErrorsOrEarlierUnboundedWidths( string pattern, int offset, bool widthError ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.Equal( widthError ? PcreByteDiagnosticCode.UnsupportedSyntax : PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(*ACCEPT :x)" )]
	[InlineData( "(*ACCEPTX:x)" )]
	[InlineData( "(*accept)" )]
	[InlineData( "(*ACCEPTx)" )]
	public void NonExactHeadersRemainExplicitlyUnsupported( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void AcceptanceClosesAndScopePruningShareWorkCancellationAndCompilationLimits() {
		const string pattern = "(?=(a(a(a(*ACCEPT)))))aaa";
		var subject = "aaa"u8.ToArray(); var steps = new List<int>();
		var compiled = Compile( pattern, new() { WorkObserver = steps.Add } );
		Compare( Reference( pattern, new() ), subject, 0, compiled.Match( subject ), pattern );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		var limited = Compile( pattern, new() { MatchWorkLimit = steps.Count - 1 } ).Match( subject );
		Assert.Null( limited.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { WorkObserver = count => { if ( count == 10 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
		var expanded = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( String.Concat( Enumerable.Repeat( "(*ACCEPT)", 5000 ) ) ) );
		Assert.Null( expanded.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, expanded.Diagnostic?.Code );
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
