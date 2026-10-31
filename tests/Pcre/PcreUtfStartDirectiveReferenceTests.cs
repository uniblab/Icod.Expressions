using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>One prefix settings phase must select UTF/UCP before validation and parsing.</summary>
public sealed class PcreUtfStartDirectiveReferenceTests {
	[Theory]
	[InlineData( @"(*UTF)\A(.)\z", "C3A9", "0:2,0:2", false, false )]
	[InlineData( @"(*UTF8)\A(.)\z", "F09F9880", "0:4,0:4", false, false )]
	[InlineData( @"(*UTF)\w", "C3A9", "", false, false )]
	[InlineData( @"(*UTF)(*UCP)\w", "C3A9", "0:2", false, false )]
	[InlineData( @"(*UCP)(*UTF8)\d", "D9A1", "0:2", false, false )]
	[InlineData( @"(*UTF)\d", "D9A1", "", false, false )]
	[InlineData( @"(*UCP)\w", "C3A9", "0:2", true, false )]
	[InlineData( @"(*UTF)\w", "C3A9", "0:2", false, true )]
	[InlineData( @"(*UTF8)\w", "C3A9", "0:2", false, true )]
	[InlineData( @"(*UTF8)(*UTF)(*UCP)(*UCP)", "", "0:0", false, false )]
	[InlineData( @"(*UTF)(*UCP)(?^)\w", "C3A9", "0:2", false, false )]
	[InlineData( @"(*UTF)(*UCP)(?aD)\d", "D9A1", "", false, false )]
	[InlineData( @"(*UTF)(*UCP)(?aW)\w", "C3A9", "", false, false )]
	[InlineData( @"(*UTF)\p{L}", "C3A9", "0:2", false, false )]
	[InlineData( @"(*UTF)(?<é>é)\k<é>", "C3A9C3A9", "0:4,0:2", false, false )]
	[InlineData( @"(*UTF)(*UCP)(?<x>\w)(?&x)", "C3A9C3A9", "0:4,0:2", false, false )]
	[InlineData( @"(*UTF)\A(?<x>é(?&x)?)\z", "C3A9C3A9", "0:4,0:4", false, false )]
	[InlineData( @"(*UTF)é(?R)?", "C3A9C3A9", "0:4", false, false )]
	[InlineData( @"(*UTF)(?<=(é){1,2})X", "C3A9C3A958", "4:1,2:2", false, false )]
	[InlineData( @"(*UTF8)(?<=é{2})(X)", "C3A9C3A958", "4:1,4:1", false, false )]
	[InlineData( @"(*ANYCRLF)(*UCP)(*UTF8)(*BSR_UNICODE)(*LF)\A(\R)(.)\z", "E280A80D", "0:4,0:3,3:1", false, false )]
	[InlineData( @"(*UTF)(*LF)(*BSR_UNICODE)(*UCP)(*ANYCRLF)(*BSR_ANYCRLF)\A\R\z", "0B", "", false, false )]
	[InlineData( @"(*UCP)(*BSR_ANYCRLF)(*UTF)(*ANYCRLF)(?m)^X", "0D58", "1:1", false, false )]
	[InlineData( "(*UTF8)(*ANYCRLF)(?x)#ignored\r(é)", "C3A9", "0:2,0:2", false, false )]
	[InlineData( @"\Q(*UTF)\E.", "282A55544629FF", "0:7", false, false )]
	[InlineData( @"[(*UTF)]\xFF", "55FF", "0:2", false, false )]
	[InlineData( @"(*UTF)\Q(*UCP)\E\w", "282A55435029C3A9", "", false, false )]
	public void EffectiveModesHavePinnedLiteralCaptureSpans( string pattern, string hex, string spans, bool callerUtf, bool callerUcp ) {
		var options = new PcreByteOptions { Utf8 = callerUtf, Ucp = callerUcp };
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern, options );
		AssertLiteral( reference, subject, spans );
		Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( "(*UTF)", false )]
	[InlineData( "(*UTF8)", false )]
	[InlineData( "(*UTF)(*UCP)", true )]
	[InlineData( "(*UCP)(*UTF8)", true )]
	public void UtfAndUcpRemainIndependentForShorthandMembership( string prefix, bool ucp ) {
		foreach ( var (body, ascii, unicode) in new[] {
			(@"\d", new[] { 48, 57 }, new[] { 0x661 }),
			(@"\w", new[] { 48, 57, 65, 95, 97 }, new[] { 0xE9, 0x661, 0x301 }),
			(@"\s", new[] { 9, 10, 11, 12, 13, 32 }, new[] { 0x85, 0xA0, 0x180E, 0x2028 }) } ) {
			var pattern = prefix + @"\A" + body + @"\z";
			var reference = Reference( pattern, new() );
			var compiled = Compile( pattern, new() );
			foreach ( var scalar in new[] { 0, 9, 10, 11, 12, 13, 32, 48, 57, 65, 95, 97, 0x85, 0xA0, 0xE9, 0x661, 0x301, 0x180E, 0x2028, 0x1F600, 0x10FFFF } ) {
				var subject = Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) );
				Assert.Equal( ascii.Contains( scalar ) || ucp && unicode.Contains( scalar ), reference.Match( subject ).Success );
				Compare( reference, subject, 0, compiled.Match( subject ), $"{pattern}, U+{scalar:X}" );
			}
		}
	}

	[Fact]
	public void CallerCaselessAndAsciiRestrictionSettingsArePreserved() {
		foreach ( var (pattern, hex, spans, options) in new[] {
			("(*UTF)é", "C389", "0:2", new PcreByteOptions { Caseless = true }),
			(@"(*UTF)(*UCP)\d", "D9A1", "", new PcreByteOptions { AsciiBsD = true }),
			(@"(*UCP)\d", "D9A1", "", new PcreByteOptions { MatchInvalidUtf = true, AsciiBsD = true }) } ) {
			var subject = Convert.FromHexString( hex );
			var reference = Reference( pattern, options );
			AssertLiteral( reference, subject, spans );
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void TolerantUtfOffsetsAndMixedPrefixesRetainSharedFragmentProtocols( bool callerAnyCrLf ) {
		var options = new PcreByteOptions { MatchInvalidUtf = true, AnyCrLfNewline = callerAnyCrLf };
		foreach ( var prefix in new[] { "(*UTF)", "(*UTF8)(*UCP)", "(*UCP)(*UTF)", "(*LF)(*UCP)(*UTF8)(*BSR_ANYCRLF)" } ) {
			foreach ( var body in new[] { @".", @"\w+", @"(\w)\1", @"\G.", @"(?<=\w)X", @"[\w]X" } ) {
				var pattern = prefix + body;
				var reference = Reference( pattern, options );
				var compiled = Compile( pattern, options );
				foreach ( var hex in new[] { "FFC3A958", "F09F9880FF58", "C3A9C3A9", "0D0A58", "C3FFA958" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( "282A55544629FF", 6 )]
	[InlineData( "282A5554463829282A55435029C0AF", 13 )]
	[InlineData( "282A55435029282A55544629EDA080", 12 )]
	[InlineData( "282A555446295C71FF", 8 )]
	[InlineData( "282A5554462928FF", 7 )]
	[InlineData( "282A5554462941F4908080", 7 )]
	public void DirectiveUtfValidationUsesOriginalBytesBeforeBodyParsing( string hex, int offset ) {
		var source = Convert.FromHexString( hex );
		foreach ( var callerUcp in new[] { false, true } ) {
			var options = new PcreByteOptions { Ucp = callerUcp };
			Assert.Throws<PcrePatternException>( () => Reference( source, options ) );
			var result = PcreByteCompiler.Compile( source, options );
			Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, result.Diagnostic?.Code );
			Assert.Equal( offset, result.Diagnostic?.ByteOffset );
		}
	}

	[Theory]
	[InlineData( "FF", 0 )]
	[InlineData( "61FF", 1 )]
	[InlineData( "EDB080", 0 )]
	public void UtfDirectiveDoesNotEnableInvalidSubjectTolerance( string hex, int offset ) {
		const string pattern = "(*UTF).";
		var subject = Convert.FromHexString( hex );
		Assert.ThrowsAny<PcreException>( () => Reference( pattern, new() ).Match( subject ) );
		var result = Compile( pattern, new() ).Match( subject );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
		Assert.Null( result.Match );
	}

	[Theory]
	[InlineData( " (*UTF)", 1 )]
	[InlineData( "(?x)(*UTF8)", 4 )]
	[InlineData( "(*UTF)(?#x)(*UCP)", 11 )]
	[InlineData( "(*UCP)(?i)(*UTF)", 10 )]
	[InlineData( "(*UTF8)|(*UTF)", 8 )]
	[InlineData( "(*utf)", 0 )]
	[InlineData( "(*UTF16)a", 0 )]
	[InlineData( "(*UTF32)a", 0 )]
	public void PlacementCaseAndOtherCodeUnitWidthsStayRejected( string pattern, int offset ) {
		var options = new PcreByteOptions { Utf8 = true };
		Assert.Throws<PcrePatternException>( () => Reference( pattern, options ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"(*UTF", 1 )]
	[InlineData( @"(*UCP", 1 )]
	[InlineData( @"(*UTF8)é\", 9 )]
	[InlineData( @"(*UTF)(*UCP)(", 12 )]
	public void MalformedBodiesKeepOriginalByteOffsets( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void ByteUcpAndExistingEarlyDiagnosticPrecedenceStayExplicit() {
		foreach ( var pattern in new[] { @"(*UCP)\w", @"(*LF)a" } ) {
			var options = new PcreByteOptions { Ucp = true };
			Assert.True( Reference( pattern, options ).Match( "a"u8.ToArray() ).Success );
			var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.Equal( 1, result.Pattern!.Match( "a"u8.ToArray() ).Match?.ByteLength );
		}
		var invalid = Convert.FromHexString( "282A55544629FF" );
		var work = PcreByteCompiler.Compile( invalid, new PcreByteOptions { MatchWorkLimit = 0 } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit, work.Diagnostic?.Code );
		Assert.Equal( 0, work.Diagnostic?.ByteOffset );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( invalid,
			new PcreByteOptions { MatchWorkLimit = 0 }, cancellation.Token ) );
	}

	[Fact]
	public void LongGlobalPrefixAddsNoVmWorkAndPreservesResourceBoundaries() {
		const string body = @"(?<x>\w+)";
		var prefix = String.Concat( Enumerable.Repeat( "(*UCP)(*UTF8)(*ANYCRLF)(*BSR_UNICODE)", 512 ) );
		var subject = "éé"u8.ToArray();
		var optionSteps = new List<int>();
		var prefixSteps = new List<int>();
		var direct = Compile( body, new() { Utf8 = true, Ucp = true, AnyCrLfNewline = true, WorkObserver = optionSteps.Add } );
		var prefixed = Compile( prefix + body, new() { BsrAnyCrLf = true, WorkObserver = prefixSteps.Add } );
		var reference = Reference( prefix + body, new() { BsrAnyCrLf = true } );
		AssertLiteral( reference, subject, "0:4,0:4" );
		Compare( reference, subject, 0, prefixed.Match( subject ), "long prefix" );
		Assert.Equal( 4, direct.Match( subject ).Match?.ByteLength );
		Assert.Equal( optionSteps, prefixSteps );
		Assert.Equal( Enumerable.Range( 1, optionSteps.Count ), prefixSteps );
		var limited = Compile( prefix + body, new() { MatchWorkLimit = prefixSteps.Count - 1 } ).Match( subject );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		Assert.Null( limited.Match );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( prefix + body, new() { WorkObserver = count => { if ( count == 4 ) cancellation.Cancel(); } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => prefixed.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( prefix + body ), cancellationToken: cancellation.Token ) );
	}

	private static void AssertLiteral( PcreRegex8Bit reference, byte[] subject, string spans ) {
		var match = reference.Match( subject );
		Assert.Equal( spans.Length > 0, match.Success );
		if ( !match.Success ) { return; }
		var groups = spans.Split( ',' );
		Assert.Equal( groups.Length, match.Groups.Count );
		for ( var group = 0; group < groups.Length; group++ ) {
			var capture = match.Groups[group];
			Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
		}
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, PcreByteOptions options ) => Reference( Encoding.UTF8.GetBytes( pattern ), options );
	private static PcreRegex8Bit Reference( byte[] pattern, PcreByteOptions options ) => new( pattern, Encoding.UTF8, new PcreRegexSettings {
		Options = PcreOptions.Compiled | ( options.Caseless ? PcreOptions.Caseless : PcreOptions.None ) | ( options.Utf8 || options.MatchInvalidUtf ? PcreOptions.Utf : PcreOptions.None )
			| ( options.Ucp ? PcreOptions.Ucp : PcreOptions.None ) | ( options.MatchInvalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ),
		NewLine = options.AnyCrLfNewline ? PcreNewLine.AnyCrLf : PcreNewLine.Lf,
		ExtraCompileOptions = options.AsciiBsD ? PcreExtraCompileOptions.AsciiBsD : PcreExtraCompileOptions.None,
		BackslashR = options.BsrAnyCrLf ? PcreBackslashR.AnyCrLf : PcreBackslashR.Unicode,
		ParensLimit = 512
	} );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
