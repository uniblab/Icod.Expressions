using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreOptionalRewindTests {
	public static IEnumerable<object[]> Cases => PcreOptionalRewindCases.All.Select( c => new object[] { c.Id } );

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void NativeOptionalRewindProtocolsPrecedeManagedComparisons( int id ) {
		var item = PcreOptionalRewindCases.All.Single( c => c.Id == id );
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
			var options = PcreNativeMatchPolicy.Options( item.Defaults, mask );
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
	public void OptionalRewindUsesOneObservedAndUnobservedWorkBudget() {
		var input = new byte[] { 0xFF, (byte)'b' }; var policy = new PcreMatchPolicy { Anchored = true };
		var steps = new List<int>();
		var observed = Compile( "(?<=a?)b", new() { MatchInvalidUtf = true, WorkObserver = steps.Add } ).Match( input, policy );
		Assert.Equal( new PcreByteCapture( 1, 1 ), observed.Match?.Captures[0] );
		var count = steps.Count; Assert.True( count > 1 );
		foreach ( var observer in new Action<int>?[] { null, _ => { } } ) {
			var exact = Compile( "(?<=a?)b", new() { MatchInvalidUtf = true, WorkObserver = observer, MatchWorkLimit = count } ).Match( input, policy );
			Assert.True( exact.IsSuccess ); Assert.Equal( new PcreByteCapture( 1, 1 ), exact.Match?.Captures[0] );
			var exhausted = Compile( "(?<=a?)b", new() { MatchInvalidUtf = true, WorkObserver = observer, MatchWorkLimit = count - 1 } ).Match( input, policy );
			Assert.Null( exhausted.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
	}

	[Fact]
	public void CancellationInterruptsTheRecoveredOptionalRewind() {
		var input = new byte[] { 0xFF, (byte)'b' }; var policy = new PcreMatchPolicy { Anchored = true };
		var steps = new List<int>();
		var observed = Compile( "(?<=a?)b", new() { MatchInvalidUtf = true, WorkObserver = steps.Add } ).Match( input, policy );
		Assert.Equal( new PcreByteCapture( 1, 1 ), observed.Match?.Captures[0] );
		var cancelAt = steps.Count - 1; using var source = new CancellationTokenSource();
		var pattern = Compile( "(?<=a?)b", new() { MatchInvalidUtf = true, WorkObserver = step => { if ( step == cancelAt ) { source.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => pattern.Match( input, policy, cancellationToken: source.Token ) );
	}

	[Fact]
	public void EmptyAndNonemptyLookbehindCapturesRetainTheirOwnResults() {
		var pattern = Compile( "(?<=(?<prefix>a?))(?<hit>b)", new() { MatchInvalidUtf = true } );
		var first = pattern.Match( new byte[] { 0xFF, (byte)'b' }, new PcreMatchPolicy { Anchored = true } ).Match;
		Assert.NotNull( first ); Assert.Equal( new PcreByteCapture( 1, 0 ), first.GetCapture( "prefix" ) );
		Assert.Equal( new PcreByteCapture( 1, 1 ), first.GetCapture( "hit" ) ); Assert.Same( pattern.CaptureInfo, first.CaptureInfo );
		var second = pattern.Match( "ab"u8.ToArray(), new PcreMatchPolicy { EndAnchored = true } ).Match;
		Assert.NotNull( second ); Assert.Equal( new PcreByteCapture( 0, 1 ), second.GetCapture( "prefix" ) );
		Assert.Equal( new PcreByteCapture( 1, 1 ), second.GetCapture( "hit" ) );
		Assert.Equal( new PcreByteCapture( 1, 0 ), first.GetCapture( "prefix" ) );
		Assert.Null( pattern.Match( new byte[] { 0xFF, (byte)'b' } ).Match );
	}

	[Fact]
	public void RecoveredRewindReadsTheOriginalSubjectView() {
		var padded = new byte[] { 0xFF, 0xFF, (byte)'x', 0xFF };
		var pattern = Compile( "(?<=a?)b", new() { MatchInvalidUtf = true, WorkObserver = step => { if ( step == 1 ) { padded[2] = (byte)'b'; } } } );
		var result = pattern.Match( padded.AsMemory( 1, 2 ), new PcreMatchPolicy { Anchored = true } );
		Assert.True( result.IsSuccess ); Assert.Equal( new PcreByteCapture( 1, 1 ), result.Match?.Captures[0] );
		Assert.Equal( 0xFF, padded[0] ); Assert.Equal( 0xFF, padded[^1] );
	}

	private static PcreCompiledBytePattern Compile( string source, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message ); return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
