using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 UTF-8 word classes and scalar-aligned boundaries.</summary>
public sealed class PcreUtf8WordTests {
	[Theory]
	[InlineData( "\\w+", "é٣‿a", true, 0, 8 )]
	[InlineData( "\\w", "a\u0301", true, 0, 1 )]
	[InlineData( "\\w", "\u0301", true, 0, 2 )]
	[InlineData( "\\w", "Ⅻ", true, 0, 3 )]
	[InlineData( "\\w", "²", true, 0, 2 )]
	[InlineData( "\\w", "😀", true, -1, 0 )]
	[InlineData( "\\W", "😀", true, 0, 4 )]
	[InlineData( "\\W", "é", true, -1, 0 )]
	[InlineData( "\\w", "éA", false, 2, 1 )]
	[InlineData( "\\W", "éA", false, 0, 2 )]
	[InlineData( "\\W", "", false, -1, 0 )]
	public void WordShorthandConsumesAWholeScalar( string pattern, string subject, bool ucp, int offset, int length ) {
		var result = Compile( pattern, ucp ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( offset < 0 ? null : offset, result.Match?.ByteIndex );
		if ( offset >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Theory]
	[InlineData( "\\bé\\b", " é!", 1, 2 )]
	[InlineData( "\\bé", "aé", -1, 0 )]
	[InlineData( "\\Bé", "aé", 1, 2 )]
	[InlineData( "\\B\u0301", "a\u0301", 1, 2 )]
	[InlineData( "\\Bb", "a\u0301b", 3, 1 )]
	[InlineData( "\\b😀", "a😀", 1, 4 )]
	[InlineData( "\\b😀", "!😀", -1, 0 )]
	[InlineData( "\\bA", "😀A", 4, 1 )]
	[InlineData( "\\bA", "𝟘A", -1, 0 )]
	[InlineData( "\\BA", "𝟘A", 4, 1 )]
	[InlineData( "\\B", "", 0, 0 )]
	[InlineData( "\\b", "", -1, 0 )]
	public void UcpBoundaryComparesAdjacentScalars( string pattern, string subject, int offset, int length ) {
		var result = Compile( pattern, true ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( offset < 0 ? null : offset, result.Match?.ByteIndex );
		if ( offset >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void NonzeroOffsetStillSeesPreviousScalar() {
		var input = "éA"u8.ToArray();
		Assert.Null( Compile( "\\bA", true ).Match( input, startOffset: 2 ).Match );
		Assert.Equal( new PcreByteCapture( 2, 1 ), Compile( "\\BA", true )
			.Match( input, startOffset: 2 ).Match?.Captures[0] );
	}

	[Fact]
	public void BacktrackingRestoresCapturesAcrossUcpBoundary() {
		var match = Compile( "(é)\\bX|(é)\\B\\2", true ).Match( "éé"u8.ToArray() ).Match;
		Assert.Equal( new PcreByteCapture( 0, 4 ), match?.Captures[0] );
		Assert.Null( match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 2 ), match?.Captures[2] );
	}

	[Fact]
	public void AsciiDigitRestrictionDoesNotChangeUcpWordClassification() {
		var result = PcreByteCompiler.Compile( "\\w٣"u8.ToArray(), new PcreByteOptions {
			Utf8 = true, Ucp = true, AsciiBsD = true
		} );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( 0, 4 ), result.Pattern?.Match( "٣٣"u8.ToArray() ).Match?.Captures[0] );
	}

	[Fact]
	public void WordBoundaryClassificationHasWorkAndCancellationChecks() {
		var limited = PcreByteCompiler.Compile( "\\b"u8.ToArray(), new PcreByteOptions {
			Utf8 = true, Ucp = true, MatchWorkLimit = 3
		} );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			limited.Pattern?.Match( "a"u8.ToArray() ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = PcreByteCompiler.Compile( "\\b"u8.ToArray(), new PcreByteOptions {
			Utf8 = true, Ucp = true,
			WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => cancellable.Pattern?.Match(
			"a"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, bool ucp ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions {
			Utf8 = true, Ucp = ucp
		} );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
