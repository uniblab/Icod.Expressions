using System.Text;

namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private readonly record struct GroupOptions(
		bool Caseless, bool Multiline, bool DotAll, bool NoAutoCapture, bool Ungreedy,
		ExtendedMode Extended, AsciiRestriction AsciiRestrictions, bool CaselessRestricted, bool DuplicateNames
	);

	// Bounded heap frames keep source nesting off the native call stack.
	private sealed class GroupFrame( int openingOffset, int depth, int number, bool atomic,
		bool branchReset, GroupOptions entryOptions, int entryGroupCount,
		int assertionKind, int assertionOpeningContext, bool nonAtomic = false, CaptureCondition? condition = null,
		bool assertionConditional = false, StaticCondition? staticCondition = null ) {
		internal int OpeningOffset { get; } = openingOffset;
		internal int Depth { get; } = depth;
		internal int Number { get; } = number;
		internal bool Atomic { get; } = atomic;
		internal bool BranchReset { get; } = branchReset;
		internal int AssertionKind { get; } = assertionKind;
		internal int AssertionOpeningContext { get; } = assertionOpeningContext;
		internal bool NonAtomic { get; } = nonAtomic;
		internal CaptureCondition? Condition { get; } = condition;
		internal bool AssertionConditional { get; } = assertionConditional;
		internal PcreByteNode? ConditionAssertion { get; set; }
		internal bool IsConditionAssertion { get; set; }
		internal bool WaitingForCondition => AssertionConditional && ConditionAssertion is null;
		internal StaticCondition? StaticCondition { get; } = staticCondition;
		internal GroupOptions EntryOptions { get; } = entryOptions;
		internal int EntryGroupCount { get; } = entryGroupCount;
		internal int MaximumGroupCount { get; set; } = entryGroupCount;
		internal bool Quoting;
		internal List<IReadOnlyList<PcreByteNode>> Branches { get; } = new();
		internal List<PcreByteNode> Nodes { get; set; } = new();
	}

	private GroupOptions CurrentGroupOptions => new( caseless, multiline, dotAll, noAutoCapture, ungreedy, extended, asciiRestrictions, caselessRestricted, duplicateNames );

	private void RestoreGroupOptions( GroupOptions options ) {
		caseless = options.Caseless;
		caselessRestricted = options.CaselessRestricted;
		duplicateNames = options.DuplicateNames;
		multiline = options.Multiline;
		dotAll = options.DotAll;
		noAutoCapture = options.NoAutoCapture;
		ungreedy = options.Ungreedy;
		extended = options.Extended;
		asciiRestrictions = options.AsciiRestrictions;
	}

	private PcreByteAlternatives ParseAlternatives() {
		var parents = new Stack<GroupFrame>();
		var current = new GroupFrame( -1, 0, 0, false, false, CurrentGroupOptions, groupCount, 0, -1 );
		while ( true ) {
			cancellationToken.ThrowIfCancellationRequested();
			SkipPatternTrivia( ref current.Quoting );
			if ( index >= source.Length ) {
				if ( current.OpeningOffset >= 0 ) {
					if ( current.AssertionOpeningContext >= 0 ) {
						throw Invalid( current.AssertionOpeningContext, "unclosed assertion" );
					}
					// Preserve the recursive parser's outermost-unclosed-group diagnostic.
					var opening = parents.LastOrDefault( frame => frame.OpeningOffset >= 0 )?.OpeningOffset
						?? current.OpeningOffset;
					throw Invalid( opening, "unclosed group" );
				}
				return FinishAlternatives( current );
			}
			if ( !current.Quoting ) {
				if ( source[index] == (byte)'|' ) {
					if ( current.StaticCondition is { IsDefinition: true } ) {
						throw Invalid( current.OpeningOffset, "a DEFINE group has only one branch" );
					}
					if ( ( current.Condition.HasValue || current.AssertionConditional || current.StaticCondition.HasValue )
						&& current.Branches.Count != 0 ) {
						throw Invalid( current.OpeningOffset, "conditional group has more than two branches" );
					}
					FinishBranch( current );
					current.Nodes = new();
					index++;
					if ( current.BranchReset ) { groupCount = current.EntryGroupCount; }
					continue;
				}
				if ( source[index] == (byte)')' ) {
					if ( current.WaitingForCondition ) { throw Invalid( current.OpeningOffset, "conditional group requires an assertion" ); }
					var body = FinishAlternatives( current );
					if ( current.OpeningOffset < 0 ) { return body; } // Parse diagnoses unmatched ')'.
					index++;
					RestoreGroupOptions( current.EntryOptions );
					PcreByteNode completed = current.Condition is { } condition ? new ParsedCaptureConditional( condition, body )
						: current.StaticCondition is { } constant ? new PcreByteStaticConditional( constant.Value, body, constant.IsDefinition )
						: current.AssertionConditional ? new PcreByteAssertionConditional( current.ConditionAssertion!, body )
						: current.AssertionKind != 0 ? FinishAssertion( current, body )
						: current.Number > 0 ? new PcreByteGroup( current.Number, body )
						: current.Atomic ? new PcreByteAtomic( new PcreByteNoncapturingGroup( body ) )
						: new PcreByteNoncapturingGroup( body );
					var groupCaseless = current.EntryOptions.Caseless;
					var groupCaselessRestricted = current.EntryOptions.CaselessRestricted;
					var conditionAssertion = current.IsConditionAssertion;
					current = parents.Pop();
					if ( conditionAssertion ) {
						current.ConditionAssertion = completed with { Caseless = groupCaseless, CaselessRestricted = groupCaselessRestricted };
						// A condition is not a directly repeatable assertion. Leave the
						// outer conditional's eventual quantifier on the ordinary group path.
						SkipPatternTrivia( ref current.Quoting );
						if ( !current.Quoting && IsRepeatQuantifier( index ) ) {
							var repeatOffset = index;
							if ( source[index] == (byte)'{' ) { ParseCountedBounds( repeatOffset ); }
							throw Invalid( repeatOffset, "a condition assertion cannot be repeated" );
						}
					} else { AppendNode( current, completed, groupCaseless, groupCaselessRestricted ); }
					continue;
				}
				if ( !current.WaitingForCondition && TryParseOptions( index, scoped: false ) ) { continue; }
			}
			var atomCaseless = caseless;
			var atomCaselessRestricted = caselessRestricted;
			GroupFrame? child = null;
			PcreByteNode? atom;
			try {
				atom = current.Quoting ? ParseQuotedLiteral()
					: ParseAtom( current.Depth, current.AssertionOpeningContext, out child );
			} catch ( PcreByteParseException error ) when ( Math.Abs( current.AssertionKind ) == 2
				&& error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax ) {
				// Preserve the old literal-behind reader's direct-body diagnostic.
				throw Unsupported( current.OpeningOffset );
			}
			if ( child is not null ) {
				if ( current.WaitingForCondition ) {
					if ( child.AssertionKind == 0 || child.NonAtomic ) {
						throw Invalid( child.OpeningOffset, "a condition requires an atomic assertion" );
					}
					child.IsConditionAssertion = true;
				}
				parents.Push( current );
				current = child;
				continue;
			}
			if ( current.WaitingForCondition ) { throw Invalid( current.OpeningOffset, "conditional group requires an assertion" ); }
			AppendNode( current, atom!, atomCaseless, atomCaselessRestricted );
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

	private PcreByteNode FinishAssertion( GroupFrame frame, PcreByteAlternatives body ) {
		// Retain the original literal instruction and its search/work behavior.
		if ( !frame.NonAtomic && !frame.IsConditionAssertion && body.Branches.Count == 1 ) {
			var literals = body.Branches[0];
			var bytes = new List<byte>();
			Span<byte> encoded = stackalloc byte[4];
			foreach ( var node in literals ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( node is not PcreByteLiteral literal || literal.Caseless != frame.EntryOptions.Caseless
					|| utf8 && literal.Caseless && literal.CaselessRestricted != frame.EntryOptions.CaselessRestricted ) {
					return FinishGeneralAssertion( frame, body );
				}
				if ( utf8 ) {
					var width = new Rune( literal.Value ).EncodeToUtf8( encoded );
					for ( var byteIndex = 0; byteIndex < width; byteIndex++ ) { bytes.Add( encoded[byteIndex] ); }
				} else { bytes.Add( (byte)literal.Value ); }
			}
			return new PcreByteLiteralAssertion( bytes.ToArray(), frame.AssertionKind > 0, Math.Abs( frame.AssertionKind ) == 2 );
		}
		return FinishGeneralAssertion( frame, body );
	}

	private PcreByteNode FinishGeneralAssertion( GroupFrame frame, PcreByteAlternatives body ) =>
		Math.Abs( frame.AssertionKind ) == 2
			? DeferLookbehind( body, frame.AssertionKind > 0, frame.OpeningOffset, frame.NonAtomic )
			: new PcreByteLookahead( body, frame.AssertionKind > 0, frame.NonAtomic );

	private void AppendNode( GroupFrame frame, PcreByteNode atom, bool atomCaseless, bool atomCaselessRestricted ) {
		if ( atom.Caseless != atomCaseless || atom.CaselessRestricted != atomCaselessRestricted ) {
			atom = atom with { Caseless = atomCaseless, CaselessRestricted = atomCaselessRestricted };
		}
		SkipPatternTrivia( ref frame.Quoting );
		if ( !frame.Quoting && IsRepeatQuantifier( index ) ) {
			var assertion = atom is PcreByteLiteralAssertion or PcreByteLookahead or PcreByteLookbehind;
			var ignoredStatic = atom is PcreByteStaticConditional { Value: false, Body.Branches.Count: 1 };
			var quantifierOffset = index;
			var counted = source[index] == (byte)'{';
			var quantifier = counted ? (byte)0 : source[index++];
			var bounds = counted ? ParseCountedBounds( quantifierOffset )
				: (quantifier == (byte)'+' ? 1 : 0,
					quantifier == (byte)'?' ? 1 : int.MaxValue);
			if ( atom is PcreByteMatchStartReset ) {
				throw Invalid( quantifierOffset, "a match-start reset cannot be repeated" );
			}
			if ( atom is PcreByteAnchor or PcreByteSearchStart or PcreByteLineAnchor or PcreByteEndBeforeFinalLf or PcreByteWordBoundary ) {
				throw Invalid( quantifierOffset, "an anchor cannot be repeated" );
			}
			var minimum = bounds.Item1;
			var maximum = bounds.Item2;
			// PCRE2 permits useful capture side effects in repeated assertions.
			// Unlimited assertion bounds become finite; no progress marks are needed.
			var unboundedAssertion = assertion && maximum == int.MaxValue;
			if ( unboundedAssertion ) { maximum = minimum + 1; }
			repeatMarks += assertion || ignoredStatic ? 0 : counted ? maximum == int.MaxValue ? 1 : 0
				: quantifier == (byte)'+' ? 2 : quantifier == (byte)'*' ? 1 : 0;
			if ( repeatMarks > MaximumRepeatMarks ) {
				throw new PcreByteParseException( new(
					PcreByteDiagnosticCode.PatternRepeatLimitExceeded, quantifierOffset, "repeat state limit exceeded"
				) );
			}
			SkipPatternTrivia( ref frame.Quoting );
			var questionSuffix = !frame.Quoting && index < source.Length && source[index] == (byte)'?';
			if ( questionSuffix ) { index++; }
			var possessive = !frame.Quoting && !questionSuffix && index < source.Length && source[index] == (byte)'+';
			if ( possessive ) { index++; }
			// U reverses ordinary/question-suffix preference; possessive is always greedy.
			var greedy = possessive || ungreedy == questionSuffix;
			atom = new PcreByteRepeat( atom, minimum, maximum, greedy,
				counted || assertion ? quantifierOffset : -1, unboundedAssertion );
			if ( possessive && !ignoredStatic ) {
				atom = new PcreByteAtomic( atom );
			}
			SkipPatternTrivia( ref frame.Quoting );
			if ( !frame.Quoting && IsRepeatQuantifier( index ) ) {
				var repeatOffset = index;
				if ( source[index] == (byte)'{' ) { ParseCountedBounds( repeatOffset ); }
				throw Invalid( repeatOffset, "repeated quantifier" );
			}
		}
		frame.Nodes.Add( atom );
	}

	private PcreByteNode? ParseGroupOpening( int offset, int depth, int assertionOpeningContext, out GroupFrame? group ) {
		group = null;
		var capturing = !noAutoCapture;
		var atomic = false;
		var branchReset = false;
		var assertionKind = 0;
		var nonAtomic = false;
		CaptureCondition? condition = null;
		var assertionConditional = false;
		StaticCondition? staticCondition = null;
		var entryOptions = CurrentGroupOptions;
		string? name = null;
		if ( TryParseGroupAlias( out atomic, out assertionKind, out nonAtomic ) ) {
			capturing = false;
			if ( assertionKind != 0 && assertionOpeningContext < 0 ) { assertionOpeningContext = offset; }
		} else if ( index < source.Length && source[index] == (byte)'*'
			&& source.AsSpan( index + 1 ).IndexOf( (byte)')' ) >= 0 ) {
			// Other directives/group forms remain pending, not repeated empty groups.
			throw Unsupported( offset );
		} else if ( index < source.Length && source[index] == (byte)'?' ) {
			if ( TryParseOptions( offset, scoped: true ) ) {
				capturing = false;
			} else if ( index + 1 < source.Length && source[index + 1] == (byte)'(' ) {
				index += 2;
				if ( index < source.Length && source[index] is (byte)'?' or (byte)'*' ) {
					// Parse the actual inner '(' with another heap frame. Its options
					// and assertion context end before the yes/no branches begin.
					index--;
					assertionConditional = true;
				} else if ( TryParseStaticCondition( offset, out var parsedStatic ) ) {
					staticCondition = parsedStatic;
				} else { condition = ParseCaptureCondition( offset ); }
				capturing = false;
			} else if ( index + 1 < source.Length && source[index + 1] == (byte)':' ) {
				index += 2;
				capturing = false;
			} else if ( index + 1 < source.Length && source[index + 1] == (byte)'|' ) {
				index += 2;
				capturing = false;
				branchReset = true;
				hasBranchReset = true;
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
					&& source[index + 2] is (byte)'=' or (byte)'!' or (byte)'*';
				var ahead = index + 1 < source.Length && source[index + 1] is (byte)'=' or (byte)'!' or (byte)'*';
				if ( !behind && !ahead ) { throw Unsupported( offset ); }
				if ( depth >= MaximumPatternDepth ) {
					throw new PcreByteParseException( new(
						PcreByteDiagnosticCode.PatternDepthLimitExceeded, offset, "pattern nesting limit exceeded"
					) );
				}
				var assertionSymbol = source[index + (behind ? 2 : 1)];
				var positive = assertionSymbol != (byte)'!';
				nonAtomic = assertionSymbol == (byte)'*';
				index += behind ? 3 : 2;
				capturing = false;
				assertionKind = ( positive ? 1 : -1 ) * ( behind ? 2 : 1 );
				if ( assertionOpeningContext < 0 ) { assertionOpeningContext = offset; }
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
		group = new GroupFrame( offset, depth + 1, number, atomic, branchReset, entryOptions, groupCount,
			assertionKind, assertionOpeningContext, nonAtomic, condition, assertionConditional, staticCondition );
		return null;
	}
}
