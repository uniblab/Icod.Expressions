using System.Buffers;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreNoDotStarAnchorReferenceTests {
	[Theory]
	[InlineData( "283F73292E2A28", 0, false, -1L, 114, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A28", 0, true, -1L, 114, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5B", 0, false, -1L, 106, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5B", 0, true, -1L, 106, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5C", 0, false, -1L, 101, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5C", 0, true, -1L, 101, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A417B322C317D", 0, false, -1L, 104, 11, "InvalidSyntax", 7 )]
	[InlineData( "283F73292E2A417B322C317D", 0, true, -1L, 104, 11, "InvalidSyntax", 7 )]
	[InlineData( "283F73292E2A41", 0, false, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F73292E2A41", 0, true, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F73292E2A28", 65536, false, -1L, 114, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A28", 65536, true, -1L, 114, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5B", 65536, false, -1L, 106, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5B", 65536, true, -1L, 106, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5C", 65536, false, -1L, 101, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5C", 65536, true, -1L, 101, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A417B322C317D", 65536, false, -1L, 104, 11, "InvalidSyntax", 7 )]
	[InlineData( "283F73292E2A417B322C317D", 65536, true, -1L, 104, 11, "InvalidSyntax", 7 )]
	[InlineData( "283F73292E2A41", 65536, false, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F73292E2A41", 65536, true, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F73292E2A28", 1, false, -1L, 114, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A28", 1, true, -1L, 114, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5B", 1, false, -1L, 106, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5B", 1, true, -1L, 106, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5C", 1, false, -1L, 101, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5C", 1, true, -1L, 101, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A417B322C317D", 1, false, -1L, 104, 11, "InvalidSyntax", 7 )]
	[InlineData( "283F73292E2A417B322C317D", 1, true, -1L, 104, 11, "InvalidSyntax", 7 )]
	[InlineData( "283F73292E2A41", 1, false, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F73292E2A41", 1, true, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F73292E2A28", 65537, false, -1L, 114, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A28", 65537, true, -1L, 114, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5B", 65537, false, -1L, 106, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5B", 65537, true, -1L, 106, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5C", 65537, false, -1L, 101, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5C", 65537, true, -1L, 101, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A417B322C317D", 65537, false, -1L, 104, 11, "InvalidSyntax", 7 )]
	[InlineData( "283F73292E2A417B322C317D", 65537, true, -1L, 104, 11, "InvalidSyntax", 7 )]
	[InlineData( "283F73292E2A41", 65537, false, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F73292E2A41", 65537, true, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F73292E2A28", 5, false, -1L, 114, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A28", 5, true, -1L, 114, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5B", 5, false, -1L, 106, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5B", 5, true, -1L, 106, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5C", 5, false, -1L, 101, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5C", 5, true, -1L, 101, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A417B322C317D", 5, false, -1L, 104, 11, "InvalidSyntax", 7 )]
	[InlineData( "283F73292E2A417B322C317D", 5, true, -1L, 104, 11, "InvalidSyntax", 7 )]
	[InlineData( "283F73292E2A41", 5, false, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F73292E2A41", 5, true, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F73292E2A28", 65541, false, -1L, 114, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A28", 65541, true, -1L, 114, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5B", 65541, false, -1L, 106, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5B", 65541, true, -1L, 106, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5C", 65541, false, -1L, 101, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A5C", 65541, true, -1L, 101, 7, "InvalidSyntax", 6 )]
	[InlineData( "283F73292E2A417B322C317D", 65541, false, -1L, 104, 11, "InvalidSyntax", 7 )]
	[InlineData( "283F73292E2A417B322C317D", 65541, true, -1L, 104, 11, "InvalidSyntax", 7 )]
	[InlineData( "283F73292E2A41", 65541, false, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F73292E2A41", 65541, true, 6L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "283F73292E2AFF", 1, false, -1L, -23, 6, "InvalidUtf8Pattern", 6 )]
	[InlineData( "283F73292E2AFF", 1, true, -1L, -23, 6, "InvalidUtf8Pattern", 6 )]
	[InlineData( "283F73292E2AFF", 65537, false, -1L, -23, 6, "InvalidUtf8Pattern", 6 )]
	[InlineData( "283F73292E2AFF", 65537, true, -1L, -23, 6, "InvalidUtf8Pattern", 6 )]
	[InlineData( "283F73292E2AFF", 5, false, -1L, -23, 6, "InvalidUtf8Pattern", 6 )]
	[InlineData( "283F73292E2AFF", 5, true, -1L, -23, 6, "InvalidUtf8Pattern", 6 )]
	[InlineData( "283F73292E2AFF", 65541, false, -1L, -23, 6, "InvalidUtf8Pattern", 6 )]
	[InlineData( "283F73292E2AFF", 65541, true, -1L, -23, 6, "InvalidUtf8Pattern", 6 )]
	[InlineData( "41", 16, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 65552, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 17, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 65553, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 21, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 65557, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 24, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 65560, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "", 16, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "", 65552, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "", 17, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "", 65553, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "", 21, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "", 65557, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "", 24, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "", 65560, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "FF", 16, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "FF", 65552, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "FF", 17, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "FF", 65553, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "FF", 21, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "FF", 65557, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "FF", 24, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "FF", 65560, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "283F73292E2A41", 16, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "283F73292E2A41", 65552, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "283F73292E2A41", 17, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "283F73292E2A41", 65553, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "283F73292E2A41", 21, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "283F73292E2A41", 65557, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "283F73292E2A41", 24, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "283F73292E2A41", 65560, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F53544152545F4F50542941", 16, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F53544152545F4F50542941", 65552, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F53544152545F4F50542941", 17, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F53544152545F4F50542941", 65553, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F53544152545F4F50542941", 21, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F53544152545F4F50542941", 65557, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F53544152545F4F50542941", 24, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "282A4E4F5F53544152545F4F50542941", 65560, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "28", 16, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "28", 65552, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "28", 17, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "28", 65553, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "28", 21, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "28", 65557, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "28", 24, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "28", 65560, true, 0L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( "41", 16, false, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 65552, false, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 17, false, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 65553, false, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 21, false, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 65557, false, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 24, false, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "41", 65560, false, 0L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( "282A5554462941", 64, false, -1L, 174, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5554462941", 64, true, -1L, 174, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5554462941", 65600, false, -1L, 174, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5554462941", 65600, true, -1L, 174, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5543502941", 128, false, -1L, 175, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5543502941", 128, true, -1L, 175, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5543502941", 65664, false, -1L, 175, 6, "InvalidSyntax", 6 )]
	[InlineData( "282A5543502941", 65664, true, -1L, 175, 6, "InvalidSyntax", 6 )]
	public void CallerFlagPreservesDiagnosticPriority( string hex, int mode, bool enabled, long maximum, int nativeCode, int nativeOffset, string code, int offset ) {
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
	[InlineData( 0, false, "" )]
	[InlineData( 0, false, "?" )]
	[InlineData( 0, false, "+" )]
	[InlineData( 0, true, "" )]
	[InlineData( 0, true, "?" )]
	[InlineData( 0, true, "+" )]
	[InlineData( 65536, false, "" )]
	[InlineData( 65536, false, "?" )]
	[InlineData( 65536, false, "+" )]
	[InlineData( 65536, true, "" )]
	[InlineData( 65536, true, "?" )]
	[InlineData( 65536, true, "+" )]
	[InlineData( 1, false, "" )]
	[InlineData( 1, false, "?" )]
	[InlineData( 1, false, "+" )]
	[InlineData( 1, true, "" )]
	[InlineData( 1, true, "?" )]
	[InlineData( 1, true, "+" )]
	[InlineData( 65537, false, "" )]
	[InlineData( 65537, false, "?" )]
	[InlineData( 65537, false, "+" )]
	[InlineData( 65537, true, "" )]
	[InlineData( 65537, true, "?" )]
	[InlineData( 65537, true, "+" )]
	[InlineData( 5, false, "" )]
	[InlineData( 5, false, "?" )]
	[InlineData( 5, false, "+" )]
	[InlineData( 5, true, "" )]
	[InlineData( 5, true, "?" )]
	[InlineData( 5, true, "+" )]
	[InlineData( 65541, false, "" )]
	[InlineData( 65541, false, "?" )]
	[InlineData( 65541, false, "+" )]
	[InlineData( 65541, true, "" )]
	[InlineData( 65541, true, "?" )]
	[InlineData( 65541, true, "+" )]
	public void CallerFlagChangesRetryWorkButNotExplicitAnchors( int mode, bool enabled, string tail ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?s).*", tail, "Z" ) );
		var subject = Encoding.ASCII.GetBytes( new string( 'B', 64 ) );
		var native = Reference( source, mode, enabled );
		AssertNative( native, subject, 0, 0, "", "" );
		AssertNative( Reference( source, mode, enabled, 1 ), subject, 0, 0, "", "" );
		var steps = new List<int>();
		var baseline = new List<int>();
		var explicitSteps = new List<int>();
		var options = Options( mode, enabled ) with { WorkObserver = steps.Add };
		Compare( native, subject, 0, 0, Compile( source, options ).Match( subject ), "caller retry work" );
		Compare( native, subject, 0, 0, Compile( source, Options( mode, false ) with { WorkObserver = baseline.Add } ).Match( subject ), "default anchor work" );
		Compare( native, subject, 0, 0, Compile( source, options with { Anchored = true, WorkObserver = explicitSteps.Add } ).Match( subject ), "explicit anchor work" );
		Assert.Equal( baseline, explicitSteps );
		if ( enabled ) { Assert.True( steps.Count > baseline.Count, "NoDotStarAnchor must restore ordinary candidate retries" ); }
		else { Assert.Equal( baseline, steps ); }
		AssertBudget( source, subject, options, native, steps );
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	public void PlainCallerKeepsBudgetContract( int mode, bool enabled ) {
		var source = "A"u8.ToArray();
		var native = Reference( source, mode, enabled );
		AssertNative( native, source, 0, 0, "0:1", "" );
		var steps = new List<int>();
		var options = Options( mode, enabled ) with { WorkObserver = steps.Add };
		Compare( native, source, 0, 0, Compile( source, options ).Match( source ), "plain caller" );
		AssertBudget( source, source, options, native, steps );
	}

	private static void AssertBudget( byte[] source, byte[] subject, PcreByteOptions options, PcreRegex8Bit native, List<int> steps ) {
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		var count = steps.Count;
		foreach ( var observed in new[] { false, true } ) {
			var trace = new List<int>();
			var exact = options with { MatchWorkLimit = count, WorkObserver = observed ? trace.Add : null };
			Compare( native, subject, 0, 0, Compile( source, exact ).Match( subject ), "exact work" );
			if ( observed ) { Assert.Equal( steps, trace ); }
			var exhausted = Compile( source, exact with { MatchWorkLimit = count - 1 } ).Match( subject );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( source, options with { WorkObserver = value => { if ( value == 2 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
	}


	[Theory]
	[InlineData( 0 )]
	[InlineData( 65536 )]
	[InlineData( 1 )]
	[InlineData( 65537 )]
	[InlineData( 5 )]
	[InlineData( 65541 )]
	public void CompiledPatternOwnsCallerFlagSourceViewAndMarks( int mode ) {
		var pattern = "(?s).*(*MARK:N)(A)"u8.ToArray();
		var source = new byte[pattern.Length + 2];
		Array.Fill( source, (byte)0xFF );
		pattern.CopyTo( source, 1 );
		var subject = "BA"u8.ToArray();
		var malformed = new byte[] { 0xFF, (byte)'A' };
		var native = Reference( pattern, mode, true );
		AssertNative( native, subject, 0, 0, "0:2,1:1", "4E" );
		if ( ( mode & 4 ) != 0 ) { AssertNative( native, malformed, 0, 0, "1:1,1:1", "4E" ); }
		var options = Options( mode, true );
		var actual = PcreByteCompiler.Compile( source.AsMemory( 1, pattern.Length ), options );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		var compiled = Assert.IsType<PcreCompiledBytePattern>( actual.Pattern );
		Array.Fill( source, (byte)'x' );
		SetFlag( options, false );
		typeof( PcreByteOptions ).GetProperty( "NoStartOptimize" )!.SetValue( options, ( mode & 65536 ) == 0 );
		Compare( native, subject, 0, 0, compiled.Match( subject ), "owned caller flag/view/mark" );
		if ( ( mode & 4 ) != 0 ) { Compare( native, malformed, 0, 0, compiled.Match( malformed ), "owned unanchored fragment retries" ); }
	}

	[Fact]
	public void CallerFlagIsAnInitOnlyBooleanDefaultingFalse() {
		var source = "A"u8.ToArray();
		AssertNative( Reference( source, 0, true ), source, 0, 0, "0:1", "" );
		var property = typeof( PcreByteOptions ).GetProperty( "NoDotStarAnchor" );
		Assert.NotNull( property );
		Assert.Equal( typeof( bool ), property.PropertyType );
		Assert.False( (bool)property.GetValue( new PcreByteOptions() )! );
		var setter = property.SetMethod;
		Assert.NotNull( setter );
		Assert.Contains( typeof( System.Runtime.CompilerServices.IsExternalInit ), setter.ReturnParameter.GetRequiredCustomModifiers() );
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
	public void ProofKeepsFullCapacitiesAndFallsBackWithoutQuota( int selection, bool enabled ) {
		var body = selection switch {
			0 => new string( 'A', 4090 ),
			1 => String.Concat( String.Concat( Enumerable.Repeat( "()", 256 ) ), "A" ),
			2 => String.Concat( String.Concat( Enumerable.Repeat( "(?:", 256 ) ), "A", new string( ')', 256 ) ),
			_ => String.Concat( String.Concat( Enumerable.Repeat( "(?:A){0}", 2200 ) ), "A" )
		};
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?s).*?", body ) );
		var subject = Encoding.ASCII.GetBytes( selection == 0 ? body : "A" );
		var spans = selection == 0 ? "0:4090" : selection == 1 ? String.Concat( "0:1,", String.Join( ",", Enumerable.Repeat( "0:0", 256 ) ) ) : "0:1";
		var native = Reference( source, 0, enabled );
		AssertNative( native, subject, 0, 0, spans, "" );
		Compare( native, subject, 0, 0, Compile( source, Options( 0, enabled ) ).Match( subject ), "capacity and bounded proof fallback" );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void InferenceDoesNotRelaxInstructionLimit( bool enabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( "(?s).*?", new string( 'A', 4091 ) ) );
		var subject = Encoding.ASCII.GetBytes( new string( 'A', 4091 ) );
		AssertNative( Reference( source, 0, enabled ), subject, 0, 0, "0:4091", "" );
		var actual = PcreByteCompiler.Compile( source, Options( 0, enabled ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, actual.Diagnostic?.Code );
	}


	[Theory]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 65537, false )]
	[InlineData( 65537, true )]
	public void ValidationPrecedesInferredCandidateAdmission( int mode, bool enabled ) {
		var source = "(?s).*A"u8.ToArray();
		var options = Options( mode, enabled );
		var native = Reference( source, mode, enabled );
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


	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 65536, false )]
	[InlineData( 65536, true )]
	public void CallerValidationAndLiteralConflictPrecedeMemoryRead( int mode, bool enabled ) {
		var source = "(?s).*A"u8.ToArray();
		var length = Assert.Throws<PcrePatternException>( () => Reference( source, mode, enabled, 0, 1 ) );
		Assert.Equal( 188, (int)length.ErrorCode );
		var literal = Assert.Throws<PcrePatternException>( () => Reference( source, mode | 16, enabled, 0, 1 ) );
		Assert.Equal( enabled ? 192 : 188, (int)literal.ErrorCode );
		using var memory = new UnreadablePatternMemory();
		var options = Options( mode, enabled, 0, 0, 1 );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( memory.Pattern, options, cancellation.Token ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit, PcreByteCompiler.Compile( memory.Pattern, options with { MatchWorkLimit = 0 } ).Diagnostic?.Code );
		Assert.Equal( enabled ? PcreByteDiagnosticCode.InvalidSyntax : PcreByteDiagnosticCode.PatternLengthLimitExceeded,
			PcreByteCompiler.Compile( memory.Pattern, options with { Literal = true } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.PatternLengthLimitExceeded, PcreByteCompiler.Compile( memory.Pattern, options ).Diagnostic?.Code );
		Assert.False( memory.ReadRequested );
	}
	private sealed class UnreadablePatternMemory : MemoryManager<byte> {
		internal bool ReadRequested { get; private set; }
		internal ReadOnlyMemory<byte> Pattern => CreateMemory( 7 );
		public override Span<byte> GetSpan() {
			ReadRequested = true;
			throw new InvalidOperationException( "rejected source memory must not be read" );
		}
		public override MemoryHandle Pin( int elementIndex = 0 ) => throw new NotSupportedException();
		public override void Unpin() { }
		protected override void Dispose( bool disposing ) { }
	}

	[Theory]
	[InlineData( 5, false )]
	[InlineData( 5, true )]
	[InlineData( 65541, false )]
	[InlineData( 65541, true )]
	public void ZeroWidthCommitStillCutsAtRequestedSplitByte( int mode, bool enabled ) {
		var source = "(?s).*?(*COMMIT)A"u8.ToArray();
		var subject = "éA"u8.ToArray();
		var native = Reference( source, mode, enabled );
		AssertNative( native, subject, 1, 0, "", "" );
		Compare( native, subject, 1, 0, Compile( source, Options( mode, enabled ) ).Match( subject, 1 ), "cut before UTF consumption" );
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
		NoStartOptimize = ( mode & 65536 ) != 0, MaxPatternLength = maximum < 0 ? null : (uint)maximum
		};
		SetFlag( options, enabled );
		return options;
	}
	private static void SetFlag( PcreByteOptions options, bool enabled ) {
		// Optional only to compile and exercise the unchanged tests-first baseline.
		typeof( PcreByteOptions ).GetProperty( "NoDotStarAnchor" )?.SetValue( options, enabled );
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
