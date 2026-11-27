using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordLineAnchorTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( "^$", "", 0, false, false ),
				( "$", "", 0, true, false ),
				( @"\Z", "", 0, true, false ),
				( "^a$", "a", 0, false, false ),
				( "^a$", "a\n", 0, true, false ),
				( "^a$", "a\n\n", 0, false, false ),
				( "^a$", "ba", 1, true, false ),
				( "$", "\n", 0, false, false ),
				( "$", "\r\n", 0, true, false ),
				( @"a\Z", "a\n", 0, false, false ),
				( @"a\z", "a\n", 0, true, false ),
				( @"\Z", "a\n\n", 0, true, false ),
				( "(?m)^", "\n", 1, false, false ),
				( "(?m)^", "", 0, true, false ),
				( "(?m)^$", "\n\n", 0, false, false ),
				( "(?m)^a$", "x\na\ny", 0, true, false ),
				( "(?m)^a$", "x\na\ny", 2, false, false ),
				( "(?m)^a$", "x\ra\ry", 0, true, false ),
				( "(?m)^a$", "x\r\na\r\n", 0, false, false ),
				( @"(?m)^a\r$", "x\r\na\r\n", 0, true, false ),
				( "(?m)$", "a\nb", 0, false, false ),
				( "(?m)$", "a\n", 2, true, false ),
				( "(?m)^$", "\n", 1, true, false ),
				( "(?m)^a(?-m)$", "a\nb", 0, true, false ),
				( "^a(?m)$", "a\nb", 0, false, false ),
				( @"(?m)\Aa$", "a\nb", 0, true, false ),
				( @"(?m)^a\Z", "x\na\n", 0, true, false ),
				( "^|a$", "a", 0, false, false ),
				( "(?m)$|a", "a\n", 0, true, false ),
				( "^a*$", "aaa\n", 0, true, false ),
				( "(?m)^a*?$", "aaa\na", 0, false, false ),
				( "(?m)a*$b?", "aaa\nb", 0, true, false ),
				( "(?m)^a*b?c*d?e*f?g*$", "aaabccdeefgg\n", 0, true, false ),
				( @"(?m)^a*?b??c*?d??e*?f??g*?\Z", "aaabccdeefgg\n", 0, false, false ),
				( "(?m)^😀+$", "x\n😀😀\n", 0, true, false ),
				( @"(?m)^\w+$", "!\nα𐐀\n", 0, true, true ),
				( @"(?m)^\x{D800}$", "x\n\uD800\n", 0, false, false ),
				( "(?m)^a$", "x\u0085a\u2028", 0, true, false ),
				( @"^\x{0}$", "\0\n", 0, false, false ),
				( "(?m)^a$", "😀\na\n", 3, true, false )
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
	public void LfAnchorsAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"line anchor {id}: {compiled.Diagnostic}" );
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
	[InlineData( "(?m)^😀+$", "x\n😀😀\n" )]
	[InlineData( @"(?m)^\w+\Z", "!\nα𐐀\n" )]
	[InlineData( "(?m)^a*b?c*d?e*f?g*$", "aaabccdeefgg\n" )]
	public void UnicodeLineAnchorsAgreeAcrossManagedWidths( string source, string subject ) {
		var subjectBytes = Encoding.UTF8.GetBytes( subject );
		var bytes = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true, Ucp = true } );
		var words = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true, Ucp = true } );
		Assert.True( bytes.IsSuccess ); Assert.True( words.IsSuccess );
		var byteMatch = bytes.Pattern!.Match( subjectBytes );
		var wordMatch = words.Pattern!.Match( subject.AsMemory() );
		Assert.True( byteMatch.IsSuccess ); Assert.True( wordMatch.IsSuccess );
		var coordinates = new PcreDualWidthCoordinates( subjectBytes, subject );
		Assert.Equal( coordinates.ByteCaptures( byteMatch.Match is { } b ? $"{b.ByteIndex}:{b.ByteLength}" : "" ),
			coordinates.WordCaptures( wordMatch.Match is { } w ? $"{w.CodeUnitIndex}:{w.CodeUnitLength}" : "" ) );
	}

	[Theory]
	[InlineData( "^X|X" )]
	[InlineData( "(?m)^X|X" )]
	[InlineData( "X$" )]
	[InlineData( @"X\Z" )]
	public void LineAnchorsDoNotExtendTheStrictValidationWindow( string source ) {
		var subject = "\uD800X";
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), true, 1 );
		Assert.Equal( "M", native.Kind );
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } )
			.Pattern!.Match( subject.AsMemory(), 1 );
		Assert.True( result.IsSuccess );
		Assert.Equal( new PcreWordCapture( 1, 1 ), result.Match?.Captures[0] );
	}

	[Theory]
	[InlineData( false, 5 )]
	[InlineData( true, 7 )]
	public void LineAssertionsShareWorkQuotaAndCancellation( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( "(?m)^a$".AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var result = pattern.Match( "a\n".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), result.Match?.Captures[0] ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
		using var cancellation = new CancellationTokenSource();
		var cancelled = PcreWordSequenceCompiler.CompileSequence( "(?m)^a$".AsMemory(), new() {
			Utf16 = utf, WorkObserver = work => { if ( work == required - 1 ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => cancelled.Match( "a\n".AsMemory(), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void LineAssertionsRespectTheSuppliedSubjectView() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( "^a$".AsMemory() ).Pattern!;
		Assert.Equal( new PcreWordCapture( 0, 1 ), pattern.Match( "xay".AsMemory( 1, 1 ) ).Match?.Captures[0] );
		Assert.Null( pattern.Match( "a\n\n".AsMemory() ).Match );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
