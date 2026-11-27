using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordAtomicTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new (string Source, string Subject, int Start, bool Utf, bool Ucp)[] {
				("a?+a", "aaab", 0, true, false),
				("a*+a", "aaab", 0, true, false),
				("a++a", "aaab", 0, true, false),
				("a{0}+a", "aaab", 0, true, false),
				("a{1}+a", "aaab", 0, true, false),
				("a{1,3}+a", "aaab", 0, true, false),
				("a{1,}+a", "aaab", 0, true, false),
				(".?+b", "aaab", 0, true, false),
				(".*+b", "aaab", 0, true, false),
				(".++b", "aaab", 0, true, false),
				(".{0}+b", "aaab", 0, true, false),
				(".{1}+b", "aaab", 0, true, false),
				(".{1,3}+b", "aaab", 0, true, false),
				(".{1,}+b", "aaab", 0, true, false),
				("[ab]?+b", "aaab", 0, true, false),
				("[ab]*+b", "aaab", 0, true, false),
				("[ab]++b", "aaab", 0, true, false),
				("[ab]{0}+b", "aaab", 0, true, false),
				("[ab]{1}+b", "aaab", 0, true, false),
				("[ab]{1,3}+b", "aaab", 0, true, false),
				("[ab]{1,}+b", "aaab", 0, true, false),
				("\\w?+x", "éΣx", 0, true, true),
				("\\w*+x", "éΣx", 0, true, true),
				("\\w++x", "éΣx", 0, true, true),
				("\\w{0}+x", "éΣx", 0, true, true),
				("\\w{1}+x", "éΣx", 0, true, true),
				("\\w{1,3}+x", "éΣx", 0, true, true),
				("\\w{1,}+x", "éΣx", 0, true, true),
				("\\R?+x", "\r\n\r\nx", 0, true, false),
				("\\R*+x", "\r\n\r\nx", 0, true, false),
				("\\R++x", "\r\n\r\nx", 0, true, false),
				("\\R{0}+x", "\r\n\r\nx", 0, true, false),
				("\\R{1}+x", "\r\n\r\nx", 0, true, false),
				("\\R{1,3}+x", "\r\n\r\nx", 0, true, false),
				("\\R{1,}+x", "\r\n\r\nx", 0, true, false),
				("😀?+x", "😀😀x", 0, true, false),
				("😀*+x", "😀😀x", 0, true, false),
				("😀++x", "😀😀x", 0, true, false),
				("😀{0}+x", "😀😀x", 0, true, false),
				("😀{1}+x", "😀😀x", 0, true, false),
				("😀{1,3}+x", "😀😀x", 0, true, false),
				("😀{1,}+x", "😀😀x", 0, true, false),
				("(a)?+b", "aaab", 0, true, false),
				("(a)*+b", "aaab", 0, true, false),
				("(a)++b", "aaab", 0, true, false),
				("(a){0}+b", "aaab", 0, true, false),
				("(a){1}+b", "aaab", 0, true, false),
				("(a){1,3}+b", "aaab", 0, true, false),
				("(a){1,}+b", "aaab", 0, true, false),
				("(?:a|ab)?+c", "aabc", 0, true, false),
				("(?:a|ab)*+c", "aabc", 0, true, false),
				("(?:a|ab)++c", "aabc", 0, true, false),
				("(?:a|ab){0}+c", "aabc", 0, true, false),
				("(?:a|ab){1}+c", "aabc", 0, true, false),
				("(?:a|ab){1,3}+c", "aabc", 0, true, false),
				("(?:a|ab){1,}+c", "aabc", 0, true, false),
				("(?>a|ab)?+c", "aabc", 0, true, false),
				("(?>a|ab)*+c", "aabc", 0, true, false),
				("(?>a|ab)++c", "aabc", 0, true, false),
				("(?>a|ab){0}+c", "aabc", 0, true, false),
				("(?>a|ab){1}+c", "aabc", 0, true, false),
				("(?>a|ab){1,3}+c", "aabc", 0, true, false),
				("(?>a|ab){1,}+c", "aabc", 0, true, false),
				("(?:(a)?b)?+b", "abbb", 0, true, false),
				("(?:(a)?b)*+b", "abbb", 0, true, false),
				("(?:(a)?b)++b", "abbb", 0, true, false),
				("(?:(a)?b){0}+b", "abbb", 0, true, false),
				("(?:(a)?b){1}+b", "abbb", 0, true, false),
				("(?:(a)?b){1,3}+b", "abbb", 0, true, false),
				("(?:(a)?b){1,}+b", "abbb", 0, true, false),
				("(?>a)", "za", 0, true, false),
				("(?>)", "", 0, true, false),
				("(?>|a)", "a", 0, true, false),
				("(?>a|)", "a", 0, true, false),
				("(?>a|ab)", "ab", 0, true, false),
				("(?>a|ab)c", "abc", 0, true, false),
				("(?:a|ab)c", "abc", 0, true, false),
				("(?>ab|a)c", "ac", 0, true, false),
				("(?>a*ab)c", "aaabc", 0, true, false),
				("(?>a*)a", "aaa", 0, true, false),
				("(?>a*?)a", "aaa", 0, true, false),
				("(*atomic:a|ab)c", "abc", 0, true, false),
				("(*atomic:ab|a)c", "ac", 0, true, false),
				("(*atomic:)", "a", 0, true, false),
				("a?(?>ab|b)c", "abc", 0, true, false),
				("a??(?>ab|b)c", "abc", 0, true, false),
				("a*(?>a|ab)bc", "aabc", 0, true, false),
				("a*(?>ab|b)c", "aabc", 0, true, false),
				("(?>a?b?c?)c?", "abc", 0, true, false),
				("(?>a?b?c?)d", "abd", 0, true, false),
				("(?>a|ab)c|abc", "abc", 0, true, false),
				("(?>(a)b|(a)c)d", "acd", 0, true, false),
				("(?>(a|ab))\\1c", "ababc", 0, true, false),
				("(?>(?<x>ab))c\\k<x>", "abcab", 0, true, false),
				("(?>(a))b|a", "a", 0, true, false),
				("(?>(a))(?>(b))\\1\\2", "abab", 0, true, false),
				("(?>((?>a|ab))c|a(b)c)", "abc", 0, true, false),
				("((?>(a)|b)c|ab)d", "abd", 0, true, false),
				("(?>a(?>b|bc)d|abc)e", "abce", 0, true, false),
				("(?>(a)|(b))*", "ab", 0, true, false),
				("(?>(a)|(b))*?c", "abc", 0, true, false),
				("(?>a|ab)+c", "aabc", 0, true, false),
				("(?>a|ab)+?c", "aabc", 0, true, false),
				("(?>a|ab){1,3}c", "aabc", 0, true, false),
				("(?>a|ab){1,3}?c", "aabc", 0, true, false),
				("(?>ab|a){1,3}c", "aabc", 0, true, false),
				("(?>a|ab)++c", "aabc", 0, true, false),
				("(?>ab|a)++c", "aabc", 0, true, false),
				("(?>a|ab){1,3}+c", "aabc", 0, true, false),
				("(?>a){0}b", "b", 0, true, false),
				("((?>a)){0}+b", "b", 0, true, false),
				("(?>a?)*a", "a", 0, true, false),
				("(?>a?)+a", "a", 0, true, false),
				("(?>a?){2}a", "a", 0, true, false),
				("(?>a?){0,3}?a", "a", 0, true, false),
				("((?>a?))*a", "a", 0, true, false),
				("((?>a?))*+a", "a", 0, true, false),
				("a*+a", "aaa", 0, true, false),
				("a*a", "aaa", 0, true, false),
				("a?+a", "a", 0, true, false),
				("a?a", "a", 0, true, false),
				("a{1,3}+a", "aaa", 0, true, false),
				("a{1,3}a", "aaa", 0, true, false),
				("a{2}+a", "aaa", 0, true, false),
				("a{,2}+b", "aab", 0, true, false),
				("a{ 1 , 2 }+b", "aab", 0, true, false),
				("a{0,0}+b", "b", 0, true, false),
				("(?U)a++a", "aaa", 0, true, false),
				("(?U)a+a", "aaa", 0, true, false),
				("(?U)(?>a+)a", "aaa", 0, true, false),
				("(?U)(?>a+?)a", "aaa", 0, true, false),
				("(a|ab)++c", "aabc", 0, true, false),
				("(ab|a)++c", "aabc", 0, true, false),
				("(a|ab){2}+c", "aabc", 0, true, false),
				("(a|ab){2}c", "aabc", 0, true, false),
				("(a?){2,3}+a", "aaa", 0, true, false),
				("(a?){2,3}a", "aaa", 0, true, false),
				("(a?){0,2}+a", "a", 0, true, false),
				("(a?){0,2}a", "a", 0, true, false),
				("(a?)*+b", "aab", 0, true, false),
				("(a?)++b", "b", 0, true, false),
				("(a?){1,}+b", "b", 0, true, false),
				("(a?){2,}+b", "b", 0, true, false),
				("(a)\\1*+a", "aaaa", 0, true, false),
				("(a)\\1*a", "aaaa", 0, true, false),
				("(a)\\1{1,2}+a", "aaaa", 0, true, false),
				("(a)\\1{2}+b", "aaab", 0, true, false),
				("(ab)\\1++c", "abababc", 0, true, false),
				("(a?)\\1*+b", "b", 0, true, false),
				("(?i)(?<x>é)(?>\\k<x>+)x", "éÉéx", 0, true, false),
				("(?i)(?<x>é)\\k<x>++x", "éÉéx", 0, true, false),
				("(?J)(?<x>a)|(?>(?<x>ab))\\k<x>c", "ababc", 0, true, false),
				("(?|(?>(a))|(?>(b)))\\1", "bb", 0, true, false),
				("(?n)(?>(?<x>a|ab))\\k<x>c", "aac", 0, true, false),
				("(?x)(?> a (b) ) \\1", "abb", 0, true, false),
				("(?>(?*(a|ab))\\1)c", "abc", 0, true, false),
				("(?*(a|ab))(?>\\1)c", "abc", 0, true, false),
				("(?*(a|ab))\\1++c", "abc", 0, true, false),
				("(?*(a|ab)){2}(?>\\1)c", "abc", 0, true, false),
				("(?=(?>(a|ab))c)abc", "abc", 0, true, false),
				("(?!(?>(a|ab))c)abc", "abc", 0, true, false),
				("(?>(?=(a))a)b\\1", "aba", 0, true, false),
				("(?>(?!(a)c)a)(b)\\2", "abb", 0, true, false),
				("(?<=(?>a|ab))c", "abc", 0, true, false),
				("(?<=(?>ab|a))c", "abc", 0, true, false),
				("(?<*(?>a{1,2}))b", "aab", 0, true, false),
				("(?<*(a{1,2}))(?>b\\1)c", "aabaac", 0, true, false),
				("(?>(?<*(a{1,2})))b\\1", "aabaa", 2, true, false),
				("(?<*a{1,2}(?>(?=(b))))\\1", "aab", 2, true, false),
				("(?<*a{1,2}(?>\\b))b", "aab", 2, true, false),
				("(?<*a{1,2}(?>\\z))b", "aab", 2, true, false),
				("(?<*a{1,2}(?>(?*b*?)))b", "aab", 2, true, false),
				("(?<=(?>a(?=b)))b", "ab", 0, true, false),
				("(?>(?<*(?<=a)b))c", "abc", 2, true, false),
				("(?>(?<*(?<!a)b))c", "abc", 2, true, false),
				("(?>(?<*(?<=a)b))c", "abc", 2, false, false),
				("(?<=a{1,2}+)b", "aab", 0, true, false),
				("(?<*(a{1,2}+))b\\1", "aabaa", 0, true, false),
				("(?=(?>a)){2}a", "a", 0, true, false),
				("(?=(?>a)){0}b", "b", 0, true, false),
				("(?:(?=(\\1a|a)))++\\1", "aaa", 0, true, false),
				("(?:(?=(\\1a|a))){1,}+\\1", "aaa", 0, true, false),
				("(?>(?=(\\1a|a)))+\\1", "aaa", 0, true, false),
				("(?>(?=(\\1a|a))){1,}\\1", "aaa", 0, true, false),
				("(?>(?*(a|ab)){2}\\1)c", "abc", 0, true, false),
				("(?>(?=ab){2})ab", "ab", 0, true, false),
				("(?>(?=ab){2}+)ab", "ab", 0, true, false),
				("(?>(?<x>a|ab))c|(?<y>ab)c", "abc", 0, true, false),
				("(?>(a)|(ab))c|(a)(b)c", "abc", 0, true, false),
				("(?>(?*(a|ab))a){0}(b)", "b", 0, true, false),
				("(?i)(?>é+)x", "éÉx", 0, true, false),
				("(?i)(?>𐐀+)x", "𐐨𐐀x", 0, true, false),
				("(?i)𐐀++x", "𐐨𐐀x", 0, true, false),
				("(?i)(?>k|K)(?ir:k)", "KK", 0, true, false),
				("(?>\\p{Greek}+)x", "Σςx", 0, true, true),
				("[éΣ]++x", "éΣx", 0, true, true),
				("(?>(\\w+))x", "éΣx", 0, true, true),
				("(?>\\R)\\n", "\r\n", 0, true, false),
				("(?>\\R|\\r)\\nx", "\r\nx", 0, true, false),
				("\\R++\\n", "\r\n\n", 0, true, false),
				("(*BSR_ANYCRLF)\\R++x", "\r\n\rx", 0, true, false),
				("(?m)(?>^a+$)", "x\naaa\nx", 0, true, false),
				("(*CR)(?m)(?>^a+$)", "x\raaa\rx", 0, true, false),
				("(*NUL)(?m)(?>^a+$)", "x\u0000aaa\u0000x", 0, true, false),
				("(?>\\Ga+)", "zaaa", 1, true, false),
				("(?>\\ba+)", " aaa", 1, true, false),
				("(?>(😀)+)x", "x😀😀x", 1, true, false),
				("(?>(😀)+)x", "x😀😀x", 1, false, false),
				("(?i)é++x", "Ééx", 0, false, true),
				("(?i)é++x", "Ééx", 0, false, false),
				("(?:(?=(\\1a|a)))*\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a)))*\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a)))*?\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a)))*?\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a)))*+\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a)))*+\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a)))+\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a)))+\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a)))+?\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a)))+?\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a)))++\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a)))++\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){1,}\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){1,}\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){1,}?\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){1,}?\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){1,}+\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){1,}+\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){2,}\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){2,}\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){2,}?\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){2,}?\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){2,}+\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){2,}+\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){3,}\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){3,}\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){3,}?\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){3,}?\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){3,}+\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){3,}+\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){0,3}\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){0,3}\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){0,3}?\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){0,3}?\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){0,3}+\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){0,3}+\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){2,3}\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){2,3}\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){2,3}?\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){2,3}?\\1", "aaaa", 0, true, false),
				("(?:(?=(\\1a|a))){2,3}+\\1", "aaaa", 0, false, false),
				("(?:(?=(\\1a|a))){2,3}+\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a)))*\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a)))*\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a)))*?\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a)))*?\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a)))*+\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a)))*+\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a)))+\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a)))+\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a)))+?\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a)))+?\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a)))++\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a)))++\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){1,}\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){1,}\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){1,}?\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){1,}?\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){1,}+\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){1,}+\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){2,}\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){2,}\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){2,}?\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){2,}?\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){2,}+\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){2,}+\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){3,}\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){3,}\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){3,}?\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){3,}?\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){3,}+\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){3,}+\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){0,3}\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){0,3}\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){0,3}?\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){0,3}?\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){0,3}+\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){0,3}+\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){2,3}\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){2,3}\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){2,3}?\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){2,3}?\\1", "aaaa", 0, true, false),
				("(?>(?=(\\1a|a))){2,3}+\\1", "aaaa", 0, false, false),
				("(?>(?=(\\1a|a))){2,3}+\\1", "aaaa", 0, true, false),
				("((?=(\\2a|a)))*\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a)))*\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a)))*?\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a)))*?\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a)))*+\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a)))*+\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a)))+\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a)))+\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a)))+?\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a)))+?\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a)))++\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a)))++\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){1,}\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){1,}\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){1,}?\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){1,}?\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){1,}+\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){1,}+\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){2,}\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){2,}\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){2,}?\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){2,}?\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){2,}+\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){2,}+\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){3,}\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){3,}\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){3,}?\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){3,}?\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){3,}+\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){3,}+\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){0,3}\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){0,3}\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){0,3}?\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){0,3}?\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){0,3}+\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){0,3}+\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){2,3}\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){2,3}\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){2,3}?\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){2,3}?\\2", "aaaa", 0, true, false),
				("((?=(\\2a|a))){2,3}+\\2", "aaaa", 0, false, false),
				("((?=(\\2a|a))){2,3}+\\2", "aaaa", 0, true, false),
				("(?>\\x{0}+)x", "\u0000\u0000x", 0, false, false),
				("(?>(\ud800)+)x", "\ud800\ud800x", 0, false, false),
				("\udc00++x", "\udc00\udc00x", 0, false, false),
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
	public void AtomicScopesAgreeWithEveryNativePolicy( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"atomic {id}: {compiled.Diagnostic}" );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, (uint)start, defaults, mask, ucp );
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			Assert.Equal( native.Kind == "M", result.Match is not null );
			Assert.Equal( native.Captures, Spans( result.Match ) );
		}
	}

	[Theory]
	[InlineData( "(?>a|ab)c", "abc", "" )]
	[InlineData( "(?:a|ab)c", "abc", "0:3" )]
	[InlineData( "(?>(a)b|(a)c)d", "acd", "0:3,-,0:1" )]
	[InlineData( "(?>(a))b|a", "a", "0:1,-" )]
	[InlineData( "(?>(a)|(b))*", "ab", "0:2,0:1,1:1" )]
	[InlineData( @"(?*(a|ab))(?>\1)c", "abc", "0:3,0:2" )]
	[InlineData( @"(?>(?*(a|ab))\1)c", "abc", "" )]
	[InlineData( "(a|ab)++c", "aabc", "" )]
	[InlineData( "(ab|a)++c", "aabc", "0:4,1:2" )]
	[InlineData( "a*+a", "aaa", "" )]
	[InlineData( "a*a", "aaa", "0:3" )]
	public void CommitmentKeepsConsumptionCapturesAndOuterRollback( string source, string subject, string expected ) {
		Assert.Equal( expected, PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 0 ).Captures );
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Pattern!.Match( subject.AsMemory() );
		Assert.True( result.IsSuccess ); Assert.Equal( expected, Spans( result.Match ) );
	}

	[Theory]
	[InlineData( "(?>(é+))x", "ééx" )]
	[InlineData( "(?i)(?>𐐀+)x", "𐐨𐐀x" )]
	[InlineData( "(ab|a)++c", "aabc" )]
	[InlineData( @"(?*(é|é😀))(?>\1)x", "é😀x" )]
	[InlineData( @"(?<*(a{1,2}))(?>b\1)c", "aabaac" )]
	[InlineData( "(*atomic:(a)|(b))*", "ab" )]
	[InlineData( @"(?>\R+)x", "\r\n\rx" )]
	public void AtomicScopesKeepCrossWidthCaptureSpansAndValues( string source, string subject ) {
		var bytes = Encoding.UTF8.GetBytes( subject );
		var compiledBytes = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true, Ucp = true } );
		var compiledWords = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true, Ucp = true } );
		Assert.True( compiledBytes.IsSuccess ); Assert.True( compiledWords.IsSuccess );
		var byteResult = compiledBytes.Pattern!.Match( bytes );
		var wordResult = compiledWords.Pattern!.Match( subject.AsMemory() );
		Assert.True( byteResult.IsSuccess ); Assert.True( wordResult.IsSuccess );
		var b = Assert.IsType<PcreByteMatch>( byteResult.Match );
		var w = Assert.IsType<PcreWordMatch>( wordResult.Match );
		Assert.Equal( PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 0, ucp: true ).Captures, Spans( w ) );
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
	[InlineData( "(?:", "1" )]
	[InlineData( "(?>", "1" )]
	[InlineData( "(", "2" )]
	public void FinalRequiredEmptyGroupsKeepNativeCapturesAcrossWidths( string opener, string reference ) {
		const string subject = "aaaa";
		var bytes = Encoding.UTF8.GetBytes( subject );
		var coordinates = new PcreDualWidthCoordinates( bytes, subject );
		foreach ( var count in new[] { "1", "2", "3" } ) {
			foreach ( var suffix in new[] { "", "?", "+" } ) {
				var source = String.Concat( opener, "(?=(\\", reference, "a|a))){", count, ",}", suffix, "\\", reference );
				foreach ( var utf in new[] { false, true } ) {
					var b = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = utf } );
					var w = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf } );
					Assert.True( b.IsSuccess ); Assert.True( w.IsSuccess );
					for ( var mask = 0; mask < 64; mask++ ) {
						var policy = PcreMatchPolicyConsumer.Policy( mask );
						var br = b.Pattern!.Match( bytes, policy );
						var wr = w.Pattern!.Match( subject.AsMemory(), policy );
						var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, 0, mask: mask );
						Assert.True( br.IsSuccess ); Assert.True( wr.IsSuccess );
						Assert.True( native.Kind is "M" or "N" );
						Assert.Equal( native.Captures, Spans( wr.Match ) );
						var spans = br.Match is null ? "" : String.Join( ",", br.Match.Captures.Select( c => c is { } span
							? $"{span.ByteIndex}:{span.ByteLength}" : "-" ) );
						Assert.Equal( coordinates.WordCaptures( native.Captures ), coordinates.ByteCaptures( spans ) );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( "(?>)", "", 6 )]
	[InlineData( "(?>a)", "a", 7 )]
	[InlineData( "(?>a|ab)", "ab", 9 )]
	[InlineData( "(?>ab|a)c", "ac", 12 )]
	[InlineData( "(?>a*)", "aaa", 12 )]
	[InlineData( "a*+", "aaa", 12 )]
	[InlineData( "a{2}+", "aa", 8 )]
	[InlineData( "a?+", "a", 9 )]
	[InlineData( "a{0}+", "", 6 )]
	[InlineData( "(a)++", "aa", 19 )]
	[InlineData( "(?>(a))b|a", "a", 13 )]
	public void AtomicEntryCommitAndRollbackConsumeExactWork( string source, string subject, int required ) {
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
	[InlineData( "(?>a|ab)", "a", 8, "0:1" )]
	[InlineData( "(?>(a)b|(a)c)d", "acd", 9, "0:3,-,0:1" )]
	[InlineData( "a*+", "aaa", 11, "0:3" )]
	public void CancellationDuringPruningOrUndoLeavesThePatternReusable( string source, string subject, int cancelAt, string expected ) {
		using var cancellation = new CancellationTokenSource();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			WorkObserver = work => { if ( work == cancelAt ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => pattern.Match( subject.AsMemory(), cancellationToken: cancellation.Token ) );
		Assert.Equal( expected, Spans( pattern.Match( subject.AsMemory() ).Match ) );
	}

	[Fact]
	public void AtomicGuardsShareTheFrameCeilingAndCommitBeforeOuterRetries() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( @"\A(?:(?>a|aa))*\z".AsMemory() ).Pattern!;
		Assert.Equal( "0:1021", Spans( pattern.Match( new string( 'a', 1021 ).AsMemory() ).Match ) );
		var result = pattern.Match( new string( 'a', 1022 ).AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchDepthLimitExceeded, result.Diagnostic?.Code ); Assert.Null( result.Match );
		Assert.Equal( "0:2", Spans( pattern.Match( "aa".AsMemory() ).Match ) );
		var scalar = PcreWordSequenceCompiler.CompileSequence( "a*+".AsMemory() ).Pattern!;
		Assert.Equal( "0:10000", Spans( scalar.Match( new string( 'a', 10000 ).AsMemory() ).Match ) );
	}

	[Fact]
	public void CompiledSourcesAndReturnedCapturesSurviveLaterAtomicCalls() {
		const string text = @"(?>(?<x>a|ab))c|(?<y>ab)c";
		var source = text.ToCharArray();
		var pattern = PcreWordSequenceCompiler.CompileSequence( source ).Pattern!;
		Array.Fill( source, 'z' );
		var first = pattern.Match( "!zabc!".AsMemory( 1, 4 ) ).Match!;
		Assert.Equal( "1:3,-,1:2", Spans( first ) );
		foreach ( var subject in new[] { "b", "x", "abc", "ac", "", "ab" } ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( text ), Words( subject ), false, 0 );
			var result = pattern.Match( subject.AsMemory() );
			Assert.True( result.IsSuccess ); Assert.Equal( native.Captures, Spans( result.Match ) );
		}
		Assert.Null( first.GetCapture( "x" ) ); Assert.Equal( new PcreWordCapture( 1, 2 ), first.GetCapture( "y" ) );
		Assert.Equal( "1:3,-,1:2", Spans( first ) );
	}

	[Fact]
	public void AtomicWrappersRetainCountPathNodeAndNestingBounds() {
		var expanded = String.Concat( String.Concat( Enumerable.Repeat( "a{64}", 63 ) ), "a{62}" );
		foreach ( var source in new[] { "(?>a?b?c?d?e?f?)", "a{64}+", "a{0,63}+", "a{64,}+",
			"(a){0,64}+", "(?>a){64,}", String.Concat( Enumerable.Repeat( "a*+", 64 ) ),
			String.Concat( "(?>", expanded, ")" ) } ) {
			Assert.True( PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).IsSuccess, source );
		}
		foreach ( var source in new[] { "(?>a?b?c?d?e?f?)g?", "a{65}+", "a{0,64}+", "a{65,}+",
			"(a){65}+", "(?>a){65,}", String.Concat( Enumerable.Repeat( "a*+", 65 ) ),
			String.Concat( "(?>", expanded, "a)" ), "(?>a(*THEN)){0}", "(a)(?>(?1)(*THEN))", "(a)(?(1)b(*THEN)|c)" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
		foreach ( var opener in new[] { "(?>", "(*atomic:" } ) {
			var nested = String.Concat( String.Concat( Enumerable.Repeat( opener, 64 ) ), "a", new string( ')', 64 ) );
			var compiled = PcreWordSequenceCompiler.CompileSequence( nested.AsMemory() );
			Assert.True( compiled.IsSuccess ); Assert.Equal( "0:1", Spans( compiled.Pattern!.Match( "a".AsMemory() ).Match ) );
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( String.Concat( opener, nested, ")" ).AsMemory() ).Diagnostic?.Code );
		}
	}

	[Theory]
	[InlineData( "é(?>a", 1 )]
	[InlineData( "😀(*atomic:a", 2 )]
	[InlineData( "éa*++", 4 )]
	[InlineData( "éa{2,1}+", 2 )]
	[InlineData( "😀a{65536}+", 3 )]
	public void InvalidAtomicSyntaxKeepsOriginalCodeUnitOffsets( string source, int offset ) {
		Assert.Equal( "E", PcreNative16Oracle.ExecuteSequence( Words( source ), Array.Empty<ushort>(), true, 0 ).Kind );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidSyntax, offset ),
			PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Diagnostic );
	}

	[Fact]
	public void AtomicAndZeroCountBodiesKeepNativeValidationWindows() {
		foreach ( var source in new[] { @"(?>\b)c", @"(?>\b){0}c", @"(?>a(?<=ab)){0}c", @"(?:(?<=ab)){0}+c" } ) {
			const string subject = "\uD800bc";
			Assert.Equal( "X", PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 1 ).Kind );
			var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Pattern!.Match( subject.AsMemory(), 1 );
			Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 0 ), result.Diagnostic );
		}
		var plain = PcreWordSequenceCompiler.CompileSequence( "(?>a)".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( "1:1", Spans( plain.Match( "\uD800a".AsMemory(), 1 ).Match ) );
		var scalar = PcreWordSequenceCompiler.CompileSequence( "😀++".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidStartOffset, 1 ), scalar.Match( "😀".AsMemory(), 1 ).Diagnostic );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Subject, 2 ), scalar.Match( "😀\uD800".AsMemory() ).Diagnostic );
		Assert.Equal( new PcreWordDiagnostic( PcreWordDiagnosticCode.InvalidUtf16Pattern, 3 ),
			PcreWordSequenceCompiler.CompileSequence( "(?>\uD800)".AsMemory(), new() { Utf16 = true } ).Diagnostic );
	}

	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( c => c is { } span ? $"{span.CodeUnitIndex}:{span.CodeUnitLength}" : "-" ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
