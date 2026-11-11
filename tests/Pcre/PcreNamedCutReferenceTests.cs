using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pinned PCRE2 10.48 JIT named PRUNE/COMMIT/THEN labels and shared cut transactions.</summary>
public sealed class PcreNamedCutReferenceTests {
	[Theory]
	[InlineData( "a(*PRUNE:V)b", "7878616162", "3:2", "56", 0 )]
	[InlineData( "a+(*PRUNE:V)b", "616163616162", "3:3", "56", 0 )]
	[InlineData( "\\A(?:a(*PRUNE:V)b|ac)", "6163", "", "", 0 )]
	[InlineData( "(?:a(*PRUNE:V)b|ac)", "6163", "", "", 0 )]
	[InlineData( "(?:a(*PRUNE:V)b|ac)", "61636162", "2:2", "56", 0 )]
	[InlineData( "(?:a(*PRUNE:V)b|ac)", "6162", "0:2", "56", 0 )]
	[InlineData( "(a|ab)(*PRUNE:V)c", "616263", "", "", 0 )]
	[InlineData( "(a|ab)c", "616263", "0:3,0:2", "", 0 )]
	[InlineData( "a(*PRUNE:V)(b|bc)c", "61626363", "0:3,1:1", "56", 0 )]
	[InlineData( "(a)(*PRUNE:V)X|(b)", "6162", "1:1,-,1:1", "", 0 )]
	[InlineData( "\\Q(*PRUNE:V)\\E", "282A5052554E4529", "", "", 0 )]
	[InlineData( "[(*PRUNE:V)]", "50", "0:1", "", 0 )]
	[InlineData( "(?x)a (*PRUNE:V) # tail\n b", "6162", "0:2", "56", 0 )]
	[InlineData( "(?i)a(*PRUNE:V)b", "4142", "0:2", "56", 0 )]
	[InlineData( "(?i)(é)(*PRUNE:V)X|é", "C3A9C3A9", "", "", 1 )]
	[InlineData( "a\\K(*PRUNE:V)X|(b)", "6162", "1:1,1:1", "", 0 )]
	[InlineData( "(*NOTEMPTY)(*PRUNE:V)|a", "61", "", "", 0 )]
	[InlineData( "(*PRUNE:V)(*ACCEPT)|a", "61", "0:0", "56", 0 )]
	[InlineData( "(*NOTEMPTY)(*PRUNE:V)(*ACCEPT)|a", "61", "", "", 0 )]
	[InlineData( "\\A(?:(?>a(*PRUNE:V))b|ac)", "6163", "0:2", "", 0 )]
	[InlineData( "\\A(?:(?>a(*PRUNE:V)b)|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?=(a(*PRUNE:V)))ac|ac", "6163", "0:2,0:1", "56", 0 )]
	[InlineData( "\\A(?=(a(*PRUNE:V)b|a))a|a", "61", "", "", 0 )]
	[InlineData( "\\A(?!a(*PRUNE:V)b|a)a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?=a(*PRUNE:V)b|a)X|a)", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?!a(*PRUNE:V)b|a)a|X)", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?!(a(*PRUNE:V)b))a|X)", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?!(?=a(*PRUNE:V)b|a)a)a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?=(?!a(*PRUNE:V)b|a)a)a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?*(a(*PRUNE:V)|ab))\\1c", "616263", "", "", 0 )]
	[InlineData( "\\A(?*(a|ab)(*PRUNE:V))\\1c", "616263", "", "", 0 )]
	[InlineData( "\\A(?*(a(*PRUNE:V)b|ab))ab", "6162", "0:2,0:2", "56", 0 )]
	[InlineData( "(?<=(a(*PRUNE:V)b))c", "616263", "2:1,0:2", "56", 0 )]
	[InlineData( "(?<=(a(*PRUNE:V)b|ab))c|c", "616263", "2:1,0:2", "56", 0 )]
	[InlineData( "(?<=(a{1,2}(*PRUNE:V)b))c", "61616263", "3:1,0:3", "56", 0 )]
	[InlineData( "(?<!(a(*PRUNE:V)b|a))c", "6163", "1:1,-", "", 0 )]
	[InlineData( "(?(?<=(a(*PRUNE:V)b))c|X)", "616263", "2:1,0:2", "56", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE:V)b|ac))(?&x)d|acd", "616364", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE:V)b|ac))(?&x)d", "616364", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a|ab))(?&x)(*PRUNE:V)c|abc", "616263", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE:V)))(?&x)b|ac", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE:V)b))(?&x)c|aac", "616163", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*PRUNE:V)b))(?&x)c|aabc", "61616263", "0:4,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&y)c)(?<y>b(*PRUNE:V)X|b))(?&x)d|abcd", "61626364", "0:4,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=a(*PRUNE:V)b|a)a))(?&x)X|a", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE:V)b|a))(?!(?&x))a", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE:V)b|a))(?(?=(?&x))X|a)", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE:V)b))(?:(?&x(2))X|ac)(?(v)Y|d)", "616364", "", "", 0 )]
	[InlineData( "\\A(?:(a(*PRUNE:V)b)?ac|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*PRUNE:V)b)*ac|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*PRUNE:V)b){0}ac|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*PRUNE:V))?b|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*PRUNE:V))??b|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*PRUNE:V))?+b|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*PRUNE:V))++b|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*PRUNE:V)b)+|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*PRUNE:V)){2}b|aa)", "6161", "", "", 0 )]
	[InlineData( "\\A(*sr:(a(*PRUNE:V)b|ac))|ac", "6163", "", "", 0 )]
	[InlineData( "\\A(*asr:(a(*PRUNE:V)))b|ac", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?J)(?<x>a)(*PRUNE:V)X|(?<x>ab)", "6162", "", "", 0 )]
	[InlineData( "\\A(?|(?<x>a(*PRUNE:V)b)|(?<x>ac))", "6163", "", "", 0 )]
	[InlineData( "\\A(?=(a(*PRUNE:V)b))a|a", "61", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE:V)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE:V)(?<v>b)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?<u>b)(*PRUNE:V)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE:V)b))(?!(?&x(2)))ac(?(v)d|X)", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE:V)b))(?(?=(?&x(2)))X|ac)(?(v)d|X)", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE:V)b))(?:(?&x)X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE:V)(?<v>b)X|abc))(?:(?&x(2))Y|abc)d", "61626364", "0:4,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE:V)b))(?:(?&x(2))X|ac(*FAIL))|acd", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?!(?<v>a)(*PRUNE:V)b)a))(?&x(2))", "61", "0:1,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?(?=(?<v>a)(*PRUNE:V)b)X|a)))(?&x(2))", "61", "0:1,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=(?<v>a)(*PRUNE:V)b)a))(?:(?&x(2))X|a)", "61", "0:1,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*PRUNE:V)c))(?:(?&x(2))X|ab)d", "616264", "0:3,-,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*PRUNE:V)c))(?:(?&x)X|ab)d", "616264", "0:3,-,-,-", "", 0 )]
	[InlineData( "a(*COMMIT:V)b", "7878616162", "", "", 0 )]
	[InlineData( "a+(*COMMIT:V)b", "616163616162", "", "", 0 )]
	[InlineData( "\\A(?:a(*COMMIT:V)b|ac)", "6163", "", "", 0 )]
	[InlineData( "(?:a(*COMMIT:V)b|ac)", "6163", "", "", 0 )]
	[InlineData( "(?:a(*COMMIT:V)b|ac)", "61636162", "", "", 0 )]
	[InlineData( "(?:a(*COMMIT:V)b|ac)", "6162", "0:2", "56", 0 )]
	[InlineData( "(a|ab)(*COMMIT:V)c", "616263", "", "", 0 )]
	[InlineData( "a(*COMMIT:V)(b|bc)c", "61626363", "0:3,1:1", "56", 0 )]
	[InlineData( "(a)(*COMMIT:V)X|(b)", "6162", "", "", 0 )]
	[InlineData( "\\Q(*COMMIT:V)\\E", "282A5052554E4529", "", "", 0 )]
	[InlineData( "[(*COMMIT:V)]", "50", "", "", 0 )]
	[InlineData( "(?x)a (*COMMIT:V) # tail\n b", "6162", "0:2", "56", 0 )]
	[InlineData( "(?i)a(*COMMIT:V)b", "4142", "0:2", "56", 0 )]
	[InlineData( "(?i)(é)(*COMMIT:V)X|é", "C3A9C3A9", "", "", 1 )]
	[InlineData( "a\\K(*COMMIT:V)X|(b)", "6162", "", "", 0 )]
	[InlineData( "(*NOTEMPTY)(*COMMIT:V)|a", "61", "", "", 0 )]
	[InlineData( "(*COMMIT:V)(*ACCEPT)|a", "61", "0:0", "56", 0 )]
	[InlineData( "(*NOTEMPTY)(*COMMIT:V)(*ACCEPT)|a", "61", "", "", 0 )]
	[InlineData( "\\A(?:(?>a(*COMMIT:V))b|ac)", "6163", "0:2", "", 0 )]
	[InlineData( "\\A(?:(?>a(*COMMIT:V)b)|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?=(a(*COMMIT:V)))ac|ac", "6163", "0:2,0:1", "56", 0 )]
	[InlineData( "\\A(?=(a(*COMMIT:V)b|a))a|a", "61", "", "", 0 )]
	[InlineData( "\\A(?!a(*COMMIT:V)b|a)a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?=a(*COMMIT:V)b|a)X|a)", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?!a(*COMMIT:V)b|a)a|X)", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?!(a(*COMMIT:V)b))a|X)", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?!(?=a(*COMMIT:V)b|a)a)a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?=(?!a(*COMMIT:V)b|a)a)a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?*(a(*COMMIT:V)|ab))\\1c", "616263", "", "", 0 )]
	[InlineData( "\\A(?*(a|ab)(*COMMIT:V))\\1c", "616263", "", "", 0 )]
	[InlineData( "\\A(?*(a(*COMMIT:V)b|ab))ab", "6162", "0:2,0:2", "56", 0 )]
	[InlineData( "(?<=(a(*COMMIT:V)b))c", "616263", "2:1,0:2", "56", 0 )]
	[InlineData( "(?<=(a(*COMMIT:V)b|ab))c|c", "616263", "2:1,0:2", "56", 0 )]
	[InlineData( "(?<=(a{1,2}(*COMMIT:V)b))c", "61616263", "3:1,0:3", "56", 0 )]
	[InlineData( "(?<!(a(*COMMIT:V)b|a))c", "6163", "1:1,-", "", 0 )]
	[InlineData( "(?(?<=(a(*COMMIT:V)b))c|X)", "616263", "2:1,0:2", "56", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT:V)b|ac))(?&x)d|acd", "616364", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT:V)b|ac))(?&x)d", "616364", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a|ab))(?&x)(*COMMIT:V)c|abc", "616263", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT:V)))(?&x)b|ac", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT:V)b))(?&x)c|aac", "616163", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*COMMIT:V)b))(?&x)c|aabc", "61616263", "0:4,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&y)c)(?<y>b(*COMMIT:V)X|b))(?&x)d|abcd", "61626364", "0:4,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=a(*COMMIT:V)b|a)a))(?&x)X|a", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT:V)b|a))(?!(?&x))a", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT:V)b|a))(?(?=(?&x))X|a)", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT:V)b))(?:(?&x(2))X|ac)(?(v)Y|d)", "616364", "", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT:V)b)?ac|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT:V)b)*ac|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT:V)b){0}ac|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT:V))?b|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT:V))??b|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT:V))?+b|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT:V))++b|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT:V)b)+|ac)", "6163", "", "", 0 )]
	[InlineData( "\\A(?:(a(*COMMIT:V)){2}b|aa)", "6161", "", "", 0 )]
	[InlineData( "\\A(*sr:(a(*COMMIT:V)b|ac))|ac", "6163", "", "", 0 )]
	[InlineData( "\\A(*asr:(a(*COMMIT:V)))b|ac", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?J)(?<x>a)(*COMMIT:V)X|(?<x>ab)", "6162", "", "", 0 )]
	[InlineData( "\\A(?|(?<x>a(*COMMIT:V)b)|(?<x>ac))", "6163", "", "", 0 )]
	[InlineData( "\\A(?=(a(*COMMIT:V)b))a|a", "61", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT:V)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT:V)(?<v>b)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?<u>b)(*COMMIT:V)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT:V)b))(?!(?&x(2)))ac(?(v)d|X)", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT:V)b))(?(?=(?&x(2)))X|ac)(?(v)d|X)", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT:V)b))(?:(?&x)X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT:V)(?<v>b)X|abc))(?:(?&x(2))Y|abc)d", "61626364", "0:4,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT:V)b))(?:(?&x(2))X|ac(*FAIL))|acd", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?!(?<v>a)(*COMMIT:V)b)a))(?&x(2))", "61", "0:1,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?(?=(?<v>a)(*COMMIT:V)b)X|a)))(?&x(2))", "61", "0:1,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=(?<v>a)(*COMMIT:V)b)a))(?:(?&x(2))X|a)", "61", "0:1,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*COMMIT:V)c))(?:(?&x(2))X|ab)d", "616264", "0:3,-,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*COMMIT:V)c))(?:(?&x)X|ab)d", "616264", "0:3,-,-,-", "", 0 )]
	[InlineData( "(*COMMIT:V)", "61", "0:0", "56", 0 )]
	[InlineData( "(*COMMIT:V)abc", "78797A616263", "3:3", "56", 0 )]
	[InlineData( "(*COMMIT:V)abc", "7861616263", "", "", 0 )]
	[InlineData( "(*COMMIT:V)a|b", "7861", "", "", 0 )]
	[InlineData( "(*COMMIT:V)a|b", "7862", "", "", 0 )]
	[InlineData( "(*COMMIT:V)(a|b)", "7861", "", "", 0 )]
	[InlineData( "(*COMMIT:V)[ab]", "7861", "", "", 0 )]
	[InlineData( "(*COMMIT:V).", "0A61", "", "", 0 )]
	[InlineData( "(*COMMIT:V)(a)", "7861", "1:1,1:1", "56", 0 )]
	[InlineData( "(?:(*COMMIT:V)a)", "7861", "1:1", "56", 0 )]
	[InlineData( "(*COMMIT:V)a+", "786161", "1:2", "56", 0 )]
	[InlineData( "(*COMMIT:V)a{1,3}", "786161", "1:2", "56", 0 )]
	[InlineData( "(*COMMIT:V)a+?", "786161", "1:1", "56", 0 )]
	[InlineData( "(*COMMIT:V)a*", "7861", "0:0", "56", 0 )]
	[InlineData( "(*COMMIT:V)(a*)b", "786162", "", "", 0 )]
	[InlineData( "(*COMMIT:V)(?=a)a", "7861", "1:1", "56", 0 )]
	[InlineData( "(?=(*COMMIT:V)a)a", "7861", "1:1", "56", 0 )]
	[InlineData( "(?=(*COMMIT:V))a", "7861", "1:1", "56", 0 )]
	[InlineData( "(*COMMIT:V)(?:)(a)", "7861", "1:1,1:1", "56", 0 )]
	[InlineData( "(*COMMIT:V)\\Ka", "7861", "1:1", "56", 0 )]
	[InlineData( "(?i)(*COMMIT:V)a", "7841", "1:1", "56", 0 )]
	[InlineData( "(*COMMIT:V)(?i:a)", "7841", "1:1", "56", 0 )]
	[InlineData( "(?i)(*COMMIT:V)k", "784B", "1:1", "56", 2 )]
	[InlineData( "(?i)(*COMMIT:V)k", "784B", "", "", 1 )]
	[InlineData( "(?i)(*COMMIT:V)s", "7853", "1:1", "56", 2 )]
	[InlineData( "(?i)(*COMMIT:V)s", "7853", "", "", 1 )]
	[InlineData( "(?ir)(*COMMIT:V)k", "784B", "1:1", "56", 0 )]
	[InlineData( "(?ir)(*COMMIT:V)s", "7853", "1:1", "56", 0 )]
	[InlineData( "(*COMMIT:V)é", "78C3A9", "1:2", "56", 0 )]
	[InlineData( "(*COMMIT:V)é", "C3AAC3A9", "", "", 0 )]
	[InlineData( "(*COMMIT:V)éa", "C3AA61C3A961", "", "", 0 )]
	[InlineData( "(?i)(*COMMIT:V)σ", "78CEA3", "", "", 0 )]
	[InlineData( "a+(*COMMIT:V)b", "7878616162", "2:3", "56", 0 )]
	[InlineData( "a+(*COMMIT:V)(*F)|(a)", "61616162", "", "", 0 )]
	[InlineData( "a+(*COMMIT:V)(*PRUNE)X|(a)", "616161", "1:1,1:1", "", 0 )]
	[InlineData( "a+(*PRUNE)(*COMMIT:V)X|(a)", "616161", "", "", 0 )]
	[InlineData( "a+(*COMMIT:V)b(*SKIP)X|(b)", "6161616262", "4:1,4:1", "", 0 )]
	[InlineData( "a+(*COMMIT:V)b(*PRUNE)X|(b)", "6161616262", "3:1,3:1", "", 0 )]
	[InlineData( "a+(*SKIP)(*COMMIT:V)X|(a)", "616161", "", "", 0 )]
	[InlineData( "(*COMMIT:V)(*SKIP)(*F)|()", "61", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a\\K(*COMMIT:V)b))(?:(?&x)X|ac)d", "616364", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT:V)))(?:(?&x(2))b|ac)d", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT:V)))(?:(?&x(2))b|ac)d", "616264", "0:3,-,0:1", "56", 0 )]
	[InlineData( "(?<=a(*COMMIT:V))X|(b)", "616162", "2:1,2:1", "", 0 )]
	[InlineData( "(?<=(*COMMIT:V)a)X|(a)", "6161", "0:1,0:1", "", 0 )]
	[InlineData( "(?<=(a{1,2}(*COMMIT:V)b))X|(b)", "616162", "", "", 0 )]
	[InlineData( "(?(?=a+(*COMMIT:V)X)Y|a)", "616161", "0:1", "", 0 )]
	[InlineData( "(?!a+(*COMMIT:V)X)a", "616161", "0:1", "", 0 )]
	[InlineData( "(?*(a+(*COMMIT:V)))\\1X|(b)", "61616162", "", "", 0 )]
	[InlineData( "é+(*COMMIT:V)(*F)|(.)", "C3A9C3A9E6BCA2", "", "", 1 )]
	[InlineData( "(?<=é(*COMMIT:V))X|(漢)", "C3A9E6BCA2", "2:3,2:3", "", 1 )]
	[InlineData( "(*UTF)(*UCP)\\X(*COMMIT:V)X|(.)", "C3A961", "", "", 1 )]
	[InlineData( "(*sr:漢+)(*COMMIT:V)(*F)|(.)", "E6BCA2E6BCA261", "", "", 1 )]
	[InlineData( "(*ANYCRLF)(*COMMIT:V)\\n", "0D0A", "1:1", "56", 0 )]
	[InlineData( "(*CRLF)(*COMMIT:V)(?s:.)", "0D0A78", "0:1", "56", 0 )]
	[InlineData( "(*ANYCRLF)(?s:.)\\B(*COMMIT:V)(*F)|(?s:(.))", "0D0A78", "", "", 0 )]
	[InlineData( "(*COMMIT:V)(a)(?:(*PRUNE)b|ac)", "616162", "1:2,1:1", "56", 0 )]
	[InlineData( "a(*THEN:V)b", "7878616162", "3:2", "56", 0 )]
	[InlineData( "a+(*THEN:V)b", "616163616162", "3:3", "56", 0 )]
	[InlineData( "\\A(?:a(*THEN:V)b|ac)", "6163", "0:2", "", 0 )]
	[InlineData( "(?:a(*THEN:V)b|ac)", "6163", "0:2", "", 0 )]
	[InlineData( "(?:a(*THEN:V)b|ac)", "61636162", "0:2", "", 0 )]
	[InlineData( "(?:a(*THEN:V)b|ac)", "6162", "0:2", "56", 0 )]
	[InlineData( "(a|ab)(*THEN:V)c", "616263", "", "", 0 )]
	[InlineData( "a(*THEN:V)(b|bc)c", "61626363", "0:3,1:1", "56", 0 )]
	[InlineData( "(a)(*THEN:V)X|(b)", "6162", "1:1,-,1:1", "", 0 )]
	[InlineData( "\\Q(*THEN:V)\\E", "282A5448454E29", "", "", 0 )]
	[InlineData( "[(*THEN:V)]", "48", "0:1", "", 0 )]
	[InlineData( "(?x)a (*THEN:V) # tail\n b", "6162", "0:2", "56", 0 )]
	[InlineData( "(?i)a(*THEN:V)b", "4142", "0:2", "56", 0 )]
	[InlineData( "(?i)(é)(*THEN:V)X|é", "C3A9C3A9", "0:2,-", "", 1 )]
	[InlineData( "a\\K(*THEN:V)X|(b)", "6162", "1:1,1:1", "", 0 )]
	[InlineData( "(*NOTEMPTY)(*THEN:V)|a", "61", "0:1", "", 0 )]
	[InlineData( "(*THEN:V)(*ACCEPT)|a", "61", "0:0", "56", 0 )]
	[InlineData( "(*NOTEMPTY)(*THEN:V)(*ACCEPT)|a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?:(?>a(*THEN:V))b|ac)", "6163", "0:2", "", 0 )]
	[InlineData( "\\A(?:(?>a(*THEN:V)b)|ac)", "6163", "0:2", "", 0 )]
	[InlineData( "\\A(?=(a(*THEN:V)))ac|ac", "6163", "0:2,0:1", "56", 0 )]
	[InlineData( "\\A(?=(a(*THEN:V)b|a))a|a", "61", "0:1,0:1", "", 0 )]
	[InlineData( "\\A(?!a(*THEN:V)b|a)a", "61", "", "", 0 )]
	[InlineData( "\\A(?(?=a(*THEN:V)b|a)X|a)", "61", "", "", 0 )]
	[InlineData( "\\A(?(?!a(*THEN:V)b|a)a|X)", "61", "", "", 0 )]
	[InlineData( "\\A(?(?!(a(*THEN:V)b))a|X)", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?!(?=a(*THEN:V)b|a)a)a", "61", "", "", 0 )]
	[InlineData( "\\A(?=(?!a(*THEN:V)b|a)a)a", "61", "", "", 0 )]
	[InlineData( "\\A(?*(a(*THEN:V)|ab))\\1c", "616263", "0:3,0:2", "", 0 )]
	[InlineData( "\\A(?*(a|ab)(*THEN:V))\\1c", "616263", "", "", 0 )]
	[InlineData( "\\A(?*(a(*THEN:V)b|ab))ab", "6162", "0:2,0:2", "56", 0 )]
	[InlineData( "(?<=(a(*THEN:V)b))c", "616263", "2:1,0:2", "56", 0 )]
	[InlineData( "(?<=(a(*THEN:V)b|ab))c|c", "616263", "2:1,0:2", "56", 0 )]
	[InlineData( "(?<=(a{1,2}(*THEN:V)b))c", "61616263", "3:1,0:3", "56", 0 )]
	[InlineData( "(?<!(a(*THEN:V)b|a))c", "6163", "", "", 0 )]
	[InlineData( "(?(?<=(a(*THEN:V)b))c|X)", "616263", "2:1,0:2", "56", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN:V)b|ac))(?&x)d|acd", "616364", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN:V)b|ac))(?&x)d", "616364", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a|ab))(?&x)(*THEN:V)c|abc", "616263", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN:V)))(?&x)b|ac", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN:V)b))(?&x)c|aac", "616163", "0:3,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&x)?(*THEN:V)b))(?&x)c|aabc", "61616263", "0:4,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(?&y)c)(?<y>b(*THEN:V)X|b))(?&x)d|abcd", "61626364", "0:4,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=a(*THEN:V)b|a)a))(?&x)X|a", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN:V)b|a))(?!(?&x))a", "61", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN:V)b|a))(?(?=(?&x))X|a)", "61", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN:V)b))(?:(?&x(2))X|ac)(?(v)Y|d)", "616364", "", "", 0 )]
	[InlineData( "\\A(?:(a(*THEN:V)b)?ac|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*THEN:V)b)*ac|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*THEN:V)b){0}ac|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*THEN:V))?b|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*THEN:V))??b|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*THEN:V))?+b|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*THEN:V))++b|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*THEN:V)b)+|ac)", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?:(a(*THEN:V)){2}b|aa)", "6161", "0:2,-", "", 0 )]
	[InlineData( "\\A(*sr:(a(*THEN:V)b|ac))|ac", "6163", "0:2,0:2", "", 0 )]
	[InlineData( "\\A(*asr:(a(*THEN:V)))b|ac", "6163", "0:2,-", "", 0 )]
	[InlineData( "\\A(?J)(?<x>a)(*THEN:V)X|(?<x>ab)", "6162", "0:2,-,0:2", "", 0 )]
	[InlineData( "\\A(?|(?<x>a(*THEN:V)b)|(?<x>ac))", "6163", "0:2,0:2", "", 0 )]
	[InlineData( "\\A(?=(a(*THEN:V)b))a|a", "61", "0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN:V)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN:V)(?<v>b)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?<u>b)(*THEN:V)X))(?:(?&x(2))Y|ab)d", "616264", "0:3,-,0:1,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN:V)b))(?!(?&x(2)))ac(?(v)d|X)", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN:V)b))(?(?=(?&x(2)))X|ac)(?(v)d|X)", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN:V)b))(?:(?&x)X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN:V)(?<v>b)X|abc))(?:(?&x(2))Y|abc)d", "61626364", "0:4,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN:V)b))(?:(?&x(2))X|ac(*FAIL))|acd", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?!(?<v>a)(*THEN:V)b)a))(?&x(2))", "61", "0:1,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?(?=(?<v>a)(*THEN:V)b)X|a)))(?&x(2))", "61", "0:1,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?=(?<v>a)(*THEN:V)b)a))(?:(?&x(2))X|a)", "61", "0:1,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*THEN:V)c))(?:(?&x(2))X|ab)d", "616264", "0:3,-,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?&y(2)))(?<y>b(*THEN:V)c))(?:(?&x)X|ab)d", "616264", "0:3,-,-,-", "", 0 )]
	[InlineData( "(*THEN:V)", "61", "0:0", "56", 0 )]
	[InlineData( "(*THEN:V)(*FAIL)|a", "61", "0:1", "", 0 )]
	[InlineData( "(*THEN:V)abc", "78797A616263", "3:3", "56", 0 )]
	[InlineData( "(*THEN:V)abc", "7861616263", "2:3", "56", 0 )]
	[InlineData( "\\A(?:a+(*THEN:V)X|a+)b", "61616162", "0:4", "", 0 )]
	[InlineData( "\\A(?:(?:a+(*THEN:V)X)|a+)b", "61616162", "0:4", "", 0 )]
	[InlineData( "\\A(?:a+(?:a(*THEN:V)X)|a+)b", "61616162", "0:4", "", 0 )]
	[InlineData( "\\Aa+(?:a(*THEN:V)X|(*F))b|aaab", "61616162", "0:4", "", 0 )]
	[InlineData( "\\Aa+(?:a(*THEN:V)X|(*F))aab", "6161616162", "", "", 0 )]
	[InlineData( "\\Aa+(?:a(*THEN:V)X)aab", "6161616162", "", "", 0 )]
	[InlineData( "\\A(?:a+X|a+(*THEN:V)Y|a+)b", "61616162", "0:4", "", 0 )]
	[InlineData( "\\A(?:a+X|a+(*THEN:V)Y)ab|aaaab", "6161616162", "0:5", "", 0 )]
	[InlineData( "\\Aa+(?:a+X|a(*THEN:V)Y)aab", "6161616162", "", "", 0 )]
	[InlineData( "\\A(?:a+(*THEN:V)b|a+(*THEN:V)c|a+d)", "61616164", "0:4", "", 0 )]
	[InlineData( "\\A(?:a+(*THEN:V)b|a+(*THEN:V)c|a+(*THEN:V)d|a+e)", "61616165", "0:4", "", 0 )]
	[InlineData( "\\A(?:a(*THEN:V)b|ac)d", "616364", "0:3", "", 0 )]
	[InlineData( "\\A(?:(a)(*THEN:V)b|(ac))d", "616364", "0:3,-,0:2", "", 0 )]
	[InlineData( "\\A(?:a\\K(*THEN:V)b|ac)d", "616364", "0:3", "", 0 )]
	[InlineData( "\\A(?=a(*THEN:V)b|a)a|X", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?=a(*THEN:V)b)a|a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?!a(*THEN:V)b|a)a|a", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?*(a|ab)(*THEN:V))\\1c|abc", "616263", "0:3,-", "", 0 )]
	[InlineData( "\\A(?=(a+(*THEN:V)X|a))\\1b", "616162", "", "", 0 )]
	[InlineData( "\\A(?(?=a(*THEN:V)b|a)a|X)", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?!a(*THEN:V)b|a)X|a)", "61", "0:1", "", 0 )]
	[InlineData( "\\A(?(?=a(*THEN:V)b)X|a)", "61", "0:1", "", 0 )]
	[InlineData( "\\A.*?(?(?=a)a|b(*THEN:V)c)", "6261", "", "", 0 )]
	[InlineData( "\\A(?:.*?(?(?=a)a|b(*THEN:V)c)|ba)", "6261", "0:2", "", 0 )]
	[InlineData( "\\A(?:a(?(?=c)c|(*THEN:V)X)|ab)", "6162", "0:2", "", 0 )]
	[InlineData( "\\A(?:a(?(1)(*THEN:V)X|b)|ac)()", "6163", "0:2,2:0", "", 0 )]
	[InlineData( "\\A(?:a(?(R)(*THEN:V)X|b)|ac)", "6163", "0:2", "", 0 )]
	[InlineData( "\\A(?:a(?(VERSION>=10.48)(*THEN:V)X|b)|ac)", "6163", "0:2", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a+(*THEN:V)X|a))(?&x)b", "616162", "", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a+(*THEN:V)X))(?&x)b|aaab", "61616162", "0:4,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a+(?:a(*THEN:V)X|a)))(?&x)b", "61616162", "0:4,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a+(?:a(*THEN:V)X)))(?&x)b|aaab", "61616162", "0:4,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN:V)b|ac))(?:(?&x(2))X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?:a(*COMMIT)b(*THEN:V)c|abd)", "616264", "0:3", "", 0 )]
	[InlineData( "\\A(?:a(*THEN:V)b(*COMMIT)c|abd)", "616264", "", "", 0 )]
	[InlineData( "\\A(?:a(*PRUNE)b(*THEN:V)c|abd)", "616264", "0:3", "", 0 )]
	[InlineData( "\\A(?:a(*THEN:V)b(*PRUNE)c|abd)", "616264", "", "", 0 )]
	[InlineData( "\\A(?:a(*SKIP)b(*THEN:V)c|abd)", "616264", "0:3", "", 0 )]
	[InlineData( "\\A(?:a(*THEN:V)b(*SKIP)c|abd)", "616264", "", "", 0 )]
	[InlineData( "\\A(?:a(*COMMIT)(?:b(*THEN:V)c|bX)|abd)", "616264", "", "", 0 )]
	[InlineData( "\\A(?:a(*COMMIT)(?:b(*THEN:V)c)|abd)", "616264", "0:3", "", 0 )]
	[InlineData( "\\A(?:a(*COMMIT)(?:bX|b(*THEN:V)c)|abd)", "616264", "", "", 0 )]
	[InlineData( "\\A(?:(?>a(*THEN:V)b|ac))d", "616364", "0:3", "", 0 )]
	[InlineData( "\\A(?:a(*THEN:V)b|ac)+d", "6163616364", "0:5", "", 0 )]
	[InlineData( "\\A(?:a(*THEN:V)b|ac)+?d", "6163616364", "0:5", "", 0 )]
	[InlineData( "\\A(?:a(*THEN:V)b|ac)++d", "6163616364", "0:5", "", 0 )]
	[InlineData( "\\A(?:a(*THEN:V)b|ac){2}d", "6163616364", "0:5", "", 0 )]
	[InlineData( "\\A(?:a(*THEN:V)b|ac){0}d", "64", "0:1", "", 0 )]
	[InlineData( "\\A(?:a(*THEN:V)b|ac)?d", "616364", "0:3", "", 0 )]
	[InlineData( "\\A(?:a+(*THEN:V)X|a+)+b", "61616162", "0:4", "", 0 )]
	[InlineData( "\\A(?:a+X|a+(*THEN:V)Y)+b|aaab", "61616162", "0:4", "", 0 )]
	[InlineData( "(?<=(a(*THEN:V)b|ac))d", "616364", "2:1,0:2", "", 0 )]
	[InlineData( "(?<=(a{1,2}(*THEN:V)X|a))b", "616162", "2:1,1:1", "", 0 )]
	[InlineData( "(?(?<=(a(*THEN:V)b|a))c|X)", "6163", "1:1,0:1", "", 0 )]
	[InlineData( "\\A(*sr:(a(*THEN:V)b|ac))d", "616364", "0:3,0:2", "", 0 )]
	[InlineData( "\\A(*asr:(a(*THEN:V)b|ac))d", "616364", "0:3,0:2", "", 0 )]
	[InlineData( "\\A(*sr:(a(*THEN:V)b))d|acd", "616364", "0:3,-", "", 0 )]
	[InlineData( "é+(*THEN:V)(*F)|(.)", "C3A9C3A9E6BCA2", "0:2,0:2", "", 1 )]
	[InlineData( "(*UTF)(*UCP)\\X(*THEN:V)X|(.)", "C3A961", "0:2,0:2", "", 1 )]
	[InlineData( "\\A(?:a(*THEN:V)(*ACCEPT)|ac)d", "6163", "0:1", "56", 0 )]
	[InlineData( "(*ANYCRLF)(?s:.)\\B(*THEN:V)(*F)|(?s:(.))", "0D0A78", "0:1,0:1", "", 0 )]
	[InlineData( "(*ANYCRLF)a+(*THEN:V)X|(b)", "61610D0A62", "4:1,4:1", "", 0 )]
	[InlineData( "\\A(?|(?<v>a)(*THEN:V)X|(?<v>ab))\\k<v>", "61626162", "0:4,0:2", "", 0 )]
	[InlineData( "\\A(?J)(?<v>a)(*THEN:V)X|(?<v>ab)", "6162", "0:2,-,0:2", "", 0 )]
	[InlineData( "(*THEN:V)(*COMMIT)abc", "78797A616263", "3:3", "56", 0 )]
	[InlineData( "(*COMMIT)(*THEN:V)abc", "78797A616263", "3:3", "56", 0 )]
	[InlineData( "\\Aa+(?:a(*THEN:V)a*|(*F))ab", "6161616162", "0:5", "56", 0 )]
	[InlineData( "\\Aa+(?:a(*THEN:V)a*)ab", "6161616162", "", "", 0 )]
	[InlineData( "\\Aa+(?:X|a(*THEN:V)a*)ab", "6161616162", "0:5", "56", 0 )]
	[InlineData( "\\A(?:a+(*THEN:V)X|a)+b", "61616162", "0:4", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(*F)|(?<v>a)(*THEN:V)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?:X|(*THEN:V)b)))(?:(?&x(2))X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?:(?<v>a)(*THEN:V)b|(*F))))(?:(?&x(2))X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?>(?:X|(?<v>a)(*THEN:V)b))))(?:(?&x(2))X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?:(?<v>a)(*THEN:V)b)))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(?:X|(*THEN:V))))(?:(?&x(2))X|ac)d", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(*F)|(?<v>a)(*THEN:V)b))(?:(?&x(2))X|ac)(?(v)X|d)", "616364", "0:3,-,-", "", 0 )]
	[InlineData( "a(*PRUNE:N)b", "6162", "0:2", "4E", 0 )]
	[InlineData( "a(*MARK:old)(*PRUNE:)b", "6162", "0:2", "6F6C64", 0 )]
	[InlineData( "a(*MARK:old)(*PRUNE:\u0000)b", "6162", "0:2", "", 0 )]
	[InlineData( "a(*PRUNE:é漢)b", "6162", "0:2", "C3A9E6BCA2", 0 )]
	[InlineData( "(?x)a (*PRUNE:n # x) b", "6162", "0:2", "6E20232078", 0 )]
	[InlineData( "a(*PRUNE:\\x41)b", "6162", "0:2", "5C783431", 0 )]
	[InlineData( "a(*PRUNE:N(*THEN)b", "6162", "0:2", "4E282A5448454E", 0 )]
	[InlineData( "a(*PRUNE:X)b(*SKIP:X)(*F)|(.)", "61626364", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:X)b(*PRUNE:X)c(*SKIP:X)(*F)|(.)", "61626364", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:X)b(*PRUNE:Y)c(*SKIP:Y)(*F)|(.)", "61626364", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:X)b(*PRUNE:Y)c(*SKIP:X)(*F)|(.)", "61626364", "1:1,1:1", "", 0 )]
	[InlineData( "a(*PRUNE:X)b(*MARK:X)c(*SKIP:X)(*F)|(.)", "61626364", "2:1,2:1", "", 0 )]
	[InlineData( "a(*PRUNE:X)b(*MARK:X)c(*PRUNE:X)d(*SKIP:X)(*F)|(.)", "6162636465", "2:1,2:1", "", 0 )]
	[InlineData( "a(?=(*PRUNE:X))b", "6162", "0:2", "58", 0 )]
	[InlineData( "a(?!b(*PRUNE:X)X)b", "6162", "0:2", "", 0 )]
	[InlineData( "a(?(?!b(*PRUNE:X)X)b|X)", "6162", "0:2", "", 0 )]
	[InlineData( "a(?(?=b(*PRUNE:X))b|X)", "6162", "0:2", "58", 0 )]
	[InlineData( "a(?*b(*PRUNE:X))bc", "616263", "0:3", "58", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*PRUNE:X)))(?&x)b", "6162", "0:2,-", "58", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*PRUNE:X)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*PRUNE:Y)(*SKIP:X)(*F))|(.)", "61616263", "1:1,1:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*PRUNE:)(*SKIP:X)(*F))|(.)", "61616263", "2:1,2:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*PRUNE:\u0000)(*SKIP:X)(*F))|(.)", "61616263", "1:1,1:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?*(*PRUNE:Y)(*SKIP:X)(*F))|(.)", "61616263", "2:1,2:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?=(*PRUNE:Y))(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?:a(*PRUNE:Y)){0}(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?(DEFINE)(*PRUNE:Y))(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "a(?>(*PRUNE:X))b(*SKIP:X)(*F)|(.)", "61626364", "0:1,0:1", "", 0 )]
	[InlineData( "a(?*(*PRUNE:X))b(*SKIP:X)(*F)|(.)", "61626364", "1:1,1:1", "", 0 )]
	[InlineData( "(*PRUNE:X)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*MARK:M)(*PRUNE:X)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*PRUNE:X)(*COMMIT)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(?<=a(*PRUNE:X)b)c", "616263", "2:1", "58", 0 )]
	[InlineData( "(?<=a{1,2}(*PRUNE:X)b)c", "61616263", "3:1", "58", 0 )]
	[InlineData( "\\A(*PRUNE:X)(*ACCEPT)", "", "0:0", "58", 0 )]
	[InlineData( "(*NOTEMPTY)(*PRUNE:X)(*ACCEPT)|a", "61", "", "", 0 )]
	[InlineData( "é(*PRUNE:名)é(*SKIP:名)(*F)|(漢)", "C3A9C3A9E6BCA2", "4:3,4:3", "", 1 )]
	[InlineData( "(*sr:a(*PRUNE:N))b", "6162", "0:2", "4E", 0 )]
	[InlineData( "(*asr:a(*PRUNE:N))b", "6162", "0:2", "4E", 0 )]
	[InlineData( "\\Q(*PRUNE:N)\\E", "282A5052554E453A4E29", "0:10", "", 0 )]
	[InlineData( "[(*PRUNE:N)]", "4E", "0:1", "", 0 )]
	[InlineData( "a(*COMMIT:N)b", "6162", "0:2", "4E", 0 )]
	[InlineData( "a(*MARK:old)(*COMMIT:)b", "6162", "0:2", "6F6C64", 0 )]
	[InlineData( "a(*MARK:old)(*COMMIT:\u0000)b", "6162", "0:2", "", 0 )]
	[InlineData( "a(*COMMIT:é漢)b", "6162", "0:2", "C3A9E6BCA2", 0 )]
	[InlineData( "(?x)a (*COMMIT:n # x) b", "6162", "0:2", "6E20232078", 0 )]
	[InlineData( "a(*COMMIT:\\x41)b", "6162", "0:2", "5C783431", 0 )]
	[InlineData( "a(*COMMIT:N(*THEN)b", "6162", "0:2", "4E282A5448454E", 0 )]
	[InlineData( "a(*COMMIT:X)b(*SKIP:X)(*F)|(.)", "61626364", "", "", 0 )]
	[InlineData( "a(*MARK:X)b(*COMMIT:X)c(*SKIP:X)(*F)|(.)", "61626364", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:X)b(*COMMIT:Y)c(*SKIP:Y)(*F)|(.)", "61626364", "", "", 0 )]
	[InlineData( "a(*MARK:X)b(*COMMIT:Y)c(*SKIP:X)(*F)|(.)", "61626364", "1:1,1:1", "", 0 )]
	[InlineData( "a(*COMMIT:X)b(*MARK:X)c(*SKIP:X)(*F)|(.)", "61626364", "2:1,2:1", "", 0 )]
	[InlineData( "a(*COMMIT:X)b(*MARK:X)c(*COMMIT:X)d(*SKIP:X)(*F)|(.)", "6162636465", "2:1,2:1", "", 0 )]
	[InlineData( "a(?=(*COMMIT:X))b", "6162", "0:2", "58", 0 )]
	[InlineData( "a(?!b(*COMMIT:X)X)b", "6162", "0:2", "", 0 )]
	[InlineData( "a(?(?!b(*COMMIT:X)X)b|X)", "6162", "0:2", "", 0 )]
	[InlineData( "a(?(?=b(*COMMIT:X))b|X)", "6162", "0:2", "58", 0 )]
	[InlineData( "a(?*b(*COMMIT:X))bc", "616263", "0:3", "58", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*COMMIT:X)))(?&x)b", "6162", "0:2,-", "58", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*COMMIT:X)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*COMMIT:Y)(*SKIP:X)(*F))|(.)", "61616263", "", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*COMMIT:)(*SKIP:X)(*F))|(.)", "61616263", "2:1,2:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*COMMIT:\u0000)(*SKIP:X)(*F))|(.)", "61616263", "", "", 0 )]
	[InlineData( "aa(*MARK:X)(?*(*COMMIT:Y)(*SKIP:X)(*F))|(.)", "61616263", "2:1,2:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?=(*COMMIT:Y))(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?:a(*COMMIT:Y)){0}(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?(DEFINE)(*COMMIT:Y))(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "a(?>(*COMMIT:X))b(*SKIP:X)(*F)|(.)", "61626364", "0:1,0:1", "", 0 )]
	[InlineData( "a(?*(*COMMIT:X))b(*SKIP:X)(*F)|(.)", "61626364", "", "", 0 )]
	[InlineData( "(*COMMIT:X)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*MARK:M)(*COMMIT:X)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*COMMIT:X)(*COMMIT)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(?<=a(*COMMIT:X)b)c", "616263", "2:1", "58", 0 )]
	[InlineData( "(?<=a{1,2}(*COMMIT:X)b)c", "61616263", "3:1", "58", 0 )]
	[InlineData( "\\A(*COMMIT:X)(*ACCEPT)", "", "0:0", "58", 0 )]
	[InlineData( "(*NOTEMPTY)(*COMMIT:X)(*ACCEPT)|a", "61", "", "", 0 )]
	[InlineData( "é(*COMMIT:名)é(*SKIP:名)(*F)|(漢)", "C3A9C3A9E6BCA2", "", "", 1 )]
	[InlineData( "(*sr:a(*COMMIT:N))b", "6162", "0:2", "4E", 0 )]
	[InlineData( "(*asr:a(*COMMIT:N))b", "6162", "0:2", "4E", 0 )]
	[InlineData( "\\Q(*COMMIT:N)\\E", "282A434F4D4D49543A4E29", "0:11", "", 0 )]
	[InlineData( "[(*COMMIT:N)]", "4E", "0:1", "", 0 )]
	[InlineData( "a(*THEN:N)b", "6162", "0:2", "4E", 0 )]
	[InlineData( "a(*MARK:old)(*THEN:)b", "6162", "0:2", "6F6C64", 0 )]
	[InlineData( "a(*MARK:old)(*THEN:\u0000)b", "6162", "0:2", "", 0 )]
	[InlineData( "a(*THEN:é漢)b", "6162", "0:2", "C3A9E6BCA2", 0 )]
	[InlineData( "(?x)a (*THEN:n # x) b", "6162", "0:2", "6E20232078", 0 )]
	[InlineData( "a(*THEN:\\x41)b", "6162", "0:2", "5C783431", 0 )]
	[InlineData( "a(*THEN:N(*THEN)b", "6162", "0:2", "4E282A5448454E", 0 )]
	[InlineData( "a(*THEN:X)b(*SKIP:X)(*F)|(.)", "61626364", "0:1,0:1", "", 0 )]
	[InlineData( "a(*MARK:X)b(*THEN:X)c(*SKIP:X)(*F)|(.)", "61626364", "1:1,1:1", "", 0 )]
	[InlineData( "a(*MARK:X)b(*THEN:Y)c(*SKIP:Y)(*F)|(.)", "61626364", "0:1,0:1", "", 0 )]
	[InlineData( "a(*MARK:X)b(*THEN:Y)c(*SKIP:X)(*F)|(.)", "61626364", "1:1,1:1", "", 0 )]
	[InlineData( "a(*THEN:X)b(*MARK:X)c(*SKIP:X)(*F)|(.)", "61626364", "2:1,2:1", "", 0 )]
	[InlineData( "a(*THEN:X)b(*MARK:X)c(*THEN:X)d(*SKIP:X)(*F)|(.)", "6162636465", "2:1,2:1", "", 0 )]
	[InlineData( "a(?=(*THEN:X))b", "6162", "0:2", "58", 0 )]
	[InlineData( "a(?!b(*THEN:X)X)b", "6162", "0:2", "", 0 )]
	[InlineData( "a(?(?!b(*THEN:X)X)b|X)", "6162", "0:2", "", 0 )]
	[InlineData( "a(?(?=b(*THEN:X))b|X)", "6162", "0:2", "58", 0 )]
	[InlineData( "a(?*b(*THEN:X))bc", "616263", "0:3", "58", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>a(*THEN:X)))(?&x)b", "6162", "0:2,-", "58", 0 )]
	[InlineData( "\\A(?(DEFINE)(?<x>(?<v>a)(*THEN:X)b))(?:(?&x(2))X|ac)d", "616364", "0:3,-,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*THEN:Y)(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*THEN:)(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(*THEN:\u0000)(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?*(*THEN:Y)(*SKIP:X)(*F))|(.)", "61616263", "2:1,2:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?=(*THEN:Y))(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?:a(*THEN:Y)){0}(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "aa(*MARK:X)(?=(?(DEFINE)(*THEN:Y))(*SKIP:X)(*F))|(.)", "61616263", "0:1,0:1", "", 0 )]
	[InlineData( "a(?>(*THEN:X))b(*SKIP:X)(*F)|(.)", "61626364", "0:1,0:1", "", 0 )]
	[InlineData( "a(?*(*THEN:X))b(*SKIP:X)(*F)|(.)", "61626364", "0:1,0:1", "", 0 )]
	[InlineData( "(*THEN:X)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*MARK:M)(*THEN:X)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(*THEN:X)(*COMMIT)abc", "78797A616263", "3:3", "58", 0 )]
	[InlineData( "(?<=a(*THEN:X)b)c", "616263", "2:1", "58", 0 )]
	[InlineData( "(?<=a{1,2}(*THEN:X)b)c", "61616263", "3:1", "58", 0 )]
	[InlineData( "\\A(*THEN:X)(*ACCEPT)", "", "0:0", "58", 0 )]
	[InlineData( "(*NOTEMPTY)(*THEN:X)(*ACCEPT)|a", "61", "0:1", "", 0 )]
	[InlineData( "é(*THEN:名)é(*SKIP:名)(*F)|(漢)", "C3A9C3A9E6BCA2", "4:3,4:3", "", 1 )]
	[InlineData( "(*sr:a(*THEN:N))b", "6162", "0:2", "4E", 0 )]
	[InlineData( "(*asr:a(*THEN:N))b", "6162", "0:2", "4E", 0 )]
	[InlineData( "\\Q(*THEN:N)\\E", "282A5448454E3A4E29", "0:9", "", 0 )]
	[InlineData( "[(*THEN:N)]", "4E", "0:1", "", 0 )]
	[InlineData( "\\A(?:a(*PRUNE:A)b(*PRUNE:B)X|abc)", "616263", "", "", 0 )]
	[InlineData( "a(*PRUNE:A)b(*PRUNE:B)c", "616263", "0:3", "42", 0 )]
	[InlineData( "\\A(?:a(*PRUNE:A)b(*COMMIT:B)X|abc)", "616263", "", "", 0 )]
	[InlineData( "a(*PRUNE:A)b(*COMMIT:B)c", "616263", "0:3", "42", 0 )]
	[InlineData( "\\A(?:a(*PRUNE:A)b(*THEN:B)X|abc)", "616263", "0:3", "", 0 )]
	[InlineData( "a(*PRUNE:A)b(*THEN:B)c", "616263", "0:3", "42", 0 )]
	[InlineData( "\\A(?:a(*COMMIT:A)b(*PRUNE:B)X|abc)", "616263", "", "", 0 )]
	[InlineData( "a(*COMMIT:A)b(*PRUNE:B)c", "616263", "0:3", "42", 0 )]
	[InlineData( "\\A(?:a(*COMMIT:A)b(*COMMIT:B)X|abc)", "616263", "", "", 0 )]
	[InlineData( "a(*COMMIT:A)b(*COMMIT:B)c", "616263", "0:3", "42", 0 )]
	[InlineData( "\\A(?:a(*COMMIT:A)b(*THEN:B)X|abc)", "616263", "0:3", "", 0 )]
	[InlineData( "a(*COMMIT:A)b(*THEN:B)c", "616263", "0:3", "42", 0 )]
	[InlineData( "\\A(?:a(*THEN:A)b(*PRUNE:B)X|abc)", "616263", "", "", 0 )]
	[InlineData( "a(*THEN:A)b(*PRUNE:B)c", "616263", "0:3", "42", 0 )]
	[InlineData( "\\A(?:a(*THEN:A)b(*COMMIT:B)X|abc)", "616263", "", "", 0 )]
	[InlineData( "a(*THEN:A)b(*COMMIT:B)c", "616263", "0:3", "42", 0 )]
	[InlineData( "\\A(?:a(*THEN:A)b(*THEN:B)X|abc)", "616263", "0:3", "", 0 )]
	[InlineData( "a(*THEN:A)b(*THEN:B)c", "616263", "0:3", "42", 0 )]
	public void NamedCutsRetainNativeCapturesLabelsAndHistoryIsolation( string pattern, string hex, string spans, string markHex, int mode ) {
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
	public void SelectedOffsetsRetainFragmentsCrLfAndNamedCutLabels( bool anyCrLf ) {
		foreach ( var verb in new[] { "PRUNE", "COMMIT", "THEN" } ) {
			foreach ( var utf in new[] { false, true } ) {
				var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf };
				foreach ( var pattern in new[] { $"a(*{verb}:X)b|ac", $"a(*{verb}:X)(*SKIP:X)(*F)|(.)", $"a(*MARK:X)b(*{verb}:X)(*SKIP:X)(*F)|(.)",
					$"(?=(a(*{verb}:X)))a|b", $"(?<=a(*{verb}:X)b)c|(.)", $"(*{verb}:X)(*COMMIT)a", $"(*MARK:M)(*{verb}:)(*ACCEPT)", $"é(*{verb}:名)é|漢" } ) {
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
	}

	[Theory]
	[InlineData( "PRUNE", 10 )]
	[InlineData( "COMMIT", 11 )]
	[InlineData( "THEN", 9 )]
	public void DirectRepeatsAndGlobalErrorsKeepOriginalByteOffsets( string verb, int namedLength ) {
		foreach ( var suffix in new[] { "*", "+", "?", "{0}", "{1,2}", @"\k<missing>" } ) {
			var pattern = $"(*{verb}:N){suffix}";
			Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
			var actual = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
			Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code ); Assert.Equal( namedLength, actual.Diagnostic?.ByteOffset );
		}
		foreach ( var prefix in new[] { $"é(*{verb}:N)", $"(*{verb}:N)(?#x)" } ) {
			var pattern = String.Concat( prefix, "+" ); Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
			var actual = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
			Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code ); Assert.Equal( Encoding.UTF8.GetByteCount( prefix ), actual.Diagnostic?.ByteOffset );
		}
		var emptyRepeat = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( $"(*{verb}:)*" ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, emptyRepeat.Diagnostic?.Code ); Assert.Equal( namedLength - 1, emptyRepeat.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "PRUNE" )]
	[InlineData( "COMMIT" )]
	[InlineData( "THEN" )]
	public void UnterminatedNamesKeepOriginalEndByte( string verb ) {
		foreach ( var pattern in new[] { $"(*{verb}", $"(*{verb}:", $"(*{verb}:N" } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
			var actual = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
			Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code ); Assert.Equal( pattern.Length, actual.Diagnostic?.ByteOffset );
		}
	}

	[Theory]
	[InlineData( "PRUNE", false )]
	[InlineData( "PRUNE", true )]
	[InlineData( "COMMIT", false )]
	[InlineData( "COMMIT", true )]
	[InlineData( "THEN", false )]
	[InlineData( "THEN", true )]
	public void RawNameByteLimitsRetainNativeBoundaries( string verb, bool utf ) {
		var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
		foreach ( var name in new[] { new string( 'x', 255 ), new string( 'x', 256 ), String.Concat( String.Concat( Enumerable.Repeat( "é", 127 ) ), "X" ), String.Concat( Enumerable.Repeat( "é", 128 ) ) } ) {
			var pattern = $"(*{verb}:{name})a"; var bytes = Encoding.UTF8.GetBytes( pattern ); var actual = PcreByteCompiler.Compile( bytes, options );
			if ( Encoding.UTF8.GetByteCount( name ) <= 255 ) {
				Assert.True( actual.IsSuccess ); Compare( Reference( pattern, options ), "a"u8.ToArray(), 0, actual.Pattern!.Match( "a"u8.ToArray() ), pattern );
			} else {
				Assert.Throws<PcrePatternException>( () => Reference( pattern, options ) );
				Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code ); Assert.Equal( bytes.Length - 2, actual.Diagnostic?.ByteOffset );
			}
		}
	}

	[Theory]
	[InlineData( "PRUNE" )]
	[InlineData( "COMMIT" )]
	[InlineData( "THEN" )]
	public void NonExactHeadersRemainExplicitlyUnsupported( string verb ) {
		foreach ( var header in new[] { verb.ToLowerInvariant(), String.Concat( verb, "X" ), String.Concat( verb, " " ) } ) {
			var actual = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( $"(*{header}:N)" ) );
			Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, actual.Diagnostic?.Code ); Assert.Equal( 0, actual.Diagnostic?.ByteOffset );
		}
	}

	[Theory]
	[InlineData( "PRUNE", 8 )]
	[InlineData( "COMMIT", 9 )]
	[InlineData( "THEN", 7 )]
	public void ByteNamesAndNulViewsPreserveUtfValidation( string verb, int firstNameByte ) {
		foreach ( var nameHex in new[] { "FF", "FF0041", "0041" } ) {
			var pattern = Encoding.ASCII.GetBytes( $"a(*{verb}:" ).Concat( Convert.FromHexString( nameHex ) ).Concat( ")b"u8.ToArray() ).ToArray();
			var actual = PcreByteCompiler.Compile( pattern ); Assert.True( actual.IsSuccess );
			Compare( Reference( pattern, new() ), "ab"u8.ToArray(), 0, actual.Pattern!.Match( "ab"u8.ToArray() ), verb );
			Assert.Equal( nameHex == "0041" ? "" : "FF", Convert.ToHexString( actual.Pattern.Match( "ab"u8.ToArray() ).Match!.MarkName.Span ) );
			if ( nameHex[0] == 'F' ) {
				Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
				var strict = PcreByteCompiler.Compile( pattern, new() { Utf8 = true } );
				Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, strict.Diagnostic?.Code ); Assert.Equal( firstNameByte + 1, strict.Diagnostic?.ByteOffset );
			}
		}
	}

	[Theory]
	[InlineData( "PRUNE", 1024, true )]
	[InlineData( "PRUNE", 1025, false )]
	[InlineData( "COMMIT", 1024, true )]
	[InlineData( "COMMIT", 1025, false )]
	[InlineData( "THEN", 1024, true )]
	[InlineData( "THEN", 1025, false )]
	public void LabelsKeepOneCutFrameAndRemainInvisibleToSkipAtTheLimit( string verb, int count, bool succeeds ) {
		var pattern = $"\\A(?:a(*{verb}:N)){{{count}}}(*SKIP:N)"; var subject = Enumerable.Repeat( (byte)'a', count ).ToArray();
		var reference = Reference( String.Concat( "(*NO_JIT)", pattern ), new() ); AssertLiteral( reference, subject, $"0:{count}" );
		Assert.Equal( "4E", Convert.ToHexString( reference.Match( subject ).Mark ) );
		var actual = Compile( pattern, new() ).Match( subject );
		if ( succeeds ) { Assert.True( actual.IsSuccess ); Assert.Equal( count, actual.Match?.ByteLength ); Assert.Equal( "4E", Convert.ToHexString( actual.Match!.MarkName.Span ) ); }
		else { Assert.Null( actual.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, actual.Diagnostic?.Code ); }
	}

	[Theory]
	[InlineData( "PRUNE" )]
	[InlineData( "COMMIT" )]
	[InlineData( "THEN" )]
	public void EmptyNamesKeepThePreviousLabelAndShareNoLabelSlot( string verb ) {
		var pattern = String.Concat( @"\A", $"(*{verb}:)", String.Concat( Enumerable.Repeat( "(?:a?)*", 256 ) ) );
		Assert.True( Reference( String.Concat( "(*NO_JIT)", pattern ), new() ).Match( Array.Empty<byte>() ).Success );
		var actual = Compile( pattern, new() ).Match( Array.Empty<byte>() ); Assert.True( actual.IsSuccess ); Assert.True( actual.Match!.MarkName.IsEmpty );
		var retained = $"(*MARK:M)(*{verb}:)a";
		Compare( Reference( retained, new() ), "a"u8.ToArray(), 0, Compile( retained, new() ).Match( "a"u8.ToArray() ), retained );
	}

	[Theory]
	[InlineData( "PRUNE" )]
	[InlineData( "COMMIT" )]
	[InlineData( "THEN" )]
	public void AllLabelKindsShareOneOfTheExisting256MarkSlots( string verb ) {
		foreach ( var count in new[] { 255, 256 } ) {
			var pattern = String.Concat( @"\A(*MARK:M)", $"(*{verb}:N)", String.Concat( Enumerable.Repeat( "(?:a?)*", count ) ) );
			Assert.True( Reference( String.Concat( "(*NO_JIT)", pattern ), new() ).Match( Array.Empty<byte>() ).Success );
			var actual = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
			if ( count == 255 ) { Assert.True( actual.IsSuccess ); Assert.True( actual.Pattern!.Match( Array.Empty<byte>() ).IsSuccess ); Assert.Equal( "4E", Convert.ToHexString( actual.Pattern.Match( Array.Empty<byte>() ).Match!.MarkName.Span ) ); }
			else { Assert.Null( actual.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, actual.Diagnostic?.Code ); }
		}
		var expanded = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( $"(?:a(*{verb}:N)){{3000}}" ) );
		Assert.Null( expanded.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, expanded.Diagnostic?.Code );
	}

	[Theory]
	[InlineData( "PRUNE" )]
	[InlineData( "COMMIT" )]
	[InlineData( "THEN" )]
	public void NamedCutsKeepChargedTransitionsWorkLimitsAndCancellation( string verb ) {
		var subject = Enumerable.Repeat( (byte)'a', 40 ).ToArray();
		var named = $"\\A(?:a(*{verb}:N)){{40}}"; var plain = $"\\A(?:a(*{verb})){{40}}";
		var namedSteps = new List<int>(); var plainSteps = new List<int>();
		Compare( Reference( named, new() ), subject, 0, Compile( named, new() { WorkObserver = namedSteps.Add } ).Match( subject ), named );
		Assert.True( Compile( plain, new() { WorkObserver = plainSteps.Add } ).Match( subject ).IsSuccess );
		Assert.Equal( plainSteps, namedSteps ); Assert.Equal( Enumerable.Range( 1, namedSteps.Count ), namedSteps );
		var limited = Compile( named, new() { MatchWorkLimit = 20 } ).Match( subject );
		Assert.Null( limited.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( named, new() { WorkObserver = step => { if ( step == 20 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( named ), cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( "PRUNE" )]
	[InlineData( "COMMIT" )]
	[InlineData( "THEN" )]
	public void CompiledLabelsSurviveCallerPatternMutation( string verb ) {
		var pattern = $"a(*{verb}:N)b"; var bytes = Encoding.ASCII.GetBytes( pattern ); var actual = PcreByteCompiler.Compile( bytes ); Assert.True( actual.IsSuccess );
		Array.Fill( bytes, (byte)'z' ); var match = actual.Pattern!.Match( "ab"u8.ToArray() ); Assert.True( match.IsSuccess );
		Assert.Equal( new PcreByteCapture( 0, 2 ), match.Match!.Captures[0] ); Assert.Equal( "4E", Convert.ToHexString( match.Match.MarkName.Span ) );
	}

	[Theory]
	[InlineData( "PRUNE" )]
	[InlineData( "COMMIT" )]
	[InlineData( "THEN" )]
	public void NamedCutsDoNotHideUnboundedLookbehindTails( string verb ) {
		var pattern = $"(?<=(*{verb}:N)a*)b"; Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
		var actual = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, actual.Diagnostic?.Code ); Assert.Equal( 0, actual.Diagnostic?.ByteOffset );
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
