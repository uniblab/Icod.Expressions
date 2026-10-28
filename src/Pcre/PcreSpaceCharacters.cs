namespace Icod.Expressions.Pcre;

/// <summary>PCRE2's fixed whitespace lists, independent of Unicode category tables and UCP.</summary>
internal static class PcreSpaceCharacters {
	internal static bool IsHorizontal( int scalar ) => scalar is
		0x09 or 0x20 or 0xA0 or 0x1680 or 0x180E
		or >= 0x2000 and <= 0x200A or 0x202F or 0x205F or 0x3000;

	internal static bool IsVertical( int scalar ) => scalar is
		>= 0x0A and <= 0x0D or 0x85 or 0x2028 or 0x2029;
}
