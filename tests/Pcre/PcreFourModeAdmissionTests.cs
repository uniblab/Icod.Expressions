using System.Globalization;
using System.Reflection;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreFourModeAdmissionTests {
	public static IEnumerable<object[]> Cases() => PcreFourModeAdmissionCases.All.SelectMany( row =>
		Enumerable.Range( 0, 4 ).Select( mode => new object[] { row.Id, mode } ) );

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void EverySourceDecisionPinsNativeAdmissionAndManagedOutcome( string id, int mode ) {
		var row = PcreFourModeAdmissionCases.All.Single( value => value.Id == id );
		var expected = row.Expected.Split( ' ' )[mode];
		var utf = ( mode & 1 ) != 0;
		foreach ( var subject in new[] { row.Subject, String.Empty, String.Concat( "z", row.Subject ) }.Distinct() ) {
			if ( mode < 2 ) {
				var bytes = Encoding.UTF8.GetBytes( subject );
				var native = ByteReference( row, bytes, utf );
				AssertNative( row, mode, native.Kind );
				var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( row.Pattern ), new() {
					Utf8 = utf, Ucp = row.Ucp, AltExtendedClass = row.Alt, AllowLookaroundBackslashK = row.AllowReset
				} );
				AssertDecision( row.Id, mode, expected, result.IsSuccess, result.Diagnostic?.Code.ToString(), result.Diagnostic?.ByteOffset );
				if ( !result.IsSuccess ) { continue; }
				Assert.True( native.Kind is "M" or "N", native.ToString() );
				var actual = result.Pattern!.Match( bytes );
				Assert.True( actual.IsSuccess, actual.Diagnostic?.ToString() );
				Assert.Equal( native.Kind == "M", actual.Match is not null );
				Assert.Equal( native.Captures, actual.Match is null ? "" : String.Join( ",", actual.Match.Captures.Select( capture =>
					capture is { } value ? FormattableString.Invariant( $"{value.ByteIndex}:{value.ByteLength}" ) : "-" ) ) );
				Assert.Equal( native.Mark, actual.Match is null ? "" : Convert.ToHexString( actual.Match.MarkName.Span ) );
			} else {
				var native = PcreNative16Oracle.ExecuteSequencePolicies( Words( row.Pattern ), Words( subject ), utf, 0,
					ucp: row.Ucp, allowLookaroundBackslashK: row.AllowReset, altExtendedClass: row.Alt )[0];
				AssertNative( row, mode, native.Kind );
				var result = PcreWordSequenceCompiler.CompileSequence( row.Pattern.AsMemory(), new() {
					Utf16 = utf, Ucp = row.Ucp, AltExtendedClass = row.Alt, AllowLookaroundBackslashK = row.AllowReset
				} );
				AssertDecision( row.Id, mode, expected, result.IsSuccess, result.Diagnostic?.Code.ToString(), result.Diagnostic?.CodeUnitOffset );
				if ( !result.IsSuccess ) { continue; }
				Assert.True( native.Kind is "M" or "N", native.ToString() );
				var actual = result.Pattern!.Match( subject.AsMemory() );
				Assert.True( actual.IsSuccess, actual.Diagnostic?.ToString() );
				Assert.Equal( native.Kind == "M", actual.Match is not null );
				Assert.Equal( native.Captures, actual.Match is null ? "" : String.Join( ",", actual.Match.Captures.Select( capture =>
					capture is { } value ? FormattableString.Invariant( $"{value.CodeUnitIndex}:{value.CodeUnitLength}" ) : "-" ) ) );
				Assert.Equal( native.Mark, actual.Match is null ? "" : String.Concat( actual.Match.MarkName.Select( unit =>
					FormattableString.Invariant( $"{(int)unit:X4}" ) ) ) );
			}
		}
	}

	[Fact]
	public void SourceCatalogHasUniqueIdentitiesAndCompleteFourModeDecisions() {
		Assert.Equal( PcreFourModeAdmissionCases.All.Count, PcreFourModeAdmissionCases.All.Select( row => row.Id ).Distinct().Count() );
		foreach ( var row in PcreFourModeAdmissionCases.All ) {
			Assert.False( String.IsNullOrWhiteSpace( row.Family ) );
			Assert.Equal( 4, row.Expected.Split( ' ' ).Length ); Assert.Equal( 4, row.NativeModes.Length );
			Assert.All( row.NativeModes, value => Assert.True( value is 'Y' or 'N' ) );
			Assert.All( row.Expected.Split( ' ' ), value => Assert.True( value == "S"
				|| value[0] is 'U' or 'I' && int.TryParse( value[1..], out var offset ) && offset >= 0 ) );
		}
	}

	public static IEnumerable<object[]> Options() => PcreFourModeOptionCases.All.Select( row => new object[] { row.ByteMember } );

	[Theory]
	[MemberData( nameof( Options ) )]
	public void EveryOptionDecisionNamesAnExistingMemberAndEvidence( string name ) {
		var row = PcreFourModeOptionCases.All.Single( value => value.ByteMember == name );
		Assert.Contains( row.ByteMember, Members( typeof( PcreByteOptions ) ) );
		Assert.False( String.IsNullOrWhiteSpace( row.Evidence ) );
		if ( row.WordMember == "-" ) { return; }
		if ( row.WordMember == "@CompileLiteral" ) {
			Assert.NotNull( typeof( PcreWordLiteralCompiler ).GetMethod( "CompileLiteral", BindingFlags.Static | BindingFlags.NonPublic ) );
			return;
		}
		var parts = row.WordMember.Split( '.' );
		Assert.Contains( parts[0], Members( typeof( PcreWordOptions ) ) );
		if ( parts.Length == 2 ) {
			Assert.Equal( "Policy", parts[0] );
			Assert.Contains( parts[1], Members( typeof( PcreMatchPolicy ) ) );
		}
	}

	[Fact]
	public void NoCurrentCallerOptionCanEscapeTheInventory() {
		var rows = PcreFourModeOptionCases.All;
		Assert.Equal( rows.Count, rows.Select( row => row.ByteMember ).Distinct().Count() );
		Assert.Equal( Members( typeof( PcreByteOptions ) ), rows.Select( row => row.ByteMember ).OrderBy( name => name, StringComparer.Ordinal ) );
		Assert.Equal( Members( typeof( PcreWordOptions ) ), rows.Where( row => row.WordMember is not "-" and not "@CompileLiteral" )
			.Select( row => row.WordMember.Split( '.' )[0] ).Distinct().OrderBy( name => name, StringComparer.Ordinal ) );
		Assert.Equal( Members( typeof( PcreMatchPolicy ) ), rows.Where( row => row.WordMember.StartsWith( "Policy.", StringComparison.Ordinal ) )
			.Select( row => row.WordMember[7..] ).OrderBy( name => name, StringComparer.Ordinal ) );
	}

	private static IEnumerable<string> Members( Type type ) => type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
		.Where( property => property.SetMethod is not null ).Select( property => property.Name ).OrderBy( name => name, StringComparer.Ordinal );

	private static void AssertNative( PcreFourModeCase row, int mode, string kind ) {
		Assert.True( ( row.NativeModes[mode] == 'Y' ) == ( kind is "M" or "N" or "X" ), $"Native {row.Id}/{mode}: {kind}" );
	}

	private static void AssertDecision( string id, int mode, string expected, bool success, string? code, int? offset ) {
		if ( expected == "S" ) { Assert.True( success, $"{id}/{mode}: {code}@{offset}" ); return; }
		Assert.False( success, $"{id}/{mode}: unexpected admission" );
		Assert.True( code == ( expected[0] == 'U' ? "UnsupportedSyntax" : "InvalidSyntax" )
			&& offset == int.Parse( expected[1..], CultureInfo.InvariantCulture ), $"{id}/{mode}: {code}@{offset}, expected {expected}" );
	}

	private static (string Kind, string Captures, string Mark) ByteReference( PcreFourModeCase row, byte[] subject, bool utf ) {
		PcreRegex8Bit compiled;
		try {
			compiled = new( Encoding.UTF8.GetBytes( row.Pattern ), utf ? Encoding.UTF8 : Encoding.Latin1, new PcreRegexSettings {
				Options = PcreOptions.Compiled | ( utf ? PcreOptions.Utf : PcreOptions.None ) | ( row.Ucp ? PcreOptions.Ucp : PcreOptions.None )
					| ( row.Alt ? (PcreOptions)0x08000000 : PcreOptions.None ),
				ExtraCompileOptions = row.AllowReset ? (PcreExtraCompileOptions)0x40 : 0, ParensLimit = 512
			} );
		} catch ( PcrePatternException ) { return ("E", "", ""); }
		var result = compiled.Match( subject );
		if ( !result.Success ) { return ("N", "", ""); }
		var captures = new string[result.Groups.Count];
		for ( var index = 0; index < captures.Length; index++ ) {
			var group = result.Groups[index];
			// PCRE.NET Length clamps reversed K endpoints. EndIndex retains the native end.
			captures[index] = group.Success ? FormattableString.Invariant( $"{group.Index}:{group.EndIndex - group.Index}" ) : "-";
		}
		return ("M", String.Join( ",", captures ), Convert.ToHexString( result.Mark.ToArray() ));
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
