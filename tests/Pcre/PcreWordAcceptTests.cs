using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordAcceptTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new (string Source, string Subject, int Start, bool Utf, bool Ucp)[] {
				("(*ACCEPT)b", "a", 0, false, false),
				("(*ACCEPT)b", "a", 0, true, false),
				("a(*ACCEPT)b", "ab", 0, false, false),
				("a(*ACCEPT)b", "ab", 0, true, false),
				("A((?:A|B(*ACCEPT)|C)D)", "AB", 0, false, false),
				("A((?:A|B(*ACCEPT)|C)D)", "AB", 0, true, false),
				("A((?:A|B(*ACCEPT)|C)D)", "AAD", 0, false, false),
				("A((?:A|B(*ACCEPT)|C)D)", "AAD", 0, true, false),
				("(a(b(*ACCEPT)c)d)e", "ab", 0, false, false),
				("(a(b(*ACCEPT)c)d)e", "ab", 0, true, false),
				("(?|(a(*ACCEPT)b)|(c))d", "a", 0, false, false),
				("(?|(a(*ACCEPT)b)|(c))d", "a", 0, true, false),
				("(?n)(a)(?<x>b(*ACCEPT)c)", "ab", 0, false, false),
				("(?n)(a)(?<x>b(*ACCEPT)c)", "ab", 0, true, false),
				("(?i)(?<x>É(*ACCEPT)z)", "é", 0, false, false),
				("(?i)(?<x>É(*ACCEPT)z)", "é", 0, true, false),
				("(?x)(a (*ACCEPT) # tail\n b)", "a", 0, false, false),
				("(?x)(a (*ACCEPT) # tail\n b)", "a", 0, true, false),
				("\\Q(*ACCEPT)\\E", "(*ACCEPT)", 0, false, false),
				("\\Q(*ACCEPT)\\E", "(*ACCEPT)", 0, true, false),
				("[(*ACCEPT)]", "A", 0, false, false),
				("[(*ACCEPT)]", "A", 0, true, false),
				("(a(*ACCEPT)??b)c", "abc", 0, false, false),
				("(a(*ACCEPT)??b)c", "abc", 0, true, false),
				("(a(*ACCEPT)??b)c", "ax", 0, false, false),
				("(a(*ACCEPT)??b)c", "ax", 0, true, false),
				("(a(*ACCEPT)??b)c", "abx", 0, false, false),
				("(a(*ACCEPT)??b)c", "abx", 0, true, false),
				("(a(*ACCEPT)?b)c", "abc", 0, false, false),
				("(a(*ACCEPT)?b)c", "abc", 0, true, false),
				("(?U)(a(*ACCEPT)?b)c", "abc", 0, false, false),
				("(?U)(a(*ACCEPT)?b)c", "abc", 0, true, false),
				("(?U)(a(*ACCEPT)??b)c", "abc", 0, false, false),
				("(?U)(a(*ACCEPT)??b)c", "abc", 0, true, false),
				("a(*ACCEPT){0}b", "ab", 0, false, false),
				("a(*ACCEPT){0}b", "ab", 0, true, false),
				("a(*ACCEPT){0,2}?b", "ax", 0, false, false),
				("a(*ACCEPT){0,2}?b", "ax", 0, true, false),
				("a(*ACCEPT){2}b", "a", 0, false, false),
				("a(*ACCEPT){2}b", "a", 0, true, false),
				("a(*ACCEPT)*b", "a", 0, false, false),
				("a(*ACCEPT)*b", "a", 0, true, false),
				("a(*ACCEPT)++b", "a", 0, false, false),
				("a(*ACCEPT)++b", "a", 0, true, false),
				("a(*ACCEPT)?+b", "a", 0, false, false),
				("a(*ACCEPT)?+b", "a", 0, true, false),
				("(a(*ACCEPT)??b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT)??b)c|abd", "abd", 0, true, false),
				("(a(*ACCEPT)b)|c", "c", 0, false, false),
				("(a(*ACCEPT)b)|c", "c", 0, true, false),
				("(?=(a(*ACCEPT)b))\\1", "a", 0, false, false),
				("(?=(a(*ACCEPT)b))\\1", "a", 0, true, false),
				("(?!(a(*ACCEPT)b))a|a", "a", 0, false, false),
				("(?!(a(*ACCEPT)b))a|a", "a", 0, true, false),
				("(a(?=(b(*ACCEPT)c))b)", "ab", 0, false, false),
				("(a(?=(b(*ACCEPT)c))b)", "ab", 0, true, false),
				("(?=(a(?=(b(*ACCEPT)c))b(*ACCEPT)c))ab", "ab", 0, false, false),
				("(?=(a(?=(b(*ACCEPT)c))b(*ACCEPT)c))ab", "ab", 0, true, false),
				("(?(?=(a(*ACCEPT)b))\\1|x)", "a", 0, false, false),
				("(?(?=(a(*ACCEPT)b))\\1|x)", "a", 0, true, false),
				("(?(?!(a(*ACCEPT)b))x|\\1)", "a", 0, false, false),
				("(?(?!(a(*ACCEPT)b))x|\\1)", "a", 0, true, false),
				("(?(?=(a(*ACCEPT)b))X|a)|a", "a", 0, false, false),
				("(?(?=(a(*ACCEPT)b))X|a)|a", "a", 0, true, false),
				("(?=(a(*ACCEPT)b))X|a", "a", 0, false, false),
				("(?=(a(*ACCEPT)b))X|a", "a", 0, true, false),
				("(?*(a(*ACCEPT)|ab))\\1c", "abc", 0, false, false),
				("(?*(a(*ACCEPT)|ab))\\1c", "abc", 0, true, false),
				("(?*(a|ab)(*ACCEPT))\\1c", "abc", 0, false, false),
				("(?*(a|ab)(*ACCEPT))\\1c", "abc", 0, true, false),
				("(?<=(a(*ACCEPT)z))x", "ax", 0, false, false),
				("(?<=(a(*ACCEPT)z))x", "ax", 0, true, false),
				("(?<=(a(*ACCEPT)b))x", "abx", 0, false, false),
				("(?<=(a(*ACCEPT)b))x", "abx", 0, true, false),
				("(?<=(*ACCEPT)a*)x", "x", 0, false, false),
				("(?<=(*ACCEPT)a*)x", "x", 0, true, false),
				("(?<=(?:a(*ACCEPT))b)x", "abx", 0, false, false),
				("(?<=(?:a(*ACCEPT))b)x", "abx", 0, true, false),
				("(?<!(a(*ACCEPT)b))x|x", "ax", 0, false, false),
				("(?<!(a(*ACCEPT)b))x|x", "ax", 0, true, false),
				("(?<=(a{1,2}(*ACCEPT)b))x", "aax", 0, false, false),
				("(?<=(a{1,2}(*ACCEPT)b))x", "aax", 0, true, false),
				("\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT)|ab))", "abc", 0, false, false),
				("\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT)|ab))", "abc", 0, true, false),
				("\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT)|ab))", "ac", 0, false, false),
				("\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT)|ab))", "ac", 0, true, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT)|ab))", "abc", 0, false, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT)|ab))", "abc", 0, true, false),
				("\\A(?=(?&x))ac(?(DEFINE)(?<x>a(*ACCEPT)b))", "ac", 0, false, false),
				("\\A(?=(?&x))ac(?(DEFINE)(?<x>a(*ACCEPT)b))", "ac", 0, true, false),
				("\\A(?&x)c(?(DEFINE)(?<x>(?=(a(*ACCEPT)b))a))", "ac", 0, false, false),
				("\\A(?&x)c(?(DEFINE)(?<x>(?=(a(*ACCEPT)b))a))", "ac", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(?&x)?(*ACCEPT)b))(?&x)c", "aac", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(?&x)?(*ACCEPT)b))(?&x)c", "aac", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(?&y)z)(?<y>b(*ACCEPT)c))(?&x)d", "abzd", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(?&y)z)(?<y>b(*ACCEPT)c))(?&x)d", "abzd", 0, true, false),
				("(?>(*ACCEPT))X", "a", 0, false, false),
				("(?>(*ACCEPT))X", "a", 0, true, false),
				("(?=(?>(*ACCEPT))X)a", "a", 0, false, false),
				("(?=(?>(*ACCEPT))X)a", "a", 0, true, false),
				("(*ACCEPT:N)b", "a", 0, false, false),
				("(*ACCEPT:N)b", "a", 0, true, false),
				("a(*ACCEPT:N)b", "ab", 0, false, false),
				("a(*ACCEPT:N)b", "ab", 0, true, false),
				("A((?:A|B(*ACCEPT:N)|C)D)", "AB", 0, false, false),
				("A((?:A|B(*ACCEPT:N)|C)D)", "AB", 0, true, false),
				("A((?:A|B(*ACCEPT:N)|C)D)", "AAD", 0, false, false),
				("A((?:A|B(*ACCEPT:N)|C)D)", "AAD", 0, true, false),
				("(a(b(*ACCEPT:N)c)d)e", "ab", 0, false, false),
				("(a(b(*ACCEPT:N)c)d)e", "ab", 0, true, false),
				("(?|(a(*ACCEPT:N)b)|(c))d", "a", 0, false, false),
				("(?|(a(*ACCEPT:N)b)|(c))d", "a", 0, true, false),
				("(?n)(a)(?<x>b(*ACCEPT:N)c)", "ab", 0, false, false),
				("(?n)(a)(?<x>b(*ACCEPT:N)c)", "ab", 0, true, false),
				("(?i)(?<x>É(*ACCEPT:N)z)", "é", 0, false, false),
				("(?i)(?<x>É(*ACCEPT:N)z)", "é", 0, true, false),
				("(?x)(a (*ACCEPT:N) # tail\n b)", "a", 0, false, false),
				("(?x)(a (*ACCEPT:N) # tail\n b)", "a", 0, true, false),
				("\\Q(*ACCEPT:N)\\E", "(*ACCEPT)", 0, false, false),
				("\\Q(*ACCEPT:N)\\E", "(*ACCEPT)", 0, true, false),
				("[(*ACCEPT:N)]", "A", 0, false, false),
				("[(*ACCEPT:N)]", "A", 0, true, false),
				("(a(*ACCEPT:N)??b)c", "abc", 0, false, false),
				("(a(*ACCEPT:N)??b)c", "abc", 0, true, false),
				("(a(*ACCEPT:N)??b)c", "ax", 0, false, false),
				("(a(*ACCEPT:N)??b)c", "ax", 0, true, false),
				("(a(*ACCEPT:N)??b)c", "abx", 0, false, false),
				("(a(*ACCEPT:N)??b)c", "abx", 0, true, false),
				("(a(*ACCEPT:N)?b)c", "abc", 0, false, false),
				("(a(*ACCEPT:N)?b)c", "abc", 0, true, false),
				("(?U)(a(*ACCEPT:N)?b)c", "abc", 0, false, false),
				("(?U)(a(*ACCEPT:N)?b)c", "abc", 0, true, false),
				("(?U)(a(*ACCEPT:N)??b)c", "abc", 0, false, false),
				("(?U)(a(*ACCEPT:N)??b)c", "abc", 0, true, false),
				("a(*ACCEPT:N){0}b", "ab", 0, false, false),
				("a(*ACCEPT:N){0}b", "ab", 0, true, false),
				("a(*ACCEPT:N){0,2}?b", "ax", 0, false, false),
				("a(*ACCEPT:N){0,2}?b", "ax", 0, true, false),
				("a(*ACCEPT:N){2}b", "a", 0, false, false),
				("a(*ACCEPT:N){2}b", "a", 0, true, false),
				("a(*ACCEPT:N)*b", "a", 0, false, false),
				("a(*ACCEPT:N)*b", "a", 0, true, false),
				("a(*ACCEPT:N)++b", "a", 0, false, false),
				("a(*ACCEPT:N)++b", "a", 0, true, false),
				("a(*ACCEPT:N)?+b", "a", 0, false, false),
				("a(*ACCEPT:N)?+b", "a", 0, true, false),
				("(a(*ACCEPT:N)??b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:N)??b)c|abd", "abd", 0, true, false),
				("(a(*ACCEPT:N)b)|c", "c", 0, false, false),
				("(a(*ACCEPT:N)b)|c", "c", 0, true, false),
				("(?=(a(*ACCEPT:N)b))\\1", "a", 0, false, false),
				("(?=(a(*ACCEPT:N)b))\\1", "a", 0, true, false),
				("(?!(a(*ACCEPT:N)b))a|a", "a", 0, false, false),
				("(?!(a(*ACCEPT:N)b))a|a", "a", 0, true, false),
				("(a(?=(b(*ACCEPT:N)c))b)", "ab", 0, false, false),
				("(a(?=(b(*ACCEPT:N)c))b)", "ab", 0, true, false),
				("(?=(a(?=(b(*ACCEPT:N)c))b(*ACCEPT:N)c))ab", "ab", 0, false, false),
				("(?=(a(?=(b(*ACCEPT:N)c))b(*ACCEPT:N)c))ab", "ab", 0, true, false),
				("(?(?=(a(*ACCEPT:N)b))\\1|x)", "a", 0, false, false),
				("(?(?=(a(*ACCEPT:N)b))\\1|x)", "a", 0, true, false),
				("(?(?!(a(*ACCEPT:N)b))x|\\1)", "a", 0, false, false),
				("(?(?!(a(*ACCEPT:N)b))x|\\1)", "a", 0, true, false),
				("(?(?=(a(*ACCEPT:N)b))X|a)|a", "a", 0, false, false),
				("(?(?=(a(*ACCEPT:N)b))X|a)|a", "a", 0, true, false),
				("(?=(a(*ACCEPT:N)b))X|a", "a", 0, false, false),
				("(?=(a(*ACCEPT:N)b))X|a", "a", 0, true, false),
				("(?*(a(*ACCEPT:N)|ab))\\1c", "abc", 0, false, false),
				("(?*(a(*ACCEPT:N)|ab))\\1c", "abc", 0, true, false),
				("(?*(a|ab)(*ACCEPT:N))\\1c", "abc", 0, false, false),
				("(?*(a|ab)(*ACCEPT:N))\\1c", "abc", 0, true, false),
				("(?<=(a(*ACCEPT:N)z))x", "ax", 0, false, false),
				("(?<=(a(*ACCEPT:N)z))x", "ax", 0, true, false),
				("(?<=(a(*ACCEPT:N)b))x", "abx", 0, false, false),
				("(?<=(a(*ACCEPT:N)b))x", "abx", 0, true, false),
				("(?<=(*ACCEPT:N)a*)x", "x", 0, false, false),
				("(?<=(*ACCEPT:N)a*)x", "x", 0, true, false),
				("(?<=(?:a(*ACCEPT:N))b)x", "abx", 0, false, false),
				("(?<=(?:a(*ACCEPT:N))b)x", "abx", 0, true, false),
				("(?<!(a(*ACCEPT:N)b))x|x", "ax", 0, false, false),
				("(?<!(a(*ACCEPT:N)b))x|x", "ax", 0, true, false),
				("(?<=(a{1,2}(*ACCEPT:N)b))x", "aax", 0, false, false),
				("(?<=(a{1,2}(*ACCEPT:N)b))x", "aax", 0, true, false),
				("\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "abc", 0, false, false),
				("\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "abc", 0, true, false),
				("\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "ac", 0, false, false),
				("\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "ac", 0, true, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "abc", 0, false, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "abc", 0, true, false),
				("\\A(?=(?&x))ac(?(DEFINE)(?<x>a(*ACCEPT:N)b))", "ac", 0, false, false),
				("\\A(?=(?&x))ac(?(DEFINE)(?<x>a(*ACCEPT:N)b))", "ac", 0, true, false),
				("\\A(?&x)c(?(DEFINE)(?<x>(?=(a(*ACCEPT:N)b))a))", "ac", 0, false, false),
				("\\A(?&x)c(?(DEFINE)(?<x>(?=(a(*ACCEPT:N)b))a))", "ac", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(?&x)?(*ACCEPT:N)b))(?&x)c", "aac", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(?&x)?(*ACCEPT:N)b))(?&x)c", "aac", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(?&y)z)(?<y>b(*ACCEPT:N)c))(?&x)d", "abzd", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(?&y)z)(?<y>b(*ACCEPT:N)c))(?&x)d", "abzd", 0, true, false),
				("(?>(*ACCEPT:N))X", "a", 0, false, false),
				("(?>(*ACCEPT:N))X", "a", 0, true, false),
				("(?=(?>(*ACCEPT:N))X)a", "a", 0, false, false),
				("(?=(?>(*ACCEPT:N))X)a", "a", 0, true, false),
				("(*ACCEPT:N)", "a", 0, false, false),
				("(*ACCEPT:N)", "a", 0, true, false),
				("(*MARK:M)a(*ACCEPT:N)b", "ab", 0, false, false),
				("(*MARK:M)a(*ACCEPT:N)b", "ab", 0, true, false),
				("(*MARK:M)a(*ACCEPT:)b", "ab", 0, false, false),
				("(*MARK:M)a(*ACCEPT:)b", "ab", 0, true, false),
				("(*MARK:M)a(*ACCEPT:\0)b", "ab", 0, false, false),
				("(*MARK:M)a(*ACCEPT:\0)b", "ab", 0, true, false),
				("a(*ACCEPT:é漢)b", "ab", 0, false, false),
				("a(*ACCEPT:é漢)b", "ab", 0, true, false),
				("(?x)a (*ACCEPT:n # x) b", "ab", 0, false, false),
				("(?x)a (*ACCEPT:n # x) b", "ab", 0, true, false),
				("a(*ACCEPT:\\x41)b", "ab", 0, false, false),
				("a(*ACCEPT:\\x41)b", "ab", 0, true, false),
				("\\Q(*ACCEPT:N)\\E", "(*ACCEPT:N)", 0, false, false),
				("\\Q(*ACCEPT:N)\\E", "(*ACCEPT:N)", 0, true, false),
				("[(*ACCEPT:N)]", "N", 0, false, false),
				("[(*ACCEPT:N)]", "N", 0, true, false),
				("a(*ACCEPT:N)|ab", "ab", 0, false, false),
				("a(*ACCEPT:N)|ab", "ab", 0, true, false),
				("a(*MARK:M)(?!b(*ACCEPT:N))b", "ab", 0, false, false),
				("a(*MARK:M)(?!b(*ACCEPT:N))b", "ab", 0, true, false),
				("(?(?=a(*ACCEPT:N))a|b)", "ab", 0, false, false),
				("(?(?=a(*ACCEPT:N))a|b)", "ab", 0, true, false),
				("(?(?!a(*ACCEPT:N))b|a)", "ab", 0, false, false),
				("(?(?!a(*ACCEPT:N))b|a)", "ab", 0, true, false),
				("(?=(a(*ACCEPT:N)b))a|a", "a", 0, false, false),
				("(?=(a(*ACCEPT:N)b))a|a", "a", 0, true, false),
				("(?!(a(*ACCEPT:N)))a|a", "a", 0, false, false),
				("(?!(a(*ACCEPT:N)))a|a", "a", 0, true, false),
				("(?*a(*ACCEPT:N))a|a", "a", 0, false, false),
				("(?*a(*ACCEPT:N))a|a", "a", 0, true, false),
				("(?<=a(*ACCEPT:N))b|b", "ab", 0, false, false),
				("(?<=a(*ACCEPT:N))b|b", "ab", 0, true, false),
				("(?<!a(*ACCEPT:N))b", "ab", 0, false, false),
				("(?<!a(*ACCEPT:N))b", "ab", 0, true, false),
				("(?<=(*ACCEPT:N)a*)b", "b", 0, false, false),
				("(?<=(*ACCEPT:N)a*)b", "b", 0, true, false),
				("(?<!(*ACCEPT:N)a*)b", "b", 0, false, false),
				("(?<!(*ACCEPT:N)a*)b", "b", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(*ACCEPT:N)b))(?&x)c|ac", "ac", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(*ACCEPT:N)b))(?&x)c|ac", "ac", 0, true, false),
				("\\A(?(DEFINE)(?<x>a(?&x)?(*ACCEPT:N)b))(?&x)c|aac", "aac", 0, false, false),
				("\\A(?(DEFINE)(?<x>a(?&x)?(*ACCEPT:N)b))(?&x)c|aac", "aac", 0, true, false),
				("é(*ACCEPT:N)X|é", "é", 0, false, false),
				("é(*ACCEPT:N)X|é", "é", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)*b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)*b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)*b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)*b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)*?b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)*?b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)*?b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)*?b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)*+b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)*+b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)*+b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)*+b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)+b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)+b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)+b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)+b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)+?b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)+?b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)+?b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)+?b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)++b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)++b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)++b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)++b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)?b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)?b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)?b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)?b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)??b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)??b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)??b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)??b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)?+b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)?+b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N)?+b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N)?+b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){0}b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){0}b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){0}b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){0}b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){1}b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){1}b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){1}b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){1}b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){2}b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){2}b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){2}b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){2}b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){0,2}b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){0,2}b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){0,2}b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){0,2}b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){0,2}?b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){0,2}?b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){0,2}?b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){0,2}?b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){0,2}+b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){0,2}+b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){0,2}+b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){0,2}+b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){2,3}b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){2,3}b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){2,3}b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){2,3}b)c", "ax", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){2,3}?b)c", "abc", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){2,3}?b)c", "abc", 0, true, false),
				("(*MARK:M)(a(*ACCEPT:N){2,3}?b)c", "ax", 0, false, false),
				("(*MARK:M)(a(*ACCEPT:N){2,3}?b)c", "ax", 0, true, false),
				("(*MARK:M)(*ACCEPT:N){0}a", "abc", 0, false, false),
				("(*MARK:M)(*ACCEPT:N){0}a", "abc", 0, true, false),
				("(*MARK:M)(*ACCEPT:N)??a", "abc", 0, false, false),
				("(*MARK:M)(*ACCEPT:N)??a", "abc", 0, true, false),
				("(*MARK:M)(*ACCEPT:N)??b", "abc", 0, false, false),
				("(*MARK:M)(*ACCEPT:N)??b", "abc", 0, true, false),
				("a(*MARK:M)(?*b(*ACCEPT:N))bc", "abc", 0, false, false),
				("a(*MARK:M)(?*b(*ACCEPT:N))bc", "abc", 0, true, false),
				("a(*MARK:M)(?*(b(*ACCEPT:N)))bc", "abc", 0, false, false),
				("a(*MARK:M)(?*(b(*ACCEPT:N)))bc", "abc", 0, true, false),
				("(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "abc", 0, false, false),
				("(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "abc", 0, true, false),
				("(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*MARK:N)(*ACCEPT)|ab))", "abc", 0, false, false),
				("(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*MARK:N)(*ACCEPT)|ab))", "abc", 0, true, false),
				("(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*MARK:N))(*ACCEPT)))", "abc", 0, false, false),
				("(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*MARK:N))(*ACCEPT)))", "abc", 0, true, false),
				("(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*ACCEPT:N))))", "abc", 0, false, false),
				("(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*ACCEPT:N))))", "abc", 0, true, false),
				("(*MARK:M)\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "abc", 0, false, false),
				("(*MARK:M)\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "abc", 0, true, false),
				("\\A(?!(?&x)c)abc(?(DEFINE)(?<x>a(*ACCEPT:N)))", "abc", 0, false, false),
				("\\A(?!(?&x)c)abc(?(DEFINE)(?<x>a(*ACCEPT:N)))", "abc", 0, true, false),
				("\\A(?(?=(?&x)c)X|abc)(?(DEFINE)(?<x>a(*ACCEPT:N)))", "abc", 0, false, false),
				("\\A(?(?=(?&x)c)X|abc)(?(DEFINE)(?<x>a(*ACCEPT:N)))", "abc", 0, true, false),
				("(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?&y))(?<y>(*ACCEPT:N)))", "abc", 0, false, false),
				("(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?&y))(?<y>(*ACCEPT:N)))", "abc", 0, true, false),
				("(*ACCEPT)x", "a", 0, false, false),
				("(*ACCEPT)x", "a", 0, true, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT))a|b)", "a", 0, false, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT))a|b)", "a", 0, true, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT))b|a)", "a", 0, false, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT))b|a)", "a", 0, true, false),
				("(*ACCEPT:)x", "a", 0, false, false),
				("(*ACCEPT:)x", "a", 0, true, false),
				("(a(b(*ACCEPT:)c)d)e", "ab", 0, false, false),
				("(a(b(*ACCEPT:)c)d)e", "ab", 0, true, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:))a|b)", "a", 0, false, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:))a|b)", "a", 0, true, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:))b|a)", "a", 0, false, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:))b|a)", "a", 0, true, false),
				("(*ACCEPT:é😀)x", "a", 0, false, false),
				("(*ACCEPT:é😀)x", "a", 0, true, false),
				("(a(b(*ACCEPT:é😀)c)d)e", "ab", 0, false, false),
				("(a(b(*ACCEPT:é😀)c)d)e", "ab", 0, true, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:é😀))a|b)", "a", 0, false, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:é😀))a|b)", "a", 0, true, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:é😀))b|a)", "a", 0, false, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:é😀))b|a)", "a", 0, true, false),
				("(*ACCEPT:A\0tail)x", "a", 0, false, false),
				("(*ACCEPT:A\0tail)x", "a", 0, true, false),
				("(a(b(*ACCEPT:A\0tail)c)d)e", "ab", 0, false, false),
				("(a(b(*ACCEPT:A\0tail)c)d)e", "ab", 0, true, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:A\0tail))a|b)", "a", 0, false, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:A\0tail))a|b)", "a", 0, true, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:A\0tail))b|a)", "a", 0, false, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:A\0tail))b|a)", "a", 0, true, false),
				("(*ACCEPT:\0)x", "a", 0, false, false),
				("(*ACCEPT:\0)x", "a", 0, true, false),
				("(a(b(*ACCEPT:\0)c)d)e", "ab", 0, false, false),
				("(a(b(*ACCEPT:\0)c)d)e", "ab", 0, true, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:\0))a|b)", "a", 0, false, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:\0))a|b)", "a", 0, true, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:\0))b|a)", "a", 0, false, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:\0))b|a)", "a", 0, true, false),
				("(*ACCEPT:\\x41)x", "a", 0, false, false),
				("(*ACCEPT:\\x41)x", "a", 0, true, false),
				("(a(b(*ACCEPT:\\x41)c)d)e", "ab", 0, false, false),
				("(a(b(*ACCEPT:\\x41)c)d)e", "ab", 0, true, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:\\x41))a|b)", "a", 0, false, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:\\x41))a|b)", "a", 0, true, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:\\x41))b|a)", "a", 0, false, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:\\x41))b|a)", "a", 0, true, false),
				("(*ACCEPT:(x)x", "a", 0, false, false),
				("(*ACCEPT:(x)x", "a", 0, true, false),
				("(a(b(*ACCEPT:(x)c)d)e", "ab", 0, false, false),
				("(a(b(*ACCEPT:(x)c)d)e", "ab", 0, true, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:(x))a|b)", "a", 0, false, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:(x))a|b)", "a", 0, true, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:(x))b|a)", "a", 0, false, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:(x))b|a)", "a", 0, true, false),
				("(*ACCEPT:a # x )x", "a", 0, false, false),
				("(*ACCEPT:a # x )x", "a", 0, true, false),
				("(a(b(*ACCEPT:a # x )c)d)e", "ab", 0, false, false),
				("(a(b(*ACCEPT:a # x )c)d)e", "ab", 0, true, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:a # x ))a|b)", "a", 0, false, false),
				("(*MARK:old)(?(?=(a)(*ACCEPT:a # x ))a|b)", "a", 0, true, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:a # x ))b|a)", "a", 0, false, false),
				("(*MARK:old)(?(?!(a)(*ACCEPT:a # x ))b|a)", "a", 0, true, false),
				("(a(*ACCEPT:é)?b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é)?b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é)?b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é)?b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N)?b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N)?b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é)??b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é)??b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é)??b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é)??b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N)??b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N)??b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é)?+b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é)?+b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é)?+b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é)?+b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N)?+b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N)?+b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é)*b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é)*b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é)*b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é)*b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N)*b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N)*b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é)*?b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é)*?b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é)*?b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é)*?b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N)*?b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N)*?b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é)*+b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é)*+b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é)*+b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é)*+b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N)*+b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N)*+b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é)+b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é)+b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é)+b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é)+b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N)+b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N)+b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é)+?b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é)+?b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é)+?b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é)+?b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N)+?b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N)+?b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é)++b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é)++b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é)++b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é)++b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N)++b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N)++b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é){0}b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é){0}b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é){0}b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é){0}b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N){0}b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N){0}b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é){2}b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é){2}b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é){2}b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é){2}b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N){2}b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N){2}b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é){0,2}b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é){0,2}b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é){0,2}b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é){0,2}b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N){0,2}b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N){0,2}b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é){0,2}?b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é){0,2}?b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é){0,2}?b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é){0,2}?b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N){0,2}?b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N){0,2}?b))a", "ab", 0, true, false),
				("(a(*ACCEPT:é){1,3}+b)c|abd", "abc", 0, false, false),
				("(a(*ACCEPT:é){1,3}+b)c|abd", "abc", 0, true, false),
				("(a(*ACCEPT:é){1,3}+b)c|abd", "abd", 0, false, false),
				("(a(*ACCEPT:é){1,3}+b)c|abd", "abd", 0, true, false),
				("(?=(a(*ACCEPT:N){1,3}+b))a", "ab", 0, false, false),
				("(?=(a(*ACCEPT:N){1,3}+b))a", "ab", 0, true, false),
				("(?<*(a(*ACCEPT:N)|ab))c", "aab", 0, false, false),
				("(?<*(a(*ACCEPT:N)|ab))c", "aab", 0, true, false),
				("(?<*(a{1,2}(*ACCEPT:N)))b", "aab", 0, false, false),
				("(?<*(a{1,2}(*ACCEPT:N)))b", "aab", 0, true, false),
				("(*MARK:A)(?(?=(a)(*ACCEPT:N))a|b)(?*(*ACCEPT)){0}", "a", 0, false, false),
				("(*MARK:A)(?(?=(a)(*ACCEPT:N))a|b)(?*(*ACCEPT)){0}", "a", 0, true, false),
				("(*MARK:A)(?(?=(a)(*ACCEPT:N))a|b)(?(DEFINE)(?<x>(?*(*ACCEPT))))", "a", 0, false, false),
				("(*MARK:A)(?(?=(a)(*ACCEPT:N))a|b)(?(DEFINE)(?<x>(?*(*ACCEPT))))", "a", 0, true, false),
				("(*MARK:A)(?(?=(a)(*ACCEPT:N))a|b)(?(VERSION>=0)(?*(*ACCEPT)){0}|x)", "a", 0, false, false),
				("(*MARK:A)(?(?=(a)(*ACCEPT:N))a|b)(?(VERSION>=0)(?*(*ACCEPT)){0}|x)", "a", 0, true, false),
				("(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*ACCEPT:N)))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*ACCEPT:N)))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(?=(*ACCEPT:N)))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(?=(*ACCEPT:N)))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*ACCEPT:N))(*ACCEPT))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*ACCEPT:N))(*ACCEPT))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(?=(*ACCEPT:N))(*ACCEPT))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(?=(*ACCEPT:N))(*ACCEPT))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?&y)(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?&y)(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(?&y)(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(?&y)(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?&y))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?&y))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(?&y))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("\\A(?&x)c|abc(?(DEFINE)(?<x>a(?&y))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(*MARK:A)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(*MARK:A)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(*MARK:A)\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(*MARK:A)\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(*MARK:A)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*ACCEPT:N)))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(*MARK:A)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*ACCEPT:N)))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(*MARK:A)\\A(?&x)c|abc(?(DEFINE)(?<x>a(?=(*ACCEPT:N)))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(*MARK:A)\\A(?&x)c|abc(?(DEFINE)(?<x>a(?=(*ACCEPT:N)))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(*MARK:A)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*ACCEPT:N))(*ACCEPT))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(*MARK:A)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?=(*ACCEPT:N))(*ACCEPT))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(*MARK:A)\\A(?&x)c|abc(?(DEFINE)(?<x>a(?=(*ACCEPT:N))(*ACCEPT))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(*MARK:A)\\A(?&x)c|abc(?(DEFINE)(?<x>a(?=(*ACCEPT:N))(*ACCEPT))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(*MARK:A)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?&y)(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(*MARK:A)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?&y)(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(*MARK:A)\\A(?&x)c|abc(?(DEFINE)(?<x>a(?&y)(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(*MARK:A)\\A(?&x)c|abc(?(DEFINE)(?<x>a(?&y)(*ACCEPT:N))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(*MARK:A)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?&y))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(*MARK:A)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(?&y))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(*MARK:A)\\A(?&x)c|abc(?(DEFINE)(?<x>a(?&y))(?<y>(*ACCEPT:Y)))", "abc", 0, false, false),
				("(*MARK:A)\\A(?&x)c|abc(?(DEFINE)(?<x>a(?&y))(?<y>(*ACCEPT:Y)))", "abc", 0, true, false),
				("(*ACCEPT)|ab", "", 0, false, false),
				("(*ACCEPT)|ab", "", 0, true, false),
				("(*ACCEPT)|ab", "ab", 0, false, false),
				("(*ACCEPT)|ab", "ab", 0, true, false),
				("(*ACCEPT)|ab", "xab", 0, false, false),
				("(*ACCEPT)|ab", "xab", 0, true, false),
				("a(*ACCEPT)|ab", "", 0, false, false),
				("a(*ACCEPT)|ab", "", 0, true, false),
				("a(*ACCEPT)|ab", "ab", 0, false, false),
				("a(*ACCEPT)|ab", "ab", 0, true, false),
				("a(*ACCEPT)|ab", "xab", 0, false, false),
				("a(*ACCEPT)|ab", "xab", 0, true, false),
				("(?:a?(*ACCEPT)|ab)", "", 0, false, false),
				("(?:a?(*ACCEPT)|ab)", "", 0, true, false),
				("(?:a?(*ACCEPT)|ab)", "ab", 0, false, false),
				("(?:a?(*ACCEPT)|ab)", "ab", 0, true, false),
				("(?:a?(*ACCEPT)|ab)", "xab", 0, false, false),
				("(?:a?(*ACCEPT)|ab)", "xab", 0, true, false),
				("(a?(*ACCEPT:N))|ab", "", 0, false, false),
				("(a?(*ACCEPT:N))|ab", "", 0, true, false),
				("(a?(*ACCEPT:N))|ab", "ab", 0, false, false),
				("(a?(*ACCEPT:N))|ab", "ab", 0, true, false),
				("(a?(*ACCEPT:N))|ab", "xab", 0, false, false),
				("(a?(*ACCEPT:N))|ab", "xab", 0, true, false),
				("a{0,2}?(*ACCEPT:N)|ab", "", 0, false, false),
				("a{0,2}?(*ACCEPT:N)|ab", "", 0, true, false),
				("a{0,2}?(*ACCEPT:N)|ab", "ab", 0, false, false),
				("a{0,2}?(*ACCEPT:N)|ab", "ab", 0, true, false),
				("a{0,2}?(*ACCEPT:N)|ab", "xab", 0, false, false),
				("a{0,2}?(*ACCEPT:N)|ab", "xab", 0, true, false),
				("(?>(a?(*ACCEPT:N)))|ab", "", 0, false, false),
				("(?>(a?(*ACCEPT:N)))|ab", "", 0, true, false),
				("(?>(a?(*ACCEPT:N)))|ab", "ab", 0, false, false),
				("(?>(a?(*ACCEPT:N)))|ab", "ab", 0, true, false),
				("(?>(a?(*ACCEPT:N)))|ab", "xab", 0, false, false),
				("(?>(a?(*ACCEPT:N)))|ab", "xab", 0, true, false),
				("(?<x>é😀)(*ACCEPT:é😀)x", "é😀", 0, false, true),
				("(?<x>é😀)(*ACCEPT:é😀)x", "é😀", 0, true, true),
				("(?i)(?<x>é)(*ACCEPT:漢)z", "É", 0, false, true),
				("(?i)(?<x>é)(*ACCEPT:漢)z", "É", 0, true, true),
				("(?<x>😀(*ACCEPT:label)x)", "aabc", 0, false, true),
				("(?<x>😀(*ACCEPT:label)x)", "aabc", 0, true, true),
				("(*CR)(?m)^(a(*ACCEPT:N))", "\ra", 0, false, true),
				("(*CR)(?m)^(a(*ACCEPT:N))", "\ra", 0, true, true),
				("(*NUL)(?m)^(a(*ACCEPT:N))", "\0a", 0, false, true),
				("(*NUL)(?m)^(a(*ACCEPT:N))", "\0a", 0, true, true),
				("(?<=(a{1,2}(*ACCEPT:N)))b", "aabc", 0, false, true),
				("(?<=(a{1,2}(*ACCEPT:N)))b", "aabc", 0, true, true),
				("(?<=(a(*ACCEPT:N))b)c", "aabc", 0, false, true),
				("(?<=(a(*ACCEPT:N))b)c", "aabc", 0, true, true),
				("(?<!(a(*ACCEPT:N)))b|b", "aabc", 0, false, true),
				("(?<!(a(*ACCEPT:N)))b|b", "aabc", 0, true, true),
				("(?<x>😀(*ACCEPT:😀)x)", "😀x", 0, true, false),
				("(?<x>\ud800)(*ACCEPT:\udfff)z", "\ud800z", 0, false, false),
				("(?<x>\udfff)(*ACCEPT:\ud800)z", "\udfffz", 0, false, false),
				("(*ACCEPT:N)x", "aab", 0, false, false),
				("(*ACCEPT:N)x", "aab", 0, true, false),
				("(*ACCEPT:N)x", "aab", 1, false, false),
				("(*ACCEPT:N)x", "aab", 1, true, false),
				("(*ACCEPT:N)x", "aab", 2, false, false),
				("(*ACCEPT:N)x", "aab", 2, true, false),
				("a(*ACCEPT:N)x", "aab", 0, false, false),
				("a(*ACCEPT:N)x", "aab", 0, true, false),
				("a(*ACCEPT:N)x", "aab", 1, false, false),
				("a(*ACCEPT:N)x", "aab", 1, true, false),
				("a(*ACCEPT:N)x", "aab", 2, false, false),
				("a(*ACCEPT:N)x", "aab", 2, true, false),
				("(?<=a)(*ACCEPT:N)x", "aab", 0, false, false),
				("(?<=a)(*ACCEPT:N)x", "aab", 0, true, false),
				("(?<=a)(*ACCEPT:N)x", "aab", 1, false, false),
				("(?<=a)(*ACCEPT:N)x", "aab", 1, true, false),
				("(?<=a)(*ACCEPT:N)x", "aab", 2, false, false),
				("(?<=a)(*ACCEPT:N)x", "aab", 2, true, false),
				("(?=(a(*ACCEPT:N)))a", "aab", 0, false, false),
				("(?=(a(*ACCEPT:N)))a", "aab", 0, true, false),
				("(?=(a(*ACCEPT:N)))a", "aab", 1, false, false),
				("(?=(a(*ACCEPT:N)))a", "aab", 1, true, false),
				("(?=(a(*ACCEPT:N)))a", "aab", 2, false, false),
				("(?=(a(*ACCEPT:N)))a", "aab", 2, true, false),
				("(*ACCEPT:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa)x", "a", 0, false, false),
				("(*ACCEPT:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa)x", "a", 0, true, false),
				("(*ACCEPT:éééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééé)x", "a", 0, false, false),
				("(*ACCEPT:éééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééé)x", "a", 0, true, false),
				("(*MARK:M)a{0,2}(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, false, false),
				("(*MARK:M)a{0,2}(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, true, false),
				("(*MARK:N)a{0,2}(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, false, false),
				("(*MARK:N)a{0,2}(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, true, false),
				("(*MARK:M)a{0,2}?(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, false, false),
				("(*MARK:M)a{0,2}?(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, true, false),
				("(*MARK:N)a{0,2}?(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, false, false),
				("(*MARK:N)a{0,2}?(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, true, false),
				("(*MARK:M)a?(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, false, false),
				("(*MARK:M)a?(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, true, false),
				("(*MARK:N)a?(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, false, false),
				("(*MARK:N)a?(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, true, false),
				("(*MARK:M)a??(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, false, false),
				("(*MARK:M)a??(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, true, false),
				("(*MARK:N)a??(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, false, false),
				("(*MARK:N)a??(?:(?&x)c|ab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, true, false),
				("(*MARK:M)(?:(*MARK:N)a{0,2}(?:(?&x)c|ab)|aab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, false, false),
				("(*MARK:M)(?:(*MARK:N)a{0,2}(?:(?&x)c|ab)|aab)(?(DEFINE)(?<x>b(*ACCEPT:N)))", "aab", 0, true, false),
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
	public void AcceptanceScopesCapturesAndLabelsAgreeWithEveryNativePolicy( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"accept {id}: {compiled.Diagnostic}" );
		var patternUnits = Words( source ); var subjectUnits = Words( subject );
		var nativePolicies = PcreNative16Oracle.ExecuteSequencePolicies( patternUnits, subjectUnits, utf, (uint)start, defaults, ucp );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = nativePolicies[mask];
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			Assert.True( native.Captures == Spans( result.Match ) && native.Mark == MarkHex( result.Match ),
				$"accept {id}/{defaults}/{mask}: expected {native.Captures}|{native.Mark}, actual {Spans( result.Match )}|{MarkHex( result.Match )}" );
			Assert.Equal( native.Kind == "M", result.Match is not null );
		}
	}

	[Theory]
	[InlineData( "(*ACCEPT)", "a", "0:0", "" )]
	[InlineData( "(a(b(*ACCEPT:é😀)x)y)z", "ab", "0:2,0:2,1:1", "é😀" )]
	[InlineData( "(*MARK:before)(*ACCEPT:)", "", "0:0", "before" )]
	[InlineData( "(*MARK:before)(*ACCEPT:\0tail)", "", "0:0", "" )]
	[InlineData( "(?=(a(*ACCEPT:N)b))a", "a", "0:1,0:1", "N" )]
	[InlineData( "(?!(a(*ACCEPT:N)b))a|a", "a", "0:1,-", "" )]
	[InlineData( "\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT:N)b))", "ac", "0:2,-", "N" )]
	[InlineData( "\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "abc", "0:3,-", "N" )]
	[InlineData( "(*MARK:M)\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "abc", "0:3,-", "" )]
	public void AcceptanceHasExplicitOriginalUnitResults( string source, string subject, string spans, string mark ) {
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 0 );
		Assert.Equal( spans, native.Captures ); Assert.Equal( Hex( mark ), native.Mark );
		var result = Compile( source, new() { Utf16 = true } ).Match( subject.AsMemory() );
		Assert.True( result.IsSuccess ); Assert.Equal( spans, Spans( result.Match ) ); Assert.Equal( mark, result.Match!.MarkName );
	}

	[Theory]
	[InlineData( "(?*(*ACCEPT)){0}" )]
	[InlineData( "(?(DEFINE)(?<x>(?*(*ACCEPT))))" )]
	[InlineData( "(?(VERSION>=0)|(?*(*ACCEPT)))" )]
	[InlineData( "(?<*(*ACCEPT)){0}" )]
	public void ErasedLexicalAcceptStillSelectsNativeInterpreterSemantics( string suffix ) {
		const string prefix = "(*MARK:before)(?(?=(?<seen>a)(*ACCEPT:inside))a|b)";
		var source = String.Concat( prefix, suffix );
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( "a" ), true, 0 );
		Assert.Equal( (nuint)0, native.JitSize ); Assert.Equal( Hex( "before" ), native.Mark );
		var result = Compile( source, new() { Utf16 = true } ).Match( "a".AsMemory() );
		Assert.True( result.IsSuccess ); Assert.Equal( native.Captures, Spans( result.Match ) ); Assert.Equal( "before", result.Match!.MarkName );
	}

	[Theory]
	[InlineData( "(*ACCEPT)", 2 )]
	[InlineData( "(*ACCEPT:)", 2 )]
	[InlineData( "(*ACCEPT:N)", 3 )]
	[InlineData( "(*ACCEPT:\0)", 3 )]
	[InlineData( "a(*ACCEPT:N)", 4 )]
	public void ImmediateAcceptanceHasAnExactWorkCost( string source, int required ) {
		var exact = Compile( source, new() { MatchWorkLimit = required } ).Match( "a".AsMemory() );
		Assert.True( exact.IsSuccess ); Assert.NotNull( exact.Match );
		var shortResult = Compile( source, new() { MatchWorkLimit = required - 1 } ).Match( "a".AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, shortResult.Diagnostic?.Code ); Assert.Null( shortResult.Match );
	}

	[Theory]
	[InlineData( "(?=(a(a(a(*ACCEPT:N)))))aaa", "aaa" )]
	[InlineData( "(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>(a|ab)(*ACCEPT:N)))", "abc" )]
	[InlineData( "(?*(a|ab)(*ACCEPT:N))\\1c|abc", "abc" )]
	[InlineData( "(*MARK:A)(?(?=(a)(*ACCEPT:N))a|b)(?*(*ACCEPT)){0}", "a" )]
	public void ScopeExitPruningAndRollbackShareQuotasAndCancellation( string source, string subject ) {
		var work = 0;
		var observed = Compile( source, new() { WorkObserver = count => work = count } ).Match( subject.AsMemory() );
		Assert.True( observed.IsSuccess ); Assert.True( work > 3 );
		var exact = Compile( source, new() { MatchWorkLimit = work } ).Match( subject.AsMemory() );
		Assert.Equal( Spans( observed.Match ), Spans( exact.Match ) ); Assert.Equal( MarkHex( observed.Match ), MarkHex( exact.Match ) );
		Assert.True( exact.IsSuccess );
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded,
			Compile( source, new() { MatchWorkLimit = work - 1 } ).Match( subject.AsMemory() ).Diagnostic?.Code );
		for ( var cancelAt = 1; cancelAt <= work; cancelAt++ ) {
			using var cancellation = new CancellationTokenSource();
			var stop = cancelAt;
			var compiled = Compile( source, new() { WorkObserver = count => { if ( count == stop ) { cancellation.Cancel(); } } } );
			Assert.Throws<OperationCanceledException>( () => compiled.Match( subject.AsMemory(), cancellationToken: cancellation.Token ) );
			var reused = compiled.Match( subject.AsMemory() );
			Assert.True( reused.IsSuccess ); Assert.Equal( Spans( observed.Match ), Spans( reused.Match ) );
			Assert.Equal( MarkHex( observed.Match ), MarkHex( reused.Match ) );
		}
		using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), cancellationToken: cancelled.Token ) );
	}

	[Fact]
	public void AcceptancePrunesRetriesAndPreservesTheFrameCeiling() {
		const string source = "(?(DEFINE)(?<x>a(?&x)?(*ACCEPT:N)))(?&x)";
		var compiled = Compile( source, new() );
		var successful = compiled.Match( new string( 'a', 200 ).AsMemory() );
		Assert.True( successful.IsSuccess ); Assert.Equal( 200, successful.Match!.CodeUnitLength ); Assert.Equal( "N", successful.Match.MarkName );
		var exhausted = compiled.Match( new string( 'a', 1100 ).AsMemory() );
		Assert.Null( exhausted.Match ); Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded, exhausted.Diagnostic?.Code );
		Assert.Equal( "N", compiled.Match( "a".AsMemory() ).Match!.MarkName );
	}

	[Theory]
	[InlineData( "é😀(*ACCEPT:unterminated", 24, 24 )]
	[InlineData( "é😀(*ACCEPT)**", 14, 13 )]
	[InlineData( "é😀(*ACCEPT){65536}", 18, 12 )]
	[InlineData( "é😀(*ACCEPT)\\k<missing>", 15, 12 )]
	public void AcceptedTailsStillReceiveOriginalUnitSyntaxDiagnostics( string source, int nativeOffset, int managedOffset ) {
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), [], true, 0 );
		Assert.Equal( "E", native.Kind ); Assert.Equal( nativeOffset, (int)native.ErrorOffset );
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.Equal( PcreWordDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code ); Assert.Equal( managedOffset, result.Diagnostic!.CodeUnitOffset );
	}

	[Theory]
	[InlineData( "\\A(?(DEFINE)(?<x>(a)(*ACCEPT:N)b))(?&x(2))\\2" )]
	[InlineData( "\\A(?(DEFINE)(?<x>(a(*ACCEPT:N)b)))(?&x(2))\\2" )]
	public void SelectedReturnAcceptanceRemainsOutsideTheQualifiedProfile( string source ) {
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( "aa" ), true, 0, mask: 1 );
		Assert.Equal( "X", native.Kind ); Assert.Equal( -44, native.ErrorCode );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
	}

	[Fact]
	public void AcceptanceRetainsSourceAdmissionAndStrictValidation() {
		foreach ( var source in new[] { "(*ACCEPT)\\X", "(*asr:a(*ACCEPT:N))X", "(*ACCEPT)(*sr:a)", "(*ACCEPT)(?:a){65}", "(?<=(a(*ACCEPT)(?<=b*)))x" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax, PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
		var sourceAtLimit = String.Concat( "(*ACCEPT:", new string( 'é', 4086 ), ")" );
		Assert.Equal( 4096, sourceAtLimit.Length ); Assert.NotNull( Compile( sourceAtLimit, new() ).Match( "".AsMemory() ).Match );
		Assert.Equal( PcreWordDiagnosticCode.PatternLengthLimitExceeded,
			PcreWordSequenceCompiler.CompileSequence( String.Concat( sourceAtLimit, "x" ).AsMemory() ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Pattern,
			PcreWordSequenceCompiler.CompileSequence( "(*ACCEPT)\ud800".AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Subject,
			Compile( "(*ACCEPT)", new() { Utf16 = true } ).Match( "a\udfff".AsMemory() ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( "(?<x>é😀)(*ACCEPT:漢😀)z", "é😀" )]
	[InlineData( "(*MARK:M)(?:\\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:é)|ab))", "abc" )]
	[InlineData( "(*MARK:before)(?(?=(?<x>é)(*ACCEPT:漢))é|b)(?*(*ACCEPT)){0}", "é" )]
	public void ByteAndWordCapturedValuesAndLabelsAgree( string source, string subject ) {
		var word = Compile( source, new() { Utf16 = true } );
		var bytes = Encoding.UTF8.GetBytes( subject );
		var compiledByte = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true } );
		Assert.True( compiledByte.IsSuccess );
		for ( var mask = 0; mask < 64; mask++ ) {
			var policy = PcreMatchPolicyConsumer.Policy( mask );
			var wr = word.Match( subject.AsMemory(), policy ); var br = compiledByte.Pattern!.Match( bytes, policy );
			Assert.True( wr.IsSuccess ); Assert.True( br.IsSuccess ); Assert.Equal( wr.Match is not null, br.Match is not null );
			if ( wr.Match is null ) { continue; }
			Assert.Equal( Encoding.UTF8.GetString( br.Match!.MarkName.Span ), wr.Match.MarkName );
			Assert.Equal( br.Match.Captures.Count, wr.Match.Captures.Count );
			for ( var index = 0; index < wr.Match.Captures.Count; index++ ) {
				var wc = wr.Match.Captures[index]; var bc = br.Match.Captures[index];
				Assert.Equal( wc.HasValue, bc.HasValue );
				if ( wc is { } span ) {
					Assert.Equal( Encoding.UTF8.GetString( bytes.AsSpan( bc!.Value.ByteIndex, bc.Value.ByteLength ) ),
						subject.Substring( span.CodeUnitIndex, span.CodeUnitLength ) );
				}
			}
		}
	}

	[Fact]
	public async Task AcceptedCapturesAndLabelsRemainOwnedAcrossConcurrentReuse() {
		var source = "(?<x>😀)(*ACCEPT:é😀)z".ToCharArray();
		var compiled = Compile( new string( source ), new() { Utf16 = true } );
		var owned = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.True( owned.IsSuccess ); Array.Fill( source, 'x' );
		var saved = owned.Pattern!.Match( "__😀xx".AsMemory( 2, 4 ) );
		Assert.Equal( "0:2,0:2", Spans( saved.Match ) ); Assert.Equal( "é😀", saved.Match!.MarkName );
		var results = await Task.WhenAll( Enumerable.Range( 0, 64 ).Select( index => Task.Run( () =>
			compiled.Match( ( index % 2 == 0 ? "😀" : "no" ).AsMemory() ) ) ) );
		for ( var index = 0; index < results.Length; index++ ) {
			Assert.True( results[index].IsSuccess ); Assert.Equal( index % 2 == 0, results[index].Match is not null );
			if ( results[index].Match is { } match ) { Assert.Equal( "é😀", match.MarkName ); Assert.Equal( "0:2,0:2", Spans( match ) ); }
		}
		Assert.Equal( "0:2,0:2", Spans( saved.Match ) ); Assert.Equal( "é😀", saved.Match.MarkName );
	}

	private static PcreCompiledWordPattern Compile( string source, PcreWordOptions options ) {
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), options );
		Assert.True( result.IsSuccess, $"{source}: {result.Diagnostic}" ); return result.Pattern!;
	}
	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( c => c is { } span ? $"{span.CodeUnitIndex}:{span.CodeUnitLength}" : "-" ) );
	private static string MarkHex( PcreWordMatch? match ) => match is null ? "" : Hex( match.MarkName );
	private static string Hex( string value ) => String.Concat( value.Select( unit => ( (ushort)unit ).ToString( "X4" ) ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
