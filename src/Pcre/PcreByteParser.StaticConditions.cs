namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	// Internal syntax compatibility target, independent of the package version.
	private const int CompatibilityMajor = 10;
	private const int CompatibilityMinor = 48;
	private readonly record struct StaticCondition( bool Value, bool IsDefinition );

	private bool TryParseStaticCondition( int offset, out StaticCondition condition ) {
		condition = default;
		var remaining = source.AsSpan( index );
		if ( remaining.StartsWith( "DEFINE)"u8 ) ) {
			index += 7;
			condition = new( false, true );
			return true;
		}
		if ( !remaining.StartsWith( "VERSION"u8 ) || remaining.Length <= 7 || remaining[7] == (byte)')' ) {
			return false; // Exact VERSION and wrapped names retain capture semantics.
		}
		index += 7;
		var atLeast = source[index] == (byte)'>';
		if ( atLeast ) { index++; }
		if ( index >= source.Length || source[index] != (byte)'=' ) {
			throw Invalid( offset, "invalid version condition operator" );
		}
		index++;
		var major = ReadConditionVersion( offset );
		var minor = 0;
		if ( index < source.Length && source[index] == (byte)'.' ) {
			index++;
			minor = ReadConditionVersion( offset );
		}
		FinishCaptureConditionHeader( offset );
		var value = atLeast
			? CompatibilityMajor > major || CompatibilityMajor == major && CompatibilityMinor >= minor
			: CompatibilityMajor == major && CompatibilityMinor == minor;
		condition = new( value, false );
		return true;
	}

	private int ReadConditionVersion( int offset ) {
		var start = index;
		var number = 0;
		while ( index < source.Length && source[index] is >= (byte)'0' and <= (byte)'9' ) {
			cancellationToken.ThrowIfCancellationRequested();
			var digit = source[index++] - (byte)'0';
			if ( number > ( 1000 - digit ) / 10 ) {
				throw Invalid( offset, "version condition component exceeds 1000" );
			}
			number = number * 10 + digit;
		}
		if ( index == start ) { throw Invalid( offset, "version condition requires ASCII decimal components" ); }
		return number;
	}
}
