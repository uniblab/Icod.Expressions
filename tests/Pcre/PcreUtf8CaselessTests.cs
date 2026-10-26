using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>UTF scalar case equivalence in the provisional internal matcher.</summary>
public sealed class PcreUtf8CaselessTests {
	[Theory]
	[InlineData( "(?i)É", "xé", false, 1, 2 )]
	[InlineData( "É", "xé", true, 1, 2 )]
	[InlineData( "É", "xé", false, -1, 0 )]
	[InlineData( "(?i)k", "K", false, 0, 3 )]
	[InlineData( "(?i)K", "K", false, 0, 1 )]
	[InlineData( "(?i)s", "ſ", false, 0, 2 )]
	[InlineData( "(?i)Σ", "ς", false, 0, 2 )]
	[InlineData( "(?i)ς", "σ", false, 0, 2 )]
	[InlineData( "(?i)I", "ı", false, -1, 0 )]
	[InlineData( "(?i)i", "İ", false, -1, 0 )]
	[InlineData( "(?i)İ", "i", false, -1, 0 )]
	[InlineData( "(?i)ı", "I", false, -1, 0 )]
	[InlineData( "(?i)I", "i", false, 0, 1 )]
	[InlineData( "(?i)İ", "İ", false, 0, 2 )]
	public void MatchesSimpleCaseEquivalenceAndSubjectByteWidth(
		string pattern, string subject, bool callerCaseless, int expectedOffset, int expectedLength
	) {
		var result = Compile( pattern, callerCaseless ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( expectedOffset < 0 ? null : expectedOffset, result.Match?.ByteIndex );
		if ( expectedOffset >= 0 ) { Assert.Equal( expectedLength, result.Match?.ByteLength ); }
	}

	[Theory]
	[InlineData( "(?i)\\p{Lu}", "a", 0, 1 )]
	[InlineData( "(?i)\\p{Ll}", "A", 0, 1 )]
	[InlineData( "(?i)\\p{Lt}", "ǆ", 0, 2 )]
	[InlineData( "(?i)\\P{Lu}", "a", -1, 0 )]
	[InlineData( "(?i)\\p{^Ll}", "A", -1, 0 )]
	[InlineData( "(?i)\\P{Lu}", "1", 0, 1 )]
	[InlineData( "(?i)\\p{M}", "A", -1, 0 )]
	public void CasedCategoriesBecomeLcAndOtherPropertiesStayIndependent(
		string pattern, string subject, int expectedOffset, int expectedLength
	) {
		var result = Compile( pattern ).Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( expectedOffset < 0 ? null : expectedOffset, result.Match?.ByteIndex );
		if ( expectedOffset >= 0 ) { Assert.Equal( expectedLength, result.Match?.ByteLength ); }
	}

	[Fact]
	public void CaselessPropertyDoesNotRequireUcpAndShorthandPolicyStaysAscii() {
		var property = Compile( "\\p{Lu}" , callerCaseless: true ).Match( "é"u8.ToArray() );
		Assert.Equal( 2, property.Match?.ByteLength );
		var word = Compile( "(?i)\\w" ).Match( "é"u8.ToArray() );
		Assert.True( word.IsSuccess, word.Diagnostic?.Message );
		Assert.Null( word.Match );
	}

	[Fact]
	public void CaselessLiteralsCannotJoinValidFragmentsAcrossInvalidUtf() {
		var result = Compile( "(?i)éa", matchInvalidUtf: true )
			.Match( new byte[] { 0xC3, 0xA9, 0xFF, (byte)'A' } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Null( result.Match );
		var after = Compile( "(?i)a", matchInvalidUtf: true )
			.Match( new byte[] { 0xC3, 0xA9, 0xFF, (byte)'A' } );
		Assert.Equal( 3, after.Match?.ByteIndex );
	}

	[Fact]
	public void CaselessScalarComparisonsSpendWorkAndAreCancellable() {
		var input = Encoding.UTF8.GetBytes( new string( 'É', 80 ) );
		var limited = Compile( "(?i)z", workLimit: 30 ).Match( input );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( "(?i)z", observer: count => {
			if ( count == 30 ) { cancellation.Cancel(); }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( input, cancellationToken: cancellation.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern, bool callerCaseless = false,
		bool matchInvalidUtf = false, int workLimit = PcreByteOptions.DefaultMatchWorkLimit,
		Action<int>? observer = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions {
			Utf8 = true, Caseless = callerCaseless, MatchInvalidUtf = matchInvalidUtf,
			MatchWorkLimit = workLimit, WorkObserver = observer
		} );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
