using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreUtfRangeUnionStarAnchorReferenceTests {
	[Theory]
	[InlineData( 1, "283F3A5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A42", 1, 114, 34, 0 )]
	[InlineData( 2, "5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A425B", 1, 106, 32, 31 )]
	[InlineData( 3, "5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A425C", 1, 101, 32, 31 )]
	[InlineData( 4, "5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A425C677B39397D", 1, 115, 37, 31 )]
	[InlineData( 5, "5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A42427B322C317D", 1, 104, 36, 32 )]
	[InlineData( 6, "283F3A5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A42", 5, 114, 34, 0 )]
	[InlineData( 7, "5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A425B", 5, 106, 32, 31 )]
	[InlineData( 8, "5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A425C", 5, 101, 32, 31 )]
	[InlineData( 9, "5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A425C677B39397D", 5, 115, 37, 31 )]
	[InlineData( 10, "5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A42427B322C317D", 5, 104, 36, 32 )]
	[InlineData( 11, "283F3A5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A42", 16389, 114, 34, 0 )]
	[InlineData( 12, "5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A425B", 16389, 106, 32, 31 )]
	[InlineData( 13, "5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A425C", 16389, 101, 32, 31 )]
	[InlineData( 14, "5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A425C677B39397D", 16389, 115, 37, 31 )]
	[InlineData( 15, "5B5C787B3130307D2D5C787B3130464646467D5C7830302D5C7846465D2A42427B322C317D", 16389, 104, 36, 32 )]
	public void RangeUnionProofPreservesDiagnostics( int diagnostic, string hex, int mode, int nativeCode, int nativeOffset, int offset ) {
		var source = Convert.FromHexString( hex );
		var native = Assert.Throws<PcrePatternException>( () => Reference( source, mode, false ) );
		Assert.Equal( nativeCode, (int)native.ErrorCode );
		Assert.EndsWith( $" at offset {nativeOffset}.", native.Message );
		var actual = PcreByteCompiler.Compile( source, Options( mode, false ) );
		Assert.True( actual.Diagnostic is not null, $"diagnostic {diagnostic}: expected an error" );
		Assert.Null( actual.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, actual.Diagnostic?.Code );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}

	private const string Union = @"[\x00-\xFF\x{100}-\x{10FFFF}]";
	private const string ReverseUnion = @"[\x{100}-\x{10FFFF}\x00-\xFF]";

	[Theory]
	[InlineData( 1, false, 0 )]
	[InlineData( 1, false, 128 )]
	[InlineData( 1, true, 0 )]
	[InlineData( 1, true, 128 )]
	[InlineData( 5, false, 0 )]
	[InlineData( 5, false, 128 )]
	[InlineData( 5, true, 0 )]
	[InlineData( 5, true, 128 )]
	[InlineData( 16389, false, 0 )]
	[InlineData( 16389, false, 128 )]
	[InlineData( 16389, true, 0 )]
	[InlineData( 16389, true, 128 )]
	[InlineData( 65541, false, 0 )]
	[InlineData( 65541, false, 128 )]
	[InlineData( 65541, true, 0 )]
	[InlineData( 65541, true, 128 )]
	[InlineData( 13, false, 0 )]
	[InlineData( 13, false, 128 )]
	[InlineData( 13, true, 0 )]
	[InlineData( 13, true, 128 )]
	public void RangeUnionRetainsCallerAnchoringWorkAndExactBudgets( int mode, bool disabled, int depth ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( String.Concat( Enumerable.Repeat( "(?:", depth ) ), ReverseUnion, "*?Z", new string( ')', depth ) ) );
		var subject = Encoding.ASCII.GetBytes( new string( 'B', 64 ) );
		var native = Reference( source, mode, disabled );
		AssertNative( native, subject, 0, 0, "", "" );
		var inferred = new List<int>();
		var caller = new List<int>();
		var options = Options( mode, disabled ) with { WorkObserver = inferred.Add };
		Compare( native, subject, 0, 0, Compile( source, options ).Match( subject ), "union work" );
		Compare( native, subject, 0, 0, Compile( source, Options( mode, disabled ) with { Anchored = !disabled, WorkObserver = caller.Add } ).Match( subject ), "caller work" );
		Assert.Equal( caller, inferred );
		AssertBudget( source, subject, options, native, inferred );
	}

	[Theory]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 5, false )]
	[InlineData( 5, true )]
	[InlineData( 16389, false )]
	[InlineData( 16389, true )]
	public void RangeUnionPolicyOwnsSourceCapturesAndMark( int mode, bool disabled ) {
		var pattern = Encoding.ASCII.GetBytes( String.Concat( ReverseUnion, "*(*MARK:N)(B)" ) );
		var backing = new byte[pattern.Length + 2];
		Array.Fill( backing, (byte)0xFF );
		pattern.CopyTo( backing, 1 );
		var native = Reference( pattern, mode, disabled );
		AssertNative( native, "AB"u8.ToArray(), 0, 0, "0:2,1:1", "4E" );
		var options = Options( mode, disabled );
		var result = PcreByteCompiler.Compile( backing.AsMemory( 1, pattern.Length ), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.Message );
		var compiled = Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
		Array.Fill( backing, (byte)'x' );
		typeof( PcreByteOptions ).GetProperty( "NoDotStarAnchor" )!.SetValue( options, !disabled );
		typeof( PcreByteOptions ).GetProperty( "NoStartOptimize" )!.SetValue( options, true );
		Compare( native, "AB"u8.ToArray(), 0, 0, compiled.Match( "AB"u8.ToArray() ), "owned union" );
		Compare( native, "B"u8.ToArray(), 0, 0, compiled.Match( "B"u8.ToArray() ), "owned capture/mark" );
		if ( ( mode & 4 ) != 0 ) {
			AssertNative( native, "éB"u8.ToArray(), 1, 0, disabled ? "2:1,2:1" : "", disabled ? "4E" : "" );
			Compare( native, "éB"u8.ToArray(), 1, 0, compiled.Match( "éB"u8.ToArray(), 1 ), "owned split offset" );
		}
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 2, false )]
	[InlineData( 2, true )]
	public void RangeUnionPreservesInstructionCaptureAndDepthCapacities( int selection, bool disabled ) {
		var pattern = selection switch {
			0 => String.Concat( ReverseUnion, "*?", new string( 'A', 4090 ) ),
			1 => String.Concat( ReverseUnion, "*?", String.Concat( Enumerable.Repeat( "()", 256 ) ), "B" ),
			_ => String.Concat( String.Concat( Enumerable.Repeat( "(?:", 256 ) ), ReverseUnion, "*?B", new string( ')', 256 ) )
		};
		var source = Encoding.ASCII.GetBytes( pattern );
		var subject = Encoding.ASCII.GetBytes( selection == 0 ? new string( 'A', 4090 ) : "B" );
		var spans = selection == 0 ? "0:4090" : selection == 1 ? String.Concat( "0:1,", String.Join( ",", Enumerable.Repeat( "0:0", 256 ) ) ) : "0:1";
		var native = Reference( source, 1, disabled );
		AssertNative( native, subject, 0, 0, spans, "" );
		Compare( native, subject, 0, 0, Compile( source, Options( 1, disabled ) ).Match( subject ), "union capacities" );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void RangeUnionDoesNotRelaxInstructionLimitOrGlobalProofLimit( bool disabled ) {
		var tooLarge = Encoding.ASCII.GetBytes( String.Concat( Union, "*?", new string( 'A', 4091 ) ) );
		AssertNative( Reference( tooLarge, 1, disabled ), Encoding.ASCII.GetBytes( new string( 'A', 4091 ) ), 0, 0, "0:4091", "" );
		var result = PcreByteCompiler.Compile( tooLarge, Options( 1, disabled ) );
		Assert.Null( result.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, result.Diagnostic?.Code );
		var erased = Encoding.ASCII.GetBytes( String.Concat( Union, "*?B(?:", new string( 'C', 4096 ), "){0}" ) );
		var native = Reference( erased, 65541, disabled );
		AssertNative( native, "AB"u8.ToArray(), 0, 0, "0:2", "" );
		Compare( native, "AB"u8.ToArray(), 0, 0, Compile( erased, Options( 65541, disabled ) ).Match( "AB"u8.ToArray() ), "erased AST proof fallback" );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void RangeUnionPreservesValidationAndCancellationPriority( bool disabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( Union, "*B" ) );
		var options = Options( 1, disabled );
		AssertNative( Reference( source, 1, disabled ), "B"u8.ToArray(), 0, 0, "0:1", "" );
		var compiled = Compile( source, options );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( new byte[] { 0xFF } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, compiled.Match( new byte[] { 0xFF, (byte)'B' }, 1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( "é"u8.ToArray(), 1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, compiled.Match( new byte[] { 0xFF }, -1 ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => compiled.Match( new byte[] { 0xFF }, -1, cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( source, options, cancellation.Token ) );
	}

	[Theory]
	[InlineData( 64, true, true, false )]
	[InlineData( 64, true, true, true )]
	[InlineData( 65, true, false, false )]
	[InlineData( 65, true, false, true )]
	[InlineData( 4096, false, true, false )]
	[InlineData( 4096, false, true, true )]
	[InlineData( 4097, false, false, false )]
	[InlineData( 4097, false, false, true )]
	public void RangeUnionCompletePassesRespectTheSharedProofBound( int count, bool reverse, bool admitted, bool disabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( CoverageRanges( count, reverse ), "*?Z" ) );
		AssertAdmissionAndWork( source, admitted, disabled );
	}

	[Theory]
	[InlineData( 2048, false )]
	[InlineData( 2048, true )]
	[InlineData( 2049, false )]
	[InlineData( 2049, true )]
	public void RangeUnionAlternativesShareOneCoverageBudget( int firstCount, bool disabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?:", CoverageRanges( firstCount, false ), "*?Z|", CoverageRanges( 2048, false ), "*?Z)" ) );
		AssertAdmissionAndWork( source, firstCount == 2048, disabled );
	}

	[Theory]
	[InlineData( 4032, false )]
	[InlineData( 4032, true )]
	[InlineData( 4033, false )]
	[InlineData( 4033, true )]
	public void RangeUnionDoesNotChargeItsAdditionalPassesToTheMemberBudget( int secondCount, bool disabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?:", CoverageRanges( 64, true ), "*?Z|", FullRangeMembers( secondCount ), "*?Z)" ) );
		AssertAdmissionAndWork( source, secondCount == 4032, disabled );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void RangeUnionCoverageIsIndependentOfAnExhaustedMemberBudget( bool disabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?:", FullRangeMembers( 4096 ), "*?Z|", Union, "*?Z)" ) );
		AssertAdmissionAndWork( source, true, disabled );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void RangeUnionCoverageIsIndependentOfAnExhaustedPredicateBudget( bool disabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?:[", String.Concat( Enumerable.Repeat( @"\P{Any}", 4095 ) ), @"\p{Any}]*?Z|", Union, "*?Z)" ) );
		AssertAdmissionAndWork( source, true, disabled );
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 2, false )]
	[InlineData( 2, true )]
	[InlineData( 3, false )]
	[InlineData( 3, true )]
	[InlineData( 4, false )]
	[InlineData( 4, true )]
	public void RangeUnionExhaustionPreservesAllEstablishedProofPaths( int selection, bool disabled ) {
		var second = selection switch {
			0 => String.Concat( @"[\x00-\x{10FFFF}", String.Concat( Enumerable.Repeat( @"\p{L}", 4097 ) ), "]" ),
			1 => @"\p{Any}",
			2 => @"[\p{L}\p{Any}]",
			3 => @"[x\p{Any}]",
			_ => FullRangeMembers( 2 )
		};
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?:", CoverageRanges( 64, true ), "*?Z|", second, "*?Z)" ) );
		AssertAdmissionAndWork( source, true, disabled );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void OversizedUnionListKeepsAnyFallbackWithoutPartialCoverageCharge( bool disabled ) {
		var ranges = CoverageRanges( 4097, false );
		var first = String.Concat( @"[\p{Any}", ranges.Substring( 1, ranges.Length - 2 ), "]" );
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?:", first, "*?Z|", Union, "*?Z)" ) );
		AssertAdmissionAndWork( source, true, disabled );
	}

	[Fact]
	public void NonCoveringRangeListConsumesItsCompletePassBeforeAnyFallback() {
		var first = String.Concat( @"[\p{Any}", new string( 'x', 4096 ), "]" );
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?:", first, "*?Z|", Union, "*?Z)" ) );
		AssertAdmissionAndWork( source, false, false );
	}

	[Fact]
	public void RangeUnionScansTheWholeListAfterEarlyCoverage() {
		var first = String.Concat( @"[\x00-\xFF\x{100}-\x{10FFFF}", new string( 'x', 4094 ), "]" );
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?:", first, "*?Z|", Union, "*?Z)" ) );
		AssertAdmissionAndWork( source, false, false );
	}

	[Theory]
	[InlineData( 4092 )]
	[InlineData( 4093 )]
	public void RangeUnionStopsAfterACompletePassMakesNoProgress( int secondCount ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( @"(?:[\p{Any}\x00x]*?Z|", CoverageRanges( secondCount, false ), "*?Z)" ) );
		AssertAdmissionAndWork( source, secondCount == 4092, false );
	}

	private static void AssertAdmissionAndWork( byte[] source, bool admitted, bool disabled ) {
		var options = Options( 65541, disabled );
		var native = Reference( source, 65541, disabled );
		AssertNative( native, "AZ"u8.ToArray(), 0, 0, "0:2", "" );
		Compare( native, "AZ"u8.ToArray(), 0, 0, Compile( source, options ).Match( "AZ"u8.ToArray() ), "union admission" );
		if ( admitted ) {
			AssertNative( native, "éZ"u8.ToArray(), 1, 0, disabled ? "2:1" : "", "" );
			Compare( native, "éZ"u8.ToArray(), 1, 0, Compile( source, options ).Match( "éZ"u8.ToArray(), 1 ), "union proof boundary" );
		}
		var subject = "AA"u8.ToArray();
		AssertNative( native, subject, 0, 0, "", "" );
		var actual = new List<int>();
		var baseline = new List<int>();
		var observed = options with { WorkObserver = actual.Add };
		Compare( native, subject, 0, 0, Compile( source, observed ).Match( subject ), "union exact work" );
		var anchored = admitted && !disabled;
		var equivalent = options with { Anchored = anchored, NoDotStarAnchor = !anchored, WorkObserver = baseline.Add };
		Compare( native, subject, 0, 0, Compile( source, equivalent ).Match( subject ), "equivalent caller or declined anchor" );
		Assert.Equal( baseline, actual );
		AssertBudget( source, subject, options, native, actual );
	}

	private static string FullRangeMembers( int count ) => String.Concat( @"[\x00-\x{10FFFF}", new string( 'x', count - 1 ), "]" );

	private static string CoverageRanges( int count, bool reverse ) {
		var ranges = new List<string>();
		for ( var value = 0; value < count - 1; value++ ) { ranges.Add( $@"\x{{{value:X}}}" ); }
		ranges.Add( $@"\x{{{count - 1:X}}}-\x{{10FFFF}}" );
		if ( reverse ) { ranges.Reverse(); }
		return String.Concat( "[", String.Concat( ranges ), "]" );
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

	private static void AssertProtocols( string hex, int mode, bool enabled, int empty, int policy, long maximum, string protocols, int profile ) {
		var source = Convert.FromHexString( hex );
		var cases = protocols.Split( ';' ).Select( Protocol.Parse ).ToArray();
		var native = Reference( source, mode, enabled, policy, maximum );
		foreach ( var item in cases ) { AssertNative( native, item.Subject, item.Offset, empty, item.Spans, item.Mark ); }
		var compiled = Compile( source, Options( mode, enabled, empty, policy, maximum ) );
		foreach ( var item in cases ) { Compare( native, item.Subject, item.Offset, empty, compiled.Match( item.Subject, item.Offset ), $"{profile}/{hex}/{mode}/{enabled}/{empty}/{policy}/{maximum}/{Convert.ToHexString( item.Subject )}/{item.Offset}" ); }
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
