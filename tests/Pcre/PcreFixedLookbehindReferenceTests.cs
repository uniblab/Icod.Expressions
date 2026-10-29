using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Fixed character-length lookbehind against test-only PCRE.NET 1.6.0 / PCRE2 10.48.</summary>
public sealed class PcreFixedLookbehindReferenceTests {
	[Theory]
	[InlineData( "(?<=[ab])c", "6163", 1, 1, "1:1" )]
	[InlineData( "(?<=[ab])c", "6463", -1, 0, "" )]
	[InlineData( "(?<![ab])c", "6463", 1, 1, "1:1" )]
	[InlineData( "(?<![ab])c", "6163", -1, 0, "" )]
	[InlineData( @"(?<=\w\d)c", "613263", 2, 1, "2:1" )]
	[InlineData( @"(?<=[^a]\x00)c", "FF0063", 2, 1, "2:1" )]
	[InlineData( "(?<=.)c", "0A63", -1, 0, "" )]
	[InlineData( "(?<=(?s).)c", "0A63", 1, 1, "1:1" )]
	[InlineData( @"(?<=\N)c", "6163", 1, 1, "1:1" )]
	[InlineData( @"(?<=(?s)\N)c", "0A63", -1, 0, "" )]
	[InlineData( "(?<=[ab]{2})c", "616263", 2, 1, "2:1" )]
	[InlineData( "(?<=[ab]{2}?)c", "616263", 2, 1, "2:1" )]
	[InlineData( "(?<=[ab]{2}+)c", "616263", 2, 1, "2:1" )]
	[InlineData( "(?<=(?:ab|cd){2})e", "6162636465", 4, 1, "4:1" )]
	[InlineData( "(?<=a|bc)d", "626364", 2, 1, "2:1" )]
	[InlineData( "(?<!a|bc)d", "786364", 2, 1, "2:1" )]
	[InlineData( "(?<!a|bc)d", "626364", -1, 0, "" )]
	[InlineData( "(?<=a|bc)d", "64", -1, 0, "" )]
	[InlineData( "(?<=(?>ab|ac))d", "616364", 2, 1, "2:1" )]
	[InlineData( "(?<=a(?=b+))b", "616262", 1, 1, "1:1" )]
	[InlineData( "(?<=a(?=b)+)b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<=(?<=a){2})b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<=a(?<=a)?b)c", "616263", 2, 1, "2:1" )]
	[InlineData( "(?<=a(?!c))b", "6162", 1, 1, "1:1" )]
	[InlineData( "(?<=(?<=[x])[a])b", "786162", 2, 1, "2:1" )]
	[InlineData( "(?<=a(?<![b]))c", "6163", 1, 1, "1:1" )]
	[InlineData( "(?<=(?i)[a])B", "4142", 1, 1, "1:1" )]
	[InlineData( "(?<=(?i)[a])b", "4142", -1, 0, "" )]
	[InlineData( "(?<=(?x) [a] # body comment\n (?# ignored) b )c", "616263", 2, 1, "2:1" )]
	[InlineData( "(?<=(?:))a", "61", 0, 1, "0:1" )]
	[InlineData( "(?<!(?:))a", "61", -1, 0, "" )]
	[InlineData( "(?<=(?:^){2}a)b", "6162", 1, 1, "1:1" )]
	public void FixedBodiesKeepOriginalPositionAndBranchLengths( string pattern, string hex,
		int expectedIndex, int expectedLength, string captures ) =>
		AssertOutcome( pattern, hex, false, 0, expectedIndex, expectedLength, captures );

	[Theory]
	[InlineData( "(?<=(a))b", "6162", 1, 1, "1:1,0:1" )]
	[InlineData( "(?<=((a)b))c", "616263", 2, 1, "2:1,0:2,0:1" )]
	[InlineData( "(?<=(a)|(ba))c", "626163", 2, 1, "2:1,1:1,-" )]
	[InlineData( "(?<=(ba)|(a))c", "626163", 2, 1, "2:1,0:2,-" )]
	[InlineData( @"^ba(?<=(a)|(ba))\2$", "62616261", -1, 0, "" )]
	[InlineData( @"^ba(?<=(ba)|(a))\1$", "62616261", 0, 4, "0:4,0:2,-" )]
	[InlineData( "(?<=(ab)|(cd))e", "636465", 2, 1, "2:1,-,0:2" )]
	[InlineData( "(?<=(a){2})b", "616162", 2, 1, "2:1,1:1" )]
	[InlineData( @"(?<=(?<left>[ab]{2}))\k<left>", "61626162", 2, 2, "2:2,0:2" )]
	[InlineData( "(?<=(?|(a)|(b)))c", "6263", 1, 1, "1:1,0:1" )]
	[InlineData( @"(?<=(?|(?<x>a)|(?<x>b)))\g{x}", "6262", 1, 1, "1:1,0:1" )]
	[InlineData( "(?n)(?<=(a)(?<x>b))c", "616263", 2, 1, "2:1,1:1" )]
	[InlineData( "(?<=(?n)(a)(?<x>b))(c)", "616263", 2, 1, "2:1,1:1,2:1" )]
	[InlineData( "(?<=a(?=(bc)))b", "616263", 1, 1, "1:1,1:2" )]
	[InlineData( "(?<=a(?=(b+)))b", "616262", 1, 1, "1:1,1:2" )]
	[InlineData( @"(?<=(a)(?=\1))a", "6161", 1, 1, "1:1,0:1" )]
	[InlineData( @"(?<x>a)(?<=a(?=\k<x>))a", "6161", 0, 2, "0:2,0:1" )]
	[InlineData( @"(?<=a(?=(\R)))\R", "610D0A", 1, 2, "1:2,1:2" )]
	[InlineData( "(?<!(a))b", "6362", 1, 1, "1:1,-" )]
	[InlineData( "(?<!(a)(?=(c)))b", "6162", 1, 1, "1:1,-,-" )]
	[InlineData( "(?:(?<=(a))c|b)", "6162", 1, 1, "1:1,-" )]
	[InlineData( "(?<=())a", "61", 0, 1, "0:1,0:0" )]
	[InlineData( "(?<=(?=(a)))a", "61", 0, 1, "0:1,0:1" )]
	[InlineData( "(?<!(?=(b)))a", "61", 0, 1, "0:1,-" )]
	public void CapturesMayPrecedeGroupZeroOrExtendBeyondTheAssertionEndpoint( string pattern, string hex,
		int expectedIndex, int expectedLength, string captures ) =>
		AssertOutcome( pattern, hex, false, 0, expectedIndex, expectedLength, captures );

	[Theory]
	[InlineData( "(?<=([é😀]{2}))a", "C3A9F09F988061", 6, 1, "6:1,0:6" )]
	[InlineData( "(?<=([é😀]{2}))a", "F09F9880C3A961", 6, 1, "6:1,0:6" )]
	[InlineData( "(?<=([é😀]))a", "F09F988061", 4, 1, "4:1,0:4" )]
	[InlineData( "(?<=.)a", "F09F988061", 4, 1, "4:1" )]
	[InlineData( @"(?<=\N{2})a", "C3A9F09F988061", 6, 1, "6:1" )]
	[InlineData( "(?<=é|😀a)b", "F09F98806162", 5, 1, "5:1" )]
	[InlineData( "(?<=(é|😀))a", "F09F988061", 4, 1, "4:1,0:4" )]
	[InlineData( "(?<![é😀])a", "C3A961", -1, 0, "" )]
	[InlineData( "(?i)(?<=(K))a", "6B61", 1, 1, "1:1,0:1" )]
	[InlineData( "(?i)(?<=(k))a", "E284AA61", 3, 1, "3:1,0:3" )]
	[InlineData( "(?i)(?<=(ſ))a", "5361", 1, 1, "1:1,0:1" )]
	[InlineData( "(?i)(?<=(𐐀))a", "F09090A861", 4, 1, "4:1,0:4" )]
	[InlineData( "(?<=(?<字>😀))a", "F09F988061", 4, 1, "4:1,0:4" )]
	[InlineData( "(?<=(?:é😀){1})a", "C3A9F09F988061", 6, 1, "6:1" )]
	public void UtfLengthsCountCharactersAndCapturesKeepSourceByteSpans( string pattern, string hex,
		int expectedIndex, int expectedLength, string captures ) =>
		AssertOutcome( pattern, hex, true, 0, expectedIndex, expectedLength, captures );

	[Theory]
	[InlineData( @"(?<=\G[a])b", "6162", 0, 1, 1, "1:1" )]
	[InlineData( @"(?<=\G[a])b", "6162", 1, -1, 0, "" )]
	[InlineData( @"(?<=[a]\G)b", "6162", 1, 1, 1, "1:1" )]
	[InlineData( @"(?<=[a]\G)b", "6162", 0, -1, 0, "" )]
	[InlineData( @"(?<!\G[a])b", "6162", 1, 1, 1, "1:1" )]
	[InlineData( @"(?<=(\G[a]))b", "786162", 1, 2, 1, "2:1,1:1" )]
	public void LookbehindRetainsTheOriginalCallerStart( string pattern, string hex, int offset,
		int expectedIndex, int expectedLength, string captures ) =>
		AssertOutcome( pattern, hex, false, offset, expectedIndex, expectedLength, captures );

	[Theory]
	[InlineData( "a(?<=([a])){2,3}b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( "a(?<=([a])){2,3}?b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( "a(?<=([a]))++b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( "a(?<!([a]))*b", "6162", 0, 2, "0:2,-" )]
	[InlineData( "a(?<!([a]))+b", "6162", -1, 0, "" )]
	[InlineData( "a(?<=([b])){0}b", "6162", 0, 2, "0:2,-" )]
	public void DirectAssertionRepeatsKeepCapturesAndFiniteBounds( string pattern, string hex,
		int expectedIndex, int expectedLength, string captures ) =>
		AssertOutcome( pattern, hex, false, 0, expectedIndex, expectedLength, captures );

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void EverySelectedOffsetKeepsLeftContextMalformedFragmentsAndNewlines( bool utf8, bool anyCrLf ) {
		// Including naked and zero-length bodies prevents a consuming suffix from hiding boundary failures.
		foreach ( var pattern in new[] { @"(?<=a)", @"(?<!a)", @"(?<=[a])", @"(?<![a])", @"(?<=(a))", @"(?<!(a))", @"(?<=(.))",
			@"(?<!.)", @"(?<=\N)", @"(?<!\N)", @"(?<=[ab]{2})", @"(?<![ab]{2})", @"(?<=a|ba)",
			@"(?<!a|ba)", @"(?<=a(?=(b)))b", @"(?<=(?<=a)[b])", @"(?<=\G[a])", @"(?<=[a]\G)",
			@"(?m)(?<=^[a])", @"(?<=[a]$)", @"(?<=\r\n)", @"(?<=(?s).)", @"(?<=(?:))", @"(?<!(?:))",
			@"(?<=())", @"(?<!())", @"(?<=[a]{0})", @"(?<![a]{0})", @"(?<=(?=a))", @"(?<!(?=a))",
			@"(?<=\b)", @"(?<!\B)", @"(?<=[a])+", @"(?<![a])*?", @"(?<=)", @"(?<!)" } ) {
			var reference = Reference( pattern, utf8, utf8, anyCrLf );
			var compiled = Compile( pattern, new PcreByteOptions {
				Utf8 = utf8, MatchInvalidUtf = utf8, AnyCrLfNewline = anyCrLf
			} );
			foreach ( var hex in new[] { "", "61", "6162", "626161", "FF", "8080", "80FF", "61FF", "61FFFF62",
				"61FF61", "8061", "C3A9", "61C3A962", "C3FF61", "F09F9880FF61", "E28261",
				"0D0A61", "610D0A", "610D62", "0A610A" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ),
						$"{pattern}, {hex}, offset {offset}, UTF={utf8}, ANYCRLF={anyCrLf}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( "(?<=[a]+)b", 0 )]
	[InlineData( "(?<![a]?)b", 0 )]
	[InlineData( "(?<=[a]{1,2})b", 0 )]
	[InlineData( "(?<=(?:a|bc))d", 0 )]
	[InlineData( "(?<=(?>a|bc))d", 0 )]
	[InlineData( "(?<=(a|bc))d", 0 )]
	[InlineData( @"(?<=\R)b", 0 )]
	[InlineData( @"(a)(?<=\1)b", 3 )]
	[InlineData( @"(?<=(a)\g{-1})b", 0 )]
	[InlineData( @"(?<x>a)(?<=\k<x>)b", 7 )]
	[InlineData( "x(?<=[a]*)b", 1 )]
	[InlineData( "(?<=(?:^)*a)b", 0 )]
	[InlineData( "(?<=(?:^)+a)b", 0 )]
	[InlineData( "(?<=[a]*){0}", 0 )]
	[InlineData( "(?<=[a]+){0}", 0 )]
	public void VariableConsumedBodiesRemainUnsupportedAtTheirOpening( string pattern, int offset ) {
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, offset );
	}

	[Theory]
	[InlineData( "(?<=(?:a*){0})b" )]
	[InlineData( @"(?<=(?:\R*){0})b" )]
	public void ErasedGroupsStillValidateTheirUnboundedConsumedBodies( string pattern ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, false ) );
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, 0 );
	}

	[Theory]
	[InlineData( "(?<=(?<=a)*)b" )]
	[InlineData( "(?<=(?<=a)+)b" )]
	public void UnboundedDirectNestedBehindRemainsUnsupported( string pattern ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, false ) );
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, 0 );
	}

	[Theory]
	[InlineData( "(?<=[a]{65535}[b])c" )]
	[InlineData( "(?<=(?:[a]{65535}){2})b" )]
	[InlineData( "(?<=(?:[a]{65535}){65535})b" )]
	public void GeneralWidthSumsAndProductsRemainBounded( string pattern ) =>
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, 0 );

	[Fact]
	public void MaximumFixedWidthCanBeValidatedWithoutEmittingItsErasedBody() {
		var result = Compile( "(?<=[a]{65535}){0}b" ).Match( "b"u8.ToArray() );
		Assert.Null( result.Diagnostic );
		Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match?.Captures[0] );
	}

	[Theory]
	[InlineData( @"x(?<=\X)a" )]
	[InlineData( @"x(?<!\C)a" )]
	public void UnsupportedBodyEscapesKeepTheLegacyBehindOpeningDiagnostic( string pattern ) =>
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, 1 );

	[Theory]
	[InlineData( @"(?<=\K)" )]
	[InlineData( @"(?<!\K)" )]
	[InlineData( @"(?<=(?:[a]\K))" )]
	[InlineData( @"(?<!(?>[a]\K))" )]
	[InlineData( @"(?<=a(?=\K))" )]
	[InlineData( @"(?<!a(?!\K))" )]
	[InlineData( @"(?=(?<=[a]\K))" )]
	public void ResetStartIsForbiddenThroughoutActiveAssertions( string pattern ) {
		foreach ( var utf8 in new[] { false, true } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern, utf8 ) );
			AssertDiagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, 0, utf8 );
		}
	}

	[Theory]
	[InlineData( "(*plb:[a])" )]
	[InlineData( "(*nlb:[a])" )]
	[InlineData( "(?<*[a])" )]
	[InlineData( "(?(?<=[a])b|c)" )]
	public void GeneralAssertionAliasesAndConditionsRemainPending( string pattern ) =>
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, 0 );

	[Fact]
	public void FixedLookbehindKeepsTheHeapParserDepthBoundary() {
		var atLimit = string.Concat( Enumerable.Repeat( "(?<=", 256 ) ) + "[a]" + new string( ')', 256 );
		var result = Compile( atLimit ).Match( "a"u8.ToArray() );
		Assert.Null( result.Diagnostic );
		Assert.Equal( new PcreByteCapture( 1, 0 ), result.Match?.Captures[0] );
		AssertDiagnostic( "(?<=" + atLimit + ")", PcreByteDiagnosticCode.PatternDepthLimitExceeded, 1024 );
	}

	[Fact]
	public void DeepNamedResolutionVisitsFixedAssertionBodies() {
		var pattern = string.Concat( Enumerable.Repeat( "(?<=", 255 ) ) + "(?<x>[a])"
			+ new string( ')', 255 ) + @"\k<x>";
		var result = Compile( pattern ).Match( "aa"u8.ToArray() );
		Assert.Null( result.Diagnostic );
		Assert.Equal( new PcreByteCapture( 1, 1 ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match?.Captures[1] );
		Assert.Equal( 2, result.Match?.Captures.Count );
	}

	[Theory]
	[InlineData( 1, 1022, true )]
	[InlineData( 1, 1023, false )]
	[InlineData( 2, 1021, true )]
	[InlineData( 2, 1022, false )]
	public void BehindGuardsShareTheExistingSavedPathCeiling( int guards, int choices, bool succeeds ) {
		// The outer body has length zero; choices live in a nested ahead and remain on its saved-path stack.
		var pattern = string.Concat( Enumerable.Repeat( "(?<=", guards ) ) + "(?="
			+ string.Concat( Enumerable.Repeat( "a??", choices ) ) + ")" + new string( ')', guards );
		var result = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 100_000 } ).Match( Array.Empty<byte>() );
		Assert.Equal( succeeds, result.IsSuccess );
		if ( succeeds ) {
			Assert.Null( result.Diagnostic );
			Assert.Equal( new PcreByteCapture( 0, 0 ), result.Match?.Captures[0] );
		} else {
			Assert.Null( result.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code );
		}
	}

	[Theory]
	[InlineData( "(?<=a(?=(?:b|bb)*c))b" )]
	[InlineData( "(?<!a(?=(?:b|bb)*c))b" )]
	[InlineData( "a(?<=([a])){30}b" )]
	public void BehindExplorationSpendsWorkAndHonorsCancellation( string pattern ) {
		var subject = Encoding.ASCII.GetBytes( "a" + new string( 'b', 40 ) );
		var limited = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 40 } ).Match( subject );
		Assert.Null( limited.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions {
			WorkObserver = work => { if ( work == 40 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
	}

	private static void AssertDiagnostic( string pattern, PcreByteDiagnosticCode code, int offset, bool utf8 = false ) {
		var result = PcreByteCompiler.Compile( ( utf8 ? Encoding.UTF8 : Encoding.Latin1 ).GetBytes( pattern ),
			new PcreByteOptions { Utf8 = utf8 } );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void AssertOutcome( string pattern, string hex, bool utf8, int offset,
		int expectedIndex, int expectedLength, string captures ) {
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern, utf8 );
		var native = reference.Match( subject, offset );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) {
			Assert.Equal( expectedLength, native.Length );
			var spans = captures.Split( ',' );
			Assert.Equal( spans.Length, native.Groups.Count );
			for ( var group = 0; group < spans.Length; group++ ) {
				var capture = native.Groups[group];
				Assert.Equal( spans[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
			}
		}
		Compare( reference, subject, offset, Compile( pattern, new PcreByteOptions { Utf8 = utf8 } ).Match( subject, offset ), pattern );
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset,
		PcreByteMatchResult actual, string context ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( native.Success ? native.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}; native index {( native.Success ? native.Index : -1 )}, managed index {actual.Match?.ByteIndex ?? -1}" );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, actual.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var capture = native.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null,
				actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8 = false, bool invalidUtf = false, bool anyCrLf = false ) {
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = options } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var encoding = options?.Utf8 == true || options?.MatchInvalidUtf == true ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
