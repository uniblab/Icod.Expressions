using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreInterpretedFirstLineTests {
	private const string Pattern = @"a(?s:.)(*SKIP)(*F)|Y";
	private const string Subject = "610A62FF63610A6259";

	public static IEnumerable<object[]> Cases => PcreInterpretedFirstLineCases.All.Select( item => new object[] { item.Id } );

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void NativeInterpretedFirstLineProtocolsPrecedeManagedComparisons( int id ) {
		var item = PcreInterpretedFirstLineCases.All.Single( definition => definition.Id == id );
		var flags = ( ( item.Mode & 1 ) != 0 ? 0x80000u : 0 ) | ( ( item.Mode & 2 ) != 0 ? 0x20000u : 0 )
			| ( ( item.Mode & 4 ) != 0 ? 0x4000000u : 0 ) | ( ( item.Mode & 512 ) != 0 ? 0x100u : 0 )
			| ( ( item.Defaults & 1 ) != 0 ? 0x80000000u : 0 ) | ( ( item.Defaults & 2 ) != 0 ? 0x20000000u : 0 );
		var reference = new PcreRegex8Bit( Convert.FromHexString( item.Pattern ), Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | (PcreOptions)flags,
			NewLine = (PcreNewLine)( ( item.Mode & 64 ) != 0 ? 5 : 2 ), BackslashR = (PcreBackslashR)1, ParensLimit = 512
		} );
		var subject = Convert.FromHexString( item.Subject );
		var protocols = item.Outcomes.Split( ';' );
		Assert.Equal( 64, protocols.Length );
		for ( var mask = 0; mask < protocols.Length; mask++ ) {
			var match = reference.Match( subject, item.Start, PcreNativeMatchPolicy.Options( item.Defaults, mask ) );
			var captures = new List<string>();
			if ( match.Success ) {
				for ( var index = 0; index < match.Groups.Count; index++ ) {
					var group = match.Groups[index];
					captures.Add( group.Success ? $"{group.Index}:{group.EndIndex - group.Index}" : "-" );
				}
			}
			var mark = match.Success ? Convert.ToHexString( match.Mark.ToArray() ) : "";
			Assert.Equal( protocols[mask], String.Concat( String.Join( ",", captures ), "/", mark ) );
		}
		Assert.Equal( item.Outcomes, PcreMatchPolicyConsumer.Run( item ) );
	}

	[Fact]
	public void InterpretedFirstLineRestartsShareTheMatchWorkBudget() {
		var input = Convert.FromHexString( Subject );
		var policy = new PcreMatchPolicy { EndAnchored = true };
		var work = 0;
		var options = new PcreByteOptions { FirstLine = true, MatchInvalidUtf = true };
		var observed = Compile( options with { WorkObserver = step => work = step } ).Match( input, policy );
		Assert.Equal( "8:1/", PcreMatchPolicyConsumer.Project( observed ) );
		Assert.True( work > input.Length );
		foreach ( var observer in new Action<int>?[] { null, _ => { } } ) {
			var exact = Compile( options with { WorkObserver = observer, MatchWorkLimit = work } ).Match( input, policy );
			Assert.Equal( "8:1/", PcreMatchPolicyConsumer.Project( exact ) );
			var exhausted = Compile( options with { WorkObserver = observer, MatchWorkLimit = work - 1 } ).Match( input, policy );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
	}

	[Fact]
	public void InterpretedFirstLineRestartsPropagateCancellation() {
		var input = Convert.FromHexString( Subject );
		var policy = new PcreMatchPolicy { EndAnchored = true };
		var work = 0;
		var options = new PcreByteOptions { FirstLine = true, MatchInvalidUtf = true };
		var observed = Compile( options with { WorkObserver = step => work = step } ).Match( input, policy );
		Assert.Equal( "8:1/", PcreMatchPolicyConsumer.Project( observed ) );
		Assert.True( work > input.Length );
		using var source = new CancellationTokenSource();
		var pattern = Compile( options with { WorkObserver = step => {
			if ( step == work - 1 ) { source.Cancel(); }
		} } );
		Assert.Throws<OperationCanceledException>( () => pattern.Match( input, policy, 0, source.Token ) );
	}

	private static PcreCompiledBytePattern Compile( PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( Pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
