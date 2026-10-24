namespace Icod.Expressions.Pcre;

/// <summary>Immutable internal syntax tree and per-match resource policy.</summary>
internal sealed class PcreCompiledBytePattern {
	private readonly PcreByteAlternatives root;
	private readonly int groupCount;
	private readonly int matchWorkLimit;
	private readonly Action<int>? workObserver;

	internal PcreCompiledBytePattern(
		PcreByteAlternatives root, int groupCount, int matchWorkLimit, Action<int>? workObserver
	) {
		this.root = root;
		this.groupCount = groupCount;
		this.matchWorkLimit = matchWorkLimit;
		this.workObserver = workObserver;
	}

	public PcreByteMatchResult Match(
		ReadOnlyMemory<byte> input,
		int startOffset = 0,
		CancellationToken cancellationToken = default
	) => PcreByteMatcher.Match( root, groupCount, input, startOffset, matchWorkLimit, workObserver, cancellationToken );
}
