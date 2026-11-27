using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordUcpClassTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				( @"\d", "x٥", 0, false, true ), ( @"\d", "x٥", 0, true, true ),
				( @"\d", "x٥", 0, false, false ), ( @"\d", "x٥", 0, true, false ),
				( @"\D", "٥A", 0, false, true ), ( @"\D", "٥A", 0, true, true ),
				( @"\w", "\u0300", 0, false, true ), ( @"\w", "\u0300", 0, true, true ),
				( @"\W", "\u0300!", 0, false, true ), ( @"\W", "\u0300!", 0, true, true ),
				( @"\s", "\u0085", 0, false, true ), ( @"\s", "\u0085", 0, true, true ),
				( @"\S", "\u00A0\u180E", 0, false, true ), ( @"\S", "\u00A0\u180E", 0, true, true ),
				( @"[\d\w]", "\u0300", 0, false, true ), ( @"[\d\w]", "\u0300", 0, true, true ),
				( @"[^\W]", "Ω", 0, false, true ), ( @"[^\W]", "Ω", 0, true, true ),
				( @"[\S[:digit:]]", "٥", 0, false, true ), ( @"[\S[:digit:]]", "٥", 0, true, true ),
				( "[[:alnum:]]", "Ⅻ", 0, false, true ), ( "[[:alnum:]]", "Ⅻ", 0, true, true ),
				( "[[:alpha:]]", "Ω", 0, false, true ), ( "[[:alpha:]]", "Ω", 0, true, true ),
				( "[[:blank:]]", "\u00A0", 0, false, true ), ( "[[:blank:]]", "\u00A0", 0, true, true ),
				( "[[:cntrl:]]", "\u0085", 0, false, true ), ( "[[:cntrl:]]", "\u0085", 0, true, true ),
				( "[[:digit:]]", "٥", 0, false, true ), ( "[[:digit:]]", "٥", 0, true, true ),
				( "[[:graph:]]", "\u200EΩ", 0, false, true ), ( "[[:graph:]]", "\u200EΩ", 0, true, true ),
				( "[[:lower:]]", "é", 0, false, true ), ( "[[:lower:]]", "é", 0, true, true ),
				( "[[:print:]]", "\u00A0", 0, false, true ), ( "[[:print:]]", "\u00A0", 0, true, true ),
				( "[[:punct:]]", "+", 0, false, true ), ( "[[:punct:]]", "+", 0, true, true ),
				( "[[:space:]]", "\u0085", 0, false, true ), ( "[[:space:]]", "\u0085", 0, true, true ),
				( "[[:upper:]]", "Ω", 0, false, true ), ( "[[:upper:]]", "Ω", 0, true, true ),
				( "[[:word:]]", "\u0300", 0, false, true ), ( "[[:word:]]", "\u0300", 0, true, true ),
				( "[[:xdigit:]]", "Ｆ", 0, false, true ), ( "[[:xdigit:]]", "Ｆ", 0, true, true ),
				( "[[:^digit:]]", "٥A", 0, false, true ), ( "[[:^digit:]]", "٥A", 0, true, true ),
				( @"\p{XWD}", "\u0300", 0, false, true ), ( @"\p{XWD}", "\u0300", 0, true, true ),
				( @"\p{XAN}", "Ⅻ", 0, false, true ), ( @"\p{XAN}", "Ⅻ", 0, true, true ),
				( @"\p{XPS}", "\u0085", 0, false, true ), ( @"\p{XPS}", "\u0085", 0, true, true ),
				( @"\p{XSP}", "\u180E", 0, false, true ), ( @"\p{XSP}", "\u180E", 0, true, true ),
				( @"(?aD)\d", "٥3", 0, false, true ), ( @"(?aD)\d", "٥3", 0, true, true ),
				( @"(?aW)\w", "Ω_", 0, false, true ), ( @"(?aW)\w", "Ω_", 0, true, true ),
				( @"(?aS)\s", "\u00A0\t", 0, false, true ), ( @"(?aS)\s", "\u00A0\t", 0, true, true ),
				( "(?aP)[[:alpha:]]", "ΩA", 0, false, true ), ( "(?aP)[[:alpha:]]", "ΩA", 0, true, true ),
				( "(?aT)[[:digit:]]", "٥5", 0, false, true ), ( "(?aT)[[:digit:]]", "٥5", 0, true, true ),
				( @"(?a)\w", "Ω_", 0, false, true ), ( @"(?a)\w", "Ω_", 0, true, true ),
				( @"(?aD)(?-aD)\d", "٥", 0, false, true ), ( @"(?aD)(?-aD)\d", "٥", 0, true, true ),
				( "(?aP)(?-aP)[[:alpha:]]", "Ω", 0, false, true ), ( "(?aP)(?-aP)[[:alpha:]]", "Ω", 0, true, true ),
				( @"\W", "😀", 0, false, true ), ( @"\W", "😀", 0, true, true ),
				( @"\W", "😀", 1, false, true )
			};
			for ( var index = 0; index < cases.Length; index++ ) {
				var (source, subject, start, utf, ucp) = cases[index];
				foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 63 } ) {
					yield return new object[] { index, source, subject, start, utf, ucp, defaults };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void UnicodeClassesAgreeWithNativePolicies( int id, string source, string subject,
		int start, bool utf, bool ucp, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"class {id}: {compiled.Diagnostic}" );
		var nativePolicies = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start, defaults, ucp );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = nativePolicies[mask];
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var match = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( match.IsSuccess, $"managed {id}/{defaults}/{mask}: {match.Diagnostic}" );
			Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( @"\d", "٥" )]
	[InlineData( @"\w", "\u0300" )]
	[InlineData( "[[:alpha:]]", "Ω" )]
	[InlineData( "[[:xdigit:]]", "Ｆ" )]
	[InlineData( @"\W", "😀" )]
	public void ValidUnicodeUcpClassesAgreeAcrossManagedWidths( string source, string subject ) {
		var subjectBytes = Encoding.UTF8.GetBytes( subject );
		var bytes = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true, Ucp = true } );
		var words = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true, Ucp = true } );
		Assert.True( bytes.IsSuccess ); Assert.True( words.IsSuccess );
		var byteMatch = bytes.Pattern!.Match( subjectBytes );
		var wordMatch = words.Pattern!.Match( subject.AsMemory() );
		Assert.True( byteMatch.IsSuccess ); Assert.True( wordMatch.IsSuccess );
		var coordinates = new PcreDualWidthCoordinates( subjectBytes, subject );
		Assert.Equal( coordinates.ByteCaptures( byteMatch.Match is { } b ? $"{b.ByteIndex}:{b.ByteLength}" : "" ),
			coordinates.WordCaptures( wordMatch.Match is { } w ? $"{w.CodeUnitIndex}:{w.CodeUnitLength}" : "" ) );
	}

	[Theory]
	[InlineData( @"\d", "٥", nameof( PcreWordOptions.AsciiBsD ), @"(?aD)\d" )]
	[InlineData( @"\s", "\u00A0", nameof( PcreWordOptions.AsciiBsS ), @"(?aS)\s" )]
	[InlineData( @"\w", "Ω", nameof( PcreWordOptions.AsciiBsW ), @"(?aW)\w" )]
	[InlineData( "[[:alpha:]]", "Ω", nameof( PcreWordOptions.AsciiPosix ), "(?aP)[[:alpha:]]" )]
	[InlineData( "[[:digit:]]", "٥", nameof( PcreWordOptions.AsciiDigit ), "(?aT)[[:digit:]]" )]
	public void AsciiOptionAgreesWithNativeInlineRestriction( string source, string subject, string option, string nativeSource ) {
		var choices = new PcreWordOptions { Utf16 = true, Ucp = true };
		choices = option switch {
			nameof( PcreWordOptions.AsciiBsD ) => choices with { AsciiBsD = true },
			nameof( PcreWordOptions.AsciiBsS ) => choices with { AsciiBsS = true },
			nameof( PcreWordOptions.AsciiBsW ) => choices with { AsciiBsW = true },
			nameof( PcreWordOptions.AsciiPosix ) => choices with { AsciiPosix = true },
			nameof( PcreWordOptions.AsciiDigit ) => choices with { AsciiDigit = true },
			_ => throw new ArgumentOutOfRangeException( nameof( option ) )
		};
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), choices );
		Assert.True( compiled.IsSuccess );
		var native = PcreNative16Oracle.ExecuteSequence( Words( nativeSource ), Words( subject ), true, 0, ucp: true );
		Assert.True( native.Kind is "M" or "N" );
		var match = compiled.Pattern!.Match( subject.AsMemory() );
		Assert.True( match.IsSuccess );
		Assert.Equal( native.Captures, match.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
	}

	[Theory]
	[InlineData( false, 3 )]
	[InlineData( true, 4 )]
	public void UnicodePredicateConsumesOneBoundedStep( bool utf, int required ) {
		foreach ( var budget in new[] { required - 1, required } ) {
			var trace = new List<int>();
			var pattern = PcreWordSequenceCompiler.CompileSequence( @"[\d]".AsMemory(), new() {
				Utf16 = utf, Ucp = true, MatchWorkLimit = budget, WorkObserver = trace.Add
			} ).Pattern!;
			var match = pattern.Match( "٥".AsMemory() );
			if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, match.Diagnostic?.Code ); }
			else { Assert.Equal( new PcreWordCapture( 0, 1 ), match.Match?.Captures[0] ); }
			Assert.Equal( Enumerable.Range( 1, budget ), trace );
		}
	}

	[Fact]
	public void UnicodeBracketBoundAndUnsupportedSpecialRemainExplicit() {
		var allowed = "[" + new string( 'a', 63 ) + @"\p{XWD}]";
		var rejected = "[" + new string( 'a', 64 ) + @"\p{XWD}]";
		Assert.Equal( new PcreWordCapture( 0, 1 ),
			PcreWordSequenceCompiler.CompileSequence( allowed.AsMemory(), new() { Ucp = true } ).Pattern!.Match( "Ω".AsMemory() ).Match?.Captures[0] );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( rejected.AsMemory(), new() { Ucp = true } ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( @"\p{XUC}".AsMemory(), new() { Utf16 = true, Ucp = true } ).Diagnostic?.Code );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
