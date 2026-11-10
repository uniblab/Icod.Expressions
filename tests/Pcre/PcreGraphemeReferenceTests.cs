using System.Globalization;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Atomic clusters pinned to the live PCRE2 10.48 JIT, not host segmentation.</summary>
public sealed class PcreGraphemeReferenceTests {
	private static string[][] Rows( string text ) => text.Split( '\n', StringSplitOptions.RemoveEmptyEntries )
		.Select( line => line.TrimEnd( '\r' ).Split( ';' ) ).ToArray();
	private static int Hex( string text ) => int.Parse( text, NumberStyles.HexNumber, CultureInfo.InvariantCulture );
	private static byte[] Bytes( params int[] scalars ) => Encoding.UTF8.GetBytes( String.Concat( scalars.Select( char.ConvertFromUtf32 ) ) );

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryUnicodeBreakBoundaryKeepsBothDirectionsAndCaptures( bool ucp ) {
		var native = Reference( @"\G(\X)", true, ucp );
		var managed = Compile( @"\G(\X)", true, ucp );
		foreach ( var row in Rows( PcreGraphemeFixtureData.Boundaries ) ) {
			var scalar = Hex( row[0] );
			var kind = int.Parse( row[1], CultureInfo.InvariantCulture );
			for ( var other = 0; other < 15; other++ ) {
				var representative = PcreGraphemeFixtureData.Representatives[other];
				var forward = Bytes( scalar, representative );
				var reverse = Bytes( representative, scalar );
				var forwardJoins = ( PcreGraphemeFixtureData.PairMasks[kind] & ( 1 << other ) ) != 0;
				var reverseJoins = ( PcreGraphemeFixtureData.PairMasks[other] & ( 1 << kind ) ) != 0;
				Assert.Equal( forwardJoins ? forward.Length : Bytes( scalar ).Length, native.Match( forward ).Length );
				Assert.Equal( reverseJoins ? reverse.Length : Bytes( representative ).Length, native.Match( reverse ).Length );
				Compare( native, managed, forward );
				Compare( native, managed, reverse );
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryTypeTripleKeepsNativeEndsAtEveryScalarStart( bool ucp ) {
		var native = Reference( @"\G(\X)", true, ucp );
		var managed = Compile( @"\G(\X)", true, ucp );
		foreach ( var row in Rows( PcreGraphemeFixtureData.Triples ) ) {
			var scalars = row[0].Split( ',' ).Select( value => PcreGraphemeFixtureData.Representatives[int.Parse( value, CultureInfo.InvariantCulture )] ).ToArray();
			var ends = row[1].Split( ',' ).Select( value => int.Parse( value, CultureInfo.InvariantCulture ) ).ToArray();
			var subject = Bytes( scalars );
			var offsets = new[] { 0, Bytes( scalars[0] ).Length, Bytes( scalars[0], scalars[1] ).Length, subject.Length };
			for ( var start = 0; start < 3; start++ ) {
				Assert.Equal( offsets[ends[start]] - offsets[start], native.Match( subject, offsets[start] ).Length );
				Compare( native, managed, subject, offsets[start] );
			}
			Compare( native, managed, subject, subject.Length );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryBytePairProjectsNativeClusterBoundaries( bool ucp ) {
		var native = Reference( @"\G(\X)", false, ucp );
		var managed = Compile( @"\G(\X)", false, ucp );
		for ( var first = 0; first < 256; first++ ) {
			for ( var second = 0; second < 256; second++ ) {
				var subject = new[] { (byte)first, (byte)second };
				Assert.Equal( first == 13 && second == 10 ? 2 : 1, native.Match( subject ).Length );
				Compare( native, managed, subject );
				Compare( native, managed, subject, 1 );
			}
		}
		Compare( native, managed, Array.Empty<byte>() );
	}

	[Theory]
	[InlineData( @"\A(\X)\z", "á", true )]
	[InlineData( @"\A(\X)\x{301}\z", "á", false )]
	[InlineData( @"\A(\X)\z", "̀́", true )]
	[InlineData( @"\A(\X)\z", "\r\n", true )]
	[InlineData( @"\A(\X)\n\z", "\r\n", false )]
	[InlineData( @"\A(\X)\z", "👩🏽‍💻", true )]
	[InlineData( @"\A(\X)\z", "👩́‍💻", true )]
	[InlineData( @"\A(\X)\z", "👩‍́💻", false )]
	[InlineData( @"\A(\X)\z", "‍💻", false )]
	[InlineData( @"\A(\X)\z", "؀a", true )]
	[InlineData( @"\A(\X)\z", "؀😀", false )]
	[InlineData( @"\A(\X)\z", "🇦́", false )]
	[InlineData( @"\A(\X)\z", "क्ष", false )]
	[InlineData( @"\A(\X)\z", "각", true )]
	[InlineData( @"\A(\X)\z", "각", true )]
	[InlineData( @"\A(\X{2})\z", "🇦🇧🇨🇩", true )]
	[InlineData( @"\A(\X+?)\X\z", "áb̈", true )]
	[InlineData( @"\A(\X+)\X\z", "áb̈c", true )]
	[InlineData( @"\A(\X*+)\X\z", "áb̈", false )]
	[InlineData( @"\A(\X){2}\z", "áb̈", true )]
	[InlineData( @"\A(?:(\X)Z|(\X))\z", "á", true )]
	[InlineData( @"(?=(\X))\1", "á", true )]
	[InlineData( @"\A(\X)\1\z", "áá", true )]
	[InlineData( @"\A(?<x>\X)(?1)\z", "áb̈", true )]
	[InlineData( @"\A(?(?=\X)(\X)|a)\z", "á", true )]
	[InlineData( @"(?*(\X+))\1a", "b̈b̈a", true )]
	[InlineData( @"(?<=a(?=(\X)))", "ab́", true )]
	[InlineData( @"a\K(\X)", "ab́", true )]
	[InlineData( @"(*CRLF)\Xx", "\r\nx", true )]
	[InlineData( @"(*CRLF)\X\Xx", "\r\nx", false )]
	[InlineData( @"(*CRLF)\X?\nx", "\r\nx", true )]
	[InlineData( @"\A\X{0}a\z", "a", true )]
	[InlineData( @"\Q\X\E", @"\X", true )]
	public void AtomicityRepeatsCapturesAndNestedGrammarKeepExactOutcomes( string pattern, string text, bool success ) {
		foreach ( var ucp in new[] { false, true } ) {
			var subject = Encoding.UTF8.GetBytes( text );
			var native = Reference( pattern, true, ucp );
			Assert.Equal( success, native.Match( subject ).Success );
			Compare( native, Compile( pattern, true, ucp ), subject );
		}
	}

	[Theory]
	[InlineData( "" )]
	[InlineData( "(?i)" )]
	[InlineData( "(?ir)" )]
	[InlineData( "(?s)" )]
	[InlineData( "(?aT)" )]
	[InlineData( "(*CRLF)(*BSR_ANYCRLF)" )]
	public void ExplicitClustersIgnoreCaseAsciiAndNewlinePolicies( string prefix ) {
		foreach ( var ucp in new[] { false, true } ) {
			var pattern = prefix + @"\G(\X)";
			var native = Reference( pattern, true, ucp );
			var managed = Compile( pattern, true, ucp );
			foreach ( var text in new[] { "á", "\r\n", "👩🏽‍💻", "🇦🇧🇨", "각" } ) {
				Compare( native, managed, Encoding.UTF8.GetBytes( text ) );
			}
		}
	}

	[Theory]
	[InlineData( @"(?<=a(?=(.)))", true )]
	[InlineData( @"(?<=a(?!(b)))", false )]
	[InlineData( @"(?<=a(?=(\X)))", true )]
	public void ForwardAssertionsInsideBehindCanReadFollowingText( string pattern, bool success ) {
		var subject = "ab́"u8.ToArray();
		var native = Reference( pattern, true );
		Assert.Equal( success, native.Match( subject ).Success );
		Compare( native, Compile( pattern, true ), subject );
	}

	[Fact]
	public void MalformedFragmentsAndInsideClusterStartsKeepOriginalByteSpans() {
		var subject = Convert.FromHexString( "FFF09F87A6F09F87A7FFF09F87A8F09F87A9F09F87AAFF61CC81E282" );
		foreach ( var pattern in new[] { @"(\X)", @"\G(\X)", @"(\X+)", @"(\X?)(\X)" } ) {
			var native = Reference( pattern, true, tolerant: true );
			var managed = Compile( pattern, true, tolerant: true );
			for ( var start = 0; start <= subject.Length; start++ ) { Compare( native, managed, subject, start ); }
		}
	}

	[Fact]
	public void LongClustersKeepExactWorkLimitsAndCancellationInsideTheAtom() {
		var subject = Encoding.UTF8.GetBytes( "a" + new string( '\u0301', 256 ) );
		var steps = new List<int>();
		var measured = CompileOptions( @"\A\X\z", new() { Utf8 = true, WorkObserver = steps.Add } );
		Assert.NotNull( measured.Match( subject ).Match );
		Assert.True( steps.Count > 514 ); // UTF validation plus every consuming scalar probe.
		var exact = CompileOptions( @"\A\X\z", new() { Utf8 = true, MatchWorkLimit = steps.Count } );
		Assert.NotNull( exact.Match( subject ).Match );
		var exhausted = CompileOptions( @"\A\X\z", new() { Utf8 = true, MatchWorkLimit = steps.Count - 1 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = CompileOptions( @"\A\X\z", new() { Utf8 = true,
			WorkObserver = count => { if ( count == 289 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void RegionalContextMemoStaysLinearAndPrefixScansRemainCancellable() {
		var subject = Bytes( Enumerable.Repeat( 0x1F1E6, 256 ).ToArray() );
		var steps = new List<int>();
		var managed = CompileOptions( @"\A\X+\z", new() { Utf8 = true, WorkObserver = steps.Add } );
		Assert.NotNull( managed.Match( subject ).Match );
		Assert.True( steps.Count < 4096, $"A contiguous RI run used {steps.Count} steps." );
		var native = Reference( @"\G(\X)", true );
		var anchored = Compile( @"\G(\X)", true );
		for ( var scalar = 0; scalar < 256; scalar++ ) { Compare( native, anchored, subject, scalar * 4 ); }
		using var cancellation = new CancellationTokenSource();
		var cancellable = CompileOptions( @"\G(\X)", new() { Utf8 = true,
			WorkObserver = count => { if ( count == 300 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, 254 * 4, cancellation.Token ) );
	}

	[Theory]
	[InlineData( @"[\X]", (int)PcreByteDiagnosticCode.InvalidSyntax, 1 )]
	[InlineData( @"é[\X]", (int)PcreByteDiagnosticCode.InvalidSyntax, 3 )]
	[InlineData( @"(?<=\X)", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"(?<!\X)a", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"(?<=(\X))", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"(\X)(?<=\1)", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 4 )]
	[InlineData( @"(\X)(?<=(?1))", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 4 )]
	public void BracketAndConsumingBehindFormsKeepOpeningByteDiagnostics( string pattern, int code, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, true ) );
		var actual = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.Equal( (PcreByteDiagnosticCode)code, actual.Diagnostic?.Code );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8, bool ucp = false, bool tolerant = false ) =>
		new( Encoding.UTF8.GetBytes( pattern ), utf8 ? Encoding.UTF8 : Encoding.Latin1, new PcreRegexSettings {
			Options = PcreOptions.Compiled | ( utf8 ? PcreOptions.Utf : PcreOptions.None ) | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None )
		} );
	private static PcreCompiledBytePattern Compile( string pattern, bool utf8, bool ucp = false, bool tolerant = false ) =>
		CompileOptions( pattern, new() { Utf8 = utf8, Ucp = ucp, MatchInvalidUtf = tolerant } );
	private static PcreCompiledBytePattern CompileOptions( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
	private static void Compare( PcreRegex8Bit native, PcreCompiledBytePattern managed, byte[] subject, int offset = 0 ) {
		var expected = native.Match( subject, offset );
		var actual = managed.Match( subject, offset );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		Assert.Equal( expected.Success, actual.Match is not null );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Index, actual.Match!.ByteIndex );
		Assert.Equal( expected.Length, actual.Match.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var i = 0; i < expected.Groups.Count; i++ ) {
			var capture = expected.Groups[i];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[i] );
		}
	}
}
