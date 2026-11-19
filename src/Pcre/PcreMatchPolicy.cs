namespace Icod.Expressions.Pcre;

/// <summary>Additional per-call restrictions; compiled and source policies remain in force.</summary>
internal readonly record struct PcreMatchPolicy {
	public bool Anchored { get; init; }
	public bool EndAnchored { get; init; }
	public bool NotEmpty { get; init; }
	public bool NotEmptyAtStart { get; init; }
	public bool NotBol { get; init; }
	public bool NotEol { get; init; }
}
