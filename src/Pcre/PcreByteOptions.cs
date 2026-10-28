namespace Icod.Expressions.Pcre;

/// <summary>Provisional internal limits for one compiled byte pattern.</summary>
internal sealed record PcreByteOptions {
	internal const int DefaultMatchWorkLimit = 1_000_000;

	public int MatchWorkLimit { get; init; } = DefaultMatchWorkLimit;
	public bool Caseless { get; init; }
	public bool Utf8 { get; init; }
	public bool MatchInvalidUtf { get; init; }
	public bool Ucp { get; init; }
	public bool AsciiBsD { get; init; }
	/// <summary>Recognize CR, LF, and CRLF as newlines, as in Grep's pinned PCRE2 build.</summary>
	public bool AnyCrLfNewline { get; init; }
	/// <summary>Restrict backslash-R sequences to CR, LF, and CRLF; independent of the newline convention.</summary>
	public bool BsrAnyCrLf { get; init; }

	/// <summary>Test-only progress hook used to cancel a running match deterministically.</summary>
	internal Action<int>? WorkObserver { get; init; }
}
