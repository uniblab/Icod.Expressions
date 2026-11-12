using System.Globalization;

namespace Icod.Expressions.Pcre;

internal abstract record PcreByteNode {
	internal bool Caseless { get; init; }
	internal bool CaselessRestricted { get; init; }
}

internal sealed record PcreByteLiteral( int Value ) : PcreByteNode;

internal sealed record PcreByteAny( bool DotAll ) : PcreByteNode;

internal sealed record PcreByteNewlineSequence : PcreByteNode;

internal sealed record PcreByteGrapheme : PcreByteNode;

internal sealed record PcreByteFailure( ReadOnlyMemory<byte> Name = default ) : PcreByteNode;

internal sealed record PcreByteAccept( ReadOnlyMemory<byte> Name = default ) : PcreByteNode;

internal sealed record PcreBytePrune( ReadOnlyMemory<byte> Name = default ) : PcreByteNode;

internal sealed record PcreByteSkip : PcreByteNode;

internal sealed record PcreByteNamedSkip( ReadOnlyMemory<byte> Name ) : PcreByteNode;

internal sealed record PcreByteCommit( ReadOnlyMemory<byte> Name = default ) : PcreByteNode;

internal sealed record PcreByteThen( ReadOnlyMemory<byte> Name = default ) : PcreByteNode;

internal sealed record PcreBytePathMark( ReadOnlyMemory<byte> Name ) : PcreByteNode;

internal enum PcreByteScalarClassKind {
	Letter, DecimalDigit, AsciiDigit, AsciiWord, UnicodeWord, AsciiSpace, UnicodeSpace,
	Ascii, AsciiAlnum, AsciiAlpha, AsciiBlank, AsciiControl, AsciiGraph,
	AsciiLower, AsciiPrintable, AsciiPunctuation, AsciiUpper, AsciiHexDigit,
	UnicodeAlnum, UnicodeBlank, UnicodeControl, UnicodeGraph, UnicodeLower,
	UnicodePrintable, UnicodePunctuation, UnicodeUpper, UnicodeHexDigit,
	CategoryOther, CasedLetter, CategoryMark, CategoryNumber, CategoryPunctuation,
	CategorySymbol, CategorySeparator, SpecificCategory, HorizontalSpace, VerticalSpace,
	AnyScalar, UniversalCharacterName, Script, ScriptExtensions, BinaryProperty, BidiClass
}

internal sealed record PcreByteScalarClass(
	PcreByteScalarClassKind Kind, bool Negated, UnicodeCategory? Category = null, int PropertyValue = 0
) : PcreByteNode;

internal readonly record struct PcreByteScalarRange( int Start, int End );

internal readonly record struct PcreByteScalarPredicate(
	PcreByteScalarClassKind Kind, bool Negated, UnicodeCategory? Category = null, int PropertyValue = 0
);

internal enum PcreByteScalarSetSource { Unknown, Literal, Class }

internal sealed record PcreByteScalarSet(
	ReadOnlyMemory<PcreByteScalarRange> Ranges,
	ReadOnlyMemory<PcreByteScalarPredicate> Predicates,
	bool Negated,
	bool ExplicitNewline
) : PcreByteNode {
	internal PcreByteScalarSetSource Source { get; init; }
	internal int SourceLiteral { get; init; } = -1;
}

internal enum PcreScalarSetOperation { Operand, Not, And, Or, Subtract, Xor }

internal readonly record struct PcreScalarSetTerm( PcreScalarSetOperation Operation, PcreByteScalarSet? Operand = null );

internal sealed record PcreByteScalarExpression(
	ReadOnlyMemory<PcreScalarSetTerm> Terms, int StackDepth, bool ExplicitNewline, int OpeningOffset
) : PcreByteNode {
	internal PcreByteSet ByteProjection { get; init; }
}

internal enum PcreByteClassPrefix { None, Atom, Exact, AnyProperty, Bitmap }

internal sealed record PcreByteClass(
	PcreByteSet Set, bool ExplicitNewline = false, PcreByteClassPrefix PrefixKind = PcreByteClassPrefix.None
) : PcreByteNode {
	internal bool PrefixAny => PrefixKind is PcreByteClassPrefix.Atom
		or PcreByteClassPrefix.Exact or PcreByteClassPrefix.AnyProperty;
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

internal sealed record PcreByteScriptRun( PcreByteNode Child, int OpeningOffset ) : PcreByteNode;

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
