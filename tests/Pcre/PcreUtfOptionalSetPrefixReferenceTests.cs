using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Preserves native bitmap entry and input consumption through optional positive sets.</summary>
public sealed class PcreUtfOptionalSetPrefixReferenceTests {
	[Theory]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+)A", "420B41", 0, "0:3,1:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[B-C]??([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]??([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]??([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]??([\x09-\x0c\x{100}]+)A", "420B41", 0, "0:3,1:1" )]
	[InlineData( @"[B-C]??([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[B-C]?+([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?+([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]?+([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?+([\x09-\x0c\x{100}]+)A", "420B41", 0, "0:3,1:1" )]
	[InlineData( @"[B-C]?+([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[B-C]{0,1}([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]{0,1}([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]{0,1}([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]{0,1}([\x09-\x0c\x{100}]+)A", "420B41", 0, "0:3,1:1" )]
	[InlineData( @"[B-C]{0,1}([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[B-C]{0,1}?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]{0,1}?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]{0,1}?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]{0,1}?([\x09-\x0c\x{100}]+)A", "420B41", 0, "0:3,1:1" )]
	[InlineData( @"[B-C]{0,1}?([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[B-C]{0,1}+([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]{0,1}+([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]{0,1}+([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]{0,1}+([\x09-\x0c\x{100}]+)A", "420B41", 0, "0:3,1:1" )]
	[InlineData( @"[B-C]{0,1}+([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[\x09-\x0c]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[\x09-\x0c]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[\x09-\x0c]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,3:1" )]
	[InlineData( @"[\x09-\x0c]?([\x09-\x0c\x{100}]+)A", "420B41", 0, "1:2,1:1" )]
	[InlineData( @"[\x09-\x0c]?([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[\x09-\x0c]??([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[\x09-\x0c]??([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[\x09-\x0c]??([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[\x09-\x0c]??([\x09-\x0c\x{100}]+)A", "420B41", 0, "1:2,1:1" )]
	[InlineData( @"[\x09-\x0c]??([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[\x09-\x0c]?+([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]?+([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]?+([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,3:1" )]
	[InlineData( @"[\x09-\x0c]?+([\x09-\x0c\x{100}]+)A", "420B41", 0, "" )]
	[InlineData( @"[\x09-\x0c]?+([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c\x{100}]+)A", "420B41", 0, "0:3,1:1" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[\x{100}-\x{200}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[\x{100}-\x{200}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[\x{100}-\x{200}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[\x{100}-\x{200}]?([\x09-\x0c\x{100}]+)A", "420B41", 0, "1:2,1:1" )]
	[InlineData( @"[\x{100}-\x{200}]?([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[B-D\x{100}-\x{200}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-D\x{100}-\x{200}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-D\x{100}-\x{200}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-D\x{100}-\x{200}]?([\x09-\x0c\x{100}]+)A", "420B41", 0, "0:3,1:1" )]
	[InlineData( @"[B-D\x{100}-\x{200}]?([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[\x00-\x0c\x0e-\x{100}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[\x00-\x0c\x0e-\x{100}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[\x00-\x0c\x0e-\x{100}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,3:1" )]
	[InlineData( @"[\x00-\x0c\x0e-\x{100}]?([\x09-\x0c\x{100}]+)A", "420B41", 0, "0:3,1:1" )]
	[InlineData( @"[\x00-\x0c\x0e-\x{100}]?([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"(?i)[b-c]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[b-c]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?i)[b-c]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?i)[b-c]?([\x09-\x0c\x{100}]+)A", "420B41", 0, "0:3,1:1" )]
	[InlineData( @"(?i)[b-c]?([\x09-\x0c\x{100}]+)A", "420D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+?)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]++)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]++)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c\x{100}]+?)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c\x{100}]++)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c\x{100}]++)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[\x09-\x0c]?+([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]?+([\x09-\x0c\x{100}]+?)A", "C4800D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]?+([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, "2:3,3:1" )]
	[InlineData( @"[\x09-\x0c]?+([\x09-\x0c\x{100}]++)A", "0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]?+([\x09-\x0c\x{100}]++)A", "C4800D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]?+([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, "2:3,3:1" )]
	[InlineData( @"[B-C]?[\x09-\x0c\x{100}]+A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[B-C]?[\x09-\x0c\x{100}]+A", "C4800D0A41", 0, "3:2" )]
	[InlineData( @"[B-C]?[\x09-\x0c\x{100}]+A", "0B0D0A0B41", 0, "2:3" )]
	[InlineData( @"[B-C]?(?:[\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[B-C]?(?:[\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2" )]
	[InlineData( @"[B-C]?(?:[\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3" )]
	[InlineData( @"([B-C]?([\x09-\x0c\x{100}]+))A", "0D0A41", 0, "1:2,1:1,1:1" )]
	[InlineData( @"([B-C]?([\x09-\x0c\x{100}]+))A", "C4800D0A41", 0, "3:2,3:1,3:1" )]
	[InlineData( @"([B-C]?([\x09-\x0c\x{100}]+))A", "0B0D0A0B41", 0, "2:3,2:2,2:2" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+)A\1?", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+)A\1?", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+)A\1?", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+)(A)\2?", "0D0A41", 0, "1:2,1:1,2:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+)(A)\2?", "C4800D0A41", 0, "3:2,3:1,4:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+)(A)\2?", "0B0D0A0B41", 0, "2:3,2:2,4:1" )]
	[InlineData( @"B?[C-D]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"B?[C-D]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"B?[C-D]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?D?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?D?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]?D?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"B{0}[C-D]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"B{0}[C-D]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"B{0}[C-D]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?(?:D){0}([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?(?:D){0}([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]?(?:D){0}([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?:B){0}[C-D]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?:B){0}[C-D]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"(?:B){0}[C-D]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?\1{0}([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?\1{0}([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]?\1{0}([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?(?:(?:(?:([\x09-\x0c\x{100}]+))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?(?:(?:(?:([\x09-\x0c\x{100}]+))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?:(?:(?:[B-C]?([\x09-\x0c\x{100}]+))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?:(?:(?:[B-C]?([\x09-\x0c\x{100}]+))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?(?:(?:(?:(?:([\x09-\x0c\x{100}]+)))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?(?:(?:(?:(?:([\x09-\x0c\x{100}]+)))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?:(?:(?:(?:[B-C]?([\x09-\x0c\x{100}]+)))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?:(?:(?:(?:[B-C]?([\x09-\x0c\x{100}]+)))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:[B-C]?([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:[B-C]?([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:[B-C]?([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:[B-C]?([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?([\x09-\x0c]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c]+)A", "0B0D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[B-C]?([\x09-\x0c]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]??([\x09-\x0c]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]??([\x09-\x0c]+)A", "0B0D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[B-C]??([\x09-\x0c]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B-C]?+([\x09-\x0c]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B-C]?+([\x09-\x0c]+)A", "0B0D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[B-C]?+([\x09-\x0c]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c]+)A", "0B0D0A41", 0, "2:2,2:1" )]
	[InlineData( @"[B\x{100}]?([\x09-\x0c]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"[\x09-\x0e]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "0:3,1:1" )]
	[InlineData( @"[\x09-\x0e]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "2:3,3:1" )]
	[InlineData( @"[\x09-\x0e]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "1:4,2:2" )]
	[InlineData( @"[^B]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "0:3,1:1" )]
	[InlineData( @"[^B]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "2:3,3:1" )]
	[InlineData( @"[^B]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "1:4,2:2" )]
	[InlineData( @"[\n]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[\n]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"[\n]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,3:1" )]
	[InlineData( @"[\r]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "0:3,1:1" )]
	[InlineData( @"[\r]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "2:3,3:1" )]
	[InlineData( @"[\r]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "1:4,2:2" )]
	[InlineData( @"([\x09-\x0c\x{100}]+)A[B-C]?", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"([\x09-\x0c\x{100}]+)A[B-C]?", "C4800D0A41", 0, "" )]
	[InlineData( @"([\x09-\x0c\x{100}]+)A[B-C]?", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"[B-C]?([\x{100}-\x{10ffff}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"[B-C]?([\x{100}-\x{10ffff}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"[B-C]?([\x{100}-\x{10ffff}]+)A", "0B0D0A0B41", 0, "" )]
	[InlineData( @"[B-C]?(\G[\x09-\x0c\x{100}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"[B-C]?(\G[\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"[B-C]?(\G[\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "" )]
	[InlineData( @"[B-C]?(\G[\x09-\x0c\x{100}]+)A", "0D0A41", 1, "1:2,1:1" )]
	[InlineData( @"[B-C]?", "0D0A41", 0, "0:0" )]
	[InlineData( @"[B-C]{0,1}+", "4241", 0, "0:1" )]
	[InlineData( @"[B\p{L}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"[B\p{L}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"[B\p{L}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"[B\P{L}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "0:3,1:1" )]
	[InlineData( @"[B\P{L}]?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "2:3,3:1" )]
	[InlineData( @"[B\P{L}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "1:4,2:2" )]
	[InlineData( @"[\x{2000}\x{2002}\x{2004}\x{2006}\x{2008}\x{200a}\x{200c}\x{200e}\x{2010}\x{2012}\x{2014}\x{2016}\x{2018}\x{201a}\x{201c}\x{201e}\x{2020}\x{2022}\x{2024}\x{2026}\x{2028}\x{202a}\x{202c}\x{202e}\x{2030}\x{2032}\x{2034}\x{2036}\x{2038}\x{203a}\x{203c}\x{203e}\x{2040}\x{2042}\x{2044}\x{2046}\x{2048}\x{204a}\x{204c}\x{204e}\x{2050}\x{2052}\x{2054}\x{2056}\x{2058}\x{205a}\x{205c}\x{205e}\x{2060}\x{2062}\x{2064}\x{2066}\x{2068}\x{206a}\x{206c}\x{206e}\x{2070}\x{2072}\x{2074}\x{2076}\x{2078}\x{207a}\x{207c}\x{207e}\x{2080}\x{2082}\x{2084}\x{2086}\x{2088}\x{208a}\x{208c}\x{208e}\x{2090}\x{2092}\x{2094}\x{2096}\x{2098}\x{209a}\x{209c}\x{209e}\x{20a0}\x{20a2}\x{20a4}\x{20a6}\x{20a8}\x{20aa}\x{20ac}\x{20ae}\x{20b0}\x{20b2}\x{20b4}\x{20b6}\x{20b8}\x{20ba}\x{20bc}\x{20be}\x{20c0}\x{20c2}\x{20c4}\x{20c6}\x{20c8}\x{20ca}\x{20cc}\x{20ce}\x{20d0}\x{20d2}\x{20d4}\x{20d6}\x{20d8}\x{20da}\x{20dc}\x{20de}\x{20e0}\x{20e2}\x{20e4}\x{20e6}\x{20e8}\x{20ea}\x{20ec}\x{20ee}\x{20f0}\x{20f2}\x{20f4}\x{20f6}\x{20f8}\x{20fa}\x{20fc}\x{20fe}\x{2100}\x{2102}\x{2104}\x{2106}\x{2108}\x{210a}\x{210c}\x{210e}\x{2110}\x{2112}\x{2114}\x{2116}\x{2118}\x{211a}\x{211c}\x{211e}\x{2120}\x{2122}\x{2124}\x{2126}\x{2128}\x{212a}\x{212c}\x{212e}\x{2130}\x{2132}\x{2134}\x{2136}\x{2138}\x{213a}\x{213c}\x{213e}\x{2140}\x{2142}\x{2144}\x{2146}\x{2148}\x{214a}\x{214c}\x{214e}\x{2150}\x{2152}\x{2154}\x{2156}\x{2158}\x{215a}\x{215c}\x{215e}\x{2160}\x{2162}\x{2164}\x{2166}\x{2168}\x{216a}\x{216c}\x{216e}\x{2170}\x{2172}\x{2174}\x{2176}\x{2178}\x{217a}\x{217c}\x{217e}\x{2180}\x{2182}\x{2184}\x{2186}\x{2188}\x{218a}\x{218c}\x{218e}\x{2190}\x{2192}\x{2194}\x{2196}\x{2198}\x{219a}\x{219c}\x{219e}\x{21a0}\x{21a2}\x{21a4}\x{21a6}\x{21a8}\x{21aa}\x{21ac}\x{21ae}\x{21b0}\x{21b2}\x{21b4}\x{21b6}\x{21b8}\x{21ba}\x{21bc}\x{21be}\x{21c0}\x{21c2}\x{21c4}\x{21c6}\x{21c8}\x{21ca}\x{21cc}\x{21ce}\x{21d0}\x{21d2}\x{21d4}\x{21d6}\x{21d8}\x{21da}\x{21dc}\x{21de}\x{21e0}\x{21e2}\x{21e4}\x{21e6}\x{21e8}\x{21ea}\x{21ec}\x{21ee}\x{21f0}\x{21f2}\x{21f4}\x{21f6}\x{21f8}\x{21fa}\x{21fc}\x{21fe}\x{2200}\x{2202}\x{2204}\x{2206}\x{2208}\x{220a}\x{220c}\x{220e}\x{2210}\x{2212}\x{2214}\x{2216}\x{2218}\x{221a}\x{221c}\x{221e}\x{2220}\x{2222}\x{2224}\x{2226}\x{2228}\x{222a}\x{222c}\x{222e}\x{2230}\x{2232}\x{2234}\x{2236}\x{2238}\x{223a}\x{223c}\x{223e}\x{2240}\x{2242}\x{2244}\x{2246}\x{2248}\x{224a}\x{224c}\x{224e}\x{2250}\x{2252}\x{2254}\x{2256}]?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[\x{2000}\x{2002}\x{2004}\x{2006}\x{2008}\x{200a}\x{200c}\x{200e}\x{2010}\x{2012}\x{2014}\x{2016}\x{2018}\x{201a}\x{201c}\x{201e}\x{2020}\x{2022}\x{2024}\x{2026}\x{2028}\x{202a}\x{202c}\x{202e}\x{2030}\x{2032}\x{2034}\x{2036}\x{2038}\x{203a}\x{203c}\x{203e}\x{2040}\x{2042}\x{2044}\x{2046}\x{2048}\x{204a}\x{204c}\x{204e}\x{2050}\x{2052}\x{2054}\x{2056}\x{2058}\x{205a}\x{205c}\x{205e}\x{2060}\x{2062}\x{2064}\x{2066}\x{2068}\x{206a}\x{206c}\x{206e}\x{2070}\x{2072}\x{2074}\x{2076}\x{2078}\x{207a}\x{207c}\x{207e}\x{2080}\x{2082}\x{2084}\x{2086}\x{2088}\x{208a}\x{208c}\x{208e}\x{2090}\x{2092}\x{2094}\x{2096}\x{2098}\x{209a}\x{209c}\x{209e}\x{20a0}\x{20a2}\x{20a4}\x{20a6}\x{20a8}\x{20aa}\x{20ac}\x{20ae}\x{20b0}\x{20b2}\x{20b4}\x{20b6}\x{20b8}\x{20ba}\x{20bc}\x{20be}\x{20c0}\x{20c2}\x{20c4}\x{20c6}\x{20c8}\x{20ca}\x{20cc}\x{20ce}\x{20d0}\x{20d2}\x{20d4}\x{20d6}\x{20d8}\x{20da}\x{20dc}\x{20de}\x{20e0}\x{20e2}\x{20e4}\x{20e6}\x{20e8}\x{20ea}\x{20ec}\x{20ee}\x{20f0}\x{20f2}\x{20f4}\x{20f6}\x{20f8}\x{20fa}\x{20fc}\x{20fe}\x{2100}\x{2102}\x{2104}\x{2106}\x{2108}\x{210a}\x{210c}\x{210e}\x{2110}\x{2112}\x{2114}\x{2116}\x{2118}\x{211a}\x{211c}\x{211e}\x{2120}\x{2122}\x{2124}\x{2126}\x{2128}\x{212a}\x{212c}\x{212e}\x{2130}\x{2132}\x{2134}\x{2136}\x{2138}\x{213a}\x{213c}\x{213e}\x{2140}\x{2142}\x{2144}\x{2146}\x{2148}\x{214a}\x{214c}\x{214e}\x{2150}\x{2152}\x{2154}\x{2156}\x{2158}\x{215a}\x{215c}\x{215e}\x{2160}\x{2162}\x{2164}\x{2166}\x{2168}\x{216a}\x{216c}\x{216e}\x{2170}\x{2172}\x{2174}\x{2176}\x{2178}\x{217a}\x{217c}\x{217e}\x{2180}\x{2182}\x{2184}\x{2186}\x{2188}\x{218a}\x{218c}\x{218e}\x{2190}\x{2192}\x{2194}\x{2196}\x{2198}\x{219a}\x{219c}\x{219e}\x{21a0}\x{21a2}\x{21a4}\x{21a6}\x{21a8}\x{21aa}\x{21ac}\x{21ae}\x{21b0}\x{21b2}\x{21b4}\x{21b6}\x{21b8}\x{21ba}\x{21bc}\x{21be}\x{21c0}\x{21c2}\x{21c4}\x{21c6}\x{21c8}\x{21ca}\x{21cc}\x{21ce}\x{21d0}\x{21d2}\x{21d4}\x{21d6}\x{21d8}\x{21da}\x{21dc}\x{21de}\x{21e0}\x{21e2}\x{21e4}\x{21e6}\x{21e8}\x{21ea}\x{21ec}\x{21ee}\x{21f0}\x{21f2}\x{21f4}\x{21f6}\x{21f8}\x{21fa}\x{21fc}\x{21fe}\x{2200}\x{2202}\x{2204}\x{2206}\x{2208}\x{220a}\x{220c}\x{220e}\x{2210}\x{2212}\x{2214}\x{2216}\x{2218}\x{221a}\x{221c}\x{221e}\x{2220}\x{2222}\x{2224}\x{2226}\x{2228}\x{222a}\x{222c}\x{222e}\x{2230}\x{2232}\x{2234}\x{2236}\x{2238}\x{223a}\x{223c}\x{223e}\x{2240}\x{2242}\x{2244}\x{2246}\x{2248}\x{224a}\x{224c}\x{224e}\x{2250}\x{2252}\x{2254}\x{2256}]?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "2:3,2:2" )]
	public void OptionalSetPrefixesPreservePinnedNativeCaptureSpans( string body, string hex, int offset, string spans ) {
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
	public void EveryCallerOffsetKeepsOptionalSetPrefixAndFragmentBoundaries( string newline ) {
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
	public void StrictOptionalSetPrefixesRetainUtfValidationAndCallerBoundaries( bool ucp ) {
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
	[InlineData( @"[B-C]?([\x09-\x0c\x{100}]+)A", "0D0A41", 1 )]
	[InlineData( @"[B-C]??([\x09-\x0c\x{100}]+)A", "C4800D0A41", 3 )]
	[InlineData( @"[\x09-\x0c]?+([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 2 )]
	[InlineData( @"(?:(?:(?:[B-C]?([\x09-\x0c\x{100}]+))))A", "0B0D0A0B41", 2 )]
	public void OptionalSetPrefixEntrySharesExactExhaustedAndObservedWork( string body, string hex, int index ) {
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
	public void OptionalSetPrefixClassesWithoutLfRetainTheDefaultBudget() {
		var pattern = String.Concat( "(*ANYCRLF)", @"[B-C]?([\x{100}-\x{10ffff}]+)A" );
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
	public void OptionalSetPrefixEntryRetainsPreAndMidMatchCancellation() {
		var pattern = String.Concat( "(*ANYCRLF)", @"[B-C]?([\x09-\x0c\x{100}]+)A\1?" );
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
	public void OptionalSetPrefixEntryDoesNotRaiseTheParserDepthCeiling() {
		var pattern = String.Concat( "[B-C]?", new string( '(', 200 ), String.Concat( Enumerable.Repeat( "(?:", 57 ) ),
			@"[\x09-\x0c\x{100}]+", new string( ')', 257 ), "A" );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new() { Utf8 = true } ).Diagnostic?.Code );
	}

	private static string[] Bodies() => new[] {
		@"[B-C]?([\x09-\x0c\x{100}]+)A",
		@"[B-C]??([\x09-\x0c\x{100}]+)A",
		@"[B-C]?+([\x09-\x0c\x{100}]+)A",
		@"[B-C]{0,1}([\x09-\x0c\x{100}]+)A",
		@"[B\x{100}]?([\x09-\x0c\x{100}]+)A",
		@"[\x{100}-\x{200}]?([\x09-\x0c\x{100}]+)A",
		@"[\x09-\x0c]?([\x09-\x0c\x{100}]+)A",
		@"[\x09-\x0c]?+([\x09-\x0c\x{100}]+)A",
		@"(?i)[b-c]?([\x09-\x0c\x{100}]+)A",
		@"[B-C]?([\x09-\x0c]+)A",
		@"[B-C]?([\x09-\x0c\x{100}]+?)A",
		@"[B-C]?([\x09-\x0c\x{100}]++)A",
		@"[B-C]?([\x09-\x0c\x{100}]+)A\1?",
		@"[\x09-\x0e]?([\x09-\x0c\x{100}]+)A",
		@"(?:B){0}[C-D]?([\x09-\x0c\x{100}]+)A",
		@"(?:(?:(?:[B-C]?([\x09-\x0c\x{100}]+))))A"
	};
	private static string[] Subjects() => new[] { "", "00", "0D0A41", "0B0D0A41", "0B0D0A0B41", "C4800D0A41", "C4800D0A0B41", "C4800B0D0A41", "FF0B0D0A41", "0BFF0D0A41", "0B0D0AFF41", "EDA0800B0D0A41", "420B41", "42430B41" };

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
