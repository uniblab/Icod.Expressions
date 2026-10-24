namespace Icod.Expressions.Pcre;

/// <summary>Executes ordered bytecode with a bounded explicit backtracking stack.</summary>
internal static class PcreByteMatcher {
	private const int MaximumBacktrackFrames = 1024;

	internal static PcreByteMatchResult Match(
		PcreByteProgram program,
		int groupCount,
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
		var work = 0;
		void Step() {
			cancellationToken.ThrowIfCancellationRequested();
			if ( work >= matchWorkLimit ) {
				throw new MatchLimitException( PcreByteDiagnosticCode.MatchWorkLimitExceeded, "match work limit exceeded" );
			}
			work++;
			workObserver?.Invoke( work );
			cancellationToken.ThrowIfCancellationRequested();
		}

		try {
			for ( var start = startOffset; start <= input.Length; start++ ) {
				Step(); // Candidate start.
				var position = start;
				var pc = 0;
				var captures = new PcreByteCapture?[groupCount + 1];
				var captureStarts = new int[groupCount + 1];
				var marks = new int[program.MarkCount];
				var backtrack = new Stack<Frame>();
				while ( true ) {
					Step(); // Instruction transition.
					var instruction = program.Instructions[pc];
					var failed = false;
					switch ( instruction.Opcode ) {
						case PcreByteOpcode.Literal:
							Step(); // Attempted byte comparison.
							if ( position < input.Length && input.Span[position] == (byte)instruction.First ) {
								position++;
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.Class:
							Step(); // Attempted byte set membership.
							if ( position < input.Length && instruction.Set.Contains( input.Span[position] ) ) {
								position++;
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.Backreference:
							var capture = captures[instruction.First];
							if ( !capture.HasValue ) {
								failed = true;
								break;
							}
							var captured = capture.Value;
							for ( var byteIndex = 0; byteIndex < captured.ByteLength; byteIndex++ ) {
								Step(); // Attempted captured-byte comparison.
								if ( byteIndex >= input.Length - position
									|| input.Span[captured.ByteIndex + byteIndex] != input.Span[position + byteIndex] ) {
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
							var beforeWord = false;
							if ( position > 0 ) {
								Step(); // Classify the preceding source byte.
								beforeWord = PcreByteSet.Word.Contains( input.Span[position - 1] );
							}
							var afterWord = false;
							if ( position < input.Length ) {
								Step(); // Classify the following source byte.
								afterWord = PcreByteSet.Word.Contains( input.Span[position] );
							}
							if ( ( beforeWord != afterWord ) == ( instruction.First == 1 ) ) { pc++; }
							else { failed = true; }
							break;
						case PcreByteOpcode.AssertLiteral:
							var literal = instruction.Bytes.Span;
							var adjacent = instruction.Second == 1 ? position - literal.Length : position;
							var equals = adjacent >= 0 && literal.Length <= input.Length - adjacent;
							if ( equals ) {
								for ( var byteIndex = 0; byteIndex < literal.Length; byteIndex++ ) {
									Step(); // Attempted assertion byte comparison.
									if ( input.Span[adjacent + byteIndex] != literal[byteIndex] ) {
										equals = false;
										break;
									}
								}
							}
							if ( equals == ( instruction.First == 1 ) ) { pc++; }
							else { failed = true; }
							break;
						case PcreByteOpcode.Start:
							if ( position == 0 ) { pc++; } else { failed = true; }
							break;
						case PcreByteOpcode.End:
							if ( position == input.Length ) { pc++; } else { failed = true; }
							break;
						case PcreByteOpcode.CaptureStart:
							captureStarts[instruction.First] = position;
							pc++;
							break;
						case PcreByteOpcode.CaptureEnd:
							captures[instruction.First] = new( captureStarts[instruction.First], position - captureStarts[instruction.First] );
							pc++;
							break;
						case PcreByteOpcode.Split:
							if ( backtrack.Count >= MaximumBacktrackFrames ) {
								throw new MatchLimitException( PcreByteDiagnosticCode.MatchDepthLimitExceeded, "backtrack stack limit exceeded" );
							}
							backtrack.Push( new Frame(
								instruction.Second, position,
								(PcreByteCapture?[])captures.Clone(), (int[])captureStarts.Clone(), (int[])marks.Clone()
							) );
							pc = instruction.First;
							break;
						case PcreByteOpcode.Jump:
							pc = instruction.First;
							break;
						case PcreByteOpcode.Mark:
							marks[instruction.First] = position;
							pc++;
							break;
						case PcreByteOpcode.Progress:
							if ( position == marks[instruction.First] ) { pc = instruction.Second; }
							else { pc++; }
							break;
						case PcreByteOpcode.Accept:
							captures[0] = new( start, position - start );
							return new( new PcreByteMatch( start, position - start, Array.AsReadOnly( captures ) ), null );
						default:
							throw new InvalidOperationException( "Compiler produced an unknown PCRE byte instruction." );
					}
					if ( !failed ) { continue; }
					if ( backtrack.Count == 0 ) { break; }
					var frame = backtrack.Pop();
					pc = frame.Pc;
					position = frame.Position;
					captures = frame.Captures;
					captureStarts = frame.CaptureStarts;
					marks = frame.Marks;
				}
			}
			return new( null, null );
		} catch ( MatchLimitException error ) {
			return new( null, new( error.Code, 0, error.Message ) );
		}
	}

	private sealed record Frame(
		int Pc, int Position, PcreByteCapture?[] Captures, int[] CaptureStarts, int[] Marks
	);

	private sealed class MatchLimitException( PcreByteDiagnosticCode code, string message ) : Exception( message ) {
		internal PcreByteDiagnosticCode Code { get; } = code;
	}
}
