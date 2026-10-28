using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Extended lexical scopes and comments against the pinned native oracle.</summary>
public sealed class PcreExtendedOptionReferenceTests {
	[Theory]
	[InlineData( @"(?x)(a) (?<b>b) \1 \k<b>", "abab", 0, 4 )]
	[InlineData( "a(?x) b", "ab", 0, 2 )]
	[InlineData( "(?x:a b) c", "ab c", 0, 4 )]
	[InlineData( "(?x:a b)c", "abc", 0, 3 )]
	[InlineData( "(?x)a (?-x) b", "a b", 0, 3 )]
	[InlineData( "(?x:a (?-x: b) c)", "a bc", 0, 4 )]
	[InlineData( "(?xx)[ a](?x)[ b]", "a ", 0, 2 )]
	[InlineData( "(?xx)(?i)[ a]", " ", -1, 0 )]
	[InlineData( "(?xx)(?xix)[ a]", " ", 0, 1 )]
	[InlineData( "(?xix)[ a]", " ", 0, 1 )]
	[InlineData( "(?xxx)[ a]", " ", -1, 0 )]
	[InlineData( "(?xx)(?-x)[ a]", " ", 0, 1 )]
	[InlineData( "(?xx)(?-xx)[ a]", " ", 0, 1 )]
	[InlineData( "(?xx-x)[ a]", " ", 0, 1 )]
	[InlineData( "(?x:a b|c d)", "cd", 0, 2 )]
	[InlineData( "(a(?x) b|c d)e", "cde", 0, 3 )]
	[InlineData( "(a(?x) b)|c d", "c d", 0, 3 )]
	[InlineData( "a(?x) b|c d", "cd", 0, 2 )]
	[InlineData( @"(?xn)(a) (?<b>b) \1", "abb", 0, 3 )]
	[InlineData( "(?xU)(a +) (a *)", "aaa", 0, 1 )]
	[InlineData( "(?xU)(a + ?) (a *)", "aaa", 0, 3 )]
	[InlineData( "(?xims:^ (a . b) $)", "x\nA\nB\nz", 2, 3 )]
	[InlineData( "(?x:(a))+ (b)", "aa b", 0, 4 )]
	[InlineData( "(?x:(a)) + (b)", "a  b", 0, 4 )]
	public void ExtendedOptionsKeepScopeAndSelectedSpans( string pattern, string text,
		int expectedIndex, int expectedLength ) {
		var subject = Encoding.ASCII.GetBytes( text );
		var reference = Reference( pattern, false );
		var native = reference.Match( subject );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( expectedLength, native.Length ); }
		Compare( reference, subject, 0, Compile( pattern ).Match( subject ), pattern );
	}

	[Fact]
	public void CapturesKeepOriginalSubjectByteSpansAcrossIgnoredPatternText() {
		const string pattern = "(?x)(é) # no capture: (?<ignored>z)\n (?<tail>b +) \\1";
		var subject = "!ébbé"u8.ToArray();
		var reference = Reference( pattern, true );
		var native = reference.Match( subject );
		Assert.True( native.Success );
		Assert.Equal( 1, native.Index );
		Assert.Equal( 6, native.Length );
		Assert.Equal( 3, native.Groups.Count );
		Assert.Equal( 1, native.Groups[1].Index );
		Assert.Equal( 2, native.Groups[1].Length );
		Assert.Equal( 3, native.Groups[2].Index );
		Assert.Equal( 2, native.Groups[2].Length );
		var actual = Compile( pattern, new PcreByteOptions { Utf8 = true } ).Match( subject );
		Compare( reference, subject, 0, actual );
		Assert.Equal( new PcreByteCapture( 1, 2 ), actual.Match!.Captures[1] );
		Assert.Equal( new PcreByteCapture( 3, 2 ), actual.Match.Captures[2] );
	}

	[Fact]
	public void LineCommentsUseTheConfiguredNewlineConvention() {
		foreach ( var anyCrLf in new[] { false, true } ) {
			foreach ( var row in new[] {
				( Pattern: "(?x)(a)# ignored\rb", Length: anyCrLf ? 2 : 1 ),
				( Pattern: "(?x)(a)# ignored\r\n(b)", Length: 2 ),
				( Pattern: "(?x)(a)# ignored\n(b)", Length: 2 ),
				( Pattern: @"(?x)(a)# ignored\n(b)", Length: 1 ),
				( Pattern: "(?xs)(a)# ignored\u0085(b)", Length: 1 ),
				( Pattern: "(?x)(a)# ignored\u2028(b)", Length: 1 ),
				( Pattern: "(?x)(a)# ignored\u2029(b)", Length: 1 )
			} ) {
				var subject = "ab"u8.ToArray();
				var reference = Reference( row.Pattern, true, anyCrLf: anyCrLf );
				Assert.Equal( row.Length, reference.Match( subject ).Length );
				foreach ( var bsrAnyCrLf in new[] { false, true } ) {
					Compare( reference, subject, 0, Compile( row.Pattern, new PcreByteOptions {
						Utf8 = true, AnyCrLfNewline = anyCrLf, BsrAnyCrLf = bsrAnyCrLf
					} ).Match( subject ), row.Pattern );
				}
			}
		}
	}

	[Fact]
	public void ParenthesizedCommentsCreateNeitherAtomsNorCaptures() {
		foreach ( var row in new[] {
			( Pattern: "(?# standalone)a", Subject: "a" ),
			( Pattern: "a(?# between)b", Subject: "ab" ),
			( Pattern: @"(?#\)a", Subject: "a" ),
			( Pattern: "(?#(a)b", Subject: "b" ),
			( Pattern: "(?#(?<ignored>a)(b)", Subject: "b" ),
			( Pattern: "(?x)(a)(?# # [] \\ body) (b)", Subject: "ab" ),
			( Pattern: "a(?# comment)+", Subject: "aaa" ),
			( Pattern: "a+(?# comment)?", Subject: "aaa" ),
			( Pattern: "a+(?# comment)+a", Subject: "aaa" ),
			( Pattern: "(?# prefix)(a)(?# middle)\\1(?# end)", Subject: "aa" )
		} ) {
			var subject = Encoding.ASCII.GetBytes( row.Subject );
			Compare( Reference( row.Pattern, false ), subject, 0,
				Compile( row.Pattern ).Match( subject ), row.Pattern );
		}
	}

	[Fact]
	public void TriviaAndEmptyQuotesMaySeparateQuantifiersAndTheirSuffixes() {
		foreach ( var separator in new[] { " \t", "# repeat\n", "(?# repeat)", @"\Q\E" } ) {
			foreach ( var quantifier in new[] { "+", "{1,3}" } ) {
				foreach ( var suffix in new[] { "?", "+" } ) {
					var pattern = $"(?x)(a{separator}{quantifier}{separator}{suffix})(a*)";
					var subject = "aaaa"u8.ToArray();
					Compare( Reference( pattern, false ), subject, 0, Compile( pattern ).Match( subject ), pattern );
				}
			}
		}
		// Edge SP/HT in a counted bound is allowed independently of extended mode.
		foreach ( var pattern in new[] { "(a{ 1,3 })(a*)", "(?x)(a {\t1,3\t} ?)(a*)" } ) {
			var subject = "aaaa"u8.ToArray();
			Compare( Reference( pattern, false ), subject, 0, Compile( pattern ).Match( subject ), pattern );
		}
	}

	[Fact]
	public void EscapedAndQuotedWhitespaceAndHashesRemainLiteral() {
		foreach ( var row in new[] {
			( Pattern: @"(?x)a\ b\#c", Subject: "a b#c" ),
			( Pattern: "(?x)a\\\tb", Subject: "a\tb" ),
			( Pattern: @"(?x)\Q a#(?x) \E", Subject: " a#(?x) " ),
			( Pattern: @"(?x)\Q \E +", Subject: "   " ),
			( Pattern: @"(?xx)[\ ]+", Subject: "  " ),
			( Pattern: @"(?xx)[\Q #\E]+", Subject: " # " ),
			( Pattern: "(?x)[# ()]+", Subject: "# ()" ),
			( Pattern: @"(?x)\Q# no terminator", Subject: "# no terminator" )
		} ) {
			var subject = Encoding.ASCII.GetBytes( row.Subject );
			Compare( Reference( row.Pattern, false ), subject, 0, Compile( row.Pattern ).Match( subject ), row.Pattern );
		}
	}

	[Theory]
	[InlineData( "(?xx)[ a\tb]+", "ab", 0, 2 )]
	[InlineData( "(?xx)[ a\tb]+", " \t", -1, 0 )]
	[InlineData( "(?x)[ a\tb]+", " \t", 0, 2 )]
	[InlineData( "(?xx)[ a - z ]+", "azm", 0, 3 )]
	[InlineData( "(?xx)[a- ]+", "a--", 0, 3 )]
	[InlineData( "(?xx)[ - a]+", "--a", 0, 3 )]
	[InlineData( "(?xx)[ ^ a ]+", "b ", 0, 2 )]
	[InlineData( "(?xx)[^ a ]+", "b ", 0, 2 )]
	[InlineData( "(?xx)[ ] a ]+", "]a", 0, 2 )]
	[InlineData( "(?xx)[ ^ ] a ]+", "bc", 0, 2 )]
	[InlineData( "(?xx)[ [:alpha:] 0 ]+", "ab0", 0, 3 )]
	[InlineData( "(?xx)[#()]+", "#()", 0, 3 )]
	[InlineData( "(?xx)[a\nb]+", "\n", 0, 1 )]
	[InlineData( "(?xx)[a\vb]+", "\v", 0, 1 )]
	[InlineData( "(?xx)[é - ê]+", "éê", 0, 4 )]
	[InlineData( "(?xx)[ \u0085 ]", "\u0085", 0, 2 )]
	public void ExtendedClassesKeepRangesNegationAndFirstClosingBracket( string pattern,
		string text, int expectedIndex, int expectedLength ) {
		foreach ( var utf8 in new[] { false, true } ) {
			var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
			var subject = encoding.GetBytes( text );
			var reference = Reference( pattern, utf8 );
			var native = reference.Match( subject );
			Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
			if ( native.Success ) {
				Assert.Equal( utf8 ? expectedLength : text.Length, native.Length );
			}
			Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions { Utf8 = utf8 } ).Match( subject ), pattern );
		}
	}

	[Fact]
	public void ExtendedWhitespaceUsesPatternWhiteSpaceRatherThanUnicodeCategories() {
		foreach ( var scalar in new[] { 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x20, 0x85, 0x200e, 0x200f, 0x2028, 0x2029 } ) {
			var pattern = "(?x)(a)" + char.ConvertFromUtf32( scalar ) + "(b)";
			var subject = "ab"u8.ToArray();
			var reference = Reference( pattern, true );
			Assert.Equal( 2, reference.Match( subject ).Length );
			Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions { Utf8 = true } ).Match( subject ), $"U+{scalar:X4}" );
			if ( scalar <= 0xff ) {
				var byteReference = Reference( pattern, false );
				Assert.Equal( 2, byteReference.Match( subject ).Length );
				Compare( byteReference, subject, 0, Compile( pattern ).Match( subject ), $"byte {scalar:X2}" );
			}
		}
		foreach ( var literal in new[] { "\u00a0", "\u2003" } ) {
			var pattern = "(?x)a" + literal + "b";
			var subject = Encoding.UTF8.GetBytes( "a" + literal + "b" );
			var reference = Reference( pattern, true );
			Assert.Equal( subject.Length, reference.Match( subject ).Length );
			Assert.False( reference.Match( "ab"u8.ToArray() ).Success );
			Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions { Utf8 = true } ).Match( subject ), pattern );
			Compare( reference, "ab"u8.ToArray(), 0, Compile( pattern, new PcreByteOptions { Utf8 = true } ).Match( "ab"u8.ToArray() ), pattern );
		}
		var byteLiteral = "(?x)a\u00a0b";
		Compare( Reference( byteLiteral, false ), new byte[] { 0x61, 0xa0, 0x62 }, 0,
			Compile( byteLiteral ).Match( new byte[] { 0x61, 0xa0, 0x62 } ) );
		// Non-UTF parsing skips byte 85; it must not decode C2 85 as one scalar.
		const string nonUtfPattern = "(?x)a\u00c2\u0085b";
		var retained = new byte[] { 0x61, 0xc2, 0x62 };
		Assert.Equal( 3, Reference( nonUtfPattern, false ).Match( retained ).Length );
		Compare( Reference( nonUtfPattern, false ), retained, 0, Compile( nonUtfPattern ).Match( retained ) );
	}

	[Fact]
	public void FixedLiteralAssertionsInheritLexicalMode() {
		foreach ( var row in new[] {
			( Pattern: "(?x)(?= a # look\n b)(a b)", Subject: "ab" ),
			( Pattern: "(?x)(?! a (?# no atom)c)(a b)", Subject: "ab" ),
			( Pattern: "(?x)(?<= a # behind\n b)(c)", Subject: "abc" ),
			( Pattern: "(?x)(?<! a (?# no atom)c)(b)", Subject: "ab" ),
			( Pattern: @"(?x)(?=\Q #\E)\ \#", Subject: " #" ),
			( Pattern: "(?x)(?<= é (?# ignored))b", Subject: "éb" ),
			( Pattern: "(?=(?# independent of x)a)a", Subject: "a" )
		} ) {
			var subject = Encoding.UTF8.GetBytes( row.Subject );
			Compare( Reference( row.Pattern, true ), subject, 0,
				Compile( row.Pattern, new PcreByteOptions { Utf8 = true } ).Match( subject ), row.Pattern );
		}
	}

	[Fact]
	public void LeadingExtendedTriviaCanInterleaveWithExistingUtfAsciiDigitOptions() {
		foreach ( var pattern in new[] {
			@"(?x) (?# leading)(?-aD) \d", "(?x)# leading\n(?aD) (?-aD) \\d",
			@"(?aD)(?x) (?# leading)(?-aD) \d"
		} ) {
			var subject = "٣"u8.ToArray();
			var reference = new PcreRegex8Bit( Encoding.UTF8.GetBytes( "(*LF)" + pattern ), Encoding.UTF8,
				new PcreRegexSettings { Options = PcreOptions.Compiled | PcreOptions.Utf | PcreOptions.Ucp,
					ExtraCompileOptions = PcreExtraCompileOptions.AsciiBsD } );
			Assert.Equal( 2, reference.Match( subject ).Length );
			Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions {
				Utf8 = true, Ucp = true, AsciiBsD = true
			} ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ExtendedModesKeepEverySelectedInvalidUtfAndCrLfOffset( bool anyCrLf ) {
		foreach ( var pattern in new[] {
			"(?x)(a *) # comment\n (b ?)", "(?xx)([ é ]+) # comment\r\n (a*)",
			"(?xim:^ a $)", "(?xs)(?= é (?# body)) .", "(?xU)(?<a>a +) \\k<a>"
		} ) {
			var reference = Reference( pattern, true, invalidUtf: true, anyCrLf: anyCrLf );
			var managed = Compile( pattern, new PcreByteOptions { MatchInvalidUtf = true, AnyCrLfNewline = anyCrLf } );
			foreach ( var hex in new[] { "", "616162", "61FF62", "C3A9C3A9", "FFC3A9FF", "0D0A610D0A", "C3FF62" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, managed.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( "(?x)é # ignored\n **", "**" )]
	[InlineData( "(?x) # ignored\n *", "*" )]
	[InlineData( "(?x)(?#c)*", "*" )]
	[InlineData( "(?x)é (?# ignored)*+?", "?" )]
	[InlineData( "(?x)é (?# never closed", "(?#" )]
	[InlineData( "(?xx)é [ a ", "[" )]
	[InlineData( "(?x)é # ignored\n )", ")" )]
	[InlineData( "(?x)é (?<na me>a)", "(?<" )]
	[InlineData( "(?x)é \\x{6 # ignored\n1}", "\\x" )]
	public void DiagnosticsRetainOriginalUtf8SourceByteOffsets( string pattern, string marker ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, true ) );
		var markerIndex = pattern.LastIndexOf( marker, StringComparison.Ordinal );
		if ( marker == "**" ) { markerIndex++; }
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( Encoding.UTF8.GetByteCount( pattern[..markerIndex] ), result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(? x)a" )]
	[InlineData( "(?x)(? :a)" )]
	[InlineData( "(?x)(?<name (?# split)>a)" )]
	public void TriviaCannotSplitGroupOrOptionHeaders( string pattern ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, false ) );
		Assert.False( PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) ).IsSuccess );
	}

	[Fact]
	public void ExtendedScopesAndCommentsKeepDepthAndExecutionBudgets() {
		var nested = string.Concat( Enumerable.Repeat( "(?xx: (?# ignored)", 256 ) ) + "[ a ]" + new string( ')', 256 );
		Assert.Equal( 1, Compile( nested ).Match( "a"u8.ToArray() ).Match?.ByteLength );
		var tooDeep = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( "(?x:" + nested + ")" ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, tooDeep.Diagnostic?.Code );
		Assert.Equal( 4 + 255 * "(?xx: (?# ignored)".Length, tooDeep.Diagnostic?.ByteOffset );
		var subject = Enumerable.Repeat( (byte)'a', 40 ).ToArray();
		const string pattern = "(?xU)^ (a | aa) * (?# failure) [b] $";
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, new PcreByteOptions { MatchWorkLimit = 500 } ).Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new PcreByteOptions {
			WorkObserver = count => { if ( count == 20 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( "(?x)#" + new string( ' ', 10000 ) ), cancellationToken: cancellation.Token ) );
	}


	[Theory]
	[InlineData( @"(?xx)[ \Q\E ^ a ]", "b", true )]
	[InlineData( @"(?xx)[ \Q\E ^ a ]", "a", false )]
	[InlineData( @"(?xx)[ \E ^ \Q\E a ]", "b", true )]
	[InlineData( @"(?xx)[ \E ^ \Q\E a ]", "a", false )]
	[InlineData( @"(?xx)[ \E ^ \Q\E ^ a ]", "^", false )]
	[InlineData( @"(?xx)[ \Q\E ^ ] a ]", "]", false )]
	[InlineData( @"(?xx)[ \Q\E ^ ] a ]", "b", true )]
	[InlineData( @"[\Q\E^a]", "b", true )]
	[InlineData( @"[\E^a]", "a", false )]
	[InlineData( @"(?xx)[ \Q^\E a ]", "^", true )]
	[InlineData( @"(?xx)[ \Q^\E a ]", "b", false )]
	public void EmptyQuotesBeforeClassNegationDoNotOccupyTheFirstItem( string pattern, string text, bool success ) {
		foreach ( var utf8 in new[] { false, true } ) {
			var subject = Encoding.ASCII.GetBytes( text );
			var reference = Reference( pattern, utf8 );
			Assert.Equal( success, reference.Match( subject ).Success );
			Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions { Utf8 = utf8 } ).Match( subject ), pattern );
		}
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset,
		PcreByteMatchResult actual, string context = "" ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( native.Success ? native.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, actual.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var captured = native.Groups[group];
			Assert.Equal( captured.Success ? new PcreByteCapture( captured.Index, captured.Length ) : null,
				actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8, bool callerCaseless = false,
		bool invalidUtf = false, bool anyCrLf = false ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		if ( callerCaseless ) { options |= PcreOptions.Caseless; }
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = options } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var encoding = options?.Utf8 == true || options?.MatchInvalidUtf == true ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
