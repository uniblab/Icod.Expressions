using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 non-newline matching, counted repetitions and Unicode-codepoint disambiguation.</summary>
public sealed class PcreNonNewlineReferenceTests {
	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void EveryByteUsesNewlineConventionAndIgnoresDotAll( bool anyCrLf, bool dotAll ) {
		var pattern = ( dotAll ? "(?s)" : "" ) + @"\A\N\z";
		var native = Reference( pattern, anyCrLf: anyCrLf );
		var managed = Compile( pattern, new PcreByteOptions { AnyCrLfNewline = anyCrLf } );
		for ( var value = 0; value < 256; value++ ) {
			var expected = value != 0x0A && ( !anyCrLf || value != 0x0D );
			var subject = new[] { (byte)value };
			Assert.True( native.Match( subject ).Success == expected, $"byte {value:X2}: reference" );
			var result = managed.Match( subject );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.True( ( result.Match is not null ) == expected, $"byte {value:X2}, newline={anyCrLf}, dotall={dotAll}" );
			if ( expected ) { Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match!.Captures[0] ); }
		}
		Assert.Null( managed.Match( ReadOnlyMemory<byte>.Empty ).Match );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void Utf8ConsumesWholeScalarsIndependentOfUcpAndBsr( bool anyCrLf, bool dotAll ) {
		int[] probes = { 0, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x85, 0xA0, 0xE9, 0x180E, 0x2028, 0x2029, 0x1F600, 0x10FFFF };
		foreach ( var ucp in new[] { false, true } ) {
		foreach ( var bsr in new[] { false, true } ) {
			var pattern = ( dotAll ? "(?is)" : "" ) + @"\A\N\z";
			var native = Reference( pattern, utf8: true, ucp: ucp, anyCrLf: anyCrLf, bsr: bsr );
			var managed = Compile( pattern, new PcreByteOptions { Utf8 = true, Ucp = ucp,
				AsciiBsD = true, AnyCrLfNewline = anyCrLf, BsrAnyCrLf = bsr } );
			foreach ( var scalar in probes ) {
				var subject = Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) );
				var expected = scalar != 0x0A && ( !anyCrLf || scalar != 0x0D );
				Assert.True( native.Match( subject ).Success == expected, $"U+{scalar:X}: reference" );
				var actual = managed.Match( subject );
				Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
				Assert.True( ( actual.Match is not null ) == expected, $"U+{scalar:X}, newline={anyCrLf}, dotall={dotAll}" );
				if ( expected ) { Assert.Equal( new PcreByteCapture( 0, subject.Length ), actual.Match!.Captures[0] ); }
			}
			Assert.Null( managed.Match( ReadOnlyMemory<byte>.Empty ).Match );
		}
		}
	}

	[Theory]
	[InlineData( @"(\N{2,3})", "61626364", false, 0, 3 )]
	[InlineData( @"\N{2,3}?a", "62626161", false, 0, 3 )]
	[InlineData( @"\N{2,3}+a", "626261", false, -1, 0 )]
	[InlineData( @"\N{,2}", "6162", false, 0, 2 )]
	[InlineData( @"\N{0}", "", false, 0, 0 )]
	[InlineData( @"\N{2,}", "610A62", false, -1, 0 )]
	[InlineData( @"\N{ 2 , 3 }", "616263", false, 0, 3 )]
	[InlineData( @"(?s)(\N)+", "61620A6364", false, 0, 2 )]
	[InlineData( @"(?s)\N.\N", "610A62", false, 0, 3 )]
	[InlineData( @"\N*+\n", "610A", false, 0, 2 )]
	[InlineData( @"(\N{2})", "C3A9F09F9880", true, 0, 6 )]
	[InlineData( @"\N+", "61FF62", true, 0, 1 )]
	[InlineData( @"\N{2}", "61FF62", true, -1, 0 )]
	[InlineData( @"(\N{U+1F600})", "F09F9880", true, 0, 4 )]
	[InlineData( @"[\N{U+0061}-\N{U+0063}]", "62", true, 0, 1 )]
	[InlineData( @"(?i)\N{U+00E9}", "C389", true, 0, 2 )]
	[InlineData( @"(?<=\N{U+1F600})x", "F09F988078", true, 4, 1 )]
	[InlineData( @"(?s)\N{U+000A}", "0A", true, 0, 1 )]
	[InlineData( @"\N{ U+0041 }", "41", true, 0, 1 )]
	[InlineData( @"\N{U+0000000000001F600}", "F09F9880", true, 0, 4 )]
	[InlineData( @"(\N{U+1F600})\g{1}", "F09F9880F09F9880", true, 0, 8 )]
	[InlineData( @"\N{U+0000}", "00", true, 0, 1 )]
	[InlineData( @"[^\N{U+000A}]", "0A", true, -1, 0 )]
	public void CountedAndCodepointFormsPreserveReferenceCaptureSpans( string pattern, string subjectHex,
		bool utf8, int expectedIndex, int expectedLength ) {
		var subject = Convert.FromHexString( subjectHex );
		var native = Reference( pattern, utf8: utf8, invalidUtf: utf8 );
		var expected = native.Match( subject );
		Assert.Equal( expectedIndex, expected.Success ? expected.Index : -1 );
		if ( expected.Success ) { Assert.Equal( expectedLength, expected.Length ); }
		var actual = Compile( pattern, new PcreByteOptions { Utf8 = utf8, MatchInvalidUtf = utf8 } ).Match( subject );
		AssertEquivalent( native, subject, 0, actual );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void EveryOffsetAroundCrLfAndMalformedUtfMatchesReference( bool utf8, bool anyCrLf ) {
		var patterns = new List<string> { @"\N", @"\N+", @"(?s)\N.\N", @"\B\N", @"\b\N", @"(\N)?b", @"\N{2}", @"\N|\r" };
		if ( utf8 ) { patterns.Add( @"\N{U+000A}" ); }
		foreach ( var pattern in patterns ) {
			var native = Reference( pattern, utf8: utf8, invalidUtf: utf8, anyCrLf: anyCrLf );
			var managed = Compile( pattern, new PcreByteOptions { Utf8 = utf8, MatchInvalidUtf = utf8, AnyCrLfNewline = anyCrLf } );
			foreach ( var hex in new[] { "610D0A62", "C3A90D0AF09F9880", "61FF62", "FF0D0AFF", "E280A861" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					AssertEquivalent( native, subject, offset, managed.Match( subject, offset ),
						$"{pattern}, {hex}, offset {offset}, UTF={utf8}, newline={anyCrLf}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"[\N]", false, 1 )]
	[InlineData( @"[\N]", true, 1 )]
	[InlineData( @"[\N{2}]", true, 1 )]
	[InlineData( @"\N{U+0041}", false, 0 )]
	[InlineData( @"\N{U+D800}", true, 0 )]
	[InlineData( @"\N{U+110000}", true, 0 )]
	[InlineData( @"\N{U+}", true, 0 )]
	[InlineData( @"\N{U+0041", true, 0 )]
	[InlineData( @"\N{U+ 0041}", true, 0 )]
	[InlineData( @"\N{u+0041}", true, 0 )]
	[InlineData( @"\N{LATIN CAPITAL LETTER A}", true, 0 )]
	[InlineData( @"\N{}", true, 0 )]
	[InlineData( @"\N{,}", true, 0 )]
	[InlineData( @"\N{2,1}", true, 2 )]
	public void InvalidFormsRetainControlledByteDiagnostics( string pattern, bool utf8, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = utf8 } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void BareNonNewlineInFixedLookbehindKeepsTheSubjectPosition() {
		var result = Compile( @"(?<=\N)x" ).Match( "ax"u8.ToArray() );
		Assert.Null( result.Diagnostic );
		Assert.Equal( new PcreByteCapture( 1, 1 ), result.Match?.Captures[0] );
	}

	[Fact]
	public void NonNewlineUsesExistingCompileWorkAndCancellationLimits() {
		var expansion = PcreByteCompiler.Compile( @"\N{65535}"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, expansion.Diagnostic?.Code );
		Assert.Equal( 2, expansion.Diagnostic?.ByteOffset );
		var subject = "é"u8.ToArray();
		var limited = Compile( @"\N", new PcreByteOptions { Utf8 = true, MatchWorkLimit = 4 } ).Match( subject );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		Assert.Equal( new PcreByteCapture( 0, 2 ),
			Compile( @"\N", new PcreByteOptions { Utf8 = true, MatchWorkLimit = 5 } ).Match( subject ).Match?.Captures[0] );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( @"\N", new PcreByteOptions { Utf8 = true,
			WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
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

	private static PcreRegex8Bit Reference( string pattern, bool utf8 = false, bool ucp = false,
		bool invalidUtf = false, bool anyCrLf = false, bool bsr = false ) {
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( ucp ) { options |= PcreOptions.Ucp; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		var prefix = ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + ( bsr ? "(*BSR_ANYCRLF)" : "(*BSR_UNICODE)" );
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		return new PcreRegex8Bit( encoding.GetBytes( prefix + pattern ), encoding,
			new PcreRegexSettings { Options = options } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
