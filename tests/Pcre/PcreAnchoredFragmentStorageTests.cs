using System.Globalization;
using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreAnchoredFragmentStorageTests {
	public static IEnumerable<object[]> Cases() => Enumerable.Range( 0, PcreAnchoredFragmentCases.All.Count ).Select( i => new object[] { i } );
	public static IEnumerable<object[]> Profiles() => PcreFragmentAllocationCases.All.SelectMany( p => Enumerable.Range( 0, 3 ).Select( path => new object[] { p.Name, path } ) );

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void NativeStartAndContextProtocolsPrecedeManagedChecks( int index ) {
		var item = PcreAnchoredFragmentCases.All[index];
		var bytes = Convert.FromHexString( item.Subject );
		var backing = new byte[bytes.Length + 2];
		backing[0] = backing[^1] = 0xFE;
		bytes.CopyTo( backing, 1 );
		var native = Reference( item.Pattern, item.Anchored );
		var compiled = Compile( item.Pattern, new PcreByteOptions { MatchInvalidUtf = true, Anchored = item.Anchored } );
		Assert.Equal( bytes.Length + 1, item.Expected.Length );
		for ( var start = 0; start <= bytes.Length; start++ ) {
			Assert.Equal( item.Expected[start], Native( native, bytes, start ) );
			var result = compiled.Match( backing.AsMemory( 1, bytes.Length ), start );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			var actual = result.Match is { } match ? String.Concat( "M|", String.Join( ",", match.Captures.Select( c => c is { } v
				? String.Concat( v.ByteIndex.ToString( CultureInfo.InvariantCulture ), ":", v.ByteLength.ToString( CultureInfo.InvariantCulture ) ) : "-" ) ) ) : "N|";
			Assert.True( item.Expected[start] == actual, $"case {index}, start {start}: expected {item.Expected[start]}, actual {actual}" );
		}
	}

	[Theory]
	[MemberData( nameof( Profiles ) )]
	public void AnchoredFragmentsHaveFixedAllocationGrowthAfterNativeChecks( string name, int path ) {
		var profile = PcreFragmentAllocationCases.All.Single( p => p.Name == name );
		var native = Reference( profile.Pattern, profile.Anchored );
		foreach ( var size in new[] { 1024, 65536 } ) {
			var input = PcreFragmentAllocationCases.Input( size ).AsSpan( 1, size ).ToArray();
			var expected = size == 1024 ? profile.SmallCaptures : profile.LargeCaptures;
			Assert.Equal( String.Concat( expected.Length > 0 ? "M|" : "N|", expected ), Native( native, input, PcreFragmentAllocationCases.Offset( profile, size ) ) );
		}
		var result = PcreFragmentAllocationConsumer.Run( profile, path );
		Assert.Equal( 8, result.Metrics.Iterations );
		Assert.True( result.Metrics.SmallAllocatedBytes >= 0 && result.Metrics.LargeAllocatedBytes >= 0 );
		Assert.True( result.Metrics.LargeAllocatedBytes - result.Metrics.SmallAllocatedBytes <= 4096L * 8,
			$"{name}, path {path}: small={result.Metrics.SmallAllocatedBytes}, large={result.Metrics.LargeAllocatedBytes}" );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void AnchoredMatchStillPaysTheWholeValidationBudget( bool late, bool observed ) {
		var last = 0;
		var pattern = Compile( "(A)", new PcreByteOptions { MatchInvalidUtf = true, Anchored = true, MatchWorkLimit = 1023,
			WorkObserver = observed ? count => { Assert.Equal( last + 1, count ); last = count; } : null } );
		var result = pattern.Match( PcreFragmentAllocationCases.Input( 1024 ).AsMemory( 1, 1024 ), late ? 1022 : 0 );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code );
		Assert.Null( result.Match );
		Assert.Equal( observed ? 1023 : 0, last );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void CancellationAtFinalValidationStepPrecedesAnyAnchoredResult( bool late ) {
		using var source = new CancellationTokenSource();
		var last = 0;
		var pattern = Compile( "(A)", new PcreByteOptions { MatchInvalidUtf = true, Anchored = true,
			WorkObserver = count => { last = count; if ( count == 1024 ) { source.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => pattern.Match( PcreFragmentAllocationCases.Input( 1024 ).AsMemory( 1, 1024 ), late ? 1022 : 0, source.Token ) );
		Assert.Equal( 1024, last );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1022 )]
	public void StrictUtfStillReportsTheFirstLateInvalidByte( int start ) {
		var subject = Enumerable.Repeat( (byte)'A', 1024 ).ToArray();
		subject[^1] = 0xFF;
		var last = 0;
		var pattern = Compile( "(A)", new PcreByteOptions { Utf8 = true, Anchored = true, WorkObserver = count => { last = count; } } );
		var result = pattern.Match( subject, start );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, result.Diagnostic?.Code );
		Assert.Equal( 1023, result.Diagnostic?.ByteOffset );
		Assert.Equal( 1024, last );
		Assert.Null( result.Match );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}

	private static PcreRegex8Bit Reference( string pattern, bool anchored ) => new( Encoding.ASCII.GetBytes( pattern ), new UTF8Encoding( false, true ), new PcreRegexSettings {
		Options = PcreOptions.Compiled | PcreOptions.Utf | PcreOptions.MatchInvalidUtf | ( anchored ? PcreOptions.Anchored : PcreOptions.None ),
		NewLine = (PcreNewLine)2, BackslashR = (PcreBackslashR)1, ParensLimit = 512
	} );

	private static string Native( PcreRegex8Bit native, byte[] subject, int start ) {
		var match = native.Match( subject, start );
		if ( !match.Success ) { return "N|"; }
		var spans = new List<string>();
		for ( var i = 0; i < match.Groups.Count; i++ ) {
			var group = match.Groups[i];
			spans.Add( group.Success ? $"{group.Index}:{group.EndIndex - group.Index}" : "-" );
		}
		return String.Concat( "M|", String.Join( ",", spans ) );
	}
}
