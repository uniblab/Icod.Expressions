using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreNoAutoPossessReferenceTests {
	[Theory]
	[InlineData( "41", 16, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 17, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 21, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 24, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "5E4124", 16, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "5E4124", 17, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "5E4124", 21, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "5E4124", 24, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A5554462941", 16, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A5554462941", 17, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A5554462941", 21, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A5554462941", 24, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "C3A9", 16, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "C3A9", 17, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "C3A9", 21, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "C3A9", 24, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "28", 0, false, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 0, true, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 1, false, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 1, true, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 5, false, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 5, true, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 0, false, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 0, true, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 1, false, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 1, true, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 5, false, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 5, true, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 0, false, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 0, true, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 1, false, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 1, true, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 5, false, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 5, true, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "417B322C317D", 0, false, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 0, true, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 1, false, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 1, true, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 5, false, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 5, true, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "282A4E4F4155544F504F535345535329", 0, false, 160, 15, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F4155544F504F535345535329", 0, true, 160, 15, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F4155544F504F535345535329", 1, false, 160, 15, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F4155544F504F535345535329", 1, true, 160, 15, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F4155544F504F535345535329", 5, false, 160, 15, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F4155544F504F535345535329", 5, true, 160, 15, "UnsupportedSyntax", 0 )]
	[InlineData( "FF", 1, true, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 5, true, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 17, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41FF", 1, true, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 5, true, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 17, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "EDA080", 1, true, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 5, true, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 17, true, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 48, false, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 48, true, 192, 0, "InvalidSyntax", 0 )]
	public void NativeFlagPreservesDiagnosticPriority( string hex, int mode, bool enabled, int nativeCode, int nativeOffset, string code, int offset ) {
		var source = Convert.FromHexString( hex );
		var native = Assert.Throws<PcrePatternException>( () => Reference( source, mode, enabled ) );
		Assert.Equal( nativeCode, (int)native.ErrorCode );
		Assert.EndsWith( $" at offset {nativeOffset}.", native.Message );
		var actual = PcreByteCompiler.Compile( source, Options( mode, enabled ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( code, actual.Diagnostic?.Code.ToString() );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( 16 )]
	[InlineData( 17 )]
	[InlineData( 21 )]
	public void LiteralFlagConflictPrecedesPatternLengthAdmission( int mode ) {
		var source = "AA"u8.ToArray();
		var native = Assert.Throws<PcrePatternException>( () => Reference( source, mode, true, 0, 1 ) );
		Assert.Equal( 192, (int)native.ErrorCode );
		Assert.EndsWith( " at offset 0.", native.Message );
		var actual = PcreByteCompiler.Compile( source, Options( mode, true ) with { MaxPatternLength = 1 } );
		Assert.Null( actual.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code );
		Assert.Equal( 0, actual.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	public void PlainPatternRetainsWorkBudgetsBeforeIntegration( int mode, bool enabled ) {
		var source = "A"u8.ToArray();
		var native = Reference( source, mode, enabled );
		AssertNative( native, source, 0, 0, "0:1", "" );
		var steps = new List<int>();
		var plain = new List<int>();
		var options = Options( mode, enabled ) with { WorkObserver = steps.Add };
		Compare( native, source, 0, 0, Compile( source, options ).Match( source ), "flagged work" );
		Compare( native, source, 0, 0, Compile( source, Options( mode, false ) with { WorkObserver = plain.Add } ).Match( source ), "default work" );
		Assert.Equal( plain, steps );
		AssertBudget( source, source, options, native, 0, steps );
	}

	[Theory]
	[InlineData( 1024 )]
	[InlineData( 1025 )]
	[InlineData( 1029 )]
	public void RetriedReportedSpanRetainsExactBudgetsAndCancellation( int mode ) {
		var source = Encoding.UTF8.GetBytes( @"(?=(A\K))A?" );
		var subject = "A"u8.ToArray();
		var native = Reference( source, mode, true );
		AssertNative( native, subject, 0, 1, "1:-1,0:1", "" );
		var steps = new List<int>();
		var options = Options( mode, true, 1 ) with { WorkObserver = steps.Add };
		Compare( native, subject, 0, 1, Compile( source, options ).Match( subject ), "retry work" );
		AssertBudget( source, subject, options, native, 1, steps );
	}

	private static void AssertBudget( byte[] source, byte[] subject, PcreByteOptions options, PcreRegex8Bit native, int empty, List<int> steps ) {
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		var workCount = steps.Count;
		Compare( native, subject, 0, empty, Compile( source, options with { MatchWorkLimit = workCount } ).Match( subject ), "exact work" );
		var exhausted = Compile( source, options with { MatchWorkLimit = workCount - 1 } ).Match( subject );
		Assert.Null( exhausted.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( source, options with { WorkObserver = count => { if ( count == 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( 1024 )]
	[InlineData( 1025 )]
	[InlineData( 1029 )]
	public void CompiledPatternOwnsSourceFlagAndSuccessfulMark( int mode ) {
		var source = Encoding.UTF8.GetBytes( @"(*MARK:N)(?=(A\K))A?" );
		var subject = "A"u8.ToArray();
		var native = Reference( source, mode, true );
		AssertNative( native, subject, 0, 1, "1:-1,0:1", "4E" );
		var options = Options( mode, true, 1 );
		var compiled = Compile( source, options );
		Array.Fill( source, (byte)'x' );
		SetFlag( options, false );
		Compare( native, subject, 0, 1, compiled.Match( subject ), "owned flag/source/mark" );
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 2, false )]
	[InlineData( 2, true )]
	public void FlagKeepsFullCaptureDepthAndInstructionCapacities( int selection, bool enabled ) {
		var pattern = selection switch {
			0 => new string( 'A', 4095 ),
			1 => String.Concat( String.Concat( Enumerable.Repeat( "()", 256 ) ), "A" ),
			_ => String.Concat( String.Concat( Enumerable.Repeat( "(?:", 256 ) ), "A", new string( ')', 256 ) )
		};
		var source = Encoding.ASCII.GetBytes( pattern );
		var subject = selection == 0 ? source : "A"u8.ToArray();
		var spans = selection == 0 ? "0:4095" : selection == 1 ? String.Concat( "0:1,", String.Join( ",", Enumerable.Repeat( "0:0", 256 ) ) ) : "0:1";
		var native = Reference( source, 0, enabled );
		AssertNative( native, subject, 0, 0, spans, "" );
		Compare( native, subject, 0, 0, Compile( source, Options( 0, enabled ) ).Match( subject ), "shared capacity" );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void FlagDoesNotRelaxTheInstructionLimit( bool enabled ) {
		var source = Encoding.ASCII.GetBytes( new string( 'A', 4096 ) );
		AssertNative( Reference( source, 0, enabled ), source, 0, 0, "0:4096", "" );
		var actual = PcreByteCompiler.Compile( source, Options( 0, enabled ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, actual.Diagnostic?.Code );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void FlagRetainsUtfOffsetAndCancellationPriority( bool enabled ) {
		var source = "A"u8.ToArray();
		var options = Options( 1, enabled );
		var native = Reference( source, 1, enabled );
		AssertNative( native, source, 0, 0, "0:1", "" );
		var compiled = Compile( source, options );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( new byte[] { 0xFF } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( "é"u8.ToArray(), 1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( new byte[] { 0xFF }, -1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit, PcreByteCompiler.Compile( new byte[] { 0xFF }, options with { MatchWorkLimit = 0 } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, PcreByteCompiler.Compile( new byte[] { 0xFF }, options ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => compiled.Match( new byte[] { 0xFF }, -1, cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( source, options, cancellation.Token ) );
	}

	private static void AssertProtocols( string hex, int mode, bool enabled, int empty, int policy, string protocols ) {
		var source = Convert.FromHexString( hex );
		var cases = protocols.Split( ';' ).Select( Protocol.Parse ).ToArray();
		var native = Reference( source, mode, enabled, policy );
		foreach ( var item in cases ) { AssertNative( native, item.Subject, item.Offset, empty, item.Spans, item.Mark ); }
		var compiled = Compile( source, Options( mode, enabled, empty, policy ) );
		foreach ( var item in cases ) { Compare( native, item.Subject, item.Offset, empty, compiled.Match( item.Subject, item.Offset ), $"{hex}/{mode}/{enabled}/{empty}/{policy}/{Convert.ToHexString( item.Subject )}/{item.Offset}" ); }
	}
	private readonly record struct Protocol( byte[] Subject, int Offset, string Spans, string Mark ) {
		internal static Protocol Parse( string value ) {
			var parts = value.Split( '/' );
			return new( Convert.FromHexString( parts[0] ), Int32.Parse( parts[1] ), parts[2], parts[3] );
		}
	}
	private static void SetFlag( PcreByteOptions options, bool enabled ) {
		// Optional only for compiling the unchanged tests-first baseline.
		typeof( PcreByteOptions ).GetProperty( "NoAutoPossess" )?.SetValue( options, enabled );
	}
	private static PcreByteOptions Options( int mode, bool enabled, int empty = 0, int policy = 0 ) {
		var options = new PcreByteOptions {
			Utf8 = ( mode & 1 ) != 0, MatchInvalidUtf = ( mode & 4 ) != 0, Caseless = ( mode & 8 ) != 0,
			Literal = ( mode & 16 ) != 0, Multiline = ( mode & 32 ) != 0, AllowLookaroundBackslashK = ( mode & 1024 ) != 0,
			NotEmpty = ( empty & 1 ) != 0, NotEmptyAtStart = ( empty & 2 ) != 0,
			Anchored = ( policy & 1 ) != 0, EndAnchored = ( policy & 2 ) != 0, FirstLine = ( policy & 4 ) != 0
		};
		SetFlag( options, enabled );
		return options;
	}
	private static PcreRegex8Bit Reference( byte[] source, int mode, bool enabled, int policy = 0, uint? maximum = null ) => new( source, Encoding.UTF8, new PcreRegexSettings {
		Options = PcreOptions.Compiled | (PcreOptions)( ( enabled ? 0x4000u : 0u ) | ( ( mode & 1 ) != 0 ? 0x80000u : 0u )
			| ( ( mode & 4 ) != 0 ? 0x4000000u : 0u ) | ( ( mode & 8 ) != 0 ? 8u : 0u ) | ( ( mode & 16 ) != 0 ? 0x2000000u : 0u )
			| ( ( mode & 32 ) != 0 ? 0x400u : 0u ) | ( ( policy & 1 ) != 0 ? 0x80000000u : 0u )
			| ( ( policy & 2 ) != 0 ? 0x20000000u : 0u ) | ( ( policy & 4 ) != 0 ? 0x100u : 0u ) ),
		ExtraCompileOptions = (PcreExtraCompileOptions)( ( mode & 1024 ) != 0 ? 64 : 0 ),
		MaxPatternLength = maximum, ParensLimit = 512, NewLine = (PcreNewLine)2, BackslashR = (PcreBackslashR)1
	} );
	private static PcreCompiledBytePattern Compile( byte[] source, PcreByteOptions options ) {
		var actual = PcreByteCompiler.Compile( source, options );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( actual.Pattern );
	}
	private static PcreMatchOptions MatchOptions( int empty ) => (PcreMatchOptions)( empty * 4 );
	private static void AssertNative( PcreRegex8Bit native, byte[] subject, int offset, int empty, string spans, string mark ) {
		var match = native.Match( subject, offset, MatchOptions( empty ) );
		Assert.Equal( spans.Length > 0, match.Success );
		if ( !match.Success ) { return; }
		Assert.Equal( mark, Convert.ToHexString( match.Mark.ToArray() ) );
		var groups = spans.Split( ',' );
		Assert.Equal( groups.Length, match.Groups.Count );
		for ( var group = 0; group < groups.Length; group++ ) {
			var capture = match.Groups[group];
			Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.EndIndex - capture.Index}" : "-" );
		}
	}
	private static void Compare( PcreRegex8Bit native, byte[] subject, int offset, int empty, PcreByteMatchResult actual, string detail ) {
		var expected = native.Match( subject, offset, MatchOptions( empty ) );
		Assert.True( actual.IsSuccess, $"{detail}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ), $"{detail}: expected index {( expected.Success ? expected.Index : -1 )}, actual {actual.Match?.ByteIndex ?? -1}" );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.EndIndex - expected.Index, actual.Match!.ByteLength );
		Assert.Equal( expected.Mark.ToArray(), actual.Match.MarkName.ToArray() );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.EndIndex - capture.Index ) : null, actual.Match.Captures[group] );
		}
	}
}
