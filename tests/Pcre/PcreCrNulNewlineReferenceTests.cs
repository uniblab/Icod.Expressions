using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>CR/NUL are single-character newline conventions, independent of BSR and UTF fragments.</summary>
public sealed class PcreCrNulNewlineReferenceTests {
	[Theory]
	[InlineData( @"(*CR)\A(.)\z", "0A", "0:1,0:1" )]
	[InlineData( @"(*CR)\A(.)\z", "0D", "" )]
	[InlineData( @"(*CR)\A(.)\z", "00", "0:1,0:1" )]
	[InlineData( @"(*NUL)\A(.)\z", "0D", "0:1,0:1" )]
	[InlineData( @"(*NUL)\A(.)\z", "00", "" )]
	[InlineData( @"(*NUL)\A(\N)\z", "0A", "0:1,0:1" )]
	[InlineData( @"(*CR)(?s)\A(.)\z", "0D", "0:1,0:1" )]
	[InlineData( @"(*NUL)(?s)\A(.)\z", "00", "0:1,0:1" )]
	[InlineData( @"(*CR)(?s)\A\N\z", "0D", "" )]
	[InlineData( @"(*NUL)(?s)\A\N\z", "00", "" )]
	[InlineData( @"(*CR)(?m)^(X)$", "0D580D", "1:1,1:1" )]
	[InlineData( @"(*NUL)(?m)^(X)$", "005800", "1:1,1:1" )]
	[InlineData( @"(*CR)(?m)^(.)", "0D0A", "1:1,1:1" )]
	[InlineData( @"(*NUL)(?m)^X", "0D0A58", "" )]
	[InlineData( @"(*CR)X$", "580D", "0:1" )]
	[InlineData( @"(*NUL)X\Z", "5800", "0:1" )]
	[InlineData( @"(*CR)X\z", "580D", "" )]
	[InlineData( @"(*NUL)X\z", "5800", "" )]
	[InlineData( @"(*CR)\A\R\z", "0D0A", "0:2" )]
	[InlineData( @"(*NUL)\A\R\z", "00", "" )]
	[InlineData( @"(*NUL)(*BSR_UNICODE)\A(\R)\z", "0B", "0:1,0:1" )]
	[InlineData( @"(*CR)(*BSR_ANYCRLF)\A\R\z", "0B", "" )]
	[InlineData( @"(*CR)(?<=X$)(\r)", "580D", "1:1,1:1" )]
	[InlineData( @"(*NUL)(?<=X\Z)(\x00)", "5800", "1:1,1:1" )]
	[InlineData( @"(*CR)(?m)(?<=(^X))(\r)", "580D", "1:1,0:1,1:1" )]
	[InlineData( @"(*NUL)(?<x>[^\x00]+)(?&x)$", "616100", "0:2,0:1" )]
	[InlineData( @"(*LF)(*NUL)(*CR)\A(.)\z", "0A", "0:1,0:1" )]
	[InlineData( @"(*CR)(*ANYCRLF)(*NUL)(?^)\A(.)\z", "0D", "0:1,0:1" )]
	[InlineData( @"(*NUL)(*LF)\A(.)\z", "00", "0:1,0:1" )]
	[InlineData( @"(*CR)(*ANYCRLF)\A.\z", "0D", "" )]
	[InlineData( @"(*NUL)(*NOTEMPTY_ATSTART)|X", "0058", "1:0" )]
	[InlineData( @"(*CR)(*NOTEMPTY_ATSTART)|X", "0D0A58", "1:0" )]
	[InlineData( @"\Q(*NUL)\E", "282A4E554C29", "0:6" )]
	[InlineData( @"[(*CR)]", "43", "0:1" )]
	public void ConventionsHavePinnedLiteralCaptureSpans( string pattern, string hex, string spans ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, AnyCrLfNewline = true };
			var subject = Convert.FromHexString( hex );
			var reference = Reference( pattern, options );
			AssertLiteral( reference, subject, spans );
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( "282A4E554C29283F7829236100285829", "58", "0:1,0:1" )]
	[InlineData( "282A4E554C29283F782923610A285829", "58", "0:0" )]
	[InlineData( "282A435229283F782923610D285829", "58", "0:1,0:1" )]
	[InlineData( "282A435229283F7829236100285829", "58", "0:0" )]
	[InlineData( "282A435229283F78782923610D5B58205D", "58", "0:1" )]
	[InlineData( "282A4E554C29283F7878292361005B58205D", "58", "0:1" )]
	[InlineData( "282A4E554C295C512378005C45", "237800", "0:3" )]
	[InlineData( "282A4E554C295C412800295C7A", "00", "0:1,0:1" )]
	public void RawNulAndCommentTerminatorsRetainOriginalSourceBytes( string patternHex, string hex, string spans ) {
		var bytes = Convert.FromHexString( patternHex );
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var subject = Convert.FromHexString( hex );
			var reference = Reference( bytes, options );
			AssertLiteral( reference, subject, spans );
			var actual = PcreByteCompiler.Compile( bytes, options );
			Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
			Compare( reference, subject, 0, actual.Pattern!.Match( subject ), patternHex );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void AllByteValuesKeepDotNonNewlineAndBsrIndependent( bool utf ) {
		foreach ( var (prefix, newline) in new[] { ("(*CR)", 13), ("(*NUL)", 0) } ) {
			foreach ( var body in new[] { @".", @"(?s)\N", @"\R" } ) {
				var pattern = prefix + @"\A" + body + @"\z";
				var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
				var reference = Reference( pattern, options );
				var compiled = Compile( pattern, options );
				for ( var value = 0; value <= 255; value++ ) {
					var subject = utf ? Encoding.UTF8.GetBytes( char.ConvertFromUtf32( value ) ) : new[] { (byte)value };
					var expected = body == @"\R" ? value is 10 or 11 or 12 or 13 or 0x85 : value != newline;
					Assert.Equal( expected, reference.Match( subject ).Success );
					Compare( reference, subject, 0, compiled.Match( subject ), $"{pattern}, {value:X2}" );
				}
				foreach ( var value in new[] { 0x301, 0x661, 0x180E, 0x2028, 0x2029, 0x1F600, 0x10FFFF } ) {
					if ( !utf ) { continue; }
					var subject = Encoding.UTF8.GetBytes( char.ConvertFromUtf32( value ) );
					Assert.Equal( body != @"\R" || value is 0x2028 or 0x2029, reference.Match( subject ).Success );
					Compare( reference, subject, 0, compiled.Match( subject ), $"{pattern}, U+{value:X}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void EveryCallerOffsetRetainsCrlfNulAndInvalidUtfProtocols( bool utf, bool callerAnyCrLf ) {
		var options = new PcreByteOptions { MatchInvalidUtf = utf, Ucp = utf, AnyCrLfNewline = callerAnyCrLf };
		foreach ( var prefix in new[] { "(*CR)", "(*NUL)" } ) {
			foreach ( var body in new[] { @".", @"\N", @"(?m)^.|.$", @".\Z", @"\G.", @"(?<=.)X", @"(\R)?X", @"(*NOTEMPTY_ATSTART)|X" } ) {
				var pattern = prefix + body;
				var reference = Reference( pattern, options );
				var compiled = Compile( pattern, options );
				foreach ( var hex in new[] { "0058", "0D0A58", "58000D", "FF0058FF", "C3A90058", "" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, UTF={utf}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( @"a(*CR)", 1 )]
	[InlineData( @"(?i)(*NUL)", 4 )]
	[InlineData( @" (*CR)", 1 )]
	[InlineData( @"(?x) (*NUL)", 5 )]
	[InlineData( @"(?:(*CR))", 3 )]
	[InlineData( @"(*CR)(?:(*NUL))", 8 )]
	[InlineData( @"(*cr)", 0 )]
	[InlineData( @"(*Nul)", 0 )]
	[InlineData( @"(*CR:x)", 0 )]
	[InlineData( @"(*NULx)", 0 )]
	public void OnlyExactPrefixDirectivesAreAccepted( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"(*CR)*", 5 )]
	[InlineData( @"(*NUL){0}", 6 )]
	[InlineData( @"(*CR", 1 )]
	[InlineData( @"(*NUL", 1 )]
	public void DirectQuantifiersAndIncompleteHeadersStayInvalid( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void NulDoesNotEndWholeSourceUtfValidationAndSubjectValidationRemainsStrict() {
		var bytes = Convert.FromHexString( "282A4E554C29282A5554462900FF" );
		Assert.Throws<PcrePatternException>( () => Reference( bytes, new() ) );
		var result = PcreByteCompiler.Compile( bytes );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, result.Diagnostic?.Code );
		Assert.Equal( 13, result.Diagnostic?.ByteOffset );
		var compiled = Compile( "(*CR)(*UTF).", new() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( new byte[] { 0xFF } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( "é"u8.ToArray(), 1 ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void NonNewlineWorkAndCancellationStayShared( bool utf ) {
		var a = new List<int>();
		var b = new List<int>();
		Assert.NotNull( Compile( "X", new() { Utf8 = utf, WorkObserver = a.Add } ).Match( "X"u8.ToArray() ).Match );
		Assert.NotNull( Compile( "(*NUL)(*CR)X", new() { Utf8 = utf, WorkObserver = b.Add } ).Match( "X"u8.ToArray() ).Match );
		Assert.Equal( a, b );
		Assert.Equal( Enumerable.Range( 1, b.Count ), b );
		var limited = Compile( "(*CR).", new() { Utf8 = utf, MatchWorkLimit = 1 } ).Match( "X"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "(*NUL).", new() { Utf8 = utf,
			WorkObserver = count => { if ( count == 2 ) cancellation.Cancel(); } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( "X"u8.ToArray(), cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( "(*CR)"u8.ToArray(), cancellationToken: cancellation.Token ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit,
			PcreByteCompiler.Compile( "(*NUL)"u8.ToArray(), new() { MatchWorkLimit = 0 } ).Diagnostic?.Code );
	}

	[Fact]
	public void LongMixedPrefixesKeepTheLastConventionAndExistingFlags() {
		var prefix = String.Concat( Enumerable.Repeat( "(*CR)(*NUL)", 1500 ) );
		var pattern = prefix + @"(*UTF)(*UCP)(*BSR_UNICODE)(*NOTEMPTY)(?^)\A(\R)(.)\z";
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
