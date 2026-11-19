using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreInterpretedBoundaryTests {
	public static IEnumerable<object[]> Cases => PcreInterpretedBoundaryCases.All.Select( c => new object[] { c.Id } );

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void NativeBoundaryProtocolsPrecedeManagedComparisons( int id ) {
		var item = PcreInterpretedBoundaryCases.All.Single( c => c.Id == id );
		var flags = ( ( item.Mode & 1 ) != 0 ? 0x80000u : 0 ) | ( ( item.Mode & 2 ) != 0 ? 0x20000u : 0 )
			| ( ( item.Mode & 4 ) != 0 ? 0x4000000u : 0 ) | ( ( item.Mode & 16 ) != 0 ? 8u : 0 )
			| ( ( item.Mode & 32 ) != 0 ? 0x2000000u : 0 ) | ( ( item.Mode & 128 ) != 0 ? 16u : 0 )
			| ( ( item.Mode & 256 ) != 0 ? 0x200000u : 0 ) | ( ( item.Mode & 512 ) != 0 ? 0x100u : 0 )
			| ( ( item.Defaults & 1 ) != 0 ? 0x80000000u : 0 ) | ( ( item.Defaults & 2 ) != 0 ? 0x20000000u : 0 );
		var reference = new PcreRegex8Bit( Convert.FromHexString( item.Pattern ), Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | (PcreOptions)flags,
			ExtraCompileOptions = (PcreExtraCompileOptions)( ( item.Mode & 8 ) != 0 ? 64 : 0 ),
			NewLine = (PcreNewLine)( ( item.Mode & 64 ) != 0 ? 5 : 2 ), BackslashR = (PcreBackslashR)1, ParensLimit = 512
		} );
		var subject = Convert.FromHexString( item.Subject );
		var protocols = item.Outcomes.Split( ';' );
		Assert.Equal( 64, protocols.Length );
		for ( var mask = 0; mask < protocols.Length; mask++ ) {
			var effective = mask | item.Defaults;
			var options = (PCRE.PcreMatchOptions)( ( ( effective & 1 ) != 0 ? 0x80000000u : 0 )
				| ( ( effective & 2 ) != 0 ? 0x20000000u : 0 ) | ( ( effective & 4 ) != 0 ? 4u : 0 )
				| ( ( effective & 8 ) != 0 ? 8u : 0 ) | ( ( effective & 16 ) != 0 ? 1u : 0 ) | ( ( effective & 32 ) != 0 ? 2u : 0 ) );
			var match = reference.Match( subject, item.Start, options );
			var captures = new List<string>();
			if ( match.Success ) {
				for ( var i = 0; i < match.Groups.Count; i++ ) {
					var group = match.Groups[i]; captures.Add( group.Success ? $"{group.Index}:{group.EndIndex - group.Index}" : "-" );
				}
			}
			Assert.Equal( protocols[mask], String.Concat( String.Join( ",", captures ), "/", match.Success ? Convert.ToHexString( match.Mark.ToArray() ) : "" ) );
		}
		Assert.Equal( item.Outcomes, PcreMatchPolicyConsumer.Run( item ) );
	}

	[Fact]
	public void InitialBoundaryCandidateSharesOneWorkBudget() {
		var input = new byte[] { 0xFF, 0xFF, 0xFF }; var policy = new PcreMatchPolicy { Anchored = true };
		var steps = new List<int>();
		var observed = Compile( "\\B", new() { MatchInvalidUtf = true, WorkObserver = steps.Add } ).Match( input, policy, 1 );
		Assert.Equal( new PcreByteCapture( 1, 0 ), observed.Match?.Captures[0] );
		var count = steps.Count; Assert.True( count > 1 );
		foreach ( var observer in new Action<int>?[] { null, _ => { } } ) {
			var exact = Compile( "\\B", new() { MatchInvalidUtf = true, WorkObserver = observer, MatchWorkLimit = count } ).Match( input, policy, 1 );
			Assert.True( exact.IsSuccess ); Assert.Equal( new PcreByteCapture( 1, 0 ), exact.Match?.Captures[0] );
			var exhausted = Compile( "\\B", new() { MatchInvalidUtf = true, WorkObserver = observer, MatchWorkLimit = count - 1 } ).Match( input, policy, 1 );
			Assert.Null( exhausted.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
	}

	[Fact]
	public void CancellationInterruptsTheInitialBoundaryCandidate() {
		var input = new byte[] { 0xFF, 0xFF, 0xFF }; var policy = new PcreMatchPolicy { Anchored = true };
		var steps = new List<int>();
		var observed = Compile( "\\B", new() { MatchInvalidUtf = true, WorkObserver = steps.Add } ).Match( input, policy, 1 );
		Assert.Equal( new PcreByteCapture( 1, 0 ), observed.Match?.Captures[0] );
		var cancelAt = steps.Count - 1; using var source = new CancellationTokenSource();
		var pattern = Compile( "\\B", new() { MatchInvalidUtf = true, WorkObserver = step => { if ( step == cancelAt ) { source.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => pattern.Match( input, policy, 1, source.Token ) );
	}

	[Fact]
	public void EmptyAndLaterCapturesRetainTheirOwnResults() {
		var pattern = Compile( "(?:(?<gap>\\B)|(?<hit>b))", new() { MatchInvalidUtf = true } );
		var input = new byte[] { 0xFF, 0xFF, 0xFF, (byte)'b' };
		var first = pattern.Match( input, new PcreMatchPolicy { Anchored = true }, 1 ).Match;
		Assert.NotNull( first ); Assert.Equal( new PcreByteCapture( 1, 0 ), first.GetCapture( "gap" ) );
		Assert.Null( first.GetCapture( "hit" ) ); Assert.Same( pattern.CaptureInfo, first.CaptureInfo );
		var second = pattern.Match( input, new PcreMatchPolicy { Anchored = true, NotEmpty = true }, 1 ).Match;
		Assert.NotNull( second ); Assert.Null( second.GetCapture( "gap" ) );
		Assert.Equal( new PcreByteCapture( 3, 1 ), second.GetCapture( "hit" ) );
		Assert.Equal( new PcreByteCapture( 3, 1 ), pattern.Match( input, 1 ).Match?.GetCapture( "hit" ) );
		Assert.Equal( new PcreByteCapture( 1, 0 ), first.GetCapture( "gap" ) ); Assert.Null( first.GetCapture( "hit" ) );
	}

	[Fact]
	public void BoundaryAndLaterAlternativeReadOriginalSubjectViews() {
		var first = Compile( "\\B", new() { MatchInvalidUtf = true } ).Match(
			new byte[] { 0xFF, 0xFF, 0xFF, (byte)'y' }, new PcreMatchPolicy { Anchored = true }, 1
		);
		Assert.Equal( new PcreByteCapture( 1, 0 ), first.Match?.Captures[0] );
		var padded = new byte[] { 0xFE, 0xFF, 0xFF, 0xFF, (byte)'y', 0xFE };
		var pattern = Compile( "\\B|b", new() { MatchInvalidUtf = true, WorkObserver = step => { if ( step == 1 ) { padded[4] = (byte)'b'; } } } );
		var result = pattern.Match( padded.AsMemory( 1, 4 ), new PcreMatchPolicy { Anchored = true, NotEmpty = true }, 1 );
		Assert.True( result.IsSuccess ); Assert.Equal( new PcreByteCapture( 3, 1 ), result.Match?.Captures[0] );
		Assert.Equal( 0xFE, padded[0] ); Assert.Equal( 0xFE, padded[^1] );
	}

	private static PcreCompiledBytePattern Compile( string source, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message ); return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
