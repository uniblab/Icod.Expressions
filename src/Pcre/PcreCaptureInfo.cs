namespace Icod.Expressions.Pcre;

/// <summary>Owned, immutable capture-number and name metadata, excluding group zero.</summary>
internal sealed class PcreCaptureInfo {
	private static readonly IReadOnlyList<int> EmptyNumbers = Array.AsReadOnly( Array.Empty<int>() );
	private readonly Dictionary<string, IReadOnlyList<int>> groupNumbers;

	internal PcreCaptureInfo( int captureCount, IReadOnlyDictionary<string, List<int>> captureNames, CancellationToken cancellationToken ) {
		cancellationToken.ThrowIfCancellationRequested();
		CaptureCount = captureCount;
		groupNumbers = new( captureNames.Count, StringComparer.Ordinal );
		var names = new string[captureNames.Count];
		var nameIndex = 0;
		foreach ( var entry in captureNames ) {
			cancellationToken.ThrowIfCancellationRequested();
			names[nameIndex++] = entry.Key;
			var numbers = new int[entry.Value.Count];
			for ( var i = 0; i < numbers.Length; i++ ) {
				cancellationToken.ThrowIfCancellationRequested();
				numbers[i] = entry.Value[i];
			}
			// The parser already deduplicates each name/number pair. Sort only
			// this owned copy; declaration order still drives named references.
			Array.Sort( numbers );
			cancellationToken.ThrowIfCancellationRequested();
			groupNumbers.Add( entry.Key, Array.AsReadOnly( numbers ) );
		}
		Array.Sort( names, StringComparer.Ordinal );
		cancellationToken.ThrowIfCancellationRequested();
		Names = Array.AsReadOnly( names );
	}

	public int CaptureCount { get; }
	public IReadOnlyList<string> Names { get; }

	public IReadOnlyList<int> GetGroupNumbers( string name ) {
		ArgumentNullException.ThrowIfNull( name );
		if ( groupNumbers.TryGetValue( name, out var numbers ) ) { return numbers; }
		return EmptyNumbers;
	}
}
