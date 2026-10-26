using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 8-bit UTF property L spans over stable Unicode categories.</summary>
public sealed class PcreUtf8LetterPropertyTests {
	[Theory]
	[InlineData( "\\p{L}+", "3éΩ𐐀!", 1, 8 )]
	[InlineData( "^\\p{L}+$", "é", 0, 2 )]
	[InlineData( "\\p{L}", "ǅ", 0, 2 )]
	[InlineData( "\\p{L}", "ᵃ", 0, 3 )]
	[InlineData( "\\p{L}", "中", 0, 3 )]
	[InlineData( "\\p{L}", "Ⅻ", -1, 0 )]
	[InlineData( "\\p{L}", "e\u0301", 0, 1 )]
	[InlineData( "\\p{L}", "😀", -1, 0 )]
	[InlineData( "\\p{L}", "", -1, 0 )]
	[InlineData( "\\p{L}+?\\p{L}", "abc", 0, 2 )]
	[InlineData( "\\pL+", "3é", 1, 2 )]
	[InlineData( "\\P{L}+", "é٣😀a", 2, 6 )]
	[InlineData( "\\p{^L}+", "é٣😀a", 2, 6 )]
	[InlineData( "\\PL+", "é٣😀a", 2, 6 )]
	[InlineData( "\\P{^L}+", "3é", 1, 2 )]
	public void PropertyLetterMatchesScalarAndReportsSourceByteSpan( string pattern, string subject, int index, int length ) {
		var result = Compile( Encoding.UTF8.GetBytes( pattern ) ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : index, result.Match?.ByteIndex );
		if ( index >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void PropertyCanMatchGrepUnicodeLetterFixture() {
		var result = Compile( "^\\p{L}+$"u8.ToArray() ).Match( "é"u8.ToArray() );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( 0, result.Match?.ByteIndex );
		Assert.Equal( 2, result.Match?.ByteLength );
		Assert.Null( Compile( "^\\p{L}+$"u8.ToArray() ).Match( "7"u8.ToArray() ).Match );
	}

	[Fact]
	public void FailedPropertyPathRestoresCaptureAndOrderedAlternative() {
		var result = Compile( "(\\p{L}+)x|(\\p{L}+)"u8.ToArray() ).Match( "éΩ"u8.ToArray() );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( 0, 4 ), result.Match?.Captures[0] );
		Assert.Null( result.Match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 4 ), result.Match?.Captures[2] );
	}

	[Fact]
	public void PropertyCaptureCanBeReusedByByteBackreference() {
		var result = Compile( "(\\p{L}+)\\1"u8.ToArray() ).Match( "éé"u8.ToArray() );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( 0, 4 ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 2 ), result.Match?.Captures[1] );
	}

	[Theory]
	[InlineData( "\\p", 0 )]
	[InlineData( "\\p{", 0 )]
	[InlineData( "\\p{}", 0 )]
	[InlineData( "\\p{^}", 0 )]
	[InlineData( "\\p{L", 0 )]
	[InlineData( "a\\P{L", 1 )]
	public void MalformedPropertyReportsSlashByteOffset( string pattern, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "\\p{Greek}", 0, true )]
	[InlineData( "\\p{L}", 0, false )]
	public void OtherPropertiesOrByteModeRemainExplicitlyUnsupported( string pattern, int offset, bool utf8 ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ),
			new PcreByteOptions { Utf8 = utf8 } );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void PropertyEvaluationUsesMatchWorkBudgetAndCancellation() {
		// Validation, candidate and instruction transition consume three steps;
		// property evaluation is step four, before Accept at step five.
		var limited = Compile( "\\p{L}"u8.ToArray(), new PcreByteOptions { Utf8 = true, MatchWorkLimit = 4 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			limited.Match( "é"u8.ToArray() ).Diagnostic?.Code );
		var exact = Compile( "\\p{L}"u8.ToArray(), new PcreByteOptions { Utf8 = true, MatchWorkLimit = 5 } );
		Assert.Equal( 2, exact.Match( "é"u8.ToArray() ).Match?.ByteLength );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "\\p{L}"u8.ToArray(), new PcreByteOptions {
			Utf8 = true, WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( "é"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( pattern, options ?? new PcreByteOptions { Utf8 = true } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
