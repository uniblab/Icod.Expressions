using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreCallerLineBoundaryReferenceTests {
	[Theory]
	[InlineData( "28", 0, 1, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 0, 2, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 0, 3, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 0, 1, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 0, 2, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 0, 3, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 0, 1, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 0, 2, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 0, 3, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B422D415D", 0, 1, 108, 4, "InvalidSyntax", 2 )]
	[InlineData( "5B422D415D", 0, 2, 108, 4, "InvalidSyntax", 2 )]
	[InlineData( "5B422D415D", 0, 3, 108, 4, "InvalidSyntax", 2 )]
	[InlineData( "417B322C317D", 0, 1, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 0, 2, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 0, 3, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "28", 1, 1, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 1, 2, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 1, 3, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 1, 1, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 1, 2, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 1, 3, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 1, 1, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 1, 2, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 1, 3, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B422D415D", 1, 1, 108, 4, "InvalidSyntax", 0 )]
	[InlineData( "5B422D415D", 1, 2, 108, 4, "InvalidSyntax", 0 )]
	[InlineData( "5B422D415D", 1, 3, 108, 4, "InvalidSyntax", 0 )]
	[InlineData( "417B322C317D", 1, 1, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 1, 2, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 1, 3, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "28", 5, 1, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 5, 2, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 5, 3, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 5, 1, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 5, 2, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 5, 3, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 5, 1, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 5, 2, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 5, 3, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B422D415D", 5, 1, 108, 4, "InvalidSyntax", 0 )]
	[InlineData( "5B422D415D", 5, 2, 108, 4, "InvalidSyntax", 0 )]
	[InlineData( "5B422D415D", 5, 3, 108, 4, "InvalidSyntax", 0 )]
	[InlineData( "417B322C317D", 5, 1, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 5, 2, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 5, 3, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "FF", 1, 1, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 1, 2, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 1, 3, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 1, 1, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 1, 2, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "C0AF", 1, 3, -17, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 1, 1, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 1, 2, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 1, 3, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 1, 1, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 1, 2, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "F4908080", 1, 3, -15, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "E282", 1, 1, -3, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "E282", 1, 2, -3, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "E282", 1, 3, -3, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "282A4E4F54424F4C29", 0, 1, 160, 8, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F54424F4C29", 0, 2, 160, 8, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F54424F4C29", 0, 3, 160, 8, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F54454F4C29", 0, 1, 160, 8, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F54454F4C29", 0, 2, 160, 8, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F54454F4C29", 0, 3, 160, 8, "UnsupportedSyntax", 0 )]
	[InlineData( "41", 176, 1, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 176, 2, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 176, 3, 192, 0, "InvalidSyntax", 0 )]
	public void MatchContextDoesNotAlterCompileDiagnostics( string hex, int mode, int context, int nativeCode, int nativeOffset, string code, int offset ) {
		var source = Convert.FromHexString( hex );
		var native = Assert.Throws<PcrePatternException>( () => Reference( source, mode ) );
		Assert.Equal( nativeCode, (int)native.ErrorCode );
		Assert.EndsWith( $" at offset {nativeOffset}.", native.Message );
		var actual = PcreByteCompiler.Compile( source, Options( mode, context ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( Enum.Parse<PcreByteDiagnosticCode>( code ), actual.Diagnostic?.Code );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( 0, 1 )]
	[InlineData( 0, 2 )]
	[InlineData( 0, 3 )]
	[InlineData( 1, 1 )]
	[InlineData( 1, 2 )]
	[InlineData( 1, 3 )]
	public void NonAnchorSuccessPreservesTraceBudgetsAndCancellationBeforeIntegration( int mode, int context ) {
		var source = "A"u8.ToArray();
		var reference = Reference( source, mode );
		AssertNative( reference, source, 0, context, 0, "0:1", "" );
		var steps = new List<int>();
		var plain = new List<int>();
		var options = Options( mode, context ) with { WorkObserver = steps.Add };
		Compare( reference, source, 0, context, 0, Compile( source, options ).Match( source ), "context trace" );
		Compare( reference, source, 0, context, 0, Compile( source, Options( mode, 0 ) with { WorkObserver = plain.Add } ).Match( source ), "plain trace" );
		Assert.Equal( plain, steps );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		// Keep the first count stable when later observers append to the list.
		var workCount = steps.Count;
		Compare( reference, source, 0, context, 0, Compile( source, options with { MatchWorkLimit = workCount } ).Match( source ), "exact work" );
		var exhausted = Compile( source, options with { MatchWorkLimit = workCount - 1 } ).Match( source );
		Assert.Null( exhausted.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( source, options with { WorkObserver = count => { if ( count == 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( source, cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	public void CompiledBoundaryContextAndLabelsSurviveCallerMutation( int context ) {
		var source = "(*MARK:N)(^A|B$|C)"u8.ToArray();
		var reference = Reference( source, 0 );
		AssertNative( reference, "A"u8.ToArray(), 0, context, 0, ( context & 1 ) != 0 ? "" : "0:1,0:1", ( context & 1 ) != 0 ? "" : "4E" );
		AssertNative( reference, "B"u8.ToArray(), 0, context, 0, ( context & 2 ) != 0 ? "" : "0:1,0:1", ( context & 2 ) != 0 ? "" : "4E" );
		AssertNative( reference, "C"u8.ToArray(), 0, context, 0, "0:1,0:1", "4E" );
		var options = Options( 0, context );
		var compiled = Compile( source, options );
		Array.Fill( source, (byte)'x' );
		SetContext( options, 0 );
		foreach ( var subject in new[] { "A"u8.ToArray(), "B"u8.ToArray(), "C"u8.ToArray() } ) {
			Compare( reference, subject, 0, context, 0, compiled.Match( subject ), "owned context/source/label" );
		}
	}

	[Theory]
	[InlineData( 0, 1 )]
	[InlineData( 0, 2 )]
	[InlineData( 0, 3 )]
	[InlineData( 1, 1 )]
	[InlineData( 1, 2 )]
	[InlineData( 1, 3 )]
	[InlineData( 2, 1 )]
	[InlineData( 2, 2 )]
	[InlineData( 2, 3 )]
	public void ContextFlagsDoNotSpendCaptureDepthOrInstructionSlots( int selection, int context ) {
		var pattern = selection switch {
			0 => new string( 'A', 4095 ),
			1 => String.Concat( String.Concat( Enumerable.Repeat( "()", 256 ) ), "A" ),
			_ => String.Concat( String.Concat( Enumerable.Repeat( "(?:", 256 ) ), "A", new string( ')', 256 ) )
		};
		var mode = selection == 0 ? 16 : 1;
		var source = Encoding.UTF8.GetBytes( pattern );
		var subject = selection == 0 ? source : "A"u8.ToArray();
		var spans = selection == 0 ? "0:4095" : selection == 1 ? String.Concat( "0:1,", String.Join( ",", Enumerable.Repeat( "0:0", 256 ) ) ) : "0:1";
		var reference = Reference( source, mode );
		AssertNative( reference, subject, 0, context, 0, spans, "" );
		Compare( reference, subject, 0, context, 0, Compile( source, Options( mode, context ) ).Match( subject ), "shared capacity" );
	}

	[Theory]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	public void ContextFlagsDoNotRelaxTheInstructionLimit( int context ) {
		var source = Encoding.ASCII.GetBytes( new string( 'A', 4096 ) );
		AssertNative( Reference( source, 16 ), source, 0, context, 0, "0:4096", "" );
		var actual = PcreByteCompiler.Compile( source, Options( 16, context ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, actual.Diagnostic?.Code );
	}

	[Theory]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	public void ImpossibleAnchorsStillValidateUtfOffsetsLimitsAndCancellation( int context ) {
		var source = "^A$"u8.ToArray();
		var options = Options( 1, context );
		var compiled = Compile( source, options );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( new byte[] { 0xFF } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( "é"u8.ToArray(), 1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( new byte[] { 0xFF }, -1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit, PcreByteCompiler.Compile( new byte[] { 0xFF }, options with { MatchWorkLimit = 0 } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Pattern, PcreByteCompiler.Compile( new byte[] { 0xFF }, options ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => compiled.Match( new byte[] { 0xFF }, -1, cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( source, options with { MatchWorkLimit = 0 }, cancellation.Token ) );
	}

	[Theory]
	[InlineData( false, false, 1 )]
	[InlineData( false, false, 2 )]
	[InlineData( false, false, 3 )]
	[InlineData( false, true, 1 )]
	[InlineData( false, true, 2 )]
	[InlineData( false, true, 3 )]
	[InlineData( true, false, 1 )]
	[InlineData( true, false, 2 )]
	[InlineData( true, false, 3 )]
	[InlineData( true, true, 1 )]
	[InlineData( true, true, 2 )]
	[InlineData( true, true, 3 )]
	public void TypedAnchorsKeepTheExactInstructionBoundary( bool start, bool over, int context ) {
		var text = new string( 'A', over ? 4095 : 4094 );
		var source = Encoding.ASCII.GetBytes( start ? String.Concat( "^", text ) : String.Concat( text, "$" ) );
		var reference = Reference( source, 0 );
		AssertNative( reference, Array.Empty<byte>(), 0, context, 0, "", "" );
		var actual = PcreByteCompiler.Compile( source, Options( 0, context ) );
		if ( over ) {
			Assert.Null( actual.Pattern );
			Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, actual.Diagnostic?.Code );
		} else {
			Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
			Compare( reference, Array.Empty<byte>(), 0, context, 0, actual.Pattern!.Match( Array.Empty<byte>() ), "anchor capacity" );
		}
	}

	private static void AssertProtocols( string hex, int mode, int context, int policy, int empty, string protocols ) {
		var source = Convert.FromHexString( hex );
		var cases = protocols.Split( ';' ).Select( Protocol.Parse ).ToArray();
		var reference = Reference( source, mode, policy );
		foreach ( var item in cases ) { AssertNative( reference, item.Subject, item.Offset, context, empty, item.Spans, item.Mark ); }
		var compiled = Compile( source, Options( mode, context, policy, empty ) );
		foreach ( var item in cases ) {
			Compare( reference, item.Subject, item.Offset, context, empty, compiled.Match( item.Subject, item.Offset ), $"{hex}/{mode}/{context}/{policy}/{empty}/{Convert.ToHexString( item.Subject )}/{item.Offset}" );
		}
	}
	private readonly record struct Protocol( byte[] Subject, int Offset, string Spans, string Mark ) {
		internal static Protocol Parse( string value ) {
			var parts = value.Split( '/' );
			return new( Convert.FromHexString( parts[0] ), Int32.Parse( parts[1] ), parts[2], parts[3] );
		}
	}
	private static void SetContext( PcreByteOptions options, int context ) {
		// Optional only for tests-first compilation; all native expectations remain mandatory.
		typeof( PcreByteOptions ).GetProperty( "NotBol" )?.SetValue( options, ( context & 1 ) != 0 );
		typeof( PcreByteOptions ).GetProperty( "NotEol" )?.SetValue( options, ( context & 2 ) != 0 );
	}
	private static PcreByteOptions Options( int mode, int context, int policy = 0, int empty = 0 ) {
		var options = new PcreByteOptions {
			Anchored = ( policy & 1 ) != 0, EndAnchored = ( policy & 2 ) != 0, FirstLine = ( policy & 4 ) != 0,
			NotEmpty = ( empty & 1 ) != 0, NotEmptyAtStart = ( empty & 2 ) != 0,
			Utf8 = ( mode & 1 ) != 0, Ucp = ( mode & 2 ) != 0, MatchInvalidUtf = ( mode & 4 ) != 0,
			Caseless = ( mode & 8 ) != 0, Literal = ( mode & 16 ) != 0, Multiline = ( mode & 32 ) != 0,
			MatchWord = ( mode & 64 ) != 0, MatchLine = ( mode & 128 ) != 0,
			AnyCrLfNewline = ( mode & 256 ) != 0, BsrAnyCrLf = ( mode & 512 ) != 0,
			AllowLookaroundBackslashK = ( mode & 1024 ) != 0, CaselessRestricted = ( mode & 2048 ) != 0,
			TurkishCasing = ( mode & 4096 ) != 0, DotAll = ( mode & 8192 ) != 0,
			DollarEndOnly = ( mode & 16384 ) != 0, AltCircumflex = ( mode & 32768 ) != 0
		};
		SetContext( options, context );
		return options;
	}
	private static PcreRegex8Bit Reference( byte[] source, int mode, int policy = 0 ) => new( source, Encoding.UTF8, new PcreRegexSettings {
		Options = PcreOptions.Compiled | (PcreOptions)( ( ( policy & 1 ) != 0 ? 0x80000000u : 0u ) | ( ( policy & 2 ) != 0 ? 0x20000000u : 0u ) | ( ( policy & 4 ) != 0 ? 0x100u : 0u )
			| ( ( mode & 1 ) != 0 ? 0x80000u : 0u ) | ( ( mode & 2 ) != 0 ? 0x20000u : 0u )
			| ( ( mode & 4 ) != 0 ? 0x4000000u : 0u ) | ( ( mode & 8 ) != 0 ? 8u : 0u )
			| ( ( mode & 16 ) != 0 ? 0x2000000u : 0u ) | ( ( mode & 32 ) != 0 ? 1024u : 0u ) | ( ( mode & 8192 ) != 0 ? 32u : 0u )
			| ( ( mode & 16384 ) != 0 ? 0x10u : 0u ) | ( ( mode & 32768 ) != 0 ? 0x200000u : 0u ) ),
		ExtraCompileOptions = (PcreExtraCompileOptions)( ( ( mode & 64 ) != 0 ? 4 : 0 ) | ( ( mode & 128 ) != 0 ? 8 : 0 )
			| ( ( mode & 1024 ) != 0 ? 64 : 0 ) | ( ( mode & 2048 ) != 0 ? 128 : 0 ) | ( ( mode & 4096 ) != 0 ? 0x10000 : 0 ) ),
		NewLine = (PcreNewLine)( ( mode & 256 ) != 0 ? 5 : 2 ), BackslashR = (PcreBackslashR)( ( mode & 512 ) != 0 ? 2 : 1 ), ParensLimit = 512
	} );
	private static PcreMatchOptions MatchOptions( int context, int empty ) => (PcreMatchOptions)( context | ( ( empty & 1 ) != 0 ? 4 : 0 ) | ( ( empty & 2 ) != 0 ? 8 : 0 ) );
	private static PcreCompiledBytePattern Compile( byte[] source, PcreByteOptions options ) {
		var actual = PcreByteCompiler.Compile( source, options );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		return Assert.IsType<PcreCompiledBytePattern>( actual.Pattern );
	}
	private static void AssertNative( PcreRegex8Bit reference, byte[] subject, int offset, int context, int empty, string spans, string mark ) {
		var match = reference.Match( subject, offset, MatchOptions( context, empty ) );
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
	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, int context, int empty, PcreByteMatchResult actual, string detail ) {
		var expected = reference.Match( subject, offset, MatchOptions( context, empty ) );
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
