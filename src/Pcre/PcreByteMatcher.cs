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
			var fragments = new List<Fragment>();
			if ( program.Utf8 ) {
				var fragmentStart = 0;
				var hasValidScalar = false;
				for ( var offset = 0; offset < input.Length; ) {
					Step(); // Inspect one scalar or invalid byte within the match work budget.
					if ( !PcreUtf8.TryRead( input.Span, offset, out _, out var width ) ) {
						if ( !program.MatchInvalidUtf ) {
							return new( null, new( PcreByteDiagnosticCode.InvalidUtf8Subject, offset,
								"subject contains invalid UTF-8" ) );
						}
						if ( hasValidScalar ) {
							fragments.Add( new( fragmentStart, offset ) );
						} else if ( ( input.Span[offset] & 0xC0 ) != 0x80 ) {
							fragments.Add( new( offset, offset ) );
						}
						hasValidScalar = false;
						offset++;
						continue;
					}
					if ( !hasValidScalar ) { fragmentStart = offset; }
					hasValidScalar = true;
					offset += width;
				}
				fragments.Add( new( hasValidScalar ? fragmentStart : input.Length, input.Length ) );
				if ( !program.MatchInvalidUtf && startOffset < input.Length
					&& ( input.Span[startOffset] & 0xC0 ) == 0x80 ) {
					return new( null, new( PcreByteDiagnosticCode.InvalidStartOffset, startOffset,
						"start byte offset is inside a UTF-8 character" ) );
				}
			} else {
				fragments.Add( new( 0, input.Length ) );
			}
			var crossedBarrier = false;
			foreach ( var fragment in fragments ) {
				if ( fragment.Start < fragment.End ) { crossedBarrier = false; }
				if ( fragment.Start == fragment.End && crossedBarrier ) { continue; }
				var barrierCandidateTried = false;
				for ( var start = Math.Max( startOffset, fragment.Start ); start <= fragment.End; start++ ) {
					if ( program.MatchInvalidUtf && start == startOffset && start == fragment.End
						&& start < input.Length && ( input.Span[start] & 0xC0 ) == 0x80 ) {
						continue; // Starting at a stray continuation skips it; an earlier search can see the preceding edge.
					}
					if ( program.Utf8 && start < fragment.End && ( input.Span[start] & 0xC0 ) == 0x80 ) {
						continue; // Search candidate boundaries, never continuation bytes.
					}
					if ( fragment.End < input.Length && start == fragment.End ) {
						barrierCandidateTried = true;
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
							if ( position < fragment.End && ( program.Utf8
								? PcreUtf8.TryRead( input.Span, position, out var scalar, out literalWidth )
									&& ScalarEquals( scalar, instruction.First, program.Caseless )
								: ScalarEquals( input.Span[position], instruction.First, program.Caseless ) ) ) {
								position += literalWidth;
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.Any:
							Step(); // Attempted scalar consumption.
							if ( position < fragment.End
								&& PcreUtf8.TryRead( input.Span, position, out var value, out var scalarWidth )
								&& ( instruction.First == 1 || value != (int)'\n' ) ) {
								position += scalarWidth;
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.ScalarClass:
							Step(); // Attempted scalar class evaluation.
							if ( position < fragment.End
								&& PcreUtf8.TryRead( input.Span, position, out var propertyScalar, out var propertyWidth )
								&& ScalarClassContains( (PcreByteScalarClassKind)instruction.First, propertyScalar )
									!= ( instruction.Second == 1 ) ) {
								position += propertyWidth;
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.ScalarSet:
							Step(); // Attempted scalar-set evaluation.
							if ( position >= fragment.End
								|| !PcreUtf8.TryRead( input.Span, position, out var setScalar, out var setWidth ) ) {
								failed = true;
								break;
							}
							var contained = false;
							foreach ( var range in instruction.Ranges.Span ) {
								Step(); // Each attempted interval inspection consumes work.
								if ( setScalar >= range.Start && setScalar <= range.End ) { contained = true; break; }
							}
							if ( !contained ) {
								foreach ( var predicate in instruction.Predicates.Span ) {
									Step(); // Each attempted scalar predicate consumes work.
									if ( ScalarClassContains( predicate.Kind, setScalar ) != predicate.Negated ) {
										contained = true;
										break;
									}
								}
							}
							if ( contained != ( instruction.Second == 1 ) ) {
								position += setWidth;
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.Class:
							Step(); // Attempted byte set membership.
							if ( position < fragment.End && instruction.Set.Contains( input.Span[position] ) ) {
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
							if ( captured.ByteLength > fragment.End - position ) {
								failed = true;
								break;
							}
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
							if ( position > fragment.Start ) {
								Step(); // Classify the preceding source byte or UTF-8 scalar.
								if ( instruction.Second == 1 ) {
									var previous = position - 1;
									while ( ( input.Span[previous] & 0xC0 ) == 0x80 ) { previous--; }
									PcreUtf8.TryRead( input.Span, previous, out var beforeScalar, out _ );
									beforeWord = ScalarClassContains( PcreByteScalarClassKind.UnicodeWord, beforeScalar );
								} else { beforeWord = PcreByteSet.Word.Contains( input.Span[position - 1] ); }
							}
							var afterWord = false;
							if ( position < fragment.End ) {
								Step(); // Classify the following source byte or UTF-8 scalar.
								if ( instruction.Second == 1 ) {
									PcreUtf8.TryRead( input.Span, position, out var afterScalar, out _ );
									afterWord = ScalarClassContains( PcreByteScalarClassKind.UnicodeWord, afterScalar );
								} else { afterWord = PcreByteSet.Word.Contains( input.Span[position] ); }
							}
							if ( ( beforeWord != afterWord ) == ( instruction.First == 1 ) ) { pc++; }
							else { failed = true; }
							break;
						case PcreByteOpcode.AssertLiteral:
							var literal = instruction.Bytes.Span;
							var adjacent = instruction.Second == 1 ? position - literal.Length : position;
							var equals = adjacent >= fragment.Start && literal.Length <= fragment.End - adjacent;
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
							if ( position == fragment.End ) { pc++; } else { failed = true; }
							break;
						case PcreByteOpcode.StartLine:
							if ( position == 0 || ( position > fragment.Start && position < fragment.End
								&& input.Span[position - 1] == (byte)'\n' ) ) {
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.EndLine:
							if ( position == input.Length || ( position < fragment.End && input.Span[position] == (byte)'\n' ) ) {
								pc++;
							} else { failed = true; }
							break;
						case PcreByteOpcode.EndBeforeFinalLf:
							var end = instruction.First == 1 ? fragment.End : input.Length;
							if ( position == end || ( position == end - 1 && input.Span[position] == (byte)'\n' ) ) {
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
				if ( barrierCandidateTried ) { crossedBarrier = true; }
			}
			return new( null, null );
		} catch ( MatchLimitException error ) {
			return new( null, new( error.Code, 0, error.Message ) );
		}
	}

	private readonly record struct Fragment( int Start, int End );

	private static bool ScalarEquals( int left, int right, bool caseless ) {
		if ( left == right ) { return true; }
		if ( !caseless ) { return false; }
		if ( left is >= (byte)'A' and <= (byte)'Z' ) { left = (byte)( left + 32 ); }
		if ( right is >= (byte)'A' and <= (byte)'Z' ) { right = (byte)( right + 32 ); }
		return left == right;
	}

	private static bool ScalarClassContains( PcreByteScalarClassKind kind, int scalar ) {
		if ( kind == PcreByteScalarClassKind.AsciiDigit ) { return scalar is >= '0' and <= '9'; }
		if ( kind == PcreByteScalarClassKind.AsciiWord ) {
			return scalar <= 0x7F && PcreByteSet.Word.Contains( (byte)scalar );
		}
		if ( kind == PcreByteScalarClassKind.AsciiSpace ) {
			return scalar <= 0x7F && PcreByteSet.WhiteSpace.Contains( (byte)scalar );
		}
		if ( kind == PcreByteScalarClassKind.UnicodeSpace
			&& scalar is >= 0x09 and <= 0x0D or 0x85 or 0x180E ) {
			return true;
		}
		var category = Rune.GetUnicodeCategory( new Rune( scalar ) );
		return kind switch {
			PcreByteScalarClassKind.DecimalDigit => category == UnicodeCategory.DecimalDigitNumber,
			PcreByteScalarClassKind.Letter => category is
				UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
				or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter
				or UnicodeCategory.OtherLetter,
			PcreByteScalarClassKind.UnicodeWord => category is
				UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
				or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter
				or UnicodeCategory.OtherLetter or UnicodeCategory.DecimalDigitNumber
				or UnicodeCategory.LetterNumber or UnicodeCategory.OtherNumber
				or UnicodeCategory.NonSpacingMark or UnicodeCategory.ConnectorPunctuation,
			PcreByteScalarClassKind.UnicodeSpace => category is
				UnicodeCategory.SpaceSeparator or UnicodeCategory.LineSeparator
				or UnicodeCategory.ParagraphSeparator,
			_ => throw new InvalidOperationException( "Compiler produced an unknown PCRE scalar class." )
		};
	}

	private sealed record Frame(
		int Pc, int Position, PcreByteCapture?[] Captures, int[] CaptureStarts, int[] Marks
	);

	private sealed class MatchLimitException( PcreByteDiagnosticCode code, string message ) : Exception( message ) {
		internal PcreByteDiagnosticCode Code { get; } = code;
	}
}
