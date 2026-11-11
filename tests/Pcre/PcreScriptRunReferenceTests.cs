using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Script-run rules from native PCRE2 10.48 JIT, including directional Han transitions.</summary>
public sealed class PcreScriptRunReferenceTests {
	private const string WholeRun = @"\A(*sr:([\s\S]*))\z";
	private static readonly Dictionary<int, byte[]> Encoded = PcreScriptRunFixtureData.Boundaries
		.Concat( PcreScriptRunFixtureData.Representatives ).Concat( PcreScriptRunFixtureData.Contexts )
		.Concat( PcreScriptRunFixtureData.DigitZeros ).Concat( PcreScriptRunFixtureData.DigitZeros.Select( x => x + 9 ) )
		.Distinct().ToDictionary( x => x, x => Encoding.UTF8.GetBytes( char.ConvertFromUtf32( x ) ) );
	private static byte[] Bytes( params int[] scalars ) {
		var result = new byte[scalars.Sum( scalar => Encoded[scalar].Length )];
		var offset = 0;
		foreach ( var scalar in scalars ) { var bytes = Encoded[scalar]; bytes.CopyTo( result, offset ); offset += bytes.Length; }
		return result;
	}
	private static byte[] Mask( string[] chunks ) => Convert.FromHexString( String.Concat( chunks ) );
	private static void Check( PcreRegex8Bit native, PcreCompiledBytePattern managed, byte[] subject, byte[] mask, int row ) {
		var expected = ( mask[row >> 3] & ( 1 << ( row & 7 ) ) ) != 0;
		Assert.True( native.Match( subject ).Success == expected, Convert.ToHexString( subject ) );
		Compare( native, managed, subject );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryScriptExtensionDescriptorPairKeepsBothOrders( bool ucp ) {
		var native = Reference( WholeRun, true, ucp );
		var managed = Compile( WholeRun, new() { Utf8 = true, Ucp = ucp } );
		var mask = Mask( PcreScriptRunFixtureData.PairsMask );
		var row = 0;
		foreach ( var first in PcreScriptRunFixtureData.Representatives ) {
			foreach ( var second in PcreScriptRunFixtureData.Representatives ) { Check( native, managed, Bytes( first, second ), mask, row++ ); }
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryScriptBoundaryAndDigitBoundaryKeepsContextAndDirection( bool ucp ) {
		var native = Reference( WholeRun, true, ucp );
		var managed = Compile( WholeRun, new() { Utf8 = true, Ucp = ucp } );
		var mask = Mask( PcreScriptRunFixtureData.BoundariesMask );
		var row = 0;
		foreach ( var scalar in PcreScriptRunFixtureData.Boundaries ) {
			foreach ( var context in PcreScriptRunFixtureData.Contexts ) {
				Check( native, managed, Bytes( scalar, context ), mask, row++ );
				Check( native, managed, Bytes( context, scalar ), mask, row++ );
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void AllSeventySevenDecimalSetsKeepZeroNineAndAdjacentSetIdentity( bool ucp ) {
		var native = Reference( WholeRun, true, ucp );
		var managed = Compile( WholeRun, new() { Utf8 = true, Ucp = ucp } );
		var mask = Mask( PcreScriptRunFixtureData.DigitsMask );
		var row = 0;
		foreach ( var first in PcreScriptRunFixtureData.DigitZeros ) {
			foreach ( var second in PcreScriptRunFixtureData.DigitZeros ) {
				for ( var variant = 0; variant < 4; variant++ ) {
					Check( native, managed, Bytes( first + ( variant & 1 ) * 9, second + ( ( variant >> 1 ) & 1 ) * 9 ), mask, row++ );
				}
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ContextTriplesKeepMapIntersectionAndNativeHanStateTransitions( bool ucp ) {
		var native = Reference( WholeRun, true, ucp );
		var managed = Compile( WholeRun, new() { Utf8 = true, Ucp = ucp } );
		var mask = Mask( PcreScriptRunFixtureData.TriplesMask );
		var row = 0;
		foreach ( var first in PcreScriptRunFixtureData.Contexts ) {
			foreach ( var second in PcreScriptRunFixtureData.Contexts ) {
				foreach ( var third in PcreScriptRunFixtureData.Contexts ) { Check( native, managed, Bytes( first, second, third ), mask, row++ ); }
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryBytePairUsesByteValuedUnicodeWithoutDecoding( bool ucp ) {
		var native = Reference( WholeRun, false, ucp );
		var managed = Compile( WholeRun, new() { Ucp = ucp } );
		var mask = Mask( PcreScriptRunFixtureData.BytesMask );
		for ( var first = 0; first < 256; first++ ) {
			for ( var second = 0; second < 256; second++ ) { Check( native, managed, [(byte)first, (byte)second], mask, first * 256 + second ); }
		}
	}

	[Theory]
	[InlineData( "sr" )]
	[InlineData( "script_run" )]
	[InlineData( "asr" )]
	[InlineData( "atomic_script_run" )]
	public void EmptyAndSingleScalarsIncludingUnknownAlwaysFormRuns( string alias ) {
		var pattern = @"\A(*" + alias + @":([\s\S]*))\z";
		var native = Reference( pattern, true );
		var managed = Compile( pattern, new() { Utf8 = true } );
		Compare( native, managed, Array.Empty<byte>() );
		foreach ( var scalar in PcreScriptRunFixtureData.Boundaries ) {
			var subject = Bytes( scalar );
			Assert.True( native.Match( subject ).Success );
			Compare( native, managed, subject );
		}
	}

	[Theory]
	[InlineData( @"\A(*sr:.*)a\z", "aa", true )]
	[InlineData( @"\A(*asr:.*)a\z", "aa", false )]
	[InlineData( @"\A(*sr:.*)β\z", "aβ", true )]
	[InlineData( @"\A(*asr:.*)β\z", "aβ", false )]
	[InlineData( @"\A(?>(*sr:.*))β\z", "aβ", true )]
	[InlineData( @"\A(*script_run:aβ|a)β\z", "aβ", true )]
	[InlineData( @"\A(*atomic_script_run:aβ|a)β\z", "aβ", false )]
	[InlineData( @"\A(*sr:(aβ)|(a))β\z", "aβ", true )]
	[InlineData( @"\A(?:(*sr:(aβ))|(aβ))\z", "aβ", true )]
	[InlineData( @"\A(?<r>(*sr:a(?&r)?|β))\z", "aβ", false )]
	[InlineData( @"\A(?<r>(*sr:a(?&r)?|β))\z", "aa", true )]
	[InlineData( @"\A(?(DEFINE)(?<r>(*sr:.*)))(?&r)\z", "aβ", false )]
	[InlineData( @"\A(*sr:(?<x>a))(?1)\z", "aa", true )]
	[InlineData( @"\A(*sr:(?=a))a\z", "a", true )]
	[InlineData( @"\A(*sr:(?=(aβ)))(aβ)\z", "aβ", true )]
	[InlineData( @"(?<=(*sr:(a)))x", "ax", true )]
	[InlineData( @"(?<=(*sr:(aβ)))x", "aβx", false )]
	[InlineData( @"(?<!(*sr:(aβ)))x", "aβx", true )]
	[InlineData( @"(?<=(*sr:(a{1,2})))x", "aax", true )]
	[InlineData( @"\A(*sr:(*sr:a)β)\z", "aβ", false )]
	[InlineData( @"\A(*sr:(*asr:a+)a)\z", "aa", false )]
	[InlineData( @"\A(*sr:.*)\Kβ\z", "aβ", true )]
	[InlineData( @"\A(*sr:a\Kβ)\z", "aβ", false )]
	[InlineData( @"\Aa\K(*sr:β)\z", "aβ", true )]
	[InlineData( @"\A(*sr:a){2,4}?\z", "aaa", true )]
	[InlineData( @"\A(*asr:a|β)+\z", "aβ", true )]
	[InlineData( @"\A(*sr:a|β)+\z", "aβ", true )]
	[InlineData( @"\A(*sr:(?[[a-z] - [aeiou]])+)\z", "bc", true )]
	[InlineData( @"(?n)\A(*sr:(a))\z", "a", true )]
	[InlineData( @"(?i)\A(*sr:µm)\z", "µm", true )]
	[InlineData( @"(?i)\A(*sr:µm)\z", "μm", false )]
	[InlineData( @"(?aT)\A(*sr:..)\z", "0０", false )]
	[InlineData( @"\A(*sr:..)\z", "٠۰", false )]
	[InlineData( @"\A(*sr:...)\z", "漢あ가", true )]
	[InlineData( @"\A(*sr:...)\z", "あ漢가", false )]
	[InlineData( @"\A(*sr:...)\z", "漢ㄅあ", false )]
	[InlineData( @"\A(*sr:...)\z", "漢가ㄅ", true )]
	[InlineData( @"\A(*sr:..)\z", "a", false )]
	[InlineData( @"\A(*sr:){0}a\z", "a", true )]
	[InlineData( @"\A(?(?=(*sr:a))a|b)\z", "a", true )]
	[InlineData( @"\A(*sr:(?i)aB)c\z", "abc", true )]
	[InlineData( @"\A(*sr:(?i)aB)c\z", "abC", false )]
	[InlineData( @"(*CRLF)(*sr:[\r\n])x", "\r\nx", true )]
	[InlineData( @"\Q(*sr:a)\E", "(*sr:a)", true )]
	public void GroupsKeepAtomicValidationJournalRecursionWidthsAndSubjectProperties( string pattern, string text, bool success ) {
		foreach ( var ucp in new[] { false, true } ) {
			var options = new PcreByteOptions { Utf8 = true, Ucp = ucp };
			var native = Reference( pattern, true, ucp );
			var subject = Encoding.UTF8.GetBytes( text );
			Assert.Equal( success, native.Match( subject ).Success );
			Compare( native, Compile( pattern, options ), subject );
		}
	}

	[Fact]
	public void InvalidUtfFragmentsAndEveryStartOffsetKeepOriginalCaptures() {
		var subject = Convert.FromHexString( "FF61CEB2FFEE808061F09F9880FF" );
		foreach ( var pattern in new[] { @"(*sr:([\s\S]*))", @"\G(*sr:([\s\S]+))", @"(*asr:([\s\S]+))", @"(*sr:((?:a|β)+))" } ) {
			var native = Reference( pattern, true, tolerant: true );
			var managed = Compile( pattern, new() { Utf8 = true, MatchInvalidUtf = true } );
			for ( var offset = 0; offset <= subject.Length; offset++ ) { Compare( native, managed, subject, offset ); }
		}
	}

	[Fact]
	public void RunScanKeepsExactWorkLimitsAndCancellationInsideValidation() {
		var text = new string( 'a', 128 );
		var subject = Encoding.UTF8.GetBytes( text );
		var pattern = @"\A(*sr:(" + text + @"))\z";
		var steps = new List<int>();
		Compare( Reference( pattern, true ), Compile( pattern, new() { Utf8 = true, WorkObserver = steps.Add } ), subject );
		var work = steps.Count;
		Assert.True( work >= 512 );
		Assert.NotNull( Compile( pattern, new() { Utf8 = true, MatchWorkLimit = work } ).Match( subject ).Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded,
			Compile( pattern, new() { Utf8 = true, MatchWorkLimit = work - 1 } ).Match( subject ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { Utf8 = true,
			WorkObserver = count => { if ( count == work - 10 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void RunMarksShareTheCompiledStateBudgetAndSourceDepthIsBounded() {
		var group = "(*sr:)";
		var atLimit = String.Concat( Enumerable.Repeat( group, 256 ) );
		Compare( Reference( atLimit, false ), Compile( atLimit, new() ), Array.Empty<byte>() );
		Assert.True( Reference( atLimit + group, false ).Match( Array.Empty<byte>() ).Success );
		var tooMany = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( atLimit + group ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, tooMany.Diagnostic?.Code );
		Assert.Equal( atLimit.Length, tooMany.Diagnostic?.ByteOffset );
		var shared = String.Concat( Enumerable.Repeat( "a*", 256 ) ) + group;
		var sharedResult = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( shared ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, sharedResult.Diagnostic?.Code );
		var nested = String.Concat( Enumerable.Repeat( "(*sr:", 128 ) ) + "a" + new string( ')', 128 );
		Compare( Reference( nested, true ), Compile( nested, new() { Utf8 = true } ), "a"u8.ToArray() );
		var deep = String.Concat( Enumerable.Repeat( "(*sr:", 257 ) ) + "a" + new string( ')', 257 );
		var deepResult = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( deep ), new() { Utf8 = true } );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, deepResult.Diagnostic?.Code );
		Assert.Equal( 1280, deepResult.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( "(*sr:a", 0, false )]
	[InlineData( "é(*script_run:a", 2, true )]
	[InlineData( "(*asr:a", 0, false )]
	[InlineData( "é(*atomic_script_run:a", 2, true )]
	public void UnclosedRunKeepsOriginalOpeningByteDiagnostic( string pattern, int offset, bool utf8 ) {
		Assert.Throws<PcrePatternException>( () => Reference( pattern, utf8 ) );
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = utf8 } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.ByteOffset );
	}

	private static PcreRegex8Bit Reference( string pattern, bool utf8, bool ucp = false, bool tolerant = false ) =>
		new( Encoding.UTF8.GetBytes( pattern ), utf8 ? Encoding.UTF8 : Encoding.Latin1, new PcreRegexSettings {
			Options = PcreOptions.Compiled | ( utf8 ? PcreOptions.Utf : PcreOptions.None ) | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None )
		} );
	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, pattern + ": " + result.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
	private static void Compare( PcreRegex8Bit native, PcreCompiledBytePattern managed, byte[] subject, int offset = 0 ) {
		var expected = native.Match( subject, offset );
		var actual = managed.Match( subject, offset );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		Assert.True( expected.Success == ( actual.Match is not null ), Convert.ToHexString( subject ) );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Index, actual.Match!.ByteIndex );
		Assert.Equal( expected.Length, actual.Match.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var i = 0; i < expected.Groups.Count; i++ ) {
			var capture = expected.Groups[i];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[i] );
		}
	}
}
