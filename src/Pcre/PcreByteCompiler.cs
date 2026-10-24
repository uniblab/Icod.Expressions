namespace Icod.Expressions.Pcre;

/// <summary>Compiles the first internal byte-mode grammar; unsupported PCRE syntax fails explicitly.</summary>
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

		var alternatives = new List<byte[]>();
		var segmentStart = 0;
		var source = pattern.Span;
		for ( var index = 0; index < source.Length; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			var current = source[index];
			if ( current == (byte)'|' ) {
				alternatives.Add( pattern.Slice( segmentStart, index - segmentStart ).ToArray() );
				segmentStart = index + 1;
				continue;
			}
			if ( IsPendingSyntax( current ) ) {
				return new( null, new(
					PcreByteDiagnosticCode.UnsupportedSyntax,
					index,
					$"PCRE syntax at byte {index} is not implemented by the internal byte foundation"
				) );
			}
		}
		alternatives.Add( pattern.Slice( segmentStart ).ToArray() );
		return new(
			new PcreCompiledBytePattern( alternatives.ToArray(), options.MatchWorkLimit, options.WorkObserver ),
			null
		);
	}

	private static bool IsPendingSyntax( byte value ) => value is
		(byte)'\\' or (byte)'.' or (byte)'^' or (byte)'$'
		or (byte)'*' or (byte)'+' or (byte)'?' or (byte)'{'
		or (byte)'}' or (byte)'[' or (byte)']' or (byte)'('
		or (byte)')';
}
