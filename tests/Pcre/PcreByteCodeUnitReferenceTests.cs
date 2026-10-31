using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Byte C is one arbitrary code unit, independent of character modes.</summary>
public sealed class PcreByteCodeUnitReferenceTests {
	[Theory]
	[InlineData( @"\C", "", "" )]
	[InlineData( @"\A(\C)\z", "00", "0:1,0:1" )]
	[InlineData( @"\A(\C)\z", "FF", "0:1,0:1" )]
	[InlineData( @"\A(\C)\z", "0A", "0:1,0:1" )]
	[InlineData( @"\A(\C)\z", "0D", "0:1,0:1" )]
	[InlineData( @"\A(\C)\z", "85", "0:1,0:1" )]
	[InlineData( @"\C", "C3A9", "0:1" )]
	[InlineData( @"\A(\C{2})\z", "C3A9", "0:2,0:2" )]
	[InlineData( @"\A(\C{3})\z", "EDA080", "0:3,0:3" )]
	[InlineData( @"(?i)\A(\C)(\cC)\z", "FF03", "0:2,0:1,1:1" )]
	[InlineData( @"(*UCP)(?ia)\A(\C)\z", "E9", "0:1,0:1" )]
	[InlineData( @"(?-s)\A(\C)\z", "0A", "0:1,0:1" )]
	[InlineData( @"(*ANY)(*BSR_ANYCRLF)\A(\C)\z", "85", "0:1,0:1" )]
	[InlineData( @"(*CRLF)\A(\C)(\C)\z", "0D0A", "0:2,0:1,1:1" )]
	[InlineData( @"\A(\C{1,3}?)\C\z", "FF0085", "0:3,0:2" )]
	[InlineData( @"\A(\C+)\1\z", "FF0AFF0A", "0:4,0:2" )]
	[InlineData( @"\A(\C++)X\z", "FF58", "" )]
	[InlineData( @"\A(?:(\C+)X|(\C+))\z", "FF00", "0:2,-,0:2" )]
	[InlineData( @"(?*(\C+))\1X", "FFFF58", "0:3,0:2" )]
	[InlineData( @"(?=(\C{2}))\1", "00FF", "0:2,0:2" )]
	[InlineData( @"(?<=(\C))X", "FF58", "1:1,0:1" )]
	[InlineData( @"(?<=\C{1,2})X", "00FF58", "2:1" )]
	[InlineData( @"(?<!\C)X", "58", "0:1" )]
	[InlineData( @"(?<!\C)X", "FF58", "" )]
	[InlineData( @"\A(?&n)\z(?(DEFINE)(?<n>\C))", "FF", "0:1,-" )]
	[InlineData( @"\A(?<n>\C(?&n)?X)\z", "00FF5858", "0:4,0:4" )]
	[InlineData( @"\A(?|(\C)X|(\C))\1\z", "FFFF", "0:2,0:1" )]
	[InlineData( @"\A(?:(\C)|X)(?(1)\C|X)\z", "FF00", "0:2,0:1" )]
	[InlineData( @"\A\C\K(\C)\z", "FF00", "1:1,1:1" )]
	[InlineData( @"\A\Q\C\E\z", "5C43", "0:2" )]
	[InlineData( @"\A[\Q\C\E]+\z", "5C43", "0:2" )]
	[InlineData( @"(*ANYCRLF)\CA", "0D0A41", "1:2" )]
	[InlineData( @"(*ANYCRLF)(\C)A", "0D0A41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)(?:\C)(A)", "0D0A41", "1:2,2:1" )]
	[InlineData( @"(*ANYCRLF)(?:(\C)A)", "0D0A41", "1:2,1:1" )]
	[InlineData( @"(*ANYCRLF)\C\n", "0D0A", "0:2" )]
	public void NativeByteSpansAndSharedCaptureTransactionsArePreserved( string pattern, string hex, string spans ) {
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern );
		AssertLiteral( reference, subject, spans );
		Compare( reference, subject, 0, Compile( pattern ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( "(*LF)" )]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CR)" )]
	[InlineData( "(*NUL)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void All256BytesIgnoreNewlineDotCaseAsciiAndUcpModes( string newline ) {
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var header in new[] { "", "(?i)", "(?s)", "(?-s)", "(?irxa)" } ) {
				var pattern = String.Concat( newline, header, @"\A(\C)\z" );
				var reference = Reference( pattern, ucp );
				var compiled = Compile( pattern, new() { Ucp = ucp } );
				for ( var value = 0; value < 256; value++ ) {
					var subject = new[] { (byte)value };
					AssertLiteral( reference, subject, "0:1,0:1" );
					Compare( reference, subject, 0, compiled.Match( subject ), $"{pattern}/{value}/{ucp}" );
				}
				Assert.Null( compiled.Match( Array.Empty<byte>() ).Match );
				Assert.Null( compiled.Match( new byte[] { 0xC3, 0xA9 } ).Match );
			}
		}
	}

	[Theory]
	[InlineData( "(*LF)" )]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CR)" )]
	[InlineData( "(*NUL)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void EveryRequestedByteOffsetUsesNativeAnyBytePrefixAndWidth( string newline ) {
		string[] bodies = { @"\C", @"\CA", @"(\C)A", @"(?:(\C)A)", @"\C\n", @"\G(\C)",
			@"(?<=(\C))A", @"(?<=\C{1,2})A", @"(\C?)|A", @"\C{2}", @"\C\K(\C)" };
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var body in bodies ) {
				var pattern = newline + body;
				var reference = Reference( pattern, ucp );
				var compiled = Compile( pattern, new() { Ucp = ucp } );
				foreach ( var hex in new[] { "", "00", "0D0A410D0A", "FF8541", "C3A941", "EDA08041", "410D0A", "00FF41" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}/{ucp}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( @"[\C]", 1 )]
	[InlineData( @"[a-\C]", 3 )]
	[InlineData( @"x[\C]", 2 )]
	[InlineData( @"(*UCP)[\C]", 7 )]
	public void BracketUseKeepsNativeInvalidSyntaxAtTheEscape( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		foreach ( var options in new[] { new PcreByteOptions(), new() { Ucp = true }, new() { Utf8 = true }, new() { MatchInvalidUtf = true } } ) {
			Diagnostic( Encoding.ASCII.GetBytes( pattern ), PcreByteDiagnosticCode.InvalidSyntax, offset, options );
		}
	}

	[Theory]
	[InlineData( @"\C", 0 )]
	[InlineData( @"(*UTF)\C", 6 )]
	[InlineData( @"(*UTF8)\C", 7 )]
	[InlineData( @"(*UCP)(*UTF)\C", 12 )]
	[InlineData( @"(?=\C)", 3 )]
	[InlineData( @"(?<=\C)X", 0 )]
	[InlineData( @"x(?<!\C)X", 1 )]
	public void UtfCodeUnitSemanticsRemainExplicitlyPending( string pattern, int offset ) {
		foreach ( var options in new[] { new PcreByteOptions { Utf8 = true }, new() { MatchInvalidUtf = true }, new() { Utf8 = true, Ucp = true } } ) {
			Diagnostic( Encoding.ASCII.GetBytes( pattern ), PcreByteDiagnosticCode.UnsupportedSyntax, offset, options );
		}
		if ( pattern.StartsWith( "(*", StringComparison.Ordinal ) ) {
			Diagnostic( Encoding.ASCII.GetBytes( pattern ), PcreByteDiagnosticCode.UnsupportedSyntax, offset );
		}
	}

	[Fact]
	public void ExtendedCommentsLowercaseControlAndQuotingKeepTheirLexicalMeaning() {
		const string pattern = "(?x)\\A(\\C) # \\C ignored\n(\\cC)(?#\\C)\\z";
		var subject = new byte[] { 0xFF, 3 };
		var reference = Reference( pattern );
		AssertLiteral( reference, subject, "0:2,0:1,1:1" );
		Compare( reference, subject, 0, Compile( pattern ).Match( subject ), pattern );
		var quoted = Compile( @"\A\\C\z" );
		Assert.Equal( 2, quoted.Match( "\\C"u8.ToArray() ).Match?.ByteLength );
		Assert.Null( quoted.Match( new byte[] { 0xFF } ).Match );
	}

	[Fact]
	public void ExistingValidationPrecedenceAndByteStartBoundsStayIntact() {
		Diagnostic( new byte[] { (byte)'\\', (byte)'C', 0xFF }, PcreByteDiagnosticCode.InvalidUtf8Pattern, 2, new() { Utf8 = true } );
		Diagnostic( "\\C"u8.ToArray(), PcreByteDiagnosticCode.InvalidWorkLimit, 0, new() { MatchWorkLimit = 0 } );
		var compiled = Compile( @"\C" );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( new byte[] { 0xFF }, -1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( new byte[] { 0xFF }, 2 ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( "\\C"u8.ToArray(),
			new() { MatchWorkLimit = 0 }, cancellation.Token ) );
	}

	[Fact]
	public void MatchWorkBudgetsAndCancellationReuseTheAnyBytePath() {
		const string raw = @"\A(\C+)\1\z";
		const string dot = @"(?s)\A(.+)\1\z";
		var subject = new byte[] { 0xFF, 10, 0xFF, 10 };
		var rawSteps = new List<int>();
		var dotSteps = new List<int>();
		Assert.Equal( 4, Compile( raw, new() { WorkObserver = rawSteps.Add } ).Match( subject ).Match?.ByteLength );
		Assert.Equal( 4, Compile( dot, new() { WorkObserver = dotSteps.Add } ).Match( subject ).Match?.ByteLength );
		Assert.Equal( dotSteps, rawSteps );
		Assert.Equal( Enumerable.Range( 1, rawSteps.Count ), rawSteps );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( raw, new() { MatchWorkLimit = rawSteps.Count - 1 } ).Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( raw, new() { WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void CompileWidthAndSavedPathLimitsRemainShared() {
		Assert.Equal( 1, Compile( @"(?<=\C{65535}){0}X" ).Match( "X"u8.ToArray() ).Match?.ByteLength );
		Diagnostic( Encoding.ASCII.GetBytes( @"(?<=\C{1,256})X" ), PcreByteDiagnosticCode.UnsupportedSyntax, 0 );
		var depth = String.Concat( new string( '(', 257 ), @"\C", new string( ')', 257 ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( depth ) ).Diagnostic?.Code );
		var captures = String.Concat( Enumerable.Repeat( @"(\C)", 257 ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternCaptureLimitExceeded, PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( captures ) ).Diagnostic?.Code );
		var expansion = String.Concat( Enumerable.Repeat( @"\C", 4100 ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( expansion ) ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, Compile( @"\A(?<n>\C(?&n)?X)\z" )
			.Match( Encoding.ASCII.GetBytes( new string( 'a', 1200 ) + new string( 'X', 1200 ) ) ).Diagnostic?.Code );
	}

	private static void Diagnostic( byte[] source, PcreByteDiagnosticCode code, int offset, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( source, options );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
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

	private static PcreRegex8Bit Reference( string pattern, bool ucp = false ) => new( Encoding.ASCII.GetBytes( pattern ), Encoding.Latin1,
		new PcreRegexSettings { Options = PcreOptions.Compiled | ( ucp ? PcreOptions.Ucp : PcreOptions.None ), NewLine = PcreNewLine.Lf, ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
