namespace Icod.Expressions.Pcre;

/// <summary>Owned, immutable capture-number and name metadata, excluding group zero.</summary>
internal sealed class PcreCaptureInfo {
	private static readonly IReadOnlyList<int> EmptyNumbers = Array.AsReadOnly( Array.Empty<int>() );
	private readonly Dictionary<string, (IReadOnlyList<int> Canonical, IReadOnlyList<int> Lookup)> groupNumbers;

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
			var ascending = true;
			for ( var i = 0; i < numbers.Length; i++ ) {
				cancellationToken.ThrowIfCancellationRequested();
				numbers[i] = entry.Value[i];
				if ( i > 0 && numbers[i - 1] > numbers[i] ) { ascending = false; }
			}
			// Native duplicate-name lookup retains registration order, which can
			// be 2,1 under branch reset. Keep the canonical numeric API separate.
			var lookup = Array.AsReadOnly( numbers );
			var canonical = lookup;
			if ( !ascending ) {
				var sorted = (int[])numbers.Clone();
				Array.Sort( sorted );
				canonical = Array.AsReadOnly( sorted );
			}
			cancellationToken.ThrowIfCancellationRequested();
			groupNumbers.Add( entry.Key, (canonical, lookup) );
		}
		Array.Sort( names, StringComparer.Ordinal );
		cancellationToken.ThrowIfCancellationRequested();
		Names = Array.AsReadOnly( names );
	}

	public int CaptureCount { get; }
	public IReadOnlyList<string> Names { get; }

	public IReadOnlyList<int> GetGroupNumbers( string name ) {
		ArgumentNullException.ThrowIfNull( name );
		if ( groupNumbers.TryGetValue( name, out var numbers ) ) { return numbers.Canonical; }
		return EmptyNumbers;
	}

	internal IReadOnlyList<int> GetLookupGroupNumbers( string name ) {
		ArgumentNullException.ThrowIfNull( name );
		if ( groupNumbers.TryGetValue( name, out var numbers ) ) { return numbers.Lookup; }
		return EmptyNumbers;
	}
}
