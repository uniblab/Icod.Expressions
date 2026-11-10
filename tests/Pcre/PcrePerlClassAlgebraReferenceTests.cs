using System.Globalization;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Perl class algebra qualified against independently exported PCRE2 10.48 JIT masks.</summary>
public sealed class PcrePerlClassAlgebraReferenceTests {
	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	[InlineData( 4 )]
	[InlineData( 5 )]
	[InlineData( 6 )]
	[InlineData( 7 )]
	public void EveryByteKeepsPrecedenceFoldingAndNativeBitmapProjection( int profile ) {
		var utf8 = ( profile & 1 ) != 0;
		var ucp = ( profile & 2 ) != 0;
		foreach ( var row in PcrePerlClassAlgebraFixtureData.ByteMasks ) {
			var fields = row.Split( ';' );
			if ( int.Parse( fields[0], CultureInfo.InvariantCulture ) != profile ) { continue; }
			var expression = PcrePerlClassAlgebraFixtureData.Expressions[int.Parse( fields[1], CultureInfo.InvariantCulture )];
			var pattern = ( ( profile & 4 ) != 0 ? "(?i)" : String.Empty ) + @"\A(?[" + expression + @"])\z";
			var mask = Convert.FromHexString( fields[2] );
			var native = Reference( pattern, utf8, ucp );
			var managed = Compile( pattern, new() { Utf8 = utf8, Ucp = ucp } );
			for ( var value = 0; value < 256; value++ ) {
				var subject = utf8 ? Encoding.UTF8.GetBytes( char.ConvertFromUtf32( value ) ) : new[] { (byte)value };
				Assert.True( native.Match( subject ).Success == ( ( mask[value >> 3] & ( 1 << ( value & 7 ) ) ) != 0 ), pattern );
				Compare( native, managed, subject, context: pattern );
			}
		}
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	public void UnicodeBoundaryMasksKeepPropertyAndOperandCaseClosure( int profile ) {
		foreach ( var row in PcrePerlClassAlgebraFixtureData.UnicodeMasks ) {
			var fields = row.Split( ';' );
			if ( int.Parse( fields[0], CultureInfo.InvariantCulture ) != profile ) { continue; }
			var expression = PcrePerlClassAlgebraFixtureData.UnicodeExpressions[int.Parse( fields[1], CultureInfo.InvariantCulture )];
			var pattern = ( ( profile & 2 ) != 0 ? "(?i)" : String.Empty ) + @"\A(?[" + expression + @"])\z";
			var ucp = ( profile & 1 ) != 0;
			var mask = Convert.FromHexString( fields[2] );
			var native = Reference( pattern, true, ucp );
			var managed = Compile( pattern, new() { Utf8 = true, Ucp = ucp } );
			for ( var i = 0; i < PcrePerlClassAlgebraFixtureData.Scalars.Length; i++ ) {
				var scalar = PcrePerlClassAlgebraFixtureData.Scalars[i];
				var subject = Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) );
				Assert.True( native.Match( subject ).Success == ( ( mask[i >> 3] & ( 1 << ( i & 7 ) ) ) != 0 ), pattern );
				Compare( native, managed, subject, context: pattern + " U+" + scalar.ToString( "X", CultureInfo.InvariantCulture ) );
			}
		}
	}

	[Theory]
	[InlineData( @"\A((?[[a-z] - [aeiou]])+)\1\z", "bcb c", false )]
	[InlineData( @"\A((?[[a-z] - [aeiou]])+)\1\z", "bcbc", true )]
	[InlineData( @"\A(?<x>(?[[a-z]&[^aeiou]]))(?1)\z", "bc", true )]
	[InlineData( @"\A(?:(?[[a] | [b]])Z|((?[[a] ^ [b]])))\z", "b", true )]
	[InlineData( @"(?<=(?[[a-z] - [b-y]]))b", "ab", true )]
	[InlineData( @"(?<!(?[[a-z] - [b-y]]))b", "cb", true )]
	[InlineData( @"\A(?=(?[[a-z] & [a-f]]))(?[[a-z]])\z", "d", true )]
	[InlineData( @"\A(?[[a-z] - [b-y]]){2,4}?\z", "azza", true )]
	[InlineData( @"\A(?[[a-z] ^ [a-z]]){0}a\z", "a", true )]
	[InlineData( @"\A(?[[a-z] ^ [a-z]])*\z", "", true )]
	[InlineData( @"\A(?[[a-z] | ![a-z]])\z", "😀", true )]
	[InlineData( @"\A(?[ [ a - z ] & ! [ b - y ] ]) \z", "a ", true )]
	[InlineData( @"\A(?[[#]])\z", "#", true )]
	[InlineData( @"\A(?[[\Q a-b\E]])\z", " ", true )]
	[InlineData( "\\A(?[[\n]])\\z", "\n", true )]
	[InlineData( @"\A(?[\N{U+0041}])\z", "A", true )]
	[InlineData( @"(?i)\A(?[[k] - [K]])\z", "K", false )]
	[InlineData( @"(?ir)\A(?[[k] | [s]])\z", "K", false )]
	[InlineData( @"(?i)\A(?[[k] | [s]])\z", "ſ", true )]
	[InlineData( @"(*CRLF)(?[[\r] | [\n]])x", "\r\nx", true )]
	public void NestedGrammarCapturesRepeatsAndSearchKeepNativeResults( string pattern, string text, bool success ) {
		var subject = Encoding.UTF8.GetBytes( text );
		foreach ( var ucp in new[] { false, true } ) {
			var native = Reference( pattern, true, ucp );
			Assert.Equal( success, native.Match( subject ).Success );
			Compare( native, Compile( pattern, new() { Utf8 = true, Ucp = ucp } ), subject );
		}
	}

	[Theory]
	[InlineData( "(?[a])" )]
	[InlineData( "(?[[a][b]])" )]
	[InlineData( "(?[[a] |])" )]
	[InlineData( "(?[&[a]])" )]
	[InlineData( "(?[[a] ![b]])" )]
	[InlineData( "(?[([a]])" )]
	[InlineData( "(?[[a]] )" )]
	[InlineData( "(?[])" )]
	[InlineData( "(?[[a] # [b]])" )]
	[InlineData( "(?[[a]\n|[b]])" )]
	[InlineData( @"(?[\Qabc\E])" )]
	[InlineData( @"(?[\Q \E])" )]
	[InlineData( @"(?[\A])" )]
	[InlineData( @"(?[\X])" )]
	[InlineData( @"(?[\R])" )]
	[InlineData( @"(?[(?i:[a])])" )]
	[InlineData( @"(?[(?[[a]])])" )]
	public void InvalidAlgebraReportsOriginalOpeningByte( string suffix ) {
		foreach ( var prefix in new[] { String.Empty, "é" } ) {
			var pattern = prefix + suffix;
			Assert.Throws<PcrePatternException>( () => Reference( pattern, true ) );
			var actual = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
			Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code );
			Assert.Equal( Encoding.UTF8.GetByteCount( prefix ), actual.Diagnostic?.ByteOffset );
		}
	}

	[Fact]
	public void ClassNestingAndFlatTermsAreBoundedWithoutNativeStackGrowth() {
		var valid = "(?[" + new string( '(', 13 ) + "[a]" + new string( ')', 13 ) + "])";
		Compare( Reference( valid, true ), Compile( valid, new() { Utf8 = true } ), "a"u8.ToArray() );
		var deep = "é(?[" + new string( '(', 14 ) + "[a]" + new string( ')', 14 ) + "])";
		Assert.Throws<PcrePatternException>( () => Reference( deep, true ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( deep ), new() { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, result.Diagnostic?.Code );
		Assert.Equal( 2, result.Diagnostic?.ByteOffset );
		var flat = "é(?[" + new string( '!', 4096 ) + "[a]])";
		Assert.True( Reference( flat, true ).Match( Encoding.UTF8.GetBytes( "éa" ) ).Success );
		var limited = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( flat ), new() { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, limited.Diagnostic?.Code );
		Assert.Equal( 2, limited.Diagnostic?.ByteOffset );
		var shared = "(?[" + new string( '!', 2100 ) + "[a]])";
		var combined = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( shared + shared ), new() { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, combined.Diagnostic?.Code );
		Assert.Equal( shared.Length, combined.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void LongExpressionKeepsExactWorkAndMidAtomCancellation() {
		var pattern = @"\A(?[" + String.Join( " ^ ", Enumerable.Repeat( "[a]", 101 ) ) + @"])\z";
		var steps = new List<int>();
		var observed = Compile( pattern, new() { Utf8 = true, WorkObserver = steps.Add } );
		Compare( Reference( pattern, true ), observed, "a"u8.ToArray() );
		var work = steps.Count;
		Assert.True( work >= 202 );
		Assert.NotNull( Compile( pattern, new() { Utf8 = true, MatchWorkLimit = work } ).Match( "a"u8.ToArray() ).Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, new() { Utf8 = true, MatchWorkLimit = work - 1 } ).Match( "a"u8.ToArray() ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { Utf8 = true,
			WorkObserver = count => { if ( count == 50 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( "a"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void MalformedFragmentsAndStartOffsetsKeepByteCaptures() {
		var subject = Convert.FromHexString( "FF61C3A9E282F09F988062FF" );
		foreach ( var pattern in new[] { @"((?[\p{L} | \p{Emoji}]))", @"\G((?[!\p{L}]))", @"((?[\p{L} | \p{Emoji}])+)" } ) {
			var native = Reference( pattern, true, tolerant: true );
			var managed = Compile( pattern, new() { Utf8 = true, MatchInvalidUtf = true } );
			for ( var offset = 0; offset <= subject.Length; offset++ ) { Compare( native, managed, subject, offset ); }
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8, bool ucp = false, bool tolerant = false ) =>
		new( Encoding.UTF8.GetBytes( pattern ), utf8 ? Encoding.UTF8 : Encoding.Latin1, new PcreRegexSettings {
			Options = PcreOptions.Compiled | ( utf8 ? PcreOptions.Utf : PcreOptions.None ) | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None )
		} );
	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, pattern + ": " + result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
	private static void Compare( PcreRegex8Bit native, PcreCompiledBytePattern managed, byte[] subject, int offset = 0, string? context = null ) {
		var expected = native.Match( subject, offset );
		var actual = managed.Match( subject, offset );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		Assert.True( expected.Success == ( actual.Match is not null ), context );
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
