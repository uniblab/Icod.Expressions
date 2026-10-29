namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private bool hasCaptureConditions;
	private readonly record struct CaptureCondition( int Number, string? Name, int Offset,
		bool RecursionFallback = false, bool RecursionTest = false );
	// Resolve after all declarations, before lookbehind analysis and compilation.
	private sealed record ParsedCaptureConditional( CaptureCondition Condition, PcreByteAlternatives Body ) : PcreByteNode;

	private CaptureCondition ParseCaptureCondition( int offset ) {
		hasCaptureConditions = true;
		if ( index >= source.Length ) { throw Invalid( offset, "unclosed capture condition" ); }
		var first = source[index];
		if ( first is >= (byte)'0' and <= (byte)'9' or (byte)'+' or (byte)'-' ) {
			var relative = first is (byte)'+' or (byte)'-';
			if ( relative ) { index++; }
			var start = index;
			var number = 0;
			while ( index < source.Length && source[index] is >= (byte)'0' and <= (byte)'9' ) {
				cancellationToken.ThrowIfCancellationRequested();
				var digit = source[index++] - (byte)'0';
				if ( number > ( MaximumCaptureCount - digit ) / 10 ) {
					throw Invalid( offset, "capture condition target exceeds capture limit" );
				}
				number = number * 10 + digit;
			}
			if ( start == index || number == 0 ) { throw Invalid( offset, "capture condition requires a nonzero target" ); }
			if ( relative ) { number = first == (byte)'+' ? groupCount + number : groupCount + 1 - number; }
			FinishCaptureConditionHeader( offset );
			return new( number, null, offset );
		}
		if ( first is (byte)'<' or (byte)'\'' ) {
			index++;
			var wrapped = ParseCaptureName( first == (byte)'<' ? (byte)'>' : (byte)'\'', offset );
			FinishCaptureConditionHeader( offset );
			return new( 0, wrapped, offset );
		}
		var remaining = source.AsSpan( index );
		if ( remaining.StartsWith( "R&"u8 ) ) {
			index += 2;
			return new( 0, ParseCaptureName( (byte)')', offset ), offset, RecursionTest: true );
		}
		if ( first is (byte)'?' or (byte)'*' ) {
			throw Unsupported( offset );
		}
		var name = ParseCaptureName( (byte)')', offset );
		var recursionFallback = name[0] == 'R';
		for ( var digit = 1; recursionFallback && digit < name.Length; digit++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			recursionFallback = name[digit] is >= '0' and <= '9';
		}
		return new( 0, name, offset, recursionFallback );
	}

	private void FinishCaptureConditionHeader( int offset ) {
		if ( index >= source.Length || source[index] != (byte)')' ) {
			throw Invalid( offset, "invalid capture condition header" );
		}
		index++;
	}

	private PcreByteAlternatives ResolveCaptureConditions( PcreByteAlternatives root ) {
		var numericTargets = new Dictionary<int, ReadOnlyMemory<int>>();
		var namedTargets = new Dictionary<string, ReadOnlyMemory<int>>( StringComparer.Ordinal );
		ReadOnlyMemory<int> NumericTarget( int number ) {
			if ( !numericTargets.TryGetValue( number, out var target ) ) {
				target = new[] { number };
				numericTargets.Add( number, target );
			}
			return target;
		}
		return RewriteTree( root, node => {
			if ( node is not ParsedCaptureConditional parsed ) { return node; }
			var condition = parsed.Condition;
			ReadOnlyMemory<int> targets;
			if ( condition.Name is { } name ) {
				if ( !namedCaptures.TryGetValue( name, out var numbers ) ) {
					if ( condition.RecursionFallback ) {
						// Only absent names fall back to numeric recursion tests.
						// R and every all-zero suffix mean any active call.
						var number = 0;
						for ( var digitIndex = 1; digitIndex < name.Length; digitIndex++ ) {
							cancellationToken.ThrowIfCancellationRequested();
							var digit = name[digitIndex] - '0';
							if ( number > ( MaximumCaptureCount - digit ) / 10 ) {
								throw Invalid( condition.Offset, "recursion condition target exceeds capture limit" );
							}
							number = number * 10 + digit;
						}
						if ( number > groupCount ) { throw Invalid( condition.Offset, "recursion condition has no capture group" ); }
						return new PcreByteRecursionConditional( number == 0 ? ReadOnlyMemory<int>.Empty : NumericTarget( number ), parsed.Body ) {
							Caseless = parsed.Caseless, CaselessRestricted = parsed.CaselessRestricted
						};
					}
					throw Invalid( condition.Offset, "capture condition has no capture name" );
				}
				if ( !namedTargets.TryGetValue( name, out targets ) ) {
					targets = numbers.Count == 1 ? NumericTarget( numbers[0] ) : numbers.ToArray();
					namedTargets.Add( name, targets );
				}
			} else {
				if ( condition.Number <= 0 || condition.Number > groupCount ) {
					throw Invalid( condition.Offset, "capture condition has no capture group" );
				}
				targets = NumericTarget( condition.Number );
			}
			if ( condition.RecursionTest ) {
				return new PcreByteRecursionConditional( targets, parsed.Body ) {
					Caseless = parsed.Caseless, CaselessRestricted = parsed.CaselessRestricted
				};
			}
			return new PcreByteCaptureConditional( targets, parsed.Body ) {
				Caseless = parsed.Caseless, CaselessRestricted = parsed.CaselessRestricted
			};
		} );
	}
}
