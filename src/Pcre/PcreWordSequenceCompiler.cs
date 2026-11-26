namespace Icod.Expressions.Pcre;

internal sealed record PcreWordSuffixChoices( PcreByteNode[][] Alternatives ) : PcreByteNode;

/// <summary>Admits a bounded width-16 sequence through the existing PCRE grammar.</summary>
internal static class PcreWordSequenceCompiler {
	private const int MaximumExpandedNodes = 4096;
	private const int MaximumExactRepeat = 64;
	private const int MaximumUnboundedRepeats = 4;

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
		var settings = PcreByteStartSettings.Parse( grammarSource, grammarOptions, cancellationToken, newlineOnly: true );
		// Pair-aware conventions also alter native bumpalong and prefix proofs.
		if ( settings.Newline is not ( PcreByteNewline.Lf or PcreByteNewline.Cr or PcreByteNewline.Nul ) ) {
			return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
		}
		try {
			var parser = new PcreByteParser( grammarSource, grammarOptions, settings, cancellationToken,
				codeUnitWidth: 16, wordSource: source );
			var root = parser.Parse();
			if ( parser.GroupCount != 0 || root.Branches.Count is < 1 or > 64 ) {
				return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
			}
			var branches = new List<PcreByteNode[]>();
			var inspectPreviousScalar = false;
			var admittedPaths = 0;
			for ( var branch = 0; branch < root.Branches.Count; branch++ ) {
				var nodes = root.Branches[branch];
				var variables = new List<(int Index, PcreByteRepeat Repeat)>[MaximumUnboundedRepeats + 1];
				var products = new int[variables.Length];
				for ( var segment = 0; segment < variables.Length; segment++ ) {
					variables[segment] = new();
					products[segment] = 1;
				}
				var repeats = new List<PcreByteRepeat>();
				for ( var index = 0; index < nodes.Count; index++ ) {
					cancellationToken.ThrowIfCancellationRequested();
					inspectPreviousScalar |= nodes[index] is PcreByteWordBoundary
						or PcreByteAnchor { IsStart: true, IsLineBoundary: false };
					if ( nodes[index] is not PcreByteRepeat repeat || repeat.Minimum == repeat.Maximum ) { continue; }
					if ( repeat.Maximum == int.MaxValue ) {
						if ( repeats.Count == MaximumUnboundedRepeats || repeat.Minimum is < 0 or > MaximumExactRepeat
							|| !IsSupportedAtom( repeat.Child ) ) {
							return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
						}
						repeats.Add( repeat );
						continue;
					}
					if ( repeat.Minimum < 0 || repeat.Maximum > MaximumExactRepeat
						|| !IsSupportedAtom( repeat.Child ) ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
					var range = repeat.Maximum - repeat.Minimum + 1;
					var segment = repeats.Count;
					if ( range > 64 / products[segment] ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
					products[segment] *= range;
					variables[segment].Add( (index, repeat) );
				}
				var depth = repeats.Count;
				var pathCount = 1;
				for ( var segment = 0; segment <= depth; segment++ ) {
					if ( products[segment] > ( 64 - admittedPaths ) / pathCount ) {
						return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
					}
					pathCount *= products[segment];
				}
				admittedPaths += pathCount;
				var counts = new int[nodes.Count];
				Array.Fill( counts, -1 );
				var paths = new PcreByteNode[pathCount][];
				var positions = new int[pathCount][];
				for ( var ordinal = 0; ordinal < pathCount; ordinal++ ) {
					var remainder = ordinal;
					for ( var segment = depth; segment >= 0; segment-- ) {
						SelectCounts( variables[segment], remainder % products[segment], counts );
						remainder /= products[segment];
					}
					if ( !TryExpandBranch( nodes, counts, cancellationToken, out var expanded ) ) {
						return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
					}
					paths[ordinal] = expanded;
					positions[ordinal] = new int[depth];
					for ( var repeatIndex = 0; repeatIndex < depth; repeatIndex++ ) {
						var position = Array.FindIndex( expanded, node => ReferenceEquals( node, repeats[repeatIndex] ) );
						if ( position < ( repeatIndex == 0 ? 0 : positions[ordinal][repeatIndex - 1] + 1 ) ) {
							throw new InvalidOperationException( "Missing ordered word repeat nodes." );
						}
						positions[ordinal][repeatIndex] = position;
					}
				}
				if ( depth == 0 ) { branches.AddRange( paths ); continue; }
				var outerStride = pathCount / products[0];
				for ( var choice = 0; choice < products[0]; choice++ ) {
					var firstPath = choice * outerStride;
					var prefix = paths[firstPath][..( positions[firstPath][0] + 1 )];
					var continuation = BuildContinuation( paths, positions, products, depth,
						1, firstPath, outerStride, cancellationToken );
					branches.Add( [.. prefix, .. continuation] );
				}
			}
			return new( new( branches.ToArray(), options, inspectPreviousScalar, settings.Newline, settings.BsrAnyCrLf ), null );
		} catch ( PcreByteParseException error ) {
			return Error( error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax
				? PcreWordDiagnosticCode.UnsupportedSyntax : PcreWordDiagnosticCode.InvalidSyntax, error.Diagnostic.ByteOffset );
		}
	}

	private static PcreByteNode[] BuildContinuation( PcreByteNode[][] paths, int[][] positions, int[] products,
		int depth, int segment, int firstPath, int groupSize, CancellationToken cancellationToken ) {
		var alternatives = new PcreByteNode[products[segment]][];
		var stride = groupSize / products[segment];
		for ( var choice = 0; choice < products[segment]; choice++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			var path = firstPath + choice * stride;
			var firstNode = positions[path][segment - 1] + 1;
			if ( segment == depth ) {
				alternatives[choice] = paths[path][firstNode..];
			} else {
				var section = paths[path][firstNode..( positions[path][segment] + 1 )];
				var continuation = BuildContinuation( paths, positions, products, depth,
					segment + 1, path, stride, cancellationToken );
				alternatives[choice] = [.. section, .. continuation];
			}
		}
		if ( alternatives.Length > 1 ) { return [new PcreWordSuffixChoices( alternatives )]; }
		return alternatives[0];
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
		|| node is PcreByteAnchor or PcreByteLineAnchor or PcreByteEndBeforeFinalLf
			or PcreByteSearchStart or PcreByteWordBoundary;

	private static bool IsSupportedAtom( PcreByteNode node ) => node is PcreByteLiteral or PcreByteAny or PcreByteNewlineSequence
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
