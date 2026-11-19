using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreCommitFragmentTests {
	public static IEnumerable<object[]> Cases => PcreCommitFragmentCases.All.Select( c => new object[] { c.Id } );

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void NativeCutAndPhysicalEndProtocolsPrecedeManagedComparisons( int id ) {
		var item = PcreCommitFragmentCases.All.Single( c => c.Id == id );
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
	public void RestartsSpendOneSharedWorkBudget() {
		var steps = new List<int>(); var input = new byte[] { (byte)'a', 0xFF, (byte)'b' };
		var policy = new PcreMatchPolicy { Anchored = true };
		var observed = Compile( "a(*COMMIT)c|b", new() { MatchInvalidUtf = true, WorkObserver = steps.Add } ).Match( input, policy );
		Assert.Equal( new PcreByteCapture( 2, 1 ), observed.Match?.Captures[0] );
		var count = steps.Count; Assert.True( count > 1 );
		var exact = Compile( "a(*COMMIT)c|b", new() { MatchInvalidUtf = true, MatchWorkLimit = count } ).Match( input, policy );
		Assert.True( exact.IsSuccess ); Assert.Equal( new PcreByteCapture( 2, 1 ), exact.Match?.Captures[0] );
		var exhausted = Compile( "a(*COMMIT)c|b", new() { MatchInvalidUtf = true, MatchWorkLimit = count - 1 } ).Match( input, policy );
		Assert.Null( exhausted.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
	}

	[Fact]
	public void CancellationCanInterruptTheLaterFragment() {
		var steps = new List<int>(); var input = new byte[] { (byte)'a', 0xFF, (byte)'b' };
		var policy = new PcreMatchPolicy { Anchored = true };
		var observed = Compile( "a(*COMMIT)c|b", new() { MatchInvalidUtf = true, WorkObserver = steps.Add } ).Match( input, policy );
		Assert.Equal( new PcreByteCapture( 2, 1 ), observed.Match?.Captures[0] );
		var cancelAt = steps.Count - 1; using var source = new CancellationTokenSource();
		var pattern = Compile( "a(*COMMIT)c|b", new() { MatchInvalidUtf = true, WorkObserver = step => { if ( step == cancelAt ) { source.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => pattern.Match( input, policy, cancellationToken: source.Token ) );
	}

	[Fact]
	public void DiscardedCapturesDoNotLeakIntoTheLaterBoundResult() {
		var pattern = Compile( "(?<bad>a)(*COMMIT)c|(?<good>b)", new() { MatchInvalidUtf = true } );
		var policy = new PcreMatchPolicy { Anchored = true };
		var first = pattern.Match( new byte[] { (byte)'a', 0xFF, (byte)'b' }, policy ).Match;
		Assert.NotNull( first ); Assert.Null( first.GetCapture( "bad" ) );
		Assert.Equal( new PcreByteCapture( 2, 1 ), first.GetCapture( "good" ) );
		Assert.Same( pattern.CaptureInfo, first.CaptureInfo );
		Assert.Equal( new PcreByteCapture( 0, 1 ), pattern.Match( "b"u8.ToArray(), policy ).Match?.GetCapture( "good" ) );
		Assert.Equal( new PcreByteCapture( 2, 1 ), first.GetCapture( "good" ) );
	}

	[Fact]
	public void RestartMatchingKeepsTheOriginalSubjectView() {
		var padded = new byte[] { 0xFF, (byte)'a', 0xFF, (byte)'x', 0xFF };
		var pattern = Compile( "a(*COMMIT)c|b", new() { MatchInvalidUtf = true, WorkObserver = step => { if ( step == 1 ) { padded[3] = (byte)'b'; } } } );
		var result = pattern.Match( padded.AsMemory( 1, 3 ), new PcreMatchPolicy { Anchored = true } );
		Assert.True( result.IsSuccess ); Assert.Equal( new PcreByteCapture( 2, 1 ), result.Match?.Captures[0] );
		Assert.Equal( 0xFF, padded[0] ); Assert.Equal( 0xFF, padded[^1] );
	}

	private static PcreCompiledBytePattern Compile( string source, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message ); return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
