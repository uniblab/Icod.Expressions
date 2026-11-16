using System.Buffers;
using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed partial class PcreNoDotStarAnchorPrefixReferenceTests {
	private const string Directive = "(*NO_DOTSTAR_ANCHOR)";
	[Theory]
	[InlineData( 1, "282A4E4F5F444F54535441525F414E43484F522928", 0, false, -1L, 114, 21, "InvalidSyntax", 20 )]
	[InlineData( 2, "282A4E4F5F444F54535441525F414E43484F52295B", 0, false, -1L, 106, 21, "InvalidSyntax", 20 )]
	[InlineData( 3, "282A4E4F5F444F54535441525F414E43484F52295C", 0, false, -1L, 101, 21, "InvalidSyntax", 20 )]
	[InlineData( 4, "282A4E4F5F444F54535441525F414E43484F5229417B322C317D", 0, false, -1L, 104, 25, "InvalidSyntax", 21 )]
	[InlineData( 5, "41282A4E4F5F444F54535441525F414E43484F5229", 0, false, -1L, 160, 20, "UnsupportedSyntax", 1 )]
	[InlineData( 6, "283F3A282A4E4F5F444F54535441525F414E43484F52294129", 0, false, -1L, 160, 22, "UnsupportedSyntax", 3 )]
	[InlineData( 7, "283F6929282A4E4F5F444F54535441525F414E43484F522941", 0, false, -1L, 160, 23, "UnsupportedSyntax", 4 )]
	[InlineData( 8, "20282A4E4F5F444F54535441525F414E43484F5229", 0, false, -1L, 160, 20, "UnsupportedSyntax", 1 )]
	[InlineData( 9, "282A4E4F5F444F54535441525F414E43484F523A582941", 0, false, -1L, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( 10, "282A4E4F444F5453544152414E43484F522941", 0, false, -1L, 160, 17, "UnsupportedSyntax", 0 )]
	[InlineData( 11, "282A6E6F5F646F74737461725F616E63686F722941", 0, false, -1L, 195, 20, "UnsupportedSyntax", 0 )]
	[InlineData( 12, "282A4E4F5F444F54535441525F414E43484F52", 0, false, -1L, 160, 19, "InvalidSyntax", 1 )]
	[InlineData( 13, "282A4E4F5F444F54535441525F414E43484F52294141", 0, false, 21L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 14, "282A4E4F5F444F54535441525F414E43484F522928", 0, true, -1L, 114, 21, "InvalidSyntax", 20 )]
	[InlineData( 15, "282A4E4F5F444F54535441525F414E43484F52295B", 0, true, -1L, 106, 21, "InvalidSyntax", 20 )]
	[InlineData( 16, "282A4E4F5F444F54535441525F414E43484F52295C", 0, true, -1L, 101, 21, "InvalidSyntax", 20 )]
	[InlineData( 17, "282A4E4F5F444F54535441525F414E43484F5229417B322C317D", 0, true, -1L, 104, 25, "InvalidSyntax", 21 )]
	[InlineData( 18, "41282A4E4F5F444F54535441525F414E43484F5229", 0, true, -1L, 160, 20, "UnsupportedSyntax", 1 )]
	[InlineData( 19, "283F3A282A4E4F5F444F54535441525F414E43484F52294129", 0, true, -1L, 160, 22, "UnsupportedSyntax", 3 )]
	[InlineData( 20, "283F6929282A4E4F5F444F54535441525F414E43484F522941", 0, true, -1L, 160, 23, "UnsupportedSyntax", 4 )]
	[InlineData( 21, "20282A4E4F5F444F54535441525F414E43484F5229", 0, true, -1L, 160, 20, "UnsupportedSyntax", 1 )]
	[InlineData( 22, "282A4E4F5F444F54535441525F414E43484F523A582941", 0, true, -1L, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( 23, "282A4E4F444F5453544152414E43484F522941", 0, true, -1L, 160, 17, "UnsupportedSyntax", 0 )]
	[InlineData( 24, "282A6E6F5F646F74737461725F616E63686F722941", 0, true, -1L, 195, 20, "UnsupportedSyntax", 0 )]
	[InlineData( 25, "282A4E4F5F444F54535441525F414E43484F52", 0, true, -1L, 160, 19, "InvalidSyntax", 1 )]
	[InlineData( 26, "282A4E4F5F444F54535441525F414E43484F52294141", 0, true, 21L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 27, "282A4E4F5F444F54535441525F414E43484F522928", 1, false, -1L, 114, 21, "InvalidSyntax", 20 )]
	[InlineData( 28, "282A4E4F5F444F54535441525F414E43484F52295B", 1, false, -1L, 106, 21, "InvalidSyntax", 20 )]
	[InlineData( 29, "282A4E4F5F444F54535441525F414E43484F52295C", 1, false, -1L, 101, 21, "InvalidSyntax", 20 )]
	[InlineData( 30, "282A4E4F5F444F54535441525F414E43484F5229417B322C317D", 1, false, -1L, 104, 25, "InvalidSyntax", 21 )]
	[InlineData( 31, "41282A4E4F5F444F54535441525F414E43484F5229", 1, false, -1L, 160, 20, "UnsupportedSyntax", 1 )]
	[InlineData( 32, "283F3A282A4E4F5F444F54535441525F414E43484F52294129", 1, false, -1L, 160, 22, "UnsupportedSyntax", 3 )]
	[InlineData( 33, "283F6929282A4E4F5F444F54535441525F414E43484F522941", 1, false, -1L, 160, 23, "UnsupportedSyntax", 4 )]
	[InlineData( 34, "20282A4E4F5F444F54535441525F414E43484F5229", 1, false, -1L, 160, 20, "UnsupportedSyntax", 1 )]
	[InlineData( 35, "282A4E4F5F444F54535441525F414E43484F523A582941", 1, false, -1L, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( 36, "282A4E4F444F5453544152414E43484F522941", 1, false, -1L, 160, 17, "UnsupportedSyntax", 0 )]
	[InlineData( 37, "282A6E6F5F646F74737461725F616E63686F722941", 1, false, -1L, 195, 20, "UnsupportedSyntax", 0 )]
	[InlineData( 38, "282A4E4F5F444F54535441525F414E43484F52", 1, false, -1L, 160, 19, "InvalidSyntax", 1 )]
	[InlineData( 39, "282A4E4F5F444F54535441525F414E43484F52294141", 1, false, 21L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 40, "282A4E4F5F444F54535441525F414E43484F522928", 1, true, -1L, 114, 21, "InvalidSyntax", 20 )]
	[InlineData( 41, "282A4E4F5F444F54535441525F414E43484F52295B", 1, true, -1L, 106, 21, "InvalidSyntax", 20 )]
	[InlineData( 42, "282A4E4F5F444F54535441525F414E43484F52295C", 1, true, -1L, 101, 21, "InvalidSyntax", 20 )]
	[InlineData( 43, "282A4E4F5F444F54535441525F414E43484F5229417B322C317D", 1, true, -1L, 104, 25, "InvalidSyntax", 21 )]
	[InlineData( 44, "41282A4E4F5F444F54535441525F414E43484F5229", 1, true, -1L, 160, 20, "UnsupportedSyntax", 1 )]
	[InlineData( 45, "283F3A282A4E4F5F444F54535441525F414E43484F52294129", 1, true, -1L, 160, 22, "UnsupportedSyntax", 3 )]
	[InlineData( 46, "283F6929282A4E4F5F444F54535441525F414E43484F522941", 1, true, -1L, 160, 23, "UnsupportedSyntax", 4 )]
	[InlineData( 47, "20282A4E4F5F444F54535441525F414E43484F5229", 1, true, -1L, 160, 20, "UnsupportedSyntax", 1 )]
	[InlineData( 48, "282A4E4F5F444F54535441525F414E43484F523A582941", 1, true, -1L, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( 49, "282A4E4F444F5453544152414E43484F522941", 1, true, -1L, 160, 17, "UnsupportedSyntax", 0 )]
	[InlineData( 50, "282A6E6F5F646F74737461725F616E63686F722941", 1, true, -1L, 195, 20, "UnsupportedSyntax", 0 )]
	[InlineData( 51, "282A4E4F5F444F54535441525F414E43484F52", 1, true, -1L, 160, 19, "InvalidSyntax", 1 )]
	[InlineData( 52, "282A4E4F5F444F54535441525F414E43484F52294141", 1, true, 21L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 53, "282A4E4F5F444F54535441525F414E43484F522928", 5, false, -1L, 114, 21, "InvalidSyntax", 20 )]
	[InlineData( 54, "282A4E4F5F444F54535441525F414E43484F52295B", 5, false, -1L, 106, 21, "InvalidSyntax", 20 )]
	[InlineData( 55, "282A4E4F5F444F54535441525F414E43484F52295C", 5, false, -1L, 101, 21, "InvalidSyntax", 20 )]
	[InlineData( 56, "282A4E4F5F444F54535441525F414E43484F5229417B322C317D", 5, false, -1L, 104, 25, "InvalidSyntax", 21 )]
	[InlineData( 57, "41282A4E4F5F444F54535441525F414E43484F5229", 5, false, -1L, 160, 20, "UnsupportedSyntax", 1 )]
	[InlineData( 58, "283F3A282A4E4F5F444F54535441525F414E43484F52294129", 5, false, -1L, 160, 22, "UnsupportedSyntax", 3 )]
	[InlineData( 59, "283F6929282A4E4F5F444F54535441525F414E43484F522941", 5, false, -1L, 160, 23, "UnsupportedSyntax", 4 )]
	[InlineData( 60, "20282A4E4F5F444F54535441525F414E43484F5229", 5, false, -1L, 160, 20, "UnsupportedSyntax", 1 )]
	[InlineData( 61, "282A4E4F5F444F54535441525F414E43484F523A582941", 5, false, -1L, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( 62, "282A4E4F444F5453544152414E43484F522941", 5, false, -1L, 160, 17, "UnsupportedSyntax", 0 )]
	[InlineData( 63, "282A6E6F5F646F74737461725F616E63686F722941", 5, false, -1L, 195, 20, "UnsupportedSyntax", 0 )]
	[InlineData( 64, "282A4E4F5F444F54535441525F414E43484F52", 5, false, -1L, 160, 19, "InvalidSyntax", 1 )]
	[InlineData( 65, "282A4E4F5F444F54535441525F414E43484F52294141", 5, false, 21L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 66, "282A4E4F5F444F54535441525F414E43484F522928", 5, true, -1L, 114, 21, "InvalidSyntax", 20 )]
	[InlineData( 67, "282A4E4F5F444F54535441525F414E43484F52295B", 5, true, -1L, 106, 21, "InvalidSyntax", 20 )]
	[InlineData( 68, "282A4E4F5F444F54535441525F414E43484F52295C", 5, true, -1L, 101, 21, "InvalidSyntax", 20 )]
	[InlineData( 69, "282A4E4F5F444F54535441525F414E43484F5229417B322C317D", 5, true, -1L, 104, 25, "InvalidSyntax", 21 )]
	[InlineData( 70, "41282A4E4F5F444F54535441525F414E43484F5229", 5, true, -1L, 160, 20, "UnsupportedSyntax", 1 )]
	[InlineData( 71, "283F3A282A4E4F5F444F54535441525F414E43484F52294129", 5, true, -1L, 160, 22, "UnsupportedSyntax", 3 )]
	[InlineData( 72, "283F6929282A4E4F5F444F54535441525F414E43484F522941", 5, true, -1L, 160, 23, "UnsupportedSyntax", 4 )]
	[InlineData( 73, "20282A4E4F5F444F54535441525F414E43484F5229", 5, true, -1L, 160, 20, "UnsupportedSyntax", 1 )]
	[InlineData( 74, "282A4E4F5F444F54535441525F414E43484F523A582941", 5, true, -1L, 160, 19, "UnsupportedSyntax", 0 )]
	[InlineData( 75, "282A4E4F444F5453544152414E43484F522941", 5, true, -1L, 160, 17, "UnsupportedSyntax", 0 )]
	[InlineData( 76, "282A6E6F5F646F74737461725F616E63686F722941", 5, true, -1L, 195, 20, "UnsupportedSyntax", 0 )]
	[InlineData( 77, "282A4E4F5F444F54535441525F414E43484F52", 5, true, -1L, 160, 19, "InvalidSyntax", 1 )]
	[InlineData( 78, "282A4E4F5F444F54535441525F414E43484F52294141", 5, true, 21L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 79, "282A4E4F5F444F54535441525F414E43484F5229FF", 1, false, -1L, -23, 20, "InvalidUtf8Pattern", 20 )]
	[InlineData( 80, "282A4E4F5F444F54535441525F414E43484F5229282A5554462941", 65, false, -1L, 174, 26, "InvalidSyntax", 26 )]
	[InlineData( 81, "282A4E4F5F444F54535441525F414E43484F5229282A5543502941", 129, false, -1L, 175, 26, "InvalidSyntax", 26 )]
	[InlineData( 82, "282A4E4F5F444F54535441525F414E43484F5229FF", 1, true, -1L, -23, 20, "InvalidUtf8Pattern", 20 )]
	[InlineData( 83, "282A4E4F5F444F54535441525F414E43484F5229282A5554462941", 65, true, -1L, 174, 26, "InvalidSyntax", 26 )]
	[InlineData( 84, "282A4E4F5F444F54535441525F414E43484F5229282A5543502941", 129, true, -1L, 175, 26, "InvalidSyntax", 26 )]
	[InlineData( 85, "282A4E4F5F444F54535441525F414E43484F5229FF", 5, false, -1L, -23, 20, "InvalidUtf8Pattern", 20 )]
	[InlineData( 86, "282A4E4F5F444F54535441525F414E43484F5229282A5554462941", 69, false, -1L, 174, 26, "InvalidSyntax", 26 )]
	[InlineData( 87, "282A4E4F5F444F54535441525F414E43484F5229282A5543502941", 133, false, -1L, 175, 26, "InvalidSyntax", 26 )]
	[InlineData( 88, "282A4E4F5F444F54535441525F414E43484F5229FF", 5, true, -1L, -23, 20, "InvalidUtf8Pattern", 20 )]
	[InlineData( 89, "282A4E4F5F444F54535441525F414E43484F5229282A5554462941", 69, true, -1L, 174, 26, "InvalidSyntax", 26 )]
	[InlineData( 90, "282A4E4F5F444F54535441525F414E43484F5229282A5543502941", 133, true, -1L, 175, 26, "InvalidSyntax", 26 )]
	[InlineData( 91, "282A4E4F5F444F54535441525F414E43484F522941", 16, true, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( 92, "282A4E4F5F444F54535441525F414E43484F522941", 16, false, 20L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 93, "282A4E4F5F444F54535441525F414E43484F522941", 65552, true, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( 94, "282A4E4F5F444F54535441525F414E43484F522941", 65552, false, 20L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 95, "282A4E4F5F444F54535441525F414E43484F522941", 17, true, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( 96, "282A4E4F5F444F54535441525F414E43484F522941", 17, false, 20L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 97, "282A4E4F5F444F54535441525F414E43484F522941", 65553, true, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( 98, "282A4E4F5F444F54535441525F414E43484F522941", 65553, false, 20L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 99, "282A4E4F5F444F54535441525F414E43484F522941", 21, true, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( 100, "282A4E4F5F444F54535441525F414E43484F522941", 21, false, 20L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 101, "282A4E4F5F444F54535441525F414E43484F522941", 65557, true, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( 102, "282A4E4F5F444F54535441525F414E43484F522941", 65557, false, 20L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 103, "282A4E4F5F444F54535441525F414E43484F522941", 24, true, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( 104, "282A4E4F5F444F54535441525F414E43484F522941", 24, false, 20L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 105, "282A4E4F5F444F54535441525F414E43484F522941", 65560, true, 1L, 192, 0, "InvalidSyntax", 0 )]
	[InlineData( 106, "282A4E4F5F444F54535441525F414E43484F522941", 65560, false, 20L, 188, 0, "PatternLengthLimitExceeded", 0 )]
	[InlineData( 107, "282A4E4F5F444F54535441525F414E43484F5229282A55544629FF", 0, false, -1L, -23, 26, "InvalidUtf8Pattern", 26 )]
	[InlineData( 108, "282A4E4F5F444F54535441525F414E43484F5229282A55544629FF", 0, true, -1L, -23, 26, "InvalidUtf8Pattern", 26 )]
	public void SourcePrefixPreservesDiagnosticPriority( int diagnostic, string hex, int mode, bool enabled, long maximum, int nativeCode, int nativeOffset, string code, int offset ) {
		var source = Convert.FromHexString( hex );
		var native = Assert.Throws<PcrePatternException>( () => Reference( source, mode, enabled, 0, maximum ) );
		Assert.Equal( nativeCode, (int)native.ErrorCode );
		Assert.EndsWith( $" at offset {nativeOffset}.", native.Message );
		var actual = PcreByteCompiler.Compile( source, Options( mode, enabled, 0, 0, maximum ) );
		Assert.True( actual.Diagnostic is not null, $"diagnostic {diagnostic}: expected an error" );
		Assert.Null( actual.Pattern );
		Assert.Equal( code, actual.Diagnostic?.Code.ToString() );
		Assert.Equal( offset, actual.Diagnostic?.ByteOffset );
	}



	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	public void CallerBudgetsRemainStableBeforeSourceIntegration( int mode, bool enabled ) {
		var source = "A"u8.ToArray();
		var native = Reference( source, mode, enabled );
		AssertNative( native, source, 0, 0, "0:1", "" );
		var steps = new List<int>();
		var options = Options( mode, enabled ) with { WorkObserver = steps.Add };
		Compare( native, source, 0, 0, Compile( source, options ).Match( source ), "caller work" );
		AssertBudget( source, source, options, native, steps );
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
	public void PrefixChainsUseTheExistingCallerWorkContract( int mode, bool enabled, int count ) {
		var body = "(?s).*?Z"u8.ToArray();
		var source = Encoding.ASCII.GetBytes( String.Concat( String.Concat( Enumerable.Repeat( Directive, count ) ), Encoding.ASCII.GetString( body ) ) );
		var subject = Encoding.ASCII.GetBytes( new string( 'B', 64 ) );
		var native = Reference( source, mode, enabled );
		AssertNative( native, subject, 0, 0, "", "" );
		var steps = new List<int>();
		var caller = new List<int>();
		var options = Options( mode, enabled ) with { WorkObserver = steps.Add };
		Compare( native, subject, 0, 0, Compile( source, options ).Match( subject ), "source prefix work" );
		Compare( native, subject, 0, 0, Compile( body, Options( mode, true ) with { WorkObserver = caller.Add } ).Match( subject ), "caller body work" );
		Assert.Equal( caller, steps );
		AssertBudget( source, subject, options, native, steps );
	}

	[Theory]
	[InlineData( 5, false )]
	[InlineData( 5, true )]
	[InlineData( 65541, false )]
	[InlineData( 65541, true )]
	public void SourceControlRetainsTheRequestedSplitByteCommitCut( int mode, bool enabled ) {
		var body = "(?s).*?(*COMMIT)A"u8.ToArray();
		var source = Encoding.ASCII.GetBytes( String.Concat( Directive, Encoding.ASCII.GetString( body ) ) );
		var subject = "éA"u8.ToArray();
		var native = Reference( source, mode, enabled );
		AssertNative( native, subject, 1, 0, "", "" );
		var steps = new List<int>();
		var caller = new List<int>();
		var options = Options( mode, enabled ) with { WorkObserver = steps.Add };
		Compare( native, subject, 1, 0, Compile( source, options ).Match( subject, 1 ), "source split cut" );
		Compare( native, subject, 1, 0, Compile( body, Options( mode, true ) with { WorkObserver = caller.Add } ).Match( subject, 1 ), "caller split cut" );
		Assert.Equal( caller, steps );
		AssertBudget( source, subject, options, native, steps, 1 );
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

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 5, false )]
	[InlineData( 5, true )]
	public void CompiledPatternOwnsThePrefixViewPolicyCapturesAndMark( int mode, bool enabled ) {
		var pattern = Encoding.ASCII.GetBytes( String.Concat( Directive, "(?s).*(*MARK:N)(A)" ) );
		var source = new byte[pattern.Length + 2];
		Array.Fill( source, (byte)0xFF );
		pattern.CopyTo( source, 1 );
		var subject = "BA"u8.ToArray();
		var malformed = new byte[] { 0xFF, (byte)'A' };
		var native = Reference( pattern, mode, enabled );
		AssertNative( native, subject, 0, 0, "0:2,1:1", "4E" );
		if ( mode == 5 ) { AssertNative( native, malformed, 0, 0, "1:1,1:1", "4E" ); }
		var options = Options( mode, enabled );
		var actual = PcreByteCompiler.Compile( source.AsMemory( 1, pattern.Length ), options );
		Assert.True( actual.IsSuccess, actual.Diagnostic?.Message );
		var compiled = Assert.IsType<PcreCompiledBytePattern>( actual.Pattern );
		Array.Fill( source, (byte)'x' );
		typeof( PcreByteOptions ).GetProperty( "NoDotStarAnchor" )!.SetValue( options, !enabled );
		typeof( PcreByteOptions ).GetProperty( "NoStartOptimize" )!.SetValue( options, true );
		Compare( native, subject, 0, 0, compiled.Match( subject ), "owned prefix/view/capture/mark" );
		if ( mode == 5 ) { Compare( native, malformed, 0, 0, compiled.Match( malformed ), "owned unanchored prefix" ); }
	}

	[Theory]
	[InlineData( 0, false )]
	[InlineData( 0, true )]
	[InlineData( 1, false )]
	[InlineData( 1, true )]
	[InlineData( 2, false )]
	[InlineData( 2, true )]
	public void PrefixDoesNotConsumeInstructionCaptureOrDepthCapacity( int selection, bool enabled ) {
		var body = selection switch {
			0 => new string( 'A', 4090 ),
			1 => String.Concat( String.Concat( Enumerable.Repeat( "()", 256 ) ), "A" ),
			_ => String.Concat( String.Concat( Enumerable.Repeat( "(?:", 256 ) ), "A", new string( ')', 256 ) )
		};
		var source = Encoding.ASCII.GetBytes( String.Concat( Directive, "(?s).*?", body ) );
		var subject = Encoding.ASCII.GetBytes( selection == 0 ? body : "A" );
		var spans = selection == 0 ? "0:4090" : selection == 1 ? String.Concat( "0:1,", String.Join( ",", Enumerable.Repeat( "0:0", 256 ) ) ) : "0:1";
		var native = Reference( source, 0, enabled );
		AssertNative( native, subject, 0, 0, spans, "" );
		Compare( native, subject, 0, 0, Compile( source, Options( 0, enabled ) ).Match( subject ), "source prefix capacities" );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void SourcePrefixDoesNotRelaxTheInstructionLimit( bool enabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( Directive, "(?s).*?", new string( 'A', 4091 ) ) );
		var subject = Encoding.ASCII.GetBytes( new string( 'A', 4091 ) );
		AssertNative( Reference( source, 0, enabled ), subject, 0, 0, "0:4091", "" );
		var actual = PcreByteCompiler.Compile( source, Options( 0, enabled ) );
		Assert.Null( actual.Pattern );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded, actual.Diagnostic?.Code );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void PrefixedPatternKeepsValidationPriority( bool enabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( Directive, "(?s).*A" ) );
		var options = Options( 1, enabled );
		var native = Reference( source, 1, enabled );
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
	[InlineData( false )]
	[InlineData( true )]
	public void RejectedPrefixMemoryIsNeverRead( bool enabled ) {
		var source = Encoding.ASCII.GetBytes( String.Concat( Directive, "A" ) );
		var length = Assert.Throws<PcrePatternException>( () => Reference( source, 0, enabled, 0, 1 ) );
		Assert.Equal( 188, (int)length.ErrorCode );
		var conflict = Assert.Throws<PcrePatternException>( () => Reference( source, 48, enabled, 0, 1 ) );
		Assert.Equal( 192, (int)conflict.ErrorCode );
		using var memory = new UnreadablePatternMemory();
		var options = Options( 0, enabled, 0, 0, 1 );
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( memory.Pattern, options, cancellation.Token ) );
		Assert.Equal( PcreByteDiagnosticCode.InvalidWorkLimit, PcreByteCompiler.Compile( memory.Pattern, options with { MatchWorkLimit = 0 } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidSyntax, PcreByteCompiler.Compile( memory.Pattern, options with { Literal = true, Multiline = true } ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.PatternLengthLimitExceeded, PcreByteCompiler.Compile( memory.Pattern, options ).Diagnostic?.Code );
		Assert.False( memory.ReadRequested );
	}
	private sealed class UnreadablePatternMemory : MemoryManager<byte> {
		internal bool ReadRequested { get; private set; }
		internal ReadOnlyMemory<byte> Pattern => CreateMemory( Directive.Length + 1 );
		public override Span<byte> GetSpan() {
			ReadRequested = true;
			throw new InvalidOperationException( "rejected prefix memory must not be read" );
		}
		public override MemoryHandle Pin( int elementIndex = 0 ) => throw new NotSupportedException();
		public override void Unpin() { }
		protected override void Dispose( bool disposing ) { }
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
