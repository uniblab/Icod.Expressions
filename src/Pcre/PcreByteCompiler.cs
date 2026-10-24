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

		var parser = new PcreByteParser( pattern.ToArray(), cancellationToken );
		try {
			var root = parser.Parse();
			return new(
				new PcreCompiledBytePattern( root, parser.GroupCount, options.MatchWorkLimit, options.WorkObserver ),
				null
			);
		} catch ( PcreByteParseException error ) {
			return new( null, error.Diagnostic );
		}
	}
}
