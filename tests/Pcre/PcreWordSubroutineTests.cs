using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordSubroutineTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new (string Source, string Subject, int Start, bool Utf, bool Ucp)[] {
				("^(a|ab)(?1)c$", "aabc", 0, true, false),
				("^(a)(?01)\\1$", "aaa", 0, true, false),
				("^(?+1)(a)$", "aa", 0, true, false),
				("^((?+1)(b))$", "bb", 0, true, false),
				("^(a)(b)(?-2)$", "aba", 0, true, false),
				("^(a)(?+01)(b)$", "abb", 0, true, false),
				("^(?<a>a)(?P>a)$", "aa", 0, true, false),
				("^\\g<+1>(a)$", "aa", 0, true, false),
				("^(a)\\g<01>$", "aa", 0, true, false),
				("^(a)(b)\\g<-2>$", "aba", 0, true, false),
				("^(?<a>a)\\g<a>$", "aa", 0, true, false),
				("^\\g'a'(?'a'x)$", "xx", 0, true, false),
				("^(a)\\g'1'$", "aa", 0, true, false),
				("^(a)\\g'+1'(b)$", "abb", 0, true, false),
				("^(a)\\g'-1'$", "aa", 0, true, false),
				("^(?|(a)|(b))(?1)$", "ba", 0, true, false),
				("^(?|(a)|(b))(?-1)$", "ba", 0, true, false),
				("^(?J)(?:(?<x>a)|(?<x>b))(?&x)$", "ba", 0, true, false),
				("^(?J)(?|(?<x>a)|(?<x>b))(?&x)$", "ba", 0, true, false),
				("^(abc)(?i:(?-1))$", "abcABC", 0, true, false),
				("^(?i:(abc))(?-1)$", "ABCABC", 0, true, false),
				("^(?(DEFINE)(?i:(?<x>a)))(?-i:(?&x))$", "A", 0, true, false),
				("^(?(DEFINE)(?<x>(?i)a))(?&x)b$", "Ab", 0, true, false),
				("^(?(DEFINE)(?<x>(?i)a))(?&x)b$", "AB", 0, true, false),
				("^(?(DEFINE)(?<x>(?<y>a)\\k<y>))(?&x)$", "aa", 0, true, false),
				("^(?<b>x)(?(DEFINE)(?<a>\\k<b>))(?&a)$", "xx", 0, true, false),
				("^(?<x>(?<y>a|b)\\k<y>)(?&x)\\k<y>$", "aabba", 0, true, false),
				("^(?(DEFINE)(?<x>a|ab))(?&x)c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<x>(a|ab)\\2))(?&x)c$", "ababc", 0, true, false),
				("^(?(DEFINE)(?<x>a|ab))(?>(?&x))c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<x>a|ab))(?&x)++c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<x>a?))(?&x){2}b$", "aab", 0, true, false),
				("^(?(DEFINE)(?<x>a?))(?&x)*b$", "aab", 0, true, false),
				("^(?(DEFINE)(?<x>(?&y)+)(?<y>a?))(?&x)b$", "aab", 0, true, false),
				("^(?(DEFINE)(?<x>(?&y))(?<y>(?&z))(?<z>a))(?&x)$", "a", 0, true, false),
				("^(?(DEFINE)(?<x>))(?&x)*$", "", 0, true, false),
				("^(?<x>a){0}(?&x)$", "a", 0, true, false),
				("^(?(DEFINE)(?<x>a))(?<=(?=(?&x)))a$", "a", 0, true, false),
				("^(?(DEFINE)(?<x>a|ab))(?*((?&x)))\\2c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<x>a|ab))(?=((?&x)))\\2c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<x>(?*(a|ab))\\2))(?&x)c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<x>a|ab))(?:(?>(?&x))d|abc)$", "abc", 0, true, false),
				("^(?(DEFINE)(?<x>a|ab))(?:(?*((?&x)))\\2d|abc)$", "abc", 0, true, false),
				("(?(DEFINE)(?R))", "", 0, true, false),
				("(?(DEFINE)(?<a>(?&a)))a", "a", 0, true, false),
				("(?(DEFINE)(?<a>(?&b))(?<b>(?&a)))a", "a", 0, true, false),
				("(?R){0}a", "a", 0, true, false),
				("\\A(?<a>a(?1)?b)\\z", "aaabbb", 0, true, false),
				("\\A(?<a>a(?01)?b)\\z", "aabb", 0, true, false),
				("\\A(?<a>a(?-1)?b)\\z", "aabb", 0, true, false),
				("\\A(?<a>a(?&a)?b)\\z", "aabb", 0, true, false),
				("\\A(?<a>a(?P>a)?b)\\z", "aabb", 0, true, false),
				("\\A(?<a>a\\g<1>?b)\\z", "aabb", 0, true, false),
				("\\A(?<a>a\\g<a>?b)\\z", "aabb", 0, true, false),
				("\\A(?<a>a\\g'1'?b)\\z", "aabb", 0, true, false),
				("\\A(?<a>a\\g'a'?b)\\z", "aabb", 0, true, false),
				("\\A(?<a>a\\g<-1>?b)\\z", "aabb", 0, true, false),
				("\\A(?<a>a\\g'-1'?b)\\z", "aabb", 0, true, false),
				("\\A(?&a)\\z(?(DEFINE)(?<a>a(?+1)?b)(?<b>c(?-2)?d))", "acabbdb", 0, true, false),
				("\\A(?+1)\\z(?(DEFINE)(?<a>a(?+1)?b)(?<b>c(?-2)?d))", "acabdb", 0, true, false),
				("\\A\\g<+1>\\z(?(DEFINE)(?<a>a\\g<+1>?b)(?<b>c\\g<-2>?d))", "acabdb", 0, true, false),
				("\\A\\g'+1'\\z(?(DEFINE)(?<a>a\\g'+1'?b)(?<b>c\\g'-2'?d))", "acabdb", 0, true, false),
				("\\A(?&a)\\z(?(DEFINE)(?<a>a(?&b)?b)(?<b>c(?&a)?d))", "acabdb", 0, true, false),
				("(?J)\\A(?&a)\\z(?(DEFINE)(?<a>a(?&a)?b)(?<a>z))", "aabb", 0, true, false),
				("(?J)\\A(?&a)\\z(?(DEFINE)(?<a>a(?&a)?b)(?<a>z))", "z", 0, true, false),
				("\\A(?|(?<a>a(?1)?b)|(?<a>z))(?1)\\z", "zaabb", 0, true, false),
				("\\A(?|(?<a>a(?&a)?b)|(?<a>z))(?&a)\\z", "zz", 0, true, false),
				("\\A(?&字٢)\\z(?(DEFINE)(?<字٢>é(?&字٢)?x))", "ééxx", 0, true, false),
				("\\A\\g<R>\\z(?(DEFINE)(?<R>a\\g<R>?b))", "aabb", 0, true, false),
				("\\((?:[^()]++|(?R))*\\)", "(ab(cd)ef)", 0, true, false),
				("\\((?:[^()]++|(?0))*\\)", "(ab(cd)ef)", 0, true, false),
				("\\((?:[^()]++|(?000))*\\)", "(ab(cd)ef)", 0, true, false),
				("\\((?:[^()]++|\\g<0>)*\\)", "(ab(cd)ef)", 0, true, false),
				("\\((?:[^()]++|\\g'0')*\\)", "(ab(cd)ef)", 0, true, false),
				("\\((?:[^()]++|\\g'000')*\\)", "(ab(cd)ef)", 0, true, false),
				("\\((?R)?\\)", "((()))", 0, true, false),
				("\\((?R)*\\)", "(()())", 0, true, false),
				("^a(?R)?b$", "aabb", 0, true, false),
				("<(?:(?<x>[a-z])|(?R))*>", "<a<b>>", 0, true, false),
				("<(?:(?<x>[a-z])|(?R(<x>)))*>", "<a<b>>", 0, true, false),
				("<(?:(?<x>[a-z])|(?0('x')))*>", "<a<b>>", 0, true, false),
				("<(?:(?<x>[a-z])|(?000(1,1)))*>", "<a<b>>", 0, true, false),
				("^((.)(?1)\\2|.?)$", "abba", 0, true, false),
				("^((.)(?1)\\2|.?)$", "abcba", 0, true, false),
				("^((.)(?1)\\2|.?)$", "abca", 0, true, false),
				("\\A(.)(\\1|a(?2))\\z", "bab", 0, true, false),
				("\\A(?<a>a(?:(?<x>b)|(?&a))c)\\z", "aabcc", 0, true, false),
				("\\A(?<a>a(?:(?<x>b)|(?&a(<x>)))c)\\z", "aabcc", 0, true, false),
				("\\A(?<a>a(?:(?<x>b)|(?&a(<x>))|d)c)\\z", "aadcc", 0, true, false),
				("\\A(?&a(<x>))\\z(?(DEFINE)(?<a>a(?:(?<x>b)|(?&a(<x>)))c))", "aaabccc", 0, true, false),
				("\\A(?&a(<x>))\\z(?(DEFINE)(?<a>a(?:(?<x>b)|(?&a))c))", "aabcc", 0, true, false),
				("\\A(?&a)\\z(?(DEFINE)(?i:(?<a>a(?&a)?b)))", "aABb", 0, true, false),
				("\\A(?i:(?&a))\\z(?(DEFINE)(?<a>a(?&a)?b))", "aABb", 0, true, false),
				("\\A(?&a)\\z(?(DEFINE)(?<a>a(?&a)?b|ab))", "aabb", 0, true, false),
				("\\A(?=(?<a>a(?&a)?b))aabb\\z", "aabb", 0, true, false),
				("\\A(?!(?<a>a(?&a)?b)c)aabbd\\z", "aabbd", 0, true, false),
				("\\A(?>(?<a>a(?&a)?b))c\\z", "aabbc", 0, true, false),
				("\\A(?*(?<a>a(?&a)?b|aabbc))\\1d\\z", "aabbcd", 0, true, false),
				("\\A(?=(?<a>a(?&a)?b|aabbc))\\1d\\z", "aabbcd", 0, true, false),
				("\\A(?>(?<a>a(?&a)?b|aabbc))d\\z", "aabbcd", 0, true, false),
				("\\A(?&a)\\z(?(DEFINE)(?<a>a(?<=a)(?&a)?b))", "aabb", 0, true, false),
				("\\Aaabb(?<=(?=(?&a))aabb)\\z(?(DEFINE)(?<a>a(?&a)?b))", "aabb", 0, true, false),
				("(?<=(?=(?<a>a(?&a)?b))a)b", "ab", 0, true, false),
				("(?<=(?(DEFINE)(?<a>(?R)))a)x", "ax", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>a)(?<y>b)))(?&g(<x>))c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>a)(?<y>b)))(?1(2,3))c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>a)))(?P>g(1,2))$", "a", 0, true, false),
				("^(?<g>(?<x>a|b))(?&g(1,2))$", "ab", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>)(?<y>a)?))(?&g('x','y'))$", "", 0, true, false),
				("^(?<g>(?<x>a)?b)(?&g(<x>))$", "abb", 0, true, false),
				("^(?<g>(?<x>a|b)(?<y>a|b))(?&g(2))$", "aabb", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>a|b)+))(?&g(2))$", "ab", 0, true, false),
				("^(?(DEFINE)(?<g>(a)))(?01(002,2,002))$", "a", 0, true, false),
				("^(?(DEFINE)(?<g>(a)))(?-2(-1))$", "a", 0, true, false),
				("^(?+1(+2))(?<g>(a))$", "aa", 0, true, false),
				("^(?+1(+2))\\2(?<g>(a)){0}$", "aa", 0, true, false),
				("^(?&g(<x>))(?<g>(?<x>a))$", "aa", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>a)))(?&g(+1))(?<later>b)$", "ab", 0, true, false),
				("(?J)^(?(DEFINE)(?<g>(?:(?<x>a)|(?<x>b))))(?&g(<x>))$", "b", 0, true, false),
				("^(?(DEFINE)(?<g>(?|(?<x>a)|(?<x>b))))(?&g(<x>,2))$", "b", 0, true, false),
				("^(?|(?<g>(?<x>a))b|(?&g(+2))c)$", "ac", 0, true, false),
				("^(?(DEFINE)(?<字٢>(?<値>é)))(?&字٢(<値>))$", "é", 0, true, false),
				("^(?(DEFINE)(?<a>(?&b(<x>)))(?<b>(?<x>c)))(?&a(<x>))$", "c", 0, true, false),
				("^(?(DEFINE)(?<a>(?&b(<x>)))(?<b>(?<x>c)))(?&a)$", "c", 0, true, false),
				("^(?(DEFINE)(?<a>(?&b))(?<b>(?<x>c)))(?&a(<x>))$", "c", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>a)|(?<y>ab)))(?&g(2,3))c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>a)|(?<y>ab)))(?>(?&g(2,3)))b$", "ab", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>a)|(?<y>ab)))(?>(?&g(2,3)))c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>a)))(?=(?&g(2)))\\2$", "a", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>a|ab)))(?*(?&g(2)))\\2c$", "abc", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>a|ab)))(?=(?&g(2)))\\2c$", "abc", 0, true, false),
				("^a(?(DEFINE)(?<g>(?<=(?<x>a))))(?&g(2))b$", "ab", 0, true, false),
				("^c(?(DEFINE)(?<g>(?<!(?<x>a))))(?&g(2))b$", "cb", 0, true, false),
				("^(?<outer>a(?&g(1,3))b)(?(DEFINE)(?<g>(?<x>c)))$", "acb", 0, true, false),
				("^(?|(a(b))|(((?1(2)))c))$", "abc", 0, true, false),
				("^(?(DEFINE)(?<g>(?<x>a?)))(?&g(2))*b$", "aab", 0, true, false),
				("^ab(?<=(?=(?&g(<x>)))ab)$(?(DEFINE)(?<g>(?<x>ab)))", "ab", 0, true, false),
				("^ab(?<=(?!(?&g(<x>))c)ab)$(?(DEFINE)(?<g>(?<x>ab)))", "ab", 0, true, false),
				("^ab(?<=(?(DEFINE)(?<q>(?&g(<x>))))ab)$(?(DEFINE)(?<g>(?<x>ab)))", "ab", 0, true, false),
				("^ab(?<=(?&outer))$(?(DEFINE)(?<outer>(?=(?&g(<x>)))ab)(?<g>(?<x>ab)))", "ab", 0, true, false),
				("^ab(?<=(?&outer))$(?(DEFINE)(?<outer>(?(DEFINE)(?<q>(?&g(<x>))))ab)(?<g>(?<x>ab)))", "ab", 0, true, false),
				("(?<x>a)(?<=(?1))b", "ab", 0, false, false),
				("(?<x>a)(?<=(?01))b", "ab", 0, false, false),
				("(?<x>a)(?<=(?-1))b", "ab", 0, false, false),
				("(?<x>a)(?<=\\g<1>)b", "ab", 0, false, false),
				("(?<x>a)(?<=\\g'1')b", "ab", 0, false, false),
				("(?<x>a)(?<=\\g<-1>)b", "ab", 0, false, false),
				("(?<x>a)(?<=\\g'-1')b", "ab", 0, false, false),
				("(?<x>a)(?<=(?&x))b", "ab", 0, false, false),
				("(?<x>a)(?<=(?P>x))b", "ab", 0, false, false),
				("(?<x>a)(?<=\\g<x>)b", "ab", 0, false, false),
				("(?<x>a)(?<=\\g'x')b", "ab", 0, false, false),
				("(?<=(?+1))(?<x>a)b", "aab", 0, false, false),
				("(?<=(?+01))(?<x>a)b", "aab", 0, false, false),
				("(?<=\\g<+1>)(?<x>a)b", "aab", 0, false, false),
				("(?<=\\g'+1')(?<x>a)b", "aab", 0, false, false),
				("(?<=(?&x))(?<x>a)b", "aab", 0, false, false),
				("(?<=(?P>x))(?<x>a)b", "aab", 0, false, false),
				("(?<=\\g<x>)(?<x>a)b", "aab", 0, false, false),
				("(?<=\\g'x')(?<x>a)b", "aab", 0, false, false),
				("(?<x>a)?(?<=(?&x))b", "aaab", 0, false, false),
				("(?<x>a)*(?<=(?&x))b", "aaab", 0, false, false),
				("(?<x>a)+(?<=(?&x))b", "aaab", 0, false, false),
				("(?<x>a){0}(?<=(?&x))b", "aaab", 0, false, false),
				("(?<x>a){1,3}(?<=(?&x))b", "aaab", 0, false, false),
				("(?(DEFINE)(?<x>(?<y>a)\\k<y>))(?<=(?&x))b", "aab", 0, false, false),
				("(?<y>a)(?(DEFINE)(?<x>\\k<y>))(?<=(?&x))b", "ab", 0, false, false),
				("(?<x>(?&y)){0}(?(DEFINE)(?<y>a))(?<=\\k<x>{0})b", "b", 0, false, false),
				("(?(DEFINE)(?<x>(?&y))(?<y>(?&z))(?<z>ab))(?<=(?&x))c", "abc", 0, false, false),
				("(?(DEFINE)(?<x>a+))(?<=(?=(?&x)))a", "a", 0, false, false),
				("(?(DEFINE)(?<x>a(?<=a)))(?<=(?&x))b", "ab", 0, false, false),
				("(?(DEFINE)(?<x>a(?=\\k<x>)))(?<=(?&x))b", "ab", 0, false, false),
				("(?<x>(?<y>a|b)\\k<y>)(?<=(?&x))c", "aac", 0, false, false),
				("(?(DEFINE)(?<x>a|ab))(?<=(?&x))c", "abc", 0, false, false),
				("(?(DEFINE)(?<x>a|ab))(?<!(?&x))c", "xc", 0, false, false),
				("(?(DEFINE)(?<x>a|ab))(?<!(?&x))c", "abc", 0, false, false),
				("(?(DEFINE)(?<x>(?>a|ab)))(?<=(?&x))c", "abc", 0, false, false),
				("(?(DEFINE)(?<x>a|ab))(?<*(?&x))b?c", "abc", 0, false, false),
				("(?(DEFINE)(?<x>a(?=b)))(?<=(?&x))b", "ab", 0, false, false),
				("(?(DEFINE)(?<x>a{1,2}(?=b)))(?<=(?&x))b", "ab", 0, false, false),
				("(?(DEFINE)(?<x>a{1,2}(?!b)))(?<=(?&x))b", "ab", 0, false, false),
				("(?(DEFINE)(?<x>a{1,2}(?=(b?))))(?<=(?&x))b", "ab", 0, false, false),
				("(?(DEFINE)(?<x>\\Ga))(?<=(?&x))b", "ab", 0, false, false),
				("(?(DEFINE)(?<x>\\Ga))(?<=(?&x))b", "ab", 1, false, false),
				("(?(DEFINE)(?<x>a\\G))(?<=(?&x))b", "ab", 1, false, false),
				("(?(DEFINE)(?<x>a\\G))(?<=(?&x))b", "ab", 0, false, false),
				("(?(DEFINE)(?<x>é))(?<=(?&x))b", "éb", 0, true, false),
				("(?(DEFINE)(?<x>[é😀]{1,2}))(?<=(?&x))b", "é😀b", 0, true, false),
				("(?(DEFINE)(?<x>😀))(?<=(?&x){2})b", "😀😀b", 0, true, false),
				("^(?<x>(?<y>a|b))b(?<=(?&x))c\\k<y>$", "abca", 0, false, false),
				("(a)(?1)", "aa", 0, true, false),
				("(a)(?1)", "aa", 0, false, false),
				("(a)(?>(?1))", "aa", 0, true, false),
				("(a)(?<=a)(?1)", "aa", 0, true, false),
				("(?<x>a|ab)(?&x)c", "aabc", 0, true, false),
				("(?<x>(a|b)\\2)(?&x)\\2", "aabba", 0, true, false),
				("(?(DEFINE)(?<x>(a|ab)))(?&x(2))\\2c", "ababc", 0, true, false),
				("(?(DEFINE)(?<x>(a|ab)))(?&x(2))b|abc", "abc", 0, true, false),
				("(?(DEFINE)(?<x>(a|ab)))(?*(?&x(2)))\\2c", "abc", 0, true, false),
				("(?(DEFINE)(?<x>(a|ab)))(?>(?&x(2)))c", "abc", 0, true, false),
				("(?(DEFINE)(?<x>(a|ab)))(?&x(2)){2}c", "aabc", 0, true, false),
				("(?(DEFINE)(?<x>(a|ab)))(?&x(2)){2}+c", "aabc", 0, true, false),
				("(?(DEFINE)(?<x>(a?)))(?&x(2)){1,}b", "aab", 0, true, false),
				("(?(DEFINE)(?<x>(a?)))(?&x(2)){2,}b", "b", 0, true, false),
				("(?(DEFINE)(?<x>(?=(\\2a|a))))(?&x(2)){1,}\\2", "aaaa", 0, true, false),
				("(?(DEFINE)(?<x>(?=(\\2a|a))))(?&x(2)){2,}\\2", "aaaa", 0, true, false),
				("(?(DEFINE)(?<x>(?=(\\2a|a))))(?&x(2)){3,}+\\2", "aaaa", 0, true, false),
				("(?(DEFINE)(?<x>(?=(\\2a|a))))(?&x(2)){1,3}\\2", "aaaa", 0, true, false),
				("(?(DEFINE)(?<x>))(?&x)*", "", 0, true, false),
				("(?(DEFINE)(?<x>))(?&x)+", "", 0, true, false),
				("(?(DEFINE)(?<x>))(?&x){2,}", "", 0, true, false),
				("(?(DEFINE)(?<x>))(?&x){2,}+", "", 0, true, false),
				("(?(DEFINE)(?<x>))(?&x){0}a", "a", 0, true, false),
				("(?(DEFINE)(?<x>é😀))(?&x)", "zé😀", 1, true, false),
				("(?<字٢>é)(?&字٢)", "éé", 0, true, false),
				("(?<𐐀>😀)(?&𐐀)", "😀😀", 0, true, false),
				("(?(DEFINE)(?i:(?<x>é𐐀)))(?&x)", "É𐐨", 0, true, false),
				("(?(DEFINE)(?<x>\\p{Greek}+))(?&x)", "Σς", 0, true, true),
				("(?(DEFINE)(?<x>\\w+))(?&x)", "éΣ", 0, true, true),
				("(?(DEFINE)(?<x>\\R))(?&x)++x", "\r\n\rx", 0, true, false),
				("(*CR)(?m)(?(DEFINE)(?<x>^a+$))(?&x)", "z\raaa\rx", 0, true, false),
				("(*NUL)(?m)(?(DEFINE)(?<x>^a+$))(?&x)", "z\u0000aaa\u0000x", 0, true, false),
				("(?(DEFINE)(?<x>\\G.a))(?&x)", "zba", 1, true, false),
				("(?(DEFINE)(?<x>(\\b)))a(?&x(2))", " a", 1, true, false),
				("(?(DEFINE)(?<x>(\\b)))a(?&x(2))", " a", 1, false, false),
				("(?(DEFINE)(?<x>😀{1,2}))(?<=(?&x))b", "😀😀b", 0, true, false),
				("(?(DEFINE)(?<x>😀{1,2}))(?<*(?&x))😀?b", "😀😀b", 0, true, false),
				("(?(DEFINE)(?<x>a{1,2}(?=b)))(?<=(?&x))b", "aab", 2, true, false),
				("(?(DEFINE)(?<x>a{1,2}(?=(b?))))(?<*(?&x))b", "aab", 2, true, false),
				("(?<x>é(?&x)?😀)", "éé😀😀", 0, true, false),
				("(?<x>é(?&x)?😀)", "éé😀😀", 0, false, false),
				("<(?:(?<x>😀)|(?R(<x>)))*>", "<😀<😀>>", 0, true, false),
				("(?(DEFINE)(?<x>(a)))(?&x(1,2))", "a", 0, true, false),
				("(?(DEFINE)(?<x>a)(?<unused>(?&unused)))a", "a", 0, true, false),
				("(?(DEFINE)(?<x>\\x{0}+))(?&x)", "\u0000\u0000", 0, false, false),
				("(?<x>\ud800)(?&x)", "\ud800\ud800", 0, false, false),
				("(?(DEFINE)(?<x>\udc00))(?&x)", "\udc00", 0, false, false),
				("(?(DEFINE)(?<x>(?=(\\2a|a))))(?&x(2)){1,}?\\2", "aaaa", 0, true, false),
				("(?(DEFINE)(?<x>(?=(\\2a|a))))(?&x(2)){1,}+\\2", "aaaa", 0, true, false),
				("(?(DEFINE)(?<x>(?=(\\2a|a))))(?&x(2)){2,}?\\2", "aaaa", 0, true, false),
				("(?(DEFINE)(?<x>(?=(\\2a|a))))(?&x(2)){2,}+\\2", "aaaa", 0, true, false),
				("(?(DEFINE)(?<x>(?=(\\2a|a))))(?&x(2)){3,}\\2", "aaaa", 0, true, false),
				("(?(DEFINE)(?<x>(?=(\\2a|a))))(?&x(2)){3,}?\\2", "aaaa", 0, true, false),
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
	public void CallsAndRecursionAgreeWithEveryNativePolicy( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"call {id}: {compiled.Diagnostic}" );
		var nativePolicies = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start, defaults, ucp );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = nativePolicies[mask];
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			Assert.True( native.Captures == Spans( result.Match ),
				$"call {id}/{defaults}/{mask}: expected {native.Captures}, actual {Spans( result.Match )}" );
			Assert.Equal( native.Kind == "M", result.Match is not null );
		}
	}

	[Theory]
	[InlineData( "(a)(?1)", "aa", "0:2,0:1" )]
	[InlineData( "(?(DEFINE)(?<g>(a)))(?&g)", "a", "0:1,-,-" )]
	[InlineData( "(?(DEFINE)(?<g>(a)))(?&g(1,2))", "a", "0:1,-,0:1" )]
	[InlineData( "(?<g>(a|b))(?&g(1,2))", "ab", "0:2,0:1,1:1" )]
	[InlineData( "(?(DEFINE)(?<g>(a|ab)))(?&g(2))c", "abc", "0:3,-,0:2" )]
	[InlineData( "(?(DEFINE)(?<g>(a|ab)))(?>(?&g(2)))c", "abc", "" )]
	[InlineData( "(?(DEFINE)(?<g>(a|ab)))(?&g(2))c|abd", "abd", "0:3,-,-" )]
	[InlineData( @"(?(DEFINE)(?<g>(a|ab)))(?*(?&g(2)))\2c", "abc", "0:3,-,0:2" )]
	[InlineData( @"\A(?<g>a(?&g)?b)\z", "aabb", "0:4,0:4" )]
	[InlineData( @"<(?:(?<x>[a-z])|(?R))*>", "<a<b>>", "0:6,1:1" )]
	[InlineData( @"<(?:(?<x>[a-z])|(?R(<x>)))*>", "<a<b>>", "0:6,3:1" )]
	[InlineData( @"(?(DEFINE)(?<g>(?=(\2a|a))))(?&g(2)){2,}\2", "aaaa", "0:3,-,0:3" )]
	public void CaptureRestorationSelectionsAndCalleeRetryHaveExplicitOutcomes( string source, string subject, string expected ) {
		Assert.Equal( expected, PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 0 ).Captures );
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.True( compiled.IsSuccess );
		var result = compiled.Pattern!.Match( subject.AsMemory() );
		Assert.True( result.IsSuccess ); Assert.Equal( expected, Spans( result.Match ) );
	}

	[Theory]
	[InlineData( "(a|ab)(?1)c", "aabc" )]
	[InlineData( "(?(DEFINE)(?<g>(a|ab)))(?&g(2))c", "abc" )]
	[InlineData( "(?<g>(a|b))(?&g(1,2))", "ab" )]
	[InlineData( @"(?(DEFINE)(?<g>(?=(\2a|a))))(?&g(2)){1,}\2", "aaaa" )]
	[InlineData( @"(?(DEFINE)(?<g>(?=(\2a|a))))(?&g(2)){1,}?\2", "aaaa" )]
	[InlineData( @"(?(DEFINE)(?<g>(?=(\2a|a))))(?&g(2)){1,}+\2", "aaaa" )]
	[InlineData( @"(?(DEFINE)(?<g>(?=(\2a|a))))(?&g(2)){2,}\2", "aaaa" )]
	[InlineData( @"(?(DEFINE)(?<g>(?=(\2a|a))))(?&g(2)){2,}?\2", "aaaa" )]
	[InlineData( @"(?(DEFINE)(?<g>(?=(\2a|a))))(?&g(2)){2,}+\2", "aaaa" )]
	[InlineData( @"(?(DEFINE)(?<g>(?=(\2a|a))))(?&g(2)){3,}+\2", "aaaa" )]
	[InlineData( @"\A(?<g>a(?&g)?b)\z", "aaabbb" )]
	[InlineData( @"<(?:(?<x>[a-z])|(?R(<x>)))*>", "<a<b>>" )]
	[InlineData( "(?(DEFINE)(?<g>(?<x>é😀)))(?&g(<x>))", "é😀" )]
	[InlineData( "(?i)(?<g>𐐀)(?&g)", "𐐨𐐀" )]
	[InlineData( "(?(DEFINE)(?<g>é{1,2}))(?<*(?&g))é?x", "ééx" )]
	[InlineData( "(?<g>é(?&g)?😀)", "éé😀😀" )]
	public void CallsKeepCrossWidthCaptureCoordinatesAndValues( string source, string subject ) {
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
	[InlineData( "(?(DEFINE)(?<g>a))(?&g)", "a", 8 )]
	[InlineData( "(?(DEFINE)(?<g>(a)))(?&g)", "a", 11 )]
	[InlineData( "(?(DEFINE)(?<g>(a)))(?&g(2))", "a", 11 )]
	[InlineData( "(?(DEFINE)(?<g>a|ab))(?&g)c", "abc", 16 )]
	[InlineData( "(?(DEFINE)(?<g>ab|a))(?&g)c", "ac", 13 )]
	[InlineData( "(?(DEFINE)(?<g>(a|ab)))(?&g)c", "abc", 23 )]
	[InlineData( "(?(DEFINE)(?<g>a|ab))(?>(?&g))", "a", 15 )]
	public void EntryReturnRestorationAndRetryConsumeExactWork( string source, string subject, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add, Policy = new() { Anchored = true }
			} );
			Assert.True( compiled.IsSuccess );
			var result = compiled.Pattern!.Match( subject.AsMemory() );
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
	[InlineData( "(?(DEFINE)(?<g>(a)))(?&g)", "a", 10, "0:1,-,-" )]
	[InlineData( "(?(DEFINE)(?<g>(a|ab)))(?&g)c", "abc", 13, "0:3,-,-" )]
	[InlineData( "(?(DEFINE)(?<g>a|ab))(?>(?&g))", "a", 13, "0:1,-" )]
	public void CancellationDuringReturnUndoAndPruningLeavesPatternsReusable( string source, string subject, int cancelAt, string expected ) {
		using var cancelled = new CancellationTokenSource();
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			WorkObserver = work => { if ( work == cancelAt ) { cancelled.Cancel(); } }
		} );
		Assert.True( compiled.IsSuccess );
		Assert.Throws<OperationCanceledException>( () => compiled.Pattern!.Match( subject.AsMemory(), cancellationToken: cancelled.Token ) );
		Assert.Equal( expected, Spans( compiled.Pattern!.Match( subject.AsMemory() ).Match ) );
		Assert.Throws<OperationCanceledException>( () => PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), cancellationToken: cancelled.Token ) );
	}

	[Fact]
	public void ReturnedCallsAndRecursionShareTheSavedFrameCeiling() {
		const string prefix = "(?(DEFINE)(?<g>))";
		var atLimit = PcreWordSequenceCompiler.CompileSequence( String.Concat( prefix, "(?:(?&g){32}){32}" ).AsMemory() ).Pattern!;
		Assert.Equal( "0:0,-", Spans( atLimit.Match( ReadOnlyMemory<char>.Empty ).Match ) );
		var tooMany = PcreWordSequenceCompiler.CompileSequence( String.Concat( prefix, "(?:(?&g){32}){32}(?&g)" ).AsMemory() ).Pattern!;
		var result = tooMany.Match( ReadOnlyMemory<char>.Empty );
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code ); Assert.Null( result.Match );
		var pruned = PcreWordSequenceCompiler.CompileSequence( String.Concat( prefix, "(?:(?>(?&g)){64}){64}" ).AsMemory() ).Pattern!;
		Assert.Equal( "0:0,-", Spans( pruned.Match( ReadOnlyMemory<char>.Empty ).Match ) );
		var recursive = PcreWordSequenceCompiler.CompileSequence( "a(?R)?b".AsMemory() ).Pattern!;
		Assert.Equal( "0:128", Spans( recursive.Match( String.Concat( new string( 'a', 64 ), new string( 'b', 64 ) ).AsMemory() ).Match ) );
		result = recursive.Match( String.Concat( new string( 'a', 1024 ), new string( 'b', 1024 ) ).AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code ); Assert.Null( result.Match );
		Assert.Equal( "0:2", Spans( recursive.Match( "ab".AsMemory() ).Match ) );
		var loop = PcreWordSequenceCompiler.CompileSequence( "(?R)".AsMemory(), new() { MatchWorkLimit = 40 } ).Pattern!;
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, loop.Match( ReadOnlyMemory<char>.Empty ).Diagnostic?.Code );
	}

	[Fact]
	public void NestedReturnsDoNotDuplicateCaptureHistoryExponentially() {
		var source = new StringBuilder( "(?(DEFINE)" );
		for ( var depth = 0; depth < 20; depth++ ) {
			source.Append( "(?<g" ).Append( depth ).Append( ">()" );
			if ( depth == 19 ) { source.Append( 'a' ); }
			else { source.Append( "(?&g" ).Append( depth + 1 ).Append( ')' ); }
			source.Append( ')' );
		}
		source.Append( ")(?&g0)" );
		var text = source.ToString();
		var compiled = PcreWordSequenceCompiler.CompileSequence( text.AsMemory(), new() { MatchWorkLimit = 10_000 } );
		Assert.True( compiled.IsSuccess ); var result = compiled.Pattern!.Match( "a".AsMemory() );
		Assert.True( result.IsSuccess );
		Assert.Equal( PcreNative16Oracle.ExecuteSequence( Words( text ), Words( "a" ), false, 0 ).Captures, Spans( result.Match ) );
	}

	[Fact]
	public void DefinitionsAndReturnedResultsOwnTheirMetadataAcrossCalls() {
		const string text = "(?(DEFINE)(?<g>(?<x>a|ab)))(?&g(<x>))c";
		var source = text.ToCharArray(); var pattern = PcreWordSequenceCompiler.CompileSequence( source ).Pattern!;
		Array.Fill( source, 'z' );
		var first = pattern.Match( "!zabc!".AsMemory( 1, 4 ) ).Match!;
		Assert.Equal( "1:3,-,1:2", Spans( first ) );
		foreach ( var subject in new[] { "ac", "abc", "", "ax", "abc", "x" } ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( text ), Words( subject ), false, 0 );
			var result = pattern.Match( subject.AsMemory() );
			Assert.True( result.IsSuccess ); Assert.Equal( native.Captures, Spans( result.Match ) );
		}
		Assert.Null( first.GetCapture( "g" ) ); Assert.Equal( new PcreWordCapture( 1, 2 ), first.GetCapture( "x" ) );
		Assert.Equal( "1:3,-,1:2", Spans( first ) );
	}

	[Fact]
	public void CallEdgesDoNotBypassSourceAdmissionLimits() {
		var declarations = String.Concat( "(?(DEFINE)", String.Concat( Enumerable.Repeat( "()", 256 ) ), ")(?256)" );
		var compiled = PcreWordSequenceCompiler.CompileSequence( declarations.AsMemory() );
		Assert.True( compiled.IsSuccess ); Assert.Equal( 257, compiled.Pattern!.Match( ReadOnlyMemory<char>.Empty ).Match!.Captures.Count );
		foreach ( var source in new[] { "(?(DEFINE)(?<g>a))(?&g){64}", "(?(DEFINE)(?<g>a?b?c?d?e?f?))(?&g)",
			"(?(DEFINE)(?<g>(?&g)))a", "(?R){0}a", "(?(DEFINE)(?R))",
			"(?(DEFINE)(?<g>a))(?(1)b|c)", "(?(R)a|b)", "(?(DEFINE)(?<g>a)){2}(?&g)" } ) {
			Assert.True( PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).IsSuccess, source );
		}
		foreach ( var source in new[] { "(?(DEFINE)(?<g>a))(?&g){65}", "(?(DEFINE)(?<g>a?b?c?d?e?f?))(?&g)g?",
			"(?(DEFINE)(?<g>a(*THEN)))(?&g){0}", "(*ANY)(?(DEFINE)(?<g>.))(?&g)" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	[Theory]
	[InlineData( "(?2)" )]
	[InlineData( "(?+0)" )]
	[InlineData( "(?&missing)" )]
	[InlineData( "(?1(0))" )]
	[InlineData( "(?1(2))" )]
	[InlineData( "(?R0)" )]
	public void InvalidCallHeadersKeepOriginalCodeUnitOffsets( string header ) {
		const string prefix = "é😀(a)";
		var source = String.Concat( prefix, header );
		Assert.Equal( "E", PcreNative16Oracle.ExecuteSequence( Words( source ), Array.Empty<ushort>(), true, 0 ).Kind );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidSyntax, prefix.Length ),
			PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Diagnostic );
	}

	[Fact]
	public void CalledAndUncalledDefinitionsKeepUtfValidationWindows() {
		foreach ( var source in new[] { @"(?(DEFINE)(?<g>(?<=ab)))(?&g)c", @"(?(DEFINE)(?<g>(?<=ab)))c",
			@"(?<g>(?<=ab)){0}(?&g){0}c" } ) {
			const string subject = "\uD800bc";
			Assert.Equal( "X", PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 1 ).Kind );
			var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Pattern!.Match( subject.AsMemory(), 1 );
			Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 0 ), result.Diagnostic );
		}
		var plain = PcreWordSequenceCompiler.CompileSequence( "(?(DEFINE)(?<g>a))(?&g)".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( "1:1,-", Spans( plain.Match( "\uD800a".AsMemory(), 1 ).Match ) );
		var scalar = PcreWordSequenceCompiler.CompileSequence( "(?(DEFINE)(?<g>😀))(?&g)".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidStartOffset, 1 ), scalar.Match( "😀".AsMemory(), 1 ).Diagnostic );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 2 ), scalar.Match( "😀\uD800".AsMemory() ).Diagnostic );
	}

	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( c => c is { } span ? $"{span.CodeUnitIndex}:{span.CodeUnitLength}" : "-" ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
