using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Capture allocation and repetition preferences against pinned PCRE2.</summary>
public sealed class PcreCaptureRepeatOptionReferenceTests {

	[Theory]
	[InlineData( @"(?n)(a)(b)", "ab", 1 )]
	[InlineData( @"a(?n)(b)(c)", "abc", 1 )]
	[InlineData( @"(?n)(a)(?<x>b)(c)\1", "abcb", 2 )]
	[InlineData( @"(?n)(?<a>a)(?'b'b)(?P<c>c)\1\2\3", "abcabc", 4 )]
	[InlineData( @"(?n:(a)(?<x>b))(c)\1\2", "abcbc", 3 )]
	[InlineData( @"((?n)a(b))(c)\1\2", "abcabc", 3 )]
	[InlineData( @"(?n:(a)(?-n:(b))(c))(d)", "abcd", 3 )]
	[InlineData( @"(?n)(a)(?-n)(b)(c)", "abc", 3 )]
	[InlineData( @"(?n-n)(a)", "a", 2 )]
	[InlineData( @"(?nn)(a)(?-nn)(b)", "ab", 2 )]
	[InlineData( @"(?n:(a)|(?-n)(b)|(c))(d)", "cd", 4 )]
	[InlineData( @"(a(?n)(b)|c(d))(e)", "cde", 3 )]
	[InlineData( @"(?n)(?|(a)(?<x>b)|(c)(?<x>d))(e)\k<x>", "cded", 2 )]
	[InlineData( @"(?n)(?|(a)(?-n)(b)|(c)(d))(e)\g{-1}", "cded", 3 )]
	[InlineData( @"(?n)(a)(?<x>b)(c)\g{-1}", "abcb", 2 )]
	[InlineData( @"(?n)(a)(?<x>b)(c)\k<x>", "abcb", 2 )]
	[InlineData( @"(?n)(?:(a)(?<x>b)|c)\k<x>", "cabb", 2 )]
	[InlineData( @"(?n)(?>(a)(?<x>b))\g{x}", "abb", 2 )]
	[InlineData( @"(?in)(a)(?<x>b)\k<x>", "aBb", 2 )]
	[InlineData( @"(?n:(é))(?<x>é)\k<x>", "ééé", 2 )]
	public void NoAutoCaptureKeepsNamedSlotsAndScopeNumbering( string pattern,
		string text, int expectedGroups ) {
		var subject = Encoding.UTF8.GetBytes( text );
		var reference = Reference( pattern, true );
		var native = reference.Match( subject );
		Assert.True( native.Success );
		Assert.Equal( expectedGroups, native.Groups.Count );
		Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions { Utf8 = true } ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( "(?U)a+", "aaa", 0, 1 )]
	[InlineData( "a(?U)a+", "aaa", 0, 2 )]
	[InlineData( "(?U)a+?", "aaa", 0, 3 )]
	[InlineData( "(?U)a++", "aaa", 0, 3 )]
	[InlineData( "(?U)a+a", "aaa", 0, 2 )]
	[InlineData( "(?U)a+?a", "aaa", 0, 3 )]
	[InlineData( "(?U)a++a", "aaa", -1, 0 )]
	[InlineData( "(?U)a*+a", "aaa", -1, 0 )]
	[InlineData( "(?U)a?+a", "aa", 0, 2 )]
	[InlineData( "(?U)a{1,3}+a", "aaa", -1, 0 )]
	[InlineData( "(?U-U)a+", "aaa", 0, 3 )]
	[InlineData( "(?UU)a+?", "aaa", 0, 3 )]
	[InlineData( "(?U:a+)(a+)", "aaa", 0, 3 )]
	[InlineData( "(?U:(a+))+", "aaa", 0, 3 )]
	[InlineData( "(?U)((?-U:a+))+", "aaa", 0, 3 )]
	[InlineData( "(?U:a+)(?-U:a+)", "aaa", 0, 3 )]
	[InlineData( "(?U:(?>a+))a", "aaa", 0, 2 )]
	[InlineData( "(?U:(?>a+?))a", "aaa", -1, 0 )]
	[InlineData( "(?U:ab|a)", "ab", 0, 2 )]
	[InlineData( "(?U)(a?)*", "a", 0, 0 )]
	[InlineData( "(?U)(a?)*?", "a", 0, 0 )]
	[InlineData( "(?U)(a??)*?", "a", 0, 1 )]
	[InlineData( "(?nU)(a+)(?<x>a+)", "aaaa", 0, 2 )]
	[InlineData( "(?inU)(a+)(?<x>a+)", "aAAA", 0, 2 )]
	[InlineData( "(?msU:^(.+)$)", "a\nb", 0, 1 )]
	[InlineData( "(?U)(é+)", "ééé", 0, 2 )]
	[InlineData( "(?U)(é+?)", "ééé", 0, 6 )]
	public void UngreedyKeepsExplicitPreferenceAndAtomicSemantics( string pattern,
		string text, int expectedIndex, int expectedLength ) {
		var subject = Encoding.UTF8.GetBytes( text );
		var reference = Reference( pattern, true );
		var native = reference.Match( subject );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( expectedLength, native.Length ); }
		Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions { Utf8 = true } ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( "*" )]
	[InlineData( "+" )]
	[InlineData( "?" )]
	[InlineData( "{1,3}" )]
	[InlineData( "{1,}" )]
	[InlineData( "{,3}" )]
	[InlineData( "{2}" )]
	public void EveryQuantifierUsesTheSettingAtItsOwnLocation( string quantifier ) {
		foreach ( var suffix in new[] { "", "?", "+" } ) {
			foreach ( var pattern in new[] {
				$"(?U)(a{quantifier}{suffix})(a*)",
				$"(?U:(a{quantifier}{suffix}))(a*)",
				$"(x(?U)b|(a{quantifier}{suffix}))(a*)",
				$"(?U)(a{quantifier}{suffix})(?-U)(a*)",
				$"(?U:(a)){quantifier}{suffix}(a*)",
				$"(?U)((?-U:a)){quantifier}{suffix}(a*)"
			} ) {
				var subject = "aaaa"u8.ToArray();
				Compare( Reference( pattern, false ), subject, 0, Compile( pattern ).Match( subject ), pattern );
			}
		}
	}

	[Fact]
	public void SuppressedGroupsDoNotChangeOctalFallbackOrConsumeCaptureBudget() {
		var suppressed = "(?n)" + string.Concat( Enumerable.Repeat( "()", 257 ) );
		var subject = new byte[] { 8 };
		var pattern = suppressed + @"\10";
		Compare( Reference( pattern, false ), subject, 0, Compile( pattern ).Match( subject ) );
		Assert.Single( Compile( pattern ).Match( subject ).Match!.Captures );
		var named = "(?n)" + string.Concat( Enumerable.Range( 1, 10 ).Select( i => $"(?<x{i}>a)" ) ) + @"\10";
		var namedSubject = "aaaaaaaaaaa"u8.ToArray();
		Compare( Reference( named, false ), namedSubject, 0, Compile( named ).Match( namedSubject ) );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void OptionsRetainSelectedInvalidUtfAndCrLfOffsets( bool anyCrLf ) {
		foreach ( var pattern in new[] {
			@"(?nU)(a*)(?<x>a*)\k<x>", @"(?n)(a)(?-n:(b?))\g{1}",
			@"(?U)(a*?)(a*)", @"(?U:.+)(?-U:.)", @"(?n)(?<x>é+)(?U:\g{x}*)"
		} ) {
			var reference = Reference( pattern, true, invalidUtf: true, anyCrLf: anyCrLf );
			var managed = Compile( pattern, new PcreByteOptions { MatchInvalidUtf = true, AnyCrLfNewline = anyCrLf } );
			foreach ( var hex in new[] { "", "616161", "61FF61", "C3A9C3A9", "FFC3A9FF", "0D0A610D0A", "C3FF62" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, managed.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Fact]
	public void QuotedOptionsAndBracketCharactersStayLiteral() {
		foreach ( var pattern in new[] { @"\Q(?nU)\E", @"(?U)\Q(?n)\E+", @"(?nU)[(?nU)]+?" } ) {
			var subject = "(?nU)(?n)"u8.ToArray();
			Compare( Reference( pattern, false ), subject, 0, Compile( pattern ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( @"(?n)(a)\1", 7 )]
	[InlineData( @"(?n)(a)\g{-1}", 7 )]
	[InlineData( "(?n)*", 4 )]
	[InlineData( "a(?U){2}", 5 )]
	[InlineData( "(?U)a*?+", 7 )]
	[InlineData( "(?U)a++?", 7 )]
	public void UnavailableSlotsAndRepeatedSuffixesAreInvalid( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, false ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(?nU:a(?x)b)", 6 )]
	[InlineData( "(?nU:(?=a|b))", 5 )]
	[InlineData( "(?nJ)(a)", 0 )]
	public void OtherOptionAndAssertionFamiliesRemainPending( string pattern, int offset ) {
		Reference( pattern, false );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void CaptureOptionsKeepDepthAndNamedCaptureLimits() {
		var nested = "(?n)" + new string( '(', 256 ) + "a" + new string( ')', 256 );
		Assert.Single( Compile( nested ).Match( "a"u8.ToArray() ).Match!.Captures );
		var tooDeep = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( "(?n)(" + nested[4..] + ")" ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, tooDeep.Diagnostic?.Code );
		Assert.Equal( 260, tooDeep.Diagnostic?.ByteOffset );
		var named = "(?n)" + string.Concat( Enumerable.Range( 1, 256 ).Select( i => $"(?<x{i}>)" ) );
		Assert.Equal( 257, Compile( named ).Match( Array.Empty<byte>() ).Match!.Captures.Count );
		var tooMany = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( named + "(?<last>)" ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternCaptureLimitExceeded, tooMany.Diagnostic?.Code );
		Assert.Equal( named.Length, tooMany.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void LazyBacktrackingKeepsWorkLimitsAndCancellation() {
		const string pattern = "(?nU)^(a|aa)*[b]$";
		var subject = Enumerable.Repeat( (byte)'a', 40 ).ToArray();
		var limited = Compile( pattern, new PcreByteOptions { MatchWorkLimit = 500 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new PcreByteOptions {
			WorkObserver = count => { if ( count == 20 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
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
