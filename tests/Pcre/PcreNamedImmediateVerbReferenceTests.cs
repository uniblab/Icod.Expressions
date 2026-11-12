using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 10.48 named ACCEPT/FAIL/F MARK expansion, scope exits and byte labels.</summary>
public sealed class PcreNamedImmediateVerbReferenceTests {
	[Theory]
	[InlineData( "(*ACCEPT:N)b", "61", "0:0", "4E", 0 )]
	[InlineData( "a(*ACCEPT:N)b", "6162", "0:1", "4E", 0 )]
	[InlineData( "A((?:A|B(*ACCEPT:N)|C)D)", "4142", "0:2,1:1", "4E", 0 )]
	[InlineData( "A((?:A|B(*ACCEPT:N)|C)D)", "414144", "0:3,1:2", "", 0 )]
	[InlineData( "(a(b(*ACCEPT:N)c)d)e", "6162", "0:2,0:2,1:1", "4E", 0 )]
	[InlineData( "(?|(a(*ACCEPT:N)b)|(c))d", "61", "0:1,0:1", "4E", 0 )]
	[InlineData( "(?n)(a)(?<x>b(*ACCEPT:N)c)", "6162", "0:2,1:1", "4E", 0 )]
	[InlineData( "(?i)(?<x>É(*ACCEPT:N)z)", "C3A9", "0:2,0:2", "4E", 1 )]
	[InlineData( "(?x)(a (*ACCEPT:N) # tail\n b)", "61", "0:1,0:1", "4E", 0 )]
	[InlineData( "\\Q(*ACCEPT:N)\\E", "282A41434345505429", "", "", 0 )]
	[InlineData( "[(*ACCEPT:N)]", "41", "0:1", "", 0 )]
	[InlineData( "(a(*ACCEPT:N)??b)c", "616263", "0:3,0:2", "", 0 )]
	[InlineData( "(a(*ACCEPT:N)??b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(a(*ACCEPT:N)??b)c", "616278", "0:1,0:1", "4E", 0 )]
	[InlineData( "(a(*ACCEPT:N)?b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(?U)(a(*ACCEPT:N)?b)c", "616263", "0:3,0:2", "", 0 )]
	[InlineData( "(?U)(a(*ACCEPT:N)??b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "a(*ACCEPT:N){0}b", "6162", "0:2", "", 0 )]
	[InlineData( "a(*ACCEPT:N){0,2}?b", "6178", "0:1", "4E", 0 )]
	[InlineData( "a(*ACCEPT:N){2}b", "61", "0:1", "4E", 0 )]
	[InlineData( "a(*ACCEPT:N)*b", "61", "0:1", "4E", 0 )]
	[InlineData( "a(*ACCEPT:N)++b", "61", "0:1", "4E", 0 )]
	[InlineData( "a(*ACCEPT:N)?+b", "61", "0:1", "4E", 0 )]
	[InlineData( "(a(*ACCEPT:N)??b)c|abd", "616264", "0:1,0:1", "4E", 0 )]
	[InlineData( "a\\K(b(*ACCEPT:N)c)", "6162", "1:1,1:1", "4E", 0 )]
	[InlineData( "(a(*ACCEPT:N)b)|c", "63", "0:1,-", "", 0 )]
	[InlineData( "(*NOTEMPTY)(*ACCEPT:N)|a", "61", "0:1", "", 0 )]
	[InlineData( "(*NOTEMPTY)((*ACCEPT:N))|a", "61", "0:1,-", "", 0 )]
	[InlineData( "(*NOTEMPTY_ATSTART)(*ACCEPT:N)|b", "61", "1:0", "4E", 0 )]
	[InlineData( "(*NOTEMPTY)a\\K(*ACCEPT:N)|ab", "6162", "0:2", "", 0 )]
	[InlineData( "(?=(a(*ACCEPT:N)b))\\1", "61", "0:1,0:1", "4E", 0 )]
	[InlineData( "(?!(a(*ACCEPT:N)b))a|a", "61", "0:1,-", "", 0 )]
	[InlineData( "(a(?=(b(*ACCEPT:N)c))b)", "6162", "0:2,0:2,1:1", "4E", 0 )]
	[InlineData( "(?=(a(?=(b(*ACCEPT:N)c))b(*ACCEPT:N)c))ab", "6162", "0:2,0:2,1:1", "4E", 0 )]
	[InlineData( "(?(?=(a(*ACCEPT:N)b))\\1|x)", "61", "0:1,0:1", "4E", 0 )]
	[InlineData( "(?(?!(a(*ACCEPT:N)b))x|\\1)", "61", "0:1,0:1", "4E", 0 )]
	[InlineData( "(?(?=(a(*ACCEPT:N)b))X|a)|a", "61", "0:1,-", "", 0 )]
	[InlineData( "(?=(a(*ACCEPT:N)b))X|a", "61", "0:1,-", "", 0 )]
	[InlineData( "(?*(a(*ACCEPT:N)|ab))\\1c", "616263", "", "", 0 )]
	[InlineData( "(?*(a|ab)(*ACCEPT:N))\\1c", "616263", "", "", 0 )]
	[InlineData( "(?<=(a(*ACCEPT:N)z))x", "6178", "1:1,0:1", "4E", 0 )]
	[InlineData( "(?<=(a(*ACCEPT:N)b))x", "616278", "", "", 0 )]
	[InlineData( "(?<=(*ACCEPT:N)a*)x", "78", "0:1", "4E", 0 )]
	[InlineData( "(?<=(?:a(*ACCEPT:N))b)x", "616278", "2:1", "4E", 0 )]
	[InlineData( "(?<!(a(*ACCEPT:N)b))x|x", "6178", "1:1,-", "", 0 )]
	[InlineData( "(?<=(a{1,2}(*ACCEPT:N)b))x", "616178", "2:1,0:2", "4E", 0 )]
	[InlineData( "(?<=(a(*ACCEPT:N)(?<=b*)))x", "6178", "1:1,0:1", "4E", 0 )]
	[InlineData( "\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "616263", "", "", 0 )]
	[InlineData( "\\A(?&x)c(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "6163", "0:2,-", "4E", 0 )]
	[InlineData( "\\A(?&x)c|abc(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))", "616263", "0:3,-", "4E", 0 )]
	[InlineData( "\\A(?=(?&x))ac(?(DEFINE)(?<x>a(*ACCEPT:N)b))", "6163", "0:2,-", "4E", 0 )]
	[InlineData( "\\A(?&x)c(?(DEFINE)(?<x>(?=(a(*ACCEPT:N)b))a))", "6163", "0:2,-,-", "4E", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(a(*ACCEPT:N)b)))(?&x(2))\\2", "6161", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(a)(*ACCEPT:N)b))(?&x(2))\\2", "6161", "0:2,-,0:1", "4E", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a\\K(*ACCEPT:N)b))(?&x)c", "6163", "1:1,-", "4E", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*ACCEPT:N)b))(?&x)c", "616163", "0:3,-", "4E", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&y)z)(?<y>b(*ACCEPT:N)c))(?&x)d", "61627A64", "0:4,-,-", "4E", 0 )]
	[InlineData( "(*sr:あ漢가(*ACCEPT:N))X", "E38182E6BCA2EAB080", "0:9", "4E", 1 )]
	[InlineData( "(?=(*sr:あ漢가(*ACCEPT:N))X).+", "E38182E6BCA2EAB080", "0:9", "4E", 1 )]
	[InlineData( "(?>(*ACCEPT:N))X", "61", "0:0", "4E", 0 )]
	[InlineData( "(?=(?>(*ACCEPT:N))X)a", "61", "0:1", "4E", 0 )]
	[InlineData( "(?<x>a(*sr:b(*ACCEPT:N)c)d)", "6162", "0:2,0:2", "4E", 0 )]
	[InlineData( "(*FAIL:N)", "", "", "", 0 )]
	[InlineData( "(*F:N)", "58", "", "", 0 )]
	[InlineData( "(*FAIL:N)|", "", "0:0", "", 0 )]
	[InlineData( "(*F:N)|a", "61", "0:1", "", 0 )]
	[InlineData( "a(*FAIL:N)|ab", "6162", "0:2", "", 0 )]
	[InlineData( "(a)(*F:N)|(b)", "6162", "1:1,-,1:1", "", 0 )]
	[InlineData( "(a(*F:N)|ab)(c)", "616263", "0:3,0:2,2:1", "", 0 )]
	[InlineData( "(?>a(*F:N)|ab)c", "616263", "0:3", "", 0 )]
	[InlineData( "(?>a|ab)(*F:N)|abc", "616263", "0:3", "", 0 )]
	[InlineData( "(?=(a(*F:N)|ab))\\1", "6162", "0:2,0:2", "", 0 )]
	[InlineData( "(?!(a(*F:N)))a", "61", "0:1,-", "", 0 )]
	[InlineData( "(?=(a))(*F:N)|a", "61", "0:1,-", "", 0 )]
	[InlineData( "(?:a(*F:N))*b", "62", "0:1", "", 0 )]
	[InlineData( "(?:a(*F:N)){0}b", "62", "0:1", "", 0 )]
	[InlineData( "(?:a(*F:N))+", "61", "", "", 0 )]
	[InlineData( "\\A(?:a(*F:N)|ab)+\\z", "61626162", "0:4", "", 0 )]
	[InlineData( "\\A(a)?(?(1)(*F:N)|b)\\z", "62", "0:1,-", "", 0 )]
	[InlineData( "\\A(a)?(?(1)(*F:N)|b)\\z", "6162", "", "", 0 )]
	[InlineData( "(?(?=a)(*F:N)|b)", "62", "0:1", "", 0 )]
	[InlineData( "(?(?=a)(*F:N)|b)", "61", "", "", 0 )]
	[InlineData( "(?<x>a(*F:N)|b)(?&x)", "6262", "0:2,0:1", "", 0 )]
	[InlineData( "(?<x>a|b(*F:N))(?&x)|ab", "6162", "0:2,-", "", 0 )]
	[InlineData( "(?(DEFINE)(?<x>a(*F:N)|b))(?&x)", "62", "0:1,-", "", 0 )]
	[InlineData( "\\A(?<!(?&x))X(?(DEFINE)(?<x>(*F:N)(?&x)))", "58", "0:1,-", "", 0 )]
	[InlineData( "(?<=(*F:N)a*)X", "58", "", "", 0 )]
	[InlineData( "(?<!(*F:N)a*)X", "58", "0:1", "", 0 )]
	[InlineData( "(?<=a(*F:N)b*)X", "6158", "", "", 0 )]
	[InlineData( "(?<!a(*F:N)b*)X", "6158", "1:1", "", 0 )]
	[InlineData( "(?<=(*F:N)a*|ab)X", "616258", "2:1", "", 0 )]
	[InlineData( "(?<=(*F:N)(?<=a*))X", "58", "", "", 0 )]
	[InlineData( "(?<!(*F:N)(?=(?<=a*)))X", "58", "0:1", "", 0 )]
	[InlineData( "(?<=(?:a(*F:N)b*|bc))X", "626358", "2:1", "", 0 )]
	[InlineData( "(?<!(*F:N))(?<x>X)", "58", "0:1,0:1", "", 0 )]
	[InlineData( "(?*(a(*F:N)|ab))\\1c", "616263", "0:3,0:2", "", 0 )]
	[InlineData( "(*F:N)\\Q*\\E|X", "58", "0:1", "", 0 )]
	[InlineData( "\\Q(*FAIL:N)\\E", "282A4641494C29", "", "", 0 )]
	[InlineData( "[(*F:N)]+", "282A4629", "0:4", "", 0 )]
	[InlineData( "(*UTF)(*UCP)(?<x>é)(*F:N)|é", "C3A9", "0:2,-", "", 0 )]
	[InlineData( "a\\K(*F:N)|ab", "6162", "0:2", "", 0 )]
	[InlineData( "(a)(*F:N)|ab", "6162", "0:2,-", "", 0 )]
	[InlineData( "(?i)(*F:N)|k", "4B", "0:1", "", 0 )]
	[InlineData( "(*ACCEPT:N)", "61", "0:0", "4E", 0 )]
	[InlineData( "(*MARK:M)a(*ACCEPT:N)b", "6162", "0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)a(*ACCEPT:)b", "6162", "0:1", "4D", 0 )]
	[InlineData( "(*MARK:M)a(*ACCEPT:\u0000)b", "6162", "0:1", "", 0 )]
	[InlineData( "a(*ACCEPT:é漢)b", "6162", "0:1", "C3A9E6BCA2", 0 )]
	[InlineData( "(?x)a (*ACCEPT:n # x) b", "6162", "0:1", "6E20232078", 0 )]
	[InlineData( "a(*ACCEPT:\\x41)b", "6162", "0:1", "5C783431", 0 )]
	[InlineData( "a(*ACCEPT:N(*THEN)b", "6162", "0:1", "4E282A5448454E", 0 )]
	[InlineData( "\\Q(*ACCEPT:N)\\E", "282A4143434550543A4E29", "0:11", "", 0 )]
	[InlineData( "[(*ACCEPT:N)]", "4E", "0:1", "", 0 )]
	[InlineData( "a(*ACCEPT:N)|ab", "6162", "0:1", "4E", 0 )]
	[InlineData( "a(*MARK:M)(?!b(*ACCEPT:N))b", "6162", "", "", 0 )]
	[InlineData( "(?(?=a(*ACCEPT:N))a|b)", "6162", "0:1", "4E", 0 )]
	[InlineData( "(?(?!a(*ACCEPT:N))b|a)", "6162", "0:1", "4E", 0 )]
	[InlineData( "(?=(a(*ACCEPT:N)b))a|a", "61", "0:1,0:1", "4E", 0 )]
	[InlineData( "(?!(a(*ACCEPT:N)))a|a", "61", "0:1,-", "", 0 )]
	[InlineData( "(?*a(*ACCEPT:N))a|a", "61", "0:1", "4E", 0 )]
	[InlineData( "a(?*(*ACCEPT:N))(*SKIP:N)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "a(?=(*ACCEPT:N))(*SKIP:N)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "(?<=a(*ACCEPT:N))b|b", "6162", "1:1", "4E", 0 )]
	[InlineData( "(?<!a(*ACCEPT:N))b", "6162", "", "", 0 )]
	[InlineData( "(?<=(*ACCEPT:N)a*)b", "62", "0:1", "4E", 0 )]
	[InlineData( "(?<!(*ACCEPT:N)a*)b", "62", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*ACCEPT:N)b))(?&x)c|ac", "6163", "0:2,-", "4E", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(a)(*ACCEPT:N)b))(?&x(2))c|ac", "6163", "0:2,-,0:1", "4E", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*ACCEPT:N)b))(?&x)c|aac", "616163", "0:3,-", "4E", 0 )]
	[InlineData( "a+(*PRUNE)(*ACCEPT:N)|(a)", "61616162", "0:3,-", "4E", 0 )]
	[InlineData( "a+(*COMMIT)(*ACCEPT:N)|(a)", "61616162", "0:3,-", "4E", 0 )]
	[InlineData( "a+(*THEN)(*ACCEPT:N)|(a)", "61616162", "0:3,-", "4E", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?:a(*ACCEPT:N)){0}(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?:a(*ACCEPT:)){0}(*SKIP:X)(*F))|(.)", "61616263", "2:1,2:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?(DEFINE)(*ACCEPT:N))(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?=(*ACCEPT:N))(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "a\\K(*ACCEPT:N)|ab", "6162", "1:0", "4E", 0 )]
	[InlineData( "(*sr:a(*ACCEPT:N))X", "61", "0:1", "4E", 0 )]
	[InlineData( "(*asr:a(*ACCEPT:N))X", "61", "0:1", "4E", 0 )]
	[InlineData( "é(*ACCEPT:N)X|é", "C3A9", "0:2", "4E", 1 )]
	[InlineData( "(*FAIL:N)", "61", "", "", 0 )]
	[InlineData( "a(*FAIL:N)b", "6162", "", "", 0 )]
	[InlineData( "(*MARK:M)a(*FAIL:N)b", "6162", "", "", 0 )]
	[InlineData( "(*MARK:M)a(*FAIL:)b", "6162", "", "", 0 )]
	[InlineData( "(*MARK:M)a(*FAIL:\u0000)b", "6162", "", "", 0 )]
	[InlineData( "a(*FAIL:é漢)b", "6162", "", "", 0 )]
	[InlineData( "(?x)a (*FAIL:n # x) b", "6162", "", "", 0 )]
	[InlineData( "a(*FAIL:\\x41)b", "6162", "", "", 0 )]
	[InlineData( "a(*FAIL:N(*THEN)b", "6162", "", "", 0 )]
	[InlineData( "\\Q(*FAIL:N)\\E", "282A4641494C3A4E29", "0:9", "", 0 )]
	[InlineData( "[(*FAIL:N)]", "4E", "0:1", "", 0 )]
	[InlineData( "a(*MARK:M)(?!b(*FAIL:N))b", "6162", "0:2", "4D", 0 )]
	[InlineData( "(?(?=a(*FAIL:N))a|b)", "6162", "1:1", "", 0 )]
	[InlineData( "(?(?!a(*FAIL:N))b|a)", "6162", "1:1", "", 0 )]
	[InlineData( "(?=(a(*FAIL:N)b))a|a", "61", "0:1,-", "", 0 )]
	[InlineData( "(?!(a(*FAIL:N)))a|a", "61", "0:1,-", "", 0 )]
	[InlineData( "(?*a(*FAIL:N))a|a", "61", "0:1", "", 0 )]
	[InlineData( "a(?*(*FAIL:N))(*SKIP:N)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "a(?=(*FAIL:N))(*SKIP:N)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "(?<=a(*FAIL:N))b|b", "6162", "1:1", "", 0 )]
	[InlineData( "(?<!a(*FAIL:N))b", "6162", "1:1", "", 0 )]
	[InlineData( "(?<=(*FAIL:N)a*)b", "62", "", "", 0 )]
	[InlineData( "(?<!(*FAIL:N)a*)b", "62", "0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*FAIL:N)b))(?&x)c|ac", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(a)(*FAIL:N)b))(?&x(2))c|ac", "6163", "0:2,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*FAIL:N)b))(?&x)c|aac", "616163", "0:3,-", "", 0 )]
	[InlineData( "a+(*PRUNE)(*FAIL:N)|(a)", "61616162", "1:1,1:1", "", 0 )]
	[InlineData( "a+(*COMMIT)(*FAIL:N)|(a)", "61616162", "", "", 0 )]
	[InlineData( "a+(*THEN)(*FAIL:N)|(a)", "61616162", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?:a(*FAIL:N)){0}(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?:a(*FAIL:)){0}(*SKIP:X)(*F))|(.)", "61616263", "2:1,2:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?(DEFINE)(*FAIL:N))(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?=(*FAIL:N))(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "(*NOTEMPTY)(*FAIL:N)|a", "61", "0:1", "", 0 )]
	[InlineData( "(*NOTEMPTY_ATSTART)(*FAIL:N)|b", "61", "", "", 0 )]
	[InlineData( "a\\K(*FAIL:N)|ab", "6162", "0:2", "", 0 )]
	[InlineData( "(*sr:a(*FAIL:N))X", "61", "", "", 0 )]
	[InlineData( "(*asr:a(*FAIL:N))X", "61", "", "", 0 )]
	[InlineData( "é(*FAIL:N)X|é", "C3A9", "0:2", "", 1 )]
	[InlineData( "(*F:N)", "61", "", "", 0 )]
	[InlineData( "a(*F:N)b", "6162", "", "", 0 )]
	[InlineData( "(*MARK:M)a(*F:N)b", "6162", "", "", 0 )]
	[InlineData( "(*MARK:M)a(*F:)b", "6162", "", "", 0 )]
	[InlineData( "(*MARK:M)a(*F:\u0000)b", "6162", "", "", 0 )]
	[InlineData( "a(*F:é漢)b", "6162", "", "", 0 )]
	[InlineData( "(?x)a (*F:n # x) b", "6162", "", "", 0 )]
	[InlineData( "a(*F:\\x41)b", "6162", "", "", 0 )]
	[InlineData( "a(*F:N(*THEN)b", "6162", "", "", 0 )]
	[InlineData( "\\Q(*F:N)\\E", "282A463A4E29", "0:6", "", 0 )]
	[InlineData( "[(*F:N)]", "4E", "0:1", "", 0 )]
	[InlineData( "a(*F:N)|ab", "6162", "0:2", "", 0 )]
	[InlineData( "a(*MARK:M)(?!b(*F:N))b", "6162", "0:2", "4D", 0 )]
	[InlineData( "(?(?=a(*F:N))a|b)", "6162", "1:1", "", 0 )]
	[InlineData( "(?(?!a(*F:N))b|a)", "6162", "1:1", "", 0 )]
	[InlineData( "(?=(a(*F:N)b))a|a", "61", "0:1,-", "", 0 )]
	[InlineData( "(?!(a(*F:N)))a|a", "61", "0:1,-", "", 0 )]
	[InlineData( "(?*a(*F:N))a|a", "61", "0:1", "", 0 )]
	[InlineData( "a(?*(*F:N))(*SKIP:N)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "a(?=(*F:N))(*SKIP:N)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "(?<=a(*F:N))b|b", "6162", "1:1", "", 0 )]
	[InlineData( "(?<!a(*F:N))b", "6162", "1:1", "", 0 )]
	[InlineData( "(?<=(*F:N)a*)b", "62", "", "", 0 )]
	[InlineData( "(?<!(*F:N)a*)b", "62", "0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*F:N)b))(?&x)c|ac", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(a)(*F:N)b))(?&x(2))c|ac", "6163", "0:2,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*F:N)b))(?&x)c|aac", "616163", "0:3,-", "", 0 )]
	[InlineData( "a+(*PRUNE)(*F:N)|(a)", "61616162", "1:1,1:1", "", 0 )]
	[InlineData( "a+(*COMMIT)(*F:N)|(a)", "61616162", "", "", 0 )]
	[InlineData( "a+(*THEN)(*F:N)|(a)", "61616162", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?:a(*F:N)){0}(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?:a(*F:)){0}(*SKIP:X)(*F))|(.)", "61616263", "2:1,2:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?(DEFINE)(*F:N))(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?=(*F:N))(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "(*NOTEMPTY)(*F:N)|a", "61", "0:1", "", 0 )]
	[InlineData( "(*NOTEMPTY_ATSTART)(*F:N)|b", "61", "", "", 0 )]
	[InlineData( "(*sr:a(*F:N))X", "61", "", "", 0 )]
	[InlineData( "(*asr:a(*F:N))X", "61", "", "", 0 )]
	[InlineData( "é(*F:N)X|é", "C3A9", "0:2", "", 1 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)*b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)*b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)*?b)c", "616263", "0:3,0:2", "4D", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)*?b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)*+b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)*+b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)+b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)+b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)+?b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)+?b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)++b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)++b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)?b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)?b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)??b)c", "616263", "0:3,0:2", "4D", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)??b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)?+b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N)?+b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){0}b)c", "616263", "0:3,0:2", "4D", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){0}b)c", "6178", "", "", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){1}b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){1}b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){2}b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){2}b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){0,2}b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){0,2}b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){0,2}?b)c", "616263", "0:3,0:2", "4D", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){0,2}?b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){0,2}+b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){0,2}+b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){2,3}b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){2,3}b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){2,3}?b)c", "616263", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(a(*ACCEPT:N){2,3}?b)c", "6178", "0:1,0:1", "4E", 0 )]
	[InlineData( "(*MARK:M)(*ACCEPT:N){0}a", "616263", "0:1", "4D", 0 )]
	[InlineData( "(*MARK:M)(*ACCEPT:N)??a", "616263", "0:1", "4D", 0 )]
	[InlineData( "(*MARK:M)(*ACCEPT:N)??b", "616263", "0:0", "4E", 0 )]
	[InlineData( "(*ACCEPT:N){0}(*SKIP:N)(*F)|a", "616263", "0:1", "", 0 )]
	[InlineData( "(*ACCEPT:N)??a(*SKIP:N)(*F)|(.)", "616263", "0:0,-", "4E", 0 )]
	[InlineData( "(*ACCEPT:N){0}a(*SKIP:N)(*F)|(.)", "616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*ACCEPT:N){0}(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*ACCEPT:){0}(*SKIP:X)(*F))|(.)", "61616263", "2:1,2:1", "", 0 )]
	[InlineData( "a(*MARK:M)(?*b(*ACCEPT:N))bc", "616263", "0:3", "4E", 0 )]
	[InlineData( "a(*MARK:M)(?*(b(*ACCEPT:N)))bc", "616263", "0:3,1:1", "4E", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*ACCEPT:\u0000){0}(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*ACCEPT:\u0000)??(*SKIP:X)(*F))|(.)", "61616263", "0:2,-", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*FAIL:\u0000)a*(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*F:\u0000)a*(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	public void ImmediateNamedVerbsRetainPinnedLabelsCapturesAndScopes( string pattern, string hex, string spans, string markHex, int mode ) {
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
	public void SelectedOffsetsRetainAcceptanceFailureUtfFragmentsAndCrLfEntry( bool anyCrLf ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf };
			foreach ( var pattern in new[] { "(*ACCEPT:N)X", @"a\K((*ACCEPT:N))X|b", "(*NOTEMPTY)(*ACCEPT:N)|b", "(*NOTEMPTY_ATSTART)(*ACCEPT:N)|b",
				"(?=(a(*ACCEPT:N)b))a", "(?!(a(*FAIL:N)))a", "(?<=(*ACCEPT:N))b", "(?<!(*F:N)a*)b", "a(*ACCEPT:N)??b", "(*MARK:M)(*ACCEPT:N){0}a",
				"a(?*(*ACCEPT:N))(*SKIP:N)(*F)|(.)", "(*F:N)|()", "(a)(*FAIL:N)|b", "a+(*PRUNE)(*FAIL:N)|(a)" } ) {
				var reference = Reference( pattern, options ); var compiled = Compile( pattern, options );
				foreach ( var hex in new[] { "", "FF", "FFFF", "FF616162FF", "C3A9FF616162", "0D0A62", "616162FF0D0A62", "F09F9880FF62", "C378C3A9", "FFC378C3A9" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, utf={utf}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( "FAIL", 9 )]
	[InlineData( "F", 6 )]
	public void NamedFailureQuantifiersRemainInvalidAtTheOriginalByte( string verb, int length ) {
		foreach ( var suffix in new[] { "*", "+", "?", "{0}", "{1,2}", "{65536}" } ) {
			var pattern = $"(*{verb}:N){suffix}"; Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
			var actual = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
			Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code ); Assert.Equal( length, actual.Diagnostic?.ByteOffset );
		}
	}

	[Theory]
	[InlineData( @"(*ACCEPT:N)\k<missing>", 11, false )]
	[InlineData( @"(*FAIL:N)\2", 9, false )]
	[InlineData( @"(*F:N)\k<missing>", 6, false )]
	[InlineData( "(*ACCEPT:N)**", 12, false )]
	[InlineData( "é(*ACCEPT:N){65536}", 13, false )]
	[InlineData( "(?<=a*(*ACCEPT:N))x", 0, true )]
	[InlineData( "(*FAIL:N)(?<=a*)", 9, true )]
	[InlineData( @"(?<=(*F:N)\k<missing>)x", 10, false )]
	public void NamesDoNotHideGlobalErrorsOrEarlierUnboundedWidths( string pattern, int offset, bool width ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var actual = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.Equal( width ? PcreByteDiagnosticCode.UnsupportedSyntax : PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code ); Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "ACCEPT" )]
	[InlineData( "FAIL" )]
	[InlineData( "F" )]
	public void NamedHeaderEofAndExistingBareHeaderDiagnosticsStayDistinct( string verb ) {
		foreach ( var pattern in new[] { $"(*{verb}:", $"(*{verb}:N" } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
			var actual = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
			Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code ); Assert.Equal( pattern.Length, actual.Diagnostic?.ByteOffset );
		}
		var bare = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( $"(*{verb}" ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, bare.Diagnostic?.Code ); Assert.Equal( 1, bare.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "ACCEPT", false )]
	[InlineData( "ACCEPT", true )]
	[InlineData( "FAIL", false )]
	[InlineData( "FAIL", true )]
	[InlineData( "F", false )]
	[InlineData( "F", true )]
	public void NameByteLimitsRemainRawInBothModes( string verb, bool utf ) {
		var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
		foreach ( var name in new[] { new string( 'x', 255 ), new string( 'x', 256 ), String.Concat( String.Concat( Enumerable.Repeat( "é", 127 ) ), "X" ), String.Concat( Enumerable.Repeat( "é", 128 ) ) } ) {
			var pattern = $"(*{verb}:{name})a|a"; var bytes = Encoding.UTF8.GetBytes( pattern ); var actual = PcreByteCompiler.Compile( bytes, options );
			if ( Encoding.UTF8.GetByteCount( name ) <= 255 ) { Assert.True( actual.IsSuccess ); Compare( Reference( pattern, options ), "a"u8.ToArray(), 0, actual.Pattern!.Match( "a"u8.ToArray() ), verb ); }
			else {
				Assert.Throws<PcrePatternException>( () => Reference( pattern, options ) );
				Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code ); Assert.Equal( bytes.Length - 4, actual.Diagnostic?.ByteOffset );
			}
		}
	}

	[Theory]
	[InlineData( "ACCEPT" )]
	[InlineData( "FAIL" )]
	[InlineData( "F" )]
	public void RawByteAndNulNamesRetainPatternUtfValidation( string verb ) {
		foreach ( var name in new[] { "FF", "FF0041", "0041" } ) {
			var prefix = $"(*MARK:M)a(*{verb}:";
			var pattern = Encoding.ASCII.GetBytes( prefix ).Concat( Convert.FromHexString( name ) ).Concat( ")b|ab"u8.ToArray() ).ToArray();
			var actual = PcreByteCompiler.Compile( pattern ); Assert.True( actual.IsSuccess );
			Compare( Reference( pattern, new() ), "ab"u8.ToArray(), 0, actual.Pattern!.Match( "ab"u8.ToArray() ), verb );
			if ( verb == "ACCEPT" ) { Assert.Equal( name == "0041" ? "" : "FF", Convert.ToHexString( actual.Pattern.Match( "ab"u8.ToArray() ).Match!.MarkName.Span ) ); }
			if ( name[0] == 'F' ) {
				Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
				var strict = PcreByteCompiler.Compile( pattern, new() { Utf8 = true } );
				Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, strict.Diagnostic?.Code ); Assert.Equal( prefix.Length, strict.Diagnostic?.ByteOffset );
			}
		}
	}

	[Theory]
	[InlineData( "ACCEPT" )]
	[InlineData( "FAIL" )]
	[InlineData( "F" )]
	public void NonExactImmediateVerbHeadersRemainUnsupported( string verb ) {
		foreach ( var header in new[] { verb.ToLowerInvariant(), String.Concat( verb, "X" ), String.Concat( verb, " " ) } ) {
			var actual = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( $"(*{header}:N)" ) );
			Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, actual.Diagnostic?.Code ); Assert.Equal( 0, actual.Diagnostic?.ByteOffset );
		}
	}

	[Theory]
	[InlineData( "ACCEPT", 1023, true )]
	[InlineData( "ACCEPT", 1024, false )]
	[InlineData( "FAIL", 1022, true )]
	[InlineData( "FAIL", 1023, false )]
	[InlineData( "F", 1022, true )]
	[InlineData( "F", 1023, false )]
	public void ImplicitMarksShareTheExisting1024FrameBudget( string verb, int count, bool succeeds ) {
		var tail = verb == "ACCEPT" ? $"(*{verb}:N)" : $"(?!(*{verb}:N))";
		var pattern = $"\\A(?:a(*PRUNE)){{{count}}}{tail}(*SKIP:missing)"; var subject = Enumerable.Repeat( (byte)'a', count ).ToArray();
		AssertLiteral( Reference( String.Concat( "(*NO_JIT)", pattern ), new() ), subject, $"0:{count}" );
		var actual = Compile( pattern, new() ).Match( subject );
		if ( succeeds ) { Assert.True( actual.IsSuccess ); Assert.Equal( count, actual.Match?.ByteLength ); }
		else { Assert.Null( actual.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code ); }
	}

	[Theory]
	[InlineData( "ACCEPT" )]
	[InlineData( "FAIL" )]
	[InlineData( "F" )]
	public void CompletedAssertionScopesReleaseImplicitHistoryFrames( string verb ) {
		var scoped = verb == "ACCEPT" ? $"(?=(*{verb}:N))" : $"(?!(*{verb}:N))";
		var pattern = String.Concat( @"\A(*MARK:M)", String.Concat( Enumerable.Repeat( scoped, 300 ) ), String.Concat( Enumerable.Repeat( "(*PRUNE)", 800 ) ), "(*SKIP:N)" );
		var reference = Reference( String.Concat( "(*NO_JIT)", pattern ), new() ); AssertLiteral( reference, Array.Empty<byte>(), "0:0" );
		var actual = Compile( pattern, new() ).Match( Array.Empty<byte>() ); Assert.True( actual.IsSuccess ); Assert.Equal( 0, actual.Match?.ByteLength );
		Assert.Equal( verb == "ACCEPT" ? "4E" : "4D", Convert.ToHexString( actual.Match!.MarkName.Span ) );
	}

	[Theory]
	[InlineData( "ACCEPT" )]
	[InlineData( "FAIL" )]
	[InlineData( "F" )]
	public void ImmediateNamesShareTheExistingLabelSlotAndExpansionBounds( string verb ) {
		var scoped = verb == "ACCEPT" ? $"(?=(*{verb}:N))" : $"(?!(*{verb}:N))";
		foreach ( var count in new[] { 255, 256 } ) {
			var pattern = String.Concat( @"\A(*MARK:M)", scoped, String.Concat( Enumerable.Repeat( "(?:a?)*", count ) ) );
			Assert.True( Reference( String.Concat( "(*NO_JIT)", pattern ), new() ).Match( Array.Empty<byte>() ).Success );
			var actual = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
			if ( count == 255 ) { Assert.True( actual.IsSuccess ); Assert.True( actual.Pattern!.Match( Array.Empty<byte>() ).IsSuccess ); Assert.Equal( verb == "ACCEPT" ? "4E" : "4D", Convert.ToHexString( actual.Pattern.Match( Array.Empty<byte>() ).Match!.MarkName.Span ) ); }
			else { Assert.Null( actual.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, actual.Diagnostic?.Code ); }
		}
		var expanded = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( String.Concat( Enumerable.Repeat( $"(*{verb}:N)", 3000 ) ) ) );
		Assert.Null( expanded.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, expanded.Diagnostic?.Code );
	}

	[Theory]
	[InlineData( "ACCEPT" )]
	[InlineData( "FAIL" )]
	[InlineData( "F" )]
	public void EmptyAndErasedNamesNeedNoRuntimeLabelSlot( string verb ) {
		var emptyScope = verb == "ACCEPT" ? $"(?=(*{verb}:))" : $"(?!(*{verb}:))";
		foreach ( var first in new[] { emptyScope, $"(?:a(*{verb}:N)){{0}}" } ) {
			var pattern = String.Concat( @"\A", first, String.Concat( Enumerable.Repeat( "(?:a?)*", 256 ) ), "(*SKIP:missing)" );
			Assert.True( Reference( String.Concat( "(*NO_JIT)", pattern ), new() ).Match( Array.Empty<byte>() ).Success );
			var actual = Compile( pattern, new() ).Match( Array.Empty<byte>() ); Assert.True( actual.IsSuccess ); Assert.True( actual.Match!.MarkName.IsEmpty );
		}
	}

	[Theory]
	[InlineData( "ACCEPT" )]
	[InlineData( "FAIL" )]
	[InlineData( "F" )]
	public void ImplicitMarkTransitionsShareWorkLimitsAndCancellation( string verb ) {
		var named = verb == "ACCEPT" ? "(?=(a(a(*ACCEPT:N))))aa" : $"(?!(a(*{verb}:N)))a";
		var plain = named.Replace( String.Concat( verb, ":N" ), verb, StringComparison.Ordinal ); var subject = "aa"u8.ToArray();
		var steps = new List<int>(); var plainSteps = new List<int>();
		Compare( Reference( named, new() ), subject, 0, Compile( named, new() { WorkObserver = steps.Add } ).Match( subject ), named );
		Assert.True( Compile( plain, new() { WorkObserver = plainSteps.Add } ).Match( subject ).IsSuccess );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps ); Assert.True( steps.Count > plainSteps.Count );
		var limited = Compile( named, new() { MatchWorkLimit = steps.Count - 1 } ).Match( subject );
		Assert.Null( limited.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( named, new() { WorkObserver = step => { if ( step == 8 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( named ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void AcceptedLabelRetainsTheOwnedPatternSnapshot() {
		var source = "a(*ACCEPT:N)X"u8.ToArray(); var compiled = PcreByteCompiler.Compile( source ); Assert.True( compiled.IsSuccess ); Array.Fill( source, (byte)'z' );
		var actual = compiled.Pattern!.Match( "ab"u8.ToArray() ); Assert.True( actual.IsSuccess ); Assert.Equal( 0, actual.Match?.ByteIndex ); Assert.Equal( 1, actual.Match?.ByteLength );
		Assert.Equal( "4E", Convert.ToHexString( actual.Match!.MarkName.Span ) );
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
