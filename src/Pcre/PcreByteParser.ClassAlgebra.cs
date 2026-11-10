namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private const int MaximumClassNesting = 15;
	private const int MaximumClassTerms = 4096;
	private bool classAlgebra;

	internal static PcreByteParseException ClassTermLimit( int offset ) => new( new(
		PcreByteDiagnosticCode.PatternRepeatLimitExceeded, offset, "compiled character-class expression limit exceeded"
	) );

	// Shunting-yard stacks are bounded by source terms and native class nesting,
	// independently of the surrounding regex group stack. No recursive descent.
	private PcreByteNode ParsePerlClass( int openingOffset ) {
		var savedExtended = extended;
		var savedAlgebra = classAlgebra;
		extended = ExtendedMode.More;
		classAlgebra = true;
		try {
			var terms = new List<PcreScalarSetTerm>();
			var operators = new Stack<byte>();
			var nesting = 1;
			var termCount = 0;
			var expectingOperand = true;
			var explicitNewline = false;
			while ( index < source.Length ) {
				cancellationToken.ThrowIfCancellationRequested();
				SkipClassSpace();
				if ( source.AsSpan( index ).StartsWith( "\\E"u8 ) ) { index += 2; continue; }
				if ( source.AsSpan( index ).StartsWith( "\\Q\\E"u8 ) ) { index += 4; continue; }
				if ( index >= source.Length ) { break; }
				var token = source[index];
				if ( token == (byte)']' ) {
					if ( expectingOperand || nesting != 1 || index + 1 >= source.Length || source[index + 1] != (byte)')' ) {
						throw Invalid( openingOffset, "extended class requires an expression and adjacent closing delimiters" );
					}
					index += 2;
					while ( operators.TryPop( out var operation ) ) { terms.Add( new( ClassOperation( operation ) ) ); }
					var stackDepth = 0;
					var maximumStack = 0;
					foreach ( var term in terms ) {
						cancellationToken.ThrowIfCancellationRequested();
						if ( term.Operation == PcreScalarSetOperation.Operand ) { maximumStack = Math.Max( maximumStack, ++stackDepth ); }
						else if ( term.Operation != PcreScalarSetOperation.Not ) { stackDepth--; }
					}
					var expression = new PcreByteScalarExpression( terms.ToArray(), maximumStack, explicitNewline, openingOffset );
					return utf8 ? expression : expression with { ByteProjection = ProjectByteExpression( expression ) };
				}
				if ( expectingOperand ) {
					if ( token == (byte)'!' ) {
						if ( ++termCount > MaximumClassTerms ) { throw ClassTermLimit( openingOffset ); }
						operators.Push( token ); index++; continue;
					}
					if ( token == (byte)'(' ) {
						CheckClassNesting( ++nesting, openingOffset );
						operators.Push( token ); index++; continue;
					}
					if ( token is not ( (byte)'[' or (byte)'\\' ) || source.AsSpan( index ).StartsWith( "\\Q"u8 ) ) {
						throw Invalid( openingOffset, "extended class operand must be an escape, POSIX item or ordinary bracket class" );
					}
					if ( ++termCount > MaximumClassTerms ) { throw ClassTermLimit( openingOffset ); }
					PcreByteScalarSet operand;
					if ( token == (byte)'[' ) {
						index++;
						if ( index < source.Length && source[index] == (byte)':' ) {
							operand = ClassOperand( ParseUtf8PosixItem( openingOffset ) );
						} else {
							CheckClassNesting( nesting + 1, openingOffset );
							operand = ParseUtf8Bracket( openingOffset );
						}
					} else { operand = ClassOperand( ParseUtf8BracketItem( openingOffset ) ); }
					operand = operand with { Caseless = caseless, CaselessRestricted = caselessRestricted };
					explicitNewline |= operand.ExplicitNewline;
					terms.Add( new( PcreScalarSetOperation.Operand, operand ) );
					expectingOperand = false;
					ApplyClassComplements( operators, terms );
					continue;
				}
				if ( token == (byte)')' ) {
					if ( nesting <= 1 ) { throw Invalid( openingOffset, "unmatched extended class parenthesis" ); }
					while ( operators.Peek() != (byte)'(' ) { terms.Add( new( ClassOperation( operators.Pop() ) ) ); }
					operators.Pop(); nesting--; index++;
					ApplyClassComplements( operators, terms );
					continue;
				}
				if ( token is not ( (byte)'&' or (byte)'|' or (byte)'+' or (byte)'-' or (byte)'^' ) ) {
					throw Invalid( openingOffset, "extended class operands require an operator" );
				}
				if ( ++termCount > MaximumClassTerms ) { throw ClassTermLimit( openingOffset ); }
				while ( operators.TryPeek( out var previous ) && previous != (byte)'('
					&& ClassPrecedence( previous ) >= ClassPrecedence( token ) ) {
					terms.Add( new( ClassOperation( operators.Pop() ) ) );
				}
				operators.Push( token ); index++; expectingOperand = true;
			}
			throw Invalid( openingOffset, "unclosed extended class" );
		} catch ( PcreByteParseException error ) when ( error.Diagnostic.Code == PcreByteDiagnosticCode.InvalidSyntax ) {
			throw Invalid( openingOffset, "invalid extended character-class expression" );
		} finally { extended = savedExtended; classAlgebra = savedAlgebra; }
	}

	private static void CheckClassNesting( int nesting, int offset ) {
		if ( nesting > MaximumClassNesting ) {
			throw new PcreByteParseException( new( PcreByteDiagnosticCode.PatternDepthLimitExceeded, offset, "extended class nesting limit exceeded" ) );
		}
	}

	private static void ApplyClassComplements( Stack<byte> operators, List<PcreScalarSetTerm> terms ) {
		while ( operators.TryPeek( out var operation ) && operation == (byte)'!' ) {
			operators.Pop(); terms.Add( new( PcreScalarSetOperation.Not ) );
		}
	}

	private static int ClassPrecedence( byte operation ) => operation == (byte)'&' ? 2 : 1;
	private static PcreScalarSetOperation ClassOperation( byte operation ) => operation switch {
		(byte)'!' => PcreScalarSetOperation.Not,
		(byte)'&' => PcreScalarSetOperation.And,
		(byte)'|' or (byte)'+' => PcreScalarSetOperation.Or,
		(byte)'-' => PcreScalarSetOperation.Subtract,
		(byte)'^' => PcreScalarSetOperation.Xor,
		_ => throw new InvalidOperationException( "Unknown extended class operator." )
	};

	private PcreByteScalarSet ClassOperand( PcreByteNode node ) {
		if ( node is PcreByteScalarClass property ) {
			return new( default, new[] { new PcreByteScalarPredicate( property.Kind, property.Negated, property.Category, property.PropertyValue ) }, false, false );
		}
		var literal = (PcreByteLiteral)node;
		PcreByteScalarRange[] ranges = [new( literal.Value, literal.Value )];
		return new( caseless && utf8 ? PcreUtf8CaseFold.CloseRanges( ranges, cancellationToken, caselessRestricted ) : ranges,
			default, false, literal.Value is '\r' or '\n' );
	}
}
