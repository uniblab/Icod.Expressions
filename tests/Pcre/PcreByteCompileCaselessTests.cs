using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Caller-supplied ASCII caseless compilation for the internal byte matcher.</summary>
public sealed class PcreByteCompileCaselessTests {
	[Theory]
	[InlineData( "abc", "aBc", false, -1, 0 )]
	[InlineData( "abc", "aBc", true, 0, 3 )]
	[InlineData( "foo|bar", "BAR", true, 0, 3 )]
	[InlineData( "(?s)a.b", "A\nB", true, 0, 3 )]
	[InlineData( "(?i)abc", "aBc", false, 0, 3 )]
	[InlineData( "(?i)abc", "aBc", true, 0, 3 )]
	[InlineData( "\\x41", "a", true, 0, 1 )]
	[InlineData( "[a-z]+", "ABC", true, 0, 3 )]
	[InlineData( "[^a]", "A", true, -1, 0 )]
	[InlineData( "[[:lower:]]+", "ABC", true, 0, 3 )]
	[InlineData( "[[:^lower:]]", "A", true, -1, 0 )]
	[InlineData( "\\d", "A", true, -1, 0 )]
	public void CallerOptionControlsAsciiCaseAcrossCompiledConstructs(
		string pattern, string subject, bool caseless, int index, int length
	) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ), new PcreByteOptions { Caseless = caseless } )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : index, result.Match?.ByteIndex );
		if ( index >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void CallerOptionPreservesOriginalCapturesInBackreferences() {
		var result = Compile( "(a)\\1"u8.ToArray(), new PcreByteOptions { Caseless = true } )
			.Match( "aA"u8.ToArray() );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( 2, result.Match?.ByteLength );
		Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match?.Captures[1] );
	}

	[Theory]
	[InlineData( "(?<=ab)CD", "aBcD", 2, 2 )]
	[InlineData( "(?=ab)AB", "aB", 0, 2 )]
	[InlineData( "(?!ab)AB", "aB", -1, 0 )]
	public void CallerOptionControlsFixedLiteralAssertions( string pattern, string subject, int index, int length ) {
		var result = Compile( System.Text.Encoding.ASCII.GetBytes( pattern ), new PcreByteOptions { Caseless = true } )
			.Match( System.Text.Encoding.ASCII.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : index, result.Match?.ByteIndex );
		if ( index >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void HighBytesAndNulRemainExact() {
		var options = new PcreByteOptions { Caseless = true };
		var match = Compile( new byte[] { 0xff, 0, (byte)'a' }, options )
			.Match( new byte[] { 0xc0, 0xff, 0, (byte)'A' } );
		Assert.True( match.IsSuccess, match.Diagnostic?.Message );
		Assert.Equal( 1, match.Match?.ByteIndex );
		Assert.Equal( 3, match.Match?.ByteLength );
		var noMatch = Compile( new byte[] { 0xc0 }, options ).Match( new byte[] { 0xe0 } );
		Assert.True( noMatch.IsSuccess, noMatch.Diagnostic?.Message );
		Assert.Null( noMatch.Match );
	}

	[Fact]
	public void CallerOptionDoesNotEnableUnsupportedFlagUnsetting() {
		var result = PcreByteCompiler.Compile( "(?-i)a"u8.ToArray(), new PcreByteOptions { Caseless = true } );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void CallerOptionRetainsWorkLimitAndCancellationDuringReferenceComparison() {
		var pattern = System.Text.Encoding.ASCII.GetBytes( "(" + new string( 'a', 128 ) + @")\1" );
		var input = System.Text.Encoding.ASCII.GetBytes( new string( 'a', 128 ) + new string( 'A', 128 ) );
		var limited = Compile( pattern, new PcreByteOptions { Caseless = true, MatchWorkLimit = 300 } ).Match( input );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );

		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions {
			Caseless = true,
			WorkObserver = count => { if ( count == 300 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( input, cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( pattern, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
