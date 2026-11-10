using System.Globalization;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>The complete binary/Bidi property catalogue pinned to PCRE2 10.48.</summary>
public sealed class PcreBinaryBidiPropertyReferenceTests {
	private static readonly string[][] Binary = ReadRows( PcreBinaryPropertyFixtureData.Binary );
	private static readonly string[][] Bidi = ReadRows( PcreBinaryPropertyFixtureData.Bidi );
	private static readonly string[][] Boundaries = ReadRows( PcreBinaryPropertyFixtureData.Boundaries );
	private static string[][] ReadRows( string text ) => text.Split( '\n', StringSplitOptions.RemoveEmptyEntries )
		.Select( line => line.TrimEnd( '\r' ).Split( ';' ) ).ToArray();
	private static int Scalar( string hex ) => int.Parse( hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture );
	private static ulong Mask( string hex ) => ulong.Parse( hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture );

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void AllBinaryAliasesAndBidiFormsCompileInBothDomains( bool utf8 ) {
		var ascii = Mask( Boundaries.Single( row => row[0] == "41" )[1] );
		foreach ( var row in Binary ) {
			var id = int.Parse( row[0], CultureInfo.InvariantCulture );
			foreach ( var name in row[4].Split( ',' ) ) {
				Check( $@"\A(\p{{{name}}})\z", utf8 ? Bytes( Scalar( row[2] ) ) : "A"u8.ToArray(),
					utf8 || ( ascii & ( 1UL << id ) ) != 0, utf8 );
			}
			if ( utf8 ) { Check( $@"\A(\p{{{row[1]}}})\z", Bytes( Scalar( row[3] ) ), false, true ); }
		}
		foreach ( var row in Bidi ) {
		foreach ( var name in new[] { "BC=" + row[1], "Bidi_Class:" + row[1], "bidi" + row[1].ToLowerInvariant() } ) {
			Check( $@"\A(\p{{{name}}})\z", utf8 ? Bytes( Scalar( row[2] ) ) : "A"u8.ToArray(), utf8 || row[1] == "L", utf8 );
		}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void AllNativeStateTransitionsKeepMemberAndNonmemberCaptures( bool ucp ) {
		var cache = new Dictionary<string, (PcreRegex8Bit Native, PcreCompiledBytePattern Managed)>();
		for ( var index = 0; index < Boundaries.Length; index++ ) {
			var row = Boundaries[index];
			var subject = Bytes( Scalar( row[0] ) );
			var mask = Mask( row[1] );
			var candidates = mask | Mask( Boundaries[Math.Max( 0, index - 1 )][1] )
				| Mask( Boundaries[Math.Min( Boundaries.Length - 1, index + 1 )][1] );
			var firstNonmember = true;
			for ( var id = 0; id < Binary.Length; id++ ) {
				var member = ( mask & ( 1UL << id ) ) != 0;
				if ( ( candidates & ( 1UL << id ) ) != 0 || !member && firstNonmember ) {
					CheckProperty( Binary[id][1], subject, member );
					if ( !member ) { firstNonmember = false; }
				}
			}
			CheckProperty( "BC=" + row[2], subject, true );
			foreach ( var value in new[] { Boundaries[Math.Max( 0, index - 1 )][2],
				Boundaries[Math.Min( Boundaries.Length - 1, index + 1 )][2], row[2] == "L" ? "R" : "L" }.Distinct() ) {
				if ( value != row[2] ) { CheckProperty( "BC=" + value, subject, false ); }
			}
		}
		void CheckProperty( string property, byte[] subject, bool expected ) {
			if ( !cache.TryGetValue( property, out var pair ) ) {
				var pattern = $@"\A(\p{{{property}}})\z";
				pair = (Reference( pattern, true, ucp ), Compile( pattern, true, ucp ));
				cache.Add( property, pair );
			}
			Assert.Equal( expected, pair.Native.Match( subject ).Success );
			Compare( pair.Native, pair.Managed, subject, 0 );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryPropertyProjectsAll256ByteValues( bool ucp ) {
		foreach ( var property in Binary.Select( row => row[1] ).Concat( Bidi.Select( row => "BC=" + row[1] ) ) ) {
			var pattern = $@"\A(\p{{{property}}})\z";
			var native = Reference( pattern, false, ucp );
			var managed = Compile( pattern, false, ucp );
			for ( var value = 0; value < 256; value++ ) { Compare( native, managed, new[] { (byte)value }, 0 ); }
		}
	}

	[Theory]
	[InlineData( @"\A(\p{Alphabetic}+)\z", "ΩA中", true )]
	[InlineData( @"\A\p{Alpha}\z", "ͅ", true )]
	[InlineData( @"(?i)\A\p{Lowercase}\z", "A", false )]
	[InlineData( @"(?i)\A\p{Lower}\z", "a", true )]
	[InlineData( @"(?i)\A\p{Uppercase}\z", "a", false )]
	[InlineData( @"(?i)\A\p{Upper}\z", "A", true )]
	[InlineData( @"\A\p{Emoji}\z", "#", true )]
	[InlineData( @"\A\p{Emoji_Modifier}\z", "🏻", true )]
	[InlineData( @"\A\p{Variation_Selector}\z", "️", true )]
	[InlineData( @"\A\p{InCB}\z", "क", true )]
	[InlineData( @"\A\p{Bidi_Mirrored}\z", "(", true )]
	[InlineData( @"\A\p{BidiC}\z", "‎", true )]
	[InlineData( @"\A\p{BC:R}\z", "\u0590", true )]
	[InlineData( @"\A\p{BC:ET}\z", "\u20cf", true )]
	[InlineData( @"\A\p{Bidi_Class=NSM}\z", "̀", true )]
	[InlineData( @"\A\p{bidil}\z", "A", true )]
	[InlineData( @"\A[\p{Emoji}\p{BC=R}]+\z", "#א", true )]
	[InlineData( @"\A[^\P{Alphabetic}]+\z", "AΩ", true )]
	[InlineData( @"\A\P{^Emoji}\z", "#", true )]
	[InlineData( @"\A\p{^Emoji}\z", "A", true )]
	[InlineData( @"\A(\p{Emoji}{1,3}?)\p{Emoji}\z", "😀😁", true )]
	[InlineData( @"(?i)\A(\p{Uppercase})\1\z", "Aa", true )]
	[InlineData( @"(?<=(\p{Alpha}{1,2}))x", "ΩAx", true )]
	[InlineData( @"\A(?(?=\p{BC:R})(\p{Alpha}+)|(\p{Emoji}+))\z", "א", true )]
	[InlineData( @"\A(\p{Alpha})(?1)\z", "ΩA", true )]
	[InlineData( @"(*CRLF)\A(\p{White_Space}{2})\z", "\r\n", true )]
	[InlineData( @"\A\p{White_Space}\z", "\u180e", false )]
	[InlineData( @"\A\p{Xps}\z", "\u180e", true )]
	[InlineData( @"\A\p{ASCII_Hex_Digit}\z", "０", false )]
	[InlineData( @"\A\p{Hex_Digit}\z", "０", true )]
	[InlineData( @"\A\p{ _a_L-p_H_a_ }\z", "Ω", true )]
	[InlineData( @"\A\p{ Bi_di Class : a_l }\z", "ا", true )]
	public void NativeRulesComplementsAndExistingGrammarKeepExactOutcomes( string pattern, string text, bool expected ) {
		foreach ( var ucp in new[] { false, true } ) { Check( pattern, Encoding.UTF8.GetBytes( text ), expected, true, ucp ); }
	}

	[Theory]
	[InlineData( "" )]
	[InlineData( "(?i)" )]
	[InlineData( "(?ir)" )]
	[InlineData( "(?a)" )]
	[InlineData( "(?iaDaWaP)" )]
	public void ExplicitPropertiesIgnoreCaseAndAsciiPolicies( string prefix ) {
		foreach ( var ucp in new[] { false, true } ) {
			Check( prefix + @"\A(\p{Alpha})\z", "Ω"u8.ToArray(), true, true, ucp );
			Check( prefix + @"\A([\p{Lower}])\z", "A"u8.ToArray(), false, true, ucp );
			Check( prefix + @"\A(\p{BC=R})\z", "א"u8.ToArray(), true, true, ucp );
		}
	}

	[Fact]
	public void MalformedFragmentsKeepOriginalOffsetsAndCaptureWidths() {
		var subject = Convert.FromHexString( "FFCEA941FFD790CCA041" );
		foreach ( var pattern in new[] { @"(\p{Alpha}+)", @"([\p{BC=R}\p{Alpha}]+)", @"(?<=\p{Alpha})\p{Alpha}" } ) {
			var native = Reference( pattern, true, true, true );
			var managed = Compile( pattern, true, true, true );
			for ( var offset = 0; offset <= subject.Length; offset++ ) { Compare( native, managed, subject, offset ); }
		}
	}

	[Fact]
	public void PropertyClassesRetainExactWorkAndCancellation() {
		var pattern = "\\p{Alpha}+"u8.ToArray();
		var subject = "ΩA"u8.ToArray();
		var steps = new List<int>();
		var measured = PcreByteCompiler.Compile( pattern, new() { Utf8 = true, WorkObserver = steps.Add } );
		Assert.True( measured.IsSuccess, measured.Diagnostic?.Message );
		Assert.NotNull( measured.Pattern!.Match( subject ).Match );
		var exact = PcreByteCompiler.Compile( pattern, new() { Utf8 = true, MatchWorkLimit = steps.Count } );
		Assert.NotNull( exact.Pattern!.Match( subject ).Match );
		var exhausted = PcreByteCompiler.Compile( pattern, new() { Utf8 = true, MatchWorkLimit = steps.Count - 1 } );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Pattern!.Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = PcreByteCompiler.Compile( pattern, new() { Utf8 = true,
			WorkObserver = count => { if ( count == 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Pattern!.Match( subject, cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( @"\p{BC=}", (int)PcreByteDiagnosticCode.InvalidSyntax, 0 )]
	[InlineData( @"é\p{Bidi_Class: }", (int)PcreByteDiagnosticCode.InvalidSyntax, 2 )]
	[InlineData( @"\p{BC=Left_To_Right}", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"\p{Alphabetic=Yes}", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"\p{InCB=Linker}", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"\p{Indic_Conjunct_Break}", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"\p{IsAlphabetic}", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"\p{InEmoticons}", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"\p{sc=Emoji}", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"\p{gc=Emoji}", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	public void UnsupportedNamesAndNamespacesKeepOpeningByteDiagnostics( string pattern, int code, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, true ) );
		var actual = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.Equal( (PcreByteDiagnosticCode)code, actual.Diagnostic?.Code );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

	private static byte[] Bytes( int scalar ) => Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) );
	private static void Check( string pattern, byte[] subject, bool expected, bool utf8, bool ucp = false ) {
		var native = Reference( pattern, utf8, ucp );
		Assert.Equal( expected, native.Match( subject ).Success );
		Compare( native, Compile( pattern, utf8, ucp ), subject, 0 );
	}
	private static void Compare( PcreRegex8Bit native, PcreCompiledBytePattern managed, byte[] subject, int offset ) {
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
	private static PcreRegex8Bit Reference( string pattern, bool utf8, bool ucp = false, bool tolerant = false ) =>
		new( Encoding.UTF8.GetBytes( pattern ), utf8 ? Encoding.UTF8 : Encoding.Latin1, new PcreRegexSettings {
			Options = PcreOptions.Compiled | ( utf8 ? PcreOptions.Utf : PcreOptions.None ) | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None )
		} );
	private static PcreCompiledBytePattern Compile( string pattern, bool utf8, bool ucp = false, bool tolerant = false ) {
		var actual = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = utf8, Ucp = ucp, MatchInvalidUtf = tolerant } );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( actual.Pattern );
	}
}
