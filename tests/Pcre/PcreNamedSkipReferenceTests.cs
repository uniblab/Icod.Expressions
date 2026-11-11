using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pinned PCRE2 10.48 JIT named SKIP history visibility, scoped cuts and byte captures.</summary>
public sealed class PcreNamedSkipReferenceTests {
	[Theory]
	[InlineData( "a(*MARK:K)(*SKIP:K)b", "7878616162", "3:2", "4B", 0 )]
	[InlineData( "a+(*MARK:K)(*SKIP:K)b", "616163616162", "3:3", "4B", 0 )]
	[InlineData( "\\A(?:a(*MARK:K)(*SKIP:K)b|ac)", "6163", "", "", 0 )]
	[InlineData( "(?:a(*MARK:K)(*SKIP:K)b|ac)", "6163", "", "", 0 )]
	[InlineData( "(?:a(*MARK:K)(*SKIP:K)b|ac)", "61636162", "2:2", "4B", 0 )]
	[InlineData( "(?:a(*MARK:K)(*SKIP:K)b|ac)", "6162", "0:2", "4B", 0 )]
	[InlineData( "(a|ab)(*MARK:K)(*SKIP:K)c", "616263", "", "", 0 )]
	[InlineData( "(a|ab)c", "616263", "0:3,0:2", "", 0 )]
	[InlineData( "a(*MARK:K)(*SKIP:K)(b|bc)c", "61626363", "0:3,1:1", "4B", 0 )]
	[InlineData( "(a)(*MARK:K)(*SKIP:K)X|(b)", "6162", "1:1,-,1:1", "", 0 )]
	[InlineData( "\\Q(*MARK:K)(*SKIP:K)\\E", "282A5052554E4529", "", "", 0 )]
	[InlineData( "[(*MARK:K)(*SKIP:K)]", "50", "0:1", "", 0 )]
	[InlineData( "(?x)a (*MARK:K)(*SKIP:K) # tail\n b", "6162", "0:2", "4B", 0 )]
	[InlineData( "(?i)a(*MARK:K)(*SKIP:K)b", "4142", "0:2", "4B", 0 )]
	[InlineData( "(?i)(é)(*MARK:K)(*SKIP:K)X|é", "C3A9C3A9", "", "", 1 )]
	[InlineData( "a\\K(*MARK:K)(*SKIP:K)X|(b)", "6162", "1:1,1:1", "", 0 )]
	[InlineData( "(*NOTEMPTY)(*MARK:K)(*SKIP:K)|a", "61", "", "", 0 )]
	[InlineData( "(*MARK:K)(*SKIP:K)(*ACCEPT)|a", "61", "0:0", "4B", 0 )]
	[InlineData( "(*NOTEMPTY)(*MARK:K)(*SKIP:K)(*ACCEPT)|a", "61", "", "", 0 )]
	[InlineData( "\\A(?:(?>a(*MARK:K)(*SKIP:K))b|ac)", "6163", "0:2", "", 0 )]
	[InlineData( "\\A(?:(?>a(*MARK:K)(*SKIP:K)b)|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?=(a(*MARK:K)(*SKIP:K)))ac|ac", "6163", "0:2,0:1", "4B", 0 )]
	[InlineData( "\\A(?=(a(*MARK:K)(*SKIP:K)b|a))a|a", "61", "", "", 0 )]
	[InlineData( "\\A(?!a(*MARK:K)(*SKIP:K)b|a)a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?=a(*MARK:K)(*SKIP:K)b|a)X|a)", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?!a(*MARK:K)(*SKIP:K)b|a)a|X)", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?!(a(*MARK:K)(*SKIP:K)b))a|X)", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?!(?=a(*MARK:K)(*SKIP:K)b|a)a)a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?=(?!a(*MARK:K)(*SKIP:K)b|a)a)a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?*(a(*MARK:K)(*SKIP:K)|ab))\\1c", "616263", "", "", 0 )]
	[InlineData( "\\A(?*(a|ab)(*MARK:K)(*SKIP:K))\\1c", "616263", "", "", 0 )]
	[InlineData( "\\A(?*(a(*MARK:K)(*SKIP:K)b|ab))ab", "6162", "0:2,0:2", "4B", 0 )]
	[InlineData( "(?<=(a(*MARK:K)(*SKIP:K)b))c", "616263", "2:1,0:2", "4B", 0 )]
	[InlineData( "(?<=(a(*MARK:K)(*SKIP:K)b|ab))c|c", "616263", "2:1,0:2", "4B", 0 )]
	[InlineData( "(?<=(a{1,2}(*MARK:K)(*SKIP:K)b))c", "61616263", "3:1,0:3", "4B", 0 )]
	[InlineData( "(?<!(a(*MARK:K)(*SKIP:K)b|a))c", "6163", "1:1,-", "", 0 )]
	[InlineData( "(?(?<=(a(*MARK:K)(*SKIP:K)b))c|X)", "616263", "2:1,0:2", "4B", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:K)(*SKIP:K)b|ac))(?&x)d|acd", "616364", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:K)(*SKIP:K)b|ac))(?&x)d", "616364", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a|ab))(?&x)(*MARK:K)(*SKIP:K)c|abc", "616263", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:K)(*SKIP:K)))(?&x)b|ac", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:K)(*SKIP:K)b))(?&x)c|aac", "616163", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*MARK:K)(*SKIP:K)b))(?&x)c|aabc", "61616263", "0:4,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&y)c)(?<y>b(*MARK:K)(*SKIP:K)X|b))(?&x)d|abcd", "61626364", "0:4,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=a(*MARK:K)(*SKIP:K)b|a)a))(?&x)X|a", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:K)(*SKIP:K)b|a))(?!(?&x))a", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:K)(*SKIP:K)b|a))(?(?=(?&x))X|a)", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:K)(*SKIP:K)b))(?:(?&x(2))X|ac)(?(v)Y|d)", "616364", "", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:K)(*SKIP:K)b)?ac|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:K)(*SKIP:K)b)*ac|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:K)(*SKIP:K)b){0}ac|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:K)(*SKIP:K))?b|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:K)(*SKIP:K))??b|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:K)(*SKIP:K))?+b|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:K)(*SKIP:K))++b|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:K)(*SKIP:K)b)+|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*MARK:K)(*SKIP:K)){2}b|aa)", "6161", "", "", 0 )]
	[InlineData( "\\A(*sr:(a(*MARK:K)(*SKIP:K)b|ac))|ac", "6163", "", "", 0 )]
	[InlineData( "\\A(*asr:(a(*MARK:K)(*SKIP:K)))b|ac", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?J)(?<x>a)(*MARK:K)(*SKIP:K)X|(?<x>ab)", "6162", "", "", 0 )]
	[InlineData( "\\A(?|(?<x>a(*MARK:K)(*SKIP:K)b)|(?<x>ac))", "6163", "", "", 0 )]
	[InlineData( "\\A(?=(a(*MARK:K)(*SKIP:K)b))a|a", "61", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:K)(*SKIP:K)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:K)(*SKIP:K)(?<v>b)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?<u>b)(*MARK:K)(*SKIP:K)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:K)(*SKIP:K)b))(?!(?&x(2)))ac(?(v)d|X)", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:K)(*SKIP:K)b))(?(?=(?&x(2)))X|ac)(?(v)d|X)", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:K)(*SKIP:K)b))(?:(?&x)X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:K)(*SKIP:K)(?<v>b)X|abc))(?:(?&x(2))Y|abc)d", "61626364", "0:4,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:K)(*SKIP:K)b))(?:(?&x(2))X|ac(*FAIL))|acd", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?!(?<v>a)(*MARK:K)(*SKIP:K)b)a))(?&x(2))", "61", "0:1,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?(?=(?<v>a)(*MARK:K)(*SKIP:K)b)X|a)))(?&x(2))", "61", "0:1,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=(?<v>a)(*MARK:K)(*SKIP:K)b)a))(?:(?&x(2))X|a)", "61", "0:1,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*MARK:K)(*SKIP:K)c))(?:(?&x(2))X|ab)d", "616264", "0:3,-,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*MARK:K)(*SKIP:K)c))(?:(?&x)X|ab)d", "616264", "0:3,-,-,-", "", 0 )]
	[InlineData( "a+(*MARK:K)(*SKIP:K)(*FAIL)|(a)", "61616162", "", "", 0 )]
	[InlineData( "a+(*PRUNE)(*FAIL)|(a)", "61616162", "1:1,1:1", "", 0 )]
	[InlineData( "aa(*MARK:K)(*SKIP:K)(*FAIL)|(a)", "616161", "2:1,2:1", "", 0 )]
	[InlineData( "aa(*MARK:K)(*SKIP:K)b|(a)", "6161616361616162", "2:1,2:1", "", 0 )]
	[InlineData( "\"[^\"]*\"(*MARK:K)(*SKIP:K)(*FAIL)|(\\w+)", "2268696464656E222073686F776E", "9:5,9:5", "", 0 )]
	[InlineData( "(?s:<.*?>)(*MARK:K)(*SKIP:K)(*F)|(\\w+)", "3C736B69703E20776F7264", "7:4,7:4", "", 0 )]
	[InlineData( "(a+)(*MARK:K)(*SKIP:K)X|(b)", "61616162", "3:1,-,3:1", "", 0 )]
	[InlineData( "(a+)\\K(*MARK:K)(*SKIP:K)X|(b)", "61616162", "3:1,-,3:1", "", 0 )]
	[InlineData( "(*MARK:K)(*SKIP:K)(*FAIL)|a", "6161", "", "", 0 )]
	[InlineData( "(*MARK:K)(*SKIP:K)(*FAIL)|()", "6161", "", "", 0 )]
	[InlineData( "(*NOTEMPTY_ATSTART)(*MARK:K)(*SKIP:K)(*ACCEPT)|a", "6161", "1:0", "4B", 0 )]
	[InlineData( "a+(*MARK:K)(*SKIP:K)(*FAIL)|()", "616161", "3:0,3:0", "", 0 )]
	[InlineData( "(?=a+(*MARK:K)(*SKIP:K)X)(.)|(b)", "61616162", "3:1,-,3:1", "", 0 )]
	[InlineData( "(?=a+(*MARK:K)(*SKIP:K)(*F))()", "616161", "", "", 0 )]
	[InlineData( "(?=a+(*MARK:K)(*SKIP:K)(*F))|(.)", "61616162", "3:1,3:1", "", 0 )]
	[InlineData( "(?*(a+(*MARK:K)(*SKIP:K)))\\1X|(b)", "61616162", "3:1,-,3:1", "", 0 )]
	[InlineData( "(?:(?>a+(*MARK:K)(*SKIP:K)))X|(a)", "616161", "0:1,0:1", "", 0 )]
	[InlineData( "(?:(?>a+(*MARK:K)(*SKIP:K)X))|(a)", "616161", "", "", 0 )]
	[InlineData( "(?<=(a)(*MARK:K)(*SKIP:K)b)X|(b)", "6162", "1:1,-,1:1", "", 0 )]
	[InlineData( "(?<=(a(*MARK:K)(*SKIP:K)b))X|(b)", "6162", "1:1,-,1:1", "", 0 )]
	[InlineData( "(?<=a(*MARK:K)(*SKIP:K))X|(b)", "616162", "2:1,2:1", "", 0 )]
	[InlineData( "(?<=(*MARK:K)(*SKIP:K)a)X|(a)", "6161", "0:1,0:1", "", 0 )]
	[InlineData( "(?<=a(*MARK:K)(*SKIP:K)b)X|(a)", "616261", "0:1,0:1", "", 0 )]
	[InlineData( "(?<=(a{1,2}(*MARK:K)(*SKIP:K)b))X|(b)", "616162", "", "", 0 )]
	[InlineData( "(?(?=a+(*MARK:K)(*SKIP:K)X)Y|a)", "616161", "0:1", "", 0 )]
	[InlineData( "(?!a+(*MARK:K)(*SKIP:K)X)a", "616161", "0:1", "", 0 )]
	[InlineData( "(?<=(?(?=a(*MARK:K)(*SKIP:K)X)a|b))c", "6263", "1:1", "", 0 )]
	[InlineData( "(a+(*MARK:K)(*SKIP:K)|b+(*PRUNE))X|(c)", "616162626263", "5:1,-,5:1", "", 0 )]
	[InlineData( "(a+(*PRUNE)|b+(*MARK:K)(*SKIP:K))X|(c)", "616162626263", "5:1,-,5:1", "", 0 )]
	[InlineData( "a+(*MARK:K)(*SKIP:K)(*F)|b+(*MARK:K)(*SKIP:K)(*F)|(c)", "61616162626263", "6:1,6:1", "", 0 )]
	[InlineData( "é+(*MARK:K)(*SKIP:K)(*F)|(.)", "C3A9C3A9E6BCA2", "4:3,4:3", "", 1 )]
	[InlineData( "(?=é+(*MARK:K)(*SKIP:K)X)(.)|(漢)", "C3A9C3A9E6BCA2", "4:3,-,4:3", "", 1 )]
	[InlineData( "(?<=é(*MARK:K)(*SKIP:K))X|(漢)", "C3A9E6BCA2", "2:3,2:3", "", 1 )]
	[InlineData( "é+\\K(*MARK:K)(*SKIP:K)(*F)|(.)", "C3A9C3A9E6BCA2", "4:3,4:3", "", 1 )]
	[InlineData( "(*UTF)(*UCP)\\X(*MARK:K)(*SKIP:K)X|(.)", "C3A961", "", "", 1 )]
	[InlineData( "(*sr:漢+)(*MARK:K)(*SKIP:K)(*F)|(.)", "E6BCA2E6BCA261", "6:1,6:1", "", 1 )]
	[InlineData( "(*UTF)(*UCP)é+(*MARK:K)(*SKIP:K)(*F)|()", "C3A9C3A9", "4:0,4:0", "", 1 )]
	[InlineData( "(*ANYCRLF)\\C(?<!\\R\\C)(*MARK:K)(*SKIP:K)(*FAIL)|(\\C)", "0D0A78", "2:1,2:1", "", 2 )]
	[InlineData( "(*CRLF)\\C(?<!\\R\\C)(*MARK:K)(*SKIP:K)(*FAIL)|(\\C)", "0D0A78", "2:1,2:1", "", 2 )]
	[InlineData( "(*ANYCRLF)(?s:.)(?<!\\R.)(*MARK:K)(*SKIP:K)(*FAIL)|(?s:(.))", "0D0A78", "2:1,2:1", "", 0 )]
	[InlineData( "(*ANYCRLF)\\r(*MARK:K)(*SKIP:K)(*FAIL)|(\\C)", "0D0A78", "1:1,1:1", "", 2 )]
	[InlineData( "(*ANYCRLF)\\C(*MARK:K)(*SKIP:K)(*FAIL)|()", "0D0A78", "3:0,3:0", "", 2 )]
	[InlineData( "(*ANYCRLF)\\C(?<!\\R\\C)(*PRUNE)(*FAIL)|(\\C)", "0D0A78", "2:1,2:1", "", 2 )]
	[InlineData( "(*ANYCRLF)(?s:.)(?<!\\R.)(*PRUNE)(*FAIL)|(?s:(.))", "0D0A78", "2:1,2:1", "", 0 )]
	[InlineData( "(*ANYCRLF)(?s:.)\\B(*MARK:K)(*SKIP:K)(*F)|(?s:(.))", "0D0A78", "1:1,1:1", "", 0 )]
	[InlineData( "(*ANYCRLF)(?s:.)\\B(*PRUNE)(*F)|(?s:(.))", "0D0A78", "2:1,2:1", "", 0 )]
	[InlineData( "(*CRLF)(?s:.)\\B(*MARK:K)(*SKIP:K)(*F)|(?s:(.))", "0D0A78", "1:1,1:1", "", 0 )]
	[InlineData( "(*ANY)(?s:.)\\B(*MARK:K)(*SKIP:K)(*F)|(?s:(.))", "0D0A78", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:X)b(*SKIP:X)(*F)|(.)", "616263", "1:1,1:1", "", 0 )]
	[InlineData( "a(*SKIP:X)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "\\A(a|ab)(*SKIP:missing)c", "616263", "0:3,0:2", "", 0 )]
	[InlineData( "\\A(?:a(*SKIP:missing)b|ac)", "6163", "0:2", "", 0 )]
	[InlineData( "a(?>(*MARK:X))(*SKIP:X)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "a(?:(*MARK:X))(*SKIP:X)(*F)|(.)", "616263", "1:1,1:1", "", 0 )]
	[InlineData( "a(?=(*MARK:X))(*SKIP:X)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "a(?*(*MARK:X))(*SKIP:X)(*F)|(.)", "616263", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:X)b(*MARK:X)c(*SKIP:X)(*F)|(.)", "61626364", "2:1,2:1", "", 0 )]
	[InlineData( "a(*MARK:X)b(*MARK:Y)c(*SKIP:X)(*F)|(.)", "61626364", "1:1,1:1", "", 0 )]
	[InlineData( "(?i)a(*MARK:X)b(*SKIP:x)(*F)|(.)", "61626364", "0:1,0:1", "", 0 )]
	[InlineData( "a(*MARK:X\u0000Y)b(*SKIP:X\u0000Z)(*F)|(.)", "61626364", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:\u0000)b(*SKIP:\u0000)(*F)|(.)", "616263", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:\u0000)b(*SKIP:\u0000z)(*F)|(.)", "616263", "1:1,1:1", "", 0 )]
	[InlineData( "(?x)a (*MARK:a # x) b (*SKIP:a # x) (*F)|(.)", "616263", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:é漢)b(*SKIP:é漢)(*F)|(.)", "616263", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:\\x41)b(*SKIP:\\x41)(*F)|(.)", "616263", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:X)b(*SKIP:)c|a", "6162", "", "", 0 )]
	[InlineData( "a(*SKIP:)b|ac", "6163", "", "", 0 )]
	[InlineData( "(*SKIP:)(*F)|a", "61", "", "", 0 )]
	[InlineData( "(*SKIP:)a", "7861", "1:1", "", 0 )]
	[InlineData( "(*SKIP:missing)a", "7861", "1:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:X)))(?&x)(*SKIP:X)(*F)|(.)", "616263", "0:1,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:X)))(?&x)(*SKIP:missing)b", "6162", "0:2,-", "58", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:X)))(?&x)(*SKIP:X)b", "6162", "0:2,-", "58", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*SKIP:X)b|ac))(?&x)d", "616364", "", "", 0 )]
	[InlineData( "(*MARK:X)\\A(?(DEFINE)(?<x>a(*SKIP:X)b|ac))(?&x)d|acd", "616364", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*SKIP:X)b|ac))(?&x)d|acd", "616364", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:X)(*SKIP:missing)b))(*MARK:caller)(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "63616C6C6572", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*MARK:X)(*SKIP:X)b))(*MARK:caller)(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "63616C6C6572", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*MARK:X)(?&x)?(*SKIP:X)b))(?&x)c|aabc", "61616263", "0:4,-", "", 0 )]
	[InlineData( "a(?!b(*SKIP:X)X)b|(.)", "616263", "0:2,-", "", 0 )]
	[InlineData( "a(?!b(*SKIP:X)c|b)b|(.)", "6162", "0:2,-", "", 0 )]
	[InlineData( "a(?=b(*SKIP:X)X)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "(*MARK:X)a(?(?=b(*SKIP:X)X)X|b)|(.)", "616263", "0:2,-", "58", 0 )]
	[InlineData( "(*MARK:X)a(?(?!b(*SKIP:X)X)b|X)|(.)", "616263", "0:2,-", "58", 0 )]
	[InlineData( "(*MARK:X)a(?=b(*MARK:Y)(*SKIP:X)X)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "(*MARK:X)a(?=b(*MARK:Y)(*SKIP:Y)X)|(.)", "616263", "2:1,2:1", "", 0 )]
	[InlineData( "a(?*(b(*MARK:X)))(*SKIP:X)(*F)|(.)", "61626364", "2:1,-,2:1", "", 0 )]
	[InlineData( "a(?*(b(*MARK:X)))(*SKIP:missing)(*F)|(.)", "61626364", "0:1,-,0:1", "", 0 )]
	[InlineData( "(?<=a(*MARK:X))b(*SKIP:X)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "(?<=a(*MARK:X)b)(*SKIP:X)X|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "(?<=(*MARK:X)a)b(*SKIP:X)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "(?<=a(*MARK:X)b(*SKIP:X))X|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "(?<=(*MARK:X)a(*SKIP:X))X|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "\\A(?:a(*MARK:X)(*COMMIT)b(*SKIP:X)X|abc)", "616263", "", "", 0 )]
	[InlineData( "\\A(?:a(*MARK:X)(*SKIP:X)b(*COMMIT)X|abc)", "616263", "", "", 0 )]
	[InlineData( "\\A(?:a(*MARK:X)(*PRUNE)b(*SKIP:X)X|abc)", "616263", "", "", 0 )]
	[InlineData( "\\A(?:a(*MARK:X)(*SKIP:X)b(*THEN)X|abc)", "616263", "0:3", "", 0 )]
	[InlineData( "\\A(?:a(*COMMIT)(*SKIP:missing)X|abc)", "616263", "", "", 0 )]
	[InlineData( "\\A(?:a(*MARK:X)b(*SKIP:X)(*ACCEPT)|abc)", "616263", "0:2", "58", 0 )]
	[InlineData( "(*MARK:X)(*SKIP:X)(*ACCEPT)", "61", "0:0", "58", 0 )]
	[InlineData( "(*NOTEMPTY)(*MARK:X)(*SKIP:X)(*ACCEPT)|a", "61", "", "", 0 )]
	[InlineData( "(*MARK:X)(*SKIP:missing)(*COMMIT)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*SKIP:)(*COMMIT)abc", "78797A616263", "3:3", "", 0 )]
	[InlineData( "(*MARK:X)(*PRUNE)(*COMMIT)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*MARK:X)(*COMMIT)(*SKIP:missing)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*MARK:X)(*COMMIT)(*SKIP:X)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*ANYCRLF)(?s:.)(*MARK:X)\\B(*SKIP:X)(*F)|(?s:(.))", "0D0A78", "1:1,1:1", "", 0 )]
	[InlineData( "(*CRLF)(?s:.)(*MARK:X)\\B(*SKIP:X)(*F)|(?s:(.))", "0D0A78", "1:1,1:1", "", 0 )]
	[InlineData( "(*ANYCRLF)(*MARK:X)(?s:.)\\B(*SKIP:X)(*F)|(?s:(.))", "0D0A78", "2:1,2:1", "", 0 )]
	[InlineData( "é(*MARK:名)é(*SKIP:名)(*F)|(.)", "C3A9C3A9E6BCA2", "2:2,2:2", "", 1 )]
	[InlineData( "(?=é(*MARK:名)é(*SKIP:名)X)(.)|(漢)", "C3A9C3A9E6BCA2", "4:3,-,4:3", "", 1 )]
	[InlineData( "(*sr:漢(*MARK:X))漢(*SKIP:X)(*F)|(.)", "E6BCA2E6BCA261", "3:3,3:3", "", 1 )]
	[InlineData( "(*asr:漢(*MARK:X))漢(*SKIP:X)(*F)|(.)", "E6BCA2E6BCA261", "0:3,0:3", "", 1 )]
	[InlineData( "(*UTF)(*UCP)\\X(*MARK:X)a(*SKIP:X)(*F)|(.)", "C3A96162", "2:1,2:1", "", 1 )]
	[InlineData( "(*MARK:X)(*MARK:Y)a(*SKIP:X)(*F)|(.)", "616263", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:X)(?:(*MARK:Y)b|c)(*SKIP:Y)(*F)|(.)", "616364", "0:1,0:1", "", 0 )]
	[InlineData( "a(*MARK:X)(?:(*MARK:Y)b|c)(*SKIP:X)(*F)|(.)", "616364", "1:1,1:1", "", 0 )]
	[InlineData( "\\Q(*SKIP:X)\\E", "282A534B49503A5829", "0:9", "", 0 )]
	[InlineData( "[(*SKIP:X)]", "58", "0:1", "", 0 )]
	[InlineData( "(*SKIP:missing)(*ACCEPT)", "61", "0:0", "", 0 )]
	[InlineData( "a(?(?=b(*MARK:X))b|c)(*SKIP:X)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "a(?(?!b(*MARK:X))c|b)(*SKIP:X)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "a(*MARK:X)(?:b(*MARK:Y))?c(*SKIP:Y)(*F)|(.)", "616364", "0:1,0:1", "", 0 )]
	[InlineData( "(*MARK:X)(*SKIP:X)(*ACCEPT)", "", "0:0", "58", 0 )]
	public void NamedSkipUsesVisibleMarkHistoryAndNativeLocalScopeExits( string pattern, string hex, string spans, string markHex, int mode ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var utf in mode == 0 ? new[] { false, true } : new[] { mode == 1 } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var reference = Reference( pattern, options ); AssertLiteral( reference, subject, spans );
			var native = reference.Match( subject ); if ( native.Success ) { Assert.Equal( markHex, Convert.ToHexString( native.Mark ) ); }
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void SelectedOffsetsRetainUtfFragmentsCrLfTargetsAndMarkTransactions( bool anyCrLf ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf };
			foreach ( var pattern in new[] { "a(*MARK:X)b(*SKIP:X)(*F)|(.)", "a(*SKIP:missing)b|ac", "(*MARK:X)(*SKIP:X)(*ACCEPT)",
				"a(?=(*MARK:X))(*SKIP:X)(*F)|(.)", "a(?*(*MARK:X))(*SKIP:X)(*F)|(.)", "(?<=a(*MARK:X)b(*SKIP:X))X|(.)",
				@"(?s:.)(*MARK:X)\B(*SKIP:X)(*F)|(?s:(.))", "(*SKIP:)(*F)|()", "(*SKIP:missing)(*COMMIT)a", "(*MARK:X)é(*SKIP:X)(*F)|(漢)" } ) {
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
	[InlineData( "(*SKIP:X)*", 9 )]
	[InlineData( "(*SKIP:X)?", 9 )]
	[InlineData( "(*SKIP:X){0}", 9 )]
	[InlineData( "(*SKIP:X){1,2}", 9 )]
	[InlineData( "(*SKIP:X)(?#x)+", 14 )]
	[InlineData( "é(*SKIP:X){65536}", 11 )]
	[InlineData( "(*SKIP:)*", 8 )]
	[InlineData( "(*SKIP:){0}", 8 )]
	[InlineData( @"(*SKIP:X)\k<missing>", 9 )]
	public void DirectRepeatsAndGlobalErrorsKeepOriginalByteOffsets( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(*SKIP", 6 )]
	[InlineData( "(*SKIP:", 7 )]
	[InlineData( "(*SKIP:X", 8 )]
	public void UnterminatedHeadersKeepOriginalEndByte( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code ); Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void NamesUseRawByteLimitsAndEmptyArgumentsUseUnnamedSkip( bool utf ) {
		var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
		foreach ( var name in new[] { new string( 'x', 255 ), new string( 'x', 256 ), String.Concat( String.Concat( Enumerable.Repeat( "é", 127 ) ), "X" ), String.Concat( Enumerable.Repeat( "é", 128 ) ) } ) {
			var pattern = String.Concat( "(*SKIP:", name, ")a" ); var bytes = Encoding.UTF8.GetBytes( pattern );
			var result = PcreByteCompiler.Compile( bytes, options );
			if ( Encoding.UTF8.GetByteCount( name ) <= 255 ) {
				Assert.True( result.IsSuccess ); Compare( Reference( pattern, options ), "a"u8.ToArray(), 0, result.Pattern!.Match( "a"u8.ToArray() ), pattern );
			} else {
				Assert.Throws<PcrePatternException>( () => Reference( pattern, options ) );
				Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code ); Assert.Equal( bytes.Length - 2, result.Diagnostic?.ByteOffset );
			}
		}
		Compare( Reference( "a(*SKIP:)b|ac", options ), "ac"u8.ToArray(), 0, Compile( "a(*SKIP:)b|ac", options ).Match( "ac"u8.ToArray() ), "empty colon is unnamed SKIP" );
	}

	[Theory]
	[InlineData( "(*skip:X)" )]
	[InlineData( "(*SKIP X:X)" )]
	[InlineData( "(*SKIPX:X)" )]
	[InlineData( "(*SKIPx)" )]
	public void NonExactHeadersRemainUnsupported( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code ); Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "61282A4D41524B3AFF2962282A534B49503AFF29282A46297C282E29" )]
	[InlineData( "61282A4D41524B3AFF00412962282A534B49503AFF004229282A46297C282E29" )]
	[InlineData( "61282A4D41524B3A00412962282A534B49503A004229282A46297C282E29" )]
	public void RawByteNamesAndNulPrefixesUseNativeByteEquality( string patternHex ) {
		var pattern = Convert.FromHexString( patternHex ); var subject = "abc"u8.ToArray();
		var result = PcreByteCompiler.Compile( pattern ); Assert.True( result.IsSuccess );
		Compare( Reference( pattern, new() ), subject, 0, result.Pattern!.Match( subject ), patternHex );
		Assert.Equal( 1, result.Pattern.Match( subject ).Match?.ByteIndex );
	}

	[Fact]
	public void NamedSkipDoesNotHideUnboundedBehindTails() {
		const string pattern = "(?<=(*SKIP:X)a*)b";
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code ); Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( 1023, true )]
	[InlineData( 1024, false )]
	public void HistoryAndNamedCutShareThe1024SavedFrameBudget( int count, bool succeeds ) {
		var pattern = $"\\A(?:a(*MARK:X)){{{count}}}(*SKIP:X)"; var subject = Enumerable.Repeat( (byte)'a', count ).ToArray();
		Assert.True( Reference( String.Concat( "(*NO_JIT)", pattern ), new() ).Match( subject ).Success );
		var actual = Compile( pattern, new() ).Match( subject );
		if ( succeeds ) { Assert.True( actual.IsSuccess ); Assert.Equal( count, actual.Match?.ByteLength ); }
		else { Assert.Null( actual.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code ); }
	}

	[Fact]
	public void IgnoredNamesNeedNoExtraFrameAtTheExistingLimit() {
		const string pattern = @"\A(?:a(*MARK:X)){1024}(*SKIP:missing)"; var subject = Enumerable.Repeat( (byte)'a', 1024 ).ToArray();
		Assert.True( Reference( String.Concat( "(*NO_JIT)", pattern ), new() ).Match( subject ).Success );
		var actual = Compile( pattern, new() ).Match( subject ); Assert.True( actual.IsSuccess ); Assert.Equal( 1024, actual.Match?.ByteLength );
	}

	[Theory]
	[InlineData( 511, true )]
	[InlineData( 512, false )]
	public void MarkHistorySharesItsBudgetWithExistingCuts( int count, bool succeeds ) {
		var pattern = $"\\A(?:a(*MARK:X)(*PRUNE)){{{count}}}(*SKIP:X)"; var subject = Enumerable.Repeat( (byte)'a', count ).ToArray();
		Assert.True( Reference( String.Concat( "(*NO_JIT)", pattern ), new() ).Match( subject ).Success );
		var actual = Compile( pattern, new() ).Match( subject );
		if ( succeeds ) { Assert.True( actual.IsSuccess ); Assert.Equal( count, actual.Match?.ByteLength ); }
		else { Assert.Null( actual.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code ); }
	}

	[Fact]
	public void HistoryRetainsTheSharedMarkAndCompilationLimits() {
		foreach ( var count in new[] { 255, 256 } ) {
			var pattern = String.Concat( @"\A(*MARK:X)", String.Concat( Enumerable.Repeat( "(?:a?)*", count ) ), "(*SKIP:missing)" );
			Assert.True( Reference( String.Concat( "(*NO_JIT)", pattern ), new() ).Match( Array.Empty<byte>() ).Success );
			var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
			if ( count == 255 ) { Assert.True( result.IsSuccess ); Assert.True( result.Pattern!.Match( Array.Empty<byte>() ).IsSuccess ); }
			else { Assert.Null( result.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, result.Diagnostic?.Code ); }
		}
		var expanded = PcreByteCompiler.Compile( "(?:a(*MARK:X)(*SKIP:X)){2000}"u8.ToArray() );
		Assert.Null( expanded.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, expanded.Diagnostic?.Code );
	}

	[Fact]
	public void HistorySearchChargesDistanceAndPollsCancellation() {
		var subject = Enumerable.Repeat( (byte)'a', 40 ).ToArray();
		var oldest = String.Concat( @"\A(*MARK:T)", String.Concat( Enumerable.Repeat( "a(*MARK:O)", 40 ) ), "(*SKIP:T)" );
		var newest = String.Concat( @"\A(*MARK:O)", String.Concat( Enumerable.Repeat( "a(*MARK:O)", 39 ) ), "a(*MARK:T)(*SKIP:T)" );
		var oldSteps = new List<int>(); var newSteps = new List<int>();
		Compare( Reference( oldest, new() ), subject, 0, Compile( oldest, new() { WorkObserver = oldSteps.Add } ).Match( subject ), oldest );
		Compare( Reference( newest, new() ), subject, 0, Compile( newest, new() { WorkObserver = newSteps.Add } ).Match( subject ), newest );
		Assert.Equal( Enumerable.Range( 1, oldSteps.Count ), oldSteps ); Assert.Equal( Enumerable.Range( 1, newSteps.Count ), newSteps );
		Assert.True( oldSteps.Count > newSteps.Count );
		var limited = Compile( oldest, new() { MatchWorkLimit = newSteps.Count } ).Match( subject );
		Assert.Null( limited.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( oldest, new() { WorkObserver = step => { if ( step == newSteps.Count ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( oldest ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void LookupNamesRemainOwnedAfterCallerPatternMutation() {
		var source = Encoding.ASCII.GetBytes( "a(*MARK:X)b(*SKIP:X)(*F)|(.)" ); var result = PcreByteCompiler.Compile( source ); Assert.True( result.IsSuccess );
		Array.Fill( source, (byte)'z' ); var actual = result.Pattern!.Match( "abc"u8.ToArray() );
		Assert.True( actual.IsSuccess ); Assert.Equal( 1, actual.Match?.ByteIndex ); Assert.Equal( 1, actual.Match?.ByteLength );
		Assert.Equal( new PcreByteCapture( 1, 1 ), actual.Match!.Captures[1] );
	}

	private static ReadOnlyMemory<byte> Mark( PcreByteMatch match ) => match.MarkName;

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
