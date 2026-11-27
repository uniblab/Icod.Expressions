using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordConditionalTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new (string Source, string Subject, int Start, bool Utf, bool Ucp)[] {
				("^(a)?(?(1)b|c)$", "ab", 0, true, false),
				("^(a)?(?(1)b|c)$", "c", 0, true, false),
				("^(a)?(?(1)b|c)$", "ac", 0, true, false),
				("^(a)?(?(1)b)$", "", 0, true, false),
				("^()(?(1)a|b)$", "a", 0, true, false),
				("^(a)(?(1)x|b)$", "ab", 0, true, false),
				("^(?(1)a|b)(a)$", "ba", 0, true, false),
				("^(?(+1)a|b)(a)$", "ba", 0, true, false),
				("^(a)?(?(-1)b|c)$", "ab", 0, true, false),
				("^(a)?(?(-1)b|c)$", "c", 0, true, false),
				("^((?(-1)a|b))$", "b", 0, true, false),
				("(a)(?(+01)b|c)(d)", "acd", 0, true, false),
				("(a)(?(-01)b|c)", "ab", 0, true, false),
				("(a)(?(01)b|c)", "ab", 0, true, false),
				("(a)(?(1)|)", "a", 0, true, false),
				("(?J)^(?:(?<x>a)|(?<x>b))(?(x)c|d)$", "bc", 0, true, false),
				("(?J)^(?<x>a)?(?<x>b)?(?(x)c|d)$", "bc", 0, true, false),
				("(?J)^(?<x>)?(?<x>b)?(?(x)c|d)$", "c", 0, true, false),
				("(?J)^(?<x>a)?(?<x>b)(?(1)c|d)$", "bd", 0, true, false),
				("(?J)^(?<x>a)?(?<x>b)(?(1)c|d)$", "bc", 0, true, false),
				("(?J)^(?<x>a)(?<x>b)(?(x)\\k<x>|b)$", "aba", 0, true, false),
				("(?J)^(?<x>a)(?<x>b)(?(x)\\k<x>|b)$", "abb", 0, true, false),
				("^(a)?(?(1)b|a)c$", "ac", 0, true, false),
				("^((?(1)b|a))+$", "ab", 0, true, false),
				("^(?*(a)|ab)(?(1)\\1x|abc)$", "abc", 0, true, false),
				("^(?=(a)|ab)(?(1)\\1x|abc)$", "abc", 0, true, false),
				("^(?!(a)b)(?(1)b|a)$", "a", 0, true, false),
				("^(?:(?>(a))x|ab)(?(1)c|d)$", "abd", 0, true, false),
				("(?n)(a)(?<x>b)(?(x)c|d)", "abc", 0, true, false),
				("(?|(a)|(b))(?(-1)c|d)", "bc", 0, true, false),
				("(a)(?(1)(?i)b|c)d", "aBd", 0, true, false),
				("(a)(?(1)(?i)b|c)d", "aBD", 0, true, false),
				("(a)?(?(1)(?i)b|c)d", "Cd", 0, true, false),
				("(?i)(a)(?(1)(?r:k)|x)", "aK", 0, true, false),
				("(?i)(a)(?(1)k|x)", "aK", 0, true, false),
				("(?ir)(a)(?(1)(?-r:k)|x)", "aK", 0, true, false),
				("(?U)^(a)?(?(1)b|a)+$", "aaa", 0, true, false),
				("(?<字٢>é)(?(字٢)😀|x)", "é😀", 0, true, false),
				("^(?<R>a)(?(R)b|c)$", "ab", 0, true, false),
				("^(?(R)b|c)(?<R>a)$", "ca", 0, true, false),
				("^(?<R01>a)(?(R01)b|c)$", "ab", 0, true, false),
				("^(?(R1)b|c)(?<R1>a)$", "ca", 0, true, false),
				("(?<R١>a)(?(R١)b|c)", "ab", 0, true, false),
				("(?<DEFINE>a)(?(<DEFINE>)b|c)", "ab", 0, true, false),
				("(?<DEFINE>a)(?('DEFINE')b|c)", "ab", 0, true, false),
				("(?<VERSION>a)(?(VERSION)b|c)", "ab", 0, true, false),
				("(?<VERSIONX>a)(?(<VERSIONX>)b|c)", "ab", 0, true, false),
				("(?<versionx>a)(?(versionx)b|c)", "ab", 0, true, false),
				("(a)?(?<=(?(1)b))x", "x", 0, true, false),
				("(a)?(?<=(?(1)b))x", "bx", 0, true, false),
				("(a)?(?<=(?(1)b|))x", "x", 0, true, false),
				("(a)(?<=(?(1)a|bb))x", "ax", 0, true, false),
				("(?<=(?<x>(?(x)b|a)))x", "ax", 0, true, false),
				("^(?(R)a|b)(?(R0)c|d)(?(R000)e|f)$", "bdf", 0, true, false),
				("^((?(R1)a|b))$", "b", 0, true, false),
				("^(?(DEFINE)(?<a>(?(R)a|x)(?(R0)b|x)(?(R000)c|x)(?(R01)d|x)))(?&a)$", "abcd", 0, true, false),
				("^(?(DEFINE)(?<a>(?&b)(?(R1)f|X))(?<b>(?(R1)X|b)(?(R2)c|X)(?(R&a)X|d)(?(R&b)e|X)))(?&a)$", "bcdef", 0, true, false),
				("^(?(DEFINE)(?<a>a|ab))(?&a)(?(R)X|c)$", "abc", 0, true, false),
				("^(?(DEFINE)(?<a>(?*(a|ab))(?(R1)\\2|X)))(?&a)c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<a>(?=(a|ab))(?(R1)\\2|X)))(?&a)c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<a>(?>(a|ab))(?(R1)|X)))(?&a)c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<a>a))(?!(?&a)b)(?(R)X|ac)$", "ac", 0, true, false),
				("^(?(DEFINE)(?<a>a))(?=(?&a))(?(R)X|a)$", "a", 0, true, false),
				("^(?(DEFINE)(?<a>a))(?:(?>(?&a))x|a)(?(R)X|b)$", "ab", 0, true, false),
				("^(?(DEFINE)(?<a>a))(?(?=(?&a))a|X)(?(R)X|b)$", "ab", 0, true, false),
				("^(?(DEFINE)(?<a>(?(R)b|a)))(?&a)$", "a", 0, true, false),
				("^(?(R)b|a)c$", "bc", 0, true, false),
				("(?J)^(?(DEFINE)(?<x>X)(?<x>(?(R&x)a|X)))(?2)$", "a", 0, true, false),
				("(?J)^(?|(?<x>(?(R&x)a|b))|(?<x>X))(?1)$", "ba", 0, true, false),
				("(?J)^(?<R>a)?(?<R>b)(?(R)c|d)$", "bc", 0, true, false),
				("^(?(R65536)b|c)(?<R65536>a)$", "ca", 0, true, false),
				("^(?(DEFINE)(?<a>(?(R1)X|b))(?<R1>a))(?&a)$", "b", 0, true, false),
				("^(?<R>a)(?(DEFINE)(?<g>(?(R&g)b|X)(?(<R>)c|X)(?('R')d|X)))(?&g)$", "abcd", 0, true, false),
				("^(?<R>(?(R&R)a|b))(?&R)$", "ba", 0, true, false),
				("^(?(DEFINE)(?<字٢>(?(R&字٢)é|x)))(?&字٢)$", "é", 0, true, false),
				("^(?<R١>a)(?(R١)b|c)$", "ab", 0, true, false),
				("^(?<r>a)(?(r)b|c)$", "ab", 0, true, false),
				("^(?(R)(?<x>a)|(?<y>b))(?(y)c|X)$", "bc", 0, true, false),
				("^(?(R)(?&x)|b)(?(DEFINE)(?<x>a))$", "b", 0, true, false),
				("(?<=(?(R)a))b", "b", 0, true, false),
				("(?<=(?(R)a))b", "ab", 0, true, false),
				("(?<=(?(R)a|))b", "b", 0, true, false),
				("(?(DEFINE)(?<g>(?<=(?(R1)a|bb))b))(?&g)", "ab", 0, true, false),
				("(?(DEFINE)(?<g>(?(R1)a|bb)))(?<=(?&g))c", "ac", 0, true, false),
				("(?(DEFINE)(?<g>(?(R1)a|bb)))(?<=(?&g))c", "bbc", 0, true, false),
				("^(?(DEFINE)(a|b))c$", "c", 0, true, false),
				("^(?<DEFINE>a)(?(DEFINE)b)c$", "ac", 0, true, false),
				("^(?<DEFINE>a)(?(<DEFINE>)b|c)$", "ab", 0, true, false),
				("^(?<DEFINE>a)(?('DEFINE')b|c)$", "ab", 0, true, false),
				("^(?<VERSION>a)(?(VERSION)b|c)$", "ab", 0, true, false),
				("^(?<VERSIONX>a)(?(<VERSIONX>)b|c)$", "ab", 0, true, false),
				("^(?<versionx>a)(?(versionx)b|c)$", "ab", 0, true, false),
				("^(?(DEFINE)(?<x>a))(?(x)b|c)(d)$", "cd", 0, true, false),
				("^(?(x)b|c)(?(DEFINE)(?<x>a))(d)$", "cd", 0, true, false),
				("^(?(DEFINE)(a))\\1$", "a", 0, true, false),
				("^(?(DEFINE)(a)){0}(b)\\2$", "bb", 0, true, false),
				("^(?(VERSION=10.48)(a)|(b))(c)$", "ac", 0, true, false),
				("^(?(VERSION=10.480)(a)|(b))(c)$", "bc", 0, true, false),
				("^(?(VERSION=10.48)b|a)$", "a", 0, true, false),
				("^(?(VERSION=10.49)a|b)$", "a", 0, true, false),
				("^(?(VERSION=10.48)(?i)a|b)c$", "Ac", 0, true, false),
				("^(?(VERSION=10.49)(?i)a|b)c$", "Bc", 0, true, false),
				("^(?(VERSION=10.49)(?i)a|b)c$", "BC", 0, true, false),
				("^(?(DEFINE)(?i)a)b$", "B", 0, true, false),
				("^(?(VERSION=10.49)a|){2}b$", "b", 0, true, false),
				("(?<=(?(DEFINE)a*))b", "b", 0, true, false),
				("(?<=(?(VERSION=10.49)a))b", "b", 0, true, false),
				("(?<=(?(VERSION=10.49)a))b", "ab", 0, true, false),
				("(?<=(?(VERSION=10.49)a){2})b", "ab", 0, true, false),
				("(?<=(?(VERSION=10.49)a){2})b", "aab", 0, true, false),
				("(?<=(?(VERSION=10.49)a|))b", "b", 0, true, false),
				("(?<=(?(VERSION=10.48)a|bb))c", "ac", 0, true, false),
				("^(?(?=(a))\\1|X)$", "a", 0, true, false),
				("^(?(?!(a))X|\\1)$", "a", 0, true, false),
				("^a(?(?<=(a))\\1|X)$", "aa", 0, true, false),
				("^a(?(?<!(a))X|\\1)$", "aa", 0, true, false),
				("^(?(?=())a|X)$", "a", 0, true, false),
				("^(?(?!())X|a)$", "a", 0, true, false),
				("^(?(?=(a)b)X|(?(1)Y|a))$", "a", 0, true, false),
				("^(?(?!(a)b)(?(1)Y|a)|X)$", "a", 0, true, false),
				("^(?(?=(a|ab))\\1c|X)$", "abc", 0, true, false),
				("^(?(?!(a|ab))X|\\1c)$", "ac", 0, true, false),
				("^(?(?!(a|ab))X|\\1c)$", "abc", 0, true, false),
				("^(?(?=(a))b|a)$", "a", 0, true, false),
				("^(?(?!b)b|a)$", "a", 0, true, false),
				("^(?:(a)|ab)(?(?=c)(?(1)X|c)|Y)$", "abc", 0, true, false),
				("^(?*(a)|ab)(?(?=a)(?(1)X|abc)|Y)$", "abc", 0, true, false),
				("^(?(?=(?*(a|ab)))\\1c|X)$", "abc", 0, true, false),
				("^(?(?=(?*(a|ab))\\1c)abc|X)$", "abc", 0, true, false),
				("^(?:(?(?=(?>(a)))X|Y)|a)(?(1)X|b)$", "ab", 0, true, false),
				("^(?(?=(?(?=(a))a|X))\\1|Y)$", "a", 0, true, false),
				("^(?(?=(?!(a)b))a|X)$", "a", 0, true, false),
				("^(?>(?(?=(a))a|b))+c$", "aabc", 0, true, false),
				("^(?(?=a)a|b){2,3}?c$", "abbc", 0, true, false),
				("^(?(?=a)a|b)++c$", "abbc", 0, true, false),
				("^(?(?=z)X)$", "", 0, true, false),
				("^(?(?!z))$", "", 0, true, false),
				("^(?(?=a)|)a$", "a", 0, true, false),
				("(?(?=a)a){0}b", "b", 0, true, false),
				("^(?(?=(?i)a)A|B)c$", "Ac", 0, true, false),
				("^(?(?=(?i)a)A|B)c$", "AC", 0, true, false),
				("^(?(?=a)(?i)a|b)c$", "Bc", 0, true, false),
				("^(?(?=a)(?i)a|b)c$", "BC", 0, true, false),
				("(?n)^(?(?=(a)(?<x>b))ab|X)\\k<x>$", "abb", 0, true, false),
				("(?J)^(?(?=(?<x>a)|(?<x>b))\\k<x>|X)$", "b", 0, true, false),
				("^(?(?=(?|(a)|(b)))\\g{-1}|X)$", "b", 0, true, false),
				("^(?(?=a)(b)|(c))(d)$", "cd", 0, true, false),
				("^(?(?=(?(+1)X|a))(a)|X)$", "a", 0, true, false),
				("(?x)^(?(?= (a) ) \\1 | X )$", "a", 0, true, false),
				("^(?(?=(?<字٢>é😀))\\k<字٢>|X)$", "é😀", 0, true, false),
				("(?(?<=(a{1,2}))\\1b|X)", "aaaab", 0, true, false),
				("^aa(?(?<=(a{1,2}))\\1b|X)$", "aaab", 0, true, false),
				("^aa(?(?<!(a{1,2}))X|\\1b)$", "aaab", 0, true, false),
				("(?<=(?(?=b)a|xx))b", "ab", 0, true, false),
				("(?<=(?(?=a)a|xx))b", "ab", 0, true, false),
				("(?<=(?(?=z)a))x", "x", 0, true, false),
				("(?<=(?(?=z)a))x", "ax", 0, true, false),
				("(?<=(?(?=z)a|))x", "x", 0, true, false),
				("(?(?<=(é|😀a))b|X)", "😀ab", 0, true, false),
				("^(?<x>a)?(?(x)b|c)$", "ab", 0, true, false),
				("^(?<x>a)?(?(x)b|c)$", "c", 0, true, false),
				("^(?(x)b|c)(?<x>a)$", "ca", 0, true, false),
				("^(?<x>)(?(x)a|b)$", "a", 0, true, false),
				("^(?<x>a)?(?(<x>)b|c)$", "ab", 0, true, false),
				("^(?<x>a)?(?(<x>)b|c)$", "c", 0, true, false),
				("^(?(<x>)b|c)(?<x>a)$", "ca", 0, true, false),
				("^(?<x>)(?(<x>)a|b)$", "a", 0, true, false),
				("^(?<x>a)?(?('x')b|c)$", "ab", 0, true, false),
				("^(?<x>a)?(?('x')b|c)$", "c", 0, true, false),
				("^(?('x')b|c)(?<x>a)$", "ca", 0, true, false),
				("^(?<x>)(?('x')a|b)$", "a", 0, true, false),
				("^(?(?=(a))a|b)$", "a", 0, false, false),
				("^(?(?=(a))a|b)$", "a", 0, true, false),
				("^(?(?=(z))a|b)$", "a", 0, false, false),
				("^(?(?=(z))a|b)$", "a", 0, true, false),
				("^(?(?=)a|b)$", "a", 0, false, false),
				("^(?(?=)a|b)$", "a", 0, true, false),
				("^(?(?!(a))b|a)$", "a", 0, false, false),
				("^(?(?!(a))b|a)$", "a", 0, true, false),
				("^(?(?!(z))b|a)$", "a", 0, false, false),
				("^(?(?!(z))b|a)$", "a", 0, true, false),
				("^(?(?!)b|a)$", "a", 0, false, false),
				("^(?(?!)b|a)$", "a", 0, true, false),
				("^a(?(?<=(a))a|b)$", "aa", 0, false, false),
				("^a(?(?<=(a))a|b)$", "aa", 0, true, false),
				("^a(?(?<=(z))a|b)$", "aa", 0, false, false),
				("^a(?(?<=(z))a|b)$", "aa", 0, true, false),
				("^a(?(?<=)a|b)$", "aa", 0, false, false),
				("^a(?(?<=)a|b)$", "aa", 0, true, false),
				("^a(?(?<!(a))b|a)$", "aa", 0, false, false),
				("^a(?(?<!(a))b|a)$", "aa", 0, true, false),
				("^a(?(?<!(z))b|a)$", "aa", 0, false, false),
				("^a(?(?<!(z))b|a)$", "aa", 0, true, false),
				("^a(?(?<!)b|a)$", "aa", 0, false, false),
				("^a(?(?<!)b|a)$", "aa", 0, true, false),
				("^(?(*pla:(a))a|b)$", "a", 0, false, false),
				("^(?(*pla:(a))a|b)$", "a", 0, true, false),
				("^(?(*pla:(z))a|b)$", "a", 0, false, false),
				("^(?(*pla:(z))a|b)$", "a", 0, true, false),
				("^(?(*pla:)a|b)$", "a", 0, false, false),
				("^(?(*pla:)a|b)$", "a", 0, true, false),
				("^(?(*positive_lookahead:(a))a|b)$", "a", 0, false, false),
				("^(?(*positive_lookahead:(a))a|b)$", "a", 0, true, false),
				("^(?(*positive_lookahead:(z))a|b)$", "a", 0, false, false),
				("^(?(*positive_lookahead:(z))a|b)$", "a", 0, true, false),
				("^(?(*positive_lookahead:)a|b)$", "a", 0, false, false),
				("^(?(*positive_lookahead:)a|b)$", "a", 0, true, false),
				("^(?(*nla:(a))b|a)$", "a", 0, false, false),
				("^(?(*nla:(a))b|a)$", "a", 0, true, false),
				("^(?(*nla:(z))b|a)$", "a", 0, false, false),
				("^(?(*nla:(z))b|a)$", "a", 0, true, false),
				("^(?(*nla:)b|a)$", "a", 0, false, false),
				("^(?(*nla:)b|a)$", "a", 0, true, false),
				("^(?(*negative_lookahead:(a))b|a)$", "a", 0, false, false),
				("^(?(*negative_lookahead:(a))b|a)$", "a", 0, true, false),
				("^(?(*negative_lookahead:(z))b|a)$", "a", 0, false, false),
				("^(?(*negative_lookahead:(z))b|a)$", "a", 0, true, false),
				("^(?(*negative_lookahead:)b|a)$", "a", 0, false, false),
				("^(?(*negative_lookahead:)b|a)$", "a", 0, true, false),
				("^a(?(*plb:(a))a|b)$", "aa", 0, false, false),
				("^a(?(*plb:(a))a|b)$", "aa", 0, true, false),
				("^a(?(*plb:(z))a|b)$", "aa", 0, false, false),
				("^a(?(*plb:(z))a|b)$", "aa", 0, true, false),
				("^a(?(*plb:)a|b)$", "aa", 0, false, false),
				("^a(?(*plb:)a|b)$", "aa", 0, true, false),
				("^a(?(*positive_lookbehind:(a))a|b)$", "aa", 0, false, false),
				("^a(?(*positive_lookbehind:(a))a|b)$", "aa", 0, true, false),
				("^a(?(*positive_lookbehind:(z))a|b)$", "aa", 0, false, false),
				("^a(?(*positive_lookbehind:(z))a|b)$", "aa", 0, true, false),
				("^a(?(*positive_lookbehind:)a|b)$", "aa", 0, false, false),
				("^a(?(*positive_lookbehind:)a|b)$", "aa", 0, true, false),
				("^a(?(*nlb:(a))b|a)$", "aa", 0, false, false),
				("^a(?(*nlb:(a))b|a)$", "aa", 0, true, false),
				("^a(?(*nlb:(z))b|a)$", "aa", 0, false, false),
				("^a(?(*nlb:(z))b|a)$", "aa", 0, true, false),
				("^a(?(*nlb:)b|a)$", "aa", 0, false, false),
				("^a(?(*nlb:)b|a)$", "aa", 0, true, false),
				("^a(?(*negative_lookbehind:(a))b|a)$", "aa", 0, false, false),
				("^a(?(*negative_lookbehind:(a))b|a)$", "aa", 0, true, false),
				("^a(?(*negative_lookbehind:(z))b|a)$", "aa", 0, false, false),
				("^a(?(*negative_lookbehind:(z))b|a)$", "aa", 0, true, false),
				("^a(?(*negative_lookbehind:)b|a)$", "aa", 0, false, false),
				("^a(?(*negative_lookbehind:)b|a)$", "aa", 0, true, false),
				("^(?(VERSION=10.48)(a)|(b))$", "a", 0, true, false),
				("^(?(VERSION=10.48)(a)|(b))$", "b", 0, true, false),
				("^(?(VERSION=10.048)(a)|(b))$", "a", 0, true, false),
				("^(?(VERSION=10.048)(a)|(b))$", "b", 0, true, false),
				("^(?(VERSION=00010.00048)(a)|(b))$", "a", 0, true, false),
				("^(?(VERSION=00010.00048)(a)|(b))$", "b", 0, true, false),
				("^(?(VERSION=10.480)(a)|(b))$", "a", 0, true, false),
				("^(?(VERSION=10.480)(a)|(b))$", "b", 0, true, false),
				("^(?(VERSION=10)(a)|(b))$", "a", 0, true, false),
				("^(?(VERSION=10)(a)|(b))$", "b", 0, true, false),
				("^(?(VERSION=0)(a)|(b))$", "a", 0, true, false),
				("^(?(VERSION=0)(a)|(b))$", "b", 0, true, false),
				("^(?(VERSION=1000.1000)(a)|(b))$", "a", 0, true, false),
				("^(?(VERSION=1000.1000)(a)|(b))$", "b", 0, true, false),
				("^(?(VERSION>=10)(a)|(b))$", "a", 0, true, false),
				("^(?(VERSION>=10)(a)|(b))$", "b", 0, true, false),
				("^(?(VERSION>=10.048)(a)|(b))$", "a", 0, true, false),
				("^(?(VERSION>=10.048)(a)|(b))$", "b", 0, true, false),
				("^(?(VERSION>=10.480)(a)|(b))$", "a", 0, true, false),
				("^(?(VERSION>=10.480)(a)|(b))$", "b", 0, true, false),
				("^(?(VERSION>=11)(a)|(b))$", "a", 0, true, false),
				("^(?(VERSION>=11)(a)|(b))$", "b", 0, true, false),
				("^(?(VERSION>=0.1000)(a)|(b))$", "a", 0, true, false),
				("^(?(VERSION>=0.1000)(a)|(b))$", "b", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x)*\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x)*\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a)))*\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x)*?\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x)*?\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a)))*?\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x)*+\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x)*+\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a)))*+\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x)+\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x)+\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a)))+\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x)+?\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x)+?\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a)))+?\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x)++\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x)++\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a)))++\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x){1,}\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x){1,}\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a))){1,}\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x){1,}?\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x){1,}?\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a))){1,}?\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x){1,}+\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x){1,}+\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a))){1,}+\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x){2,}\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x){2,}\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a))){2,}\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x){2,}?\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x){2,}?\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a))){2,}?\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x){2,}+\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x){2,}+\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a))){2,}+\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x){3}\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x){3}\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a))){3}\\1", "aaaa", 0, true, false),
				("()(?(1)(?=(\\2a|a))|x){1,3}?\\2", "aaaa", 0, true, false),
				("(?(VERSION=10.48)(?=(\\1a|a))|x){1,3}?\\1", "aaaa", 0, true, false),
				("(?(?=(\\1a|a))){1,3}?\\1", "aaaa", 0, true, false),
				("(a)?(?(1)b?c?|a?b?)d", "abcd", 0, true, false),
				("(a)?(?(1)b?c?|a?b?)d", "abd", 0, true, false),
				("(a)?(?(1)b?c?|a?b?)d", "bd", 0, true, false),
				("(a)(?(1)b?c?|ab)d", "aabd", 0, true, false),
				("(?(VERSION=10.48)a?b?|c?d?)x", "abx", 0, true, false),
				("(?(VERSION=0)a?b?|c?d?)x", "cdx", 0, true, false),
				("(?(?=a)a?b?|c?d?)x", "abx", 0, true, false),
				("(?(?=a)c?d?|a?b?)x", "abx", 0, true, false),
				("(?(DEFINE)(?<g>(?(R1)a?b?|X)))(?&g)x", "abx", 0, true, false),
				("(?(DEFINE)(?<g>(a|ab)))(?&g(2))(?(2)\\2c|X)", "ababc", 0, true, false),
				("(?(DEFINE)(?<g>(a|ab)))(?&g)(?(2)X|c)", "abc", 0, true, false),
				("(?(DEFINE)(?<g>(a)))(?&g(2))(?(2)\\2|X)", "aa", 0, true, false),
				("(?(DEFINE)(?<g>(?(R&g)a(?&g)?b|X)))(?&g)", "aaabbb", 0, true, false),
				("<(?(R)a|b)(?R)?>(?(R)|c)", "<b<a>>c", 0, true, false),
				("(?(DEFINE)(?<a>(?&b)(?(R&a)a|X))(?<b>(?(R&a)X|b)))(?&a)(?(R)X|c)", "bac", 0, true, false),
				("(?(DEFINE)(?<a>(?(R1)(a|ab)|X)))(?&a(2))(?(R)X|c)", "abc", 0, true, false),
				("(?(DEFINE)(?<a>(?(R1)a|X)))(?>(?&a))(?(R)X|b)", "ab", 0, true, false),
				("(?(DEFINE)(?<a>(?(R1)a|X)))(?*(?&a))(?(R)X|a)", "a", 0, true, false),
				("(?(DEFINE)(?<a>(?(R1)a|X)))(?(?=(?&a))(?(R)X|a)|X)", "a", 0, true, false),
				("(?(DEFINE)(?<a>a)){2}(?&a)", "a", 0, true, false),
				("(?(DEFINE)(?<a>a))*+(?&a)", "a", 0, true, false),
				("(?(VERSION=0)(?<a>a)){2}(?&a)", "a", 0, true, false),
				("(?(VERSION=0)a){0,64}+b", "b", 0, true, false),
				("(?(VERSION=0)(?<a>a)|(?<b>b)){2}(?(b)b|X)", "bbb", 0, true, false),
				("(?<=(?(R)a)|b{1,2})x", "ax", 0, true, false),
				("(?<=(?(R)a)|b{1,2})x", "x", 0, true, false),
				("(?<!(?(R)a)|b{1,2})x", "ax", 0, true, false),
				("(?<=(?(R)a|b{1,2}))x", "bbx", 0, true, false),
				("(?(?<=(?(R)a)|b{1,2})x|y)", "ax", 0, true, false),
				("(?<*(?(R)a)|b{1,2})x", "ax", 0, true, false),
				("(?<字٢>é)(?(字٢)😀|x)", "zé😀", 1, true, false),
				("(?<𐐀>😀)(?(<𐐀>)é|x)", "😀é", 0, true, false),
				("(?i)(?<x>𐐀)(?(x)𐐀|x)", "𐐨𐐨", 0, true, false),
				("(?i)(?(VERSION=10.48)é𐐀|x)", "É𐐨", 0, true, false),
				("(?(?=(?<x>é😀)))(?(x)\\k<x>|X)", "é😀", 0, true, false),
				("(?(?!(?<x>é😀))X|\\k<x>)", "é😀", 0, true, false),
				("(?(DEFINE)(?<字>(?(R&字)\\p{Greek}+|X)))(?&字)", "Σς", 0, true, true),
				("(?<x>\\w+)(?(x)\\k<x>|X)", "éé", 0, true, true),
				("(?m)(?(?=^a)(a)$|b)", "z\na\nx", 0, true, false),
				("(*CR)(?m)(?(?=^a)(a)$|b)", "z\ra\rx", 0, true, false),
				("(*NUL)(?m)(?(?=^a)(a)$|b)", "z\u0000a\u0000x", 0, true, false),
				("(?(?=\\R)\\R|x)++a", "\r\n\ra", 0, true, false),
				("(?(?=\\G).|X)", "z😀", 1, true, false),
				("(?(?<=😀)a|b)", "😀a", 2, true, false),
				("(?(VERSION=10.48)\\x{0}|a)", "\u0000", 0, false, false),
				("(?<x>\ud800)(?(x)\\k<x>|a)", "\ud800\ud800", 0, false, false),
				("(?(?=\udc00)\udc00|a)", "\udc00", 0, false, false),
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
	public void AllConditionFamiliesAgreeWithEveryNativePolicy( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"condition {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			Assert.True( native.Captures == Spans( result.Match ),
				$"condition {id}/{defaults}/{mask}: expected {native.Captures}, actual {Spans( result.Match )}" );
			Assert.Equal( native.Kind == "M", result.Match is not null );
		}
	}

	[Theory]
	[InlineData( @"(a)?(?(1)b|c)", "c", "0:1,-" )]
	[InlineData( @"()(?(1)a|b)", "a", "0:1,0:0" )]
	[InlineData( @"(a)(?(1)b|a)", "aa", "" )]
	[InlineData( @"(?(?!(a))X|\1)", "a", "0:1,0:1" )]
	[InlineData( @"(?(?=(a)b)X|(?(1)Y|a))", "a", "0:1,-" )]
	[InlineData( @"(?(?=(a|ab))\1c|X)", "abc", "" )]
	[InlineData( @"(?(VERSION=10.480)(a)|(b))", "b", "0:1,-,0:1" )]
	[InlineData( @"(?<=(?(R)a))x", "ax", "1:1" )]
	[InlineData( @"(?<=(?(R)a|))x", "x", "0:1" )]
	[InlineData( @"(?(DEFINE)(?<g>(?(R1)(a|ab)|X)))(?&g(2))(?(R)X|c)", "abc", "0:3,-,0:2" )]
	[InlineData( @"(?(DEFINE)(?<a>(?&b)(?(R&a)a|X))(?<b>(?(R&a)X|b)))(?&a)(?(R)X|c)", "bac", "0:3,-,-" )]
	[InlineData( @"()(?(1)(?=(\2a|a))|x){2,}\2", "aaaa", "0:2,0:0,0:2" )]
	public void CommittedArmsCaptureStateAndCurrentCallsHaveExplicitOutcomes( string source, string subject, string expected ) {
		Assert.Equal( expected, PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 0 ).Captures );
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.True( compiled.IsSuccess ); var result = compiled.Pattern!.Match( subject.AsMemory() );
		Assert.True( result.IsSuccess ); Assert.Equal( expected, Spans( result.Match ) );
	}

	[Theory]
	[InlineData( @"()(?(1)(?=(\2a|a))|x){1,}\2", "aaaa" )]
	[InlineData( @"()(?(1)(?=(\2a|a))|x){2,}\2", "aaaa" )]
	[InlineData( @"()(?(1)(?=(\2a|a))|x){2,}+\2", "aaaa" )]
	[InlineData( @"(?(VERSION=10.48)(?=(\1a|a))|x){2,}\1", "aaaa" )]
	[InlineData( @"(?(?=(\1a|a))){2,}\1", "aaaa" )]
	[InlineData( @"(?(DEFINE)(?<g>(?(R1)(?=(\2a|a))|x){2,}\2))(?&g(2))", "aaaa" )]
	[InlineData( @"(?(?!(a))X|\1)", "a" )]
	[InlineData( @"(?<=(?(R)a)|b{1,2})x", "ax" )]
	[InlineData( @"(?<=(?(R)a|))x", "x" )]
	[InlineData( @"(?(DEFINE)(?<g>(?(R1)(a|ab)|X)))(?&g(2))(?(R)X|c)", "abc" )]
	[InlineData( @"(?(DEFINE)(?<g>(?(R1)a(?&g)?b|X)))(?&g)", "aaabbb" )]
	[InlineData( @"(?(DEFINE)(?<g>a)){2}(?&g)", "a" )]
	[InlineData( @"(?<字٢>é)(?(字٢)😀|x)", "zé😀" )]
	[InlineData( @"(?i)(?<x>𐐀)(?(x)𐐀|x)", "𐐨𐐨" )]
	[InlineData( @"(?(?<=(é|😀a))b|X)", "😀ab" )]
	[InlineData( @"(?(?!(?<x>é😀))X|\k<x>)", "é😀" )]
	public void ConditionsKeepCrossWidthCaptureCoordinatesAndValues( string source, string subject ) {
		var bytes = Encoding.UTF8.GetBytes( subject );
		var b = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true, Ucp = true } );
		var w = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true, Ucp = true } );
		Assert.True( b.IsSuccess ); Assert.True( w.IsSuccess );
		var coordinates = new PcreDualWidthCoordinates( bytes, subject );
		for ( var mask = 0; mask < 64; mask++ ) {
			var policy = PcreMatchPolicyConsumer.Policy( mask );
			var br = b.Pattern!.Match( bytes, policy ); var wr = w.Pattern!.Match( subject.AsMemory(), policy );
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 0, mask: mask, ucp: true );
			Assert.True( br.IsSuccess ); Assert.True( wr.IsSuccess );
			Assert.True( native.Kind is "M" or "N" ); Assert.Equal( native.Captures, Spans( wr.Match ) );
			var spans = br.Match is null ? "" : String.Join( ",", br.Match.Captures.Select( c => c is { } span
				? $"{span.ByteIndex}:{span.ByteLength}" : "-" ) );
			Assert.Equal( coordinates.WordCaptures( native.Captures ), coordinates.ByteCaptures( spans ) );
			if ( br.Match is null ) { Assert.Null( wr.Match ); continue; }
			for ( var number = 0; number < br.Match.Captures.Count; number++ ) {
				if ( br.Match.Captures[number] is not { } bc ) { Assert.Null( wr.Match!.Captures[number] ); continue; }
				var wc = Assert.IsType<PcreWordCapture>( wr.Match!.Captures[number] );
				Assert.Equal( Encoding.UTF8.GetString( bytes.AsSpan( bc.ByteIndex, bc.ByteLength ) ),
					subject.Substring( wc.CodeUnitIndex, wc.CodeUnitLength ) );
			}
		}
	}

	[Theory]
	[InlineData( "(?(VERSION=10.48)a|b)", "a", 5 )]
	[InlineData( "()(?(1)a|b)", "a", 8 )]
	[InlineData( "(?(R)a|b)", "b", 5 )]
	[InlineData( "(?(DEFINE)(?<g>(?(R1)a|X)))(?&g)", "a", 11 )]
	[InlineData( "(?(?=a)a|b)", "a", 9 )]
	[InlineData( "(?(?!a)b|a)", "a", 9 )]
	[InlineData( "(?(?=z)a|b)", "b", 8 )]
	[InlineData( @"(?(?!(a))X|\1)", "a", 12 )]
	public void SelectionLookupAndAssertionGuardsConsumeExactWork( string source, string subject, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add, Policy = new() { Anchored = true }
			} );
			Assert.True( compiled.IsSuccess ); var result = compiled.Pattern!.Match( subject.AsMemory() );
			if ( budget < required ) {
				Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); Assert.Null( result.Match );
			} else {
				Assert.True( result.IsSuccess ); Assert.NotNull( result.Match );
				Assert.Equal( PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), false, 0, 1 ).Captures, Spans( result.Match ) );
			}
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Theory]
	[InlineData( @"(?(?=(a|ab))\1c|X)", "ac", 10, "0:2,0:1" )]
	[InlineData( @"(?(?=(a)b)X|a)", "a", 8, "0:1,-" )]
	[InlineData( @"(?(?!(a))X|\1)", "a", 8, "0:1,0:1" )]
	public void CancellationDuringTestPruningUndoAndCommitLeavesPatternsReusable( string source, string subject, int cancelAt, string expected ) {
		using var cancellation = new CancellationTokenSource();
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			WorkObserver = work => { if ( work == cancelAt ) { cancellation.Cancel(); } }
		} );
		Assert.True( compiled.IsSuccess );
		Assert.Throws<OperationCanceledException>( () => compiled.Pattern!.Match( subject.AsMemory(), cancellationToken: cancellation.Token ) );
		Assert.Equal( expected, Spans( compiled.Pattern!.Match( subject.AsMemory() ).Match ) );
	}

	[Fact]
	public void DuplicateCaptureAndRecursionLookupsHonorWorkAndCancellation() {
		var prefix = String.Concat( "(?J)", String.Concat( Enumerable.Repeat( "(?<x>){0}", 255 ) ) );
		foreach ( var source in new[] { String.Concat( prefix, "(?<x>)(?(x)a|b)" ),
			String.Concat( prefix, "(?<x>(?(R&x)a|b)){0}(?256)" ) } ) {
			var limited = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { MatchWorkLimit = 300 } );
			Assert.True( limited.IsSuccess );
			Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, limited.Pattern!.Match( "a".AsMemory() ).Diagnostic?.Code );
			using var cancellation = new CancellationTokenSource();
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				WorkObserver = work => { if ( work == 300 ) { cancellation.Cancel(); } }
			} );
			Assert.True( compiled.IsSuccess );
			Assert.Throws<OperationCanceledException>( () => compiled.Pattern!.Match( "a".AsMemory(), cancellationToken: cancellation.Token ) );
			var result = compiled.Pattern!.Match( "a".AsMemory() ); Assert.True( result.IsSuccess );
			Assert.Equal( PcreNative16Oracle.ExecuteSequence( Words( source ), Words( "a" ), false, 0 ).Captures, Spans( result.Match ) );
			Assert.Throws<OperationCanceledException>( () => PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), cancellationToken: cancellation.Token ) );
		}
	}

	[Theory]
	[InlineData( "(?(1)a??|b)" )]
	[InlineData( "(?(R)b|a??)" )]
	[InlineData( "(?(VERSION=10.48)a??|b)" )]
	[InlineData( "(?(?=)a??|b)" )]
	public void SelectingAnArmAddsNoSiblingAlternativeToTheFrameBudget( string condition ) {
		var source = String.Concat( "()(?:(?:", condition, "){32}){32}" );
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory() );
		Assert.True( compiled.IsSuccess );
		Assert.Equal( "0:0,0:0", Spans( compiled.Pattern!.Match( ReadOnlyMemory<char>.Empty ).Match ) );
		var overflow = PcreWordSequenceCompiler.CompileSequence( String.Concat( source, condition ).AsMemory() );
		Assert.True( overflow.IsSuccess ); var result = overflow.Pattern!.Match( ReadOnlyMemory<char>.Empty );
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code ); Assert.Null( result.Match );
		var pruned = PcreWordSequenceCompiler.CompileSequence( String.Concat( "()(?:(?>", condition, "){64}){64}" ).AsMemory() );
		Assert.True( pruned.IsSuccess ); Assert.Equal( "0:0,0:0", Spans( pruned.Pattern!.Match( ReadOnlyMemory<char>.Empty ).Match ) );
	}

	[Fact]
	public void InactiveArmsAndTestsRemainInSourceAdmissionAndUtfValidation() {
		var atDepth = String.Concat( String.Concat( Enumerable.Repeat( "(?(VERSION=10.48)", 64 ) ), "a", new string( ')', 64 ) );
		foreach ( var source in new[] { atDepth, "(?(R)a?b?c?d?e?|f?g?h?i?j?)", "(?(DEFINE)(a)){64}" } ) {
			Assert.True( PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).IsSuccess, source );
		}
		foreach ( var source in new[] { String.Concat( "(?(VERSION=10.48)", atDepth, ")" ),
			"(?(R)a?b?c?d?e?f?|x)", "(?(R)a?b?c?d?e?|f?g?h?i?j?)k?",
			"(?(?=a|b)a?b?c?d?e?|x)", "(?(VERSION=10.48)a|(*ACCEPT))",
			"(?(DEFINE)(a)){65}", "(?(?=a)b|c){65}", "(*ANY)(?(R)a|b)" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax, PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
		foreach ( var source in new[] { "(?(VERSION=10.48)a|(?<=ab)b)", "(?(VERSION=0)(?<=ab)b|a)", "(?(?<=ab)b|a)" } ) {
			const string subject = "\uD800a";
			Assert.Equal( "X", PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 1 ).Kind );
			var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Pattern!.Match( subject.AsMemory(), 1 );
			Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 0 ), result.Diagnostic );
		}
	}

	[Theory]
	[InlineData( "(?(0)a|b)", 0 )]
	[InlineData( "(?(2)a|b)", 0 )]
	[InlineData( "(?(R2)a|b)", 0 )]
	[InlineData( "(?(R&missing)a|b)", 0 )]
	[InlineData( "(?(VERSION<=10.48)a|b)", 0 )]
	[InlineData( "(?(VERSION=10.48)a|b|c)", 0 )]
	[InlineData( "(?(DEFINE)a|b)", 0 )]
	[InlineData( "(?(?*a)a|b)", 2 )]
	[InlineData( "(?(?<*a)a|b)", 2 )]
	public void InvalidConditionsRetainOriginalCodeUnitOffsets( string condition, int innerOffset ) {
		const string prefix = "é😀()";
		var source = String.Concat( prefix, condition );
		Assert.Equal( "E", PcreNative16Oracle.ExecuteSequence( Words( source ), Array.Empty<ushort>(), true, 0 ).Kind );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidSyntax, prefix.Length + innerOffset ),
			PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Diagnostic );
	}

	[Fact]
	public void CompiledConditionsAndResultsRetainOwnedNamesAndSpans() {
		const string text = "(?(?=(?<x>a|ab)))(?(x)\\k<x>c|X)";
		var source = text.ToCharArray(); var pattern = PcreWordSequenceCompiler.CompileSequence( source ).Pattern!;
		Array.Fill( source, 'z' );
		var first = pattern.Match( "!zac!".AsMemory( 1, 3 ) ).Match!;
		Assert.Equal( "1:2,1:1", Spans( first ) );
		foreach ( var subject in new[] { "ac", "abc", "", "X", "ac" } ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( text ), Words( subject ), false, 0 );
			var result = pattern.Match( subject.AsMemory() );
			Assert.True( result.IsSuccess ); Assert.Equal( native.Captures, Spans( result.Match ) );
		}
		Assert.Equal( new PcreWordCapture( 1, 1 ), first.GetCapture( "x" ) ); Assert.Equal( "1:2,1:1", Spans( first ) );
	}

	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( c => c is { } span ? $"{span.CodeUnitIndex}:{span.CodeUnitLength}" : "-" ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
