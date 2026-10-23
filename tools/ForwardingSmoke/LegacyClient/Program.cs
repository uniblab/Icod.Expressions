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
Type[] oldPublicTypes = [
	typeof( CompiledRegularExpressionPreparedInputExtensions ),
	typeof( GnuBasicRegularExpressionProvider ),
	typeof( GnuEmacsRegularExpressionProvider ),
	typeof( GnuExtendedRegularExpressionProvider ),
	typeof( GnuRegularExpressionSyntax ),
	typeof( ICompiledRegularExpression ),
	typeof( IRegularExpressionCharacterClassProvider ),
	typeof( IRegularExpressionProvider ),
	typeof( PosixCLocaleRegularExpressionCharacterClassProvider ),
	typeof( RegularExpressionByteCapture ),
	typeof( RegularExpressionByteMatch ),
	typeof( RegularExpressionByteMatchOptions ),
	typeof( RegularExpressionByteMatchResult ),
	typeof( RegularExpressionCapture ),
	typeof( RegularExpressionCompileResult ),
	typeof( RegularExpressionDiagnostic ),
	typeof( RegularExpressionDiagnosticCode ),
	typeof( RegularExpressionInputOptions ),
	typeof( RegularExpressionMatch ),
	typeof( RegularExpressionMatchOptions ),
	typeof( RegularExpressionMatchResult ),
	typeof( RegularExpressionOptions ),
	typeof( RegularExpressionPreparedByteInput ),
	typeof( UnicodeRegularExpressionCharacterClassProvider ),
	typeof( InvalidEncodingPolicy ),
	typeof( TextDecodingMode )
];
foreach ( var publicType in oldPublicTypes ) {
	var assemblyName = publicType.Assembly.GetName().Name;
	if ( assemblyName != expectedRegexAssembly ) {
		throw new InvalidOperationException(
			$"Expected {publicType.FullName} in {expectedRegexAssembly}, got {assemblyName}."
		);
	}
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

Console.WriteLine( $"{args[ 0 ]}: {oldPublicTypes.Length} regex/Text types from {expectedRegexAssembly}; string and bytes passed." );
return 0;
