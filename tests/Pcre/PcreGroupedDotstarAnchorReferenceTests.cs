using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreGroupedDotstarAnchorReferenceTests {
	[Theory]
	[InlineData( 1, "283F7329283F3A2E2A41", 0, 114, 10, "InvalidSyntax", 4 )]
	[InlineData( 2, "283F7329283F3A2E2A41295B", 0, 106, 12, "InvalidSyntax", 11 )]
	[InlineData( 3, "283F7329283F3A2E2A41295C", 0, 101, 12, "InvalidSyntax", 11 )]
	[InlineData( 4, "283F7329283F3A2E2A4129417B322C317D", 0, 104, 16, "InvalidSyntax", 12 )]
	[InlineData( 5, "283F7329283F3A2E2A41295C677B397D", 0, 115, 16, "InvalidSyntax", 12 )]
	[InlineData( 7, "283F7329283F3A2E2A41", 1, 114, 10, "InvalidSyntax", 4 )]
	[InlineData( 8, "283F7329283F3A2E2A41295B", 1, 106, 12, "InvalidSyntax", 11 )]
	[InlineData( 9, "283F7329283F3A2E2A41295C", 1, 101, 12, "InvalidSyntax", 11 )]
	[InlineData( 10, "283F7329283F3A2E2A4129417B322C317D", 1, 104, 16, "InvalidSyntax", 12 )]
	[InlineData( 11, "283F7329283F3A2E2A41295C677B397D", 1, 115, 16, "InvalidSyntax", 12 )]
	[InlineData( 13, "283F7329283F3A2E2A41", 5, 114, 10, "InvalidSyntax", 4 )]
	[InlineData( 14, "283F7329283F3A2E2A41295B", 5, 106, 12, "InvalidSyntax", 11 )]
	[InlineData( 15, "283F7329283F3A2E2A41295C", 5, 101, 12, "InvalidSyntax", 11 )]
	[InlineData( 16, "283F7329283F3A2E2A4129417B322C317D", 5, 104, 16, "InvalidSyntax", 12 )]
	[InlineData( 17, "283F7329283F3A2E2A41295C677B397D", 5, 115, 16, "InvalidSyntax", 12 )]
	public void GroupedProofPreservesDiagnostics( int diagnostic, string hex, int mode, int nativeCode, int nativeOffset, string code, int offset ) {
		var source = Convert.FromHexString( hex );
		var native = Assert.Throws<PcrePatternException>( () => Reference( source, mode, false ) );
		Assert.Equal( nativeCode, (int)native.ErrorCode );
		Assert.EndsWith( $" at offset {nativeOffset}.", native.Message );
		var actual = PcreByteCompiler.Compile( source, Options( mode, false ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( code, actual.Diagnostic?.Code.ToString() );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}
	[Theory]
	[InlineData( 0, false, 1 )]
	[InlineData( 0, false, 256 )]
	[InlineData( 0, true, 1 )]
	[InlineData( 0, true, 256 )]
	[InlineData( 1, false, 1 )]
	[InlineData( 1, false, 256 )]
	[InlineData( 1, true, 1 )]
	[InlineData( 1, true, 256 )]
	[InlineData( 5, false, 1 )]
	[InlineData( 5, false, 256 )]
	[InlineData( 5, true, 1 )]
	[InlineData( 5, true, 256 )]
	[InlineData( 65536, false, 1 )]
	[InlineData( 65536, true, 1 )]
	[InlineData( 65541, false, 1 )]
	[InlineData( 65541, true, 1 )]
	public void MandatoryGroupChainsRetainDirectStarWorkAndBudgets( int mode, bool disabled, int depth ) {
		var body = "(?s).*?Z"u8.ToArray();
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?s)", String.Concat( Enumerable.Repeat( "(?:", depth ) ), ".*?Z", new string( ')', depth ) ) );
		var subject = Encoding.ASCII.GetBytes( new string( 'B', 64 ) );
		var native = Reference( source, mode, disabled );
		AssertNative( native, subject, 0, 0, "", "" );
		var direct = new List<int>();
		var grouped = new List<int>();
		var options = Options( mode, disabled ) with { WorkObserver = grouped.Add };
		Compare( native, subject, 0, 0, Compile( source, options ).Match( subject ), "group work" );
		Compare( native, subject, 0, 0, Compile( body, Options( mode, disabled ) with { WorkObserver = direct.Add } ).Match( subject ), "direct work" );
		Assert.Equal( direct, grouped );
		AssertBudget( source, subject, options, native, grouped );
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 5, false )]
	[InlineData( 5, true )]
	public void GroupedPolicyOwnsSlicedSourceCapturesAndMark( int mode, bool disabled ) {
		var pattern = "(?s)((?:.*)(*MARK:N)(A))"u8.ToArray();
		var backing = new byte[pattern.Length + 2];
		Array.Fill( backing, (byte)0xFF );
		pattern.CopyTo( backing, 1 );
		var subject = "BA"u8.ToArray();
		var split = "éA"u8.ToArray();
		var native = Reference( pattern, mode, disabled );
		AssertNative( native, subject, 0, 0, "0:2,0:2,1:1", "4E" );
		if ( mode == 5 ) { AssertNative( native, split, 1, 0, disabled ? "2:1,2:1,2:1" : "", disabled ? "4E" : "" ); }
		var options = Options( mode, disabled );
		var result = PcreByteCompiler.Compile( backing.AsMemory( 1, pattern.Length ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		var compiled = Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
		Array.Fill( backing, (byte)'x' );
		typeof( PcreByteOptions ).GetProperty( "NoDotStarAnchor" )!.SetValue( options, !disabled );
		typeof( PcreByteOptions ).GetProperty( "NoStartOptimize" )!.SetValue( options, true );
		Compare( native, subject, 0, 0, compiled.Match( subject ), "owned groups" );
		if ( mode == 5 ) { Compare( native, split, 1, 0, compiled.Match( split, 1 ), "owned grouped split" ); }
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 2, false )]
	[InlineData( 2, true )]
	public void GroupProofPreservesFullInstructionCaptureAndDepthCapacities( int selection, bool disabled ) {
		var pattern = selection switch {
			0 => String.Concat( "(?s)(?:.*?", new string( 'A', 4090 ), ")" ),
			1 => String.Concat( "(?s)", new string( '(', 256 ), ".*?A", new string( ')', 256 ) ),
			_ => String.Concat( "(?s)", String.Concat( Enumerable.Repeat( "(?:", 256 ) ), ".*?A", new string( ')', 256 ) )
		};
		var source = Encoding.ASCII.GetBytes( pattern );
		var subject = Encoding.ASCII.GetBytes( selection == 0 ? new string( 'A', 4090 ) : "A" );
		var spans = selection == 0 ? "0:4090" : selection == 1 ? String.Join( ",", Enumerable.Repeat( "0:1", 257 ) ) : "0:1";
		var native = Reference( source, 0, disabled );
		AssertNative( native, subject, 0, 0, spans, "" );
		Compare( native, subject, 0, 0, Compile( source, Options( 0, disabled ) ).Match( subject ), "group capacities" );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void GroupProofDoesNotRelaxInstructionLimit( bool disabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?s)(?:.*?", new string( 'A', 4091 ), ")" ) );
		var subject = Encoding.ASCII.GetBytes( new string( 'A', 4091 ) );
		AssertNative( Reference( source, 0, disabled ), subject, 0, 0, "0:4091", "" );
		var result = PcreByteCompiler.Compile( source, Options( 0, disabled ) );
		Assert.Null( result.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, result.Diagnostic?.Code );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void GroupProofExhaustionFallsBackWithoutAnAdmissionLimit( bool disabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?s)(?:.*?A)", String.Concat( Enumerable.Repeat( "B{0}", 4096 ) ) ) );
		var native = Reference( source, 0, disabled );
		AssertNative( native, "BA"u8.ToArray(), 0, 0, "0:2", "" );
		Compare( native, "BA"u8.ToArray(), 0, 0, Compile( source, Options( 0, disabled ) ).Match( "BA"u8.ToArray() ), "proof exhaustion" );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void GroupedProgramPreservesValidationAndCancellationPriority( bool disabled ) {
		var source = "(?s)(?:.*A)"u8.ToArray();
		var options = Options( 1, disabled );
		var native = Reference( source, 1, disabled );
		AssertNative( native, "A"u8.ToArray(), 0, 0, "0:1", "" );
		var compiled = Compile( source, options );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( new byte[] { 0xFF } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( "é"u8.ToArray(), 1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( new byte[] { 0xFF }, -1 ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => compiled.Match( new byte[] { 0xFF }, -1, cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( source, options, cancellation.Token ) );
	}

	private static void AssertBudget( byte[] source, byte[] subject, PcreByteOptions options, PcreRegex8Bit native, List<int> steps, int offset = 0 ) {
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		var count = steps.Count;
		foreach ( var observed in new[] { false, true } ) {
			var trace = new List<int>();
			var exact = options with { MatchWorkLimit = count, WorkObserver = observed ? trace.Add : null };
			Compare( native, subject, offset, 0, Compile( source, exact ).Match( subject, offset ), "exact work" );
			if ( observed ) { Assert.Equal( steps, trace ); }
			var exhausted = Compile( source, exact with { MatchWorkLimit = count - 1 } ).Match( subject, offset );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( source, options with { WorkObserver = value => { if ( value == 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, offset, cancellation.Token ) );
	}

	private static void AssertProtocols( string hex, int mode, bool enabled, int empty, int policy, long maximum, string protocols ) {
		var source = Convert.FromHexString( hex );
		var cases = protocols.Split( ';' ).Select( Protocol.Parse ).ToArray();
		var native = Reference( source, mode, enabled, policy, maximum );
		foreach ( var item in cases ) { AssertNative( native, item.Subject, item.Offset, empty, item.Spans, item.Mark ); }
		var compiled = Compile( source, Options( mode, enabled, empty, policy, maximum ) );
		foreach ( var item in cases ) { Compare( native, item.Subject, item.Offset, empty, compiled.Match( item.Subject, item.Offset ), $"{hex}/{mode}/{enabled}/{empty}/{policy}/{maximum}/{Convert.ToHexString( item.Subject )}/{item.Offset}" ); }
	}
	private readonly record struct Protocol( byte[] Subject, int Offset, string Spans, string Mark ) {
		internal static Protocol Parse( string value ) {
			var parts = value.Split( '/' );
			return new( Convert.FromHexString( parts[0] ), Int32.Parse( parts[1] ), parts[2], parts[3] );
		}
	}
	private static PcreByteOptions Options( int mode, bool enabled, int empty = 0, int policy = 0, long maximum = -1 ) {
		var options = new PcreByteOptions {
		Utf8 = ( mode & 1 ) != 0, MatchInvalidUtf = ( mode & 4 ) != 0, Caseless = ( mode & 8 ) != 0,
		Literal = ( mode & 16 ) != 0, Multiline = ( mode & 32 ) != 0, NeverUtf = ( mode & 64 ) != 0,
		NeverUcp = ( mode & 128 ) != 0, NeverBackslashC = ( mode & 256 ) != 0, NeverCallout = ( mode & 512 ) != 0,
		AllowLookaroundBackslashK = ( mode & 1024 ) != 0, Extended = ( mode & 32768 ) != 0, NoAutoPossess = ( mode & 2048 ) != 0,
		MatchWord = ( mode & 4096 ) != 0, MatchLine = ( mode & 8192 ) != 0, Ucp = ( mode & 16384 ) != 0,
		NotEmpty = ( empty & 1 ) != 0, NotEmptyAtStart = ( empty & 2 ) != 0,
		Anchored = ( policy & 1 ) != 0, EndAnchored = ( policy & 2 ) != 0, FirstLine = ( policy & 4 ) != 0,
		NoDotStarAnchor = enabled, NoStartOptimize = ( mode & 65536 ) != 0, MaxPatternLength = maximum < 0 ? null : (uint)maximum
		};
		return options;
	}
	private static PcreRegex8Bit Reference( byte[] source, int mode, bool enabled, int policy = 0, long maximum = -1 ) => new( source, Encoding.UTF8, new PcreRegexSettings {
		Options = PcreOptions.Compiled | (PcreOptions)( ( enabled ? 0x8000u : 0u ) | ( ( mode & 65536 ) != 0 ? 0x10000u : 0u ) | ( ( mode & 1 ) != 0 ? 0x80000u : 0u )
			| ( ( mode & 4 ) != 0 ? 0x4000000u : 0u ) | ( ( mode & 8 ) != 0 ? 8u : 0u ) | ( ( mode & 16 ) != 0 ? 0x2000000u : 0u )
			| ( ( mode & 32 ) != 0 ? 0x400u : 0u ) | ( ( mode & 64 ) != 0 ? 0x1000u : 0u ) | ( ( mode & 128 ) != 0 ? 0x800u : 0u )
			| ( ( mode & 256 ) != 0 ? 0x100000u : 0u ) | ( ( mode & 2048 ) != 0 ? 0x4000u : 0u ) | ( ( mode & 32768 ) != 0 ? 0x80u : 0u ) | ( ( mode & 16384 ) != 0 ? 0x20000u : 0u )
			| ( ( policy & 1 ) != 0 ? 0x80000000u : 0u ) | ( ( policy & 2 ) != 0 ? 0x20000000u : 0u ) | ( ( policy & 4 ) != 0 ? 0x100u : 0u ) ),
		ExtraCompileOptions = (PcreExtraCompileOptions)( ( ( mode & 1024 ) != 0 ? 64 : 0 ) | ( ( mode & 512 ) != 0 ? 0x8000 : 0 ) | ( ( mode & 4096 ) != 0 ? 4 : 0 ) | ( ( mode & 8192 ) != 0 ? 8 : 0 ) ),
		MaxPatternLength = maximum < 0 ? null : (uint)maximum, ParensLimit = 512, NewLine = (PcreNewLine)2, BackslashR = (PcreBackslashR)1
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
