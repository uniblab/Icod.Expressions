using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins native candidate entry through omitted direct reference prefixes.</summary>
public sealed class PcreUtfPrunedPrefixReferenceTests {
	[Theory]
	[InlineData( @"\1{0}([\x09-\x0c]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}([\x09-\x0c]+)A", "0B0D0A41", 0, "" )]
	[InlineData( @"\1{0}([\x09-\x0c]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}+([\x09-\x0c]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}+([\x09-\x0c]+)A", "0B0D0A41", 0, "" )]
	[InlineData( @"\1{0}+([\x09-\x0c]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}([\x09-\x0c]+?)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}([\x09-\x0c]+?)A", "0B0D0A41", 0, "" )]
	[InlineData( @"\1{0}([\x09-\x0c]+?)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}+([\x09-\x0c]+?)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}+([\x09-\x0c]+?)A", "0B0D0A41", 0, "" )]
	[InlineData( @"\1{0}+([\x09-\x0c]+?)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}([\x09-\x0c]++)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}([\x09-\x0c]++)A", "0B0D0A41", 0, "" )]
	[InlineData( @"\1{0}([\x09-\x0c]++)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}+([\x09-\x0c]++)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}+([\x09-\x0c]++)A", "0B0D0A41", 0, "" )]
	[InlineData( @"\1{0}+([\x09-\x0c]++)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}+([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}+([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"\1{0}+([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+?)A", "C4800D0A41", 0, "" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}+([\x09-\x0c\x{100}]+?)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}+([\x09-\x0c\x{100}]+?)A", "C4800D0A41", 0, "" )]
	[InlineData( @"\1{0}+([\x09-\x0c\x{100}]+?)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]++)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]++)A", "C4800D0A41", 0, "" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}+([\x09-\x0c\x{100}]++)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}+([\x09-\x0c\x{100}]++)A", "C4800D0A41", 0, "" )]
	[InlineData( @"\1{0}+([\x09-\x0c\x{100}]++)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\g{+1}{0}([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\g{+1}{0}([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"\g{+1}{0}([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\k<x>{0}(?<x>[\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\k<x>{0}(?<x>[\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"\k<x>{0}(?<x>[\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}?([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}?([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"\1{0}?([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0,0}([\x09-\x0c\x{100}]+)A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0,0}([\x09-\x0c\x{100}]+)A", "C4800D0A41", 0, "" )]
	[InlineData( @"\1{0,0}([\x09-\x0c\x{100}]+)A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"(?:\1{0}([\x09-\x0c\x{100}]+))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?:\1{0}([\x09-\x0c\x{100}]+))A", "C4800D0A41", 0, "" )]
	[InlineData( @"(?:\1{0}([\x09-\x0c\x{100}]+))A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"(\2{0}([\x09-\x0c\x{100}]+))A", "0D0A41", 0, "1:2,1:1,1:1" )]
	[InlineData( @"(\2{0}([\x09-\x0c\x{100}]+))A", "C4800D0A41", 0, "" )]
	[InlineData( @"(\2{0}([\x09-\x0c\x{100}]+))A", "0B0D0A0B41", 0, "3:2,3:1,3:1" )]
	[InlineData( @"\1{0}\2{0}([\x09-\x0c\x{100}]+)(A)", "0D0A41", 0, "1:2,1:1,2:1" )]
	[InlineData( @"\1{0}\2{0}([\x09-\x0c\x{100}]+)(A)", "C4800D0A41", 0, "" )]
	[InlineData( @"\1{0}\2{0}([\x09-\x0c\x{100}]+)(A)", "0B0D0A0B41", 0, "3:2,3:1,4:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)A\1?", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)A\1?", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)A\1?", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)(?(1)A|B)", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)(?(1)A|B)", "C4800D0A41", 0, "3:2,3:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)(?(1)A|B)", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?J)\k<x>{0}(?<x>[\x09-\x0c\x{100}]+)(?<x>A)", "0D0A41", 0, "1:2,1:1,2:1" )]
	[InlineData( @"(?J)\k<x>{0}(?<x>[\x09-\x0c\x{100}]+)(?<x>A)", "C4800D0A41", 0, "" )]
	[InlineData( @"(?J)\k<x>{0}(?<x>[\x09-\x0c\x{100}]+)(?<x>A)", "0B0D0A0B41", 0, "3:2,3:1,4:1" )]
	[InlineData( @"\1{0}(?i:([\x09-\x0c\x{100}]+)a)", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"\1{0}(?i:([\x09-\x0c\x{100}]+)a)", "C4800D0A41", 0, "" )]
	[InlineData( @"\1{0}(?i:([\x09-\x0c\x{100}]+)a)", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"(?:(?:(?:\1{0}([\x09-\x0c\x{100}]+))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?:(?:(?:\1{0}([\x09-\x0c\x{100}]+))))A", "0B0D0A0B41", 0, "3:2,3:1" )]
	[InlineData( @"(?:(?:(?:(?:\1{0}([\x09-\x0c\x{100}]+)))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?:(?:(?:(?:\1{0}([\x09-\x0c\x{100}]+)))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:\1{0}([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:\1{0}([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:\1{0}([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:(?:\1{0}([\x09-\x0c\x{100}]+))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))))A", "0B0D0A0B41", 0, "2:3,2:2" )]
	[InlineData( @"(\1{0})", "0D0A41", 0, "0:0,0:0" )]
	[InlineData( @"(?:\1){0}([\x09-\x0c\x{100}]+)A", "0B41", 0, "0:2,0:1" )]
	[InlineData( @"(?:\1{0}){0}([\x09-\x0c\x{100}]+)A", "0B41", 0, "0:2,0:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)A(?1)?", "0B41", 0, "0:2,0:1" )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)A(?(DEFINE)\1)", "0B41", 0, "0:2,0:1" )]
	[InlineData( @"\1{0}([\x{100}-\x{10ffff}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"\1{0}([\x09-\x0e\x{100}]+)A", "0D0A41", 0, "0:3,0:2" )]
	[InlineData( @"\1{0}(\G[\x09-\x0c\x{100}]+)A", "0D0A41", 0, "" )]
	[InlineData( @"\1{0}(\G[\x09-\x0c\x{100}]+)A", "0D0A41", 1, "1:2,1:1" )]
	public void PrunedPrefixesPreservePinnedNativeCaptureSpans( string body, string hex, int offset, string spans ) {
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
	public void EveryCallerOffsetKeepsPrunedPrefixAndFragmentBoundaries( string newline ) {
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
	public void StrictPrunedPrefixesRetainUtfValidationAndCallerBoundaries( bool ucp ) {
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
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)A", "0D0A41", 1 )]
	[InlineData( @"\1{0}([\x09-\x0c]+)A", "0B0D0A41", -1 )]
	[InlineData( @"\1{0}([\x09-\x0c\x{100}]+)A\1?", "C4800D0A41", 3 )]
	[InlineData( @"(?:(?:(?:(?:\1{0}([\x09-\x0c\x{100}]+)))))A", "0B0D0A0B41", 2 )]
	public void PrunedPrefixEntrySharesExactExhaustedAndObservedWork( string body, string hex, int index ) {
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
	public void PrunedPrefixClassesWithoutLfRetainTheDefaultBudget() {
		var pattern = String.Concat( "(*ANYCRLF)", @"\1{0}([\x{100}-\x{10ffff}]+)A" );
		var subject = Encoding.ASCII.GetBytes( String.Concat( Enumerable.Repeat( "\r\n", 40_000 ) ) );
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
	public void PrunedPrefixEntryRetainsPreAndMidMatchCancellation() {
		var pattern = String.Concat( "(*ANYCRLF)", @"\1{0}([\x09-\x0c\x{100}]+)A" );
		var subject = Convert.FromHexString( "0D0A41" );
		var steps = new List<int>();
		var result = Compile( pattern, new() { Utf8 = true, WorkObserver = steps.Add } ).Match( subject );
		Assert.Equal( 1, result.Match?.ByteIndex );
		Compare( Reference( pattern ), subject, 0, result, pattern );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( pattern, new() { Utf8 = true,
			WorkObserver = count => { if ( count == steps.Count / 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void PrunedPrefixEntryDoesNotRaiseTheParserDepthCeiling() {
		var pattern = String.Concat( new string( '(', 200 ), String.Concat( Enumerable.Repeat( "(?:", 57 ) ),
			@"[\x09-\x0c\x{100}]+", new string( ')', 257 ), "A" );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new() { Utf8 = true } ).Diagnostic?.Code );
	}

	private static string[] Bodies() => new[] {
		@"\1{0}([\x09-\x0c]+)A",
		@"\1{0}([\x09-\x0c\x{100}]+)A",
		@"\1{0}([\x09-\x0c\x{100}]+?)A",
		@"\1{0}([\x09-\x0c\x{100}]++)A",
		@"\1{0}+([\x09-\x0c\x{100}]+)A",
		@"\g{+1}{0}([\x09-\x0c\x{100}]+)A",
		@"\k<x>{0}(?<x>[\x09-\x0c\x{100}]+)A",
		@"\1{0}?([\x09-\x0c\x{100}]+)A",
		@"\1{0,0}([\x09-\x0c\x{100}]+)A",
		@"(?:\1{0}([\x09-\x0c\x{100}]+))A",
		@"(\2{0}([\x09-\x0c\x{100}]+))A",
		@"\1{0}\2{0}([\x09-\x0c\x{100}]+)(A)",
		@"\1{0}([\x09-\x0c\x{100}]+)A\1?",
		@"\1{0}([\x09-\x0c\x{100}]+)(?(1)A|B)",
		@"(?J)\k<x>{0}(?<x>[\x09-\x0c\x{100}]+)(?<x>A)",
		@"(?:(?:(?:(?:\1{0}([\x09-\x0c\x{100}]+)))))A"
	};
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
