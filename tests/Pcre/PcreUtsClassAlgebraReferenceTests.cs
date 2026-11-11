using System.Globalization;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>UTS#18 class algebra qualified against independently exported PCRE2 10.48 JIT masks.</summary>
public sealed class PcreUtsClassAlgebraReferenceTests {
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
		foreach ( var row in PcreUtsClassAlgebraFixtureData.ByteMasks ) {
			var fields = row.Split( ';' );
			if ( int.Parse( fields[0], CultureInfo.InvariantCulture ) != profile ) { continue; }
			var expression = PcreUtsClassAlgebraFixtureData.Expressions[int.Parse( fields[1], CultureInfo.InvariantCulture )];
			var pattern = ( ( profile & 4 ) != 0 ? "(?i)" : String.Empty ) + @"\A" + expression + @"\z";
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
		foreach ( var row in PcreUtsClassAlgebraFixtureData.UnicodeMasks ) {
			var fields = row.Split( ';' );
			if ( int.Parse( fields[0], CultureInfo.InvariantCulture ) != profile ) { continue; }
			var expression = PcreUtsClassAlgebraFixtureData.UnicodeExpressions[int.Parse( fields[1], CultureInfo.InvariantCulture )];
			var pattern = ( ( profile & 2 ) != 0 ? "(?i)" : String.Empty ) + @"\A" + expression + @"\z";
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
	[InlineData( @"\A([a-z--[aeiou]]+)\1\z", "bcbc", true )]
	[InlineData( @"\A([a-z--[aeiou]]+)\1\z", "baba", false )]
	[InlineData( @"\A(?<x>[a-z&&[^aeiou]])(?1)\z", "bc", true )]
	[InlineData( @"\A(?:[a||b]Z|([a~~b]))\z", "b", true )]
	[InlineData( @"(?<=[a-z--b-y])b", "ab", true )]
	[InlineData( @"(?<![a-z--b-y])b", "cb", true )]
	[InlineData( @"\A(?=[a-z&&a-f])[a-z]\z", "d", true )]
	[InlineData( @"\A[a-z--b-y]{2,4}?\z", "azza", true )]
	[InlineData( @"\A[a~~a]{0}a\z", "a", true )]
	[InlineData( @"\A[a~~a]*\z", "", true )]
	[InlineData( @"\A[a||[^a]]\z", "😀", true )]
	[InlineData( @"(?xx)\A[ a - z && [ ^ b - y ] ] \z", "a", true )]
	[InlineData( @"(?x)\A[a &&[a ]]\z", " ", true )]
	[InlineData( @"\A[\Q[a&&b]\E]\z", "[", true )]
	[InlineData( @"\A[#&&[#]]\z", "#", true )]
	[InlineData( "\\A[\n&&[\n]]\\z", "\n", true )]
	[InlineData( @"(?i)\A[k--K]\z", "K", false )]
	[InlineData( @"(?ir)\A[k||s]\z", "K", false )]
	[InlineData( @"(?i)\A[k||s]\z", "ſ", true )]
	[InlineData( @"\A(?[[a&&b] & [b]])\z", "b", true )]
	[InlineData( @"\A(?[[a||b] & [|]])\z", "|", true )]
	[InlineData( @"\A[a&&b][a||b]\z", "ba", false )]
	[InlineData( @"(*CRLF)[\r||\n]x", "\r\nx", true )]
	[InlineData( @"\A[\N{U+0041}--B]\z", "A", true )]
	public void NestedGrammarCapturesRepeatsAndPerlLeavesKeepNativeResults( string pattern, string text, bool success ) {
		var subject = Encoding.UTF8.GetBytes( text );
		foreach ( var ucp in new[] { false, true } ) {
			var native = Reference( pattern, true, ucp );
			Assert.Equal( success, native.Match( subject ).Success );
			Compare( native, Compile( pattern, new() { Utf8 = true, Ucp = ucp } ), subject );
		}
	}

	[Theory]
	[InlineData( "[a&&b--c]" )]
	[InlineData( "[a||b&&c]" )]
	[InlineData( "[a~~b||c]" )]
	[InlineData( "[a--b~~c]" )]
	[InlineData( "[a|||b]" )]
	[InlineData( "[a&&&b]" )]
	[InlineData( "[a---b]" )]
	[InlineData( "[a~~~~b]" )]
	[InlineData( "[&&a]" )]
	[InlineData( "[--1]" )]
	[InlineData( "[a||]" )]
	[InlineData( @"[a&&\Q\E]" )]
	[InlineData( "[a[]" )]
	[InlineData( "[a&&[b]" )]
	[InlineData( "[a-[z-a]]" )]
	[InlineData( @"[\d-a&&b]" )]
	[InlineData( @"[a-\d&&b]" )]
	[InlineData( @"[\A&&a]" )]
	public void InvalidExtendedGrammarReportsOutermostOpeningByte( string suffix ) {
		foreach ( var prefix in new[] { String.Empty, "é" } ) {
			var pattern = prefix + suffix;
			Assert.Throws<PcrePatternException>( () => Reference( pattern, true ) );
			var actual = CompileResult( pattern, new() { Utf8 = true } );
			Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code );
			Assert.Equal( Encoding.UTF8.GetByteCount( prefix ), actual.Diagnostic?.ByteOffset );
		}
	}

	[Fact]
	public void ActivationIsOptInAndLeavesOrdinaryGrammarAndPerlClassesAvailable() {
		foreach ( var utf8 in new[] { false, true } ) {
			foreach ( var pattern in new[] { "[a&&b]", "[a||b]", @"\Q[a&&b]\E", @"(?[[a&&b] | [|]])" } ) {
				var ordinary = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = utf8 } );
				Assert.True( ordinary.IsSuccess );
				var enabled = Compile( pattern, new() { Utf8 = utf8 } );
				foreach ( var subject in new[] { "a", "b", "&", "|", "[a&&b]" } ) {
					var bytes = Encoding.UTF8.GetBytes( subject );
					Compare( Reference( pattern, utf8, alt: false ), ordinary.Pattern!, bytes );
					Compare( Reference( pattern, utf8 ), enabled, bytes );
				}
			}
		}
	}

	[Fact]
	public void NestingAndTermBudgetsBoundBothNativeModes() {
		foreach ( var utf8 in new[] { false, true } ) {
			var valid = new string( '[', 15 ) + "a" + new string( ']', 15 );
			Compare( Reference( valid, utf8 ), Compile( valid, new() { Utf8 = utf8 } ), "a"u8.ToArray() );
			var deep = "ab" + new string( '[', 16 ) + "a" + new string( ']', 16 );
			Assert.Throws<PcrePatternException>( () => Reference( deep, utf8 ) );
			var depth = CompileResult( deep, new() { Utf8 = utf8 } );
			Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, depth.Diagnostic?.Code );
			Assert.Equal( 2, depth.Diagnostic?.ByteOffset );
			var flat = "ab[" + String.Join( "||", Enumerable.Repeat( "[a]", 2050 ) ) + "]";
			// Native LINK_SIZE=2 also rejects this pattern by compiled size;
			// the managed term budget and its opening offset are independent.
			Assert.Throws<PcrePatternException>( () => Reference( flat, utf8 ) );
			var terms = CompileResult( flat, new() { Utf8 = utf8 } );
			Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, terms.Diagnostic?.Code );
			Assert.Equal( 2, terms.Diagnostic?.ByteOffset );
			var one = "[" + String.Join( "||", Enumerable.Repeat( "[a]", 1050 ) ) + "]";
			var shared = CompileResult( one + one, new() { Utf8 = utf8 } );
			Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, shared.Diagnostic?.Code );
			Assert.Equal( one.Length, shared.Diagnostic?.ByteOffset );
		}
	}

	[Fact]
	public void LongExpressionKeepsExactWorkAndMidAtomCancellation() {
		var pattern = @"\A[" + String.Join( "~~", Enumerable.Repeat( "[a]", 101 ) ) + @"]\z";
		var steps = new List<int>();
		Compare( Reference( pattern, true ), Compile( pattern, new() { Utf8 = true, WorkObserver = steps.Add } ), "a"u8.ToArray() );
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
		foreach ( var pattern in new[] { @"([\pL||\p{Emoji}])", @"\G([^\pL&&\p{Any}])", @"([\pL||\p{Emoji}]+)" } ) {
			var native = Reference( pattern, true, tolerant: true );
			var managed = Compile( pattern, new() { Utf8 = true, MatchInvalidUtf = true } );
			for ( var offset = 0; offset <= subject.Length; offset++ ) { Compare( native, managed, subject, offset ); }
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8, bool ucp = false, bool tolerant = false, bool alt = true ) =>
		new( Encoding.UTF8.GetBytes( pattern ), utf8 ? Encoding.UTF8 : Encoding.Latin1, new PcreRegexSettings {
			Options = PcreOptions.Compiled | ( utf8 ? PcreOptions.Utf : PcreOptions.None ) | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ) | ( alt ? (PcreOptions)0x08000000 : PcreOptions.None )
		} );
	private static PcreByteCompileResult CompileResult( string pattern, PcreByteOptions options ) {
		// Reflection keeps the tests-first commit buildable before the new internal
		// compile option exists. All assertions exercise the real compiler/matcher.
		var option = typeof( PcreByteOptions ).GetProperty( "AltExtendedClass" );
		Assert.NotNull( option );
		option.SetValue( options, true );
		return PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
	}
	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = CompileResult( pattern, options );
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
