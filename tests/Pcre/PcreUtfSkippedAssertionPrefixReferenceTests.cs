using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins native bitmap-skipped assertion prefixes and their recovery barriers.</summary>
public sealed class PcreUtfSkippedAssertionPrefixReferenceTests {
	[Theory]
	[InlineData( @"(?<=B)([\x09-\x0c\x{100}])A", "420B41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<=\r)([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<=B)([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "" )]
	[InlineData( @"(?<=B)([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "" )]
	[InlineData( @"(?<=B)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?!\p{Lu})([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?!\p{Lu})([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?!\p{Lu})([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?![\x09-\x0e]B)([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?![\x09-\x0e]B)([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?![\x09-\x0e]B)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?<!B)([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<!B)([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<!B)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?<!B|C)([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<!B|C)([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<!B|C)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?<=\p{Cc})([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<=\p{Cc})([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<=\p{Cc})([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?<*\s)([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<*\s)([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<*\s)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?<=\s{1,2})([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<=\s{1,2})([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<=\s{1,2})([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,0:1,1:1" )]
	[InlineData( @"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,0:2,2:1" )]
	[InlineData( @"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,2:1,3:1" )]
	[InlineData( @"\B(?!B)\B([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"\B(?!B)\B([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"\B(?!B)\B([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"X{0}(?!B)(?:C){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"X{0}(?!B)(?:C){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"X{0}(?!B)(?:C){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?!B)(?<!C)([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?!B)(?<!C)([\x09-\x0c\x{100}])A", "090D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?!B)(?<!C)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, false, "3:2,3:1" )]
	[InlineData( @"(?:(?:(?:(?:(?!B|C)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(((((?!B|C)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1,1:1,1:1,1:1,1:1" )]
	[InlineData( @"(?>(?>(?>(?>(?!B|C)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?!B|C)[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 0, false, "1:3" )]
	[InlineData( @"(?!B|C)(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, false, "1:2" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]+)A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]+)A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, false, "2:3,2:2" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]+?)A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, false, "2:3,2:2" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]++)A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]++)A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, false, "2:3,2:2" )]
	[InlineData( @"(?:(?:(?:(?:(?<=\s)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(((((?<=\s)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1,1:1,1:1,1:1,1:1" )]
	[InlineData( @"(?>(?>(?>(?>(?<=\s)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<=\s)[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 0, false, "1:3" )]
	[InlineData( @"(?<=\s)(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, false, "1:2" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]+)A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]+)A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, false, "2:3,2:2" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]+?)A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, false, "2:3,2:2" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]++)A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]++)A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, false, "2:3,2:2" )]
	[InlineData( @"(?:(?:(?:(?:(?<!\w)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(((((?<!\w)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1,1:1,1:1,1:1,1:1" )]
	[InlineData( @"(?>(?>(?>(?>(?<!\w)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<!\w)[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 0, false, "1:3" )]
	[InlineData( @"(?<!\w)(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, false, "1:2" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]+)A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]+)A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, false, "2:3,2:2" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]+?)A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, false, "2:3,2:2" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]++)A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]++)A", "0B0D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, false, "2:3,2:2" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A|(?!C)[A-C]B", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A|(?!C)[A-C]B", "0D0A42", 0, false, "" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A|(?!C)[A-C]B", "0D0A410D", 0, false, "1:2,1:1" )]
	[InlineData( @"(?:(?!B)([\x09-\x0c\x{100}])A|(?<!C)([\x09-\x0c])B)", "0D0A41", 0, false, "1:2,1:1,-" )]
	[InlineData( @"(?:(?!B)([\x09-\x0c\x{100}])A|(?<!C)([\x09-\x0c])B)", "0D0A42", 0, false, "1:2,-,1:1" )]
	[InlineData( @"(?:(?!B)([\x09-\x0c\x{100}])A|(?<!C)([\x09-\x0c])B)", "0D0A410D", 0, false, "1:2,1:1,-" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A\1?", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A\1?", "0D0A42", 0, false, "" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A\1?", "0D0A410D", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<=(\s))([\x09-\x0c\x{100}])A\1?", "0D0A41", 0, false, "1:2,0:1,1:1" )]
	[InlineData( @"(?<=(\s))([\x09-\x0c\x{100}])A\1?", "0D0A42", 0, false, "" )]
	[InlineData( @"(?<=(\s))([\x09-\x0c\x{100}])A\1?", "0D0A410D", 0, false, "1:3,0:1,1:1" )]
	[InlineData( @"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A\1?", "0D0A41", 0, false, "1:2,0:1,1:1" )]
	[InlineData( @"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A\1?", "0D0A42", 0, false, "" )]
	[InlineData( @"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A\1?", "0D0A410D", 0, false, "1:3,0:1,1:1" )]
	[InlineData( @"(?!([B-C]))([\x09-\x0c\x{100}])A(?(1)B|)", "0D0A41", 0, false, "1:2,-,1:1" )]
	[InlineData( @"(?!([B-C]))([\x09-\x0c\x{100}])A(?(1)B|)", "0D0A42", 0, false, "" )]
	[InlineData( @"(?!([B-C]))([\x09-\x0c\x{100}])A(?(1)B|)", "0D0A410D", 0, false, "1:2,-,1:1" )]
	[InlineData( @"(?!B)B*([\x09-\x0c\x{100}]+)A", "420D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"B*(?!B)([\x09-\x0c\x{100}]+)A", "420D0A41", 0, false, "" )]
	[InlineData( @"B*(?!B)([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "" )]
	[InlineData( @"(?!B)B?([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?<!\w)B*([\x09-\x0c\x{100}]+)A", "420D0A41", 0, false, "2:2,2:1" )]
	[InlineData( @"B*(?<!\w)([\x09-\x0c\x{100}]+)A", "420D0A41", 0, false, "" )]
	[InlineData( @"B*(?<!\w)([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "" )]
	[InlineData( @"(?<!\w)B?([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "1:2,1:1" )]
	[InlineData( @"(?!B)([\x09-\x0e\x{100}])A", "0D0A41", 0, false, "" )]
	[InlineData( @"(?!B)([\x{100}-\x{10ffff}])A", "0D0A41", 0, false, "" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}\p{L}])A", "0D0A41", 0, false, "" )]
	[InlineData( @"(?!B)([^\r])A", "0D0A41", 0, false, "" )]
	[InlineData( @"(?=B)([\x09-\x0c\x{100}])A", "0B41", 0, false, "" )]
	[InlineData( @"(?=B|C)([\x09-\x0c\x{100}])A", "0B41", 0, false, "" )]
	[InlineData( @"(?*B|C)([\x09-\x0c\x{100}])A", "0B41", 0, false, "" )]
	[InlineData( @"(?!B)?([\x09-\x0c\x{100}])A", "0B41", 0, false, "0:2,0:1" )]
	[InlineData( @"(?<!B)?([\x09-\x0c\x{100}])A", "0B41", 0, false, "0:2,0:1" )]
	[InlineData( @"(?!B)[\x09-\x0c\x{100}]{2}A", "0B0B41", 0, false, "0:3" )]
	[InlineData( @"(?!B)\G([\x09-\x0c\x{100}])A", "0D0A41", 0, false, "" )]
	[InlineData( @"(?!B)[\x09-\x0c\x{100}]A|(?!C)[\x09-\x0e]B", "0D0A42", 0, false, "" )]
	[InlineData( @"\B(?!B)|(?<!C)([\x09-\x0c\x{100}])A", "420D0A41", 0, false, "" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "0D0A41", 1, false, "1:2,1:1" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "420A41", 1, false, "1:2,1:1" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "42420B41", 1, false, "2:2,2:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}])A", "0D0A41", 1, false, "1:2,1:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}])A", "420A41", 1, false, "" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}])A", "42420B41", 1, false, "" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}])A", "0D0A41", 1, false, "1:2,1:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}])A", "420A41", 1, false, "" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}])A", "42420B41", 1, false, "" )]
	[InlineData( @"(?<=B)([\x09-\x0c\x{100}])A", "420B41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<=\r)([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<=B)([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "" )]
	[InlineData( @"(?<=B)([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "" )]
	[InlineData( @"(?<=B)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?!\p{Lu})([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?!\p{Lu})([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?!\p{Lu})([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?![\x09-\x0e]B)([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?![\x09-\x0e]B)([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?![\x09-\x0e]B)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?<!B)([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<!B)([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<!B)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?<!B|C)([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<!B|C)([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<!B|C)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?<=\p{Cc})([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<=\p{Cc})([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<=\p{Cc})([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?<*\s)([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<*\s)([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<*\s)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?<=\s{1,2})([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<=\s{1,2})([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<=\s{1,2})([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,0:1,1:1" )]
	[InlineData( @"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,0:2,2:1" )]
	[InlineData( @"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,2:1,3:1" )]
	[InlineData( @"\B(?!B)\B([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"\B(?!B)\B([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"\B(?!B)\B([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"X{0}(?!B)(?:C){0}([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"X{0}(?!B)(?:C){0}([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"X{0}(?!B)(?:C){0}([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?!B)(?<!C)([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?!B)(?<!C)([\x09-\x0c\x{100}])A", "090D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?!B)(?<!C)([\x09-\x0c\x{100}])A", "C4800D0A41", 0, true, "3:2,3:1" )]
	[InlineData( @"(?:(?:(?:(?:(?!B|C)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(((((?!B|C)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1,1:1,1:1,1:1,1:1" )]
	[InlineData( @"(?>(?>(?>(?>(?!B|C)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?!B|C)[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 0, true, "1:3" )]
	[InlineData( @"(?!B|C)(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, true, "1:2" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]+)A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]+)A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, true, "2:3,2:2" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]+?)A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, true, "2:3,2:2" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]++)A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]++)A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?!B|C)([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, true, "2:3,2:2" )]
	[InlineData( @"(?:(?:(?:(?:(?<=\s)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(((((?<=\s)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1,1:1,1:1,1:1,1:1" )]
	[InlineData( @"(?>(?>(?>(?>(?<=\s)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<=\s)[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 0, true, "1:3" )]
	[InlineData( @"(?<=\s)(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, true, "1:2" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]+)A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]+)A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, true, "2:3,2:2" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]+?)A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, true, "2:3,2:2" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]++)A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]++)A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, true, "2:3,2:2" )]
	[InlineData( @"(?:(?:(?:(?:(?<!\w)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(((((?<!\w)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1,1:1,1:1,1:1,1:1" )]
	[InlineData( @"(?>(?>(?>(?>(?<!\w)([\x09-\x0c\x{100}])))))A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<!\w)[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 0, true, "1:3" )]
	[InlineData( @"(?<!\w)(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, true, "1:2" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]+)A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]+)A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, true, "2:3,2:2" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]+?)A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, true, "2:3,2:2" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]++)A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]++)A", "0B0D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, true, "2:3,2:2" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A|(?!C)[A-C]B", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A|(?!C)[A-C]B", "0D0A42", 0, true, "" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A|(?!C)[A-C]B", "0D0A410D", 0, true, "1:2,1:1" )]
	[InlineData( @"(?:(?!B)([\x09-\x0c\x{100}])A|(?<!C)([\x09-\x0c])B)", "0D0A41", 0, true, "1:2,1:1,-" )]
	[InlineData( @"(?:(?!B)([\x09-\x0c\x{100}])A|(?<!C)([\x09-\x0c])B)", "0D0A42", 0, true, "1:2,-,1:1" )]
	[InlineData( @"(?:(?!B)([\x09-\x0c\x{100}])A|(?<!C)([\x09-\x0c])B)", "0D0A410D", 0, true, "1:2,1:1,-" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A\1?", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A\1?", "0D0A42", 0, true, "" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A\1?", "0D0A410D", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<=(\s))([\x09-\x0c\x{100}])A\1?", "0D0A41", 0, true, "1:2,0:1,1:1" )]
	[InlineData( @"(?<=(\s))([\x09-\x0c\x{100}])A\1?", "0D0A42", 0, true, "" )]
	[InlineData( @"(?<=(\s))([\x09-\x0c\x{100}])A\1?", "0D0A410D", 0, true, "1:3,0:1,1:1" )]
	[InlineData( @"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A\1?", "0D0A41", 0, true, "1:2,0:1,1:1" )]
	[InlineData( @"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A\1?", "0D0A42", 0, true, "" )]
	[InlineData( @"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A\1?", "0D0A410D", 0, true, "1:3,0:1,1:1" )]
	[InlineData( @"(?!([B-C]))([\x09-\x0c\x{100}])A(?(1)B|)", "0D0A41", 0, true, "1:2,-,1:1" )]
	[InlineData( @"(?!([B-C]))([\x09-\x0c\x{100}])A(?(1)B|)", "0D0A42", 0, true, "" )]
	[InlineData( @"(?!([B-C]))([\x09-\x0c\x{100}])A(?(1)B|)", "0D0A410D", 0, true, "1:2,-,1:1" )]
	[InlineData( @"(?!B)B*([\x09-\x0c\x{100}]+)A", "420D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"B*(?!B)([\x09-\x0c\x{100}]+)A", "420D0A41", 0, true, "" )]
	[InlineData( @"B*(?!B)([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "" )]
	[InlineData( @"(?!B)B?([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?<!\w)B*([\x09-\x0c\x{100}]+)A", "420D0A41", 0, true, "2:2,2:1" )]
	[InlineData( @"B*(?<!\w)([\x09-\x0c\x{100}]+)A", "420D0A41", 0, true, "" )]
	[InlineData( @"B*(?<!\w)([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "" )]
	[InlineData( @"(?<!\w)B?([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "1:2,1:1" )]
	[InlineData( @"(?!B)([\x09-\x0e\x{100}])A", "0D0A41", 0, true, "" )]
	[InlineData( @"(?!B)([\x{100}-\x{10ffff}])A", "0D0A41", 0, true, "" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}\p{L}])A", "0D0A41", 0, true, "" )]
	[InlineData( @"(?!B)([^\r])A", "0D0A41", 0, true, "" )]
	[InlineData( @"(?=B)([\x09-\x0c\x{100}])A", "0B41", 0, true, "" )]
	[InlineData( @"(?=B|C)([\x09-\x0c\x{100}])A", "0B41", 0, true, "" )]
	[InlineData( @"(?*B|C)([\x09-\x0c\x{100}])A", "0B41", 0, true, "" )]
	[InlineData( @"(?!B)?([\x09-\x0c\x{100}])A", "0B41", 0, true, "0:2,0:1" )]
	[InlineData( @"(?<!B)?([\x09-\x0c\x{100}])A", "0B41", 0, true, "0:2,0:1" )]
	[InlineData( @"(?!B)[\x09-\x0c\x{100}]{2}A", "0B0B41", 0, true, "0:3" )]
	[InlineData( @"(?!B)\G([\x09-\x0c\x{100}])A", "0D0A41", 0, true, "" )]
	[InlineData( @"(?!B)[\x09-\x0c\x{100}]A|(?!C)[\x09-\x0e]B", "0D0A42", 0, true, "" )]
	[InlineData( @"\B(?!B)|(?<!C)([\x09-\x0c\x{100}])A", "420D0A41", 0, true, "" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "0D0A41", 1, true, "1:2,1:1" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "420A41", 1, true, "1:2,1:1" )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "42420B41", 1, true, "2:2,2:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}])A", "0D0A41", 1, true, "1:2,1:1" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}])A", "420A41", 1, true, "" )]
	[InlineData( @"(?<=\s)([\x09-\x0c\x{100}])A", "42420B41", 1, true, "" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}])A", "0D0A41", 1, true, "1:2,1:1" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}])A", "420A41", 1, true, "" )]
	[InlineData( @"(?<!\w)([\x09-\x0c\x{100}])A", "42420B41", 1, true, "" )]
	public void SkippedAssertionPrefixesPreservePinnedNativeCaptureSpans( string body, string hex, int offset, bool ucp, string spans ) {
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
	public void EveryCallerOffsetKeepsSkippedAssertionPrefixAndFragmentBoundaries( string newline ) {
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
	public void StrictSkippedAssertionPrefixesRetainUtfValidationAndCallerBoundaries( bool ucp ) {
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
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A", "0D0A41", 1, false )]
	[InlineData( @"(?!B|C)[\x09-\x0c\x{100}]{1,2}A", "0D0A0B41", 1, true )]
	[InlineData( @"(?!B)([\x09-\x0c\x{100}])A|(?!C)[A-C]B", "0D0A41", 1, false )]
	[InlineData( @"(?!B)([^\r])A", "0D0A41", -1, true )]
	public void SkippedAssertionPrefixSharesExactExhaustedAndObservedWork( string body, string hex, int index, bool ucp ) {
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
	public void SkippedAssertionPrefixesWithoutLfRetainTheDefaultBudget() {
		var pattern = String.Concat( "(*ANYCRLF)", @"(?!B)([\x{100}-\x{10ffff}])A" );
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
	public void SkippedAssertionPrefixRetainsPreAndMidMatchCancellation() {
		var pattern = String.Concat( "(*ANYCRLF)", @"(?!B)([\x09-\x0c\x{100}])A" );
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
	public void LongSkippedAssertionPrefixesKeepOrdinaryMatchingAndCancellation( int count ) {
		var pattern = String.Concat( "(*ANYCRLF)", String.Concat( Enumerable.Repeat( @"(?!B)", count ) ), @"([\x09-\x0c\x{100}])A" );
		var subject = Convert.FromHexString( "0B41" );
		Compare( Reference( pattern ), subject, 0, Compile( pattern, new() { Utf8 = true } ).Match( subject ), pattern );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ),
			new() { Utf8 = true }, cancellation.Token ) );
	}

	private static string[] Bodies() => new[] {
		@"(?!B)([\x09-\x0c\x{100}])A",
		@"(?!B|C)([\x09-\x0c\x{100}])A",
		@"(?<=\s)([\x09-\x0c\x{100}])A",
		@"(?<!\w)([\x09-\x0c\x{100}])A",
		@"(?<*(\s{1,2}))([\x09-\x0c\x{100}])A",
		@"(?:(?!B)([\x09-\x0c\x{100}])A|(?<!C)([\x09-\x0c])B)",
		@"(?!B)[\x09-\x0c\x{100}]{1,2}A",
		@"(?!B)([\x09-\x0c\x{100}]+)A"
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
