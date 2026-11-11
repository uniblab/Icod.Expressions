using System.Text;
using System.Reflection;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pinned PCRE2 10.48 JIT MARK successful-path byte labels, rollback and scope returns.</summary>
public sealed class PcrePathMarkReferenceTests {
	[Theory]
	[InlineData( "a(*MARK:P)(*PRUNE)b", "7878616162", "3:2", "50", 0 )]
	[InlineData( "a+(*MARK:P)(*PRUNE)b", "616163616162", "3:3", "50", 0 )]
	[InlineData( "\\A(?:a(*MARK:P)(*PRUNE)b|ac)", "6163", "", "", 0 )]
	[InlineData( "(?:a(*MARK:P)(*PRUNE)b|ac)", "6163", "", "", 0 )]
	[InlineData( "(?:a(*MARK:P)(*PRUNE)b|ac)", "61636162", "2:2", "50", 0 )]
	[InlineData( "(?:a(*MARK:P)(*PRUNE)b|ac)", "6162", "0:2", "50", 0 )]
	[InlineData( "(a|ab)(*MARK:P)(*PRUNE)c", "616263", "", "", 0 )]
	[InlineData( "(a|ab)c", "616263", "0:3,0:2", "", 0 )]
	[InlineData( "a(*MARK:P)(*PRUNE)(b|bc)c", "61626363", "0:3,1:1", "50", 0 )]
	[InlineData( "(a)(*MARK:P)(*PRUNE)X|(b)", "6162", "1:1,-,1:1", "", 0 )]
	[InlineData( "\\Q(*MARK:P)(*PRUNE)\\E", "282A5052554E4529", "", "", 0 )]
	[InlineData( "[(*MARK:P)(*PRUNE)]", "50", "0:1", "", 0 )]
	[InlineData( "(?x)a (*MARK:P)(*PRUNE) # tail\n b", "6162", "0:2", "50", 0 )]
	[InlineData( "(?i)a(*MARK:P)(*PRUNE)b", "4142", "0:2", "50", 0 )]
	[InlineData( "(?i)(é)(*MARK:P)(*PRUNE)X|é", "C3A9C3A9", "", "", 1 )]
	[InlineData( "a\\K(*MARK:P)(*PRUNE)X|(b)", "6162", "1:1,1:1", "", 0 )]
	[InlineData( "(*NOTEMPTY)(*MARK:P)(*PRUNE)|a", "61", "", "", 0 )]
	[InlineData( "(*MARK:P)(*PRUNE)(*ACCEPT)|a", "61", "0:0", "50", 0 )]
	[InlineData( "(*NOTEMPTY)(*MARK:P)(*PRUNE)(*ACCEPT)|a", "61", "", "", 0 )]
	[InlineData( "\\A(?:(?>a(*MARK:P)(*PRUNE))b|ac)", "6163", "0:2", "", 0 )]
	[InlineData( "\\A(?:(?>a(*MARK:P)(*PRUNE)b)|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?=(a(*MARK:P)(*PRUNE)))ac|ac", "6163", "0:2,0:1", "50", 0 )]
	[InlineData( "\\A(?=(a(*MARK:P)(*PRUNE)b|a))a|a", "61", "", "", 0 )]
	[InlineData( "\\A(?!a(*MARK:P)(*PRUNE)b|a)a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?=a(*MARK:P)(*PRUNE)b|a)X|a)", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?!a(*MARK:P)(*PRUNE)b|a)a|X)", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?!(a(*MARK:P)(*PRUNE)b))a|X)", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?!(?=a(*MARK:P)(*PRUNE)b|a)a)a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?=(?!a(*MARK:P)(*PRUNE)b|a)a)a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?*(a(*MARK:P)(*PRUNE)|ab))\\1c", "616263", "", "", 0 )]
	[InlineData( "\\A(?*(a|ab)(*MARK:P)(*PRUNE))\\1c", "616263", "", "", 0 )]
	[InlineData( "\\A(?*(a(*MARK:P)(*PRUNE)b|ab))ab", "6162", "0:2,0:2", "50", 0 )]
	[InlineData( "(?<=(a(*MARK:P)(*PRUNE)b))c", "616263", "2:1,0:2", "50", 0 )]
	[InlineData( "(?<=(a(*MARK:P)(*PRUNE)b|ab))c|c", "616263", "2:1,0:2", "50", 0 )]
	[InlineData( "(?<=(a{1,2}(*MARK:P)(*PRUNE)b))c", "61616263", "3:1,0:3", "50", 0 )]
	[InlineData( "(?<!(a(*MARK:P)(*PRUNE)b|a))c", "6163", "1:1,-", "", 0 )]
	[InlineData( "(?(?<=(a(*MARK:P)(*PRUNE)b))c|X)", "616263", "2:1,0:2", "50", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:P)(*PRUNE)b|ac))(?&x)d|acd", "616364", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:P)(*PRUNE)b|ac))(?&x)d", "616364", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a|ab))(?&x)(*MARK:P)(*PRUNE)c|abc", "616263", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:P)(*PRUNE)))(?&x)b|ac", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:P)(*PRUNE)b))(?&x)c|aac", "616163", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*MARK:P)(*PRUNE)b))(?&x)c|aabc", "61616263", "0:4,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&y)c)(?<y>b(*MARK:P)(*PRUNE)X|b))(?&x)d|abcd", "61626364", "0:4,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=a(*MARK:P)(*PRUNE)b|a)a))(?&x)X|a", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:P)(*PRUNE)b|a))(?!(?&x))a", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:P)(*PRUNE)b|a))(?(?=(?&x))X|a)", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:P)(*PRUNE)b))(?:(?&x(2))X|ac)(?(v)Y|d)", "616364", "", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:P)(*PRUNE)b)?ac|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:P)(*PRUNE)b)*ac|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:P)(*PRUNE)b){0}ac|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:P)(*PRUNE))?b|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:P)(*PRUNE))??b|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:P)(*PRUNE))?+b|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:P)(*PRUNE))++b|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:P)(*PRUNE)b)+|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:P)(*PRUNE)){2}b|aa)", "6161", "", "", 0 )]
	[InlineData( "\\A(*sr:(a(*MARK:P)(*PRUNE)b|ac))|ac", "6163", "", "", 0 )]
	[InlineData( "\\A(*asr:(a(*MARK:P)(*PRUNE)))b|ac", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?J)(?<x>a)(*MARK:P)(*PRUNE)X|(?<x>ab)", "6162", "", "", 0 )]
	[InlineData( "\\A(?|(?<x>a(*MARK:P)(*PRUNE)b)|(?<x>ac))", "6163", "", "", 0 )]
	[InlineData( "\\A(?=(a(*MARK:P)(*PRUNE)b))a|a", "61", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:P)(*PRUNE)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:P)(*PRUNE)(?<v>b)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?<u>b)(*MARK:P)(*PRUNE)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:P)(*PRUNE)b))(?!(?&x(2)))ac(?(v)d|X)", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:P)(*PRUNE)b))(?(?=(?&x(2)))X|ac)(?(v)d|X)", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:P)(*PRUNE)b))(?:(?&x)X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:P)(*PRUNE)(?<v>b)X|abc))(?:(?&x(2))Y|abc)d", "61626364", "0:4,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:P)(*PRUNE)b))(?:(?&x(2))X|ac(*FAIL))|acd", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?!(?<v>a)(*MARK:P)(*PRUNE)b)a))(?&x(2))", "61", "0:1,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?(?=(?<v>a)(*MARK:P)(*PRUNE)b)X|a)))(?&x(2))", "61", "0:1,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=(?<v>a)(*MARK:P)(*PRUNE)b)a))(?:(?&x(2))X|a)", "61", "0:1,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*MARK:P)(*PRUNE)c))(?:(?&x(2))X|ab)d", "616264", "0:3,-,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*MARK:P)(*PRUNE)c))(?:(?&x)X|ab)d", "616264", "0:3,-,-,-", "", 0 )]
	[InlineData( "(*MARK:X)", "", "0:0", "58", 0 )]
	[InlineData( "(*:X)a", "61", "0:1", "58", 0 )]
	[InlineData( "a(*MARK:one)b(*:two)c", "616263", "0:3", "74776F", 0 )]
	[InlineData( "(*MARK:X)a|b", "62", "0:1", "", 0 )]
	[InlineData( "a(*MARK:X)c|b", "6162", "1:1", "", 0 )]
	[InlineData( "a(*MARK:X)c|b", "616162", "2:1", "", 0 )]
	[InlineData( "(*MARK:A)aX|(*MARK:B)ab", "6162", "0:2", "42", 0 )]
	[InlineData( "(*MARK:A)(?:a(*MARK:B)b|ac)", "6163", "0:2", "41", 0 )]
	[InlineData( "(*MARK:A)(?:a(*MARK:B)b|ac)", "6162", "0:2", "42", 0 )]
	[InlineData( "(?x)(*MARK:a # x ) a", "61", "0:1", "612023207820", 0 )]
	[InlineData( "(*MARK:\\x41)a", "61", "0:1", "5C783431", 0 )]
	[InlineData( "(*MARK:(a)a", "61", "0:1", "2861", 0 )]
	[InlineData( "(*MARK:é漢)a", "61", "0:1", "C3A9E6BCA2", 0 )]
	[InlineData( "(*:é漢)a", "61", "0:1", "C3A9E6BCA2", 0 )]
	[InlineData( "(*MARK:A\u0000B)a", "61", "0:1", "41", 0 )]
	[InlineData( "(*MARK:\u0000)a", "61", "0:1", "", 0 )]
	[InlineData( "\\Q(*MARK:X)\\E", "282A4D41524B3A5829", "0:9", "", 0 )]
	[InlineData( "[(*MARK:X)]", "4D", "0:1", "", 0 )]
	[InlineData( "(*MARK:A)(a|ab)(*MARK:B)c", "616263", "0:3,0:2", "42", 0 )]
	[InlineData( "(*MARK:A)(a|ab)c", "616263", "0:3,0:2", "41", 0 )]
	[InlineData( "(*MARK:A)a\\K(*MARK:B)b", "6162", "1:1", "42", 0 )]
	[InlineData( "(*MARK:A)a\\Kb", "6162", "1:1", "41", 0 )]
	[InlineData( "(*MARK:A)(?:a(*MARK:B))*a", "6161", "0:2", "42", 0 )]
	[InlineData( "(*MARK:A)(?:a(*MARK:B))*?a", "6161", "0:1", "41", 0 )]
	[InlineData( "(*MARK:A)(?:a(*MARK:B))*+b", "616162", "0:3", "42", 0 )]
	[InlineData( "(*MARK:A)(?:a(*MARK:B)){2}b", "616162", "0:3", "42", 0 )]
	[InlineData( "(*MARK:A)(?:a(*MARK:B)){0}b", "62", "0:1", "41", 0 )]
	[InlineData( "(?:a(*MARK:X)|b(*MARK:Y))+", "616261", "0:3", "58", 0 )]
	[InlineData( "(?:a(*MARK:X)|b(*MARK:Y))+", "626162", "0:3", "59", 0 )]
	[InlineData( "(?:(*MARK:X))*", "", "0:0", "58", 0 )]
	[InlineData( "(?:(*MARK:X))??", "", "0:0", "", 0 )]
	[InlineData( "(?:(*MARK:X))?+", "", "0:0", "58", 0 )]
	[InlineData( "(*MARK:root)(?=a(*MARK:yes))a", "61", "0:1", "796573", 0 )]
	[InlineData( "(*MARK:root)(?=a(*MARK:no)b)a|a", "61", "0:1", "", 0 )]
	[InlineData( "(*MARK:root)(?!a(*MARK:no)b)a", "61", "0:1", "726F6F74", 0 )]
	[InlineData( "(*MARK:root)(?!a(*MARK:no))a|a", "61", "0:1", "", 0 )]
	[InlineData( "(*MARK:root)(?(?=a(*MARK:yes))a|b)", "61", "0:1", "796573", 0 )]
	[InlineData( "(*MARK:root)(?(?=a(*MARK:no)b)b|a)", "61", "0:1", "726F6F74", 0 )]
	[InlineData( "(*MARK:root)(?(?!a(*MARK:no)b)a|b)", "61", "0:1", "726F6F74", 0 )]
	[InlineData( "(*MARK:root)(?(?!a(*MARK:no))b|a)", "61", "0:1", "6E6F", 0 )]
	[InlineData( "(*MARK:root)(?*(a(*MARK:one)|ab(*MARK:two)))\\1c", "616263", "0:3,0:2", "74776F", 0 )]
	[InlineData( "(*MARK:root)(?*(a(*MARK:one)|ab(*MARK:two)))\\1c", "61626162616263", "4:3,4:2", "74776F", 0 )]
	[InlineData( "(*MARK:root)(?<=a(*MARK:behind))b", "6162", "1:1", "626568696E64", 0 )]
	[InlineData( "(*MARK:root)(?<!(a(*MARK:no)b))b", "6162", "1:1,-", "726F6F74", 0 )]
	[InlineData( "(*MARK:root)(?(?<=(a(*MARK:yes)))b|X)", "6162", "1:1,0:1", "796573", 0 )]
	[InlineData( "(*MARK:root)(?(?<!(a(*MARK:no)))X|b)", "6162", "1:1,0:1", "6E6F", 0 )]
	[InlineData( "\\A(*MARK:root)(?:(?>a(*MARK:in))b|ac)", "6163", "0:2", "726F6F74", 0 )]
	[InlineData( "\\A(*MARK:root)(?>a(*MARK:in))b", "6162", "0:2", "696E", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:callee)))(*MARK:caller)(?&x)b", "6162", "0:2,-", "63616C6C6565", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:callee)))(*MARK:caller)(?&x)(*MARK:tail)b", "6162", "0:2,-", "7461696C", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:callee)b))(*MARK:caller)(?:(?&x)X|ac)d", "616364", "0:3,-", "63616C6C6572", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:callee)(*PRUNE)b))(*MARK:caller)(?:(?&x)X|ac)d", "616364", "0:3,-", "63616C6C6572", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:callee)(*THEN)b))(*MARK:caller)(?:(?&x)X|ac)d", "616364", "0:3,-", "63616C6C6572", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:in)(?&x)?b))(*MARK:root)(?&x)c", "6161626263", "0:5,-", "696E", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:X)(?&y)?b)(?<y>c(*MARK:Y)(?&x)?d))(?&x)e", "6163646265", "0:5,-,-", "59", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:callee)(*COMMIT)b))(*MARK:caller)(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "63616C6C6572", 0 )]
	[InlineData( "\\A(?|(?<x>a(*MARK:A))|(?<x>b(*MARK:B)))\\k<x>", "6262", "0:2,0:1", "42", 0 )]
	[InlineData( "\\A(?J)(?<x>a)(*MARK:A)X|(?<x>ab)(*MARK:B)", "6162", "0:2,-,0:2", "42", 0 )]
	[InlineData( "(*MARK:A)(*ACCEPT)z", "61", "0:0", "41", 0 )]
	[InlineData( "(*NOTEMPTY)(*MARK:A)(*ACCEPT)|a(*MARK:B)", "61", "0:1", "42", 0 )]
	[InlineData( "(*MARK:root)(?=a(*MARK:yes)(*ACCEPT))a", "61", "0:1", "796573", 0 )]
	[InlineData( "(*MARK:root)(?!a(*MARK:no)(*ACCEPT))a|a(*MARK:win)", "61", "0:1", "77696E", 0 )]
	[InlineData( "(*MARK:root)(?(?=a(*MARK:yes)(*ACCEPT))a|b)", "61", "0:1", "796573", 0 )]
	[InlineData( "(*MARK:root)(?(?!a(*MARK:no)(*ACCEPT))b|a)", "61", "0:1", "6E6F", 0 )]
	[InlineData( "(*MARK:X)(*COMMIT)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*COMMIT)(*MARK:X)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*MARK:X)(*THEN)(*COMMIT)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*MARK:X)a(*SKIP)(*F)|b(*MARK:Y)", "616162", "2:1", "59", 0 )]
	[InlineData( "(*sr:(*MARK:X)漢(*MARK:Y)字)", "E6BCA2E5AD97", "0:6", "59", 1 )]
	[InlineData( "(*asr:(*MARK:X)漢(*MARK:Y)字)", "E6BCA2E5AD97", "0:6", "59", 1 )]
	[InlineData( "(*sr:(*MARK:X)漢(*MARK:Y)a)|漢a(*MARK:Z)", "E6BCA261", "0:4", "5A", 1 )]
	[InlineData( "(*sr:(*MARK:X)漢(*ACCEPT)a)", "E6BCA261", "0:3", "58", 1 )]
	[InlineData( "(*MARK:X)\\X(*MARK:Y)", "C3A9CC81", "0:4", "59", 1 )]
	[InlineData( "(*ANYCRLF)(*MARK:A)(?s:.)\\B(*PRUNE)(*F)|(?s:(.))(*MARK:B)", "0D0A78", "2:1,2:1", "42", 0 )]
	[InlineData( "(*MARK:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx)a", "61", "0:1", "787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878787878", 0 )]
	[InlineData( "(*MARK:éééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééééX)a", "61", "0:1", "C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A9C3A958", 1 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(*F)|(?<v>a(*MARK:C))(*THEN)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a(*MARK:C))(?:X|(*THEN)b)))(?:(?&x(2))X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a(*MARK:C))(*THEN)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?:(?<v>a(*MARK:C))(*THEN)b|(*F))))(?:(?&x(2))X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?>(?:X|(?<v>a(*MARK:C))(*THEN)b))))(?:(?&x(2))X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?:(?<v>a(*MARK:C))(*THEN)b)))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a(*MARK:C))(?:X|(*THEN))))(?:(?&x(2))X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(*F)|(?<v>a(*MARK:C))(*THEN)b))(?:(?&x(2))X|ac)(?(v)X|d)", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "(*MARK:a\\)a", "61", "0:1", "615C", 0 )]
	[InlineData( "(*MARK:a:b)a", "61", "0:1", "613A62", 0 )]
	public void WinningPathRetainsTheNativeByteLabelAndCaptureTransactions( string pattern, string hex, string spans, string markHex, int mode ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var utf in mode == 0 ? new[] { false, true } : new[] { mode == 1 } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var reference = Reference( pattern, options ); AssertLiteral( reference, subject, spans );
			var native = reference.Match( subject );
			if ( native.Success ) { Assert.Equal( markHex, Convert.ToHexString( native.Mark ) ); }
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EverySelectedOffsetPreservesUtfFragmentsNewlinesAndWinningLabels( bool anyCrLf ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf };
			foreach ( var pattern in new[] { "(*MARK:X)a|b", "a(*MARK:X)c|b", "(*MARK:A)(?=a(*MARK:B))a|b",
				"(*MARK:A)(?!a(*MARK:B)c)a|b", "(*MARK:A)(?(?!a(*MARK:B))b|a)", "(*MARK:A)(?<=a(*MARK:B))b|()",
				"(*MARK:A)(*COMMIT)a", "(*COMMIT)(*MARK:A)é", "(*MARK:A)a(*SKIP)(*F)|b(*MARK:B)", "(*MARK:A)(*ACCEPT)" } ) {
				var reference = Reference( pattern, options ); var compiled = Compile( pattern, options );
				foreach ( var hex in new[] { "", "FF", "FFFF", "FF616162FF", "C3A9FF616162", "0D0A62", "616162FF0D0A62", "F09F9880FF62", "C378C3A9", "FFC378C3A9", "C3AAC3A9" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, utf={utf}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( "(*MARK)", 6 )]
	[InlineData( "(*MARK:)", 7 )]
	[InlineData( "(*:)", 3 )]
	[InlineData( "(*MARK", 6 )]
	[InlineData( "(*MARK:", 7 )]
	[InlineData( "(*:", 3 )]
	[InlineData( "(*MARK:x", 8 )]
	[InlineData( "(*:x", 4 )]
	public void MissingNamesAndTerminatorsRetainTheNativeByte( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(*MARK:x)*", 9 )]
	[InlineData( "(*MARK:x)?", 9 )]
	[InlineData( "(*MARK:x){0}", 9 )]
	[InlineData( "(*MARK:x){1,2}", 9 )]
	[InlineData( "(*MARK:x)(?#x)+", 14 )]
	[InlineData( "é(*MARK:x){65536}", 11 )]
	[InlineData( "(*:x)+", 5 )]
	[InlineData( "(*:x){0}", 5 )]
	[InlineData( @"(*MARK:x)\k<missing>", 9 )]
	public void DirectRepeatsAndGlobalErrorsRetainOriginalByteOffsets( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(*mark:x)" )]
	[InlineData( "(*MARKER:x)" )]
	[InlineData( "(*MARKx)" )]
	[InlineData( "(*MARK X:x)" )]
	public void NonExactMarkHeadersRemainUnsupported( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void NameLimitsCountBytesForBothHeadersAndUtfScalars( bool utf ) {
		var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
		foreach ( var header in new[] { "(*MARK:", "(*:" } ) {
			foreach ( var name in new[] { new string( 'x', 255 ), new string( 'x', 256 ), String.Concat( String.Concat( Enumerable.Repeat( "é", 127 ) ), "X" ), String.Concat( Enumerable.Repeat( "é", 128 ) ) } ) {
				var pattern = String.Concat( header, name, ")a" ); var subject = "a"u8.ToArray();
				var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
				if ( Encoding.UTF8.GetByteCount( name ) <= 255 ) {
					Assert.True( result.IsSuccess ); Compare( Reference( pattern, options ), subject, 0, result.Pattern!.Match( subject ), pattern );
					Assert.Equal( Encoding.UTF8.GetBytes( name ), Mark( result.Pattern.Match( subject ).Match! ).ToArray() );
				} else {
					Assert.Throws<PcrePatternException>( () => Reference( pattern, options ) );
					Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
					Assert.Equal( Encoding.UTF8.GetByteCount( String.Concat( header, name ) ), result.Diagnostic?.ByteOffset );
				}
			}
		}
	}

	[Theory]
	[InlineData( "282A4D41524B3AFF2961", "FF" )]
	[InlineData( "282A4D41524B3AFF00FE2961", "FF" )]
	[InlineData( "282A3A00FE2961", "" )]
	public void ByteNamesRetainRawOctetsAndNativeNulTermination( string patternHex, string markHex ) {
		var pattern = Convert.FromHexString( patternHex ); var subject = "a"u8.ToArray();
		var reference = Reference( pattern, new() ); var native = reference.Match( subject );
		Assert.True( native.Success ); Assert.Equal( markHex, Convert.ToHexString( native.Mark ) );
		var result = PcreByteCompiler.Compile( pattern ); Assert.True( result.IsSuccess );
		var actual = result.Pattern!.Match( subject ); Compare( reference, subject, 0, actual, patternHex );
		Assert.Equal( markHex, Convert.ToHexString( Mark( actual.Match! ).Span ) );
	}

	[Fact]
	public void InvalidUtfPatternNameIsRejectedBeforeHeaderParsing() {
		var pattern = Convert.FromHexString( "282A4D41524B3AFF2961" );
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var result = PcreByteCompiler.Compile( pattern, new() { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, result.Diagnostic?.Code );
		Assert.Equal( 7, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void MarksDoNotHideUnboundedBehindTails() {
		const string pattern = "(?<=(*MARK:x)a*)b";
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void SuccessfulLabelsDoNotConsumeSavedBacktrackFrames() {
		const int count = 1025;
		var pattern = $"\\A(?:a(*MARK:X)){{{count}}}"; var subject = Enumerable.Repeat( (byte)'a', count ).ToArray();
		Compare( Reference( pattern, new() ), subject, 0, Compile( pattern, new() ).Match( subject ), pattern );
		Assert.Equal( "58", Convert.ToHexString( Mark( Compile( pattern, new() ).Match( subject ).Match! ).Span ) );
	}

	[Fact]
	public void PathLabelSlotSharesThe256MarksWithNullableProgress() {
		foreach ( var count in new[] { 255, 256 } ) {
			var pattern = String.Concat( @"\A(*MARK:X)", String.Concat( Enumerable.Repeat( "(?:a?)*", count ) ) );
			Assert.True( Reference( pattern, new() ).Match( Array.Empty<byte>() ).Success );
			var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
			if ( count == 255 ) { Assert.True( result.IsSuccess ); Assert.Equal( "58", Convert.ToHexString( Mark( result.Pattern!.Match( Array.Empty<byte>() ).Match! ).Span ) ); }
			else { Assert.Null( result.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, result.Diagnostic?.Code ); }
		}
	}

	[Fact]
	public void GroupedMarkExpansionUsesTheSharedCompilationBudget() {
		var result = PcreByteCompiler.Compile( "(?:a(*MARK:X)){2000}"u8.ToArray() );
		Assert.Null( result.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, result.Diagnostic?.Code );
	}

	[Fact]
	public void MarkRollbackUsesSharedWorkAndCancellation() {
		const string pattern = @"\A(?:(*MARK:A)a?){20}(*MARK:Z)z|a(*MARK:W)";
		var subject = "a"u8.ToArray(); var steps = new List<int>();
		Compare( Reference( pattern, new() ), subject, 0, Compile( pattern, new() { WorkObserver = steps.Add } ).Match( subject ), pattern );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		var limited = Compile( pattern, new() { MatchWorkLimit = steps.Count - 1 } ).Match( subject );
		Assert.Null( limited.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { WorkObserver = count => { if ( count == 10 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void NamesRemainIndependentOfCallerPatternMutationAndLaterMatches() {
		var source = Encoding.ASCII.GetBytes( "a(*MARK:one)|b(*MARK:two)" );
		var result = PcreByteCompiler.Compile( source ); Assert.True( result.IsSuccess );
		Array.Fill( source, (byte)'x' );
		var first = result.Pattern!.Match( "a"u8.ToArray() ).Match!;
		var second = result.Pattern.Match( "b"u8.ToArray() ).Match!;
		Assert.Equal( "6F6E65", Convert.ToHexString( Mark( first ).Span ) );
		Assert.Equal( "74776F", Convert.ToHexString( Mark( second ).Span ) );
		Assert.Equal( "6F6E65", Convert.ToHexString( Mark( first ).Span ) );
	}

	// Read the evolving internal result boundary without adding production scaffolding before RED.
	private static ReadOnlyMemory<byte> Mark( PcreByteMatch match ) => Assert.IsType<ReadOnlyMemory<byte>>(
		typeof( PcreByteMatch ).GetProperty( "MarkName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )?.GetValue( match ) );

	private static void AssertLiteral( PcreRegex8Bit reference, byte[] subject, string spans ) {
		var match = reference.Match( subject );
		Assert.Equal( spans.Length > 0, match.Success );
		if ( !match.Success ) { return; }
		var groups = spans.Split( ',' );
		Assert.Equal( groups.Length, match.Groups.Count );
		for ( var group = 0; group < groups.Length; group++ ) {
			var capture = match.Groups[group];
			Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
		}
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Mark.ToArray(), Mark( actual.Match! ).ToArray() );
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, PcreByteOptions options ) => Reference( Encoding.UTF8.GetBytes( pattern ), options );
	private static PcreRegex8Bit Reference( byte[] pattern, PcreByteOptions options ) => new( pattern, Encoding.UTF8, new PcreRegexSettings {
		Options = PcreOptions.Compiled | ( options.Caseless ? PcreOptions.Caseless : PcreOptions.None ) | ( options.Utf8 || options.MatchInvalidUtf ? PcreOptions.Utf : PcreOptions.None )
			| ( options.Ucp ? PcreOptions.Ucp : PcreOptions.None ) | ( options.MatchInvalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ),
		NewLine = options.AnyCrLfNewline ? PcreNewLine.AnyCrLf : PcreNewLine.Lf,
		ExtraCompileOptions = options.AsciiBsD ? PcreExtraCompileOptions.AsciiBsD : PcreExtraCompileOptions.None,
		BackslashR = options.BsrAnyCrLf ? PcreBackslashR.AnyCrLf : PcreBackslashR.Unicode,
		ParensLimit = 512
	} );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
