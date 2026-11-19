using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreNamedCaptureResultTests {
	public static IEnumerable<object[]> Cases() => PcreNamedCaptureCases.All.Select( c => new object[] { c.Id } );

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void NativeNamedSubstringOutcomesPrecedeManagedResultLookups( int id ) {
		var item = PcreNamedCaptureCases.All.Single( c => c.Id == id );
		var basis = PcreCaptureInfoCases.All.Single( c => c.Id == item.BaseId );
		var bytes = Convert.FromHexString( basis.Subject );
		var name = Encoding.UTF8.GetString( Convert.FromHexString( item.Name ) );
		var regex = new PcreRegex8Bit( Convert.FromHexString( basis.Pattern ), new UTF8Encoding( false, true ), new PcreRegexSettings {
			Options = PcreOptions.Compiled | ( ( basis.Mode & 1 ) != 0 ? PcreOptions.Utf : PcreOptions.None )
				| ( ( basis.Mode & 2 ) != 0 ? PcreOptions.Ucp : PcreOptions.None ) | ( ( basis.Mode & 4 ) != 0 ? PcreOptions.Literal : PcreOptions.None )
				| ( ( basis.Mode & 8 ) != 0 ? PcreOptions.NoAutoCapture : PcreOptions.None ) | ( ( basis.Mode & 16 ) != 0 ? PcreOptions.DupNames : PcreOptions.None ),
			NewLine = (PcreNewLine)2, BackslashR = (PcreBackslashR)1, ParensLimit = 512
		} );
		var native = regex.Match( bytes );
		var spans = new List<string>();
		if ( native.Success ) { for ( var i = 0; i < native.Groups.Count; i++ ) { var g = native.Groups[i]; spans.Add( g.Success ? $"{g.Index}:{g.EndIndex - g.Index}" : "-" ); } }
		Assert.Equal( basis.Captures, String.Join( ",", spans ) );
		var kind = "N"; var span = ""; var value = "";
		if ( native.Success ) {
			kind = "A";
			if ( regex.PatternInfo.GroupNames.Contains( name, StringComparer.Ordinal ) ) {
				kind = "U";
				foreach ( var number in regex.PatternInfo.GetGroupIndexesByName( name ) ) {
					var g = native.Groups[number];
					if ( !g.Success ) { continue; }
					kind = "M"; span = $"{g.Index}:{g.EndIndex - g.Index}";
					value = Convert.ToHexString( bytes.AsSpan( g.Index, g.EndIndex - g.Index ) ); break;
				}
			}
		}
		Assert.Equal( new PcreNamedCaptureProtocol( item.Kind, item.Span, item.Value ), new PcreNamedCaptureProtocol( kind, span, value ) );
		Assert.Equal( new PcreNamedCaptureProtocol( item.Kind, item.Span, item.Value ), PcreNamedCaptureConsumer.Run( item ) );
	}

	[Theory]
	[InlineData( "" )]
	[InlineData( "X" )]
	[InlineData( "x\0" )]
	public void UnknownQueriesUseExactOrdinalNames( string name ) {
		var match = Compile( @"(?<x>a)" ).Match( "a"u8.ToArray() ).Match!;
		Assert.Null( match.GetCapture( name ) );
		Assert.Equal( new PcreByteCapture( 0, 1 ), match.GetCapture( "x" ) );
	}

	[Fact]
	public void NullNameIsAnArgumentError() {
		var match = Compile( @"(?<x>a)" ).Match( "a"u8.ToArray() ).Match!;
		Assert.Throws<ArgumentNullException>( () => match.GetCapture( null! ) );
	}

	[Fact]
	public void ResultsKeepTheirMetadataAndCapturesAcrossSubsequentMatches() {
		var pattern = Compile( @"(?J)(?<x>a)|(?<x>b)" );
		var first = pattern.Match( "zzb"u8.ToArray() ).Match!;
		Assert.Equal( new PcreByteCapture( 2, 1 ), first.GetCapture( "x" ) );
		Assert.Same( pattern.CaptureInfo, first.CaptureInfo );
		var second = pattern.Match( "a"u8.ToArray() ).Match!;
		Assert.Equal( new PcreByteCapture( 0, 1 ), second.GetCapture( "x" ) );
		_ = Compile( @"(?<other>z)" ).Match( "z"u8.ToArray() );
		Assert.Equal( new PcreByteCapture( 2, 1 ), first.GetCapture( "x" ) );
		Assert.Equal( new[] { "x" }, first.CaptureInfo.Names );
	}

	[Fact]
	public void TolerantEmptyFastPathAlsoBindsTheCompiledMetadata() {
		var result = PcreByteCompiler.Compile( Array.Empty<byte>(), new PcreByteOptions { MatchInvalidUtf = true } );
		Assert.True( result.IsSuccess );
		var match = result.Pattern!.Match( new byte[] { 0xFF }, 1 ).Match!;
		Assert.Same( result.Pattern.CaptureInfo, match.CaptureInfo );
		Assert.Equal( 0, match.CaptureInfo.CaptureCount );
		Assert.Null( match.GetCapture( "missing" ) );
	}

	[Fact]
	public void WarmedNamedAndMissingLookupsDoNotAllocate() {
		var match = Compile( @"(?J)(?<x>a)|(?<x>b)" ).Match( "b"u8.ToArray() ).Match!;
		for ( var i = 0; i < 64; i++ ) { Read(); }
		var before = GC.GetAllocatedBytesForCurrentThread();
		for ( var i = 0; i < 1024; i++ ) { Read(); }
		var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
		Assert.Equal( 0L, allocated );
		void Read() { if ( match.GetCapture( "x" ) is not { ByteIndex: 0, ByteLength: 1 } || match.GetCapture( "missing" ) is not null ) { throw new InvalidOperationException( "named lookup changed" ); } }
	}

	[Fact]
	public void PrecancelledMatchingPrecedesResultBinding() {
		var pattern = Compile( @"(?<x>a)" ); using var source = new CancellationTokenSource(); source.Cancel();
		Assert.Throws<OperationCanceledException>( () => pattern.Match( "a"u8.ToArray(), cancellationToken: source.Token ) );
	}

	[Fact]
	public void WorkExhaustionDoesNotReturnABoundSuccess() {
		var result = PcreByteCompiler.Compile( "(?<x>a)"u8.ToArray(), new PcreByteOptions { MatchWorkLimit = 1 } );
		Assert.True( result.IsSuccess ); var match = result.Pattern!.Match( "a"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code );
		Assert.Null( match.Match );
	}

	private static PcreCompiledBytePattern Compile( string pattern ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ) ); Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
