using System.Globalization;
using System.Text;

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
			if ( program.Utf8 ) {
				for ( var offset = 0; offset < input.Length; ) {
					Step(); // Validate one scalar within the same match work budget.
					if ( !PcreUtf8.TryRead( input.Span, offset, out _, out var width ) ) {
						return new( null, new( PcreByteDiagnosticCode.InvalidUtf8Subject, offset,
							"subject contains invalid UTF-8" ) );
					}
					offset += width;
				}
				if ( startOffset < input.Length && ( input.Span[startOffset] & 0xC0 ) == 0x80 ) {
					return new( null, new( PcreByteDiagnosticCode.InvalidStartOffset, startOffset,
						"start byte offset is inside a UTF-8 character" ) );
				}
			}
			for ( var start = startOffset; start <= input.Length; start++ ) {
				if ( program.Utf8 && start < input.Length && ( input.Span[start] & 0xC0 ) == 0x80 ) {
					continue; // Search candidate boundaries, never continuation bytes.
				}
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
							Step(); // Attempted scalar/byte comparison.
							var literalWidth = 1;
							if ( position < input.Length && ( program.Utf8
								? PcreUtf8.TryRead( input.Span, position, out var scalar, out literalWidth )
									&& ScalarEquals( scalar, instruction.First, program.Caseless )
								: ScalarEquals( input.Span[position], instruction.First, program.Caseless ) ) ) {
								position += literalWidth;
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.Any:
							Step(); // Attempted scalar consumption.
							if ( PcreUtf8.TryRead( input.Span, position, out var value, out var scalarWidth )
								&& ( instruction.First == 1 || value != (int)'\n' ) ) {
								position += scalarWidth;
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.LetterProperty:
							Step(); // Attempted Unicode property evaluation.
							if ( PcreUtf8.TryRead( input.Span, position, out var propertyScalar, out var propertyWidth )
								&& IsLetter( propertyScalar ) != ( instruction.First == 1 ) ) {
								position += propertyWidth;
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
									|| !ScalarEquals( input.Span[captured.ByteIndex + byteIndex], input.Span[position + byteIndex], program.Caseless ) ) {
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
									if ( !ScalarEquals( input.Span[adjacent + byteIndex], literal[byteIndex], program.Caseless ) ) {
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
						case PcreByteOpcode.StartLine:
							if ( position == 0 || ( position < input.Length && input.Span[position - 1] == (byte)'\n' ) ) {
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.EndLine:
							if ( position == input.Length || input.Span[position] == (byte)'\n' ) {
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.EndBeforeFinalLf:
							if ( position == input.Length || ( position == input.Length - 1 && input.Span[position] == (byte)'\n' ) ) {
								pc++;
							} else { failed = true; }
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

	private static bool ScalarEquals( int left, int right, bool caseless ) {
		if ( left == right ) { return true; }
		if ( !caseless ) { return false; }
		if ( left is >= (byte)'A' and <= (byte)'Z' ) { left = (byte)( left + 32 ); }
		if ( right is >= (byte)'A' and <= (byte)'Z' ) { right = (byte)( right + 32 ); }
		return left == right;
	}

	private static bool IsLetter( int scalar ) => Rune.GetUnicodeCategory( new Rune( scalar ) ) is
		UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
		or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter
		or UnicodeCategory.OtherLetter;

	private sealed record Frame(
		int Pc, int Position, PcreByteCapture?[] Captures, int[] CaptureStarts, int[] Marks
	);

	private sealed class MatchLimitException( PcreByteDiagnosticCode code, string message ) : Exception( message ) {
		internal PcreByteDiagnosticCode Code { get; } = code;
	}
}
