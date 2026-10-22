using System.Text;

using Icod.CommandFramework.RegularExpressions;
using Icod.CommandFramework.Text;

if ( typeof( GnuExtendedRegularExpressionProvider ).Assembly.GetName().Name != "Icod.Expressions" ) {
	throw new InvalidOperationException( "The package consumer loaded the wrong assembly." );
}

var provider = new GnuExtendedRegularExpressionProvider( PosixCLocaleRegularExpressionCharacterClassProvider.Instance );
var expression = provider.Compile( "(a|aa)" );
if ( !expression.IsSuccess ) {
	throw new InvalidOperationException( $"Compile failed: {expression.Diagnostic?.Code}" );
}

var text = expression.Expression!.Match( "zaa" );
if ( !text.IsSuccess || !text.IsMatch || text.Match!.Index != 1 || text.Match.Length != 2 || text.Match.Value != "aa" ) {
	throw new InvalidOperationException( "String leftmost-longest matching failed." );
}

var bytes = provider.Compile( "a+" ).Expression!.Match(
	Encoding.ASCII.GetBytes( "xxaaa" ),
	new RegularExpressionInputOptions { DecodingMode = TextDecodingMode.Bytes }
);
if ( !bytes.IsSuccess || !bytes.IsMatch || bytes.Match!.ByteIndex != 2 || bytes.Match.ByteLength != 3 ) {
	throw new InvalidOperationException( "Authoritative byte offsets failed." );
}

Console.WriteLine( $"Icod.Expressions package smoke passed on {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}." );
