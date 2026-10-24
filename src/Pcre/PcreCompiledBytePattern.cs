namespace Icod.Expressions.Pcre;

/// <summary>Immutable internal byte-pattern snapshot.</summary>
internal sealed class PcreCompiledBytePattern {
	private readonly byte[][] alternatives;
	private readonly int matchWorkLimit;
	private readonly Action<int>? workObserver;

	internal PcreCompiledBytePattern( byte[][] alternatives, int matchWorkLimit, Action<int>? workObserver ) {
		this.alternatives = alternatives;
		this.matchWorkLimit = matchWorkLimit;
		this.workObserver = workObserver;
	}

	public PcreByteMatchResult Match(
		ReadOnlyMemory<byte> input,
		int startOffset = 0,
		CancellationToken cancellationToken = default
	) => PcreByteMatcher.Match( alternatives, input, startOffset, matchWorkLimit, workObserver, cancellationToken );
}
