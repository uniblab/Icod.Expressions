using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Default decimal/octal disambiguation against pinned PCRE2 10.48.</summary>
public sealed class PcreNumericEscapeReferenceTests {
	[Theory]
	[InlineData( @"\0", "00", false, 0, 1 )]
	[InlineData( @"\00", "00", false, 0, 1 )]
	[InlineData( @"\0000", "0030", false, 0, 2 )]
	[InlineData( @"\08", "0038", false, 0, 2 )]
	[InlineData( @"\10", "08", false, 0, 1 )]
	[InlineData( @"(a)\10", "6108", false, 0, 2 )]
	[InlineData( @"(a)\11", "6109", false, 0, 2 )]
	[InlineData( @"\18", "0138", false, 0, 2 )]
	[InlineData( @"\118", "0938", false, 0, 2 )]
	[InlineData( @"\1234", "5334", false, 0, 2 )]
	[InlineData( @"\400", "C480", true, 0, 2 )]
	[InlineData( @"\777", "C7BF", true, 0, 2 )]
	[InlineData( @"\o{200}", "80", false, 0, 1 )]
	[InlineData( @"\o{200}", "C280", true, 0, 2 )]
	[InlineData( @"\o{377}", "FF", false, 0, 1 )]
	[InlineData( @"\o{377}", "C3BF", true, 0, 2 )]
	[InlineData( @"\o{373000}", "F09F9880", true, 0, 4 )]
	[InlineData( @"\o{4177777}", "F48FBFBF", true, 0, 4 )]
	[InlineData( @"\o{177777}", "EFBFBF", true, 0, 3 )]
	[InlineData( "\\o{ \t000101\t }", "41", false, 0, 1 )]
	[InlineData( @"[\101-\103]", "42", false, 0, 1 )]
	[InlineData( @"[^\101-\103]", "44", false, 0, 1 )]
	[InlineData( @"[\8\9]", "39", false, 0, 1 )]
	[InlineData( @"[\8\9]", "38", true, 0, 1 )]
	[InlineData( @"[\o{373000}-\o{373001}]", "F09F9881", true, 0, 4 )]
	[InlineData( @"(?<=\o{373000})x", "F09F988078", true, 4, 1 )]
	[InlineData( @"(?=\10)\10", "08", false, 0, 1 )]
	[InlineData( @"\o{101}{2}", "4141", false, 0, 2 )]
	[InlineData( @"(?i)\o{101}", "61", false, 0, 1 )]
	[InlineData( @"(?i)\o{113}", "E284AA", true, 0, 3 )]
	[InlineData( "(a)\\g{ \t1\t }", "6161", false, 0, 2 )]
	[InlineData( @"(a)(b)\g{ -1 }", "616262", false, 0, 3 )]
	[InlineData( @"\g{ +1 }(a)", "61", false, -1, 0 )]
	public void LiteralContextsAndBracedReferencesKeepPinnedSpans( string pattern, string hex,
		bool utf8, int expectedIndex, int expectedLength ) {
		var bytes = Convert.FromHexString( hex );
		var reference = Reference( pattern, utf8 );
		var native = reference.Match( bytes );
		Assert.Equal( expectedIndex, native.Success ? native.Index : -1 );
		if ( native.Success ) { Assert.Equal( expectedLength, native.Length ); }
		Compare( reference, bytes, 0, Compile( pattern, new PcreByteOptions { Utf8 = utf8 } ).Match( bytes ) );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void EveryByteValueHasBothOctalSpellingsInAtomsAndBrackets( bool braced, bool bracket ) {
		for ( var value = 0; value <= 255; value++ ) {
			var octal = Convert.ToString( value, 8 );
			var atom = braced ? @"\o{" + octal + "}" : @"\" + octal.PadLeft( 3, '0' );
			var pattern = bracket ? "[" + atom + "]" : atom;
			var bytes = new[] { (byte)value };
			var reference = Reference( pattern, false );
			Assert.Equal( 1, reference.Match( bytes ).Length );
			Compare( reference, bytes, 0, Compile( pattern ).Match( bytes ) );
		}
	}

	[Theory]
	[InlineData( 10 )]
	[InlineData( 80 )]
	[InlineData( 90 )]
	[InlineData( 256 )]
	public void PreviouslyAllocatedSlotsSelectTheEntireDecimalReference( int number ) {
		var pattern = string.Concat( Enumerable.Repeat( "()", number - 1 ) ) + "(a)\\" + number;
		var reference = Reference( pattern, false );
		var bytes = "aa"u8.ToArray();
		Assert.Equal( 2, reference.Match( bytes ).Length );
		Compare( reference, bytes, 0, Compile( pattern ).Match( bytes ) );
	}

	[Theory]
	[InlineData( 8 )]
	[InlineData( 9 )]
	[InlineData( 80 )]
	[InlineData( 90 )]
	public void LeadingEightOrNineAllowsForwardReferencesUsedInLaterIterations( int number ) {
		var groups = string.Concat( Enumerable.Repeat( "()", number - 1 ) ) + "(a)";
		var pattern = "^(?:\\" + number + "|" + groups + ")+$";
		var bytes = "aa"u8.ToArray();
		var reference = Reference( pattern, false );
		Assert.Equal( 2, reference.Match( bytes ).Length );
		Compare( reference, bytes, 0, Compile( pattern ).Match( bytes ) );
	}

	[Fact]
	public void FullDigitRunsAndBranchResetCountersControlDisambiguation() {
		var tenGroups = string.Concat( Enumerable.Repeat( "()", 9 ) ) + "(a)";
		var cases = new[] {
			(@"\10" + tenGroups, "0861"),
			(tenGroups + @"\101", "6141"),
			("(?|A" + string.Concat( Enumerable.Repeat( "()", 10 ) ) + @"\10|B\10)", "4208"),
			(@"(?|(a)\1|(b)\1)", "6262"),
			(@"\257", "AF")
		};
		foreach ( var (pattern, hex) in cases ) {
			var bytes = Convert.FromHexString( hex );
			var reference = Reference( pattern, false );
			Assert.Equal( bytes.Length, reference.Match( bytes ).Length );
			Compare( reference, bytes, 0, Compile( pattern ).Match( bytes ) );
		}
	}

	[Fact]
	public void SaturatingDecimalScanAndLeadingOctalZerosRemainBounded() {
		var pattern = @"\" + new string( '1', 512 );
		var bytes = Encoding.ASCII.GetBytes( "I" + new string( '1', 509 ) );
		Compare( Reference( pattern, false ), bytes, 0, Compile( pattern ).Match( bytes ) );
		var zeros = @"\o{" + new string( '0', 10000 ) + "101}";
		Assert.Equal( 1, Compile( zeros ).Match( "A"u8.ToArray() ).Match?.ByteLength );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile(
			Encoding.ASCII.GetBytes( zeros ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void NumericLiteralsAndReferencesKeepInvalidUtfOffsetsAndBarriers() {
		foreach ( var pattern in new[] { @"\o{351}+", @"\0", @"(\0?)\1", @"[\000-\377]" } ) {
			var reference = Reference( pattern, true, true );
			var managed = Compile( pattern, new PcreByteOptions { MatchInvalidUtf = true } );
			foreach ( var hex in new[] { "", "0000", "00FF00", "C3A9C3A9", "FFC3A9FF", "C3FFC3" } ) {
				var bytes = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= bytes.Length; offset++ ) {
					Compare( reference, bytes, offset, managed.Match( bytes, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"\o", false, 0 )]
	[InlineData( @"\o101", false, 0 )]
	[InlineData( @"\o{}", false, 0 )]
	[InlineData( @"\o{ }", false, 0 )]
	[InlineData( @"\o{8}", false, 0 )]
	[InlineData( @"\o{1 2}", false, 0 )]
	[InlineData( @"\o{+1}", false, 0 )]
	[InlineData( @"\o{-1}", false, 0 )]
	[InlineData( @"\o{101", false, 0 )]
	[InlineData( @"\400", false, 0 )]
	[InlineData( @"\777", false, 0 )]
	[InlineData( @"[\400]", false, 1 )]
	[InlineData( @"[\o{400}]", false, 1 )]
	[InlineData( @"\o{154000}", true, 0 )]
	[InlineData( @"\o{157777}", true, 0 )]
	[InlineData( @"\o{4200000}", true, 0 )]
	[InlineData( @"\o{7777777777777777777777}", true, 0 )]
	[InlineData( @"\80", false, 0 )]
	[InlineData( @"\9999999999999999999999", false, 0 )]
	[InlineData( @"(a)\g{1 2}", false, 3 )]
	public void InvalidNumericFormsHaveControlledEscapeByteDiagnostics( string pattern, bool utf8, int offset ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, utf8 ) );
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), new PcreByteOptions { Utf8 = utf8 } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	[Fact]
	public void QuotingKeepsNumericSyntaxLiteralAndMatchingWorkIsUnchanged() {
		const string text = @"\10\o{101}";
		var bytes = Encoding.ASCII.GetBytes( text );
		Compare( Reference( @"\Q" + text + @"\E", false ), bytes, 0,
			Compile( @"\Q" + text + @"\E" ).Match( bytes ) );
		var literalSteps = new List<int>();
		var octalSteps = new List<int>();
		Compile( "a", new PcreByteOptions { WorkObserver = literalSteps.Add } ).Match( "a"u8.ToArray() );
		Compile( @"\o{141}", new PcreByteOptions { WorkObserver = octalSteps.Add } ).Match( "a"u8.ToArray() );
		Assert.Equal( literalSteps, octalSteps );
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset,
		PcreByteMatchResult actual, string context = "" ) {
		var native = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( native.Success ? native.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), context );
		if ( !native.Success ) { return; }
		Assert.Equal( native.Length, actual.Match!.ByteLength );
		Assert.Equal( native.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < native.Groups.Count; group++ ) {
			var captured = native.Groups[group];
			Assert.Equal( captured.Success ? new PcreByteCapture( captured.Index, captured.Length ) : null,
				actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8, bool invalidUtf = false ) {
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var options = PcreOptions.Compiled;
		if ( utf8 ) { options |= PcreOptions.Utf; }
		if ( invalidUtf ) { options |= PcreOptions.MatchInvalidUtf; }
		return new PcreRegex8Bit( encoding.GetBytes( pattern ), encoding, new PcreRegexSettings { Options = options } );
	}

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var encoding = options?.Utf8 == true || options?.MatchInvalidUtf == true ? Encoding.UTF8 : Encoding.Latin1;
		var result = PcreByteCompiler.Compile( encoding.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
