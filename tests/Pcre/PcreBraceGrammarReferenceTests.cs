using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Ordinary brace lexing against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreBraceGrammarReferenceTests {
	[Theory]
	[InlineData( "{", "{", 0, 1, 1 )]
	[InlineData( "}", "}", 0, 1, 1 )]
	[InlineData( "a{", "a{", 0, 2, 1 )]
	[InlineData( "a{2,3", "a{2,3", 0, 5, 1 )]
	[InlineData( "a{}", "a{}", 0, 3, 1 )]
	[InlineData( "a{,}", "a{,}", 0, 4, 1 )]
	[InlineData( "a{foo}", "a{foo}", 0, 6, 1 )]
	[InlineData( "a{2x}", "a{2x}", 0, 5, 1 )]
	[InlineData( "a{1 2}", "a{1 2}", 0, 6, 1 )]
	[InlineData( "a{65536x}", "a{65536x}", 0, 9, 1 )]
	[InlineData( "a{999999999999999999999,", "a{999999999999999999999,", 0, 24, 1 )]
	[InlineData( "^{٢}$", "{٢}", 0, 4, 1 )]
	[InlineData( "^a{0}$", "", 0, 0, 1 )]
	[InlineData( "^a{2}$", "aa", 0, 2, 1 )]
	[InlineData( "^a{1,2}$", "aa", 0, 2, 1 )]
	[InlineData( "^a{2,}$", "aaa", 0, 3, 1 )]
	[InlineData( "^a{,2}$", "", 0, 0, 1 )]
	[InlineData( "^a{  \t2\t }$", "aa", 0, 2, 1 )]
	[InlineData( "^a{ \t1 \t, \t2\t }$", "aa", 0, 2, 1 )]
	[InlineData( "^a{ \t2\t ,\t }$", "aaa", 0, 3, 1 )]
	[InlineData( "^a{ \t, \t2\t }$", "aa", 0, 2, 1 )]
	[InlineData( "^a{1,\n2}$", "a{1,\n2}", 0, 7, 1 )]
	[InlineData( "^a{1,\v2}$", "a{1,\v2}", 0, 7, 1 )]
	[InlineData( "^a{1,\u00a02}$", "a{1,\u00a02}", 0, 8, 1 )]
	[InlineData( "(?x)^a{1,\n2}$", "a{1,2}", 0, 6, 1 )]
	[InlineData( "(?xx)^a{1 # comment\n,2}$", "a{1,2}", 0, 6, 1 )]
	[InlineData( "(?x)^a{2(?#c)}$", "a{2}", 0, 4, 1 )]
	[InlineData( @"(?x)^a{2\Qx\E}$", "a{2x}", 0, 5, 1 )]
	[InlineData( "^a{(?i)b}$", "a{B}", 0, 4, 1 )]
	[InlineData( "^({(a)})$", "{a}", 0, 3, 3 )]
	[InlineData( "^a{2*}$", "a{222}", 0, 6, 1 )]
	[InlineData( "^{{2}$", "{{", 0, 2, 1 )]
	[InlineData( "^}{2}$", "}}", 0, 2, 1 )]
	[InlineData( "^a{foo}{2}$", "a{foo}}", 0, 7, 1 )]
	[InlineData( "^a+{x}$", "aaa{x}", 0, 6, 1 )]
	[InlineData( "^a{2}{x}$", "aa{x}", 0, 5, 1 )]
	[InlineData( "^(?i){x}$", "{X}", 0, 3, 1 )]
	[InlineData( @"^\b{x}$", "{x}", -1, 0, 0 )]
	[InlineData( @"^(?(?=\{){x}|a)$", "{x}", 0, 3, 1 )]
	[InlineData( "^a(?(?<=a){x}|b)$", "a{x}", 0, 4, 1 )]
	[InlineData( @"^\Q{2}\E+$", "{2}}", 0, 4, 1 )]
	[InlineData( @"^\{2\}$", "{2}", 0, 3, 1 )]
	[InlineData( "^[{}]{2}$", "{}", 0, 2, 1 )]
	[InlineData( @"^\x{7b}{2}\o{175}$", "{{}", 0, 3, 1 )]
	[InlineData( @"^\N{2}$", "ab", 0, 2, 1 )]
	[InlineData( @"^\N{U+1F600}{2}$", "😀😀", 0, 8, 1 )]
	[InlineData( @"^(?<x>\{)\k{x}\g{x}$", "{{{", 0, 3, 2 )]
	[InlineData( "^(?:(a){2}?)(a*)$", "aaaa", 0, 4, 3 )]
	[InlineData( "(?U)^(a{1,3})(a*)$", "aaa", 0, 3, 3 )]
	[InlineData( "^a{1,2}+a$", "aa", -1, 0, 0 )]
	[InlineData( "é({😀})(?=})}", "xé{😀}}", 1, 9, 2 )]
	[InlineData( "(?<=({é😀}))x", "{é😀}x", 8, 1, 2 )]
	[InlineData( "(?<=({a?|}}))x", "{ax", 2, 1, 2 )]
	[InlineData( "(?<=a{2x})b$", "a{2x}b", 5, 1, 1 )]
	[InlineData( @"^(?(?=({x}))\1|Y)$", "{x}", 0, 3, 2 )]
	public void LiteralFallbackAndRealRepeatsKeepEveryNumberedByteSpan( string pattern, string text,
		int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( "a{65536}", 1 )]
	[InlineData( "a{0,65536}", 1 )]
	[InlineData( "a{65536,}", 1 )]
	[InlineData( "a{,65536}", 1 )]
	[InlineData( "a{2,1}", 1 )]
	[InlineData( "(a{65536}){0}", 2 )]
	[InlineData( "{2}", 0 )]
	[InlineData( "é^{1}", 3 )]
	[InlineData( @"é\b{1}", 4 )]
	[InlineData( "é(?i){1}", 6 )]
	[InlineData( "éa+{1}", 4 )]
	[InlineData( "éa{2}{1}", 6 )]
	[InlineData( "é(?(?=a){1}a|b)", 9 )]
	[InlineData( @"é\N{x}", 2 )]
	[InlineData( @"é\N{2x}", 2 )]
	[InlineData( @"é\N{}", 2 )]
	[InlineData( @"é\x{zz}", 2 )]
	[InlineData( @"é\o{8}", 2 )]
	[InlineData( @"é\N{U+110000}", 2 )]
	[InlineData( @"é(?<x{>a)", 2 )]
	[InlineData( @"é(?<x>a)\k{x{z}", 9 )]
	public void CompleteInvalidCountsAndExplicitBracedTokensNeverFallBack( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, offset );
	}

	[Fact]
	public void MaximumLegalCountsAndLeadingZerosValidateBeforeErasureOrExpansion() {
		Check( "^(?:a{65535}){0}$", "", 0, 0, 1 );
		Check( "^(?:a{0,65535}){0}$", "", 0, 0, 1 );
		Check( "^a{" + new string( '0', 512 ) + "2}$", "aa", 0, 2, 1 );
		Diagnostic( "a{65535}", PcreByteDiagnosticCode.PatternRepeatLimitExceeded, 1 );
		Diagnostic( "}{65535}", PcreByteDiagnosticCode.PatternRepeatLimitExceeded, 1 );
		var literal = "a{" + new string( '9', 512 ) + "x}";
		Check( literal, literal, 0, literal.Length, 1 );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void InvalidUtfCrLfAndEveryCallerOffsetRetainCapturesAndBarriers( bool anyCrLf ) {
		foreach ( var pattern in new[] { "({x})", "(}?)", "(?<=(é{2x}))}", @"(?m)^({?)(.)$",
			@"(?(?=({))\1|})", @"\G({?)(\N{,2})" } ) {
			var reference = Reference( pattern, invalidUtf: true, anyCrLf: anyCrLf );
			var compiled = Compile( pattern, Options( invalidUtf: true, anyCrLf: anyCrLf ) );
			foreach ( var hex in new[] { "", "7B787D", "FF7B787DFF", "C3A97B32787D7D", "C37BFF7D", "0D0A7B787D0D0A" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Fact]
	public void LiteralBracesShareWorkExpansionAndCancellationLimits() {
		const string pattern = "^({|{{)*z$";
		var subject = Encoding.ASCII.GetBytes( new string( '{', 16 ) );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, Options() with { MatchWorkLimit = 400 } ).Match( subject ).Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( pattern, Options() with { WorkObserver = work => { if ( work == 400 ) { cancelled.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancelled.Token ) );
		foreach ( var suffix in new[] { "}", "x}" } ) {
			var bytes = Encoding.ASCII.GetBytes( "a{" + new string( '0', 10000 ) + "2" + suffix );
			Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( bytes, Options(), cancelled.Token ) );
		}
	}

	private static void Diagnostic( string pattern, PcreByteDiagnosticCode code, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void Check( string pattern, string text, int index, int length, int groups ) {
		foreach ( var utf8 in pattern.All( c => c < 128 ) && text.All( c => c < 128 ) ? new[] { false, true } : new[] { true } ) {
			var subject = Encoding.UTF8.GetBytes( text );
			var reference = Reference( pattern, utf8: utf8 );
			var native = reference.Match( subject );
			Assert.Equal( index, native.Success ? native.Index : -1 );
			if ( native.Success ) { Assert.Equal( length, native.Length ); Assert.Equal( groups, native.Groups.Count ); }
			Compare( reference, subject, 0, Compile( pattern, Options( utf8: utf8 ) ).Match( subject ), pattern );
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

	private static PcreRegex8Bit Reference( string pattern, bool invalidUtf = false, bool anyCrLf = false, bool utf8 = true ) => new(
		Encoding.UTF8.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), Encoding.UTF8,
		new PcreRegexSettings { Options = PcreOptions.Compiled | ( utf8 ? PcreOptions.Utf : PcreOptions.None )
			| ( invalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ) } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options ?? Options() );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
