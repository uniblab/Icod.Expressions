namespace Icod.Expressions.Pcre;

/// <summary>Immutable internal syntax tree and per-match resource policy.</summary>
internal sealed class PcreCompiledBytePattern {
	private readonly PcreByteProgram program;
	private readonly int groupCount;
	private readonly int matchWorkLimit;
	private readonly Action<int>? workObserver;

	public PcreCaptureInfo CaptureInfo { get; }

	internal PcreCompiledBytePattern(
		PcreByteProgram program, int groupCount, int matchWorkLimit, Action<int>? workObserver, PcreCaptureInfo captureInfo
	) {
		this.program = program;
		this.groupCount = groupCount;
		this.matchWorkLimit = matchWorkLimit;
		this.workObserver = workObserver;
		CaptureInfo = captureInfo;
	}

	public PcreByteMatchResult Match(
		ReadOnlyMemory<byte> input,
		PcreMatchPolicy policy,
		int startOffset = 0,
		CancellationToken cancellationToken = default
	) => throw new NotImplementedException( "per-call match policy" );

	public PcreByteMatchResult Match(
		ReadOnlyMemory<byte> input,
		int startOffset = 0,
		CancellationToken cancellationToken = default
	) => PcreByteMatcher.Match( program, groupCount, CaptureInfo, input, startOffset, matchWorkLimit, workObserver, cancellationToken );
}
