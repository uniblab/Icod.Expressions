using System.Text;
using Icod.Expressions.Pcre;

namespace Icod.Expressions.PackageSmoke;

internal sealed record PcreCaptureInfoProtocol( int CaptureCount, string Names, string Captures );

internal static class PcreCaptureInfoConsumer {
	internal static PcreCaptureInfoProtocol Run( PcreCaptureInfoCase item ) {
		var pattern = Convert.FromHexString( item.Pattern );
		var source = new byte[pattern.Length + 2];
		source[0] = source[^1] = 0xFF;
		pattern.CopyTo( source, 1 );
		var result = PcreByteCompiler.Compile( source.AsMemory( 1, pattern.Length ), new PcreByteOptions {
			Utf8 = ( item.Mode & 1 ) != 0, Ucp = ( item.Mode & 2 ) != 0, Literal = ( item.Mode & 4 ) != 0,
			NoAutoCapture = ( item.Mode & 8 ) != 0, DuplicateNames = ( item.Mode & 16 ) != 0
		} );
		if ( !result.IsSuccess ) { throw new InvalidOperationException( $"Metadata case {item.Id}: {result.Diagnostic?.Code}." ); }
		Array.Fill( source, (byte)'X' );
		var subject = Convert.FromHexString( item.Subject );
		var backing = new byte[subject.Length + 2];
		backing[0] = backing[^1] = 0xFF;
		subject.CopyTo( backing, 1 );
		var match = result.Pattern!.Match( backing.AsMemory( 1, subject.Length ) );
		if ( !match.IsSuccess ) { throw new InvalidOperationException( $"Metadata case {item.Id}: {match.Diagnostic?.Code}." ); }
		var captures = match.Match is { } value ? String.Join( ",", value.Captures.Select( c => c is { } v ? $"{v.ByteIndex}:{v.ByteLength}" : "-" ) ) : "";
		if ( captures != item.Captures ) { throw new InvalidOperationException( $"Metadata case {item.Id} changed the existing match: {captures}." ); }
		var info = result.Pattern.CaptureInfo;
		var names = String.Join( ";", info.Names.Select( name => String.Concat( Convert.ToHexString( Encoding.UTF8.GetBytes( name ) ), "=",
			String.Join( ",", info.GetGroupNumbers( name ) ) ) ) );
		return new( info.CaptureCount, names, captures );
	}
}
