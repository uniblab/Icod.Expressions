using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Source-local caseless restrictions against pinned PCRE2/JIT.</summary>
public sealed class PcreCaselessRestrictionReferenceTests {
	[Theory]
	[InlineData( "(?ir)k", "K", -1, 0, 0 )]
	[InlineData( "(?ir)K", "k", -1, 0, 0 )]
	[InlineData( "(?ir)s", "ſ", -1, 0, 0 )]
	[InlineData( "(?ir)ſ", "S", -1, 0, 0 )]
	[InlineData( "(?ir)k", "K", 0, 1, 1 )]
	[InlineData( "(?ir)K", "K", 0, 3, 1 )]
	[InlineData( "(?ir)é", "É", 0, 2, 1 )]
	[InlineData( "(?ir)Σ", "ς", 0, 2, 1 )]
	[InlineData( "(?ir)ǅ", "ǆ", 0, 2, 1 )]
	[InlineData( "(?ir)I", "ı", -1, 0, 0 )]
	[InlineData( "(?ir)İ", "i", -1, 0, 0 )]
	[InlineData( "(?r)(?i)k", "K", -1, 0, 0 )]
	[InlineData( "(?ir-r)k", "K", 0, 3, 1 )]
	[InlineData( "(?irr-r)k", "K", 0, 3, 1 )]
	[InlineData( "(?ir)(?^i)k", "K", 0, 3, 1 )]
	[InlineData( "(?i)(?^r)k", "K", -1, 0, 0 )]
	[InlineData( "(?i)(?^ir)k", "K", -1, 0, 0 )]
	[InlineData( "(?ir:k)k", "Kk", 0, 2, 1 )]
	[InlineData( "(?i)(?r:k)k", "kK", 0, 4, 1 )]
	[InlineData( "(?ir)(?-r:k)k", "Kk", 0, 4, 1 )]
	[InlineData( "(?ir)(?-r:k)k", "KK", -1, 0, 0 )]
	[InlineData( "(?i)k(?r)k", "Kk", 0, 4, 1 )]
	[InlineData( "(?i)k(?r)k", "kK", -1, 0, 0 )]
	[InlineData( "(?i)(x(?r)k|k)k", "kK", 0, 4, 2 )]
	[InlineData( "(?i)(x(?r)k|k)", "K", -1, 0, 0 )]
	[InlineData( "(?irU)(?^i)k+", "KK", 0, 3, 1 )]
	[InlineData( "(?ir)(?^iaW)\\w", "K", -1, 0, 0 )]
	public void LocalModesKeepExplicitOutcomesAndCaptureSlots( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( "(?ir)[k]", "K", -1, 0, 0 )]
	[InlineData( "(?ir)[K]", "K", -1, 0, 0 )]
	[InlineData( "(?ir)[K]", "K", 0, 3, 1 )]
	[InlineData( "(?ir)[a-z]", "ſ", -1, 0, 0 )]
	[InlineData( "(?ir)[ſ]", "s", -1, 0, 0 )]
	[InlineData( "(?ir)[Σ]", "ς", 0, 2, 1 )]
	[InlineData( "(?ir)[^k]", "K", 0, 3, 1 )]
	[InlineData( "(?ir)[^K]", "K", 0, 1, 1 )]
	[InlineData( "(?ir)[kK]", "K", 0, 3, 1 )]
	[InlineData( "(?ir)[kK]", "K", 0, 1, 1 )]
	[InlineData( "(?ir)[\\p{Ll}k]", "K", 0, 3, 1 )]
	[InlineData( "(?ir)[\\w]", "K", 0, 3, 1 )]
	[InlineData( "(?ir)\\Qk\\E", "K", -1, 0, 0 )]
	[InlineData( "(?ir)[\\QK\\E]", "k", -1, 0, 0 )]
	public void BracketClosureAndRuntimeKeysUseTheSameRestrictedPolicy( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( @"(?ir)(k)\1", "kK", -1, 0, 0 )]
	[InlineData( @"(?ir)(K)\1", "Kk", -1, 0, 0 )]
	[InlineData( @"(?ir)(k)(?-r:\1)", "kK", 0, 4, 2 )]
	[InlineData( @"(?i)(k)(?r:\1)", "kK", -1, 0, 0 )]
	[InlineData( @"(?ir)(?<x>k)\k<x>", "kK", -1, 0, 0 )]
	[InlineData( @"(?ir)(?<x>k)(?-r:\k<x>)", "kK", 0, 4, 2 )]
	[InlineData( @"(?ir)(?<x>k)(?-r:(?P=x))", "kK", 0, 4, 2 )]
	[InlineData( @"(?ir)(?:\k<x>|(?<x>k)){2}", "kk", 0, 2, 2 )]
	[InlineData( @"(?ir)(?:\k<x>|(?<x>k)){2}", "kK", -1, 0, 0 )]
	[InlineData( @"(?ir)(?<x>k)(?<=\k<x>)(?-r:\k<x>)", "kK", 0, 4, 2 )]
	[InlineData( @"(?i)(?<x>k).(?<=(?r:\k<x>))", "kK", -1, 0, 0 )]
	[InlineData( @"(?ir)(?<x>k).(?<=(?-r:\k<x>))", "kK", 0, 4, 2 )]
	[InlineData( @"(?ir)(?:(k)x|(k))(?-r:\2)", "kK", 0, 4, 3 )]
	public void ReferenceFixupsAndRollbackRetainTheirOwnComparisonModes( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( "(?ir)(?=k).", "K", -1, 0, 0 )]
	[InlineData( "(?ir)(?!k).", "K", 0, 3, 1 )]
	[InlineData( "(?ir)(?<=k)a", "Ka", -1, 0, 0 )]
	[InlineData( "(?ir)(?<!k)a", "Ka", 3, 1, 1 )]
	[InlineData( "(?i)(?=(?r)k).", "K", -1, 0, 0 )]
	[InlineData( "(?ir)(?=(?-r)k).", "K", 0, 3, 1 )]
	[InlineData( "(?i)(?<=(?r)k)a", "Ka", -1, 0, 0 )]
	[InlineData( "(?ir)(?<=(?-r)k)a", "Ka", 3, 1, 1 )]
	[InlineData( "(?i)(?=(?r:(k)))k", "K", -1, 0, 0 )]
	[InlineData( "(?ir)(?=(?-r:(k)))k", "k", 0, 1, 2 )]
	[InlineData( "(?ir)(?<=(?-r:(k)))a", "Ka", 3, 1, 2 )]
	[InlineData( "(?ir)(?<=k{1,2})a", "Ka", -1, 0, 0 )]
	[InlineData( "(?i)(?<=(?r:K|k))a", "Ka", 3, 1, 1 )]
	[InlineData( "(?r)(?<=(?r)k)a", "ka", 1, 1, 1 )]
	public void LiteralAndGeneralAssertionsKeepEntryAndBodyModesDistinct( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( "(?i)[[:lower:]]", "K", false )]
	[InlineData( "(?i)[[:lower:]]", "K", false )]
	[InlineData( "(?i)[[:lower:]]", "é", true )]
	[InlineData( "(?i)[[:upper:]]", "ǆ", false )]
	[InlineData( "(?i)[[:upper:]]", "Σ", true )]
	[InlineData( "(?i)[[:^upper:]]", "ǆ", true )]
	[InlineData( "(?i)[^[:^upper:]]", "a", false )]
	[InlineData( "(?ir)[[:lower:]]", "K", false )]
	[InlineData( "(?ir)\\p{Ll}", "K", true )]
	[InlineData( "(?ir)[\\p{Ll}]", "K", true )]
	[InlineData( "(?ir)[\\p{Lu}]", "ǆ", true )]
	[InlineData( "(?ir)[\\p{Lt}]", "a", true )]
	[InlineData( "(?iraP)[[:lower:]]", "A", true )]
	public void UnicodePosixCaseCategoriesRemainDistinctFromCaselessProperties( string pattern,
		string text, bool expected ) => Check( pattern, text, expected ? 0 : -1,
			expected ? Encoding.UTF8.GetByteCount( text ) : 0, expected ? 1 : 0 );

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void UtfModeNeedsNoUcpAndCallerCaselessIsRestrictedLocally( bool ucp ) {
		foreach ( var row in new[] {
			( Pattern: "(?r)k", Index: -1, Length: 0 ),
			( Pattern: "(?r)(?-r:k)", Index: 0, Length: 3 ),
			( Pattern: "(?r)é", Index: 3, Length: 2 )
		} ) {
			var subject = "KÉ"u8.ToArray();
			var options = Options( ucp: ucp, callerCaseless: true );
			var reference = Reference( row.Pattern, ucp: ucp, callerCaseless: true );
			AssertExpected( reference, subject, row.Index, row.Length, 1 );
			Compare( reference, subject, 0, Compile( row.Pattern, options ).Match( subject ), row.Pattern );
		}
	}

	[Fact]
	public void ByteModeKeepsItsExistingAsciiCaseFolding() {
		const string pattern = @"(?ir)(k)[a-z]\1";
		var subject = "KsK"u8.ToArray();
		var options = Options( utf8: false, ucp: false );
		var reference = Reference( pattern, utf8: false, ucp: false );
		AssertExpected( reference, subject, 0, 3, 2 );
		Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		foreach ( var row in new[] {
			( Pattern: "(?i)(?=(?r)k)k", Index: 0 ),
			( Pattern: "(?ir)(?<=(?-r)k)a", Index: 1 )
		} ) {
			var assertionSubject = "ka"u8.ToArray();
			var assertion = Reference( row.Pattern, utf8: false, ucp: false );
			AssertExpected( assertion, assertionSubject, row.Index, 1, 1 );
			Compare( assertion, assertionSubject, 0, Compile( row.Pattern, options ).Match( assertionSubject ), row.Pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void SelectedMalformedUtfAndCrLfOffsetsKeepCaptureParity( bool anyCrLf ) {
		foreach ( var pattern in new[] {
			@"(?ir)(k?)(?-r:\1)", @"(?ir)[^k]|(?-r:(k))",
			@"(?i)(?<=(?r)(k))a", @"(?ir)(?=(?-r:(k))).",
			"(?mir)^(k*)$"
		} ) {
			var options = Options( invalidUtf: true, anyCrLf: anyCrLf );
			var reference = Reference( pattern, invalidUtf: true, anyCrLf: anyCrLf );
			var managed = Compile( pattern, options );
			foreach ( var hex in new[] { "", "6BE284AA", "FFE284AAFF6B", "E2FF6B", "FFFF", "0D0A6BE284AA0D0A" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, managed.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( "(?r)*", 4 )]
	[InlineData( "a(?ir){2}", 6 )]
	[InlineData( "(?xr) #x\n +", 10 )]
	public void BareSettingsRemainUnrepeatableAtTheirOriginalOffsets( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?Rr)a", 0 )]
	[InlineData( "(?r r)a", 0 )]
	[InlineData( "(?r-r-r)a", 0 )]
	[InlineData( "(?^r-r)a", 0 )]
	[InlineData( "é(?r", 2 )]
	public void MalformedHeadersKeepOriginalByteOpeners( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void WideRangesAndResourceBoundariesRetainExistingPolicies() {
		const string full = "(?ir)[\\x{0}-\\x{10FFFF}]";
		Check( full, "K", 0, 3, 1 );
		Check( "(?ir)[\\x{0}-\\x{7f}]", "K", -1, 0, 0 );
		Check( "(?ir)[^\\x{0}-\\x{7f}]", "K", 0, 3, 1 );
		var nested = string.Concat( Enumerable.Repeat( "(?r:", 256 ) ) + "k" + new string( ')', 256 );
		Assert.Equal( 1, Compile( nested ).Match( "k"u8.ToArray() ).Match?.ByteLength );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( "(?r:" + nested + ")" ), Options() ).Diagnostic?.Code );
		var captures = "(?r)" + string.Concat( Enumerable.Repeat( "()", 256 ) );
		Assert.Equal( 257, Compile( captures ).Match( Array.Empty<byte>() ).Match!.Captures.Count );
		Assert.Equal( PcreByteDiagnosticCode.PatternCaptureLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( captures + "()" ), Options() ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( full ), Options(), cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( "(?ir)[A-\\x{10FFFF}]" ), Options(), cancellation.Token ) );
	}

	[Fact]
	public void WorkAndMatchCancellationRemainBounded() {
		const string pattern = @"(?ir)^(k|kk)*[b]$";
		var subject = Enumerable.Repeat( (byte)'k', 40 ).ToArray();
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

	private static PcreByteOptions Options( bool utf8 = true, bool ucp = true,
		bool callerCaseless = false, bool invalidUtf = false, bool anyCrLf = false ) => new() {
		Utf8 = utf8, Ucp = ucp, Caseless = callerCaseless, MatchInvalidUtf = invalidUtf, AnyCrLfNewline = anyCrLf
	};

	private static PcreRegex8Bit Reference( string pattern, bool utf8 = true, bool ucp = true,
		bool callerCaseless = false, bool invalidUtf = false, bool anyCrLf = false ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( ucp ) { options |= PcreOptions.Ucp; }
		if ( callerCaseless ) { options |= PcreOptions.Caseless; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = options } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		options ??= Options();
		var encoding = options.Utf8 || options.MatchInvalidUtf ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
