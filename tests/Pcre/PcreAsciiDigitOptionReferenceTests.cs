using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Local ASCII digit policies and source-byte captures against pinned PCRE2/JIT.</summary>
public sealed class PcreAsciiDigitOptionReferenceTests {
	[Theory]
	[InlineData( @"a(?-aD)(\d)", "a٣", 0, 3, 2 )]
	[InlineData( @"(?iaD)(a)(?-aD:\d)\1", "A٣A", 0, 4, 2 )]
	[InlineData( @"(?im-aD)^(\d)$", "x\n٣\nz", 2, 2, 2 )]
	[InlineData( @"(?aDaD-aD)\d", "٣", 0, 2, 1 )]
	[InlineData( @"(?-aDaD)\d", "٣", 0, 2, 1 )]
	[InlineData( @"(?aD-)\d", "٣", -1, 0, 0 )]
	[InlineData( @"(?-aD)(?aD:\d)(\d)", "3٣", 0, 3, 2 )]
	[InlineData( @"(?-aD)(\d)(?aD:\d)", "٣3", 0, 3, 2 )]
	[InlineData( @"(?-aD)(\d(?aD)|x)\d", "٣٣", 0, 4, 2 )]
	[InlineData( @"(?-aD)(?:x(?aD)\d|\d)", "٣", -1, 0, 0 )]
	[InlineData( @"(x(?-aD)\d|(\d))\d", "٣3", 0, 3, 3 )]
	[InlineData( @"(?:x(?-aD)\d|(\d))\d", "٣3", 0, 3, 2 )]
	[InlineData( @"(?>x(?-aD)\d|(\d))\d", "٣3", 0, 3, 2 )]
	[InlineData( @"(?|x(?-aD)(\d)|(\d))\d", "٣3", 0, 3, 2 )]
	[InlineData( @"(?=(?-aD)(\d))(?-aD:\d)\d", "٣3", 0, 3, 2 )]
	[InlineData( @"(?-aD:\d)(?<=(?-aD)(\d))\d", "٣3", 0, 3, 2 )]
	[InlineData( @"(?=(?-aD)\d)\d", "٣", -1, 0, 0 )]
	[InlineData( @"(?!(?-aD)x)\d", "٣", -1, 0, 0 )]
	[InlineData( @"(?<!(?-aD)x)\d", "٣", -1, 0, 0 )]
	[InlineData( @"(?-aD)(?aD:\d)+(\d)", "33٣", 0, 4, 2 )]
	[InlineData( @"(?n)(?-aD)(?aD:(\d))(?<x>\d)\k<x>", "3٣٣", 0, 5, 2 )]
	[InlineData( "(?xaD) (?-aD: \\d ) # trailing\n (\\d)", "٣3", 0, 3, 2 )]
	[InlineData( @"(?xxaD:[ \d ])(?-aD:[ \d ])", "3٣", 0, 3, 1 )]
	public void LocalPoliciesKeepExplicitOutcomesAndCaptureSlots( string pattern,
		string text, int expectedIndex, int expectedLength, int expectedGroups ) {
		var subject = Encoding.UTF8.GetBytes( text );
		var reference = Reference( pattern );
		AssertExpected( reference, subject, expectedIndex, expectedLength, expectedGroups );
		Compare( reference, subject, 0, Compile( pattern ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( @"(?^aD)\d", false, "٣", -1, 0 )]
	[InlineData( @"(?^aD:\d)", false, "٣", -1, 0 )]
	[InlineData( @"(?^aD:\D)", false, "٣", 0, 2 )]
	[InlineData( @"(?-aD:\d)", true, "٣", 0, 2 )]
	[InlineData( @"(?^:\d)", false, "٣", 0, 2 )]
	[InlineData( @"(?^:\d)", true, "٣", -1, 0 )]
	[InlineData( @"(?aD:\d)(\d)", false, "3٣", 0, 3 )]
	[InlineData( @"(?aD:\d)(\d)", true, "3٣", -1, 0 )]
	[InlineData( @"(?-aD:\d)(\d)", true, "٣٣", -1, 0 )]
	[InlineData( @"(?-aD:\d)(\d)", false, "٣٣", 0, 4 )]
	public void CallerRestrictionRestoresAfterScopesAndSurvivesReset( string pattern,
		bool asciiBsD, string text, int expectedIndex, int expectedLength ) {
		var subject = Encoding.UTF8.GetBytes( text );
		var reference = Reference( pattern, asciiBsD: asciiBsD );
		var native = reference.Match( subject );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( expectedLength, native.Length ); }
		Compare( reference, subject, 0, Compile( pattern, Options( asciiBsD: asciiBsD ) ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( @"(?aD:[\d])", "٣", -1, 0 )]
	[InlineData( @"(?-aD:[\d])", "٣", 0, 2 )]
	[InlineData( @"(?aD:[\D])", "٣", 0, 2 )]
	[InlineData( @"(?-aD:[^\D])", "٣", 0, 2 )]
	[InlineData( @"(?-aD:[^\d])", "٣", -1, 0 )]
	[InlineData( @"(?aD:[\d٣])", "٣", 0, 2 )]
	[InlineData( @"(?aD:[[:digit:]])", "٣", 0, 2 )]
	[InlineData( @"(?aD:\p{Nd})", "٣", 0, 2 )]
	[InlineData( @"(?aD:\w)", "é", 0, 2 )]
	[InlineData( @"(?aD:\s)", "\u2003", 0, 3 )]
	[InlineData( @"(?aD:\d)", "３", -1, 0 )]
	[InlineData( @"(?-aD:\d)", "𝟛", 0, 4 )]
	public void DigitClassesChangeLocallyWithoutChangingOtherPredicates( string pattern,
		string text, int expectedIndex, int expectedLength ) {
		var subject = Encoding.UTF8.GetBytes( text );
		var reference = Reference( pattern );
		AssertExpected( reference, subject, expectedIndex, expectedLength, 1 );
		Compare( reference, subject, 0, Compile( pattern ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void AsciiPoliciesAreAcceptedWithoutUcpAndKeepTheirExistingMeaning( bool utf8 ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		foreach ( var asciiBsD in new[] { false, true } ) {
			foreach ( var row in new[] {
				( Pattern: @"(?iaD)(a)(?-aD:\d)\1", Text: "A3A", Length: 3, Groups: 2 ),
				( Pattern: @"(?^aD:\d+)(?-aD:\D)", Text: "33é", Length: utf8 ? 4 : 3, Groups: 1 )
			} ) {
				var subject = encoding.GetBytes( row.Text );
				var reference = Reference( row.Pattern, utf8: utf8, ucp: false, asciiBsD: asciiBsD );
				AssertExpected( reference, subject, 0, row.Length, row.Groups );
				Compare( reference, subject, 0, Compile( row.Pattern,
					Options( utf8: utf8, ucp: false, asciiBsD: asciiBsD ) ).Match( subject ), row.Pattern );
			}
		}
		if ( utf8 ) {
			const string pattern = @"(?-aD:\d)";
			var subject = "٣"u8.ToArray();
			var reference = Reference( pattern, ucp: false );
			Assert.False( reference.Match( subject ).Success );
			Compare( reference, subject, 0, Compile( pattern, Options( ucp: false ) ).Match( subject ), pattern );
		}
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax,
			PcreByteCompiler.Compile( "(?aD)\\d"u8.ToArray(), new PcreByteOptions { Ucp = true } ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void SelectedMalformedUtfAndCrLfOffsetsKeepAllCaptureSpans( bool anyCrLf ) {
		foreach ( var pattern in new[] {
			@"(?-aD:(\d?))(?aD:\D?)", @"(?aD:[^\d])|(?-aD:(\d))",
			@"(?=(?-aD)(\d))(?-aD:\d)", @"(?<=(?-aD)(\d))(?aD:\D)",
			"(?maD)^(?-aD:(\\d))$"
		} ) {
			var reference = Reference( pattern, invalidUtf: true, anyCrLf: anyCrLf );
			var managed = Compile( pattern, Options( invalidUtf: true, anyCrLf: anyCrLf ) );
			foreach ( var hex in new[] { "", "33D9A3", "D9A333", "FFD9A3FF33", "D9FF33", "0D0AD9A30D0A" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, managed.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"(?aD)*", 5 )]
	[InlineData( @"(?aD){2}", 5 )]
	[InlineData( @"(?-aD){2}", 6 )]
	[InlineData( @"a(?iaD)+", 7 )]
	[InlineData( "(?x)(?aD) #c\n *", 14 )]
	public void UnifiedBareSettingsRetainNonrepeatabilityAndOriginalQuantifierOffsets( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"(?ad:\d)", 0 )]
	[InlineData( @"(?AD:\d)", 0 )]
	[InlineData( @"(?a D:\d)", 0 )]
	[InlineData( @"(?aD aD:\d)", 0 )]
	[InlineData( @"(?aD-aD-aD:\d)", 0 )]
	[InlineData( @"(?^aD-aD:\d)", 0 )]
	[InlineData( @"é(?aD", 2 )]
	public void MalformedOptionTokensStayAtTheirOriginalByteOpeners( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"(?a:\d)" )]
	[InlineData( @"(?aS:\s)" )]
	[InlineData( @"(?aW:\w)" )]
	[InlineData( @"(?aP:[[:digit:]])" )]
	[InlineData( @"(?aT:[[:digit:]])" )]
	[InlineData( @"(?aDJ:\d)" )]
	[InlineData( @"(?aDr:\d)" )]
	public void OtherOptionFamiliesRemainExplicitlyPending( string pattern ) {
		Reference( pattern );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void UnclosedScopedPolicyKeepsItsOriginalUtfByteOpener() {
		const string pattern = @"é(?aD:\d";
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( 2, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void ScopedAndBarePoliciesRetainDepthCaptureAndCancellationLimits() {
		var nested = string.Concat( Enumerable.Repeat( "(?aD:", 256 ) ) + @"\d" + new string( ')', 256 );
		Assert.Equal( 1, Compile( nested ).Match( "3"u8.ToArray() ).Match?.ByteLength );
		var tooDeep = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( "(?aD:" + nested + ")" ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, tooDeep.Diagnostic?.Code );
		Assert.Equal( 1280, tooDeep.Diagnostic?.ByteOffset );
		var captures = "(?-aD)" + string.Concat( Enumerable.Repeat( "()", 256 ) );
		Assert.Equal( 257, Compile( captures ).Match( Array.Empty<byte>() ).Match!.Captures.Count );
		Assert.Equal( PcreByteDiagnosticCode.PatternCaptureLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( captures + "()" ), Options() ).Diagnostic?.Code );
		var settings = string.Concat( Enumerable.Repeat( "(?aD)(?-aD)", 1000 ) );
		Assert.Single( Compile( settings + @"\d" ).Match( "٣"u8.ToArray() ).Match!.Captures );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( settings ), Options(), cancellation.Token ) );
	}

	[Fact]
	public void ScopedDigitPredicatesKeepWorkAndMatchCancellationBudgets() {
		const string pattern = @"^(?-aD:(\d|\d\d)*)[b]$";
		var subject = Enumerable.Repeat( (byte)'3', 40 ).ToArray();
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, Options() with { MatchWorkLimit = 500 } ).Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, Options() with {
			WorkObserver = count => { if ( count == 20 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}

	private static void AssertExpected( PcreRegex8Bit reference, byte[] subject,
		int index, int length, int groups ) {
		var native = reference.Match( subject );
		Assert.Equal( index, native.Success ? native.Index : -1 );
		if ( !native.Success ) { return; }
		Assert.Equal( length, native.Length );
		Assert.Equal( groups, native.Groups.Count );
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset,
		PcreByteMatchResult actual, string context ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( native.Success ? native.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, actual.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var capture = native.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null,
				actual.Match.Captures[group] );
		}
	}

	private static PcreByteOptions Options( bool utf8 = true, bool ucp = true,
		bool asciiBsD = true, bool invalidUtf = false, bool anyCrLf = false ) => new() {
		Utf8 = utf8, Ucp = ucp, AsciiBsD = asciiBsD,
		MatchInvalidUtf = invalidUtf, AnyCrLfNewline = anyCrLf
	};

	private static PcreRegex8Bit Reference( string pattern, bool utf8 = true, bool ucp = true,
		bool asciiBsD = true, bool invalidUtf = false, bool anyCrLf = false ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( ucp ) { options |= PcreOptions.Ucp; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings {
				Options = options,
				ExtraCompileOptions = asciiBsD ? PcreExtraCompileOptions.AsciiBsD : PcreExtraCompileOptions.None
			} );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		options ??= Options();
		var encoding = options.Utf8 || options.MatchInvalidUtf ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
