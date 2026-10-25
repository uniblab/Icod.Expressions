using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 UTF/UCP decimal-digit fixtures for the current Grep consumer contract.</summary>
public sealed class PcreUtf8DigitTests {
	private static readonly PcreByteOptions GrepOptions = new() { Utf8 = true, Ucp = true, AsciiBsD = true };

	[Fact]
	public void ReproducesGrepsAsciiSlashDAndUnicodePosixDigitContract() {
		var arabic = "٣"u8.ToArray();
		Assert.Null( Compile( "^\\d$"u8.ToArray(), GrepOptions ).Match( arabic ).Match );
		Assert.Equal( 0, Compile( "^\\d$"u8.ToArray(), GrepOptions ).Match( "3"u8.ToArray() ).Match?.ByteIndex );
		Assert.Equal( new PcreByteCapture( 0, 2 ), Compile( "^[[:digit:]]$"u8.ToArray(), GrepOptions ).Match( arabic ).Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 2 ), Compile( "(?-aD)^\\d$"u8.ToArray(), GrepOptions ).Match( arabic ).Match?.Captures[0] );
	}

	[Theory]
	[InlineData( "\\p{Nd}+", "x٣１7Ⅻ", 1, 6 )]
	[InlineData( "\\P{Nd}+", "٣é😀7", 2, 6 )]
	[InlineData( "\\p{^Nd}+", "٣é😀7", 2, 6 )]
	[InlineData( "\\P{^Nd}+", "x٣7!", 1, 3 )]
	[InlineData( "\\p{Nd}", "²", -1, 0 )]
	[InlineData( "\\p{Nd}", "Ⅻ", -1, 0 )]
	[InlineData( "\\p{Nd}", "", -1, 0 )]
	public void ExplicitNdPropertyConsumesWholeScalar( string pattern, string input, int offset, int length ) {
		var result = Compile( Encoding.UTF8.GetBytes( pattern ), GrepOptions ).Match( Encoding.UTF8.GetBytes( input ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( offset < 0 ? null : offset, result.Match?.ByteIndex );
		if ( offset >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Theory]
	[InlineData( false, false, "\\d", "٣", -1, 0 )]
	[InlineData( true, false, "\\d", "٣", 0, 2 )]
	[InlineData( true, true, "\\d", "٣", -1, 0 )]
	[InlineData( true, true, "\\D", "٣", 0, 2 )]
	[InlineData( true, false, "\\D", "٣", -1, 0 )]
	[InlineData( true, false, "\\D", "😀", 0, 4 )]
	[InlineData( true, true, "\\p{Nd}", "٣", 0, 2 )]
	[InlineData( false, false, "\\p{Nd}", "٣", 0, 2 )]
	[InlineData( true, true, "\\P{Nd}", "é", 0, 2 )]
	[InlineData( true, false, "(?aD)\\d", "٣", -1, 0 )]
	[InlineData( true, true, "(?-aD)\\d", "٣", 0, 2 )]
	[InlineData( false, false, "(?-aD)\\d", "٣", -1, 0 )]
	public void SlashDRestrictionIsIndependentOfExplicitNd( bool ucp, bool asciiBsD, string pattern,
		string input, int offset, int length ) {
		var result = Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions {
			Utf8 = true, Ucp = ucp, AsciiBsD = asciiBsD
		} ).Match( Encoding.UTF8.GetBytes( input ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( offset < 0 ? null : offset, result.Match?.ByteIndex );
		if ( offset >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Theory]
	[InlineData( false, false, "[[:digit:]]", "٣", -1, 0 )]
	[InlineData( true, false, "[[:digit:]]", "٣", 0, 2 )]
	[InlineData( true, true, "[[:digit:]]", "٣", 0, 2 )]
	[InlineData( true, true, "[[:^digit:]]", "٣", -1, 0 )]
	[InlineData( true, true, "[^[:digit:]]", "٣", -1, 0 )]
	[InlineData( true, true, "[^[:^digit:]]", "٣", 0, 2 )]
	[InlineData( true, true, "[[:^digit:]]", "é", 0, 2 )]
	[InlineData( true, true, "[[:^digit:]]", "😀", 0, 4 )]
	[InlineData( false, false, "[[:^digit:]]", "٣", 0, 2 )]
	public void StandalonePosixDigitHasIndependentUcpAndNegation( bool ucp, bool asciiBsD,
		string pattern, string input, int offset, int length ) {
		var result = Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions {
			Utf8 = true, Ucp = ucp, AsciiBsD = asciiBsD
		} ).Match( Encoding.UTF8.GetBytes( input ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( offset < 0 ? null : offset, result.Match?.ByteIndex );
		if ( offset >= 0 ) { Assert.Equal( length, result.Match?.ByteLength ); }
	}

	[Fact]
	public void FailedAsciiDigitPathRestoresCaptureBeforeUnicodePosixAlternative() {
		var result = Compile( "(\\d+)x|([[:digit:]]+)"u8.ToArray(), GrepOptions ).Match( "3٣"u8.ToArray() );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( new PcreByteCapture( 0, 3 ), result.Match?.Captures[0] );
		Assert.Null( result.Match?.Captures[1] );
		Assert.Equal( new PcreByteCapture( 0, 3 ), result.Match?.Captures[2] );
	}

	[Fact]
	public void NegatedDigitAtomsDoNotConsumeEndOfSubject() {
		foreach ( var pattern in new[] { "\\D", "\\P{Nd}", "[[:^digit:]]" } ) {
			var result = Compile( Encoding.UTF8.GetBytes( pattern ), GrepOptions ).Match( Array.Empty<byte>() );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.Null( result.Match );
		}
	}

	[Theory]
	[InlineData( "[[:digit:]_]", 0 )]
	[InlineData( "[0-9]", 0 )]
	[InlineData( "[[:alpha:]]", 0 )]
	[InlineData( "[\\p{Nd}]", 0 )]
	[InlineData( "(?-aD:\\d)", 0 )]
	[InlineData( "a(?-aD)\\d", 1 )]
	[InlineData( "(?m)(?-aD)\\d", 4 )]
	[InlineData( "\\p{N}", 0 )]
	public void OutsideDigitSubsetRemainsExplicitlyUnsupported( string pattern, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), GrepOptions );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void UcpWithoutUtf8IsNotSilentlyInterpretedAsByteMode() {
		var result = PcreByteCompiler.Compile( "\\d"u8.ToArray(), new PcreByteOptions { Ucp = true } );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
	}

	[Fact]
	public void UnicodeDigitPredicateIsBudgetedAndCancellable() {
		var limited = Compile( "\\p{Nd}"u8.ToArray(), new PcreByteOptions {
			Utf8 = true, Ucp = true, MatchWorkLimit = 4
		} );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Match( "٣"u8.ToArray() ).Diagnostic?.Code );
		var exact = Compile( "\\p{Nd}"u8.ToArray(), new PcreByteOptions {
			Utf8 = true, Ucp = true, MatchWorkLimit = 5
		} );
		Assert.Equal( 2, exact.Match( "٣"u8.ToArray() ).Match?.ByteLength );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "[[:digit:]]"u8.ToArray(), new PcreByteOptions {
			Utf8 = true, Ucp = true,
			WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( "٣"u8.ToArray(), cancellationToken: cancellation.Token ) );
		var longInput = Encoding.UTF8.GetBytes( new string( '٣', 50 ) );
		var bounded = Compile( "\\p{Nd}+"u8.ToArray(), new PcreByteOptions {
			Utf8 = true, Ucp = true, MatchWorkLimit = 65
		} );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, bounded.Match( longInput ).Diagnostic?.Code );
	}

	private static PcreCompiledBytePattern Compile( byte[] pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( pattern, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
