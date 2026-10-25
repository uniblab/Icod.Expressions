using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Fixed literal assertions over valid UTF-8 islands and source-byte captures.</summary>
public sealed class PcreUtf8LiteralLookaroundTests {
	private static readonly PcreByteOptions GrepOptions = new() {
		Utf8 = true, Ucp = true, AsciiBsD = true, MatchInvalidUtf = true
	};

	[Theory]
	[InlineData( "(?<=foo)bar", "foobar", 3, 3 )]
	[InlineData( "(?<=é)A", "éA", 2, 1 )]
	[InlineData( "(?<=😀)A", "😀A", 4, 1 )]
	[InlineData( "(?=😀)😀", "x😀", 1, 4 )]
	[InlineData( "é(?=A)", "éA", 0, 2 )]
	[InlineData( "é(?!A)", "éB", 0, 2 )]
	[InlineData( "(?<!é)A", "àA", 2, 1 )]
	[InlineData( "(?<!é)A", "A", 0, 1 )]
	[InlineData( "(?=)", "é", 0, 0 )]
	[InlineData( "(?<=)", "é", 0, 0 )]
	[InlineData( "(?!)", "é", -1, 0 )]
	[InlineData( "(?<!)", "é", -1, 0 )]
	[InlineData( "(?<=é)A", "àA", -1, 0 )]
	[InlineData( "(?<!é)A", "éA", -1, 0 )]
	[InlineData( "é(?=A)", "éB", -1, 0 )]
	[InlineData( "é(?!A)", "éA", -1, 0 )]
	public void Utf8LiteralAssertionsKeepOriginalByteSpans(
		string pattern, string subject, int index, int length
	) {
		var result = Compile( pattern ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : new PcreByteCapture( index, length ), result.Match?.Captures[0] );
	}

	[Theory]
	[InlineData( @"(?<=\xFF)A", "C3BF41", 2, 1 )]
	[InlineData( @"(?=\xFF)ÿ", "C3BF", 0, 2 )]
	[InlineData( @"(?<=\x80)A", "C28041", 2, 1 )]
	[InlineData( @"(?<=\x00)é", "00C3A9", 1, 2 )]
	[InlineData( @"(?<=\xFF)A", "FF41", -1, 0 )]
	public void HexEscapesInAssertionAreUnicodeScalars(
		string pattern, string subjectHex, int index, int length
	) {
		var result = Compile( pattern ).Match( Convert.FromHexString( subjectHex ) );
		Assert.Equal( index < 0 ? null : new PcreByteCapture( index, length ), result.Match?.Captures[0] );
	}

	[Theory]
	[InlineData( "(?<=é)A", "C3A9FF41", -1, 0 )]
	[InlineData( "(?<!é)A", "C3A9FF41", 3, 1 )]
	[InlineData( "é(?=A)", "C3A9FF41", -1, 0 )]
	[InlineData( "é(?!A)", "C3A9FF41", 0, 2 )]
	[InlineData( "(?=é)é", "FFC3A9", 1, 2 )]
	public void AssertionCannotInspectAcrossMalformedUtf8(
		string pattern, string subjectHex, int index, int length
	) {
		var result = Compile( pattern ).Match( Convert.FromHexString( subjectHex ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : new PcreByteCapture( index, length ), result.Match?.Captures[0] );
	}

	[Fact]
	public void FailedAssertionRestoresCaptureBeforeAlternative() {
		var result = Compile( "(é)(?<=😀)A|(é)(?<=é)A" ).Match( "éA"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 0, 3 ), result.Match?.Captures[0] );
		Assert.Null( result.Match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 2 ), result.Match?.Captures[2] );
	}

	[Fact]
	public void StartOffsetInUtf8ScalarAdvancesOnlyWithInvalidUtfPolicy() {
		var input = "éA"u8.ToArray();
		Assert.Equal( 2, Compile( "(?<=é)A" ).Match( input, startOffset: 1 ).Match?.ByteIndex );
		var strict = Compile( "(?<=é)A", new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset,
			strict.Match( input, startOffset: 1 ).Diagnostic?.Code );
		Assert.Equal( 2, strict.Match( input, startOffset: 2 ).Match?.ByteIndex );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject,
			strict.Match( Convert.FromHexString( "C3A9FF41" ) ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( "(?<=é+)A", PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "(?=\\w)A", PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "(?=\\p{L})A", PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "(?<=é|à)A", PcreByteDiagnosticCode.UnsupportedSyntax )]
	[InlineData( "(?<=é", PcreByteDiagnosticCode.InvalidSyntax )]
	public void NonliteralBodiesRemainDiagnosed( string pattern, PcreByteDiagnosticCode code ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), GrepOptions );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void AssertionComparisonObservesWorkAndCancellation() {
		var input = "éééééééééé"u8.ToArray();
		var limited = Compile( "(?=éééééééééé)", GrepOptions with { MatchWorkLimit = 14 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Match( input ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( "(?=éééééééééé)", GrepOptions with {
			WorkObserver = count => { if ( count == 14 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () =>
			cancellable.Match( input, cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options ?? GrepOptions );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
