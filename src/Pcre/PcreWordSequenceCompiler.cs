namespace Icod.Expressions.Pcre;

internal sealed record PcreWordSuffixChoices( PcreByteNode[][] Alternatives ) : PcreByteNode;

internal sealed record PcreWordPathMark( string Name ) : PcreByteNode;
internal sealed record PcreWordFailure( string? Mark = null ) : PcreByteNode;
internal sealed record PcreWordAccept( string? Mark = null ) : PcreByteNode;
internal enum PcreWordCutKind { Prune, Commit, Then, Skip }
internal sealed record PcreWordCut( PcreWordCutKind Kind, string? Mark = null, string? SkipName = null ) : PcreByteNode;
internal sealed record PcreWordPruneScan( bool Start ) : PcreByteNode;

internal sealed record PcreWordGroup(
	PcreByteNode[][] Alternatives, int PathCount, int ExpandedNodes, int UnboundedRepeats, int CaptureNumber = 0,
	int[]? SourceBranchStarts = null, bool[]? CheckAssertionEnds = null, bool IsolatesPathMarks = false
) : PcreByteNode;

internal enum PcreWordConditionKind { Capture, Recursion, Constant, Assertion }

internal sealed record PcreWordConditional(
	PcreWordGroup Group, PcreByteNode[] WhenTrue, PcreByteNode[] WhenFalse, PcreWordConditionKind Kind,
	ReadOnlyMemory<int> Numbers = default, bool Value = false, PcreWordAssertion? Assertion = null
) : PcreByteNode;

internal sealed record PcreWordGroupRepeat(
	PcreWordGroup Group, int Minimum, int Maximum, bool Greedy, bool CheckFinalRequiredProgress
) : PcreByteNode {
	internal PcreByteNode[] Body { get; } = [Group];
}

internal sealed record PcreWordAssertion(
	PcreWordGroup Group, bool IsPositive, bool IsBehind = false, bool NonAtomic = false, bool VariableBehind = false
) : PcreByteNode {
	internal PcreByteNode[] Body { get; } = [Group];
}

internal sealed record PcreWordAtomic( PcreWordGroup Group ) : PcreByteNode {
	internal PcreByteNode[] Body { get; } = [Group];
}

internal sealed record PcreWordScriptRun( PcreWordGroup Group, bool Atomic ) : PcreByteNode {
	internal PcreByteNode[] Body { get; } = Atomic ? [new PcreWordAtomic( Group )] : [Group];
}

internal sealed record PcreWordReverse( int Maximum, int Minimum ) : PcreByteNode;

/// <summary>Admits a bounded width-16 sequence through the existing PCRE grammar.</summary>
internal static class PcreWordSequenceCompiler {
	private const int MaximumExpandedNodes = 4096;
	private const int MaximumExactRepeat = 64;
	private const int MaximumUnboundedRepeats = 64;

	internal static PcreWordCompileResult CompileSequence( ReadOnlyMemory<char> pattern, PcreWordOptions? options = null,
		CancellationToken cancellationToken = default ) {
		cancellationToken.ThrowIfCancellationRequested();
		options ??= new();
		if ( options.MatchWorkLimit <= 0 ) { return Error( PcreWordDiagnosticCode.InvalidWorkLimit, 0 ); }
		if ( pattern.Length > 4096 || (uint)pattern.Length > options.MaxPatternLength ) {
			return Error( PcreWordDiagnosticCode.PatternLengthLimitExceeded, 0 );
		}
		var source = pattern.ToArray();
		// ASCII grammar tokens keep their original indices. Non-ASCII units are
		// placeholders read directly from the owned word source by the parser.
		var grammarSource = new byte[source.Length];
		for ( var index = 0; index < source.Length; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			grammarSource[index] = source[index] > 127 ? (byte)0x80 : (byte)source[index];
		}
		var grammarOptions = new PcreByteOptions {
			Utf8 = options.Utf16, Ucp = options.Ucp, Caseless = options.Caseless,
			AltExtendedClass = options.AltExtendedClass,
			CaselessRestricted = options.CaselessRestricted,
			AsciiBsD = options.AsciiBsD, AsciiBsS = options.AsciiBsS, AsciiBsW = options.AsciiBsW,
			AsciiPosix = options.AsciiPosix, AsciiDigit = options.AsciiDigit,
			AllowLookaroundBackslashK = options.AllowLookaroundBackslashK
		};
		var settings = PcreByteStartSettings.Parse( grammarSource, grammarOptions, cancellationToken, codeUnitWidth: 16 );
		// The ASCII prefix selects the mode of the complete owned source.
		// Preserve every original code-unit index, including the prefix itself.
		if ( options.Utf16 != settings.Utf8 || options.Ucp != settings.Ucp ) {
			options = options with { Utf16 = settings.Utf8, Ucp = settings.Ucp };
		}
		if ( options.Utf16 ) {
			for ( var offset = 0; offset < source.Length; ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( !PcreUtf16.TryRead( source, offset, true, out _, out var width ) ) {
					return Error( PcreWordDiagnosticCode.InvalidUtf16Pattern, offset );
				}
				offset += width;
			}
		}
		try {
			var parser = new PcreByteParser( grammarSource, grammarOptions, settings, cancellationToken,
				codeUnitWidth: 16, wordSource: source );
			var root = parser.Parse();
			var pairNewline = settings.Newline is PcreByteNewline.CrLf or PcreByteNewline.AnyCrLf or PcreByteNewline.Any;
			if ( root.Branches.Count is < 1 or > 64 ) {
				return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
			}
			var pruneScan = PcrePruneLiteralScan.Find( root, cancellationToken );
			var compiled = new Dictionary<PcreByteAlternatives, PcreWordGroup>( ReferenceEqualityComparer.Instance );
			var pending = new Stack<(PcreByteAlternatives Body, int Depth, bool Complete, PcreByteLookbehind? Behind)>();
			var validationLookbehind = 0;
			var hasAccept = false;
			var hasCommit = false;
			var hasSelectedReturns = false;
			var expressionTerms = 0;
			var expressionMembers = 0;
			var expressionStack = 0;
			pending.Push( (root, 0, false, null) );
			while ( pending.TryPop( out var item ) ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( item.Depth > 64 || item.Body.Branches.Count is < 1 or > 64 ) {
					return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
				}
				if ( !item.Complete ) {
					pending.Push( (item.Body, item.Depth, true, item.Behind) );
					foreach ( var path in item.Body.Branches ) {
						foreach ( var node in path ) {
							cancellationToken.ThrowIfCancellationRequested();
							var nested = node;
							while ( nested is PcreByteRepeat or PcreByteAtomic or PcreByteScriptRun ) {
								nested = nested switch { PcreByteRepeat repeat => repeat.Child, PcreByteAtomic atomic => atomic.Child, _ => ( (PcreByteScriptRun)nested ).Child };
							}
							if ( nested is PcreByteScalarExpression expression ) {
								if ( expression.Terms.Length > MaximumExpandedNodes - expressionTerms
									|| !IsSupportedExpression( expression, ref expressionMembers, cancellationToken ) ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, expression.OpeningOffset ); }
								expressionTerms += expression.Terms.Length;
								expressionStack = Math.Max( expressionStack, expression.StackDepth );
							}
							hasAccept |= nested is PcreByteAccept;
							hasCommit |= nested is PcreByteCommit;
							hasSelectedReturns |= nested is PcreByteSubroutineCall selected && !selected.ReturnedCaptures.IsEmpty;
							var nestedBody = nested switch {
								PcreByteGroup direct => direct.Body,
								PcreByteNoncapturingGroup direct => direct.Body,
								PcreByteLookahead assertion => assertion.Body,
								PcreByteLookbehind assertion => assertion.Body,
								PcreByteCaptureConditional condition => condition.Body,
								PcreByteRecursionConditional condition => condition.Body,
								PcreByteStaticConditional condition => condition.Body,
								PcreByteAssertionConditional condition => condition.Body,
								_ => null
							};
							if ( nestedBody is not null ) {
								pending.Push( (nestedBody, item.Depth + 1, false, nested as PcreByteLookbehind) );
							}
							if ( nested is PcreByteAssertionConditional assertionCondition ) {
								var assertionBody = assertionCondition.Assertion is PcreByteLookahead ahead
									? ahead.Body : ( (PcreByteLookbehind)assertionCondition.Assertion ).Body;
								pending.Push( (assertionBody, item.Depth + 2, false, assertionCondition.Assertion as PcreByteLookbehind) );
							}
						}
					}
					continue;
				}
				var body = BuildBody( item.Body, compiled, ref validationLookbehind, cancellationToken, item.Behind,
					pruneScan is { } scan && ReferenceEquals( item.Body, scan.Body ) ? scan.Repeat : null );
				if ( body is null ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
				compiled.Add( item.Body, body );
			}
			var targets = new Dictionary<int, PcreByteNode[]>();
			var cutTargets = new HashSet<int>();
			// The pinned interpreter reports INTERNAL (-44) when ACCEPT returns
			// selected captures. Keep this combination outside the bounded profile
			// until its cross-dispatch contract is qualified, including erased bodies.
			if ( hasAccept && hasSelectedReturns ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
			foreach ( var (number, target) in parser.SubroutineTargets ) {
				cancellationToken.ThrowIfCancellationRequested();
				// Integer call edges can refer to the root or a recursive definition.
				// Source bodies were compiled once; do not expand or traverse call edges.
				// A call executes the definition's body, not its enclosing capture close.
				targets.Add( number, [compiled[target.Body]] );
				if ( HasLexicalControl( target.Body, cancellationToken, localQuitOnly: true ) ) { cutTargets.Add( number ); }
			}
			var assertedOnly = false;
			var startLiteral = hasCommit ? FindCommitStart( root, compiled.Keys, options, cancellationToken, out assertedOnly ) : null;
			if ( hasCommit && assertedOnly ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
			// THEN needs the original root alternative owner inside the retry stack.
			var branches = parser.HasThen ? new PcreByteNode[][] { [compiled[root]] } : compiled[root].Alternatives;
			return new( new( branches, options, validationLookbehind, settings.Newline,
				settings.BsrAnyCrLf, parser.CreateCaptureInfo(), targets, parser.HasNonAtomicAccept,
				cutTargets, startLiteral, parser.HasThen, parser.HasNamedSkip,
				parser.HasNamedSkip && PcreSourceStartAnchor.HasAlternativeStartAnchor( root, cancellationToken ),
				pairNewline && PcreByteProgramCompiler.HasExplicitNewline( root ),
				pairNewline && PcreWordStartStudy.HasLfStartClass( root, compiled.Keys, cancellationToken ),
				pairNewline ? PcreWordStartStudy.FindFixedPrefix( root, options.Utf16, cancellationToken ) : null, expressionStack ), null );
		} catch ( PcreByteParseException error ) {
			return Error( error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax
				? PcreWordDiagnosticCode.UnsupportedSyntax : PcreWordDiagnosticCode.InvalidSyntax, error.Diagnostic.ByteOffset );
		}
	}

	private static PcreWordGroup? BuildBody( PcreByteAlternatives root,
		Dictionary<PcreByteAlternatives, PcreWordGroup> compiled, ref int validationLookbehind,
		CancellationToken cancellationToken, PcreByteLookbehind? behind = null, PcreByteRepeat? scanRepeat = null ) {
		var branches = new List<PcreByteNode[]>();
		var maximumNodes = 0;
		var maximumRepeats = 0;
		var admittedPaths = 0;
		var sourceBranchStarts = new int[root.Branches.Count + 1];
		for ( var branch = 0; branch < root.Branches.Count; branch++ ) {
			sourceBranchStarts[branch] = branches.Count;
			foreach ( var node in root.Branches[branch] ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( AssertionRepeat( node ) is { } repeat
					&& ( repeat.Minimum is < 0 or > MaximumExactRepeat || repeat.Maximum < repeat.Minimum
						|| repeat.Maximum > MaximumExactRepeat ) ) { return null; }
			}
			var nodes = new PcreByteNode[root.Branches[branch].Count];
			for ( var original = 0; original < nodes.Length; original++ ) {
				var node = root.Branches[branch][original];
				cancellationToken.ThrowIfCancellationRequested();
				if ( node is PcreByteAtomic atomic && AssertionRepeat( atomic ) is null ) {
					// Source groups are already compiled in postorder. A possessive
					// repeat needs one synthetic branch through the ordinary lowering;
					// this does not recursively walk nested source group bodies.
					var atomicBody = atomic.Child is PcreByteNoncapturingGroup group ? compiled[group.Body]
						: atomic.Child is PcreByteRepeat ? BuildBody( new( [new[] { atomic.Child }] ),
							compiled, ref validationLookbehind, cancellationToken ) : null;
					if ( atomicBody is null ) { return null; }
					nodes[original] = new PcreWordAtomic( atomicBody );
					continue;
				}
				var runSource = node is PcreByteRepeat runRepeat ? runRepeat.Child : node;
				if ( runSource is PcreByteScriptRun run ) {
					var atomicRun = run.Child is PcreByteAtomic;
					var child = atomicRun ? ( (PcreByteAtomic)run.Child ).Child : run.Child;
					if ( child is not PcreByteNoncapturingGroup runGroup ) { return null; }
					var loweredRun = new PcreWordScriptRun( compiled[runGroup.Body], atomicRun );
					nodes[original] = node is PcreByteRepeat countedRun
						? new PcreWordGroupRepeat( new( [[loweredRun]], loweredRun.Group.PathCount,
							loweredRun.Group.ExpandedNodes + ( atomicRun ? 3 : 2 ), loweredRun.Group.UnboundedRepeats ),
							countedRun.Minimum, countedRun.Maximum, countedRun.Greedy, true ) : loweredRun;
					continue;
				}
				if ( node is PcreByteStaticConditional { IsDefinition: true } definition ) {
					nodes[original] = new PcreWordGroupRepeat( compiled[definition.Body], 0, 0, true, false );
					continue;
				}
				var conditionalSource = node is PcreByteRepeat conditionalRepeat ? conditionalRepeat.Child : node;
				if ( ConditionalBody( conditionalSource ) is { } conditionalBody ) {
					var conditional = BuildConditional( conditionalSource, compiled[conditionalBody], compiled );
					if ( conditional is null ) { return null; }
					nodes[original] = node is PcreByteRepeat conditionCount
						? new PcreWordGroupRepeat( new( [[conditional]], conditional.Group.PathCount,
							conditional.Group.ExpandedNodes + 1, conditional.Group.UnboundedRepeats ),
							conditionCount.Minimum, conditionCount.Maximum, conditionCount.Greedy, true ) : conditional;
					continue;
				}
				nodes[original] = node switch {
					PcreBytePrune prune => new PcreWordCut( PcreWordCutKind.Prune, CutMark( prune.WordName ) ),
					PcreByteCommit commit => new PcreWordCut( PcreWordCutKind.Commit, CutMark( commit.WordName ) ),
					PcreByteThen then => new PcreWordCut( PcreWordCutKind.Then, CutMark( then.WordName ) ),
					PcreByteSkip => new PcreWordCut( PcreWordCutKind.Skip ),
					PcreByteNamedSkip skip => new PcreWordCut( PcreWordCutKind.Skip, SkipName: VisibleMark( skip.WordName! ) ),
					PcreByteAccept accept => BuildAccept( accept ),
					PcreByteRepeat { Child: PcreByteAccept accept } repeat => new PcreWordGroupRepeat(
						new( [[BuildAccept( accept )]], 1, 1, 0 ), repeat.Minimum, repeat.Maximum, repeat.Greedy, true ),
					PcreBytePathMark mark => new PcreWordPathMark( VisibleMark( mark.WordName! ) ),
					PcreByteFailure failure => new PcreWordFailure(
						failure.WordName is { Length: > 0 } name ? VisibleMark( name ) : null ),
					PcreByteNode repeated when AssertionRepeat( repeated ) is { } repeat =>
						BuildAssertionRepeat( repeat, compiled, repeated is PcreByteAtomic ),
					PcreByteGroup group => compiled[group.Body] with { CaptureNumber = group.Number },
					PcreByteNoncapturingGroup group => compiled[group.Body],
					PcreByteLookahead or PcreByteLookbehind => BuildAssertion( node, compiled ),
					PcreByteRepeat { Child: PcreByteGroup group } repeat => new PcreWordGroupRepeat(
						compiled[group.Body] with { CaptureNumber = group.Number }, repeat.Minimum, repeat.Maximum, repeat.Greedy,
						true ),
					PcreByteRepeat { Child: PcreByteNoncapturingGroup group } repeat => new PcreWordGroupRepeat(
						compiled[group.Body], repeat.Minimum, repeat.Maximum, repeat.Greedy,
						true ),
					PcreByteRepeat { Child: PcreByteAtomic { Child: PcreByteNoncapturingGroup group } } repeat =>
						new PcreWordGroupRepeat( AtomicIteration( compiled[group.Body] ),
							repeat.Minimum, repeat.Maximum, repeat.Greedy, true ),
					// References consume whole captured strings, possibly empty. Reuse the
					// variable-width group loop instead of scalar-atom expansion/rewinding.
					PcreByteRepeat { Child: PcreByteBackreference reference } repeat => new PcreWordGroupRepeat(
						new( [[reference]], 1, 1, 0 ), repeat.Minimum, repeat.Maximum, repeat.Greedy,
						repeat.Offset < 0 && repeat.Minimum == 1 ),
					PcreByteRepeat { Child: PcreByteSubroutineCall call } repeat => new PcreWordGroupRepeat(
						new( [[call]], 1, 1, 0 ), repeat.Minimum, repeat.Maximum, repeat.Greedy, repeat.Minimum == 1 ),
					_ => node
				};
			}
			if ( scanRepeat is not null && branch == 0 ) {
				var scanIndex = Array.IndexOf( nodes, scanRepeat );
				nodes = [.. nodes[..scanIndex], new PcreWordPruneScan( true ), scanRepeat,
					new PcreWordPruneScan( false ), .. nodes[( scanIndex + 1 )..]];
			}
			if ( behind is not null ) {
				if ( behind.BranchLengths.Length != root.Branches.Count ) { return null; }
				var maximum = behind.BranchLengths.Span[branch];
				validationLookbehind = Math.Max( validationLookbehind, maximum );
				// Each original branch owns its rewind, before any finite body choices.
				nodes = [new PcreWordReverse( maximum, behind.Minimum < 0 ? maximum : behind.Minimum ), .. nodes];
			}
			var nestedPaths = 1;
			var nestedRepeats = 0;
			foreach ( var node in nodes ) {
				cancellationToken.ThrowIfCancellationRequested();
				var group = node as PcreWordGroup;
				if ( node is PcreWordAssertion assertion ) { group = assertion.Group; }
				if ( node is PcreWordAtomic atomic ) { group = atomic.Group; }
				if ( node is PcreWordScriptRun run ) { group = run.Group; }
				if ( node is PcreWordConditional conditional ) { group = conditional.Group; }
				if ( node is PcreWordGroupRepeat repeat ) {
					if ( repeat.Minimum is < 0 or > MaximumExactRepeat || repeat.Maximum < repeat.Minimum
						|| repeat.Maximum != int.MaxValue && repeat.Maximum > MaximumExactRepeat ) { return null; }
					group = repeat.Group;
					if ( repeat.Maximum == int.MaxValue ) { nestedRepeats++; }
				}
				if ( group is null ) { continue; }
				if ( group.PathCount > 64 / nestedPaths ) { return null; }
				nestedPaths *= group.PathCount;
				nestedRepeats += group.UnboundedRepeats;
			}
			var unboundedCount = nodes.Count( node => node is PcreByteRepeat { Maximum: int.MaxValue } );
			if ( unboundedCount + nestedRepeats > MaximumUnboundedRepeats ) { return null; }
			maximumRepeats = Math.Max( maximumRepeats, unboundedCount + nestedRepeats );
			// A group also separates finite choices: its alternatives must retry before
			// a preceding choice, and a following choice must retry before the group.
			var boundaryCount = nodes.Count( node => node is PcreByteRepeat repeat && IsRuntimeRepeat( repeat ) )
				+ nodes.Count( IsContinuationBoundary );
			var variables = new List<(int Index, PcreByteRepeat Repeat)>[boundaryCount + 1];
			var products = new int[variables.Length];
			for ( var segment = 0; segment < variables.Length; segment++ ) {
				variables[segment] = new();
				products[segment] = 1;
			}
			var boundaries = new List<PcreByteNode>();
			for ( var index = 0; index < nodes.Length; index++ ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( nodes[index] is PcreByteWordBoundary or PcreByteAnchor { IsStart: true, IsLineBoundary: false } ) {
					validationLookbehind = Math.Max( validationLookbehind, 1 );
				}
				if ( IsContinuationBoundary( nodes[index] ) ) { boundaries.Add( nodes[index] ); continue; }
				if ( nodes[index] is not PcreByteRepeat repeat || repeat.Minimum == repeat.Maximum ) { continue; }
				if ( IsRuntimeRepeat( repeat ) ) {
					if ( repeat.Minimum is < 0 or > MaximumExactRepeat
						|| repeat.Maximum < repeat.Minimum || repeat.Maximum != int.MaxValue && repeat.Maximum > MaximumExactRepeat
						|| !IsSupportedAtom( repeat.Child ) ) {
						return null;
					}
					boundaries.Add( repeat );
					continue;
				}
				if ( repeat.Minimum < 0 || repeat.Maximum > MaximumExactRepeat
					|| !IsSupportedAtom( repeat.Child ) ) { return null; }
				var range = repeat.Maximum - repeat.Minimum + 1;
				var segment = boundaries.Count;
				if ( range > 64 / products[segment] ) { return null; }
				products[segment] *= range;
				variables[segment].Add( (index, repeat) );
			}
			var depth = boundaries.Count;
			var pathCount = 1;
			for ( var segment = 0; segment <= depth; segment++ ) {
				if ( products[segment] > ( 64 - admittedPaths ) / pathCount ) {
					return null;
				}
				pathCount *= products[segment];
			}
			if ( nestedPaths > ( 64 - admittedPaths ) / pathCount ) { return null; }
			admittedPaths += pathCount * nestedPaths;
			var counts = new int[nodes.Length];
			Array.Fill( counts, -1 );
			var paths = new PcreByteNode[pathCount][];
			var positions = new int[pathCount][];
			for ( var ordinal = 0; ordinal < pathCount; ordinal++ ) {
				var remainder = ordinal;
				for ( var segment = depth; segment >= 0; segment-- ) {
					SelectCounts( variables[segment], remainder % products[segment], counts );
					remainder /= products[segment];
				}
				if ( !TryExpandBranch( nodes, counts, cancellationToken, out var expanded, out var expandedNodes ) ) {
					return null;
				}
				maximumNodes = Math.Max( maximumNodes, expandedNodes );
				paths[ordinal] = expanded;
				positions[ordinal] = new int[depth];
				for ( var boundary = 0; boundary < depth; boundary++ ) {
					var position = Array.FindIndex( expanded, node => ReferenceEquals( node, boundaries[boundary] ) );
					if ( position < ( boundary == 0 ? 0 : positions[ordinal][boundary - 1] + 1 ) ) {
						throw new InvalidOperationException( "Missing ordered word continuation boundaries." );
					}
					positions[ordinal][boundary] = position;
				}
			}
			if ( depth == 0 ) { branches.AddRange( paths ); continue; }
			var outerStride = pathCount / products[0];
			for ( var choice = 0; choice < products[0]; choice++ ) {
				var firstPath = choice * outerStride;
				var prefix = paths[firstPath][..( positions[firstPath][0] + 1 )];
				var continuation = BuildContinuation( paths, positions, products, depth,
					firstPath, outerStride, cancellationToken );
				branches.Add( [.. prefix, .. continuation] );
			}
		}
		sourceBranchStarts[^1] = branches.Count;
		bool[]? checkAssertionEnds = null;
		if ( behind is { Minimum: >= 0 } ) {
			checkAssertionEnds = new bool[branches.Count];
			for ( var branch = 0; branch < root.Branches.Count; branch++ ) {
				// Native checks the endpoint only for an actual ranged rewind.
				// Fixed branches trust their declared width, even with an omitted else.
				if ( behind.BranchLengths.Span[branch] > behind.Minimum ) {
					Array.Fill( checkAssertionEnds, true, sourceBranchStarts[branch], sourceBranchStarts[branch + 1] - sourceBranchStarts[branch] );
				}
			}
		}
		return new( branches.ToArray(), admittedPaths, maximumNodes, maximumRepeats,
			SourceBranchStarts: sourceBranchStarts, CheckAssertionEnds: checkAssertionEnds,
			IsolatesPathMarks: HasLexicalControl( root, cancellationToken, localQuitOnly: false ) );
	}

	private static bool IsRuntimeRepeat( PcreByteRepeat repeat ) => repeat.Maximum == int.MaxValue
		|| repeat.Child is PcreByteGrapheme && repeat.Minimum != repeat.Maximum;

	// A label write must execute once before finite suffix choices. Replaying it
	// on every expanded branch would erase a later accepted callee's committed label.
	private static bool IsContinuationBoundary( PcreByteNode node ) => node is PcreWordPruneScan or PcreWordCut or PcreWordPathMark or PcreWordGroup or PcreWordGroupRepeat
		or PcreWordAssertion or PcreWordAtomic or PcreWordScriptRun or PcreWordReverse or PcreByteSubroutineCall or PcreWordConditional;

	private static string VisibleMark( string name ) {
		var nul = name.IndexOf( '\0' );
		return nul < 0 ? name : name[..nul];
	}

	private static string? CutMark( string? name ) => name is { Length: > 0 } ? VisibleMark( name ) : null;

	private static bool HasLexicalControl( PcreByteAlternatives body, CancellationToken cancellationToken, bool localQuitOnly ) {
		var pending = new Stack<PcreByteNode>();
		AddBody( body );
		while ( pending.TryPop( out var node ) ) {
			cancellationToken.ThrowIfCancellationRequested();
			switch ( node ) {
				case PcreByteThen: return true;
				case PcreBytePathMark when !localQuitOnly: return true;
				case PcreBytePrune prune when localQuitOnly || !prune.Name.IsEmpty: return true;
				case PcreByteCommit commit when localQuitOnly || !commit.Name.IsEmpty: return true;
				case PcreByteSkip or PcreByteNamedSkip when localQuitOnly: return true;
				case PcreByteAccept accept when !localQuitOnly && !accept.Name.IsEmpty: return true;
				case PcreByteFailure failure when !localQuitOnly && !failure.Name.IsEmpty: return true;
				case PcreByteGroup group: AddBody( group.Body ); break;
				case PcreByteNoncapturingGroup group: AddBody( group.Body ); break;
				case PcreByteLookahead assertion: AddBody( assertion.Body ); break;
				case PcreByteLookbehind assertion: AddBody( assertion.Body ); break;
				case PcreByteAssertionConditional conditional:
					AddBody( conditional.Body ); pending.Push( conditional.Assertion ); break;
				case PcreByteAtomic atomic: pending.Push( atomic.Child ); break;
				case PcreByteScriptRun run: pending.Push( run.Child ); break;
				case PcreByteRepeat repeat: pending.Push( repeat.Child ); break;
				default:
					if ( ConditionalBody( node ) is { } conditionalBody ) { AddBody( conditionalBody ); }
					break;
			}
		}
		return false;
		void AddBody( PcreByteAlternatives alternatives ) {
			foreach ( var branch in alternatives.Branches ) {
				foreach ( var node in branch ) { cancellationToken.ThrowIfCancellationRequested(); pending.Push( node ); }
			}
		}
	}

	private static PcreByteLiteral? FindCommitStart( PcreByteAlternatives root, IEnumerable<PcreByteAlternatives> sourceBodies,
		PcreWordOptions options,
		CancellationToken cancellationToken, out bool assertedOnly ) {
		assertedOnly = false;
		// Source bodies arrive in postorder. Distinguish an empty prefix from a
		// consuming/nullable prefix whose first unit is unknown. Source alternatives
		// must agree before their first literal can skip a candidate ahead of COMMIT.
		var firstUnits = new Dictionary<PcreByteAlternatives, (PcreByteLiteral? Literal, bool Empty)>( ReferenceEqualityComparer.Instance );
		foreach ( var body in sourceBodies ) {
			(PcreByteLiteral? Literal, bool Empty)? common = null;
			foreach ( var branch in body.Branches ) {
				(PcreByteLiteral? Literal, bool Empty) first = (null, true);
				foreach ( var node in branch ) {
					cancellationToken.ThrowIfCancellationRequested();
					first = FirstNode( node );
					if ( !first.Empty ) { break; }
				}
				if ( common.HasValue && ( common.Value.Empty != first.Empty
					|| !SameFirstUnit( common.Value.Literal, first.Literal ) ) ) { common = (null, false); break; }
				common = first;
			}
			firstUnits.Add( body, common ?? (null, true) );
		}
		if ( firstUnits[root].Literal is { } consumingFirst ) { return consumingFirst; }
		// Asserted-only study also depends on native required-unit and start-bitmap
		// optimizations. Keep that separate qualification gate explicit rather than
		// choosing a candidate at which COMMIT would not actually run natively.
		var assertedUnits = new Dictionary<PcreByteAlternatives, (PcreByteLiteral? Outside, PcreByteLiteral? Inside)>( ReferenceEqualityComparer.Instance );
		foreach ( var body in sourceBodies ) {
			assertedUnits.Add( body, (AssertedBody( body, false ), AssertedBody( body, true )) );
		}
		assertedOnly = assertedUnits[root].Outside is not null;
		return null;

		PcreByteLiteral? AssertedBody( PcreByteAlternatives body, bool inside ) {
			PcreByteLiteral? common = null;
			foreach ( var branch in body.Branches ) {
				PcreByteLiteral? first = null;
				foreach ( var source in branch ) {
					cancellationToken.ThrowIfCancellationRequested();
					var node = source;
					if ( node is PcreBytePathMark or PcreByteWordBoundary or PcreByteLookbehind
						or PcreByteLookahead { IsPositive: false } or PcreByteStaticConditional { IsDefinition: true }
						or PcreByteRepeat { Maximum: 0 } or PcreByteCommit { WordName.Length: > 0 }
						or PcreBytePrune { WordName.Length: > 0 } or PcreByteThen { WordName.Length: > 0 } ) { continue; }
					while ( node is PcreByteAtomic or PcreByteRepeat { Minimum: > 0 } or PcreByteScriptRun ) {
						cancellationToken.ThrowIfCancellationRequested();
						node = node switch { PcreByteAtomic atomic => atomic.Child, PcreByteRepeat repeat => repeat.Child, _ => ( (PcreByteScriptRun)node ).Child };
					}
					var nested = node switch {
						PcreByteGroup group => group.Body,
						PcreByteNoncapturingGroup group => group.Body,
						PcreByteLookahead { IsPositive: true } assertion => assertion.Body,
						_ => null
					};
					if ( nested is not null ) {
						var entry = assertedUnits[nested];
						first = inside || node is PcreByteLookahead ? entry.Inside : entry.Outside;
					} else if ( inside ) { first = FirstNode( node ).Literal; }
					break;
				}
				if ( first is null || common is not null && !SameFirstUnit( common, first ) ) { return null; }
				common = first;
			}
			return common;
		}

		// Native first-unit flags carry caselessness, not lexical r. Both literals
		// have already passed the multi-case-set gate, so r cannot change this pair.
		static bool SameFirstUnit( PcreByteLiteral? left, PcreByteLiteral? right ) => left is null ? right is null
			: right is not null && left.Value == right.Value && left.Caseless == right.Caseless;

		PcreByteLiteral? UnitLiteral( PcreByteLiteral literal ) {
			if ( literal.Caseless && ( options.Utf16 || options.Ucp )
				&& ( options.Utf16 && literal.Value > 0xffff
					|| PcreUtf8CaseFold.HasMultipleOtherCases( literal.Value, literal.CaselessRestricted, cancellationToken ) ) ) { return null; }
			return literal with { Value = literal.Value > 0xffff ? 0xd800 + ( ( literal.Value - 0x10000 ) >> 10 ) : literal.Value };
		}
		(PcreByteLiteral? Literal, bool Empty) FirstNode( PcreByteNode node ) {
			var optional = false;
			while ( node is PcreByteRepeat or PcreByteAtomic or PcreByteScriptRun ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( node is PcreByteRepeat repeat ) { optional |= repeat.Minimum == 0; node = repeat.Child; }
				else { node = node is PcreByteAtomic atomic ? atomic.Child : ( (PcreByteScriptRun)node ).Child; }
			}
			(PcreByteLiteral? Literal, bool Empty) result;
			switch ( node ) {
				case PcreByteLiteral literal: result = (UnitLiteral( literal ), false); break;
				case PcreByteGroup group: result = firstUnits[group.Body]; break;
				case PcreByteNoncapturingGroup group: result = firstUnits[group.Body]; break;
				case PcreBytePathMark or PcreBytePrune or PcreByteCommit or PcreByteThen or PcreByteSkip or PcreByteNamedSkip or PcreByteFailure
					or PcreByteLookahead or PcreByteLookbehind or PcreByteAnchor or PcreByteSearchStart or PcreByteMatchStartReset
					or PcreByteEndBeforeFinalLf or PcreByteWordBoundary or PcreByteStaticConditional { IsDefinition: true }:
					result = (null, true); break;
				case PcreByteScalarSet { Negated: false, Predicates.IsEmpty: true } set:
					var value = set.SourceLiteral;
					var caseless = set.Caseless;
					if ( value < 0 && set.Ranges.Length is 1 or 2 && set.Ranges.Span[0] is var range && range.Start == range.End ) {
						value = range.Start;
						if ( set.Ranges.Length == 2 ) {
							var second = set.Ranges.Span[1];
							var partners = options.Utf16 || options.Ucp
								? PcreUtf8CaseFold.Equals( value, second.Start, set.CaselessRestricted )
								: value is >= 'A' and <= 'Z' && second.Start == value + 32;
							if ( second.Start != second.End || !partners ) { value = -1; }
							else { caseless = true; }
						}
					}
					result = (value < 0 ? null : UnitLiteral( new( value ) { Caseless = caseless, CaselessRestricted = set.CaselessRestricted } ), false);
					break;
				default:
					var conditional = ConditionalBody( node );
					result = conditional is { Branches.Count: 2 } ? firstUnits[conditional] : (null, false);
					break;
			}
			return optional && !result.Empty ? (null, false) : result;
		}

	}

	private static PcreByteAlternatives? ConditionalBody( PcreByteNode node ) => node switch {
		PcreByteCaptureConditional conditional => conditional.Body,
		PcreByteRecursionConditional conditional => conditional.Body,
		PcreByteStaticConditional conditional => conditional.Body,
		PcreByteAssertionConditional conditional => conditional.Body,
		_ => null
	};

	private static PcreWordConditional? BuildConditional( PcreByteNode node, PcreWordGroup body,
		Dictionary<PcreByteAlternatives, PcreWordGroup> compiled ) {
		// Keep original arm boundaries after finite choices expand into alternatives.
		// A failed selected arm must never fall through into the other arm.
		PcreByteNode[] Arm( int branch ) {
			var starts = body.SourceBranchStarts!;
			if ( branch + 1 >= starts.Length ) { return []; }
			var choices = body.Alternatives[starts[branch]..starts[branch + 1]];
			return choices.Length == 1 ? choices[0] : [new PcreWordSuffixChoices( choices )];
		}
		var whenTrue = Arm( 0 );
		var whenFalse = Arm( 1 );
		if ( node is PcreByteCaptureConditional capture ) {
			return new( body, whenTrue, whenFalse, PcreWordConditionKind.Capture, capture.CaptureNumbers );
		}
		if ( node is PcreByteRecursionConditional recursion ) {
			return new( body, whenTrue, whenFalse, PcreWordConditionKind.Recursion, recursion.GroupNumbers );
		}
		if ( node is PcreByteStaticConditional constant ) {
			return new( body, whenTrue, whenFalse, PcreWordConditionKind.Constant, Value: constant.Value );
		}
		var assertion = BuildAssertion( ( (PcreByteAssertionConditional)node ).Assertion, compiled );
		if ( assertion.Group.PathCount > 64 / body.PathCount ) { return null; }
		var metadata = body with {
			PathCount = body.PathCount * assertion.Group.PathCount,
			ExpandedNodes = body.ExpandedNodes + assertion.Group.ExpandedNodes + 2,
			UnboundedRepeats = body.UnboundedRepeats + assertion.Group.UnboundedRepeats
		};
		return new( metadata, whenTrue, whenFalse, PcreWordConditionKind.Assertion, Assertion: assertion );
	}

	private static PcreByteRepeat? AssertionRepeat( PcreByteNode node ) => node switch {
		PcreByteRepeat { Child: PcreByteLookahead or PcreByteLookbehind } repeat => repeat,
		PcreByteAtomic { Child: PcreByteRepeat { Child: PcreByteLookahead or PcreByteLookbehind } repeat } => repeat,
		_ => null
	};

	private static PcreWordGroup AtomicIteration( PcreWordGroup body ) =>
		new( [[new PcreWordAtomic( body )]], body.PathCount, body.ExpandedNodes + 2, body.UnboundedRepeats );

	private static PcreWordAssertion BuildAssertion( PcreByteNode node,
		Dictionary<PcreByteAlternatives, PcreWordGroup> compiled ) => node switch {
		PcreByteLookahead assertion => new( compiled[assertion.Body], assertion.IsPositive, NonAtomic: assertion.NonAtomic ),
		PcreByteLookbehind assertion => new( compiled[assertion.Body], assertion.IsPositive,
			true, assertion.NonAtomic, assertion.Minimum >= 0 ),
		_ => throw new InvalidOperationException( "Expected a word assertion." )
	};

	private static PcreByteNode BuildAssertionRepeat( PcreByteRepeat repeat,
		Dictionary<PcreByteAlternatives, PcreWordGroup> compiled, bool possessive ) {
		var assertion = BuildAssertion( repeat.Child, compiled );
		var body = new PcreWordGroup( [[assertion]], assertion.Group.PathCount,
			assertion.Group.ExpandedNodes + 2, assertion.Group.UnboundedRepeats );
		// The parser normalizes unlimited direct assertions to minimum + one.
		// Keep every finite iteration; captures can change without consuming input.
		var repeated = new PcreWordGroupRepeat( body, repeat.Minimum, repeat.Maximum, repeat.Greedy, false );
		if ( !possessive ) { return repeated; }
		// This entire repeat is zero-width. An atomic positive guard can commit
		// its choices while preserving the established direct-assertion work path.
		return new PcreWordAssertion( new( [[repeated]], body.PathCount,
			body.ExpandedNodes + 2, body.UnboundedRepeats ), true );
	}

	private static PcreByteNode[] BuildContinuation( PcreByteNode[][] paths, int[][] positions, int[] products,
		int depth, int firstPath, int groupSize, CancellationToken cancellationToken ) {
		var tails = new PcreByteNode[groupSize][];
		var stride = 1;
		var nextChoiceSegment = depth + 1;
		for ( var segment = depth; segment > 0; segment-- ) {
			cancellationToken.ThrowIfCancellationRequested();
			// Flatten deterministic sections once; only choice levels need tails.
			if ( products[segment] == 1 && segment > 1 ) { continue; }
			var width = stride * products[segment];
			for ( var group = 0; group < groupSize; group += width ) {
				var alternatives = new PcreByteNode[products[segment]][];
				for ( var choice = 0; choice < alternatives.Length; choice++ ) {
					cancellationToken.ThrowIfCancellationRequested();
					var child = group + choice * stride;
					var path = firstPath + child;
					var firstNode = positions[path][segment - 1] + 1;
					alternatives[choice] = nextChoiceSegment > depth ? paths[path][firstNode..]
						: [.. paths[path][firstNode..( positions[path][nextChoiceSegment - 1] + 1 )], .. tails[child]];
				}
				tails[group] = alternatives.Length > 1 ? [new PcreWordSuffixChoices( alternatives )] : alternatives[0];
			}
			stride = width;
			nextChoiceSegment = segment;
		}
		return tails[0];
	}

	private static void SelectCounts( IReadOnlyList<(int Index, PcreByteRepeat Repeat)> variables,
		int choice, int[] counts ) {
		for ( var index = variables.Count - 1; index >= 0; index-- ) {
			var (nodeIndex, repeat) = variables[index];
			var range = repeat.Maximum - repeat.Minimum + 1;
			var digit = choice % range;
			choice /= range;
			counts[nodeIndex] = repeat.Greedy ? repeat.Maximum - digit : repeat.Minimum + digit;
		}
	}

	private static bool TryExpandBranch( IReadOnlyList<PcreByteNode> nodes, int[] counts,
		CancellationToken cancellationToken, out PcreByteNode[] expanded, out int expandedNodes ) {
		expandedNodes = 0;
		var output = new List<PcreByteNode>();
		for ( var index = 0; index < nodes.Count; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			var node = nodes[index];
			if ( node is PcreByteRepeat repeat ) {
				if ( IsRuntimeRepeat( repeat )
					&& repeat.Minimum is >= 0 and <= MaximumExactRepeat
					&& ( repeat.Maximum == int.MaxValue || repeat.Maximum <= MaximumExactRepeat ) && IsSupportedAtom( repeat.Child )
					&& expandedNodes < MaximumExpandedNodes ) {
					output.Add( repeat );
					expandedNodes++;
					continue;
				}
				var count = counts[index] >= 0 ? counts[index] : repeat.Maximum;
				if ( counts[index] < 0 && repeat.Minimum != repeat.Maximum
					|| count is < 0 or > MaximumExactRepeat || !IsSupportedAtom( repeat.Child )
					|| expandedNodes > MaximumExpandedNodes - count ) {
					expanded = []; return false;
				}
				expandedNodes += count;
				for ( var copy = 0; copy < count; copy++ ) {
					cancellationToken.ThrowIfCancellationRequested();
					output.Add( repeat.Child );
				}
			} else if ( IsSupportedNode( node ) && expandedNodes < MaximumExpandedNodes ) {
				var weight = node switch {
					PcreWordGroup group => group.ExpandedNodes + 1,
					PcreWordGroupRepeat groupRepeat => groupRepeat.Group.ExpandedNodes + 2,
					PcreWordAssertion assertion => assertion.Group.ExpandedNodes + 2,
					PcreWordAtomic atomic => atomic.Group.ExpandedNodes + 2,
					PcreWordScriptRun run => run.Group.ExpandedNodes + ( run.Atomic ? 3 : 2 ),
					PcreWordConditional conditional => conditional.Group.ExpandedNodes + 1,
					_ => 1
				};
				if ( weight > MaximumExpandedNodes - expandedNodes ) { expanded = []; return false; }
				expandedNodes += weight;
				output.Add( node );
			} else { expanded = []; return false; }
		}
		expanded = output.ToArray();
		return true;
	}

	private static bool IsSupportedNode( PcreByteNode node ) => IsSupportedAtom( node )
		|| node is PcreWordPathMark or PcreWordFailure or PcreWordAccept or PcreWordCut or PcreWordPruneScan
		|| node is PcreByteAnchor or PcreByteLineAnchor or PcreByteEndBeforeFinalLf
			or PcreByteSearchStart or PcreByteMatchStartReset or PcreByteWordBoundary or PcreByteBackreference
			or PcreWordGroup or PcreWordGroupRepeat or PcreWordAssertion or PcreWordAtomic or PcreWordScriptRun or PcreWordReverse or PcreByteSubroutineCall or PcreWordConditional;

	private static PcreWordAccept BuildAccept( PcreByteAccept accept ) =>
		new( accept.WordName is { Length: > 0 } name ? VisibleMark( name ) : null );

	private static bool IsSupportedAtom( PcreByteNode node ) => node is PcreByteLiteral or PcreByteAny or PcreByteNewlineSequence or PcreByteGrapheme or PcreByteScalarExpression
		|| node is PcreByteScalarClass property && IsWordClassPredicate( property.Kind )
		|| node is PcreByteScalarSet set && IsSupportedSet( set );

	private static bool IsSupportedSet( PcreByteScalarSet set ) {
		if ( set.SourceMemberCount > 64 || set.Ranges.Length + set.Predicates.Length is < 1 or > 64 ) { return false; }
		foreach ( var predicate in set.Predicates.Span ) {
			if ( !IsWordClassPredicate( predicate.Kind ) ) { return false; }
		}
		return true;
	}

	private static bool IsSupportedExpression( PcreByteScalarExpression expression, ref int members, CancellationToken cancellationToken ) {
		if ( expression.Terms.Length is < 1 or > MaximumExpandedNodes
			|| expression.StackDepth < 1 || expression.StackDepth > expression.Terms.Length ) { return false; }
		var depth = 0;
		var maximum = 0;
		foreach ( var term in expression.Terms.Span ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( term.Operand is { } operand ) {
				if ( term.Operation != PcreScalarSetOperation.Operand || operand.SourceMemberCount > 64
					|| operand.Ranges.Length + operand.Predicates.Length > MaximumExpandedNodes - members ) { return false; }
				members += operand.Ranges.Length + operand.Predicates.Length;
				foreach ( var predicate in operand.Predicates.Span ) {
					cancellationToken.ThrowIfCancellationRequested();
					if ( predicate.Kind is not ( PcreByteScalarClassKind.AnyScalar or PcreByteScalarClassKind.UniversalCharacterName ) && !IsWordClassPredicate( predicate.Kind ) ) { return false; }
				}
				maximum = Math.Max( maximum, ++depth );
			} else if ( term.Operation == PcreScalarSetOperation.Not ) {
				if ( depth < 1 ) { return false; }
			} else if ( term.Operation is PcreScalarSetOperation.And or PcreScalarSetOperation.Or
				or PcreScalarSetOperation.Subtract or PcreScalarSetOperation.Xor ) {
				if ( depth < 2 ) { return false; }
				depth--;
			} else { return false; }
		}
		return depth == 1 && maximum == expression.StackDepth;
	}

	private static bool IsWordClassPredicate( PcreByteScalarClassKind kind ) => kind is
		PcreByteScalarClassKind.AsciiDigit or PcreByteScalarClassKind.AsciiWord or PcreByteScalarClassKind.AsciiSpace
		or PcreByteScalarClassKind.Ascii or PcreByteScalarClassKind.AsciiAlnum or PcreByteScalarClassKind.AsciiAlpha
		or PcreByteScalarClassKind.AsciiBlank or PcreByteScalarClassKind.AsciiControl
		or PcreByteScalarClassKind.AsciiGraph or PcreByteScalarClassKind.AsciiLower
		or PcreByteScalarClassKind.AsciiPrintable or PcreByteScalarClassKind.AsciiPunctuation
		or PcreByteScalarClassKind.AsciiUpper or PcreByteScalarClassKind.AsciiHexDigit
		or PcreByteScalarClassKind.HorizontalSpace or PcreByteScalarClassKind.VerticalSpace
		or PcreByteScalarClassKind.Letter or PcreByteScalarClassKind.CasedLetter
		or PcreByteScalarClassKind.CategoryOther or PcreByteScalarClassKind.CategoryMark
		or PcreByteScalarClassKind.CategoryNumber or PcreByteScalarClassKind.CategoryPunctuation
		or PcreByteScalarClassKind.CategorySymbol or PcreByteScalarClassKind.CategorySeparator
		or PcreByteScalarClassKind.SpecificCategory
		or PcreByteScalarClassKind.Script or PcreByteScalarClassKind.ScriptExtensions
		or PcreByteScalarClassKind.BinaryProperty or PcreByteScalarClassKind.BidiClass
		or PcreByteScalarClassKind.DecimalDigit or PcreByteScalarClassKind.UnicodeWord
		or PcreByteScalarClassKind.UnicodeSpace or PcreByteScalarClassKind.UnicodeAlnum
		or PcreByteScalarClassKind.UnicodeBlank or PcreByteScalarClassKind.UnicodeControl
		or PcreByteScalarClassKind.UnicodeGraph or PcreByteScalarClassKind.UnicodeLower
		or PcreByteScalarClassKind.UnicodePrintable or PcreByteScalarClassKind.UnicodePunctuation
		or PcreByteScalarClassKind.UnicodeUpper or PcreByteScalarClassKind.UnicodeHexDigit;

	private static PcreWordCompileResult Error( PcreWordDiagnosticCode code, int offset ) => new( null, new( code, offset ) );
}
