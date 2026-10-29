using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Direct assertion repetition uses finite PCRE2 bounds, not a no-progress loop.</summary>
public sealed class PcreAssertionRepeatReferenceTests {
	[Theory]
	[InlineData( "^(?=a)*a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?=b)*a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?=b)+a$", "61", -1, 0, "" )]
	[InlineData( "^(?=a)+a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?!a)*a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?!a)+a$", "61", -1, 0, "" )]
	[InlineData( "^(?!b){3,}a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?=[a])?a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?=[b]){0}a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?=[b]){0,0}+a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?=[a]){2}a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?=[a]){2,4}?a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?=[a]){,2}a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?=[a]){ 2 , 3 }+a$", "61", 0, 1, "0:1" )]
	[InlineData( @"^(?=(\1a|a))+\1$", "6161", 0, 2, "0:2,0:2" )]
	[InlineData( @"^(?=(\1a|a))+?\1$", "6161", 0, 2, "0:2,0:2" )]
	[InlineData( @"^(?=(\1a|a))++\1$", "6161", 0, 2, "0:2,0:2" )]
	[InlineData( @"^(?=(\1a|a)){3,}\1$", "61616161", 0, 4, "0:4,0:4" )]
	[InlineData( @"^(?=(\1a|a)){3,}?\1$", "61616161", 0, 4, "0:4,0:4" )]
	[InlineData( @"^(?=(\1a|a)){3,}+\1$", "61616161", 0, 4, "0:4,0:4" )]
	[InlineData( @"^(?=(\1a|a)){1,4}\1$", "616161", 0, 3, "0:3,0:3" )]
	[InlineData( @"^(?=(\1a|a)){1,4}?\1$", "616161", 0, 3, "0:3,0:3" )]
	[InlineData( @"^(?=(\1a|a)){1,4}+\1$", "616161", -1, 0, "" )]
	[InlineData( @"^(?=(\1a|a))+a\1$", "6161", 0, 2, "0:2,0:1" )]
	[InlineData( @"^(?=(\1a|a))++a\1$", "6161", -1, 0, "" )]
	[InlineData( "(?=(a))*", "61", 0, 0, "0:0,0:1" )]
	[InlineData( "(?=(a))*?", "61", 0, 0, "0:0,-" )]
	[InlineData( "(?U)(?=(a))*", "61", 0, 0, "0:0,-" )]
	[InlineData( "(?U)(?=(a))*?", "61", 0, 0, "0:0,0:1" )]
	[InlineData( "(?U)(?=(a))*+", "61", 0, 0, "0:0,0:1" )]
	[InlineData( @"(?U)^(?=(\1a|a))+\1", "6161", 0, 1, "0:1,0:1" )]
	[InlineData( @"(?U)^(?=(\1a|a))+?\1", "6161", 0, 2, "0:2,0:2" )]
	[InlineData( @"(?U)^(?=(\1a|a))++\1", "6161", 0, 2, "0:2,0:2" )]
	[InlineData( "^(?=(a)){0}(b)$", "62", 0, 1, "0:1,-,0:1" )]
	[InlineData( "^(?!(a)){0}(b)$", "62", 0, 1, "0:1,-,0:1" )]
	[InlineData( "^(?!(a)){2,3}b$", "62", 0, 1, "0:1,-" )]
	[InlineData( "^(?:(?=(a))+b|a)$", "61", 0, 1, "0:1,-" )]
	[InlineData( @"^(?=(a|ab)){2}\1c$", "616263", -1, 0, "" )]
	[InlineData( @"^(?=(a|ab)c){2}\1c$", "616263", 0, 3, "0:3,0:2" )]
	[InlineData( @"^(?=(?<x>\k<x>a|a)){3,}\k<x>$", "61616161", 0, 4, "0:4,0:4" )]
	[InlineData( @"^(?=(?|(a)|(b))){2}\1$", "62", 0, 1, "0:1,0:1" )]
	[InlineData( "^(?=(?=[a])+[a]){2}a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?!(?=[b])+[b]){2}a$", "61", 0, 1, "0:1" )]
	[InlineData( "^(?x)(?= (a) ) # between\n {2,3} # suffix\n ? a$", "61", 0, 1, "0:1,0:1" )]
	[InlineData( "^(?U:(?-U:(?=(a))))?a$", "61", 0, 1, "0:1,0:1" )]
	[InlineData( "a(?<=a)+b", "6162", 0, 2, "0:2" )]
	[InlineData( "a(?<!a)*b", "6162", 0, 2, "0:2" )]
	[InlineData( "a(?<!a)+b", "6162", -1, 0, "" )]
	[InlineData( "a(?<=a){3,4}+b", "6162", 0, 2, "0:2" )]
	[InlineData( "a(?<=b){0}b", "6162", 0, 2, "0:2" )]
	public void DirectRepeatsKeepFiniteBoundsCapturesAndRetryPreference( string pattern, string hex,
		int expectedIndex, int expectedLength, string expectedCaptures ) =>
		AssertOutcome( pattern, hex, false, expectedIndex, expectedLength, expectedCaptures );

	[Theory]
	[InlineData( @"^(?=(\1é|é)){3,}\1$", "C3A9C3A9C3A9C3A9", "0:8,0:8" )]
	[InlineData( @"^(?=(\1😀|😀))+\1$", "F09F9880F09F9880", "0:8,0:8" )]
	[InlineData( @"(?i)^(?=(é)){2}\1$", "C389", "0:2,0:2" )]
	[InlineData( "é(?<=é){2,}😀", "C3A9F09F9880", "0:6" )]
	[InlineData( "é(?<!😀)*😀", "C3A9F09F9880", "0:6" )]
	public void UtfRepeatsRetainSourceByteCaptureSpans( string pattern, string hex, string captures ) =>
		AssertOutcome( pattern, hex, true, 0, Convert.FromHexString( hex ).Length, captures );

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void SelectedCallerOffsetsKeepFragmentsAndNewlineSemantics( bool utf8, bool anyCrLf ) {
		foreach ( var pattern in new[] { @"(?=a)*", @"(?=(a))+", @"(?=(a))??", @"(?!a){2,}",
			@"(?<=a)+", @"(?<!a){0,2}?", @"(?=\G[a]?){2}", @"(?=(\R)){2}\1",
			@"(?=\B)*", @"(?=\b)+", @"(?!(?<=a)){1,2}", @"(?=(.)){0,2}",
			@"(?=(\1a|a)){1,3}\1", @"(?=(a)){0}", @"(?=(a)){0}+" } ) {
			var reference = Reference( pattern, utf8, utf8, anyCrLf );
			var compiled = Compile( pattern, new PcreByteOptions {
				Utf8 = utf8, MatchInvalidUtf = utf8, AnyCrLfNewline = anyCrLf
			} );
			foreach ( var hex in new[] { "", "61", "616161", "80FF", "61FF", "61FFFF62", "C3A9",
				"61C3A961", "F09F9880FF61", "0D0A61", "610D0A", "C3FF61" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ),
						$"{pattern}, {hex}, offset {offset}, UTF={utf8}, ANYCRLF={anyCrLf}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( "(?=[a]){2,1}", 7 )]
	[InlineData( "(?=[a]){65536}", 7 )]
	[InlineData( "(?=[a]){2", 7 )]
	[InlineData( "(?=[a])**", 8 )]
	[InlineData( "(?=[a])+?+", 9 )]
	public void InvalidQuantifiersRetainSourceByteDiagnostics( string pattern, int offset ) {
		var actual = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?=[a]){65535,}", 7 )]
	[InlineData( "(?=[a]){65535}", 7 )]
	[InlineData( "(?=[a]){0,65535}", 7 )]
	public void FiniteExpansionRetainsBoundedCompileFailure( string pattern, int offset ) {
		var actual = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, actual.Diagnostic?.Code );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void DirectFiniteAssertionsDoNotSpendUnboundedProgressMarks() {
		// 300 direct pluses need no progress marks; each contributes two literal checks.
		var pattern = string.Concat( Enumerable.Repeat( "(?=a)+", 300 ) ) + "a";
		var result = Compile( pattern ).Match( "a"u8.ToArray() );
		Assert.Null( result.Diagnostic );
		Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match?.Captures[0] );
	}

	[Fact]
	public void RepeatedAssertionsConsumeWorkAndObserveCancellation() {
		const string pattern = "^(?=([a])){30}a$";
		var subject = "a"u8.ToArray();
		var limited = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 20 } ).Match( subject );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions {
			WorkObserver = work => { if ( work == 20 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
	}

	private static void AssertOutcome( string pattern, string hex, bool utf8,
		int expectedIndex, int expectedLength, string captures ) {
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern, utf8 );
		var native = reference.Match( subject );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) {
			Assert.Equal( expectedLength, native.Length );
			var spans = captures.Split( ',' );
			Assert.Equal( spans.Length, native.Groups.Count );
			for ( var group = 0; group < spans.Length; group++ ) {
				var capture = native.Groups[group];
				Assert.Equal( spans[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
			}
		}
		Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions { Utf8 = utf8 } ).Match( subject ), pattern );
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset,
		PcreByteMatchResult actual, string context ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( native.Success ? native.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}; native index {( native.Success ? native.Index : -1 )}, managed index {actual.Match?.ByteIndex ?? -1}" );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, actual.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var capture = native.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null,
				actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8, bool invalidUtf = false, bool anyCrLf = false ) {
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = options } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var encoding = options?.Utf8 == true || options?.MatchInvalidUtf == true ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
