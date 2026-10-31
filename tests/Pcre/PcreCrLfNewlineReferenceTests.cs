using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Only complete CRLF pairs are newlines; VM width, end caps and byte-prefix entry remain native.</summary>
public sealed class PcreCrLfNewlineReferenceTests {
	[Theory]
	[InlineData( @"(*CRLF)", "", "0:0" )]
	[InlineData( @"(*CRLF)\A(.)\z", "0D", "0:1,0:1" )]
	[InlineData( @"(*CRLF)\A(.)\z", "0A", "0:1,0:1" )]
	[InlineData( @"(*CRLF)\A..\z", "0D0A", "" )]
	[InlineData( @"(*CRLF)(?s)\A(..)\z", "0D0A", "0:2,0:2" )]
	[InlineData( @"(*CRLF)(?s)\A\N.\z", "0D0A", "" )]
	[InlineData( @"(*CRLF)\A(\R)\z", "0D", "0:1,0:1" )]
	[InlineData( @"(*CRLF)\A(\R)\z", "0A", "0:1,0:1" )]
	[InlineData( @"(*CRLF)\A(\R)\z", "0D0A", "0:2,0:2" )]
	[InlineData( @"(*CRLF)(?m)^(X)", "0D0A58", "2:1,2:1" )]
	[InlineData( @"(*CRLF)(?m)^X", "0D58", "" )]
	[InlineData( @"(*CRLF)(?m)^X", "0A58", "" )]
	[InlineData( @"(*CRLF)X$", "580D0A", "0:1" )]
	[InlineData( @"(*CRLF)X$", "580D", "" )]
	[InlineData( @"(*CRLF)X$", "580A", "" )]
	[InlineData( @"(*CRLF)X\Z", "580D0A", "0:1" )]
	[InlineData( @"(*CRLF)X\Z", "580D", "" )]
	[InlineData( @"(*CRLF)X\z", "580D0A", "" )]
	[InlineData( @"(*CRLF)(?m)\r$", "0D0A", "" )]
	[InlineData( @"(*CRLF)(?m)^$", "0D0A", "0:0" )]
	[InlineData( @"(*CRLF)(?<=.)\n", "0D0A", "" )]
	[InlineData( @"(*CRLF)(?<=.{1,2})\n", "0D0A", "1:1" )]
	[InlineData( @"(*CRLF)(?<*.)\n", "0D0A", "" )]
	[InlineData( @"(*CRLF)(?<=(.{1,2}))\n", "0D0A", "1:1,0:1" )]
	[InlineData( @"(*CRLF)(?=(.))\n", "0D0A", "1:1,1:1" )]
	[InlineData( @"(*CRLF)(?(?=\r\n)(\R)|.)", "0D0A", "0:2,0:2" )]
	[InlineData( @"(*CRLF)(?<x>.)(?&x)", "0D58", "0:2,0:1" )]
	[InlineData( @"(*CRLF)(?<x>.)(?&x)", "0D0A", "" )]
	[InlineData( @"(*CRLF)(*NOTEMPTY_ATSTART)", "0D0A58", "2:0" )]
	[InlineData( @"(*CRLF)(*BSR_UNICODE)\A\R\z", "0B", "0:1" )]
	[InlineData( @"(*CRLF)(*BSR_ANYCRLF)\A\R\z", "0B", "" )]
	[InlineData( @"(*CRLF)(*LF)(?^)\A(.)\z", "0D", "0:1,0:1" )]
	[InlineData( @"(*CRLF)(*CR)\A(.)\z", "0A", "0:1,0:1" )]
	[InlineData( @"(*CRLF)(*NUL)\A(.)\z", "0D", "0:1,0:1" )]
	[InlineData( @"(*CRLF)(*UTF8)(*UCP)(?<=(é.))(X)", "C3A90D58", "3:1,0:3,3:1" )]
	[InlineData( @"(*LF)(*CR)(*NUL)(*CRLF)\A(.)\z", "0A", "0:1,0:1" )]
	[InlineData( @"\Q(*CRLF)\E", "282A43524C4629", "0:7" )]
	[InlineData( @"[(*CRLF)]", "43", "0:1" )]
	public void PairSemanticsHavePinnedLiteralCaptureSpans( string pattern, string hex, string spans ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, AnyCrLfNewline = true };
			var subject = Convert.FromHexString( hex );
			var reference = Reference( pattern, options );
			AssertLiteral( reference, subject, spans );
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( @"(*CRLF).A", "1:2", "" )]
	[InlineData( @"(*CRLF)(.A)", "1:2,1:2", "" )]
	[InlineData( @"(*CRLF)(?:.A)", "1:2", "" )]
	[InlineData( @"(*CRLF)(.)A", "1:2,1:1", "" )]
	[InlineData( @"(*CRLF)(?:.)A", "1:2", "" )]
	[InlineData( @"(*CRLF)(?s).A", "1:2", "" )]
	[InlineData( @"(*CRLF)\NA", "1:2", "" )]
	public void OnlyBytePrefixScanningEntersTheLfOfPairFromZero( string pattern, string byteSpans, string utfSpans ) {
		var subject = "\r\nA"u8.ToArray();
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var reference = Reference( pattern, options );
			AssertLiteral( reference, subject, utf ? utfSpans : byteSpans );
			var compiled = Compile( pattern, options );
			Compare( reference, subject, 0, compiled.Match( subject ), pattern );
			Compare( reference, subject, 1, compiled.Match( subject, 1 ), pattern );
		}
	}

	[Theory]
	[InlineData( "282A43524C4629283F782923780D0A285829", "58", "0:1,0:1" )]
	[InlineData( "282A43524C4629283F782923780D285829", "58", "0:0" )]
	[InlineData( "282A43524C4629283F782923780A285829", "58", "0:0" )]
	[InlineData( "282A43524C4629283F7829237800285829", "58", "0:0" )]
	[InlineData( "282A43524C4629283F78782923780D0A5B58205D", "58", "0:1" )]
	[InlineData( "282A43524C46295C5123780D0A5C45", "23780D0A", "0:4" )]
	public void OnlyCompletePairsTerminateExtendedComments( string patternHex, string hex, string spans ) {
		var pattern = Convert.FromHexString( patternHex );
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var subject = Convert.FromHexString( hex );
			var reference = Reference( pattern, options );
			AssertLiteral( reference, subject, spans );
			var compiled = PcreByteCompiler.Compile( pattern, options );
			Assert.True( compiled.IsSuccess, compiled.Diagnostic?.Message );
			Compare( reference, subject, 0, compiled.Pattern!.Match( subject ), patternHex );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void DotAndNonNewlineDistinguishLoneCharactersAndPairsAcrossMembership( bool utf ) {
		var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
		foreach ( var body in new[] { @".", @"(?s).", @"\N", @"(?s)\N" } ) {
			var lone = "(*CRLF)" + @"\A" + body + @"\z";
			var pair = "(*CRLF)" + @"\A" + body + @"\n\z";
			var referenceLone = Reference( lone, options );
			var referencePair = Reference( pair, options );
			var compiledLone = Compile( lone, options );
			var compiledPair = Compile( pair, options );
			foreach ( var scalar in Enumerable.Range( 0, 256 ).Concat( utf
				? new[] { 0x301, 0x661, 0x180E, 0x2028, 0x2029, 0x1F600, 0x10FFFF } : Array.Empty<int>() ) ) {
				var subject = utf ? Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) ) : new[] { (byte)scalar };
				Assert.True( referenceLone.Match( subject ).Success );
				Compare( referenceLone, subject, 0, compiledLone.Match( subject ), $"{lone}, U+{scalar:X}" );
				subject = subject.Append( (byte)'\n' ).ToArray();
				Assert.Equal( scalar != '\r' || body == "(?s).", referencePair.Match( subject ).Success );
				Compare( referencePair, subject, 0, compiledPair.Match( subject ), $"{pair}, U+{scalar:X}" );
			}
		}
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void EveryOffsetRetainsPairsOriginalStartAndInvalidUtfFragments( bool utf, bool callerAnyCrLf ) {
		var options = new PcreByteOptions { MatchInvalidUtf = utf, Ucp = utf, AnyCrLfNewline = callerAnyCrLf };
		foreach ( var body in new[] { @".", @"\N", @".A", @"(?s).A", @"\NA", @"(.)A", @"(?m)^.|.$", @".\Z", @"\G.", @"(?<=.)\n", @"(?<=.{1,2})\n", @"(\R)?A", @"(*NOTEMPTY_ATSTART)|A" } ) {
			var pattern = "(*CRLF)" + body;
			var reference = Reference( pattern, options );
			var compiled = Compile( pattern, options );
			foreach ( var hex in new[] { "0D0A41", "410D0A", "0D41", "0A41", "FF0D0A41", "0DFF0A41", "C3A90D0A41", "" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, UTF={utf}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"a(*CRLF)", 1 )]
	[InlineData( @"(?i)(*CRLF)", 4 )]
	[InlineData( @" (*CRLF)", 1 )]
	[InlineData( @"(?x) (*CRLF)", 5 )]
	[InlineData( @"(?:(*CRLF))", 3 )]
	[InlineData( @"(*CRLF)(?:(*CRLF))", 10 )]
	[InlineData( @"(*crlf)", 0 )]
	[InlineData( @"(*CRLF:x)", 0 )]
	[InlineData( @"(*CRLFx)", 0 )]
	public void OnlyExactPrefixGrammarIsAccepted( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"(*CRLF)*", 7 )]
	[InlineData( @"(*CRLF){0}", 7 )]
	[InlineData( @"(*CRLF", 1 )]
	[InlineData( @"(*CRLF)(", 7 )]
	public void QuantifiersAndUnclosedHeadersRetainOriginalBytes( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void StrictValidationAndNulBytesRetainWholeSourceAndSubjectBoundaries() {
		var pattern = Convert.FromHexString( "282A43524C4629282A5554462900FF" );
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( pattern );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, result.Diagnostic?.Code );
		Assert.Equal( 14, result.Diagnostic?.ByteOffset );
		var compiled = Compile( "(*CRLF)(*UTF).", new() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( new byte[] { 0xFF } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( "é"u8.ToArray(), 1 ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void SharedAnyWorkAndCancellationStayBounded( bool utf ) {
		var plain = new List<int>();
		var paired = new List<int>();
		Assert.NotNull( Compile( ".", new() { Utf8 = utf, WorkObserver = plain.Add } ).Match( "X"u8.ToArray() ).Match );
		Assert.NotNull( Compile( "(*CRLF).", new() { Utf8 = utf, WorkObserver = paired.Add } ).Match( "X"u8.ToArray() ).Match );
		Assert.Equal( plain, paired );
		Assert.Equal( Enumerable.Range( 1, paired.Count ), paired );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( "(*CRLF).", new() { Utf8 = utf, MatchWorkLimit = 1 } ).Match( "\r\n"u8.ToArray() ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "(*CRLF).", new() { Utf8 = utf,
			WorkObserver = count => { if ( count == 2 ) cancellation.Cancel(); } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( "\r\n"u8.ToArray(), cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( "(*CRLF)"u8.ToArray(), cancellationToken: cancellation.Token ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit,
			PcreByteCompiler.Compile( "(*CRLF)"u8.ToArray(), new() { MatchWorkLimit = 0 } ).Diagnostic?.Code );
	}

	[Fact]
	public void LongMixedPrefixUsesLastPairSettingAndPreservesBsrAndUtf() {
		var prefix = String.Concat( Enumerable.Repeat( "(*CRLF)(*NUL)", 1500 ) );
		var pattern = prefix + @"(*UTF8)(*UCP)(*BSR_UNICODE)(*CRLF)(?^)\A(\R)(.)\z";
		var subject = "\v\r"u8.ToArray();
		var reference = Reference( pattern, new() );
		Compare( reference, subject, 0, Compile( pattern, new() ).Match( subject ), "long prefix" );
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
