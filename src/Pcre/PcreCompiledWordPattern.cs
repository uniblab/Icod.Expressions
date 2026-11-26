namespace Icod.Expressions.Pcre;

/// <summary>Immutable width-16 sequence; matching reads the original subject view.</summary>
internal sealed class PcreCompiledWordPattern {
	private readonly PcreByteNode[][] branches;
	private readonly PcreWordOptions options;
	private readonly bool inspectPreviousScalar;
	private readonly char newlineCharacter;
	private readonly bool bsrAnyCrLf;
	internal PcreCompiledWordPattern( PcreByteNode[] nodes, PcreWordOptions options ) : this( [nodes], options ) { }
	internal PcreCompiledWordPattern( PcreByteNode[][] branches, PcreWordOptions options, bool inspectPreviousScalar = false,
		PcreByteNewline newline = PcreByteNewline.Lf, bool bsrAnyCrLf = false ) {
		this.branches = branches;
		this.options = options;
		this.inspectPreviousScalar = inspectPreviousScalar;
		newlineCharacter = (char)newline.Character();
		this.bsrAnyCrLf = bsrAnyCrLf;
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
		// -1: work exhausted; 0: failed or policy-rejected; 1: complete match.
		int TryFinish( PcreByteNode[] nodes, int first, ReadOnlySpan<char> source, int at,
			int candidate, out int end ) {
			end = at;
			for ( var index = first; index < nodes.Length; index++ ) {
				if ( !Step() ) { return -1; }
				switch ( nodes[index] ) {
					case PcreByteLiteral or PcreByteAny or PcreByteScalarSet or PcreByteScalarClass or PcreByteNewlineSequence:
						if ( !TryConsumeAtom( nodes[index], source, end, out var width ) ) { return 0; }
						end += width;
						break;
					case PcreByteAnchor or PcreByteLineAnchor or PcreByteEndBeforeFinalLf:
						if ( !MatchAnchor( nodes[index], source, end ) ) { return 0; }
						break;
					case PcreByteSearchStart:
						if ( end != startOffset ) { return 0; }
						break;
					case PcreByteWordBoundary boundary:
						var boundaryResult = MatchBoundary( boundary, source, end );
						if ( boundaryResult != 1 ) { return boundaryResult; }
						break;
					case PcreByteRepeat repeat:
						var count = 0;
						var repeatStart = end;
						if ( repeat.Greedy ) {
							while ( true ) {
								if ( !Step() ) { return -1; }
								if ( !TryConsumeAtom( repeat.Child, source, end, out var repeatWidth ) ) { break; }
								end += repeatWidth;
								count++;
							}
							while ( count >= repeat.Minimum ) {
								var outcome = TryRepeatFinish( nodes, index + 1, source, end, candidate, out var acceptedEnd );
								if ( outcome != 0 ) { end = acceptedEnd; return outcome; }
								if ( index == nodes.Length - 1 || count == repeat.Minimum ) { break; }
								if ( !Step() ) { return -1; } // Rewind one complete matched atom.
								end -= RepeatRewindWidth( repeat.Child, source, end, repeatStart );
								count--;
							}
						} else {
							while ( true ) {
								if ( count >= repeat.Minimum ) {
									var outcome = TryRepeatFinish( nodes, index + 1, source, end, candidate, out var acceptedEnd );
									if ( outcome != 0 ) { end = acceptedEnd; return outcome; }
								}
								if ( !Step() ) { return -1; }
								if ( !TryConsumeAtom( repeat.Child, source, end, out var repeatWidth ) ) { break; }
								end += repeatWidth;
								count++;
							}
						}
						return 0;
					default: throw new InvalidOperationException( "Unsupported word repeat continuation." );
				}
			}
			if ( !Step() ) { return -1; }
			var empty = end == candidate;
			return ( !endAnchored || end == source.Length )
				&& !( empty && ( notEmpty || notEmptyAtStart && candidate == startOffset ) ) ? 1 : 0;
		}
		int TryRepeatFinish( PcreByteNode[] nodes, int first, ReadOnlySpan<char> source, int at,
			int candidate, out int end ) {
			if ( first >= nodes.Length || nodes[first] is not PcreWordSuffixChoices choices ) {
				return TryFinish( nodes, first, source, at, candidate, out end );
			}
			foreach ( var suffix in choices.Alternatives ) {
				if ( choices.Alternatives.Length > 1 && !Step() ) { end = at; return -1; }
				var outcome = TryFinish( suffix, 0, source, at, candidate, out end );
				if ( outcome != 0 ) { return outcome; }
			}
			end = at;
			return 0;
		}
		for ( var candidate = startOffset; ; ) {
			if ( !Step() ) { return Exhausted(); } // Candidate admission.
			for ( var branch = 0; branch < branches.Length; branch++ ) {
				if ( branches.Length > 1 && !Step() ) { return Exhausted(); } // Ordered branch admission.
				var nodes = branches[branch];
				var position = candidate;
				var matched = true;
				for ( var nodeIndex = 0; nodeIndex < nodes.Length; nodeIndex++ ) {
					var node = nodes[nodeIndex];
					if ( !Step() ) { return Exhausted(); } // Comparison, repeat admission or assertion.
					switch ( node ) {
						case PcreByteLiteral or PcreByteAny or PcreByteScalarSet or PcreByteScalarClass or PcreByteNewlineSequence:
							matched = TryConsumeAtom( node, input, position, out var atomWidth );
							if ( matched ) { position += atomWidth; }
							break;
						case PcreByteRepeat repeat:
							var count = 0;
							var repeatStart = position;
							if ( repeat.Greedy ) {
								while ( true ) {
									if ( !Step() ) { return Exhausted(); }
									if ( !TryConsumeAtom( repeat.Child, input, position, out var repeatWidth ) ) { break; }
									position += repeatWidth;
									count++;
								}
								while ( count >= repeat.Minimum ) {
									var outcome = TryRepeatFinish( nodes, nodeIndex + 1, input, position, candidate, out var end );
									if ( outcome < 0 ) { return Exhausted(); }
									if ( outcome > 0 ) { return new( new( candidate, end - candidate ), null ); }
									if ( nodeIndex == nodes.Length - 1 || count == repeat.Minimum ) { break; }
									if ( !Step() ) { return Exhausted(); } // Rewind one complete matched atom.
									position -= RepeatRewindWidth( repeat.Child, input, position, repeatStart );
									count--;
								}
							} else {
								while ( true ) {
									if ( count >= repeat.Minimum ) {
										var outcome = TryRepeatFinish( nodes, nodeIndex + 1, input, position, candidate, out var end );
										if ( outcome < 0 ) { return Exhausted(); }
										if ( outcome > 0 ) { return new( new( candidate, end - candidate ), null ); }
									}
									if ( !Step() ) { return Exhausted(); }
									if ( !TryConsumeAtom( repeat.Child, input, position, out var repeatWidth ) ) { break; }
									position += repeatWidth;
									count++;
								}
							}
							matched = false; // Every eligible count has failed or been rejected.
							break;
						case PcreByteAnchor or PcreByteLineAnchor or PcreByteEndBeforeFinalLf:
							matched = MatchAnchor( node, input, position );
							break;
						case PcreByteSearchStart: matched = position == startOffset; break;
						case PcreByteWordBoundary boundary:
							var boundaryResult = MatchBoundary( boundary, input, position );
							if ( boundaryResult < 0 ) { return Exhausted(); }
							matched = boundaryResult == 1;
							break;
						default: throw new InvalidOperationException( "Unsupported node reached the word sequence executor." );
					}
					if ( !matched ) { break; }
				}
				if ( matched ) {
					if ( !Step() ) { return Exhausted(); } // Root completion, including policy rejection.
					var empty = position == candidate;
					if ( ( !endAnchored || position == input.Length )
						&& !( empty && ( notEmpty || notEmptyAtStart && candidate == startOffset ) ) ) {
						return new( new( candidate, position - candidate ), null );
					}
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

	private bool LiteralEquals( int value, PcreByteLiteral literal ) {
		if ( value == literal.Value ) { return true; }
		if ( !literal.Caseless ) { return false; }
		if ( options.Utf16 || options.Ucp ) {
			return PcreUtf8CaseFold.Equals( value, literal.Value, literal.CaselessRestricted );
		}
		return AsciiOppositeCase( value ) == literal.Value;
	}

	private static int AsciiOppositeCase( int value ) => value is >= 'A' and <= 'Z' ? value + 32
		: value is >= 'a' and <= 'z' ? value - 32 : value;
}
