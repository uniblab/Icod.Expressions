namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private readonly record struct GroupOptions(
		bool Caseless, bool Multiline, bool DotAll, bool NoAutoCapture, bool Ungreedy
	);

	// Bounded heap frames keep source nesting off the native call stack.
	private sealed class GroupFrame( int openingOffset, int depth, int number, bool atomic,
		bool branchReset, GroupOptions entryOptions, int entryGroupCount ) {
		internal int OpeningOffset { get; } = openingOffset;
		internal int Depth { get; } = depth;
		internal int Number { get; } = number;
		internal bool Atomic { get; } = atomic;
		internal bool BranchReset { get; } = branchReset;
		internal GroupOptions EntryOptions { get; } = entryOptions;
		internal int EntryGroupCount { get; } = entryGroupCount;
		internal int MaximumGroupCount { get; set; } = entryGroupCount;
		internal bool Quoting;
		internal List<IReadOnlyList<PcreByteNode>> Branches { get; } = new();
		internal List<PcreByteNode> Nodes { get; set; } = new();
	}

	private GroupOptions CurrentGroupOptions => new( caseless, multiline, dotAll, noAutoCapture, ungreedy );

	private void RestoreGroupOptions( GroupOptions options ) {
		caseless = options.Caseless;
		multiline = options.Multiline;
		dotAll = options.DotAll;
		noAutoCapture = options.NoAutoCapture;
		ungreedy = options.Ungreedy;
	}

	private PcreByteAlternatives ParseAlternatives() {
		var parents = new Stack<GroupFrame>();
		var current = new GroupFrame( -1, 0, 0, false, false, CurrentGroupOptions, groupCount );
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( index >= source.Length ) {
				if ( current.OpeningOffset >= 0 ) {
					// Preserve the recursive parser's outermost-unclosed-group diagnostic.
					var opening = parents.LastOrDefault( frame => frame.OpeningOffset >= 0 )?.OpeningOffset
						?? current.OpeningOffset;
					throw Invalid( opening, "unclosed group" );
				}
				return FinishAlternatives( current );
			}
			if ( TryConsumeQuoteDelimiter( ref current.Quoting ) ) { continue; }
			if ( !current.Quoting ) {
				if ( source[index] == (byte)'|' ) {
					FinishBranch( current );
					current.Nodes = new();
					index++;
					if ( current.BranchReset ) { groupCount = current.EntryGroupCount; }
					continue;
				}
				if ( source[index] == (byte)')' ) {
					var body = FinishAlternatives( current );
					if ( current.OpeningOffset < 0 ) { return body; } // Parse diagnoses unmatched ')'.
					index++;
					RestoreGroupOptions( current.EntryOptions );
					PcreByteNode completed = current.Number > 0 ? new PcreByteGroup( current.Number, body )
						: current.Atomic ? new PcreByteAtomic( new PcreByteNoncapturingGroup( body ) )
						: new PcreByteNoncapturingGroup( body );
					var groupCaseless = current.EntryOptions.Caseless;
					current = parents.Pop();
					AppendNode( current, completed, groupCaseless );
					continue;
				}
				if ( TryParseOptions( index, scoped: false ) ) { continue; }
			}
			var atomCaseless = caseless;
			GroupFrame? child = null;
			var atom = current.Quoting ? ParseQuotedLiteral() : ParseAtom( current.Depth, out child );
			if ( child is not null ) {
				parents.Push( current );
				current = child;
				continue;
			}
			AppendNode( current, atom!, atomCaseless );
		}
	}

	private void FinishBranch( GroupFrame frame ) {
		frame.Branches.Add( frame.Nodes );
		if ( frame.BranchReset ) { frame.MaximumGroupCount = Math.Max( frame.MaximumGroupCount, groupCount ); }
	}

	private PcreByteAlternatives FinishAlternatives( GroupFrame frame ) {
		FinishBranch( frame );
		if ( frame.BranchReset ) { groupCount = frame.MaximumGroupCount; }
		return new( frame.Branches );
	}

	private void AppendNode( GroupFrame frame, PcreByteNode atom, bool atomCaseless ) {
		if ( atom.Caseless != atomCaseless ) { atom = atom with { Caseless = atomCaseless }; }
		while ( TryConsumeQuoteDelimiter( ref frame.Quoting ) ) { }
		if ( !frame.Quoting && index < source.Length && source[index] is (byte)'*' or (byte)'+' or (byte)'?' or (byte)'{' ) {
			if ( atom is PcreByteLiteralAssertion ) {
				throw Unsupported( index );
			}
			if ( atom is PcreByteMatchStartReset ) {
				throw Invalid( index, "a match-start reset cannot be repeated" );
			}
			if ( atom is PcreByteAnchor or PcreByteSearchStart or PcreByteLineAnchor or PcreByteEndBeforeFinalLf or PcreByteWordBoundary ) {
				throw Invalid( index, "an anchor cannot be repeated" );
			}
			var quantifierOffset = index;
			var counted = source[index] == (byte)'{';
			var quantifier = counted ? (byte)0 : source[index++];
			var bounds = counted ? ParseCountedBounds( quantifierOffset )
				: (quantifier == (byte)'+' ? 1 : 0,
					quantifier == (byte)'?' ? 1 : int.MaxValue);
			var minimum = bounds.Item1;
			var maximum = bounds.Item2;
			repeatMarks += counted ? maximum == int.MaxValue ? 1 : 0
				: quantifier == (byte)'+' ? 2 : quantifier == (byte)'*' ? 1 : 0;
			if ( repeatMarks > MaximumRepeatMarks ) {
				throw new PcreByteParseException( new(
					PcreByteDiagnosticCode.PatternRepeatLimitExceeded, quantifierOffset, "repeat state limit exceeded"
				) );
			}
			var questionSuffix = index < source.Length && source[index] == (byte)'?';
			if ( questionSuffix ) { index++; }
			var possessive = !questionSuffix && index < source.Length && source[index] == (byte)'+';
			if ( possessive ) { index++; }
			// U reverses ordinary/question-suffix preference; possessive is always greedy.
			var greedy = possessive || ungreedy == questionSuffix;
			atom = new PcreByteRepeat( atom, minimum, maximum, greedy,
				counted ? quantifierOffset : -1 );
			if ( possessive ) {
				atom = new PcreByteAtomic( atom );
			}
			if ( index < source.Length && source[index] is (byte)'*' or (byte)'+' or (byte)'?' or (byte)'{' ) {
				throw Invalid( index, "repeated quantifier" );
			}
		}
		frame.Nodes.Add( atom );
	}

	private PcreByteNode? ParseGroupOpening( int offset, int depth, out GroupFrame? group ) {
		group = null;
		// PCRE2's (*...) directives and aliases are pending, not repeated empty groups.
		if ( index < source.Length && source[index] == (byte)'*'
			&& source.AsSpan( index + 1 ).IndexOf( (byte)')' ) >= 0 ) {
			throw Unsupported( offset );
		}
		var capturing = !noAutoCapture;
		var atomic = false;
		var branchReset = false;
		var entryOptions = CurrentGroupOptions;
		string? name = null;
		if ( index < source.Length && source[index] == (byte)'?' ) {
			if ( TryParseOptions( offset, scoped: true ) ) {
				capturing = false;
			} else if ( index + 1 < source.Length && source[index + 1] == (byte)':' ) {
				index += 2;
				capturing = false;
			} else if ( index + 1 < source.Length && source[index + 1] == (byte)'|' ) {
				index += 2;
				capturing = false;
				branchReset = true;
			} else if ( index + 1 < source.Length && source[index + 1] == (byte)'>' ) {
				index += 2;
				capturing = false;
				atomic = true;
			} else if ( source.AsSpan( index ).StartsWith( "?P="u8 ) ) {
				index += 3;
				return ParseNamedReference( (byte)')', offset );
			} else if ( source.AsSpan( index ).StartsWith( "?P<"u8 ) ) {
				index += 3;
				name = ParseCaptureName( (byte)'>', offset );
				capturing = true;
			} else if ( source.AsSpan( index ).StartsWith( "?'"u8 ) ) {
				index += 2;
				name = ParseCaptureName( (byte)'\'', offset );
				capturing = true;
			} else if ( index + 1 < source.Length && source[index + 1] == (byte)'<'
				&& ( index + 2 >= source.Length || source[index + 2] is not (byte)'=' and not (byte)'!' and not (byte)'*' ) ) {
				index += 2;
				name = ParseCaptureName( (byte)'>', offset );
				capturing = true;
			} else {
				var behind = index + 2 < source.Length && source[index + 1] == (byte)'<'
					&& source[index + 2] is (byte)'=' or (byte)'!';
				var ahead = index + 1 < source.Length && source[index + 1] is (byte)'=' or (byte)'!';
				if ( !behind && !ahead ) { throw Unsupported( offset ); }
				if ( depth >= MaximumPatternDepth ) {
					throw new PcreByteParseException( new(
						PcreByteDiagnosticCode.PatternDepthLimitExceeded, offset, "pattern nesting limit exceeded"
					) );
				}
				var positive = source[index + (behind ? 2 : 1)] == (byte)'=';
				index += behind ? 3 : 2;
				return ParseLiteralAssertion( offset, positive, behind );
			}
		}
		if ( depth >= MaximumPatternDepth ) {
			throw new PcreByteParseException( new(
				PcreByteDiagnosticCode.PatternDepthLimitExceeded, offset, "pattern nesting limit exceeded"
			) );
		}
		var number = capturing ? ++groupCount : 0;
		if ( groupCount > MaximumCaptureCount ) {
			throw new PcreByteParseException( new(
				PcreByteDiagnosticCode.PatternCaptureLimitExceeded, offset, "capture count limit exceeded"
			) );
		}
		if ( name is not null ) { RegisterCaptureName( name, number, offset ); }
		group = new GroupFrame( offset, depth + 1, number, atomic, branchReset, entryOptions, groupCount );
		return null;
	}
}
