using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Static conditions against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreStaticConditionReferenceTests {
	[Theory]
	[InlineData( @"^(?(DEFINE)(a|b))c$", "c", 0, 1, 2 )]
	[InlineData( @"^(?<DEFINE>a)(?(DEFINE)b)c$", "ac", 0, 2, 2 )]
	[InlineData( @"^(?<DEFINE>a)(?(<DEFINE>)b|c)$", "ab", 0, 2, 2 )]
	[InlineData( @"^(?<DEFINE>a)(?('DEFINE')b|c)$", "ab", 0, 2, 2 )]
	[InlineData( @"^(?<VERSION>a)(?(VERSION)b|c)$", "ab", 0, 2, 2 )]
	[InlineData( @"^(?<VERSIONX>a)(?(<VERSIONX>)b|c)$", "ab", 0, 2, 2 )]
	[InlineData( @"^(?<versionx>a)(?(versionx)b|c)$", "ab", 0, 2, 2 )]
	[InlineData( @"^(?(DEFINE)(?<x>a))(?(x)b|c)(d)$", "cd", 0, 2, 3 )]
	[InlineData( @"^(?(x)b|c)(?(DEFINE)(?<x>a))(d)$", "cd", 0, 2, 3 )]
	[InlineData( @"^(?(DEFINE)(a))\1$", "a", -1, 0, 0 )]
	[InlineData( @"^(?(DEFINE)(a)){0}(b)\2$", "bb", 0, 2, 3 )]
	[InlineData( @"^(?(VERSION=10.48)(a)|(b))(c)$", "ac", 0, 2, 4 )]
	[InlineData( @"^(?(VERSION=10.480)(a)|(b))(c)$", "bc", 0, 2, 4 )]
	[InlineData( @"^(?(VERSION=10.48)b|a)$", "a", -1, 0, 0 )]
	[InlineData( @"^(?(VERSION=10.49)a|b)$", "a", -1, 0, 0 )]
	[InlineData( @"^(?(VERSION=10.48)(?i)a|b)c$", "Ac", 0, 2, 1 )]
	[InlineData( @"^(?(VERSION=10.49)(?i)a|b)c$", "Bc", 0, 2, 1 )]
	[InlineData( @"^(?(VERSION=10.49)(?i)a|b)c$", "BC", -1, 0, 0 )]
	[InlineData( @"^(?(DEFINE)(?i)a)b$", "B", -1, 0, 0 )]
	[InlineData( @"^(?(VERSION=10.49)a){300}+b$", "b", 0, 1, 1 )]
	[InlineData( @"^(?(DEFINE)(a)){300}+b$", "b", 0, 1, 2 )]
	[InlineData( @"^(?(VERSION=10.49)a|){2}b$", "b", 0, 1, 1 )]
	[InlineData( @"^(?(VERSION=10.48)a\K|b)c$", "ac", 1, 1, 1 )]
	[InlineData( @"^(?(VERSION=10.49)a|b\K)c$", "bc", 1, 1, 1 )]
	[InlineData( @"(?<=(?(DEFINE)a*))b", "b", 0, 1, 1 )]
	[InlineData( @"(?<=(?(DEFINE)a)){256}b", "b", 0, 1, 1 )]
	[InlineData( @"(?<=(?(VERSION=10.49)a))b", "b", -1, 0, 0 )]
	[InlineData( @"(?<=(?(VERSION=10.49)a))b", "ab", 1, 1, 1 )]
	[InlineData( @"(?<=(?(VERSION=10.49)a){2})b", "ab", -1, 0, 0 )]
	[InlineData( @"(?<=(?(VERSION=10.49)a){2})b", "aab", 2, 1, 1 )]
	[InlineData( @"(?<=(?(VERSION=10.49)a|))b", "b", 0, 1, 1 )]
	[InlineData( @"(?<=(?(VERSION=10.48)a|bb))c", "ac", 1, 1, 1 )]
	[InlineData( @"(?<=(?(DEFINE)(?<=a*)))b", "b", 0, 1, 1 )]
	[InlineData( @"(?<=(?(DEFINE)(?=(?<=a*))))b", "b", 0, 1, 1 )]
	[InlineData( @"(?<=(?=(?<=(?(DEFINE)(?<=a*)))))b", "b", 0, 1, 1 )]
	public void SelectionDeclarationsOptionsAndWidthsPreserveNativeSpans( string pattern, string text,
		int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( "=10.48", true )]
	[InlineData( "=10.048", true )]
	[InlineData( "=00010.00048", true )]
	[InlineData( "=10.480", false )]
	[InlineData( "=10", false )]
	[InlineData( "=0", false )]
	[InlineData( "=1000.1000", false )]
	[InlineData( ">=10", true )]
	[InlineData( ">=10.048", true )]
	[InlineData( ">=10.480", false )]
	[InlineData( ">=11", false )]
	[InlineData( ">=0.1000", true )]
	public void VersionUsesBoundedIntegerComponentsRatherThanDecimals( string comparison, bool selected ) =>
		Check( "^(?(VERSION" + comparison + ")(a)|(b))$", selected ? "a" : "b", 0, 1, 3 );

	[Theory]
	[InlineData( "VERSIONX" )]
	[InlineData( "VERSION=" )]
	[InlineData( "VERSION>10" )]
	[InlineData( "VERSION<=10.48" )]
	[InlineData( "VERSION==10.48" )]
	[InlineData( "VERSION!=10.48" )]
	[InlineData( "VERSION=+10" )]
	[InlineData( "VERSION=-10" )]
	[InlineData( "VERSION=10." )]
	[InlineData( "VERSION=.48" )]
	[InlineData( "VERSION=10.48.0" )]
	[InlineData( "VERSION=1001" )]
	[InlineData( "VERSION=10.1001" )]
	[InlineData( "VERSION=2147483648" )]
	[InlineData( "VERSION=١٠.48" )]
	[InlineData( "VERSION=10.４８" )]
	[InlineData( "VERSION= 10.48" )]
	[InlineData( "VERSION=10.48 " )]
	public void ReservedMalformedHeadersReportTheOriginalUtfByteOpener( string header ) {
		var pattern = "é(?x)(?(" + header + ")a|b)";
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, 6 );
	}

	[Theory]
	[InlineData( "(?(DEFINE)a|b)", 0 )]
	[InlineData( "é(?(VERSION=10.48)a|b|c)", 2 )]
	[InlineData( "(?<VERSIONX>a)(?(VERSIONX)b|c)", 14 )]
	[InlineData( @"(?(DEFINE)\g{9})", 10 )]
	[InlineData( @"(?(VERSION=10.48)a|\g{9})", 19 )]
	[InlineData( @"(?(VERSION=0)[)", 13 )]
	[InlineData( @"(?(DEFINE)a){3,2}", 12 )]
	[InlineData( @"(?(DEFINE)a)++?", 14 )]
	[InlineData( @"(?=(?(DEFINE)\K))", 0 )]
	[InlineData( @"(?=(?(VERSION=0)\K|a))", 0 )]
	public void UnexecutedBodiesAndIgnoredRepeatsStillValidate( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, offset );
	}

	[Theory]
	[InlineData( "(?(DEFINE)(?R))", 10 )]
	[InlineData( "(a)(?(VERSION=0)(?1)|b)", 16 )]
	[InlineData( "(?(VERSION=10.48)a|(?C))", 19 )]
	public void PendingFamiliesStayUnsupportedAtTheirActualSyntaxOpener( string pattern, int offset ) {
		_ = Reference( pattern );
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, offset );
	}

	[Theory]
	[InlineData( "(?(DEFINE)(?<=a*))" )]
	[InlineData( "(?<=(?=(?(DEFINE)(?<=a*))))b" )]
	[InlineData( "(?<=(?(DEFINE)(?<=a*)))b(?<=a*)" )]
	[InlineData( @"(?<=(?(DEFINE)(?<x>(?<=a*))))b(?<=\k<x>)" )]
	[InlineData( "(?(VERSION=10.48)a|(?<=a*))" )]
	[InlineData( "(?(VERSION=0)(?<=a*)|b){0}" )]
	[InlineData( "(?<=(?(DEFINE)a)*)b" )]
	[InlineData( "(?<=(?(VERSION=0)a)*)b" )]
	[InlineData( "(?<=(?(VERSION=10.48)a{256}|))b" )]
	public void GlobalScansAndConsumedReferencesRestoreBehindValidation( string pattern ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax,
			PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() ).Diagnostic?.Code );
	}

	[Fact]
	public void ExplicitOnlyWidthsAndAllBodyNewlineMetadataSurviveStaticSelection() {
		foreach ( var condition in new[] { "VERSION=10.48", "VERSION=0" } ) {
			Check( "(?<=(?(" + condition + ")a{256}))b", new string( 'a', 256 ) + "b", 256, 1, 1 );
		}
		foreach ( var pattern in new[] { @"(?(DEFINE)\n)(?s:.)\z", @"(?(VERSION=0)\n)(?s:.)\z",
			@"(?(VERSION=10.48)|\n)(?s:.)\z" } ) { Check( pattern, "\r\n", 1, 1, 1, anyCrLf: true ); }
		Check( @"(?s:.)\z", "\r\n", -1, 0, 0, anyCrLf: true );
	}

	[Fact]
	public void SelectedNullabilityMalformedUtfCrLfAndGOffsetsUseTheSharedMatcher() {
		foreach ( var anyCrLf in new[] { false, true } ) {
			foreach ( var pattern in new[] { @"(?(DEFINE)a)", @"(?(VERSION=0)a)", @"(?(VERSION=10.48)a|)",
				@"(?(VERSION=0)a|)", @"\G(?(VERSION=10.48)(.?))", @"(?(DEFINE)\n)(?m)$" } ) {
				var reference = Reference( pattern, invalidUtf: true, anyCrLf: anyCrLf );
				var compiled = Compile( pattern, Options( invalidUtf: true, anyCrLf: anyCrLf ) );
				foreach ( var hex in new[] { "", "61FF62", "FFC3A9", "FFFF", "0D0A610D0A" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), pattern + ", " + hex );
					}
				}
			}
		}
	}

	[Fact]
	public void DepthCapturesInstructionsAndRepeatMarksKeepExistingBudgets() {
		const string header = "(?(VERSION=10.48)";
		var nested = string.Concat( Enumerable.Repeat( header, 256 ) ) + "a" + new string( ')', 256 );
		Check( nested, "a", 0, 1, 1 );
		Diagnostic( header + nested + ")", PcreByteDiagnosticCode.PatternDepthLimitExceeded, 256 * header.Length );
		var declarations = "(?(DEFINE)" + string.Concat( Enumerable.Repeat( "()", 256 ) );
		Check( declarations + ")a", "a", 0, 1, 257 );
		Diagnostic( declarations + "())a", PcreByteDiagnosticCode.PatternCaptureLimitExceeded, declarations.Length );
		var ignored = string.Concat( Enumerable.Repeat( "(?(DEFINE)a)+", 300 ) );
		Check( ignored + string.Concat( Enumerable.Repeat( "a*", 256 ) ), "", 0, 0, 1 );
		var ordinary = string.Concat( Enumerable.Repeat( "(?(VERSION=0)a|)*", 257 ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( ordinary ), Options() ).Diagnostic?.Code );
		foreach ( var pattern in new[] { "(?(DEFINE)" + string.Concat( Enumerable.Repeat( "[a]", 4096 ) ) + ")",
			"(?(VERSION=10.48)a|b){2048}" } ) {
			_ = Reference( pattern );
			Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
				PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), Options() ).Diagnostic?.Code );
		}
		foreach ( var condition in new[] { "DEFINE", "VERSION=0" } ) {
			foreach ( var suffix in new[] { "{65535}", "{65535,}" } ) {
				var pattern = "(?(" + condition + ")a)" + suffix;
				Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
				Diagnostic( pattern, PcreByteDiagnosticCode.PatternRepeatLimitExceeded, pattern.IndexOf( '{' ) );
			}
		}
	}

	[Fact]
	public void SelectedWorkAndCancellationStayBounded() {
		const string pattern = "^(?(VERSION=10.48)(a|aa)*b|x)$";
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

	private static void Check( string pattern, string text, int index, int length, int groups, bool anyCrLf = false ) {
		foreach ( var utf8 in new[] { false, true } ) {
			var subject = ( utf8 ? Encoding.UTF8 : Encoding.Latin1 ).GetBytes( text );
			var reference = Reference( pattern, anyCrLf: anyCrLf, utf8: utf8 );
			var native = reference.Match( subject );
			Assert.Equal( index, native.Success ? native.Index : -1 );
			if ( native.Success ) { Assert.Equal( length, native.Length ); Assert.Equal( groups, native.Groups.Count ); }
			Compare( reference, subject, 0, Compile( pattern, Options( anyCrLf: anyCrLf, utf8: utf8 ) ).Match( subject ), pattern );
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
