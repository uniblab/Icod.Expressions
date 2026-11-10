using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Complete default simple-case equivalence pinned to PCRE2 10.48 / Unicode17.</summary>
public sealed class PcreUnicodeCaseFoldVersionReferenceTests {
	[Theory]
	[InlineData( false, "BC64140B4140E63F4F5B29612A7605EAA1E11C7DAA1745F0D06A0F9478EC161E" )]
	[InlineData( true, "B4F0A081AA1123671D63192F3625681FCC3989D28D881C120F7908AD6081C3D0" )]
	public void AllValidScalarEquivalenceClassesHaveTheNativeFingerprint( bool restricted, string expected ) {
		// Normalize semantic classes, not the implementation's choice of fold key.
		var representatives = new int[0x110000];
		Array.Fill( representatives, -1 );
		for ( var scalar = 0; scalar <= 0x10FFFF; scalar++ ) {
			if ( !Rune.IsValid( scalar ) ) { continue; }
			var key = PcreUtf8CaseFold.Fold( scalar, restricted );
			if ( !Rune.IsValid( key ) ) { throw new InvalidOperationException( "Invalid scalar fold key." ); }
			if ( representatives[key] < 0 ) { representatives[key] = scalar; }
		}
		var bytes = new byte[( 0x110000 - 0x800 ) * 4];
		var index = 0;
		for ( var scalar = 0; scalar <= 0x10FFFF; scalar++ ) {
			if ( !Rune.IsValid( scalar ) ) { continue; }
			BinaryPrimitives.WriteInt32LittleEndian( bytes.AsSpan( index, 4 ),
				representatives[PcreUtf8CaseFold.Fold( scalar, restricted )] );
			index += 4;
		}
		Assert.Equal( bytes.Length, index );
		Assert.Equal( expected, Convert.ToHexString( SHA256.HashData( bytes ) ) );
	}

	[Theory]
	[InlineData( 0x1C89, 0x1C8A )]
	[InlineData( 0xA7CB, 0x0264 )]
	[InlineData( 0xA7CC, 0xA7CD )]
	[InlineData( 0xA7CE, 0xA7CF )]
	[InlineData( 0xA7D2, 0xA7D3 )]
	[InlineData( 0xA7D4, 0xA7D5 )]
	[InlineData( 0xA7D6, 0xA7D7 )]
	[InlineData( 0xA7DA, 0xA7DB )]
	[InlineData( 0xA7DC, 0x019B )]
	[InlineData( 0x10D50, 0x10D70 )]
	[InlineData( 0x10D65, 0x10D85 )]
	[InlineData( 0x16EA0, 0x16EBB )]
	[InlineData( 0x16EB8, 0x16ED3 )]
	[InlineData( 0x0345, 0x03B9 )]
	[InlineData( 0x03D0, 0x03B2 )]
	[InlineData( 0x03F4, 0x03D1 )]
	[InlineData( 0x1FBE, 0x0399 )]
	[InlineData( 0x1E9B, 0x1E61 )]
	[InlineData( 0x1E9E, 0x00DF )]
	[InlineData( 0x2126, 0x03C9 )]
	[InlineData( 0x212B, 0x00E5 )]
	[InlineData( 0x13A0, 0xAB70 )]
	[InlineData( 0x1E900, 0x1E922 )]
	public void PinnedPairsWorkInEveryExistingComparisonPath( int first, int second ) {
		foreach ( var pair in new[] { (first, second), (second, first) } ) {
			var left = char.ConvertFromUtf32( pair.Item1 );
			var right = char.ConvertFromUtf32( pair.Item2 );
			foreach ( var ucp in new[] { false, true } ) {
			foreach ( var restricted in new[] { false, true } ) {
				var prefix = restricted ? "(?ir)" : "(?i)";
				Check( prefix + $@"\A({left})\z", right, true, ucp );
				Check( prefix + $@"\A([{left}])\z", right, true, ucp );
				Check( prefix + $@"\A([\x{{{pair.Item1:X}}}-\x{{{pair.Item1:X}}}])\z", right, true, ucp );
				Check( prefix + $@"\A[^{left}]\z", right, false, ucp );
				Check( prefix + $@"\A(?={left})(.)\z", right, true, ucp );
				Check( prefix + $@"\A(.)(?<={left})\z", right, true, ucp );
				Check( prefix + $@"\A({left})\1\z", right + left, true, ucp );
				Check( prefix + $@"\A({left})(?<=\1){left}\z", right + left, true, ucp );
				Check( prefix + $@"\A(?:({left})x|({left}))\2\z", right + left, true, ucp );
				Check( $@"\A({left})\z", right, false, ucp );
			}
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void MultiMemberSetsAndAsciiRestrictionsRemainSymmetric( bool restricted ) {
		int[][] sets = {
			new[] { 0x004B, 0x006B, 0x212A }, new[] { 0x0053, 0x0073, 0x017F },
			new[] { 0x0398, 0x03B8, 0x03D1, 0x03F4 }, new[] { 0x03A3, 0x03C2, 0x03C3 },
			new[] { 0x0345, 0x0399, 0x03B9, 0x1FBE }, new[] { 0x13A0, 0xAB70 }
		};
		foreach ( var set in sets ) {
		foreach ( var left in set ) {
		foreach ( var right in set ) {
			var expected = !restricted || ( left < 128 ) == ( right < 128 );
			Check( ( restricted ? "(?ir)" : "(?i)" ) + $@"\A({char.ConvertFromUtf32( left )})\z",
				char.ConvertFromUtf32( right ), expected );
		}
		}
		}
	}

	[Theory]
	[InlineData( "ß", "SS" )]
	[InlineData( "ﬀ", "ff" )]
	[InlineData( "İ", "i" )]
	[InlineData( "ı", "I" )]
	[InlineData( "I", "İ" )]
	public void SimpleDefaultFoldingDoesNotAdmitFullOrTurkicMappings( string pattern, string subject ) =>
		Check( "(?i)\\A(" + pattern + ")\\z", subject, false );

	[Theory]
	[InlineData( 0xA7CB, 0x0264 )]
	[InlineData( 0x10D50, 0x10D70 )]
	[InlineData( 0x16EA0, 0x16EBB )]
	public void NewFoldPairsKeepMalformedFragmentOffsetsAndCaptureWidths( int first, int second ) {
		var left = char.ConvertFromUtf32( first );
		var right = char.ConvertFromUtf32( second );
		var subject = Encoding.UTF8.GetBytes( "!" + right ).Concat( new byte[] { 0xFF } )
			.Concat( Encoding.UTF8.GetBytes( left + right + "!" ) ).ToArray();
		foreach ( var pattern in new[] { $"(?i)({left})", $@"(?i)({left})\1", $"(?ir)(?={left})({left})" } ) {
			var native = Reference( pattern, false, true );
			var compiled = Compile( pattern, false, true );
			for ( var offset = 0; offset <= subject.Length; offset++ ) { Compare( native, compiled, subject, offset ); }
		}
	}

	private static void Check( string pattern, string text, bool expected, bool ucp = false ) {
		var subject = Encoding.UTF8.GetBytes( text );
		var native = Reference( pattern, ucp );
		Assert.Equal( expected, native.Match( subject ).Success );
		Compare( native, Compile( pattern, ucp ), subject, 0 );
	}

	private static void Compare( PcreRegex8Bit native, PcreCompiledBytePattern compiled, byte[] subject, int offset ) {
		var expected = native.Match( subject, offset );
		var actual = compiled.Match( subject, offset );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		Assert.Equal( expected.Success, actual.Match is not null );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Index, actual.Match!.ByteIndex );
		Assert.Equal( expected.Length, actual.Match.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool ucp = false, bool tolerant = false ) =>
		new( Encoding.UTF8.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | PcreOptions.Utf | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None )
		} );

	private static PcreCompiledBytePattern Compile( string pattern, bool ucp = false, bool tolerant = false ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ),
			new PcreByteOptions { Utf8 = true, Ucp = ucp, MatchInvalidUtf = tolerant } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
