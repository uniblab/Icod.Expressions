namespace Icod.Expressions.Pcre;

/// <summary>Immutable width-16 sequence; matching reads the original subject view.</summary>
internal sealed class PcreCompiledWordPattern {
	private const int MaximumBacktrackFrames = 1024;
	private readonly PcreByteNode[][] branches;
	private readonly PcreWordOptions options;
	private readonly int validationLookbehind;
	private readonly PcreByteNewline newline;
	private readonly bool explicitNewline;
	private readonly bool allowsLfEntry;
	private readonly PcreWordPrefix? fixedPrefix;
	private readonly bool bsrAnyCrLf;
	private readonly IReadOnlyDictionary<int, PcreByteNode[]>? callTargets;
	private readonly bool hasNonAtomicAccept;
	private readonly IReadOnlySet<int>? cutTargets;
	private readonly PcreByteLiteral? commitStart;
	private readonly bool hasThen;
	private readonly bool hasNamedSkip;
	private readonly bool leadingStartAnchor;
	public PcreCaptureInfo CaptureInfo { get; }
	internal PcreCompiledWordPattern( PcreByteNode[] nodes, PcreWordOptions options ) : this( [nodes], options ) { }
	internal PcreCompiledWordPattern( PcreByteNode[][] branches, PcreWordOptions options, int validationLookbehind = 0,
		PcreByteNewline newline = PcreByteNewline.Lf, bool bsrAnyCrLf = false, PcreCaptureInfo? captureInfo = null,
		IReadOnlyDictionary<int, PcreByteNode[]>? callTargets = null, bool hasNonAtomicAccept = false,
		IReadOnlySet<int>? cutTargets = null, PcreByteLiteral? commitStart = null, bool hasThen = false,
		bool hasNamedSkip = false, bool leadingStartAnchor = false, bool explicitNewline = false, bool allowsLfEntry = false, PcreWordPrefix? fixedPrefix = null ) {
		this.branches = branches;
		this.options = options;
		this.validationLookbehind = validationLookbehind;
		this.newline = newline;
		this.explicitNewline = explicitNewline;
		this.allowsLfEntry = allowsLfEntry;
		this.fixedPrefix = fixedPrefix;
		this.bsrAnyCrLf = bsrAnyCrLf;
		this.callTargets = callTargets;
		this.hasNonAtomicAccept = hasNonAtomicAccept;
		this.cutTargets = cutTargets;
		this.commitStart = commitStart;
		this.hasThen = hasThen;
		this.hasNamedSkip = hasNamedSkip;
		this.leadingStartAnchor = leadingStartAnchor;
		CaptureInfo = captureInfo ?? new( 0, new Dictionary<string, List<int>>(), default );
	}

	internal PcreWordMatchResult Match( ReadOnlyMemory<char> subject, int startOffset = 0,
		CancellationToken cancellationToken = default ) => Match( subject, default, startOffset, cancellationToken );

	internal PcreWordMatchResult Match( ReadOnlyMemory<char> subject, PcreMatchPolicy policy, int startOffset = 0,
		CancellationToken cancellationToken = default ) {
		cancellationToken.ThrowIfCancellationRequested();
		if ( startOffset < 0 || startOffset > subject.Length ) {
			return new( null, new( PcreWordDiagnosticCode.InvalidStartOffset, startOffset ) );
		}
		var work = 0;
		bool Step() {
			cancellationToken.ThrowIfCancellationRequested();
			if ( work >= options.MatchWorkLimit ) { return false; }
			work++;
			options.WorkObserver?.Invoke( work );
			cancellationToken.ThrowIfCancellationRequested();
			return true;
		}
		PcreWordMatchResult Exhausted() => new( null, new( PcreWordDiagnosticCode.MatchWorkLimitExceeded, 0 ) );
		var input = subject.Span;
		var visibleEnd = input.Length;
		var reverseStart = 0;
		var interpreted = hasNonAtomicAccept || policy.Anchored || policy.EndAnchored;
		if ( options.Utf16 ) {
			// Retained assertion widths extend native validation to the left. Word
			// boundaries and absolute-start assertions require at least one character.
			if ( startOffset > 0 && startOffset < input.Length && Char.IsLowSurrogate( input[startOffset] ) ) {
				return new( null, new( PcreWordDiagnosticCode.InvalidStartOffset, startOffset ) );
			}
			var validationStart = startOffset;
			for ( var previous = 0; previous < validationLookbehind && validationStart > 0; previous++ ) {
				do {
					if ( !Step() ) { return Exhausted(); }
					validationStart--;
				} while ( validationStart > 0 && Char.IsLowSurrogate( input[validationStart] ) );
			}
			for ( var offset = validationStart; offset < input.Length; ) {
				if ( !Step() ) { return Exhausted(); }
				if ( !PcreUtf16.TryRead( input, offset, true, out _, out var width ) ) {
					return new( null, new( PcreWordDiagnosticCode.InvalidUtf16Subject, offset ) );
				}
				offset += width;
			}
			// These call-time flags select the pinned interpreter, whose reverse
			// reads and word boundaries stop at the UTF validation window.
			if ( interpreted ) { reverseStart = validationStart; }
		}
		var anchored = options.Policy.Anchored || policy.Anchored;
		var endAnchored = options.Policy.EndAnchored || policy.EndAnchored;
		var notEmpty = options.Policy.NotEmpty || policy.NotEmpty;
		var notEmptyAtStart = options.Policy.NotEmptyAtStart || policy.NotEmptyAtStart;
		var notBol = options.Policy.NotBol || policy.NotBol;
		var notEol = options.Policy.NotEol || policy.NotEol;
		var skipCrLfTail = !explicitNewline && newline is PcreByteNewline.CrLf or PcreByteNewline.AnyCrLf or PcreByteNewline.Any;
		bool MatchAnchor( PcreByteNode node, ReadOnlySpan<char> source, int at ) => node switch {
			PcreByteAnchor anchor => !anchor.IsDisabled
				&& ( !anchor.IsLineBoundary || !( anchor.IsStart ? notBol : notEol ) )
				&& at == ( anchor.IsStart ? 0 : source.Length ),
			PcreByteLineAnchor { IsStart: true } start =>
				at == 0 && !start.SuppressSubjectBoundary && !notBol
				|| at > 0 && ( at < source.Length || start.AllowSubjectEnd ) && PcreWordNewline.IsBefore( newline, source, at ),
			PcreByteLineAnchor end =>
				at == source.Length && !end.SuppressSubjectBoundary && !notEol
				|| at < source.Length && PcreWordNewline.LengthAt( newline, source, at ) > 0,
			PcreByteEndBeforeFinalLf end => !end.IsDisabled && ( !end.IsLineBoundary || !notEol )
				&& ( at == source.Length || PcreWordNewline.LengthAt( newline, source, at ) is var length && length > 0 && length == source.Length - at ),
			_ => throw new InvalidOperationException( "Unsupported word anchor." )
		};
		int MatchBoundary( PcreByteWordBoundary boundary, ReadOnlySpan<char> source, int at ) {
			var kind = boundary.UnicodeWord ? PcreByteScalarClassKind.UnicodeWord : PcreByteScalarClassKind.AsciiWord;
			var beforeWord = false;
			if ( at > reverseStart ) {
				if ( !Step() ) { return -1; }
				beforeWord = PcreUtf16.TryReadPrevious( source, at, options.Utf16, out var before, out _ )
					&& PcreScalarClassMembership.Contains( kind, before );
			}
			var afterWord = false;
			if ( at < source.Length ) {
				if ( !Step() ) { return -1; }
				afterWord = PcreUtf16.TryRead( source, at, options.Utf16, out var after, out _ )
					&& PcreScalarClassMembership.Contains( kind, after );
			}
			return ( beforeWord != afterWord ) == boundary.IsBoundary ? 1 : 0;
		}
		Stack<BacktrackFrame>? frames = null;
		var activeCall = -1;
		var mark = String.Empty;
		var markVersion = 0;
		var nextMarkVersion = 0;
		Dictionary<int, string>? acceptedMarks = null;
		void SetMark( string value ) { mark = value; markVersion = ++nextMarkVersion; }
		void RestoreMark( int version, string value ) {
			markVersion = version;
			mark = acceptedMarks is not null && acceptedMarks.TryGetValue( version, out var committed ) ? committed : value;
		}
		// Slot zero journals the reported start until a match supplies its end.
		var captures = new PcreWordCapture?[CaptureInfo.CaptureCount + 1];
		Dictionary<int, PcreWordCapture?>? cutCaptures = null;
		var pruneLiteralEnd = -1;
		var skipRestart = -1;
		var namedSkipCount = 0;
		var ignoredNamedSkips = 0;
		var candidateMark = String.Empty;
		int MatchReference( PcreByteBackreference reference, ReadOnlySpan<char> source, int position, out int end ) {
			end = position;
			var capture = captures[reference.Number];
			if ( !reference.DuplicateNumbers.IsEmpty ) {
				capture = null;
				foreach ( var number in reference.DuplicateNumbers.Span ) {
					if ( !Step() ) { return -1; } // Declaration-ordered participation lookup.
					capture = captures[number];
					if ( capture.HasValue ) { break; } // Empty captures participate too.
				}
			}
			// A comparison failure cannot select a later duplicate-name slot.
			if ( capture is not { } span ) { return reference.UnsetMatchesEmpty ? 1 : 0; }
			var capturedEnd = span.CodeUnitIndex + span.CodeUnitLength;
			var candidate = position;
			for ( var offset = span.CodeUnitIndex; offset < capturedEnd; ) {
				if ( !Step() ) { return -1; } // One captured scalar/raw-unit comparison.
				if ( !PcreUtf16.TryRead( source, offset, options.Utf16, out var left, out var leftWidth )
					|| leftWidth > capturedEnd - offset
					|| !PcreUtf16.TryRead( source[..visibleEnd], candidate, options.Utf16, out var right, out var rightWidth )
					|| !ScalarEquals( left, right, reference.Caseless, reference.CaselessRestricted ) ) { return 0; }
				offset += leftWidth;
				candidate += rightWidth;
			}
			end = candidate;
			return 1;
		}
		List<CaptureChange>? changes = null;
		int[]? restoredCaptures = null;
		var returnGeneration = 0;
		void SaveCapture( int number, int start, int end ) {
			if ( frames is { Count: > 0 } ) {
				changes ??= new();
				changes.Add( new( number, captures[number] ) );
			}
			captures[number] = new( start, end - start );
		}
		bool RestoreCaptures( int changeCount ) {
			if ( changes is null ) { return true; }
			while ( changes.Count > changeCount ) {
				if ( !Step() ) { return false; }
				var change = changes[^1];
				captures[change.Number] = change.Previous;
				changes.RemoveAt( changes.Count - 1 );
			}
			return true;
		}
		bool PushFrame( BacktrackFrame frame ) {
			frames ??= new();
			if ( frames.Count >= MaximumBacktrackFrames ) { return false; }
			frames.Push( frame with { ChangeCount = changes?.Count ?? 0, VisibleEnd = visibleEnd,
				ActiveCall = activeCall, Mark = mark, MarkVersion = markVersion } );
			return true;
		}
		bool ReturnCaptures( int checkpoint, ReadOnlyMemory<int> selected ) {
			if ( changes is null ) { return true; }
			restoredCaptures ??= new int[captures.Length];
			var generation = ++returnGeneration;
			var originalEnd = changes.Count;
			// Preserve the original history for callee retries. Restore each slot's
			// oldest value once and journal the inverse write for later backtracking.
			for ( var entry = checkpoint; entry < originalEnd; entry++ ) {
				if ( !Step() ) { return false; }
				var change = changes[entry];
				if ( change.Number == 0 || selected.Span.BinarySearch( change.Number ) >= 0
					|| restoredCaptures[change.Number] == generation ) { continue; }
				restoredCaptures[change.Number] = generation;
				changes.Add( new( change.Number, captures[change.Number] ) );
				captures[change.Number] = change.Previous;
			}
			return true;
		}
		// -2: frame limit; -1: work exhausted; 0: failed; 1: match; 2: next candidate; 3: committed failure; 4: named-SKIP replay.
		int TrySequence( PcreByteNode[] nodes, ReadOnlySpan<char> source, int candidate, out int end ) {
			frames?.Clear();
			changes?.Clear();
			Array.Clear( captures );
			if ( cutCaptures is not null ) {
				foreach ( var (number, capture) in cutCaptures ) {
					if ( !Step() ) { end = candidate; return -1; }
					captures[number] = capture;
				}
			}
			visibleEnd = source.Length;
			activeCall = -1;
			RestoreMark( 0, candidateMark );
			Continuation? continuation = null;
			var index = 0;
			var position = candidate;
			end = candidate;
			bool RejectEmpty( int at ) => at == ( captures[0]?.CodeUnitIndex ?? candidate )
				&& ( notEmpty || notEmptyAtStart && at == startOffset );
			bool PushPathMark( string name ) {
				if ( hasNamedSkip && !PushFrame( new( nodes, index, position, 0, 0, continuation,
					FrameKind.PathMark, HistoryName: name ) ) ) { return false; }
				SetMark( name );
				return true;
			}
			void EnterConditionalArm( PcreWordConditional conditional, bool value ) {
				continuation = new( nodes, index + 1, continuation );
				nodes = value ? conditional.WhenTrue : conditional.WhenFalse;
				index = 0;
			}
			bool ContinueGroupRepeat( PcreWordGroupRepeat repeat, int count ) {
				if ( count == repeat.Maximum ) { index++; return true; }
				if ( count >= repeat.Minimum ) {
					if ( !PushFrame( new( nodes, index, position, position, count, continuation,
						repeat.Greedy ? FrameKind.RepeatExit : FrameKind.RepeatGrow ) ) ) { return false; }
					if ( !repeat.Greedy ) { index++; return true; }
				}
				continuation = new( nodes, index, continuation, repeat, count, position );
				nodes = repeat.Body;
				index = 0;
				return true;
			}
			while ( true ) {
				if ( index == nodes.Length ) {
					if ( continuation is { } parent ) {
						if ( !Step() ) { return -1; } // Return from a completed group branch.
						if ( parent.CaptureNumber > 0 ) { SaveCapture( parent.CaptureNumber, parent.Start, position ); }
						nodes = parent.Nodes;
						index = parent.Index;
						continuation = parent.Parent;
						if ( parent.GuardBoundary >= 0 || parent.CallCheckpoint >= 0 ) { visibleEnd = parent.SavedEnd; }
						if ( parent.CheckAssertionEnd && parent.AssertionEnd >= 0 && position != parent.AssertionEnd ) {
							// A shorter/longer body must retry before the assertion can commit.
						} else if ( parent.CallCheckpoint >= 0 ) {
							if ( !ReturnCaptures( parent.CallCheckpoint, parent.ReturnedCaptures ) ) { return -1; }
							activeCall = parent.Caller;
							continue; // Keep consumption, the call guard and all callee choices.
						} else if ( parent.GuardBoundary >= 0 ) {
							if ( parent.NonAtomicAssertion ) {
								// Keep the body's choices and guard so a failing suffix can
								// re-enter it with the original capture checkpoint and return.
								position = parent.Start;
								continue;
							}
							// Commit only choices made inside this assertion or atomic group.
							var guardFrames = frames!;
							while ( guardFrames.Count > parent.GuardBoundary + 1 ) {
								if ( !Step() ) { return -1; }
								guardFrames.Pop();
							}
							var guard = guardFrames.Pop();
							if ( guard.Kind != FrameKind.AtomicGroup ) { position = guard.Position; }
							if ( guard.Kind is FrameKind.PositiveCondition or FrameKind.NegativeCondition ) {
								// Both signs keep captures when the condition body matches.
								// The interpreter restores the start before entering either arm.
								if ( interpreted ) { captures[0] = parent.EntryMatchStart; }
								if ( guardFrames.Count == 0 ) { changes?.Clear(); }
								activeCall = guard.ActiveCall;
								EnterConditionalArm( (PcreWordConditional)nodes[index], guard.Kind == FrameKind.PositiveCondition );
								continue;
							}
							if ( guard.Kind is FrameKind.PositiveAssertion or FrameKind.AtomicGroup ) {
								if ( guardFrames.Count == 0 ) { changes?.Clear(); }
								continue; // Keep successful captures, even beyond the outer match.
							}
							if ( !RestoreCaptures( guard.ChangeCount ) ) { return -1; }
							RestoreMark( guard.MarkVersion, guard.Mark! );
							if ( guardFrames.Count == 0 ) { changes?.Clear(); }
							// A matching negative body fails into the enclosing retry stack.
						} else if ( parent.Repeat is { } completedRepeat ) {
							// Earlier required copies may be empty. Native group repeats
							// check the final required copy before entering the optional loop.
							if ( completedRepeat.Maximum == int.MaxValue && position == parent.Start
								&& ( parent.Count >= completedRepeat.Minimum || completedRepeat.CheckFinalRequiredProgress
									&& parent.Count + 1 >= completedRepeat.Minimum ) ) {
								index++;
							} else if ( !ContinueGroupRepeat( completedRepeat, parent.Count + 1 ) ) { return -2; }
							continue;
						} else {
							continue;
						}
					} else {
						if ( !Step() ) { return -1; } // Completion, including policy rejection.
						if ( ( !endAnchored || position == source.Length ) && !RejectEmpty( position ) ) {
							end = position; return 1;
						}
					}
				} else if ( nodes[index] is PcreWordConditional conditional ) {
					if ( !Step() ) { return -1; } // Condition selection or assertion-test guard.
					if ( conditional.Assertion is { } test ) {
						var boundary = frames?.Count ?? 0;
						if ( !PushFrame( new( nodes, index, position, position, 0, continuation,
							test.IsPositive ? FrameKind.PositiveCondition : FrameKind.NegativeCondition ) ) ) { return -2; }
						continuation = new( nodes, index, continuation, Start: position, GuardBoundary: boundary,
							AssertionEnd: test.IsBehind ? position : -1, SavedEnd: visibleEnd, CheckAssertionEnd: false, CatchesCut: true,
						IsolatePathMarks: hasNamedSkip && test.Group.IsolatesPathMarks, EntryMatchStart: captures[0] );
					if ( test.VariableBehind && !interpreted ) { visibleEnd = position; }
						nodes = test.Body;
						index = 0;
						continue;
					}
					var value = conditional.Value;
					if ( conditional.Kind == PcreWordConditionKind.Capture
						|| conditional.Kind == PcreWordConditionKind.Recursion && activeCall >= 0 ) {
						value = conditional.Kind == PcreWordConditionKind.Recursion && conditional.Numbers.IsEmpty;
						foreach ( var number in conditional.Numbers.Span ) {
							if ( !Step() ) { return -1; } // Ordered duplicate-name participation lookup.
							if ( conditional.Kind == PcreWordConditionKind.Capture ? captures[number].HasValue : activeCall == number ) {
								value = true;
								break;
							}
						}
					}
					EnterConditionalArm( conditional, value );
					continue;
				} else if ( nodes[index] is PcreWordAssertion assertion ) {
					if ( !Step() ) { return -1; } // Assertion entry and capture rollback guard.
					var boundary = frames?.Count ?? 0;
					if ( !PushFrame( new( nodes, index + 1, position, position, 0, continuation,
						assertion.IsPositive ? FrameKind.PositiveAssertion : FrameKind.NegativeAssertion ) ) ) { return -2; }
					continuation = new( nodes, index + 1, continuation, Start: position, GuardBoundary: boundary,
						AssertionEnd: assertion.IsBehind ? position : -1, NonAtomicAssertion: assertion.NonAtomic,
						SavedEnd: visibleEnd, CheckAssertionEnd: assertion.IsBehind && assertion.NonAtomic
							&& !interpreted, CatchesCut: !assertion.IsPositive,
						IsolatePathMarks: hasNamedSkip && !assertion.NonAtomic && assertion.Group.IsolatesPathMarks,
						EntryMatchStart: captures[0] );
					// Native JIT caps nested reads at a variable lookbehind's endpoint.
					if ( assertion.VariableBehind && !interpreted ) { visibleEnd = position; }
					nodes = assertion.Body;
					index = 0;
					continue;
				} else if ( nodes[index] is PcreWordAtomic atomic ) {
					if ( !Step() ) { return -1; } // Atomic entry and capture rollback guard.
					var boundary = frames?.Count ?? 0;
					if ( !PushFrame( new( nodes, index + 1, position, position, 0, continuation, FrameKind.AtomicGroup ) ) ) { return -2; }
					continuation = new( nodes, index + 1, continuation, GuardBoundary: boundary,
						SavedEnd: visibleEnd, AtomicGroup: true );
					nodes = atomic.Body;
					index = 0;
					continue;
				} else if ( nodes[index] is PcreByteSubroutineCall call ) {
					if ( !Step() ) { return -1; } // Call entry and caller capture checkpoint.
					var checkpoint = changes?.Count ?? 0;
					var boundary = frames?.Count ?? 0;
					if ( !PushFrame( new( nodes, index + 1, position, position, 0, continuation, FrameKind.Subroutine ) ) ) { return -2; }
					continuation = new( nodes, index + 1, continuation, SavedEnd: visibleEnd,
						CallCheckpoint: checkpoint, ReturnedCaptures: call.ReturnedCaptures, Caller: activeCall, CallBoundary: boundary );
					activeCall = call.Number;
					nodes = callTargets![call.Number];
					index = 0;
					continue;
				} else if ( nodes[index] is PcreWordSuffixChoices firstChoices ) {
					if ( !Step() ) { return -1; } // Ordered suffix choice admission.
					if ( !PushFrame( new( nodes, index, position, position, 1, continuation ) ) ) { return -2; }
					nodes = firstChoices.Alternatives[0];
					index = 0;
					continue;
				} else if ( nodes[index] is PcreWordGroup firstGroup ) {
					if ( !Step() ) { return -1; } // Group entry.
					var thenBoundary = hasThen && firstGroup.SourceBranchStarts is { Length: > 2 } ? frames?.Count ?? 0 : -1;
					if ( firstGroup.Alternatives.Length > 1 ) {
						if ( !Step() ) { return -1; } // Ordered group branch admission.
						if ( !PushFrame( new( nodes, index, position, position, 1, continuation ) ) ) { return -2; }
					}
					continuation = new( nodes, index + 1, continuation, Start: position, CaptureNumber: firstGroup.CaptureNumber,
						AssertionEnd: firstGroup.CheckAssertionEnds?[0] == true ? continuation!.AssertionEnd : -1,
						ThenBoundary: thenBoundary, ThenNext: thenBoundary >= 0 ? firstGroup.SourceBranchStarts![1] : -1,
						ThenGroup: thenBoundary >= 0 ? firstGroup : null, ThenEntryMark: mark, ThenEntryMarkVersion: markVersion );
					nodes = firstGroup.Alternatives[0];
					index = 0;
					continue;
				} else if ( nodes[index] is PcreWordGroupRepeat groupRepeat ) {
					if ( !Step() ) { return -1; } // Repeated-group admission and first count decision.
					if ( !ContinueGroupRepeat( groupRepeat, 0 ) ) { return -2; }
					continue;
				} else {
					if ( !Step() ) { return -1; } // Comparison, repeat admission or assertion.
					var matched = true;
					switch ( nodes[index] ) {
						case PcreWordPruneScan scan:
							if ( scan.Start ) { matched = interpreted || position >= pruneLiteralEnd; }
							else if ( !interpreted ) { pruneLiteralEnd = Math.Max( pruneLiteralEnd, position ); }
							break;
						case PcreWordCut cut:
							if ( interpreted && cut.SkipName is not null ) {
								namedSkipCount++;
								if ( namedSkipCount <= ignoredNamedSkips ) { break; }
							}
							Continuation? thenOwner = null;
							if ( cut.Kind == PcreWordCutKind.Then ) {
								for ( var scope = continuation; scope is not null; scope = scope.Parent ) {
									if ( !Step() ) { return -1; } // Only source alternatives own THEN.
									if ( scope.ThenBoundary >= 0 ) { thenOwner = scope; break; }
									if ( scope.CallCheckpoint >= 0 || scope.GuardBoundary >= 0 && !scope.AtomicGroup ) { break; }
								}
							}
							var cutScope = continuation;
							var historyBoundary = 0;
							while ( cutScope is not null ) {
								if ( !Step() ) { return -1; } // Only active scopes catch a cut.
								if ( !interpreted && cutScope.IsolatePathMarks ) { historyBoundary = Math.Max( historyBoundary, cutScope.GuardBoundary + 1 ); }
								if ( cutScope.CallCheckpoint >= 0 || cutScope.CatchesCut
									|| cut.Kind == PcreWordCutKind.Then && cutScope.GuardBoundary >= 0 && !cutScope.AtomicGroup
										&& ( interpreted || thenOwner is not null || activeCall < 0 || cutScope.NonAtomicAssertion ) ) { break; }
								cutScope = cutScope.Parent;
							}
							var cutBoundary = cutScope is null ? 0
								: ( cutScope.CallCheckpoint >= 0 ? cutScope.CallBoundary : cutScope.GuardBoundary ) + 1;
							if ( thenOwner is not null ) {
								cutBoundary = thenOwner.ThenBoundary + ( thenOwner.ThenNext < thenOwner.ThenGroup!.Alternatives.Length ? 1 : 0 );
								cutScope = thenOwner;
							}
							var skipPosition = position;
							var unmatchedNamedSkip = false;
							if ( cut.SkipName is { } skipName ) {
								var resolveAssertion = interpreted && cutScope is { CallCheckpoint: < 0 };
								if ( resolveAssertion ) {
									// An interpreted name may resolve outside the catching assertion.
									cutScope = continuation;
									while ( cutScope is not null && cutScope.CallCheckpoint < 0 ) {
										if ( !Step() ) { return -1; }
										cutScope = cutScope.Parent;
									}
									cutBoundary = cutScope is null ? 0 : cutScope.CallBoundary + 1;
									historyBoundary = cutBoundary;
								}
								// JIT catches local quits before lookup; only root history is searched.
								if ( cutBoundary == 0 || resolveAssertion ) {
									skipPosition = -1;
									var historyDepth = frames?.Count ?? 0;
									if ( frames is not null ) {
										foreach ( var history in frames ) {
											if ( historyDepth-- <= historyBoundary ) { break; }
											if ( !Step() ) { return -1; } // Charge each inspected history frame.
											if ( history.Kind != FrameKind.PathMark || history.HistoryName != skipName
												|| !interpreted && history.ActiveCall != activeCall ) { continue; }
											skipPosition = history.Position;
											if ( resolveAssertion ) {
												for ( var scope = history.Continuation; scope is not null; scope = scope.Parent ) {
													if ( !Step() ) { return -1; }
													if ( scope.GuardBoundary >= 0 && scope.GuardBoundary + 1 <= cutBoundary ) { break; }
													if ( scope.CatchesCut ) { cutScope = scope; cutBoundary = scope.GuardBoundary + 1; break; }
												}
											}
											break;
										}
									}
									if ( skipPosition < 0 ) {
										if ( cutBoundary == 0 ) {
											if ( !interpreted ) { break; }
											unmatchedNamedSkip = true;
										}
										skipPosition = position;
									}
								}
							}
							if ( !PushFrame( new( nodes, index, skipPosition, cutBoundary, 0, continuation,
								thenOwner is not null ? FrameKind.ThenAlternative : unmatchedNamedSkip ? FrameKind.SkipReplay : cut.Kind switch {
									PcreWordCutKind.Commit => FrameKind.Commit, PcreWordCutKind.Skip => FrameKind.Skip, _ => FrameKind.Prune
								},
								CutScope: cutScope ) ) ) { return -2; }
							if ( cut.Mark is { } cutMark ) { SetMark( cutMark ); }
							break;
						case PcreWordPathMark pathMark:
							if ( !PushPathMark( pathMark.Name ) ) { return -2; }
							break;
						case PcreWordFailure failure:
							if ( failure.Mark is { } failureMark ) {
								if ( !PushPathMark( failureMark ) ) { return -2; }
								if ( !Step() ) { return -1; } // Named FAIL expands to MARK followed by FAIL.
							}
							matched = false;
							break;
						case PcreWordAccept accept:
							if ( accept.Mark is { } acceptMark ) {
								if ( !PushPathMark( acceptMark ) ) { return -2; }
								if ( !Step() ) { return -1; } // Named ACCEPT expands inside its quantifier.
							}
							var target = continuation;
							while ( target is not null && target.CallCheckpoint < 0
								&& ( target.GuardBoundary < 0 || target.AtomicGroup ) ) {
								if ( !Step() ) { return -1; } // Walk open captures/groups without recursion.
								if ( activeCall < 0 && target.CaptureNumber > 0 ) {
									SaveCapture( target.CaptureNumber, target.Start, position );
								}
								target = target.Parent;
							}
							if ( target is not null ) {
								var boundary = target.CallCheckpoint >= 0 ? target.CallBoundary : target.GuardBoundary;
								while ( frames!.Count > boundary + 1 ) {
									if ( !Step() ) { return -1; }
									frames.Pop();
								}
								var guard = frames.Peek();
								if ( target.CallCheckpoint >= 0 && !interpreted ) {
									// Native JIT commits an accepted callee's label across older
									// checkpoints that share its entry write. Distinct earlier
									// writes still roll back. No new retry frames are introduced.
									acceptedMarks ??= new();
									if ( cutTargets?.Contains( activeCall ) == true ) {
										// Lexical local-quit verbs save the call's entry label,
										// including erased bodies and nested accepted callees.
										acceptedMarks[guard.MarkVersion] = guard.Mark!;
										SetMark( mark );
									} else {
										acceptedMarks[guard.MarkVersion] = mark;
										markVersion = guard.MarkVersion;
									}
								} else if ( interpreted && target.CallCheckpoint < 0 ) {
									// Interpreted assertion ACCEPT keeps the caller's reported start.
									captures[0] = target.EntryMatchStart;
									if ( guard.Kind is FrameKind.PositiveCondition or FrameKind.NegativeCondition ) {
										RestoreMark( guard.MarkVersion, guard.Mark! );
									}
								}
								// Reuse ordinary scope return, bypassing remaining instructions,
								// repeats and lookbehind endpoint checks; ACCEPT commits non-atomic assertions too.
								continuation = target with { CheckAssertionEnd = false, NonAtomicAssertion = false };
								nodes = []; index = 0;
								continue;
							}
							var rejectedEmpty = RejectEmpty( position );
							var rejectedEnd = endAnchored && position != source.Length;
							if ( !rejectedEmpty && !rejectedEnd ) { end = position; return 1; }
							if ( rejectedEnd && ( !interpreted || !rejectedEmpty ) ) { return 2; }
							matched = false;
							break;
						case PcreWordReverse reverse:
							var reversed = position;
							var reversedCount = 0;
							while ( reversedCount < reverse.Maximum ) {
								if ( !Step() ) { return -1; } // One original scalar/unit of left context.
								if ( reversed <= reverseStart
									|| !PcreUtf16.TryReadPrevious( source, reversed, options.Utf16, out _, out var reverseWidth ) ) { break; }
								reversed -= reverseWidth;
								reversedCount++;
							}
							matched = reversedCount >= reverse.Minimum;
							if ( matched ) {
								position = reversed;
								if ( reversedCount > reverse.Minimum && !PushFrame( new( nodes, index + 1, position, 0,
									reversedCount - reverse.Minimum, continuation, FrameKind.ReverseRetry ) ) ) { return -2; }
							}
							break;
						case PcreByteLiteral or PcreByteAny or PcreByteScalarSet or PcreByteScalarClass or PcreByteNewlineSequence:
							matched = TryConsumeAtom( nodes[index], source[..visibleEnd], position, out var width );
							if ( matched ) { position += width; }
							break;
						case PcreByteAnchor or PcreByteLineAnchor or PcreByteEndBeforeFinalLf:
							matched = MatchAnchor( nodes[index], source[..visibleEnd], position );
							break;
						case PcreByteSearchStart: matched = position == startOffset; break;
						case PcreByteMatchStartReset:
							SaveCapture( 0, position, position );
							break;
						case PcreByteWordBoundary boundary:
							var boundaryResult = MatchBoundary( boundary, source[..visibleEnd], position );
							if ( boundaryResult < 0 ) { return -1; }
							matched = boundaryResult == 1;
							break;
						case PcreByteBackreference reference:
							var referenceResult = MatchReference( reference, source, position, out var referenceEnd );
							if ( referenceResult < 0 ) { return -1; }
							matched = referenceResult == 1;
							if ( matched ) { position = referenceEnd; }
							break;
						case PcreByteRepeat repeat:
							var count = 0;
							var repeatStart = position;
							while ( repeat.Greedy || count < repeat.Minimum ) {
								if ( !Step() ) { return -1; }
								if ( !TryConsumeAtom( repeat.Child, source[..visibleEnd], position, out var repeatWidth ) ) { break; }
								position += repeatWidth;
								count++;
							}
							matched = count >= repeat.Minimum;
							if ( matched && ( !repeat.Greedy || count > repeat.Minimum
								&& ( index < nodes.Length - 1 || continuation is not null ) ) ) {
								if ( !PushFrame( new( nodes, index, position, repeatStart, count, continuation ) ) ) { return -2; }
							}
							break;
						default: throw new InvalidOperationException( "Unsupported node reached the word sequence executor." );
					}
					if ( matched ) { index++; continue; }
				}

				var resumed = false;
				while ( frames is not null && frames.TryPop( out var frame ) ) {
					if ( !RestoreCaptures( frame.ChangeCount ) ) { return -1; }
					if ( frames.Count == 0 ) { changes?.Clear(); }
					nodes = frame.Nodes;
					index = frame.Index;
					position = frame.Position;
					continuation = frame.Continuation;
					visibleEnd = frame.VisibleEnd;
					activeCall = frame.ActiveCall;
					RestoreMark( frame.MarkVersion, frame.Mark! );
					if ( frame.Kind == FrameKind.PathMark ) {
						if ( !Step() ) { return -1; } // MARK history has no retry edge.
						continue;
					}
					if ( frame.Kind is FrameKind.Prune or FrameKind.Commit or FrameKind.ThenAlternative or FrameKind.Skip or FrameKind.SkipReplay ) {
						if ( !Step() ) { return -1; } // Reaching a cut, rather than executing it, discards choices.
						if ( !interpreted && frame.CutScope is { CallCheckpoint: >= 0 } callScope
							&& !callScope.ReturnedCaptures.IsEmpty && changes is not null ) {
							var kept = callScope.CallCheckpoint;
							for ( var entry = kept; entry < changes.Count; entry++ ) {
								if ( !Step() ) { return -1; }
								var change = changes[entry];
								if ( callScope.ReturnedCaptures.Span.BinarySearch( change.Number ) >= 0 ) {
									cutCaptures ??= new();
									cutCaptures[change.Number] = captures[change.Number];
									continue;
								}
								changes[kept++] = change;
							}
							changes.RemoveRange( kept, changes.Count - kept );
						}
						while ( frames.Count > frame.RepeatStart ) {
							if ( !Step() ) { return -1; }
							frames.Pop();
						}
						if ( !interpreted && frame.Kind == FrameKind.ThenAlternative && frame.CutScope is { } labelOwner ) {
							// A reached owned THEN restores its branch entry, including labels
							// overwritten by accepted calls. An unreached THEN changes nothing.
							acceptedMarks ??= new();
							acceptedMarks[labelOwner.ThenEntryMarkVersion] = labelOwner.ThenEntryMark!;
							RestoreMark( labelOwner.ThenEntryMarkVersion, labelOwner.ThenEntryMark! );
						}
						if ( frame.Kind == FrameKind.ThenAlternative && frame.CutScope is { } owner
							&& owner.ThenNext < owner.ThenGroup!.Alternatives.Length ) {
							// The retained retry belongs to the group entry. Skip finite
							// expansions of the failed source branch without replacing its state.
							if ( !Step() ) { return -1; }
							var nextBranch = frames.Pop();
							frames.Push( nextBranch with { Count = owner.ThenNext } );
						}
						if ( frame.Kind == FrameKind.SkipReplay ) { return 4; }
						if ( frame.RepeatStart == 0 ) {
							if ( frame.Kind == FrameKind.Skip && frame.Position > candidate ) { skipRestart = frame.Position; }
							return frame.Kind == FrameKind.Commit ? 3 : frame.Kind == FrameKind.ThenAlternative ? 0 : 2;
						}
						continue; // The retained guard resolves the failed call/assertion.
					} else if ( frame.Kind is FrameKind.PositiveCondition or FrameKind.NegativeCondition ) {
						if ( !Step() ) { return -1; } // Exhausted test rolls back before selecting the other arm.
						EnterConditionalArm( (PcreWordConditional)nodes[index], frame.Kind == FrameKind.NegativeCondition );
					} else if ( frame.Kind is FrameKind.PositiveAssertion or FrameKind.NegativeAssertion or FrameKind.AtomicGroup or FrameKind.Subroutine ) {
						if ( !Step() ) { return -1; } // Resolve an exhausted guarded body.
						if ( frame.Kind != FrameKind.NegativeAssertion ) { continue; }
					} else if ( frame.Kind == FrameKind.ReverseRetry ) {
						if ( !Step() ) { return -1; } // Try the next shorter rewind after body choices.
						if ( !PcreUtf16.TryRead( source, position, options.Utf16, out _, out var advance ) ) {
							throw new InvalidOperationException( "A lookbehind retry lost its original character boundary." );
						}
						position += advance;
						if ( frame.Count > 1 && !PushFrame( frame with { Position = position, Count = frame.Count - 1 } ) ) { return -2; }
					} else if ( frame.Kind is FrameKind.RepeatExit or FrameKind.RepeatGrow ) {
						if ( !Step() ) { return -1; } // Retry an iteration or its enclosing suffix.
						if ( frame.Kind == FrameKind.RepeatExit ) { index++; }
						else {
							var growing = (PcreWordGroupRepeat)nodes[index];
							continuation = new( nodes, index, continuation, growing, frame.Count, position );
							nodes = growing.Body;
							index = 0;
						}
					} else if ( nodes[index] is PcreWordSuffixChoices choices ) {
						if ( !Step() ) { return -1; }
						if ( frame.Count + 1 < choices.Alternatives.Length ) {
							if ( !PushFrame( frame with { Count = frame.Count + 1 } ) ) { return -2; }
						}
						nodes = choices.Alternatives[frame.Count];
						index = 0;
					} else if ( nodes[index] is PcreWordGroup group ) {
						if ( !Step() ) { return -1; }
						var thenBoundary = hasThen && group.SourceBranchStarts is { Length: > 2 } ? frames.Count : -1;
						var thenNext = -1;
						if ( thenBoundary >= 0 ) {
							foreach ( var start in group.SourceBranchStarts! ) {
								if ( !Step() ) { return -1; }
								if ( start > frame.Count ) { thenNext = start; break; }
							}
						}
						if ( frame.Count + 1 < group.Alternatives.Length ) {
							if ( !PushFrame( frame with { Count = frame.Count + 1 } ) ) { return -2; }
						}
						continuation = new( nodes, index + 1, continuation, Start: position, CaptureNumber: group.CaptureNumber,
							AssertionEnd: group.CheckAssertionEnds?[frame.Count] == true ? continuation!.AssertionEnd : -1,
							ThenBoundary: thenBoundary, ThenNext: thenNext, ThenGroup: thenBoundary >= 0 ? group : null,
							ThenEntryMark: mark, ThenEntryMarkVersion: markVersion );
						nodes = group.Alternatives[frame.Count];
						index = 0;
					} else {
						var repeat = (PcreByteRepeat)nodes[index];
						if ( !Step() ) { return -1; } // Grow lazily or rewind one complete atom.
						var count = frame.Count;
						if ( repeat.Greedy ) {
							position -= RepeatRewindWidth( repeat.Child, source, position, frame.RepeatStart );
							count--;
						} else {
							if ( !TryConsumeAtom( repeat.Child, source[..visibleEnd], position, out var width ) ) { continue; }
							position += width;
							count++;
						}
						if ( !repeat.Greedy || count > repeat.Minimum ) {
							if ( !PushFrame( frame with { Position = position, Count = count } ) ) { return -2; }
						}
						index++;
					}
					resumed = true;
					break;
				}
				if ( !resumed ) { return 0; }
			}
		}
		for ( var candidate = startOffset; ; ) {
			skipRestart = -1;
			var replayNamedSkip = false;
			var resetCandidateMark = false;
			namedSkipCount = 0;
			acceptedMarks?.Clear();
			cutCaptures?.Clear();
			if ( !Step() ) { return Exhausted(); } // Candidate admission.
			var eligible = true;
			if ( !anchored && commitStart is { } first ) {
				if ( !Step() ) { return Exhausted(); } // Native first-unit candidate proof.
				eligible = candidate < input.Length && LiteralEquals( input[candidate], first );
			}
			for ( var branch = 0; eligible && branch < branches.Length; branch++ ) {
				if ( branches.Length > 1 && !Step() ) { return Exhausted(); } // Ordered branch admission.
				var outcome = TrySequence( branches[branch], input, candidate, out var end );
				if ( outcome == -2 ) { return new( null, new( PcreWordDiagnosticCode.MatchDepthLimitExceeded, 0 ) ); }
				if ( outcome < 0 ) { return Exhausted(); }
				if ( outcome == 2 ) { resetCandidateMark = true; break; }
				if ( outcome == 4 ) { replayNamedSkip = true; break; }
				if ( outcome == 3 ) { return new( null, null ); }
				if ( outcome > 0 ) {
					var reportedStart = captures[0]?.CodeUnitIndex ?? candidate;
					var length = end - reportedStart;
					captures[0] = new( reportedStart, length );
					// Transfer this call's capture array; no runner or subject memory escapes.
					return new( new( reportedStart, length, Array.AsReadOnly( captures ), CaptureInfo ) { MarkName = mark }, null );
				}
			}
			if ( !interpreted ) {
				// JIT ordinary bumpalong preserves an accepted call's committed entry
				// label. Root cuts/reset paths clear it before the next candidate.
				if ( resetCandidateMark ) { candidateMark = String.Empty; }
				else if ( acceptedMarks is not null && acceptedMarks.TryGetValue( 0, out var committedEntry ) ) { candidateMark = committedEntry; }
			}
			if ( replayNamedSkip ) {
				// Suppress every named visit so far, including earlier resolved names.
				ignoredNamedSkips = namedSkipCount;
				if ( anchored || leadingStartAnchor ) { return new( null, null ); }
				if ( skipCrLfTail && candidate > startOffset && PcreWordNewline.IsCrLf( input, candidate - 1 ) ) { candidate++; }
				if ( !Step() ) { return Exhausted(); }
				continue; // Reenter with clean candidate storage and the original budget.
			}
			if ( skipRestart <= candidate ) { ignoredNamedSkips = 0; }
			// Native JIT permits explicit forward SKIP entry even with compiled
			// anchoring. Interpreter anchoring stops before any forced restart.
			if ( skipRestart > candidate && !( interpreted && anchored ) ) {
				if ( !Step() ) { return Exhausted(); } // Enter an original-unit SKIP target.
				candidate = skipRestart;
				if ( interpreted && skipCrLfTail && PcreWordNewline.IsCrLf( input, candidate - 1 ) ) { candidate++; }
				continue;
			}
			if ( anchored || candidate == input.Length ) { return new( null, null ); }
			if ( !Step() ) { return Exhausted(); } // Advance one original unit/scalar.
			if ( !PcreUtf16.TryRead( input, candidate, options.Utf16, out _, out var advance ) ) {
				return new( null, new( PcreWordDiagnosticCode.InvalidUtf16Subject, candidate ) );
			}
			candidate += advance;
			if ( skipCrLfTail && !allowsLfEntry && PcreWordNewline.IsCrLf( input, candidate - 1 ) ) {
				var skipLf = true;
				if ( !interpreted && fixedPrefix is { } prefix ) {
					skipLf = false;
					if ( prefix.RequiresComparison || endAnchored ) {
						if ( !Step() ) { return Exhausted(); }
						if ( prefix.Width < input.Length - ( candidate - 1 ) ) {
							var offset = candidate - 1 + prefix.Width;
							foreach ( var value in prefix.Literals.Span ) {
								if ( !Step() ) { return Exhausted(); }
								skipLf |= LiteralEquals( input[offset], value );
							}
						}
					}
				}
				if ( skipLf ) {
					if ( !Step() ) { return Exhausted(); }
					candidate++;
				}
			}
		}
	}

	private readonly record struct BacktrackFrame(
		PcreByteNode[] Nodes, int Index, int Position, int RepeatStart, int Count, Continuation? Continuation,
		FrameKind Kind = FrameKind.Retry, int ChangeCount = 0, int VisibleEnd = 0, int ActiveCall = -1,
		string? Mark = null, int MarkVersion = 0, Continuation? CutScope = null, string? HistoryName = null
	);

	private enum FrameKind { Retry, RepeatExit, RepeatGrow, PositiveAssertion, NegativeAssertion, PositiveCondition, NegativeCondition, AtomicGroup, Subroutine, ReverseRetry, Prune, Commit, ThenAlternative, Skip, SkipReplay, PathMark }
	private readonly record struct CaptureChange( int Number, PcreWordCapture? Previous );

	private sealed record Continuation( PcreByteNode[] Nodes, int Index, Continuation? Parent,
		PcreWordGroupRepeat? Repeat = null, int Count = 0, int Start = 0, int CaptureNumber = 0,
		int GuardBoundary = -1, int AssertionEnd = -1, bool NonAtomicAssertion = false, int SavedEnd = -1,
		int CallCheckpoint = -1, ReadOnlyMemory<int> ReturnedCaptures = default, int Caller = -1, bool CheckAssertionEnd = true,
		bool AtomicGroup = false, int CallBoundary = -1, bool CatchesCut = false, bool IsolatePathMarks = false,
		int ThenBoundary = -1, int ThenNext = -1, PcreWordGroup? ThenGroup = null,
		string? ThenEntryMark = null, int ThenEntryMarkVersion = 0, PcreWordCapture? EntryMatchStart = null );

	private bool TryConsumeAtom( PcreByteNode node, ReadOnlySpan<char> input, int position, out int width ) {
		if ( !PcreUtf16.TryRead( input, position, options.Utf16, out var value, out width ) ) { return false; }
		if ( node is PcreByteNewlineSequence ) {
			if ( bsrAnyCrLf ? value is not ( '\r' or '\n' ) : !PcreSpaceCharacters.IsVertical( value ) ) { return false; }
			if ( value == '\r' && position < input.Length - 1 && input[position + 1] == '\n' ) { width = 2; }
			return true;
		}
		return node switch {
			PcreByteLiteral literal => LiteralEquals( value, literal ),
			PcreByteAny any => any.DotAll || PcreWordNewline.LengthAt( newline, input, position ) == 0,
			PcreByteScalarClass property => PcreScalarClassMembership.Contains( property.Kind, value,
				property.Category, property.Caseless, property.PropertyValue ) != property.Negated,
			PcreByteScalarSet set => ContainsSet( set, value ),
			_ => throw new InvalidOperationException( "Unsupported word repeat atom." )
		};
	}

	private int RepeatRewindWidth( PcreByteNode node, ReadOnlySpan<char> input, int position, int repeatStart ) {
		// A CRLF consumed by \R is indivisible. A leading LF may instead have
		// been consumed at the repeat's start, after a CR matched by its prefix.
		if ( node is PcreByteNewlineSequence && position - repeatStart >= 2
			&& input[position - 2] == '\r' && input[position - 1] == '\n' ) { return 2; }
		if ( !PcreUtf16.TryReadPrevious( input, position, options.Utf16, out _, out var width ) ) {
			throw new InvalidOperationException( "Invalid word repeat rewind." );
		}
		return width;
	}

	private bool ContainsSet( PcreByteScalarSet set, int value ) {
		var included = false;
		var rangeValue = set.Caseless && ( options.Utf16 || options.Ucp )
			? PcreUtf8CaseFold.Fold( value, set.CaselessRestricted ) : value;
		var opposite = set.Caseless && !options.Utf16 && !options.Ucp
			? AsciiOppositeCase( value ) : rangeValue;
		foreach ( var range in set.Ranges.Span ) {
			if ( rangeValue >= range.Start && rangeValue <= range.End
				|| opposite >= range.Start && opposite <= range.End ) { included = true; break; }
		}
		if ( !included ) {
			foreach ( var predicate in set.Predicates.Span ) {
				if ( PcreScalarClassMembership.Contains( predicate.Kind, value, predicate.Category,
					set.Caseless, predicate.PropertyValue ) != predicate.Negated ) {
					included = true; break;
				}
			}
		}
		return included != set.Negated;
	}

	private bool LiteralEquals( int value, PcreByteLiteral literal ) =>
		ScalarEquals( value, literal.Value, literal.Caseless, literal.CaselessRestricted );

	private bool ScalarEquals( int left, int right, bool caseless, bool restricted ) {
		if ( left == right ) { return true; }
		if ( !caseless ) { return false; }
		if ( options.Utf16 || options.Ucp ) {
			return PcreUtf8CaseFold.Equals( left, right, restricted );
		}
		return AsciiOppositeCase( left ) == right;
	}

	private static int AsciiOppositeCase( int value ) => value is >= 'A' and <= 'Z' ? value + 32
		: value is >= 'a' and <= 'z' ? value - 32 : value;
}
