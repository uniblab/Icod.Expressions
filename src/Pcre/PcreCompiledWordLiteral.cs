namespace Icod.Expressions.Pcre;

/// <summary>Immutable width-16 literal program; matching reads the original subject view.</summary>
internal sealed class PcreCompiledWordLiteral {
	private readonly PcreByteLiteral[] literals;
	private readonly PcreWordLiteralOptions options;
	internal PcreCompiledWordLiteral( PcreByteLiteral[] literals, PcreWordLiteralOptions options ) {
		this.literals = literals;
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
			// Literal programs have no lookbehind. Native strict validation starts
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
			var position = candidate;
			var matched = true;
			foreach ( var literal in literals ) {
				if ( !Step() ) { return Exhausted(); } // One scalar/unit comparison, including end failure.
				if ( !PcreUtf16.TryRead( input, position, options.Utf16, out var value, out var width ) || value != literal.Value ) {
					matched = false;
					break;
				}
				position += width;
			}
			if ( matched ) {
				if ( !Step() ) { return Exhausted(); } // Root completion, including policy rejection.
				var empty = position == candidate;
				if ( ( !endAnchored || position == input.Length )
					&& !( empty && ( notEmpty || notEmptyAtStart && candidate == startOffset ) ) ) {
					return new( new( candidate, position - candidate ), null );
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
}
