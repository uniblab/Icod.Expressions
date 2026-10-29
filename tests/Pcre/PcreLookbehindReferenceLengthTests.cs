using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Consumed backreference widths against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreLookbehindReferenceLengthTests {
	[Theory]
	[InlineData( @"(a)(?<=\1)b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(a)(?<!\1)b", "6162", -1, 0, "" )]
	[InlineData( @"(a)b(?<!\1)c", "616263", 0, 3, "0:3,0:1" )]
	[InlineData( @"(a)(?<=\g{1})b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(a)(?<=\g{-1})b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(a)(b)(?<=\g{-2}\g{-1})c", "616263", 0, 3, "0:3,0:1,1:1" )]
	[InlineData( @"(?<x>a)(?<=\k<x>)b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(?'x'a)(?<=\k'x')b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(?P<x>a)(?<=\k{x})b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(?<x>a)(?<=\g{x})b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(?<x>a)(?<=(?P=x))b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(?<=(a)\1)b", "616162", 2, 1, "2:1,0:1" )]
	[InlineData( @"(?<=(a)\g{-1})b", "616162", 2, 1, "2:1,0:1" )]
	[InlineData( @"(?<=(?<x>a)\k<x>)b", "616162", 2, 1, "2:1,0:1" )]
	[InlineData( @"(?<=(a{1,2})\1)b", "6161616162", 4, 1, "4:1,0:2" )]
	[InlineData( @"(?<=(a{1,2}?)\1)b", "6161616162", 4, 1, "4:1,0:2" )]
	[InlineData( @"(?<=(a|aa)\1)b", "6161616162", 4, 1, "4:1,0:2" )]
	[InlineData( @"(a{1,2})(?<=\1)b", "616162", 0, 3, "0:3,0:2" )]
	[InlineData( @"(a{1,2}?)(?<=\1)b", "616162", 0, 3, "0:3,0:2" )]
	[InlineData( @"(a|bb)(?<=\1)c", "626263", 0, 3, "0:3,0:2" )]
	[InlineData( @"(a)(?<=\1{2})b", "616162", 1, 2, "1:2,1:1" )]
	[InlineData( @"(a)(?<=\1{1,2})b", "616162", 1, 2, "1:2,1:1" )]
	[InlineData( @"(a)(?<=\1?)b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(a)(?<=\1{0})b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(?<=(a))(?<=\1)b", "6162", 1, 1, "1:1,0:1" )]
	[InlineData( @"(?<=(a{1,2}))(?<=\1)b", "616162", 2, 1, "2:1,0:2" )]
	public void NumberedRelativeNamedAndQuantifiedReferencesUseTheirTargetBody( string pattern, string hex,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, false, 0, index, length, captures );

	[Theory]
	// Repeating or omitting a capture changes participation, not the width of its captured body.
	[InlineData( @"(a)?(?<=\1)b", "62", -1, 0, "" )]
	[InlineData( @"(a)?(?<!\1)b", "62", 0, 1, "0:1,-" )]
	[InlineData( @"(a)*(?<=\1)b", "62", -1, 0, "" )]
	[InlineData( @"(a)*(?<!\1)b", "62", 0, 1, "0:1,-" )]
	[InlineData( @"(a){0}(?<=\1)b", "6162", -1, 0, "" )]
	[InlineData( @"(a){0}(?<!\1)b", "62", 0, 1, "0:1,-" )]
	[InlineData( @"(a){1,3}(?<=\1)b", "61616162", 0, 4, "0:4,2:1" )]
	[InlineData( @"(a)+(?<=\1)b", "61616162", 0, 4, "0:4,2:1" )]
	[InlineData( @"(a?)(?<=\1)b", "62", 0, 1, "0:1,0:0" )]
	[InlineData( @"()(?<=\1)b", "62", 0, 1, "0:1,0:0" )]
	[InlineData( @"(?<x>a)?(?<!\k<x>)b", "62", 0, 1, "0:1,-" )]
	[InlineData( @"(?<x>a){0}(?<=\k<x>{0})b", "62", 0, 1, "0:1,-" )]
	[InlineData( @"(?<=\1)(a)", "61", -1, 0, "" )]
	[InlineData( @"(?<!\1)(a)", "61", 0, 1, "0:1,0:1" )]
	[InlineData( @"(?<=\g{+1})(a)", "61", -1, 0, "" )]
	[InlineData( @"(?<!\g{+1})(a)", "61", 0, 1, "0:1,0:1" )]
	[InlineData( @"(?<=\k<x>)(?<x>a)", "61", -1, 0, "" )]
	[InlineData( @"(?<!\k<x>)(?<x>a)", "61", 0, 1, "0:1,0:1" )]
	[InlineData( @"^(?:(?<=\1)a|(a))+$", "6161", 0, 2, "0:2,0:1" )]
	[InlineData( @"^(?:(?<=\k<x>)a|(?<x>a))+$", "6161", 0, 2, "0:2,0:1" )]
	public void UnsetForwardAndRepeatedCapturesRetainDefaultParticipationRules( string pattern, string hex,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, false, 0, index, length, captures );

	[Theory]
	[InlineData( @"(a)(\1)(?<=\2)b", "616162", 0, 3, "0:3,0:1,1:1" )]
	[InlineData( @"(a{1,2})(\1)(?<=\2)b", "6161616162", 0, 5, "0:5,0:2,2:2" )]
	[InlineData( @"(?<x>a)(?<y>\k<x>)(?<=\k<y>)b", "616162", 0, 3, "0:3,0:1,1:1" )]
	[InlineData( @"(\g{2}){0}(a)(?<!\1)b", "6162", 0, 2, "0:2,-,0:1" )]
	[InlineData( @"(?<x>\k<y>){0}(?<y>\k<z>){0}(?<z>a)(?<!\k<x>)b", "6162", 0, 2, "0:2,-,-,0:1" )]
	[InlineData( @"(?<x>a){0}(?<=\k<x>{0})b", "62", 0, 1, "0:1,-" )]
	[InlineData( @"(?<x>a{1,256}){0}(?<=\k<x>{0})b", "62", 0, 1, "0:1,-" )]
	public void AcyclicDependenciesAndErasedDefinitionsResolveAcrossTheFullPattern( string pattern, string hex,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, false, 0, index, length, captures );

	[Theory]
	[InlineData( @"(a)(?<=\1(?=b))b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(a{1,2})(?<=\1(?=b))b", "6162", -1, 0, "" )]
	[InlineData( @"(a{1,2})(?<=\1(?!b))b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(a{1,2})(?<!\1(?=b))b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(a{1,2})(?<=\1\z)b", "6162", 0, 2, "0:2,0:1" )]
	[InlineData( @"(a)(?<=\1\z)b", "6162", -1, 0, "" )]
	[InlineData( @"(a{1,2})(?<=\1(?=(b?)))b", "6162", 0, 2, "0:2,0:1,1:0" )]
	[InlineData( @"(a)(?<=\1(?=b)|x{1,2})b", "6162", -1, 0, "" )]
	[InlineData( @"(a+)(?<=a(?=\1))a", "6161", 0, 2, "0:2,0:1" )]
	[InlineData( @"(?|(?<x>a)|(?<x>b))(?<=a(?=\k<x>))a", "6161", 0, 2, "0:2,0:1" )]
	[InlineData( @"(a)(?<=a(?=\1))a(?|)", "6161", 0, 2, "0:2,0:1" )]
	[InlineData( @"(a)(?<=\1)(b)", "6162", 0, 2, "0:2,0:1,1:1" )]
	public void OnlyConsumedReferencesContributeWidthAndVariableRangesScopeTheJitEndCap( string pattern, string hex,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, false, 0, index, length, captures );

	[Theory]
	[InlineData( @"(é)(?<=\1)b", "C3A962", 0, 3, "0:3,0:2" )]
	[InlineData( @"(😀)(?<=\1)b", "F09F988062", 0, 5, "0:5,0:4" )]
	[InlineData( @"([é😀]{1,2})(?<=\1)b", "C3A9F09F988062", 0, 7, "0:7,0:6" )]
	[InlineData( @"(?<=(é)\1)b", "C3A9C3A962", 4, 1, "4:1,0:2" )]
	[InlineData( @"(?<=(😀)\1)b", "F09F9880F09F988062", 8, 1, "8:1,0:4" )]
	[InlineData( @"(?<=(?<字>[é😀]{1,2})\k<字>)b", "C3A9F09F9880C3A9F09F988062", 12, 1, "12:1,0:6" )]
	[InlineData( @"(?i)(k)K(?<=\1)b", "6BE284AA62", 0, 5, "0:5,0:1" )]
	[InlineData( @"(?i)(K)k(?<=\1)b", "E284AA6B62", 0, 5, "0:5,0:3" )]
	[InlineData( @"(?i)(ſ)S(?<=\1)b", "C5BF5362", 0, 4, "0:4,0:2" )]
	[InlineData( @"(?i)(𐐀)𐐨(?<=\1)b", "F0909080F09090A862", 0, 9, "0:9,0:4" )]
	public void UtfBodyWidthsCountScalarsWhileCapturesAndCaselessReferencesKeepByteSpans( string pattern, string hex,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, true, 0, index, length, captures );

	[Theory]
	[InlineData( @"(?<=(a))(?<=\G\1)b", "6162", 0, 1, 1, "1:1,0:1" )]
	[InlineData( @"(?<=(a))(?<=\G\1)b", "6162", 1, -1, 0, "" )]
	[InlineData( @"(?<=(a))(?<=\1\G)b", "6162", 1, 1, 1, "1:1,0:1" )]
	[InlineData( @"(?<=(a))(?<=\1\G)b", "6162", 0, -1, 0, "" )]
	public void ReferencesCanReadBeforeGroupZeroAndRetainCallerStart( string pattern, string hex, int offset,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, false, offset, index, length, captures );

	[Theory]
	[InlineData( @"(a)(?<=\1)b", "FF6162", 1, 2, "1:2,1:1" )]
	[InlineData( @"(a{1,2})(?<=\1)b", "FF6162", -1, 0, "" )]
	[InlineData( @"()(?<=\1)b", "FF62", 1, 1, "1:1,1:0" )]
	public void TolerantUtfRetainsTheMaximumRewindBarrierForReferenceRanges( string pattern, string hex,
		int index, int length, string captures ) => AssertOutcome( pattern, hex, true, 0, index, length, captures, true );

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void EveryByteOffsetComparesReferenceRangesMalformedUtfAndCrLf( bool utf8, bool anyCrLf ) {
		foreach ( var pattern in new[] { @"(a)(?<=\1)", @"(a)(?<!\1)", @"(a)?(?<=\1)", @"(a)?(?<!\1)",
			@"(a?)(?<=\1)", @"(?<=(a)\1)", @"(?<=(a{1,2})\1)", @"(?<!(a{1,2})\1)",
			@"(?<x>[ab]{1,2})(?<=\k<x>)", @"(?<=(a))(?<=\G\1)", @"(?<=(a))(?<=\1\G)",
			@"(\R)(?<=\1)", @"(?<=(\R)\1)", @"((?s).)(?<=\1)", @"(?<!\k<x>)(?<x>a)",
			@"(a{1,2})(?<=\1(?=(b?)))b", @"(a){0}(?<=\1{0})", @"(a)(?<=a(?=\1))a" } ) {
			var reference = Reference( pattern, utf8, utf8, anyCrLf );
			var compiled = Compile( pattern, new PcreByteOptions {
				Utf8 = utf8, MatchInvalidUtf = utf8, AnyCrLfNewline = anyCrLf
			} );
			foreach ( var hex in new[] { "", "61", "6162", "6161616162", "6262", "FF", "FF6162", "61FF61", "61FF62",
				"80806161", "C3A9C3A962", "F09F9880F09F988062", "C3FF6162", "E2826162", "0D0A0D0A62", "610D0A62", "610D62", "0A610A62" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ),
						$"{pattern}, {hex}, offset {offset}, UTF={utf8}, ANYCRLF={anyCrLf}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"(a+)(?<=\1)b" )]
	[InlineData( @"(a*)(?<=\1)b" )]
	[InlineData( @"(?<x>a+)(?<!\k<x>)b" )]
	[InlineData( @"(a+){0}(?<=\1{0})b" )]
	[InlineData( @"(?<x>a*){0}(?<=\k<x>{0})b" )]
	[InlineData( @"(\1)(?<=\1)b" )]
	[InlineData( @"(a(?<=\1))" )]
	[InlineData( @"(?<x>a(?<=\k<x>))" )]
	[InlineData( @"(\2)(\1)(?<=\1)b" )]
	[InlineData( @"(?<x>\k<y>)(?<y>\k<x>)(?<=\k<x>)b" )]
	[InlineData( @"(?<x>\k<x>){0}(?<=\k<x>{0})b" )]
	[InlineData( @"(a)(?<=\1)b(?|)" )]
	[InlineData( @"(a)(?|)(?<=\1)b" )]
	[InlineData( @"(?|)(a)(?<=\1)b" )]
	[InlineData( @"(?|(?<x>a)|(?<x>b))(?<=\k<x>)c" )]
	[InlineData( @"(a){0}(?<=\1{0})b(?|)" )]
	public void UnboundedCyclicLexicallySelfAndGlobalBranchResetTargetsRejectAtBehindOpening( string pattern ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		var opening = pattern.IndexOf( "(?<=", StringComparison.Ordinal );
		if ( opening < 0 ) { opening = pattern.IndexOf( "(?<!", StringComparison.Ordinal ); }
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, opening );
	}

	[Theory]
	[InlineData( @"(?<x>a(?<=\k<y>))(?<y>\k<x>)", 6 )]
	[InlineData( @"(?<x>a(?<=\k<y>))(?<y>b(?<=\k<x>))(?<=\k<x>)", 6 )]
	[InlineData( @"(?<x>a(?=(?<=\k<y>)))(?<y>\k<x>)", 9 )]
	public void NestedBehindValidationRetainsTheActiveCaptureDependencyChain( string pattern, int opening ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, opening );
	}

	[Theory]
	[InlineData( @"(?<x>(?=\k<x>)a)(?<=\k<x>)", "61", -1, 0, "" )]
	[InlineData( @"(?<x>a(?<=a))(?<=\k<x>)b", "6162", 0, 2, "0:2,0:1" )]
	public void OrdinaryAheadReferencesAndIndependentBehindChecksDoNotCreateFalseCycles(
		string pattern, string hex, int index, int length, string captures ) =>
		AssertOutcome( pattern, hex, false, 0, index, length, captures );

	[Theory]
	[InlineData( @"(?<=\k<missing>)b", 4 )]
	[InlineData( @"(?<!\g{missing})b", 4 )]
	[InlineData( @"(?<=\1)b", 4 )]
	[InlineData( @"(a)(?<=\g{2})b", 7 )]
	[InlineData( @"(?<=\g{+2})(a)", 4 )]
	public void UnknownTargetsHaveInvalidReferenceDiagnosticsInsteadOfUnknownWidths( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, offset );
	}

	[Fact]
	public void FinalBehindRangeHasTheVariable255CapAndTargetAnalysisHasTheHard65535Cap() {
		var hex = Convert.ToHexString( Encoding.ASCII.GetBytes( new string( 'a', 255 ) + "b" ) );
		AssertOutcome( @"(a{1,255})(?<=\1)b", hex, false, 0, 0, 256, "0:256,0:255" );
		AssertOutcome( @"(a{256}){0}(?<=\1){0}b", "62", false, 0, 0, 1, "0:1,-" );
		AssertOutcome( @"(a{1,256}){0}(?<=\1{0})b", "62", false, 0, 0, 1, "0:1,-" );
		foreach ( var pattern in new[] { @"(a{1,256}){0}(?<=\1)b", @"(a{1,255}){0}(?<=\1a)b",
			@"(a{256}){0}(?<=\1|x{1,2})b", @"(a{65535}a){0}(?<=\1{0})b",
			@"((?:a{65535}){2}){0}(?<=\1{0})b" } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
			AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, pattern.IndexOf( "(?<=", StringComparison.Ordinal ) );
		}
	}

	[Fact]
	public void FullNameResolutionTraversesTheHeapParserDepthBoundary() {
		var nested = string.Concat( Enumerable.Repeat( "(?<=", 255 ) ) + @"\k<x>" + new string( ')', 255 );
		AssertOutcome( nested + "(?<x>a)", "61", false, 0, -1, 0, "" );
		AssertOutcome( "(?<x>a)" + nested + "b", "6162", false, 0, 0, 2, "0:2,0:1" );
		var atLimit = string.Concat( Enumerable.Repeat( "(?<=", 256 ) ) + @"\k<x>" + new string( ')', 256 ) + "(?<x>a)";
		AssertOutcome( atLimit, "61", false, 0, -1, 0, "" );
		AssertDiagnostic( "(?<=" + atLimit + ")", PcreByteDiagnosticCode.PatternDepthLimitExceeded, 1024 );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( atLimit ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void AcyclicForwardDependenciesResolveAndTheCorrespondingCycleRejects() {
		var definitions = string.Concat( Enumerable.Range( 1, 63 ).Select(
			n => "(?<g" + n + @">\k<g" + ( n + 1 ) + ">){0}" ) );
		var pattern = definitions + @"(?<g64>a)(?<!\k<g1>)b";
		var captures = "0:2," + string.Concat( Enumerable.Repeat( "-,", 63 ) ) + "0:1";
		AssertOutcome( pattern, "6162", false, 0, 0, 2, captures );
		var cyclic = definitions + @"(?<g64>\k<g1>){0}(?<=\k<g1>{0})b";
		Assert.Throws<PcrePatternException>( () => Reference( cyclic ) );
		AssertDiagnostic( cyclic, PcreByteDiagnosticCode.UnsupportedSyntax, cyclic.IndexOf( "(?<=", StringComparison.Ordinal ) );
	}

	[Fact]
	public void FlatCaptureDependenciesReachTheFull256CaptureLimitWithoutUsingTheClrStack() {
		static string Pattern( int count ) => "(a)" + string.Concat( Enumerable.Range( 1, count - 1 ).Select(
			n => @"(\g{" + n + "})" ) ) + @"(?<=\g{" + count + "})b";
		var nativeCaptures = "0:65," + string.Join( ",", Enumerable.Range( 0, 64 ).Select( n => $"{n}:1" ) );
		AssertOutcome( Pattern( 64 ), Convert.ToHexString( Encoding.ASCII.GetBytes( new string( 'a', 64 ) + "b" ) ),
			false, 0, 0, 65, nativeCaptures );
		var result = Compile( Pattern( 256 ) ).Match( Encoding.ASCII.GetBytes( new string( 'a', 256 ) + "b" ) );
		Assert.Null( result.Diagnostic );
		Assert.NotNull( result.Match );
		Assert.Equal( 257, result.Match.Captures.Count );
		Assert.Equal( new PcreByteCapture( 0, 257 ), result.Match.Captures[0] );
		for ( var group = 1; group <= 256; group++ ) {
			Assert.Equal( new PcreByteCapture( group - 1, 1 ), result.Match.Captures[group] );
		}
	}

	[Theory]
	[InlineData( @"(a)(?<=\1{1,254}x)b" )]
	[InlineData( @"(a)(?<!\1{1,254}x)b" )]
	[InlineData( @"a(?<=([a])\1?){30}b" )]
	public void ReferenceReverseExplorationSpendsWorkAndHonorsCancellation( string pattern ) {
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 254 ) + "b" );
		var limited = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 40 } ).Match( subject );
		Assert.Null( limited.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions {
			WorkObserver = work => { if ( work == 40 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
	}

	private static void AssertDiagnostic( string pattern, PcreByteDiagnosticCode code, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.Latin1.GetBytes( pattern ) );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void AssertOutcome( string pattern, string hex, bool utf8, int offset,
		int expectedIndex, int expectedLength, string captures, bool invalidUtf = false ) {
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern, utf8, invalidUtf );
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
		Compare( reference, subject, offset, Compile( pattern, new PcreByteOptions {
			Utf8 = utf8, MatchInvalidUtf = invalidUtf
		} ).Match( subject, offset ), pattern );
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
		// Raise only the native parser ceiling so the managed 256-level boundary has an oracle.
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = options, ParensLimit = 512 } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var encoding = options?.Utf8 == true || options?.MatchInvalidUtf == true ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
