namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private sealed class UtsClassFrame( bool negated ) {
		internal bool Negated { get; } = negated;
		internal bool First = true;
		internal bool Quoting;
		internal bool Extended;
		internal bool HasSequence;
		internal byte Operator;
		internal bool PendingOperator;
		internal List<PcreByteScalarRange> Ranges { get; } = new();
		internal List<PcreByteScalarPredicate> Predicates { get; } = new();
		internal int Items;
		internal bool SoleLiteral;
		internal bool ExplicitNewline;
	}

	// Contiguous raw items form one ordinary leaf. A nested bracket breaks that
	// run; implicit unions are emitted before the containing explicit operation.
	private PcreByteNode ParseUtsClass( int openingOffset ) {
		var savedAlgebra = classAlgebra;
		classAlgebra = true;
		try {
			var terms = new List<PcreScalarSetTerm>();
			var frames = new Stack<UtsClassFrame>();
			frames.Push( new( ParseClassNegation() ) );
			while ( index < source.Length ) {
				cancellationToken.ThrowIfCancellationRequested();
				var frame = frames.Peek();
				SkipClassTrivia( ref frame.Quoting );
				if ( index >= source.Length ) { break; }
				var value = source[index];
				if ( !frame.Quoting && value == (byte)']' && !frame.First ) {
					if ( frames.Count == 1 && !frame.Extended ) {
						// The existing readers own ordinary source/optimizer metadata.
						classAlgebra = savedAlgebra;
						index = openingOffset + 1;
						return utf8 ? ParseUtf8Bracket( openingOffset ) : ParseBracket( openingOffset );
					}
					FlushUtsRun( frame, terms, openingOffset, !frame.Extended && frame.Negated );
					if ( !frame.HasSequence ) { throw Invalid( openingOffset, "extended class has a trailing operator" ); }
					if ( frame.PendingOperator ) { AddUtsTerm( terms, new( UtsOperation( frame.Operator ) ), openingOffset ); }
					if ( frame.Extended && frame.Negated ) { AddUtsTerm( terms, new( PcreScalarSetOperation.Not ), openingOffset ); }
					index++; frames.Pop();
					if ( frames.Count == 0 ) { return FinishClassExpression( terms, openingOffset ); }
					JoinUtsSequence( frames.Peek(), terms, openingOffset );
					continue;
				}
				if ( !frame.Quoting && IsUtsOperator( index ) ) {
					if ( index + 2 < source.Length && source[index + 2] == value ) {
						throw Invalid( openingOffset, "extended class operators have exactly two characters" );
					}
					FlushUtsRun( frame, terms, openingOffset );
					if ( !frame.HasSequence || frame.Operator != 0 && frame.Operator != value ) {
						throw Invalid( openingOffset, "extended class requires operands and one operator kind per level" );
					}
					if ( frame.PendingOperator ) { AddUtsTerm( terms, new( UtsOperation( frame.Operator ) ), openingOffset ); }
					frame.Operator = value; frame.PendingOperator = true;
					frame.Extended = true; frame.HasSequence = false;
					index += 2;
					continue;
				}
				if ( !frame.Quoting && IsUtsNestedStart( index ) ) {
					CheckClassNesting( frames.Count + 1, openingOffset );
					FlushUtsRun( frame, terms, openingOffset );
					frame.Extended = true; frame.First = false;
					index++;
					frames.Push( new( ParseClassNegation() ) );
					continue;
				}
				var first = frame.Quoting ? ParseQuotedLiteral() : ParseUtf8BracketItem( openingOffset );
				frame.First = false;
				SkipClassTrivia( ref frame.Quoting );
				frame.SoleLiteral = first is PcreByteLiteral;
				if ( first is PcreByteLiteral { Value: '\r' or '\n' } ) { frame.ExplicitNewline = true; }
				if ( TryStartUtsRange( ref frame.Quoting ) ) {
					if ( first is not PcreByteLiteral start || index >= source.Length ) {
						throw Invalid( openingOffset, "range requires literal endpoints" );
					}
					var last = frame.Quoting ? ParseQuotedLiteral() : ParseUtf8BracketItem( openingOffset );
					if ( last is not PcreByteLiteral end || end.Value < start.Value ) {
						throw Invalid( openingOffset, "range requires ordered literal endpoints" );
					}
					frame.Ranges.Add( new( start.Value, end.Value ) );
					frame.SoleLiteral = start.Value == end.Value;
					if ( end.Value is '\r' or '\n' || !utf8 && !frame.Negated
						&& ( start.Value <= '\n' && end.Value >= '\n' || start.Value <= '\r' && end.Value >= '\r' ) ) {
						frame.ExplicitNewline = true;
					}
				} else if ( first is PcreByteLiteral literal ) { frame.Ranges.Add( new( literal.Value, literal.Value ) ); }
				else if ( first is PcreByteScalarClass property ) {
					frame.Predicates.Add( new( property.Kind, property.Negated, property.Category, property.PropertyValue ) );
				} else { throw Invalid( openingOffset, "invalid extended class member" ); }
				frame.Items++;
			}
			throw Invalid( openingOffset, "unclosed extended class" );
		} catch ( PcreByteParseException error ) when ( error.Diagnostic.Code == PcreByteDiagnosticCode.InvalidSyntax ) {
			throw Invalid( openingOffset, "invalid UTS extended character class" );
		} finally { classAlgebra = savedAlgebra; }
	}

	private void FlushUtsRun( UtsClassFrame frame, List<PcreScalarSetTerm> terms, int offset, bool negated = false ) {
		if ( frame.Items == 0 ) { return; }
		var sole = frame.Items == 1 && frame.SoleLiteral;
		var operand = new PcreByteScalarSet(
		caseless && utf8 ? PcreUtf8CaseFold.CloseRanges( frame.Ranges, cancellationToken, caselessRestricted, turkishCasing ) : frame.Ranges.ToArray(),
			frame.Predicates.ToArray(), negated, frame.ExplicitNewline && !( negated && sole )
		) { Caseless = caseless, CaselessRestricted = caselessRestricted };
		AddUtsTerm( terms, new( PcreScalarSetOperation.Operand, operand ), offset );
		JoinUtsSequence( frame, terms, offset );
		frame.Ranges.Clear(); frame.Predicates.Clear(); frame.Items = 0;
		frame.ExplicitNewline = false;
	}

	private static void JoinUtsSequence( UtsClassFrame frame, List<PcreScalarSetTerm> terms, int offset ) {
		if ( frame.HasSequence ) { AddUtsTerm( terms, new( PcreScalarSetOperation.Or ), offset ); }
		frame.HasSequence = true;
	}

	private static void AddUtsTerm( List<PcreScalarSetTerm> terms, PcreScalarSetTerm term, int offset ) {
		if ( terms.Count >= MaximumClassTerms ) { throw ClassTermLimit( offset ); }
		terms.Add( term );
	}

	private bool IsUtsOperator( int at ) => at + 1 < source.Length && source[at] == source[at + 1]
		&& source[at] is (byte)'&' or (byte)'|' or (byte)'-' or (byte)'~';

	private bool IsUtsNestedStart( int at ) => at < source.Length && source[at] == (byte)'['
		&& !( at + 1 < source.Length && source[at + 1] is (byte)':' or (byte)'.' or (byte)'='
			&& HasCompleteUtf8SpecialClassToken( at + 1 ) );

	private bool TryStartUtsRange( ref bool quoting ) {
		if ( quoting || index >= source.Length || source[index] != (byte)'-' || IsUtsOperator( index ) ) { return false; }
		var savedIndex = index++;
		SkipClassTrivia( ref quoting );
		if ( !quoting && index < source.Length
			&& ( source[index] == (byte)']' || IsUtsNestedStart( index ) || IsUtsOperator( index ) ) ) {
			index = savedIndex;
			return false;
		}
		return true;
	}

	private static PcreScalarSetOperation UtsOperation( byte operation ) => operation switch {
		(byte)'&' => PcreScalarSetOperation.And,
		(byte)'|' => PcreScalarSetOperation.Or,
		(byte)'-' => PcreScalarSetOperation.Subtract,
		(byte)'~' => PcreScalarSetOperation.Xor,
		_ => throw new InvalidOperationException( "Unknown UTS class operator." )
	};

	private PcreByteScalarExpression FinishClassExpression( List<PcreScalarSetTerm> terms, int offset ) {
		var depth = 0;
		var maximum = 0;
		var explicitNewline = false;
		foreach ( var term in terms ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( term.Operand is { } operand ) { maximum = Math.Max( maximum, ++depth ); explicitNewline |= operand.ExplicitNewline; }
			else if ( term.Operation != PcreScalarSetOperation.Not ) { depth--; }
		}
		var expression = new PcreByteScalarExpression( terms.ToArray(), maximum, explicitNewline, offset );
		return utf8 ? expression : expression with { ByteProjection = ProjectByteExpression( expression ) };
	}
}
