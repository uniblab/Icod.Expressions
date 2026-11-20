using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreFragmentCaretTests {
	public static IEnumerable<object[]> Cases => PcreFragmentCaretCases.All.Select( c => new object[] { c.Id } );

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void NativeCaretBoundaryProtocolsPrecedeManagedComparisons( int id ) {
		var item = PcreFragmentCaretCases.All.Single( c => c.Id == id );
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
	public void RejectedFragmentAndLaterMatchShareOneWorkBudget() {
		var input = new byte[] { (byte)'a', (byte)'\n', 0xFF, (byte)'b' };
		var policy = new PcreMatchPolicy { Anchored = true };
		var steps = new List<int>();
		var observed = Compile( "(?m)a\n^|b", new() { MatchInvalidUtf = true, WorkObserver = steps.Add } ).Match( input, policy );
		Assert.Equal( new PcreByteCapture( 3, 1 ), observed.Match?.Captures[0] );
		var count = steps.Count; Assert.True( count > 1 );
		foreach ( var observer in new Action<int>?[] { null, _ => { } } ) {
			var exact = Compile( "(?m)a\n^|b", new() { MatchInvalidUtf = true, WorkObserver = observer, MatchWorkLimit = count } ).Match( input, policy );
			Assert.True( exact.IsSuccess ); Assert.Equal( new PcreByteCapture( 3, 1 ), exact.Match?.Captures[0] );
			var exhausted = Compile( "(?m)a\n^|b", new() { MatchInvalidUtf = true, WorkObserver = observer, MatchWorkLimit = count - 1 } ).Match( input, policy );
			Assert.Null( exhausted.Match ); Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
	}

	[Fact]
	public void CancellationCanInterruptTheLaterCaretAlternative() {
		var input = new byte[] { (byte)'a', (byte)'\n', 0xFF, (byte)'b' };
		var policy = new PcreMatchPolicy { Anchored = true };
		var steps = new List<int>();
		var observed = Compile( "(?m)a\n^|b", new() { MatchInvalidUtf = true, WorkObserver = steps.Add } ).Match( input, policy );
		Assert.Equal( new PcreByteCapture( 3, 1 ), observed.Match?.Captures[0] );
		var cancelAt = steps.Count - 1; using var source = new CancellationTokenSource();
		var pattern = Compile( "(?m)a\n^|b", new() { MatchInvalidUtf = true, WorkObserver = step => { if ( step == cancelAt ) { source.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => pattern.Match( input, policy, cancellationToken: source.Token ) );
	}

	[Fact]
	public void RejectedCaretCapturesDoNotLeakOrMutatePreviousResults() {
		var pattern = Compile( "(?m)(?<early>a\n)^|(?<later>b)", new() { MatchInvalidUtf = true } );
		var input = new byte[] { (byte)'a', (byte)'\n', 0xFF, (byte)'b' };
		var first = pattern.Match( input, new PcreMatchPolicy { EndAnchored = true } ).Match;
		Assert.NotNull( first ); Assert.Null( first.GetCapture( "early" ) );
		Assert.Equal( new PcreByteCapture( 3, 1 ), first.GetCapture( "later" ) );
		Assert.Same( pattern.CaptureInfo, first.CaptureInfo );
		var original = pattern.Match( input ).Match;
		Assert.NotNull( original ); Assert.Equal( new PcreByteCapture( 0, 2 ), original.GetCapture( "early" ) );
		Assert.Null( original.GetCapture( "later" ) );
		Assert.Equal( new PcreByteCapture( 3, 1 ), first.GetCapture( "later" ) );
		Assert.Null( first.GetCapture( "early" ) );
	}

	[Fact]
	public void CaretFragmentTraversalReadsTheOriginalSubjectView() {
		var padded = new byte[] { 0xFF, (byte)'a', (byte)'\n', 0xFF, (byte)'x', 0xFF };
		var pattern = Compile( "(?m)a\n^|b", new() { MatchInvalidUtf = true, WorkObserver = step => { if ( step == 1 ) { padded[4] = (byte)'b'; } } } );
		var result = pattern.Match( padded.AsMemory( 1, 4 ), new PcreMatchPolicy { Anchored = true } );
		Assert.True( result.IsSuccess ); Assert.Equal( new PcreByteCapture( 3, 1 ), result.Match?.Captures[0] );
		Assert.Equal( 0xFF, padded[0] ); Assert.Equal( 0xFF, padded[^1] );
	}

	private static PcreCompiledBytePattern Compile( string source, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message ); return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
