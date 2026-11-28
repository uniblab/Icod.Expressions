using Icod.Expressions.Pcre;

namespace Icod.Expressions.PackageSmoke;

internal sealed record PcreWordSmokeCase(
	int Id, string Pattern, string Subject, PcreWordOptions Options, int Start, PcreSmokeProtocol Expected, bool Literal = false
);

internal static class PcreWordSmokeCases {
	internal static IReadOnlyList<PcreWordSmokeCase> All { get; } = new PcreWordSmokeCase[] {
		new( 1, "\ud800", "a\ud800b", new(), 0, new( "M", "1:1" ), Literal: true ),
		new( 2, "😀", "a😀b", new() { Utf16 = true }, 0, new( "M", "1:2" ), Literal: true ),
		new( 3, "a+", "za+b", new(), 0, new( "M", "1:2" ), Literal: true ),
		new( 4, "", "", new() { Utf16 = true }, 0, new( "M", "0:0" ), Literal: true ),
		new( 5, @"(\x{0})(\x{ffff})", "\0\uffff", new(), 0, new( "M", "0:2,0:1,1:1" ) ),
		new( 6, "(\ud800)\\K(x)", "\ud800x", new(), 0, new( "M", "1:1,0:1,1:1" ) ),
		new( 7, @"(?<face>😀)\K(?<tail>x)", "z😀x", new() { Utf16 = true }, 1, new( "M", "3:1,1:2,3:1" ) ),
		new( 8, @"(?:ab\KX|ab)c", "abc", new(), 0, new( "M", "0:3" ) ),
		new( 9, @"(?:ab\KX|ab)c", "abc", new() { Utf16 = true }, 0, new( "M", "0:3" ) ),
		new( 10, "(a)|(b)", "b", new(), 0, new( "M", "0:1,-,0:1" ) ),
		new( 11, "(a|ab)c", "abc", new() { Utf16 = true }, 0, new( "M", "0:3,0:2" ) ),
		new( 12, "(a+?)b", "aaab", new(), 0, new( "M", "0:4,0:3" ) ),
		new( 13, @"(?<x>é)\k<x>", "éé", new() { Utf16 = true }, 0, new( "M", "0:2,0:1" ) ),
		new( 14, @"(?<=😀)(x)", "😀x", new() { Utf16 = true }, 2, new( "M", "2:1,2:1" ) ),
		new( 15, @"(?=(a))a", "a", new(), 0, new( "M", "0:1,0:1" ) ),
		new( 16, @"(?*a|ab)abc", "abc", new() { Utf16 = true }, 0, new( "M", "0:3" ) ),
		new( 17, @"(?(DEFINE)(?<x>a\K))(?&x)b", "ab", new(), 0, new( "M", "1:1,-" ) ),
		new( 18, @"(?(DEFINE)(?<x>a\K))(?&x)X|(ab)", "ab", new() { Utf16 = true }, 0, new( "M", "0:2,-,0:2" ) ),
		new( 19, @"a\K(*MARK:é😀)b", "ab", new() { Utf16 = true }, 0, new( "M", "1:1", "00E9D83DDE00" ) ),
		new( 20, @"a\K(*ACCEPT:done)X", "ab", new(), 0, new( "M", "1:0", "0064006F006E0065" ) ),
		new( 21, @"a\K(*THEN)X|(ab)", "ab", new(), 0, new( "M", "0:2,0:2" ) ),
		new( 22, @"a(*MARK:M)(*SKIP:M)X|(b)", "ab", new() { Utf16 = true }, 0, new( "M", "1:1,1:1" ) ),
		new( 23, "(a)(*SKIP:missing)X|(ab)", "ab", new() { Policy = new() { EndAnchored = true } }, 0, new( "M", "0:2,-,0:2" ) ),
		new( 24, @"(?=a\K)", "a", new() { AllowLookaroundBackslashK = true }, 0, new( "M", "1:-1" ) ),
		new( 25, @"(?<=\Ka)b", "ab", new() { Utf16 = true, AllowLookaroundBackslashK = true }, 1, new( "M", "0:2" ) ),
		new( 26, @"a\K|ab", "ab", new() { Policy = new() { NotEmpty = true } }, 0, new( "M", "0:2" ) ),
		new( 27, @"\p{L}+", "éλ", new() { Utf16 = true, Ucp = true }, 0, new( "M", "0:2" ) ),
		new( 28, "x", "abc", new(), 0, new( "N" ) ),
		new( 29, "x", "😀", new() { Utf16 = true }, 0, new( "N" ) ),
		new( 30, "", "abc", new() { Utf16 = true }, 3, new( "M", "3:0" ) ),
		new( 31, "(?>a|ab)c", "abc", new(), 0, new( "N" ) ),
		new( 32, "a++a", "aa", new() { Utf16 = true }, 0, new( "N" ) ),
		new( 33, "a", "a", new() { MatchWorkLimit = 0 }, 0, new( "C", Diagnostic: "InvalidWorkLimit" ) ),
		new( 34, "a", "a", new() { MatchWorkLimit = -1 }, 0, new( "C", Diagnostic: "InvalidWorkLimit" ) ),
		new( 35, "ab", "ab", new() { MaxPatternLength = 1 }, 0, new( "C", Diagnostic: "PatternLengthLimitExceeded" ) ),
		new( 36, "\ud800", "", new() { Utf16 = true }, 0, new( "C", Diagnostic: "InvalidUtf16Pattern" ) ),
		new( 37, "a\udfff", "", new() { Utf16 = true }, 0, new( "C", Diagnostic: "InvalidUtf16Pattern", DiagnosticOffset: 1 ) ),
		new( 38, "a", "a", new(), -1, new( "X", Diagnostic: "InvalidStartOffset", DiagnosticOffset: -1 ) ),
		new( 39, "a", "a", new(), 2, new( "X", Diagnostic: "InvalidStartOffset", DiagnosticOffset: 2 ) ),
		new( 40, "x", "😀x", new() { Utf16 = true }, 1, new( "X", Diagnostic: "InvalidStartOffset", DiagnosticOffset: 1 ) ),
		new( 41, ".", "a\udfff", new() { Utf16 = true }, 0, new( "X", Diagnostic: "InvalidUtf16Subject", DiagnosticOffset: 1 ) ),
		new( 42, "a", "a", new() { MatchWorkLimit = 1 }, 0, new( "X", Diagnostic: "MatchWorkLimitExceeded" ) ),
		new( 43, @"(?=a\K)", "a", new(), 0, new( "C", Diagnostic: "InvalidSyntax" ) ),
		new( 44, @"\X", "a", new() { Utf16 = true }, 0, new( "C", Diagnostic: "UnsupportedSyntax" ) ),
		new( 45, "(*CRLF)(?m)^(😀+)$", "x\r\n😀😀\r\n", new() { Utf16 = true }, 0, new( "M", "3:4,3:4" ) ),
		new( 46, "(*ANYCRLF)(?s).X", "\r\nX", new(), 0, new( "M", "1:2" ) ),
		new( 47, @"(*ANY)\R$", "😀\u2028", new() { Utf16 = true }, 0, new( "M", "2:1" ) ),
		new( 48, @"(*CRLF)(?s)\N", "\r\nX", new(), 0, new( "M", "2:1" ) ),
		new( 49, @"(*ANYCRLF)[\x09-\x0c]X", "\r\nX", new() { Utf16 = true }, 0, new( "M", "1:2" ) ),
		new( 50, @"(*ANY)(?s:.{2})X", "\r\nbX", new(), 0, new( "M", "1:3" ) ),
	};
}
