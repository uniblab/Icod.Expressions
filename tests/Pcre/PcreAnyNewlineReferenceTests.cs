using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>ANY uses fixed newline scalars and native consumed-CR anchors without confusing UTF continuation bytes.</summary>
public sealed class PcreAnyNewlineReferenceTests {
	[Theory]
	[InlineData( @"(*ANY)", "", "0:0", "0:0" )]
	[InlineData( @"(*ANY)\A(.)\z", "00", "0:1,0:1", "0:1,0:1" )]
	[InlineData( @"(*ANY)\A.\z", "0A", "", "" )]
	[InlineData( @"(*ANY)\A.\z", "0B", "", "" )]
	[InlineData( @"(*ANY)\A.\z", "0C", "", "" )]
	[InlineData( @"(*ANY)\A.\z", "0D", "", "" )]
	[InlineData( @"(*ANY).", "C285", "0:1", "" )]
	[InlineData( @"(*ANY).", "E280A8", "0:1", "" )]
	[InlineData( @"(*ANY)(?s)\A(.)\z", "C285", "", "0:2,0:2" )]
	[InlineData( @"(*ANY)\A\N\z", "C485", "", "0:2" )]
	[InlineData( @"(*ANY)(?s)\A\N\z", "E280A8", "", "" )]
	[InlineData( @"(*ANY)(?m)^X", "C28558", "2:1", "2:1" )]
	[InlineData( @"(*ANY)(?m)^X", "E280A858", "", "3:1" )]
	[InlineData( @"(*ANY)(?m)^X", "E280A958", "", "3:1" )]
	[InlineData( @"(*ANY)(?m)^X", "C48558", "2:1", "" )]
	[InlineData( @"(*ANY)X$", "58C285", "", "0:1" )]
	[InlineData( @"(*ANY)X\Z", "58E280A8", "", "0:1" )]
	[InlineData( @"(*ANY)X\Z", "580D0A", "0:1", "0:1" )]
	[InlineData( @"(*ANY)X\z", "580D0A", "", "" )]
	[InlineData( @"(*ANY)\r(?m:^)", "0D0A", "0:1", "0:1" )]
	[InlineData( @"(*ANY)\r$", "0D0A", "0:1", "0:1" )]
	[InlineData( @"(*ANY)\r\Z", "0D0A", "0:1", "0:1" )]
	[InlineData( @"(*ANY)(?m)^$", "0D0A", "0:0", "0:0" )]
	[InlineData( @"(*ANY)\A(\R)\z", "C285", "", "0:2,0:2" )]
	[InlineData( @"(*ANY)(*BSR_ANYCRLF)\A\R\z", "C285", "", "" )]
	[InlineData( @"(*ANY)(*BSR_UNICODE)\A(\R)\z", "0B", "0:1,0:1", "0:1,0:1" )]
	[InlineData( @"(*ANY).A", "0D0A41", "", "" )]
	[InlineData( @"(*ANY)(?s).A", "0D0A41", "1:2", "" )]
	[InlineData( @"(*ANY)(?s:(.))A", "0D0A41", "1:2,1:1", "" )]
	[InlineData( @"(*ANY)\NA", "0D0A41", "", "" )]
	[InlineData( @"(*ANY)(?s)\NA", "0D0A41", "", "" )]
	[InlineData( @"(*ANY)(?<=.)\n", "0D0A", "", "" )]
	[InlineData( @"(*ANY)(?<=.{1,2})\n", "0D0A", "", "" )]
	[InlineData( @"(*ANY)(?<=X$)(\x{85})", "58C285", "", "1:2,1:2" )]
	[InlineData( @"(*ANY)(?m)(?<=(^X))(\R)", "C285580A", "3:1,2:1,3:1", "3:1,2:1,3:1" )]
	[InlineData( @"(*ANY)(?<x>[^X])(?&x)$", "6162C285", "1:2,1:1", "0:2,0:1" )]
	[InlineData( @"(*ANY)(*NOTEMPTY_ATSTART)", "0D0A58", "2:0", "2:0" )]
	[InlineData( @"(*ANY)(*LF)\A(.)\z", "0B", "0:1,0:1", "0:1,0:1" )]
	[InlineData( @"(*ANY)(*CRLF)\A(.)\z", "0A", "0:1,0:1", "0:1,0:1" )]
	[InlineData( @"(*ANY)(*NUL)\A(.)\z", "0D", "0:1,0:1", "0:1,0:1" )]
	[InlineData( @"(*ANY)(*ANYCRLF)\A(.)\z", "C285", "", "0:2,0:2" )]
	[InlineData( @"(*ANY)(*UTF8)(*UCP)\A(\p{Zl})\z", "E280A8", "0:3,0:3", "0:3,0:3" )]
	[InlineData( @"(*LF)(*CRLF)(*NUL)(*ANY)\A.\z", "0B", "", "" )]
	[InlineData( @"(*ANYCRLF)\r(?m:^)", "0D0A", "0:1", "0:1" )]
	[InlineData( @"(*ANYCRLF)\r$", "0D0A", "0:1", "0:1" )]
	[InlineData( @"(*ANYCRLF)\r\Z", "0D0A", "0:1", "0:1" )]
	[InlineData( @"\Q(*ANY)\E", "282A414E5929", "0:6", "0:6" )]
	[InlineData( @"[(*ANY)]", "41", "0:1", "0:1" )]
	public void AnyAndConsumedCrAnchorsHavePinnedCaptureSpans( string pattern, string hex, string byteSpans, string utfSpans ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, AnyCrLfNewline = true };
			var reference = Reference( pattern, options );
			AssertLiteral( reference, subject, utf ? utfSpans : byteSpans );
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( "282A414E5929283F78292378C285285829", "58", "0:1,0:1", "0:1,0:1" )]
	[InlineData( "282A414E5929283F78292378C485285829", "58", "0:1,0:1", "0:0" )]
	[InlineData( "282A414E5929283F78292378E280A8285829", "58", "0:0", "0:1,0:1" )]
	[InlineData( "282A414E5929283F78292378E280A9285829", "58", "0:0", "0:1,0:1" )]
	[InlineData( "282A414E5929283F7829237800285829", "58", "0:0", "0:0" )]
	[InlineData( "282A414E5929283F782923780D0A285829", "58", "0:1,0:1", "0:1,0:1" )]
	[InlineData( "282A414E5929283F7878292378E280A85B58205D", "58", "0:0", "0:1" )]
	[InlineData( "282A414E59295C512378E280A85C45", "2378E280A8", "0:5", "0:5" )]
	public void CommentScalarsAndContinuationBytesKeepWidthSpecificParsing( string patternHex, string hex, string byteSpans, string utfSpans ) {
		var bytes = Convert.FromHexString( patternHex );
		var subject = Convert.FromHexString( hex );
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var reference = Reference( bytes, options );
			AssertLiteral( reference, subject, utf ? utfSpans : byteSpans );
			var result = PcreByteCompiler.Compile( bytes, options );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Compare( reference, subject, 0, result.Pattern!.Match( subject ), patternHex );
		}
	}

	[Fact]
	public void RawByteNelEndsByteCommentsButStrictUtfPatternValidationRejectsIt() {
		var pattern = Convert.FromHexString( "282A414E5929283F7829237885285829" );
		var subject = "X"u8.ToArray();
		var reference = Reference( pattern, new() );
		AssertLiteral( reference, subject, "0:1,0:1" );
		var actual = PcreByteCompiler.Compile( pattern );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		Compare( reference, subject, 0, actual.Pattern!.Match( subject ), "byte NEL" );
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var strict = PcreByteCompiler.Compile( pattern, new() { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, strict.Diagnostic?.Code );
		Assert.Equal( 12, strict.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ByteAndUnicodeMembershipAndAnchorLengthsUseTheFixedNewlineList( bool utf ) {
		var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
		foreach ( var body in new[] { @"\A.\z", @"\A\N\z", @"(?s)\A.\z", @"\A\R\z", @"(?s:\A.)(?m:^X)\z", @"\AX\Z" } ) {
			var pattern = "(*ANY)" + body;
			var reference = Reference( pattern, options );
			var compiled = Compile( pattern, options );
			foreach ( var scalar in Enumerable.Range( 0, 256 ).Concat( utf ? new[] {
				0x301, 0x661, 0x180E, 0x200E, 0x2027, 0x2028, 0x2029, 0x202A, 0x1F600, 0x10FFFF
			} : Array.Empty<int>() ) ) {
				var subject = utf ? Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) ) : new[] { (byte)scalar };
				var newline = scalar is >= 10 and <= 13 or 0x85 or 0x2028 or 0x2029;
				var expected = body == @"(?s)\A.\z" || ( body is @"\A.\z" or @"\A\N\z" ? !newline : newline );
				if ( body == @"(?s:\A.)(?m:^X)\z" ) { subject = subject.Append( (byte)'X' ).ToArray(); }
				if ( body == @"\AX\Z" ) { subject = new byte[] { (byte)'X' }.Concat( subject ).ToArray(); }
				Assert.Equal( expected, reference.Match( subject ).Success );
				Compare( reference, subject, 0, compiled.Match( subject ), $"{pattern}, U+{scalar:X}" );
			}
		}
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void EveryCallerOffsetKeepsScalarNewlinesAndInvalidFragments( bool utf, bool callerAnyCrLf ) {
		var options = new PcreByteOptions { MatchInvalidUtf = utf, Ucp = utf, AnyCrLfNewline = callerAnyCrLf };
		foreach ( var body in new[] { @".", @"\N", @"(?s).", @".X", @"(?s).X", @"(?m)^X", @"(?m)^", @"(?m)$", @"X\Z", @"\G.", @"(?<=.)X", @"(\R)?X", @"(*NOTEMPTY_ATSTART)|X" } ) {
			var pattern = "(*ANY)" + body;
			var reference = Reference( pattern, options );
			var compiled = Compile( pattern, options );
			foreach ( var hex in new[] { "0D0A58", "C28558", "E280A858", "C48558", "FF8558", "C2FF8558", "58E280A9", "00FF58", "" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, UTF={utf}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"a(*ANY)", 1 )]
	[InlineData( @"(?i)(*ANY)", 4 )]
	[InlineData( @" (*ANY)", 1 )]
	[InlineData( @"(?x) (*ANY)", 5 )]
	[InlineData( @"(?:(*ANY))", 3 )]
	[InlineData( @"(*ANY)(?:(*ANY))", 9 )]
	[InlineData( @"(*any)", 0 )]
	[InlineData( @"(*ANY:x)", 0 )]
	[InlineData( @"(*ANYx)", 0 )]
	public void OnlyExactOriginalPrefixGrammarIsAccepted( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"(*ANY)*", 6 )]
	[InlineData( @"(*ANY){0}", 6 )]
	[InlineData( @"(*ANY", 1 )]
	[InlineData( @"(*ANY)(", 6 )]
	public void MalformedPatternsRetainOriginalByteOffsets( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void WholeSourceAndStrictSubjectValidationRemainBeforeMatching() {
		var pattern = Convert.FromHexString( "282A414E5929282A5554462900FF" );
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( pattern );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, result.Diagnostic?.Code );
		Assert.Equal( 13, result.Diagnostic?.ByteOffset );
		var compiled = Compile( "(*ANY)(*UTF).", new() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( new byte[] { 0x85 } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( "ą"u8.ToArray(), 1 ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void WorkCancellationAndLimitsStayShared( bool utf ) {
		var a = new List<int>();
		var b = new List<int>();
		Assert.NotNull( Compile( ".", new() { Utf8 = utf, WorkObserver = a.Add } ).Match( "X"u8.ToArray() ).Match );
		Assert.NotNull( Compile( "(*ANY).", new() { Utf8 = utf, WorkObserver = b.Add } ).Match( "X"u8.ToArray() ).Match );
		Assert.Equal( a, b );
		Assert.Equal( Enumerable.Range( 1, b.Count ), b );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( "(*ANY)(?m)^X", new() { Utf8 = utf, MatchWorkLimit = 1 } ).Match( "\vX"u8.ToArray() ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "(*ANY)(?m)^X", new() { Utf8 = utf,
			WorkObserver = count => { if ( count == 2 ) cancellation.Cancel(); } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( "\vX"u8.ToArray(), cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( "(*ANY)"u8.ToArray(), cancellationToken: cancellation.Token ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit,
			PcreByteCompiler.Compile( "(*ANY)"u8.ToArray(), new() { MatchWorkLimit = 0 } ).Diagnostic?.Code );
	}

	[Fact]
	public void LongPrefixKeepsFinalScalarConventionAndIndependentBsr() {
		var prefix = String.Concat( Enumerable.Repeat( "(*ANY)(*CRLF)", 1500 ) );
		var pattern = prefix + @"(*UTF8)(*UCP)(*BSR_ANYCRLF)(*ANY)(?^)\A(\R)(.)\z";
		var subject = "\r\n\0"u8.ToArray();
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
