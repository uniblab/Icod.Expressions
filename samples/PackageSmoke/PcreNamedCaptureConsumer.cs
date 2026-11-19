using System.Globalization;
using System.Text;
using Icod.Expressions.Pcre;

namespace Icod.Expressions.PackageSmoke;

internal sealed record PcreNamedCaptureProtocol( string Kind, string Span, string Value );

internal static class PcreNamedCaptureConsumer {
	internal static PcreNamedCaptureProtocol Run( PcreNamedCaptureCase item ) {
		var basis = PcreCaptureInfoCases.All.Single( c => c.Id == item.BaseId );
		var bytes = Convert.FromHexString( basis.Pattern );
		var source = new byte[bytes.Length + 2]; source[0] = source[^1] = 0xFF; bytes.CopyTo( source, 1 );
		var compiled = PcreByteCompiler.Compile( source.AsMemory( 1, bytes.Length ), new PcreByteOptions {
			Utf8 = ( basis.Mode & 1 ) != 0, Ucp = ( basis.Mode & 2 ) != 0, Literal = ( basis.Mode & 4 ) != 0,
			NoAutoCapture = ( basis.Mode & 8 ) != 0, DuplicateNames = ( basis.Mode & 16 ) != 0
		} );
		if ( !compiled.IsSuccess ) { throw new InvalidOperationException( $"Named result {item.Id}: {compiled.Diagnostic?.Code}." ); }
		Array.Fill( source, (byte)'X' );
		var input = Convert.FromHexString( basis.Subject );
		var padded = new byte[input.Length + 2]; padded[0] = padded[^1] = 0xFF; input.CopyTo( padded, 1 );
		var result = compiled.Pattern!.Match( padded.AsMemory( 1, input.Length ) );
		if ( !result.IsSuccess ) { throw new InvalidOperationException( $"Named result {item.Id}: {result.Diagnostic?.Code}." ); }
		var spans = result.Match is { } whole ? String.Join( ",", whole.Captures.Select( c => c is { } v ? $"{v.ByteIndex}:{v.ByteLength}" : "-" ) ) : "";
		if ( spans != basis.Captures ) { throw new InvalidOperationException( $"Named result {item.Id} changed existing numbered captures: {spans}." ); }
		if ( result.Match is not { } match ) { return new( "N", "", "" ); }
		var name = Encoding.UTF8.GetString( Convert.FromHexString( item.Name ) );
		var capture = match.GetCapture( name );
		if ( !ReferenceEquals( compiled.Pattern.CaptureInfo, match.CaptureInfo ) ) { throw new InvalidOperationException( "Match metadata is not the compiled snapshot." ); }
		if ( capture is not { } selected ) {
			return new( match.CaptureInfo.GetGroupNumbers( name ).Count > 0 ? "U" : "A", "", "" );
		}
		return new( "M", String.Concat( selected.ByteIndex.ToString( CultureInfo.InvariantCulture ), ":", selected.ByteLength.ToString( CultureInfo.InvariantCulture ) ),
			Convert.ToHexString( padded.AsSpan( 1 + selected.ByteIndex, selected.ByteLength ) ) );
	}
}
