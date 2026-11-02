using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Preserves original class source shape before canonical case folding.</summary>
public sealed class PcreUtfSetSourceShapeReferenceTests {
	[Theory]
	[InlineData( @"(?i)[kk]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kk]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kk]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[Kk]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[Kk]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[Kk]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[k-kk]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[k-kk]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[k-kk]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[\Qkk\E]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[\Qkk\E]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[\Qkk\E]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[\x6b\x6b]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[\x6b\x6b]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[\x6b\x6b]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[k\x{212a}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[k\x{212a}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[k\x{212a}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[\x{212a}\x{212a}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[\x{212a}\x{212a}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[\x{212a}\x{212a}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[ss]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[ss]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[ss]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[sS]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[sS]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[sS]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[s\x{17f}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[s\x{17f}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[s\x{17f}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[\x{3c3}\x{3c2}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[\x{3c3}\x{3c2}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[\x{3c3}\x{3c2}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[\x{3a3}\x{3c2}\x{3c3}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[\x{3a3}\x{3c2}\x{3c3}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[\x{3a3}\x{3c2}\x{3c3}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[\x{1c4}-\x{1c6}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[\x{1c4}-\x{1c6}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[\x{1c4}-\x{1c6}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[bb]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[bb]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[bb]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[bB]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[bB]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[bB]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]??([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]??([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]??([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]??([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]??([\x09-\x0c\x{100}]+?)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]??([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]??([\x09-\x0c\x{100}]++)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]??([\x09-\x0c\x{100}]++)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]??([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]?+([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]?+([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]?+([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]?+([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]?+([\x09-\x0c\x{100}]+?)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]?+([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]?+([\x09-\x0c\x{100}]++)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]?+([\x09-\x0c\x{100}]++)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]?+([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]{0,1}([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]{0,1}([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]{0,1}([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]{0,1}([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]{0,1}([\x09-\x0c\x{100}]+?)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]{0,1}([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]{0,1}([\x09-\x0c\x{100}]++)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]{0,1}([\x09-\x0c\x{100}]++)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]{0,1}([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]{0,1}?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]{0,1}?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]{0,1}?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]{0,1}?([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]{0,1}?([\x09-\x0c\x{100}]+?)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]{0,1}?([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]{0,1}?([\x09-\x0c\x{100}]++)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]{0,1}?([\x09-\x0c\x{100}]++)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]{0,1}?([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]{0,1}+([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]{0,1}+([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]{0,1}+([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]{0,1}+([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]{0,1}+([\x09-\x0c\x{100}]+?)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]{0,1}+([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]{0,1}+([\x09-\x0c\x{100}]++)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kK]{0,1}+([\x09-\x0c\x{100}]++)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kK]{0,1}+([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kk]?[\x09-\x0c\x{100}]+A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?i)[kk]?[\x09-\x0c\x{100}]+A", "C4800D0A41", 0, "3:2" )]
	[InlineData( @"(?i)[kk]?[\x09-\x0c\x{100}]+A", "0B0D0A0B41", 0, "2:3" )]
	[InlineData( @"(?i)[kk]?(?:[\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?i)[kk]?(?:[\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2" )]
	[InlineData( @"(?i)[kk]?(?:[\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3" )]
	[InlineData( @"(?i)([kk]?([\x09-\x0c\x{100}]+))A", "0D0A41", 0, "1:2,1:1,1:1" )]
	[InlineData( @"(?i)([kk]?([\x09-\x0c\x{100}]+))A", "C4800D0A41", 0, "3:2,3:1,3:1" )]
	[InlineData( @"(?i)([kk]?([\x09-\x0c\x{100}]+))A", "0B0D0A0B41", 0, "2:3,2:2,2:2" )]
	[InlineData( @"(?i)[kk]?([\x09-\x0c\x{100}]+)A\1?", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kk]?([\x09-\x0c\x{100}]+)A\1?", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kk]?([\x09-\x0c\x{100}]+)A\1?", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kk]?([\x09-\x0c\x{100}]+)(A)\2?", "0D0A41", 0, "1:2,1:1,2:1" )]
	[InlineData( @"(?i)[kk]?([\x09-\x0c\x{100}]+)(A)\2?", "C4800D0A41", 0, "3:2,3:1,4:1" )]
	[InlineData( @"(?i)[kk]?([\x09-\x0c\x{100}]+)(A)\2?", "0B0D0A0B41", 0, "2:3,2:2,4:1" )]
	[InlineData( @"(?i)B?[kk]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)B?[kk]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)B?[kk]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kk]?B?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kk]?B?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kk]?B?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)B{0}[kk]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)B{0}[kk]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)B{0}[kk]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kk]?(?:B){0}([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kk]?(?:B){0}([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kk]?(?:B){0}([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)(?:B){0}[kk]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)(?:B){0}[kk]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)(?:B){0}[kk]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kk]?\1{0}([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kk]?\1{0}([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kk]?\1{0}([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kk]?(?:(?:(?:([\x09-\x0c\x{100}]+))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kk]?(?:(?:(?:([\x09-\x0c\x{100}]+))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)(?:(?:(?:[kk]?([\x09-\x0c\x{100}]+))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)(?:(?:(?:[kk]?([\x09-\x0c\x{100}]+))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kk]?(?:(?:(?:(?:([\x09-\x0c\x{100}]+)))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kk]?(?:(?:(?:(?:([\x09-\x0c\x{100}]+)))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)(?:(?:(?:(?:[kk]?([\x09-\x0c\x{100}]+)))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)(?:(?:(?:(?:[kk]?([\x09-\x0c\x{100}]+)))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kk]?(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kk]?(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:[kk]?([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:[kk]?([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kk]?(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[kk]?(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:[kk]?([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:[kk]?([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[kK]?([\x09-\x0c\x{100}]+)A", "6B0B41", 0, "0:3,1:1" )]
	[InlineData( @"(?i)[kK]?([\x09-\x0c\x{100}]+)A", "E284AA0B41", 0, "0:5,3:1" )]
	[InlineData( @"(?i)[sS]?([\x09-\x0c\x{100}]+)A", "C5BF0B41", 0, "0:4,2:1" )]
	[InlineData( @"(?i)[\x{3c3}\x{3c2}]?([\x09-\x0c\x{100}]+)A", "CF820B41", 0, "0:4,2:1" )]
	[InlineData( @"(?i)[\x{1c4}-\x{1c6}]?([\x09-\x0c\x{100}]+)A", "C7840B41", 0, "0:4,2:1" )]
	[InlineData( @"(?i)[bb]?([\x09-\x0c\x{100}]+)A", "420B41", 0, "0:3,1:1" )]
	[InlineData( @"(?i)[k]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"(?i)[k]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"(?i)[k]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[k-k]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"(?i)[k-k]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"(?i)[k-k]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[\Qk\E]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"(?i)[\Qk\E]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"(?i)[\Qk\E]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[\x{212a}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"(?i)[\x{212a}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"(?i)[\x{212a}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[\x09-\x0e]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "0:3,1:1" )]
	[InlineData( @"(?i)[\x09-\x0e]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "2:3,3:1" )]
	[InlineData( @"(?i)[\x09-\x0e]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "1:4,2:2" )]
	[InlineData( @"(?i)[^kk]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "0:3,1:1" )]
	[InlineData( @"(?i)[^kk]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "2:3,3:1" )]
	[InlineData( @"(?i)[^kk]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "1:4,2:2" )]
	[InlineData( @"(?i)[B\p{L}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"(?i)[B\p{L}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"(?i)[B\p{L}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[\n\n]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[\n\n]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[\n\n]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,3:1" )]
	[InlineData( @"(?i)([\x09-\x0c\x{100}]+)A[kk]?", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)([\x09-\x0c\x{100}]+)A[kk]?", "C4800D0A41", 0, "" )]
	[InlineData( @"(?i)([\x09-\x0c\x{100}]+)A[kk]?", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[kk]?([\x{100}-\x{10ffff}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"(?i)[kk]?([\x{100}-\x{10ffff}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"(?i)[kk]?([\x{100}-\x{10ffff}]+)A", "0B0D0A0B41", 0, "" )]
	[InlineData( @"(?i)[kk]?(\G[\x09-\x0c\x{100}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"(?i)[kk]?(\G[\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"(?i)[kk]?(\G[\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "" )]
	[InlineData( @"(?i)[kk]?(\G[\x09-\x0c\x{100}]+)A", "0D0A41", 1, "1:2,1:1" )]
	[InlineData( @"(?i)[kk]?", "0D0A41", 0, "0:0" )]
	public void SetSourceShapePrefixesPreservePinnedNativeCaptureSpans( string body, string hex, int offset, string spans ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		foreach ( var ucp in new[] { false, true } ) {
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
	}

	[Theory]
	[InlineData( "(*LF)" )]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CR)" )]
	[InlineData( "(*NUL)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void EveryCallerOffsetKeepsSetSourceShapePrefixAndFragmentBoundaries( string newline ) {
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
	public void StrictSetSourceShapePrefixesRetainUtfValidationAndCallerBoundaries( bool ucp ) {
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
	[InlineData( @"(?i)[kk]?([\x09-\x0c\x{100}]+)A", "0D0A41", 1 )]
	[InlineData( @"(?i)[kK]??([\x09-\x0c\x{100}]+)A", "C4800D0A41", 3 )]
	[InlineData( @"(?i)[kK]?+([\x09-\x0c\x{100}]+)A", "E284AA0B41", 0 )]
	[InlineData( @"(?i)(?:(?:(?:[kk]?([\x09-\x0c\x{100}]+))))A", "0B0D0A0B41", 2 )]
	public void SetSourceShapePrefixEntrySharesExactExhaustedAndObservedWork( string body, string hex, int index ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		var steps = new List<int>();
		var result = Compile( pattern, new() { Utf8 = true, WorkObserver = steps.Add } ).Match( subject );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index, result.Match?.ByteIndex ?? -1 );
		Compare( Reference( pattern ), subject, 0, result, pattern );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		foreach ( var observed in new[] { false, true } ) {
			Compare( Reference( pattern ), subject, 0, Compile( pattern, new() { Utf8 = true,
				MatchWorkLimit = steps.Count, WorkObserver = observed ? _ => { } : null } ).Match( subject ), pattern );
			var exhausted = Compile( pattern, new() { Utf8 = true, MatchWorkLimit = steps.Count - 1,
				WorkObserver = observed ? _ => { } : null } ).Match( subject );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
	}

	[Fact]
	public void SetSourceShapePrefixClassesWithoutLfRetainTheDefaultBudget() {
		var pattern = String.Concat( "(*ANYCRLF)", @"(?i)[kk]?([\x{100}-\x{10ffff}]+)A" );
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
	public void SetSourceShapePrefixEntryRetainsPreAndMidMatchCancellation() {
		var pattern = String.Concat( "(*ANYCRLF)", @"(?i)[kk]?([\x09-\x0c\x{100}]+)A\1?" );
		var subject = Convert.FromHexString( "C4800D0A41" );
		var steps = new List<int>();
		var result = Compile( pattern, new() { Utf8 = true, WorkObserver = steps.Add } ).Match( subject );
		Assert.Equal( 3, result.Match?.ByteIndex );
		Compare( Reference( pattern ), subject, 0, result, pattern );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new() { Utf8 = true,
			WorkObserver = count => { if ( count == steps.Count / 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void SetSourceShapePrefixEntryDoesNotRaiseTheParserDepthCeiling() {
		var pattern = String.Concat( "(?i)[kk]?", new string( '(', 200 ), String.Concat( Enumerable.Repeat( "(?:", 57 ) ),
			@"[\x09-\x0c\x{100}]+", new string( ')', 257 ), "A" );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new() { Utf8 = true } ).Diagnostic?.Code );
	}

	private static string[] Bodies() => new[] {
		@"(?i)[kk]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[kK]??([\x09-\x0c\x{100}]+)A",
		@"(?i)[kK]?+([\x09-\x0c\x{100}]+)A",
		@"(?i)[kK]{0,1}([\x09-\x0c\x{100}]+)A",
		@"(?i)[\Qkk\E]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[k-kk]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[k\x{212a}]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[sS]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[\x{3c3}\x{3c2}]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[\x{1c4}-\x{1c6}]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[bb]?([\x09-\x0c\x{100}]+)A",
		@"(?i)[kk]?([\x09-\x0c\x{100}]+?)A",
		@"(?i)[kk]?([\x09-\x0c\x{100}]+)A\1?",
		@"(?i)[k]?([\x09-\x0c\x{100}]+)A",
		@"(?i)(?:B){0}[kk]?([\x09-\x0c\x{100}]+)A",
		@"(?i)(?:(?:(?:[kk]?([\x09-\x0c\x{100}]+))))A"
	};
	private static string[] Subjects() => new[] { "", "00", "0D0A41", "0B0D0A41", "0B0D0A0B41", "C4800D0A41", "C4800D0A0B41", "FF0B0D0A41", "0BFF0D0A41", "0B0D0AFF41", "EDA0800B0D0A41", "6B0B41", "E284AA0B41", "CF820B41" };

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
