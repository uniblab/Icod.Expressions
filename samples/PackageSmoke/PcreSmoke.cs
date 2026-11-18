using System.Globalization;
using Icod.Expressions.Pcre;

namespace Icod.Expressions.PackageSmoke;

// Shared consumer exercise code: source-linked into tests, package-only in this app.
internal static class PcreSmoke {
	internal static PcreSmokeProtocol Run( PcreSmokeCase item ) {
		var patternBytes = Convert.FromHexString( item.Pattern );
		var subjectBytes = Convert.FromHexString( item.Subject );
		var pattern = Padded( patternBytes );
		var subject = Padded( subjectBytes );
		var compiled = PcreByteCompiler.Compile( pattern.AsMemory( 1, patternBytes.Length ), new PcreByteOptions {
			Utf8 = ( item.Mode & 1 ) != 0, Ucp = ( item.Mode & 2 ) != 0, MatchWorkLimit = item.WorkLimit
		} );
		if ( !compiled.IsSuccess ) {
			var diagnostic = compiled.Diagnostic ?? throw new InvalidOperationException( "Missing compile diagnostic." );
			return new( "C", Diagnostic: diagnostic.Code.ToString(), DiagnosticOffset: diagnostic.ByteOffset );
		}
		// The compiler owns the visible pattern snapshot, including mark bytes.
		Array.Fill( pattern, (byte)'X' );
		var result = compiled.Pattern!.Match( subject.AsMemory( 1, subjectBytes.Length ), item.Start );
		if ( !result.IsSuccess ) {
			var diagnostic = result.Diagnostic ?? throw new InvalidOperationException( "Missing match diagnostic." );
			return new( "X", Diagnostic: diagnostic.Code.ToString(), DiagnosticOffset: diagnostic.ByteOffset );
		}
		if ( result.Match is not { } match ) { return new( "N" ); }
		return new( "M", String.Join( ",", match.Captures.Select( capture => capture is { } value
			? String.Concat( value.ByteIndex.ToString( CultureInfo.InvariantCulture ), ":", value.ByteLength.ToString( CultureInfo.InvariantCulture ) )
			: "-" ) ), Convert.ToHexString( match.MarkName.Span ) );
	}

	internal static void VerifyObservedView() {
		var subject = new byte[] { 0xFF, (byte)'A', 0xFE };
		var compiled = PcreByteCompiler.Compile( new byte[] { (byte)'B' }, new PcreByteOptions {
			WorkObserver = count => { if ( count == 1 ) { subject[1] = (byte)'B'; } }
		} );
		if ( !compiled.IsSuccess ) { throw new InvalidOperationException( "Observed-view pattern did not compile." ); }
		var result = compiled.Pattern!.Match( subject.AsMemory( 1, 1 ) );
		if ( !result.IsSuccess || result.Match is not { ByteIndex: 0, ByteLength: 1 }
			|| subject[0] != 0xFF || subject[1] != (byte)'B' || subject[2] != 0xFE ) {
			throw new InvalidOperationException( "Observed match did not use the authoritative visible byte view." );
		}
	}

	internal static void VerifyCancellation( bool compile ) {
		using var source = new CancellationTokenSource();
		source.Cancel();
		var pattern = new byte[] { (byte)'B' };
		if ( compile ) {
			_ = PcreByteCompiler.Compile( pattern, cancellationToken: source.Token );
			return;
		}
		var compiled = PcreByteCompiler.Compile( pattern );
		if ( !compiled.IsSuccess ) { throw new InvalidOperationException( "Cancellation pattern did not compile." ); }
		_ = compiled.Pattern!.Match( pattern, cancellationToken: source.Token );
	}

	private static byte[] Padded( byte[] bytes ) {
		var result = new byte[checked( bytes.Length + 2 )];
		result[0] = 0xFF;
		result[^1] = 0xFE;
		bytes.AsSpan().CopyTo( result.AsSpan( 1 ) );
		return result;
	}
}
