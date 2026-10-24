using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 8-bit subject start and end assertions with LF newline convention.</summary>
public sealed class PcreByteSubjectAnchorTests {
	[Theory]
	[InlineData( "\\Afoo", "foo", 0, 3 )]
	[InlineData( "\\Afoo", "xfoo", -1, 0 )]
	[InlineData( "foo\\z", "foo", 0, 3 )]
	[InlineData( "foo\\z", "foo\n", -1, 0 )]
	[InlineData( "foo\\Z", "foo\n", 0, 3 )]
	[InlineData( "foo\\Z", "foo", 0, 3 )]
	[InlineData( "foo\\Z", "foo\n\n", -1, 0 )]
	[InlineData( "\\Z", "x\n\n", 2, 0 )]
	[InlineData( "\\Z", "\n", 0, 0 )]
	[InlineData( "\\z", "\n", 1, 0 )]
	[InlineData( "\\A", "", 0, 0 )]
	[InlineData( "\\Z", "", 0, 0 )]
	[InlineData( "\\z", "", 0, 0 )]
	public void MatchesSubjectEdgesWithoutConsumingBytes( string pattern, string subject, int index, int length ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : index, result.Match?.ByteIndex );
		if ( index >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void SearchOffsetDoesNotMoveAbsoluteSubjectEdges() {
		Assert.Null( Compile( "\\Afoo"u8.ToArray() ).Match( "foo"u8.ToArray(), startOffset: 1 ).Match );
		Assert.Equal( 3, Compile( "\\z"u8.ToArray() ).Match( "foo"u8.ToArray(), startOffset: 2 ).Match?.ByteIndex );
		Assert.Equal( 3, Compile( "\\Z"u8.ToArray() ).Match( "foo\n"u8.ToArray(), startOffset: 3 ).Match?.ByteIndex );
	}

	[Fact]
	public void AlternativeRestoresCaptureAfterEarlierAnchorSuccess() {
		var match = Compile( "(a)\\Zx|(a)\\z"u8.ToArray() ).Match( "a"u8.ToArray() ).Match;
		Assert.Equal( 0, match?.ByteIndex );
		Assert.Equal( 1, match?.ByteLength );
		Assert.Equal( 3, match?.Captures.Count );
		Assert.Null( match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 1 ), match?.Captures[2] );
	}

	[Fact]
	public void RawNulAndHighBytesKeepExactOffsets() {
		var pattern = new byte[] { (byte)'\\', (byte)'A', 0xff, 0, (byte)'\\', (byte)'z' };
		var match = Compile( pattern ).Match( new byte[] { 0xff, 0 } ).Match;
		Assert.Equal( 0, match?.ByteIndex );
		Assert.Equal( 2, match?.ByteLength );
	}

	[Theory]
	[InlineData( "[\\A]" )]
	[InlineData( "[\\Z]" )]
	[InlineData( "[\\z]" )]
	public void BracketAnchorEscapesAreUnsupported( string pattern ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 1, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "\\A+" )]
	[InlineData( "\\Z*" )]
	[InlineData( "\\z?" )]
	public void RejectsDirectAnchorRepetition( string pattern ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( 2, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void LongSearchRemainsBoundedAndCancellable() {
		var input = System.Text.Encoding.ASCII.GetBytes( new string( 'x', 80 ) );
		var limited = Compile( "x\\Z"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 12 } ).Match( input );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );

		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "x\\Z"u8.ToArray(), new PcreByteOptions {
			WorkObserver = count => { if ( count == 8 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( input, cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( pattern, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
