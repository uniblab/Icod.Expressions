using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Unicode17 categories pinned independently to PCRE2 10.48.</summary>
public sealed class PcreUnicodeCategoryVersionReferenceTests {
	private static readonly string[] Names = {
		"Lu", "Ll", "Lt", "Lm", "Lo", "Mn", "Mc", "Me", "Nd", "Nl", "No", "Zs", "Zl", "Zp",
		"Cc", "Cf", "Cs", "Co", "Pc", "Pd", "Ps", "Pe", "Pi", "Pf", "Po", "Sm", "Sc", "Sk", "So", "Cn"
	};
	private static readonly (PcreByteScalarClassKind Kind, UnicodeCategory[] Categories)[] Families = {
		(PcreByteScalarClassKind.CategoryOther, new[] { UnicodeCategory.OtherNotAssigned, UnicodeCategory.PrivateUse,
			UnicodeCategory.Control, UnicodeCategory.Format, UnicodeCategory.Surrogate }),
		(PcreByteScalarClassKind.Letter, new[] { UnicodeCategory.OtherLetter, UnicodeCategory.LowercaseLetter,
			UnicodeCategory.UppercaseLetter, UnicodeCategory.ModifierLetter, UnicodeCategory.TitlecaseLetter }),
		(PcreByteScalarClassKind.CategoryMark, new[] { UnicodeCategory.NonSpacingMark, UnicodeCategory.SpacingCombiningMark,
			UnicodeCategory.EnclosingMark }),
		(PcreByteScalarClassKind.CategoryNumber, new[] { UnicodeCategory.DecimalDigitNumber, UnicodeCategory.OtherNumber,
			UnicodeCategory.LetterNumber }),
		(PcreByteScalarClassKind.CategoryPunctuation, new[] { UnicodeCategory.OtherPunctuation, UnicodeCategory.ConnectorPunctuation,
			UnicodeCategory.DashPunctuation, UnicodeCategory.OpenPunctuation, UnicodeCategory.ClosePunctuation,
			UnicodeCategory.InitialQuotePunctuation, UnicodeCategory.FinalQuotePunctuation }),
		(PcreByteScalarClassKind.CategorySymbol, new[] { UnicodeCategory.OtherSymbol, UnicodeCategory.MathSymbol,
			UnicodeCategory.CurrencySymbol, UnicodeCategory.ModifierSymbol }),
		(PcreByteScalarClassKind.CategorySeparator, new[] { UnicodeCategory.SpaceSeparator, UnicodeCategory.LineSeparator,
			UnicodeCategory.ParagraphSeparator })
	};

	[Fact]
	public void AllValidScalarsHaveThePinnedNativeCategoryFingerprint() {
		// Independent C probe: PCRE2 10.48 UCD_CHARTYPE, mapped to .NET enum bytes.
		// Includes unassigned/noncharacters/private use; skips only surrogate code points.
		var categories = new byte[0x110000 - 0x800];
		var index = 0;
		for ( var scalar = 0; scalar <= 0x10FFFF; scalar++ ) {
			if ( scalar is >= 0xD800 and <= 0xDFFF ) { continue; }
			categories[index++] = (byte)Classify( scalar );
		}
		Assert.Equal( categories.Length, index );
		Assert.Equal( "2DBBCF96F6B264878E1862474E8A7C214EB6D1ECA5E3EBA4B0B99E30CA28EC75",
			Convert.ToHexString( SHA256.HashData( categories ) ) );
	}

	private static UnicodeCategory Classify( int scalar ) {
		foreach ( var family in Families ) {
			if ( !PcreScalarClassMembership.Contains( family.Kind, scalar ) ) { continue; }
			foreach ( var category in family.Categories ) {
				if ( PcreScalarClassMembership.Contains( PcreByteScalarClassKind.SpecificCategory, scalar, category ) ) {
					return category;
				}
			}
			throw new InvalidOperationException( $"No specific category for U+{scalar:X}." );
		}
		throw new InvalidOperationException( $"No general category for U+{scalar:X}." );
	}

	[Theory]
	[InlineData( 0x088F, "Lo" )]
	[InlineData( 0x1ACF, "Mn" )]
	[InlineData( 0x1C89, "Lu" )]
	[InlineData( 0x1C8A, "Ll" )]
	[InlineData( 0xA7CB, "Lu" )]
	[InlineData( 0x105C0, "Lo" )]
	[InlineData( 0x10D40, "Nd" )]
	[InlineData( 0x11380, "Lo" )]
	[InlineData( 0x11B60, "Mn" )]
	[InlineData( 0x16D40, "Lm" )]
	[InlineData( 0x1CC00, "So" )]
	[InlineData( 0x1E5D0, "Lo" )]
	[InlineData( 0x1E6C0, "Lo" )]
	[InlineData( 0x2EBF0, "Lo" )]
	[InlineData( 0x31350, "Lo" )]
	[InlineData( 0x33479, "Lo" )]
	[InlineData( 0x3347A, "Cn" )]
	[InlineData( 0x10FFFF, "Cn" )]
	public void RecentAssignmentsAndTheirComplementsUsePinnedCategories( int scalar, string category ) {
		var subject = Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) );
		foreach ( var ucp in new[] { false, true } ) {
		foreach ( var caseless in new[] { false, true } ) {
		foreach ( var form in new[] { $@"\p{{{category}}}", $@"[\p{{{category}}}]", $@"[^\P{{{category}}}]" } ) {
			Check( $@"\A({form})\z", subject, true, ucp, caseless );
		}
		foreach ( var form in new[] { $@"\P{{{category}}}", $@"[^\p{{{category}}}]", $@"[\P{{{category}}}]" } ) {
			Check( $@"\A({form})\z", subject, false, ucp, caseless );
		}
		}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void BoundedLiveJitSamplingCoversEveryCategoryAcrossTheScalarDomain( bool ucp ) {
		var pattern = @"\A(?:" + string.Join( "|", Names.Select( name => $@"(\p{{{name}}})" ) ) + @")\z";
		var reference = Reference( pattern, ucp );
		var compiled = Compile( pattern, ucp );
		foreach ( var scalar in Enumerable.Range( 0, 128 ).Concat( Enumerable.Range( 0, 1088 )
			.Select( n => n * 1024 + 1023 ) ).Where( n => n < 0xD800 || n > 0xDFFF ) ) {
			Compare( reference, compiled, Encoding.UTF8.GetBytes( char.ConvertFromUtf32( scalar ) ) );
		}
	}

	[Theory]
	[InlineData( 0x088F )]
	[InlineData( 0x1C89 )]
	[InlineData( 0x105C0 )]
	[InlineData( 0x16D40 )]
	[InlineData( 0x1E6C0 )]
	[InlineData( 0x2EBF0 )]
	public void UtfCaptureNamesAcceptNewLettersAndDecimalSuffixes( int scalar ) {
		var name = char.ConvertFromUtf32( scalar ) + char.ConvertFromUtf32( 0x10D40 );
		foreach ( var declaration in new[] { $"(?<{name}>a)", $"(?'{name}'a)", $"(?P<{name}>a)" } ) {
			foreach ( var tail in new[] { $@"\k<{name}>", $@"\g{{{name}}}", $"(?&{name})" } ) {
				Check( @"\A" + declaration + tail + @"\z", "aa"u8.ToArray(), true );
			}
		}
	}

	[Theory]
	[InlineData( 0x10D40 )]
	[InlineData( 0x1ACF )]
	[InlineData( 0x11B60 )]
	[InlineData( 0x1CC00 )]
	[InlineData( 0x3347A )]
	public void NonLettersStillCannotStartNamesAndSubroutineNumbersStayAscii( int scalar ) {
		foreach ( var pattern in new[] { $"(?<{char.ConvertFromUtf32( scalar )}>a)", $"(?{char.ConvertFromUtf32( 0x10D40 )})" } ) {
			Assert.Throws<PcrePatternException>( () => Reference( pattern ) );
			var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
			Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
			Assert.Equal( 0, result.Diagnostic?.ByteOffset );
		}
	}

	private static void Check( string pattern, byte[] subject, bool expected, bool ucp = false, bool caseless = false ) {
		var reference = Reference( pattern, ucp, caseless );
		Assert.Equal( expected, reference.Match( subject ).Success );
		Compare( reference, Compile( pattern, ucp, caseless ), subject );
	}

	private static void Compare( PcreRegex8Bit reference, PcreCompiledBytePattern compiled, byte[] subject ) {
		var native = reference.Match( subject );
		var actual = compiled.Match( subject );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		Assert.Equal( native.Success, actual.Match is not null );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Index, actual.Match!.ByteIndex );
		Assert.Equal( native.Length, actual.Match.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var i = 0; i < native.Groups.Count; i++ ) {
			var capture = native.Groups[i];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[i] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool ucp = false, bool caseless = false ) =>
		new( Encoding.UTF8.GetBytes( pattern ), Encoding.UTF8, new PcreRegexSettings {
			Options = PcreOptions.Compiled | PcreOptions.Utf | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( caseless ? PcreOptions.Caseless : PcreOptions.None )
		} );

	private static PcreCompiledBytePattern Compile( string pattern, bool ucp = false, bool caseless = false ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ),
			new PcreByteOptions { Utf8 = true, Ucp = ucp, Caseless = caseless } );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
