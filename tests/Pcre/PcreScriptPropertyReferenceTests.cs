using System.Globalization;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>All native Unicode17 scripts, aliases and membership transition boundaries.</summary>
public sealed class PcreScriptPropertyReferenceTests {
	private static readonly string[][] Scripts = PcreScriptPropertyFixtureData.Scripts.Split( '\n' )
		.Select( line => line.Split( ';' ) ).ToArray();

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryNativeAliasAndScriptPropertyFormCompilesInBothDomains( bool utf8 ) {
		foreach ( var row in Scripts ) {
			var scalar = int.Parse( row[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture );
			var subject = utf8 ? Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) ) : "A"u8.ToArray();
			foreach ( var name in row[2].Split( ',' ) ) {
			foreach ( var form in new[] { name, "sc:" + name, "Script=" + name, "scx=" + name, "Script_Extensions:" + name } ) {
				Check( $@"\A(\p{{{form}}})\z", subject, utf8 || row[0] == "Latn", utf8 );
			}
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryNativeMembershipTransitionPreservesPrimaryExtendedAndNonmemberCaptures( bool ucp ) {
		var cache = new Dictionary<string, (PcreRegex8Bit Native, PcreCompiledBytePattern Managed)>();
		foreach ( var text in PcreScriptPropertyFixtureData.Boundaries.Split( '\n' ) ) {
			var row = text.Split( ';' );
			var scalar = int.Parse( row[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture );
			var subject = Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) );
			var members = row[2].Split( ',' );
			CheckProperty( "sc=" + row[1], subject, true );
			foreach ( var member in members ) {
				CheckProperty( "scx=" + member, subject, true );
				if ( member != row[1] ) { CheckProperty( "sc=" + member, subject, false ); }
			}
			var nonmember = Scripts.First( script => !members.Contains( script[0] ) )[0];
			CheckProperty( "sc=" + nonmember, subject, false );
			CheckProperty( "scx=" + nonmember, subject, false );
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
	public void EveryScriptProjectsAll256BytesIndependentlyOfUcp( bool ucp ) {
		foreach ( var row in Scripts ) {
		foreach ( var kind in new[] { "sc", "scx" } ) {
			var pattern = $@"\A(\p{{{kind}={row[0]}}})\z";
			var native = Reference( pattern, false, ucp );
			var managed = Compile( pattern, false, ucp );
			for ( var value = 0; value < 256; value++ ) { Compare( native, managed, new[] { (byte)value }, 0 ); }
		}
		}
	}

	[Theory]
	[InlineData( @"\A(\p{sc=Grek}+)\z", "Ωα", true )]
	[InlineData( @"\A(\p{scx=Grek}+)\z", "·̀Ω", true )]
	[InlineData( @"\A(\p{Greek}+)\z", "·̀Ω", true )]
	[InlineData( @"\A\p{sc=Grek}\z", "̀", false )]
	[InlineData( @"\A\p{sc=Inherited}\z", "̀", true )]
	[InlineData( @"\A\p{scx=Zinh}\z", "̀", true )]
	[InlineData( @"\A\p{scx=Common}\z", "·", true )]
	[InlineData( @"\A\p{sc=Zyyy}\z", "ー", true )]
	[InlineData( @"\A\p{Hira}\z", "ー", true )]
	[InlineData( @"\A\p{sc=Hira}\z", "ー", false )]
	[InlineData( @"\A[\p{Grek}\p{Latn}]+\z", "AΩ·", true )]
	[InlineData( @"\A[^\P{scx=Grek}]+\z", "Ω·", true )]
	[InlineData( @"\A[\p{scx=Grek}\P{sc=Zyyy}]+\z", "À·", true )]
	[InlineData( @"\A[^\p{sc=Grek}A]\z", "Ω", false )]
	[InlineData( @"\A[^\p{sc=Grek}A]\z", "B", true )]
	[InlineData( @"\A\p{ S_c_R_i_p_t Ex_te_n_s_i_o_n_s : g_R_e_K }\z", "·", true )]
	[InlineData( @"\A\P{^scx=Greek}\z", "·", true )]
	[InlineData( @"\A\p{^Script=Greek}+\z", "AB", true )]
	[InlineData( @"(?i)\A(?<x>\p{sc=Grek}+)\k<x>\z", "ΩαωΑ", true )]
	[InlineData( @"\A(?=(\p{scx=Grek}+))(\p{Grek}+)\z", "·Ω", true )]
	[InlineData( @"\A(?:(\p{sc=Grek}+)x|(\p{Grek}+))\z", "Ὼ", true )]
	[InlineData( @"(?<=(\p{scx=Grek}{1,2}))x", "Ὼx", true )]
	[InlineData( @"\A(?(?=\p{sc=Grek})(\p{Grek}+)|(\p{sc=Latn}+))\z", "Ω·", true )]
	[InlineData( @"\A(\p{scx=Grek})(?1)\z", "Ω·", true )]
	[InlineData( @"(*CRLF)\A(\p{Common}{2})\z", "\r\n", true )]
	[InlineData( @"(*ANYCRLF)\A([\p{Zyyy}]{2})\z", "\r\n", true )]
	public void ExistingGrammarAndComplementsKeepPinnedOutcomes( string pattern, string text, bool expected ) {
		foreach ( var ucp in new[] { false, true } ) { Check( pattern, Encoding.UTF8.GetBytes( text ), expected, true, ucp ); }
	}

	[Theory]
	[InlineData( "" )]
	[InlineData( "(?i)" )]
	[InlineData( "(?ir)" )]
	[InlineData( "(?a)" )]
	[InlineData( "(?iaDaWaP)" )]
	public void ExplicitScriptMembershipIgnoresLocalCaseAndAsciiRestrictions( string prefix ) {
		foreach ( var ucp in new[] { false, true } ) {
			Check( prefix + @"\A(\p{scx=Grek})\z", "·"u8.ToArray(), true, true, ucp );
			Check( prefix + @"\A(\p{sc=Grek})\z", "µ"u8.ToArray(), false, true, ucp );
			Check( prefix + @"\A([\p{sc=Grek}])\z", "µ"u8.ToArray(), false, true, ucp );
		}
	}

	[Fact]
	public void MalformedUtfFragmentsRetainOriginalOffsetsAndScalarCaptures() {
		var subject = Convert.FromHexString( "FFC2B7CEA9FFCCA041" );
		foreach ( var pattern in new[] { @"(\p{scx=Grek}+)", @"([^\P{Grek}]+)", @"(?<=\p{Grek})\p{Grek}" } ) {
			var native = Reference( pattern, true, true, true );
			var managed = Compile( pattern, true, true, true );
			for ( var offset = 0; offset <= subject.Length; offset++ ) { Compare( native, managed, subject, offset ); }
		}
	}

	[Fact]
	public void ScriptClassesKeepExactWorkLimitsAndCancellation() {
		var steps = new List<int>();
		var pattern = "\\p{Greek}+"u8.ToArray();
		var subject = "Ωα"u8.ToArray();
		var measured = PcreByteCompiler.Compile( pattern, new() { Utf8 = true, WorkObserver = steps.Add } );
		Assert.True( measured.IsSuccess, measured.Diagnostic?.Message );
		Assert.NotNull( measured.Pattern!.Match( subject ).Match );
		Assert.NotEmpty( steps );
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
	[InlineData( @"\p{sc=}", (int)PcreByteDiagnosticCode.InvalidSyntax, 0 )]
	[InlineData( @"a\p{scx: }", (int)PcreByteDiagnosticCode.InvalidSyntax, 1 )]
	[InlineData( @"é\p{Script_Extensions=}", (int)PcreByteDiagnosticCode.InvalidSyntax, 2 )]
	[InlineData( @"\p{sc=NoSuch}", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"\p{Hrkt}", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"\p{sc=Alphabetic}", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"\p{gc=Greek}", (int)PcreByteDiagnosticCode.UnsupportedSyntax, 0 )]
	[InlineData( @"\p{sc=Greek", (int)PcreByteDiagnosticCode.InvalidSyntax, 0 )]
	public void InvalidValuesAndWrongNamespacesKeepOpeningByteDiagnostics( string pattern, int code, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, true ) );
		var actual = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.Equal( (PcreByteDiagnosticCode)code, actual.Diagnostic?.Code );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

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
