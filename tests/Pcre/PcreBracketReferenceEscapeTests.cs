using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Reference-looking text inside ordinary brackets stays literal class source.</summary>
public sealed class PcreBracketReferenceEscapeTests {
	[Theory]
	[InlineData( @"[\g]", "67", false, false )]
	[InlineData( @"[\k]", "6B", false, false )]
	[InlineData( @"[^\g]", "67", true, false )]
	[InlineData( @"[^\k]", "6B", true, false )]
	[InlineData( @"[\g\k]", "676B", false, false )]
	[InlineData( @"[\g{999}]", "67397B7D", false, false )]
	[InlineData( @"[\k'name']", "6B276E616D65", false, false )]
	[InlineData( @"[\g<2>]", "673C323E", false, false )]
	[InlineData( @"[\k{missing}]", "6B7B6D69736E677D", false, false )]
	[InlineData( @"[\g-\k]", "6768696A6B", false, false )]
	[InlineData( @"[a-\g]", "61626364656667", false, false )]
	[InlineData( @"[\k-z]", "6B6C6D6E6F707172737475767778797A", false, false )]
	[InlineData( @"(?i)[\k]", "4B6B", false, true )]
	[InlineData( @"(?ir)[\k]", "4B6B", false, false )]
	[InlineData( @"(?i)[^\k]", "4B6B", true, true )]
	[InlineData( @"(?x)[\g# ]", "672320", false, false )]
	[InlineData( @"(?xx)[\g #]", "6723", false, false )]
	[InlineData( @"[\Q\g\E]", "5C67", false, false )]
	[InlineData( @"(?aT)[\g[:digit:]]", "6730313233343536373839", false, false )]
	public void ByteAndUtfMembershipIsPinnedToLiteralSets( string pattern, string membersHex, bool negated, bool kelvinMember ) {
		var members = Convert.FromHexString( membersHex );
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var reference = Reference( @"\A" + pattern + @"\z", options );
			var compiled = Compile( @"\A" + pattern + @"\z", options );
			var probes = Enumerable.Range( 0, 256 ).Concat( utf ? new[] { 0x100, 0x212A, 0x1F600, 0x10FFFF } : Array.Empty<int>() );
			foreach ( var scalar in probes ) {
				var member = scalar <= 255 ? members.Contains( (byte)scalar ) : scalar == 0x212A && kelvinMember;
				var expected = member != negated;
				var subject = utf ? Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) ) : new[] { (byte)scalar };
				Assert.Equal( expected, reference.Match( subject ).Success );
				Compare( reference, subject, 0, compiled.Match( subject ), $"{pattern}, U+{scalar:X}, UTF={utf}" );
			}
		}
	}

	[Theory]
	[InlineData( @"([\g])([\k])", "676B", "0:2,0:1,1:1" )]
	[InlineData( @"[\g]]", "675D", "0:2" )]
	[InlineData( @"[\k]name", "6B6E616D65", "0:5" )]
	[InlineData( @"[\g<missing>]+", "3C6D697373696E673E", "0:9" )]
	[InlineData( @"[\g{999}]", "7B", "0:1" )]
	[InlineData( @"[\k{missing}]", "7D", "0:1" )]
	[InlineData( @"(?<x>[\g])\k<x>", "6767", "0:2,0:1" )]
	[InlineData( @"(?<x>[\k])(?&x)", "6B6B", "0:2,0:1" )]
	[InlineData( @"\A(?<x>[\g](?&x)?)\z", "676767", "0:3,0:3" )]
	[InlineData( @"(?<=[\g-\k]{2})X", "676B58", "2:1" )]
	[InlineData( @"(?<=[\k]{1,2})(X)", "6B6B58", "2:1,2:1" )]
	[InlineData( @"(?=([\g]+))\1", "6767", "0:2,0:2" )]
	[InlineData( @"\A[\g]+?g\z", "676767", "0:3" )]
	[InlineData( @"\A[\g]++g\z", "676767", "" )]
	[InlineData( @"([\k]|[\k]g)X", "6B6758", "0:3,0:2" )]
	[InlineData( @"(?>[\k]|[\k]g)X", "6B6758", "" )]
	[InlineData( @"(?x)[\k] # comment
X", "6B58", "0:2" )]
	[InlineData( @"[\Q\g\E]", "5C", "0:1" )]
	public void CapturesWidthsAndTokenBoundariesHaveLiteralSpans( string pattern, string hex, string spans ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf };
			var reference = Reference( pattern, options );
			AssertLiteral( reference, subject, spans );
			Compare( reference, subject, 0, Compile( pattern, options ).Match( subject ), pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryCallerOffsetRetainsLiteralCandidateAndFragmentProtocols( bool anyCrLf ) {
		foreach ( var utf in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = utf, Ucp = utf, MatchInvalidUtf = utf, AnyCrLfNewline = anyCrLf };
			foreach ( var pattern in new[] { @"[\g]X", @"[^\k]X", @"[\g-\k]", @"\G[\k]", @"(?<=[\g])X", @"([\k])X\1" } ) {
				var reference = Reference( pattern, options );
				var compiled = Compile( pattern, options );
				foreach ( var hex in new[] { "676B58", "0D0A58", "FF6758", "6BFF586B", "C3A96B586B" } ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( @"[\g", 0 )]
	[InlineData( @"[\k", 0 )]
	[InlineData( @"[\k-\g]", 0 )]
	[InlineData( @"é[\k-\g]", 2 )]
	public void MalformedClassesRetainOriginalOpeningByteDiagnostics( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, new() { Utf8 = true } ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void OutsideReferencesAndUtfValidationKeepTheirExistingBoundaries() {
		foreach ( var (pattern, code) in new[] { (@"\g", PcreByteDiagnosticCode.UnsupportedSyntax),
			(@"\k", PcreByteDiagnosticCode.InvalidSyntax), (@"\g{missing}", PcreByteDiagnosticCode.InvalidSyntax),
			(@"\k<missing>", PcreByteDiagnosticCode.InvalidSyntax) } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern, new() ) );
			var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ) );
			Assert.Null( result.Pattern );
			Assert.Equal( code, result.Diagnostic?.Code );
			Assert.Equal( 0, result.Diagnostic?.ByteOffset );
		}
		var invalid = new byte[] { (byte)'[', (byte)'\\', (byte)'g', 0xff, (byte)']' };
		Assert.Throws<PcrePatternException>( () => Reference( invalid, new() { Utf8 = true } ) );
		var utfError = PcreByteCompiler.Compile( invalid, new PcreByteOptions { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, utfError.Diagnostic?.Code );
		Assert.Equal( 3, utfError.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void WorkBudgetsAndCancellationMatchUnescapedClassLetters() {
		const string escaped = @"(?<x>[\g\k]+)";
		var subject = "ggkk"u8.ToArray();
		var literalSteps = new List<int>();
		var escapedSteps = new List<int>();
		var literal = Compile( "(?<x>[gk]+)", new() { WorkObserver = literalSteps.Add } );
		var actual = Compile( escaped, new() { WorkObserver = escapedSteps.Add } );
		var reference = Reference( escaped, new() );
		AssertLiteral( reference, subject, "0:4,0:4" );
		Compare( reference, subject, 0, actual.Match( subject ), escaped );
		Assert.Equal( 4, literal.Match( subject ).Match?.ByteLength );
		Assert.Equal( literalSteps, escapedSteps );
		Assert.Equal( Enumerable.Range( 1, literalSteps.Count ), escapedSteps );
		var limited = Compile( escaped, new() { MatchWorkLimit = escapedSteps.Count - 1 } ).Match( subject );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		Assert.Null( limited.Match );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( escaped, new() { WorkObserver = count => { if ( count == 4 ) cancellation.Cancel(); } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => actual.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( escaped ), cancellationToken: cancellation.Token ) );
	}

	private static void AssertLiteral( PcreRegex8Bit reference, byte[] subject, string spans ) {
		var match = reference.Match( subject );
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
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, PcreByteOptions options ) => Reference( Encoding.UTF8.GetBytes( pattern ), options );
	private static PcreRegex8Bit Reference( byte[] pattern, PcreByteOptions options ) => new( pattern, Encoding.UTF8, new PcreRegexSettings {
		Options = PcreOptions.Compiled | ( options.Utf8 || options.MatchInvalidUtf ? PcreOptions.Utf : PcreOptions.None )
			| ( options.Ucp ? PcreOptions.Ucp : PcreOptions.None ) | ( options.MatchInvalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None ),
		NewLine = options.AnyCrLfNewline ? PcreNewLine.AnyCrLf : PcreNewLine.Lf,
		ParensLimit = 512
	} );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
