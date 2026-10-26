using System.Globalization;

namespace Icod.Expressions.Pcre;

internal enum PcreByteOpcode {
	Literal,
	Any,
	ScalarClass,
	ScalarSet,
	Class,
	Backreference,
	WordBoundary,
	AssertLiteral,
	Start,
	End,
	StartLine,
	EndLine,
	EndBeforeFinalLf,
	CaptureStart,
	CaptureEnd,
	Split,
	Jump,
	Mark,
	Progress,
	Accept
}

internal sealed record PcreByteInstruction(
	PcreByteOpcode Opcode, int First = 0, int Second = 0, PcreByteSet Set = default,
	ReadOnlyMemory<byte> Bytes = default, ReadOnlyMemory<PcreByteScalarRange> Ranges = default,
	ReadOnlyMemory<PcreByteScalarPredicate> Predicates = default, UnicodeCategory? Category = null
);

internal sealed record PcreByteProgram(
	PcreByteInstruction[] Instructions, int MarkCount, bool Caseless, bool Utf8, bool MatchInvalidUtf
);

/// <summary>Compiles the supported AST into ordered bytecode with explicit backtrack edges.</summary>
internal sealed class PcreByteProgramCompiler {
	private const int MaximumInstructions = 4096;
	private const int MaximumMarks = 256;
	private readonly List<PcreByteInstruction> instructions = new();
	private readonly CancellationToken cancellationToken;
	private readonly bool caseless;
	private readonly bool utf8;
	private readonly bool matchInvalidUtf;
	private int markCount;
	private int countedOffset = -1;
	private int lastCountedOffset = -1;
	private int compileVisits;

	internal PcreByteProgramCompiler( CancellationToken cancellationToken, bool caseless, bool utf8, bool matchInvalidUtf ) {
		this.cancellationToken = cancellationToken;
		this.caseless = caseless;
		this.utf8 = utf8;
		this.matchInvalidUtf = matchInvalidUtf;
	}

	internal PcreByteProgram Compile( PcreByteAlternatives root ) {
		CompileAlternatives( root );
		Emit( PcreByteOpcode.Accept );
		return new( instructions.ToArray(), markCount, caseless, utf8, matchInvalidUtf );
	}

	private int Emit( PcreByteOpcode opcode, int first = 0, int second = 0, PcreByteSet set = default,
		ReadOnlyMemory<byte> bytes = default, ReadOnlyMemory<PcreByteScalarRange> ranges = default,
		ReadOnlyMemory<PcreByteScalarPredicate> predicates = default, UnicodeCategory? category = null ) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( instructions.Count >= MaximumInstructions ) { throw RepeatExpansionLimit(); }
		var index = instructions.Count;
		instructions.Add( new( opcode, first, second, set, bytes, ranges, predicates, category ) );
		return index;
	}

	private void Patch( int index, int first, int second = 0 ) {
		cancellationToken.ThrowIfCancellationRequested();
		instructions[index] = instructions[index] with { First = first, Second = second };
	}

	private void CompileAlternatives( PcreByteAlternatives alternatives ) {
		var endJumps = new List<int>();
		for ( var index = 0; index < alternatives.Branches.Count; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			var split = index + 1 < alternatives.Branches.Count ? Emit( PcreByteOpcode.Split ) : -1;
			var first = instructions.Count;
			foreach ( var node in alternatives.Branches[index] ) { CompileNode( node ); }
			if ( split >= 0 ) {
				endJumps.Add( Emit( PcreByteOpcode.Jump ) );
				Patch( split, first, instructions.Count );
			}
		}
		foreach ( var jump in endJumps ) { Patch( jump, instructions.Count ); }
	}

	private void CompileNode( PcreByteNode node ) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( ++compileVisits > MaximumInstructions ) { throw RepeatExpansionLimit(); }
		switch ( node ) {
			case PcreByteLiteral literal:
				Emit( PcreByteOpcode.Literal, literal.Value );
				break;
			case PcreByteAny any:
				Emit( PcreByteOpcode.Any, any.DotAll ? 1 : 0 );
				break;
			case PcreByteScalarClass scalarClass:
				Emit( PcreByteOpcode.ScalarClass, (int)scalarClass.Kind, scalarClass.Negated ? 1 : 0,
					category: scalarClass.Category );
				break;
			case PcreByteScalarSet scalarSet:
				Emit( PcreByteOpcode.ScalarSet, second: scalarSet.Negated ? 1 : 0,
					ranges: scalarSet.Ranges, predicates: scalarSet.Predicates );
				break;
			case PcreByteClass characterClass:
				Emit( PcreByteOpcode.Class, set: characterClass.Set );
				break;
			case PcreByteBackreference reference:
				Emit( PcreByteOpcode.Backreference, reference.Number );
				break;
			case PcreByteWordBoundary boundary:
				Emit( PcreByteOpcode.WordBoundary, boundary.IsBoundary ? 1 : 0,
					boundary.UnicodeWord ? 1 : 0 );
				break;
			case PcreByteLiteralAssertion assertion:
				Emit( PcreByteOpcode.AssertLiteral, assertion.IsPositive ? 1 : 0,
					assertion.IsBehind ? 1 : 0, bytes: assertion.Bytes );
				break;
			case PcreByteAnchor anchor:
				Emit( anchor.IsStart ? PcreByteOpcode.Start : PcreByteOpcode.End );
				break;
			case PcreByteLineAnchor lineAnchor:
				Emit( lineAnchor.IsStart ? PcreByteOpcode.StartLine : PcreByteOpcode.EndLine );
				break;
			case PcreByteEndBeforeFinalLf end:
				Emit( PcreByteOpcode.EndBeforeFinalLf, end.FragmentEnd ? 1 : 0 );
				break;
			case PcreByteGroup group:
				Emit( PcreByteOpcode.CaptureStart, group.Number );
				CompileAlternatives( group.Body );
				Emit( PcreByteOpcode.CaptureEnd, group.Number );
				break;
			case PcreByteNoncapturingGroup noncapturing:
				CompileAlternatives( noncapturing.Body );
				break;
			case PcreByteRepeat repeat:
				CompileRepeat( repeat );
				break;
			default:
				throw new InvalidOperationException( "Parser produced an unknown PCRE byte node." );
		}
	}

	private void CompileRepeat( PcreByteRepeat repeat ) {
		if ( repeat.Offset >= 0 ) {
			var previousOffset = countedOffset;
			if ( countedOffset < 0 ) { countedOffset = repeat.Offset; }
			lastCountedOffset = countedOffset;
			try { CompileCountedRepeat( repeat ); }
			finally { countedOffset = previousOffset; }
			return;
		}
		if ( repeat.Maximum == 1 ) {
			var split = Emit( PcreByteOpcode.Split );
			var body = instructions.Count;
			CompileNode( repeat.Child );
			var end = instructions.Count;
			Patch( split, repeat.Greedy ? body : end, repeat.Greedy ? end : body );
			return;
		}
		var initialProgress = -1;
		if ( repeat.Minimum == 1 ) {
			var initialMark = AllocateMark();
			Emit( PcreByteOpcode.Mark, initialMark );
			CompileNode( repeat.Child );
			initialProgress = Emit( PcreByteOpcode.Progress, initialMark );
		}
		var loop = Emit( PcreByteOpcode.Split );
		var bodyStart = instructions.Count;
		var mark = AllocateMark();
		Emit( PcreByteOpcode.Mark, mark );
		CompileNode( repeat.Child );
		var loopProgress = Emit( PcreByteOpcode.Progress, mark );
		Emit( PcreByteOpcode.Jump, loop );
		var endPosition = instructions.Count;
		Patch( loopProgress, mark, endPosition );
		if ( initialProgress >= 0 ) { Patch( initialProgress, instructions[initialProgress].First, endPosition ); }
		Patch( loop,
			repeat.Greedy ? bodyStart : endPosition,
			repeat.Greedy ? endPosition : bodyStart
		);
	}

	private void CompileCountedRepeat( PcreByteRepeat repeat ) {
		for ( var index = 0; index < repeat.Minimum; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			CompileNode( repeat.Child );
		}
		if ( repeat.Maximum == repeat.Minimum ) { return; }
		if ( repeat.Maximum == int.MaxValue ) {
			var loop = Emit( PcreByteOpcode.Split );
			var body = instructions.Count;
			var mark = AllocateMark();
			Emit( PcreByteOpcode.Mark, mark );
			CompileNode( repeat.Child );
			var progress = Emit( PcreByteOpcode.Progress, mark );
			Emit( PcreByteOpcode.Jump, loop );
			var end = instructions.Count;
			Patch( progress, mark, end );
			Patch( loop, repeat.Greedy ? body : end, repeat.Greedy ? end : body );
			return;
		}
		var options = new List<(int Split, int Body)>();
		for ( var index = repeat.Minimum; index < repeat.Maximum; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			var split = Emit( PcreByteOpcode.Split );
			var body = instructions.Count;
			CompileNode( repeat.Child );
			options.Add( (split, body) );
		}
		var exit = instructions.Count;
		foreach ( var (split, body) in options ) {
			Patch( split, repeat.Greedy ? body : exit, repeat.Greedy ? exit : body );
		}
	}

	private int AllocateMark() {
		if ( markCount >= MaximumMarks ) { throw RepeatExpansionLimit(); }
		return markCount++;
	}

	private PcreByteParseException RepeatExpansionLimit() => new( new(
		PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
		countedOffset >= 0 ? countedOffset : Math.Max( 0, lastCountedOffset ),
		"compiled repeat expansion limit exceeded"
	) );
}
