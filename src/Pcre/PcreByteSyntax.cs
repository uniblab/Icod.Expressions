namespace Icod.Expressions.Pcre;

internal abstract record PcreByteNode;

internal sealed record PcreByteLiteral( byte Value ) : PcreByteNode;

internal sealed record PcreByteClass( PcreByteSet Set ) : PcreByteNode;

internal sealed record PcreByteAnchor( bool IsStart ) : PcreByteNode;

internal sealed record PcreByteGroup( int Number, PcreByteAlternatives Body ) : PcreByteNode;

internal sealed record PcreByteRepeat( PcreByteNode Child, int Minimum, int Maximum, bool Greedy ) : PcreByteNode;

internal sealed record PcreByteAlternatives( IReadOnlyList<IReadOnlyList<PcreByteNode>> Branches );
