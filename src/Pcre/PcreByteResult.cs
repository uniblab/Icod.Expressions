namespace Icod.Expressions.Pcre;

internal enum PcreByteDiagnosticCode {
	UnsupportedSyntax,
	InvalidWorkLimit,
	InvalidStartOffset,
	MatchWorkLimitExceeded
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

internal sealed record PcreByteMatch( int ByteIndex, int ByteLength );

internal sealed record PcreByteMatchResult(
	PcreByteMatch? Match,
	PcreByteDiagnostic? Diagnostic
) {
	public bool IsSuccess => Diagnostic is null;
}
