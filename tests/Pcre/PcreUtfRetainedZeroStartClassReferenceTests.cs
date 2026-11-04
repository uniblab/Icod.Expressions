using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins retained zero-count prefixes before direct UTF starter classes.</summary>
public sealed class PcreUtfRetainedZeroStartClassReferenceTests {
	[Theory]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,-,1:1" )]
	[InlineData( @"(B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,-,2:1" )]
	[InlineData( @"(B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,-,3:1" )]
	[InlineData( @"(?>B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?>B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?>B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?=B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?=B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?=B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?!B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?!B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?!B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?<=B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<=B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<=B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?<!B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<!B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<!B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?=B|C){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?=B|C){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?=B|C){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?<=B|C){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<=B|C){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<=B|C){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?:\p{Lu}|\r?){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?:\p{Lu}|\r?){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?:\p{Lu}|\r?){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?:\p{Lu}){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?:\p{Lu}){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?:\p{Lu}){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?:[\x09-\x0e]){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?:[\x09-\x0e]){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?:[\x09-\x0e]){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?:B){0}?([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?:B){0}?([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?:B){0}?([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?:B){0}+([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?:B){0}+([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?:B){0}+([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?>B){0}+([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?>B){0}+([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?>B){0}+([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"\B(?:B){0}\B([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"\B(?:B){0}\B([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"\B(?:B){0}\B([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"X{0}(?:B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"X{0}(?:B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"X{0}(?:B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?:(?:(?:(?:(?:B){0}([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(((((?:B){0}([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1,1:1,1:1,1:1,1:1" )]
	[InlineData( @"(?>(?>(?>(?>(?:B){0}([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?:B){0}[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 0, false, "1:3" )]
	[InlineData( @"(?:B){0}(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, false, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:(?=B|C){0}([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(((((?=B|C){0}([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1,1:1,1:1,1:1,1:1" )]
	[InlineData( @"(?>(?>(?>(?>(?=B|C){0}([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?=B|C){0}[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 0, false, "1:3" )]
	[InlineData( @"(?=B|C){0}(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, false, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:(?>B){0}+([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(((((?>B){0}+([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1,1:1,1:1,1:1,1:1" )]
	[InlineData( @"(?>(?>(?>(?>(?>B){0}+([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?>B){0}+[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 0, false, "1:3" )]
	[InlineData( @"(?>B){0}+(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, false, "1:2" )]
	[InlineData( @"(?<x>B){0}([\x09-\x0c\x{100}])A\k<x>?", "0D0A41", 0, false, "1:2,-,1:1" )]
	[InlineData( @"(?<x>B){0}([\x09-\x0c\x{100}])A\k<x>?", "C4800D0A41", 0, false, "3:2,-,3:1" )]
	[InlineData( @"(B){0}([\x09-\x0c\x{100}])A(?(1)B|)", "0D0A41", 0, false, "1:2,-,1:1" )]
	[InlineData( @"(B){0}([\x09-\x0c\x{100}])A(?(1)B|)", "C4800D0A41", 0, false, "3:2,-,3:1" )]
	[InlineData( @"(?|(B)|(C)){0}([\x09-\x0c\x{100}])A\1?", "0D0A41", 0, false, "1:2,-,1:1" )]
	[InlineData( @"(?|(B)|(C)){0}([\x09-\x0c\x{100}])A\1?", "C4800D0A41", 0, false, "3:2,-,3:1" )]
	[InlineData( @"(?<x>B){0}(?:\k<x>|C){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,-,1:1" )]
	[InlineData( @"(?<x>B){0}(?:\k<x>|C){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,-,3:1" )]
	[InlineData( @"(?<x>B){0}(?:(?&x)){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,-,1:1" )]
	[InlineData( @"(?<x>B){0}(?:(?&x)){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,-,3:1" )]
	[InlineData( @"(B){0}(?(1)C|D){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,-,1:1" )]
	[InlineData( @"(B){0}(?(1)C|D){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,-,3:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A|(?:C){0}[A-C]B", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A|(?:C){0}[A-C]B", "0D0A42", 0, false, "" )]
	[InlineData( @"(?:(?:B){0}([\x09-\x0c\x{100}])A|(?=C){0}([\x09-\x0c])B)", "0D0A41", 0, false, "1:2,1:1,-" )]
	[InlineData( @"(?:(?:B){0}([\x09-\x0c\x{100}])A|(?=C){0}([\x09-\x0c])B)", "0D0A42", 0, false, "1:2,-,1:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A\1?", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A\1?", "0D0A42", 0, false, "" )]
	[InlineData( @"(?:B){0}([\x09-\x0e\x{100}])A", "0D0A41", 0, false, "" )]
	[InlineData( @"(?:B){0}([\x{100}-\x{10ffff}])A", "0D0A41", 0, false, "" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}\p{L}])A", "0D0A41", 0, false, "" )]
	[InlineData( @"(?:B){0}([^\r])A", "0D0A41", 0, false, "" )]
	[InlineData( @"(?:B)?([\x09-\x0c\x{100}])A", "0B41", 0, false, "0:2,0:1" )]
	[InlineData( @"(?:B)*([\x09-\x0c\x{100}])A", "0B41", 0, false, "0:2,0:1" )]
	[InlineData( @"(?(R)B|C){0}([\x09-\x0c\x{100}])A", "0B41", 0, false, "0:2,0:1" )]
	[InlineData( @"(?(DEFINE)(?<x>B)){0}([\x09-\x0c\x{100}])A", "0B41", 0, false, "0:2,-,0:1" )]
	[InlineData( @"(B){0}(?1){0}([\x09-\x0c\x{100}])A", "0B41", 0, false, "0:2,-,0:1" )]
	[InlineData( @"(?:B){0}[\x09-\x0c\x{100}]{2}A", "0B0B41", 0, false, "0:3" )]
	[InlineData( @"(?:B){0}\G([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "" )]
	[InlineData( @"(?:B){0}[\x09-\x0c\x{100}]A|(?:C){0}[\x09-\x0e]B", "0D0A42", 0, false, "" )]
	[InlineData( @"\B(?:B){0}|\B(?:C){0}([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}]+)A", "420D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?:B){0}B*([\x09-\x0c\x{100}]+)A", "420D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"B*(?:C){0}([\x09-\x0c\x{100}]+)A", "420D0A41", 0, false, "" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}]+)A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?=B){0}([\x09-\x0c\x{100}]+?)A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?>B){0}+([\x09-\x0c\x{100}]++)A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,-,1:1" )]
	[InlineData( @"(B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,-,2:1" )]
	[InlineData( @"(B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,-,3:1" )]
	[InlineData( @"(?>B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?>B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?>B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?=B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?=B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?=B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?!B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?!B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?!B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?<=B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<=B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<=B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?<!B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<!B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<!B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?=B|C){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?=B|C){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?=B|C){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?<=B|C){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<=B|C){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<=B|C){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?:\p{Lu}|\r?){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?:\p{Lu}|\r?){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?:\p{Lu}|\r?){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?:\p{Lu}){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?:\p{Lu}){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?:\p{Lu}){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?:[\x09-\x0e]){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?:[\x09-\x0e]){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?:[\x09-\x0e]){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?:B){0}?([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?:B){0}?([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?:B){0}?([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?:B){0}+([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?:B){0}+([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?:B){0}+([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?>B){0}+([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?>B){0}+([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?>B){0}+([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"\B(?:B){0}\B([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"\B(?:B){0}\B([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"\B(?:B){0}\B([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"X{0}(?:B){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"X{0}(?:B){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"X{0}(?:B){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?:(?:(?:(?:(?:B){0}([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(((((?:B){0}([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1,1:1,1:1,1:1,1:1" )]
	[InlineData( @"(?>(?>(?>(?>(?:B){0}([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?:B){0}[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 0, true, "1:3" )]
	[InlineData( @"(?:B){0}(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, true, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:(?=B|C){0}([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(((((?=B|C){0}([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1,1:1,1:1,1:1,1:1" )]
	[InlineData( @"(?>(?>(?>(?>(?=B|C){0}([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?=B|C){0}[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 0, true, "1:3" )]
	[InlineData( @"(?=B|C){0}(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, true, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:(?>B){0}+([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(((((?>B){0}+([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1,1:1,1:1,1:1,1:1" )]
	[InlineData( @"(?>(?>(?>(?>(?>B){0}+([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?>B){0}+[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 0, true, "1:3" )]
	[InlineData( @"(?>B){0}+(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, true, "1:2" )]
	[InlineData( @"(?<x>B){0}([\x09-\x0c\x{100}])A\k<x>?", "0D0A41", 0, true, "1:2,-,1:1" )]
	[InlineData( @"(?<x>B){0}([\x09-\x0c\x{100}])A\k<x>?", "C4800D0A41", 0, true, "3:2,-,3:1" )]
	[InlineData( @"(B){0}([\x09-\x0c\x{100}])A(?(1)B|)", "0D0A41", 0, true, "1:2,-,1:1" )]
	[InlineData( @"(B){0}([\x09-\x0c\x{100}])A(?(1)B|)", "C4800D0A41", 0, true, "3:2,-,3:1" )]
	[InlineData( @"(?|(B)|(C)){0}([\x09-\x0c\x{100}])A\1?", "0D0A41", 0, true, "1:2,-,1:1" )]
	[InlineData( @"(?|(B)|(C)){0}([\x09-\x0c\x{100}])A\1?", "C4800D0A41", 0, true, "3:2,-,3:1" )]
	[InlineData( @"(?<x>B){0}(?:\k<x>|C){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,-,1:1" )]
	[InlineData( @"(?<x>B){0}(?:\k<x>|C){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,-,3:1" )]
	[InlineData( @"(?<x>B){0}(?:(?&x)){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,-,1:1" )]
	[InlineData( @"(?<x>B){0}(?:(?&x)){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,-,3:1" )]
	[InlineData( @"(B){0}(?(1)C|D){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,-,1:1" )]
	[InlineData( @"(B){0}(?(1)C|D){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,-,3:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A|(?:C){0}[A-C]B", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A|(?:C){0}[A-C]B", "0D0A42", 0, true, "" )]
	[InlineData( @"(?:(?:B){0}([\x09-\x0c\x{100}])A|(?=C){0}([\x09-\x0c])B)", "0D0A41", 0, true, "1:2,1:1,-" )]
	[InlineData( @"(?:(?:B){0}([\x09-\x0c\x{100}])A|(?=C){0}([\x09-\x0c])B)", "0D0A42", 0, true, "1:2,-,1:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A\1?", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A\1?", "0D0A42", 0, true, "" )]
	[InlineData( @"(?:B){0}([\x09-\x0e\x{100}])A", "0D0A41", 0, true, "" )]
	[InlineData( @"(?:B){0}([\x{100}-\x{10ffff}])A", "0D0A41", 0, true, "" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}\p{L}])A", "0D0A41", 0, true, "" )]
	[InlineData( @"(?:B){0}([^\r])A", "0D0A41", 0, true, "" )]
	[InlineData( @"(?:B)?([\x09-\x0c\x{100}])A", "0B41", 0, true, "0:2,0:1" )]
	[InlineData( @"(?:B)*([\x09-\x0c\x{100}])A", "0B41", 0, true, "0:2,0:1" )]
	[InlineData( @"(?(R)B|C){0}([\x09-\x0c\x{100}])A", "0B41", 0, true, "0:2,0:1" )]
	[InlineData( @"(?(DEFINE)(?<x>B)){0}([\x09-\x0c\x{100}])A", "0B41", 0, true, "0:2,-,0:1" )]
	[InlineData( @"(B){0}(?1){0}([\x09-\x0c\x{100}])A", "0B41", 0, true, "0:2,-,0:1" )]
	[InlineData( @"(?:B){0}[\x09-\x0c\x{100}]{2}A", "0B0B41", 0, true, "0:3" )]
	[InlineData( @"(?:B){0}\G([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "" )]
	[InlineData( @"(?:B){0}[\x09-\x0c\x{100}]A|(?:C){0}[\x09-\x0e]B", "0D0A42", 0, true, "" )]
	[InlineData( @"\B(?:B){0}|\B(?:C){0}([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}]+)A", "420D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?:B){0}B*([\x09-\x0c\x{100}]+)A", "420D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"B*(?:C){0}([\x09-\x0c\x{100}]+)A", "420D0A41", 0, true, "" )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}]+)A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?=B){0}([\x09-\x0c\x{100}]+?)A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?>B){0}+([\x09-\x0c\x{100}]++)A", "0B0D0A41", 0, true, "2:2,2:1" )]
	public void RetainedZeroStartClassesPreservePinnedNativeCaptureSpans( string body, string hex, int offset, bool ucp, string spans ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		foreach ( var tolerant in new[] { false, true } ) {
			var reference = Reference( pattern, ucp, tolerant );
			var expected = reference.Match( subject, offset );
			Assert.Equal( spans.Length > 0, expected.Success );
			if ( expected.Success ) {
				var groups = spans.Split( ',' );
				Assert.Equal( groups.Length, expected.Groups.Count );
				for ( var group = 0; group < groups.Length; group++ ) {
					var capture = expected.Groups[group];
					Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
				}
			}
			Compare( reference, subject, offset, Compile( pattern, new() { Utf8 = true, Ucp = ucp,
				MatchInvalidUtf = tolerant } ).Match( subject, offset ), pattern );
		}
	}

	[Theory]
	[InlineData( "(*LF)" )]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CR)" )]
	[InlineData( "(*NUL)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void EveryCallerOffsetKeepsRetainedZeroStartClassAndFragmentBoundaries( string newline ) {
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var body in Bodies() ) {
				var pattern = String.Concat( newline, body );
				var reference = Reference( pattern, ucp, true );
				var compiled = Compile( pattern, new() { Utf8 = true, Ucp = ucp, MatchInvalidUtf = true } );
				foreach ( var hex in Subjects() ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}/{ucp}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void StrictRetainedZeroStartClassesRetainUtfValidationAndCallerBoundaries( bool ucp ) {
		foreach ( var newline in new[] { "(*LF)", "(*ANYCRLF)" } ) {
			foreach ( var body in Bodies() ) {
				var pattern = String.Concat( newline, body );
				var reference = Reference( pattern, ucp, false );
				var compiled = Compile( pattern, new() { Utf8 = true, Ucp = ucp } );
				foreach ( var hex in new[] { "0D0A41", "0B0D0A0B41", "C4800D0A41", "C4800D0A0B41" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						if ( offset < subject.Length && ( subject[offset] & 0xC0 ) == 0x80 ) {
							Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( subject, offset ).Diagnostic?.Code );
						} else {
							Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}" );
						}
					}
				}
				Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject,
					compiled.Match( Convert.FromHexString( "FF0B0D0A41" ) ).Diagnostic?.Code );
			}
		}
	}

	[Theory]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A", "0D0A41", 1, false )]
	[InlineData( @"(?:B){0}[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 1, true )]
	[InlineData( @"(?:B){0}([\x09-\x0c\x{100}])A|(?:C){0}[A-C]B", "0D0A41", 1, false )]
	[InlineData( @"(?:B){0}([^\r])A", "0D0A41", -1, true )]
	public void RetainedZeroStartClassSharesExactExhaustedAndObservedWork( string body, string hex, int index, bool ucp ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		var steps = new List<int>();
		var result = Compile( pattern, new() { Utf8 = true, Ucp = ucp, WorkObserver = steps.Add } ).Match( subject );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index, result.Match?.ByteIndex ?? -1 );
		Compare( Reference( pattern, ucp ), subject, 0, result, pattern );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		foreach ( var observed in new[] { false, true } ) {
			Compare( Reference( pattern, ucp ), subject, 0, Compile( pattern, new() { Utf8 = true, Ucp = ucp,
				MatchWorkLimit = steps.Count, WorkObserver = observed ? _ => { } : null } ).Match( subject ), pattern );
			var exhausted = Compile( pattern, new() { Utf8 = true, Ucp = ucp, MatchWorkLimit = steps.Count - 1,
				WorkObserver = observed ? _ => { } : null } ).Match( subject );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
	}

	[Fact]
	public void RetainedZeroStartClassesWithoutLfRetainTheDefaultBudget() {
		var pattern = String.Concat( "(*ANYCRLF)", @"(?:B){0}([\x{100}-\x{10ffff}])A" );
		var subject = Encoding.ASCII.GetBytes( String.Concat( Enumerable.Repeat( "\r\n", 40_000 ) ) );
		Assert.False( Reference( pattern ).Match( subject ).Success );
		foreach ( var observed in new[] { false, true } ) {
			var work = 0;
			var result = Compile( pattern, new() { Utf8 = true,
				WorkObserver = observed ? count => { work = count; } : null } ).Match( subject );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.Null( result.Match );
			if ( observed ) { Assert.InRange( work, 1, PcreByteOptions.DefaultMatchWorkLimit ); }
		}
	}

	[Fact]
	public void RetainedZeroStartClassRetainsPreAndMidMatchCancellation() {
		var pattern = String.Concat( "(*ANYCRLF)", @"(?:B){0}([\x09-\x0c\x{100}])A" );
		var subject = Convert.FromHexString( "090D0A41" );
		var steps = new List<int>();
		var result = Compile( pattern, new() { Utf8 = true, WorkObserver = steps.Add } ).Match( subject );
		Assert.Equal( 2, result.Match?.ByteIndex );
		Compare( Reference( pattern ), subject, 0, result, pattern );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new() { Utf8 = true,
			WorkObserver = count => { if ( count == steps.Count / 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( 33 )]
	[InlineData( 999 )]
	[InlineData( 1001 )]
	public void LongRetainedZeroStartClassesKeepOrdinaryMatchingAndCancellation( int count ) {
		var pattern = String.Concat( "(*ANYCRLF)", String.Concat( Enumerable.Repeat( @"(?:B){0}", count ) ), @"([\x09-\x0c\x{100}])A" );
		var subject = Convert.FromHexString( "0B41" );
		Compare( Reference( pattern ), subject, 0, Compile( pattern, new() { Utf8 = true } ).Match( subject ), pattern );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ),
			new() { Utf8 = true }, cancellation.Token ) );
	}

	private static string[] Bodies() => new[] {
		@"(?:B){0}([\x09-\x0c\x{100}])A",
		@"\B(?:B){0}\B([\x09-\x0c\x{100}])A",
		@"(?>B){0}+([\x09-\x0c\x{100}])A",
		@"(?=B|C){0}([\x09-\x0c\x{100}])A",
		@"(?<=B|C){0}([\x09-\x0c\x{100}])A",
		@"(?<x>B){0}([\x09-\x0c\x{100}])A\k<x>?",
		@"(?:(?:B){0}([\x09-\x0c\x{100}])A|(?=C){0}([\x09-\x0c])B)",
		@"(B){0}(?(1)C|D){0}([\x09-\x0c\x{100}])A",
		@"(?:B){0}[\x09-\x0c\x{100}]{1,2}A",
		@"(?:B){0}(?:[\x09-\x0c\x{100}]A)+"
	};
	private static string[] Subjects() => new[] { "", "00", "0D0A41", "0D0A42", "0B0D0A41", "0B0D0A0B41", "C4800D0A41", "C48041", "FF0B0D0A41", "0BFF0D0A41", "0B0D0AFF41", "EDA0800B0D0A41", "090D0A41" };

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}: native={( expected.Success ? expected.Index : -1 )}, managed={actual.Match?.ByteIndex ?? -1}" );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool ucp = false, bool tolerant = false ) => new(
		Encoding.ASCII.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | PcreOptions.Utf | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
