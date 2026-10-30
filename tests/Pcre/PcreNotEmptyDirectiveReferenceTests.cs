using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Empty whole-match rejection must backtrack at the reported start without changing inner success.</summary>
public sealed class PcreNotEmptyDirectiveReferenceTests {
	[Theory]
	[InlineData( @"(*NOTEMPTY)", "", "" )]
	[InlineData( @"(*NOTEMPTY)", "61", "" )]
	[InlineData( @"(*NOTEMPTY_ATSTART)", "61", "1:0" )]
	[InlineData( @"(*NOTEMPTY_ATSTART)", "", "" )]
	[InlineData( @"(*NOTEMPTY)|a", "61", "0:1" )]
	[InlineData( @"(*NOTEMPTY_ATSTART)|a", "61", "0:1" )]
	[InlineData( @"(*NOTEMPTY)(a*?)", "6161", "0:1,0:1" )]
	[InlineData( @"(*NOTEMPTY_ATSTART)(a*?)", "6161", "0:1,0:1" )]
	[InlineData( @"(*NOTEMPTY)(a?)|(b)", "62", "0:1,-,0:1" )]
	[InlineData( @"(*NOTEMPTY_ATSTART)(a?)|(b)", "62", "0:1,-,0:1" )]
	[InlineData( @"(*NOTEMPTY)a\K", "61", "" )]
	[InlineData( @"(*NOTEMPTY_ATSTART)a\K", "61", "1:0" )]
	[InlineData( @"(*NOTEMPTY)(a)\K|(a)", "61", "0:1,-,0:1" )]
	[InlineData( @"(*NOTEMPTY_ATSTART)(a)\K|(a)", "61", "1:0,0:1,-" )]
	[InlineData( @"(*NOTEMPTY)()(?=a)a", "61", "0:1,0:0" )]
	[InlineData( @"(*NOTEMPTY)(?=(a?))a", "61", "0:1,0:1" )]
	[InlineData( @"(*NOTEMPTY)(?<=())a", "61", "0:1,0:0" )]
	[InlineData( @"(*NOTEMPTY)(?!(b))a", "61", "0:1,-" )]
	[InlineData( @"(*NOTEMPTY)(?>|a)", "61", "" )]
	[InlineData( @"(*NOTEMPTY)(?:|a)", "61", "0:1" )]
	[InlineData( @"(*NOTEMPTY_ATSTART)(?>|a)", "61", "1:0" )]
	[InlineData( @"(*NOTEMPTY)(?(?=())a|b)", "61", "0:1,0:0" )]
	[InlineData( @"(*NOTEMPTY)(?(DEFINE)(?<x>))(?&x)a", "61", "0:1,-" )]
	[InlineData( @"(*NOTEMPTY)(?(DEFINE)(?<x>a?))(?&x)", "61", "0:1,-" )]
	[InlineData( @"(*NOTEMPTY)(?<x>a(?&x)?)", "6161", "0:2,0:2" )]
	[InlineData( @"(*NOTEMPTY)(?*()|a)\1a", "61", "0:1,0:0" )]
	[InlineData( @"(*NOTEMPTY)(*FAIL)|a", "61", "0:1" )]
	[InlineData( @"(*NOTEMPTY_ATSTART)(?<=a)", "61", "1:0" )]
	[InlineData( @"(*NOTEMPTY_ATSTART)\G", "61", "" )]
	[InlineData( @"(*NOTEMPTY)(*NOTEMPTY_ATSTART)a\K", "61", "" )]
	[InlineData( @"(*NOTEMPTY_ATSTART)(*NOTEMPTY)a\K", "61", "" )]
	[InlineData( @"(*NOTEMPTY)(*NOTEMPTY)(?^)a?", "61", "0:1" )]
	[InlineData( @"(*NOTEMPTY_ATSTART)(*LF)(*UTF8)(*UCP)(*BSR_ANYCRLF)(*ANYCRLF)(é?)", "C3A9", "0:2,0:2" )]
	[InlineData( @"\Q(*NOTEMPTY)\E", "282A4E4F54454D50545929", "0:11" )]
	[InlineData( @"[(*NOTEMPTY)]", "4E", "0:1" )]
	public void WholeMatchFlagsHavePinnedLiteralCaptureSpans( string pattern, string hex, string spans ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var subject = Convert.FromHexString( hex );
			var reference = Reference( pattern, options );
			AssertLiteral( reference, subject, spans );
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void EveryOffsetKeepsOriginalStartAndFragmentProtocols( bool utf, bool anyCrLf ) {
		var options = new PcreByteOptions { MatchInvalidUtf = utf, Ucp = utf, AnyCrLfNewline = anyCrLf };
		foreach ( var prefix in new[] { "(*NOTEMPTY)", "(*NOTEMPTY_ATSTART)" } ) {
			foreach ( var body in new[] { @"", @"()", @"|.", @"(a?)|(b)", @"a\K|a", @"\G|.", @"(?<=a)|.", @"(?>|.)", @"(?=.)|.", @"()\n|." } ) {
				var pattern = prefix + body;
				var reference = Reference( pattern, options );
				var compiled = Compile( pattern, options );
				foreach ( var hex in new[] { "", "6162", "0D0A58", "FF61", "C3A961", "C3A9FF58", "FFFF61FF" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, UTF={utf}, ANYCRLF={anyCrLf}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( @"a(*NOTEMPTY)", 1 )]
	[InlineData( @"(?i)(*NOTEMPTY)", 4 )]
	[InlineData( @" (*NOTEMPTY_ATSTART)", 1 )]
	[InlineData( @"(?x) (*NOTEMPTY)", 5 )]
	[InlineData( @"(?:(*NOTEMPTY))", 3 )]
	[InlineData( @"(*NOTEMPTY)(?:(*NOTEMPTY_ATSTART))", 14 )]
	[InlineData( @"(*notempty)", 0 )]
	[InlineData( @"(*NOTEMPTY:x)", 0 )]
	[InlineData( @"(*NOTEMPTY_ATSTART:x)", 0 )]
	[InlineData( @"(*NOTEMPTYx)", 0 )]
	public void PrefixOnlyExactGrammarRetainsOpeningByteErrors( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"(*NOTEMPTY)*", 11 )]
	[InlineData( @"(*NOTEMPTY_ATSTART){0}", 19 )]
	[InlineData( @"(*NOTEMPTY", 1 )]
	[InlineData( @"(*NOTEMPTY_ATSTART", 1 )]
	public void DirectRepeatsAndIncompleteHeadersStayInvalid( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "282A4E4F54454D50545929282A55544629FF", 17 )]
	[InlineData( "282A55544629282A4E4F54454D5054595F4154535441525429FF", 25 )]
	public void MixedPrefixesValidateTheWholeOriginalUtfPattern( string hex, int offset ) {
		var pattern = Convert.FromHexString( hex );
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( pattern );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void StrictUtfValidationStillPrecedesMatchAcceptance( bool atStart ) {
		var pattern = atStart ? "(*UTF)(*NOTEMPTY_ATSTART)" : "(*UTF)(*NOTEMPTY)";
		var compiled = Compile( pattern, new() );
		var reference = Reference( pattern, new() );
		Assert.Throws<PcreMatchException>( () => reference.Match( new byte[] { 0xFF } ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( new byte[] { 0xFF } ).Diagnostic?.Code );
		Assert.Throws<PcreMatchException>( () => reference.Match( "é"u8.ToArray(), 1 ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( "é"u8.ToArray(), 1 ).Diagnostic?.Code );
	}

	[Fact]
	public void LongMixedPrefixesRemainGlobalWithoutAddingInstructions() {
		var prefix = String.Concat( Enumerable.Repeat( "(*NOTEMPTY_ATSTART)(*NOTEMPTY)", 1500 ) );
		var pattern = prefix + "(*LF)(*UTF)(*UCP)(?^)a?";
		var reference = Reference( pattern, new() );
		var compiled = Compile( pattern, new() );
		Compare( reference, "a"u8.ToArray(), 0, compiled.Match( "a"u8.ToArray() ), pattern );
		Compare( reference, Array.Empty<byte>(), 0, compiled.Match( Array.Empty<byte>() ), pattern );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void NonemptyResultsKeepSharedWorkAndRejectedEmptyPathsRemainCancellable( bool utf ) {
		var plainSteps = new List<int>();
		var flaggedSteps = new List<int>();
		var plain = Compile( "a", new() { Utf8 = utf, WorkObserver = plainSteps.Add } );
		var flagged = Compile( "(*NOTEMPTY_ATSTART)(*NOTEMPTY)a", new() { Utf8 = utf, WorkObserver = flaggedSteps.Add } );
		Assert.Equal( new PcreByteCapture( 0, 1 ), plain.Match( "a"u8.ToArray() ).Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 1 ), flagged.Match( "a"u8.ToArray() ).Match?.Captures[0] );
		Assert.Equal( plainSteps, flaggedSteps );
		Assert.Equal( Enumerable.Range( 1, flaggedSteps.Count ), flaggedSteps );
		var limited = Compile( "(*NOTEMPTY)", new() { Utf8 = utf, MatchWorkLimit = 1 } ).Match( "a"u8.ToArray() );
		Assert.Null( limited.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( "(*NOTEMPTY_ATSTART)", new() { Utf8 = utf,
			WorkObserver = count => { if ( count == 2 ) cancellation.Cancel(); } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( "a"u8.ToArray(), cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => flagged.Match( "a"u8.ToArray(), cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( "(*NOTEMPTY)"u8.ToArray(), cancellationToken: cancellation.Token ) );
		var invalidLimit = PcreByteCompiler.Compile( "(*NOTEMPTY)(*UTF)"u8.ToArray(), new() { MatchWorkLimit = 0 } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit, invalidLimit.Diagnostic?.Code );
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
