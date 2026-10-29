using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Atomic group/assertion aliases against PCRE.NET 1.6.0 / PCRE2 10.48 JIT.</summary>
public sealed class PcreGroupAliasReferenceTests {
	[Theory]
	[InlineData( "atomic", "(?>", 0 )]
	[InlineData( "pla", "(?=", 1 )]
	[InlineData( "positive_lookahead", "(?=", 1 )]
	[InlineData( "nla", "(?!", -1 )]
	[InlineData( "negative_lookahead", "(?!", -1 )]
	[InlineData( "plb", "(?<=", 2 )]
	[InlineData( "positive_lookbehind", "(?<=", 2 )]
	[InlineData( "nlb", "(?<!", -2 )]
	[InlineData( "negative_lookbehind", "(?<!", -2 )]
	public void EverySpellingUsesCanonicalCapturesAndEmptyBodySemantics( string name, string canonical, int kind ) {
		var prefix = "(*" + name + ":";
		var body = kind < 0 ? "(b)" : "(a)";
		var tail = kind == 0 ? @"\1" : Math.Abs( kind ) == 2 ? "b" : "a";
		var subject = Math.Abs( kind ) == 2 ? "ab" : kind == 0 ? "aa" : "a";
		foreach ( var header in new[] { prefix, canonical } ) {
			foreach ( var utf8 in new[] { false, true } ) {
				Check( header + body + ")" + tail, subject, Math.Abs( kind ) == 2 ? 1 : 0,
					kind == 0 ? 2 : 1, 2, utf8 );
				Check( header + ")", "", kind < 0 ? -1 : 0, 0, kind < 0 ? 0 : 1, utf8 );
			}
		}
		Check( "(?x)" + prefix + " a )" + ( kind == 0 ? "" : "a" ), "a",
			kind is -1 or 2 ? -1 : 0, kind is -1 or 2 ? 0 : 1, kind is -1 or 2 ? 0 : 1 );
	}

	[Theory]
	[InlineData( "(*atomic:a|ab)c", "abc", -1, 0, 0 )]
	[InlineData( "(*atomic:ab|a)c", "abc", 0, 3, 1 )]
	[InlineData( @"^(?:(*atomic:(a))c|a(b))\2$", "abb", 0, 3, 3 )]
	[InlineData( @"(*pla:(a|ab))\1c", "abc", -1, 0, 0 )]
	[InlineData( @"(*pla:(ab|a))\1c", "abc", 0, 3, 2 )]
	[InlineData( @"(*nla:(a)c)(a)\2", "aa", 0, 2, 3 )]
	[InlineData( @"(*plb:(?<x>a{1,2}))b", "aab", 2, 1, 2 )]
	[InlineData( @"(*positive_lookbehind:a|bb)c", "bbc", 2, 1, 1 )]
	[InlineData( @"(*negative_lookbehind:a{1,2})b", "aab", -1, 0, 0 )]
	[InlineData( @"(?<x>a)(*plb:\k<x>)b", "ab", 0, 2, 2 )]
	[InlineData( @"(?J)(*pla:(?<x>a))(?<x>a)\k<x>", "aa", 0, 2, 3 )]
	[InlineData( @"(*atomic:(?J)(?<x>a)(?<x>b))\k<x>", "aba", 0, 3, 3 )]
	[InlineData( "(?n)(*atomic:(a)(?<x>b))", "ab", 0, 2, 2 )]
	[InlineData( "(*atomic:(?i)a)b", "Ab", 0, 2, 1 )]
	[InlineData( "(*atomic:(?i)a)b", "AB", -1, 0, 0 )]
	[InlineData( "(?i)(*pla:(?-i:k))k", "K", -1, 0, 0 )]
	[InlineData( "(?ir)(*positive_lookahead:(?-r:(k)))k", "k", 0, 1, 2 )]
	[InlineData( "(?i)(*positive_lookbehind:(?r:k))a", "Ka", -1, 0, 0 )]
	[InlineData( @"(*atomic:(*pla:(?<x>é))é)\k<x>", "éé", 0, 4, 2 )]
	[InlineData( @"(*plb:(😀))é", "😀é", 4, 2, 2 )]
	[InlineData( @"(*pla:(a))+\1", "a", 0, 1, 2 )]
	[InlineData( @"(*positive_lookahead:(a)){2,}\1", "a", 0, 1, 2 )]
	[InlineData( "(?U)(*atomic:a+)a", "aa", 0, 2, 1 )]
	[InlineData( "(*atomic:a+)a", "aa", -1, 0, 0 )]
	[InlineData( "(*pla:a)++a", "a", 0, 1, 1 )]
	[InlineData( @"\Q(*pla:a)\E", "(*pla:a)", 0, 8, 1 )]
	[InlineData( "(?x)(?# (*pla:a )a", "a", 0, 1, 1 )]
	[InlineData( @"(*atomic:a\K)b", "ab", 1, 1, 1 )]
	public void AliasesPreserveCommitmentOptionsReferencesAndQuantifiers( string pattern,
		string text, int index, int length, int groups ) => Check( pattern, text, index, length, groups );

	[Theory]
	[InlineData( "(*PLA:a)" )]
	[InlineData( "(*Atomic:a)" )]
	[InlineData( "(*positive_Lookahead:a)" )]
	[InlineData( "(*plaa:a)" )]
	[InlineData( "(*npla:a)" )]
	[InlineData( "(*nplb:a)" )]
	[InlineData( "(*pla)" )]
	[InlineData( "(*atomic)" )]
	[InlineData( "(*pla :a)" )]
	[InlineData( "(*pla\t:a)" )]
	[InlineData( "(?x)(*pla :a)" )]
	[InlineData( "(?i)(*PLA:a)" )]
	public void HeaderRecognitionRequiresExactLowercaseAndAdjacentColon( string pattern ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, pattern.IndexOf( "(*", StringComparison.Ordinal ) );
	}

	[Theory]
	[InlineData( "é(*atomic:a", 2 )]
	[InlineData( "é(*pla:a", 2 )]
	[InlineData( "é(*negative_lookahead:a", 2 )]
	[InlineData( "é(*plb:a", 2 )]
	[InlineData( "é(*negative_lookbehind:a", 2 )]
	[InlineData( "((*atomic:a", 0 )]
	[InlineData( @"é(*pla:(*atomic:\K))", 2 )]
	[InlineData( @"é(*plb:\K)", 2 )]
	[InlineData( @"é(*atomic:(*pla:\K))", 11 )]
	public void RecognizedBodiesRetainOriginalUtfByteOpenerAndAssertionContext( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, offset );
	}

	[Theory]
	[InlineData( @"(*plb:a*)b", 0 )]
	[InlineData( @"(?J)(?<x>a)(?<x>b)(*plb:\k<x>)c", 18 )]
	[InlineData( @"(?|(a)|(b))(*plb:\1)c", 11 )]
	public void BehindAliasesRetainConsumedWidthRejections( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, offset );
	}

	[Theory]
	[InlineData( "(*sr:a)" )]
	[InlineData( "(*asr:a)" )]
	[InlineData( "(*ACCEPT)" )]
	public void OtherValidNativeGroupsAndVerbsAreNeverReinterpretedAsAtomicAliases( string pattern ) {
		Reference( pattern );
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, 0 );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void SelectedMalformedUtfAndCrLfOffsetsKeepCanonicalCaptureTransactions( bool anyCrLf ) {
		foreach ( var pattern in new[] { @"(*pla:(a?))\1", @"(*plb:(a{1,2}))b", @"(*atomic:(?<x>.))\k<x>", @"(*nlb:[a])(?m:.)" } ) {
			var reference = Reference( pattern, invalidUtf: true, anyCrLf: anyCrLf );
			var compiled = Compile( pattern, Options( invalidUtf: true, anyCrLf: anyCrLf ) );
			foreach ( var hex in new[] { "", "61", "616162", "61FF62", "FFC3A9C3A9", "FFFF", "0D0A61610D0A" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( "atomic" )]
	[InlineData( "pla" )]
	[InlineData( "plb" )]
	[InlineData( "negative_lookahead" )]
	[InlineData( "negative_lookbehind" )]
	public void AliasesUseOneDepthLevelAndNoImplicitCaptureSlot( string name ) {
		var header = "(*" + name + ":";
		var nested = string.Concat( Enumerable.Repeat( header, 256 ) ) + "[a]" + new string( ')', 256 );
		Assert.True( Compile( nested ).Match( "a"u8.ToArray() ).IsSuccess );
		Diagnostic( header + nested + ")", PcreByteDiagnosticCode.PatternDepthLimitExceeded, 256 * header.Length );
		var captures = header + string.Concat( Enumerable.Repeat( "()", 256 ) ) + ")";
		Compile( captures );
		Diagnostic( captures[..^1] + "())", PcreByteDiagnosticCode.PatternCaptureLimitExceeded, header.Length + 512 );
	}

	[Theory]
	[InlineData( 1023, true )]
	[InlineData( 1024, false )]
	public void AliasAssertionGuardsShareTheSavedPathCeiling( int choices, bool succeeds ) {
		var result = Compile( "(*pla:" + string.Concat( Enumerable.Repeat( "a??", choices ) ) + ")",
			Options() with { MatchWorkLimit = 100_000 } ).Match( Array.Empty<byte>() );
		Assert.Equal( succeeds, result.IsSuccess );
		Assert.Equal( succeeds ? null : PcreByteDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code );
	}

	[Fact]
	public void AliasBodiesShareWorkLimitsAndCancellation() {
		const string pattern = "^(*atomic:(?:a|aa)*[b])$";
		var subject = Encoding.ASCII.GetBytes( new string( 'a', 40 ) );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, Options() with { MatchWorkLimit = 40 } ).Match( subject ).Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( pattern, Options() with { WorkObserver = work => { if ( work == 40 ) { cancelled.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancelled.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( "(*pla:a)"u8.ToArray(), Options(), cancelled.Token ) );
	}

	[Fact]
	public void AliasAndCanonicalFormsSpendIdenticalObservedMatchWork() {
		foreach ( var (alias, canonical) in new[] {
			("(*atomic:(a|aa))b", "(?>(a|aa))b"),
			("(*pla:(a{1,2}))a", "(?=(a{1,2}))a"),
			("(*plb:(a{1,2}))b", "(?<=(a{1,2}))b")
		} ) {
			var aliasSteps = new List<int>();
			var canonicalSteps = new List<int>();
			var subject = "aab"u8.ToArray();
			var actual = Compile( alias, Options() with { WorkObserver = aliasSteps.Add } ).Match( subject );
			var expected = Compile( canonical, Options() with { WorkObserver = canonicalSteps.Add } ).Match( subject );
			Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
			Assert.True( expected.IsSuccess, expected.Diagnostic?.Message );
			Assert.Equal( expected.Match?.ByteIndex, actual.Match?.ByteIndex );
			Assert.Equal( canonicalSteps, aliasSteps );
		}
	}

	private static void Diagnostic( string pattern, PcreByteDiagnosticCode code, int offset ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), Options() );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static void Check( string pattern, string text, int index, int length, int groups, bool utf8 = true ) {
		var subject = ( utf8 ? Encoding.UTF8 : Encoding.Latin1 ).GetBytes( text );
		var reference = Reference( pattern, utf8: utf8 );
		var native = reference.Match( subject );
		Assert.Equal( index, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( length, native.Length ); Assert.Equal( groups, native.Groups.Count ); }
		Compare( reference, subject, 0, Compile( pattern, Options( utf8: utf8 ) ).Match( subject ), pattern );
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( native.Success ? native.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, actual.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var capture = native.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreByteOptions Options( bool invalidUtf = false, bool anyCrLf = false, bool utf8 = true ) => new() {
		Utf8 = utf8, MatchInvalidUtf = invalidUtf, AnyCrLfNewline = anyCrLf
	};

	private static PcreRegex8Bit Reference( string pattern, bool invalidUtf = false, bool anyCrLf = false, bool utf8 = true ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = PcreOptions.Compiled | ( utf8 ? PcreOptions.Utf : PcreOptions.None )
				| ( invalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		options ??= Options();
		var encoding = options.Utf8 || options.MatchInvalidUtf ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
