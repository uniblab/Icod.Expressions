using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>UTF special properties pinned to PCRE.NET 1.6.0 / PCRE2 10.48 Compiled/JIT.</summary>
public sealed class PcreSpecialPropertyReferenceTests {
	private static readonly string[] Names = { "Any", "Xan", "Xps", "Xsp", "Xwd", "Xuc" };

	// Stable category representatives and scalar boundaries: no runtime category API is an oracle.
	private static readonly (int Scalar, string Members)[] UnicodeCases = {
		( 0x0085, "Any Xps Xsp" ), ( 0x009F, "Any" ),
		( 0x00A0, "Any Xps Xsp Xuc" ), ( 0x00A1, "Any Xuc" ),
		( 0x00B2, "Any Xan Xwd Xuc" ), ( 0x00E9, "Any Xan Xwd Xuc" ),
		( 0x01C5, "Any Xan Xwd Xuc" ), ( 0x02B0, "Any Xan Xwd Xuc" ),
		( 0x0301, "Any Xwd Xuc" ), ( 0x0378, "Any Xuc" ),
		( 0x0660, "Any Xan Xwd Xuc" ), ( 0x0903, "Any Xuc" ),
		( 0x1680, "Any Xps Xsp Xuc" ), ( 0x180D, "Any Xwd Xuc" ),
		( 0x180E, "Any Xps Xsp Xuc" ), ( 0x180F, "Any Xwd Xuc" ),
		( 0x1FFF, "Any Xuc" ), ( 0x2000, "Any Xps Xsp Xuc" ),
		( 0x2001, "Any Xps Xsp Xuc" ), ( 0x2002, "Any Xps Xsp Xuc" ),
		( 0x2003, "Any Xps Xsp Xuc" ), ( 0x2004, "Any Xps Xsp Xuc" ),
		( 0x2005, "Any Xps Xsp Xuc" ), ( 0x2006, "Any Xps Xsp Xuc" ),
		( 0x2007, "Any Xps Xsp Xuc" ), ( 0x2008, "Any Xps Xsp Xuc" ),
		( 0x2009, "Any Xps Xsp Xuc" ), ( 0x200A, "Any Xps Xsp Xuc" ),
		( 0x200B, "Any Xuc" ), ( 0x200D, "Any Xuc" ),
		( 0x2027, "Any Xuc" ), ( 0x2028, "Any Xps Xsp Xuc" ),
		( 0x2029, "Any Xps Xsp Xuc" ), ( 0x202A, "Any Xuc" ),
		( 0x202F, "Any Xps Xsp Xuc" ), ( 0x203F, "Any Xwd Xuc" ),
		( 0x205F, "Any Xps Xsp Xuc" ), ( 0x2060, "Any Xuc" ),
		( 0x20DD, "Any Xuc" ), ( 0x212A, "Any Xan Xwd Xuc" ),
		( 0x2160, "Any Xan Xwd Xuc" ), ( 0x3000, "Any Xps Xsp Xuc" ),
		( 0x3001, "Any Xuc" ), ( 0x4E00, "Any Xan Xwd Xuc" ),
		( 0xD7FF, "Any Xuc" ), ( 0xE000, "Any Xuc" ),
		( 0xFDD0, "Any Xuc" ), ( 0xFEFF, "Any Xuc" ),
		( 0xFFFE, "Any Xuc" ), ( 0xFFFF, "Any Xuc" ),
		( 0x10000, "Any Xan Xwd Xuc" ), ( 0x10400, "Any Xan Xwd Xuc" ),
		( 0x1D7CE, "Any Xan Xwd Xuc" ), ( 0x1F600, "Any Xuc" ),
		( 0x10FFFE, "Any Xuc" ), ( 0x10FFFF, "Any Xuc" )
	};

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void LiteralMembershipIncludesEveryAsciiScalarAndStableUnicodeCategories( bool ucp ) {
		foreach ( var name in Names ) {
		foreach ( var form in new[] { $@"\p{{{name}}}", $@"\P{{{name}}}", $@"\p{{^{name}}}",
			$@"\P{{^{name}}}", $@"[\p{{{name}}}]", $@"[^\P{{{name}}}]" } ) {
			var pattern = $@"\A{form}\z";
			var reference = Reference( pattern, ucp: ucp );
			var rows = Enumerable.Range( 0, 128 ).Select( scalar => (Scalar: scalar,
				Member: name switch {
					"Any" => true,
					"Xan" => scalar is >= 0x30 and <= 0x39 or >= 0x41 and <= 0x5A or >= 0x61 and <= 0x7A,
					"Xwd" => scalar is >= 0x30 and <= 0x39 or >= 0x41 and <= 0x5A or >= 0x61 and <= 0x7A or 0x5F,
					"Xuc" => scalar is 0x24 or 0x40 or 0x60,
					_ => scalar is >= 0x09 and <= 0x0D or 0x20
				} ) ).Concat( UnicodeCases.Select( row => ( row.Scalar, Member: row.Members.Split( ' ' ).Contains( name ) ) ) )
				.Select( row => (Subject: Encoding.UTF8.GetBytes( char.ConvertFromUtf32( row.Scalar ) ),
					Expected: form.StartsWith( @"\P{^", StringComparison.Ordinal ) || form.StartsWith( "[^", StringComparison.Ordinal )
						? row.Member : form.StartsWith( @"\P", StringComparison.Ordinal ) || form.StartsWith( @"\p{^", StringComparison.Ordinal ) ? !row.Member : row.Member) ).ToArray();
			foreach ( var row in rows ) {
				AssertNative( reference, row.Subject, 0, row.Expected ? 0 : -1,
					row.Expected ? row.Subject.Length : 0, row.Expected ? $"0:{row.Subject.Length}" : "" );
			}
			AssertNative( reference, Array.Empty<byte>(), 0, -1, 0, "" );
			var compiled = Compile( pattern, new PcreByteOptions { Utf8 = true, Ucp = ucp } );
			foreach ( var row in rows ) { Compare( reference, row.Subject, 0, compiled.Match( row.Subject ), pattern ); }
			Compare( reference, Array.Empty<byte>(), 0, compiled.Match( Array.Empty<byte>() ), pattern );
		}
		}
	}

	[Theory]
	[InlineData( @"\p{ a_n-Y }", "\0", 0, 1, "0:1" )]
	[InlineData( @"\p{X_A-N}", "é", 0, 2, "0:2" )]
	[InlineData( @"\p{ x-p_s }", "\u180e", 0, 3, "0:3" )]
	[InlineData( @"\p{ x_s-p }", "\u0085", 0, 2, "0:2" )]
	[InlineData( @"\p{ x_w-d }", "\u0301", 0, 2, "0:2" )]
	[InlineData( @"\p{ x-u_c }", "@", 0, 1, "0:1" )]
	[InlineData( @"\p{ _- ^ _-Xwd}", "!", 0, 1, "0:1" )]
	[InlineData( @"\P{ _- ^ _-Xwd}", "é", 0, 2, "0:2" )]
	[InlineData( @"[^\p{ _- ^ _-Xwd}]", "é", 0, 2, "0:2" )]
	[InlineData( @"[^\p{Xan}\p{Xps}]", "!", 0, 1, "0:1" )]
	[InlineData( @"[^\p{Xan}\p{Xps}]", "é", -1, 0, "" )]
	[InlineData( @"[\P{Any}\p{Xwd}]", "\u0301", 0, 2, "0:2" )]
	[InlineData( @"[\p{Any}\P{Any}]", "😀", 0, 4, "0:4" )]
	[InlineData( @"[^\p{Any}\P{Any}]", "😀", -1, 0, "" )]
	[InlineData( @"\A(\p{Any}{2,3})\p{Any}\z", "é😀x", 0, 7, "0:7,0:6" )]
	[InlineData( @"\A(\p{Any}{1,3}?)\p{Any}\z", "é😀x", 0, 7, "0:7,0:6" )]
	[InlineData( @"\A\p{Any}++x\z", "éx", -1, 0, "" )]
	[InlineData( @"\A(?:(\p{Xan}+)x|(\p{Xwd}+))\z", "é\u0301", 0, 4, "0:4,-,0:4" )]
	[InlineData( @"\A(\p{Xuc}+)\1\z", "é😀é😀", 0, 12, "0:12,0:6" )]
	[InlineData( @"(?=(\p{Any}{2}))\1", "é😀", 0, 6, "0:6,0:6" )]
	[InlineData( @"(?!(\p{Xan}))\p{Any}", "!", 0, 1, "0:1,-" )]
	[InlineData( @"(?>(\p{Any}+))x", "éx", -1, 0, "" )]
	[InlineData( @"(?*(\p{Any}+))\1x", "ééx", 0, 5, "0:5,0:4" )]
	[InlineData( @"(?<=(\p{Any}))x", "😀x", 4, 1, "4:1,0:4" )]
	[InlineData( @"(?<=\p{Xuc}{1,2})x", "é😀x", 6, 1, "6:1" )]
	[InlineData( @"(?<!\p{Xwd})x", "\u0301x", -1, 0, "" )]
	[InlineData( @"\A(?&a)\z(?(DEFINE)(?<a>\p{Xan}(?&a)?\p{Xps}))", "éΩ \t", 0, 6, "0:6,-" )]
	[InlineData( @"\A(?<a>\p{Xan}(?&a)?\p{Xps})\z", "éΩ \t", 0, 6, "0:6,0:6" )]
	[InlineData( @"\A(?:(?<a>\p{Xan}(?&a)?\p{Xps})x|\p{Any}+)(?(a)X|)\z", "éΩ \t", 0, 6, "0:6,-" )]
	public void LooseNamesComplementsAndSharedContinuationsPreserveAllCaptureSlots( string pattern,
		string text, int index, int length, string captures ) => Check( pattern, text, index, length, captures );

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ExplicitPropertiesIgnoreUcpCaselessRestrictionsAndAsciiShorthandModes( bool ucp ) {
		foreach ( var mode in new[] { "", "(?i)", "(?ir)", "(?aD)", "(?aS)", "(?aW)", "(?aP)", "(?aT)", "(?iaT)", "(?iraT)" } ) {
		foreach ( var row in new[] { ("Any", "😀"), ("Xan", "٣"), ("Xps", "\u180e"),
			("Xsp", "\u0085"), ("Xwd", "\u0301"), ("Xuc", "\u00a0") } ) {
			var pattern = mode + $@"\A(\p{{{row.Item1}}})\z";
			var bytes = Encoding.UTF8.GetByteCount( row.Item2 );
			Check( pattern, row.Item2, 0, bytes, $"0:{bytes},0:{bytes}", ucp );
		}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void InvalidUtfBarriersOffsetsGAndCrLfSearchUseTheExistingSubjectProtocol( bool anyCrLf ) {
		// Positive Xuc has a pinned native JIT invalid-UTF defect; its safe
		// fragment contract is tested with explicit expectations below.
		foreach ( var atom in new[] { @"\p{Any}", @"\P{Any}", @"\p{Xps}", @"\P{Xps}",
			@"\P{Xuc}", @"[\p{Any}]", @"[^\P{Xps}]" } ) {
		foreach ( var pattern in new[] { atom, atom + "+", @"\G(" + atom + @")", @"(?m)" + atom + "$",
			atom + "A", @"(?:" + atom + @"|\n)A", @"(?<=" + atom + ")A" } ) {
			var reference = Reference( pattern, invalidUtf: true, anyCrLf: anyCrLf );
			var compiled = Compile( pattern, new PcreByteOptions { Utf8 = true, MatchInvalidUtf = true, AnyCrLfNewline = anyCrLf } );
			foreach ( var hex in new[] { "", "00", "61FF41", "FFC3A941", "F0908041", "EDA08041", "F48FBFBF41", "0D0A410D0A" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}, {anyCrLf}" );
				}
			}
		}
		}
		// Matching LF membership does not by itself give a property explicit-newline metadata.
		Check( @"\p{Any}A", "\r\nA", -1, 0, "", anyCrLf: true );
		Check( @"\p{Xps}A", "\r\nA", -1, 0, "", anyCrLf: true );
		Check( @"(?:\p{Any}|\n)A", "\r\nA", 1, 2, "1:2", anyCrLf: true );
	}

	[Theory]
	[InlineData( @"\p{Xuc}", "", "-" )]
	[InlineData( @"\p{Xuc}", "00", "-|-" )]
	[InlineData( @"\p{Xuc}", "61FF41", "-|-|-|-" )]
	[InlineData( @"\p{Xuc}", "FFC3A941", "1:2|1:2|-|-|-" )]
	[InlineData( @"\p{Xuc}", "F0908041", "-|-|-|-|-" )]
	[InlineData( @"\p{Xuc}", "EDA08041", "-|-|-|-|-" )]
	[InlineData( @"\p{Xuc}", "F48FBFBF41", "0:4|-|-|-|-|-" )]
	[InlineData( @"[\p{Xuc}]", "61FF41", "-|-|-|-" )]
	[InlineData( @"\p{Xuc}+", "FFC3A941", "1:2|1:2|-|-|-" )]
	[InlineData( @"\G(\p{Xuc})", "FFC3A941", "-|1:2,1:2|-|-|-" )]
	[InlineData( @"\p{Xuc}A", "FFC3A941", "1:3|1:3|-|-|-" )]
	[InlineData( @"\p{Xuc}A", "F48FBFBF41", "0:5|-|-|-|-|-" )]
	[InlineData( @"(?<=\p{Xuc})A", "FFC3A941", "3:1|3:1|3:1|3:1|-" )]
	[InlineData( @"(?<=\p{Xuc})A", "F48FBFBF41", "4:1|4:1|4:1|4:1|4:1|-" )]
	[InlineData( @"(?:\p{Xuc}|\n)A", "0D0A410D0A", "1:2|1:2|-|-|-|-" )]
	[InlineData( @"\p{Xuc}", "24FF40", "0:1|2:1|2:1|-" )]
	[InlineData( @"\p{Xuc}+", "24FF40", "0:1|2:1|2:1|-" )]
	[InlineData( @"\p{Xuc}A", "2441FF4041", "0:2|3:2|3:2|3:2|-|-" )]
	[InlineData( @"\G(\p{Xuc})", "24FF40", "0:1,0:1|-|2:1,2:1|-" )]
	public void PositiveXucKeepsValidFragmentBoundsInsteadOfNativeJitInvalidUtfSpans(
		string pattern, string hex, string results ) {
		var subject = Convert.FromHexString( hex );
		var expectedOffsets = results.Split( '|' );
		Assert.Equal( subject.Length + 1, expectedOffsets.Length );
		foreach ( var anyCrLf in new[] { false, true } ) {
			var compiled = Compile( pattern, new PcreByteOptions { Utf8 = true, MatchInvalidUtf = true, AnyCrLfNewline = anyCrLf } );
			for ( var offset = 0; offset <= subject.Length; offset++ ) {
				var actual = compiled.Match( subject, offset );
				Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
				if ( expectedOffsets[offset] == "-" ) { Assert.Null( actual.Match ); continue; }
				Assert.NotNull( actual.Match );
				var spans = expectedOffsets[offset].Split( ',' );
				Assert.Equal( spans.Length, actual.Match.Captures.Count );
				for ( var group = 0; group < spans.Length; group++ ) {
					var fields = spans[group].Split( ':' ).Select( int.Parse ).ToArray();
					Assert.Equal( new PcreByteCapture( fields[0], fields[1] ), actual.Match.Captures[group] );
				}
				Assert.InRange( actual.Match.ByteIndex + actual.Match.ByteLength, 0, subject.Length );
			}
		}
	}

	[Theory]
	[InlineData( @"é\p{Any", 2 )]
	[InlineData( @"é\P{^}", 2 )]
		[InlineData( @"é[\p{Any}-z]", 2 )]
	[InlineData( @"é[a-\p{Xuc}]", 2 )]
	public void MalformedPropertiesAndNonLiteralRangeEndpointsReportTheirOwner( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.InvalidSyntax, offset );
	}

	[Theory]
	[InlineData( @"é\p{^^Xwd}", 2 )]
	[InlineData( @"é\p{gc:Any}", 2 )]
	[InlineData( @"é\p{GeneralCategory=Any}", 2 )]
	[InlineData( @"é[\p{gc:Xwd}]", 2 )]
	[InlineData( @"é\p{Not_A_Property}", 2 )]
	public void SpecialNamesDoNotBecomeGeneralCategoriesAndUnknownNamesStayPending( string pattern, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
		Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, offset );
	}

	[Fact]
	public void BytePropertiesAndExistingScriptAndBinaryFamiliesKeepTheirLibraryGate() {
		foreach ( var name in Names ) {
			_ = Reference( $@"\p{{{name}}}", utf8: false );
			Diagnostic( $@"\p{{{name}}}", PcreByteDiagnosticCode.UnsupportedSyntax, 0, utf8: false );
		}
		foreach ( var pattern in new[] { @"\p{Greek}", @"\p{Alphabetic}" } ) {
			_ = Reference( pattern );
			Diagnostic( pattern, PcreByteDiagnosticCode.UnsupportedSyntax, 0 );
		}
	}

	[Fact]
	public void SpecialPropertiesShareWorkCancellationAndRecursiveFrameLimits() {
		const string pattern = @"\A(?:\p{Any}|\p{Xuc})+\z";
		var text = new string( 'é', 256 );
		Check( pattern, text, 0, 512, "0:512" );
		var subject = Encoding.UTF8.GetBytes( text );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, new PcreByteOptions { Utf8 = true, MatchWorkLimit = 40 } ).Match( subject ).Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( pattern, new PcreByteOptions { Utf8 = true,
			WorkObserver = work => { if ( work == 40 ) { cancelled.Cancel(); } }
		} );
		Assert.Throws<OperationCanceledException>( () => compiled.Match( subject, cancellationToken: cancelled.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.UTF8.GetBytes( pattern ), cancellationToken: cancelled.Token ) );
		const string recursion = @"\A(?&a)\z(?(DEFINE)(?<a>\p{Xan}(?&a)?\p{Xps}))";
		Check( recursion, new string( 'a', 32 ) + new string( ' ', 32 ), 0, 64, "0:64,-" );
		Assert.Equal( PcreByteDiagnosticCode.MatchDepthLimitExceeded, Compile( recursion )
			.Match( Encoding.ASCII.GetBytes( new string( 'a', 1200 ) + new string( ' ', 1200 ) ) ).Diagnostic?.Code );
	}


	[Fact]
	public void PropertyAtomsUseTheExistingCaptureAndInstructionBudgets() {
		var captures = string.Concat( Enumerable.Repeat( @"(\p{Any})", 256 ) );
		Check( captures, new string( 'a', 256 ), 0, 256,
			"0:256," + string.Join( ",", Enumerable.Range( 0, 256 ).Select( index => $"{index}:1" ) ) );
		_ = Reference( captures + @"(\p{Any})" );
		Diagnostic( captures + @"(\p{Any})", PcreByteDiagnosticCode.PatternCaptureLimitExceeded, captures.Length );
		var instructions = string.Concat( Enumerable.Repeat( @"[\p{Any}]", 4096 ) );
		_ = Reference( instructions );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( instructions ), new PcreByteOptions { Utf8 = true } ).Diagnostic?.Code );
	}

	private static void Check( string pattern, string text, int index, int length, string captures, bool ucp = false, bool anyCrLf = false ) {
		var subject = Encoding.UTF8.GetBytes( text );
		var reference = Reference( pattern, ucp: ucp, anyCrLf: anyCrLf );
		AssertNative( reference, subject, 0, index, length, captures );
		Compare( reference, subject, 0, Compile( pattern, new PcreByteOptions { Utf8 = true, Ucp = ucp, AnyCrLfNewline = anyCrLf } ).Match( subject ), pattern );
	}

	private static void AssertNative( PcreRegex8Bit reference, byte[] subject, int offset, int index, int length, string captures ) {
		var native = reference.Match( subject, offset );
		Assert.Equal( index, native.Success ? native.Index : -1 );
		if ( !native.Success ) { return; }
		Assert.Equal( length, native.Length );
		var spans = captures.Split( ',' );
		Assert.Equal( spans.Length, native.Groups.Count );
		for ( var group = 0; group < spans.Length; group++ ) {
			var capture = native.Groups[group];
			Assert.Equal( spans[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
		}
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( native.Success ? native.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, actual.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var capture = native.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static void Diagnostic( string pattern, PcreByteDiagnosticCode code, int offset, bool utf8 = true ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = utf8 } );
		Assert.Equal( code, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static PcreRegex8Bit Reference( string pattern, bool ucp = false, bool invalidUtf = false, bool anyCrLf = false, bool utf8 = true ) {
		var options = PcreOptions.Compiled | ( utf8 ? PcreOptions.Utf : PcreOptions.None )
			| ( ucp ? PcreOptions.Ucp : PcreOptions.None ) | ( invalidUtf ? PcreOptions.MatchInvalidUtf : PcreOptions.None );
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		return new PcreRegex8Bit( encoding.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), encoding,
			new PcreRegexSettings { Options = options, ParensLimit = 512 } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options ?? new PcreByteOptions { Utf8 = true } );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
