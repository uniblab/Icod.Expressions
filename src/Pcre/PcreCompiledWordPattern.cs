namespace Icod.Expressions.Pcre;

/// <summary>Immutable width-16 sequence; matching reads the original subject view.</summary>
internal sealed class PcreCompiledWordPattern {
	private readonly PcreByteNode[][] branches;
	private readonly PcreWordOptions options;
	internal PcreCompiledWordPattern( PcreByteNode[] nodes, PcreWordOptions options ) : this( [nodes], options ) { }
	internal PcreCompiledWordPattern( PcreByteNode[][] branches, PcreWordOptions options ) {
		this.branches = branches;
		this.options = options;
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
			// Supported sequences have no lookbehind. Native strict validation starts
			// at the requested offset; an earlier malformed prefix is not inspected.
			if ( startOffset > 0 && startOffset < input.Length && Char.IsLowSurrogate( input[startOffset] ) ) {
				return new( null, new( PcreWordDiagnosticCode.InvalidStartOffset, startOffset ) );
			}
			for ( var offset = startOffset; offset < input.Length; ) {
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
		// -1: work exhausted; 0: failed or policy-rejected; 1: complete match.
		int TryFinish( PcreByteNode[] nodes, int first, ReadOnlySpan<char> source, int at,
			int candidate, out int end ) {
			end = at;
			for ( var index = first; index < nodes.Length; index++ ) {
				if ( !Step() ) { return -1; }
				switch ( nodes[index] ) {
					case PcreByteLiteral or PcreByteAny or PcreByteScalarSet or PcreByteScalarClass:
						if ( !TryConsumeAtom( nodes[index], source, end, out var width ) ) { return 0; }
						end += width;
						break;
					case PcreByteAnchor anchor:
						if ( end != ( anchor.IsStart ? 0 : source.Length ) ) { return 0; }
						break;
					case PcreByteSearchStart:
						if ( end != startOffset ) { return 0; }
						break;
					case PcreByteRepeat repeat:
						var count = 0;
						if ( repeat.Greedy ) {
							while ( true ) {
								if ( !Step() ) { return -1; }
								if ( !TryConsumeAtom( repeat.Child, source, end, out var repeatWidth ) ) { break; }
								end += repeatWidth;
								count++;
							}
							while ( count >= repeat.Minimum ) {
								var outcome = TryFinish( nodes, index + 1, source, end, candidate, out var acceptedEnd );
								if ( outcome != 0 ) { end = acceptedEnd; return outcome; }
								if ( index == nodes.Length - 1 || count == repeat.Minimum ) { break; }
								if ( !Step() ) { return -1; } // Rewind one previously matched scalar.
								if ( !PcreUtf16.TryReadPrevious( source, end, options.Utf16, out _, out var rewindWidth ) ) {
									throw new InvalidOperationException( "Invalid word repeat rewind." );
								}
								end -= rewindWidth;
								count--;
							}
						} else {
							while ( true ) {
								if ( count >= repeat.Minimum ) {
									var outcome = TryFinish( nodes, index + 1, source, end, candidate, out var acceptedEnd );
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
						case PcreByteLiteral or PcreByteAny or PcreByteScalarSet or PcreByteScalarClass:
							matched = TryConsumeAtom( node, input, position, out var atomWidth );
							if ( matched ) { position += atomWidth; }
							break;
						case PcreByteRepeat repeat:
							var count = 0;
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
									if ( !Step() ) { return Exhausted(); } // Rewind one previously matched scalar.
									if ( !PcreUtf16.TryReadPrevious( input, position, options.Utf16, out _, out var width ) ) {
										throw new InvalidOperationException( "Invalid word repeat rewind." );
									}
									position -= width;
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
						case PcreByteAnchor anchor: matched = position == ( anchor.IsStart ? 0 : input.Length ); break;
						case PcreByteSearchStart: matched = position == startOffset; break;
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
		return node switch {
			PcreByteLiteral literal => LiteralEquals( value, literal ),
			PcreByteAny any => any.DotAll || value != '\n',
			PcreByteScalarClass property => PcreScalarClassMembership.Contains( property.Kind, value,
				property.Category, property.Caseless, property.PropertyValue ) != property.Negated,
			PcreByteScalarSet set => ContainsSet( set, value ),
			_ => throw new InvalidOperationException( "Unsupported word repeat atom." )
		};
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
