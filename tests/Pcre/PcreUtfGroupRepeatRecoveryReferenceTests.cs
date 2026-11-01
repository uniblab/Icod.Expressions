using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins the native four-level noncapture descent for leading UTF class PLUS.</summary>
public sealed class PcreUtfGroupRepeatRecoveryReferenceTests {
	[Theory]
	[InlineData( @"(?:[\x09-\x0c]+)A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:[\x09-\x0c]+)A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:[\x09-\x0c]+?)A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:[\x09-\x0c]+?)A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:[\x09-\x0c]++)A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:[\x09-\x0c]++)A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c]+))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c]+))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c]+?))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c]+?))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c]++))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c]++))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c]+)))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c]+)))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c]+?)))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c]+?)))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c]++)))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c]++)))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c]+))))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c]+))))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c]+?))))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c]+?))))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c]++))))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c]++))))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:[\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:[\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:[\x09-\x0c\x{100}]+?)A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:[\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:[\x09-\x0c\x{100}]++)A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:[\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c\x{100}]+))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c\x{100}]+))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c\x{100}]+?))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c\x{100}]+?))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c\x{100}]++))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c\x{100}]++))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c\x{100}]+)))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c\x{100}]+)))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c\x{100}]+?)))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c\x{100}]+?)))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c\x{100}]++)))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c\x{100}]++)))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c\x{100}]+))))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c\x{100}]+))))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c\x{100}]+?))))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c\x{100}]+?))))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c\x{100}]++))))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c\x{100}]++))))A", "0B0D0A0B41", 0, "3:2" )]
	[InlineData( @"(?:[\x09-\x0c]{1,})A", "0B0D0A41", 0, "" )]
	[InlineData( @"(?:[\x09-\x0c\x{100}]{1,})A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c]{1,}?))))A", "0B0D0A41", 0, "" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c\x{100}]{1,}+))))A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c]+A))))", "0B0D0A41", 0, "" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c\x{100}]+A))))", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:[\x09-\x0c]+(A)))", "0B0D0A0B41", 0, "3:2,4:1" )]
	[InlineData( @"(?:(?:(?:[\x09-\x0c\x{100}]+)))(A)", "0D0A41", 0, "1:2,2:1" )]
	[InlineData( @"(?i:[\x09-\x0c]+a)", "0B0D0A41", 0, "" )]
	[InlineData( @"(?i:[\x09-\x0c\x{100}]+a)", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c]+))))A", "C4800D0A41", 0, "3:2" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c\x{100}]+))))A", "C4800D0A41", 0, "" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c\x{100}]+))))A", "C4800D0A0B41", 0, "4:2" )]
	[InlineData( @"(?:[\x09-\x0c]+)A", "0B0D0A41", 2, "2:2" )]
	[InlineData( @"(?>[\x09-\x0c]+)A", "0B0D0A41", 0, "2:2" )]
	[InlineData( @"(?:(?>[\x09-\x0c]+)A)", "0B0D0A41", 0, "2:2" )]
	[InlineData( @"(?:[\x09-\x0c]+A|[\x09-\x0c]+B)", "0B0D0A41", 0, "2:2" )]
	[InlineData( @"(?:)", "0D0A41", 0, "0:0" )]
	[InlineData( @"(?:(?:(?:(?:[\x09-\x0c\n]+))))A", "0B0D0A41", 0, "2:2" )]
	public void GroupedRepeatsPreservePinnedNativeCaptureSpans( string body, string hex, int offset, string spans ) {
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
	public void EveryCallerOffsetRetainsGroupAndFragmentBoundaries( string newline ) {
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
	public void StrictGroupedRepeatsRetainUtfValidationAndCallerBoundaries( bool ucp ) {
		foreach ( var newline in new[] { "(*LF)", "(*ANYCRLF)" } ) {
			foreach ( var body in Bodies() ) {
				var pattern = String.Concat( newline, body );
				var reference = Reference( pattern, ucp, false );
				var compiled = Compile( pattern, new() { Utf8 = true, Ucp = ucp } );
				foreach ( var hex in new[] { "0D0A41", "0B0D0A0B41", "C4800D0A41", "C4800D0A0B41" } ) {
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
	[InlineData( 5 )]
	[InlineData( 6 )]
	[InlineData( 200 )]
	public void GroupsBeyondNativeAccelerationKeepLfCandidateEntry( int depth ) {
		var pattern = String.Concat( "(*ANYCRLF)", Wrap( @"[\x09-\x0c]+", depth ), "A" );
		var subject = Convert.FromHexString( "0B0D0A41" );
		foreach ( var tolerant in new[] { false, true } ) {
			var reference = Reference( pattern, false, tolerant );
			var expected = reference.Match( subject );
			Assert.True( expected.Success );
			Assert.Equal( 2, expected.Index );
			Assert.Equal( 2, expected.Length );
			Compare( reference, subject, 0, Compile( pattern, new() { Utf8 = true, MatchInvalidUtf = tolerant } ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( @"[\x09-\x0c]+", 1, "0B0D0A41", -1 )]
	[InlineData( @"[\x09-\x0c]+?", 4, "0B0D0A0B41", 3 )]
	[InlineData( @"[\x09-\x0c]++", 4, "0B0D0A41", -1 )]
	[InlineData( @"[\x09-\x0c\x{100}]+", 1, "0D0A41", 1 )]
	[InlineData( @"[\x09-\x0c\x{100}]{1,}?", 4, "0D0A41", 1 )]
	[InlineData( @"[\x09-\x0c\x{100}]++", 4, "C4800D0A0B41", 4 )]
	public void GroupRecoverySharesExactExhaustedAndObservedWork( string head, int depth, string hex, int index ) {
		var pattern = String.Concat( "(*ANYCRLF)", Wrap( head, depth ), "A" );
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
	public void GroupedClassesWithoutLfRetainTheDefaultBudget() {
		var pattern = String.Concat( "(*ANYCRLF)", Wrap( "[A-C]+", 4 ), "A" );
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
	public void GroupRecoveryRetainsPreAndMidMatchCancellation() {
		var pattern = String.Concat( "(*ANYCRLF)", Wrap( @"[\x09-\x0c]+", 4 ), "A" );
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

	private static string Wrap( string body, int depth ) =>
		String.Concat( String.Concat( Enumerable.Repeat( "(?:", depth ) ), body, new string( ')', depth ) );
	private static string[] Bodies() => new[] { @"(?:[\x09-\x0c]+)A", @"(?:(?:[\x09-\x0c]+))A", @"(?:(?:(?:[\x09-\x0c]+)))A", @"(?:(?:(?:(?:[\x09-\x0c]+))))A", @"(?:[\x09-\x0c\x{100}]+)A", @"(?:(?:[\x09-\x0c\x{100}]+))A", @"(?:(?:(?:[\x09-\x0c\x{100}]+)))A", @"(?:(?:(?:(?:[\x09-\x0c\x{100}]+))))A", @"(?:(?:(?:(?:[\x09-\x0c]+?))))A", @"(?:(?:(?:(?:[\x09-\x0c]++))))A", @"(?:(?:(?:(?:[\x09-\x0c\x{100}]+?))))A", @"(?:(?:(?:(?:[\x09-\x0c\x{100}]++))))A", @"(?:(?:(?:(?:[\x09-\x0c]+A))))", @"(?:(?:(?:(?:[\x09-\x0c\x{100}]+A))))", @"(?:(?:[\x09-\x0c]+(A)))", @"(?:(?:(?:[\x09-\x0c\x{100}]+)))(A)", @"(?:(?:(?:(?:(?:[\x09-\x0c]+)))))A", @"(?>[\x09-\x0c]+)A", @"(?:[\x09-\x0c]+A|[\x09-\x0c]+B)", @"(?:(?:(?:(?:[\x09-\x0c\n]+))))A" };
	private static string[] Subjects() => new[] { "", "00", "0D0A41", "0B0D0A41", "0B0D0A0B41", "C4800D0A41", "C4800D0A0B41", "C4800B0D0A41", "FF0B0D0A41", "0BFF0D0A41", "0B0D0AFF41", "EDA0800B0D0A41", "0B410D0A41", "0B0D0A41000D0A41" };

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
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
