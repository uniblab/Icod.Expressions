using System.Globalization;
using System.Numerics;
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
	RecursionTest,
	Call,
	Return,
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
	Accept,
	Fail
}

internal sealed record PcreByteInstruction(
	PcreByteOpcode Opcode, int First = 0, int Second = 0, PcreByteSet Set = default,
	ReadOnlyMemory<byte> Bytes = default, ReadOnlyMemory<PcreByteScalarRange> Ranges = default,
	ReadOnlyMemory<PcreByteScalarPredicate> Predicates = default, UnicodeCategory? Category = null,
	bool Caseless = false, bool CaselessRestricted = false, ReadOnlyMemory<int> CaptureNumbers = default,
	bool SkipCrLfAfterRepeat = false
);

internal readonly record struct PcreBytePrefixLiteral( int ByteOffset, int Value, bool Caseless, bool CaselessRestricted );

internal sealed record PcreByteProgram(
	PcreByteInstruction[] Instructions, int MarkCount, bool HasCaselessComparisons, bool Utf8, bool MatchInvalidUtf,
	ReadOnlyMemory<byte> RequiredLiteralSuffix, PcreByteNewline Newline, bool ExplicitNewline,
	bool CanMatchEmptyAtAnyOffset, bool ByteCrLfPrefix, bool UtfCrLfStartClass, bool NotEmpty, bool NotEmptyAtStart,
	bool LineStartSearch, bool Ucp, PcreBytePrefixLiteral? ByteCrLfPrefixLiteral
) {
	internal bool AnyCrLfNewline => Newline == PcreByteNewline.AnyCrLf;
	internal byte NewlineCharacter => Newline.Character();
}

/// <summary>Compiles the supported AST into ordered bytecode with explicit backtrack edges.</summary>
internal sealed class PcreByteProgramCompiler {
	private const int MaximumInstructions = 4096;
	private const int MaximumMarks = 256;
	private const int MaximumStartClassVisits = 1000;
	private const int MaximumBytePrefixLength = 12;
	private readonly List<PcreByteInstruction> instructions = new();
	// Compilation continuations live on the heap; AST depth must not consume
	// platform-dependent native stack space. This is not VM backtracking state.
	private readonly Stack<Action> compileWork = new();
	private readonly CancellationToken cancellationToken;
	private bool hasCaselessComparisons;
	private readonly bool utf8;
	private readonly bool ucp;
	private readonly bool matchInvalidUtf;
	private readonly PcreByteNewline newline;
	private readonly bool bsrAnyCrLf;
	private readonly bool notEmpty;
	private readonly bool notEmptyAtStart;
	private int markCount;
	private int countedOffset = -1;
	private int lastCountedOffset = -1;
	private int compileVisits;
	private PcreByteScalarSet? utfCrLfLeadingRepeatSet;
	private PcreByteNode? utfCrLfLeadingStarAtom;
	private bool skipUtfCrLfLeadingStar;
	private bool skipUtfCrLfLeadingRepeat;
	private bool utfCrLfJoinedNullablePrefix;
	private readonly List<(int Instruction, int Number)> calls = new();

	internal PcreByteProgramCompiler( CancellationToken cancellationToken, bool utf8, bool matchInvalidUtf,
		PcreByteNewline newline, bool bsrAnyCrLf, bool notEmpty, bool notEmptyAtStart, bool ucp ) {
		this.cancellationToken = cancellationToken;
		this.utf8 = utf8;
		this.ucp = ucp;
		this.matchInvalidUtf = matchInvalidUtf;
		this.newline = newline;
		this.bsrAnyCrLf = bsrAnyCrLf;
		this.notEmpty = notEmpty;
		this.notEmptyAtStart = notEmptyAtStart;
	}

	internal PcreByteProgram Compile( PcreByteAlternatives root, IReadOnlyDictionary<int, PcreByteGroup> targets ) {
		(utfCrLfLeadingStarAtom, skipUtfCrLfLeadingStar) = utf8 ? FindUtfCrLfRootStarAtom( root ) : default;
		(utfCrLfLeadingRepeatSet, skipUtfCrLfLeadingRepeat) = utf8 ? FindUtfCrLfLeadingRepeat( root ) : default;
		utfCrLfJoinedNullablePrefix = false;
		if ( utf8 && utfCrLfLeadingRepeatSet is null ) {
			utfCrLfLeadingRepeatSet = FindUtfCrLfJoinedRepeat( root );
			skipUtfCrLfLeadingRepeat = false;
			utfCrLfJoinedNullablePrefix = utfCrLfLeadingRepeatSet is not null;
		}
		if ( utfCrLfLeadingRepeatSet is null ) {
			utfCrLfLeadingStarAtom = null;
			skipUtfCrLfLeadingStar = false;
		}
		CompileAlternatives( root );
		while ( compileWork.TryPop( out var next ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			next();
		}
		Emit( PcreByteOpcode.Accept );
		var entries = new Dictionary<int, int>();
		foreach ( var target in targets ) {
			entries.Add( target.Key, instructions.Count );
			// A subroutine calls the body, bypassing its root capture and exterior repeat.
			CompileAlternatives( target.Value.Body );
			while ( compileWork.TryPop( out var next ) ) {
				cancellationToken.ThrowIfCancellationRequested();
				next();
			}
			Emit( PcreByteOpcode.Return );
		}
		foreach ( var call in calls ) { Patch( call.Instruction, entries[call.Number], call.Number ); }
		var bytePrefix = utf8 ? default : FindByteCrLfPrefix( root );
		return new( instructions.ToArray(), markCount, hasCaselessComparisons, utf8, matchInvalidUtf,
			FindRequiredLiteralSuffix( root ), newline, HasExplicitNewline( root ), CanMatchEmptyAtAnyOffset( root ),
			bytePrefix.Unconditional, utf8 && HasUtfCrLfStartClass( root ), notEmpty, notEmptyAtStart,
			matchInvalidUtf && newline == PcreByteNewline.Any && HasLineStartSearch( root ), ucp, bytePrefix.Literal );
	}

	private bool HasLineStartSearch( PcreByteAlternatives root ) {
		// Exclude character/first-code-unit search, then follow native is_startline:
		// the first significant node in every branch must itself require ^.
		foreach ( var instruction in instructions ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( instruction.Opcode is not ( PcreByteOpcode.CaptureStart or PcreByteOpcode.CaptureEnd
				or PcreByteOpcode.StartLine or PcreByteOpcode.EndLine or PcreByteOpcode.EndBeforeFinalLf or PcreByteOpcode.End
				or PcreByteOpcode.Split or PcreByteOpcode.Jump or PcreByteOpcode.AtomicEnter or PcreByteOpcode.AtomicExit
				or PcreByteOpcode.Accept ) ) { return false; }
		}
		var pending = new Stack<PcreByteNode>();
		if ( !AddBranches( root ) ) { return false; }
		while ( pending.TryPop( out var node ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			switch ( node ) {
				case PcreByteLineAnchor { IsStart: true }: break;
				case PcreByteGroup group:
					if ( !AddBranches( group.Body ) ) { return false; }
					break;
				case PcreByteNoncapturingGroup group:
					if ( !AddBranches( group.Body ) ) { return false; }
					break;
				case PcreByteAtomic atomic: pending.Push( atomic.Child ); break;
				default: return false;
			}
		}
		return true;

		bool AddBranches( PcreByteAlternatives body ) {
			if ( body.Branches.Count == 0 ) { return false; }
			foreach ( var branch in body.Branches ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( branch.Count == 0 ) { return false; }
				pending.Push( branch[0] );
			}
			return true;
		}
	}

	private (PcreByteNode? Atom, bool Skip) FindUtfCrLfRootStarAtom( PcreByteAlternatives root ) {
		if ( root.Branches.Count != 1 || root.Branches[0].Count == 0 ) { return default; }
		var visits = 1;
		var branch = root.Branches[0];
		var index = 0;
		var groupDepth = 0;
		var skip = true;
		HashSet<int>? captures = null;
		while ( index < branch.Count ) {
			cancellationToken.ThrowIfCancellationRequested();
			var node = branch[index++];
			// Native bitmap and early-fail study both ignore word boundaries.
			if ( node is PcreByteWordBoundary ) {
				if ( ++visits > MaximumStartClassVisits ) { return default; }
				continue;
			}
			var cost = 2;
			if ( node is PcreByteAtomic atomic ) {
				// Bitmap study crosses explicit atomic groups, but early-fail
				// descent does not. Possessive repeats keep their iterator rules.
				if ( atomic.Child is PcreByteNoncapturingGroup ) {
					if ( ++visits > MaximumStartClassVisits ) { return default; }
					skip = false;
				} else if ( atomic.Child is not PcreByteRepeat ) { return default; }
				if ( atomic.Child is PcreByteRepeat { Child: PcreByteGroup or PcreByteNoncapturingGroup
					or PcreByteAtomic { Child: PcreByteNoncapturingGroup } } ) { skip = false; }
				node = atomic.Child;
				cost++;
			}
			if ( IsUtfCrLfErasedRepeat( node ) ) {
				if ( visits > MaximumStartClassVisits - cost ) { return default; }
				visits += cost;
				continue;
			}
			// Native copies required groups with ordinary KET before optional/loop bodies.
			if ( node is PcreByteRepeat { Minimum: >= 1, Maximum: > 0 } required
				&& ( required.Maximum != int.MaxValue || required.Minimum >= 2 ) ) {
				if ( required.Child is not ( PcreByteGroup or PcreByteNoncapturingGroup
					or PcreByteAtomic { Child: PcreByteNoncapturingGroup } )
					|| visits > MaximumStartClassVisits - cost ) { return default; }
				visits += cost;
				node = required.Child;
				if ( node is PcreByteAtomic { Child: PcreByteNoncapturingGroup group } ) {
					if ( ++visits > MaximumStartClassVisits ) { return default; }
					skip = false;
					node = group;
				}
			}
			var body = node switch {
				PcreByteGroup group => group.Body,
				PcreByteNoncapturingGroup group => group.Body,
				_ => null
			};
			if ( body is not null ) {
				if ( visits >= MaximumStartClassVisits || body.Branches.Count != 1
					|| body.Branches[0].Count == 0 ) { return default; }
				if ( node is PcreByteGroup capture ) { (captures ??= new()).Add( capture.Number ); }
				visits++;
				groupDepth++;
				branch = body.Branches[0];
				index = 0;
				continue;
			}
			// Bitmap study reaches deeper groups than iterator recovery. Only
			// ancestors of this star count; its following capture is independent.
			if ( node is not PcreByteRepeat { Minimum: 0, Maximum: int.MaxValue } repeat ) { return default; }
			skip &= groupDepth <= 4;
			if ( captures is not null ) {
				// Only references/tests on this ancestor chain prevent its native
				// early-fail descent. A later capture does not own this star's cache.
				var classification = ClassifyUtfCaptureRepeat( root, captures );
				if ( !classification.Eligible ) { return default; }
				skip &= classification.Skip;
			}
			// A group loop is not a scalar early-fail cache owner.
			if ( repeat.Child is PcreByteGroup or PcreByteNoncapturingGroup
				or PcreByteAtomic { Child: PcreByteNoncapturingGroup } ) { skip = false; }
			return (repeat.Child, skip);
		}
		return default;
	}

	private static bool IsUtfCrLfErasedRepeat( PcreByteNode node ) => node is PcreByteRepeat {
		Maximum: 0, Child: PcreByteBackreference or PcreByteLiteral or PcreByteAny
			or PcreByteScalarClass or PcreByteScalarSet or PcreByteNewlineSequence
	};

	private (PcreByteScalarSet? Set, bool Skip) FindUtfCrLfLeadingRepeat( PcreByteAlternatives root ) {
		// Native bitmap study recognizes deeper leading groups than type_skip,
		// which follows only four optimized groups to PLUS. Keep recognition
		// separate from iterator acceleration so deeper repeats retain LF entry.
		// Explicit atomic groups wrap a noncapturing group; a possessive quantifier
		// wraps the repeat itself. ASCII and wide classes share this recovery.
		if ( root.Branches.Count != 1 || root.Branches[0].Count == 0 ) { return default; }
		var visits = 1;
		var leading = FindUtfCrLfLeadingNode( root.Branches[0], ref visits );
		var node = leading.Node;
		var skip = !leading.BlocksSkip;
		var groupDepth = 0;
		HashSet<int>? captures = null;
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			var body = node switch {
				PcreByteGroup group => group.Body,
				PcreByteNoncapturingGroup group => group.Body,
				// Atomic groups retain mandatory-set bitmap study even without
				// a first star, but cannot assign recovery to the following PLUS.
				PcreByteAtomic { Child: PcreByteNoncapturingGroup group } => group.Body,
				_ => null
			};
			if ( body is null ) { break; }
			if ( ++visits > MaximumStartClassVisits || body.Branches.Count != 1
				|| body.Branches[0].Count == 0 ) { return default; }
			if ( node is PcreByteGroup capture ) { (captures ??= new()).Add( capture.Number ); }
			skip &= node is not PcreByteAtomic;
			groupDepth++;
			leading = FindUtfCrLfLeadingNode( body.Branches[0], ref visits );
			skip &= !leading.BlocksSkip;
			node = leading.Node;
		}
		var set = FindUtfCrLfMandatoryRepeatSet( node );
		if ( set is null ) { return default; }
		skip &= groupDepth <= 4;
		if ( captures is not null ) {
			var classification = ClassifyUtfCaptureRepeat( root, captures );
			if ( !classification.Eligible ) { return default; }
			skip &= classification.Skip;
		}
		return (set, skip);
	}

	private PcreByteScalarSet? FindUtfCrLfMandatoryRepeatSet( PcreByteNode? node ) {
		if ( node is PcreByteAtomic atomic ) { node = atomic.Child; }
		if ( node is not PcreByteRepeat { Minimum: 1, Maximum: int.MaxValue,
			Child: PcreByteScalarSet { Negated: false } set } || !set.Predicates.IsEmpty ) { return null; }
		var lf = false;
		foreach ( var range in set.Ranges.Span ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( range.Start < 0 || range.End > 0x10FFFF || range.Start > range.End
				|| range.Start <= '\r' && range.End >= '\r' ) { return null; }
			lf |= range.Start <= '\n' && range.End >= '\n';
		}
		return lf ? set : null;
	}

	private PcreByteScalarSet? FindUtfCrLfJoinedRepeat( PcreByteAlternatives root ) {
		if ( root.Branches.Count != 1 || root.Branches[0].Count == 0 ) { return null; }
		// Compile-time branch continuations only; no VM frames or subject spans.
		var continuations = new Stack<(IReadOnlyList<PcreByteNode> Branch, int Index,
			PcreByteAlternatives Body, int NextBranch, int RepeatCopies, int RepeatStartVisits)>();
		var captures = new HashSet<int>();
		var branch = root.Branches[0];
		var index = 0;
		var visits = 1;
		var blocksSkip = false;
		var joined = false;
		var pendingAlternatives = 0;
		var pendingRepeats = 0;
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			var leading = FindUtfCrLfLeadingNode( branch, ref visits, index, blocksSkip );
			blocksSkip = leading.BlocksSkip;
			if ( leading.Nullable ) {
				if ( !continuations.TryPop( out var next ) ) { return null; }
				if ( next.NextBranch < next.Body.Branches.Count ) {
					if ( ++visits > MaximumStartClassVisits ) { return null; }
					continuations.Push( (next.Branch, next.Index, next.Body, next.NextBranch + 1,
						next.RepeatCopies, next.RepeatStartVisits) );
					branch = next.Body.Branches[next.NextBranch];
					index = 0;
					continue;
				}
				if ( next.RepeatCopies > 0 ) {
					var copies = next.RepeatCopies - 1;
					var perCopy = visits - next.RepeatStartVisits;
					// Include the group visit even for an empty body; never multiply unchecked.
					if ( copies > 0 ) {
						if ( perCopy > ( MaximumStartClassVisits - visits ) / copies ) { return null; }
						visits += perCopy * copies;
					}
					pendingRepeats--;
				}
				if ( next.Body.Branches.Count > 1 ) { pendingAlternatives--; }
				(branch, index) = (next.Branch, next.Index);
				// The returned group is retained even when its body is empty or erased.
				blocksSkip = true;
				joined = true;
				continue;
			}
			var node = leading.Node;
			if ( node is null ) { return null; }
			var candidate = node;
			var repeatCopies = 0;
			var repeatMinimum = 0;
			var repeatCost = 0;
			var repeatBlocksSkip = false;
			if ( candidate is PcreByteAtomic { Child: PcreByteRepeat } modifier ) {
				candidate = modifier.Child;
				repeatCost = 1;
				repeatBlocksSkip = true;
			}
			if ( candidate is PcreByteRepeat { Minimum: >= 0, Maximum: > 0 } repeat ) {
				candidate = repeat.Child;
				repeatCopies = repeat.Maximum;
				repeatMinimum = repeat.Minimum;
				repeatCost++;
				repeatBlocksSkip |= repeat.Minimum == 0;
			}
			var body = candidate switch {
				PcreByteGroup group => group.Body,
				PcreByteNoncapturingGroup group => group.Body,
				PcreByteAtomic { Child: PcreByteNoncapturingGroup group } => group.Body,
				_ => null
			};
			if ( body is not null ) {
				if ( repeatCopies == int.MaxValue ) {
					// Required copies plus one loop body; guard before adding.
					if ( repeatMinimum >= MaximumStartClassVisits ) { return null; }
					repeatCopies = repeatMinimum + 1;
					repeatBlocksSkip |= repeatMinimum <= 1;
				}
				var cost = 1 + repeatCost;
				if ( visits > MaximumStartClassVisits - cost || body.Branches.Count == 0 ) { return null; }
				visits += cost;
				if ( candidate is PcreByteGroup capture ) { captures.Add( capture.Number ); }
				if ( repeatCopies > 0 ) {
					pendingRepeats++;
					blocksSkip |= repeatBlocksSkip;
				}
				// Atomic groups stop native early-fail descent into their bodies.
				blocksSkip |= candidate is PcreByteAtomic;
				if ( body.Branches.Count > 1 ) {
					pendingAlternatives++;
					// Native alternative groups start early-fail study with a retained count.
					blocksSkip = true;
				}
				continuations.Push( (branch, leading.NextIndex, body, 1,
					repeatCopies, repeatCopies > 0 ? visits - 1 : 0) );
				branch = body.Branches[0];
				index = 0;
				continue;
			}
			var set = FindUtfCrLfMandatoryRepeatSet( node );
			// A nested return must not bypass unchecked alternatives or repeat bodies.
			if ( !joined || pendingAlternatives != 0 || pendingRepeats != 0 || set is null
				|| !ClassifyUtfCaptureRepeat( root, captures ).Eligible ) { return null; }
			// Returned groups block later PLUS recovery, with or without a first-star owner.
			return set;
		}
	}

	private (PcreByteNode? Node, bool BlocksSkip, int NextIndex, bool Nullable) FindUtfCrLfLeadingNode(
		IReadOnlyList<PcreByteNode> branch, ref int visits, int startIndex = 0, bool blocksSkip = false ) {
		// Bitmap study crosses retained zero brackets and supported optional atoms, but
		// their opcodes prevent repeat skipping. Direct primitive/reference0 vanishes.
		for ( var index = startIndex; index < branch.Count; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			var node = branch[index];
			var candidate = node;
			var wrapper = 0;
			if ( candidate is PcreByteAtomic atomic ) { candidate = atomic.Child; wrapper = 1; }
			var (cost, retained) = candidate switch {
				// Boundaries assert at runtime but do not raise native early-fail count.
				PcreByteWordBoundary => (1, false),
				PcreByteRepeat omitted when IsUtfCrLfErasedRepeat( omitted ) => (2, false),
				// Queries raise early-fail count to three. A root star owns type_skip;
				// both prevent the following PLUS from receiving its own skip marker.
				PcreByteRepeat { Minimum: 0, Maximum: > 0, Child: PcreByteLiteral literal } repeat
					when CanCrossUtfCrLfOptionalRepeat( repeat, blocksSkip )
						&& HasUtfCrLfLiteralBitmap( literal.Value, literal.Caseless, literal.CaselessRestricted ) => (2, true),
				PcreByteRepeat { Minimum: 0, Maximum: > 0, Child: PcreByteScalarSet set } repeat
					when CanCrossUtfCrLfOptionalRepeat( repeat, blocksSkip ) && IsUtfCrLfOptionalSet( set, ref visits ) => (2, true),
				// These type operands have bitmap study and exclude CR.
				// Parsed kinds already distinguish UCP properties and ASCII restrictions.
				PcreByteRepeat { Minimum: 0, Maximum: > 0, Child: PcreByteScalarClass {
					Kind: PcreByteScalarClassKind.AsciiDigit or PcreByteScalarClassKind.AsciiWord
						or PcreByteScalarClassKind.HorizontalSpace, Negated: false } } repeat
					when CanCrossUtfCrLfOptionalRepeat( repeat, blocksSkip ) => (2, true),
				PcreByteRepeat { Minimum: 0, Maximum: > 0, Child: PcreByteScalarClass {
					Kind: PcreByteScalarClassKind.AsciiSpace, Negated: true } } repeat
					when CanCrossUtfCrLfOptionalRepeat( repeat, blocksSkip ) => (2, true),
				PcreByteRepeat { Maximum: 0, Child: PcreByteGroup or PcreByteNoncapturingGroup
					or PcreByteLookahead or PcreByteLookbehind or PcreByteLiteralAssertion
					or PcreByteCaptureConditional } => (2, true),
				PcreByteRepeat { Maximum: 0, Child: PcreByteAtomic { Child: PcreByteNoncapturingGroup } } => (3, true),
				_ => (0, false)
			};
			if ( cost == 0 ) { return (node, blocksSkip, index + 1, false); }
			cost += wrapper;
			if ( visits > MaximumStartClassVisits - cost ) { return default; }
			visits += cost;
			blocksSkip |= retained;
		}
		return (null, blocksSkip, branch.Count, true);
	}

	private bool CanCrossUtfCrLfOptionalRepeat( PcreByteRepeat repeat, bool blocksSkip ) =>
		repeat.Maximum != int.MaxValue || blocksSkip || ReferenceEquals( repeat.Child, utfCrLfLeadingStarAtom );

	private bool IsUtfCrLfOptionalSet( PcreByteScalarSet set, ref int visits ) {
		if ( set.Negated || !set.Predicates.IsEmpty || set.Ranges.IsEmpty ) { return false; }
		foreach ( var range in set.Ranges.Span ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( ++visits > MaximumStartClassVisits || range.Start < 0 || range.End > 0x10FFFF
				|| range.Start > range.End || range.Start <= '\r' && range.End >= '\r' ) { return false; }
		}
		// Class sources retain their bitmap after folding. Literal sources need
		// their original scalar to distinguish CHAR/CHARI from PT_CLIST queries.
		return !set.Caseless || set.Source == PcreByteScalarSetSource.Class || set.Ranges.Length != 1
			|| set.Source == PcreByteScalarSetSource.Literal
				&& HasUtfCrLfLiteralBitmap( set.SourceLiteral, set.Caseless, set.CaselessRestricted )
			|| set.Ranges.Span[0].Start != set.Ranges.Span[0].End;
	}

	private static bool HasUtfCrLfLiteralBitmap( int scalar, bool caseless, bool restricted ) {
		if ( scalar < 0 || scalar > 0x10FFFF || scalar == '\r' ) { return false; }
		if ( !caseless ) { return true; }
		// PCRE2 10.48 default UCD_CASESET opcode classification, not membership.
		// Restricted casing suppresses the two ASCII-crossing property casesets.
		if ( restricted && scalar is 0x004B or 0x006B or 0x212A or 0x0053 or 0x0073 or 0x017F ) { return true; }
		return scalar is not (
			0x0053 or 0x0073 or 0x017F
			or 0x01C4 or 0x01C5 or 0x01C6
			or 0x01C7 or 0x01C8 or 0x01C9
			or 0x01CA or 0x01CB or 0x01CC
			or 0x01F1 or 0x01F2 or 0x01F3
			or 0x0345 or 0x0399 or 0x03B9 or 0x1FBE
			or 0x00B5 or 0x039C or 0x03BC
			or 0x03A3 or 0x03C2 or 0x03C3
			or 0x0392 or 0x03B2 or 0x03D0
			or 0x0398 or 0x03B8 or 0x03D1 or 0x03F4
			or 0x03A6 or 0x03C6 or 0x03D5
			or 0x03A0 or 0x03C0 or 0x03D6
			or 0x039A or 0x03BA or 0x03F0
			or 0x03A1 or 0x03C1 or 0x03F1
			or 0x0395 or 0x03B5 or 0x03F5
			or 0x0412 or 0x0432 or 0x1C80
			or 0x0414 or 0x0434 or 0x1C81
			or 0x041E or 0x043E or 0x1C82
			or 0x0421 or 0x0441 or 0x1C83
			or 0x0422 or 0x0442 or 0x1C84 or 0x1C85
			or 0x042A or 0x044A or 0x1C86
			or 0x0462 or 0x0463 or 0x1C87
			or 0x1E60 or 0x1E61 or 0x1E9B
			or 0x03A9 or 0x03C9 or 0x2126
			or 0x004B or 0x006B or 0x212A
			or 0x00C5 or 0x00E5 or 0x212B
			or 0x1C88 or 0xA64A or 0xA64B );
	}

	private (bool Eligible, bool Skip) ClassifyUtfCaptureRepeat( PcreByteAlternatives root, IReadOnlySet<int> captures ) {
		// Native reference/test slots clear optimization only for those captures.
		// Keep their bitmap proof while disabling repeat skipping through a related
		// leading capture. Calls and unsupported pruning remain conservative.
		var pending = new Stack<PcreByteNode>();
		var visits = 0;
		var skip = true;
		if ( !AddBody( root ) ) { return default; }
		while ( pending.TryPop( out var node ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( ++visits > MaximumStartClassVisits ) { return default; }
			switch ( node ) {
				case PcreByteGroup group:
					if ( !AddBody( group.Body ) ) { return default; }
					break;
				case PcreByteNoncapturingGroup group:
					if ( !AddBody( group.Body ) ) { return default; }
					break;
				case PcreByteLookahead assertion:
					if ( !AddBody( assertion.Body ) ) { return default; }
					break;
				case PcreByteLookbehind assertion:
					if ( !AddBody( assertion.Body ) ) { return default; }
					break;
				case PcreByteBackreference reference:
					if ( captures.Contains( reference.Number ) ) { skip = false; }
					ObserveCaptures( reference.DuplicateNumbers );
					break;
				case PcreByteCaptureConditional conditional:
					ObserveCaptures( conditional.CaptureNumbers );
					if ( !AddBody( conditional.Body ) ) { return default; }
					break;
				case PcreByteAtomic atomic: pending.Push( atomic.Child ); break;
				// Native omits a direct zero-count reference, so it has no dependency
				// effect. Retained zero-count group bodies still use generic traversal.
				case PcreByteRepeat { Maximum: 0, Child: PcreByteBackreference }: break;
				case PcreByteRepeat repeat: pending.Push( repeat.Child ); break;
				case PcreByteLiteral or PcreByteAny or PcreByteNewlineSequence or PcreByteFailure
					or PcreByteScalarClass or PcreByteScalarSet or PcreByteClass or PcreByteAnchor
					or PcreByteSearchStart or PcreByteMatchStartReset or PcreByteLineAnchor
					or PcreByteEndBeforeFinalLf or PcreByteWordBoundary or PcreByteLiteralAssertion:
					break;
				default: return default;
			}
		}
		return (true, skip);

		void ObserveCaptures( ReadOnlyMemory<int> numbers ) {
			foreach ( var number in numbers.Span ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( captures.Contains( number ) ) { skip = false; }
			}
		}

		bool AddBody( PcreByteAlternatives body ) {
			foreach ( var branch in body.Branches ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( ++visits > MaximumStartClassVisits
					|| branch.Count > MaximumStartClassVisits - visits - pending.Count ) { return false; }
				foreach ( var node in branch ) {
					cancellationToken.ThrowIfCancellationRequested();
					pending.Push( node );
				}
			}
			return true;
		}
	}

	private bool HasUtfCrLfStartClass( PcreByteAlternatives root ) {
		if ( utfCrLfJoinedNullablePrefix && utfCrLfLeadingRepeatSet is not null ) {
			cancellationToken.ThrowIfCancellationRequested();
			// The complete fallback proof checked every nullable prefix and this set.
			return true;
		}
		// Native prefix/start-bitmap scanning rejects CR before VM entry when
		// every initial mandatory positive class excludes it. Wider UTF-8 leaders
		// do not change ASCII CR/LF membership. Keep this independent
		// of source newline metadata and avoid recursive compile-time traversal.
		var pending = new Stack<PcreByteNode>();
		var visits = 1; // Root; conservatively include every proof node below.
		var canStartAtLf = false;
		if ( !AddBranches( root ) ) { return false; }
		while ( pending.TryPop( out var node ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			// Native start-bitmap analysis fails after 1,000 cumulative group
			// calls. Extra proof-node charges keep this bounded subset within it.
			if ( ++visits > MaximumStartClassVisits ) { return false; }
			switch ( node ) {
				case PcreByteGroup group:
					if ( !AddBranches( group.Body ) ) { return false; }
					break;
				case PcreByteNoncapturingGroup group:
					if ( !AddBranches( group.Body ) ) { return false; }
					break;
				case PcreByteAtomic atomic:
					pending.Push( atomic.Child );
					break;
				case PcreByteRepeat { Minimum: 1, Maximum: int.MaxValue, Child: PcreByteScalarSet repeatedSet }:
					if ( !ReferenceEquals( repeatedSet, utfCrLfLeadingRepeatSet ) ) {
						foreach ( var range in repeatedSet.Ranges.Span ) {
							cancellationToken.ThrowIfCancellationRequested();
							if ( range.End > 127 ) { return false; }
						}
					}
					pending.Push( repeatedSet );
					break;
				case PcreByteRepeat repeat when repeat.Minimum >= 1
					&& ( repeat.Minimum != repeat.Maximum || repeat.Minimum == 1 ):
					// Exact larger repeats can expose later native prefix tests.
					pending.Push( repeat.Child );
					break;
				case PcreByteScalarSet { Negated: false } set when set.Predicates.IsEmpty && !set.Ranges.IsEmpty:
					foreach ( var range in set.Ranges.Span ) {
						cancellationToken.ThrowIfCancellationRequested();
						if ( range.Start < 0 || range.End > 0x10FFFF
							|| range.Start <= '\r' && range.End >= '\r' ) { return false; }
						canStartAtLf |= range.Start <= '\n' && range.End >= '\n';
					}
					break;
				default: return false;
			}
		}
		// Do not spend additional VM work at LF if no initial class admits it.
		return canStartAtLf;

		bool AddBranches( PcreByteAlternatives alternatives ) {
			if ( alternatives.Branches.Count == 0 ) { return false; }
			foreach ( var branch in alternatives.Branches ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( branch.Count == 0 ) { return false; }
				// Only cross supported prefixes when the single-chain repeat selector
				// already established their source shape. Other proofs stay conservative.
				var first = utfCrLfLeadingRepeatSet is null ? branch[0]
					: FindUtfCrLfLeadingNode( branch, ref visits ).Node;
				if ( first is null ) { return false; }
				pending.Push( first );
			}
			return true;
		}
	}

	private (bool Unconditional, PcreBytePrefixLiteral? Literal) FindByteCrLfPrefix( PcreByteAlternatives root ) {
		// Native byte prefix scanning passes through ordinary/atomic groups and
		// exact Any repeats, but only examines twelve positions. After unrestricted
		// positions a sole final literal explains every failed attempt. Longer
		// restrictive NOT/PROP/XCLASS/Any prefixes require that literal's actual
		// rejection at CR: a passing literal followed by VM failure must skip LF.
		var pending = new Stack<PcreByteNode>();
		var visits = 1;
		var prefixLength = 0;
		var restricted = false;
		var sparseBitmapPrefix = false;
		PcreByteLiteral? literal = null;
		if ( !AddSequence( root ) ) { return default; }
		while ( pending.TryPop( out var node ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( ++visits > MaximumStartClassVisits ) { return default; }
			switch ( node ) {
				case PcreByteGroup group:
					if ( !AddSequence( group.Body ) ) { return default; }
					break;
				case PcreByteNoncapturingGroup group:
					if ( !AddSequence( group.Body ) ) { return default; }
					break;
				case PcreByteAtomic atomic: pending.Push( atomic.Child ); break;
				case PcreByteAny any:
					if ( !AddWidth( 1, any.DotAll ) ) { return default; }
					break;
				case PcreByteClass { PrefixAny: true }:
					if ( !AddWidth( 1, false ) ) { return default; }
					break;
				case PcreByteRepeat { Child: PcreByteAny repeatedAny } repeat
					when repeat.Minimum > 0 && repeat.Minimum == repeat.Maximum:
					if ( !AddWidth( repeat.Minimum, repeatedAny.DotAll ) ) { return default; }
					break;
				case PcreByteRepeat { Child: PcreByteClass repeatedClass } repeat
					when repeat.Minimum > 0 && repeat.Minimum == repeat.Maximum
						&& ( repeatedClass.PrefixKind is PcreByteClassPrefix.Exact or PcreByteClassPrefix.AnyProperty
							|| repeat.Minimum > 1 && repeatedClass.PrefixKind == PcreByteClassPrefix.Bitmap
								&& ( IsBroadBitmap( repeatedClass.Set )
									|| prefixLength == 0 && repeatedClass.Set.Contains( (byte)'\r' )
										&& repeatedClass.Set.Contains( (byte)'\n' ) )
							|| repeat.Minimum == 1 && repeatedClass.PrefixAny ):
					// Native removes an XCLASS count1; larger exact XCLASS repeats
					// stop at CRRANGE, unlike NOT/type/ALLANY repeats.
					// Broad bitmap CRRANGE also continues; its count1/atoms do not.
					var sparseBitmap = repeatedClass.PrefixKind == PcreByteClassPrefix.Bitmap
						&& !IsBroadBitmap( repeatedClass.Set );
					if ( !AddWidth( repeat.Minimum, false ) ) { return default; }
					// Sparse selection is proven only for one complete repeated head.
					sparseBitmapPrefix |= sparseBitmap;
					break;
				case PcreByteRepeat repeat
					when repeat.Minimum > 0 && repeat.Minimum == repeat.Maximum
						&& repeat.Minimum < MaximumBytePrefixLength
						&& ( repeat.Child is PcreByteGroup or PcreByteNoncapturingGroup or PcreByteAtomic ):
					// Native copies fixed groups. Walk every copy; do not turn its
					// contained XCLASS into a directly repeated XCLASS+CRRANGE.
					if ( !AddCopies( repeat.Child, repeat.Minimum ) ) { return default; }
					break;
				case PcreByteLiteral value:
					if ( prefixLength == 0 || literal is not null ) { return default; }
					literal = value;
					break;
				default: return default;
			}
		}
		if ( literal is null ) { return default; }
		if ( restricted && prefixLength > 1 ) {
			return (false, new PcreBytePrefixLiteral( prefixLength, literal.Value, literal.Caseless, literal.CaselessRestricted ));
		}
		return (true, null);

		bool AddSequence( PcreByteAlternatives body ) {
			if ( body.Branches.Count != 1 || body.Branches[0].Count == 0 ) { return false; }
			var branch = body.Branches[0];
			if ( branch.Count > MaximumStartClassVisits - visits - pending.Count ) { return false; }
			for ( var index = branch.Count - 1; index >= 0; index-- ) { pending.Push( branch[index] ); }
			return true;
		}
		bool AddCopies( PcreByteNode node, int count ) {
			if ( count > MaximumStartClassVisits - visits - pending.Count ) { return false; }
			for ( var copy = 0; copy < count; copy++ ) { pending.Push( node ); }
			return true;
		}
		bool AddWidth( int width, bool unrestricted ) {
			if ( sparseBitmapPrefix || literal is not null || width >= MaximumBytePrefixLength - prefixLength ) { return false; }
			prefixLength += width;
			restricted |= !unrestricted;
			return true;
		}
		// Native treats effective byteFF membership as broad even for small sets.
		static bool IsBroadBitmap( PcreByteSet set ) =>
			set.Contains( (byte)'\r' ) && set.Contains( (byte)'\n' )
				&& ( set.Contains( 0xFF ) || BitOperations.PopCount( set.A ) + BitOperations.PopCount( set.B )
					+ BitOperations.PopCount( set.C ) + BitOperations.PopCount( set.D ) >= 6 );
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
		PcreByteBackreference or PcreByteSubroutineCall => true,
		// A negative or empty literal assertion can accept without reading the
		// requested code unit, just like a nullable general assertion.
		PcreByteLiteralAssertion assertion => !assertion.IsPositive || assertion.Bytes.IsEmpty,
		PcreByteLookahead or PcreByteLookbehind => true,
		PcreByteGroup group => CanMatchEmptyAtAnyOffset( group.Body ),
		PcreByteNoncapturingGroup group => CanMatchEmptyAtAnyOffset( group.Body ),
		PcreByteCaptureConditional conditional => conditional.Body.Branches.Count == 1
			|| CanMatchEmptyAtAnyOffset( conditional.Body ),
		PcreByteRecursionConditional conditional => conditional.Body.Branches.Count == 1
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
		PcreByteScalarSet scalarSet => scalarSet.ExplicitNewline,
		PcreByteLiteralAssertion assertion => assertion.Bytes.Span.IndexOf( (byte)'\r' ) >= 0
			|| assertion.Bytes.Span.IndexOf( (byte)'\n' ) >= 0,
		PcreByteLookahead assertion => HasExplicitNewline( assertion.Body ),
		PcreByteLookbehind assertion => HasExplicitNewline( assertion.Body ),
		PcreByteGroup group => HasExplicitNewline( group.Body ),
		PcreByteNoncapturingGroup group => HasExplicitNewline( group.Body ),
		PcreByteCaptureConditional conditional => HasExplicitNewline( conditional.Body ),
		PcreByteRecursionConditional conditional => HasExplicitNewline( conditional.Body ),
		PcreByteStaticConditional conditional => HasExplicitNewline( conditional.Body ),
		PcreByteAssertionConditional conditional => HasExplicitNewline( conditional.Assertion )
			|| HasExplicitNewline( conditional.Body ),
		PcreByteAtomic atomic => HasExplicitNewline( atomic.Child ),
		PcreByteRepeat repeat => HasExplicitNewline( repeat.Child ),
		_ => false
	};

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
		bool caseless = false, bool caselessRestricted = false, ReadOnlyMemory<int> captureNumbers = default,
		bool skipCrLfAfterRepeat = false ) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( instructions.Count >= MaximumInstructions ) { throw RepeatExpansionLimit(); }
		var index = instructions.Count;
		instructions.Add( new( opcode, first, second, set, bytes, ranges, predicates, category, caseless, caselessRestricted,
			captureNumbers, skipCrLfAfterRepeat ) );
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

	private bool TakeUtfCrLfLeadingStarSkip( PcreByteNode node ) {
		if ( !skipUtfCrLfLeadingStar || !ReferenceEquals( node, utfCrLfLeadingStarAtom ) ) { return false; }
		// AST identity is shared by copied groups; only its first instruction owns recovery.
		skipUtfCrLfLeadingStar = false;
		return true;
	}

	private void CompileNode( PcreByteNode node ) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( ++compileVisits > MaximumInstructions ) { throw RepeatExpansionLimit(); }
		switch ( node ) {
			case PcreByteFailure:
				Emit( PcreByteOpcode.Fail );
				break;
			case PcreByteLiteral literal:
				Emit( PcreByteOpcode.Literal, literal.Value, caseless: literal.Caseless, caselessRestricted: literal.CaselessRestricted,
					skipCrLfAfterRepeat: TakeUtfCrLfLeadingStarSkip( literal ) );
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
					category: scalarClass.Category, caseless: scalarClass.Caseless, caselessRestricted: scalarClass.CaselessRestricted,
					skipCrLfAfterRepeat: TakeUtfCrLfLeadingStarSkip( scalarClass ) );
				break;
			case PcreByteScalarSet scalarSet:
				Emit( PcreByteOpcode.ScalarSet, second: scalarSet.Negated ? 1 : 0,
					ranges: scalarSet.Ranges, predicates: scalarSet.Predicates, caseless: scalarSet.Caseless,
					caselessRestricted: scalarSet.CaselessRestricted,
					skipCrLfAfterRepeat: TakeUtfCrLfLeadingStarSkip( scalarSet )
						|| skipUtfCrLfLeadingRepeat && ReferenceEquals( scalarSet, utfCrLfLeadingRepeatSet ) );
				break;
			case PcreByteClass characterClass:
				Emit( PcreByteOpcode.Class, set: characterClass.Set, caseless: characterClass.Caseless, caselessRestricted: characterClass.CaselessRestricted );
				break;
			case PcreByteBackreference reference:
				Emit( PcreByteOpcode.Backreference, reference.Number, caseless: reference.Caseless,
					caselessRestricted: reference.CaselessRestricted, captureNumbers: reference.DuplicateNumbers );
				break;
			case PcreByteSubroutineCall call:
				calls.Add( (Emit( PcreByteOpcode.Call, captureNumbers: call.ReturnedCaptures ), call.Number) );
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
				CompileConditional( conditional.Body, PcreByteOpcode.CaptureTest, conditional.CaptureNumbers );
				break;
			case PcreByteRecursionConditional conditional:
				CompileConditional( conditional.Body, PcreByteOpcode.RecursionTest, conditional.GroupNumbers );
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

	private void CompileConditional( PcreByteAlternatives body, PcreByteOpcode opcode, ReadOnlyMemory<int> targets ) {
		// A condition chooses once. Only alternatives inside its selected body
		// may save paths; never offer the other conditional branch as a retry.
		var test = Emit( opcode, captureNumbers: targets );
		var yes = instructions.Count;
		compileWork.Push( () => {
			var endJump = Emit( PcreByteOpcode.Jump );
			var no = instructions.Count;
			compileWork.Push( () => {
				Patch( test, yes, no );
				Patch( endJump, instructions.Count );
			} );
			if ( body.Branches.Count > 1 ) { CompileSequence( body.Branches[1], 0 ); }
		} );
		CompileSequence( body.Branches[0], 0 );
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
