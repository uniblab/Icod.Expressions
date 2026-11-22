namespace Icod.Expressions.Pcre;

/// <summary>Executes ordered bytecode with a bounded explicit backtracking stack.</summary>
internal static partial class PcreByteMatcher {
	private const int MaximumBacktrackFrames = 1024;

	private static void ChargeWork( ref int work, int matchWorkLimit, Action<int>? workObserver, CancellationToken cancellationToken ) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( work >= matchWorkLimit ) {
			throw new MatchLimitException( PcreByteDiagnosticCode.MatchWorkLimitExceeded, "match work limit exceeded" );
		}
		work++;
		workObserver?.Invoke( work );
		cancellationToken.ThrowIfCancellationRequested();
	}

	internal static PcreByteMatchResult Match(
		PcreByteProgram program,
		int groupCount,
		PcreCaptureInfo captureInfo,
		PcreMatchPolicy policy,
		ReadOnlyMemory<byte> input,
		int startOffset,
		int matchWorkLimit,
		Action<int>? workObserver,
		CancellationToken cancellationToken
	) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( startOffset < 0 || startOffset > input.Length ) {
			return new( null, new(
				PcreByteDiagnosticCode.InvalidStartOffset,
				startOffset,
				"start byte offset is outside the input"
			) );
		}
		var anchored = program.Anchored || program.DotStarAnchored || policy.Anchored;
		var endAnchored = program.EndAnchored || policy.EndAnchored;
		var notEmpty = program.NotEmpty || policy.NotEmpty;
		var notEmptyAtStart = program.NotEmptyAtStart || policy.NotEmptyAtStart;
		// Native match-time anchoring selects its interpreter. In tolerant UTF,
		// that driver retries separate valid fragments with fragment-local ends.
		var matchTimeAnchoring = policy.Anchored || policy.EndAnchored;
		var interpretedUtfFragments = program.MatchInvalidUtf && matchTimeAnchoring;
		var retainAnchoredFragment = anchored && !interpretedUtfFragments;
		var surrogateLiterals = program.MatchInvalidUtf && program.HasSurrogateLiterals;
		// No candidate can reach the VM when the required first byte is absent.
		// This is limited to unobserved, noncancellable exact byte searches.
		var unobservedByteSearch = !program.NoStartOptimize && !anchored && !program.FirstLine && !program.Utf8 && !program.HasCaselessComparisons
			&& !cancellationToken.CanBeCanceled && workObserver is null;
		var leadingLiteral = -1;
		var failedLiteralCost = 3;
		if ( unobservedByteSearch ) {
			var first = program.Instructions[0];
			if ( first.Opcode == PcreByteOpcode.Literal && first.First is >= 0 and <= 255 ) {
				leadingLiteral = first.First;
			} else if ( first.Opcode == PcreByteOpcode.CaptureStart
				&& program.Instructions.Length > 1
				&& program.Instructions[1] is { Opcode: PcreByteOpcode.Literal, First: >= 0 and <= 255 } capturedLiteral ) {
				// Captures and match-start resets both add one zero-width instruction.
				// Every successful path here must still consume this first literal.
				leadingLiteral = capturedLiteral.First;
				failedLiteralCost = 4;
			}
		}
		if ( leadingLiteral >= 0 && input.Span.Slice( startOffset ).IndexOf( (byte)leadingLiteral ) < 0 ) {
			// Include the final empty candidate, whose literal comparison also fails.
			if ( ( (long)input.Length - startOffset + 1 ) * failedLiteralCost > matchWorkLimit ) {
				return new( null, new( PcreByteDiagnosticCode.MatchWorkLimitExceeded, 0, "match work limit exceeded" ) );
			}
			return new( null, null );
		}
		if ( unobservedByteSearch && program.Instructions.Length > 1
			&& program.Instructions[0] is { Opcode: PcreByteOpcode.AssertLiteral, First: 1, Second: 1 } lookbehind
			&& program.Instructions[1] is { Opcode: PcreByteOpcode.Literal, First: >= 0 and <= 255 } nextLiteral
			&& input.Span.Slice( startOffset ).IndexOf( (byte)nextLiteral.First ) < 0 ) {
			// Each candidate reaches at most one assertion, its byte comparisons,
			// and one failed literal. Only skip if even that bound fits the budget.
			var candidates = (long)input.Length - startOffset + 1;
			if ( candidates * ( 4L + lookbehind.Bytes.Length ) <= matchWorkLimit ) {
				return new( null, null );
			}
		}
		var unobservedUnicodeSearch = !program.NoStartOptimize && !anchored && !program.FirstLine && !cancellationToken.CanBeCanceled && workObserver is null
			&& !program.RequiredLiteralSuffix.IsEmpty;
		if ( unobservedUnicodeSearch && program.MatchInvalidUtf
			&& MissingRequiredLiteralSuffixWithinBudget( program, input.Span, startOffset, matchWorkLimit ) ) {
			// Tolerant mode has no invalid-subject diagnostic to preserve.
			return new( null, null );
		}
		var work = 0;
		bool[]? scalarExpressionStack = null;
		void Step() => ChargeWork( ref work, matchWorkLimit, workObserver, cancellationToken );
		void SkipFailedLiteralCandidates( long count ) {
			// A capture wrapper adds one instruction before the failed literal.
			var cost = count * failedLiteralCost;
			if ( cost > matchWorkLimit - work ) {
				throw new MatchLimitException( PcreByteDiagnosticCode.MatchWorkLimitExceeded, "match work limit exceeded" );
			}
			work += (int)cost;
		}

		try {
			var fragments = new List<Fragment>();
			if ( program.Utf8 ) {
				var fragmentStart = 0;
				var hasValidScalar = false;
				for ( var offset = 0; offset < input.Length; ) {
					Step(); // Inspect one scalar or invalid byte within the match work budget.
					if ( !PcreUtf8.TryReadLiteral( input.Span, offset, surrogateLiterals, out _, out var width ) ) {
						if ( !program.MatchInvalidUtf ) {
							return new( null, new( PcreByteDiagnosticCode.InvalidUtf8Subject, offset,
								"subject contains invalid UTF-8" ) );
						}
						if ( hasValidScalar ) {
							RetainSearchFragment( fragments, new( fragmentStart, offset ), retainAnchoredFragment, startOffset );
						} else if ( offset == 0 ) {
							RetainSearchFragment( fragments, new( offset, offset ), retainAnchoredFragment, startOffset );
						}
						hasValidScalar = false;
						offset++;
						continue;
					}
					if ( !hasValidScalar ) { fragmentStart = offset; }
					hasValidScalar = true;
					offset += width;
				}
				RetainSearchFragment( fragments, new( hasValidScalar ? fragmentStart : input.Length, input.Length ), retainAnchoredFragment, startOffset );
				if ( !program.MatchInvalidUtf && startOffset < input.Length
					&& ( input.Span[startOffset] & 0xC0 ) == 0x80 ) {
					return new( null, new( PcreByteDiagnosticCode.InvalidStartOffset, startOffset,
						"start byte offset is inside a UTF-8 character" ) );
				}
			} else {
				fragments.Add( new( 0, input.Length ) );
			}
			var searchOffset = startOffset;
			if ( interpretedUtfFragments ) {
				while ( searchOffset < input.Length && ( input.Span[searchOffset] & 0xC0 ) == 0x80 ) { Step(); searchOffset++; }
			}
			if ( unobservedUnicodeSearch && !program.MatchInvalidUtf
				&& MissingRequiredLiteralSuffixWithinBudget( program, input.Span, startOffset, matchWorkLimit ) ) {
				// Strict mode must validate UTF-8 and the start boundary first.
				return new( null, null );
			}
			// Native STARTLINE search begins at the requested byte, so a newline
			// whose encoding began before it cannot supply the first line candidate.
			var excludedLineStart = !anchored && program.LineStartSearch ? PcreByteNewlineExtensions.PartialAnyNewlineEnd( input.Span, startOffset ) : -1;
			if ( program.MatchInvalidUtf && !interpretedUtfFragments && !endAnchored && !notEmpty && !notEmptyAtStart && program.Instructions.Length == 1
				&& program.Instructions[0].Opcode == PcreByteOpcode.Accept ) {
				// PCRE2 permits an empty pattern at the requested byte offset,
				// including inside a valid scalar or a malformed byte run.
				Step(); // Candidate start.
				Step(); // Accept transition.
				var emptyCaptures = new PcreByteCapture?[groupCount + 1];
				emptyCaptures[0] = new( startOffset, 0 );
				return new( new PcreByteMatch( startOffset, 0, Array.AsReadOnly( emptyCaptures ), captureInfo ), null );
			}
			// The interpreter can try an empty initial fragment at a malformed byte.
			if ( program.MatchInvalidUtf && ( interpretedUtfFragments || program.CanMatchEmptyAtAnyOffset )
				&& searchOffset < input.Length && !PcreUtf8.TryReadLiteral( input.Span, searchOffset, surrogateLiterals, out _, out _ ) ) {
				// PCRE2 can accept a zero-width path exactly at a requested byte offset,
				// including inside a scalar or malformed run. Consumption remains barred.
				// A real fragment ending here already permits a zero-width candidate
				// with its valid preceding context. Do not shadow it with an empty
				// fragment: nested lookbehind/boundary assertions may inspect that text.
				var hasContextCandidate = false;
				foreach ( var existing in fragments ) {
					cancellationToken.ThrowIfCancellationRequested();
					if ( existing.End == searchOffset ) { hasContextCandidate = true; break; }
				}
				if ( !hasContextCandidate ) {
					// Starting inside a valid scalar makes its remaining continuation
					// bytes zero-width candidates, just like a malformed byte run.
					var end = interpretedUtfFragments ? searchOffset + 1
						: Math.Max( searchOffset + 1, PcreByteNewlineExtensions.PartialScalarEnd( input.Span, searchOffset ) );
					for ( var candidate = searchOffset; candidate < end; candidate++ ) {
						fragments.Insert( candidate - searchOffset, new( candidate, candidate ) );
					}
				}
			}
			// Candidate positions share storage; a successful match returns before the storage is reused.
			var captures = new PcreByteCapture?[groupCount + 1];
			var captureStarts = new int[groupCount + 1];
			var marks = new int[program.MarkCount];
			var backtrack = new Stack<Frame>();
			var changes = new List<StateChange>();
			var atomicDepths = new List<int>();
			List<AssertionScope>? assertionScopes = null;
			List<CallScope>? callScopes = null;
			int[]? restoredFields = null;
			var returnGeneration = 0;
			var regionalRun = new RegionalRun( -1, -1 );
			var pruneLiteralEnd = -1;
			// Keep the observed and cancellable path byte-for-byte accountable to Step().
			var nullableInvalidCandidates = !anchored && !interpretedUtfFragments && program.MatchInvalidUtf && program.CanMatchEmptyAtAnyOffset;
			var previousFragmentEnd = -1;
			var firstLinePosition = startOffset;
			var forcedSkipStart = -1;
			var ignoredNamedSkips = 0;
			var xdigitFragmentEnd = startOffset;
			for ( var fragmentIndex = 0; fragmentIndex < fragments.Count; ) {
				var fragment = fragments[fragmentIndex];
				if ( interpretedUtfFragments && fragment.Start == input.Length && searchOffset < input.Length ) { break; }
				if ( interpretedUtfFragments && searchOffset > startOffset && fragment.Start < searchOffset ) {
					// Native skipped a continuation start, so preceding text cannot
					// provide assertion context for this recovered first fragment.
					fragment = new( searchOffset, fragment.End );
				}
				if ( nullableInvalidCandidates && previousFragmentEnd >= 0
					&& previousFragmentEnd < fragment.Start - 1 && startOffset < fragment.Start - 1 ) {
					// Gaps contain only malformed bytes: valid UTF scalars remain whole
					// fragments. Retry nullable paths at later bytes in an invalid run,
					// without duplicating the explicitly requested candidate or storing gaps.
					var gap = Math.Max( previousFragmentEnd + 1, startOffset + 1 );
					fragment = new( gap, gap );
				} else { fragmentIndex++; }
				previousFragmentEnd = fragment.End;
				if ( program.UtfXdigitFragmentBarrier && fragment.Start <= startOffset && startOffset < fragment.End
					&& ( input.Span[startOffset] & 0xC0 ) != 0x80 ) { xdigitFragmentEnd = fragment.End; }
				var fragmentOffset = Math.Max( searchOffset, fragment.Start );
				var matchEnd = interpretedUtfFragments ? fragment.End : input.Length;
				if ( interpretedUtfFragments && program.FirstLine ) {
					// The interpreter starts a new first-line search in each valid fragment.
					firstLinePosition = fragmentOffset;
				}
				for ( var start = fragmentOffset; start <= fragment.End; start++ ) {
					if ( anchored && start != ( interpretedUtfFragments ? fragmentOffset : startOffset ) && start != forcedSkipStart ) { return new( null, null ); }
					if ( program.UtfXdigitFragmentBarrier && start > xdigitFragmentEnd ) { return new( null, null ); }
					if ( program.Utf8 && start < fragment.End && ( input.Span[start] & 0xC0 ) == 0x80 ) {
						continue; // Search candidate boundaries, never continuation bytes.
					}
					if ( program.FirstLine && !anchored
						&& !( start == forcedSkipStart && program.FirstLineSkipBypassesWindow ) ) {
						while ( firstLinePosition < start ) {
							Step(); // Inspect one intervening scalar or malformed byte.
							if ( IsNewlineAt( program, input.Span, firstLinePosition, matchEnd ) ) { break; }
							firstLinePosition += program.Utf8 && PcreUtf8.TryReadLiteral( input.Span, firstLinePosition,
								surrogateLiterals, out _, out var firstLineWidth ) ? firstLineWidth : 1;
						}
						if ( firstLinePosition < start ) {
							if ( interpretedUtfFragments ) { break; }
							return new( null, null );
						}
					}
					forcedSkipStart = -1; // Consume an explicitly selected entry before matching.
					if ( leadingLiteral >= 0 ) {
						var next = input.Span.Slice( start, fragment.End - start ).IndexOf( (byte)leadingLiteral );
						if ( next < 0 ) {
							SkipFailedLiteralCandidates( (long)fragment.End - start + 1 );
							break;
						}
						SkipFailedLiteralCandidates( next );
						start += next;
					}
					if ( !anchored && program.CommitStartBytes is { } commitBytes ) {
						Step(); // Native first-code-unit entry precedes a leading COMMIT.
						if ( start == input.Length || !commitBytes.Contains( input.Span[start] ) ) { continue; }
					}
					Step(); // Candidate start.
					var position = start;
					var utfRepeatEnd = start;
					var skipRestart = -1;
					var namedSkipCount = 0;
					var replayNamedSkip = false;
					var committedFragment = false;
					var pc = 0;
					Array.Clear( captures );
					Array.Clear( captureStarts );
					captureStarts[0] = start; // Reported start; backtracked resets use the capture journal.
					Array.Clear( marks );
					backtrack.Clear();
					changes.Clear();
					atomicDepths.Clear();
					assertionScopes?.Clear();
					callScopes?.Clear();
					var activeScope = -1;
					var activeCall = -1;
					while ( true ) {
						Step(); // Instruction transition.
						var instruction = program.Instructions[pc];
						var endLimit = activeScope >= 0 ? assertionScopes![activeScope].LimitEnd : -1;
						var subjectEnd = endLimit >= 0 ? Math.Min( fragment.End, endLimit ) : fragment.End;
						var absoluteEnd = endLimit >= 0 ? endLimit : input.Length;
						var suppressLineEnd = policy.NotEol || interpretedUtfFragments && fragment.End < input.Length;
						var failed = false;
						switch ( instruction.Opcode ) {
						case PcreByteOpcode.PruneRepeatStart:
							if ( position < pruneLiteralEnd ) { failed = true; }
							else { pc++; }
							break;
						case PcreByteOpcode.PruneRepeatEnd:
							pruneLiteralEnd = Math.Max( pruneLiteralEnd, position );
							pc++;
							break;
						case PcreByteOpcode.Then:
						case PcreByteOpcode.Commit:
						case PcreByteOpcode.Skip:
						case PcreByteOpcode.SkipNamed:
						case PcreByteOpcode.Prune:
							if ( matchTimeAnchoring && instruction.Opcode == PcreByteOpcode.SkipNamed ) {
								namedSkipCount++;
								if ( namedSkipCount <= ignoredNamedSkips ) { pc++; break; }
							}
							if ( instruction.Opcode != PcreByteOpcode.SkipNamed && backtrack.Count >= MaximumBacktrackFrames ) {
								throw new MatchLimitException( PcreByteDiagnosticCode.MatchDepthLimitExceeded, "backtrack stack limit exceeded" );
							}
							var pruneBoundary = activeCall >= 0 ? callScopes![activeCall].Boundary : 0;
							var historyBoundary = 0;
							for ( var pruneScope = activeScope; pruneScope >= 0; ) {
								Step();
								var catchingScope = assertionScopes![pruneScope];
								if ( catchingScope.Boundary + 1 <= pruneBoundary ) { break; }
								if ( catchingScope.IsolatePathMarks ) {
									historyBoundary = Math.Max( historyBoundary, catchingScope.Boundary + 1 );
								}
								// Interpreted matching catches THEN at this assertion.
								// Default/JIT uses a call-local quit for an unowned THEN
								// inside a called atomic positive assertion.
								var thenCatches = instruction.Opcode == PcreByteOpcode.Then
									&& ( matchTimeAnchoring || instruction.First >= 0 || activeCall < 0
										|| catchingScope.Kind != FrameKind.PositiveAssertion );
								if ( thenCatches || catchingScope.Kind is FrameKind.NegativeAssertion or FrameKind.ConditionalAssertion ) {
									pruneBoundary = catchingScope.Boundary + 1;
									break;
								}
								pruneScope = catchingScope.Parent;
							}
							if ( instruction.Opcode == PcreByteOpcode.Then && instruction.First >= 0 ) {
								// Keep just the owning group's next-branch frame. Last branches
								// retain only the entry depth, permitting earlier outer retries.
								pruneBoundary = Math.Max( pruneBoundary, marks[instruction.First] + instruction.Second );
							}
							var skipPosition = position;
							var unmatchedNamedSkip = false;
							if ( instruction.Opcode == PcreByteOpcode.SkipNamed ) {
								// Native JIT takes a local assertion/callee quit before lookup.
								// Root lookup sees only history still visible to backtracking;
								// default/JIT hides returned callee history. The interpreter
								// can resolve names against those retained frames.
								var namedSkipCallBoundary = activeCall >= 0 ? callScopes![activeCall].Boundary : 0;
								var resolveAssertionSkip = matchTimeAnchoring && pruneBoundary > namedSkipCallBoundary;
								if ( resolveAssertionSkip ) {
									// The interpreter propagates an unresolved name past assertions.
									// A matching MARK chooses where ordinary SKIP can be caught.
									pruneBoundary = namedSkipCallBoundary;
									historyBoundary = namedSkipCallBoundary;
								}
								if ( pruneBoundary == 0 || resolveAssertionSkip ) {
									skipPosition = -1;
									var historyDepth = backtrack.Count;
									foreach ( var history in backtrack ) {
										if ( historyDepth-- <= historyBoundary ) { break; }
										Step();
										if ( history.Kind == FrameKind.PathMark && ( matchTimeAnchoring || history.CallScope == activeCall )
											&& program.Instructions[history.Pc].Bytes.Span.SequenceEqual( instruction.Bytes.Span ) ) {
											skipPosition = history.Position;
											if ( resolveAssertionSkip ) {
												for ( var markScope = history.AssertionScope; markScope >= 0; ) {
													Step();
													var markCatcher = assertionScopes![markScope];
													if ( markCatcher.Boundary + 1 <= pruneBoundary ) { break; }
													if ( markCatcher.Kind is FrameKind.NegativeAssertion or FrameKind.ConditionalAssertion ) {
														pruneBoundary = markCatcher.Boundary + 1;
														break;
													}
													markScope = markCatcher.Parent;
												}
											}
											break;
										}
									}
									if ( skipPosition < 0 ) {
										if ( pruneBoundary > 0 ) { skipPosition = position; } // The active callee catches the unresolved name.
										else {
											if ( !matchTimeAnchoring ) { pc++; break; }
											// The interpreter can replay this candidate when failure reaches the verb.
											unmatchedNamedSkip = true;
											skipPosition = position;
										}
									}
								}
								if ( backtrack.Count >= MaximumBacktrackFrames ) {
									throw new MatchLimitException( PcreByteDiagnosticCode.MatchDepthLimitExceeded, "backtrack stack limit exceeded" );
								}
							}
							// Pc stores a live saved-frame boundary, not an instruction target.
							// Only active scopes catch: older retained siblings are ordinary paths.
							backtrack.Push( new Frame( pruneBoundary, skipPosition, changes.Count, atomicDepths.Count,
								activeScope, instruction.Opcode switch {
									PcreByteOpcode.Then => instruction.First >= 0 ? FrameKind.ThenAlternative : FrameKind.Then,
									PcreByteOpcode.Commit => FrameKind.Commit,
									PcreByteOpcode.SkipNamed when unmatchedNamedSkip => FrameKind.SkipReplay,
									PcreByteOpcode.Skip or PcreByteOpcode.SkipNamed => FrameKind.Skip, _ => FrameKind.Prune
								}, CallScope: activeCall ) );
							if ( instruction.PropertyValue > 0 ) {
								// Named PRUNE/COMMIT/THEN share the label journal and their
								// cut checkpoint. They do not create named-SKIP history.
								var cutLabelSlot = instruction.PropertyValue - 1;
								changes.Add( new( StateKind.Mark, cutLabelSlot, marks[cutLabelSlot], null ) );
								marks[cutLabelSlot] = pc + 1;
							}
							pc++;
							break;
						case PcreByteOpcode.Fail:
							failed = true;
							break;
						case PcreByteOpcode.Literal:
							Step(); // Attempted scalar/byte comparison.
							var literalWidth = 1;
							if ( position < subjectEnd && ( program.Utf8
								? PcreUtf8.TryReadLiteral( input.Span, position, surrogateLiterals, out var scalar, out literalWidth )
									&& literalWidth <= subjectEnd - position
									&& ( instruction.Caseless ? PcreUtf8CaseFold.Equals( scalar, instruction.First, instruction.CaselessRestricted, program.TurkishCasing )
										: scalar == instruction.First )
								: ScalarEquals( input.Span[position], instruction.First, instruction.Caseless, program.Ucp, instruction.CaselessRestricted ) ) ) {
								position += literalWidth;
								if ( instruction.CommitOptional && ( notEmpty || notEmptyAtStart ) ) {
									// Commit only this literal query's immediately preceding Split.
									// A failed literal still takes its ordinary zero-length edge.
									PopFrame( backtrack, assertionScopes, callScopes );
									if ( backtrack.Count == 0 ) { changes.Clear(); }
								}
								if ( instruction.SkipCrLfAfterRepeat && activeCall < 0 && activeScope < 0 ) {
									utfRepeatEnd = Math.Max( utfRepeatEnd, position );
								}
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.Any:
							Step(); // Attempted byte/scalar consumption.
							var value = position < subjectEnd ? (int)input.Span[position] : 0;
							var scalarWidth = 1;
							if ( position < subjectEnd
								&& ( !program.Utf8 || PcreUtf8.TryRead( input.Span, position, out value, out scalarWidth ) )
								&& scalarWidth <= subjectEnd - position
								&& ( instruction.First == 1 || ( program.Newline == PcreByteNewline.CrLf
									? !PcreByteNewlineExtensions.IsCrLf( input.Span, position, subjectEnd )
									: program.Newline == PcreByteNewline.Any ? !PcreSpaceCharacters.IsVertical( value )
									: value != program.NewlineCharacter && ( !program.AnyCrLfNewline || value != (int)'\r' ) ) ) ) {
								position += scalarWidth;
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.Grapheme:
							var graphemeEnd = ConsumeGrapheme( input, position, subjectEnd, fragment.Start,
								program.Utf8, program.MatchInvalidUtf, ref work, matchWorkLimit, workObserver, cancellationToken, ref regionalRun );
							if ( graphemeEnd >= 0 ) { position = graphemeEnd; pc++; }
							else { failed = true; }
							break;
						case PcreByteOpcode.ScalarClass:
							Step(); // Attempted scalar class evaluation.
							if ( position < subjectEnd
								&& PcreUtf8.TryRead( input.Span, position, out var propertyScalar, out var propertyWidth )
								&& propertyWidth <= subjectEnd - position
								&& PcreScalarClassMembership.Contains( (PcreByteScalarClassKind)instruction.First, propertyScalar,
									instruction.Category, program.Utf8 && instruction.Caseless, instruction.PropertyValue )
									!= ( instruction.Second == 1 ) ) {
								position += propertyWidth;
								if ( instruction.SkipCrLfAfterRepeat && activeCall < 0 && activeScope < 0 ) {
									utfRepeatEnd = Math.Max( utfRepeatEnd, position );
								}
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.ScalarExpression:
							Step();
							if ( position < subjectEnd && PcreUtf8.TryRead( input.Span, position, out var expressionScalar, out var expressionWidth )
								&& expressionWidth <= subjectEnd - position
								&& ContainsScalarExpression( instruction.Terms, expressionScalar,
									scalarExpressionStack ??= new bool[program.ScalarExpressionStackDepth],
									ref work, matchWorkLimit, workObserver, program.TurkishCasing, cancellationToken ) ) {
								position += expressionWidth; pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.ScalarSet:
							Step(); // Attempted scalar-set evaluation.
							if ( position >= subjectEnd
								|| !PcreUtf8.TryRead( input.Span, position, out var setScalar, out var setWidth ) || setWidth > subjectEnd - position ) {
								failed = true;
								break;
							}
							var contained = false;
							var rangeScalar = instruction.Caseless ? PcreUtf8CaseFold.Fold( setScalar, instruction.CaselessRestricted, program.TurkishCasing ) : setScalar;
							foreach ( var range in instruction.Ranges.Span ) {
								Step(); // Each attempted interval inspection consumes work.
								// Canonical folded intervals are sorted; raw source intervals need not be.
								if ( instruction.Caseless && rangeScalar < range.Start ) { break; }
								if ( rangeScalar >= range.Start && rangeScalar <= range.End ) { contained = true; break; }
							}
							if ( !contained ) {
								foreach ( var predicate in instruction.Predicates.Span ) {
									Step(); // Each attempted scalar predicate consumes work.
									if ( PcreScalarClassMembership.Contains( predicate.Kind, setScalar, predicate.Category,
										instruction.Caseless, predicate.PropertyValue ) != predicate.Negated ) {
										contained = true;
										break;
									}
								}
							}
							if ( contained != ( instruction.Second == 1 ) ) {
								position += setWidth;
								if ( instruction.SkipCrLfAfterRepeat && activeCall < 0 && activeScope < 0 ) {
									utfRepeatEnd = Math.Max( utfRepeatEnd, position );
								}
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.Class:
							Step(); // Attempted byte set membership.
							if ( position < subjectEnd && instruction.Set.Contains( input.Span[position] ) ) {
								position++;
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.CaptureTest:
							var participated = false;
							for ( var slot = 0; slot < instruction.CaptureNumbers.Length; slot++ ) {
								Step(); // Each declaration-ordered participation lookup is bounded work.
								if ( captures[instruction.CaptureNumbers.Span[slot]].HasValue ) {
									participated = true;
									break; // Empty captures participate too.
								}
							}
							pc = participated ? instruction.First : instruction.Second;
							break;
						case PcreByteOpcode.RecursionTest:
							var inCall = activeCall >= 0;
							if ( inCall && !instruction.CaptureNumbers.IsEmpty ) {
								inCall = false;
								var currentCallNumber = callScopes![activeCall].Number;
								for ( var slot = 0; slot < instruction.CaptureNumbers.Length; slot++ ) {
									Step(); // One current-call target lookup, never an ancestor/guard scan.
									if ( instruction.CaptureNumbers.Span[slot] == currentCallNumber ) {
										inCall = true;
										break;
									}
								}
							}
							pc = inCall ? instruction.First : instruction.Second;
							break;
						case PcreByteOpcode.Backreference:
							PcreByteCapture? capture;
							if ( instruction.CaptureNumbers.IsEmpty ) {
								capture = captures[instruction.First];
							} else {
								capture = null;
								for ( var slot = 0; slot < instruction.CaptureNumbers.Length; slot++ ) {
									Step(); // One declaration-ordered participation lookup.
									capture = captures[instruction.CaptureNumbers.Span[slot]];
									if ( capture.HasValue ) { break; } // Empty captures participate too.
								}
							}
							// Commit before comparison: failure must not try a later capture.
							if ( !capture.HasValue ) {
								if ( instruction.Second == 1 ) { pc++; }
								else { failed = true; }
								break;
							}
							var captured = capture.Value;
							if ( program.Utf8 && instruction.Caseless ) {
								var capturedOffset = captured.ByteIndex;
								var capturedEnd = captured.ByteIndex + captured.ByteLength;
								var candidate = position;
								while ( capturedOffset < capturedEnd ) {
									Step(); // Attempted captured-scalar comparison.
									if ( candidate >= subjectEnd
										|| !PcreUtf8.TryRead( input.Span, capturedOffset, out var captureScalar, out var captureWidth )
										|| capturedOffset + captureWidth > capturedEnd
										|| !PcreUtf8.TryRead( input.Span, candidate, out var referenceScalar, out var referenceWidth )
										|| candidate + referenceWidth > subjectEnd
										|| !PcreUtf8CaseFold.Equals( captureScalar, referenceScalar, instruction.CaselessRestricted, program.TurkishCasing ) ) {
										failed = true;
										break;
									}
									capturedOffset += captureWidth;
									candidate += referenceWidth;
								}
								if ( !failed ) { position = candidate; pc++; }
								break;
							}
							if ( captured.ByteLength > subjectEnd - position ) {
								failed = true;
								break;
							}
							for ( var byteIndex = 0; byteIndex < captured.ByteLength; byteIndex++ ) {
								Step(); // Attempted captured-byte comparison.
								if ( byteIndex >= absoluteEnd - position
									|| !ScalarEquals( input.Span[captured.ByteIndex + byteIndex], input.Span[position + byteIndex], instruction.Caseless, program.Ucp, instruction.CaselessRestricted ) ) {
									failed = true;
									break;
								}
							}
							if ( !failed ) {
								position += captured.ByteLength;
								pc++;
							}
							break;
						case PcreByteOpcode.WordBoundary:
							// PCRE2 JIT has a third boundary state: invalid codepoints on
							// both sides reject both b and B. A synthetic/gap candidate has
							// no valid preceding scalar; subject start and EOF are distinct.
							if ( program.MatchInvalidUtf && !interpretedUtfFragments && position > 0 && position == fragment.Start
								&& position < absoluteEnd && !PcreUtf8.TryRead( input.Span, position, out _, out _ ) ) {
								failed = true;
								break;
							}
							var beforeWord = false;
							if ( position > fragment.Start ) {
								Step(); // Classify the preceding source byte or UTF-8 scalar.
								if ( instruction.Second == 1 ) {
									var beforeScalar = (int)input.Span[position - 1];
									if ( program.Utf8 ) {
										var previous = position - 1;
										while ( ( input.Span[previous] & 0xC0 ) == 0x80 ) { previous--; }
										PcreUtf8.TryRead( input.Span, previous, out beforeScalar, out _ );
									}
									beforeWord = PcreScalarClassMembership.Contains( PcreByteScalarClassKind.UnicodeWord, beforeScalar );
								} else { beforeWord = PcreByteSet.Word.Contains( input.Span[position - 1] ); }
							}
							var afterWord = false;
							if ( position < subjectEnd ) {
								Step(); // Classify the following source byte or UTF-8 scalar.
								if ( instruction.Second == 1 ) {
									var afterScalar = (int)input.Span[position];
									if ( program.Utf8 ) { PcreUtf8.TryRead( input.Span, position, out afterScalar, out _ ); }
									afterWord = PcreScalarClassMembership.Contains( PcreByteScalarClassKind.UnicodeWord, afterScalar );
								} else { afterWord = PcreByteSet.Word.Contains( input.Span[position] ); }
							}
							if ( ( beforeWord != afterWord ) == ( instruction.First == 1 ) ) { pc++; }
							else { failed = true; }
							break;
						case PcreByteOpcode.Reverse:
						case PcreByteOpcode.ReverseRange:
							var rangedReverse = instruction.Opcode == PcreByteOpcode.ReverseRange;
							var minimumReverse = rangedReverse ? instruction.Second : instruction.First;
							var reversed = position;
							var reversedCount = 0;
							while ( reversedCount < instruction.First ) {
								Step(); // One asserted character of left context.
								if ( reversed <= fragment.Start ) {
									// Pinned invalid-UTF JIT fails optional rewind at malformed barriers.
									failed = reversedCount < minimumReverse || rangedReverse && program.MatchInvalidUtf && !interpretedUtfFragments && fragment.Start > 0;
									break;
								}
								var reverseEnd = reversed;
								reversed--;
								if ( program.Utf8 ) {
									while ( reversed > fragment.Start && ( input.Span[reversed] & 0xC0 ) == 0x80 ) {
										Step();
										reversed--;
									}
									if ( !PcreUtf8.TryReadLiteral( input.Span, reversed, surrogateLiterals, out _, out var reverseWidth )
										|| reversed + reverseWidth != reverseEnd ) { failed = true; break; }
								}
								reversedCount++;
							}
							if ( !failed ) {
								position = reversed;
								if ( rangedReverse && reversedCount > minimumReverse ) {
									if ( backtrack.Count >= MaximumBacktrackFrames ) {
										throw new MatchLimitException( PcreByteDiagnosticCode.MatchDepthLimitExceeded, "backtrack stack limit exceeded" );
									}
									backtrack.Push( new Frame( pc + 1, NextReverseStart( input.Span, position, program.Utf8, surrogateLiterals ),
										changes.Count, atomicDepths.Count, activeScope, FrameKind.ReverseRetry,
										(byte)( reversedCount - minimumReverse - 1 ), activeCall ) );
								}
								pc++;
							}
							break;
						case PcreByteOpcode.ReverseEnd:
							if ( position == assertionScopes![activeScope].Position ) { pc++; } else { failed = true; }
							break;
						case PcreByteOpcode.AssertLiteral:
							var literal = instruction.Bytes.Span;
							if ( program.Utf8 && instruction.Caseless ) {
								var scalarEquals = true;
								var scalarAdjacent = position;
								if ( instruction.Second == 1 ) {
									for ( var patternOffset = 0; patternOffset < literal.Length; ) {
										Step(); // Count an asserted scalar.
										PcreUtf8.TryRead( literal, patternOffset, out _, out var patternWidth );
										patternOffset += patternWidth;
										if ( scalarAdjacent <= fragment.Start ) { scalarEquals = false; break; }
										do {
											scalarAdjacent--;
											Step(); // Find the previous scalar boundary within this fragment.
										} while ( scalarAdjacent > fragment.Start
											&& ( input.Span[scalarAdjacent] & 0xC0 ) == 0x80 );
									}
								}
								if ( scalarEquals ) {
									var subjectOffset = scalarAdjacent;
									for ( var patternOffset = 0; patternOffset < literal.Length; ) {
										Step(); // Attempted assertion-scalar comparison.
										if ( subjectOffset >= subjectEnd
											|| !PcreUtf8.TryRead( literal, patternOffset, out var assertionScalar, out var assertionWidth )
											|| !PcreUtf8.TryRead( input.Span, subjectOffset, out var subjectScalar, out var subjectWidth )
											|| subjectOffset + subjectWidth > subjectEnd
											|| !PcreUtf8CaseFold.Equals( assertionScalar, subjectScalar, instruction.CaselessRestricted, program.TurkishCasing ) ) {
											scalarEquals = false;
											break;
										}
										patternOffset += assertionWidth;
										subjectOffset += subjectWidth;
									}
									if ( instruction.Second == 1 && subjectOffset != position ) { scalarEquals = false; }
								}
								if ( scalarEquals == ( instruction.First == 1 ) ) { pc++; }
								else { failed = true; }
								break;
							}
							var adjacent = instruction.Second == 1 ? position - literal.Length : position;
							var equals = adjacent >= fragment.Start && literal.Length <= subjectEnd - adjacent;
							if ( equals ) {
								for ( var byteIndex = 0; byteIndex < literal.Length; byteIndex++ ) {
									Step(); // Attempted assertion byte comparison.
									if ( !ScalarEquals( input.Span[adjacent + byteIndex], literal[byteIndex], instruction.Caseless, program.Ucp, instruction.CaselessRestricted ) ) {
										equals = false;
										break;
									}
								}
							}
							if ( equals == ( instruction.First == 1 ) ) { pc++; }
							else { failed = true; }
							break;
						case PcreByteOpcode.Start:
							if ( position == 0 && ( instruction.First != 1 || !policy.NotBol ) ) { pc++; } else { failed = true; }
							break;
						case PcreByteOpcode.SearchStart:
							if ( position == startOffset ) { pc++; } else { failed = true; }
							break;
						case PcreByteOpcode.End:
							if ( position == absoluteEnd && ( instruction.First != 1 || !suppressLineEnd ) ) { pc++; } else { failed = true; }
							break;
						case PcreByteOpcode.StartLine:
							// Native newline search still retries physical EOF when the
							// requested offset splits the alternate caret's final newline.
							if ( position == excludedLineStart && ( instruction.First != 1 || position != input.Length ) ) {
								failed = true; break;
							}
							if ( ( position == 0 && instruction.Second != 1 && !policy.NotBol ) || ( position > fragment.Start
								&& ( position < subjectEnd || position == subjectEnd && ( position < absoluteEnd && !interpretedUtfFragments || instruction.First == 1 ) )
								&& ( program.Newline == PcreByteNewline.Any
									? PcreByteNewlineExtensions.IsAnyBefore( input.Span, position, fragment.Start, program.Utf8 )
									: program.Newline == PcreByteNewline.CrLf
									? position - fragment.Start >= 2 && PcreByteNewlineExtensions.IsCrLf( input.Span, position - 2, position )
									: input.Span[position - 1] == program.NewlineCharacter
									|| program.AnyCrLfNewline && input.Span[position - 1] == (byte)'\r' ) ) ) {
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.EndLine:
							// JIT allows the LF after consumed CR under ANY/ANYCRLF;
							// CRLF-only requires the complete pair. Bumpalong stays separate.
							if ( ( position == absoluteEnd && instruction.Second != 1 && !suppressLineEnd ) || ( position < subjectEnd
							&& ( program.Newline == PcreByteNewline.Any
								? PcreByteNewlineExtensions.AnyLengthAt( input.Span, position, subjectEnd, program.Utf8 ) > 0
								: program.Newline == PcreByteNewline.CrLf
								? PcreByteNewlineExtensions.IsCrLf( input.Span, position, subjectEnd )
								: input.Span[position] == program.NewlineCharacter
								|| program.AnyCrLfNewline && input.Span[position] == (byte)'\r' ) ) ) {
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.EndBeforeFinalLf:
							if ( instruction.Second == 1 && suppressLineEnd ) { failed = true; break; }
							var end = instruction.First == 1 ? subjectEnd : absoluteEnd;
							var newlineLength = program.Newline == PcreByteNewline.Any
								? PcreByteNewlineExtensions.AnyLengthAt( input.Span, position, end, program.Utf8 ) : 0;
							if ( position == end || ( program.Newline == PcreByteNewline.Any
								? newlineLength > 0 && newlineLength == end - position
								: program.Newline == PcreByteNewline.CrLf
								? position == end - 2 && PcreByteNewlineExtensions.IsCrLf( input.Span, position, end )
								: ( position == end - 1 && input.Span[position] == program.NewlineCharacter )
							|| program.AnyCrLfNewline && ( position == end - 1 && input.Span[position] == (byte)'\r'
								|| position == end - 2 && input.Span[position] == (byte)'\r'
									&& input.Span[position + 1] == (byte)'\n' ) ) ) {
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.CaptureStart:
							if ( backtrack.Count > 0 ) {
								changes.Add( new( StateKind.CaptureStart, instruction.First,
									captureStarts[instruction.First], null ) );
							}
							captureStarts[instruction.First] = position;
							pc++;
							break;
						case PcreByteOpcode.CaptureEnd:
							if ( backtrack.Count > 0 ) {
								changes.Add( new( StateKind.Capture, instruction.First, 0,
									captures[instruction.First] ) );
							}
							captures[instruction.First] = new( captureStarts[instruction.First], position - captureStarts[instruction.First] );
							pc++;
							break;
						case PcreByteOpcode.Split:
							if ( backtrack.Count >= MaximumBacktrackFrames ) {
								throw new MatchLimitException( PcreByteDiagnosticCode.MatchDepthLimitExceeded, "backtrack stack limit exceeded" );
							}
							backtrack.Push( new Frame( instruction.Second, position, changes.Count, atomicDepths.Count,
								activeScope, CallScope: activeCall ) );
							pc = instruction.First;
							break;
						case PcreByteOpcode.Call:
							if ( backtrack.Count >= MaximumBacktrackFrames ) {
								throw new MatchLimitException( PcreByteDiagnosticCode.MatchDepthLimitExceeded, "backtrack stack limit exceeded" );
							}
							callScopes ??= new();
							backtrack.Push( new Frame( pc + 1, position, changes.Count, atomicDepths.Count,
								activeScope, FrameKind.Call, CallScope: activeCall ) );
							var callId = callScopes.Count;
							callScopes.Add( new( pc + 1, activeCall, changes.Count, instruction.Second, activeScope, backtrack.Count,
								program.PathMarkSlot >= 0 ? marks[program.PathMarkSlot] : 0 ) );
							activeCall = callId;
							pc = instruction.First;
							break;
						case PcreByteOpcode.EarlyAccept:
							// Native JIT synthetic closes apply outside called groups only.
							// Ordinary completed/selected captures still use Return's journal.
							if ( activeCall < 0 ) {
								foreach ( var number in instruction.CaptureNumbers.Span ) {
									Step();
									if ( backtrack.Count > 0 ) { changes.Add( new( StateKind.Capture, number, 0, captures[number] ) ); }
									captures[number] = new( captureStarts[number], position - captureStarts[number] );
								}
							}
							if ( activeCall >= 0 && callScopes![activeCall].AssertionScope == activeScope ) {
								// Commit this invocation, retaining its guard to restore caller
								// state if a later caller instruction fails. Inner arenas pop LIFO.
								var acceptedCaller = callScopes[activeCall];
								var acceptedCallBoundary = acceptedCaller.Boundary;
								while ( backtrack.Count > acceptedCallBoundary ) { Step(); PopFrame( backtrack, assertionScopes, callScopes ); }
								var acceptedCallDepth = backtrack.Peek().AtomicDepth;
								atomicDepths.RemoveRange( acceptedCallDepth, atomicDepths.Count - acceptedCallDepth );
								if ( program.PathMarkSlot >= 0 ) {
									// Interior checkpoints are gone. Native ACCEPT commits the
									// callee's label unless its lexical local-quit flag saved the
									// entry value. Older caller checkpoints remain untouched.
									var kept = acceptedCaller.ChangeCount;
									for ( var entry = kept; entry < changes.Count; entry++ ) {
										Step();
										var change = changes[entry];
										if ( change.Kind == StateKind.Mark && change.Index == program.PathMarkSlot ) { continue; }
										changes[kept++] = change;
									}
									changes.RemoveRange( kept, changes.Count - kept );
									if ( program.Instructions[acceptedCaller.ReturnPc - 1].PropertyValue != 0 ) {
										changes.Add( new( StateKind.Mark, program.PathMarkSlot, acceptedCaller.EntryLabel, null ) );
									}
								}
								goto case PcreByteOpcode.Return;
							}
							if ( activeScope >= 0 ) {
								var acceptedScopeBoundary = assertionScopes![activeScope].Boundary + 1;
								while ( backtrack.Count > acceptedScopeBoundary ) { Step(); PopFrame( backtrack, assertionScopes, callScopes ); }
								var acceptedScopeDepth = backtrack.Peek().AtomicDepth;
								atomicDepths.RemoveRange( acceptedScopeDepth, atomicDepths.Count - acceptedScopeDepth );
								pc = instruction.First; // Bypass ReverseEnd/script-run/body suffixes.
								break;
							}
							if ( endAnchored && position != matchEnd ) {
								// A short root ACCEPT abandons this candidate, including cuts
								// and alternatives. Scoped exits above retain their own meaning.
								backtrack.Clear();
								failed = true;
								break;
							}
							// Root NOTEMPTY rejection is retryable, including synthetic closes.
							goto case PcreByteOpcode.Accept;
						case PcreByteOpcode.Return:
							var caller = callScopes![activeCall];
							var returnedCaptures = program.Instructions[caller.ReturnPc - 1].CaptureNumbers;
							// Original entries remain available to interior retry checkpoints.
							// Restore each field's oldest value once, appending inverse writes.
							// Generation tags contain no capture values and bound nested history.
							restoredFields ??= new int[captures.Length * 2 + marks.Length];
							var generation = ++returnGeneration; // Each return spends a VM step.
							var originalEnd = changes.Count;
							for ( var entry = caller.ChangeCount; entry < originalEnd; entry++ ) {
								Step();
								var change = changes[entry];
								if ( change.Kind != StateKind.Mark && change.Index == 0 ) { continue; } // Keep K/G0.
								// A successful callee contributes its winning path label; positional
								// marks still restore. Original entries remain for failed-call rollback.
								if ( change.Kind == StateKind.Mark && change.Index == program.PathMarkSlot ) { continue; }
								// Keep only selected values, not their temporary starts. Original
								// journal entries still restore values on callee/caller retry.
								if ( change.Kind == StateKind.Capture && returnedCaptures.Span.BinarySearch( change.Index ) >= 0 ) { continue; }
								var field = change.Kind == StateKind.Mark ? captures.Length * 2 + change.Index
									: change.Index * 2 + ( change.Kind == StateKind.CaptureStart ? 1 : 0 );
								if ( restoredFields[field] == generation ) { continue; }
								restoredFields[field] = generation;
								switch ( change.Kind ) {
									case StateKind.Capture:
										changes.Add( new( StateKind.Capture, change.Index, 0, captures[change.Index] ) );
										captures[change.Index] = change.PriorCapture;
										break;
									case StateKind.CaptureStart:
										changes.Add( new( StateKind.CaptureStart, change.Index, captureStarts[change.Index], null ) );
										captureStarts[change.Index] = change.PriorValue;
										break;
									case StateKind.Mark:
										changes.Add( new( StateKind.Mark, change.Index, marks[change.Index], null ) );
										marks[change.Index] = change.PriorValue;
										break;
								}
							}
							activeCall = caller.Parent;
							pc = caller.ReturnPc;
							break; // Retain the guard, body choices and their arena nodes.
						case PcreByteOpcode.Jump:
							pc = instruction.First;
							break;
						case PcreByteOpcode.PathMark:
						case PcreByteOpcode.ThenMark:
						case PcreByteOpcode.Mark:
							if ( instruction.Opcode == PcreByteOpcode.PathMark && program.TrackPathMarks ) {
								if ( backtrack.Count >= MaximumBacktrackFrames ) {
									throw new MatchLimitException( PcreByteDiagnosticCode.MatchDepthLimitExceeded, "backtrack stack limit exceeded" );
								}
								// Checkpoint before this label write so ordinary failure restores
								// the preceding label as well as discarding its lookup history.
								backtrack.Push( new Frame( pc, position, changes.Count, atomicDepths.Count,
									activeScope, FrameKind.PathMark, CallScope: activeCall ) );
							}
							if ( backtrack.Count > 0 ) {
								changes.Add( new( StateKind.Mark, instruction.First, marks[instruction.First], null ) );
							}
							marks[instruction.First] = instruction.Opcode switch {
								PcreByteOpcode.PathMark => pc + 1, PcreByteOpcode.ThenMark => backtrack.Count, _ => position
							};
							pc++;
							break;
						case PcreByteOpcode.ScriptRunCheck:
							if ( IsScriptRun( input, marks[instruction.First], position, program.Utf8,
								ref work, matchWorkLimit, workObserver, cancellationToken ) ) { pc++; }
							else { failed = true; }
							break;
						case PcreByteOpcode.Progress:
							if ( position == marks[instruction.First] ) { pc = instruction.Second; }
							else { pc++; }
							break;
						case PcreByteOpcode.AtomicEnter:
							atomicDepths.Add( backtrack.Count );
							pc++;
							break;
						case PcreByteOpcode.AtomicExit:
							var boundary = atomicDepths[^1];
							while ( backtrack.Count > boundary ) { PopFrame( backtrack, assertionScopes, callScopes ); }
							if ( backtrack.Count == 0 ) { changes.Clear(); }
							atomicDepths.RemoveAt( atomicDepths.Count - 1 );
							pc++;
							break;
						case PcreByteOpcode.AssertionEnter:
							if ( backtrack.Count >= MaximumBacktrackFrames ) {
								throw new MatchLimitException( PcreByteDiagnosticCode.MatchDepthLimitExceeded, "backtrack stack limit exceeded" );
							}
							assertionScopes ??= new();
							var guard = new Frame( instruction.First, position, changes.Count, atomicDepths.Count,
								activeScope, ( instruction.Second & 8 ) != 0 ? FrameKind.ConditionalAssertion
									: ( instruction.Second & 4 ) != 0 ? FrameKind.NonAtomicAssertion
									: ( instruction.Second & 1 ) != 0 ? FrameKind.PositiveAssertion : FrameKind.NegativeAssertion, CallScope: activeCall );
							var scopeId = assertionScopes.Count;
							// JIT narrows subject visibility at a variable lookbehind endpoint.
							var visibleEnd = !matchTimeAnchoring && ( instruction.Second & 2 ) != 0 ? position : endLimit;
							assertionScopes.Add( new( backtrack.Count, visibleEnd,
								activeScope, instruction.First, position, guard.Kind, ( instruction.Second & 16 ) != 0 ) );
							activeScope = scopeId;
							backtrack.Push( guard ); // Forces body mutations into the shared journal.
							pc++;
							break;
						case PcreByteOpcode.AssertionExit:
							// Copy before pruning: PopFrame releases the guard-owned scope node.
							var scope = assertionScopes![activeScope];
							activeScope = scope.Parent;
							if ( ( instruction.Second & 4 ) != 0 ) {
								position = scope.Position;
								pc = scope.ReturnPc;
								break; // Keep body choices and their scopes available for later failure.
							}
							while ( backtrack.Count > scope.Boundary + 1 ) { PopFrame( backtrack, assertionScopes, callScopes ); }
							var completedGuard = PopFrame( backtrack, assertionScopes, callScopes );
							position = completedGuard.Position;
							pc = ( instruction.Second & 8 ) != 0 ? instruction.First : completedGuard.Pc;
							if ( completedGuard.Kind is FrameKind.PositiveAssertion or FrameKind.ConditionalAssertion ) {
								// First body success is atomic. Keep captures, including those beyond G0,
								// journaled until any enclosing retry/guard can no longer restore them.
								if ( backtrack.Count == 0 ) { changes.Clear(); }
							} else {
								RestoreFrameState( completedGuard, changes, captures, captureStarts, marks, atomicDepths );
								failed = true; // A matching negative body makes the assertion fail.
							}
							break;
						case PcreByteOpcode.Accept:
							if ( endAnchored && position != matchEnd ) {
								// Ordinary completion can retry a saved path that consumes more.
								failed = true;
								break;
							}
							var reportedStart = captureStarts[0];
							// Reject at final acceptance so ordinary saved paths can retry.
							// K may have moved the reported start beyond the candidate start.
							if ( position == reportedStart && ( notEmpty
								|| notEmptyAtStart && position == startOffset ) ) {
								failed = true;
								break;
							}
							captures[0] = new( reportedStart, position - reportedStart );
							var labelInstruction = program.PathMarkSlot >= 0 ? marks[program.PathMarkSlot] - 1 : -1;
							return new( new PcreByteMatch( reportedStart, position - reportedStart, Array.AsReadOnly( captures ), captureInfo ) {
								MarkName = labelInstruction >= 0 ? program.Instructions[labelInstruction].Bytes : default
							}, null );
						default:
							throw new InvalidOperationException( "Compiler produced an unknown PCRE byte instruction." );
					}
					if ( !failed ) { continue; }
					while ( failed && backtrack.Count > 0 ) {
						var frame = PopFrame( backtrack, assertionScopes, callScopes );
						if ( frame.Kind != FrameKind.Branch ) { Step(); } // Resolve a guard or retry another reverse start.
						RestoreFrameState( frame, changes, captures, captureStarts, marks, atomicDepths );
						activeScope = frame.AssertionScope;
						activeCall = frame.CallScope;
						pc = frame.Pc;
						position = frame.Position;
						if ( frame.Kind == FrameKind.PathMark ) { continue; } // History has no retry edge.
						if ( frame.Kind is FrameKind.Prune or FrameKind.Skip or FrameKind.SkipReplay or FrameKind.Commit or FrameKind.Then or FrameKind.ThenAlternative ) {
							if ( !matchTimeAnchoring && activeCall >= 0
								&& frame.Pc == callScopes![activeCall].Boundary && frame.Kind != FrameKind.ThenAlternative ) {
								var cutCall = callScopes[activeCall];
								var selected = program.Instructions[cutCall.ReturnPc - 1].CaptureNumbers;
								if ( !selected.IsEmpty ) {
									// Native JIT keeps selected values completed before the cut
									// on forced callee failure. Right-side changes already rolled
									// back. Retained caller checkpoints precede this interval.
									var kept = cutCall.ChangeCount;
									for ( var entry = kept; entry < changes.Count; entry++ ) {
										Step();
										var change = changes[entry];
										if ( change.Kind == StateKind.Capture && selected.Span.BinarySearch( change.Index ) >= 0 ) { continue; }
										changes[kept++] = change;
									}
									changes.RemoveRange( kept, changes.Count - kept );
								}
							}
							if ( frame.Kind == FrameKind.Skip && frame.Pc == 0 && frame.Position > start ) {
								skipRestart = frame.Position;
							}
							while ( backtrack.Count > frame.Pc ) { Step(); PopFrame( backtrack, assertionScopes, callScopes ); }
							if ( frame.Kind == FrameKind.SkipReplay ) {
								replayNamedSkip = true;
								break;
							}
							// Delay root termination until this cut is actually reached. A later
							// PRUNE/SKIP or a successful atomic scope may discard COMMIT first.
							if ( frame.Kind == FrameKind.Commit && frame.Pc == 0 ) {
								if ( !interpretedUtfFragments ) { return new( null, null ); }
								committedFragment = true;
								failed = true;
								break;
							}
							failed = true;
							continue; // Resolve the retained catch guard, or fail this candidate.
						}
						if ( frame.Kind == FrameKind.ReverseRetry && frame.ReverseRemaining > 0 ) {
							// The popped retry frees its slot; keep only one pending next start.
							backtrack.Push( frame with {
								Position = NextReverseStart( input.Span, position, program.Utf8, surrogateLiterals ),
								ReverseRemaining = (byte)( frame.ReverseRemaining - 1 )
							} );
						}
						// A normal retry or exhausted negative body resumes. An exhausted
						// positive body propagates failure to the next enclosing saved path.
						failed = frame.Kind is FrameKind.PositiveAssertion or FrameKind.NonAtomicAssertion or FrameKind.Call;
						if ( backtrack.Count == 0 ) { changes.Clear(); }
					}
					if ( failed ) { break; }
				}
				if ( replayNamedSkip ) {
					// PCRE2 ignores every named SKIP visited so far, including earlier resolved names.
					ignoredNamedSkips = namedSkipCount;
					if ( anchored || program.LeadingStartAnchor ) {
						if ( interpretedUtfFragments ) { break; }
						return new( null, null );
					}
					if ( program.FirstLine && IsNewlineAt( program, input.Span, start, fragment.End ) ) { break; }
					// The interpreter applies its CRLF bumpalong rule even to a same-offset retry.
					if ( AdvancePastCrLfTail( program, input.Span, start, fragment.End, startOffset ) == start ) {
						forcedSkipStart = start;
						start--; // Reenter with clean candidate storage and the same work budget.
					}
					continue;
				}
				// Native interpreter COMMIT reaches ENDLOOP, then retries later fragments.
				if ( committedFragment ) { break; }
				// Only an ordinary bumpalong clears replay suppression. Forward SKIP keeps it.
				if ( ignoredNamedSkips != 0 && skipRestart <= start ) { ignoredNamedSkips = 0; }
				// Interpreter anchoring stops within this fragment before a SKIP retry.
				if ( matchTimeAnchoring && anchored ) {
					if ( interpretedUtfFragments ) { break; }
					return new( null, null );
				}
				if ( skipRestart > start ) {
					if ( matchTimeAnchoring ) {
						// The interpreter tests the failed candidate before changing its start.
						if ( program.FirstLine && IsNewlineAt( program, input.Span, start, fragment.End ) ) { break; }
						skipRestart = AdvancePastCrLfTail( program, input.Span, skipRestart, fragment.End, startOffset );
						firstLinePosition = skipRestart;
					}
					// JIT enters the explicit target directly. The loop increment supplies the entry byte.
					forcedSkipStart = skipRestart;
					start = skipRestart - 1;
					continue;
				}
				// Anchoring constrains ordinary candidate retries, not forced SKIP entry.
				if ( anchored ) {
					if ( interpretedUtfFragments ) { break; }
					return new( null, null );
				}
				if ( program.UtfCrLfStartClass && !program.ExplicitNewline
					&& ( program.Newline is PcreByteNewline.AnyCrLf or PcreByteNewline.CrLf or PcreByteNewline.Any )
					&& utfRepeatEnd > start && utfRepeatEnd < fragment.End - 1
					&& input.Span[utfRepeatEnd] == (byte)'\r' && input.Span[utfRepeatEnd + 1] == (byte)'\n' ) {
					// Native root-leading PLUS or a first star resumes at its repeat end.
					// Its scalar comparisons were already charged; no rescan is needed.
					start = utfRepeatEnd + 1;
				}
				if ( ( program.Newline is PcreByteNewline.AnyCrLf or PcreByteNewline.CrLf or PcreByteNewline.Any )
					&& !program.ExplicitNewline && !program.ByteCrLfPrefix
					&& !program.UtfCrLfStartClass
					&& start + 1 < fragment.End && input.Span[start] == (byte)'\r'
					&& input.Span[start + 1] == (byte)'\n' ) {
					// After a failed unanchored attempt at CR, PCRE2 advances past
					// the entire pair unless the pattern explicitly mentions CR or LF.
					// Unconditional byte prefix proofs bypass this guard. Longer
					// restrictive prefixes permit LF only when their sole literal
					// rejects CR before native VM entry. Charge that comparison too.
					// A mandatory UTF positive start class excluding CR likewise lets
					// native prefix/bitmap scanning enter at LF without CR bumpalong.
					var skipLf = true;
					if ( program.ByteCrLfPrefixLiteral is { } prefix ) {
						Step(); // Attempted fixed byte-prefix literal comparison.
						skipLf = prefix.ByteOffset < fragment.End - start
							&& ScalarEquals( input.Span[start + prefix.ByteOffset], prefix.Value,
								prefix.Caseless, program.Ucp, prefix.CaselessRestricted );
					}
					if ( skipLf ) { start++; }
				}
			}
			}
			return new( null, null );
		} catch ( MatchLimitException error ) {
			return new( null, new( error.Code, 0, error.Message ) );
		}
	}

	private static int AdvancePastCrLfTail( PcreByteProgram program, ReadOnlySpan<byte> input, int target, int end, int startOffset ) {
		if ( target > startOffset && !program.ExplicitNewline
			&& ( program.Newline is PcreByteNewline.CrLf or PcreByteNewline.AnyCrLf or PcreByteNewline.Any )
			&& PcreByteNewlineExtensions.IsCrLf( input, target - 1, end ) ) {
			return target + 1;
		}
		return target;
	}

	private static bool IsNewlineAt( PcreByteProgram program, ReadOnlySpan<byte> input, int position, int end ) {
		if ( position >= end ) { return false; }
		return program.Newline switch {
			PcreByteNewline.Any => PcreByteNewlineExtensions.AnyLengthAt( input, position, end, program.Utf8 ) > 0,
			PcreByteNewline.CrLf => PcreByteNewlineExtensions.IsCrLf( input, position, end ),
			_ => input[position] == program.NewlineCharacter || program.AnyCrLfNewline && input[position] == (byte)'\r'
		};
	}

	private static void RestoreFrameState( Frame frame, List<StateChange> changes,
		PcreByteCapture?[] captures, int[] captureStarts, int[] marks,
		List<int> atomicDepths ) {
		for ( var changeIndex = changes.Count - 1; changeIndex >= frame.ChangeCount; changeIndex-- ) {
			var change = changes[changeIndex];
			switch ( change.Kind ) {
				case StateKind.Capture: captures[change.Index] = change.PriorCapture; break;
				case StateKind.CaptureStart: captureStarts[change.Index] = change.PriorValue; break;
				case StateKind.Mark: marks[change.Index] = change.PriorValue; break;
				default: throw new InvalidOperationException( "Unknown PCRE journal entry." );
			}
		}
		changes.RemoveRange( frame.ChangeCount, changes.Count - frame.ChangeCount );
		atomicDepths.RemoveRange( frame.AtomicDepth, atomicDepths.Count - frame.AtomicDepth );
	}

	private static Frame PopFrame( Stack<Frame> backtrack, List<AssertionScope>? assertionScopes, List<CallScope>? callScopes ) {
		var frame = backtrack.Pop();
		if ( frame.Kind is FrameKind.PositiveAssertion or FrameKind.NegativeAssertion
			or FrameKind.NonAtomicAssertion or FrameKind.ConditionalAssertion ) {
			// Guards and arena nodes share creation/LIFO order, including inactive siblings.
			var scopes = assertionScopes!;
			scopes.RemoveAt( scopes.Count - 1 );
		}
		if ( frame.Kind == FrameKind.Call ) { callScopes!.RemoveAt( callScopes.Count - 1 ); }
		return frame;
	}

	private static bool MissingRequiredLiteralSuffixWithinBudget( PcreByteProgram program,
		ReadOnlySpan<byte> input, int startOffset, int matchWorkLimit ) {
		var candidates = (long)input.Length - startOffset + 1;
		// UTF-8 inspection costs at most input.Length steps. For one class+ and
		// at most eight literal scalars, 256 steps per candidate and remaining
		// byte exceeds the VM's no-match work even with invalid fragments.
		// At most 64 bytes after the start also keep backtracking below the depth limit.
		return candidates <= 64
			&& input.Length + 256L * candidates * candidates <= matchWorkLimit
			&& input.Slice( startOffset ).IndexOf( program.RequiredLiteralSuffix.Span ) < 0;
	}

	private readonly record struct Fragment( int Start, int End );

	private static void RetainSearchFragment( List<Fragment> fragments, Fragment fragment, bool anchored, int startOffset ) {
		// Validation still visits the whole subject. Anchored search can enter only
		// its first relevant fragment; a forward SKIP stays within that fragment.
		// Keep an end exactly at startOffset for preceding assertion context.
		if ( !anchored || fragments.Count == 0 && fragment.End >= startOffset ) {
			fragments.Add( fragment );
		}
	}

	private static int NextReverseStart( ReadOnlySpan<byte> input, int position, bool utf8, bool surrogateLiterals ) {
		if ( !utf8 ) { return position + 1; }
		PcreUtf8.TryReadLiteral( input, position, surrogateLiterals, out _, out var width );
		return position + width;
	}

	private static bool ScalarEquals( int left, int right, bool caseless, bool unicode, bool restricted ) {
		if ( left == right ) { return true; }
		if ( !caseless ) { return false; }
		if ( unicode ) { return PcreUtf8CaseFold.Equals( left, right, restricted ); }
		if ( left is >= (byte)'A' and <= (byte)'Z' ) { left = (byte)( left + 32 ); }
		if ( right is >= (byte)'A' and <= (byte)'Z' ) { right = (byte)( right + 32 ); }
		return left == right;
	}


	private enum StateKind : byte { Capture, CaptureStart, Mark }

	private readonly record struct StateChange(
		StateKind Kind, int Index, int PriorValue, PcreByteCapture? PriorCapture
	);

	private enum FrameKind : byte { Branch, PositiveAssertion, NegativeAssertion, ReverseRetry, NonAtomicAssertion, ConditionalAssertion, Call, Prune, Skip, SkipReplay, Commit, Then, ThenAlternative, PathMark }

	private readonly record struct CallScope( int ReturnPc, int Parent, int ChangeCount, int Number, int AssertionScope, int Boundary, int EntryLabel );

	private readonly record struct AssertionScope( int Boundary, int LimitEnd, int Parent, int ReturnPc, int Position, FrameKind Kind, bool IsolatePathMarks );

	private readonly record struct Frame(
		int Pc, int Position, int ChangeCount, int AtomicDepth, int AssertionScope,
		FrameKind Kind = FrameKind.Branch, byte ReverseRemaining = 0, int CallScope = -1
	);

	private sealed class MatchLimitException( PcreByteDiagnosticCode code, string message ) : Exception( message ) {
		internal PcreByteDiagnosticCode Code { get; } = code;
	}
}
