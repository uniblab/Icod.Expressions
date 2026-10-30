using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Failure verbs share backtracking but terminate native consumed-width branches.</summary>
public sealed class PcreFailureVerbReferenceTests {
	[Theory]
	[InlineData( @"(*FAIL)", "", "" )]
	[InlineData( @"(*F)", "58", "" )]
	[InlineData( @"(*FAIL)|", "", "0:0" )]
	[InlineData( @"(*F)|a", "61", "0:1" )]
	[InlineData( @"a(*FAIL)|ab", "6162", "0:2" )]
	[InlineData( @"(a)(*F)|(b)", "6162", "1:1,-,1:1" )]
	[InlineData( @"(a(*F)|ab)(c)", "616263", "0:3,0:2,2:1" )]
	[InlineData( @"(?>a(*F)|ab)c", "616263", "0:3" )]
	[InlineData( @"(?>a|ab)(*F)|abc", "616263", "0:3" )]
	[InlineData( @"(?=(a(*F)|ab))\1", "6162", "0:2,0:2" )]
	[InlineData( @"(?!(a(*F)))a", "61", "0:1,-" )]
	[InlineData( @"(?=(a))(*F)|a", "61", "0:1,-" )]
	[InlineData( @"(?:a(*F))*b", "62", "0:1" )]
	[InlineData( @"(?:a(*F)){0}b", "62", "0:1" )]
	[InlineData( @"(?:a(*F))+", "61", "" )]
	[InlineData( @"\A(?:a(*F)|ab)+\z", "61626162", "0:4" )]
	[InlineData( @"\A(a)?(?(1)(*F)|b)\z", "62", "0:1,-" )]
	[InlineData( @"\A(a)?(?(1)(*F)|b)\z", "6162", "" )]
	[InlineData( @"(?(?=a)(*F)|b)", "62", "0:1" )]
	[InlineData( @"(?(?=a)(*F)|b)", "61", "" )]
	[InlineData( @"(?<x>a(*F)|b)(?&x)", "6262", "0:2,0:1" )]
	[InlineData( @"(?<x>a|b(*F))(?&x)|ab", "6162", "0:2,-" )]
	[InlineData( @"(?(DEFINE)(?<x>a(*F)|b))(?&x)", "62", "0:1,-" )]
	[InlineData( @"\A(?<!(?&x))X(?(DEFINE)(?<x>(*F)(?&x)))", "58", "0:1,-" )]
	[InlineData( @"(?<=(*F)a*)X", "58", "" )]
	[InlineData( @"(?<!(*F)a*)X", "58", "0:1" )]
	[InlineData( @"(?<=a(*F)b*)X", "6158", "" )]
	[InlineData( @"(?<!a(*F)b*)X", "6158", "1:1" )]
	[InlineData( @"(?<=(*F)a*|ab)X", "616258", "2:1" )]
	[InlineData( @"(?<=(*F)(?<=a*))X", "58", "" )]
	[InlineData( @"(?<!(*F)(?=(?<=a*)))X", "58", "0:1" )]
	[InlineData( @"(?<=(?:a(*F)b*|bc))X", "626358", "2:1" )]
	[InlineData( @"(?<!(*F))(?<x>X)", "58", "0:1,0:1" )]
	[InlineData( @"(?*(a(*F)|ab))\1c", "616263", "0:3,0:2" )]
	[InlineData( @"(*F)\Q*\E|X", "58", "0:1" )]
	[InlineData( @"\Q(*FAIL)\E", "282A4641494C29", "0:7" )]
	[InlineData( @"[(*F)]+", "282A4629", "0:4" )]
	[InlineData( @"(*UTF)(*UCP)(?<x>é)(*F)|é", "C3A9", "0:2,-" )]
	[InlineData( @"a\K(*F)|ab", "6162", "0:2" )]
	[InlineData( @"(a)(*F)|ab", "6162", "0:2,-" )]
	[InlineData( @"(?i)(*F)|k", "4B", "0:1" )]
	public void FailureAliasesHavePinnedCaptureAndWidthBehavior( string pattern, string hex, string spans ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var reference = Reference( pattern, options );
			AssertLiteral( reference, subject, spans );
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryOffsetRetainsCaptureRollbackNewlineMetadataAndUtfFragments( bool anyCrLf ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf };
			foreach ( var pattern in new[] { @"(*F)", @"(*F)|()", @"(a)(*F)|X", @"a\K(*F)|aX",
				@"(?!(a(*F)))X", @"(*F)\n|[^r]X", @"(*F)|[^r]X", @"\G(*F)|X" } ) {
				var reference = Reference( pattern, options );
				var compiled = Compile( pattern, options );
				foreach ( var hex in new[] { "6158", "0D0A58", "FF6158", "C3A9FF58", "6161FF58" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( @"(*FAIL)*", 7 )]
	[InlineData( @"(*FAIL)?", 7 )]
	[InlineData( @"(*F){0}", 4 )]
	[InlineData( @"(*F){1,2}", 4 )]
	[InlineData( @"(*FAIL){,2}", 7 )]
	[InlineData( @"(*F){65536}", 4 )]
	[InlineData( @"(*F)(?#x)*", 9 )]
	[InlineData( @"é(*FAIL)+", 9 )]
	public void DirectQuantifiersStayInvalidAtTheirOriginalByte( string pattern, int offset ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf };
			Assert.Throws<PcrePatternException>( () => Reference( pattern, options ) );
			var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
			Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
			Assert.Equal( offset, result.Diagnostic?.ByteOffset );
		}
	}

	[Theory]
	[InlineData( @"(?<=a*(*F))X", 0, true )]
	[InlineData( @"(?<=(?&x))X(?(DEFINE)(?<x>(?&x)(*F)))", 0, true )]
	[InlineData( @"(*F)(?<=a*)", 4, true )]
	[InlineData( @"(?<=(*F)a*|b*)X", 0, true )]
	[InlineData( @"(*F)\k<missing>", 4, false )]
	[InlineData( @"(*FAIL)\2", 7, false )]
	[InlineData( @"(?<=(*F)\k<missing>)X", 8, false )]
	public void FailDoesNotHideGlobalErrorsOrEarlierUnboundedWidths( string pattern, int offset, bool widthError ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( widthError ? PcreByteDiagnosticCode.UnsupportedSyntax : PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"(*fail)", true )]
	[InlineData( @"(*FA)", true )]
	[InlineData( @"(*FAILx)", true )]
	[InlineData( @"(*FAIL:x)", false )]
	[InlineData( @"(*F:x)", false )]
	public void ExactNoArgumentScopeRetainsOtherVerbRejection( string pattern, bool invalidNative ) {
		if ( invalidNative ) { Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) ); }
		else { Assert.False( Reference( pattern, new() ).Match( "x"u8.ToArray() ).Success ); }
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"(*F", 1 )]
	[InlineData( @"(*FAIL", 1 )]
	public void UnterminatedHeadersKeepExistingMalformedGroupDiagnostics( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void SimpleFailureUsesExistingTransitionWorkAndCancellation( bool utf ) {
		var subject = "abc"u8.ToArray();
		var assertionSteps = new List<int>();
		var failureSteps = new List<int>();
		var assertion = Compile( "(?!)", new() { Utf8 = utf, WorkObserver = assertionSteps.Add } );
		var failure = Compile( "(*FAIL)", new() { Utf8 = utf, WorkObserver = failureSteps.Add } );
		Assert.Null( assertion.Match( subject ).Match );
		var actual = failure.Match( subject );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		Assert.Null( actual.Match );
		Assert.Equal( assertionSteps, failureSteps );
		Assert.Equal( Enumerable.Range( 1, failureSteps.Count ), failureSteps );
		var limited = Compile( "(*F)", new() { Utf8 = utf, MatchWorkLimit = 1 } ).Match( subject );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		Assert.Null( limited.Match );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( "(*F)", new() { Utf8 = utf, WorkObserver = count => { if ( count == 2 ) cancellation.Cancel(); } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => failure.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( "(*F)"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void FailureInstructionsStillRespectTheSharedCompileBudget() {
		var pattern = Encoding.ASCII.GetBytes( String.Concat( Enumerable.Repeat( "(*F)", 5000 ) ) );
		var result = PcreByteCompiler.Compile( pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, result.Diagnostic?.Code );
		Assert.Null( result.Pattern );
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
