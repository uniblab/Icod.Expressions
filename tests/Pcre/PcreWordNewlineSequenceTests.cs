using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordNewlineSequenceTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( @"\R", "", 0, false, false ),
				( @"\R", "\r", 0, false, false ),
				( @"\R", "\n", 0, true, false ),
				( @"\R", "\u000B", 0, false, false ),
				( @"\R", "\u000C", 0, true, false ),
				( @"\R", "\u0085", 0, false, false ),
				( @"\R", "\u2028", 0, false, false ),
				( @"\R", "\u2029", 0, true, false ),
				( @"\R", "\r\n", 0, false, false ),
				( @"\R", "\r\nX", 1, true, false ),
				( @"\R", "a \0\t", 0, false, false ),
				( @"\R", "😀\r\n", 0, true, false ),
				( @"\R", "\uD800\n", 0, false, false ),
				( @"\R", "\uD800", 0, false, false ),
				( @"\R+", "\r\n\u0085\u2028\u2029\r", 0, true, false ),
				( @"\R+?", "\r\n\u0085\u2028\u2029\r", 0, false, false ),
				( @"\R{2}", "\r\n", 0, true, false ),
				( @"\R{2}", "\r\n\n", 0, false, false ),
				( @"\R{0,2}\n", "\r\n", 0, true, false ),
				( @"\R{0,2}?\n", "\r\n", 0, false, false ),
				( @"\A\R+\n\z", "\r\n", 0, false, false ),
				( @"\A\R+?\n\z", "\r\n", 0, true, false ),
				( @"\A\R*\r\n\z", "\r\n", 0, true, false ),
				( @"\A\r\R*\n\z", "\r\n", 0, false, false ),
				( @"\Aa*\r\R*\n\z", "aaa\r\n", 0, true, false ),
				( @"\Aa*\R+\n\z", "aaa\r\n", 0, false, false ),
				( @"\Aa*\R*\r\n\z", "aaa\r\n", 0, true, false ),
				( @"\R*|a", "a", 0, false, false ),
				( @"a|\R*", "\r\n", 0, true, false ),
				( @"a*\R*b?c*\R*\r\n", "aa\r\nbcc\r\n\r\n", 0, true, false ),
				( @"a*?\R*?b??c*?\R*?\r\n", "aa\r\nbcc\r\n\r\n", 0, false, false ),
				( @"\R*b?\R*c?\R*d?\R*\r\n", "\r\nb\nc\rd\r\n\r\n", 0, true, false ),
				( @"\R+X", "\u2028X", 0, true, true ),
				( @"(?m)^\R$", "\r\n", 0, false, false ),
				( @"\R$", "\u2028\n", 0, true, false ),
				( @"\R\bX", "\r\nX", 0, false, false ),
				( @"\R\R", "\n\r\n", 0, true, false ),
				( @"\R*", "\r\n", 2, true, false ),
				( @"\R", "\u2028", 0, false, true ),
				( @"\R*\n", "\r\n", 1, true, false )
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
	public void NewlineSequencesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"newline {id}: {compiled.Diagnostic}" );
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
	[InlineData( @"\R+", "😀\r\n\u0085\u2028\u2029" )]
	[InlineData( @"\Aa*\R*\r\n\z", "aaa\r\n" )]
	[InlineData( @"\Aa*\R+\n\z", "aaa\r\n" )]
	[InlineData( @"\R*b?\R*c?\R*d?\R*\r\n", "\r\nb\nc\rd\r\n\r\n" )]
	public void UnicodeNewlineSequencesAgreeAcrossManagedWidths( string source, string subject ) {
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
	[InlineData( @"\R*\r\n\z", false, 10, 6 )]
	[InlineData( @"\R*\r\n\z", true, 12, 8 )]
	[InlineData( @"a*\R*\r\n\z", false, 12, 8 )]
	[InlineData( @"a*\R*\r\n\z", true, 14, 10 )]
	public void NewlineRewindsShareWorkQuotaAndCancellation( string source, bool utf, int required, int rewind ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				Utf16 = utf, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var result = pattern.Match( "\r\n".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); }
			else { Assert.True( result.IsSuccess ); Assert.Equal( new PcreWordCapture( 0, 2 ), result.Match?.Captures[0] ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
		using var cancellation = new CancellationTokenSource();
		var cancelled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, WorkObserver = work => { if ( work == rewind ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => cancelled.Match( "\r\n".AsMemory(), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void NewlineSequencesRespectSubjectViewsAndValidationStart() {
		var pattern = PcreWordSequenceCompiler.CompileSequence( @"\R".AsMemory(), new() { Utf16 = true } ).Pattern!;
		Assert.Equal( new PcreWordCapture( 0, 1 ), pattern.Match( "\r\n".AsMemory( 0, 1 ) ).Match?.Captures[0] );
		Assert.Equal( new PcreWordCapture( 0, 1 ), pattern.Match( "\r\n".AsMemory( 1, 1 ) ).Match?.Captures[0] );
		var subject = "\uD800\r\n";
		var native = PcreNative16Oracle.ExecuteSequence( Words( @"\R" ), Words( subject ), true, 1 );
		Assert.Equal( "M", native.Kind ); Assert.Equal( "1:2", native.Captures );
		Assert.Equal( new PcreWordCapture( 1, 2 ), pattern.Match( subject.AsMemory(), 1 ).Match?.Captures[0] );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Subject, pattern.Match( subject.AsMemory() ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( @"(*ANYCRLF)(*UTF)\R" )]
	[InlineData( @"(*ANY)(*UTF)\R" )]
	[InlineData( @"\X" )]
	[InlineData( @"\R{65,}+" )]
	public void OtherNewlineProfilesAndExecutionFormsRemainExplicit( string source ) {
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
