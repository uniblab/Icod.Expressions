using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Empty option settings and selective resets against pinned PCRE2/JIT.</summary>
public sealed class PcreResetOptionReferenceTests {
	[Theory]
	[InlineData( "(?)", "", 0, 0, 1 )]
	[InlineData( "(?-)", "", 0, 0, 1 )]
	[InlineData( "(?i-)", "", 0, 0, 1 )]
	[InlineData( "(?)a", "a", 0, 1, 1 )]
	[InlineData( "a(?-)(b)", "ab", 0, 2, 2 )]
	[InlineData( "(?i-)a", "A", 0, 1, 1 )]
	[InlineData( "(?-:a)(b)", "ab", 0, 2, 2 )]
	[InlineData( "(?i-:a)b", "Ab", 0, 2, 1 )]
	[InlineData( "(?i-:a)b", "AB", -1, 0, 0 )]
	[InlineData( "(?inUx)(?)(?-)(?i-)(a +)", "AAA", 0, 1, 1 )]
	[InlineData( "(?^)", "", 0, 0, 1 )]
	[InlineData( "(?^:){2}", "", 0, 0, 1 )]
	[InlineData( "(?i)a(?^)b", "Ab", 0, 2, 1 )]
	[InlineData( "(?i)a(?^)b", "AB", -1, 0, 0 )]
	[InlineData( "(?i)(?^i)a", "A", 0, 1, 1 )]
	[InlineData( "(?m)(?^)^a$", "x\na\nz", -1, 0, 0 )]
	[InlineData( "(?m)(?^m)^a$", "x\na\nz", 2, 1, 1 )]
	[InlineData( "(?s)(?^).", "\n", -1, 0, 0 )]
	[InlineData( "(?s)(?^s).", "\n", 0, 1, 1 )]
	[InlineData( @"(?n)(?^)(a)\1", "aa", 0, 2, 2 )]
	[InlineData( "(?n)(?^n)(a)", "a", 0, 1, 1 )]
	[InlineData( "(?U)(?^)a+", "aaa", 0, 1, 1 )]
	[InlineData( "(?^U)a+", "aaa", 0, 1, 1 )]
	[InlineData( "(?U)(?^U)a+?", "aaa", 0, 3, 1 )]
	[InlineData( "(?U)(?^:a+)(a*)", "aaa", 0, 1, 2 )]
	[InlineData( "(?U)(?^:a)+", "aaa", 0, 1, 1 )]
	[InlineData( "(?U)(?^:a)++", "aaa", 0, 3, 1 )]
	[InlineData( "(?i)(?^:a)b", "aB", 0, 2, 1 )]
	[InlineData( "(?i)(?^:a)b", "AB", -1, 0, 0 )]
	[InlineData( "(?i:a(?^)b|c)d", "cd", 0, 2, 1 )]
	[InlineData( "(?i:a(?^)b|c)d", "Cd", -1, 0, 0 )]
	[InlineData( "(?i)a(?^)b|c", "C", -1, 0, 0 )]
	[InlineData( @"(?n)(?^:(a))(?<b>b)\1\2", "abab", 0, 4, 3 )]
	[InlineData( "(?n)(?^:(a))(b)", "ab", 0, 2, 2 )]
	[InlineData( @"(?n)(?|(a)(?^)(b)|(c)(d))(?^)(e)\1\2\3", "cdecde", 0, 6, 4 )]
	[InlineData( @"(?i)(?=(?^:(a)))a\1", "aa", 0, 2, 2 )]
	[InlineData( @"(?i)(?=(?^:(a)))a\1", "AA", -1, 0, 0 )]
	[InlineData( "(?i)(?<=(?^:(a)))b", "aB", 1, 1, 2 )]
	[InlineData( "(?i)(?<=(?^:(a)))b", "AB", -1, 0, 0 )]
	[InlineData( @"(?i)(a)(?^:\1)", "AA", 0, 2, 2 )]
	[InlineData( @"(?i)(a)(?^:\1)", "Aa", -1, 0, 0 )]
	public void EmptyAndResetSettingsKeepExplicitOutcomesAndCaptureSlots( string pattern,
		string text, int expectedIndex, int expectedLength, int expectedGroups ) {
		foreach ( var utf8 in new[] { false, true } ) {
			var subject = Encoding.ASCII.GetBytes( text );
			var reference = Reference( pattern, utf8 );
			var native = reference.Match( subject );
			Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
			if ( native.Success ) {
				Assert.Equal( expectedLength, native.Length );
				Assert.Equal( expectedGroups, native.Groups.Count );
			}
			Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions { Utf8 = utf8 } ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( "(?x)(?^) a#b", " a#b", 0, 4 )]
	[InlineData( "(?x)(?^) *", "   ", 0, 3 )]
	[InlineData( "(?x)(?^x) a # comment\n b", "ab", 0, 2 )]
	[InlineData( "(?xx)(?^)[ a]", " ", 0, 1 )]
	[InlineData( "(?xx)(?^x)[ a]", " ", 0, 1 )]
	[InlineData( "(?xx)(?^xx)[ a]", " ", -1, 0 )]
	[InlineData( "(?x)(?^: a#b) c", " a#bc", 0, 5 )]
	[InlineData( "(?x)(?^)#not-a-comment", "#not-a-comment", 0, 14 )]
	[InlineData( @"(?x)\Q(?^)\E", "(?^)", 0, 4 )]
	[InlineData( @"(?x)[(?^)]+", "(?^)", 0, 4 )]
	[InlineData( "(?x)(?# (?^ hidden)a", "a", 0, 1 )]
	public void ResetChangesTriviaAtItsOwnHeaderAndKeepsQuotedTextLiteral( string pattern,
		string text, int expectedIndex, int expectedLength ) {
		var subject = Encoding.ASCII.GetBytes( text );
		var reference = Reference( pattern, false );
		var native = reference.Match( subject );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( expectedLength, native.Length ); }
		Compare( reference, subject, 0, Compile( pattern ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( "(?)a", "A", 0 )]
	[InlineData( "(?-)a", "A", 0 )]
	[InlineData( "(?^)a", "A", -1 )]
	[InlineData( "(?^i)a", "A", 0 )]
	[InlineData( "(?^:a)b", "aB", 0 )]
	[InlineData( "(?^:a)b", "AB", -1 )]
	public void ResetClearsCallerCaselessAndRestoresItAfterAScope( string pattern,
		string text, int expectedIndex ) {
		var subject = Encoding.ASCII.GetBytes( text );
		var reference = Reference( pattern, false, callerCaseless: true );
		var native = reference.Match( subject );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions { Caseless = true } ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( @"(?^)\d", true, -1 )]
	[InlineData( @"(?^)\d", false, 0 )]
	[InlineData( @"(?-aD)(?^)\d", true, 0 )]
	[InlineData( @"(?aD)(?^)\d", false, -1 )]
	[InlineData( @"(?^:\d)", true, -1 )]
	[InlineData( @"(?^:\d)", false, 0 )]
	public void ResetPreservesCallerAndLeadingAsciiRestrictions( string pattern,
		bool asciiBsD, int expectedIndex ) {
		var subject = "٣"u8.ToArray();
		var reference = Reference( pattern, true, ucp: true, asciiBsD: asciiBsD );
		var native = reference.Match( subject );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( 2, native.Length ); }
		Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions {
			Utf8 = true, Ucp = true, AsciiBsD = asciiBsD
		} ).Match( subject ), pattern );
	}

	[Fact]
	public void ResetKeepsUtfCaptureByteSpansAndLocalReferenceCasing() {
		foreach ( var row in new[] {
			( Pattern: @"(?i)(é)(?^:\1)", Text: "ÉÉ", Length: 4 ),
			( Pattern: @"(?i)(é)(?^:\1)", Text: "Éé", Length: -1 ),
			( Pattern: @"(?i)(K)(?^i:\1)", Text: "KK", Length: 4 )
		} ) {
			var subject = Encoding.UTF8.GetBytes( row.Text );
			var reference = Reference( row.Pattern, true );
			var native = reference.Match( subject );
			Assert.Equal( row.Length, native.Success ? native.Length : -1 );
			Compare( reference, subject, 0, Compile( row.Pattern, new PcreByteOptions { Utf8 = true } ).Match( subject ), row.Pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void SelectedMalformedUtfAndCrLfCallerOffsetsKeepFullCaptureParity( bool anyCrLf ) {
		foreach ( var pattern in new[] {
			@"(?i)(?^:(é?))(?i:\1)", "(?ms)(?^m:^a$)",
			"(?s)(?^:.)", "(?i)(?<=(?^:(a)))b", "(?x)(?^) a"
		} ) {
			var reference = Reference( pattern, true, invalidUtf: true, anyCrLf: anyCrLf );
			var managed = Compile( pattern, new PcreByteOptions { MatchInvalidUtf = true, AnyCrLfNewline = anyCrLf } );
			foreach ( var hex in new[] { "", "6141", "61FF41", "C3A9C389", "FFC3A9FF", "0D0A610D0A", "C3FF62" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, managed.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( "(?^-i)a", 0 )]
	[InlineData( "(?^i-:a)", 0 )]
	[InlineData( "(?i^:a)", 0 )]
	[InlineData( "(?--:a)", 0 )]
	[InlineData( "(?-i-:a)", 0 )]
	[InlineData( "(?^x a)", 0 )]
	[InlineData( "(?x)(?^ x:a)", 4 )]
	[InlineData( "(?^", 0 )]
	[InlineData( "é(?^-i)a", 2 )]
	public void MalformedHeadersRemainUnsupportedAtOriginalByteOpeners( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, true ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?)*", 3 )]
	[InlineData( "(?-)+", 4 )]
	[InlineData( "a(?i-)?", 6 )]
	[InlineData( "(?^){2}", 4 )]
	[InlineData( "(?^x) # ignored\n +", 17 )]
	public void BareSettingsCannotBeRepeated( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, false ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void UnclosedResetScopeRetainsItsOriginalByteOpener() {
		const string pattern = "é(?^:a";
		Assert.Throws<PcrePatternException>( () => Reference( pattern, true ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( 2, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?^J)a" )]
	public void NewOptionFamiliesStayExplicitlyPending( string pattern ) {
		Reference( pattern, true );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void ScopedSettingsKeepDepthAndRestoredCaptureLimits() {
		var nested = string.Concat( Enumerable.Repeat( "(?^:", 256 ) ) + "a" + new string( ')', 256 );
		Assert.Equal( 1, Compile( nested ).Match( "a"u8.ToArray() ).Match?.ByteLength );
		var tooDeep = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( "(?^:" + nested + ")" ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, tooDeep.Diagnostic?.Code );
		Assert.Equal( 1024, tooDeep.Diagnostic?.ByteOffset );
		var captures = "(?n)(?^)" + string.Concat( Enumerable.Repeat( "()", 256 ) );
		Assert.Equal( 257, Compile( captures ).Match( Array.Empty<byte>() ).Match!.Captures.Count );
		var tooMany = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( captures + "()" ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternCaptureLimitExceeded, tooMany.Diagnostic?.Code );
		Assert.Equal( captures.Length, tooMany.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void BareSettingsDoNotNestOrAllocateCapturesAndCancellationStillInterrupts() {
		var settings = string.Concat( Enumerable.Repeat( "(?)(?-)(?^)", 1000 ) );
		Assert.Single( Compile( settings + "a" ).Match( "a"u8.ToArray() ).Match!.Captures );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( settings ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void ResetsRetainMatchWorkAndCancellationBudgets() {
		const string pattern = "(?nU)(?^)^(a|aa)*[b]$";
		var subject = Enumerable.Repeat( (byte)'a', 40 ).ToArray();
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, new PcreByteOptions { MatchWorkLimit = 500 } ).Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new PcreByteOptions {
			WorkObserver = count => { if ( count == 20 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
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

	private static PcreRegex8Bit Reference( string pattern, bool utf8, bool callerCaseless = false,
		bool invalidUtf = false, bool anyCrLf = false, bool ucp = false, bool asciiBsD = false ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		if ( callerCaseless ) { options |= PcreOptions.Caseless; }
		if ( ucp ) { options |= PcreOptions.Ucp; }
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings {
				Options = options,
				ExtraCompileOptions = asciiBsD ? PcreExtraCompileOptions.AsciiBsD : PcreExtraCompileOptions.None
			} );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var encoding = options?.Utf8 == true || options?.MatchInvalidUtf == true ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
