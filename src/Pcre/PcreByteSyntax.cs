using System.Globalization;

namespace Icod.Expressions.Pcre;

internal abstract record PcreByteNode {
	internal bool Caseless { get; init; }
	internal bool CaselessRestricted { get; init; }
}

internal sealed record PcreByteLiteral( int Value ) : PcreByteNode;

internal sealed record PcreByteAny( bool DotAll ) : PcreByteNode;

internal sealed record PcreByteNewlineSequence : PcreByteNode;

internal sealed record PcreByteFailure : PcreByteNode;

internal enum PcreByteScalarClassKind {
	Letter, DecimalDigit, AsciiDigit, AsciiWord, UnicodeWord, AsciiSpace, UnicodeSpace,
	Ascii, AsciiAlnum, AsciiAlpha, AsciiBlank, AsciiControl, AsciiGraph,
	AsciiLower, AsciiPrintable, AsciiPunctuation, AsciiUpper, AsciiHexDigit,
	UnicodeAlnum, UnicodeBlank, UnicodeControl, UnicodeGraph, UnicodeLower,
	UnicodePrintable, UnicodePunctuation, UnicodeUpper, UnicodeHexDigit,
	CategoryOther, CasedLetter, CategoryMark, CategoryNumber, CategoryPunctuation,
	CategorySymbol, CategorySeparator, SpecificCategory, HorizontalSpace, VerticalSpace,
	AnyScalar, UniversalCharacterName
}

internal sealed record PcreByteScalarClass(
	PcreByteScalarClassKind Kind, bool Negated, UnicodeCategory? Category = null
) : PcreByteNode;

internal readonly record struct PcreByteScalarRange( int Start, int End );

internal readonly record struct PcreByteScalarPredicate(
	PcreByteScalarClassKind Kind, bool Negated, UnicodeCategory? Category = null
);

internal sealed record PcreByteScalarSet(
	ReadOnlyMemory<PcreByteScalarRange> Ranges,
	ReadOnlyMemory<PcreByteScalarPredicate> Predicates,
	bool Negated,
	bool ExplicitNewline
) : PcreByteNode;

internal enum PcreByteClassPrefix { None, Atom, Exact, AnyProperty }

internal sealed record PcreByteClass(
	PcreByteSet Set, bool ExplicitNewline = false, PcreByteClassPrefix PrefixKind = PcreByteClassPrefix.None
) : PcreByteNode {
	internal bool PrefixAny => PrefixKind != PcreByteClassPrefix.None;
}

internal sealed record PcreByteBackreference(
	int Number, ReadOnlyMemory<int> DuplicateNumbers = default
) : PcreByteNode;

internal sealed record PcreByteSubroutineCall( int Number, int Offset, ReadOnlyMemory<int> ReturnedCaptures ) : PcreByteNode;

internal sealed record PcreByteAnchor( bool IsStart ) : PcreByteNode;

internal sealed record PcreByteSearchStart : PcreByteNode;

internal sealed record PcreByteMatchStartReset : PcreByteNode;

internal sealed record PcreByteLineAnchor( bool IsStart ) : PcreByteNode;

internal sealed record PcreByteEndBeforeFinalLf( bool FragmentEnd = false ) : PcreByteNode;

internal sealed record PcreByteWordBoundary( bool IsBoundary, bool UnicodeWord = false ) : PcreByteNode;

internal sealed record PcreByteGroup( int Number, PcreByteAlternatives Body ) : PcreByteNode;

internal sealed record PcreByteNoncapturingGroup( PcreByteAlternatives Body ) : PcreByteNode;

internal sealed record PcreByteCaptureConditional(
	ReadOnlyMemory<int> CaptureNumbers, PcreByteAlternatives Body
) : PcreByteNode;

internal sealed record PcreByteRecursionConditional(
	ReadOnlyMemory<int> GroupNumbers, PcreByteAlternatives Body
) : PcreByteNode;

internal sealed record PcreByteAssertionConditional(
	PcreByteNode Assertion, PcreByteAlternatives Body
) : PcreByteNode;

internal sealed record PcreByteStaticConditional(
	bool Value, PcreByteAlternatives Body, bool IsDefinition = false
) : PcreByteNode;

internal sealed record PcreByteAtomic( PcreByteNode Child ) : PcreByteNode;

internal sealed record PcreByteLiteralAssertion( ReadOnlyMemory<byte> Bytes, bool IsPositive, bool IsBehind ) : PcreByteNode;

internal sealed record PcreByteLookahead(
	PcreByteAlternatives Body, bool IsPositive, bool NonAtomic = false
) : PcreByteNode;

internal sealed record PcreByteLookbehind(
	PcreByteAlternatives Body, bool IsPositive, ReadOnlyMemory<int> BranchLengths, int Minimum = -1,
	int OpeningOffset = -1, bool NonAtomic = false
) : PcreByteNode;

internal sealed record PcreByteRepeat(
	PcreByteNode Child, int Minimum, int Maximum, bool Greedy, int Offset = -1, bool UnboundedAssertion = false
) : PcreByteNode;

internal sealed record PcreByteAlternatives( IReadOnlyList<IReadOnlyList<PcreByteNode>> Branches );
