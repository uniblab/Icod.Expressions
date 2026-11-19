using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreCaptureInfoTests {
	public static IEnumerable<object[]> Cases() => PcreCaptureInfoCases.All.Select( c => new object[] { c.Id } );

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void NativeCaptureMetadataAndMatchesPrecedeTheManagedConsumer( int id ) {
		var item = PcreCaptureInfoCases.All.Single( c => c.Id == id );
		var native = new PcreRegex8Bit( Convert.FromHexString( item.Pattern ), new UTF8Encoding( false, true ), new PcreRegexSettings {
			Options = PcreOptions.Compiled | ( ( item.Mode & 1 ) != 0 ? PcreOptions.Utf : PcreOptions.None )
				| ( ( item.Mode & 2 ) != 0 ? PcreOptions.Ucp : PcreOptions.None ) | ( ( item.Mode & 4 ) != 0 ? PcreOptions.Literal : PcreOptions.None )
				| ( ( item.Mode & 8 ) != 0 ? PcreOptions.NoAutoCapture : PcreOptions.None ) | ( ( item.Mode & 16 ) != 0 ? PcreOptions.DupNames : PcreOptions.None ),
			NewLine = (PcreNewLine)2, BackslashR = (PcreBackslashR)1, ParensLimit = 512
		} );
		Assert.Equal( item.CaptureCount, native.PatternInfo.CaptureCount );
		var names = String.Join( ";", native.PatternInfo.GroupNames.OrderBy( n => n, StringComparer.Ordinal ).Select( n =>
			String.Concat( Convert.ToHexString( Encoding.UTF8.GetBytes( n ) ), "=", String.Join( ",", native.PatternInfo.GetGroupIndexesByName( n ).Distinct().OrderBy( v => v ) ) ) ) );
		Assert.Equal( item.Names, names );
		var match = native.Match( Convert.FromHexString( item.Subject ) );
		var spans = new List<string>();
		if ( match.Success ) {
			for ( var i = 0; i < match.Groups.Count; i++ ) { var g = match.Groups[i]; spans.Add( g.Success ? $"{g.Index}:{g.EndIndex - g.Index}" : "-" ); }
		}
		Assert.Equal( item.Captures.Length > 0, match.Success );
		Assert.Equal( item.Captures, String.Join( ",", spans ) );
		Assert.Equal( new PcreCaptureInfoProtocol( item.CaptureCount, item.Names, item.Captures ), PcreCaptureInfoConsumer.Run( item ) );
	}

	[Theory]
	[InlineData( "" )]
	[InlineData( "X" )]
	[InlineData( "x\0" )]
	public void UnknownNamesReturnEmptyWithoutChangingTheKnownMapping( string name ) {
		var info = Compile( @"(?J)(?<x>a)|(?<x>b)" ).CaptureInfo;
		Assert.Empty( info.GetGroupNumbers( name ) );
		Assert.Equal( new[] { 1, 2 }, info.GetGroupNumbers( "x" ) );
		Assert.Equal( new[] { "x" }, info.Names );
	}

	[Fact]
	public void NullLookupIsAnArgumentError() {
		var info = Compile( @"(?<x>a)" ).CaptureInfo;
		Assert.Throws<ArgumentNullException>( () => info.GetGroupNumbers( null! ) );
	}

	[Fact]
	public void ExposedNameAndNumberCollectionsCannotMutateCompiledMetadata() {
		var info = Compile( @"(?J)(?<x>a)|(?<x>b)" ).CaptureInfo;
		Assert.False( info.Names is string[] );
		Assert.False( info.GetGroupNumbers( "x" ) is int[] );
		if ( info.Names is IList<string> names ) {
			Assert.True( names.IsReadOnly );
			Assert.Throws<NotSupportedException>( () => names[0] = "changed" );
			Assert.Throws<NotSupportedException>( () => names.Add( "changed" ) );
		}
		if ( info.GetGroupNumbers( "x" ) is IList<int> numbers ) {
			Assert.True( numbers.IsReadOnly );
			Assert.Throws<NotSupportedException>( () => numbers[0] = 99 );
			Assert.Throws<NotSupportedException>( () => numbers.Add( 99 ) );
		}
		Assert.Equal( new[] { "x" }, info.Names );
		Assert.Equal( new[] { 1, 2 }, info.GetGroupNumbers( "x" ) );
	}

	[Fact]
	public void MetadataIdentityAndLookupViewsSurviveMatchesAndOtherCompilations() {
		var pattern = Compile( @"(?J)(?<x>a)|(?<x>b)" );
		var info = pattern.CaptureInfo;
		var numbers = info.GetGroupNumbers( "x" );
		Assert.NotNull( pattern.Match( "b"u8.ToArray() ).Match );
		Assert.Null( pattern.Match( "z"u8.ToArray() ).Match );
		Assert.Equal( 1, Compile( @"(?<other>z)" ).CaptureInfo.CaptureCount );
		Assert.Same( info, pattern.CaptureInfo );
		Assert.Same( numbers, info.GetGroupNumbers( "x" ) );
		Assert.Equal( new[] { 1, 2 }, numbers );
	}

	[Fact]
	public void MetadataRemainsAvailableAfterAMatchWorkDiagnostic() {
		var result = PcreByteCompiler.Compile( "(?<x>a)"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 1 } );
		Assert.True( result.IsSuccess );
		var info = result.Pattern!.CaptureInfo;
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, result.Pattern.Match( "a"u8.ToArray() ).Diagnostic?.Code );
		Assert.Same( info, result.Pattern.CaptureInfo );
		Assert.Equal( 1, info.CaptureCount );
		Assert.Equal( new[] { 1 }, info.GetGroupNumbers( "x" ) );
	}

	[Fact]
	public void RepeatedMetadataReadsAndKnownLookupsDoNotAllocate() {
		var pattern = Compile( @"(?J)(?<x>a)|(?<x>b)" );
		for ( var i = 0; i < 64; i++ ) { Read(); }
		var before = GC.GetAllocatedBytesForCurrentThread();
		for ( var i = 0; i < 1024; i++ ) { Read(); }
		var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.Equal( 0L, allocated );
		void Read() {
			var info = pattern.CaptureInfo;
			if ( info.CaptureCount != 2 || info.Names.Count != 1 || info.GetGroupNumbers( "x" ).Count != 2 ) { throw new InvalidOperationException( "metadata changed" ); }
		}
	}

	[Fact]
	public void PrecancelledCompilationStillPrecedesMetadataCreation() {
		using var source = new CancellationTokenSource();
		source.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( "(?<x>a)"u8.ToArray(), cancellationToken: source.Token ) );
	}

	private static PcreCompiledBytePattern Compile( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ) );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
