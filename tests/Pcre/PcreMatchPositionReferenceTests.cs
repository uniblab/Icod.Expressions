using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Requested search starts and journaled reported starts against pinned PCRE2.</summary>
public sealed class PcreMatchPositionReferenceTests {
	[Theory]
	[InlineData( @"\Ga", "6261", false, 0, -1, 0 )]
	[InlineData( @"\Ga", "6261", false, 1, 1, 1 )]
	[InlineData( @"(?m)\Ga", "0A61", false, 0, -1, 0 )]
	[InlineData( @"(?m)\Ga", "0A61", false, 1, 1, 1 )]
	[InlineData( @"\Ga|b", "7862", false, 0, 1, 1 )]
	[InlineData( @"a\G", "61", false, 0, -1, 0 )]
	[InlineData( @"\G", "616263", false, 2, 2, 0 )]
	[InlineData( @"\G\K", "616263", false, 2, 2, 0 )]
	[InlineData( @"\Ga\Kb", "786162", false, 1, 2, 1 )]
	[InlineData( @"a\K\G", "61", false, 0, -1, 0 )]
	[InlineData( @"(foo)\Kbar", "7878666F6F626172", false, 0, 5, 3 )]
	[InlineData( @"^foo\Kbar", "666F6F626172", false, 0, 3, 3 )]
	[InlineData( @"foo\K", "666F6F", false, 0, 3, 0 )]
	[InlineData( @"foo\Kbar\Kbaz", "666F6F62617262617A", false, 0, 6, 3 )]
	[InlineData( @"(a)\K\g{1}", "6161", false, 0, 1, 1 )]
	[InlineData( @"(?:a\Kb|ac)", "6163", false, 0, 0, 2 )]
	[InlineData( @"a(?:\Kb|c)", "6163", false, 0, 0, 2 )]
	[InlineData( @"(?:(?>a\K)b|ac)", "6163", false, 0, 0, 2 )]
	[InlineData( @"(?:(a)\Kb|a(c))", "6163", false, 0, 0, 2 )]
	[InlineData( @"a\Kz|b", "6162", false, 0, 1, 1 )]
	[InlineData( @"(?:a\K)+a", "616161", false, 0, 2, 1 )]
	[InlineData( @"(?:a\K)+?a", "616161", false, 0, 1, 1 )]
	[InlineData( @"(?:a\K)++a", "616161", false, 0, -1, 0 )]
	[InlineData( @"(?:a\K){1,2}a", "616161", false, 0, 2, 1 )]
	[InlineData( @"(?:(a)\K)+\g{1}", "616161", false, 0, 2, 1 )]
	[InlineData( @"\Kx", "6178", false, 0, 1, 1 )]
	[InlineData( @"(?i)(é)\K\g{1}", "C389C3A9", true, 0, 2, 2 )]
	[InlineData( @"(é)\K😀", "C3A9F09F9880", true, 0, 2, 4 )]
	[InlineData( @"(\xFF)\K\x00", "FF00", false, 0, 1, 1 )]
	[InlineData( @"\Q\G\K\E", "5C475C4B", false, 0, 0, 4 )]
	public void SuccessfulPathKeepsPinnedWholeAndNumberedCaptureSpans( string pattern, string subjectHex,
		bool utf8, int offset, int expectedIndex, int expectedLength ) {
		var subject = Convert.FromHexString( subjectHex );
		var reference = Reference( pattern, utf8: utf8, invalidUtf: utf8 );
		var native = reference.Match( subject, offset );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( expectedLength, native.Length ); }
		var actual = Compile( pattern, new PcreByteOptions { Utf8 = utf8, MatchInvalidUtf = utf8 } ).Match( subject, offset );
		AssertEquivalent( reference, subject, offset, actual );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void EveryOffsetPreservesReferenceEmptyAndConsumingPaths( bool utf8, bool anyCrLf ) {
		foreach ( var pattern in new[] { @"\G", @"\K", @"\G\K", @"\G(?:a|.)", @"(?:a\K)?", @"(a?)\K",
			@"\Ka", @"\Ga", @"a\K", @"\G\K(?:a|b)", @"(?:\G|a)\K", @"a?\K|b", @"\G|\K" } ) {
			var reference = Reference( pattern, utf8: utf8, invalidUtf: utf8, anyCrLf: anyCrLf );
			var managed = Compile( pattern, new PcreByteOptions { Utf8 = utf8, MatchInvalidUtf = utf8,
				AnyCrLfNewline = anyCrLf } );
			foreach ( var hex in new[] { "", "61", "6261", "FF", "61FF62", "C3A9", "61C3A962", "C3FF61", "0D0A61", "61FF" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					AssertEquivalent( reference, subject, offset, managed.Match( subject, offset ),
						$"{pattern}, {hex}, offset {offset}, UTF={utf8}, newline={anyCrLf}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"[\G]", 1 )]
	[InlineData( @"[\K]", 1 )]
	[InlineData( @"\G*", 2 )]
	[InlineData( @"\K+", 2 )]
	[InlineData( @"\G{1}", 2 )]
	[InlineData( @"\K{0}", 2 )]
	[InlineData( @"(?=\K)", 0 )]
	[InlineData( @"(?!\K)", 0 )]
	[InlineData( @"(?<=\K)a", 0 )]
	public void InvalidContextsHaveControlledByteDiagnostics( string pattern, int expectedOffset ) {
		foreach ( var utf8 in new[] { false, true } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern, utf8: utf8 ) );
			var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new PcreByteOptions { Utf8 = utf8 } );
			Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
			Assert.Equal( expectedOffset, result.Diagnostic?.ByteOffset );
		}
	}

	[Fact]
	public void SearchStartInsideLookbehindRemainsUnsupported() {
		var result = PcreByteCompiler.Compile( @"(?<=\G)"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( @"\G" )]
	[InlineData( @"\K" )]
	public void StrictUtfValidationAndOffsetsArePreserved( string pattern ) {
		var compiled = Compile( pattern, new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( Convert.FromHexString( "FF" ) ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( "é"u8.ToArray(), 1 ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( @"\G" )]
	[InlineData( @"\K" )]
	public void PositionInstructionsSpendWorkAndHonorCancellation( string pattern ) {
		var subject = Array.Empty<byte>();
		var limited = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 2 } ).Match( subject );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		Assert.Equal( new PcreByteCapture( 0, 0 ),
			Compile( pattern, new PcreByteOptions { MatchWorkLimit = 3 } ).Match( subject ).Match?.Captures[0] );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new PcreByteOptions {
			WorkObserver = count => { if ( count == 2 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void ResetPrefixKeepsMissingLiteralFastPathBudget() {
		Assert.Null( Compile( @"\Kx", new PcreByteOptions { MatchWorkLimit = 8 } ).Match( "a"u8.ToArray() ).Diagnostic );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( @"\Kx", new PcreByteOptions { MatchWorkLimit = 7 } ).Match( "a"u8.ToArray() ).Diagnostic?.Code );
	}

	private static void AssertEquivalent( PcreRegex8Bit reference, byte[] subject, int offset,
		PcreByteMatchResult actual, string context = "" ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		Assert.True( ( actual.Match?.ByteIndex ?? -1 ) == ( native.Success ? native.Index : -1 ), context );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, actual.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var capture = native.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null,
				actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8 = false, bool invalidUtf = false, bool anyCrLf = false ) {
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf | PcreOptions.Ucp; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
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
