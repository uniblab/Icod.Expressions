namespace Icod.Expressions.Pcre;

/// <summary>Immutable width-16 sequence; matching reads the original subject view.</summary>
internal sealed class PcreCompiledWordPattern {
	private const int MaximumBacktrackFrames = 1024;
	private readonly PcreByteNode[][] branches;
	private readonly PcreWordOptions options;
	private readonly bool inspectPreviousScalar;
	private readonly char newlineCharacter;
	private readonly bool bsrAnyCrLf;
	public PcreCaptureInfo CaptureInfo { get; }
	internal PcreCompiledWordPattern( PcreByteNode[] nodes, PcreWordOptions options ) : this( [nodes], options ) { }
	internal PcreCompiledWordPattern( PcreByteNode[][] branches, PcreWordOptions options, bool inspectPreviousScalar = false,
		PcreByteNewline newline = PcreByteNewline.Lf, bool bsrAnyCrLf = false, PcreCaptureInfo? captureInfo = null ) {
		this.branches = branches;
		this.options = options;
		this.inspectPreviousScalar = inspectPreviousScalar;
		newlineCharacter = (char)newline.Character();
		this.bsrAnyCrLf = bsrAnyCrLf;
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
		if ( options.Utf16 ) {
			// PCRE2 gives word boundaries and absolute-start assertions a one-character
			// validation lookbehind, even when another alternative could match.
			if ( startOffset > 0 && startOffset < input.Length && Char.IsLowSurrogate( input[startOffset] ) ) {
				return new( null, new( PcreWordDiagnosticCode.InvalidStartOffset, startOffset ) );
			}
			var validationStart = startOffset;
			if ( inspectPreviousScalar && validationStart > 0 ) {
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
		}
		var anchored = options.Policy.Anchored || policy.Anchored;
		var endAnchored = options.Policy.EndAnchored || policy.EndAnchored;
		var notEmpty = options.Policy.NotEmpty || policy.NotEmpty;
		var notEmptyAtStart = options.Policy.NotEmptyAtStart || policy.NotEmptyAtStart;
		var notBol = options.Policy.NotBol || policy.NotBol;
		var notEol = options.Policy.NotEol || policy.NotEol;
		bool MatchAnchor( PcreByteNode node, ReadOnlySpan<char> source, int at ) => node switch {
			PcreByteAnchor anchor => !anchor.IsDisabled
				&& ( !anchor.IsLineBoundary || !( anchor.IsStart ? notBol : notEol ) )
				&& at == ( anchor.IsStart ? 0 : source.Length ),
			PcreByteLineAnchor { IsStart: true } start =>
				at == 0 && !start.SuppressSubjectBoundary && !notBol
				|| at > 0 && ( at < source.Length || start.AllowSubjectEnd ) && source[at - 1] == newlineCharacter,
			PcreByteLineAnchor end =>
				at == source.Length && !end.SuppressSubjectBoundary && !notEol
				|| at < source.Length && source[at] == newlineCharacter,
			PcreByteEndBeforeFinalLf end => !end.IsDisabled && ( !end.IsLineBoundary || !notEol )
				&& ( at == source.Length || at == source.Length - 1 && source[at] == newlineCharacter ),
			_ => throw new InvalidOperationException( "Unsupported word anchor." )
		};
		int MatchBoundary( PcreByteWordBoundary boundary, ReadOnlySpan<char> source, int at ) {
			var kind = boundary.UnicodeWord ? PcreByteScalarClassKind.UnicodeWord : PcreByteScalarClassKind.AsciiWord;
			var beforeWord = false;
			if ( at > 0 ) {
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
		var captures = new PcreWordCapture?[CaptureInfo.CaptureCount + 1];
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
					|| !PcreUtf16.TryRead( source, candidate, options.Utf16, out var right, out var rightWidth )
					|| !ScalarEquals( left, right, reference.Caseless, reference.CaselessRestricted ) ) { return 0; }
				offset += leftWidth;
				candidate += rightWidth;
			}
			end = candidate;
			return 1;
		}
		List<CaptureChange>? changes = null;
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
			frames.Push( frame with { ChangeCount = changes?.Count ?? 0 } );
			return true;
		}
		// -2: frame limit; -1: work exhausted; 0: failed or policy-rejected; 1: match.
		int TrySequence( PcreByteNode[] nodes, ReadOnlySpan<char> source, int candidate, out int end ) {
			frames?.Clear();
			changes?.Clear();
			Array.Clear( captures );
			Continuation? continuation = null;
			var index = 0;
			var position = candidate;
			end = candidate;
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
						if ( parent.Repeat is { } completedRepeat ) {
							// Required counted copies may be empty. The optional unbounded
							// loop stops on no progress; shorthand '+' also guards its first copy.
							if ( completedRepeat.Maximum == int.MaxValue && position == parent.Start
								&& ( parent.Count >= completedRepeat.Minimum || completedRepeat.CheckMinimumProgress ) ) {
								index++;
							} else if ( !ContinueGroupRepeat( completedRepeat, parent.Count + 1 ) ) { return -2; }
						}
						continue;
					}
					if ( !Step() ) { return -1; } // Completion, including policy rejection.
					var empty = position == candidate;
					if ( ( !endAnchored || position == source.Length )
						&& !( empty && ( notEmpty || notEmptyAtStart && candidate == startOffset ) ) ) {
						end = position; return 1;
					}
				} else if ( nodes[index] is PcreWordSuffixChoices firstChoices ) {
					if ( !Step() ) { return -1; } // Ordered suffix choice admission.
					if ( !PushFrame( new( nodes, index, position, position, 1, continuation ) ) ) { return -2; }
					nodes = firstChoices.Alternatives[0];
					index = 0;
					continue;
				} else if ( nodes[index] is PcreWordGroup firstGroup ) {
					if ( !Step() ) { return -1; } // Group entry.
					if ( firstGroup.Alternatives.Length > 1 ) {
						if ( !Step() ) { return -1; } // Ordered group branch admission.
						if ( !PushFrame( new( nodes, index, position, position, 1, continuation ) ) ) { return -2; }
					}
					continuation = new( nodes, index + 1, continuation, Start: position, CaptureNumber: firstGroup.CaptureNumber );
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
						case PcreByteLiteral or PcreByteAny or PcreByteScalarSet or PcreByteScalarClass or PcreByteNewlineSequence:
							matched = TryConsumeAtom( nodes[index], source, position, out var width );
							if ( matched ) { position += width; }
							break;
						case PcreByteAnchor or PcreByteLineAnchor or PcreByteEndBeforeFinalLf:
							matched = MatchAnchor( nodes[index], source, position );
							break;
						case PcreByteSearchStart: matched = position == startOffset; break;
						case PcreByteWordBoundary boundary:
							var boundaryResult = MatchBoundary( boundary, source, position );
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
								if ( !TryConsumeAtom( repeat.Child, source, position, out var repeatWidth ) ) { break; }
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
					if ( frame.Kind is FrameKind.RepeatExit or FrameKind.RepeatGrow ) {
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
						if ( frame.Count + 1 < group.Alternatives.Length ) {
							if ( !PushFrame( frame with { Count = frame.Count + 1 } ) ) { return -2; }
						}
						continuation = new( nodes, index + 1, continuation, Start: position, CaptureNumber: group.CaptureNumber );
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
							if ( !TryConsumeAtom( repeat.Child, source, position, out var width ) ) { continue; }
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
			if ( !Step() ) { return Exhausted(); } // Candidate admission.
			for ( var branch = 0; branch < branches.Length; branch++ ) {
				if ( branches.Length > 1 && !Step() ) { return Exhausted(); } // Ordered branch admission.
				var outcome = TrySequence( branches[branch], input, candidate, out var end );
				if ( outcome == -2 ) { return new( null, new( PcreWordDiagnosticCode.MatchDepthLimitExceeded, 0 ) ); }
				if ( outcome < 0 ) { return Exhausted(); }
				if ( outcome > 0 ) {
					captures[0] = new( candidate, end - candidate );
					// Transfer this call's capture array; no runner or subject memory escapes.
					return new( new( candidate, end - candidate, Array.AsReadOnly( captures ), CaptureInfo ), null );
				}
			}
			if ( anchored || candidate == input.Length ) { return new( null, null ); }
			if ( !Step() ) { return Exhausted(); } // Advance one original unit/scalar.
			if ( !PcreUtf16.TryRead( input, candidate, options.Utf16, out _, out var advance ) ) {
				return new( null, new( PcreWordDiagnosticCode.InvalidUtf16Subject, candidate ) );
			}
			candidate += advance;
		}
	}

	private readonly record struct BacktrackFrame(
		PcreByteNode[] Nodes, int Index, int Position, int RepeatStart, int Count, Continuation? Continuation,
		FrameKind Kind = FrameKind.Retry, int ChangeCount = 0
	);

	private enum FrameKind { Retry, RepeatExit, RepeatGrow }
	private readonly record struct CaptureChange( int Number, PcreWordCapture? Previous );

	private sealed record Continuation( PcreByteNode[] Nodes, int Index, Continuation? Parent,
		PcreWordGroupRepeat? Repeat = null, int Count = 0, int Start = 0, int CaptureNumber = 0 );

	private bool TryConsumeAtom( PcreByteNode node, ReadOnlySpan<char> input, int position, out int width ) {
		if ( !PcreUtf16.TryRead( input, position, options.Utf16, out var value, out width ) ) { return false; }
		if ( node is PcreByteNewlineSequence ) {
			if ( bsrAnyCrLf ? value is not ( '\r' or '\n' ) : !PcreSpaceCharacters.IsVertical( value ) ) { return false; }
			if ( value == '\r' && position < input.Length - 1 && input[position + 1] == '\n' ) { width = 2; }
			return true;
		}
		return node switch {
			PcreByteLiteral literal => LiteralEquals( value, literal ),
			PcreByteAny any => any.DotAll || value != newlineCharacter,
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
