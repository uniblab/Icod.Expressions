using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins wide UTF leading classes without changing CR/LF source metadata.</summary>
public sealed class PcreUtfWideStartClassReferenceTests {
	[Theory]
	[InlineData( @"[\x09-\x0c\x{80}]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{ff}]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{100}]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{7ff}]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{800}]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{ffff}]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{10000}]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{10ffff}]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{100}-\x{10ffff}]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x{100}-\x{10ffff}\x09-\x0c]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{100}\x{10000}]AA", "0D0A4141", 0, "1:3" )]
	[InlineData( @"(?<x>[\x09-\x0c\x{100}])(A)", "0D0A41", 0, "1:2,1:1,2:1" )]
	[InlineData( @"(?<x>[\x09-\x0c\x{100}]A)", "0D0A41", 0, "1:2,1:2" )]
	[InlineData( @"(?:[\x09-\x0c\x{100}])(?:A)", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?>[\x09-\x0c\x{100}]A)", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{100}]+A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{100}]+?A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{100}]++A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{100}]{1}A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{100}]{1,2}A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c\x{100}]{2,3}A", "0D0A0B41", 0, "1:3" )]
	[InlineData( @"(?:[\x09-\x0c\x{100}]A)+", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:[\x09-\x0c\x{100}]A|[A-C]B)", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:[\x09-\x0c\x{100}]A|[\x09-\x0b\x{10000}]B)", "0D0A42", 0, "1:2" )]
	[InlineData( @"(?|([\x09-\x0c\x{100}])A|([\x09-\x0c\x{100}])B)", "0D0A42", 0, "1:2,1:1" )]
	[InlineData( @"(?i)[\x09-\x0c\x{100}]a", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?i)[\x09-\x0c\x{212a}]a", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?ir)[\x09-\x0c\x{17f}]a", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?i)[\x09-\x0c\x{df}\x{3c2}\x{10400}]a", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?xx)[ \x09 - \x0c \x{100} ] A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?<x>[\x09-\x0c\x{100}])(A)", "C48041", 0, "0:3,0:2,2:1" )]
	[InlineData( @"(?<x>[\x09-\x0c\x{10000}])(A)", "F090808041", 0, "0:5,0:4,4:1" )]
	[InlineData( @"(?<x>[\x09-\x0c\x{10ffff}])(A)", "F48FBFBF41", 0, "0:5,0:4,4:1" )]
	[InlineData( @"(?<x>[\x09-\x0c\x{100}])(A)", "C3A90D0A41", 0, "3:2,3:1,4:1" )]
	[InlineData( @"[\x09-\x0e\x{100}]A", "0D0A41", 0, "" )]
	[InlineData( @"[\x00-\x{10ffff}]A", "0D0A41", 0, "" )]
	[InlineData( @"[\x{80}-\x{100}]A", "0D0A41", 0, "" )]
	[InlineData( @"[\x{100}-\x{10ffff}]A", "0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c\x{100}\p{L}]A", "0D0A41", 0, "" )]
	[InlineData( @"[^\r]A", "0D0A41", 0, "" )]
	[InlineData( @"\G[\x09-\x0c\x{100}]A", "0D0A41", 0, "" )]
	[InlineData( @"\G[\x09-\x0c\x{100}]A", "0D0A41", 1, "1:2" )]
	[InlineData( @"(?m)^[\x09-\x0c\x{100}]A", "0D0A41", 0, "" )]
	[InlineData( @"[\n-\r\x{100}]A", "0D0A41", 0, "1:2" )]
	public void WideLeadingClassesPreservePinnedNativeCaptureSpans( string body, string hex, int offset, string spans ) {
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
	public void EveryCallerOffsetPreservesWideClassesAndFragmentBarriers( string newline ) {
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
	public void StrictWideClassSearchKeepsCallerBoundaryAndValidationErrors( bool ucp ) {
		foreach ( var newline in new[] { "(*LF)", "(*ANYCRLF)" } ) {
			foreach ( var body in Bodies() ) {
				var pattern = String.Concat( newline, body );
				var reference = Reference( pattern, ucp, false );
				var compiled = Compile( pattern, new() { Utf8 = true, Ucp = ucp } );
				foreach ( var hex in new[] { "0D0A41", "C4800D0A41", "0D0AC48041", "F09080800D0A41" } ) {
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
					compiled.Match( Convert.FromHexString( "FF0D0A41" ) ).Diagnostic?.Code );
			}
		}
	}

	[Theory]
	[InlineData( @"[\x09-\x0c\x{100}]A", "0D0A41" )]
	[InlineData( @"(?<x>[\x09-\x0c\x{100}])(A)", "0D0A41" )]
	[InlineData( @"[\x09-\x0c\x{100}-\x{10ffff}]{2,3}A", "0D0A0B41" )]
	public void WideClassLfEntrySharesObservedExactAndExhaustedBudgets( string body, string hex ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		var steps = new List<int>();
		var result = Compile( pattern, new() { Utf8 = true, WorkObserver = steps.Add } ).Match( subject );
		Assert.True( result.IsSuccess );
		Assert.Equal( 1, result.Match?.ByteIndex );
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

	[Theory]
	[InlineData( @"[\x{80}-\x{100}]A" )]
	[InlineData( @"[\x{100}-\x{10ffff}]A" )]
	public void WideClassesWithoutLfRetainDefaultBudgetOnLongCrLfInput( string body ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Encoding.ASCII.GetBytes( String.Concat( Enumerable.Repeat( "\r\n", 120_000 ) ) );
		Assert.False( Reference( pattern ).Match( subject ).Success );
		foreach ( var observed in new[] { false, true } ) {
			var work = 0;
			var result = Compile( pattern, new() { Utf8 = true, WorkObserver = observed ? count => { work = count; } : null } ).Match( subject );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.Null( result.Match );
			if ( observed ) { Assert.InRange( work, 1, PcreByteOptions.DefaultMatchWorkLimit ); }
		}
	}

	[Theory]
	[InlineData( 34, true )]
	[InlineData( 220, false )]
	public void WideClassAlternativesRetainCumulativeNativeProofLimit( int depth, bool matches ) {
		var branch = String.Concat( String.Concat( Enumerable.Repeat( "(?:", depth ) ), @"[\x09-\x0c\x{100}]A", new string( ')', depth ) );
		var pattern = String.Concat( "(*ANYCRLF)", String.Join( "|", Enumerable.Repeat( branch, 5 ) ) );
		var subject = "\r\nA"u8.ToArray();
		foreach ( var tolerant in new[] { false, true } ) {
			var reference = Reference( pattern, false, tolerant );
			var expected = reference.Match( subject );
			Assert.Equal( matches, expected.Success );
			if ( matches ) { Assert.Equal( 1, expected.Index ); Assert.Equal( 2, expected.Length ); }
			Compare( reference, subject, 0, Compile( pattern, new() { Utf8 = true, MatchInvalidUtf = tolerant } ).Match( subject ), pattern );
		}
	}

	[Fact]
	public void WideClassLfEntryKeepsSharedMidAndPreCancellation() {
		const string pattern = @"(*ANYCRLF)(?<x>[\x09-\x0c\x{100}])A";
		var subject = "\r\nA"u8.ToArray();
		Assert.Equal( 1, Compile( pattern, new() { Utf8 = true } ).Match( subject ).Match?.ByteIndex );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new() { Utf8 = true,
			WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	private static string[] Bodies() => new[] { @"[\x09-\x0c\x{100}]A", @"[\x09-\x0c\x{80}]A", @"[\x09-\x0c\x{10ffff}]A", @"[\x09-\x0c\x{100}-\x{10ffff}]A", @"(?<x>[\x09-\x0c\x{100}])(A)", @"(?>[\x09-\x0c\x{100}]A)", @"[\x09-\x0c\x{100}]+A", @"[\x09-\x0c\x{100}]{1}A", @"[\x09-\x0c\x{100}]{2,3}A", @"(?:[\x09-\x0c\x{100}]A|[A-C]B)", @"(?i)[\x09-\x0c\x{212a}]a", @"(?ir)[\x09-\x0c\x{17f}]a", @"[\x09-\x0e\x{100}]A", @"[\x{100}-\x{10ffff}]A" };
	private static string[] Subjects() => new[] { "", "00", "0D0A", "0D0A41", "0D0A0B41", "0D0A4141", "FF0D0A41", "C4800D0A41", "0D0AC48041", "0D0AFF41", "EDA0800D0A41", "F090808041", "0D0A410D0A41" };

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
