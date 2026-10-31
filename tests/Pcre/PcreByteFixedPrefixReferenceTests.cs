using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins native fixed byte prefixes without replacing CRLF bumpalong globally.</summary>
public sealed class PcreByteFixedPrefixReferenceTests {
	[Theory]
	[InlineData( @"(?>[^\r]A)", "0D0A41", "1:2" )]
	[InlineData( @"(?>[^\r])A", "0D0A41", "1:2" )]
	[InlineData( @"[^\r](?>A)", "0D0A41", "1:2" )]
	[InlineData( @"(?>(?<x>[^\r])(?<y>A))", "0D0A41", "1:2,1:1,2:1" )]
	[InlineData( @"(?<x>(?>[^\r]A))", "0D0A41", "1:2,1:2" )]
	[InlineData( @"(?>(?>(?:[^\r]))(?>A))", "0D0A41", "1:2" )]
	[InlineData( @"(?>\p{Any}A)", "0D0A41", "1:2" )]
	[InlineData( @"(?>[\p{L}\n]A)", "0D0A41", "1:2" )]
	[InlineData( @"(?>\CA)", "0D0A41", "1:2" )]
	[InlineData( @"(?>\C)A", "0D0A41", "1:2" )]
	[InlineData( @"\C(?>A)", "0D0A41", "1:2" )]
	[InlineData( @"(?>(\C))(A)", "0D0A41", "1:2,1:1,2:1" )]
	[InlineData( @"\C{1}A", "0D0A41", "1:2" )]
	[InlineData( @"(?i)(?>\C)a", "0D0A41", "1:2" )]
	[InlineData( @"\C{2}A", "0D0A4241", "1:3" )]
	[InlineData( @"\C{2}?A", "0D0A4241", "1:3" )]
	[InlineData( @"\C{2}+A", "0D0A4241", "1:3" )]
	[InlineData( @"(\C{2})(A)", "0D0A4241", "1:3,1:2,3:1" )]
	[InlineData( @"(?>\C{2}A)", "0D0A4241", "1:3" )]
	[InlineData( @"\C\CA", "0D0A4241", "1:3" )]
	[InlineData( @"(?<x>\C)(?<y>\C)(A)", "0D0A4241", "1:3,1:1,2:1,3:1" )]
	[InlineData( @"(*CRLF)(?>(?s:.{2})A)", "0D0A4241", "1:3" )]
	[InlineData( @"\C{1,2}A", "0D0A4241", "2:2" )]
	[InlineData( @"\C{2,3}A", "0D0A4241", "0:4" )]
	[InlineData( @"\G\C{2}A", "0D0A4241", "" )]
	[InlineData( @"\C{6}\C{5}A", "0D0A4242424242424242424241", "1:12" )]
	[InlineData( @"\C{11}A", "0D0A4242424242424242424241", "1:12" )]
	[InlineData( @"(?>\C{11}A)", "0D0A4242424242424242424241", "1:12" )]
	[InlineData( @"\C{12}A", "0D0A424242424242424242424241", "" )]
	[InlineData( @"(?>\C{12}A)", "0D0A424242424242424242424241", "" )]
	[InlineData( @"\C{13}A", "0D0A42424242424242424242424241", "" )]
	[InlineData( @"\C{0}A", "0D0A41", "2:1" )]
	[InlineData( @"[^\r]\CA", "0D0A4141", "" )]
	[InlineData( @"(?>[^\r]\CA)", "0D0A4141", "" )]
	[InlineData( @"[^\r]{2}A", "0D0A4141", "" )]
	[InlineData( @"\p{L}\CA", "0D0A4141", "" )]
	[InlineData( @"\C{2}A(?!)", "0D0A4241", "" )]
	[InlineData( @"\C{2}A|Z", "0D0A5A5A", "2:1" )]
	[InlineData( @"\C{2}\n", "0D0A420A", "1:3" )]
	[InlineData( @"\C{2}\x00", "0D0A4200", "1:3" )]
	[InlineData( @"\C{2}\xFF", "0D0A42FF", "1:3" )]
	[InlineData( @"(?i)(\C{2})(a)", "0D0A4241", "1:3,1:2,3:1" )]
	public void FixedPrefixesAndAtomicWrappersHavePinnedCaptureSpans( string body, string hex, string spans ) {
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
	public void EveryCallerOffsetPreservesNativeByteModesAndCutoffs( string newline ) {
		// Longer restrictive prefixes have subject-dependent native optimizer entry (#99).
		// Their literal miss controls above do not imply every-offset parity.
		string[] bodies = { @"(?>\CA)", @"(?>(\C))(A)", @"(?<x>(?>[^\r]A))", @"(?>\p{Any}A)",
			@"\C{1}A", @"(\C{2})(A)", @"\C{2}?A", @"\C{2}+A", @"\C\CA", @"\C{6}\C{5}A",
			@"(?>\C{11}A)", @"\C{12}A", @"\C{13}A",
			@"\C{1,2}A", @"\C{2,3}A", @"\G\C{2}A", @"(?i)(\C{2})(a)" };
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
	public void UtfScalarPrefixesRetainPairSkippingAndFragmentBoundaries( string newline ) {
		foreach ( var body in new[] { @"(?>[^\r]A)", @"(?s)(?>.{2}A)", @"(?s)(.{11})A", @"(?>\p{Any}A)" } ) {
			var pattern = newline + body;
			var reference = Reference( pattern, true, true );
			var compiled = Compile( pattern, new() { Utf8 = true, Ucp = true, MatchInvalidUtf = true } );
			foreach ( var hex in new[] { "0D0A41", "0D0A4241", "C3A90D0A41", "FF0D0A4241", "0D0AFF41", "EDA0800D0A41" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"(?>[^\r]A)", "0D0A41" )]
	[InlineData( @"(?>\CA)", "0D0A41" )]
	[InlineData( @"(\C{2})(A)", "0D0A4241" )]
	[InlineData( @"\C{6}\C{5}A", "0D0A4242424242424242424241" )]
	public void RetryUsesSharedWorkBudgetAndCancellation( string body, string hex ) {
		var pattern = "(*ANYCRLF)" + body;
		var subject = Convert.FromHexString( hex );
		var steps = new List<int>();
		var result = Compile( pattern, new() { WorkObserver = steps.Add } ).Match( subject );
		Assert.True( result.IsSuccess );
		Assert.Equal( 1, result.Match?.ByteIndex );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		foreach ( var observe in new[] { false, true } ) {
			var exact = Compile( pattern, new() { MatchWorkLimit = steps.Count, WorkObserver = observe ? _ => { } : null } );
			Compare( Reference( pattern ), subject, 0, exact.Match( subject ), pattern );
			var exhausted = Compile( pattern, new() { MatchWorkLimit = steps.Count - 1, WorkObserver = observe ? _ => { } : null } ).Match( subject );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { WorkObserver = count => { if ( count == 3 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void DeepWrappersAndRepeatCeilingsRemainBounded() {
		var wrapped = "(*ANYCRLF)" + String.Concat( Enumerable.Repeat( "(?>", 200 ) ) + @"(\C{2})(A)" + new string( ')', 200 );
		var subject = "\r\nBA"u8.ToArray();
		Compare( Reference( wrapped ), subject, 0, Compile( wrapped ).Match( subject ), wrapped );
		var depth = String.Concat( Enumerable.Repeat( "(?>", 257 ) ) + @"\CA" + new string( ')', 257 );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( depth ) ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( @"\C{4096}A" ) ).Diagnostic?.Code );
	}

	private static string[] Subjects() => new[] { "", "00", "0D0A41", "0D0A4241", "0D0A4141", "FF0D0A4241",
		"0D0AFF41", "C3A90D0A4161", "0D0A410D0A4241", "0D0A4200", "0D0A4242424242424242424241",
		"0D0A424242424242424242424241", "0D0A42424242424242424242424241" };

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
