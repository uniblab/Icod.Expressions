using System.Text;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreQuotedLiteralTests {
	[Theory]
	[InlineData( @"\Qa.b|c(d)\E", "a.b|c(d)", 0, 8 )]
	[InlineData( @"\QA\B\E", @"A\B", 0, 3 )]
	[InlineData( @"\Qabc", "abc", 0, 3 )]
	[InlineData( @"\Eab", "ab", 0, 2 )]
	[InlineData( @"\Qab\E+", "abbb", 0, 4 )]
	[InlineData( @"\Qa+\E", "a+", 0, 2 )]
	[InlineData( @"\Q(a|b)\E", "(a|b)", 0, 5 )]
	[InlineData( @"[\Q]\E]", "]", 0, 1 )]
	[InlineData( @"[\Q-\E]", "-", 0, 1 )]
	[InlineData( @"[\Qab\E]", "b", 0, 1 )]
	[InlineData( @"[\Qa\E-z]", "b", 0, 1 )]
	[InlineData( @"[a-\Qz\E]", "b", 0, 1 )]
	[InlineData( @"[\Qa-z\E]", "-", 0, 1 )]
	[InlineData( @"(?<=\Qa.\E)b", "a.b", 2, 1 )]
	public void QuotedSyntaxMatchesLiteralBytes( string pattern, string subject, int start, int length ) {
		var compiled = PcreByteCompiler.Compile( Encoding.Latin1.GetBytes( pattern ) );
		Assert.True( compiled.IsSuccess, compiled.Diagnostic?.Message );
		var match = compiled.Pattern!.Match( Encoding.Latin1.GetBytes( subject ) );
		Assert.True( match.IsSuccess, match.Diagnostic?.Message );
		Assert.Equal( start, match.Match?.ByteIndex );
		Assert.Equal( length, match.Match?.ByteLength );
	}

	[Theory]
	[InlineData( @"\Qé.\E", "é.", 3 )]
	[InlineData( @"[\Qé]\E]", "é", 2 )]
	[InlineData( @"[\Qé\E-ë]", "ê", 2 )]
	[InlineData( @"[é-\Që\E]", "ê", 2 )]
	[InlineData( @"(?<=\Qé.\E)x", "é.x", 1 )]
	public void QuotedUtf8ScalarsPreserveSubjectByteSpans( string pattern, string subject, int length ) {
		var compiled = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new PcreByteOptions { Utf8 = true } );
		Assert.True( compiled.IsSuccess, compiled.Diagnostic?.Message );
		var match = compiled.Pattern!.Match( Encoding.UTF8.GetBytes( subject ) );
		Assert.True( match.IsSuccess, match.Diagnostic?.Message );
		Assert.Equal( length, match.Match?.ByteLength );
	}

	[Fact]
	public void UnterminatedQuoteInsideBracketLeavesBracketUnclosed() {
		var compiled = PcreByteCompiler.Compile( @"[\Q]"u8.ToArray() );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, compiled.Diagnostic?.Code );
		Assert.Equal( 0, compiled.Diagnostic?.ByteOffset );
	}
}
