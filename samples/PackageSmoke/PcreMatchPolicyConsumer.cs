using Icod.Expressions.Pcre;

namespace Icod.Expressions.PackageSmoke;

internal static class PcreMatchPolicyConsumer {
	internal static PcreMatchPolicy Policy( int mask ) => new() {
		Anchored = ( mask & 1 ) != 0, EndAnchored = ( mask & 2 ) != 0,
		NotEmpty = ( mask & 4 ) != 0, NotEmptyAtStart = ( mask & 8 ) != 0,
		NotBol = ( mask & 16 ) != 0, NotEol = ( mask & 32 ) != 0
	};

	internal static PcreByteOptions Options( PcreMatchPolicyCase item ) => new() {
		Utf8 = ( item.Mode & 1 ) != 0, Ucp = ( item.Mode & 2 ) != 0, MatchInvalidUtf = ( item.Mode & 4 ) != 0,
		AllowLookaroundBackslashK = ( item.Mode & 8 ) != 0, Caseless = ( item.Mode & 16 ) != 0,
		Literal = ( item.Mode & 32 ) != 0, AnyCrLfNewline = ( item.Mode & 64 ) != 0,
		DollarEndOnly = ( item.Mode & 128 ) != 0, AltCircumflex = ( item.Mode & 256 ) != 0,
		FirstLine = ( item.Mode & 512 ) != 0,
		Anchored = ( item.Defaults & 1 ) != 0, EndAnchored = ( item.Defaults & 2 ) != 0,
		NotEmpty = ( item.Defaults & 4 ) != 0, NotEmptyAtStart = ( item.Defaults & 8 ) != 0,
		NotBol = ( item.Defaults & 16 ) != 0, NotEol = ( item.Defaults & 32 ) != 0
	};

	internal static string Project( PcreByteMatchResult result ) {
		if ( !result.IsSuccess ) { throw new InvalidOperationException( $"Policy match: {result.Diagnostic?.Code}." ); }
		if ( result.Match is not { } match ) { return "/"; }
		return String.Concat( String.Join( ",", match.Captures.Select( c => c is { } v ? $"{v.ByteIndex}:{v.ByteLength}" : "-" ) ), "/", Convert.ToHexString( match.MarkName.Span ) );
	}

	internal static string Run( PcreMatchPolicyCase item ) {
		var bytes = Convert.FromHexString( item.Pattern );
		var source = new byte[bytes.Length + 2]; source[0] = source[^1] = 0xFF; bytes.CopyTo( source, 1 );
		var compiled = PcreByteCompiler.Compile( source.AsMemory( 1, bytes.Length ), Options( item ) );
		if ( !compiled.IsSuccess ) { throw new InvalidOperationException( $"Policy compile {item.Id}: {compiled.Diagnostic?.Code}." ); }
		Array.Fill( source, (byte)'X' );
		var bytesIn = Convert.FromHexString( item.Subject );
		var padded = new byte[bytesIn.Length + 2]; padded[0] = padded[^1] = 0xFF; bytesIn.CopyTo( padded, 1 );
		var input = padded.AsMemory( 1, bytesIn.Length );
		var pattern = compiled.Pattern!;
		var baseline = Project( pattern.Match( input, item.Start ) );
		if ( baseline != item.Outcomes.Split( ';' )[0] ) { throw new InvalidOperationException( $"Policy {item.Id} changed the compiled default: {baseline}." ); }
		var outcomes = new string[64];
		for ( var mask = 0; mask < outcomes.Length; mask++ ) {
			var result = pattern.Match( input, Policy( mask ), item.Start );
			outcomes[mask] = Project( result );
			if ( result.Match is { } match && !ReferenceEquals( pattern.CaptureInfo, match.CaptureInfo ) ) { throw new InvalidOperationException( "Policy lost metadata identity." ); }
			if ( Project( pattern.Match( input, item.Start ) ) != baseline ) { throw new InvalidOperationException( $"Policy {item.Id}/{mask} leaked into the next call." ); }
		}
		if ( padded[0] != 0xFF || padded[^1] != 0xFF ) { throw new InvalidOperationException( "Policy changed subject sentinels." ); }
		return String.Join( ";", outcomes );
	}
}
