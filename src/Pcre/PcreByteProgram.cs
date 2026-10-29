using System.Globalization;
using System.Text;

namespace Icod.Expressions.Pcre;

internal enum PcreByteOpcode {
	Literal,
	Any,
	ScalarClass,
	ScalarSet,
	Class,
	Backreference,
	CaptureTest,
	WordBoundary,
	AssertLiteral,
	Reverse,
	ReverseRange,
	ReverseEnd,
	AssertionEnter,
	AssertionExit,
	Start,
	SearchStart,
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
	AtomicEnter,
	AtomicExit,
	Accept
}

internal sealed record PcreByteInstruction(
	PcreByteOpcode Opcode, int First = 0, int Second = 0, PcreByteSet Set = default,
	ReadOnlyMemory<byte> Bytes = default, ReadOnlyMemory<PcreByteScalarRange> Ranges = default,
	ReadOnlyMemory<PcreByteScalarPredicate> Predicates = default, UnicodeCategory? Category = null,
	bool Caseless = false, bool CaselessRestricted = false, ReadOnlyMemory<int> CaptureNumbers = default
);

internal sealed record PcreByteProgram(
	PcreByteInstruction[] Instructions, int MarkCount, bool HasCaselessComparisons, bool Utf8, bool MatchInvalidUtf,
	ReadOnlyMemory<byte> RequiredLiteralSuffix, bool AnyCrLfNewline, bool ExplicitNewline,
	bool CanMatchEmptyAtAnyOffset
);

/// <summary>Compiles the supported AST into ordered bytecode with explicit backtrack edges.</summary>
internal sealed class PcreByteProgramCompiler {
	private const int MaximumInstructions = 4096;
	private const int MaximumMarks = 256;
	private readonly List<PcreByteInstruction> instructions = new();
	// Compilation continuations live on the heap; AST depth must not consume
	// platform-dependent native stack space. This is not VM backtracking state.
	private readonly Stack<Action> compileWork = new();
	private readonly CancellationToken cancellationToken;
	private bool hasCaselessComparisons;
	private readonly bool utf8;
	private readonly bool matchInvalidUtf;
	private readonly bool anyCrLfNewline;
	private readonly bool bsrAnyCrLf;
	private int markCount;
	private int countedOffset = -1;
	private int lastCountedOffset = -1;
	private int compileVisits;

	internal PcreByteProgramCompiler( CancellationToken cancellationToken, bool utf8, bool matchInvalidUtf,
		bool anyCrLfNewline, bool bsrAnyCrLf ) {
		this.cancellationToken = cancellationToken;
		this.utf8 = utf8;
		this.matchInvalidUtf = matchInvalidUtf;
		this.anyCrLfNewline = anyCrLfNewline;
		this.bsrAnyCrLf = bsrAnyCrLf;
	}

	internal PcreByteProgram Compile( PcreByteAlternatives root ) {
		CompileAlternatives( root );
		while ( compileWork.TryPop( out var next ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			next();
		}
		Emit( PcreByteOpcode.Accept );
		return new( instructions.ToArray(), markCount, hasCaselessComparisons, utf8, matchInvalidUtf,
			FindRequiredLiteralSuffix( root ), anyCrLfNewline, HasExplicitNewline( root ), CanMatchEmptyAtAnyOffset( root ) );
	}

	private static bool CanMatchEmptyAtAnyOffset( PcreByteAlternatives alternatives ) {
		foreach ( var branch in alternatives.Branches ) {
			if ( CanMatchEmptyAtAnyOffset( branch ) ) { return true; }
		}
		return false;
	}

	private static bool CanMatchEmptyAtAnyOffset( IReadOnlyList<PcreByteNode> nodes ) {
		foreach ( var node in nodes ) {
			if ( !CanMatchEmptyAtAnyOffset( node ) ) { return false; }
		}
		return true;
	}

	private static bool CanMatchEmptyAtAnyOffset( PcreByteNode node ) => node switch {
		PcreByteSearchStart or PcreByteMatchStartReset => true,
		// A participating capture may be empty. The VM still checks participation
		// and forbids nonempty consumption across the synthetic fragment boundary.
		PcreByteBackreference => true,
		// A negative or empty literal assertion can accept without reading the
		// requested code unit, just like a nullable general assertion.
		PcreByteLiteralAssertion assertion => !assertion.IsPositive || assertion.Bytes.IsEmpty,
		PcreByteLookahead or PcreByteLookbehind => true,
		PcreByteGroup group => CanMatchEmptyAtAnyOffset( group.Body ),
		PcreByteNoncapturingGroup group => CanMatchEmptyAtAnyOffset( group.Body ),
		PcreByteCaptureConditional conditional => conditional.Body.Branches.Count == 1
			|| CanMatchEmptyAtAnyOffset( conditional.Body ),
		PcreByteAssertionConditional conditional => conditional.Body.Branches.Count == 1
			|| CanMatchEmptyAtAnyOffset( conditional.Body ),
		PcreByteStaticConditional conditional => !conditional.Value && conditional.Body.Branches.Count == 1
			|| CanMatchEmptyAtAnyOffset( conditional.Body.Branches[conditional.Value ? 0 : 1] ),
		PcreByteAtomic atomic => CanMatchEmptyAtAnyOffset( atomic.Child ),
		PcreByteRepeat repeat => repeat.Minimum == 0 || CanMatchEmptyAtAnyOffset( repeat.Child ),
		_ => false
	};

	private static bool HasExplicitNewline( PcreByteAlternatives alternatives ) {
		foreach ( var branch in alternatives.Branches ) {
			foreach ( var node in branch ) {
				if ( HasExplicitNewline( node ) ) { return true; }
			}
		}
		return false;
	}

	private static bool HasExplicitNewline( PcreByteNode node ) => node switch {
		PcreByteLiteral literal => literal.Value is '\r' or '\n',
		PcreByteClass characterClass => characterClass.ExplicitNewline,
		PcreByteScalarSet scalarSet => HasExplicitNewline( scalarSet ),
		PcreByteLiteralAssertion assertion => assertion.Bytes.Span.IndexOf( (byte)'\r' ) >= 0
			|| assertion.Bytes.Span.IndexOf( (byte)'\n' ) >= 0,
		PcreByteLookahead assertion => HasExplicitNewline( assertion.Body ),
		PcreByteLookbehind assertion => HasExplicitNewline( assertion.Body ),
		PcreByteGroup group => HasExplicitNewline( group.Body ),
		PcreByteNoncapturingGroup group => HasExplicitNewline( group.Body ),
		PcreByteCaptureConditional conditional => HasExplicitNewline( conditional.Body ),
		PcreByteStaticConditional conditional => HasExplicitNewline( conditional.Body ),
		PcreByteAssertionConditional conditional => HasExplicitNewline( conditional.Assertion )
			|| HasExplicitNewline( conditional.Body ),
		PcreByteAtomic atomic => HasExplicitNewline( atomic.Child ),
		PcreByteRepeat repeat => HasExplicitNewline( repeat.Child ),
		_ => false
	};

	private static bool HasExplicitNewline( PcreByteScalarSet set ) {
		if ( set.Negated ) { return false; }
		foreach ( var range in set.Ranges.Span ) {
			if ( range.Start <= '\n' && range.End >= '\n'
				|| range.Start <= '\r' && range.End >= '\r' ) { return true; }
		}
		return false;
	}

	private ReadOnlyMemory<byte> FindRequiredLiteralSuffix( PcreByteAlternatives root ) {
		// A single scalar-class + followed only by literals must consume this suffix.
		// Restrict the shape so a bounded no-match shortcut can preserve VM limits.
		if ( !utf8 || hasCaselessComparisons || root.Branches.Count != 1 ) { return default; }
		var branch = root.Branches[0];
		if ( branch.Count is < 3 or > 9
			|| branch[0] is not PcreByteRepeat {
				Child: PcreByteScalarClass, Minimum: 1, Maximum: int.MaxValue, Greedy: true, Offset: -1
			} ) { return default; }
		var suffix = new byte[( branch.Count - 1 ) * 4];
		var length = 0;
		for ( var index = 1; index < branch.Count; index++ ) {
			if ( branch[index] is not PcreByteLiteral literal
				|| !Rune.TryCreate( literal.Value, out var scalar )
				|| !scalar.TryEncodeToUtf8( suffix.AsSpan( length ), out var width ) ) {
				return default;
			}
			length += width;
		}
		Array.Resize( ref suffix, length );
		return suffix;
	}

	private int Emit( PcreByteOpcode opcode, int first = 0, int second = 0, PcreByteSet set = default,
		ReadOnlyMemory<byte> bytes = default, ReadOnlyMemory<PcreByteScalarRange> ranges = default,
		ReadOnlyMemory<PcreByteScalarPredicate> predicates = default, UnicodeCategory? category = null,
		bool caseless = false, bool caselessRestricted = false, ReadOnlyMemory<int> captureNumbers = default ) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( instructions.Count >= MaximumInstructions ) { throw RepeatExpansionLimit(); }
		var index = instructions.Count;
		instructions.Add( new( opcode, first, second, set, bytes, ranges, predicates, category, caseless, caselessRestricted, captureNumbers ) );
		hasCaselessComparisons |= caseless;
		return index;
	}

	private void Patch( int index, int first, int second = 0 ) {
		cancellationToken.ThrowIfCancellationRequested();
		instructions[index] = instructions[index] with { First = first, Second = second };
	}

	private void CompileAlternatives( PcreByteAlternatives alternatives, ReadOnlyMemory<int> reverseLengths = default, int reverseMinimum = -1 ) {
		var endJumps = new List<int>();
		compileWork.Push( () => CompileBranch( alternatives, 0, endJumps, reverseLengths, reverseMinimum ) );
	}

	private void CompileBranch( PcreByteAlternatives alternatives, int index, List<int> endJumps,
		ReadOnlyMemory<int> reverseLengths, int reverseMinimum ) {
		var split = index + 1 < alternatives.Branches.Count ? Emit( PcreByteOpcode.Split ) : -1;
		var first = instructions.Count;
		var ranged = !reverseLengths.IsEmpty && reverseMinimum >= 0 && reverseLengths.Span[index] > reverseMinimum;
		compileWork.Push( () => {
			if ( ranged ) { Emit( PcreByteOpcode.ReverseEnd ); }
			if ( split >= 0 ) {
				endJumps.Add( Emit( PcreByteOpcode.Jump ) );
				Patch( split, first, instructions.Count );
			}
			if ( index + 1 < alternatives.Branches.Count ) {
				CompileBranch( alternatives, index + 1, endJumps, reverseLengths, reverseMinimum );
			} else {
				foreach ( var jump in endJumps ) { Patch( jump, instructions.Count ); }
			}
		} );
		// Split saves the original endpoint; each branch rewinds independently.
		if ( !reverseLengths.IsEmpty && reverseLengths.Span[index] > 0 ) {
			Emit( ranged ? PcreByteOpcode.ReverseRange : PcreByteOpcode.Reverse, reverseLengths.Span[index],
				ranged ? reverseMinimum : 0 );
		}
		CompileSequence( alternatives.Branches[index], 0 );
	}

	private void CompileSequence( IReadOnlyList<PcreByteNode> nodes, int index ) {
		if ( index >= nodes.Count ) { return; }
		compileWork.Push( () => CompileSequence( nodes, index + 1 ) );
		ScheduleNode( nodes[index] );
	}

	private void ScheduleNode( PcreByteNode node ) => compileWork.Push( () => CompileNode( node ) );

	private void CompileNode( PcreByteNode node ) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( ++compileVisits > MaximumInstructions ) { throw RepeatExpansionLimit(); }
		switch ( node ) {
			case PcreByteLiteral literal:
				Emit( PcreByteOpcode.Literal, literal.Value, caseless: literal.Caseless, caselessRestricted: literal.CaselessRestricted );
				break;
			case PcreByteAny any:
				Emit( PcreByteOpcode.Any, any.DotAll ? 1 : 0 );
				break;
			case PcreByteNewlineSequence:
				// Synthetic CR/LF literals must not mark the source pattern as
				// explicitly mentioning a newline for the candidate-search policy.
				ScheduleNode( LowerNewlineSequence() );
				break;
			case PcreByteScalarClass scalarClass:
				Emit( PcreByteOpcode.ScalarClass, (int)scalarClass.Kind, scalarClass.Negated ? 1 : 0,
					category: scalarClass.Category, caseless: scalarClass.Caseless, caselessRestricted: scalarClass.CaselessRestricted );
				break;
			case PcreByteScalarSet scalarSet:
				Emit( PcreByteOpcode.ScalarSet, second: scalarSet.Negated ? 1 : 0,
					ranges: scalarSet.Ranges, predicates: scalarSet.Predicates, caseless: scalarSet.Caseless, caselessRestricted: scalarSet.CaselessRestricted );
				break;
			case PcreByteClass characterClass:
				Emit( PcreByteOpcode.Class, set: characterClass.Set, caseless: characterClass.Caseless, caselessRestricted: characterClass.CaselessRestricted );
				break;
			case PcreByteBackreference reference:
				Emit( PcreByteOpcode.Backreference, reference.Number, caseless: reference.Caseless,
					caselessRestricted: reference.CaselessRestricted, captureNumbers: reference.DuplicateNumbers );
				break;
			case PcreByteWordBoundary boundary:
				Emit( PcreByteOpcode.WordBoundary, boundary.IsBoundary ? 1 : 0,
					boundary.UnicodeWord ? 1 : 0 );
				break;
			case PcreByteLiteralAssertion assertion:
				Emit( PcreByteOpcode.AssertLiteral, assertion.IsPositive ? 1 : 0,
					assertion.IsBehind ? 1 : 0, bytes: assertion.Bytes, caseless: assertion.Caseless, caselessRestricted: assertion.CaselessRestricted );
				break;
			case PcreByteLookahead assertion:
				CompileAssertion( assertion.Body, assertion.IsPositive, nonAtomic: assertion.NonAtomic );
				break;
			case PcreByteLookbehind assertion:
				CompileAssertion( assertion.Body, assertion.IsPositive, assertion.BranchLengths, assertion.Minimum, assertion.NonAtomic );
				break;
			case PcreByteAnchor anchor:
				Emit( anchor.IsStart ? PcreByteOpcode.Start : PcreByteOpcode.End );
				break;
			case PcreByteSearchStart:
				Emit( PcreByteOpcode.SearchStart );
				break;
			case PcreByteMatchStartReset:
				Emit( PcreByteOpcode.CaptureStart, 0 );
				break;
			case PcreByteLineAnchor lineAnchor:
				Emit( lineAnchor.IsStart ? PcreByteOpcode.StartLine : PcreByteOpcode.EndLine );
				break;
			case PcreByteEndBeforeFinalLf end:
				Emit( PcreByteOpcode.EndBeforeFinalLf, end.FragmentEnd ? 1 : 0 );
				break;
			case PcreByteGroup group:
				Emit( PcreByteOpcode.CaptureStart, group.Number );
				compileWork.Push( () => Emit( PcreByteOpcode.CaptureEnd, group.Number ) );
				CompileAlternatives( group.Body );
				break;
			case PcreByteNoncapturingGroup noncapturing:
				CompileAlternatives( noncapturing.Body );
				break;
			case PcreByteCaptureConditional conditional:
				CompileConditional( conditional );
				break;
			case PcreByteAssertionConditional conditional:
				CompileAssertionConditional( conditional );
				break;
			case PcreByteStaticConditional conditional:
				CompileStaticConditional( conditional );
				break;
			case PcreByteAtomic atomic:
				Emit( PcreByteOpcode.AtomicEnter );
				compileWork.Push( () => Emit( PcreByteOpcode.AtomicExit ) );
				ScheduleNode( atomic.Child );
				break;
			case PcreByteRepeat repeat:
				CompileRepeat( repeat );
				break;
			default:
				throw new InvalidOperationException( "Parser produced an unknown PCRE byte node." );
		}
	}

	private void CompileConditional( PcreByteCaptureConditional conditional ) {
		// A condition chooses once. Only alternatives inside its selected body
		// may save paths; never offer the other conditional branch as a retry.
		var test = Emit( PcreByteOpcode.CaptureTest, captureNumbers: conditional.CaptureNumbers );
		var yes = instructions.Count;
		compileWork.Push( () => {
			var endJump = Emit( PcreByteOpcode.Jump );
			var no = instructions.Count;
			compileWork.Push( () => {
				Patch( test, yes, no );
				Patch( endJump, instructions.Count );
			} );
			if ( conditional.Body.Branches.Count > 1 ) { CompileSequence( conditional.Body.Branches[1], 0 ); }
		} );
		CompileSequence( conditional.Body.Branches[0], 0 );
	}

	private void CompileStaticConditional( PcreByteStaticConditional conditional ) {
		var choice = Emit( PcreByteOpcode.Jump );
		var yes = instructions.Count;
		compileWork.Push( () => {
			var endJump = Emit( PcreByteOpcode.Jump );
			var no = instructions.Count;
			compileWork.Push( () => {
				Patch( choice, conditional.Value ? yes : no );
				Patch( endJump, instructions.Count );
			} );
			if ( conditional.Body.Branches.Count > 1 ) { CompileSequence( conditional.Body.Branches[1], 0 ); }
		} );
		// Unreachable declarations still resolve and compile within shared budgets.
		CompileSequence( conditional.Body.Branches[0], 0 );
	}

	private void CompileAssertionConditional( PcreByteAssertionConditional conditional ) {
		var (body, positive, lengths, minimum) = conditional.Assertion switch {
			PcreByteLookahead assertion => (assertion.Body, assertion.IsPositive, ReadOnlyMemory<int>.Empty, -1),
			PcreByteLookbehind assertion => (assertion.Body, assertion.IsPositive, assertion.BranchLengths, assertion.Minimum),
			_ => throw new InvalidOperationException( "A condition requires a general atomic assertion." )
		};
		var flags = ( positive ? 1 : 0 ) | ( minimum >= 0 ? 2 : 0 ) | 8;
		var enter = Emit( PcreByteOpcode.AssertionEnter, second: flags );
		compileWork.Push( () => {
			var exit = Emit( PcreByteOpcode.AssertionExit, second: 8 );
			var yes = instructions.Count;
			compileWork.Push( () => {
				var endJump = Emit( PcreByteOpcode.Jump );
				var no = instructions.Count;
				compileWork.Push( () => {
					// Body exhaustion restores the guard; body match commits either
					// polarity's captures. Neither outcome saves its sibling branch.
					Patch( enter, positive ? no : yes, flags );
					Patch( exit, positive ? yes : no, 8 );
					Patch( endJump, instructions.Count );
				} );
				if ( conditional.Body.Branches.Count > 1 ) { CompileSequence( conditional.Body.Branches[1], 0 ); }
			} );
			CompileSequence( conditional.Body.Branches[0], 0 );
		} );
		CompileAlternatives( body, lengths, minimum );
	}

	private PcreByteAtomic LowerNewlineSequence() {
		var branches = new List<IReadOnlyList<PcreByteNode>> {
			new PcreByteNode[] { new PcreByteLiteral( '\r' ), new PcreByteLiteral( '\n' ) }
		};
		if ( bsrAnyCrLf ) {
			branches.Add( new PcreByteNode[] { new PcreByteLiteral( '\r' ) } );
			branches.Add( new PcreByteNode[] { new PcreByteLiteral( '\n' ) } );
		} else {
			PcreByteNode single = utf8 ? new PcreByteScalarClass( PcreByteScalarClassKind.VerticalSpace, false )
				: new PcreByteClass( PcreByteSet.VerticalSpace );
			branches.Add( new[] { single } );
		}
		return new PcreByteAtomic( new PcreByteNoncapturingGroup( new PcreByteAlternatives( branches ) ) );
	}

	private void CompileAssertion( PcreByteAlternatives body, bool positive,
		ReadOnlyMemory<int> reverseLengths = default, int reverseMinimum = -1, bool nonAtomic = false ) {
		var flags = ( positive ? 1 : 0 ) | ( reverseMinimum >= 0 ? 2 : 0 ) | ( nonAtomic ? 4 : 0 );
		var enter = Emit( PcreByteOpcode.AssertionEnter, second: flags );
		compileWork.Push( () => {
			Emit( PcreByteOpcode.AssertionExit, second: nonAtomic ? 4 : 0 );
			Patch( enter, instructions.Count, flags );
		} );
		CompileAlternatives( body, reverseLengths, reverseMinimum );
	}

	private void CompileRepeat( PcreByteRepeat repeat ) {
		if ( repeat.Offset >= 0 ) {
			var previousOffset = countedOffset;
			if ( countedOffset < 0 ) { countedOffset = repeat.Offset; }
			lastCountedOffset = countedOffset;
			compileWork.Push( () => countedOffset = previousOffset );
			if ( repeat.Child is PcreByteStaticConditional { Value: false, Body.Branches.Count: 1 } ) {
				CompileIgnoredStaticRepeat( repeat );
			} else { CompileCountedRepeat( repeat, 0 ); }
			return;
		}
		if ( repeat.Child is PcreByteStaticConditional { Value: false, Body.Branches.Count: 1 } ) {
			CompileIgnoredStaticRepeat( repeat );
			return;
		}
		if ( repeat.Maximum == 1 ) {
			var split = Emit( PcreByteOpcode.Split );
			var body = instructions.Count;
			compileWork.Push( () => {
				var end = instructions.Count;
				Patch( split, repeat.Greedy ? body : end, repeat.Greedy ? end : body );
			} );
			ScheduleNode( repeat.Child );
			return;
		}
		if ( repeat.Minimum == 1 ) {
			var initialMark = AllocateMark();
			Emit( PcreByteOpcode.Mark, initialMark );
			compileWork.Push( () => CompileUnboundedRepeat( repeat, Emit( PcreByteOpcode.Progress, initialMark ) ) );
			ScheduleNode( repeat.Child );
		} else {
			CompileUnboundedRepeat( repeat );
		}
	}

	private void CompileIgnoredStaticRepeat( PcreByteRepeat repeat ) {
		// Native ignores execution but still budgets its original repeat. Retain
		// the AST for width analysis and charge logical visits without expansion.
		var count = repeat.Maximum == int.MaxValue ? repeat.Minimum + 1 : repeat.Maximum;
		var charge = Math.Max( count - 1, 0 );
		if ( charge > MaximumInstructions - compileVisits ) { throw RepeatExpansionLimit(); }
		compileVisits += charge;
		ScheduleNode( repeat.Child );
	}

	private void CompileUnboundedRepeat( PcreByteRepeat repeat, int initialProgress = -1 ) {
		var loop = Emit( PcreByteOpcode.Split );
		var bodyStart = instructions.Count;
		var mark = AllocateMark();
		Emit( PcreByteOpcode.Mark, mark );
		compileWork.Push( () => {
			var loopProgress = Emit( PcreByteOpcode.Progress, mark );
			Emit( PcreByteOpcode.Jump, loop );
			var endPosition = instructions.Count;
			Patch( loopProgress, mark, endPosition );
			if ( initialProgress >= 0 ) { Patch( initialProgress, instructions[initialProgress].First, endPosition ); }
			Patch( loop,
				repeat.Greedy ? bodyStart : endPosition,
				repeat.Greedy ? endPosition : bodyStart
			);
		} );
		ScheduleNode( repeat.Child );
	}

	private void CompileCountedRepeat( PcreByteRepeat repeat, int index ) {
		if ( index < repeat.Minimum ) {
			compileWork.Push( () => CompileCountedRepeat( repeat, index + 1 ) );
			ScheduleNode( repeat.Child );
			return;
		}
		if ( repeat.Maximum == repeat.Minimum ) { return; }
		if ( repeat.Maximum == int.MaxValue ) {
			CompileUnboundedRepeat( repeat );
			return;
		}
		var options = new List<(int Split, int Body)>();
		CompileCountedOptions( repeat, index, options );
	}

	private void CompileCountedOptions( PcreByteRepeat repeat, int index, List<(int Split, int Body)> options ) {
		if ( index < repeat.Maximum ) {
			var split = Emit( PcreByteOpcode.Split );
			var body = instructions.Count;
			compileWork.Push( () => {
				options.Add( (split, body) );
				CompileCountedOptions( repeat, index + 1, options );
			} );
			ScheduleNode( repeat.Child );
			return;
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
