namespace Icod.Expressions.Pcre;

internal enum PcreByteDiagnosticCode {
	UnsupportedSyntax,
	InvalidSyntax,
	PatternDepthLimitExceeded,
	InvalidWorkLimit,
	InvalidStartOffset,
	MatchWorkLimitExceeded,
	MatchDepthLimitExceeded
}

internal sealed record PcreByteDiagnostic(
	PcreByteDiagnosticCode Code,
	int ByteOffset,
	string Message
);

internal sealed record PcreByteCompileResult(
	PcreCompiledBytePattern? Pattern,
	PcreByteDiagnostic? Diagnostic
) {
	public bool IsSuccess => Diagnostic is null && Pattern is not null;
}

internal readonly record struct PcreByteCapture( int ByteIndex, int ByteLength );

internal sealed record PcreByteMatch( int ByteIndex, int ByteLength, IReadOnlyList<PcreByteCapture?> Captures );

internal sealed record PcreByteMatchResult(
	PcreByteMatch? Match,
	PcreByteDiagnostic? Diagnostic
) {
	public bool IsSuccess => Diagnostic is null;
}
