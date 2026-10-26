using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreAtomicTests {
	[Theory]
	[InlineData( "^(?>a|ab)c$", "abc", false )]
	[InlineData( "^(?>a|ab)b$", "ab", true )]
	[InlineData( "^(?>ab|a)c$", "ac", true )]
	[InlineData( "^(?:a|ab)c$", "abc", true )]
	[InlineData( "^(?:a|ab)(?>b)c$", "abbc", true )]
	[InlineData( "^(?>a+?)a$", "aaa", false )]
	[InlineData( "^a*+a$", "aa", false )]
	[InlineData( "^a++a$", "aa", false )]
	[InlineData( "^a?+a$", "a", false )]
	[InlineData( "^a{1,2}+a$", "aa", false )]
	[InlineData( "^a{1,2}+a$", "aaa", true )]
	[InlineData( "^a{2}+a$", "aaa", true )]
	[InlineData( "^a{1,}+a$", "aa", false )]
	[InlineData( "^(?:)*+a$", "a", true )]
	[InlineData( "^(a|ab)++c$", "abc", false )]
	[InlineData( "^(?>(?>a|ab)|ab)c$", "abc", false )]
	public void AtomicSectionsCommitTheirFirstSuccessfulPath( string pattern, string subject, bool matches ) {
		var result = Compile( pattern ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( matches, result.Match is not null );
	}

	[Fact]
	public void OuterAlternativeCanRecoverAndRollBackAtomicCaptures() {
		var result = Compile( "^(?:(?>(a|ab))d|abc)$" ).Match( "abc"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 0, 3 ), result.Match?.Captures[0] );
		Assert.Null( result.Match?.Captures[1] );
	}

	[Fact]
	public void CaptureInsideAtomicGroupRetainsOriginalByteSpan() {
		var result = Compile( "^(?>(ab|a))c$" ).Match( "abc"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 0, 2 ), result.Match?.Captures[1] );
	}

	[Fact]
	public void AtomicGroupRespectsUtf8ScalarBoundaries() {
		var compiled = Compile( "^(?>é|éé)x$", new PcreByteOptions { Utf8 = true } );
		Assert.Null( compiled.Match( "ééx"u8.ToArray() ).Match );
		Assert.Equal( new PcreByteCapture( 0, 3 ),
			Compile( "^(?>é+)x$", new PcreByteOptions { Utf8 = true } )
				.Match( "éx"u8.ToArray() ).Match?.Captures[0] );
	}

	[Fact]
	public void AtomicGroupPreservesHighByteAndNul() {
		var pattern = new byte[] { (byte)'^', (byte)'(', (byte)'?', (byte)'>', 0xFF, (byte)'|', 0xFF, 0x00, (byte)')', 0x00, (byte)'$' };
		var compiled = PcreByteCompiler.Compile( pattern );
		Assert.True( compiled.IsSuccess, compiled.Diagnostic?.Message );
		Assert.Null( compiled.Pattern?.Match( new byte[] { 0xFF, 0x00, 0x00 } ).Match );
	}

	[Theory]
	[InlineData( "a*?+" )]
	[InlineData( "a++?" )]
	[InlineData( "a{1,2}?+" )]
	public void RejectsLazyPossessiveMixtures( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
	}

	[Fact]
	public void AtomicTransitionsStillSpendMatchWork() {
		var compiled = Compile( "^(?>a|b)c$", new PcreByteOptions { MatchWorkLimit = 2 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			compiled.Match( "ac"u8.ToArray() ).Diagnostic?.Code );
	}

	[Fact]
	public void CancellationInterruptsAtomicExecution() {
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "^(?>a|b)c$", new PcreByteOptions {
			WorkObserver = count => { if ( count == 5 ) cancellation.Cancel(); }
		} );
		Assert.Throws<OperationCanceledException>( () =>
			compiled.Match( "ac"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
