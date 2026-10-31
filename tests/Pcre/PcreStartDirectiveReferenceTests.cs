using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins prefix-only global newline/BSR selection without rewriting source bytes.</summary>
public sealed class PcreStartDirectiveReferenceTests {
	[Theory]
	[InlineData( @"(*LF)", "", "0:0", true, false )]
	[InlineData( @"(*BSR_ANYCRLF)(*BSR_UNICODE)", "", "0:0", false, true )]
	[InlineData( @"(*LF)\A(.)\z", "0D", "0:1,0:1", true, false )]
	[InlineData( @"(*ANYCRLF)\A(.)\z", "0D", "", false, false )]
	[InlineData( @"(*LF)\A\N\z", "0D", "0:1", true, false )]
	[InlineData( @"(*ANYCRLF)\A\N\z", "0D", "", false, false )]
	[InlineData( @"(*LF)(?m)^X", "0D58", "", true, false )]
	[InlineData( @"(*ANYCRLF)(?m)^X", "0D58", "1:1", false, false )]
	[InlineData( @"(*LF)\AX$", "580D", "", true, false )]
	[InlineData( @"(*ANYCRLF)\AX$", "580D", "0:1", false, false )]
	[InlineData( @"(*LF)\AX\Z", "580D", "", true, false )]
	[InlineData( @"(*ANYCRLF)\AX\Z", "580D", "0:1", false, false )]
	[InlineData( @"(*LF)(*ANYCRLF)(?m)^X", "0D58", "1:1", false, false )]
	[InlineData( @"(*ANYCRLF)(*LF)(?m)^X", "0D58", "", true, false )]
	[InlineData( @"(*BSR_UNICODE)\A(\R)\z", "0B", "0:1,0:1", false, true )]
	[InlineData( @"(*BSR_ANYCRLF)\A\R\z", "0B", "", false, false )]
	[InlineData( @"(*BSR_ANYCRLF)(*BSR_UNICODE)\R", "0B0D", "0:1", false, true )]
	[InlineData( @"(*BSR_UNICODE)(*BSR_ANYCRLF)\R", "0B0D", "1:1", false, false )]
	[InlineData( @"(*ANYCRLF)(*LF)(*BSR_ANYCRLF)(*BSR_UNICODE)\A(\R)(.)\z", "0B0D", "0:2,0:1,1:1", true, true )]
	[InlineData( @"(*LF)(*ANYCRLF)(*BSR_UNICODE)(*BSR_ANYCRLF)\A(\R)(.)\z", "0D0B", "0:2,0:1,1:1", false, false )]
	[InlineData( @"(*LF)(*LF)(?s:(.))", "0D", "0:1,0:1", true, false )]
	[InlineData( @"(*ANYCRLF)(?^)(?m:(^X))", "0D58", "1:1,1:1", false, false )]
	[InlineData( @"(*BSR_UNICODE)(?^)(?<x>\R)(?&x)", "0B0C", "0:2,0:1", false, true )]
	[InlineData( @"(*BSR_ANYCRLF)(?<=\R{1,2})(X)", "0D0A58", "2:1,2:1", false, false )]
	[InlineData( @"(*BSR_UNICODE)(?<=\R)(X)", "0B58", "1:1,1:1", false, true )]
	[InlineData( @"(*BSR_ANYCRLF)(\R)\n|(\r)\n", "0D0A", "0:2,-,0:1", false, false )]
	[InlineData( @"(*BSR_UNICODE)\A\R?\n\z", "0D0A", "", false, true )]
	[InlineData( @"(*ANYCRLF)(*BSR_UNICODE)\R|(?m:^X)", "0B58", "0:1", false, true )]
	[InlineData( "(*LF)(?x)#ignored\rX", "58", "0:0", true, false )]
	[InlineData( "(*ANYCRLF)(?x)#ignored\rX", "58", "0:1", false, false )]
	[InlineData( "(*ANYCRLF)(?xx)#ignored\r[X ]", "58", "0:1", false, false )]
	[InlineData( @"(*LF)\Q(*ANYCRLF)\E(.)", "282A414E5943524C46290D", "0:11,10:1", true, false )]
	[InlineData( @"(*LF)[(*ANYCRLF)](.)", "410D", "0:2,1:1", true, false )]
	public void PrefixesHavePinnedGlobalSpans( string pattern, string hex, string spans, bool callerNewline, bool callerBsr ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf,
				AnyCrLfNewline = callerNewline, BsrAnyCrLf = callerBsr };
			var reference = Reference( pattern, options );
			AssertLiteral( reference, subject, spans );
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ByteMembershipSelectsIndependentFamiliesOverOppositeDefaults( bool any ) {
		var newline = any ? "(*ANYCRLF)" : "(*LF)";
		var bsr = any ? "(*BSR_ANYCRLF)" : "(*BSR_UNICODE)";
		foreach ( var (pattern, isBsr) in new[] { (newline + @"\A.\z", false), (bsr + @"\A\R\z", true) } ) {
			var options = new PcreByteOptions { AnyCrLfNewline = !any, BsrAnyCrLf = !any };
			var reference = Reference( pattern, options );
			var compiled = Compile( pattern, options );
			for ( var value = 0; value <= 255; value++ ) {
				var expected = isBsr ? value is 0x0A or 0x0D || !any && value is 0x0B or 0x0C or 0x85
					: value != 0x0A && !( any && value == 0x0D );
				var subject = new[] { (byte)value };
				Assert.Equal( expected, reference.Match( subject ).Success );
				Compare( reference, subject, 0, compiled.Match( subject ), $"{pattern}, {value:X2}" );
			}
		}
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void UtfMembershipAndMissingFamilyKeepTheirOwnSettings( bool anyNewline, bool anyBsr ) {
		var options = new PcreByteOptions { Utf8 = true, Ucp = true,
			AnyCrLfNewline = anyNewline, BsrAnyCrLf = anyBsr };
		foreach ( var (pattern, isBsr) in new[] {
			(( anyNewline ? "(*ANYCRLF)" : "(*LF)" ) + @"\A\R\z", true),
			(( anyBsr ? "(*BSR_ANYCRLF)" : "(*BSR_UNICODE)" ) + @"\A.\z", false) } ) {
			var reference = Reference( pattern, options );
			var compiled = Compile( pattern, options );
			foreach ( var scalar in new[] { 0, 9, 10, 11, 12, 13, 14, 0x84, 0x85, 0x86, 0x180E, 0x2028, 0x2029, 0x1F600, 0x10FFFF } ) {
				var expected = isBsr ? scalar is 10 or 13 || !anyBsr && scalar is 11 or 12 or 0x85 or 0x2028 or 0x2029
					: scalar != 10 && !( anyNewline && scalar == 13 );
				var subject = Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) );
				Assert.Equal( expected, reference.Match( subject ).Success );
				Compare( reference, subject, 0, compiled.Match( subject ), $"{pattern}, U+{scalar:X}" );
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryCallerOffsetRetainsCandidateAndFragmentProtocols( bool utf ) {
		foreach ( var prefix in new[] { "(*LF)(*BSR_UNICODE)", "(*ANYCRLF)(*BSR_ANYCRLF)" } ) {
			foreach ( var body in new[] { @".", @"\N", @"(?m)^X|X$", @"\R", @"\G\R", @"(?<=\R)X", @"(\R)?X", @"[^\r]X", @"[\n]X" } ) {
				var pattern = prefix + body;
				var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf,
					AnyCrLfNewline = true, BsrAnyCrLf = true };
				var reference = Reference( pattern, options );
				var compiled = Compile( pattern, options );
				foreach ( var hex in new[] { "0D0A58", "0B580D0A", "FF0D0A58", "0DFF0A58", "E280A858FF" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, UTF={utf}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( @" (*LF)", 1 )]
	[InlineData( @"(?x)(*ANYCRLF)", 4 )]
	[InlineData( @"(?#x)(*BSR_UNICODE)", 5 )]
	[InlineData( @"(*LF)(?#x)(*LF)", 10 )]
	[InlineData( @"(*LF)(?^)(*LF)", 9 )]
	[InlineData( @"a(*BSR_ANYCRLF)", 1 )]
	[InlineData( @"(?:(*LF))", 3 )]
	[InlineData( @"(?=(*LF))", 3 )]
	[InlineData( @"(*LF)|(*ANYCRLF)", 6 )]
	[InlineData( @"(*LF)(*ANY)", 5 )]
	[InlineData( @"(*lf)", 0 )]
	[InlineData( @"(*LFx)", 0 )]
	[InlineData( @"(*BSR_ANYCRLF )", 0 )]
	public void UnrecognizedOrMisplacedDirectiveRetainsOpeningByteDiagnostic( string pattern, int offset ) {
		if ( pattern != "(*LF)(*ANY)" ) { Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) ); }
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"(*LF", 1 )]
	[InlineData( @"(*BSR_UNICODE", 1 )]
	[InlineData( @"(*LF)*", 5 )]
	[InlineData( @"(*ANYCRLF){2}", 10 )]
	[InlineData( @"(*BSR_UNICODE)(", 14 )]
	[InlineData( @"(*LF)é\", 7 )]
	public void MalformedPatternsKeepOriginalUtfByteOffsets( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void InvalidUtfPatternValidationStillPrecedesDirectives() {
		var bytes = "(*LF)"u8.ToArray().Concat( new byte[] { 0xff } ).ToArray();
		Assert.Throws<PcrePatternException>( () => Reference( bytes, new() { Utf8 = true } ) );
		var result = PcreByteCompiler.Compile( bytes, new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, result.Diagnostic?.Code );
		Assert.Equal( 5, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void LongPrefixAddsNoVmWorkAndKeepsBudgetsAndCancellation() {
		const string body = @"(?<x>\R+)";
		var prefix = String.Concat( Enumerable.Repeat( "(*LF)(*BSR_UNICODE)", 1024 ) );
		var subject = "\v\r\n"u8.ToArray();
		var optionSteps = new List<int>();
		var prefixSteps = new List<int>();
		var direct = Compile( body, new() { WorkObserver = optionSteps.Add } );
		var prefixed = Compile( prefix + body, new() { AnyCrLfNewline = true, BsrAnyCrLf = true, WorkObserver = prefixSteps.Add } );
		var reference = Reference( prefix + body, new() { AnyCrLfNewline = true, BsrAnyCrLf = true } );
		AssertLiteral( reference, subject, "0:3,0:3" );
		Compare( reference, subject, 0, prefixed.Match( subject ), "long prefix" );
		Assert.Equal( 3, direct.Match( subject ).Match?.ByteLength );
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
		Options = PcreOptions.Compiled | ( options.Utf8 || options.MatchInvalidUtf ? PcreOptions.Utf : PcreOptions.None )
			| ( options.Ucp ? PcreOptions.Ucp : PcreOptions.None ) | ( options.MatchInvalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ),
		NewLine = options.AnyCrLfNewline ? PcreNewLine.AnyCrLf : PcreNewLine.Lf,
		BackslashR = options.BsrAnyCrLf ? PcreBackslashR.AnyCrLf : PcreBackslashR.Unicode,
		ParensLimit = 512
	} );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
