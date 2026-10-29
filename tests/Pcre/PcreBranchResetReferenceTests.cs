using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Shared capture numbers and aliases compared with pinned PCRE2 10.48.</summary>
public sealed class PcreBranchResetReferenceTests {
	[Theory]
	[InlineData( @"(?|(a)|(b))\1", "6161", false, 0, 2, 2 )]
	[InlineData( @"(?|(a)|(b))\1", "6262", false, 0, 2, 2 )]
	[InlineData( @"(?|(a)(b)|(c))(d)\g{3}", "636464", false, 0, 3, 4 )]
	[InlineData( @"(?|(a)(b)|(c))(d)\g{3}", "61626464", false, 0, 4, 4 )]
	[InlineData( @"(?|(a)|(b)(c))(d)", "6164", false, 0, 2, 4 )]
	[InlineData( @"(p)(?|(a)|(b)(c))(d)\g{4}", "7062636464", false, 0, 5, 5 )]
	[InlineData( @"(?|(a)(?|(b)|(c)(d))|(e))(f)\g{4}", "656666", false, 0, 3, 5 )]
	[InlineData( @"(?|(a)(?|(b)|(c)(d))|(e))(f)\g{4}", "6163646666", false, 0, 5, 5 )]
	[InlineData( @"(?:(?|(a)|(b))|(c))(d)", "6364", false, 0, 2, 4 )]
	[InlineData( @"(?|(?<x>a)|(b))\k<x>", "6262", false, 0, 2, 2 )]
	[InlineData( @"(?|(a)|(?<x>b))\k<x>", "6161", false, 0, 2, 2 )]
	[InlineData( @"(?|(?<x>a)|(?P<x>b)|(?'x'c))(?P=x)", "6363", false, 0, 2, 2 )]
	[InlineData( @"(?|(?|(?<x>a)|(b))|(?<x>c))\g{x}", "6262", false, 0, 2, 2 )]
	[InlineData( @"(?|(a)\g{-1}|(b)\g{-1})", "6262", false, 0, 2, 2 )]
	[InlineData( @"^(?:\g{1}|(?|(a)|(b)))+$", "61626261", false, 0, 4, 2 )]
	[InlineData( @"^(?|(a)(b)|(c))+\g{2}$", "61626362", false, 0, 4, 3 )]
	[InlineData( @"(?|(a)b|a(c))\g{1}", "616363", false, 0, 3, 2 )]
	[InlineData( @"(?|(?>(a))b|a(c))\g{1}", "616363", false, 0, 3, 2 )]
	[InlineData( @"(?|(a)|(b))+\1", "626262", false, 0, 3, 2 )]
	[InlineData( @"(?|(a)|(b))+?\1", "626262", false, 0, 2, 2 )]
	[InlineData( @"(?|(a)|(b))++\1", "626262", false, -1, 0, 2 )]
	[InlineData( @"(?|(a)|(b)){1,2}\1", "626262", false, 0, 3, 2 )]
	[InlineData( @"(?|(a)|(b))\K\1", "6262", false, 1, 1, 2 )]
	[InlineData( @"(?|\Q(a)|b\E|(c))\1", "6363", false, 0, 2, 2 )]
	[InlineData( @"(?|([a|])|(b))\1", "7C7C", false, 0, 2, 2 )]
	[InlineData( @"(?i)(?|(é)|(ø))\g{1}", "C398C3B8", true, 0, 4, 2 )]
	[InlineData( @"(?|(?<字>é)|(😀))\k<字>", "F09F9880F09F9880", true, 0, 8, 2 )]
	[InlineData( @"(?|(\xFF)|(\x00))\1", "FFFF", false, 0, 2, 2 )]
	[InlineData( @"(?|(\xFF)|(\x00))\1", "0000", false, 0, 2, 2 )]
	[InlineData( @"(?|(a)?|(b))\1", "", false, -1, 0, 2 )]
	[InlineData( @"(?|(a)(b)|(c))\g{2}", "6362", false, -1, 0, 3 )]
	[InlineData( @"(?|(a)(b)|(c)\g{+1})", "6363", false, -1, 0, 3 )]
	[InlineData( @"\k<x>(?|(?<x>a)|(b))", "6161", false, -1, 0, 2 )]
	[InlineData( "(?|)", "", false, 0, 0, 1 )]
	[InlineData( "(?||(a))", "61", false, 0, 0, 2 )]
	[InlineData( "(?|(a)|)", "", false, 0, 0, 2 )]
	public void SharedNumbersKeepReferenceWholeAndCaptureSpans( string pattern, string hex, bool utf8,
		int expectedIndex, int expectedLength, int expectedGroups ) {
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern, utf8 );
		var native = reference.Match( subject );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) {
			Assert.Equal( expectedLength, native.Length );
			Assert.Equal( expectedGroups, native.Groups.Count );
		}
		Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions { Utf8 = utf8 } ).Match( subject ) );
	}

	[Fact]
	public void EmptySharedReferencesKeepMalformedUtfOffsetAndFragmentRules() {
		foreach ( var pattern in new[] { @"(?|(?<x>a?)|(?<x>b?))\k<x>",
			@"(?|(?<x>.)|(?<x>a))\g{x}", @"(?|(a?)|())\g{1}", @"(?|(a)|())" } ) {
			var reference = Reference( pattern, true, true );
			var managed = Compile( pattern, new PcreByteOptions { MatchInvalidUtf = true } );
			foreach ( var hex in new[] { "", "6161", "61FF61", "C3A9C3A9", "FFC3A9C3A9", "C3FFC3", "F09F9880F09F9880" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, managed.Match( subject, offset ), $"{pattern}, {hex}, offset {offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( "(?|(?<x>a)|(?<y>b))", 11 )]
	[InlineData( "(?|(?<x>a)|(?<x>b)(?<x>c))", 18 )]
	[InlineData( "(?|(?<x>a)|(b))(?<x>c)", 15 )]
	[InlineData( "(?|(?<x>a)|(b)(?<x>c))", 14 )]
	[InlineData( "(?|(?<x>a)(?<y>b)|(?<y>c))", 18 )]
	[InlineData( @"(?|(a)|(b))\g{2}", 11 )]
	[InlineData( @"(?|(a)(b)|\g{-1})", 10 )]
	[InlineData( @"(?|(a)(b)|(c)\g{-2})", 13 )]
	[InlineData( "(?|", 0 )]
	[InlineData( "(?|(a)|", 0 )]
	public void ConflictingAliasesAndMalformedResetsHaveByteDiagnostics( string pattern, int expectedOffset ) {
		foreach ( var utf8 in new[] { false, true } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern, utf8 ) );
			var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new PcreByteOptions { Utf8 = utf8 } );
			Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
			Assert.Equal( expectedOffset, result.Diagnostic?.ByteOffset );
		}
	}

	[Theory]
	[InlineData( 128 )]
	[InlineData( 256 )]
	public void CaptureCeilingUsesHighestSlotRatherThanDeclarationCount( int slots ) {
		var branch = string.Concat( Enumerable.Range( 1, slots ).Select( n => $"(?<g{n}>)" ) );
		var pattern = "(?|" + branch + "|" + branch + ")";
		var reference = Reference( pattern, false );
		Assert.Equal( slots + 1, reference.Match( Array.Empty<byte>() ).Groups.Count );
		Compare( reference, Array.Empty<byte>(), 0, Compile( pattern ).Match( Array.Empty<byte>() ) );
		if ( slots == 256 ) {
			var overflow = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern + "()" ) );
			Assert.Equal( PcreByteDiagnosticCode.PatternCaptureLimitExceeded, overflow.Diagnostic?.Code );
			Assert.Equal( pattern.Length, overflow.Diagnostic?.ByteOffset );
		}
	}

	[Fact]
	public void ResetGroupsRetainDepthAndExpansionLimits() {
		var atLimit = string.Concat( Enumerable.Repeat( "(?|", 256 ) ) + "a" + new string( ')', 256 );
		Assert.Equal( 1, Compile( atLimit ).Match( "a"u8.ToArray() ).Match?.ByteLength );
		var overflow = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( "(?|" + atLimit + ")" ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, overflow.Diagnostic?.Code );
		Assert.Equal( 768, overflow.Diagnostic?.ByteOffset );
		var expanded = PcreByteCompiler.Compile( @"(?|(a)|(b)){4096}"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, expanded.Diagnostic?.Code );
	}

	[Theory]
	[InlineData( "(", ")" )]
	[InlineData( "(?>", ")" )]
	[InlineData( "(?:", ")?" )]
	[InlineData( "(?:", "){1}" )]
	public void OtherGroupShapesCompileAtTheSameDepthBoundary( string opening, string closing ) {
		var pattern = string.Concat( Enumerable.Repeat( opening, 256 ) ) + "a"
			+ string.Concat( Enumerable.Repeat( closing, 256 ) );
		Assert.Equal( 1, Compile( pattern ).Match( "a"u8.ToArray() ).Match?.ByteLength );
	}

	[Fact]
	public void DeepNamedResolutionAndCaptureCompilationKeepTheirBoundary() {
		var pattern = string.Concat( Enumerable.Range( 1, 256 ).Select( n => $"(?<g{n}>" ) )
			+ "a" + new string( ')', 256 ) + @"\g{g256}";
		var match = Compile( pattern ).Match( "aa"u8.ToArray() ).Match;
		Assert.Equal( 2, match?.ByteLength );
		Assert.Equal( 257, match?.Captures.Count );
		Assert.Equal( new PcreByteCapture( 0, 1 ), match?.Captures[256] );
	}

	[Fact]
	public void ResetAlternativesSpendMatchWorkAndHonorCancellation() {
		const string pattern = "(?|(a)|(aa))*b";
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 100 ) );
		var limited = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 40 } ).Match( subject );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new PcreByteOptions {
			WorkObserver = count => { if ( count == 40 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( "(?J)(?|(?<x>a)|(?<x>b))", 0 )]
	[InlineData( @"(?|(a)|(b))\g<1>", 11 )]
	[InlineData( "(?|(a)|(b))(?1)", 11 )]
	[InlineData( @"(?<=(?|(a)|(bb)))", 0 )]
	public void DuplicateNameOptionsSubroutinesAndGeneralAssertionsRemainPending( string pattern, int offset ) {
		Reference( pattern, false );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
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

	private static PcreRegex8Bit Reference( string pattern, bool utf8, bool invalidUtf = false ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		return new PcreRegex8Bit( encoding.GetBytes( pattern ), encoding, new PcreRegexSettings { Options = options } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var encoding = options?.Utf8 == true || options?.MatchInvalidUtf == true ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
