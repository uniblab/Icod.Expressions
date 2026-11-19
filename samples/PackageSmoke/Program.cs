using System.Text;
using System.Text.Json;
using Icod.Expressions.PackageSmoke;

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

var pcreCases = 0;
foreach ( var item in PcreSmokeCases.All ) {
	var actual = PcreSmoke.Run( item );
	if ( actual != item.Expected ) {
		throw new InvalidOperationException( $"PCRE package case {item.Id}: expected {item.Expected}, actual {actual}." );
	}
	pcreCases++;
}
PcreSmoke.VerifyObservedView();
var pcreCancellations = 0;
foreach ( var compile in new[] { true, false } ) {
	try {
		PcreSmoke.VerifyCancellation( compile );
		throw new InvalidOperationException( "PCRE cancellation did not propagate." );
	} catch ( OperationCanceledException ) {
		pcreCancellations++;
	}
}
Console.WriteLine( JsonSerializer.Serialize( new { kind = "pcre-package-smoke", cases = pcreCases, view = 1, cancellation = pcreCancellations, failed = 0 } ) );

var pcreAllocationProfiles = PcreAllocationCases.All.Select( PcreAllocationConsumer.Run ).ToArray();
Console.WriteLine( JsonSerializer.Serialize( new {
	kind = "pcre-allocation-smoke", passed = pcreAllocationProfiles.Length, failed = 0,
	profiles = pcreAllocationProfiles.Select( profile => new {
		name = profile.Name, smallInputBytes = profile.SmallInputBytes, largeInputBytes = profile.LargeInputBytes,
		iterations = profile.Metrics.Iterations, smallAllocatedBytes = profile.Metrics.SmallAllocatedBytes,
		largeAllocatedBytes = profile.Metrics.LargeAllocatedBytes, allowanceBytesPerCall = PcreAllocationCases.FixedAllowance
	} )
} ) );

var pcreFragmentProfiles = PcreFragmentAllocationCases.All.Select( profile => PcreFragmentAllocationConsumer.Run( profile ) ).ToArray();
Console.WriteLine( JsonSerializer.Serialize( new {
	kind = "pcre-fragment-allocation-smoke", passed = pcreFragmentProfiles.Length, failed = 0,
	profiles = pcreFragmentProfiles.Select( profile => new {
		name = profile.Name, smallInputBytes = profile.SmallInputBytes, largeInputBytes = profile.LargeInputBytes,
		iterations = profile.Metrics.Iterations, smallAllocatedBytes = profile.Metrics.SmallAllocatedBytes,
		largeAllocatedBytes = profile.Metrics.LargeAllocatedBytes, allowanceBytesPerCall = PcreAllocationCases.FixedAllowance
	} )
} ) );

var pcreCaptureInfoCases = 0;
foreach ( var item in PcreCaptureInfoCases.All ) {
	var actual = PcreCaptureInfoConsumer.Run( item );
	if ( actual != new PcreCaptureInfoProtocol( item.CaptureCount, item.Names, item.Captures ) ) {
		throw new InvalidOperationException( $"PCRE capture metadata case {item.Id} changed." );
	}
	pcreCaptureInfoCases++;
}
Console.WriteLine( JsonSerializer.Serialize( new { kind = "pcre-capture-info-smoke", cases = pcreCaptureInfoCases, failed = 0 } ) );

var pcreNamedCaptureCases = 0;
foreach ( var item in PcreNamedCaptureCases.All ) {
	if ( PcreNamedCaptureConsumer.Run( item ) != new PcreNamedCaptureProtocol( item.Kind, item.Span, item.Value ) ) {
		throw new InvalidOperationException( $"PCRE named capture result {item.Id} changed." );
	}
	pcreNamedCaptureCases++;
}
Console.WriteLine( JsonSerializer.Serialize( new { kind = "pcre-named-capture-smoke", cases = pcreNamedCaptureCases, failed = 0 } ) );

Console.WriteLine( $"Icod.Expressions package smoke passed on {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}." );
