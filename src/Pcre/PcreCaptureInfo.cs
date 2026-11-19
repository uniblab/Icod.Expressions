namespace Icod.Expressions.Pcre;

/// <summary>Owned, immutable capture-number and name metadata, excluding group zero.</summary>
internal sealed class PcreCaptureInfo {
	public int CaptureCount => throw new NotImplementedException();
	public IReadOnlyList<string> Names => throw new NotImplementedException();
	public IReadOnlyList<int> GetGroupNumbers( string name ) => throw new NotImplementedException();
}
