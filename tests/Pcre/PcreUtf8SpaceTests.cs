using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 UTF-8 whitespace and standalone UCP POSIX space/word fixtures.</summary>
public sealed class PcreUtf8SpaceTests {
	private static readonly PcreByteOptions GrepOptions = new() { Utf8 = true, Ucp = true, AsciiBsD = true };

	[Theory]
	[InlineData( true, "\\s", "é\u00a0x", 2, 2 )]
	[InlineData( true, "\\s", "\u0085", 0, 2 )]
	[InlineData( true, "\\s", "\u180e", 0, 3 )]
	[InlineData( true, "\\s", "\u2028", 0, 3 )]
	[InlineData( true, "\\s", "\u2029", 0, 3 )]
	[InlineData( true, "\\s", "\u200b", -1, 0 )]
	[InlineData( true, "\\S", "😀", 0, 4 )]
	[InlineData( true, "\\S", "\u00a0", -1, 0 )]
	[InlineData( false, "\\s", "\t", 0, 1 )]
	[InlineData( false, "\\s", "\u00a0", -1, 0 )]
	[InlineData( false, "\\s", "\u0085", -1, 0 )]
	[InlineData( false, "\\S", "\u00a0", 0, 2 )]
	[InlineData( false, "\\S", "", -1, 0 )]
	public void SpaceShorthandConsumesOneScalar( bool ucp, string pattern, string subject, int offset, int length ) {
		var result = Compile( pattern, new PcreByteOptions { Utf8 = true, Ucp = ucp } )
			.Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( offset < 0 ? null : offset, result.Match?.ByteIndex );
		if ( offset >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Theory]
	[InlineData( true, "[[:space:]]", "\u00a0", 0, 2 )]
	[InlineData( true, "[[:space:]]", "\u0085", 0, 2 )]
	[InlineData( true, "[[:space:]]", "\u180e", 0, 3 )]
	[InlineData( false, "[[:space:]]", "\u00a0", -1, 0 )]
	[InlineData( false, "[[:space:]]", " ", 0, 1 )]
	[InlineData( true, "[[:^space:]]", "\u00a0", -1, 0 )]
	[InlineData( true, "[^[:space:]]", "😀", 0, 4 )]
	[InlineData( true, "[^[:^space:]]", "\u00a0", 0, 2 )]
	[InlineData( true, "[[:word:]]", "é", 0, 2 )]
	[InlineData( true, "[[:word:]]", "\u0301", 0, 2 )]
	[InlineData( true, "[[:word:]]", "²", 0, 2 )]
	[InlineData( false, "[[:word:]]", "é", -1, 0 )]
	[InlineData( false, "[[:word:]]", "_", 0, 1 )]
	[InlineData( true, "[[:^word:]]", "é", -1, 0 )]
	[InlineData( true, "[[:^word:]]", "😀", 0, 4 )]
	[InlineData( true, "[^[:word:]]", "😀", 0, 4 )]
	[InlineData( true, "[^[:^word:]]", "é", 0, 2 )]
	[InlineData( true, "[^[:^word:]]", "", -1, 0 )]
	public void StandalonePosixSpaceAndWordUseScalarPredicates(
		bool ucp, string pattern, string subject, int offset, int length
	) {
		var result = Compile( pattern, new PcreByteOptions {
			Utf8 = true, Ucp = ucp, AsciiBsD = true
		} ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( offset < 0 ? null : offset, result.Match?.ByteIndex );
		if ( offset >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void GrepWordSpaceBackreferencePreservesUtf8CaptureBytes() {
		var compiled = Compile( @"^(\w+)\s+\1$", GrepOptions );
		var result = compiled.Match( "été\u00a0été"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 0, 12 ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 5 ), result.Match?.Captures[1] );
		Assert.Null( compiled.Match( "été\u00a0ete"u8.ToArray() ).Match );
	}

	[Fact]
	public void FailedWordBranchRestoresCaptureBeforeSpaceAlternative() {
		var result = Compile( @"(\w+)\s+\1|([[:space:]]+)", GrepOptions ).Match( "\u00a0"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 0, 2 ), result.Match?.Captures[0] );
		Assert.Null( result.Match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 2 ), result.Match?.Captures[2] );
	}

	[Theory]
	[InlineData( "[[:space:]_]" )]
	[InlineData( "[[:word:]0]" )]
	[InlineData( "[[:alpha:]]" )]
	[InlineData( "(?aS)\\s" )]
	public void BroaderUtfBracketAndSpaceOptionSyntaxRemainUnsupported( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), GrepOptions );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void WhitespacePredicateHonorsWorkAndCancellationLimits() {
		var limited = Compile( "\\s", new PcreByteOptions {
			Utf8 = true, Ucp = true, MatchWorkLimit = 4
		} );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			limited.Match( "\u00a0"u8.ToArray() ).Diagnostic?.Code );
		var exact = Compile( "\\s", new PcreByteOptions {
			Utf8 = true, Ucp = true, MatchWorkLimit = 5
		} );
		Assert.Equal( new PcreByteCapture( 0, 2 ), exact.Match( "\u00a0"u8.ToArray() ).Match?.Captures[0] );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( "[[:space:]]", new PcreByteOptions {
			Utf8 = true, Ucp = true,
			WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () =>
			cancellable.Match( "\u00a0"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
