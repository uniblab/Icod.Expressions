namespace Icod.Expressions.Pcre;

/// <summary>Provisional internal limits for one compiled byte pattern.</summary>
internal sealed record PcreByteOptions {
	internal const int DefaultMatchWorkLimit = 1_000_000;

	public int MatchWorkLimit { get; init; } = DefaultMatchWorkLimit;
	public bool Caseless { get; init; }

	/// <summary>Test-only progress hook used to cancel a running match deterministically.</summary>
	internal Action<int>? WorkObserver { get; init; }
}
