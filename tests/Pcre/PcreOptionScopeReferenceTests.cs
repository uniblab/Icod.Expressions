using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Compile-time option inheritance and instruction-local casing against pinned PCRE2.</summary>
public sealed class PcreOptionScopeReferenceTests {
	[Theory]
	[InlineData( "a(?i)b", "6142", false, 0, 2 )]
	[InlineData( "a(?i)b", "4162", false, -1, 0 )]
	[InlineData( "(?i:a)b", "4162", false, 0, 2 )]
	[InlineData( "(?i:a)b", "4142", false, -1, 0 )]
	[InlineData( "(?i)a(?-i)b", "4162", false, 0, 2 )]
	[InlineData( "(?i)a(?-i)b", "4142", false, -1, 0 )]
	[InlineData( "(?i:a(?-i:b)c)d", "41624364", false, 0, 4 )]
	[InlineData( "(?i:a(?-i:b)c)d", "41424364", false, -1, 0 )]
	[InlineData( "(a(?i)b)c", "614263", false, 0, 3 )]
	[InlineData( "(a(?i)b)c", "614243", false, -1, 0 )]
	[InlineData( "(a(?i)b|c)d", "4364", false, 0, 2 )]
	[InlineData( "(a(?i)b|c)d", "4344", false, -1, 0 )]
	[InlineData( "(a(?i)b)|c", "43", false, -1, 0 )]
	[InlineData( "a(?i)b|c", "43", false, 0, 1 )]
	[InlineData( "(?|(a(?i)b)|(c))d", "4364", false, -1, 0 )]
	[InlineData( "(?|(a)(?i)b|(c))d", "4364", false, 0, 2 )]
	[InlineData( @"(a)(?i:\1)", "6141", false, 0, 2 )]
	[InlineData( @"(?i:(a))\1", "4161", false, -1, 0 )]
	[InlineData( @"(?i:(a))(?-i:\1)", "4141", false, 0, 2 )]
	[InlineData( @"(?<x>a)(?i:\k<x>)(?-i:\g{x})", "614161", false, 0, 3 )]
	[InlineData( @"(é)(?i:\g{1})(?-i:\g{1})", "C3A9C389C3A9", true, 0, 6 )]
	[InlineData( @"(?i:(é))\g{1}", "C389C3A9", true, -1, 0 )]
	[InlineData( @"(K)(?i:\g{1})", "4BE284AA", true, 0, 4 )]
	[InlineData( "(?i:[a-c])(?-i:[A-C])", "6242", false, 0, 2 )]
	[InlineData( "(?i:[a-c])(?-i:[A-C])", "6262", false, -1, 0 )]
	[InlineData( "(?i:[K])(?-i:[K])", "E284AA4B", true, 0, 4 )]
	[InlineData( "(?i:[K])(?-i:[K])", "E284AAE284AA", true, -1, 0 )]
	[InlineData( @"(?i:\p{Lu})(?-i:\p{Lu})", "6141", true, 0, 2 )]
	[InlineData( @"(?i:\p{Lu})(?-i:\p{Lu})", "6161", true, -1, 0 )]
	[InlineData( @"(?i:[\p{Lu}0])(?-i:[\p{Lu}0])", "6141", true, 0, 2 )]
	[InlineData( "(?i:(?<=a))b", "4162", false, 1, 1 )]
	[InlineData( "(?i:(?<=é))x", "C38978", true, 2, 1 )]
	[InlineData( "(?i:(?=a))a", "41", false, -1, 0 )]
	[InlineData( "(?i:(?=a)a)b", "4162", false, 0, 2 )]
	[InlineData( "(?s:.)", "0A", false, 0, 1 )]
	[InlineData( "(?s:.).", "0A0A", false, -1, 0 )]
	[InlineData( ".(?s:.)", "610A", false, 0, 2 )]
	[InlineData( "(?s).(?-s:.)", "0A61", false, 0, 2 )]
	[InlineData( "(?s).(?-s:.)", "0A0A", false, -1, 0 )]
	[InlineData( "(?m:^a$)", "780A610A7A", false, 2, 1 )]
	[InlineData( "(?m:^a)$", "780A610A7A", false, -1, 0 )]
	[InlineData( "(?ms:^a.b$)", "780A610A620A7A", false, 2, 3 )]
	[InlineData( "(?ms-s:^a.b$)", "610A62", false, -1, 0 )]
	[InlineData( "(?i:a)+a", "41414161", false, 0, 4 )]
	[InlineData( "(?i:a)+a", "41414141", false, -1, 0 )]
	[InlineData( "(?i:a)++a", "414161", false, -1, 0 )]
	[InlineData( "^(?:(?i)a|(?-i)b)+$", "4162", false, 0, 2 )]
	[InlineData( "^(?:(?i)a|(?-i)b)+$", "4142", false, -1, 0 )]
	[InlineData( "(?:(?i:(?>a))b|(?-i:Ac))", "4163", false, 0, 2 )]
	[InlineData( @"(?i:\Q[a]\E)b", "5B415D62", false, 0, 4 )]
	[InlineData( "(?i:[(?i)])", "49", false, 0, 1 )]
	[InlineData( "a(?i)", "61", false, 0, 1 )]
	[InlineData( "a(?i)", "41", false, -1, 0 )]
	[InlineData( "(?i:a(?-i))b", "4162", false, 0, 2 )]
	[InlineData( "(?i:){2}", "", false, 0, 0 )]
	public void LocalOptionsKeepPinnedWholeAndCaptureSpans( string pattern, string hex,
		bool utf8, int expectedIndex, int expectedLength ) {
		var bytes = Convert.FromHexString( hex );
		var reference = Reference( pattern, utf8 );
		var native = reference.Match( bytes );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( expectedLength, native.Length ); }
		Compare( reference, bytes, 0, Compile( pattern, new PcreByteOptions { Utf8 = utf8 } ).Match( bytes ) );
	}

	[Fact]
	public void CallerCaselessIsInheritedAndRestoredAfterLocalUnsetting() {
		foreach ( var pattern in new[] { "(?-i:a)b", "(?-i:a(?i:b))c", @"(?<X>a)(?-i:\k<X>)(?i:\g{X})" } ) {
			foreach ( var text in new[] { "aB", "AB", "aBC", "aaa", "aAa", "aaA" } ) {
				var bytes = Encoding.ASCII.GetBytes( text );
				Compare( Reference( pattern, false, callerCaseless: true ), bytes, 0,
					Compile( pattern, new PcreByteOptions { Caseless = true } ).Match( bytes ) );
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void LocalOptionsRetainEverySelectedInvalidUtfAndCrLfOffset( bool anyCrLf ) {
		foreach ( var pattern in new[] { "(?i:a)|b", @"(?i:(a?))(?-i:\1)", "(?s:.)", "(?m:^a$)", "(?i:(?<=a))b" } ) {
			var reference = Reference( pattern, true, invalidUtf: true, anyCrLf: anyCrLf );
			var managed = Compile( pattern, new PcreByteOptions { MatchInvalidUtf = true, AnyCrLfNewline = anyCrLf } );
			foreach ( var hex in new[] { "", "6141", "61FF41", "C3A9", "FFC3A9FF", "0D0A610D0A", "C3FF62" } ) {
				var bytes = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= bytes.Length; offset++ ) {
					Compare( reference, bytes, offset, managed.Match( bytes, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Fact]
	public void ScopesKeepDepthWorkAndCancellationPolicies() {
		var nested = string.Concat( Enumerable.Repeat( "(?i:", 256 ) ) + "a" + new string( ')', 256 );
		Assert.Equal( 1, Compile( nested ).Match( "A"u8.ToArray() ).Match?.ByteLength );
		var tooDeep = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( "(?i:" + nested + ")" ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, tooDeep.Diagnostic?.Code );
		Assert.Equal( 1024, tooDeep.Diagnostic?.ByteOffset );
		var subject = "aaaa"u8.ToArray();
		Assert.Null( Compile( "x(?i:y)", new PcreByteOptions { MatchWorkLimit = 15 } ).Match( subject ).Diagnostic );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( "x(?i:y)", new PcreByteOptions { MatchWorkLimit = 14 } ).Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( "x(?i:y)", new PcreByteOptions {
			WorkObserver = count => { if ( count == 5 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( "a(?i)+", 5 )]
	[InlineData( "(?m)*", 4 )]
	[InlineData( "a(?s){2}", 5 )]
	public void StandaloneOptionChangesCannotBeQuantified( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, false ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?)a", 0 )]
	[InlineData( "(?^i:a)", 0 )]
	[InlineData( "a(?J)b", 1 )]
	[InlineData( @"(?aD:\d)", 0 )]
	[InlineData( @"(?i)(?aD:\d)", 4 )]
	[InlineData( "(?i:(?=a|b))", 4 )]
	[InlineData( "(?=(?i:a))", 0 )]
	public void OtherOptionsAndGeneralAssertionBodiesRemainPending( string pattern, int offset ) {
		Reference( pattern, true );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset,
		PcreByteMatchResult actual, string context = "" ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( native.Success ? native.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, actual.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var captured = native.Groups[group];
			Assert.Equal( captured.Success ? new PcreByteCapture( captured.Index, captured.Length ) : null,
				actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8, bool callerCaseless = false,
		bool invalidUtf = false, bool anyCrLf = false ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		if ( callerCaseless ) { options |= PcreOptions.Caseless; }
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = options } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var encoding = options?.Utf8 == true || options?.MatchInvalidUtf == true ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
