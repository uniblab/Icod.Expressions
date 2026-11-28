using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordAssertionRepeatTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new (string Source, string Subject, int Start, bool Utf, bool Ucp)[] {
				("(?=a)?a", "za", 1, true, false),
				("(?=a)??a", "za", 1, true, false),
				("(?=a)?+a", "za", 1, true, false),
				("(?=a)*a", "za", 1, true, false),
				("(?=a)*?a", "za", 1, true, false),
				("(?=a)*+a", "za", 1, true, false),
				("(?=a)+a", "za", 1, true, false),
				("(?=a)+?a", "za", 1, true, false),
				("(?=a)++a", "za", 1, true, false),
				("(?=a){0}a", "za", 1, true, false),
				("(?=a){0}+a", "za", 1, true, false),
				("(?=a){1}+a", "za", 1, true, false),
				("(?=a){2}a", "za", 1, true, false),
				("(?=a){2}+a", "za", 1, true, false),
				("(?=a){1,3}a", "za", 1, true, false),
				("(?=a){1,3}?a", "za", 1, true, false),
				("(?=a){1,3}+a", "za", 1, true, false),
				("(?=a){2,}a", "za", 1, true, false),
				("(?=a){2,}?a", "za", 1, true, false),
				("(?=a){2,}+a", "za", 1, true, false),
				("(?=a){,2}a", "za", 1, true, false),
				("(?=a){ 1 , 2 }+a", "za", 1, true, false),
				("(?!a)?a", "za", 1, true, false),
				("(?!a)??a", "za", 1, true, false),
				("(?!a)?+a", "za", 1, true, false),
				("(?!a)*a", "za", 1, true, false),
				("(?!a)*?a", "za", 1, true, false),
				("(?!a)*+a", "za", 1, true, false),
				("(?!a)+a", "za", 1, true, false),
				("(?!a)+?a", "za", 1, true, false),
				("(?!a)++a", "za", 1, true, false),
				("(?!a){0}a", "za", 1, true, false),
				("(?!a){0}+a", "za", 1, true, false),
				("(?!a){1}+a", "za", 1, true, false),
				("(?!a){2}a", "za", 1, true, false),
				("(?!a){2}+a", "za", 1, true, false),
				("(?!a){1,3}a", "za", 1, true, false),
				("(?!a){1,3}?a", "za", 1, true, false),
				("(?!a){1,3}+a", "za", 1, true, false),
				("(?!a){2,}a", "za", 1, true, false),
				("(?!a){2,}?a", "za", 1, true, false),
				("(?!a){2,}+a", "za", 1, true, false),
				("(?!a){,2}a", "za", 1, true, false),
				("(?!a){ 1 , 2 }+a", "za", 1, true, false),
				("(?*a)?a", "za", 1, true, false),
				("(?*a)??a", "za", 1, true, false),
				("(?*a)?+a", "za", 1, true, false),
				("(?*a)*a", "za", 1, true, false),
				("(?*a)*?a", "za", 1, true, false),
				("(?*a)*+a", "za", 1, true, false),
				("(?*a)+a", "za", 1, true, false),
				("(?*a)+?a", "za", 1, true, false),
				("(?*a)++a", "za", 1, true, false),
				("(?*a){0}a", "za", 1, true, false),
				("(?*a){0}+a", "za", 1, true, false),
				("(?*a){1}+a", "za", 1, true, false),
				("(?*a){2}a", "za", 1, true, false),
				("(?*a){2}+a", "za", 1, true, false),
				("(?*a){1,3}a", "za", 1, true, false),
				("(?*a){1,3}?a", "za", 1, true, false),
				("(?*a){1,3}+a", "za", 1, true, false),
				("(?*a){2,}a", "za", 1, true, false),
				("(?*a){2,}?a", "za", 1, true, false),
				("(?*a){2,}+a", "za", 1, true, false),
				("(?*a){,2}a", "za", 1, true, false),
				("(?*a){ 1 , 2 }+a", "za", 1, true, false),
				("(?<=a)?b", "ab", 1, true, false),
				("(?<=a)??b", "ab", 1, true, false),
				("(?<=a)?+b", "ab", 1, true, false),
				("(?<=a)*b", "ab", 1, true, false),
				("(?<=a)*?b", "ab", 1, true, false),
				("(?<=a)*+b", "ab", 1, true, false),
				("(?<=a)+b", "ab", 1, true, false),
				("(?<=a)+?b", "ab", 1, true, false),
				("(?<=a)++b", "ab", 1, true, false),
				("(?<=a){0}b", "ab", 1, true, false),
				("(?<=a){0}+b", "ab", 1, true, false),
				("(?<=a){1}+b", "ab", 1, true, false),
				("(?<=a){2}b", "ab", 1, true, false),
				("(?<=a){2}+b", "ab", 1, true, false),
				("(?<=a){1,3}b", "ab", 1, true, false),
				("(?<=a){1,3}?b", "ab", 1, true, false),
				("(?<=a){1,3}+b", "ab", 1, true, false),
				("(?<=a){2,}b", "ab", 1, true, false),
				("(?<=a){2,}?b", "ab", 1, true, false),
				("(?<=a){2,}+b", "ab", 1, true, false),
				("(?<=a){,2}b", "ab", 1, true, false),
				("(?<=a){ 1 , 2 }+b", "ab", 1, true, false),
				("(?<!a)?b", "ab", 1, true, false),
				("(?<!a)??b", "ab", 1, true, false),
				("(?<!a)?+b", "ab", 1, true, false),
				("(?<!a)*b", "ab", 1, true, false),
				("(?<!a)*?b", "ab", 1, true, false),
				("(?<!a)*+b", "ab", 1, true, false),
				("(?<!a)+b", "ab", 1, true, false),
				("(?<!a)+?b", "ab", 1, true, false),
				("(?<!a)++b", "ab", 1, true, false),
				("(?<!a){0}b", "ab", 1, true, false),
				("(?<!a){0}+b", "ab", 1, true, false),
				("(?<!a){1}+b", "ab", 1, true, false),
				("(?<!a){2}b", "ab", 1, true, false),
				("(?<!a){2}+b", "ab", 1, true, false),
				("(?<!a){1,3}b", "ab", 1, true, false),
				("(?<!a){1,3}?b", "ab", 1, true, false),
				("(?<!a){1,3}+b", "ab", 1, true, false),
				("(?<!a){2,}b", "ab", 1, true, false),
				("(?<!a){2,}?b", "ab", 1, true, false),
				("(?<!a){2,}+b", "ab", 1, true, false),
				("(?<!a){,2}b", "ab", 1, true, false),
				("(?<!a){ 1 , 2 }+b", "ab", 1, true, false),
				("(?<*a)?b", "ab", 1, true, false),
				("(?<*a)??b", "ab", 1, true, false),
				("(?<*a)?+b", "ab", 1, true, false),
				("(?<*a)*b", "ab", 1, true, false),
				("(?<*a)*?b", "ab", 1, true, false),
				("(?<*a)*+b", "ab", 1, true, false),
				("(?<*a)+b", "ab", 1, true, false),
				("(?<*a)+?b", "ab", 1, true, false),
				("(?<*a)++b", "ab", 1, true, false),
				("(?<*a){0}b", "ab", 1, true, false),
				("(?<*a){0}+b", "ab", 1, true, false),
				("(?<*a){1}+b", "ab", 1, true, false),
				("(?<*a){2}b", "ab", 1, true, false),
				("(?<*a){2}+b", "ab", 1, true, false),
				("(?<*a){1,3}b", "ab", 1, true, false),
				("(?<*a){1,3}?b", "ab", 1, true, false),
				("(?<*a){1,3}+b", "ab", 1, true, false),
				("(?<*a){2,}b", "ab", 1, true, false),
				("(?<*a){2,}?b", "ab", 1, true, false),
				("(?<*a){2,}+b", "ab", 1, true, false),
				("(?<*a){,2}b", "ab", 1, true, false),
				("(?<*a){ 1 , 2 }+b", "ab", 1, true, false),
				("^(?=(\\1a|a))+\\1$", "aaa", 0, true, false),
				("^(?=(\\1a|a))+?\\1$", "aaa", 0, true, false),
				("^(?=(\\1a|a))++\\1$", "aaa", 0, true, false),
				("^(?=(\\1a|a)){3,}\\1$", "aaaa", 0, true, false),
				("^(?=(\\1a|a)){3,}?\\1$", "aaaa", 0, true, false),
				("^(?=(\\1a|a)){3,}+\\1$", "aaaa", 0, true, false),
				("^(?=(\\1a|a)){1,4}\\1$", "aaa", 0, true, false),
				("^(?=(\\1a|a)){1,4}?\\1$", "aaa", 0, true, false),
				("^(?=(\\1a|a)){1,4}+\\1$", "aaa", 0, true, false),
				("(?=(a|ab)){1}\\1c", "abc", 0, true, false),
				("(?=(a|ab)){1}+\\1c", "abc", 0, true, false),
				("(?=(a|ab)){2}\\1c", "abc", 0, true, false),
				("(?=(a|ab)){1,2}?\\1c", "abc", 0, true, false),
				("(?=(a|ab)){1,2}+\\1c", "abc", 0, true, false),
				("(?=(a|ab))*\\1c", "abc", 0, true, false),
				("(?=(a|ab))*?\\1c", "abc", 0, true, false),
				("(?=(a|ab))*+\\1c", "abc", 0, true, false),
				("^(?*(\\1a|a))+\\1$", "aaa", 0, true, false),
				("^(?*(\\1a|a))+?\\1$", "aaa", 0, true, false),
				("^(?*(\\1a|a))++\\1$", "aaa", 0, true, false),
				("^(?*(\\1a|a)){3,}\\1$", "aaaa", 0, true, false),
				("^(?*(\\1a|a)){3,}?\\1$", "aaaa", 0, true, false),
				("^(?*(\\1a|a)){3,}+\\1$", "aaaa", 0, true, false),
				("^(?*(\\1a|a)){1,4}\\1$", "aaa", 0, true, false),
				("^(?*(\\1a|a)){1,4}?\\1$", "aaa", 0, true, false),
				("^(?*(\\1a|a)){1,4}+\\1$", "aaa", 0, true, false),
				("(?*(a|ab)){1}\\1c", "abc", 0, true, false),
				("(?*(a|ab)){1}+\\1c", "abc", 0, true, false),
				("(?*(a|ab)){2}\\1c", "abc", 0, true, false),
				("(?*(a|ab)){1,2}?\\1c", "abc", 0, true, false),
				("(?*(a|ab)){1,2}+\\1c", "abc", 0, true, false),
				("(?*(a|ab))*\\1c", "abc", 0, true, false),
				("(?*(a|ab))*?\\1c", "abc", 0, true, false),
				("(?*(a|ab))*+\\1c", "abc", 0, true, false),
				("^(?=(\\1a|a))+a\\1$", "aa", 0, true, false),
				("^(?=(\\1a|a))++a\\1$", "aa", 0, true, false),
				("^(?=(\\1a|a)){3,}\\1$", "aaaaa", 0, true, false),
				("^(?:(?=(\\1a|a)))+\\1$", "aaaa", 0, true, false),
				("(?U)(?=(a))*", "a", 0, true, false),
				("(?U)(?=(a))*?", "a", 0, true, false),
				("(?U)(?=(a))*+", "a", 0, true, false),
				("(?U)(?*(a|ab))+\\1c", "abc", 0, true, false),
				("(?U)(?*(a|ab))++\\1c", "abc", 0, true, false),
				("(?=(a)){0}(b)", "b", 0, true, false),
				("(?!(a)){0}+(b)", "b", 0, true, false),
				("(?*(a)){0}(b)", "b", 0, true, false),
				("(?<*(a)){0}+(b)", "b", 0, true, false),
				("(?!(a)){2,3}b", "b", 0, true, false),
				("(?!(a)){2,3}+b", "b", 0, true, false),
				("(?:(?=(a))+b|a)", "a", 0, true, false),
				("(?:(?*(a|ab)){2}+d|a(b)c)", "abc", 0, true, false),
				("(?*(a|ab)){2}\\1d|a(b)c", "abc", 0, true, false),
				("(?J)(?*(?<x>a)|(?<x>ab)){2}\\k<x>c", "abc", 0, true, false),
				("(?n)(?*(?<x>a|ab)){1,2}?\\k<x>c", "abc", 0, true, false),
				("(?=(?<x>\\k<x>a|a)){3,}\\k<x>", "aaaa", 0, true, false),
				("(?=(?|(a)|(b))){2}\\1", "b", 0, true, false),
				("(?x)(?= (a) ) # count\n {2,3} # preference\n ? a", "a", 0, true, false),
				("(?=(?=a)+a){2}a", "a", 0, true, false),
				("(?!(?=b)+b){2}a", "a", 0, true, false),
				("(?=(?*(a|ab)){2})\\1c", "abc", 0, true, false),
				("(?*(?=(a|ab)){2})\\1c", "abc", 0, true, false),
				("(?*(?*(a|ab)){2})\\1c", "abc", 0, true, false),
				("(?*(a|ab))(?=\\1){2}+\\1c", "abc", 0, true, false),
				("(?:(?*(a|ab)){1,2}\\1)+c", "aabc", 0, true, false),
				("((?=(a?)){2})*a", "a", 0, true, false),
				("((?*(a?)){2}){2}a", "a", 0, true, false),
				("(?=(a))?a(?=(b))?b\\1\\2", "abab", 0, true, false),
				("(?<=(a{1,3})){2}b\\1", "aaabaa", 0, true, false),
				("(?<*(a{1,3})){2}b\\1", "aaabaa", 0, true, false),
				("(?<*(a{1,3})){2}+b\\1", "aaabaa", 0, true, false),
				("(?<*(b)|(ab)){1,2}?(?:\\1|\\2)c", "ababc", 0, true, false),
				("(?<*(a{1,2})){1,2}?\\1b", "aaab", 2, true, false),
				("(?<=a{1,2}(?=(b))){2}\\1", "aab", 2, true, false),
				("(?<*a{1,2}(?=(b))){1,2}+\\1", "aab", 2, true, false),
				("(?<*a{1,2}\\b){2}b", "aab", 2, true, false),
				("(?<*a{1,2}\\z){2}+b", "aab", 2, true, false),
				("(?<*a{1,2}(?*b*?){2}){1,2}b", "aab", 2, true, false),
				("(?<*(?<=a){2}b){2}c", "abc", 2, true, false),
				("(?<*(?<!a){2}b){2}c", "abc", 2, true, false),
				("(?<*(?<*a){2}b){2}c", "abc", 2, true, false),
				("(?<=(?=a){2}a)b", "ab", 0, true, false),
				("(?<=a(?=b){2})b", "ab", 0, true, false),
				("(?<=(?=x){0}a)b", "ab", 0, true, false),
				("(?<=ab){0}c", "abc", 2, true, false),
				("(?<!ab){0}+c", "abc", 2, true, false),
				("(*pla:(a)){2}\\1", "a", 0, true, false),
				("(*negative_lookahead:b){2,}a", "a", 0, true, false),
				("(*napla:(a|ab)){2}\\1c", "abc", 0, true, false),
				("(*non_atomic_positive_lookahead:(a|ab)){1,2}?\\1c", "abc", 0, true, false),
				("(*plb:(a)){2}b\\1", "aba", 0, true, false),
				("(*negative_lookbehind:x){2,}b", "ab", 1, true, false),
				("(*naplb:(a{1,3})){2}b\\1", "aaabaa", 0, true, false),
				("(*non_atomic_positive_lookbehind:(a{1,3})){2}+b\\1", "aaabaa", 0, true, false),
				("^(?=(\\1é|é)){3,}\\1$", "éééé", 0, true, false),
				("^(?=(\\1😀|😀))+\\1$", "😀😀", 0, true, false),
				("(?i)(?*(𐐀|𐐀a)){2}\\1b", "𐐨ab", 0, true, false),
				("(?i)(?*(é|éa)){2}\\1b", "Éab", 0, false, true),
				("(?i)(?*(é|éa)){2}\\1b", "Éab", 0, false, false),
				("(?*(\\w{1,3})){1,2}?\\1x", "éΣx", 0, true, true),
				("(?*(\\R|\\R\\R)){2}\\1x", "\r\n\r\nx", 0, true, false),
				("(*BSR_ANYCRLF)(?=(\\R)){2}\\1", "\r\n", 0, true, false),
				("(?m)(?=^a+$){2}a+", "x\naaa\nx", 0, true, false),
				("(*CR)(?m)(?=^a+$){2}a+", "x\raaa\rx", 0, true, false),
				("(*NUL)(?m)(?=^a+$){2}a+", "x\u0000aaa\u0000x", 0, true, false),
				("(?=\\G(a|ab)){2}\\1c", "zabc", 1, true, false),
				("(?*\\b(a|ab)){2}\\1c", " abc", 1, true, false),
				("(?<*😀){2}a", "x😀a", 3, true, false),
				("(?<*😀){2}a", "x😀a", 3, false, false),
				("(?=\\x{0}){2}\\x{0}", "\u0000", 0, false, false),
				("(?*(\ud800|\ud800a)){2}\\1b", "\ud800ab", 0, false, false),
				("(?<*(\udc00{1,2})){2}a\\1", "\udc00\udc00a\udc00", 0, false, false),
			};
			for ( var id = 0; id < cases.Length; id++ ) {
				var (source, subject, start, utf, ucp) = cases[id];
				foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 16, 32, 63 } ) {
					yield return new object[] { id, source, subject, start, utf, ucp, defaults };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void DirectCountsAgreeWithEveryNativePolicy( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"assertion repeat {id}: {compiled.Diagnostic}" );
		var nativePolicies = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start, defaults, ucp );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = nativePolicies[mask];
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			Assert.Equal( native.Kind == "M", result.Match is not null );
			Assert.Equal( native.Captures, Spans( result.Match ) );
		}
	}

	[Theory]
	[InlineData( @"^(?=(\1a|a)){3,}\1$", "aaaa", "0:4,0:4" )]
	[InlineData( @"^(?=(\1a|a)){3,}\1$", "aaaaa", "" )]
	[InlineData( @"^(?:(?=(\1a|a)))+\1$", "aaaa", "" )]
	[InlineData( @"^(?=(\1a|a))+a\1$", "aa", "0:2,0:1" )]
	[InlineData( @"^(?=(\1a|a))++a\1$", "aa", "" )]
	[InlineData( @"(?*(a|ab)){1}\1c", "abc", "0:3,0:2" )]
	[InlineData( @"(?*(a|ab)){1}+\1c", "abc", "" )]
	[InlineData( @"(?*(a|ab)){2}\1c", "abc", "0:3,0:2" )]
	[InlineData( @"(?<*(a{1,3})){2}b\1", "aaabaa", "3:3,1:2" )]
	[InlineData( @"(?=(a)){0}(b)", "b", "0:1,-,0:1" )]
	public void FiniteIterationsKeepCaptureGrowthAndCommitSemantics( string source, string subject, string expected ) {
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 0 );
		Assert.Equal( expected, native.Captures );
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Pattern!.Match( subject.AsMemory() );
		Assert.True( result.IsSuccess ); Assert.Equal( expected, Spans( result.Match ) );
	}

	[Theory]
	[InlineData( @"^(?=(\1é|é)){3,}\1$", "éééé" )]
	[InlineData( @"^(?=(\1😀|😀))+\1$", "😀😀" )]
	[InlineData( @"(?i)(?*(𐐀|𐐀a)){2}\1b", "𐐨ab" )]
	[InlineData( @"(?<*(a{1,3})){2}b\1", "aaabaa" )]
	[InlineData( @"(?*(\R|\R\R)){2}\1x", "\r\n\r\nx" )]
	[InlineData( @"(?=(a)){0}(b)", "b" )]
	[InlineData( @"(*napla:(a|ab)){2}\1c", "abc" )]
	public void DirectIterationsKeepCrossWidthCaptures( string source, string subject ) {
		var bytes = Encoding.UTF8.GetBytes( subject );
		var compiledBytes = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true, Ucp = true } );
		var compiledWords = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true, Ucp = true } );
		Assert.True( compiledBytes.IsSuccess ); Assert.True( compiledWords.IsSuccess );
		var byteResult = compiledBytes.Pattern!.Match( bytes );
		var wordResult = compiledWords.Pattern!.Match( subject.AsMemory() );
		Assert.True( byteResult.IsSuccess ); Assert.True( wordResult.IsSuccess );
		var b = Assert.IsType<PcreByteMatch>( byteResult.Match );
		var w = Assert.IsType<PcreWordMatch>( wordResult.Match );
		var coordinates = new PcreDualWidthCoordinates( bytes, subject );
		Assert.Equal( coordinates.ByteCaptures( String.Join( ",", b.Captures.Select( c => c is { } span
			? $"{span.ByteIndex}:{span.ByteLength}" : "-" ) ) ), coordinates.WordCaptures( Spans( w ) ) );
		Assert.Equal( b.Captures.Count, w.Captures.Count );
		for ( var number = 0; number < b.Captures.Count; number++ ) {
			if ( b.Captures[number] is not { } bc ) { Assert.Null( w.Captures[number] ); continue; }
			var wc = Assert.IsType<PcreWordCapture>( w.Captures[number] );
			Assert.Equal( Encoding.UTF8.GetString( bytes.AsSpan( bc.ByteIndex, bc.ByteLength ) ),
				subject.Substring( wc.CodeUnitIndex, wc.CodeUnitLength ) );
		}
	}

	[Theory]
	[InlineData( "(?=a){0}a", 4 )]
	[InlineData( "(?=a){0}+a", 8 )]
	[InlineData( "(?=a){2}a", 20 )]
	[InlineData( "(?*a){2}a", 20 )]
	[InlineData( "(?=(a)){2}a", 24 )]
	[InlineData( "(?=a)*a", 12 )]
	[InlineData( "(?=a)*?a", 4 )]
	[InlineData( "(?=a){2}+a", 24 )]
	[InlineData( "(?*a){2}+a", 26 )]
	[InlineData( "(?=a)+a", 20 )]
	[InlineData( "(?=a)+?a", 12 )]
	public void EveryFiniteIterationAndPossessiveCommitConsumesWork( string source, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add, Policy = new() { Anchored = true }
			} );
			Assert.True( compiled.IsSuccess );
			var result = compiled.Pattern!.Match( "a".AsMemory() );
			if ( budget < required ) {
				Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); Assert.Null( result.Match );
			} else {
				Assert.True( result.IsSuccess ); Assert.NotNull( result.Match );
				Assert.Equal( PcreNative16Oracle.ExecuteSequence( Words( source ), Words( "a" ), false, 0, 1 ).Captures, Spans( result.Match ) );
			}
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Theory]
	[InlineData( "(?=(a)){3}a" )]
	[InlineData( "(?*(a)){3}a" )]
	[InlineData( "(?*(a)){3}+a" )]
	public void CancellationDuringRepeatedBodiesLeavesThePatternReusable( string source ) {
		using var cancellation = new CancellationTokenSource();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			WorkObserver = work => { if ( work == 19 ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => pattern.Match( "a".AsMemory(), cancellationToken: cancellation.Token ) );
		Assert.Equal( "0:1,0:1", Spans( pattern.Match( "a".AsMemory() ).Match ) );
	}

	[Fact]
	public void RepeatedGuardsAndPossessiveCommitsUseTheExistingFrameCeiling() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( @"\A(?:(?*a){2}a)*\z".AsMemory() ).Pattern!;
		Assert.Equal( "0:340", Spans( pattern.Match( new string( 'a', 340 ).AsMemory() ).Match ) );
		var result = pattern.Match( new string( 'a', 341 ).AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code ); Assert.Null( result.Match );
		Assert.Equal( "0:2", Spans( pattern.Match( "aa".AsMemory() ).Match ) );
		var committed = PcreWordSequenceCompiler.CompileSequence( @"\A(?:(?*a){2}+a)*\z".AsMemory() ).Pattern!;
		Assert.Equal( "0:512", Spans( committed.Match( new string( 'a', 512 ).AsMemory() ).Match ) );
	}

	[Fact]
	public void DirectCountsKeepSourceOwnershipAndDurableCaptureResults() {
		const string text = @"(?*(?<x>a|ab)){1,2}\k<x>c|(?<y>b)";
		var source = text.ToCharArray();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source ).Pattern!;
		Array.Fill( source, 'z' );
		var first = pattern.Match( "!zabc!".AsMemory( 1, 4 ) ).Match!;
		Assert.Equal( "1:3,1:2,-", Spans( first ) );
		foreach ( var subject in new[] { "b", "x", "abc", "ac", "", "ab" } ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( text ), Words( subject ), false, 0 );
			var result = pattern.Match( subject.AsMemory() );
			Assert.True( result.IsSuccess ); Assert.Equal( native.Captures, Spans( result.Match ) );
		}
		Assert.Equal( new PcreWordCapture( 1, 2 ), first.GetCapture( "x" ) );
		Assert.Null( first.GetCapture( "y" ) ); Assert.Equal( "1:3,1:2,-", Spans( first ) );
	}

	[Fact]
	public void NormalizedCountsAndRetainedBodiesKeepAdmissionLimits() {
		foreach ( var source in new[] { "(?=a){64}", "(?=a){63,}", "(?=a){64}+",
			"(?*a){0,64}", "(?<*a){0,64}+", "(?<=a?b?c?d?e?f?){0}",
			String.Concat( Enumerable.Repeat( "(?=a)+", 300 ) ) } ) {
			Assert.True( PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).IsSuccess, source );
		}
		foreach ( var source in new[] { "(?=a){65}", "(?=a){64,}", "(?=a){64,}+",
			"(?*a){0,65}", "(?<*a){65535}", "(?=a){65535,}", "(?<=a?b?c?d?e?f?){0}g?",
			String.Concat( Enumerable.Repeat( "(?=a*){0}", 65 ) ), "(?=(?>(*sr:(?:a){65}a)a)){0}",
			"(?=a(*sr:(?:a){65}a)){0}", "(?:(?=a)){65}+", "(?>(*sr:(?:a){65}a)a)", "a{65,}+" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
		foreach ( var opener in new[] { "(?=", "(?<*", "(*napla:" } ) {
			var nested = String.Concat( String.Concat( Enumerable.Repeat( opener, 64 ) ),
				String.Concat( Enumerable.Repeat( "){0}+", 64 ) ) );
			var compiled = PcreWordSequenceCompiler.CompileSequence( nested.AsMemory() );
			Assert.True( compiled.IsSuccess ); Assert.Equal( "0:0", Spans( compiled.Pattern!.Match( ReadOnlyMemory<char>.Empty ).Match ) );
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( String.Concat( opener, nested, "){0}+" ).AsMemory() ).Diagnostic?.Code );
		}
		var repeated = PcreWordSequenceCompiler.CompileSequence( String.Concat(
			String.Concat( Enumerable.Repeat( "(?=a)+", 300 ) ), "a" ).AsMemory() ).Pattern!;
		Assert.Equal( "0:1", Spans( repeated.Match( "a".AsMemory() ).Match ) );
	}

	[Theory]
	[InlineData( "é(?=a){2,1}", 6 )]
	[InlineData( "é(?=a){65536}", 6 )]
	[InlineData( "😀(?*a)**", 8 )]
	[InlineData( "é(?=a)+?+", 8 )]
	public void MalformedQuantifiersRetainOriginalCodeUnitOffsets( string source, int offset ) {
		Assert.Equal( "E", PcreNative16Oracle.ExecuteSequence( Words( source ), Array.Empty<ushort>(), true, 0 ).Kind );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidSyntax, offset ),
			PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Diagnostic );
	}

	[Fact]
	public void ZeroCountBodiesStillContributeNativeValidationWindows() {
		foreach ( var source in new[] { @"(?=\b){0}c", @"(?<=ab){0}c", @"(?<*ab){0}+c", @"(?=a(?<=abc)){0}c" } ) {
			const string subject = "\uD800bc";
			Assert.Equal( "X", PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 1 ).Kind );
			var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Pattern!.Match( subject.AsMemory(), 1 );
			Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 0 ), result.Diagnostic );
		}
		var behind = PcreWordSequenceCompiler.CompileSequence( "(?<=ab){0}c".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( "3:1", Spans( behind.Match( "\uD800abc".AsMemory(), 3 ).Match ) );
		var scalar = PcreWordSequenceCompiler.CompileSequence( "(?=😀){2}😀".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidStartOffset, 1 ), scalar.Match( "😀".AsMemory(), 1 ).Diagnostic );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 2 ), scalar.Match( "😀\uD800".AsMemory() ).Diagnostic );
	}

	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( c => c is { } span ? $"{span.CodeUnitIndex}:{span.CodeUnitLength}" : "-" ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
