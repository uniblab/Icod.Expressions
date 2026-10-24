using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 multiline line anchors under the provisional LF convention.</summary>
public sealed class PcreByteLeadingMultilineTests {
	[Theory]
	[InlineData( "^foo", "bar\nfoo\n", -1, 0 )]
	[InlineData( "(?m)^foo", "bar\nfoo\n", 4, 3 )]
	[InlineData( "foo$", "foo\nbar", -1, 0 )]
	[InlineData( "(?m)foo$", "foo\nbar", 0, 3 )]
	[InlineData( "(?m)^foo$", "bar\nfoo\nbaz", 4, 3 )]
	[InlineData( "(?m)^foo$", "foo\n", 0, 3 )]
	[InlineData( "(?m)^foo$", "foo\n\n", 0, 3 )]
	[InlineData( "(?m)$\\n", "foo\nbar", 3, 1 )]
	[InlineData( "(?m)\\n^", "x\ny", 1, 1 )]
	[InlineData( "(?m)\\n^", "x\n", -1, 0 )]
	[InlineData( "(?m)^", "", 0, 0 )]
	[InlineData( "(?m)$", "", 0, 0 )]
	[InlineData( "(?m)^foo", "bar\rfoo", -1, 0 )]
	[InlineData( "(?m)foo$", "foo\rbar", -1, 0 )]
	[InlineData( "(?m)\\Afoo", "bar\nfoo", -1, 0 )]
	[InlineData( "(?m)foo\\Z", "foo\nbar", -1, 0 )]
	[InlineData( "(?m)foo\\z", "foo\n", -1, 0 )]
	[InlineData( "(?m)[$^]", "x$", 1, 1 )]
	public void MultilineAnchorsUseLfWithoutMovingSubjectEdges( string pattern, string subject, int index, int length ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : index, result.Match?.ByteIndex );
		if ( index >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void SearchOffsetDoesNotTurnFinalLfIntoLineStart() {
		var internalLine = Compile( "(?m)^"u8.ToArray() ).Match( "a\nb"u8.ToArray(), startOffset: 2 );
		Assert.True( internalLine.IsSuccess, internalLine.Diagnostic?.Message );
		Assert.Equal( 2, internalLine.Match?.ByteIndex );
		var finalLf = Compile( "(?m)^"u8.ToArray() ).Match( "a\n"u8.ToArray(), startOffset: 2 );
		Assert.True( finalLf.IsSuccess, finalLf.Diagnostic?.Message );
		Assert.Null( finalLf.Match );
		var dollar = Compile( "(?m)$"u8.ToArray() ).Match( "a\nb"u8.ToArray(), startOffset: 1 );
		Assert.True( dollar.IsSuccess, dollar.Diagnostic?.Message );
		Assert.Equal( 1, dollar.Match?.ByteIndex );
	}

	[Fact]
	public void AlternativeRestoresCaptureAfterLineAssertionSucceeds() {
		var result = Compile( "(?m)(a)$x|(a)$"u8.ToArray() ).Match( "a\n"u8.ToArray() );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( 0, result.Match?.ByteIndex );
		Assert.Equal( 1, result.Match?.ByteLength );
		Assert.Null( result.Match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match?.Captures[2] );
	}

	[Fact]
	public void MultilinePreservesCallerCaselessAndRawBytePositions() {
		var caseless = Compile( "(?m)^foo$"u8.ToArray(), new PcreByteOptions { Caseless = true } )
			.Match( "x\nFOO\ny"u8.ToArray() );
		Assert.True( caseless.IsSuccess, caseless.Diagnostic?.Message );
		Assert.Equal( 2, caseless.Match?.ByteIndex );
		Assert.Equal( 3, caseless.Match?.ByteLength );
		var bytes = new byte[] { (byte)'(', (byte)'?', (byte)'m', (byte)')', (byte)'^', 0xff, 0, (byte)'$' };
		var result = Compile( bytes ).Match( new byte[] { (byte)'x', (byte)'\n', 0xff, 0, (byte)'\n', (byte)'z' } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( 2, result.Match?.ByteIndex );
		Assert.Equal( 2, result.Match?.ByteLength );
	}

	[Theory]
	[InlineData( "a(?m)^", 1 )]
	[InlineData( "(?m:^)", 0 )]
	[InlineData( "(?im)^", 0 )]
	[InlineData( "(?m)(?m)^", 4 )]
	[InlineData( "(?m)(?s)^", 4 )]
	public void UnsupportedFlagPlacementRemainsExplicit( string pattern, int offset ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?m)^+" )]
	[InlineData( "(?m)$?" )]
	public void RejectsDirectLineAnchorRepetition( string pattern ) {
		var result = PcreByteCompiler.Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( 5, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void LongSearchRemainsBoundedAndCancellable() {
		var input = System.Text.Encoding.ASCII.GetBytes( new string( 'x', 80 ) );
		var limited = Compile( "(?m)^y$"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 12 } ).Match( input );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "(?m)^y$"u8.ToArray(), new PcreByteOptions {
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
