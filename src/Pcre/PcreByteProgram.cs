namespace Icod.Expressions.Pcre;

internal enum PcreByteOpcode {
	Literal,
	Class,
	Backreference,
	Start,
	End,
	CaptureStart,
	CaptureEnd,
	Split,
	Jump,
	Mark,
	Progress,
	Accept
}

internal sealed record PcreByteInstruction(
	PcreByteOpcode Opcode, int First = 0, int Second = 0, PcreByteSet Set = default
);

internal sealed record PcreByteProgram( PcreByteInstruction[] Instructions, int MarkCount );

/// <summary>Compiles the supported AST into ordered bytecode with explicit backtrack edges.</summary>
internal sealed class PcreByteProgramCompiler {
	private readonly List<PcreByteInstruction> instructions = new();
	private readonly CancellationToken cancellationToken;
	private int markCount;

	internal PcreByteProgramCompiler( CancellationToken cancellationToken ) {
		this.cancellationToken = cancellationToken;
	}

	internal PcreByteProgram Compile( PcreByteAlternatives root ) {
		CompileAlternatives( root );
		Emit( PcreByteOpcode.Accept );
		return new( instructions.ToArray(), markCount );
	}

	private int Emit( PcreByteOpcode opcode, int first = 0, int second = 0, PcreByteSet set = default ) {
		cancellationToken.ThrowIfCancellationRequested();
		var index = instructions.Count;
		instructions.Add( new( opcode, first, second, set ) );
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
		switch ( node ) {
			case PcreByteLiteral literal:
				Emit( PcreByteOpcode.Literal, literal.Value );
				break;
			case PcreByteClass characterClass:
				Emit( PcreByteOpcode.Class, set: characterClass.Set );
				break;
			case PcreByteBackreference reference:
				Emit( PcreByteOpcode.Backreference, reference.Number );
				break;
			case PcreByteAnchor anchor:
				Emit( anchor.IsStart ? PcreByteOpcode.Start : PcreByteOpcode.End );
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
			var initialMark = markCount++;
			Emit( PcreByteOpcode.Mark, initialMark );
			CompileNode( repeat.Child );
			initialProgress = Emit( PcreByteOpcode.Progress, initialMark );
		}
		var loop = Emit( PcreByteOpcode.Split );
		var bodyStart = instructions.Count;
		var mark = markCount++;
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
}
