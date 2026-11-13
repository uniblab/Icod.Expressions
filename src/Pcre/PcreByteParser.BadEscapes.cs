namespace Icod.Expressions.Pcre;

internal sealed partial class PcreByteParser {
	private const int MaximumNativeReference = 65535;
	private bool suppressBadEscapeLiteral;

	private PcreByteNode ParseEscape( int offset, bool inBracket = false, bool allowBadLiteral = true ) {
		if ( !badEscapeIsLiteral || suppressBadEscapeLiteral || !allowBadLiteral ) { return ParseEscapeCore( offset, inBracket ); }
		var start = index;
		var savedReferences = backreferences.Count;
		var savedNamed = hasNamedReferences;
		var savedSubroutines = hasSubroutines;
		try { return ParseEscapeCore( offset, inBracket ); }
		catch ( PcreByteParseException error ) when ( error.Diagnostic.Code is
			PcreByteDiagnosticCode.InvalidSyntax or PcreByteDiagnosticCode.UnsupportedSyntax ) {
			// Eligibility scans belong in the catch body: a filter would swallow
			// cancellation raised while examining a long native reference number.
			if ( !CanLiteralizeBadEscape( start, inBracket, error ) ) { throw; }
			backreferences.RemoveRange( savedReferences, backreferences.Count - savedReferences );
			hasNamedReferences = savedNamed;
			hasSubroutines = savedSubroutines;
			index = start < source.Length ? start + 1 : start;
			return new PcreByteLiteral( start < source.Length ? source[start] : (byte)'\\' );
		}
	}

	private bool CanLiteralizeBadEscape( int start, bool inBracket, PcreByteParseException error ) {
		if ( start == source.Length ) { return true; }
		var value = source[start];
		if ( value >= 0x80 ) { return false; } // Valid escaped UTF is not yet implemented.
		if ( value is (byte)'p' or (byte)'P' ) { return !inBracket; }
		if ( value == (byte)'C' ) { return !inBracket && neverBackslashC; }
		if ( value is (byte)'X' or (byte)'R' or (byte)'G' or (byte)'K' ) { return false; }
		if ( value == (byte)'N' ) {
			return !inBracket || start + 1 < source.Length && source[start + 1] == (byte)'{';
		}
		if ( value == (byte)'g' ) { return InvalidNativeReferenceEscape( start ); }
		if ( value is >= (byte)'0' and <= (byte)'9' ) {
			return error.IsNumericEscape || !inBracket && NativeDecimalReferenceOverflow( start );
		}
		return value is >= (byte)'A' and <= (byte)'Z' or >= (byte)'a' and <= (byte)'z';
	}

	// Called only after the shared reader failed. A native-valid reference that
	// exceeds our capture ceiling must remain an error, never become a literal.
	private bool InvalidNativeReferenceEscape( int start ) {
		var cursor = start + 1;
		if ( cursor == source.Length ) { return true; }
		var delimiter = source[cursor];
		var braced = delimiter == (byte)'{';
		var call = delimiter is (byte)'<' or (byte)'\'';
		var terminator = braced ? (byte)'}'
			: delimiter == (byte)'<' ? (byte)'>' : (byte)'\'';
		if ( braced || call ) { cursor++; }
		if ( braced ) { SkipCountedSpace( ref cursor ); }
		var sign = 0;
		if ( cursor < source.Length && source[cursor] is (byte)'+' or (byte)'-' ) {
			sign = source[cursor++] == (byte)'+' ? 1 : -1;
		}
		var firstDigit = cursor;
		var maximum = sign > 0 ? MaximumNativeReference - groupCount : MaximumNativeReference;
		var magnitude = 0;
		while ( cursor < source.Length && source[cursor] is >= (byte)'0' and <= (byte)'9' ) {
			cancellationToken.ThrowIfCancellationRequested();
			var digit = source[cursor++] - (byte)'0';
			if ( magnitude > ( maximum - digit ) / 10 ) { return true; }
			magnitude = magnitude * 10 + digit;
		}
		// A name arriving here already failed the ordinary native name grammar.
		if ( cursor == firstDigit ) { return true; }
		if ( magnitude == 0 && ( !call || sign != 0 ) || sign < 0 && magnitude > groupCount ) { return true; }
		if ( braced ) { SkipCountedSpace( ref cursor ); }
		return ( braced || call ) && ( cursor >= source.Length || source[cursor] != terminator );
	}

	private bool NativeDecimalReferenceOverflow( int start ) {
		if ( source[start] == (byte)'0' || !pythonOctal && source[start] < (byte)'8' ) { return false; }
		var number = 0;
		for ( var cursor = start; cursor < source.Length && source[cursor] is >= (byte)'0' and <= (byte)'9'; cursor++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			var digit = source[cursor] - (byte)'0';
			if ( number > ( MaximumNativeReference - digit ) / 10 ) { return true; }
			number = number * 10 + digit;
		}
		return false;
	}
}
