namespace Icod.Expressions.Pcre;

internal sealed record PcreWordSuffixChoices( PcreByteNode[][] Alternatives ) : PcreByteNode;

/// <summary>Admits a bounded width-16 sequence through the existing PCRE grammar.</summary>
internal static class PcreWordSequenceCompiler {
	private const int MaximumExpandedNodes = 4096;
	private const int MaximumExactRepeat = 64;

	internal static PcreWordCompileResult CompileSequence( ReadOnlyMemory<char> pattern, PcreWordOptions? options = null,
		CancellationToken cancellationToken = default ) {
		cancellationToken.ThrowIfCancellationRequested();
		options ??= new();
		if ( options.MatchWorkLimit <= 0 ) { return Error( PcreWordDiagnosticCode.InvalidWorkLimit, 0 ); }
		if ( pattern.Length > 4096 || (uint)pattern.Length > options.MaxPatternLength ) {
			return Error( PcreWordDiagnosticCode.PatternLengthLimitExceeded, 0 );
		}
		var source = pattern.ToArray();
		if ( options.Utf16 ) {
			for ( var offset = 0; offset < source.Length; ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( !PcreUtf16.TryRead( source, offset, true, out _, out var width ) ) {
					return Error( PcreWordDiagnosticCode.InvalidUtf16Pattern, offset );
				}
				offset += width;
			}
		}
		// ASCII grammar tokens keep their original indices. Non-ASCII units are
		// placeholders read directly from the owned word source by the parser.
		var grammarSource = new byte[source.Length];
		for ( var index = 0; index < source.Length; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			grammarSource[index] = source[index] > 127 ? (byte)0x80 : (byte)source[index];
		}
		var grammarOptions = new PcreByteOptions {
			Utf8 = options.Utf16, Ucp = options.Ucp, Caseless = options.Caseless,
			CaselessRestricted = options.CaselessRestricted,
			AsciiBsD = options.AsciiBsD, AsciiBsS = options.AsciiBsS, AsciiBsW = options.AsciiBsW,
			AsciiPosix = options.AsciiPosix, AsciiDigit = options.AsciiDigit
		};
		var settings = PcreByteStartSettings.Parse( grammarSource, grammarOptions, cancellationToken );
		if ( settings.BodyOffset != 0 ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
		try {
			var parser = new PcreByteParser( grammarSource, grammarOptions, settings, cancellationToken,
				codeUnitWidth: 16, wordSource: source );
			var root = parser.Parse();
			if ( parser.GroupCount != 0 || root.Branches.Count is < 1 or > 64 ) {
				return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
			}
			var branches = new List<PcreByteNode[]>();
			var admittedPaths = 0;
			for ( var branch = 0; branch < root.Branches.Count; branch++ ) {
				var nodes = root.Branches[branch];
				var prefixVariables = new List<(int Index, PcreByteRepeat Repeat)>();
				var suffixVariables = new List<(int Index, PcreByteRepeat Repeat)>();
				var tailVariables = new List<(int Index, PcreByteRepeat Repeat)>();
				var unboundedIndex = -1;
				var secondUnboundedIndex = -1;
				var unboundedCount = 0;
				var prefixChoices = 1;
				var suffixChoices = 1;
				var tailChoices = 1;
				for ( var index = 0; index < nodes.Count; index++ ) {
					cancellationToken.ThrowIfCancellationRequested();
					if ( nodes[index] is not PcreByteRepeat repeat || repeat.Minimum == repeat.Maximum ) { continue; }
					if ( repeat.Maximum == int.MaxValue ) {
						if ( unboundedCount == 3 || repeat.Minimum is < 0 or > MaximumExactRepeat
							|| !IsSupportedAtom( repeat.Child ) ) {
							return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
						}
						if ( unboundedCount == 0 ) { unboundedIndex = index; }
						else if ( unboundedCount == 1 ) { secondUnboundedIndex = index; }
						unboundedCount++;
						continue;
					}
					if ( repeat.Minimum < 0 || repeat.Maximum > MaximumExactRepeat
						|| !IsSupportedAtom( repeat.Child ) ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
					var range = repeat.Maximum - repeat.Minimum + 1;
					if ( unboundedCount == 2 ) {
						if ( range > 64 / tailChoices ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
						tailChoices *= range;
						tailVariables.Add( (index, repeat) );
					} else if ( unboundedIndex >= 0 ) {
						if ( range > 64 / suffixChoices ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
						suffixChoices *= range;
						suffixVariables.Add( (index, repeat) );
					} else {
						if ( range > 64 / prefixChoices ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
						prefixChoices *= range;
						prefixVariables.Add( (index, repeat) );
					}
				}
				if ( unboundedCount == 3 && ( suffixVariables.Count != 0 || tailVariables.Count != 0 )
					|| prefixChoices > ( 64 - admittedPaths ) / ( suffixChoices * tailChoices ) ) {
					return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
				}
				admittedPaths += prefixChoices * suffixChoices * tailChoices;
				var counts = new int[nodes.Count];
				Array.Fill( counts, -1 );
				for ( var prefixChoice = 0; prefixChoice < prefixChoices; prefixChoice++ ) {
					SelectCounts( prefixVariables, prefixChoice, counts );
					if ( unboundedCount == 2 && ( suffixChoices > 1 || tailChoices > 1 ) ) {
						var middleAlternatives = new PcreByteNode[suffixChoices][];
						PcreByteNode[]? firstPrefix = null;
						for ( var middleChoice = 0; middleChoice < suffixChoices; middleChoice++ ) {
							SelectCounts( suffixVariables, middleChoice, counts );
							var tails = new PcreByteNode[tailChoices][];
							PcreByteNode[]? middleWithSecond = null;
							for ( var tailChoice = 0; tailChoice < tailChoices; tailChoice++ ) {
								SelectCounts( tailVariables, tailChoice, counts );
								if ( !TryExpandBranch( nodes, counts, cancellationToken, out var expanded ) ) {
									return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
								}
								var first = Array.FindIndex( expanded, node => ReferenceEquals( node, nodes[unboundedIndex] ) );
								var second = Array.FindIndex( expanded, node => ReferenceEquals( node, nodes[secondUnboundedIndex] ) );
								if ( first < 0 || second <= first ) {
									throw new InvalidOperationException( "Missing ordered word repeat nodes." );
								}
								firstPrefix ??= expanded[..( first + 1 )];
								middleWithSecond ??= expanded[( first + 1 )..( second + 1 )];
								tails[tailChoice] = expanded[( second + 1 )..];
							}
							if ( tailChoices > 1 ) {
								middleAlternatives[middleChoice] = [.. middleWithSecond!, new PcreWordSuffixChoices( tails )];
							} else {
								middleAlternatives[middleChoice] = [.. middleWithSecond!, .. tails[0]];
							}
						}
						if ( suffixChoices > 1 ) {
							branches.Add( [.. firstPrefix!, new PcreWordSuffixChoices( middleAlternatives )] );
						} else {
							branches.Add( [.. firstPrefix!, .. middleAlternatives[0]] );
						}
						continue;
					}
					if ( suffixChoices == 1 ) {
						if ( !TryExpandBranch( nodes, counts, cancellationToken, out var expanded ) ) {
							return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
						}
						branches.Add( expanded );
						continue;
					}
					var suffixes = new PcreByteNode[suffixChoices][];
					PcreByteNode[]? prefix = null;
					for ( var suffixChoice = 0; suffixChoice < suffixChoices; suffixChoice++ ) {
						SelectCounts( suffixVariables, suffixChoice, counts );
						if ( !TryExpandBranch( nodes, counts, cancellationToken, out var expanded ) ) {
							return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
						}
						var repeatIndex = Array.FindIndex( expanded, node => ReferenceEquals( node, nodes[unboundedIndex] ) );
						if ( repeatIndex < 0 ) { throw new InvalidOperationException( "Missing word repeat node." ); }
						prefix ??= expanded[..( repeatIndex + 1 )];
						suffixes[suffixChoice] = expanded[( repeatIndex + 1 )..];
					}
					branches.Add( [.. prefix!, new PcreWordSuffixChoices( suffixes )] );
				}
			}
			return new( new( branches.ToArray(), options ), null );
		} catch ( PcreByteParseException error ) {
			return Error( error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax
				? PcreWordDiagnosticCode.UnsupportedSyntax : PcreWordDiagnosticCode.InvalidSyntax, error.Diagnostic.ByteOffset );
		}
	}

	private static void SelectCounts( IReadOnlyList<(int Index, PcreByteRepeat Repeat)> variables,
		int choice, int[] counts ) {
		for ( var index = variables.Count - 1; index >= 0; index-- ) {
			var (nodeIndex, repeat) = variables[index];
			var range = repeat.Maximum - repeat.Minimum + 1;
			var digit = choice % range;
			choice /= range;
			counts[nodeIndex] = repeat.Greedy ? repeat.Maximum - digit : repeat.Minimum + digit;
		}
	}

	private static bool TryExpandBranch( IReadOnlyList<PcreByteNode> nodes, int[] counts,
		CancellationToken cancellationToken, out PcreByteNode[] expanded ) {
		var output = new List<PcreByteNode>();
		for ( var index = 0; index < nodes.Count; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			var node = nodes[index];
			if ( node is PcreByteRepeat repeat ) {
				if ( repeat.Maximum == int.MaxValue
					&& repeat.Minimum is >= 0 and <= MaximumExactRepeat && IsSupportedAtom( repeat.Child )
					&& output.Count < MaximumExpandedNodes ) {
					output.Add( repeat );
					continue;
				}
				var count = counts[index] >= 0 ? counts[index] : repeat.Maximum;
				if ( counts[index] < 0 && repeat.Minimum != repeat.Maximum
					|| count is < 0 or > MaximumExactRepeat || !IsSupportedAtom( repeat.Child )
					|| output.Count > MaximumExpandedNodes - count ) {
					expanded = []; return false;
				}
				for ( var copy = 0; copy < count; copy++ ) {
					cancellationToken.ThrowIfCancellationRequested();
					output.Add( repeat.Child );
				}
			} else if ( IsSupportedNode( node ) && output.Count < MaximumExpandedNodes ) {
				output.Add( node );
			} else { expanded = []; return false; }
		}
		expanded = output.ToArray();
		return true;
	}

	private static bool IsSupportedNode( PcreByteNode node ) => IsSupportedAtom( node )
		|| node is PcreByteAnchor { IsLineBoundary: false, IsDisabled: false } or PcreByteSearchStart;

	private static bool IsSupportedAtom( PcreByteNode node ) => node is PcreByteLiteral or PcreByteAny
		|| node is PcreByteScalarClass property && IsWordClassPredicate( property.Kind )
		|| node is PcreByteScalarSet set && IsSupportedSet( set );

	private static bool IsSupportedSet( PcreByteScalarSet set ) {
		if ( set.SourceMemberCount > 64 || set.Ranges.Length + set.Predicates.Length is < 1 or > 64 ) { return false; }
		foreach ( var predicate in set.Predicates.Span ) {
			if ( !IsWordClassPredicate( predicate.Kind ) ) { return false; }
		}
		return true;
	}

	private static bool IsWordClassPredicate( PcreByteScalarClassKind kind ) => kind is
		PcreByteScalarClassKind.AsciiDigit or PcreByteScalarClassKind.AsciiWord or PcreByteScalarClassKind.AsciiSpace
		or PcreByteScalarClassKind.Ascii or PcreByteScalarClassKind.AsciiAlnum or PcreByteScalarClassKind.AsciiAlpha
		or PcreByteScalarClassKind.AsciiBlank or PcreByteScalarClassKind.AsciiControl
		or PcreByteScalarClassKind.AsciiGraph or PcreByteScalarClassKind.AsciiLower
		or PcreByteScalarClassKind.AsciiPrintable or PcreByteScalarClassKind.AsciiPunctuation
		or PcreByteScalarClassKind.AsciiUpper or PcreByteScalarClassKind.AsciiHexDigit
		or PcreByteScalarClassKind.HorizontalSpace or PcreByteScalarClassKind.VerticalSpace
		or PcreByteScalarClassKind.Letter or PcreByteScalarClassKind.CasedLetter
		or PcreByteScalarClassKind.CategoryOther or PcreByteScalarClassKind.CategoryMark
		or PcreByteScalarClassKind.CategoryNumber or PcreByteScalarClassKind.CategoryPunctuation
		or PcreByteScalarClassKind.CategorySymbol or PcreByteScalarClassKind.CategorySeparator
		or PcreByteScalarClassKind.SpecificCategory
		or PcreByteScalarClassKind.Script or PcreByteScalarClassKind.ScriptExtensions
		or PcreByteScalarClassKind.BinaryProperty or PcreByteScalarClassKind.BidiClass
		or PcreByteScalarClassKind.DecimalDigit or PcreByteScalarClassKind.UnicodeWord
		or PcreByteScalarClassKind.UnicodeSpace or PcreByteScalarClassKind.UnicodeAlnum
		or PcreByteScalarClassKind.UnicodeBlank or PcreByteScalarClassKind.UnicodeControl
		or PcreByteScalarClassKind.UnicodeGraph or PcreByteScalarClassKind.UnicodeLower
		or PcreByteScalarClassKind.UnicodePrintable or PcreByteScalarClassKind.UnicodePunctuation
		or PcreByteScalarClassKind.UnicodeUpper or PcreByteScalarClassKind.UnicodeHexDigit;

	private static PcreWordCompileResult Error( PcreWordDiagnosticCode code, int offset ) => new( null, new( code, offset ) );
}
