using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Fixed PCRE2 whitespace lists, independent of UCP and newline convention.</summary>
public sealed class PcreSpaceEscapeReferenceTests {
	[Theory]
	[InlineData( "h", false )]
	[InlineData( "H", false )]
	[InlineData( "v", false )]
	[InlineData( "V", false )]
	[InlineData( "h", true )]
	[InlineData( "H", true )]
	[InlineData( "v", true )]
	[InlineData( "V", true )]
	public void EveryByteMatchesPinnedMembershipInsideAndOutsideBrackets( string escape, bool bracket ) {
		var pattern = bracket ? $"\\A[\\{escape}]\\z" : $"\\A\\{escape}\\z";
		var native = Reference( pattern );
		var managed = Compile( pattern );
		for ( var value = 0; value < 256; value++ ) {
			var subject = new[] { (byte)value };
			var expected = escape switch {
				"h" => value is 0x09 or 0x20 or 0xA0,
				"H" => value is not (0x09 or 0x20 or 0xA0),
				"v" => value is >= 0x0A and <= 0x0D or 0x85,
				_ => value is not (>= 0x0A and <= 0x0D or 0x85)
			};
			var reference = native.Match( subject );
			Assert.True( reference.Success == expected, $"{pattern}, byte {value:X2}: reference membership" );
			var result = managed.Match( subject );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.True( ( result.Match is not null ) == expected, $"{pattern}, byte {value:X2}: managed membership" );
			if ( expected ) { Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match!.Captures[0] ); }
		}
		Assert.Null( managed.Match( ReadOnlyMemory<byte>.Empty ).Match );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void UnicodeListsAndTheirBoundariesAgreeWithPinnedReference( bool ucp, bool caseless ) {
		// Literal reference lists are deliberately independent of the production predicates.
		int[] horizontal = { 0x09, 0x20, 0xA0, 0x1680, 0x180E, 0x2000, 0x2001, 0x2002,
			0x2003, 0x2004, 0x2005, 0x2006, 0x2007, 0x2008, 0x2009, 0x200A, 0x202F, 0x205F, 0x3000 };
		int[] vertical = { 0x0A, 0x0B, 0x0C, 0x0D, 0x85, 0x2028, 0x2029 };
		var probes = horizontal.Concat( vertical ).SelectMany( scalar => new[] { scalar - 1, scalar, scalar + 1 } )
			.Concat( new[] { 0, 0x41, 0xE9, 0x200B, 0xFEFF, 0x1F600, 0x10FFFF } ).Distinct().ToArray();
		foreach ( var escape in new[] { "h", "H", "v", "V" } ) {
		foreach ( var bracket in new[] { false, true } ) {
			var atom = bracket ? $"[\\{escape}]" : $"\\{escape}";
			var pattern = $"\\A{atom}\\z";
			var native = Reference( pattern, utf8: true, ucp: ucp, caseless: caseless );
			var managed = Compile( pattern, new PcreByteOptions { Utf8 = true, Ucp = ucp,
				Caseless = caseless, AsciiBsD = true } );
			foreach ( var scalar in probes ) {
				var subject = Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) );
				var member = ( escape is "h" or "H" ? horizontal : vertical ).Contains( scalar );
				var expected = escape is "H" or "V" ? !member : member;
				Assert.True( native.Match( subject ).Success == expected, $"{pattern}, U+{scalar:X}: reference membership" );
				var result = managed.Match( subject );
				Assert.True( result.IsSuccess, result.Diagnostic?.Message );
				Assert.True( ( result.Match is not null ) == expected, $"{pattern}, U+{scalar:X}: managed membership" );
				if ( expected ) { Assert.Equal( new PcreByteCapture( 0, subject.Length ), result.Match!.Captures[0] ); }
			}
			Assert.Null( managed.Match( ReadOnlyMemory<byte>.Empty ).Match );
		}
		}
	}

	[Theory]
	[InlineData( @"(\h+)\H", "0920A058", false, 0, 4 )]
	[InlineData( @"(\v+?)\v", "0D0A0B", false, 0, 2 )]
	[InlineData( @"[^\H]", "A0", false, 0, 1 )]
	[InlineData( @"[\h\v]", "85", false, 0, 1 )]
	[InlineData( @"[^\h\v]", "85", false, -1, 0 )]
	[InlineData( @"[a-z\h]", "20", false, 0, 1 )]
	[InlineData( @"(\h+)\H", "09C2A0E19A8058", true, 0, 7 )]
	[InlineData( @"[\h\v]", "E280A8", true, 0, 3 )]
	[InlineData( @"[^\V]", "E280A9", true, 0, 3 )]
	[InlineData( @"\H+", "FF61C3A9", true, 1, 3 )]
	[InlineData( @"\h\h", "09FFC2A0", true, -1, 0 )]
	public void RepetitionUnionsCapturesAndInvalidUtfKeepReferenceByteSpans(
		string pattern, string subjectHex, bool utf8, int expectedIndex, int expectedLength ) {
		var subject = Convert.FromHexString( subjectHex );
		var native = Reference( pattern, utf8: utf8, invalidUtf: utf8 ).Match( subject );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( expectedLength, native.Length ); }
		var result = Compile( pattern, new PcreByteOptions { Utf8 = utf8, MatchInvalidUtf = utf8 } ).Match( subject );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		Assert.Equal( expectedIndex, result.Match?.ByteIndex ?? -1 );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Groups.Count, result.Match!.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var capture = native.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null,
				result.Match.Captures[group] );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void CrLfCandidateSearchAgreesAtEveryStartOffset( bool utf8 ) {
		var subject = "\r\nX\r\n"u8.ToArray();
		foreach ( var pattern in new[] { @"\hX", @"\HX", @"\vX", @"\VX", @"[\v]X", @"[^\h]X", @"(\v)\v" } ) {
			var native = Reference( pattern, utf8: utf8, anyCrLf: true );
			var managed = Compile( pattern, new PcreByteOptions { Utf8 = utf8, AnyCrLfNewline = true } );
			for ( var offset = 0; offset <= subject.Length; offset++ ) {
				var expected = native.Match( subject, offset );
				var actual = managed.Match( subject, offset );
				Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
				Assert.True( ( actual.Match?.ByteIndex ?? -1 ) == ( expected.Success ? expected.Index : -1 ),
					$"{pattern}, offset {offset}, UTF={utf8}: CRLF candidate span" );
				if ( expected.Success ) { Assert.Equal( expected.Length, actual.Match!.ByteLength ); }
			}
		}
	}

	[Theory]
	[InlineData( @"[\h-z]", false )]
	[InlineData( @"[a-\v]", false )]
	[InlineData( @"[\h-z]", true )]
	[InlineData( @"[a-\v]", true )]
	public void WhitespaceClassesCannotBeRangeEndpoints( string pattern, bool utf8 ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new PcreByteOptions { Utf8 = utf8 } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
	}

	[Fact]
	public void ScalarWhitespaceKeepsWorkAndCancellationAccounting() {
		var subject = "\u00a0"u8.ToArray();
		var limited = Compile( @"\h", new PcreByteOptions { Utf8 = true, MatchWorkLimit = 4 } ).Match( subject );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		Assert.Equal( new PcreByteCapture( 0, 2 ),
			Compile( @"\h", new PcreByteOptions { Utf8 = true, MatchWorkLimit = 5 } ).Match( subject ).Match?.Captures[0] );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( @"[\h]", new PcreByteOptions { Utf8 = true,
			WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8 = false, bool ucp = false,
		bool caseless = false, bool invalidUtf = false, bool anyCrLf = false ) {
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( ucp ) { options |= PcreOptions.Ucp; }
		if ( caseless ) { options |= PcreOptions.Caseless; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ),
			encoding, new PcreRegexSettings { Options = options,
				ExtraCompileOptions = utf8 ? PcreExtraCompileOptions.AsciiBsD : PcreExtraCompileOptions.None } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var compiled = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( compiled.IsSuccess, compiled.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( compiled.Pattern );
	}
}
