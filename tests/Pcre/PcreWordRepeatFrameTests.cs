using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordRepeatFrameTests {
	public static IEnumerable<object[]> Cases {
		get {
			var id = 0;
			foreach ( var depth in new[] { 5, 8, 16, 32, 64 } ) {
				var plusSubject = new string( Enumerable.Range( 0, depth + 1 )
					.Select( i => Math.Min( i, depth - 1 ) % 2 == 0 ? 'a' : 'b' ).ToArray() );
				var cases = new[] {
					( String.Concat( Repeated( "a*", depth ), "b" ), "aab", false ),
					( String.Concat( Repeated( "a*?", depth ), "b" ), "aab", true ),
					( String.Concat( Enumerable.Range( 0, depth ).Select( i => i % 2 == 0 ? "a+" : "b+" ) ), plusSubject, false ),
					( String.Concat( Enumerable.Range( 0, depth ).Select( i => i % 2 == 0 ? "a+?" : "b+?" ) ), plusSubject, true ),
					( ChoicePattern( depth, false ), "bbx", false ),
					( ChoicePattern( depth, true ), "bbx", true ),
					( String.Concat( @"(*BSR_ANYCRLF)\A", Repeated( @"\R*", depth ), @"\r\n\z" ), "\r\n", true ),
					( String.Concat( Repeated( "😀*", depth ), "x" ), "😀😀x", true ),
					( Repeated( "a*", depth ), "", false ),
					( Repeated( "a*?", depth ), "", true ),
					( String.Concat( Repeated( "a*", depth ), "x" ), "", false ),
					( String.Concat( Repeated( "a*?", depth ), "x|b" ), "b", true )
				};
				foreach ( var (source, subject, utf) in cases ) {
					foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 16, 32, 63 } ) {
						yield return new object[] { id, source, subject, utf, defaults };
					}
					id++;
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void ExplicitRepeatFramesAgreeWithNativePolicies( int id, string source, string subject, bool utf, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"repeat frames {id}: {compiled.Diagnostic}" );
		var nativePolicies = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, 0, defaults );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = nativePolicies[mask];
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ) );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( 5 )]
	[InlineData( 32 )]
	[InlineData( 64 )]
	public void DeepUnicodeRetriesAgreeAcrossManagedWidths( int depth ) {
		foreach ( var source in new[] { String.Concat( Repeated( "😀*?", depth ), "x" ),
			String.Concat( Repeated( @"\R*", depth ), @"\r\nx" ) } ) {
			var subject = source.Contains( 'R' ) ? "😀\r\nx" : "😀😀x";
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
	}

	[Theory]
	[InlineData( 5, false )]
	[InlineData( 5, true )]
	[InlineData( 32, false )]
	[InlineData( 32, true )]
	[InlineData( 64, false )]
	[InlineData( 64, true )]
	public void DeepFailureAndUnwindRetainExactWorkQuota( int depth, bool lazy ) {
		var source = String.Concat( Repeated( lazy ? "a*?" : "a*", depth ), "x" );
		var required = 2 * depth + 2;
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				MatchWorkLimit = budget, WorkObserver = trace.Add
			} );
			Assert.True( compiled.IsSuccess );
			var result = compiled.Pattern!.Match( ReadOnlyMemory<char>.Empty );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); }
			else { Assert.True( result.IsSuccess ); Assert.Null( result.Match ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
		using var cancellation = new CancellationTokenSource();
		var cancelled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			WorkObserver = work => { if ( work == depth + 3 ) { cancellation.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => cancelled.Match( ReadOnlyMemory<char>.Empty, cancellationToken: cancellation.Token ) );
		Assert.True( cancelled.Match( ReadOnlyMemory<char>.Empty ).IsSuccess );
	}

	[Fact]
	public void DeepFramesResetAcrossBranchesCandidatesAndCalls() {
		var prefix = Repeated( "a*?", 64 );
		foreach ( var source in new[] { String.Concat( prefix, "x|b" ), String.Concat( prefix, "b" ) } ) {
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory() );
			Assert.True( compiled.IsSuccess );
			foreach ( var subject in new[] { "b", "zab", "", "b" } ) {
				var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), false, 0 );
				Assert.True( native.Kind is "M" or "N" );
				var result = compiled.Pattern!.Match( subject.AsMemory() );
				Assert.True( result.IsSuccess );
				Assert.Equal( native.Captures, result.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
			}
		}
		var raw = PcreWordSequenceCompiler.CompileSequence( String.Concat( Repeated( @"\x{D800}*", 64 ), "x" ).AsMemory() );
		Assert.True( raw.IsSuccess );
		Assert.Equal( new PcreWordCapture( 0, 2 ), raw.Pattern!.Match( "\uD800x".AsMemory() ).Match?.Captures[0] );
	}

	[Fact]
	public void RepeatDepthAndChoiceProductStayBoundedSeparately() {
		Assert.True( PcreWordSequenceCompiler.CompileSequence( Repeated( "a*", 64 ).AsMemory() ).IsSuccess );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( ChoicePattern( 64, false ).AsMemory() ).IsSuccess );
		var branches = String.Join( '|', Enumerable.Repeat( "a*", 63 ).Prepend( Repeated( "a*", 64 ) ) );
		Assert.True( PcreWordSequenceCompiler.CompileSequence( branches.AsMemory() ).IsSuccess );
		// The shared parser still limits total repeat-state marks across branches.
		var repeatLimited = String.Join( '|', Enumerable.Repeat( Repeated( "a*", 16 ), 64 ) );
		Assert.Equal( PcreWordDiagnosticCode.InvalidSyntax,
			PcreWordSequenceCompiler.CompileSequence( repeatLimited.AsMemory() ).Diagnostic?.Code );
		foreach ( var source in new[] { Repeated( "a*", 65 ), String.Concat( ChoicePattern( 64, false ), "b?" ),
			String.Concat( ChoicePattern( 64, false ), "|x" ), String.Concat( branches, "|x" ), "(?:a*){65,}+b*", "(?>(*sr:a)a*)b*", "a{65,}+b*" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
	}

	[Fact]
	public void AmbiguousDeepSearchStopsAtItsWorkLimit() {
		var trace = new List<int>();
		var compiled = PcreWordSequenceCompiler.CompileSequence( String.Concat( Repeated( "a+", 32 ), "x" ).AsMemory(), new() {
			MatchWorkLimit = 10_000, WorkObserver = trace.Add
		} );
		Assert.True( compiled.IsSuccess );
		var result = compiled.Pattern!.Match( new string( 'a', 64 ).AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code );
		Assert.Equal( Enumerable.Range( 1, 10_000 ), trace );
	}

	private static string ChoicePattern( int depth, bool lazy ) {
		var source = new StringBuilder();
		var choices = Enumerable.Range( 0, 6 ).Select( i => i * depth / 5 ).ToHashSet();
		for ( var index = 0; index <= depth; index++ ) {
			if ( choices.Contains( index ) ) { source.Append( lazy ? "b??" : "b?" ); }
			if ( index < depth ) { source.Append( lazy ? "a*?" : "a*" ); }
		}
		return source.Append( 'x' ).ToString();
	}

	private static string Repeated( string atom, int count ) => String.Concat( Enumerable.Repeat( atom, count ) );
	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
