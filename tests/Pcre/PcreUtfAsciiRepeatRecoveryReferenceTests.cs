using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins direct ASCII UTF class PLUS recovery at a failed run's CRLF endpoint.</summary>
public sealed class PcreUtfAsciiRepeatRecoveryReferenceTests {
	[Theory]
	[InlineData( @"[\x09-\x0c]+A", "0B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]+A", "0B0D0A41", 1, "2:2" )]
	[InlineData( @"[\x09-\x0c]+A", "0B0D0A41", 2, "2:2" )]
	[InlineData( @"[\x09-\x0c]+A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c]+A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"[\x09-\x0c]+A", "000B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]+A", "C4800D0A41", 0, "3:2" )]
	[InlineData( @"[\x09-\x0c]+?A", "0B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]+?A", "0B0D0A41", 1, "2:2" )]
	[InlineData( @"[\x09-\x0c]+?A", "0B0D0A41", 2, "2:2" )]
	[InlineData( @"[\x09-\x0c]+?A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c]+?A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"[\x09-\x0c]+?A", "000B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]+?A", "C4800D0A41", 0, "3:2" )]
	[InlineData( @"[\x09-\x0c]++A", "0B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]++A", "0B0D0A41", 1, "2:2" )]
	[InlineData( @"[\x09-\x0c]++A", "0B0D0A41", 2, "2:2" )]
	[InlineData( @"[\x09-\x0c]++A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c]++A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"[\x09-\x0c]++A", "000B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]++A", "C4800D0A41", 0, "3:2" )]
	[InlineData( @"[\x09-\x0c]{1,}A", "0B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]{1,}A", "0B0D0A41", 1, "2:2" )]
	[InlineData( @"[\x09-\x0c]{1,}A", "0B0D0A41", 2, "2:2" )]
	[InlineData( @"[\x09-\x0c]{1,}A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c]{1,}A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"[\x09-\x0c]{1,}A", "000B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]{1,}A", "C4800D0A41", 0, "3:2" )]
	[InlineData( @"[\x09-\x0c]{1,}?A", "0B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]{1,}?A", "0B0D0A41", 1, "2:2" )]
	[InlineData( @"[\x09-\x0c]{1,}?A", "0B0D0A41", 2, "2:2" )]
	[InlineData( @"[\x09-\x0c]{1,}?A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c]{1,}?A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"[\x09-\x0c]{1,}?A", "000B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]{1,}?A", "C4800D0A41", 0, "3:2" )]
	[InlineData( @"[\x09-\x0c]{1,}+A", "0B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]{1,}+A", "0B0D0A41", 1, "2:2" )]
	[InlineData( @"[\x09-\x0c]{1,}+A", "0B0D0A41", 2, "2:2" )]
	[InlineData( @"[\x09-\x0c]{1,}+A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c]{1,}+A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"[\x09-\x0c]{1,}+A", "000B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]{1,}+A", "C4800D0A41", 0, "3:2" )]
	[InlineData( @"[\x09-\x0b]+A", "0B0D0A41", 0, "" )]
	[InlineData( @"[\x07-\x0c]+A", "0B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0cA]+A", "0B0D0A4141", 0, "3:2" )]
	[InlineData( @"[\x09-\x0cA]+A", "0B410D0A41", 0, "0:2" )]
	[InlineData( @"(?i)[\x09-\x0cB]+a", "0B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]+(A)", "0B0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c]+(A)", "0B0D0A41", 2, "2:2,3:1" )]
	[InlineData( @"[\x09-\x0c]+(A)", "0B0D0A0B41", 0, "3:2,4:1" )]
	[InlineData( @"[\x09-\x0c]{1,2}A", "0B0D0A41", 0, "2:2" )]
	[InlineData( @"[\x09-\x0c]{2,}A", "0B0D0A0B41", 0, "2:3" )]
	[InlineData( @"[\x09-\x0c]{1}A", "0B0D0A41", 0, "2:2" )]
	[InlineData( @"[\x09-\x0c]A", "0B0D0A41", 0, "2:2" )]
	[InlineData( @"(?>[\x09-\x0c]+)A", "0B0D0A41", 0, "2:2" )]
	[InlineData( @"(?:[\x09-\x0c]A)+", "0B0D0A41", 0, "2:2" )]
	[InlineData( @"(?:[\x09-\x0c]+A|[\x09-\x0c]+B)", "0B0D0A41", 0, "2:2" )]
	[InlineData( @"[\x09-\x0c]+A|[\x09-\x0c]+B", "0B0D0A41", 0, "2:2" )]
	[InlineData( @"[\x09-\x0c\n]+A", "0B0D0A41", 0, "2:2" )]
	[InlineData( @"[\x09-\x0e]+A", "0B0D0A41", 0, "0:4" )]
	[InlineData( @"[A-C]+A", "0B0D0A41", 0, "" )]
	[InlineData( @"\G[\x09-\x0c]+A", "0B0D0A41", 0, "" )]
	[InlineData( @"\G[\x09-\x0c]+A", "0B0D0A41", 2, "2:2" )]
	[InlineData( @"[\x09-\x0c]+", "0B0D0A41", 0, "0:1" )]
	[InlineData( @"[\x09-\x0c\x{100}]+A", "C4800D0A41", 0, "" )]
	public void RootAsciiRepeatsPreservePinnedNativeCaptureSpans( string body, string hex, int offset, string spans ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var tolerant in new[] { false, true } ) {
				var reference = Reference( pattern, ucp, tolerant );
				var expected = reference.Match( subject, offset );
				Assert.Equal( spans.Length > 0, expected.Success );
				if ( expected.Success ) {
					var groups = spans.Split( ',' );
					Assert.Equal( groups.Length, expected.Groups.Count );
					for ( var group = 0; group < groups.Length; group++ ) {
						var capture = expected.Groups[group];
						Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
					}
				}
				Compare( reference, subject, offset, Compile( pattern, new() { Utf8 = true, Ucp = ucp,
					MatchInvalidUtf = tolerant } ).Match( subject, offset ), pattern );
			}
		}
	}

	[Theory]
	[InlineData( "(*LF)" )]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CR)" )]
	[InlineData( "(*NUL)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void EveryCallerOffsetKeepsRootRepeatAndFragmentBoundaries( string newline ) {
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var body in Bodies() ) {
				var pattern = String.Concat( newline, body );
				var reference = Reference( pattern, ucp, true );
				var compiled = Compile( pattern, new() { Utf8 = true, Ucp = ucp, MatchInvalidUtf = true } );
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
	[InlineData( false )]
	[InlineData( true )]
	public void StrictRootRepeatSearchRetainsUtfValidationAndCallerBoundaries( bool ucp ) {
		foreach ( var newline in new[] { "(*LF)", "(*ANYCRLF)" } ) {
			foreach ( var body in Bodies() ) {
				var pattern = String.Concat( newline, body );
				var reference = Reference( pattern, ucp, false );
				var compiled = Compile( pattern, new() { Utf8 = true, Ucp = ucp } );
				foreach ( var hex in new[] { "0B0D0A41", "0B0D0A0B41", "C4800D0A41", "C4800B0D0A41" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						if ( offset < subject.Length && ( subject[offset] & 0xC0 ) == 0x80 ) {
							Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( subject, offset ).Diagnostic?.Code );
						} else {
							Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}" );
						}
					}
				}
				Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject,
					compiled.Match( Convert.FromHexString( "FF0B0D0A41" ) ).Diagnostic?.Code );
			}
		}
	}

	[Theory]
	[InlineData( @"[\x09-\x0c]+A", "0B0D0A41", -1 )]
	[InlineData( @"[\x09-\x0c]+?A", "0B0D0A41", -1 )]
	[InlineData( @"[\x09-\x0c]++A", "0B0D0A41", -1 )]
	[InlineData( @"[\x09-\x0c]{1,}A", "0B0D0A0B41", 3 )]
	[InlineData( @"[\x09-\x0c]{1,}?A", "0B0D0A0B41", 3 )]
	[InlineData( @"[\x09-\x0c]{1,}+A", "0B0D0A0B41", 3 )]
	public void RootRepeatRecoverySharesExactExhaustedAndObservedWork( string body, string hex, int index ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		var steps = new List<int>();
		var result = Compile( pattern, new() { Utf8 = true, WorkObserver = steps.Add } ).Match( subject );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( index, result.Match?.ByteIndex ?? -1 );
		Compare( Reference( pattern ), subject, 0, result, pattern );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		foreach ( var observed in new[] { false, true } ) {
			Compare( Reference( pattern ), subject, 0, Compile( pattern, new() { Utf8 = true,
				MatchWorkLimit = steps.Count, WorkObserver = observed ? _ => { } : null } ).Match( subject ), pattern );
			var exhausted = Compile( pattern, new() { Utf8 = true, MatchWorkLimit = steps.Count - 1,
				WorkObserver = observed ? _ => { } : null } ).Match( subject );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
	}

	[Fact]
	public void ClassesWithoutLfRetainTheDefaultBudgetOnLongCrLfInput() {
		const string pattern = "(*ANYCRLF)[A-C]+A";
		var subject = Encoding.ASCII.GetBytes( String.Concat( Enumerable.Repeat( "\r\n", 120_000 ) ) );
		Assert.False( Reference( pattern ).Match( subject ).Success );
		foreach ( var observed in new[] { false, true } ) {
			var work = 0;
			var result = Compile( pattern, new() { Utf8 = true,
				WorkObserver = observed ? count => { work = count; } : null } ).Match( subject );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.Null( result.Match );
			if ( observed ) { Assert.InRange( work, 1, PcreByteOptions.DefaultMatchWorkLimit ); }
		}
	}

	[Fact]
	public void RootRepeatRecoveryRetainsPreAndMidMatchCancellation() {
		const string pattern = @"(*ANYCRLF)[\x09-\x0c]+A";
		var subject = Convert.FromHexString( "0B0D0A0B41" );
		var steps = new List<int>();
		var result = Compile( pattern, new() { Utf8 = true, WorkObserver = steps.Add } ).Match( subject );
		Assert.Equal( 3, result.Match?.ByteIndex );
		Compare( Reference( pattern ), subject, 0, result, pattern );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new() { Utf8 = true,
			WorkObserver = count => { if ( count == steps.Count / 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	private static string[] Bodies() => new[] { @"[\x09-\x0c]+A", @"[\x09-\x0c]+?A", @"[\x09-\x0c]++A", @"[\x09-\x0c]{1,}A", @"[\x09-\x0c]{1,}?A", @"[\x09-\x0c]{1,}+A", @"[\x09-\x0cA]+A", @"[\x09-\x0c]+(A)", @"[\x09-\x0c]{1,2}A", @"[\x09-\x0c]{2,}A", @"[\x09-\x0c]A", @"(?>[\x09-\x0c]+)A", @"(?:[\x09-\x0c]A)+", @"(?:[\x09-\x0c]+A|[\x09-\x0c]+B)", @"[\x09-\x0c]+A|[\x09-\x0c]+B", @"[\x09-\x0c\n]+A", @"[\x09-\x0e]+A", @"[\x09-\x0c\x{100}]+A" };
	private static string[] Subjects() => new[] { "", "00", "0D0A41", "0B0D0A41", "0B0D0A0B41", "0B0D0A4141", "0A0B0D0A41", "000B0D0A41", "C4800D0A41", "C4800B0D0A41", "FF0B0D0A41", "0BFF0D0A41", "0B0D0AFF41", "EDA0800B0D0A41", "0B410D0A41", "0B0D0A41000D0A41" };

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

	private static PcreRegex8Bit Reference( string pattern, bool ucp = false, bool tolerant = false ) => new(
		Encoding.ASCII.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | PcreOptions.Utf | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ) } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
