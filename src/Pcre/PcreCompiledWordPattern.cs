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
		for ( var candidate = startOffset; ; ) {
			if ( !Step() ) { return Exhausted(); } // Candidate admission.
			for ( var branch = 0; branch < branches.Length; branch++ ) {
				if ( branches.Length > 1 && !Step() ) { return Exhausted(); } // Ordered branch admission.
				var nodes = branches[branch];
				var position = candidate;
				var matched = true;
				foreach ( var node in nodes ) {
					if ( !Step() ) { return Exhausted(); } // One comparison or zero-width assertion.
					switch ( node ) {
						case PcreByteLiteral literal:
							if ( PcreUtf16.TryRead( input, position, options.Utf16, out var value, out var width )
								&& LiteralEquals( value, literal ) ) {
								position += width;
							} else { matched = false; }
							break;
						case PcreByteAny any:
							if ( PcreUtf16.TryRead( input, position, options.Utf16, out var dotValue, out var dotWidth )
								&& ( any.DotAll || dotValue != '\n' ) ) {
								position += dotWidth;
							} else { matched = false; }
							break;
						case PcreByteScalarSet set:
							if ( PcreUtf16.TryRead( input, position, options.Utf16, out var classValue, out var classWidth ) ) {
								var included = false;
								var rangeValue = set.Caseless && ( options.Utf16 || options.Ucp )
									? PcreUtf8CaseFold.Fold( classValue, set.CaselessRestricted ) : classValue;
								var opposite = set.Caseless && !options.Utf16 && !options.Ucp
									? AsciiOppositeCase( classValue ) : rangeValue;
								foreach ( var range in set.Ranges.Span ) {
									if ( rangeValue >= range.Start && rangeValue <= range.End
										|| opposite >= range.Start && opposite <= range.End ) { included = true; break; }
								}
								if ( !included ) {
									foreach ( var predicate in set.Predicates.Span ) {
										if ( PcreScalarClassMembership.Contains( predicate.Kind, classValue, predicate.Category,
											set.Caseless, predicate.PropertyValue ) != predicate.Negated ) {
											included = true; break;
										}
									}
								}
								if ( included != set.Negated ) { position += classWidth; }
								else { matched = false; }
							} else { matched = false; }
							break;
						case PcreByteScalarClass property:
							if ( PcreUtf16.TryRead( input, position, options.Utf16, out var propertyValue, out var propertyWidth )
								&& PcreScalarClassMembership.Contains( property.Kind, propertyValue, property.Category,
									property.Caseless, property.PropertyValue ) != property.Negated ) {
								position += propertyWidth;
							} else { matched = false; }
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
