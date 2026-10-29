using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Consumed nonrecursive call widths against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreSubroutineLookbehindReferenceTests {
	[Theory]
	[InlineData( @"(?<x>a)(?<=(?1))b", "6162", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?<x>a)(?<=(?01))b", "6162", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?<x>a)(?<=(?-1))b", "6162", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?<x>a)(?<=\g<1>)b", "6162", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?<x>a)(?<=\g'1')b", "6162", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?<x>a)(?<=\g<-1>)b", "6162", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?<x>a)(?<=\g'-1')b", "6162", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?<x>a)(?<=(?&x))b", "6162", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?<x>a)(?<=(?P>x))b", "6162", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?<x>a)(?<=\g<x>)b", "6162", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?<x>a)(?<=\g'x')b", "6162", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?<=(?+1))(?<x>a)b", "616162", false, 0, 1, 2, "1:2,1:1", false, false )]
	[InlineData( @"(?<=(?+01))(?<x>a)b", "616162", false, 0, 1, 2, "1:2,1:1", false, false )]
	[InlineData( @"(?<=\g<+1>)(?<x>a)b", "616162", false, 0, 1, 2, "1:2,1:1", false, false )]
	[InlineData( @"(?<=\g'+1')(?<x>a)b", "616162", false, 0, 1, 2, "1:2,1:1", false, false )]
	[InlineData( @"(?<=(?&x))(?<x>a)b", "616162", false, 0, 1, 2, "1:2,1:1", false, false )]
	[InlineData( @"(?<=(?P>x))(?<x>a)b", "616162", false, 0, 1, 2, "1:2,1:1", false, false )]
	[InlineData( @"(?<=\g<x>)(?<x>a)b", "616162", false, 0, 1, 2, "1:2,1:1", false, false )]
	[InlineData( @"(?<=\g'x')(?<x>a)b", "616162", false, 0, 1, 2, "1:2,1:1", false, false )]
	[InlineData( @"(?J)(?(DEFINE)(?<x>a)(?<x>b{256}))(?<=(?&x))c", "6163", false, 0, 1, 1, "1:1,-,-", false, false )]
	[InlineData( @"(?|(?<x>a)|(?<x>b{256}))(?<=(?1))c", "6163", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?J)(?|(?<x>a)|(?<x>b{256}))(?<=(?&x))c", "6163", false, 0, 0, 2, "0:2,0:1", false, false )]
	[InlineData( @"(?<x>a)?(?<=(?&x))b", "61616162", false, 0, 2, 2, "2:2,2:1", false, false )]
	[InlineData( @"(?<x>a)*(?<=(?&x))b", "61616162", false, 0, 0, 4, "0:4,2:1", false, false )]
	[InlineData( @"(?<x>a)+(?<=(?&x))b", "61616162", false, 0, 0, 4, "0:4,2:1", false, false )]
	[InlineData( @"(?<x>a){0}(?<=(?&x))b", "61616162", false, 0, 3, 1, "3:1,-", false, false )]
	[InlineData( @"(?<x>a){1,3}(?<=(?&x))b", "61616162", false, 0, 0, 4, "0:4,2:1", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>(?<y>a)\k<y>))(?<=(?&x))b", "616162", false, 0, 2, 1, "2:1,-,-", false, false )]
	[InlineData( @"(?<y>a)(?(DEFINE)(?<x>\k<y>))(?<=(?&x))b", "6162", false, 0, 0, 2, "0:2,0:1,-", false, false )]
	[InlineData( @"(?<x>(?&y)){0}(?(DEFINE)(?<y>a))(?<=\k<x>{0})b", "62", false, 0, 0, 1, "0:1,-,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>(?&y))(?<y>(?&z))(?<z>ab))(?<=(?&x))c", "616263", false, 0, 2, 1, "2:1,-,-,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a+))(?<=(?=(?&x)))a", "61", false, 0, 0, 1, "0:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a(?<=a)))(?<=(?&x))b", "6162", false, 0, 1, 1, "1:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a(?=\k<x>)))(?<=(?&x))b", "6162", false, 0, -1, 0, "", false, false )]
	[InlineData( @"(?<x>(?<y>a|b)\k<y>)(?<=(?&x))c", "616163", false, 0, 0, 3, "0:3,0:2,0:1", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a|ab))(?<=(?&x))c", "616263", false, 0, 2, 1, "2:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a|ab))(?<!(?&x))c", "7863", false, 0, 1, 1, "1:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a|ab))(?<!(?&x))c", "616263", false, 0, -1, 0, "", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>(?>a|ab)))(?<=(?&x))c", "616263", false, 0, -1, 0, "", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a|ab))(?<*(?&x))b?c", "616263", false, 0, 1, 2, "1:2,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a|ab))(?(?<=(?&x))c|d)", "616263", false, 0, 2, 1, "2:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a(?=b)))(?<=(?&x))b", "6162", false, 0, 1, 1, "1:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a{1,2}(?=b)))(?<=(?&x))b", "6162", false, 0, -1, 0, "", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a{1,2}(?!b)))(?<=(?&x))b", "6162", false, 0, 1, 1, "1:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a{1,2}(?=(b?))))(?<=(?&x))b", "6162", false, 0, 1, 1, "1:1,-,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>\Ga))(?<=(?&x))b", "6162", false, 0, 1, 1, "1:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>\Ga))(?<=(?&x))b", "6162", false, 1, -1, 0, "", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a\G))(?<=(?&x))b", "6162", false, 1, 1, 1, "1:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a\G))(?<=(?&x))b", "6162", false, 0, -1, 0, "", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>é))(?<=(?&x))b", "C3A962", true, 0, 2, 1, "2:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>[é😀]{1,2}))(?<=(?&x))b", "C3A9F09F988062", true, 0, 6, 1, "6:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>😀))(?<=(?&x){2})b", "F09F9880F09F988062", true, 0, 8, 1, "8:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a))(?<=(?&x))b", "FF6162", true, 0, 2, 1, "2:1,-", true, false )]
	[InlineData( @"(?(DEFINE)(?<x>a{1,2}))(?<=(?&x))b", "FF6162", true, 0, -1, 0, "", true, false )]
	[InlineData( @"(?(DEFINE)(?<x>))(?<=(?&x))b", "FF62", true, 0, 1, 1, "1:1,-", true, false )]
	[InlineData( @"(?(DEFINE)(?<x>\R))(?<=(?&x))b", "0D0A62", false, 0, 2, 1, "2:1,-", false, true )]
	[InlineData( @"(?(DEFINE)(?<x>.))(?<=(?&x))b", "0D0A62", false, 0, -1, 0, "", false, true )]
	[InlineData( @"^(?<x>(?<y>a|b))b(?<=(?&x))c\k<y>$", "61626361", false, 0, 0, 4, "0:4,0:1,0:1", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a\Kb))(?<=(?&x))c", "616263", false, 0, 1, 2, "1:2,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a\Kx|ab))(?<=(?&x))c", "616263", false, 0, 2, 1, "2:1,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a{1,2}\Kb))(?<=(?&x))c", "61616263", false, 0, 2, 2, "2:2,-", false, false )]
	[InlineData( @"(?(DEFINE)(?<x>a\Kb))(?<=(?&x){0})c", "63", false, 0, 0, 1, "0:1,-", false, false )]
	public void CallsUseDefinitionBodyWidthsAndRestoreCallerCaptures( string pattern, string hex,
		bool utf8, int offset, int index, int length, string captures, bool invalidUtf, bool anyCrLf ) =>
		AssertOutcome( pattern, hex, utf8, offset, index, length, captures, invalidUtf, anyCrLf );

	[Theory]
	[InlineData( @"(?(DEFINE)(?<x>a+))(?<=(?&x))b", "(?<=" )]
	[InlineData( @"(?(DEFINE)(?<x>a+))(?<=(?&x){0})b", "(?<=" )]
	[InlineData( @"(?(DEFINE)(?<x>a{1,256}))(?<=(?&x))b", "(?<=" )]
	[InlineData( @"(?(DEFINE)(?<x>a|b{256}))(?<=(?&x))c", "(?<=" )]
	[InlineData( @"(?(DEFINE)(?<x>a{1,2}))(?<=(?&x)|b{256})c", "(?<=" )]
	[InlineData( @"(?(DEFINE)(?<x>a\k<y>)(?<y>b(?&x)))(?<=(?&x))c", "(?<=" )]
	[InlineData( @"(?(DEFINE)(?<x>a(?&y))(?<y>b\k<x>))(?<=(?&x))c", "(?<=" )]
	[InlineData( @"(?(DEFINE)(?<x>a(?<=\k<x>)))(?<=(?&x))b", "(?<=" )]
	[InlineData( @"(?(DEFINE)(?<x>a(?<=a*)))(?<=(?&x))b", "(?<=" )]
	[InlineData( @"(?<=(?(DEFINE)(?<x>a(?<=a*)))b)(?<=(?&x))c", "(?<=a*" )]
	[InlineData( @"(?(DEFINE)(?<x>a+))(?<=(?=(?<=(?&x))))b", "(?<=(?&x)" )]
	public void UnboundedMixedCyclesAndNestedInvalidBehindsReportTheirOwningOpening( string pattern, string opening ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, pattern.IndexOf( opening, StringComparison.Ordinal ) );
	}

	[Theory]
	[InlineData( @"(?<=(?&missing))b", 4 )]
	[InlineData( @"é(?<=\g<missing>)b", 6 )]
	public void MissingTargetsReportTheCallUtfByteOpening( string pattern, int opening ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, true ) );
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, opening );
	}

	[Fact]
	public void SkippedGeneralBehindMetadataStillRejectsAtTheCallAfterWidthValidation() {
		const string pattern = "(?<=(?(DEFINE)(?<x>a(?<=a{1,2})))b)(?<=(?&x))c";
		_ = Reference( pattern );
		AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, pattern.LastIndexOf( "(?&", StringComparison.Ordinal ) );
	}

	[Fact]
	public void FinalRangesMergeCalleeAlternativesAndKeepThe255And65535Boundaries() {
		var hex = Convert.ToHexString( Encoding.ASCII.GetBytes( new string( 'a', 255 ) + "b" ) );
		AssertOutcome( @"(?(DEFINE)(?<x>a{1,255}))(?<=(?&x))b", hex, false, 0, 255, 1, "255:1,-" );
		AssertOutcome( @"(?(DEFINE)(?<x>a{256}))(?<=(?&x)|b)c", "6263", false, 0, 1, 1, "1:1,-" );
		AssertOutcome( @"(?(DEFINE)(?<x>a{256}))(?<=(?&x){0})b", "62", false, 0, 0, 1, "0:1,-" );
		AssertOutcome( @"(?(DEFINE)(?<x>a{1,256}))(?<=(?&x){0})b", "62", false, 0, 0, 1, "0:1,-" );
		// Compact source and emitted call stubs keep the instruction budget independent of the measured width.
		const string maximum = "(?(DEFINE)(?<g0>a)(?<g1>(?&g0){255})(?<g2>(?&g1){257}))(?<=(?&g2){0})b";
		AssertOutcome( maximum, "62", false, 0, 0, 1, "0:1,-,-,-" );
		foreach ( var pattern in new[] { maximum.Replace( "{255}", "{256}" ).Replace( "{257}", "{256}" ),
			@"(?(DEFINE)(?<x>a{65535}a))(?<=(?&x){0})b" } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
			AssertDiagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, pattern.IndexOf( "(?<=", StringComparison.Ordinal ) );
		}
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void CallerOffsetsMalformedUtfAndCrLfCompareEveryCapture( bool utf8, bool anyCrLf ) {
		foreach ( var pattern in new[] { @"(?(DEFINE)(?<x>.?))(?<=(?&x))b",
			@"(?(DEFINE)(?<x>\G.?))(?<=(?&x))b", @"(?(DEFINE)(?<x>\R))(?<=(?&x))b" } ) {
			var reference = Reference( pattern, utf8, utf8, anyCrLf );
			var compiled = Compile( pattern, new PcreByteOptions {
				Utf8 = utf8, MatchInvalidUtf = utf8, AnyCrLfNewline = anyCrLf
			} );
			foreach ( var hex in new[] { "", "6162", "FF6162", "0D0A62" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ),
						$"{pattern}, {hex}, {offset}, UTF={utf8}, ANYCRLF={anyCrLf}" );
				}
			}
		}
	}

	[Fact]
	public void ReverseCallExplorationSpendsSharedWorkAndHonorsCancellation() {
		const string pattern = "(?(DEFINE)(?<x>a))(?<=(?&x){1,254}x)b";
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 254 ) + "b" );
		Assert.False( Reference( pattern ).Match( subject ).Success );
		var limited = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 40 } ).Match( subject );
		Assert.Null( limited.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions {
			WorkObserver = work => { if ( work == 40 ) { cancelled.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancelled.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancelled.Token ) );
	}

	private static void AssertDiagnostic( string pattern, PcreByteDiagnosticCode code, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void AssertOutcome( string pattern, string hex, bool utf8, int offset,
		int index, int length, string captures, bool invalidUtf = false, bool anyCrLf = false ) {
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern, utf8, invalidUtf, anyCrLf );
		var native = reference.Match( subject, offset );
		Assert.Equal( index, native.Success ? native.Index : -1 );
		if ( native.Success ) {
			Assert.Equal( length, native.Length );
			var spans = captures.Split( ',' );
			Assert.Equal( spans.Length, native.Groups.Count );
			for ( var group = 0; group < spans.Length; group++ ) {
				var capture = native.Groups[group];
				Assert.Equal( spans[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
			}
		}
		Compare( reference, subject, offset, Compile( pattern, new PcreByteOptions {
			Utf8 = utf8, MatchInvalidUtf = invalidUtf, AnyCrLfNewline = anyCrLf
		} ).Match( subject, offset ), pattern );
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

	private static PcreRegex8Bit Reference( string pattern, bool utf8 = false, bool invalidUtf = false, bool anyCrLf = false ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = PcreOptions.Compiled | ( utf8 ? PcreOptions.Utf : PcreOptions.None )
				| ( invalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var encoding = options?.Utf8 == true || options?.MatchInvalidUtf == true ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
