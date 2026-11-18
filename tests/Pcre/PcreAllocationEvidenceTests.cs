using System.Runtime.InteropServices;
using System.Text;
using Icod.Expressions.PackageSmoke;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreAllocationEvidenceTests {
	public static IEnumerable<object[]> Profiles() => PcreAllocationCases.All.Select( p => new object[] { p.Name } );

	[Theory]
	[MemberData( nameof( Profiles ) )]
	public void NativeProtocolsPrecedePairedConsumerAllocationChecks( string name ) {
		var profile = PcreAllocationCases.All.Single( p => p.Name == name );
		var reference = new PcreRegex8Bit( Encoding.ASCII.GetBytes( profile.Pattern ), new UTF8Encoding( false, true ), new PcreRegexSettings {
			Options = PcreOptions.Compiled | PcreOptions.Anchored
				| ( ( profile.Mode & 1 ) != 0 ? PcreOptions.Utf : PcreOptions.None )
				| ( ( profile.Mode & 2 ) != 0 ? PcreOptions.Ucp : PcreOptions.None ),
			NewLine = (PcreNewLine)2, BackslashR = (PcreBackslashR)1, ParensLimit = 512
		} );
		foreach ( var size in new[] { PcreAllocationCases.SmallSize, PcreAllocationCases.LargeSize } ) {
			var match = reference.Match( PcreAllocationCases.Input( profile, size ) );
			var spans = new List<string>();
			if ( match.Success ) {
				for ( var i = 0; i < match.Groups.Count; i++ ) {
					var group = match.Groups[i];
					spans.Add( group.Success ? $"{group.Index}:{group.EndIndex - group.Index}" : "-" );
				}
			}
			Assert.Equal( profile.Captures.Length > 0, match.Success );
			Assert.Equal( profile.Captures, String.Join( ",", spans ) );
		}
		var evidence = PcreAllocationConsumer.Run( profile );
		Assert.Equal( profile.Name, evidence.Name );
		Assert.Equal( 1024, evidence.SmallInputBytes );
		Assert.Equal( 65536, evidence.LargeInputBytes );
		Assert.Equal( 8, evidence.Metrics.Iterations );
		Assert.InRange( evidence.Metrics.SmallAllocatedBytes, 0, Int64.MaxValue );
		Assert.InRange( evidence.Metrics.LargeAllocatedBytes, 0, Int64.MaxValue );
		Assert.True( evidence.Metrics.LargeAllocatedBytes - evidence.Metrics.SmallAllocatedBytes <= 4096L * 8 );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void DeliberateWholeInputCopyAndDecodeControlsExposeLinearGrowth( bool decode ) {
		var small = Enumerable.Repeat( (byte)'A', 1024 ).ToArray();
		var large = Enumerable.Repeat( (byte)'A', 65536 ).ToArray();
		var result = PcreAllocationProbe.Measure( small, large, input => {
			if ( decode ) { GC.KeepAlive( Encoding.UTF8.GetString( input.Span ) ); }
			else { GC.KeepAlive( input.ToArray() ); }
		}, 4 );
		Assert.Equal( 4, result.Iterations );
		Assert.True( result.LargeAllocatedBytes - result.SmallAllocatedBytes >= 4L * ( 65536 - 1024 ) );
		Assert.True( result.LargeAllocatedBytes - result.SmallAllocatedBytes > 4096L * 4 );
	}

	[Theory]
	[InlineData( 1 )]
	[InlineData( 8 )]
	[InlineData( 32 )]
	public void WarmupAndMeasuredVisitsAreBoundedWithoutProbeAllocations( int iterations ) {
		var calls = 0;
		var result = PcreAllocationProbe.Measure( new byte[1], new byte[2], _ => { calls++; }, iterations );
		Assert.Equal( 16 + 2 * iterations, calls );
		Assert.Equal( iterations, result.Iterations );
		Assert.Equal( 0L, result.SmallAllocatedBytes );
		Assert.Equal( 0L, result.LargeAllocatedBytes );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	[InlineData( 4 )]
	[InlineData( 5 )]
	public void InvalidArgumentsDoNotInvokeTheCallback( int selection ) {
		var calls = 0;
		Action<ReadOnlyMemory<byte>> visit = _ => { calls++; };
		if ( selection == 0 ) { Assert.Throws<ArgumentNullException>( () => PcreAllocationProbe.Measure( new byte[1], new byte[2], null! ) ); }
		else if ( selection == 1 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreAllocationProbe.Measure( new byte[1], new byte[2], visit, 0 ) ); }
		else if ( selection == 2 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreAllocationProbe.Measure( new byte[1], new byte[2], visit, 33 ) ); }
		else if ( selection == 3 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreAllocationProbe.Measure( new byte[8], new byte[8], visit ) ); }
		else if ( selection == 4 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreAllocationProbe.Measure( new byte[8], new byte[4], visit ) ); }
		else { Assert.Throws<ArgumentOutOfRangeException>( () => PcreAllocationProbe.Measure( Array.Empty<byte>(), new byte[262145], visit ) ); }
		Assert.Equal( 0, calls );
	}

	[Fact]
	public void PrecancelledMeasurementsDoNotInvokeTheCallback() {
		using var source = new CancellationTokenSource();
		source.Cancel();
		var calls = 0;
		Assert.Throws<OperationCanceledException>( () => PcreAllocationProbe.Measure( new byte[1], new byte[2], _ => { calls++; }, cancellationToken: source.Token ) );
		Assert.Equal( 0, calls );
	}

	[Fact]
	public void CallbackCancellationPropagatesBeforeAnotherVisit() {
		using var source = new CancellationTokenSource();
		var calls = 0;
		Assert.Throws<OperationCanceledException>( () => PcreAllocationProbe.Measure( new byte[1], new byte[2], _ => { calls++; source.Cancel(); }, cancellationToken: source.Token ) );
		Assert.Equal( 1, calls );
	}

	[Fact]
	public void CallbackErrorsPropagateWithoutBecomingMeasurements() {
		var failure = new InvalidOperationException( "measurement callback failed" );
		Assert.Same( failure, Assert.Throws<InvalidOperationException>( () => PcreAllocationProbe.Measure( new byte[1], new byte[2], _ => throw failure ) ) );
	}

	[Fact]
	public void CallbacksReceiveTheOriginalVisibleMemoryViews() {
		var small = new byte[] { 255, 65, 254 };
		var large = new byte[] { 255, 66, 67, 254 };
		var calls = 0;
		_ = PcreAllocationProbe.Measure( small.AsMemory( 1, 1 ), large.AsMemory( 1, 2 ), input => {
			Assert.True( MemoryMarshal.TryGetArray( input, out var segment ) );
			Assert.Same( input.Length == 1 ? small : large, segment.Array );
			Assert.Equal( 1, segment.Offset );
			Assert.Equal( input.Length, segment.Count );
			calls++;
		}, 1 );
		Assert.Equal( 18, calls );
	}

	[Fact]
	public void EmptySmallViewIsAValidBaseline() {
		var result = PcreAllocationProbe.Measure( ReadOnlyMemory<byte>.Empty, new byte[1], _ => { }, 1 );
		Assert.Equal( 1, result.Iterations );
		Assert.Equal( 0L, result.SmallAllocatedBytes );
		Assert.Equal( 0L, result.LargeAllocatedBytes );
	}

	[Fact]
	public void MaximumLargeViewIsAccepted() {
		var result = PcreAllocationProbe.Measure( new byte[1], new byte[262144], _ => { }, 1 );
		Assert.Equal( 1, result.Iterations );
		Assert.Equal( 0L, result.SmallAllocatedBytes );
		Assert.Equal( 0L, result.LargeAllocatedBytes );
	}
}
