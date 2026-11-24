namespace Icod.Expressions.Pcre;

/// <summary>Admits a bounded width-16 sequence through the existing PCRE grammar.</summary>
internal static class PcreWordSequenceCompiler {
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
			if ( parser.GroupCount != 0 || root.Branches.Count != 1 ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
			var nodes = root.Branches[0];
			foreach ( var node in nodes ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( node is PcreByteLiteral or PcreByteAny
					or PcreByteAnchor { IsLineBoundary: false, IsDisabled: false } or PcreByteSearchStart
					|| node is PcreByteScalarClass property && IsWordClassPredicate( property.Kind )
					|| node is PcreByteScalarSet set && IsSupportedSet( set ) ) { continue; }
				return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
			}
			return new( new( nodes.ToArray(), options ), null );
		} catch ( PcreByteParseException error ) {
			return Error( error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax
				? PcreWordDiagnosticCode.UnsupportedSyntax : PcreWordDiagnosticCode.InvalidSyntax, error.Diagnostic.ByteOffset );
		}
	}

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
