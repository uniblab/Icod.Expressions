using System.Globalization;
using System.Text;

namespace Icod.Expressions.Pcre;

/// <summary>Shared membership over decoded scalars and same-valued byte code points.</summary>
internal static class PcreScalarClassMembership {
	internal static bool Contains( PcreByteScalarClassKind kind, int scalar,
		UnicodeCategory? specificCategory = null, bool caseless = false ) {
		if ( kind == PcreByteScalarClassKind.AnyScalar ) { return true; } // Decoder admits valid scalars only.
		if ( kind == PcreByteScalarClassKind.UniversalCharacterName ) { return scalar is '$' or '@' or '`' or >= 0xA0; }
		if ( kind == PcreByteScalarClassKind.Letter && scalar <= 0x7F ) {
			return scalar is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
		}
		if ( kind == PcreByteScalarClassKind.AsciiDigit ) { return scalar is >= '0' and <= '9'; }
		if ( kind == PcreByteScalarClassKind.Ascii ) { return scalar <= 0x7F; }
		if ( caseless && ( kind is PcreByteScalarClassKind.AsciiLower or PcreByteScalarClassKind.AsciiUpper ) ) {
			return scalar <= 0x7F && PcreByteSet.Alpha.Contains( (byte)scalar );
		}
		if ( kind is PcreByteScalarClassKind.AsciiAlnum or PcreByteScalarClassKind.AsciiAlpha
			or PcreByteScalarClassKind.AsciiBlank or PcreByteScalarClassKind.AsciiControl
			or PcreByteScalarClassKind.AsciiGraph or PcreByteScalarClassKind.AsciiLower
			or PcreByteScalarClassKind.AsciiPrintable or PcreByteScalarClassKind.AsciiPunctuation
			or PcreByteScalarClassKind.AsciiUpper or PcreByteScalarClassKind.AsciiHexDigit ) {
			if ( scalar > 0x7F ) { return false; }
			var value = (byte)scalar;
			return kind switch {
				PcreByteScalarClassKind.AsciiAlnum => PcreByteSet.Alnum.Contains( value ),
				PcreByteScalarClassKind.AsciiAlpha => PcreByteSet.Alpha.Contains( value ),
				PcreByteScalarClassKind.AsciiBlank => PcreByteSet.Blank.Contains( value ),
				PcreByteScalarClassKind.AsciiControl => PcreByteSet.Control.Contains( value ),
				PcreByteScalarClassKind.AsciiGraph => PcreByteSet.Graph.Contains( value ),
				PcreByteScalarClassKind.AsciiLower => PcreByteSet.Lower.Contains( value ),
				PcreByteScalarClassKind.AsciiPrintable => PcreByteSet.Printable.Contains( value ),
				PcreByteScalarClassKind.AsciiPunctuation => PcreByteSet.Punctuation.Contains( value ),
				PcreByteScalarClassKind.AsciiUpper => PcreByteSet.Upper.Contains( value ),
				PcreByteScalarClassKind.AsciiHexDigit => PcreByteSet.HexDigit.Contains( value ),
				_ => throw new InvalidOperationException( "Compiler produced an unknown PCRE ASCII class." )
			};
		}
		if ( kind == PcreByteScalarClassKind.AsciiWord ) {
			return scalar <= 0x7F && PcreByteSet.Word.Contains( (byte)scalar );
		}
		if ( kind == PcreByteScalarClassKind.AsciiSpace ) {
			return scalar <= 0x7F && PcreByteSet.WhiteSpace.Contains( (byte)scalar );
		}
		if ( kind == PcreByteScalarClassKind.UnicodeSpace
			&& scalar is >= 0x09 and <= 0x0D or 0x85 or 0x180E ) {
			return true;
		}
		if ( kind is PcreByteScalarClassKind.UnicodeBlank or PcreByteScalarClassKind.HorizontalSpace ) {
			return PcreSpaceCharacters.IsHorizontal( scalar );
		}
		if ( kind == PcreByteScalarClassKind.VerticalSpace ) {
			return PcreSpaceCharacters.IsVertical( scalar );
		}
		if ( kind == PcreByteScalarClassKind.UnicodeHexDigit ) {
			return scalar is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f'
				or >= 0xFF10 and <= 0xFF19 or >= 0xFF21 and <= 0xFF26 or >= 0xFF41 and <= 0xFF46;
		}
		var category = Rune.GetUnicodeCategory( new Rune( scalar ) );
		return kind switch {
			PcreByteScalarClassKind.SpecificCategory => specificCategory.HasValue
				? caseless && IsCasedCategory( specificCategory.Value )
						? IsCasedCategory( category )
						: category == specificCategory.Value
				: throw new InvalidOperationException( "Compiler omitted a specific Unicode category." ),
			PcreByteScalarClassKind.CategoryOther => category is
				UnicodeCategory.Control or UnicodeCategory.Format or UnicodeCategory.OtherNotAssigned
				or UnicodeCategory.PrivateUse or UnicodeCategory.Surrogate,
			PcreByteScalarClassKind.CasedLetter => category is
				UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
				or UnicodeCategory.TitlecaseLetter,
			PcreByteScalarClassKind.CategoryMark => category is
				UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark
				or UnicodeCategory.EnclosingMark,
			PcreByteScalarClassKind.CategoryNumber => category is
				UnicodeCategory.DecimalDigitNumber or UnicodeCategory.LetterNumber
				or UnicodeCategory.OtherNumber,
			PcreByteScalarClassKind.CategoryPunctuation => category is
				UnicodeCategory.ConnectorPunctuation or UnicodeCategory.DashPunctuation
				or UnicodeCategory.OpenPunctuation or UnicodeCategory.ClosePunctuation
				or UnicodeCategory.InitialQuotePunctuation or UnicodeCategory.FinalQuotePunctuation
				or UnicodeCategory.OtherPunctuation,
			PcreByteScalarClassKind.CategorySymbol => category is
				UnicodeCategory.MathSymbol or UnicodeCategory.CurrencySymbol
				or UnicodeCategory.ModifierSymbol or UnicodeCategory.OtherSymbol,
			PcreByteScalarClassKind.CategorySeparator => category is
				UnicodeCategory.SpaceSeparator or UnicodeCategory.LineSeparator
				or UnicodeCategory.ParagraphSeparator,
			PcreByteScalarClassKind.DecimalDigit => category == UnicodeCategory.DecimalDigitNumber,
			PcreByteScalarClassKind.UnicodeAlnum => category is
				UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
				or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter
				or UnicodeCategory.OtherLetter or UnicodeCategory.DecimalDigitNumber
				or UnicodeCategory.LetterNumber or UnicodeCategory.OtherNumber,
			PcreByteScalarClassKind.UnicodeControl => category == UnicodeCategory.Control,
			PcreByteScalarClassKind.UnicodeGraph => UnicodeGraphContains( category, scalar ),
			PcreByteScalarClassKind.UnicodeLower => category == UnicodeCategory.LowercaseLetter,
			PcreByteScalarClassKind.UnicodePrintable => category == UnicodeCategory.SpaceSeparator
				|| UnicodeGraphContains( category, scalar ),
			PcreByteScalarClassKind.UnicodePunctuation => category is
				UnicodeCategory.ConnectorPunctuation or UnicodeCategory.DashPunctuation
				or UnicodeCategory.OpenPunctuation or UnicodeCategory.ClosePunctuation
				or UnicodeCategory.InitialQuotePunctuation or UnicodeCategory.FinalQuotePunctuation
				or UnicodeCategory.OtherPunctuation || ( scalar < 0x80 && category is
				UnicodeCategory.MathSymbol or UnicodeCategory.CurrencySymbol
				or UnicodeCategory.ModifierSymbol or UnicodeCategory.OtherSymbol ),
			PcreByteScalarClassKind.UnicodeUpper => category == UnicodeCategory.UppercaseLetter,
			PcreByteScalarClassKind.Letter => category is
				UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
				or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter
				or UnicodeCategory.OtherLetter,
			PcreByteScalarClassKind.UnicodeWord => category is
				UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
				or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter
				or UnicodeCategory.OtherLetter or UnicodeCategory.DecimalDigitNumber
				or UnicodeCategory.LetterNumber or UnicodeCategory.OtherNumber
				or UnicodeCategory.NonSpacingMark or UnicodeCategory.ConnectorPunctuation,
			PcreByteScalarClassKind.UnicodeSpace => category is
				UnicodeCategory.SpaceSeparator or UnicodeCategory.LineSeparator
				or UnicodeCategory.ParagraphSeparator,
			_ => throw new InvalidOperationException( "Compiler produced an unknown PCRE scalar class." )
		};
	}

	private static bool IsCasedCategory( UnicodeCategory category ) => category is
		UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or UnicodeCategory.TitlecaseLetter;

	private static bool UnicodeGraphContains( UnicodeCategory category, int scalar ) {
		if ( scalar is 0x061C or 0x180E or >= 0x2066 and <= 0x2069 ) { return false; }
		return category is
			UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
			or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter
			or UnicodeCategory.OtherLetter or UnicodeCategory.DecimalDigitNumber
			or UnicodeCategory.LetterNumber or UnicodeCategory.OtherNumber
			or UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark
			or UnicodeCategory.EnclosingMark or UnicodeCategory.ConnectorPunctuation
			or UnicodeCategory.DashPunctuation or UnicodeCategory.OpenPunctuation
			or UnicodeCategory.ClosePunctuation or UnicodeCategory.InitialQuotePunctuation
			or UnicodeCategory.FinalQuotePunctuation or UnicodeCategory.OtherPunctuation
			or UnicodeCategory.MathSymbol or UnicodeCategory.CurrencySymbol
			or UnicodeCategory.ModifierSymbol or UnicodeCategory.OtherSymbol
			or UnicodeCategory.Format;
	}

}
