using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Native prefix rejection and VM prefix failure have different CRLF continuations.</summary>
public sealed class PcreBytePrefixSelectorReferenceTests {
	[Theory]
	[InlineData( @"[^\r]\CA", "0D0A4241", "1:3" )]
	[InlineData( @"[^\r]\CA", "0D0A4141", "" )]
	[InlineData( @"(?>[^\r]\CA)", "0D0A4241", "1:3" )]
	[InlineData( @"(?>[^\r]\CA)", "0D0A4141", "" )]
	[InlineData( @"(?<x>[^\r])(?<y>\C)(A)", "0D0A4241", "1:3,1:1,2:1,3:1" )]
	[InlineData( @"(?<x>[^\r])(?<y>\C)(A)", "0D0A4141", "" )]
	[InlineData( @"\C[^\n]A", "0D0A4241", "1:3" )]
	[InlineData( @"\C[^\n]A", "0D0A4141", "" )]
	[InlineData( @"[^\r][^\n]A", "0D0A4241", "1:3" )]
	[InlineData( @"[^\r][^\n]A", "0D0A4141", "" )]
	[InlineData( @"[^\r]\p{Any}A", "0D0A4241", "1:3" )]
	[InlineData( @"[^\r]\p{Any}A", "0D0A4141", "" )]
	[InlineData( @"\p{Any}\CA", "0D0A4241", "1:3" )]
	[InlineData( @"\p{Any}\CA", "0D0A4141", "0:3" )]
	[InlineData( @"\P{L}\CA", "0D0A4241", "1:3" )]
	[InlineData( @"\P{L}\CA", "0D0A4141", "0:3" )]
	[InlineData( @"[\p{L}\P{L}]\CA", "0D0A4241", "1:3" )]
	[InlineData( @"[\p{L}\P{L}]\CA", "0D0A4141", "0:3" )]
	[InlineData( @"(*CRLF).\CA", "0D0A4241", "1:3" )]
	[InlineData( @"(*CRLF).\CA", "0D0A4141", "" )]
	[InlineData( @"(*CRLF).{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"(*CRLF).{2}A", "0D0A4141", "" )]
	[InlineData( @"(*CRLF)\N{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"(*CRLF)\N{2}A", "0D0A4141", "" )]
	[InlineData( @"[^\r]\C{10}A", "0D0A4242424242424242424241", "1:12" )]
	[InlineData( @"[^\r]\C{10}A", "0D0A4242424242424242424141", "" )]
	[InlineData( @"[^\r]\C{11}A", "0D0A424242424242424242424241", "" )]
	[InlineData( @"\C{10}[^\n]A", "0D0A4242424242424242424241", "1:12" )]
	[InlineData( @"[^\r]\C{0}A", "0D0A4241", "2:2" )]
	[InlineData( @"[^\r]\C{1}A", "0D0A4241", "1:3" )]
	[InlineData( @"[^\r]\C{2}?A", "0D0A424241", "1:4" )]
	[InlineData( @"[^\r]\C{2}+A", "0D0A424241", "1:4" )]
	[InlineData( @"(?i)[^\r]\Ca", "0D0A6141", "" )]
	[InlineData( @"(?ir)[^\r]\Ca", "0D0A6141", "" )]
	[InlineData( @"(?i:[^\r]\C)a", "0D0A4161", "1:3" )]
	[InlineData( @"[^\r]\C(?i:a)", "0D0A4161", "" )]
	[InlineData( @"[^\r]\C\xE9", "0D0AC9E9", "1:3" )]
	[InlineData( @"(*UCP)(?i)[^\r]\C\xE9", "0D0AC9E9", "" )]
	[InlineData( @"(*UCP)(?ir)[^\r]\C\xE9", "0D0AC9E9", "" )]
	[InlineData( @"(*UCP)(?i)[^\r]\C\xE9", "0D0A42E9", "1:3" )]
	[InlineData( @"[^\r]\C\x00", "0D0A4200", "1:3" )]
	[InlineData( @"[^\r]\C\x00", "0D0A0000", "" )]
	[InlineData( @"[^\r]\C\xFF", "0D0A42FF", "1:3" )]
	[InlineData( @"[^\r]\C\xFF", "0D0AFFFF", "" )]
	[InlineData( @"[^\r]\C\n", "0D0A420A", "1:3" )]
	[InlineData( @"(*CRLF).{1}A", "0D0A41", "1:2" )]
	[InlineData( @"[^\r]\CA", "0D0A", "" )]
	[InlineData( @"[^\r]{2}A", "0D0A4141", "" )]
	[InlineData( @"[^\r]\CA(?!)", "0D0A4241", "" )]
	public void LiteralSelectionPreservesPinnedPositiveAndMissCaptureSpans( string body, string hex, string spans ) {
		var pattern = "(*ANYCRLF)" + body;
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern );
		var expected = reference.Match( subject );
		Assert.Equal( spans.Length > 0, expected.Success );
		if ( expected.Success ) {
			var groups = spans.Split( ',' );
			Assert.Equal( groups.Length, expected.Groups.Count );
			for ( var group = 0; group < groups.Length; group++ ) {
				var capture = expected.Groups[group];
				Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
			}
		}
		Compare( reference, subject, 0, Compile( pattern ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( "(*LF)" )]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CR)" )]
	[InlineData( "(*NUL)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void EveryOffsetPreservesConditionalByteEntryAndCutoff( string newline ) {
		string[] bodies = { @"[^\r]\CA", @"(?>[^\r]\CA)", @"(?<x>[^\r])(?<y>\C)(A)", @"\C[^\n]A",
			@"[^\r][^\n]A", @"[^\r]\p{Any}A", @"\p{Any}\CA", @"[\p{L}\P{L}]\CA", @"\P{L}\CA",
			@"[^\r]\C{10}A", @"[^\r]\C{11}A", @"\C{10}[^\n]A", @"[^\r]\C{2}?A", @"[^\r]\C{2}+A",
			@"(?i)[^\r]\Ca", @"(?i:[^\r]\C)a", @"[^\r]\C(?i:a)", @"(?i)[^\r]\C\xE9" };
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var body in bodies ) {
				var pattern = newline + body;
				var reference = Reference( pattern, ucp );
				var compiled = Compile( pattern, new() { Ucp = ucp } );
				foreach ( var hex in Subjects() ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}/{ucp}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void UtfPrefixesRetainNativePairSkippingAndInvalidFragmentBounds( string newline ) {
		foreach ( var body in new[] { @"[^\r](?s:.)A", @"(?>[^\r](?s:.)A)", @"\p{Any}(?s:.)A", @".{2}A" } ) {
			var pattern = newline + body;
			var reference = Reference( pattern, true, true );
			var compiled = Compile( pattern, new() { Utf8 = true, Ucp = true, MatchInvalidUtf = true } );
			foreach ( var hex in new[] { "0D0A4241", "0D0A4141", "C3A90D0A4241", "FF0D0A4241", "0D0AFF41", "EDA0800D0A41" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"[^\r]\CA", "0D0A4241" )]
	[InlineData( @"(?>[^\r]\CA)", "0D0A4241" )]
	[InlineData( @"(?<x>[^\r])(?<y>\C)(A)", "0D0A4241" )]
	[InlineData( @"[^\r]\C{10}A", "0D0A4242424242424242424241" )]
	public void ConditionalEntrySharesObservedExactAndExhaustedBudgets( string body, string hex ) {
		var pattern = "(*ANYCRLF)" + body;
		var subject = Convert.FromHexString( hex );
		var steps = new List<int>();
		var result = Compile( pattern, new() { WorkObserver = steps.Add } ).Match( subject );
		Assert.True( result.IsSuccess );
		Assert.Equal( 1, result.Match?.ByteIndex );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		foreach ( var observe in new[] { false, true } ) {
			Compare( Reference( pattern ), subject, 0, Compile( pattern, new() { MatchWorkLimit = steps.Count,
				WorkObserver = observe ? _ => { } : null } ).Match( subject ), pattern );
			var exhausted = Compile( pattern, new() { MatchWorkLimit = steps.Count - 1,
				WorkObserver = observe ? _ => { } : null } ).Match( subject );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
	}

	[Fact]
	public void FailedCrLiteralSelectionIsChargedAndCancellable() {
		const string pattern = @"(*ANYCRLF)[^\r]\CA";
		var subject = "\r\nAA"u8.ToArray();
		// CR consumes3 work, its selector1, then candidates A/A/end consume7/5/3.
		// Budget19 completes the miss;18 must fail instead of hiding the new comparison.
		foreach ( var observed in new[] { false, true } ) {
			var steps = new List<int>();
			var exact = Compile( pattern, new() { MatchWorkLimit = 19, WorkObserver = observed ? steps.Add : null } ).Match( subject );
			Assert.True( exact.IsSuccess );
			Assert.Null( exact.Match );
			if ( observed ) { Assert.Equal( Enumerable.Range( 1, 19 ), steps ); }
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
				Compile( pattern, new() { MatchWorkLimit = 18, WorkObserver = observed ? _ => { } : null } ).Match( subject ).Diagnostic?.Code );
		}
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void ConditionalProofPreservesDepthAndRepeatCeilings() {
		var wrapped = "(*ANYCRLF)" + String.Concat( Enumerable.Repeat( "(?>", 200 ) ) + @"(?<x>[^\r])(?<y>\C)(A)" + new string( ')', 200 );
		var subject = "\r\nBA"u8.ToArray();
		Compare( Reference( wrapped ), subject, 0, Compile( wrapped ).Match( subject ), wrapped );
		var depth = String.Concat( Enumerable.Repeat( "(?>", 257 ) ) + @"[^\r]\CA" + new string( ')', 257 );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( depth ) ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( @"[^\r]\C{4096}A" ) ).Diagnostic?.Code );
	}

	private static string[] Subjects() => new[] { "", "00", "0D0A", "0D0A4241", "0D0A4141", "0D0A4161", "FF0D0A4241",
		"0D0AFF41", "C3A90D0A4241", "0D0A42410D0A4141", "0D0AC9E9", "0D0A4242424242424242424241",
		"0D0A424242424242424242424241" };

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}: native={( expected.Success ? expected.Index : -1 )}, managed={actual.Match?.ByteIndex ?? -1}" );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool ucp = false, bool utf = false ) => new(
		Encoding.ASCII.GetBytes( pattern ), Encoding.Latin1, new PcreRegexSettings {
			Options = PcreOptions.Compiled | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( utf ? PcreOptions.Utf | PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
