using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Scoped ASCII restriction masks against the pinned PCRE2/JIT oracle.</summary>
public sealed class PcreAsciiRestrictionReferenceTests {
	[Theory]
	[InlineData( @"(?aS:\s)", "\u2003", -1, 0, 0 )]
	[InlineData( @"(?aS:\S)", "\u2003", 0, 3, 1 )]
	[InlineData( @"(?aS-aS:\s)", "\u2003", 0, 3, 1 )]
	[InlineData( @"(?aW:\w)", "é", -1, 0, 0 )]
	[InlineData( @"(?aW:\W)", "é", 0, 2, 1 )]
	[InlineData( @"(?aW-aW:\w)", "é", 0, 2, 1 )]
	[InlineData( @"(?aT:[[:digit:]])", "٣", -1, 0, 0 )]
	[InlineData( @"(?aT:[[:xdigit:]])", "Ｆ", -1, 0, 0 )]
	[InlineData( @"(?aT:[[:alnum:]])", "٣", 0, 2, 1 )]
	[InlineData( @"(?aP:\w)", "é", 0, 2, 1 )]
	[InlineData( @"(?aP:\s)", "\u2003", 0, 3, 1 )]
	[InlineData( @"(?aS:[[:space:]])", "\u2003", 0, 3, 1 )]
	[InlineData( @"(?aW:[[:word:]])", "é", 0, 2, 1 )]
	[InlineData( @"(?aT:\d)", "٣", 0, 2, 1 )]
	[InlineData( @"(?aD:[[:digit:]])", "٣", 0, 2, 1 )]
	[InlineData( @"(?a:\p{L})", "é", 0, 2, 1 )]
	[InlineData( @"(?a:\p{Nd})", "٣", 0, 2, 1 )]
	[InlineData( @"(?a:\h)", "\u00a0", 0, 2, 1 )]
	[InlineData( @"(?a:\v)", "\u2028", 0, 3, 1 )]
	[InlineData( @"(?a:\R)", "\u2028", 0, 3, 1 )]
	[InlineData( @"(?a:[^[:alpha:]])", "é", 0, 2, 1 )]
	[InlineData( @"(?aP:[[:^alpha:]])", "é", 0, 2, 1 )]
	[InlineData( @"(?aP:[^[:^alpha:]])", "é", -1, 0, 0 )]
	[InlineData( @"(?aP:[[:alpha:]é])", "é", 0, 2, 1 )]
	[InlineData( @"(?aW:[\wé])", "é", 0, 2, 1 )]
	public void IndividualRestrictionsKeepTheirIndependentPredicates( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( @"(?aP-aT)[[:digit:]]", "٣", -1, 0, 0 )]
	[InlineData( @"(?aT-aP)[[:digit:]]", "٣", 0, 2, 1 )]
	[InlineData( @"(?a-aT)[[:digit:]]", "٣", -1, 0, 0 )]
	[InlineData( @"(?a-aP)[[:digit:]]", "٣", 0, 2, 1 )]
	[InlineData( @"(?a-aP)\d", "٣", -1, 0, 0 )]
	[InlineData( @"(?a-aD)\d", "٣", 0, 2, 1 )]
	[InlineData( @"(?a-aD)[[:digit:]]", "٣", -1, 0, 0 )]
	[InlineData( @"(?aP-aP)[[:alpha:]]", "é", 0, 2, 1 )]
	[InlineData( @"(?aPaT-aP)[[:digit:]]", "٣", 0, 2, 1 )]
	[InlineData( @"(?aPaT-aT)[[:digit:]]", "٣", -1, 0, 0 )]
	[InlineData( @"(?aT-aT)[[:xdigit:]]", "Ｆ", 0, 3, 1 )]
	[InlineData( @"(?aaT-aP)\s", "\u2003", -1, 0, 0 )]
	[InlineData( @"(?aP)(?^)[[:alpha:]]", "é", -1, 0, 0 )]
	[InlineData( @"(?^aP)[[:alpha:]]", "é", -1, 0, 0 )]
	[InlineData( @"(?a)(?^)\s", "\u2003", -1, 0, 0 )]
	[InlineData( @"(?a)(?-a)\s", "\u2003", 0, 3, 1 )]
	[InlineData( @"(?ai)(a)\w", "Aa", 0, 2, 2 )]
	[InlineData( @"(?aSiaW)(a)\w", "Aa", 0, 2, 2 )]
	public void OverlappingMasksAndBareAFollowNativeSetUnsetAlgebra( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( "alnum", "é", "a", true )]
	[InlineData( "alpha", "é", "a", true )]
	[InlineData( "ascii", "é", "\u007f", false )]
	[InlineData( "blank", "\u00a0", "\t", true )]
	[InlineData( "cntrl", "\u0085", "\u007f", true )]
	[InlineData( "digit", "٣", "3", false )]
	[InlineData( "graph", "\u200d", "!", true )]
	[InlineData( "lower", "ß", "a", true )]
	[InlineData( "print", "\u00a0", " ", true )]
	[InlineData( "punct", "£", "!", true )]
	[InlineData( "space", "\u2003", "\t", true )]
	[InlineData( "upper", "Σ", "A", true )]
	[InlineData( "word", "é", "_", true )]
	[InlineData( "xdigit", "Ｆ", "F", false )]
	public void EveryPosixNameDistinguishesAllPosixFromDigitOnlyRestriction( string name,
		string unicodeText, string asciiText, bool digitOnlyMatches ) {
		foreach ( var flag in new[] { "aP", "aT" } ) {
			var pattern = $"(?{flag}:[[:{name}:]])";
			Check( pattern, asciiText, 0, Encoding.UTF8.GetByteCount( asciiText ), 1 );
			var expected = flag == "aT" && digitOnlyMatches;
			Check( pattern, unicodeText, expected ? 0 : -1,
				expected ? Encoding.UTF8.GetByteCount( unicodeText ) : 0, expected ? 1 : 0 );
		}
	}

	[Theory]
	[InlineData( @"(?aW:(\w))(\w)", "aé", 0, 3, 3 )]
	[InlineData( @"(?a:\w)(\s)([[:digit:]])", "a\u2003٣", 0, 6, 3 )]
	[InlineData( @"(\w)(?aW:\w)", "éa", 0, 3, 2 )]
	[InlineData( @"(\w(?aW)|x)\w", "éé", 0, 4, 2 )]
	[InlineData( @"(?:x(?aW)\w|\w)", "é", -1, 0, 0 )]
	[InlineData( @"(x(?aW)\w|(\w))\w", "aé", 0, 3, 3 )]
	[InlineData( @"(?:x(?aW)\w|(\w))\w", "aé", 0, 3, 2 )]
	[InlineData( @"(?>x(?aW)\w|(\w))\w", "aé", 0, 3, 2 )]
	[InlineData( @"(?|x(?aW)(\w)|(\w))\w", "aé", 0, 3, 2 )]
	[InlineData( @"(?=(?aW)(\w))\w(\w)", "aé", 0, 3, 3 )]
	[InlineData( @"(?<=x(?aW)(\w))(\w)", "xaé", 2, 2, 3 )]
	[InlineData( @"(?aW:\w)+(\w)", "aaé", 0, 4, 2 )]
	[InlineData( @"(?n)(?aW:(\w))(?<x>\w)\k<x>", "aéé", 0, 5, 2 )]
	[InlineData( "(?sUaW:(.)(\\w+))", "\naaa", 0, 2, 3 )]
	[InlineData( "(?xxaS:[ \\s ])", "\t", 0, 1, 1 )]
	public void AllGroupKindsRestoreMasksAndPriorNodesKeepTheirOwnPolicy( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( @"(?iaP)[[:alpha:]]", "K", -1, 0, 0 )]
	[InlineData( @"(?iaP)[[:lower:]]", "K", -1, 0, 0 )]
	[InlineData( @"(?iaP)[[:upper:]]", "ſ", -1, 0, 0 )]
	[InlineData( @"(?iaW)\w", "K", -1, 0, 0 )]
	[InlineData( @"(?iaW)[\w]", "K", -1, 0, 0 )]
	[InlineData( @"(?iaP)k", "K", 0, 3, 1 )]
	[InlineData( @"(?iaP)[k]", "K", 0, 3, 1 )]
	[InlineData( @"(?iaP)[a-z]", "ſ", 0, 2, 1 )]
	[InlineData( @"(?iaP)[[:lower:]]", "A", 0, 1, 1 )]
	public void AsciiClassesStayStrictWhileLiteralAndRangeCaseFoldingIsIndependent( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( @"(?aW)\b", "é", -1, 0, 0 )]
	[InlineData( @"(?aW)\B", "é", 0, 0, 1 )]
	[InlineData( @"(?aW)a\b", "aé", 0, 1, 1 )]
	[InlineData( @"(?aS)a\b", "aé", -1, 0, 0 )]
	[InlineData( @"(?aP)a\b", "aé", -1, 0, 0 )]
	[InlineData( @"(?aW:a\b)(\w)", "aé", 0, 3, 2 )]
	[InlineData( @"(?aW)(?-aW)a\b", "aé", -1, 0, 0 )]
	[InlineData( @"(?iaW)\BK", "K", 0, 3, 1 )]
	public void OnlyWordRestrictionChangesWordBoundaryClassification( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void CallerDigitRestrictionSurvivesOtherBitsAndRestoresAfterBareAScope( bool asciiBsD ) {
		foreach ( var pattern in new[] { @"(?aW:\w)(\d)", @"(?a:\w)(\d)" } ) {
			var subject = "a٣"u8.ToArray();
			var reference = Reference( pattern, asciiBsD: asciiBsD );
			AssertExpected( reference, subject, asciiBsD ? -1 : 0, asciiBsD ? 0 : 3, asciiBsD ? 0 : 2 );
			Compare( reference, subject, 0, Compile( pattern, Options( asciiBsD: asciiBsD ) ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ByteAndNonUcpModesAcceptRestrictionsWithoutWideningUnicode( bool utf8 ) {
		const string pattern = @"(?a:\w+)\W(?-a:[[:alpha:]])";
		var subject = "aa!b"u8.ToArray();
		var reference = Reference( pattern, utf8: utf8, ucp: false );
		AssertExpected( reference, subject, 0, 4, 1 );
		Compare( reference, subject, 0, Compile( pattern, Options( utf8: utf8, ucp: false ) ).Match( subject ), pattern );
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var nonword = encoding.GetBytes( "é" );
		const string negative = @"(?-a:\w)";
		var negativeReference = Reference( negative, utf8: utf8, ucp: false );
		Assert.False( negativeReference.Match( nonword ).Success );
		Compare( negativeReference, nonword, 0, Compile( negative,
			Options( utf8: utf8, ucp: false ) ).Match( nonword ), negative );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax,
			PcreByteCompiler.Compile( "(?a)\\w"u8.ToArray(), new PcreByteOptions { Ucp = true } ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void BoundaryAndClassMasksKeepSelectedMalformedUtfAndCrLfOffsets( bool anyCrLf ) {
		foreach ( var pattern in new[] {
			@"(?aW)\b(\w*)", @"(?aW)\B", @"(?aW:\W)|(?-aW:(\w))",
			@"(?aP:[^[:alpha:]])|(?-aP:([[:alpha:]]))",
			@"(?<=(?aW)(\w))(?-aW:\w)", "(?maS)^(\\S*)$"
		} ) {
			var reference = Reference( pattern, invalidUtf: true, anyCrLf: anyCrLf );
			var managed = Compile( pattern, Options( invalidUtf: true, anyCrLf: anyCrLf ) );
			foreach ( var hex in new[] { "", "61C3A9", "FFC3A9FF61", "C3FF61", "FFFF", "0D0A61C3A90D0A" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, managed.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( "(?a)*", 4 )]
	[InlineData( "a(?aW){2}", 6 )]
	[InlineData( "(?x)(?aS) #x\n +", 14 )]
	public void BareSettingsRemainUnrepeatableAtOriginalQuantifierOffsets( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?aQ)a", 0 )]
	[InlineData( "(?AS)a", 0 )]
	[InlineData( "(?a S)a", 0 )]
	[InlineData( "(?aPW)a", 0 )]
	[InlineData( "(?aP-aS-aT)a", 0 )]
	[InlineData( "(?^aP-aT)a", 0 )]
	[InlineData( "é(?aW", 2 )]
	public void MalformedHeadersKeepOriginalByteOpeners( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void UnclosedScopeKeepsItsUtfByteOpener() {
		const string pattern = @"é(?aW:\w";
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( 2, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void RestrictionMasksRetainDepthCaptureAndCompileCancellationLimits() {
		var nested = string.Concat( Enumerable.Repeat( "(?a:", 256 ) ) + @"\w" + new string( ')', 256 );
		Assert.Equal( 1, Compile( nested ).Match( "a"u8.ToArray() ).Match?.ByteLength );
		var tooDeep = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( "(?a:" + nested + ")" ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, tooDeep.Diagnostic?.Code );
		Assert.Equal( 1024, tooDeep.Diagnostic?.ByteOffset );
		var captures = "(?a)" + string.Concat( Enumerable.Repeat( "()", 256 ) );
		Assert.Equal( 257, Compile( captures ).Match( Array.Empty<byte>() ).Match!.Captures.Count );
		Assert.Equal( PcreByteDiagnosticCode.PatternCaptureLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( captures + "()" ), Options() ).Diagnostic?.Code );
		var settings = string.Concat( Enumerable.Repeat( "(?a)(?-a)", 1000 ) );
		Assert.Single( Compile( settings + @"\w" ).Match( "é"u8.ToArray() ).Match!.Captures );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( settings ), Options(), cancellation.Token ) );
	}

	[Fact]
	public void RestrictedPredicatesKeepWorkAndMatchCancellationBudgets() {
		const string pattern = @"^(?aW:(\w|\w\w)*)[b]$";
		var subject = Enumerable.Repeat( (byte)'a', 40 ).ToArray();
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, Options() with { MatchWorkLimit = 500 } ).Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, Options() with {
			WorkObserver = count => { if ( count == 20 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}

	private static void Check( string pattern, string text, int index, int length, int groups ) {
		var subject = Encoding.UTF8.GetBytes( text );
		var reference = Reference( pattern );
		AssertExpected( reference, subject, index, length, groups );
		Compare( reference, subject, 0, Compile( pattern ).Match( subject ), pattern );
	}

	private static void AssertExpected( PcreRegex8Bit reference, byte[] subject, int index, int length, int groups ) {
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
		bool asciiBsD = false, bool invalidUtf = false, bool anyCrLf = false ) => new() {
		Utf8 = utf8, Ucp = ucp, AsciiBsD = asciiBsD,
		MatchInvalidUtf = invalidUtf, AnyCrLfNewline = anyCrLf
	};

	private static PcreRegex8Bit Reference( string pattern, bool utf8 = true, bool ucp = true,
		bool asciiBsD = false, bool invalidUtf = false, bool anyCrLf = false ) {
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
