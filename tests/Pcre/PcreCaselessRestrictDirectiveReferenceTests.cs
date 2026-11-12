using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pinned native CASELESS_RESTRICT prefix behavior across existing byte and UTF paths.</summary>
public sealed class PcreCaselessRestrictDirectiveReferenceTests {
	private const string Directive = "(*CASELESS_RESTRICT)";

	[Theory]
	[InlineData( "(*CASELESS_RESTRICT)k", "4B", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)k", "4B", "0:1", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)k", "4B", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)k", "4B", "0:1", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)k", "4B", "0:1", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)k", "4B", "0:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)k", "E284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)k", "E284AA", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)k", "E284AA", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)k", "E284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)k", "E284AA", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)K", "4B", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)K", "4B", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)K", "4B", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)K", "4B", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)K", "4B", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)K", "4B", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)s", "C5BF", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)s", "C5BF", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)s", "C5BF", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)s", "C5BF", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)s", "C5BF", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)s", "C5BF", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)ſ", "53", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)ſ", "53", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)ſ", "53", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)ſ", "53", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)ſ", "53", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)ſ", "53", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)é", "C389", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)é", "C389", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)é", "C389", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)é", "C389", "0:2", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)é", "C389", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)é", "C389", "0:2", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)Σ", "CF82", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)Σ", "CF82", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)Σ", "CF82", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)Σ", "CF82", "0:2", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)Σ", "CF82", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)Σ", "CF82", "0:2", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)ǅ", "C786", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)ǅ", "C786", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)ǅ", "C786", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)ǅ", "C786", "0:2", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)ǅ", "C786", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)ǅ", "C786", "0:2", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)I", "C4B1", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)I", "C4B1", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)I", "C4B1", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)I", "C4B1", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)I", "C4B1", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)I", "C4B1", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)İ", "69", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)İ", "69", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)İ", "69", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)İ", "69", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)İ", "69", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)İ", "69", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k", "E284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k", "E284AA", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k", "E284AA", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k", "E284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k", "E284AA", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k", "4B", "0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k", "4B", "0:1", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k", "4B", "0:1", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k", "4B", "0:1", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k", "4B", "0:1", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k", "4B", "0:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:k)k", "E284AA6B", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:k)k", "E284AA6B", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:k)k", "E284AA6B", "0:4", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:k)k", "E284AA6B", "0:4", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:k)k", "E284AA6B", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:k)k", "E284AA6B", "0:4", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:k)k", "E284AAE284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:k)k", "E284AAE284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:k)k", "E284AAE284AA", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:k)k", "E284AAE284AA", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:k)k", "E284AAE284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:k)k", "E284AAE284AA", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)k", "E284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)k", "E284AA", "0:3", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)k", "E284AA", "0:3", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)k", "E284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)k", "E284AA", "0:3", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^ir)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^ir)k", "E284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^ir)k", "E284AA", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^ir)k", "E284AA", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^ir)k", "E284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^ir)k", "E284AA", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?-r)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?-r)(?i)k", "E284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?-r)(?i)k", "E284AA", "0:3", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?-r)(?i)k", "E284AA", "0:3", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?-r)(?i)k", "E284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?-r)(?i)k", "E284AA", "0:3", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?-i)k", "4B", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?-i)k", "4B", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?-i)k", "4B", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?-i)k", "4B", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?-i)k", "4B", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?-i)k", "4B", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k)(?-r:\\1)", "6BE284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k)(?-r:\\1)", "6BE284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k)(?-r:\\1)", "6BE284AA", "0:4,0:1", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k)(?-r:\\1)", "6BE284AA", "0:4,0:1", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k)(?-r:\\1)", "6BE284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k)(?-r:\\1)", "6BE284AA", "0:4,0:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)\\k<x>", "6BE284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)\\k<x>", "6BE284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)\\k<x>", "6BE284AA", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)\\k<x>", "6BE284AA", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)\\k<x>", "6BE284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)\\k<x>", "6BE284AA", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)(?-r:\\k<x>)", "6BE284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)(?-r:\\k<x>)", "6BE284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)(?-r:\\k<x>)", "6BE284AA", "0:4,0:1", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)(?-r:\\k<x>)", "6BE284AA", "0:4,0:1", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)(?-r:\\k<x>)", "6BE284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)(?-r:\\k<x>)", "6BE284AA", "0:4,0:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:(k)x|(k))(?-r:\\2)", "6BE284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:(k)x|(k))(?-r:\\2)", "6BE284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:(k)x|(k))(?-r:\\2)", "6BE284AA", "0:4,-,0:1", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:(k)x|(k))(?-r:\\2)", "6BE284AA", "0:4,-,0:1", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:(k)x|(k))(?-r:\\2)", "6BE284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:(k)x|(k))(?-r:\\2)", "6BE284AA", "0:4,-,0:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[k]", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[k]", "E284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[k]", "E284AA", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[k]", "E284AA", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[k]", "E284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[k]", "E284AA", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[^k]", "E284AA", "0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[^k]", "E284AA", "0:1", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[^k]", "E284AA", "0:3", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[^k]", "E284AA", "0:3", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[^k]", "E284AA", "0:1", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[^k]", "E284AA", "0:3", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[kK]", "4B", "0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[kK]", "4B", "0:1", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[kK]", "4B", "0:1", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[kK]", "4B", "0:1", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[kK]", "4B", "0:1", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[kK]", "4B", "0:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[Σ]", "CF82", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[Σ]", "CF82", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[Σ]", "CF82", "0:2", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[Σ]", "CF82", "0:2", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[Σ]", "CF82", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[Σ]", "CF82", "0:2", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)\\Qk\\E", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)\\Qk\\E", "E284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)\\Qk\\E", "E284AA", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)\\Qk\\E", "E284AA", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)\\Qk\\E", "E284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)\\Qk\\E", "E284AA", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[\\p{Ll}k]", "E284AA", "0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[\\p{Ll}k]", "E284AA", "0:1", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[\\p{Ll}k]", "E284AA", "0:3", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[\\p{Ll}k]", "E284AA", "0:3", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[\\p{Ll}k]", "E284AA", "0:1", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[\\p{Ll}k]", "E284AA", "0:3", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)\\w", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)\\w", "E284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)\\w", "E284AA", "0:3", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)\\w", "E284AA", "0:3", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)\\w", "E284AA", "0:1", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)\\w", "E284AA", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[[:lower:]]", "4B", "0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[[:lower:]]", "4B", "0:1", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[[:lower:]]", "4B", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[[:lower:]]", "4B", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[[:lower:]]", "4B", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)[[:lower:]]", "4B", "0:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?[ [k] | [s] ])", "C5BF", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?[ [k] | [s] ])", "C5BF", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?[ [k] | [s] ])", "C5BF", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?[ [k] | [s] ])", "C5BF", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?[ [k] | [s] ])", "C5BF", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?[ [k] | [s] ])", "C5BF", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?[ [k] | [s] ])", "4B", "0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?[ [k] | [s] ])", "4B", "0:1", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?[ [k] | [s] ])", "4B", "0:1", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?[ [k] | [s] ])", "4B", "0:1", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?[ [k] | [s] ])", "4B", "0:1", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?[ [k] | [s] ])", "4B", "0:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=(k))k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=(k))k", "E284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=(k))k", "E284AA", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=(k))k", "E284AA", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=(k))k", "E284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=(k))k", "E284AA", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=(?-r:(k)))k", "6B", "0:1,0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=(?-r:(k)))k", "6B", "0:1,0:1", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=(?-r:(k)))k", "6B", "0:1,0:1", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=(?-r:(k)))k", "6B", "0:1,0:1", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=(?-r:(k)))k", "6B", "0:1,0:1", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=(?-r:(k)))k", "6B", "0:1,0:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?!k).", "E284AA", "0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?!k).", "E284AA", "0:1", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?!k).", "E284AA", "0:3", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?!k).", "E284AA", "0:3", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?!k).", "E284AA", "0:1", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?!k).", "E284AA", "0:3", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=k)a", "E284AA61", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=k)a", "E284AA61", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=k)a", "E284AA61", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=k)a", "E284AA61", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=k)a", "E284AA61", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=k)a", "E284AA61", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=(?-r:k))a", "E284AA61", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=(?-r:k))a", "E284AA61", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=(?-r:k))a", "E284AA61", "3:1", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=(?-r:k))a", "E284AA61", "3:1", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=(?-r:k))a", "E284AA61", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=(?-r:k))a", "E284AA61", "3:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=k{1,2})a", "E284AA6B61", "4:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=k{1,2})a", "E284AA6B61", "4:1", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=k{1,2})a", "E284AA6B61", "4:1", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=k{1,2})a", "E284AA6B61", "4:1", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=k{1,2})a", "E284AA6B61", "4:1", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<=k{1,2})a", "E284AA6B61", "4:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)(?&x)", "6B4B", "0:2,0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)(?&x)", "6B4B", "0:2,0:1", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)(?&x)", "6B4B", "0:2,0:1", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)(?&x)", "6B4B", "0:2,0:1", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)(?&x)", "6B4B", "0:2,0:1", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?<x>k)(?&x)", "6B4B", "0:2,0:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:(?&x))(?(DEFINE)(?<x>k))", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:(?&x))(?(DEFINE)(?<x>k))", "E284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:(?&x))(?(DEFINE)(?<x>k))", "E284AA", "", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:(?&x))(?(DEFINE)(?<x>k))", "E284AA", "", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:(?&x))(?(DEFINE)(?<x>k))", "E284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?-r:(?&x))(?(DEFINE)(?<x>k))", "E284AA", "", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?&x)(?(DEFINE)(?<x>(?-r:k)))", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?&x)(?(DEFINE)(?<x>(?-r:k)))", "E284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?&x)(?(DEFINE)(?<x>(?-r:k)))", "E284AA", "0:3,-", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?&x)(?(DEFINE)(?<x>(?-r:k)))", "E284AA", "0:3,-", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?&x)(?(DEFINE)(?<x>(?-r:k)))", "E284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?&x)(?(DEFINE)(?<x>(?-r:k)))", "E284AA", "0:3,-", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k(?1)?s)", "6B4B7353", "0:4,0:4", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k(?1)?s)", "6B4B7353", "0:4,0:4", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k(?1)?s)", "6B4B7353", "0:4,0:4", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k(?1)?s)", "6B4B7353", "0:4,0:4", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k(?1)?s)", "6B4B7353", "0:4,0:4", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k(?1)?s)", "6B4B7353", "0:4,0:4", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:k+|s+)", "E284AA6B6B", "3:2", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:k+|s+)", "E284AA6B6B", "3:2", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:k+|s+)", "E284AA6B6B", "3:2", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:k+|s+)", "E284AA6B6B", "3:2", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:k+|s+)", "E284AA6B6B", "3:2", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:k+|s+)", "E284AA6B6B", "3:2", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?>k|s).", "4B73", "0:2", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?>k|s).", "4B73", "0:2", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?>k|s).", "4B73", "0:2", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?>k|s).", "4B73", "0:2", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?>k|s).", "4B73", "0:2", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?>k|s).", "4B73", "0:2", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k(*ACCEPT:N)", "4B", "0:1", "4E", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k(*ACCEPT:N)", "4B", "0:1", "4E", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k(*ACCEPT:N)", "4B", "0:1", "4E", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k(*ACCEPT:N)", "4B", "0:1", "4E", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k(*ACCEPT:N)", "4B", "0:1", "4E", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)k(*ACCEPT:N)", "4B", "0:1", "4E", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=k(*ACCEPT:N))k", "4B", "0:1", "4E", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=k(*ACCEPT:N))k", "4B", "0:1", "4E", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=k(*ACCEPT:N))k", "4B", "0:1", "4E", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=k(*ACCEPT:N))k", "4B", "0:1", "4E", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=k(*ACCEPT:N))k", "4B", "0:1", "4E", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?=k(*ACCEPT:N))k", "4B", "0:1", "4E", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:k(*FAIL:N)|k(*MARK:M))", "4B", "0:1", "4D", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:k(*FAIL:N)|k(*MARK:M))", "4B", "0:1", "4D", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:k(*FAIL:N)|k(*MARK:M))", "4B", "0:1", "4D", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:k(*FAIL:N)|k(*MARK:M))", "4B", "0:1", "4D", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:k(*FAIL:N)|k(*MARK:M))", "4B", "0:1", "4D", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(?:k(*FAIL:N)|k(*MARK:M))", "4B", "0:1", "4D", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k)(?(1)(?-r:k)|s)", "6BE284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k)(?(1)(?-r:k)|s)", "6BE284AA", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k)(?(1)(?-r:k)|s)", "6BE284AA", "0:4,0:1", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k)(?(1)(?-r:k)|s)", "6BE284AA", "0:4,0:1", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k)(?(1)(?-r:k)|s)", "6BE284AA", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(k)(?(1)(?-r:k)|s)", "6BE284AA", "0:4,0:1", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)", "", "0:0", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)", "", "0:0", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)", "", "0:0", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)", "", "0:0", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)", "", "0:0", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)", "", "0:0", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)(?<x>k)(?-r:\\k<x>)", "E284AA6B", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)(?<x>k)(?-r:\\k<x>)", "E284AA6B", "", "", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)(?<x>k)(?-r:\\k<x>)", "E284AA6B", "0:4,0:3", "", 2 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)(?<x>k)(?-r:\\k<x>)", "E284AA6B", "0:4,0:3", "", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)(?<x>k)(?-r:\\k<x>)", "E284AA6B", "", "", 4 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)(?<x>k)(?-r:\\k<x>)", "E284AA6B", "0:4,0:3", "", 5 )]
	[InlineData( "(*CASELESS_RESTRICT)(*UTF)(*UCP)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*UTF)(*UCP)(?^i)k", "E284AA", "0:3", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*UTF)(*UCP)(?i)(k)", "4B", "0:1,0:1", "", 0 )]
	[InlineData( "(*UTF)(*CASELESS_RESTRICT)(*UCP)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*UTF)(*CASELESS_RESTRICT)(*UCP)(?^i)k", "E284AA", "0:3", "", 0 )]
	[InlineData( "(*UTF)(*CASELESS_RESTRICT)(*UCP)(?i)(k)", "4B", "0:1,0:1", "", 0 )]
	[InlineData( "(*UTF)(*UCP)(*CASELESS_RESTRICT)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*UTF)(*UCP)(*CASELESS_RESTRICT)(?^i)k", "E284AA", "0:3", "", 0 )]
	[InlineData( "(*UTF)(*UCP)(*CASELESS_RESTRICT)(?i)(k)", "4B", "0:1,0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*CASELESS_RESTRICT)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*CASELESS_RESTRICT)(?^i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*CASELESS_RESTRICT)(?i)(k)", "4B", "0:1,0:1", "", 0 )]
	[InlineData( "(*ANYCRLF)(*BSR_ANYCRLF)(*CASELESS_RESTRICT)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*ANYCRLF)(*BSR_ANYCRLF)(*CASELESS_RESTRICT)(?^i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*ANYCRLF)(*BSR_ANYCRLF)(*CASELESS_RESTRICT)(?i)(k)", "4B", "0:1,0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*NOTEMPTY_ATSTART)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*NOTEMPTY_ATSTART)(?^i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*NOTEMPTY_ATSTART)(?i)(k)", "4B", "0:1,0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*NOTEMPTY)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*NOTEMPTY)(?^i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*NOTEMPTY)(?i)(k)", "4B", "0:1,0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*NUL)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*NUL)(?^i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*NUL)(?i)(k)", "4B", "0:1,0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*CRLF)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*CRLF)(?^i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*CRLF)(?i)(k)", "4B", "0:1,0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*ANY)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*ANY)(?^i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*ANY)(?i)(k)", "4B", "0:1,0:1", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*BSR_UNICODE)(?i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*BSR_UNICODE)(?^i)k", "E284AA", "", "", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*BSR_UNICODE)(?i)(k)", "4B", "0:1,0:1", "", 0 )]
	public void PrefixModesRetainPinnedSpansCapturesAndLabels( string pattern, string hex, string spans, string markHex, int mode ) {
		var options = Options( mode ); var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern, options ); AssertLiteral( reference, subject, spans );
		if ( spans.Length > 0 ) { Assert.Equal( markHex, Convert.ToHexString( reference.Match( subject ).Mark ) ); }
		Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void AllOffsetsKeepFragmentsCrLfAndScopedComparisonModes( bool utf, bool anyCrLf ) {
		var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf };
		foreach ( var body in new[] { @"(?i)(k|s)", @"(?i)(?-r:(k))", @"(?i)(?<x>k)(?-r:\k<x>)", @"(?i)(?<=(?-r:k))a", @"(?i)(?=k).", @"(?i)[^k]+", @"(?i)k(*ACCEPT:N)", @"(?i)(k)(*SKIP:N)|s" } ) {
			var pattern = String.Concat( Directive, body ); var reference = Reference( pattern, options ); var compiled = Compile( pattern, options );
			foreach ( var hex in new[] { "E284AA6B4B0D0A73C5BF", "FF6B61E284AA61", "6BE284AAFF73", "0D0A6B4B73", "" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) { Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}" ); }
			}
		}
	}

	[Theory]
	[InlineData( " (*CASELESS_RESTRICT)", 1 )]
	[InlineData( "(?x)(*CASELESS_RESTRICT)", 4 )]
	[InlineData( "(?#x)(*CASELESS_RESTRICT)", 5 )]
	[InlineData( "(?i)(*CASELESS_RESTRICT)", 4 )]
	[InlineData( "a(*CASELESS_RESTRICT)", 1 )]
	[InlineData( "(?:(*CASELESS_RESTRICT))", 3 )]
	[InlineData( "(?=(*CASELESS_RESTRICT))", 3 )]
	[InlineData( "(*CASELESS_RESTRICT)|(*CASELESS_RESTRICT)", 21 )]
	[InlineData( "(*CASELESS_RESTRICT)(?#x)(*CASELESS_RESTRICT)", 25 )]
	[InlineData( "(*CASELESS_RESTRICT)(?^i)(*CASELESS_RESTRICT)", 25 )]
	[InlineData( "(*UTF)(?i)(*CASELESS_RESTRICT)", 10 )]
	[InlineData( "(*caseless_restrict)", 0 )]
	[InlineData( "(*CASELESS_RESTRICT:x)", 0 )]
	[InlineData( "(*CASELESS_RESTRICT )", 0 )]
	[InlineData( "(*CASELESS_RESTRICTX)", 0 )]
	[InlineData( "(*CASELESS_RESTRICT=1)", 0 )]
	[InlineData( "(*CASELESS_RESTRICT)(*TURKISH_CASINGX)", 20 )]
	[InlineData( "(*CASELESS_RESTRICT)(*NO_JIT)", 20 )]
	public void MisplacedNonExactAndPendingDirectivesKeepOpeningDiagnostics( string pattern, int offset ) {
		if ( !pattern.EndsWith( "(*NO_JIT)", StringComparison.Ordinal ) ) { Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) ); }
		else { Reference( pattern, new() ); }
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ) );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code ); Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(*CASELESS_RESTRICT", 1 )]
	[InlineData( "(*CASELESS_RESTRICT)*", 20 )]
	[InlineData( "(*CASELESS_RESTRICT){2}", 20 )]
	[InlineData( "(*CASELESS_RESTRICT)(", 20 )]
	[InlineData( "(*CASELESS_RESTRICT)é\\", 22 )]
	[InlineData( "(*CASELESS_RESTRICT)(?i)(", 24 )]
	public void MalformedBodiesKeepOriginalSourceByteDiagnostics( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code ); Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void UtfActivationStillValidatesTheEntireOwnedSource( bool directiveFirst ) {
		var prefix = directiveFirst ? String.Concat( Directive, "(*UTF)" ) : String.Concat( "(*UTF)", Directive );
		var bytes = Encoding.ASCII.GetBytes( prefix ).Concat( new byte[] { 0xff } ).ToArray();
		Assert.Throws<PcrePatternException>( () => Reference( bytes, new() ) );
		var result = PcreByteCompiler.Compile( bytes ); Assert.Null( result.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, result.Diagnostic?.Code ); Assert.Equal( prefix.Length, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void LongDuplicatePrefixesAddNoVmWorkAndKeepSharedBudgets() {
		var prefix = String.Concat( Enumerable.Repeat( Directive, 1024 ) ); const string body = @"(?i)(k+)(?-r:k)"; var subject = "kkK"u8.ToArray();
		var expectedSteps = new List<int>(); var steps = new List<int>();
		var direct = Compile( String.Concat( "(?r)", body ), new() { Utf8 = true, Ucp = true, WorkObserver = expectedSteps.Add } );
		var compiled = Compile( String.Concat( prefix, body ), new() { Utf8 = true, Ucp = true, WorkObserver = steps.Add } );
		var reference = Reference( String.Concat( prefix, body ), new() { Utf8 = true, Ucp = true } ); AssertLiteral( reference, subject, "0:3,0:2" );
		Compare( reference, subject, 0, compiled.Match( subject ), "long prefix" ); Assert.NotNull( direct.Match( subject ).Match );
		Assert.Equal( expectedSteps, steps ); Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		var limited = Compile( String.Concat( prefix, body ), new() { Utf8 = true, Ucp = true, MatchWorkLimit = steps.Count - 1 } ).Match( subject );
		Assert.Null( limited.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( String.Concat( prefix, body ), new() { WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( prefix ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void PrefixesDoNotConsumeCaptureMarkOrInstructionAllowances() {
		var prefix = String.Concat( Enumerable.Repeat( Directive, 300 ) );
		foreach ( var count in new[] { 256, 257 } ) {
			var captures = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( String.Concat( prefix, String.Concat( Enumerable.Repeat( "()", count ) ) ) ) );
			var marks = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( String.Concat( prefix, String.Concat( Enumerable.Repeat( "(?:a?)*", count ) ) ) ) );
			if ( count == 256 ) { Assert.True( captures.IsSuccess ); Assert.True( marks.IsSuccess ); Assert.NotNull( marks.Pattern!.Match( Array.Empty<byte>() ).Match ); }
			else { Assert.Equal( PcreByteDiagnosticCode.PatternCaptureLimitExceeded, captures.Diagnostic?.Code ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, marks.Diagnostic?.Code ); }
		}
		var expanded = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( String.Concat( prefix, "a{4096}" ) ) );
		Assert.Null( expanded.Pattern ); Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, expanded.Diagnostic?.Code );
	}

	[Fact]
	public void QuotedOrBracketedTextIsLiteralAndCompiledSourceIsOwned() {
		foreach ( var pattern in new[] { @"\Q(*CASELESS_RESTRICT)\E", @"[(*CASELESS_RESTRICT)]" } ) {
			var subject = Encoding.ASCII.GetBytes( Directive ); var reference = Reference( pattern, new() ); Compare( reference, subject, 0, Compile( pattern, new() ).Match( subject ), pattern );
		}
		var source = Encoding.UTF8.GetBytes( String.Concat( Directive, "(?i)k(*MARK:owned)" ) );
		var result = PcreByteCompiler.Compile( source, Options( 2 ) ); Assert.True( result.IsSuccess ); Array.Fill( source, (byte)'x' );
		Assert.Null( result.Pattern!.Match( Encoding.UTF8.GetBytes( "K" ) ).Match );
		Assert.Equal( "owned", Encoding.ASCII.GetString( result.Pattern.Match( "K"u8.ToArray() ).Match!.MarkName.Span ) );
	}

	private static PcreByteOptions Options( int mode ) => new() { Utf8 = mode is 2 or 3 or 5, Ucp = mode is 2 or 3 or 4, Caseless = mode is 1 or 3 or 4 or 5 };
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
