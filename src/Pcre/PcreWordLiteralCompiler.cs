namespace Icod.Expressions.Pcre;

internal sealed record PcreWordOptions {
	public bool Utf16 { get; init; }
	public bool Ucp { get; init; }
	public bool Caseless { get; init; }
	public bool CaselessRestricted { get; init; }
	public bool AsciiBsD { get; init; }
	public bool AsciiBsS { get; init; }
	public bool AsciiBsW { get; init; }
	public bool AsciiPosix { get; init; }
	public bool AsciiDigit { get; init; }
	public PcreMatchPolicy Policy { get; init; }
	public int MatchWorkLimit { get; init; } = 1_000_000;
	public uint MaxPatternLength { get; init; } = 4096;
	public Action<int>? WorkObserver { get; init; }
}

internal enum PcreWordDiagnosticCode {
	InvalidWorkLimit, PatternLengthLimitExceeded, InvalidUtf16Pattern,
	InvalidUtf16Subject, InvalidStartOffset, MatchWorkLimitExceeded, InvalidSyntax, UnsupportedSyntax,
	MatchDepthLimitExceeded
}

internal sealed record PcreWordDiagnostic( PcreWordDiagnosticCode Code, int CodeUnitOffset );
internal readonly record struct PcreWordCapture( int CodeUnitIndex, int CodeUnitLength );
internal sealed record PcreWordMatch( int CodeUnitIndex, int CodeUnitLength,
	IReadOnlyList<PcreWordCapture?> Captures, PcreCaptureInfo CaptureInfo ) {
	internal string MarkName { get; init; } = String.Empty;
	public PcreWordCapture? GetCapture( string name ) {
		var numbers = CaptureInfo.GetLookupGroupNumbers( name );
		for ( var index = 0; index < numbers.Count; index++ ) {
			if ( Captures[numbers[index]] is { } capture ) { return capture; }
		}
		return null;
	}
}
internal sealed record PcreWordMatchResult( PcreWordMatch? Match, PcreWordDiagnostic? Diagnostic ) {
	public bool IsSuccess => Diagnostic is null;
}
internal sealed record PcreWordCompileResult( PcreCompiledWordPattern? Pattern, PcreWordDiagnostic? Diagnostic ) {
	public bool IsSuccess => Pattern is not null && Diagnostic is null;
}

/// <summary>Compiles an explicitly literal width-16 pattern using shared literal semantics.</summary>
internal static class PcreWordLiteralCompiler {
	private const int MaximumPatternUnits = 4096;
	internal static PcreWordCompileResult CompileLiteral(
		ReadOnlyMemory<char> pattern, PcreWordOptions? options = null, CancellationToken cancellationToken = default
	) {
		cancellationToken.ThrowIfCancellationRequested();
		options ??= new();
		if ( options.MatchWorkLimit <= 0 ) { return new( null, new( PcreWordDiagnosticCode.InvalidWorkLimit, 0 ) ); }
		if ( pattern.Length > MaximumPatternUnits || (uint)pattern.Length > options.MaxPatternLength ) {
			return new( null, new( PcreWordDiagnosticCode.PatternLengthLimitExceeded, 0 ) );
		}
		var source = pattern.ToArray();
		var literals = new List<PcreByteLiteral>();
		for ( var offset = 0; offset < source.Length; ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( !PcreUtf16.TryRead( source, offset, options.Utf16, out var value, out var width ) ) {
				return new( null, new( PcreWordDiagnosticCode.InvalidUtf16Pattern, offset ) );
			}
			literals.Add( new( value ) );
			offset += width;
		}
		cancellationToken.ThrowIfCancellationRequested();
		return new( new( literals.ToArray(), options ), null );
	}
}
