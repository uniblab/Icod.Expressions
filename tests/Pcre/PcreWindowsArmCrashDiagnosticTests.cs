using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Temporary component isolation for the Windows ARM64 host crash.</summary>
public sealed class PcreWindowsArmCrashDiagnosticTests {
	private const string Pattern = @"(*ANYCRLF)(?!B){500}([\x09-\x0c\x{100}])A";

	[Fact]
	public void ManagedCountedAssertion500() {
		var compiled = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( Pattern ), new() { Utf8 = true } );
		Assert.True( compiled.IsSuccess );
		var result = compiled.Pattern!.Match( Convert.FromHexString( "0B41" ) );
		Assert.True( result.IsSuccess );
		Assert.Equal( new PcreByteCapture( 0, 2 ), result.Match?.Captures[0] );
		Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match?.Captures[1] );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( Pattern ), new() { Utf8 = true }, cancellation.Token ) );
	}

	[Fact]
	public void NativeCountedAssertion500() {
		var reference = new PcreRegex8Bit( Encoding.ASCII.GetBytes( Pattern ), Encoding.UTF8,
			new PcreRegexSettings { Options = PcreOptions.Compiled | PcreOptions.Utf, ParensLimit = 512 } );
		var result = reference.Match( Convert.FromHexString( "0B41" ) );
		Assert.True( result.Success );
		Assert.Equal( 0, result.Index );
		Assert.Equal( 2, result.Length );
		Assert.Equal( 0, result.Groups[1].Index );
		Assert.Equal( 1, result.Groups[1].Length );
	}
}
