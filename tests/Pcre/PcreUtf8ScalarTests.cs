using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>UTF-8 scalar and byte-span fixtures for the internal, strict PCRE mode.</summary>
public sealed class PcreUtf8ScalarTests {
	[Theory]
	[InlineData( "é", "aéz", 1, 2 )]
	[InlineData( "😀", "x😀y", 1, 4 )]
	[InlineData( "^é$", "é", 0, 2 )]
	[InlineData( "é|😀", "x😀é", 1, 4 )]
	[InlineData( "\\xFF", "aÿz", 1, 2 )]
	public void LiteralMatchesWholeScalarWithByteSpan( string pattern, string subject, int index, int length ) {
		var result = Compile( Encoding.UTF8.GetBytes( pattern ) ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index, result.Match?.ByteIndex );
		Assert.Equal( length, result.Match?.ByteLength );
	}

	[Theory]
	[InlineData( ".", "é", 0, 2 )]
	[InlineData( ".", "😀", 0, 4 )]
	[InlineData( "^.$", "😀", 0, 4 )]
	[InlineData( ".", "\né", 1, 2 )]
	[InlineData( ".", "\n", -1, 0 )]
	[InlineData( "(?s).", "\n", 0, 1 )]
	[InlineData( "(?s)a.b", "a\nb", 0, 3 )]
	[InlineData( "^..$", "é😀", 0, 6 )]
	[InlineData( ".", "\U0010FFFF", 0, 4 )]
	public void DotConsumesOneScalarAndHonorsLf( string pattern, string subject, int index, int length ) {
		var result = Compile( Encoding.UTF8.GetBytes( pattern ) ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index < 0 ? null : index, result.Match?.ByteIndex );
		if ( index >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void CapturesAndBackreferencesRetainSourceBytePositions() {
		var result = Compile( "(é|😀)+\\1"u8.ToArray() ).Match( "xé😀😀"u8.ToArray() );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( 1, result.Match?.ByteIndex );
		Assert.Equal( 10, result.Match?.ByteLength );
		Assert.Equal( new PcreByteCapture( 3, 4 ), result.Match?.Captures[1] );
	}

	[Fact]
	public void StartOffsetMustBeAtScalarBoundary() {
		var compiled = Compile( "😀"u8.ToArray() );
		var input = "é😀"u8.ToArray();
		Assert.Equal( 2, compiled.Match( input, startOffset: 2 ).Match?.ByteIndex );
		var inside = compiled.Match( input, startOffset: 3 );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, inside.Diagnostic?.Code );
		Assert.Equal( 3, inside.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void ZeroWidthMatchesRemainOnCharacterBoundaries() {
		var compiled = Compile( Array.Empty<byte>() );
		var input = "é😀"u8.ToArray();
		Assert.Equal( 2, compiled.Match( input, startOffset: 2 ).Match?.ByteIndex );
		Assert.Equal( input.Length, compiled.Match( input, startOffset: input.Length ).Match?.ByteIndex );
	}

	[Fact]
	public void AsciiWordBoundariesSeeAdjacentNonAsciiScalarsAsNonWords() {
		var result = Compile( "\\bA\\b"u8.ToArray() ).Match( "éA😀"u8.ToArray() );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( 2, result.Match?.ByteIndex );
		Assert.Equal( 1, result.Match?.ByteLength );
	}

	[Theory]
	[InlineData( "FF", 0 )]
	[InlineData( "61C0AF", 1 )]
	[InlineData( "EDB080", 0 )]
	[InlineData( "F09F98", 0 )]
	[InlineData( "E282", 0 )]
	[InlineData( "80", 0 )]
	[InlineData( "6180", 1 )]
	[InlineData( "F4908080", 0 )]
	public void InvalidPatternReportsFirstSequenceOffset( string hex, int offset ) {
		var result = PcreByteCompiler.Compile( Convert.FromHexString( hex ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "78FF79", 1 )]
	[InlineData( "61C0AF", 1 )]
	[InlineData( "EDB080", 0 )]
	[InlineData( "F09F98", 0 )]
	[InlineData( "80", 0 )]
	[InlineData( "6180", 1 )]
	[InlineData( "F4908080", 0 )]
	public void InvalidSubjectReportsFirstSequenceOffset( string hex, int offset ) {
		var result = Compile( "x"u8.ToArray() ).Match( Convert.FromHexString( hex ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
		Assert.Null( result.Match );
	}

	[Theory]
	[InlineData( "\\p{L}", 0 )]
	[InlineData( "\\d", 0 )]
	[InlineData( "[é]", 0 )]
	[InlineData( "(?<=é)x", 0 )]
	[InlineData( "(?i)é", 0 )]
	public void UnicodeSensitiveConstructsRemainExplicitlyUnsupported( string pattern, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void CaselessOptionIsRejectedUntilUnicodeCaseFoldingExists() {
		var result = PcreByteCompiler.Compile( "é"u8.ToArray(), new PcreByteOptions { Utf8 = true, Caseless = true } );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
	}

	[Fact]
	public void ByteModeStillMatchesOneHighByteAndCanStartWithinUtf8Sequence() {
		var bytePattern = PcreByteCompiler.Compile( new byte[] { 0xA9 } );
		Assert.True( bytePattern.IsSuccess, bytePattern.Diagnostic?.Message );
		var matched = bytePattern.Pattern!.Match( "é"u8.ToArray(), startOffset: 1 );
		Assert.Equal( 1, matched.Match?.ByteIndex );
		Assert.Equal( 1, matched.Match?.ByteLength );
	}

	[Fact]
	public void SubjectValidationConsumesWorkAndHonorsCancellation() {
		var input = Encoding.UTF8.GetBytes( new string( 'é', 50 ) );
		var limited = Compile( "z"u8.ToArray(), new PcreByteOptions { Utf8 = true, MatchWorkLimit = 10 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Match( input ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "z"u8.ToArray(), new PcreByteOptions {
			Utf8 = true, WorkObserver = count => { if ( count == 8 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( input, cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( pattern, options ?? new PcreByteOptions { Utf8 = true } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
