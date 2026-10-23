using System.Text;
using Icod.CommandFramework.RegularExpressions;
using Icod.CommandFramework.Text;

if ( args.Length != 1 || ( args[ 0 ] != "--original" && args[ 0 ] != "--forwarded" ) ) {
	Console.Error.WriteLine( "Expected --original or --forwarded." );
	return 2;
}

var expectedRegexAssembly = args[ 0 ] == "--forwarded"
	? "Icod.Expressions"
	: "Icod.CommandFramework";
var regexAssembly = typeof( GnuExtendedRegularExpressionProvider ).Assembly.GetName().Name;
var decodingAssembly = typeof( TextDecodingMode ).Assembly.GetName().Name;
if ( regexAssembly != expectedRegexAssembly || decodingAssembly != expectedRegexAssembly ) {
	throw new InvalidOperationException(
		$"Expected {expectedRegexAssembly}, got regex={regexAssembly}, decoding={decodingAssembly}."
	);
}

var provider = new GnuExtendedRegularExpressionProvider(
	PosixCLocaleRegularExpressionCharacterClassProvider.Instance
);
var compiled = provider.Compile( "(a|aa)" );
if ( !compiled.IsSuccess ) {
	throw new InvalidOperationException( $"Compile failed: {compiled.Diagnostic?.Code}" );
}
var text = compiled.Expression!.Match( "zaa" );
if ( !text.IsSuccess || !text.IsMatch || text.Match!.Index != 1
	|| text.Match.Length != 2 || text.Match.Captures[ 0 ].Value != "aa" ) {
	throw new InvalidOperationException( "String match or capture changed." );
}

var byteMatch = provider.Compile( "a+" ).Expression!.Match(
	Encoding.ASCII.GetBytes( "xxaaa" ),
	new RegularExpressionInputOptions { DecodingMode = TextDecodingMode.Bytes }
);
if ( !byteMatch.IsSuccess || !byteMatch.IsMatch || byteMatch.Match!.ByteIndex != 2
	|| byteMatch.Match.ByteLength != 3 ) {
	throw new InvalidOperationException( "Authoritative byte offsets changed." );
}

Console.WriteLine( $"{args[ 0 ]}: regex and Text enum from {regexAssembly}; string and bytes passed." );
return 0;
