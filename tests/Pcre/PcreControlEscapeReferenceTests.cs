using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins printable-ASCII control escape operands without rewriting source offsets.</summary>
public sealed class PcreControlEscapeReferenceTests {
	private const string Values = "606162636465666768696A6B6C6D6E6F707172737475767778797A7B7C7D7E7F"
		+ "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F"
		+ "200102030405060708090A0B0C0D0E0F101112131415161718191A3B3C3D3E";

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryPrintableOperandHasPinnedLiteralBehaviorInAllContexts( bool utf ) {
		var values = Convert.FromHexString( Values );
		Assert.Equal( 95, values.Length );
		for ( var operand = 0x20; operand <= 0x7e; operand++ ) {
			var atom = String.Concat( @"\c", ( (char)operand ).ToString() );
			var value = values[operand - 0x20];
			foreach ( var (pattern, subject, spans) in new[] {
				(String.Concat( @"\A(", atom, @")\z" ), new[] { value }, "0:1,0:1"),
				(String.Concat( @"\A[", atom, @"]\z" ), new[] { value }, "0:1"),
				(String.Concat( @"\A[^", atom, @"]\z" ), new[] { value == 0 ? (byte)1 : (byte)0 }, "0:1"),
				(String.Concat( @"\A(?=", atom, ")", atom, @"\z" ), new[] { value }, "0:1"),
				(String.Concat( @"(?<=", atom, ")X" ), new[] { value, (byte)'X' }, "1:1"),
				(String.Concat( @"\A", atom, @"{2}\z" ), new[] { value, value }, "0:2")
			} ) {
				var reference = Reference( pattern, utf );
				AssertLiteral( reference, subject, 0, spans );
				Compare( reference, subject, 0, Compile( pattern, new() { Utf8 = utf, Ucp = utf } ).Match( subject ), pattern );
			}
		}
	}

	[Theory]
	[InlineData( @"\c@", "00", "0:1" )]
	[InlineData( @"\ca", "01", "0:1" )]
	[InlineData( @"\c?", "7F", "0:1" )]
	[InlineData( @"\c ", "60", "0:1" )]
	[InlineData( @"\c!", "61", "0:1" )]
	[InlineData( @"\c1", "71", "0:1" )]
	[InlineData( @"\c`", "20", "0:1" )]
	[InlineData( @"\c{2}", "3B327D", "0:3" )]
	[InlineData( @"\cA{2}", "0101", "0:2" )]
	[InlineData( @"(\c))", "69", "0:1,0:1" )]
	[InlineData( @"[\c]]", "1D", "0:1" )]
	[InlineData( @"[\c[]", "1B", "0:1" )]
	[InlineData( @"[\c\]", "1C", "0:1" )]
	[InlineData( @"\c\Q", "1C51", "0:2" )]
	[InlineData( @"\Q\cA\E", "5C6341", "0:3" )]
	[InlineData( @"[\Q\cA\E]", "5C", "0:1" )]
	[InlineData( @"(?x)\c A", "6041", "0:2" )]
	[InlineData( @"(?x)\c#A", "6341", "0:2" )]
	[InlineData( @"(?xx)[\c ]", "60", "0:1" )]
	[InlineData( @"[\cA-\cZ]", "08", "0:1" )]
	[InlineData( @"[^\cA-\cZ]", "20", "0:1" )]
	[InlineData( @"(?i)\c!", "41", "0:1" )]
	[InlineData( @"(?i)[\c1-\c9]", "51", "0:1" )]
	[InlineData( @"(?<x>\cA)\k<x>", "0101", "0:2,0:1" )]
	[InlineData( @"(?<x>\cA)(?&x)", "0101", "0:2,0:1" )]
	[InlineData( @"\A(?<x>\cA(?&x)?)\z", "0101", "0:2,0:2" )]
	[InlineData( @"(\cA|\cA\cB)\cC", "010203", "0:3,0:2" )]
	[InlineData( @"(?>\cA|\cA\cB)\cC", "010203", "" )]
	[InlineData( @"(?<=\cA{2})X", "010158", "2:1" )]
	[InlineData( @"(?<=\cA{1,2})X", "010158", "2:1" )]
	[InlineData( @"[\c[:digit:]]", "1B5D", "0:2" )]
	public void DelimitersTriviaQuotesAndCaptureWidthsKeepLiteralSpans( string pattern, string hex, string spans ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var utf in new[] { false, true } ) {
			var reference = Reference( pattern, utf );
			AssertLiteral( reference, subject, 0, spans );
			Compare( reference, subject, 0, Compile( pattern, new() { Utf8 = utf, Ucp = utf } ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( @"(?i)\c+", "0:3" )]
	[InlineData( @"(?ir)\c+", "" )]
	[InlineData( @"(?i)[\c+]", "0:3" )]
	[InlineData( @"(?ir)[\c+]", "" )]
	public void ProducedLettersUseExistingUtfCasePolicies( string pattern, string spans ) {
		var subject = "K"u8.ToArray();
		var reference = Reference( pattern, true );
		AssertLiteral( reference, subject, 0, spans );
		Compare( reference, subject, 0, Compile( pattern, new() { Utf8 = true, Ucp = true } ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void AllCallerOffsetsKeepSourceNewlineAndFragmentProtocols( bool anyCrLf ) {
		foreach ( var utf in new[] { false, true } ) {
			foreach ( var pattern in new[] { @"\cJX", @"[\cJ]X", @"[^\cM]X", @"[^\cM\cM]X",
				@"[\cI-\cL]X", @"\G\cJX", @"(?<=\cM)\cJX", @"(?<x>\cJ)X\k<x>", @"\c@", @"[\cA-\cZ]" } ) {
				var reference = Reference( pattern, utf, utf, anyCrLf );
				var compiled = Compile( pattern, new() { Utf8 = utf, Ucp = utf,
					MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf } );
				foreach ( var hex in new[] { "0D0A58", "FF0D0A58", "0D0AFF58", "C3A9000D0A580A", "010058" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
					}
				}
			}
		}
	}

	[Fact]
	public void NonprintableByteOperandsAreInvalidInAtomsAndClasses() {
		for ( var operand = 0; operand <= 255; operand++ ) {
			if ( operand is >= 0x20 and <= 0x7e ) { continue; }
			foreach ( var bracket in new[] { false, true } ) {
				var pattern = bracket ? new byte[] { (byte)'[', (byte)'x', (byte)'\\', (byte)'c', (byte)operand, (byte)']' }
					: new byte[] { (byte)'x', (byte)'\\', (byte)'c', (byte)operand };
				Assert.Throws<PcrePatternException>( () => Reference( pattern, false ) );
				var result = PcreByteCompiler.Compile( pattern );
				Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
				Assert.Equal( bracket ? 2 : 1, result.Diagnostic?.ByteOffset );
			}
		}
	}

	[Theory]
	[InlineData( @"\c", false, 0 )]
	[InlineData( @"x\c", false, 1 )]
	[InlineData( @"[\c", false, 1 )]
	[InlineData( @"[\c]", false, 0 )]
	[InlineData( "\\c\t", false, 0 )]
	[InlineData( "(?x)\\c\n", false, 4 )]
	[InlineData( @"(?=\c", false, 3 )]
	[InlineData( @"\cµ", true, 0 )]
	[InlineData( @"é\cµ", true, 2 )]
	[InlineData( @"[\cµ]", true, 1 )]
	public void InvalidOperandsKeepEscapeOrOuterStructuralByteOffsets( string pattern, bool utf, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, utf ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = utf } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void InvalidUtfPatternStillPrecedesEscapeParsingAndUnknownEscapesStayUnsupported() {
		var invalid = new byte[] { (byte)'\\', (byte)'c', 0xff };
		Assert.Throws<PcrePatternException>( () => Reference( invalid, true ) );
		var result = PcreByteCompiler.Compile( invalid, new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, result.Diagnostic?.Code );
		Assert.Equal( 2, result.Diagnostic?.ByteOffset );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, PcreByteCompiler.Compile( "\\C"u8.ToArray() ).Diagnostic?.Code );
	}

	[Fact]
	public void WorkObserversBudgetsAndCancellationUseTheLiteralPath() {
		const string pattern = @"(?<x>\c!+)";
		var subject = "aaa"u8.ToArray();
		var literalSteps = new List<int>();
		var controlSteps = new List<int>();
		var literal = Compile( "(?<x>a+)", new() { WorkObserver = literalSteps.Add } );
		var control = Compile( pattern, new() { WorkObserver = controlSteps.Add } );
		var actual = control.Match( subject );
		Compare( Reference( pattern, false ), subject, 0, actual, pattern );
		Assert.Equal( literal.Match( subject ).Match?.ByteLength, actual.Match?.ByteLength );
		Assert.Equal( literalSteps, controlSteps );
		Assert.Equal( Enumerable.Range( 1, literalSteps.Count ), literalSteps );
		var exhausted = Compile( pattern, new() { MatchWorkLimit = literalSteps.Count - 1 } ).Match( subject );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		Assert.Null( exhausted.Match );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { WorkObserver = count => { if ( count == 4 ) cancellation.Cancel(); } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => control.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	private static void AssertLiteral( PcreRegex8Bit reference, byte[] subject, int offset, string spans ) {
		var match = reference.Match( subject, offset );
		Assert.Equal( spans.Length > 0, match.Success );
		if ( !match.Success ) { return; }
		var groups = spans.Split( ',' );
		Assert.Equal( groups.Length, match.Groups.Count );
		for ( var group = 0; group < groups.Length; group++ ) {
			var capture = match.Groups[group];
			Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
		}
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}: native={ ( expected.Success ? expected.Index : -1 ) }, managed={actual.Match?.ByteIndex ?? -1}" );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf, bool tolerant = false, bool anyCrLf = false ) =>
		Reference( Encoding.UTF8.GetBytes( pattern ), utf, tolerant, anyCrLf );

	private static PcreRegex8Bit Reference( byte[] pattern, bool utf, bool tolerant = false, bool anyCrLf = false ) => new(
		Encoding.ASCII.GetBytes( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ).Concat( pattern ).ToArray(), Encoding.UTF8,
		new PcreRegexSettings { Options = PcreOptions.Compiled | ( utf ? PcreOptions.Utf | PcreOptions.Ucp : PcreOptions.None )
			| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
