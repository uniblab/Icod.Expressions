using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Compile-time name aliases compared with pinned PCRE2 numbered spans.</summary>
public sealed class PcreNamedCaptureReferenceTests {
	[Fact]
	public void EveryDeclarationAndReferenceSpellingSharesNumberedCaptures() {
		foreach ( var declaration in new[] { "(?<word>a+)", "(?'word'a+)", "(?P<word>a+)" } ) {
			foreach ( var reference in new[] { @"\k<word>", @"\k'word'", @"\k{word}", @"\g{word}", "(?P=word)" } ) {
				AssertEquivalent( "^(b)" + declaration + reference + @"\g{2}\g{-1}$", "6261616161", false, 0, 0, 5 );
			}
		}
	}

	[Theory]
	[InlineData( @"(?<x>a)\k<x>", "786161", false, 0, 1, 2 )]
	[InlineData( @"(?<x>a)\k<x>", "6161", false, 1, -1, 0 )]
	[InlineData( @"(?<x>a)?b\g{x}", "62", false, 0, -1, 0 )]
	[InlineData( @"(?<x>)\k{x}", "", false, 0, 0, 0 )]
	[InlineData( @"\k<x>(?<x>a)", "61", false, 0, -1, 0 )]
	[InlineData( @"^(?:\k<x>|(?<x>a))+$", "6161", false, 0, 0, 2 )]
	[InlineData( @"^(?:(?P=x)|(?P<x>a))+$", "6161", false, 0, 0, 2 )]
	[InlineData( @"(?<x>a|ab)\g{x}", "61626162", false, 0, 0, 4 )]
	[InlineData( @"(?:(?<x>a)b|a(?<y>c))\k<y>", "616363", false, 0, 0, 3 )]
	[InlineData( @"(?<x>a)+a\k<x>", "616161", false, 0, 0, 3 )]
	[InlineData( @"(?<x>a)+?a\k<x>", "616161", false, 0, 0, 3 )]
	[InlineData( @"(?<x>a)++a\k<x>", "616161", false, 0, -1, 0 )]
	[InlineData( @"(?:(?>(?<x>a))b|a(?<y>c))\g{y}", "616363", false, 0, 0, 3 )]
	[InlineData( @"(?<x>a){1,2}\k<x>", "616161", false, 0, 0, 3 )]
	[InlineData( @"(?<x>a)\K\k<x>", "6161", false, 0, 1, 1 )]
	[InlineData( @"(?<outer>(?<inner>a)b)\g{inner}\g{outer}", "6162616162", false, 0, 0, 5 )]
	[InlineData( @"(?<x>\xFF)\k{x}", "FFFF", false, 0, 0, 2 )]
	[InlineData( @"(?<x>\x00)\k'x'", "0000", false, 0, 0, 2 )]
	[InlineData( @"(?i)(?<X>a)(?<x>b)\k<X>\k<x>", "41626142", false, 0, 0, 4 )]
	[InlineData( @"(?<é>é)\k<é>", "C3A9C3A9", true, 0, 0, 4 )]
	[InlineData( @"(?i)(?<é>é)\k<é>", "C389C3A9", true, 0, 0, 4 )]
	[InlineData( @"(?<字٢>😀)\g{字٢}", "F09F9880F09F9880", true, 0, 0, 8 )]
	[InlineData( @"(?<𐐀>é)(?P=𐐀)", "C3A9C3A9", true, 0, 0, 4 )]
	[InlineData( @"(?<_1>a)\k<_1>", "6161", true, 0, 0, 2 )]
	[InlineData( @"(?<x>a)|(?<X>b)", "62", true, 0, 0, 1 )]
	public void NamesKeepOrderedNumberedCaptureSemantics( string pattern, string hex, bool utf8,
		int offset, int expectedIndex, int expectedLength ) =>
		AssertEquivalent( pattern, hex, utf8, offset, expectedIndex, expectedLength );

	[Fact]
	public void InvalidUtfFragmentAndOffsetBehaviorIsUnchanged() {
		foreach ( var pattern in new[] { @"(?<x>a?)\k<x>", @"(?<x>.)\g{x}", @"(?:\k<x>|(?<x>a))+" } ) {
			var reference = Reference( pattern, true, true );
			var compiled = Compile( pattern, new PcreByteOptions { MatchInvalidUtf = true } );
			foreach ( var hex in new[] { "", "61", "6161", "61FF61", "FFC3A9C3A9", "C3FFC3", "F09F9880F09F9880" } ) {
				var bytes = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= bytes.Length; offset++ ) {
					Compare( reference, bytes, offset, compiled.Match( bytes, offset ) );
				}
			}
		}
	}

	[Theory]
	[InlineData( "", false )]
	[InlineData( "1x", false )]
	[InlineData( "a-b", false )]
	[InlineData( "a b", false )]
	[InlineData( "é", false )]
	[InlineData( "٢x", true )]
	[InlineData( "á", true )]
	[InlineData( "😀", true )]
	[InlineData( @"\x61", false )]
	public void InvalidNamesAreRejectedByBothCompilers( string name, bool utf8 ) {
		AssertInvalid( "(?<" + name + ">a)", utf8, 0 );
	}

	[Theory]
	[InlineData( @"(?<x>a)(?<x>b)", 7 )]
	[InlineData( @"\k<missing>", 0 )]
	[InlineData( @"\k'missing'", 0 )]
	[InlineData( @"\k{missing}", 0 )]
	[InlineData( @"\g{missing}", 0 )]
	[InlineData( "(?P=missing)", 0 )]
	[InlineData( @"(?i)(?<X>a)\k<x>", 11 )]
	[InlineData( "(?<x", 0 )]
	[InlineData( "(?'x", 0 )]
	[InlineData( "(?P<x", 0 )]
	[InlineData( @"\k<x", 0 )]
	[InlineData( @"\k'x", 0 )]
	[InlineData( @"\k{x", 0 )]
	[InlineData( @"\g{x", 0 )]
	[InlineData( "(?P=x", 0 )]
	[InlineData( @"[\k<x>]", 1 )]
	[InlineData( @"[\g{x}]", 1 )]
	public void MalformedOrUnknownNamesHaveControlledDiagnostics( string pattern, int offset ) {
		AssertInvalid( pattern, false, offset );
		AssertInvalid( pattern, true, offset );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void NameLengthUsesCodeUnitsRatherThanScalarCount( bool utf8 ) {
		var unit = utf8 ? "é" : "a";
		var name = string.Concat( Enumerable.Repeat( unit, utf8 ? 64 : 128 ) );
		AssertEquivalent( "(?<" + name + @">a)\k<" + name + ">", "6161", utf8, 0, 0, 2 );
		AssertInvalid( "(?<" + name + "a>a)", utf8, 0 );
		AssertInvalid( @"\k<" + name + "a>", utf8, 0 );
	}

	[Fact]
	public void NamesDoNotChangeCaptureLimitOrMatchWork() {
		var atLimit = string.Concat( Enumerable.Range( 1, 256 ).Select( n => $"(?<g{n}>)" ) );
		AssertEquivalent( atLimit + @"\g{g256}", "", false, 0, 0, 0 );
		var tooMany = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( atLimit + "(?<last>)" ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternCaptureLimitExceeded, tooMany.Diagnostic?.Code );
		var numberedSteps = new List<int>();
		var namedSteps = new List<int>();
		Compile( @"(a)\g{1}", new PcreByteOptions { WorkObserver = numberedSteps.Add } ).Match( "aa"u8.ToArray() );
		Compile( @"(?<x>a)\k<x>", new PcreByteOptions { WorkObserver = namedSteps.Add } ).Match( "aa"u8.ToArray() );
		Assert.Equal( numberedSteps, namedSteps );
	}

	[Fact]
	public void QuotingDoesNotDeclareOrReferenceNames() {
		AssertEquivalent( @"\Q(?<x>a)\k<x>\E", "283F3C783E61295C6B3C783E", false, 0, 0, 12 );
		AssertInvalid( @"\Q(?<x>a)\E\k<x>", false, 11 );
	}

	[Theory]
	[InlineData( @"(?<x>a)\g<x>", 7 )]
	[InlineData( @"(?<x>a)\g'x'", 7 )]
	[InlineData( "(?<x>a)(?&x)", 7 )]
	[InlineData( "(?J)(?<x>a)(?<x>b)", 0 )]
	[InlineData( "(?|(?<x>a)|(?<x>b))", 0 )]
	[InlineData( @"(?<x>a)(?=\k<x>)", 7 )]
	public void SubroutinesAndGeneralAssertionsRemainUnsupported( string pattern, int offset ) {
		Reference( pattern, false, false ); // Valid PCRE2 syntax, distinct from implemented aliases.
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void AssertInvalid( string pattern, bool utf8, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, utf8, false ) );
		var result = PcreByteCompiler.Compile( ( utf8 ? Encoding.UTF8 : Encoding.Latin1 ).GetBytes( pattern ),
			new PcreByteOptions { Utf8 = utf8 } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void AssertEquivalent( string pattern, string hex, bool utf8, int offset,
		int expectedIndex, int expectedLength ) {
		var bytes = Convert.FromHexString( hex );
		var reference = Reference( pattern, utf8, false );
		var native = reference.Match( bytes, offset );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( expectedLength, native.Length ); }
		Compare( reference, bytes, offset, Compile( pattern, new PcreByteOptions { Utf8 = utf8 } ).Match( bytes, offset ) );
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult result ) {
		var native = reference.Match( subject, offset );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( native.Success ? native.Index : -1, result.Match?.ByteIndex ?? -1 );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, result.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, result.Match.Captures.Count );
		for ( var n = 0; n < native.Groups.Count; n++ ) {
			var capture = native.Groups[n];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null,
				result.Match.Captures[n] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8, bool invalidUtf ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		return new PcreRegex8Bit( encoding.GetBytes( pattern ), encoding, new PcreRegexSettings { Options = options } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var encoding = options.Utf8 || options.MatchInvalidUtf ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
