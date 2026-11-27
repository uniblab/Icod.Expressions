namespace Icod.Expressions.Pcre;

internal sealed record PcreWordSuffixChoices( PcreByteNode[][] Alternatives ) : PcreByteNode;

internal sealed record PcreWordGroup(
	PcreByteNode[][] Alternatives, int PathCount, int ExpandedNodes, int UnboundedRepeats, int CaptureNumber = 0,
	int[]? SourceBranchStarts = null, bool[]? CheckAssertionEnds = null
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
		if ( options.Utf16 ) {
			for ( var offset = 0; offset < source.Length; ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( !PcreUtf16.TryRead( source, offset, true, out _, out var width ) ) {
					return Error( PcreWordDiagnosticCode.InvalidUtf16Pattern, offset );
				}
				offset += width;
			}
		}
		// ASCII grammar tokens keep their original indices. Non-ASCII units are
		// placeholders read directly from the owned word source by the parser.
		var grammarSource = new byte[source.Length];
		for ( var index = 0; index < source.Length; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			grammarSource[index] = source[index] > 127 ? (byte)0x80 : (byte)source[index];
		}
		var grammarOptions = new PcreByteOptions {
			Utf8 = options.Utf16, Ucp = options.Ucp, Caseless = options.Caseless,
			CaselessRestricted = options.CaselessRestricted,
			AsciiBsD = options.AsciiBsD, AsciiBsS = options.AsciiBsS, AsciiBsW = options.AsciiBsW,
			AsciiPosix = options.AsciiPosix, AsciiDigit = options.AsciiDigit
		};
		var settings = PcreByteStartSettings.Parse( grammarSource, grammarOptions, cancellationToken, newlineOnly: true );
		// Pair-aware conventions also alter native bumpalong and prefix proofs.
		if ( settings.Newline is not ( PcreByteNewline.Lf or PcreByteNewline.Cr or PcreByteNewline.Nul ) ) {
			return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
		}
		try {
			var parser = new PcreByteParser( grammarSource, grammarOptions, settings, cancellationToken,
				codeUnitWidth: 16, wordSource: source );
			var root = parser.Parse();
			if ( root.Branches.Count is < 1 or > 64 ) {
				return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
			}
			var compiled = new Dictionary<PcreByteAlternatives, PcreWordGroup>( ReferenceEqualityComparer.Instance );
			var pending = new Stack<(PcreByteAlternatives Body, int Depth, bool Complete, PcreByteLookbehind? Behind)>();
			var validationLookbehind = 0;
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
							while ( nested is PcreByteRepeat or PcreByteAtomic ) {
								nested = nested is PcreByteRepeat repeat ? repeat.Child : ( (PcreByteAtomic)nested ).Child;
							}
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
				var body = BuildBody( item.Body, compiled, ref validationLookbehind, cancellationToken, item.Behind );
				if ( body is null ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
				compiled.Add( item.Body, body );
			}
			var targets = new Dictionary<int, PcreByteNode[]>();
			foreach ( var (number, target) in parser.SubroutineTargets ) {
				cancellationToken.ThrowIfCancellationRequested();
				// Integer call edges can refer to the root or a recursive definition.
				// Source bodies were compiled once; do not expand or traverse call edges.
				// A call executes the definition's body, not its enclosing capture close.
				targets.Add( number, [compiled[target.Body]] );
			}
			return new( new( compiled[root].Alternatives, options, validationLookbehind, settings.Newline,
				settings.BsrAnyCrLf, parser.CreateCaptureInfo(), targets ), null );
		} catch ( PcreByteParseException error ) {
			return Error( error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax
				? PcreWordDiagnosticCode.UnsupportedSyntax : PcreWordDiagnosticCode.InvalidSyntax, error.Diagnostic.ByteOffset );
		}
	}

	private static PcreWordGroup? BuildBody( PcreByteAlternatives root,
		Dictionary<PcreByteAlternatives, PcreWordGroup> compiled, ref int validationLookbehind,
		CancellationToken cancellationToken, PcreByteLookbehind? behind = null ) {
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
			var boundaryCount = unboundedCount + nodes.Count( IsContinuationBoundary );
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
				if ( repeat.Maximum == int.MaxValue ) {
					if ( repeat.Minimum is < 0 or > MaximumExactRepeat
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
			SourceBranchStarts: sourceBranchStarts, CheckAssertionEnds: checkAssertionEnds );
	}

	private static bool IsContinuationBoundary( PcreByteNode node ) => node is PcreWordGroup or PcreWordGroupRepeat
		or PcreWordAssertion or PcreWordAtomic or PcreWordReverse or PcreByteSubroutineCall or PcreWordConditional;

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
				if ( repeat.Maximum == int.MaxValue
					&& repeat.Minimum is >= 0 and <= MaximumExactRepeat && IsSupportedAtom( repeat.Child )
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
		|| node is PcreByteAnchor or PcreByteLineAnchor or PcreByteEndBeforeFinalLf
			or PcreByteSearchStart or PcreByteWordBoundary or PcreByteBackreference
			or PcreWordGroup or PcreWordGroupRepeat or PcreWordAssertion or PcreWordAtomic or PcreWordReverse or PcreByteSubroutineCall or PcreWordConditional;

	private static bool IsSupportedAtom( PcreByteNode node ) => node is PcreByteLiteral or PcreByteAny or PcreByteNewlineSequence
		|| node is PcreByteScalarClass property && IsWordClassPredicate( property.Kind )
		|| node is PcreByteScalarSet set && IsSupportedSet( set );

	private static bool IsSupportedSet( PcreByteScalarSet set ) {
		if ( set.SourceMemberCount > 64 || set.Ranges.Length + set.Predicates.Length is < 1 or > 64 ) { return false; }
		foreach ( var predicate in set.Predicates.Span ) {
			if ( !IsWordClassPredicate( predicate.Kind ) ) { return false; }
		}
		return true;
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
