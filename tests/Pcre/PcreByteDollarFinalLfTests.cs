using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Default byte-mode dollar behavior with an LF newline convention.</summary>
public sealed class PcreByteDollarFinalLfTests {
	[Theory]
	[InlineData( "foo$", "foo\n", 0, 3 )]
	[InlineData( "foo$", "foo", 0, 3 )]
	[InlineData( "foo$", "foo\n\n", -1, 0 )]
	[InlineData( "foo$", "foo\nbar", -1, 0 )]
	[InlineData( "$", "x\n\n", 2, 0 )]
	[InlineData( "$", "\n", 0, 0 )]
	[InlineData( "$", "", 0, 0 )]
	[InlineData( "^foo$", "foo\n", 0, 3 )]
	[InlineData( "^foo$", "foo\n\n", -1, 0 )]
	[InlineData( "(?s)^foo$", "foo\n", 0, 3 )]
	[InlineData( "foo$\\n", "foo\n", 0, 4 )]
	[InlineData( "foo\\z", "foo\n", -1, 0 )]
	[InlineData( "foo\\Z", "foo\n", 0, 3 )]
	[InlineData( "[$]", "x$", 1, 1 )]
	public void DollarTestsSubjectEndOrBeforeOneFinalLfWithoutConsumingIt(
		string pattern, string subject, int index, int length
	) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ) )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : index, result.Match?.ByteIndex );
		if ( index >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void NonzeroSearchOffsetDoesNotMoveSubjectEnd() {
		var beforeLf = Compile( "$"u8.ToArray() ).Match( "a\n"u8.ToArray(), startOffset: 1 );
		Assert.True( beforeLf.IsSuccess, beforeLf.Diagnostic?.Message );
		Assert.Equal( 1, beforeLf.Match?.ByteIndex );
		var atEnd = Compile( "$"u8.ToArray() ).Match( "a\n"u8.ToArray(), startOffset: 2 );
		Assert.True( atEnd.IsSuccess, atEnd.Diagnostic?.Message );
		Assert.Equal( 2, atEnd.Match?.ByteIndex );
		var strict = Compile( "a\\z"u8.ToArray() ).Match( "a\n"u8.ToArray(), startOffset: 1 );
		Assert.True( strict.IsSuccess, strict.Diagnostic?.Message );
		Assert.Null( strict.Match );
	}

	[Fact]
	public void FinalLfAssertionPreservesRawByteCaptures() {
		var pattern = new byte[] { (byte)'(', 0xff, 0, (byte)')', (byte)'$' };
		var result = Compile( pattern ).Match( new byte[] { 0xff, 0, (byte)'\n' } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( 0, result.Match?.ByteIndex );
		Assert.Equal( 2, result.Match?.ByteLength );
		Assert.Equal( new PcreByteCapture( 0, 2 ), result.Match?.Captures[1] );
	}

	[Fact]
	public void RejectsDirectDollarRepetition() {
		var result = PcreByteCompiler.Compile( "$+"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( 1, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void LongSearchRemainsBoundedAndCancellable() {
		var input = System.Text.Encoding.ASCII.GetBytes( new string( 'x', 80 ) );
		var limited = Compile( "x$"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 12 } ).Match( input );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );

		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "x$"u8.ToArray(), new PcreByteOptions {
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
