namespace Icod.Expressions.Pcre;

/// <summary>Provisional internal limits for one compiled byte pattern.</summary>
internal sealed record PcreByteOptions {
	internal const int DefaultMatchWorkLimit = 1_000_000;

	public int MatchWorkLimit { get; init; } = DefaultMatchWorkLimit;
	public bool Caseless { get; init; }
	public bool Utf8 { get; init; }
	public bool MatchInvalidUtf { get; init; }
	public bool Ucp { get; init; }
	/// <summary>Enable PCRE2 UTS#18 nested brackets and doubled set operators.</summary>
	public bool AltExtendedClass { get; init; }
	public bool AsciiBsD { get; init; }
	/// <summary>Use Python's three-digit octal rule instead of preceding-capture disambiguation.</summary>
	public bool PythonOctal { get; init; }
	/// <summary>Reject a bare backslash-zero escape while retaining multi-digit and braced octals.</summary>
	public bool NoBackslashZero { get; init; }
	/// <summary>Interpret the backslash-r escape as LF while retaining other CR spellings.</summary>
	public bool EscapedCrIsLf { get; init; }
	/// <summary>Recognize CR, LF, and CRLF as newlines, as in Grep's pinned PCRE2 build.</summary>
	public bool AnyCrLfNewline { get; init; }
	/// <summary>Restrict backslash-R sequences to CR, LF, and CRLF; independent of the newline convention.</summary>
	public bool BsrAnyCrLf { get; init; }

	/// <summary>Test-only progress hook used to cancel a running match deterministically.</summary>
	internal Action<int>? WorkObserver { get; init; }
}
