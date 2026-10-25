namespace Icod.Expressions.Pcre;

internal abstract record PcreByteNode;

internal sealed record PcreByteLiteral( int Value ) : PcreByteNode;

internal sealed record PcreByteAny( bool DotAll ) : PcreByteNode;

internal enum PcreByteScalarClassKind {
	Letter, DecimalDigit, AsciiDigit, AsciiWord, UnicodeWord, AsciiSpace, UnicodeSpace
}

internal sealed record PcreByteScalarClass( PcreByteScalarClassKind Kind, bool Negated ) : PcreByteNode;

internal sealed record PcreByteClass( PcreByteSet Set ) : PcreByteNode;

internal sealed record PcreByteBackreference( int Number ) : PcreByteNode;

internal sealed record PcreByteAnchor( bool IsStart ) : PcreByteNode;

internal sealed record PcreByteLineAnchor( bool IsStart ) : PcreByteNode;

internal sealed record PcreByteEndBeforeFinalLf( bool FragmentEnd = false ) : PcreByteNode;

internal sealed record PcreByteWordBoundary( bool IsBoundary, bool UnicodeWord = false ) : PcreByteNode;

internal sealed record PcreByteGroup( int Number, PcreByteAlternatives Body ) : PcreByteNode;

internal sealed record PcreByteNoncapturingGroup( PcreByteAlternatives Body ) : PcreByteNode;

internal sealed record PcreByteLiteralAssertion( ReadOnlyMemory<byte> Bytes, bool IsPositive, bool IsBehind ) : PcreByteNode;

internal sealed record PcreByteRepeat( PcreByteNode Child, int Minimum, int Maximum, bool Greedy ) : PcreByteNode;

internal sealed record PcreByteAlternatives( IReadOnlyList<IReadOnlyList<PcreByteNode>> Branches );
