using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreMatchPolicyTests {
	public static IEnumerable<object[]> Cases => PcreMatchPolicyCases.All.Select( c => new object[] { c.Id } );

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void AllNativePolicyCombinationsPrecedeTheReusableConsumer( int id ) {
		var item = PcreMatchPolicyCases.All.Single( c => c.Id == id );
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

	[Theory]
	[InlineData( -1 )]
	[InlineData( 2 )]
	public void InvalidStartIsStructuredBeforePolicyExecution( int offset ) {
		var result = Compile( "a", new() ).Match( "a"u8.ToArray(), new PcreMatchPolicy { Anchored = true }, offset );
		Assert.Null( result.Match ); Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, result.Diagnostic?.Code ); Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void StrictMalformedSubjectRemainsAnErrorUnderARestrictivePolicy() {
		var result = Compile( "", new() { Utf8 = true } ).Match( new byte[] { 0xFF }, new PcreMatchPolicy { Anchored = true, NotEmpty = true } );
		Assert.Null( result.Match ); Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, result.Diagnostic?.Code );
	}

	[Fact]
	public void StrictSplitStartRemainsAnError() {
		var result = Compile( ".", new() { Utf8 = true } ).Match( "é"u8.ToArray(), new PcreMatchPolicy { Anchored = true }, 1 );
		Assert.Null( result.Match ); Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, result.Diagnostic?.Code );
	}

	[Fact]
	public void PreCancellationPrecedesEvenAnInvalidOffset() {
		using var source = new CancellationTokenSource(); source.Cancel();
		var pattern = Compile( "a", new() );
		Assert.Throws<OperationCanceledException>( () => pattern.Match( "a"u8.ToArray(), new PcreMatchPolicy { NotEmpty = true }, -1, source.Token ) );
	}

	[Fact]
	public void RunningCancellationUsesTheExistingWorkHook() {
		using var source = new CancellationTokenSource();
		var pattern = Compile( "a+", new() { WorkObserver = step => { if ( step == 3 ) { source.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => pattern.Match( "aaaa"u8.ToArray(), new PcreMatchPolicy { Anchored = true }, cancellationToken: source.Token ) );
	}

	[Fact]
	public void RuntimeAndCompiledPoliciesHaveIdenticalWorkAndExactBoundaries() {
		var expectedSteps = new List<int>(); var actualSteps = new List<int>(); var bytes = "aa"u8.ToArray();
		var expectedOptions = new PcreByteOptions { Anchored = true, NotEmpty = true, NotBol = true, WorkObserver = expectedSteps.Add };
		var expected = Compile( "(?m)^a*", expectedOptions ).Match( bytes );
		var policy = new PcreMatchPolicy { Anchored = true, NotEmpty = true, NotBol = true };
		var actual = Compile( "(?m)^a*", new() { WorkObserver = actualSteps.Add } ).Match( bytes, policy );
		Assert.Equal( PcreMatchPolicyConsumer.Project( expected ), PcreMatchPolicyConsumer.Project( actual ) );
		Assert.Equal( expectedSteps, actualSteps ); Assert.NotEmpty( expectedSteps );
		Assert.True( Compile( "(?m)^a*", new() { MatchWorkLimit = expectedSteps.Count } ).Match( bytes, policy ).IsSuccess );
		var exhausted = Compile( "(?m)^a*", new() { MatchWorkLimit = expectedSteps.Count - 1 } ).Match( bytes, policy );
		Assert.Null( exhausted.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
	}

	[Fact]
	public void PolicyMatchingObservesTheOriginalPaddedView() {
		var bytes = new byte[] { 0xFF, (byte)'a', 0xFF };
		var pattern = Compile( "b", new() { WorkObserver = step => { if ( step == 1 ) { bytes[1] = (byte)'b'; } } } );
		var result = pattern.Match( bytes.AsMemory( 1, 1 ), new PcreMatchPolicy { Anchored = true } );
		Assert.True( result.IsSuccess ); Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match?.Captures[0] );
		Assert.Equal( 0xFF, bytes[0] ); Assert.Equal( 0xFF, bytes[2] );
	}

	[Fact]
	public void ExplicitDefaultPolicyAddsNoAllocationOverTheOriginalEntry() {
		var pattern = Compile( "a", new() ); var bytes = "a"u8.ToArray();
		for ( var i = 0; i < 8; i++ ) { _ = pattern.Match( bytes ); _ = pattern.Match( bytes, default( PcreMatchPolicy ) ); }
		var before = GC.GetAllocatedBytesForCurrentThread();
		for ( var i = 0; i < 8; i++ ) { _ = pattern.Match( bytes ); }
		var baseline = GC.GetAllocatedBytesForCurrentThread() - before;
		before = GC.GetAllocatedBytesForCurrentThread();
		for ( var i = 0; i < 8; i++ ) { _ = pattern.Match( bytes, default( PcreMatchPolicy ) ); }
		Assert.Equal( baseline, GC.GetAllocatedBytesForCurrentThread() - before );
	}

	[Fact]
	public async Task PoliciesAreIndependentAcrossConcurrentCalls() {
		var item = PcreMatchPolicyCases.All.Single( c => c.Id == 7 ); var expected = item.Outcomes.Split( ';' );
		var pattern = Compile( "(?:|a)", new() ); var bytes = "a"u8.ToArray(); using var source = new CancellationTokenSource();
		var tasks = Enumerable.Range( 0, 64 ).Select( mask => Task.Run( () => {
			source.Token.ThrowIfCancellationRequested();
			return PcreMatchPolicyConsumer.Project( pattern.Match( bytes, PcreMatchPolicyConsumer.Policy( mask ), cancellationToken: source.Token ) );
		}, source.Token ) ).ToArray();
		Assert.Equal( expected, await Task.WhenAll( tasks ) );
	}

	[Fact]
	public void RejectedLaterCallsPreserveTheEarlierBoundResult() {
		var pattern = Compile( "(?<x>a)", new() ); var bytes = "a"u8.ToArray();
		var first = pattern.Match( bytes, default( PcreMatchPolicy ) ).Match!;
		Assert.Null( pattern.Match( bytes, new PcreMatchPolicy { NotBol = true, NotEol = true, NotEmpty = true }, 1 ).Match );
		Assert.Same( pattern.CaptureInfo, first.CaptureInfo ); Assert.Equal( new PcreByteCapture( 0, 1 ), first.GetCapture( "x" ) );
	}

	private static PcreCompiledBytePattern Compile( string source, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), options ); Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
