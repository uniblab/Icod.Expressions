namespace Icod.Expressions.Pcre;

/// <summary>Compiles implemented byte-mode syntax without a runtime regex fallback.</summary>
internal static class PcreByteCompiler {
	public static PcreByteCompileResult Compile(
		ReadOnlyMemory<byte> pattern,
		PcreByteOptions? options = null,
		CancellationToken cancellationToken = default
	) {
		cancellationToken.ThrowIfCancellationRequested();
		options ??= new PcreByteOptions();
		if ( options.MatchWorkLimit <= 0 ) {
			return new( null, new(
				PcreByteDiagnosticCode.InvalidWorkLimit,
				0,
				"match work limit must be positive"
			) );
		}
		if ( options.Utf8 && options.Caseless ) {
			return new( null, new( PcreByteDiagnosticCode.UnsupportedSyntax, 0,
				"UTF-8 caseless matching is not implemented" ) );
		}
		var source = pattern.ToArray();
		if ( options.Utf8 ) {
			for ( var offset = 0; offset < source.Length; ) {
				cancellationToken.ThrowIfCancellationRequested();
				if ( !PcreUtf8.TryRead( source, offset, out _, out var width ) ) {
					return new( null, new( PcreByteDiagnosticCode.InvalidUtf8Pattern, offset,
						"pattern contains invalid UTF-8" ) );
				}
				offset += width;
			}
		}

		var parser = new PcreByteParser( source, options.Caseless, options.Utf8, cancellationToken );
		try {
			var root = parser.Parse();
			var program = new PcreByteProgramCompiler( cancellationToken, parser.Caseless, options.Utf8 ).Compile( root );
			return new(
				new PcreCompiledBytePattern( program, parser.GroupCount, options.MatchWorkLimit, options.WorkObserver ),
				null
			);
		} catch ( PcreByteParseException error ) {
			return new( null, error.Diagnostic );
		}
	}
}
