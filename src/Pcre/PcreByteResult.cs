namespace Icod.Expressions.Pcre;

internal enum PcreByteDiagnosticCode {
	UnsupportedSyntax,
	InvalidSyntax,
	PatternDepthLimitExceeded,
	PatternCaptureLimitExceeded,
	PatternRepeatLimitExceeded,
	InvalidWorkLimit,
	InvalidStartOffset,
	InvalidUtf8Pattern,
	InvalidUtf8Subject,
	MatchWorkLimitExceeded,
	MatchDepthLimitExceeded,
	PatternLengthLimitExceeded
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

internal sealed record PcreByteMatch( int ByteIndex, int ByteLength, IReadOnlyList<PcreByteCapture?> Captures, PcreCaptureInfo CaptureInfo ) {
	internal ReadOnlyMemory<byte> MarkName { get; init; }
	public PcreByteCapture? GetCapture( string name ) {
		var numbers = CaptureInfo.GetLookupGroupNumbers( name );
		for ( var i = 0; i < numbers.Count; i++ ) {
			if ( Captures[numbers[i]] is { } capture ) { return capture; }
		}
		return null;
	}
}

internal sealed record PcreByteMatchResult(
	PcreByteMatch? Match,
	PcreByteDiagnostic? Diagnostic
) {
	public bool IsSuccess => Diagnostic is null;
}
