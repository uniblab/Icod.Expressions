using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordNewlineProfileTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "(*LF)^a$", "a\n", 0, false, false ),
				( "(*CR)^a$", "a\r", 0, true, false ),
				( "(*CR)^a$", "a\n", 0, false, false ),
				( @"(*CR)\Aa\Z", "a\r", 0, true, false ),
				( @"(*CR)\Aa\z", "a\r", 0, false, false ),
				( "(*CR)(?m)^a$", "x\ra\ry", 0, true, false ),
				( "(*CR)(?m)^", "\r", 1, false, false ),
				( "(*CR)(?m)^", "\r\n", 0, true, false ),
				( "(*CR)(?m)$", "\r\n", 0, false, false ),
				( "(*CR).", "\r\n", 0, true, false ),
				( "(*CR)(?s).", "\r", 0, false, false ),
				( @"(*CR)(?s)\N", "\r\n", 0, true, false ),
				( "(*CR)[^a]", "\r", 0, false, false ),
				( "(*NUL)^a$", "a\0", 0, true, false ),
				( "(*NUL)^a$", "a\n", 0, false, false ),
				( @"(*NUL)\Aa\Z", "a\0", 0, true, false ),
				( "(*NUL)(?m)^a$", "x\0a\0y", 0, false, false ),
				( "(*NUL).", "\0\n", 0, true, false ),
				( @"(*NUL)(?s)\N", "\0\n", 0, false, false ),
				( "(*NUL)(?m)^", "\0", 1, true, false ),
				( "(*NUL)(?m)$", "a\0b", 0, false, false ),
				( "(*CR)^a*b?c*d?e*f?g*$", "aaabccdeefgg\r", 0, true, false ),
				( "(*NUL)^a*?b??c*?d??e*?f??g*?$", "aaabccdeefgg\0", 0, false, false ),
				( "(*CR)(?m)^😀+$", "x\r😀😀\r", 0, true, false ),
				( @"(*NUL)(?m)^\w+$", "!\0α𐐀\0", 0, true, true ),
				( @"(*BSR_ANYCRLF)\R", "\r\n", 0, false, false ),
				( @"(*BSR_ANYCRLF)\R", "\u2028", 0, true, false ),
				( @"(*BSR_UNICODE)\R", "\u2028", 0, false, false ),
				( @"(*CR)(*BSR_ANYCRLF)\R+X", "\r\nX", 0, true, false ),
				( @"(*NUL)(*BSR_ANYCRLF)\R", "\0", 0, false, false ),
				( @"(*NUL)(*BSR_UNICODE)\R", "\u2029", 0, true, false ),
				( @"(*CR)(*BSR_ANYCRLF)\A\R*\r\n\z", "\r\n", 0, false, false ),
				( @"(*CR)(*BSR_ANYCRLF)\Aa*\r\R*\n\z", "aaa\r\n", 0, true, false ),
				( "(*CRLF)(*CR)^a$", "a\r", 0, false, false ),
				( "(*CR)(*LF)^a$", "a\n", 0, true, false ),
				( @"(*BSR_UNICODE)(*BSR_ANYCRLF)\R", "\u2028", 0, false, false ),
				( @"(*BSR_ANYCRLF)(*BSR_UNICODE)\R", "\u2028", 0, true, false ),
				( @"(*CR)(*NUL)\R$", "\u2028\0", 0, true, false ),
				( "(*NUL).", "\uD800\0", 0, false, false ),
				( "(*LF).", "\uD800\n", 0, false, false )
			};
			for ( var index = 0; index < cases.Length; index++ ) {
				var (source, subject, start, utf, ucp) = cases[index];
				foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 16, 32, 63 } ) {
					yield return new object[] { index, source, subject, start, utf, ucp, defaults };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void NewlineProfilesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"newline profile {id}: {compiled.Diagnostic}" );
		var nativePolicies = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start, defaults, ucp );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = nativePolicies[mask];
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( "(*CR)(?m)^😀+$", "x\r😀😀\r" )]
	[InlineData( @"(*NUL)\R$", "😀\u2028\0" )]
	[InlineData( @"(*CR)(*BSR_ANYCRLF)\Aa*\r\R*\n\z", "aaa\r\n" )]
	[InlineData( @"(*BSR_UNICODE)(*BSR_ANYCRLF)\R", "\u2028" )]
	public void UnicodeProfilesAgreeAcrossManagedWidths( string source, string subject ) {
		var subjectBytes = Encoding.UTF8.GetBytes( subject );
		var bytes = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true } );
		var words = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.True( bytes.IsSuccess ); Assert.True( words.IsSuccess );
		var byteMatch = bytes.Pattern!.Match( subjectBytes );
		var wordMatch = words.Pattern!.Match( subject.AsMemory() );
		Assert.True( byteMatch.IsSuccess ); Assert.True( wordMatch.IsSuccess );
		var coordinates = new PcreDualWidthCoordinates( subjectBytes, subject );
		Assert.Equal( coordinates.ByteCaptures( byteMatch.Match is { } b ? $"{b.ByteIndex}:{b.ByteLength}" : "" ),
			coordinates.WordCaptures( wordMatch.Match is { } w ? $"{w.CodeUnitIndex}:{w.CodeUnitLength}" : "" ) );
	}

	[Theory]
	[InlineData( "(*CR)(?m)^a$", "a\r", false, 5 )]
	[InlineData( "(*NUL)(?m)^a$", "a\0", true, 7 )]
	[InlineData( @"(*BSR_ANYCRLF)\R*\r\n\z", "\r\n", false, 10 )]
	[InlineData( @"(*BSR_ANYCRLF)\R*\r\n\z", "\r\n", true, 12 )]
	public void ProfilesRetainWorkQuotaAndCancellation( string source, string subject, bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var result = pattern.Match( subject.AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); }
			else { Assert.True( result.IsSuccess ); Assert.NotNull( result.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
		using var cancellation = new CancellationTokenSource();
		var cancelled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, WorkObserver = work => { if ( work == required - 1 ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => cancelled.Match( subject.AsMemory(), cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( @"(*CR)X\Z", "\r" )]
	[InlineData( @"(*NUL)X\Z", "\0" )]
	public void ProfilesKeepTheStrictValidationWindow( string source, string newline ) {
		var subject = String.Concat( "\uD800X", newline );
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 1 );
		Assert.Equal( "M", native.Kind ); Assert.Equal( "1:1", native.Captures );
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } )
			.Pattern!.Match( subject.AsMemory(), 1 );
		Assert.True( result.IsSuccess ); Assert.Equal( new PcreWordCapture( 1, 1 ), result.Match?.Captures[0] );
	}

	[Fact]
	public void PrefixesPreserveOriginalPatternCoordinatesAndLiteralSpelling() {
		var source = "(*CR)\\";
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( 5, result.Diagnostic?.CodeUnitOffset );
		var literal = PcreWordLiteralCompiler.CompileLiteral( "(*CR)".AsMemory() ).Pattern!;
		Assert.Equal( new PcreWordCapture( 0, 5 ), literal.Match( "(*CR)".AsMemory() ).Match?.Captures[0] );
		var sequence = PcreWordSequenceCompiler.CompileSequence( @"\Q(*CR)\E".AsMemory() ).Pattern!;
		Assert.Equal( new PcreWordCapture( 0, 5 ), sequence.Match( "(*CR)".AsMemory() ).Match?.Captures[0] );
	}

	[Theory]
	[InlineData( "(*CRLF)^a$" )]
	[InlineData( "(*ANYCRLF)^a$" )]
	[InlineData( "(*ANY)^a$" )]
	[InlineData( "(*CR)(*UTF)a" )]
	[InlineData( "(*UTF)(*CR)a" )]
	[InlineData( "(*NUL)(*UCP)a" )]
	[InlineData( "(*BSR_ANYCRLF)(*NOTEMPTY)a" )]
	[InlineData( "(*LF)(*NO_START_OPT)a" )]
	[InlineData( "(*BSR_UNICODE)(*CASELESS_RESTRICT)a" )]
	[InlineData( "a(*CR)" )]
	public void BroaderProfilesAndOtherDirectivesRemainExplicit( string source ) {
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
