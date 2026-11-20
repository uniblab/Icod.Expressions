using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreInterpretedThenTests {
	public static IEnumerable<object[]> Cases => PcreInterpretedThenCases.All.Select( c => new object[] { c.Id } );

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void NativeThenProtocolsPrecedeManagedComparisons( int id ) {
		var item = PcreInterpretedThenCases.All.Single( c => c.Id == id );
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

	private const string Pattern = @"\G(?(DEFINE)(?<x>(?<v>a|ab)(?=(*THEN)c)))(?&x(2))c";

	[Fact]
	public void AssertionRetriesShareOneWorkBudget() {
		var input = "abc"u8.ToArray();
		var policy = new PcreMatchPolicy { Anchored = true };
		var steps = new List<int>();
		var observed = Compile( new() { WorkObserver = steps.Add } ).Match( input, policy );
		Assert.True( observed.IsSuccess );
		Assert.Equal( new PcreByteCapture( 0, 3 ), observed.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 2 ), observed.Match?.GetCapture( "v" ) );
		Assert.True( steps.Count > 1 );
		foreach ( var observer in new Action<int>?[] { null, _ => { } } ) {
			var exact = Compile( new() { WorkObserver = observer, MatchWorkLimit = steps.Count } ).Match( input, policy );
			Assert.True( exact.IsSuccess );
			Assert.Equal( new PcreByteCapture( 0, 3 ), exact.Match?.Captures[0] );
			Assert.Equal( new PcreByteCapture( 0, 2 ), exact.Match?.GetCapture( "v" ) );
			var exhausted = Compile( new() { WorkObserver = observer, MatchWorkLimit = steps.Count - 1 } ).Match( input, policy );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
	}

	[Fact]
	public void CancellationPropagatesAfterTheRetriedAssertion() {
		var input = "abc"u8.ToArray();
		var policy = new PcreMatchPolicy { Anchored = true };
		var steps = new List<int>();
		var observed = Compile( new() { WorkObserver = steps.Add } ).Match( input, policy );
		Assert.Equal( new PcreByteCapture( 0, 3 ), observed.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 2 ), observed.Match?.GetCapture( "v" ) );
		using var source = new CancellationTokenSource();
		var cancelAt = steps.Count - 1;
		var pattern = Compile( new() { WorkObserver = step => {
			if ( step == cancelAt ) { source.Cancel(); }
		} } );
		Assert.Throws<OperationCanceledException>( () => pattern.Match( input, policy, 0, source.Token ) );
	}

	[Fact]
	public void CompiledAnchorsAndCompletedResultsKeepTheirOwnBehavior() {
		var input = "abc"u8.ToArray();
		for ( var defaults = 1; defaults <= 3; defaults++ ) {
			var flags = ( defaults & 1 ) != 0 ? 0x80000000u : 0;
			if ( ( defaults & 2 ) != 0 ) { flags |= 0x20000000u; }
			var reference = new PcreRegex8Bit( Encoding.UTF8.GetBytes( Pattern ), Encoding.UTF8, new PcreRegexSettings {
				Options = PcreOptions.Compiled | (PcreOptions)flags
			} );
			Assert.False( reference.Match( input ).Success );
			var native = reference.Match( input, 0, (PCRE.PcreMatchOptions)0x80000000u );
			Assert.True( native.Success );
			Assert.Equal( 0, native.Groups[2].Index );
			Assert.Equal( 2, native.Groups[2].Length );
			var pattern = Compile( new() { Anchored = ( defaults & 1 ) != 0, EndAnchored = ( defaults & 2 ) != 0 } );
			var baseline = pattern.Match( input );
			Assert.True( baseline.IsSuccess );
			Assert.Null( baseline.Match );
			var interpreted = pattern.Match( input, new PcreMatchPolicy { Anchored = true } ).Match;
			Assert.NotNull( interpreted );
			Assert.Equal( new PcreByteCapture( 0, 2 ), interpreted.GetCapture( "v" ) );
			Assert.Same( pattern.CaptureInfo, interpreted.CaptureInfo );
			Assert.Null( pattern.Match( input ).Match );
			Assert.Equal( new PcreByteCapture( 0, 2 ), interpreted.GetCapture( "v" ) );
		}
	}

	[Fact]
	public void AssertionRetriesReadTheOriginalSubjectView() {
		var padded = new byte[] { 0xFE, (byte)'z', (byte)'a', (byte)'b', (byte)'x', 0xFE };
		var pattern = Compile( new() { WorkObserver = step => {
			if ( step == 1 ) { padded[4] = (byte)'c'; }
		} } );
		var result = pattern.Match( padded.AsMemory( 1, 4 ), new PcreMatchPolicy { Anchored = true }, 1 );
		Assert.True( result.IsSuccess );
		Assert.Equal( new PcreByteCapture( 1, 3 ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 1, 2 ), result.Match?.GetCapture( "v" ) );
		Assert.Equal( 0xFE, padded[0] );
		Assert.Equal( 0xFE, padded[^1] );
	}

	private static PcreCompiledBytePattern Compile( PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( Pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
