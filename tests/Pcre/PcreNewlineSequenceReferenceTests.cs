using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>PCRE2 newline sequences, their atomicity, and independent BSR/newline choices.</summary>
public sealed class PcreNewlineSequenceReferenceTests {
	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryByteHasPinnedNewlineMembership( bool bsrAnyCrLf ) {
		const string pattern = @"\A\R\z";
		var native = Reference( pattern, bsrAnyCrLf: bsrAnyCrLf );
		var managed = Compile( pattern, new PcreByteOptions { BsrAnyCrLf = bsrAnyCrLf } );
		for ( var value = 0; value < 256; value++ ) {
			var expected = value is 0x0A or 0x0D || !bsrAnyCrLf && value is 0x0B or 0x0C or 0x85;
			var subject = new[] { (byte)value };
			Assert.True( native.Match( subject ).Success == expected, $"BSR={bsrAnyCrLf}, byte {value:X2}: reference" );
			var result = managed.Match( subject );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.True( ( result.Match is not null ) == expected, $"BSR={bsrAnyCrLf}, byte {value:X2}: managed" );
			if ( expected ) { Assert.Equal( new PcreByteCapture( 0, 1 ), result.Match!.Captures[0] ); }
		}
		Assert.Null( managed.Match( ReadOnlyMemory<byte>.Empty ).Match );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void UnicodeMembershipIsIndependentOfNewlineConventionAndOptions( bool bsrAnyCrLf, bool anyCrLfNewline ) {
		int[] probes = { 0, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x20, 0x84, 0x85, 0x86,
			0xA0, 0x180E, 0x2027, 0x2028, 0x2029, 0x202A, 0x1F600, 0x10FFFF };
		foreach ( var ucp in new[] { false, true } ) {
		foreach ( var prefix in new[] { "", "(?is)" } ) {
			var pattern = prefix + @"\A\R\z";
			var native = Reference( pattern, true, ucp, bsrAnyCrLf: bsrAnyCrLf, anyCrLfNewline: anyCrLfNewline );
			var managed = Compile( pattern, new PcreByteOptions { Utf8 = true, Ucp = ucp,
				BsrAnyCrLf = bsrAnyCrLf, AnyCrLfNewline = anyCrLfNewline } );
			foreach ( var scalar in probes ) {
				var expected = scalar is 0x0A or 0x0D
					|| !bsrAnyCrLf && scalar is 0x0B or 0x0C or 0x85 or 0x2028 or 0x2029;
				var subject = Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) );
				Assert.True( native.Match( subject ).Success == expected, $"U+{scalar:X}, BSR={bsrAnyCrLf}: reference" );
				var result = managed.Match( subject );
				Assert.True( result.IsSuccess, result.Diagnostic?.Message );
				Assert.True( ( result.Match is not null ) == expected, $"U+{scalar:X}, BSR={bsrAnyCrLf}: managed" );
				if ( expected ) { Assert.Equal( new PcreByteCapture( 0, subject.Length ), result.Match!.Captures[0] ); }
			}
			Assert.Null( managed.Match( ReadOnlyMemory<byte>.Empty ).Match );
		}
		}
	}

	[Theory]
	[InlineData( @"(\R)", "0D0A", false, false, 0, 2 )]
	[InlineData( @"\R\n", "0D0A", false, false, -1, 0 )]
	[InlineData( @"(\R)\n|(\r)\n", "0D0A", false, false, 0, 2 )]
	[InlineData( @"\A\R?\n\z", "0D0A", false, false, -1, 0 )]
	[InlineData( @"(\R)+", "0D0A0A", false, false, 0, 3 )]
	[InlineData( @"\R+?\n", "0A0A0A", false, false, 0, 2 )]
	[InlineData( @"\R{2}", "0D0A0A", false, false, 0, 3 )]
	[InlineData( @"\R{2}", "0D0A", false, false, -1, 0 )]
	[InlineData( @"\R++\n", "0A0A0A", false, false, -1, 0 )]
	[InlineData( @"\R+\r\n", "0D0A0D0A", false, false, 0, 4 )]
	[InlineData( @"(\R)", "0D0A", false, true, 0, 2 )]
	[InlineData( @"\R", "0B", false, true, -1, 0 )]
	[InlineData( @"(\R)", "E280A8", true, false, 0, 3 )]
	[InlineData( @"\R", "E280A8", true, true, -1, 0 )]
	[InlineData( @"(\R)", "FF0D0A", true, false, 1, 2 )]
	[InlineData( @"\R\R", "0DFF0A", true, false, -1, 0 )]
	public void AtomicityRepetitionAndCapturesKeepReferenceByteSpans( string pattern, string subjectHex,
		bool utf8, bool bsrAnyCrLf, int expectedIndex, int expectedLength ) {
		var subject = Convert.FromHexString( subjectHex );
		var reference = Reference( pattern, utf8: utf8, invalidUtf: utf8, bsrAnyCrLf: bsrAnyCrLf );
		var native = reference.Match( subject );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( expectedLength, native.Length ); }
		var result = Compile( pattern, new PcreByteOptions { Utf8 = utf8, MatchInvalidUtf = utf8,
			BsrAnyCrLf = bsrAnyCrLf } ).Match( subject );
		AssertEquivalent( reference, subject, 0, result );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void CandidateSearchMatchesReferenceAtEveryCrLfAndInvalidUtfOffset( bool utf8, bool bsrAnyCrLf ) {
		var subjects = new[] { "610D0A58", "0D0A", "0D0A0A", "0DFF0A", "FF0D0AFF" };
		foreach ( var anyCrLf in new[] { false, true } ) {
		foreach ( var pattern in new[] { @"\R", @"\R\n", @"\R{2}", @"\B\R", @"\b\R", @"(\R)?X", @"\R|\n" } ) {
			var native = Reference( pattern, utf8: utf8, invalidUtf: utf8,
				bsrAnyCrLf: bsrAnyCrLf, anyCrLfNewline: anyCrLf );
			var managed = Compile( pattern, new PcreByteOptions { Utf8 = utf8, MatchInvalidUtf = utf8,
				BsrAnyCrLf = bsrAnyCrLf, AnyCrLfNewline = anyCrLf } );
			foreach ( var subjectHex in subjects ) {
				var subject = Convert.FromHexString( subjectHex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					var actual = managed.Match( subject, offset );
					AssertEquivalent( native, subject, offset, actual, $"{pattern}, {subjectHex}, offset {offset}, UTF={utf8}, BSR={bsrAnyCrLf}, newline={anyCrLf}" );
				}
			}
		}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void NewlineSequenceIsInvalidInsideBrackets( bool utf8 ) {
		var result = PcreByteCompiler.Compile( @"[\R]"u8.ToArray(), new PcreByteOptions { Utf8 = utf8 } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( 1, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void SequenceInGeneralLookaroundRemainsExplicitlyUnsupported() {
		var result = PcreByteCompiler.Compile( @"(?=\R)"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void ExpansionWorkAndCancellationRemainBounded() {
		var expansion = PcreByteCompiler.Compile( @"\R{65535}"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, expansion.Diagnostic?.Code );
		Assert.Equal( 2, expansion.Diagnostic?.ByteOffset );
		var limited = Compile( @"\R", new PcreByteOptions { MatchWorkLimit = 2 } ).Match( "\r\n"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, limited.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var compiled = Compile( @"\R", new PcreByteOptions {
			WorkObserver = count => { if ( count == 4 ) { cancellation.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( "\r\n"u8.ToArray(), cancellationToken: cancellation.Token ) );
	}

	private static void AssertEquivalent( PcreRegex8Bit reference, byte[] subject, int offset,
		PcreByteMatchResult actual, string context = "" ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		Assert.True( ( actual.Match?.ByteIndex ?? -1 ) == ( native.Success ? native.Index : -1 ), context );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, actual.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var capture = native.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null,
				actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8 = false, bool ucp = false,
		bool invalidUtf = false, bool bsrAnyCrLf = false, bool anyCrLfNewline = false ) {
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( ucp ) { options |= PcreOptions.Ucp; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		var prefix = ( anyCrLfNewline ? "(*ANYCRLF)" : "(*LF)" )
			+ ( bsrAnyCrLf ? "(*BSR_ANYCRLF)" : "(*BSR_UNICODE)" );
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		return new PcreRegex8Bit( encoding.GetBytes( prefix + pattern ), encoding, new PcreRegexSettings { Options = options } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
