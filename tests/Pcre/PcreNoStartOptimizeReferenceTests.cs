using System.Buffers;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreNoStartOptimizeReferenceTests {
	[Theory]
	[InlineData( "28", 0, false, -1L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 0, true, -1L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 1, false, -1L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 1, true, -1L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 5, false, -1L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "28", 5, true, -1L, 114, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 0, false, -1L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 0, true, -1L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 1, false, -1L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 1, true, -1L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 5, false, -1L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5B", 5, true, -1L, 106, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 0, false, -1L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 0, true, -1L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 1, false, -1L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 1, true, -1L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 5, false, -1L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "5C", 5, true, -1L, 101, 1, "InvalidSyntax", 0 )]
	[InlineData( "417B322C317D", 0, false, -1L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 0, true, -1L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 1, false, -1L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 1, true, -1L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 5, false, -1L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "417B322C317D", 5, true, -1L, 104, 5, "InvalidSyntax", 1 )]
	[InlineData( "282A4E4F53544152544F505429", 0, false, -1L, 160, 12, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F53544152544F505429", 0, true, -1L, 160, 12, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F53544152544F505429", 1, false, -1L, 160, 12, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F53544152544F505429", 1, true, -1L, 160, 12, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F53544152544F505429", 5, false, -1L, 160, 12, "UnsupportedSyntax", 0 )]
	[InlineData( "282A4E4F53544152544F505429", 5, true, -1L, 160, 12, "UnsupportedSyntax", 0 )]
	[InlineData( "FF", 1, false, -1L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 1, true, -1L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 5, false, -1L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 5, true, -1L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 17, false, -1L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "FF", 17, true, -1L, -23, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "41FF", 1, false, -1L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 1, true, -1L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 5, false, -1L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 5, true, -1L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 17, false, -1L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "41FF", 17, true, -1L, -23, 1, "InvalidUtf8Pattern", 1 )]
	[InlineData( "EDA080", 1, false, -1L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 1, true, -1L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 5, false, -1L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 5, true, -1L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 17, false, -1L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "EDA080", 17, true, -1L, -16, 0, "InvalidUtf8Pattern", 0 )]
	[InlineData( "41", 48, false, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 48, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 2064, false, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 2064, true, -1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A5554462941", 64, false, -1L, 174, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5554462941", 64, true, -1L, 174, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5554462941", 65, false, -1L, 174, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5554462941", 65, true, -1L, 174, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5543502941", 128, false, -1L, 175, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5543502941", 128, true, -1L, 175, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5543502941", 16512, false, -1L, 175, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5543502941", 16512, true, -1L, 175, 6, "InvalidSyntax", 6 )]
	[InlineData( "4141", 0, false, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "4141", 0, true, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "4141", 1, false, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "4141", 1, true, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "4141", 16, false, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "4141", 16, true, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "4141", 17, false, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "4141", 17, true, 1L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	public void NativeFlagRetainsAdmissionPriority( string hex, int mode, bool enabled, long maximum, int nativeCode, int nativeOffset, string code, int offset ) {
		var source = Convert.FromHexString( hex );
		var native = Assert.Throws<PcrePatternException>( () => Reference( source, mode, enabled, 0, maximum ) );
		Assert.Equal( nativeCode, (int)native.ErrorCode );
		Assert.EndsWith( $" at offset {nativeOffset}.", native.Message );
		var actual = PcreByteCompiler.Compile( source, Options( mode, enabled, 0, 0, maximum ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( code, actual.Diagnostic?.Code.ToString() );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	public void PlainPatternBudgetsRemainStableBeforeIntegration( int mode, bool enabled ) {
		var source = "A"u8.ToArray();
		var native = Reference( source, mode, enabled );
		AssertNative( native, source, 0, 0, "0:1", "" );
		var steps = new List<int>();
		var plain = new List<int>();
		var options = Options( mode, enabled ) with { WorkObserver = steps.Add };
		Compare( native, source, 0, 0, Compile( source, options ).Match( source ), "flagged work" );
		Compare( native, source, 0, 0, Compile( source, Options( mode, false ) with { WorkObserver = plain.Add } ).Match( source ), "default work" );
		Assert.Equal( plain, steps );
		AssertBudget( source, source, options, native, steps );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 5 )]
	public void UnfilteredCommitKeepsExactBudgetsAndCancellation( int mode ) {
		var source = Encoding.ASCII.GetBytes( "(*COMMIT)A" );
		var subject = "BA"u8.ToArray();
		var native = Reference( source, mode, true );
		AssertNative( native, subject, 0, 0, "", "" );
		var steps = new List<int>();
		var options = Options( mode, true ) with { WorkObserver = steps.Add };
		Compare( native, subject, 0, 0, Compile( source, options ).Match( subject ), "unfiltered commit" );
		AssertBudget( source, subject, options, native, steps );
	}

	private static void AssertBudget( byte[] source, byte[] subject, PcreByteOptions options, PcreRegex8Bit native, List<int> steps ) {
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		var count = steps.Count;
		Compare( native, subject, 0, 0, Compile( source, options with { MatchWorkLimit = count } ).Match( subject ), "exact work" );
		var exhausted = Compile( source, options with { MatchWorkLimit = count - 1 } ).Match( subject );
		Assert.Null( exhausted.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( source, options with { WorkObserver = value => { if ( value == 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 5, false )]
	[InlineData( 5, true )]
	public void UnobservedMissingLiteralKeepsExactBudgets( int mode, bool enabled ) {
		var source = "A"u8.ToArray();
		var subject = "BBB"u8.ToArray();
		var native = Reference( source, mode, enabled );
		AssertNative( native, subject, 0, 0, "", "" );
		var steps = new List<int>();
		var options = Options( mode, enabled );
		Compare( native, subject, 0, 0, Compile( source, options with { WorkObserver = steps.Add } ).Match( subject ), "observed missing literal" );
		var count = steps.Count;
		Compare( native, subject, 0, 0, Compile( source, options with { MatchWorkLimit = count } ).Match( subject ), "unobserved exact work" );
		var exhausted = Compile( source, options with { MatchWorkLimit = count - 1 } ).Match( subject );
		Assert.Null( exhausted.Match );
		Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 5 )]
	public void CompiledProgramOwnsSourceFlagAndSuccessfulMark( int mode ) {
		var source = Encoding.ASCII.GetBytes( "(*MARK:N)(*COMMIT)A" );
		var rejected = "BA"u8.ToArray();
		var accepted = "A"u8.ToArray();
		var native = Reference( source, mode, true );
		AssertNative( native, rejected, 0, 0, "", "" );
		AssertNative( native, accepted, 0, 0, "0:1", "4E" );
		var options = Options( mode, true );
		var compiled = Compile( source, options );
		Array.Fill( source, (byte)'x' );
		SetFlag( options, false );
		Compare( native, rejected, 0, 0, compiled.Match( rejected ), "owned unfiltered policy" );
		Compare( native, accepted, 0, 0, compiled.Match( accepted ), "owned source/mark" );
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 2, false )]
	[InlineData( 2, true )]
	public void FlagPreservesFullCapacities( int selection, bool enabled ) {
		var body = selection switch {
			0 => new string( 'A', 4095 ),
			1 => String.Concat( String.Concat( Enumerable.Repeat( "()", 256 ) ), "A" ),
			_ => String.Concat( String.Concat( Enumerable.Repeat( "(?:", 256 ) ), "A", new string( ')', 256 ) )
		};
		var source = Encoding.ASCII.GetBytes( body );
		var subject = selection == 0 ? source : "A"u8.ToArray();
		var spans = selection == 0 ? "0:4095" : selection == 1 ? String.Concat( "0:1,", String.Join( ",", Enumerable.Repeat( "0:0", 256 ) ) ) : "0:1";
		var native = Reference( source, 0, enabled );
		AssertNative( native, subject, 0, 0, spans, "" );
		Compare( native, subject, 0, 0, Compile( source, Options( 0, enabled ) ).Match( subject ), "full capacity" );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void FlagDoesNotRelaxInstructionLimit( bool enabled ) {
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
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => compiled.Match( new byte[] { 0xFF }, -1, cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( source, options, cancellation.Token ) );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ExistingAdmissionRejectsBeforeReadingMemory( bool enabled ) {
		var source = "AA"u8.ToArray();
		var length = Assert.Throws<PcrePatternException>( () => Reference( source, 0, enabled, 0, 1 ) );
		Assert.Equal( 188, (int)length.ErrorCode );
		Assert.EndsWith( " at offset 0.", length.Message );
		var conflict = Assert.Throws<PcrePatternException>( () => Reference( source, 48, enabled, 0, 1 ) );
		Assert.Equal( 192, (int)conflict.ErrorCode );
		Assert.EndsWith( " at offset 0.", conflict.Message );
		using var memory = new UnreadablePatternMemory();
		var options = Options( 0, enabled, 0, 0, 1 );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( memory.Pattern, options, cancellation.Token ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit, PcreByteCompiler.Compile( memory.Pattern, options with { MatchWorkLimit = 0 } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, PcreByteCompiler.Compile( memory.Pattern, options with { Literal = true, Multiline = true } ).Diagnostic?.Code );
		var actual = PcreByteCompiler.Compile( memory.Pattern, options );
		Assert.Null( actual.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternLengthLimitExceeded, actual.Diagnostic?.Code );
		Assert.False( memory.ReadRequested );
	}
	private sealed class UnreadablePatternMemory : MemoryManager<byte> {
		internal bool ReadRequested { get; private set; }
		internal ReadOnlyMemory<byte> Pattern => CreateMemory( 2 );
		public override Span<byte> GetSpan() {
			ReadRequested = true;
			throw new InvalidOperationException( "rejected memory must not be read" );
		}
		public override MemoryHandle Pin( int elementIndex = 0 ) => throw new NotSupportedException();
		public override void Unpin() { }
		protected override void Dispose( bool disposing ) { }
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
	private static void SetFlag( PcreByteOptions options, bool enabled ) {
		// Optional only for compiling the unchanged tests-first baseline.
		typeof( PcreByteOptions ).GetProperty( "NoStartOptimize" )?.SetValue( options, enabled );
	}
	private static PcreByteOptions Options( int mode, bool enabled, int empty = 0, int policy = 0, long maximum = -1 ) {
		var options = new PcreByteOptions {
			Utf8 = ( mode & 1 ) != 0, MatchInvalidUtf = ( mode & 4 ) != 0, Caseless = ( mode & 8 ) != 0,
			Literal = ( mode & 16 ) != 0, Multiline = ( mode & 32 ) != 0, NeverUtf = ( mode & 64 ) != 0,
			NeverUcp = ( mode & 128 ) != 0, AllowLookaroundBackslashK = ( mode & 1024 ) != 0,
			NoAutoPossess = ( mode & 2048 ) != 0, Ucp = ( mode & 16384 ) != 0,
			NotEmpty = ( empty & 1 ) != 0, NotEmptyAtStart = ( empty & 2 ) != 0,
			Anchored = ( policy & 1 ) != 0, EndAnchored = ( policy & 2 ) != 0, FirstLine = ( policy & 4 ) != 0,
			MaxPatternLength = maximum < 0 ? null : (uint)maximum
		};
		SetFlag( options, enabled );
		return options;
	}
	private static PcreRegex8Bit Reference( byte[] source, int mode, bool enabled, int policy = 0, long maximum = -1 ) => new( source, Encoding.UTF8, new PcreRegexSettings {
		Options = PcreOptions.Compiled | (PcreOptions)( ( enabled ? 0x10000u : 0u ) | ( ( mode & 1 ) != 0 ? 0x80000u : 0u )
			| ( ( mode & 4 ) != 0 ? 0x4000000u : 0u ) | ( ( mode & 8 ) != 0 ? 8u : 0u ) | ( ( mode & 16 ) != 0 ? 0x2000000u : 0u )
			| ( ( mode & 32 ) != 0 ? 0x400u : 0u ) | ( ( mode & 64 ) != 0 ? 0x1000u : 0u ) | ( ( mode & 128 ) != 0 ? 0x800u : 0u )
			| ( ( mode & 2048 ) != 0 ? 0x4000u : 0u ) | ( ( mode & 16384 ) != 0 ? 0x20000u : 0u )
			| ( ( policy & 1 ) != 0 ? 0x80000000u : 0u ) | ( ( policy & 2 ) != 0 ? 0x20000000u : 0u ) | ( ( policy & 4 ) != 0 ? 0x100u : 0u ) ),
		ExtraCompileOptions = (PcreExtraCompileOptions)( ( mode & 1024 ) != 0 ? 64 : 0 ),
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
