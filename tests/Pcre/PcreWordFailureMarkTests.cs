using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordFailureMarkTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new (string Source, string Subject, int Start, bool Utf, bool Ucp)[] {
				("(*FAIL)", "", 0, false, false),
				("(*FAIL)", "", 0, true, false),
				("(*F)", "X", 0, false, false),
				("(*F)", "X", 0, true, false),
				("(*FAIL)|", "", 0, false, false),
				("(*FAIL)|", "", 0, true, false),
				("(*F)|a", "a", 0, false, false),
				("(*F)|a", "a", 0, true, false),
				("a(*FAIL)|ab", "ab", 0, false, false),
				("a(*FAIL)|ab", "ab", 0, true, false),
				("(a)(*F)|(b)", "ab", 0, false, false),
				("(a)(*F)|(b)", "ab", 0, true, false),
				("(a(*F)|ab)(c)", "abc", 0, false, false),
				("(a(*F)|ab)(c)", "abc", 0, true, false),
				("(?>a(*F)|ab)c", "abc", 0, false, false),
				("(?>a(*F)|ab)c", "abc", 0, true, false),
				("(?>a|ab)(*F)|abc", "abc", 0, false, false),
				("(?>a|ab)(*F)|abc", "abc", 0, true, false),
				("(?=(a(*F)|ab))\\1", "ab", 0, false, false),
				("(?=(a(*F)|ab))\\1", "ab", 0, true, false),
				("(?!(a(*F)))a", "a", 0, false, false),
				("(?!(a(*F)))a", "a", 0, true, false),
				("(?=(a))(*F)|a", "a", 0, false, false),
				("(?=(a))(*F)|a", "a", 0, true, false),
				("(?:a(*F))*b", "b", 0, false, false),
				("(?:a(*F))*b", "b", 0, true, false),
				("(?:a(*F)){0}b", "b", 0, false, false),
				("(?:a(*F)){0}b", "b", 0, true, false),
				("(?:a(*F))+", "a", 0, false, false),
				("(?:a(*F))+", "a", 0, true, false),
				("\\A(?:a(*F)|ab)+\\z", "abab", 0, false, false),
				("\\A(?:a(*F)|ab)+\\z", "abab", 0, true, false),
				("\\A(a)?(?(1)(*F)|b)\\z", "b", 0, false, false),
				("\\A(a)?(?(1)(*F)|b)\\z", "b", 0, true, false),
				("\\A(a)?(?(1)(*F)|b)\\z", "ab", 0, false, false),
				("\\A(a)?(?(1)(*F)|b)\\z", "ab", 0, true, false),
				("(?(?=a)(*F)|b)", "b", 0, false, false),
				("(?(?=a)(*F)|b)", "b", 0, true, false),
				("(?(?=a)(*F)|b)", "a", 0, false, false),
				("(?(?=a)(*F)|b)", "a", 0, true, false),
				("(?<x>a(*F)|b)(?&x)", "bb", 0, false, false),
				("(?<x>a(*F)|b)(?&x)", "bb", 0, true, false),
				("(?<x>a|b(*F))(?&x)|ab", "ab", 0, false, false),
				("(?<x>a|b(*F))(?&x)|ab", "ab", 0, true, false),
				("(?(DEFINE)(?<x>a(*F)|b))(?&x)", "b", 0, false, false),
				("(?(DEFINE)(?<x>a(*F)|b))(?&x)", "b", 0, true, false),
				("\\A(?<!(?&x))X(?(DEFINE)(?<x>(*F)(?&x)))", "X", 0, false, false),
				("\\A(?<!(?&x))X(?(DEFINE)(?<x>(*F)(?&x)))", "X", 0, true, false),
				("(?<=(*F)a*)X", "X", 0, false, false),
				("(?<=(*F)a*)X", "X", 0, true, false),
				("(?<!(*F)a*)X", "X", 0, false, false),
				("(?<!(*F)a*)X", "X", 0, true, false),
				("(?<=a(*F)b*)X", "aX", 0, false, false),
				("(?<=a(*F)b*)X", "aX", 0, true, false),
				("(?<!a(*F)b*)X", "aX", 0, false, false),
				("(?<!a(*F)b*)X", "aX", 0, true, false),
				("(?<=(*F)a*|ab)X", "abX", 0, false, false),
				("(?<=(*F)a*|ab)X", "abX", 0, true, false),
				("(?<=(?:a(*F)b*|bc))X", "bcX", 0, false, false),
				("(?<=(?:a(*F)b*|bc))X", "bcX", 0, true, false),
				("(?<!(*F))(?<x>X)", "X", 0, false, false),
				("(?<!(*F))(?<x>X)", "X", 0, true, false),
				("(?*(a(*F)|ab))\\1c", "abc", 0, false, false),
				("(?*(a(*F)|ab))\\1c", "abc", 0, true, false),
				("(*F)\\Q*\\E|X", "X", 0, false, false),
				("(*F)\\Q*\\E|X", "X", 0, true, false),
				("\\Q(*FAIL)\\E", "(*FAIL)", 0, false, false),
				("\\Q(*FAIL)\\E", "(*FAIL)", 0, true, false),
				("[(*F)]+", "(*F)", 0, false, false),
				("[(*F)]+", "(*F)", 0, true, false),
				("(?<x>é)(*F)|é", "é", 0, false, true),
				("(?<x>é)(*F)|é", "é", 0, true, true),
				("(a)(*F)|ab", "ab", 0, false, false),
				("(a)(*F)|ab", "ab", 0, true, false),
				("(?i)(*F)|k", "K", 0, false, false),
				("(?i)(*F)|k", "K", 0, true, false),
				("(a|ab)c", "abc", 0, false, false),
				("(a|ab)c", "abc", 0, true, false),
				("(*MARK:X)", "", 0, false, false),
				("(*MARK:X)", "", 0, true, false),
				("(*:X)a", "a", 0, false, false),
				("(*:X)a", "a", 0, true, false),
				("a(*MARK:one)b(*:two)c", "abc", 0, false, false),
				("a(*MARK:one)b(*:two)c", "abc", 0, true, false),
				("(*MARK:X)a|b", "b", 0, false, false),
				("(*MARK:X)a|b", "b", 0, true, false),
				("a(*MARK:X)c|b", "ab", 0, false, false),
				("a(*MARK:X)c|b", "ab", 0, true, false),
				("a(*MARK:X)c|b", "aab", 0, false, false),
				("a(*MARK:X)c|b", "aab", 0, true, false),
				("(*MARK:A)aX|(*MARK:B)ab", "ab", 0, false, false),
				("(*MARK:A)aX|(*MARK:B)ab", "ab", 0, true, false),
				("(*MARK:A)(?:a(*MARK:B)b|ac)", "ac", 0, false, false),
				("(*MARK:A)(?:a(*MARK:B)b|ac)", "ac", 0, true, false),
				("(*MARK:A)(?:a(*MARK:B)b|ac)", "ab", 0, false, false),
				("(*MARK:A)(?:a(*MARK:B)b|ac)", "ab", 0, true, false),
				("(?x)(*MARK:a # x ) a", "a", 0, false, false),
				("(?x)(*MARK:a # x ) a", "a", 0, true, false),
				("(*MARK:\\x41)a", "a", 0, false, false),
				("(*MARK:\\x41)a", "a", 0, true, false),
				("(*MARK:(a)a", "a", 0, false, false),
				("(*MARK:(a)a", "a", 0, true, false),
				("(*MARK:é漢)a", "a", 0, false, false),
				("(*MARK:é漢)a", "a", 0, true, false),
				("(*:é漢)a", "a", 0, false, false),
				("(*:é漢)a", "a", 0, true, false),
				("(*MARK:A\u0000B)a", "a", 0, false, false),
				("(*MARK:A\u0000B)a", "a", 0, true, false),
				("(*MARK:\u0000)a", "a", 0, false, false),
				("(*MARK:\u0000)a", "a", 0, true, false),
				("\\Q(*MARK:X)\\E", "(*MARK:X)", 0, false, false),
				("\\Q(*MARK:X)\\E", "(*MARK:X)", 0, true, false),
				("[(*MARK:X)]", "M", 0, false, false),
				("[(*MARK:X)]", "M", 0, true, false),
				("(*MARK:A)(a|ab)(*MARK:B)c", "abc", 0, false, false),
				("(*MARK:A)(a|ab)(*MARK:B)c", "abc", 0, true, false),
				("(*MARK:A)(a|ab)c", "abc", 0, false, false),
				("(*MARK:A)(a|ab)c", "abc", 0, true, false),
				("(*MARK:A)(?:a(*MARK:B))*a", "aa", 0, false, false),
				("(*MARK:A)(?:a(*MARK:B))*a", "aa", 0, true, false),
				("(*MARK:A)(?:a(*MARK:B))*?a", "aa", 0, false, false),
				("(*MARK:A)(?:a(*MARK:B))*?a", "aa", 0, true, false),
				("(*MARK:A)(?:a(*MARK:B))*+b", "aab", 0, false, false),
				("(*MARK:A)(?:a(*MARK:B))*+b", "aab", 0, true, false),
				("(*MARK:A)(?:a(*MARK:B)){2}b", "aab", 0, false, false),
				("(*MARK:A)(?:a(*MARK:B)){2}b", "aab", 0, true, false),
				("(*MARK:A)(?:a(*MARK:B)){0}b", "b", 0, false, false),
				("(*MARK:A)(?:a(*MARK:B)){0}b", "b", 0, true, false),
				("(?:a(*MARK:X)|b(*MARK:Y))+", "aba", 0, false, false),
				("(?:a(*MARK:X)|b(*MARK:Y))+", "aba", 0, true, false),
				("(?:a(*MARK:X)|b(*MARK:Y))+", "bab", 0, false, false),
				("(?:a(*MARK:X)|b(*MARK:Y))+", "bab", 0, true, false),
				("(?:(*MARK:X))*", "", 0, false, false),
				("(?:(*MARK:X))*", "", 0, true, false),
				("(?:(*MARK:X))??", "", 0, false, false),
				("(?:(*MARK:X))??", "", 0, true, false),
				("(?:(*MARK:X))?+", "", 0, false, false),
				("(?:(*MARK:X))?+", "", 0, true, false),
				("(*MARK:root)(?=a(*MARK:yes))a", "a", 0, false, false),
				("(*MARK:root)(?=a(*MARK:yes))a", "a", 0, true, false),
				("(*MARK:root)(?=a(*MARK:no)b)a|a", "a", 0, false, false),
				("(*MARK:root)(?=a(*MARK:no)b)a|a", "a", 0, true, false),
				("(*MARK:root)(?!a(*MARK:no)b)a", "a", 0, false, false),
				("(*MARK:root)(?!a(*MARK:no)b)a", "a", 0, true, false),
				("(*MARK:root)(?!a(*MARK:no))a|a", "a", 0, false, false),
				("(*MARK:root)(?!a(*MARK:no))a|a", "a", 0, true, false),
				("(*MARK:root)(?(?=a(*MARK:yes))a|b)", "a", 0, false, false),
				("(*MARK:root)(?(?=a(*MARK:yes))a|b)", "a", 0, true, false),
				("(*MARK:root)(?(?=a(*MARK:no)b)b|a)", "a", 0, false, false),
				("(*MARK:root)(?(?=a(*MARK:no)b)b|a)", "a", 0, true, false),
				("(*MARK:root)(?(?!a(*MARK:no)b)a|b)", "a", 0, false, false),
				("(*MARK:root)(?(?!a(*MARK:no)b)a|b)", "a", 0, true, false),
				("(*MARK:root)(?(?!a(*MARK:no))b|a)", "a", 0, false, false),
				("(*MARK:root)(?(?!a(*MARK:no))b|a)", "a", 0, true, false),
				("(*MARK:root)(?*(a(*MARK:one)|ab(*MARK:two)))\\1c", "abc", 0, false, false),
				("(*MARK:root)(?*(a(*MARK:one)|ab(*MARK:two)))\\1c", "abc", 0, true, false),
				("(*MARK:root)(?*(a(*MARK:one)|ab(*MARK:two)))\\1c", "abababc", 0, false, false),
				("(*MARK:root)(?*(a(*MARK:one)|ab(*MARK:two)))\\1c", "abababc", 0, true, false),
				("(*MARK:root)(?<=a(*MARK:behind))b", "ab", 0, false, false),
				("(*MARK:root)(?<=a(*MARK:behind))b", "ab", 0, true, false),
				("(*MARK:root)(?<!(a(*MARK:no)b))b", "ab", 0, false, false),
				("(*MARK:root)(?<!(a(*MARK:no)b))b", "ab", 0, true, false),
				("(*MARK:root)(?(?<=(a(*MARK:yes)))b|X)", "ab", 0, false, false),
				("(*MARK:root)(?(?<=(a(*MARK:yes)))b|X)", "ab", 0, true, false),
				("(*MARK:root)(?(?<!(a(*MARK:no)))X|b)", "ab", 0, false, false),
				("(*MARK:root)(?(?<!(a(*MARK:no)))X|b)", "ab", 0, true, false),
				("\\A(*MARK:root)(?:(?>a(*MARK:in))b|ac)", "ac", 0, false, false),
				("\\A(*MARK:root)(?:(?>a(*MARK:in))b|ac)", "ac", 0, true, false),
				("\\A(*MARK:root)(?>a(*MARK:in))b", "ab", 0, false, false),
				("\\A(*MARK:root)(?>a(*MARK:in))b", "ab", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(*MARK:callee)))(*MARK:caller)(?&x)b", "ab", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(*MARK:callee)))(*MARK:caller)(?&x)b", "ab", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(*MARK:callee)))(*MARK:caller)(?&x)(*MARK:tail)b", "ab", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(*MARK:callee)))(*MARK:caller)(?&x)(*MARK:tail)b", "ab", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(*MARK:callee)b))(*MARK:caller)(?:(?&x)X|ac)d", "acd", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(*MARK:callee)b))(*MARK:caller)(?:(?&x)X|ac)d", "acd", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(*MARK:in)(?&x)?b))(*MARK:root)(?&x)c", "aabbc", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(*MARK:in)(?&x)?b))(*MARK:root)(?&x)c", "aabbc", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(*MARK:X)(?&y)?b)(?<y>c(*MARK:Y)(?&x)?d))(?&x)e", "acdbe", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(*MARK:X)(?&y)?b)(?<y>c(*MARK:Y)(?&x)?d))(?&x)e", "acdbe", 0, true, false),
				("\\A(?|(?<x>a(*MARK:A))|(?<x>b(*MARK:B)))\\k<x>", "bb", 0, false, false),
				("\\A(?|(?<x>a(*MARK:A))|(?<x>b(*MARK:B)))\\k<x>", "bb", 0, true, false),
				("\\A(?J)(?<x>a)(*MARK:A)X|(?<x>ab)(*MARK:B)", "ab", 0, false, false),
				("\\A(?J)(?<x>a)(*MARK:A)X|(?<x>ab)(*MARK:B)", "ab", 0, true, false),
				("(*MARK:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)a", "a", 0, false, false),
				("(*MARK:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)a", "a", 0, true, false),
				("(*MARK:éééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééX)a", "a", 0, false, false),
				("(*MARK:éééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééX)a", "a", 0, true, false),
				("(*MARK:a\\)a", "a", 0, false, false),
				("(*MARK:a\\)a", "a", 0, true, false),
				("(*MARK:a:b)a", "a", 0, false, false),
				("(*MARK:a:b)a", "a", 0, true, false),
				("(*FAIL:N)", "", 0, false, false),
				("(*FAIL:N)", "", 0, true, false),
				("(*F:N)", "X", 0, false, false),
				("(*F:N)", "X", 0, true, false),
				("(*FAIL:N)|", "", 0, false, false),
				("(*FAIL:N)|", "", 0, true, false),
				("(*F:N)|a", "a", 0, false, false),
				("(*F:N)|a", "a", 0, true, false),
				("a(*FAIL:N)|ab", "ab", 0, false, false),
				("a(*FAIL:N)|ab", "ab", 0, true, false),
				("(a)(*F:N)|(b)", "ab", 0, false, false),
				("(a)(*F:N)|(b)", "ab", 0, true, false),
				("(a(*F:N)|ab)(c)", "abc", 0, false, false),
				("(a(*F:N)|ab)(c)", "abc", 0, true, false),
				("(?>a(*F:N)|ab)c", "abc", 0, false, false),
				("(?>a(*F:N)|ab)c", "abc", 0, true, false),
				("(?>a|ab)(*F:N)|abc", "abc", 0, false, false),
				("(?>a|ab)(*F:N)|abc", "abc", 0, true, false),
				("(?=(a(*F:N)|ab))\\1", "ab", 0, false, false),
				("(?=(a(*F:N)|ab))\\1", "ab", 0, true, false),
				("(?!(a(*F:N)))a", "a", 0, false, false),
				("(?!(a(*F:N)))a", "a", 0, true, false),
				("(?=(a))(*F:N)|a", "a", 0, false, false),
				("(?=(a))(*F:N)|a", "a", 0, true, false),
				("(?:a(*F:N))*b", "b", 0, false, false),
				("(?:a(*F:N))*b", "b", 0, true, false),
				("(?:a(*F:N)){0}b", "b", 0, false, false),
				("(?:a(*F:N)){0}b", "b", 0, true, false),
				("(?:a(*F:N))+", "a", 0, false, false),
				("(?:a(*F:N))+", "a", 0, true, false),
				("\\A(?:a(*F:N)|ab)+\\z", "abab", 0, false, false),
				("\\A(?:a(*F:N)|ab)+\\z", "abab", 0, true, false),
				("\\A(a)?(?(1)(*F:N)|b)\\z", "b", 0, false, false),
				("\\A(a)?(?(1)(*F:N)|b)\\z", "b", 0, true, false),
				("\\A(a)?(?(1)(*F:N)|b)\\z", "ab", 0, false, false),
				("\\A(a)?(?(1)(*F:N)|b)\\z", "ab", 0, true, false),
				("(?(?=a)(*F:N)|b)", "b", 0, false, false),
				("(?(?=a)(*F:N)|b)", "b", 0, true, false),
				("(?(?=a)(*F:N)|b)", "a", 0, false, false),
				("(?(?=a)(*F:N)|b)", "a", 0, true, false),
				("(?<x>a(*F:N)|b)(?&x)", "bb", 0, false, false),
				("(?<x>a(*F:N)|b)(?&x)", "bb", 0, true, false),
				("(?<x>a|b(*F:N))(?&x)|ab", "ab", 0, false, false),
				("(?<x>a|b(*F:N))(?&x)|ab", "ab", 0, true, false),
				("(?(DEFINE)(?<x>a(*F:N)|b))(?&x)", "b", 0, false, false),
				("(?(DEFINE)(?<x>a(*F:N)|b))(?&x)", "b", 0, true, false),
				("\\A(?<!(?&x))X(?(DEFINE)(?<x>(*F:N)(?&x)))", "X", 0, false, false),
				("\\A(?<!(?&x))X(?(DEFINE)(?<x>(*F:N)(?&x)))", "X", 0, true, false),
				("(?<=(*F:N)a*)X", "X", 0, false, false),
				("(?<=(*F:N)a*)X", "X", 0, true, false),
				("(?<!(*F:N)a*)X", "X", 0, false, false),
				("(?<!(*F:N)a*)X", "X", 0, true, false),
				("(?<=a(*F:N)b*)X", "aX", 0, false, false),
				("(?<=a(*F:N)b*)X", "aX", 0, true, false),
				("(?<!a(*F:N)b*)X", "aX", 0, false, false),
				("(?<!a(*F:N)b*)X", "aX", 0, true, false),
				("(?<=(*F:N)a*|ab)X", "abX", 0, false, false),
				("(?<=(*F:N)a*|ab)X", "abX", 0, true, false),
				("(?<=(?:a(*F:N)b*|bc))X", "bcX", 0, false, false),
				("(?<=(?:a(*F:N)b*|bc))X", "bcX", 0, true, false),
				("(?<!(*F:N))(?<x>X)", "X", 0, false, false),
				("(?<!(*F:N))(?<x>X)", "X", 0, true, false),
				("(?*(a(*F:N)|ab))\\1c", "abc", 0, false, false),
				("(?*(a(*F:N)|ab))\\1c", "abc", 0, true, false),
				("(*F:N)\\Q*\\E|X", "X", 0, false, false),
				("(*F:N)\\Q*\\E|X", "X", 0, true, false),
				("\\Q(*FAIL:N)\\E", "(*FAIL)", 0, false, false),
				("\\Q(*FAIL:N)\\E", "(*FAIL)", 0, true, false),
				("[(*F:N)]+", "(*F)", 0, false, false),
				("[(*F:N)]+", "(*F)", 0, true, false),
				("(?<x>é)(*F:N)|é", "é", 0, false, true),
				("(?<x>é)(*F:N)|é", "é", 0, true, true),
				("(a)(*F:N)|ab", "ab", 0, false, false),
				("(a)(*F:N)|ab", "ab", 0, true, false),
				("(?i)(*F:N)|k", "K", 0, false, false),
				("(?i)(*F:N)|k", "K", 0, true, false),
				("(*FAIL:N)", "a", 0, false, false),
				("(*FAIL:N)", "a", 0, true, false),
				("a(*FAIL:N)b", "ab", 0, false, false),
				("a(*FAIL:N)b", "ab", 0, true, false),
				("(*MARK:M)a(*FAIL:N)b", "ab", 0, false, false),
				("(*MARK:M)a(*FAIL:N)b", "ab", 0, true, false),
				("(*MARK:M)a(*FAIL:)b", "ab", 0, false, false),
				("(*MARK:M)a(*FAIL:)b", "ab", 0, true, false),
				("(*MARK:M)a(*FAIL:\u0000)b", "ab", 0, false, false),
				("(*MARK:M)a(*FAIL:\u0000)b", "ab", 0, true, false),
				("a(*FAIL:é漢)b", "ab", 0, false, false),
				("a(*FAIL:é漢)b", "ab", 0, true, false),
				("(?x)a (*FAIL:n # x) b", "ab", 0, false, false),
				("(?x)a (*FAIL:n # x) b", "ab", 0, true, false),
				("a(*FAIL:\\x41)b", "ab", 0, false, false),
				("a(*FAIL:\\x41)b", "ab", 0, true, false),
				("\\Q(*FAIL:N)\\E", "(*FAIL:N)", 0, false, false),
				("\\Q(*FAIL:N)\\E", "(*FAIL:N)", 0, true, false),
				("[(*FAIL:N)]", "N", 0, false, false),
				("[(*FAIL:N)]", "N", 0, true, false),
				("a(*MARK:M)(?!b(*FAIL:N))b", "ab", 0, false, false),
				("a(*MARK:M)(?!b(*FAIL:N))b", "ab", 0, true, false),
				("(?(?=a(*FAIL:N))a|b)", "ab", 0, false, false),
				("(?(?=a(*FAIL:N))a|b)", "ab", 0, true, false),
				("(?(?!a(*FAIL:N))b|a)", "ab", 0, false, false),
				("(?(?!a(*FAIL:N))b|a)", "ab", 0, true, false),
				("(?=(a(*FAIL:N)b))a|a", "a", 0, false, false),
				("(?=(a(*FAIL:N)b))a|a", "a", 0, true, false),
				("(?!(a(*FAIL:N)))a|a", "a", 0, false, false),
				("(?!(a(*FAIL:N)))a|a", "a", 0, true, false),
				("(?*a(*FAIL:N))a|a", "a", 0, false, false),
				("(?*a(*FAIL:N))a|a", "a", 0, true, false),
				("(?<=a(*FAIL:N))b|b", "ab", 0, false, false),
				("(?<=a(*FAIL:N))b|b", "ab", 0, true, false),
				("(?<!a(*FAIL:N))b", "ab", 0, false, false),
				("(?<!a(*FAIL:N))b", "ab", 0, true, false),
				("(?<=(*FAIL:N)a*)b", "b", 0, false, false),
				("(?<=(*FAIL:N)a*)b", "b", 0, true, false),
				("(?<!(*FAIL:N)a*)b", "b", 0, false, false),
				("(?<!(*FAIL:N)a*)b", "b", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(*FAIL:N)b))(?&x)c|ac", "ac", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(*FAIL:N)b))(?&x)c|ac", "ac", 0, true, false),
				("\\A(?(DEFINE)(?<x>(a)(*FAIL:N)b))(?&x(2))c|ac", "ac", 0, false, false),
				("\\A(?(DEFINE)(?<x>(a)(*FAIL:N)b))(?&x(2))c|ac", "ac", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(?&x)?(*FAIL:N)b))(?&x)c|aac", "aac", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(?&x)?(*FAIL:N)b))(?&x)c|aac", "aac", 0, true, false),
				("é(*FAIL:N)X|é", "é", 0, false, false),
				("é(*FAIL:N)X|é", "é", 0, true, false),
				("(*F:N)", "a", 0, false, false),
				("(*F:N)", "a", 0, true, false),
				("a(*F:N)b", "ab", 0, false, false),
				("a(*F:N)b", "ab", 0, true, false),
				("(*MARK:M)a(*F:N)b", "ab", 0, false, false),
				("(*MARK:M)a(*F:N)b", "ab", 0, true, false),
				("(*MARK:M)a(*F:)b", "ab", 0, false, false),
				("(*MARK:M)a(*F:)b", "ab", 0, true, false),
				("(*MARK:M)a(*F:\u0000)b", "ab", 0, false, false),
				("(*MARK:M)a(*F:\u0000)b", "ab", 0, true, false),
				("a(*F:é漢)b", "ab", 0, false, false),
				("a(*F:é漢)b", "ab", 0, true, false),
				("(?x)a (*F:n # x) b", "ab", 0, false, false),
				("(?x)a (*F:n # x) b", "ab", 0, true, false),
				("a(*F:\\x41)b", "ab", 0, false, false),
				("a(*F:\\x41)b", "ab", 0, true, false),
				("\\Q(*F:N)\\E", "(*F:N)", 0, false, false),
				("\\Q(*F:N)\\E", "(*F:N)", 0, true, false),
				("[(*F:N)]", "N", 0, false, false),
				("[(*F:N)]", "N", 0, true, false),
				("a(*F:N)|ab", "ab", 0, false, false),
				("a(*F:N)|ab", "ab", 0, true, false),
				("a(*MARK:M)(?!b(*F:N))b", "ab", 0, false, false),
				("a(*MARK:M)(?!b(*F:N))b", "ab", 0, true, false),
				("(?(?=a(*F:N))a|b)", "ab", 0, false, false),
				("(?(?=a(*F:N))a|b)", "ab", 0, true, false),
				("(?(?!a(*F:N))b|a)", "ab", 0, false, false),
				("(?(?!a(*F:N))b|a)", "ab", 0, true, false),
				("(?=(a(*F:N)b))a|a", "a", 0, false, false),
				("(?=(a(*F:N)b))a|a", "a", 0, true, false),
				("(?!(a(*F:N)))a|a", "a", 0, false, false),
				("(?!(a(*F:N)))a|a", "a", 0, true, false),
				("(?*a(*F:N))a|a", "a", 0, false, false),
				("(?*a(*F:N))a|a", "a", 0, true, false),
				("(?<=a(*F:N))b|b", "ab", 0, false, false),
				("(?<=a(*F:N))b|b", "ab", 0, true, false),
				("(?<!a(*F:N))b", "ab", 0, false, false),
				("(?<!a(*F:N))b", "ab", 0, true, false),
				("(?<=(*F:N)a*)b", "b", 0, false, false),
				("(?<=(*F:N)a*)b", "b", 0, true, false),
				("(?<!(*F:N)a*)b", "b", 0, false, false),
				("(?<!(*F:N)a*)b", "b", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(*F:N)b))(?&x)c|ac", "ac", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(*F:N)b))(?&x)c|ac", "ac", 0, true, false),
				("\\A(?(DEFINE)(?<x>(a)(*F:N)b))(?&x(2))c|ac", "ac", 0, false, false),
				("\\A(?(DEFINE)(?<x>(a)(*F:N)b))(?&x(2))c|ac", "ac", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(?&x)?(*F:N)b))(?&x)c|aac", "aac", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(?&x)?(*F:N)b))(?&x)c|aac", "aac", 0, true, false),
				("é(*F:N)X|é", "é", 0, false, false),
				("é(*F:N)X|é", "é", 0, true, false),
				("(*MARK:root)(?:a{0,2}(*MARK:first)x|a{0,2}(*MARK:last)b)", "aab", 0, false, false),
				("(*MARK:root)(?:a{0,2}(*MARK:first)x|a{0,2}(*MARK:last)b)", "aab", 0, true, false),
				("(*MARK:root)(?:a{0,2}(*MARK:first)x|a{0,2}b)", "aab", 0, false, false),
				("(*MARK:root)(?:a{0,2}(*MARK:first)x|a{0,2}b)", "aab", 0, true, false),
				("(*MARK:root)(a)?(?(1)b(*MARK:yes)|c(*MARK:no))", "ab", 0, false, false),
				("(*MARK:root)(a)?(?(1)b(*MARK:yes)|c(*MARK:no))", "ab", 0, true, false),
				("(*MARK:root)(a)?(?(1)b(*MARK:yes)|c(*MARK:no))", "c", 0, false, false),
				("(*MARK:root)(a)?(?(1)b(*MARK:yes)|c(*MARK:no))", "c", 0, true, false),
				("(*MARK:root)(a)(?(1)b(*FAIL:bad)|a)", "aa", 0, false, false),
				("(*MARK:root)(a)(?(1)b(*FAIL:bad)|a)", "aa", 0, true, false),
				("(*MARK:root)(?(VERSION=10.48)a(*MARK:yes)|b(*MARK:no))", "a", 0, false, false),
				("(*MARK:root)(?(VERSION=10.48)a(*MARK:yes)|b(*MARK:no))", "a", 0, true, false),
				("(*MARK:root)(?(VERSION=0)a(*MARK:yes)|b(*MARK:no))", "b", 0, false, false),
				("(*MARK:root)(?(VERSION=0)a(*MARK:yes)|b(*MARK:no))", "b", 0, true, false),
				("(*MARK:root)(?(?!(a(*MARK:no)))X|\\1)", "a", 0, false, false),
				("(*MARK:root)(?(?!(a(*MARK:no)))X|\\1)", "a", 0, true, false),
				("(*MARK:root)(?(?!(a(*FAIL:bad)))a|X)", "a", 0, false, false),
				("(*MARK:root)(?(?!(a(*FAIL:bad)))a|X)", "a", 0, true, false),
				("(*MARK:root)(?(?<!(a(*MARK:no)))X|b)", "ab", 1, false, false),
				("(*MARK:root)(?(?<!(a(*MARK:no)))X|b)", "ab", 1, true, false),
				("(*MARK:root)(?(?<!(a(*FAIL:bad)))b|X)", "ab", 1, false, false),
				("(*MARK:root)(?(?<!(a(*FAIL:bad)))b|X)", "ab", 1, true, false),
				("(*MARK:root)(?:(?=(a(*MARK:yes)))X|a)", "a", 0, false, false),
				("(*MARK:root)(?:(?=(a(*MARK:yes)))X|a)", "a", 0, true, false),
				("(*MARK:root)(?:(?>(a(*MARK:yes)))X|a)", "a", 0, false, false),
				("(*MARK:root)(?:(?>(a(*MARK:yes)))X|a)", "a", 0, true, false),
				("(*MARK:root)(?<*(a(*MARK:one)|aa(*MARK:two)))\\1b", "aaaab", 2, false, false),
				("(*MARK:root)(?<*(a(*MARK:one)|aa(*MARK:two)))\\1b", "aaaab", 2, true, false),
				("(*MARK:root)(?:(?=(*MARK:discard)(a)))?a", "a", 0, false, false),
				("(*MARK:root)(?:(?=(*MARK:discard)(a)))?a", "a", 0, true, false),
				("(*MARK:root)(?:(?=(*MARK:discard)(a))){2}a", "a", 0, false, false),
				("(*MARK:root)(?:(?=(*MARK:discard)(a))){2}a", "a", 0, true, false),
				("(*MARK:root)(?=(a(*FAIL:bad)|ab(*MARK:good)))\\1", "ab", 0, false, false),
				("(*MARK:root)(?=(a(*FAIL:bad)|ab(*MARK:good)))\\1", "ab", 0, true, false),
				("(*MARK:root)(?!(a(*FAIL:bad)))a", "a", 0, false, false),
				("(*MARK:root)(?!(a(*FAIL:bad)))a", "a", 0, true, false),
				("(*MARK:root)(?<!(a(*FAIL:bad)))b", "ab", 1, false, false),
				("(*MARK:root)(?<!(a(*FAIL:bad)))b", "ab", 1, true, false),
				("(*MARK:root)(?<=(*FAIL:bad)a*|a(*MARK:good))b", "ab", 1, false, false),
				("(*MARK:root)(?<=(*FAIL:bad)a*|a(*MARK:good))b", "ab", 1, true, false),
				("(*MARK:root)(?<=a(*MARK:one)|aa(*MARK:two))b", "aab", 2, false, false),
				("(*MARK:root)(?<=a(*MARK:one)|aa(*MARK:two))b", "aab", 2, true, false),
				("(*MARK:root)(?<=a{1,2}(*MARK:yes))b", "aab", 2, false, false),
				("(*MARK:root)(?<=a{1,2}(*MARK:yes))b", "aab", 2, true, false),
				("(*MARK:root)(?<=(*F)(?&x))b(?(DEFINE)(?<x>(*F)(?&x)))", "b", 0, false, false),
				("(*MARK:root)(?<=(*F)(?&x))b(?(DEFINE)(?<x>(*F)(?&x)))", "b", 0, true, false),
				("(*MARK:root)(?(DEFINE)(?<x>(a(*MARK:callee)|ab(*MARK:retry))))(?&x(2))c", "abc", 0, false, false),
				("(*MARK:root)(?(DEFINE)(?<x>(a(*MARK:callee)|ab(*MARK:retry))))(?&x(2))c", "abc", 0, true, false),
				("(*MARK:root)(?(DEFINE)(?<x>(a(*MARK:callee)|ab(*MARK:retry))))(?&x)c", "abc", 0, false, false),
				("(*MARK:root)(?(DEFINE)(?<x>(a(*MARK:callee)|ab(*MARK:retry))))(?&x)c", "abc", 0, true, false),
				("(*MARK:root)(?(DEFINE)(?<x>(a(*FAIL:bad)|ab(*MARK:good))))(?&x(2))c", "abc", 0, false, false),
				("(*MARK:root)(?(DEFINE)(?<x>(a(*FAIL:bad)|ab(*MARK:good))))(?&x(2))c", "abc", 0, true, false),
				("(*MARK:root)(?(DEFINE)(?<x>(?(R&x)a(*MARK:called)|X)))(?&x)b", "ab", 0, false, false),
				("(*MARK:root)(?(DEFINE)(?<x>(?(R&x)a(*MARK:called)|X)))(?&x)b", "ab", 0, true, false),
				("(*MARK:root)(?(DEFINE)(?<x>a(*MARK:outer)(?&y)?b)(?<y>a(*MARK:inner)(?&x)?b))(?&x)c", "aabbc", 0, false, false),
				("(*MARK:root)(?(DEFINE)(?<x>a(*MARK:outer)(?&y)?b)(?<y>a(*MARK:inner)(?&x)?b))(?&x)c", "aabbc", 0, true, false),
				("(*MARK:root)(?(DEFINE)(?<x>a(*MARK:callee)b))(?:(?&x)X|ac)d", "acd", 0, false, false),
				("(*MARK:root)(?(DEFINE)(?<x>a(*MARK:callee)b))(?:(?&x)X|ac)d", "acd", 0, true, false),
				("(*MARK:root)(a(*MARK:once))?+b", "ab", 0, false, false),
				("(*MARK:root)(a(*MARK:once))?+b", "ab", 0, true, false),
				("(*MARK:root)(a(*MARK:once))??b", "b", 0, false, false),
				("(*MARK:root)(a(*MARK:once))??b", "b", 0, true, false),
				("(*MARK:root)(a(*MARK:once)){0}b", "b", 0, false, false),
				("(*MARK:root)(a(*MARK:once)){0}b", "b", 0, true, false),
				("(?m)(*MARK:line)^a(*MARK:end)$", "z\na\nx", 0, false, false),
				("(?m)(*MARK:line)^a(*MARK:end)$", "z\na\nx", 0, true, false),
				("(*CR)(?m)(*MARK:line)^a(*MARK:end)$", "z\ra\rx", 0, false, false),
				("(*CR)(?m)(*MARK:line)^a(*MARK:end)$", "z\ra\rx", 0, true, false),
				("(*NUL)(?m)(*MARK:line)^a(*MARK:end)$", "z\u0000a\u0000x", 0, false, false),
				("(*NUL)(?m)(*MARK:line)^a(*MARK:end)$", "z\u0000a\u0000x", 0, true, false),
				("(*MARK:root)(?:\\R(*MARK:nl))+a", "\r\n\ra", 0, false, false),
				("(*MARK:root)(?:\\R(*MARK:nl))+a", "\r\n\ra", 0, true, false),
				("(*MARK:é😀)é(*:漢𐐀)😀", "zé😀", 1, false, false),
				("(*MARK:é😀)é(*:漢𐐀)😀", "zé😀", 1, true, false),
				("(?i)(*MARK:lower)é𐐀(*MARK:終)", "É𐐨", 0, false, false),
				("(?i)(*MARK:lower)é𐐀(*MARK:終)", "É𐐨", 0, true, false),
				("(*MARK:é\u0000😀)a", "a", 0, false, false),
				("(*MARK:é\u0000😀)a", "a", 0, true, false),
				("(*MARK:root)(*:local\u0000tail)a", "a", 0, false, false),
				("(*MARK:root)(*:local\u0000tail)a", "a", 0, true, false),
				("(*MARK:root)(*MARK:\u0000)a", "a", 0, false, false),
				("(*MARK:root)(*MARK:\u0000)a", "a", 0, true, false),
				("(*MARK:root)(?!(*F:))a", "a", 0, false, false),
				("(*MARK:root)(?!(*F:))a", "a", 0, true, false),
				("(*MARK:root)(?!(*F:\u0000))a", "a", 0, false, false),
				("(*MARK:root)(?!(*F:\u0000))a", "a", 0, true, false),
				("(*MARK:root)(?!(*F:é😀))a", "a", 0, false, false),
				("(*MARK:root)(?!(*F:é😀))a", "a", 0, true, false),
				("(*MARK:root)(?!(?:a(*MARK:inside)))(*F)|(*MARK:win)a", "a", 0, false, false),
				("(*MARK:root)(?!(?:a(*MARK:inside)))(*F)|(*MARK:win)a", "a", 0, true, false),
				("(*MARK:A)(?:(*F:B)|(*MARK:C))", "", 0, false, false),
				("(*MARK:A)(?:(*F:B)|(*MARK:C))", "", 0, true, false),
				("(*FAIL:é😀)|(*MARK:ok)é", "é", 0, false, false),
				("(*FAIL:é😀)|(*MARK:ok)é", "é", 0, true, false),
				("(?x)(*MARK:é # 😀 ) a", "a", 0, false, false),
				("(?x)(*MARK:é # 😀 ) a", "a", 0, true, false),
				("(*MARK:\\x{1F600})a", "a", 0, false, false),
				("(*MARK:\\x{1F600})a", "a", 0, true, false),
				("(*MARK:root)a|(*MARK:other)b", "zab", 2, false, false),
				("(*MARK:root)a|(*MARK:other)b", "zab", 2, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow))*\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad))*b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow))*\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad))*b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow))*?\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad))*?b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow))*?\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad))*?b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow))*+\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad))*+b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow))*+\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad))*+b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow))+\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad))+b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow))+\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad))+b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow))+?\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad))+?b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow))+?\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad))+?b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow))++\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad))++b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow))++\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad))++b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){1,}\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){1,}b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){1,}\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){1,}b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){1,}?\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){1,}?b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){1,}?\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){1,}?b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){1,}+\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){1,}+b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){1,}+\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){1,}+b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){2,}\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){2,}b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){2,}\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){2,}b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){2,}?\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){2,}?b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){2,}?\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){2,}?b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){2,}+\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){2,}+b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){2,}+\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){2,}+b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){3}\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){3}b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){3}\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){3}b", "b", 0, true, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){1,3}?\\1", "aaaa", 0, false, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){1,3}?b", "b", 0, false, false),
				("(*MARK:root)(?:(?=(\\1a|a))(*MARK:grow)){1,3}?\\1", "aaaa", 0, true, false),
				("(*MARK:root)(?:(a)(*FAIL:bad)){1,3}?b", "b", 0, true, false),
				("(*MARK:\ud800)\ud800", "\ud800", 0, false, false),
				("(*MARK:root)(?!(*FAIL:\ud800))\ud800", "\ud800", 0, false, false),
				("(*MARK:\udc00)\udc00", "\udc00", 0, false, false),
				("(*MARK:root)(?!(*FAIL:\udc00))\udc00", "\udc00", 0, false, false),
				("(*MARK:￿)￿", "￿", 0, false, false),
				("(*MARK:root)(?!(*FAIL:￿))￿", "￿", 0, false, false),
				("(*MARK:ééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééé)a", "a", 0, false, false),
				("(*MARK:ééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééé)a", "a", 0, true, false),
				("(*MARK:😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀X)a", "a", 0, false, false),
				("(*MARK:😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀X)a", "a", 0, true, false),
				("(*MARK:\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000)a", "a", 0, false, false),
				("(*MARK:\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000)a", "a", 0, true, false),
				("(*MARK:éééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééé)a", "a", 0, false, false),
				("(*MARK:éééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééé)a", "a", 0, true, false),
				("(*MARK:😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀)a", "a", 0, false, false),
				("(*MARK:😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀😀)a", "a", 0, true, false),
				("(*MARK:\u0000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)a", "a", 0, false, false),
				("(*MARK:\u0000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)a", "a", 0, true, false),
				("(*MARK:éééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééé)a", "a", 0, false, false),
				("(*MARK:éééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééé)a", "a", 0, true, false),
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
	public void FailureAndSuccessfulLabelsAgreeWithEveryNativePolicy( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"failure/mark {id}: {compiled.Diagnostic}" );
		var patternUnits = Words( source ); var subjectUnits = Words( subject );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( patternUnits, subjectUnits, utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			Assert.True( native.Captures == Spans( result.Match ) && native.Mark == MarkHex( result.Match ),
				$"failure/mark {id}/{defaults}/{mask}: expected {native.Captures}|{native.Mark}, actual {Spans( result.Match )}|{MarkHex( result.Match )}" );
			Assert.Equal( native.Kind == "M", result.Match is not null );
		}
	}

	[Theory]
	[InlineData( "(*MARK:é😀)a", "a", "0:1", "é😀" )]
	[InlineData( "(*MARK:root)(*:é\0😀)a", "a", "0:1", "é" )]
	[InlineData( "(*MARK:root)(*:\0)a", "a", "0:1", "" )]
	[InlineData( "(*MARK:A)(?:a(*MARK:B)x|ab)", "ab", "0:2", "A" )]
	[InlineData( "(*MARK:A)(?=a(*MARK:B))a", "a", "0:1", "B" )]
	[InlineData( "(*MARK:A)(?!a(*MARK:B)x)a", "a", "0:1", "A" )]
	[InlineData( @"(*MARK:A)(?(?!(a(*MARK:B)))X|\1)", "a", "0:1,0:1", "B" )]
	[InlineData( "(*MARK:A)(?(?!(a(*FAIL:B)))a|X)", "a", "0:1,-", "A" )]
	[InlineData( @"(*MARK:A)(?*(a(*MARK:B)|ab(*MARK:C)))\1c", "abc", "0:3,0:2", "C" )]
	[InlineData( "(*MARK:A)(?(DEFINE)(?<x>a(*MARK:B)))(?&x)b", "ab", "0:2,-", "B" )]
	[InlineData( "(*MARK:A)(?(DEFINE)(?<x>a(*MARK:B)x))(?:(?&x)|ab)", "ab", "0:2,-", "A" )]
	[InlineData( "(*FAIL:é😀)|a(*MARK:win)", "a", "0:1", "win" )]
	public void MarksHaveExplicitOriginalWordValues( string source, string subject, string spans, string mark ) {
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 0 );
		Assert.Equal( spans, native.Captures ); Assert.Equal( Hex( mark ), native.Mark );
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.True( compiled.IsSuccess ); var result = compiled.Pattern!.Match( subject.AsMemory() );
		Assert.True( result.IsSuccess ); Assert.Equal( spans, Spans( result.Match ) ); Assert.Equal( mark, result.Match!.MarkName );
	}

	[Theory]
	[InlineData( "(*MARK:é😀)(a|ab)c", "abc" )]
	[InlineData( "(*MARK:A)(?:a(*MARK:B)x|ab)", "ab" )]
	[InlineData( @"(*MARK:A)(?*(a(*MARK:B)|ab(*MARK:C)))\1c", "abc" )]
	[InlineData( "(*MARK:A)(?(?<!(a(*MARK:é)))X|b)", "ab" )]
	[InlineData( "(*MARK:A)(?(DEFINE)(?<x>a(*MARK:é)))(?&x)b", "ab" )]
	[InlineData( "(*MARK:A)(?(DEFINE)(?<x>(a(*MARK:B)|ab(*MARK:C))))(?&x(2))c", "abc" )]
	[InlineData( "(*MARK:A)(?!a(*FAIL:é))a", "a" )]
	[InlineData( "(*FAIL:é😀)|(*MARK:win)é", "é" )]
	[InlineData( "(*MARK:A)(?:(?=(a))(*MARK:B)){2}a", "a" )]
	public void ByteAndWordLabelsAndCapturedValuesAgree( string source, string subject ) {
		var word = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		var bytes = Encoding.UTF8.GetBytes( subject );
		var compiledByte = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true } );
		Assert.True( word.IsSuccess ); Assert.True( compiledByte.IsSuccess );
		for ( var mask = 0; mask < 64; mask++ ) {
			var policy = PcreMatchPolicyConsumer.Policy( mask );
			var wr = word.Pattern!.Match( subject.AsMemory(), policy ); var br = compiledByte.Pattern!.Match( bytes, policy );
			Assert.True( wr.IsSuccess ); Assert.True( br.IsSuccess );
			Assert.Equal( wr.Match is not null, br.Match is not null );
			if ( wr.Match is null ) { continue; }
			Assert.Equal( Encoding.UTF8.GetString( br.Match!.MarkName.Span ), wr.Match.MarkName );
			Assert.Equal( br.Match.Captures.Count, wr.Match.Captures.Count );
			for ( var number = 0; number < wr.Match.Captures.Count; number++ ) {
				if ( br.Match.Captures[number] is not { } bc ) { Assert.Null( wr.Match.Captures[number] ); continue; }
				var wc = Assert.IsType<PcreWordCapture>( wr.Match.Captures[number] );
				Assert.Equal( Encoding.UTF8.GetString( bytes.AsSpan( bc.ByteIndex, bc.ByteLength ) ), subject.Substring( wc.CodeUnitIndex, wc.CodeUnitLength ) );
			}
		}
	}

	[Theory]
	[InlineData( "(*F)", "", 2 )]
	[InlineData( "(*FAIL:)", "", 2 )]
	[InlineData( "(*FAIL:é)", "", 3 )]
	[InlineData( "(*FAIL:\0)", "", 3 )]
	[InlineData( "(*MARK:é)", "", 3 )]
	[InlineData( "(*MARK:é)a", "a", 4 )]
	[InlineData( "(*MARK:A)(*:B)a", "a", 5 )]
	[InlineData( "(*MARK:A)(?!(*FAIL:B))a", "a", 9 )]
	public void LabelsAndFailureChargeExactWork( string source, string subject, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add, Policy = new() { Anchored = true }
			} );
			Assert.True( compiled.IsSuccess ); var result = compiled.Pattern!.Match( subject.AsMemory() );
			if ( budget < required ) {
				Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); Assert.Null( result.Match );
			} else {
				Assert.True( result.IsSuccess );
				var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), false, 0, 1 );
				Assert.Equal( native.Captures, Spans( result.Match ) ); Assert.Equal( native.Mark, MarkHex( result.Match ) );
			}
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Theory]
	[InlineData( "(*MARK:A)(?:(a(*MARK:B))x|ab(*MARK:C))", "ab" )]
	[InlineData( "(*MARK:A)(?(?!(a(*MARK:B)))X|a)", "a" )]
	[InlineData( "(*MARK:A)(?!(a(*FAIL:B)))a", "a" )]
	public void CancellationAtEveryTransitionLeavesThePatternReusable( string source, string subject ) {
		var trace = new List<int>();
		var baseline = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { WorkObserver = trace.Add } );
		Assert.True( baseline.IsSuccess ); var expected = baseline.Pattern!.Match( subject.AsMemory() ); Assert.True( expected.IsSuccess );
		Assert.Equal( Enumerable.Range( 1, trace.Count ), trace );
		for ( var cancelAt = 1; cancelAt <= trace.Count; cancelAt++ ) {
			using var cancellation = new CancellationTokenSource();
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				WorkObserver = work => { if ( work == cancelAt ) { cancellation.Cancel(); } }
			} );
			Assert.True( compiled.IsSuccess );
			Assert.Throws<OperationCanceledException>( () => compiled.Pattern!.Match( subject.AsMemory(), cancellationToken: cancellation.Token ) );
			var reused = compiled.Pattern!.Match( subject.AsMemory() ); Assert.True( reused.IsSuccess );
			Assert.Equal( Spans( expected.Match ), Spans( reused.Match ) ); Assert.Equal( MarkHex( expected.Match ), MarkHex( reused.Match ) );
			Assert.Throws<OperationCanceledException>( () => PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), cancellationToken: cancellation.Token ) );
		}
	}

	[Fact]
	public void LabelsDoNotConsumeSavedFramesAndAreRestoredAtFrameBoundaries() {
		const string boundary = "(*MARK:A)(?:(?:a??(*MARK:B)){32}){32}";
		var compiled = PcreWordSequenceCompiler.CompileSequence( boundary.AsMemory() ); Assert.True( compiled.IsSuccess );
		var result = compiled.Pattern!.Match( ReadOnlyMemory<char>.Empty ); Assert.True( result.IsSuccess ); Assert.Equal( "B", result.Match!.MarkName );
		var overflow = PcreWordSequenceCompiler.CompileSequence( String.Concat( boundary, "a??" ).AsMemory() ); Assert.True( overflow.IsSuccess );
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded, overflow.Pattern!.Match( ReadOnlyMemory<char>.Empty ).Diagnostic?.Code );
		var pruned = PcreWordSequenceCompiler.CompileSequence( "(*MARK:A)(?:(?>a??(*MARK:B))){64}".AsMemory() ); Assert.True( pruned.IsSuccess );
		Assert.Equal( "B", pruned.Pattern!.Match( ReadOnlyMemory<char>.Empty ).Match!.MarkName );
		var many = PcreWordSequenceCompiler.CompileSequence( "(?:(?:(*MARK:é😀)){64}){64}".AsMemory() ); Assert.True( many.IsSuccess );
		Assert.Equal( "é😀", many.Pattern!.Match( ReadOnlyMemory<char>.Empty ).Match!.MarkName );
	}

	[Theory]
	[InlineData( "(*MARK)", 6 )]
	[InlineData( "(*MARK:)", 7 )]
	[InlineData( "(*:)", 3 )]
	[InlineData( "(*MARK", 6 )]
	[InlineData( "(*MARK:x", 8 )]
	[InlineData( "(*FAIL", 1 )]
	[InlineData( "(*FAIL:x", 8 )]
	[InlineData( "(*MARK:x)*", 9 )]
	[InlineData( "(*F){0}", 4 )]
	[InlineData( "(*FAIL:é)+", 9 )]
	[InlineData( "(*MARK:x)(?#x)+", 14 )]
	[InlineData( @"(*MARK:x)\k<missing>", 9 )]
	public void MalformedHeadersKeepOriginalWordOffsets( string suffix, int offset ) {
		const string prefix = "é😀"; var source = String.Concat( prefix, suffix );
		Assert.Equal( "E", PcreNative16Oracle.ExecuteSequence( Words( source ), Array.Empty<ushort>(), true, 0 ).Kind );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidSyntax, prefix.Length + offset ),
			PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Diagnostic );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void WordLabelsExceedByteLimitsAndRetainOriginalUnits( bool utf ) {
		foreach ( var header in new[] { "(*MARK:", "(*:", "(*FAIL:", "(*F:" } ) {
			foreach ( var label in new[] { new string( 'é', 255 ), new string( 'é', 256 ),
				String.Concat( String.Concat( Enumerable.Repeat( "😀", 127 ) ), "X" ),
				String.Concat( Enumerable.Repeat( "😀", 128 ) ), String.Concat( "\0", new string( 'x', 255 ) ) } ) {
				var source = String.Concat( header, label, ")a" );
				var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( "a" ), utf, 0 );
				var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf } );
				Assert.True( compiled.IsSuccess ); var result = compiled.Pattern!.Match( "a".AsMemory() ); Assert.True( result.IsSuccess );
				Assert.True( native.Kind is "M" or "N" );
				Assert.Equal( native.Captures, Spans( result.Match ) ); Assert.Equal( native.Mark, MarkHex( result.Match ) );
			}
		}
	}

	[Fact]
	public void WordLabelLengthUsesTheWholePatternBudgetWithoutChangingTheByteLimit() {
		var wordOnly = String.Concat( "(*MARK:", new string( 'x', 256 ), ")a" );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( wordOnly.AsMemory() ).IsSuccess );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( wordOnly ) ).Diagnostic?.Code );
		var label = new string( 'é', 4088 ); var atLimit = String.Concat( "(*MARK:", label, ")" );
		Assert.Equal( 4096, atLimit.Length );
		var native = PcreNative16Oracle.ExecuteSequence( Words( atLimit ), Array.Empty<ushort>(), true, 0 );
		Assert.Equal( "M", native.Kind ); Assert.Equal( Hex( label ), native.Mark );
		var compiled = PcreWordSequenceCompiler.CompileSequence( atLimit.AsMemory(), new() { Utf16 = true, MatchWorkLimit = 3 } );
		Assert.True( compiled.IsSuccess ); Assert.Equal( label, compiled.Pattern!.Match( ReadOnlyMemory<char>.Empty ).Match!.MarkName );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.PatternLengthLimitExceeded, 0 ),
			PcreWordSequenceCompiler.CompileSequence( String.Concat( atLimit, "a" ).AsMemory() ).Diagnostic );
		Assert.Equal( PcreWordDiagnosticCode.PatternLengthLimitExceeded,
			PcreWordSequenceCompiler.CompileSequence( atLimit.AsMemory(), new() { MaxPatternLength = 4095 } ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( "(?<=(*F)(?<=a*))X" )]
	[InlineData( "(?<!(*F)(?=(?<=a*)))X" )]
	[InlineData( "(?<=(*F:N)(?<=a*))X" )]
	[InlineData( "(?<!(*F:N)(?=(?<=a*)))X" )]
	public void NativeSuppressedBehindWidthsRemainOutsideTheBoundedWordProfile( string source ) {
		foreach ( var utf in new[] { false, true } ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( "X" ), utf, 0 );
			Assert.True( native.Kind is "M" or "N" );
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf } ).Diagnostic?.Code );
		}
	}

	[Fact]
	public void IgnoredTailsKeepSourceAdmissionAndStrictValidation() {
		foreach ( var source in new[] { "(*F)(*COMMIT)", "(*F)(*PRUNE)", "(?(DEFINE)(*MARK:x)(*SKIP))",
			"(*MARK:x)(?<=a*)", "(?<=(*MARK:x)a*)b", "(?<=a*(*F))b", "(*F)a{65}" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax, PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
		foreach ( var header in new[] { "(*MARK:", "(*FAIL:" } ) {
			var source = String.Concat( header, "a\0\uD800)b" );
			Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Pattern, header.Length + 2 ),
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Diagnostic );
			Assert.True( PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).IsSuccess );
		}
		var behind = PcreWordSequenceCompiler.CompileSequence( "(*F)(?<=a)b|b".AsMemory(), new() { Utf16 = true } ); Assert.True( behind.IsSuccess );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 0 ), behind.Pattern!.Match( "\uD800b".AsMemory(), 1 ).Diagnostic );
	}

	[Fact]
	public async Task CompiledLabelsAndPriorResultsOwnTheirDataAcrossConcurrentMatches() {
		const string text = "(?<x>a)(*MARK:é😀)|b(*MARK:other)";
		var source = text.ToCharArray(); var compiled = PcreWordSequenceCompiler.CompileSequence( source, new() { Utf16 = true } ); Assert.True( compiled.IsSuccess );
		Array.Fill( source, 'z' );
		var first = compiled.Pattern!.Match( "!zax!".AsMemory( 1, 3 ) ).Match!;
		Assert.Equal( "1:1,1:1", Spans( first ) ); Assert.Equal( "é😀", first.MarkName );
		await Task.WhenAll( Enumerable.Range( 0, 64 ).Select( index => Task.Run( () => {
			var subject = index % 3 == 0 ? "a" : index % 3 == 1 ? "b" : "x";
			var result = compiled.Pattern.Match( subject.AsMemory() ); Assert.True( result.IsSuccess );
			Assert.Equal( subject == "a" ? "é😀" : subject == "b" ? "other" : null, result.Match?.MarkName );
		} ) ) );
		Assert.Equal( "é😀", first.MarkName ); Assert.Equal( new PcreWordCapture( 1, 1 ), first.GetCapture( "x" ) );
		Assert.Equal( String.Empty, PcreWordLiteralCompiler.CompileLiteral( "a".AsMemory() ).Pattern!.Match( "a".AsMemory() ).Match!.MarkName );
	}

	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( c => c is { } span ? $"{span.CodeUnitIndex}:{span.CodeUnitLength}" : "-" ) );
	private static string MarkHex( PcreWordMatch? match ) => match is null ? "" : Hex( match.MarkName );
	private static string Hex( string value ) => String.Concat( value.Select( unit => ( (ushort)unit ).ToString( "X4" ) ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
