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
		// The first shared-grammar binding admits ASCII source units only. Their
		// byte and word indices coincide; wide literal values use numeric escapes.
		var grammarSource = new byte[source.Length];
		for ( var index = 0; index < source.Length; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( source[index] > 127 ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, index ); }
			grammarSource[index] = (byte)source[index];
		}
		var grammarOptions = new PcreByteOptions { Utf8 = options.Utf16 };
		var settings = PcreByteStartSettings.Parse( grammarSource, grammarOptions, cancellationToken );
		if ( settings.BodyOffset != 0 ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
		try {
			var parser = new PcreByteParser( grammarSource, grammarOptions, settings, cancellationToken, codeUnitWidth: 16 );
			var root = parser.Parse();
			if ( parser.GroupCount != 0 || root.Branches.Count != 1 ) { return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 ); }
			var nodes = root.Branches[0];
			foreach ( var node in nodes ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( node is PcreByteLiteral { Caseless: false }
					or PcreByteAnchor { IsLineBoundary: false, IsDisabled: false } or PcreByteSearchStart ) { continue; }
				return Error( PcreWordDiagnosticCode.UnsupportedSyntax, 0 );
			}
			return new( new( nodes.ToArray(), options ), null );
		} catch ( PcreByteParseException error ) {
			return Error( error.Diagnostic.Code == PcreByteDiagnosticCode.UnsupportedSyntax
				? PcreWordDiagnosticCode.UnsupportedSyntax : PcreWordDiagnosticCode.InvalidSyntax, error.Diagnostic.ByteOffset );
		}
	}

	private static PcreWordCompileResult Error( PcreWordDiagnosticCode code, int offset ) => new( null, new( code, offset ) );
}
