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
									&& ( program.Caseless ? PcreUtf8CaseFold.Equals( scalar, instruction.First )
										: scalar == instruction.First )
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
								&& ScalarClassContains( (PcreByteScalarClassKind)instruction.First, propertyScalar,
									instruction.Category, program.Utf8 && program.Caseless )
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
									if ( ScalarClassContains( predicate.Kind, setScalar, predicate.Category ) != predicate.Negated ) {
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
							if ( program.Utf8 && program.Caseless ) {
								var capturedOffset = captured.ByteIndex;
								var capturedEnd = captured.ByteIndex + captured.ByteLength;
								var candidate = position;
								while ( capturedOffset < capturedEnd ) {
									Step(); // Attempted captured-scalar comparison.
									if ( candidate >= fragment.End
										|| !PcreUtf8.TryRead( input.Span, capturedOffset, out var captureScalar, out var captureWidth )
										|| capturedOffset + captureWidth > capturedEnd
										|| !PcreUtf8.TryRead( input.Span, candidate, out var referenceScalar, out var referenceWidth )
										|| candidate + referenceWidth > fragment.End
										|| !PcreUtf8CaseFold.Equals( captureScalar, referenceScalar ) ) {
										failed = true;
										break;
									}
									capturedOffset += captureWidth;
									candidate += referenceWidth;
								}
								if ( !failed ) { position = candidate; pc++; }
								break;
							}
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
							if ( program.Utf8 && program.Caseless ) {
								var scalarEquals = true;
								var scalarAdjacent = position;
								if ( instruction.Second == 1 ) {
									for ( var patternOffset = 0; patternOffset < literal.Length; ) {
										Step(); // Count an asserted scalar.
										PcreUtf8.TryRead( literal, patternOffset, out _, out var patternWidth );
										patternOffset += patternWidth;
										if ( scalarAdjacent <= fragment.Start ) { scalarEquals = false; break; }
										do {
											scalarAdjacent--;
											Step(); // Find the previous scalar boundary within this fragment.
										} while ( scalarAdjacent > fragment.Start
											&& ( input.Span[scalarAdjacent] & 0xC0 ) == 0x80 );
									}
								}
								if ( scalarEquals ) {
									var subjectOffset = scalarAdjacent;
									for ( var patternOffset = 0; patternOffset < literal.Length; ) {
										Step(); // Attempted assertion-scalar comparison.
										if ( subjectOffset >= fragment.End
											|| !PcreUtf8.TryRead( literal, patternOffset, out var assertionScalar, out var assertionWidth )
											|| !PcreUtf8.TryRead( input.Span, subjectOffset, out var subjectScalar, out var subjectWidth )
											|| subjectOffset + subjectWidth > fragment.End
											|| !PcreUtf8CaseFold.Equals( assertionScalar, subjectScalar ) ) {
											scalarEquals = false;
											break;
										}
										patternOffset += assertionWidth;
										subjectOffset += subjectWidth;
									}
									if ( instruction.Second == 1 && subjectOffset != position ) { scalarEquals = false; }
								}
								if ( scalarEquals == ( instruction.First == 1 ) ) { pc++; }
								else { failed = true; }
								break;
							}
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

	private static bool ScalarClassContains( PcreByteScalarClassKind kind, int scalar,
		UnicodeCategory? specificCategory = null, bool caseless = false ) {
		if ( kind == PcreByteScalarClassKind.AsciiDigit ) { return scalar is >= '0' and <= '9'; }
		if ( kind == PcreByteScalarClassKind.Ascii ) { return scalar <= 0x7F; }
		if ( kind is PcreByteScalarClassKind.AsciiAlnum or PcreByteScalarClassKind.AsciiAlpha
			or PcreByteScalarClassKind.AsciiBlank or PcreByteScalarClassKind.AsciiControl
			or PcreByteScalarClassKind.AsciiGraph or PcreByteScalarClassKind.AsciiLower
			or PcreByteScalarClassKind.AsciiPrintable or PcreByteScalarClassKind.AsciiPunctuation
			or PcreByteScalarClassKind.AsciiUpper or PcreByteScalarClassKind.AsciiHexDigit ) {
			if ( scalar > 0x7F ) { return false; }
			var value = (byte)scalar;
			return kind switch {
				PcreByteScalarClassKind.AsciiAlnum => PcreByteSet.Alnum.Contains( value ),
				PcreByteScalarClassKind.AsciiAlpha => PcreByteSet.Alpha.Contains( value ),
				PcreByteScalarClassKind.AsciiBlank => PcreByteSet.Blank.Contains( value ),
				PcreByteScalarClassKind.AsciiControl => PcreByteSet.Control.Contains( value ),
				PcreByteScalarClassKind.AsciiGraph => PcreByteSet.Graph.Contains( value ),
				PcreByteScalarClassKind.AsciiLower => PcreByteSet.Lower.Contains( value ),
				PcreByteScalarClassKind.AsciiPrintable => PcreByteSet.Printable.Contains( value ),
				PcreByteScalarClassKind.AsciiPunctuation => PcreByteSet.Punctuation.Contains( value ),
				PcreByteScalarClassKind.AsciiUpper => PcreByteSet.Upper.Contains( value ),
				PcreByteScalarClassKind.AsciiHexDigit => PcreByteSet.HexDigit.Contains( value ),
				_ => throw new InvalidOperationException( "Compiler produced an unknown PCRE ASCII class." )
			};
		}
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
		if ( kind == PcreByteScalarClassKind.UnicodeBlank ) {
			return scalar is 0x09 or 0x20 or 0xA0 or 0x1680 or 0x180E
				or >= 0x2000 and <= 0x200A or 0x202F or 0x205F or 0x3000;
		}
		if ( kind == PcreByteScalarClassKind.UnicodeHexDigit ) {
			return scalar is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f'
				or >= 0xFF10 and <= 0xFF19 or >= 0xFF21 and <= 0xFF26 or >= 0xFF41 and <= 0xFF46;
		}
		var category = Rune.GetUnicodeCategory( new Rune( scalar ) );
		return kind switch {
			PcreByteScalarClassKind.SpecificCategory => specificCategory.HasValue
				? caseless && IsCasedCategory( specificCategory.Value )
						? IsCasedCategory( category )
						: category == specificCategory.Value
				: throw new InvalidOperationException( "Compiler omitted a specific Unicode category." ),
			PcreByteScalarClassKind.CategoryOther => category is
				UnicodeCategory.Control or UnicodeCategory.Format or UnicodeCategory.OtherNotAssigned
				or UnicodeCategory.PrivateUse or UnicodeCategory.Surrogate,
			PcreByteScalarClassKind.CasedLetter => category is
				UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
				or UnicodeCategory.TitlecaseLetter,
			PcreByteScalarClassKind.CategoryMark => category is
				UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark
				or UnicodeCategory.EnclosingMark,
			PcreByteScalarClassKind.CategoryNumber => category is
				UnicodeCategory.DecimalDigitNumber or UnicodeCategory.LetterNumber
				or UnicodeCategory.OtherNumber,
			PcreByteScalarClassKind.CategoryPunctuation => category is
				UnicodeCategory.ConnectorPunctuation or UnicodeCategory.DashPunctuation
				or UnicodeCategory.OpenPunctuation or UnicodeCategory.ClosePunctuation
				or UnicodeCategory.InitialQuotePunctuation or UnicodeCategory.FinalQuotePunctuation
				or UnicodeCategory.OtherPunctuation,
			PcreByteScalarClassKind.CategorySymbol => category is
				UnicodeCategory.MathSymbol or UnicodeCategory.CurrencySymbol
				or UnicodeCategory.ModifierSymbol or UnicodeCategory.OtherSymbol,
			PcreByteScalarClassKind.CategorySeparator => category is
				UnicodeCategory.SpaceSeparator or UnicodeCategory.LineSeparator
				or UnicodeCategory.ParagraphSeparator,
			PcreByteScalarClassKind.DecimalDigit => category == UnicodeCategory.DecimalDigitNumber,
			PcreByteScalarClassKind.UnicodeAlnum => category is
				UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
				or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter
				or UnicodeCategory.OtherLetter or UnicodeCategory.DecimalDigitNumber
				or UnicodeCategory.LetterNumber or UnicodeCategory.OtherNumber,
			PcreByteScalarClassKind.UnicodeControl => category == UnicodeCategory.Control,
			PcreByteScalarClassKind.UnicodeGraph => UnicodeGraphContains( category, scalar ),
			PcreByteScalarClassKind.UnicodeLower => category == UnicodeCategory.LowercaseLetter,
			PcreByteScalarClassKind.UnicodePrintable => category == UnicodeCategory.SpaceSeparator
				|| UnicodeGraphContains( category, scalar ),
			PcreByteScalarClassKind.UnicodePunctuation => category is
				UnicodeCategory.ConnectorPunctuation or UnicodeCategory.DashPunctuation
				or UnicodeCategory.OpenPunctuation or UnicodeCategory.ClosePunctuation
				or UnicodeCategory.InitialQuotePunctuation or UnicodeCategory.FinalQuotePunctuation
				or UnicodeCategory.OtherPunctuation || ( scalar < 0x100 && category is
				UnicodeCategory.MathSymbol or UnicodeCategory.CurrencySymbol
				or UnicodeCategory.ModifierSymbol or UnicodeCategory.OtherSymbol ),
			PcreByteScalarClassKind.UnicodeUpper => category == UnicodeCategory.UppercaseLetter,
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

	private static bool IsCasedCategory( UnicodeCategory category ) => category is
		UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or UnicodeCategory.TitlecaseLetter;

	private static bool UnicodeGraphContains( UnicodeCategory category, int scalar ) {
		if ( scalar is 0x061C or 0x180E or >= 0x2066 and <= 0x2069 ) { return false; }
		return category is
			UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter
			or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter
			or UnicodeCategory.OtherLetter or UnicodeCategory.DecimalDigitNumber
			or UnicodeCategory.LetterNumber or UnicodeCategory.OtherNumber
			or UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark
			or UnicodeCategory.EnclosingMark or UnicodeCategory.ConnectorPunctuation
			or UnicodeCategory.DashPunctuation or UnicodeCategory.OpenPunctuation
			or UnicodeCategory.ClosePunctuation or UnicodeCategory.InitialQuotePunctuation
			or UnicodeCategory.FinalQuotePunctuation or UnicodeCategory.OtherPunctuation
			or UnicodeCategory.MathSymbol or UnicodeCategory.CurrencySymbol
			or UnicodeCategory.ModifierSymbol or UnicodeCategory.OtherSymbol
			or UnicodeCategory.Format;
	}

	private sealed record Frame(
		int Pc, int Position, PcreByteCapture?[] Captures, int[] CaptureStarts, int[] Marks
	);

	private sealed class MatchLimitException( PcreByteDiagnosticCode code, string message ) : Exception( message ) {
		internal PcreByteDiagnosticCode Code { get; } = code;
	}
}
