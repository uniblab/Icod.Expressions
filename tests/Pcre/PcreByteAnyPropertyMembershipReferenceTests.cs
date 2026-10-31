using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Byte mixed-Any classes preserve pinned native bitmap inclusion and membership.</summary>
public sealed class PcreByteAnyPropertyMembershipReferenceTests {
	[Theory]
	[InlineData( @"[\p{Any}\p{L}]" )]
	[InlineData( @"[\p{L}\p{Any}]" )]
	[InlineData( @"[^\p{Any}\p{L}]" )]
	[InlineData( @"[\P{^Any}\p{L}]" )]
	[InlineData( @"[\p{Any}\P{L}]" )]
	[InlineData( @"[^\p{Any}\P{L}]" )]
	[InlineData( @"[\p{Any}\p{Lu}]" )]
	[InlineData( @"[\p{Any}\P{Any}\p{L}]" )]
	[InlineData( @"[\p{Any}\p{L}\P{L}]" )]
	[InlineData( @"[\P{Any}\p{L}]" )]
	[InlineData( @"[\p{Any}\p{L}A]" )]
	[InlineData( @"[A\p{Any}\p{L}]" )]
	[InlineData( @"[\p{Any}\p{L}A-A]" )]
	[InlineData( @"[\p{Any}\p{L}\x00-\x00]" )]
	[InlineData( @"[\p{Any}\p{L}\Q\E]" )]
	[InlineData( @"[\p{Any}\p{L}\Q!\E]" )]
	[InlineData( @"[\p{Any}\p{L} ]" )]
	[InlineData( @"[\p{Any}\d]" )]
	[InlineData( @"[\p{Any}\D]" )]
	[InlineData( @"[\p{Any}\w]" )]
	[InlineData( @"[\p{Any}\W]" )]
	[InlineData( @"[\p{Any}\s]" )]
	[InlineData( @"[\p{Any}\S]" )]
	[InlineData( @"[\p{Any}[:alpha:]]" )]
	[InlineData( @"[\p{Any}[:lower:]]" )]
	[InlineData( @"[\p{Any}[:ascii:]]" )]
	[InlineData( @"[\p{Any}[:blank:]]" )]
	[InlineData( @"[\p{Any}[:digit:]]" )]
	[InlineData( @"[\p{Any}[:xdigit:]]" )]
	[InlineData( @"[\p{Any}[:graph:]]" )]
	[InlineData( @"[\p{Any}[:punct:]]" )]
	[InlineData( @"[\p{Any}\h\p{L}]" )]
	[InlineData( @"[\p{Any}\v\p{L}]" )]
	[InlineData( @"[\p{Any}\P{Any}]" )]
	public void EveryBytePreservesNativeBitmapPresence( string body ) {
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var mode in new[] { "", "(?i)", "(?ir)", "(?a)", "(?xx)", "(?aD)", "(?aP)", "(?aT)" } ) {
				var pattern = String.Concat( mode, @"\A", body, @"\z" );
				var reference = Reference( pattern, ucp );
				var compiled = Compile( pattern, new() { Ucp = ucp } );
				for ( var value = 0; value < 256; value++ ) {
					var subject = new[] { (byte)value };
					Compare( reference, subject, 0, compiled.Match( subject ), $"{pattern}/{value}/{ucp}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"(*ANYCRLF)[\p{Any}\p{L}]{2}A", "0D0A4141", "" )]
	[InlineData( @"(*ANYCRLF)[\p{Any}\p{L}]{2}A", "21214141", "" )]
	[InlineData( @"(*ANYCRLF)[\p{L}\p{Any}]{2}A", "0D0A4141", "" )]
	[InlineData( @"(*ANYCRLF)[\p{L}\p{Any}]{2}A", "21214141", "" )]
	[InlineData( @"(*ANYCRLF)[^\p{Any}\p{L}]A", "0D0A4141", "1:2" )]
	[InlineData( @"(*ANYCRLF)[^\p{Any}\p{L}]A", "21214141", "1:2" )]
	[InlineData( @"(*ANYCRLF)[^\p{Any}\P{L}]A", "0D0A4141", "2:2" )]
	[InlineData( @"(*ANYCRLF)[^\p{Any}\P{L}]A", "21214141", "2:2" )]
	[InlineData( @"(*ANYCRLF)[\p{Any}\p{L}A]{2}A", "0D0A4141", "0:3" )]
	[InlineData( @"(*ANYCRLF)[\p{Any}\p{L}A]{2}A", "21214141", "0:3" )]
	[InlineData( @"(*ANYCRLF)[\p{Any}\p{L}\Q\E]{2}A", "0D0A4141", "" )]
	[InlineData( @"(*ANYCRLF)[\p{Any}\p{L}\Q\E]{2}A", "21214141", "" )]
	[InlineData( @"(*ANYCRLF)([\p{Any}\p{L}]){2}A", "0D0A4141", "" )]
	[InlineData( @"(*ANYCRLF)([\p{Any}\p{L}]){2}A", "21214141", "" )]
	[InlineData( @"(*ANYCRLF)(?>[\p{Any}\p{L}]{2})A", "0D0A4141", "" )]
	[InlineData( @"(*ANYCRLF)(?>[\p{Any}\p{L}]{2})A", "21214141", "" )]
	[InlineData( @"(*ANYCRLF)(?<x>[\p{Any}\p{L}]{2})(A)", "0D0A4141", "" )]
	[InlineData( @"(*ANYCRLF)(?<x>[\p{Any}\p{L}]{2})(A)", "21214141", "" )]
	[InlineData( @"(*ANYCRLF)\A([\p{Any}\p{L}]+)\1\z", "0D0A4141", "" )]
	[InlineData( @"(*ANYCRLF)\A([\p{Any}\p{L}]+)\1\z", "21214141", "" )]
	[InlineData( @"(*ANYCRLF)(?=([\p{Any}\p{L}]{2}))\1", "0D0A4141", "2:2,2:2" )]
	[InlineData( @"(*ANYCRLF)(?=([\p{Any}\p{L}]{2}))\1", "21214141", "2:2,2:2" )]
	[InlineData( @"(*ANYCRLF)(?<=([\p{Any}\p{L}]))X", "0D0A4141", "" )]
	[InlineData( @"(*ANYCRLF)(?<=([\p{Any}\p{L}]))X", "21214141", "" )]
	[InlineData( @"(*ANYCRLF)(?<![\p{Any}\p{L}])X", "0D0A4141", "" )]
	[InlineData( @"(*ANYCRLF)(?<![\p{Any}\p{L}])X", "21214141", "" )]
	[InlineData( @"(*ANYCRLF)\A(?:(?<a>[\p{Any}\p{L}])x|(?<b>[\p{Any}\p{L}]))\z", "0D0A4141", "" )]
	[InlineData( @"(*ANYCRLF)\A(?:(?<a>[\p{Any}\p{L}])x|(?<b>[\p{Any}\p{L}]))\z", "21214141", "" )]
	[InlineData( @"(*ANYCRLF)\A(?&a)\z(?(DEFINE)(?<a>[\p{Any}\p{L}]+))", "0D0A4141", "" )]
	[InlineData( @"(*ANYCRLF)\A(?&a)\z(?(DEFINE)(?<a>[\p{Any}\p{L}]+))", "21214141", "" )]
	[InlineData( @"\A([\p{Any}\p{L}]+)\1\z", "E9E9", "0:2,0:1" )]
	[InlineData( @"(?<=([\p{Any}\p{L}]))X", "E958", "1:1,0:1" )]
	[InlineData( @"(?<=([\p{Any}\p{L}]))X", "2158", "" )]
	[InlineData( @"(?<![\p{Any}\p{L}])X", "2158", "1:1" )]
	[InlineData( @"\A(?:(?<a>[\p{Any}\p{L}])x|(?<b>[\p{Any}\p{L}]))\z", "E9", "0:1,-,0:1" )]
	[InlineData( @"(*UCP)[\p{Any}\d]", "41", "" )]
	[InlineData( @"(*UCP)(?aD)[\p{Any}\d]", "41", "0:1" )]
	[InlineData( @"(*UCP)[\p{Any}[:digit:]]", "41", "" )]
	[InlineData( @"(*UCP)(?aT)[\p{Any}[:digit:]]", "41", "0:1" )]
	[InlineData( @"(?xx)[\p{Any}\p{L} ]", "21", "" )]
	[InlineData( @"(?xx)[\p{Any}\p{L}\Q \E]", "21", "0:1" )]
	[InlineData( @"[\p{Any}\p{L} ]", "21", "0:1" )]
	[InlineData( @"(?i)[\p{Any}\p{Lu}]", "E9", "0:1" )]
	[InlineData( @"[\P{Any}\p{L}]", "E9", "0:1" )]
	[InlineData( @"[^\p{Any}\p{L}]", "00", "0:1" )]
	public void MixedAnyMembershipPreservesPinnedCaptureSpans( string body, string hex, string spans ) {
		var pattern = body;
		var subject = Convert.FromHexString( hex );
		var reference = Reference( pattern );
		var expected = reference.Match( subject );
		Assert.Equal( spans.Length > 0, expected.Success );
		if ( expected.Success ) {
			var groups = spans.Split( ',' );
			Assert.Equal( groups.Length, expected.Groups.Count );
			for ( var group = 0; group < groups.Length; group++ ) {
				var capture = expected.Groups[group];
				Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
			}
		}
		Compare( reference, subject, 0, Compile( pattern ).Match( subject ), pattern );
	}

	[Theory]
	[InlineData( "(*LF)" )]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CR)" )]
	[InlineData( "(*NUL)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void EveryOffsetRetainsMixedAnySearchAndByteSpans( string newline ) {
		string[] bodies = { @"[\p{Any}\p{L}]A", @"[\p{L}\p{Any}]A", @"[^\p{Any}\p{L}]A", @"[\p{Any}\p{L}]{2}A", @"(?:[\p{Any}\p{L}]){2}A", @"[\p{Any}\p{L}A]{2}A", @"[\P{Any}\p{L}]A", @"[\p{Any}\d]A", @"(?aD)[\p{Any}\d]A", @"(?i)[\p{Any}\p{Lu}]A", @"[\p{Any}\p{L}\Q\E]A", @"[\p{Any}\p{L}\Q!\E]A" };
		foreach ( var ucp in new[] { false, true } ) {
			foreach ( var body in bodies ) {
				var pattern = String.Concat( newline, body );
				var reference = Reference( pattern, ucp );
				var compiled = Compile( pattern, new() { Ucp = ucp } );
				foreach ( var hex in Subjects() ) {
					var subject = Convert.FromHexString( hex );
					for ( var offset = 0; offset <= subject.Length; offset++ ) {
						Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}/{ucp}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( "(*ANYCRLF)" )]
	[InlineData( "(*CRLF)" )]
	[InlineData( "(*ANY)" )]
	public void UtfRepeatControlsRetainScalarAndInvalidFragmentRules( string newline ) {
		foreach ( var body in new[] { @"[\p{Any}\p{L}]{2}A", @"[^\p{Any}\p{L}]A", @"([\p{Any}\p{L}]){2}A", @"[\p{Any}\p{L}A]A" } ) {
			var pattern = String.Concat( newline, body );
			var reference = Reference( pattern, true, true );
			var compiled = Compile( pattern, new() { Utf8 = true, Ucp = true, MatchInvalidUtf = true } );
			foreach ( var hex in new[] { "0D0A4241", "0D0A4141", "C3A90D0A4241", "FF0D0A4241", "0D0AFF41", "EDA0800D0A41" } ) {
				var subject = Convert.FromHexString( hex );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}/{hex}/{offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"\A[\p{Any}\p{L}]\z", "21" )]
	[InlineData( @"\A[^\p{Any}\p{L}]\z", "21" )]
	[InlineData( @"\A([\p{Any}\p{L}]+)\1\z", "E9E9" )]
	[InlineData( @"[\p{Any}\p{L}]{2}A", "0D0A4141" )]
	[InlineData( @"[\p{Any}\p{L}A]{2}A", "0D0A4241" )]
	[InlineData( @"[^\p{Any}\p{L}]A", "0D0A4141" )]
	public void RepeatContinuationSharesObservedExactAndExhaustedBudgets( string body, string hex ) {
		var pattern = String.Concat( "(*ANYCRLF)", body );
		var subject = Convert.FromHexString( hex );
		var steps = new List<int>();
		var result = Compile( pattern, new() { WorkObserver = steps.Add } ).Match( subject );
		Assert.True( result.IsSuccess );
		Compare( Reference( pattern ), subject, 0, result, pattern );
		Assert.NotEmpty( steps );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		foreach ( var observe in new[] { false, true } ) {
			Compare( Reference( pattern ), subject, 0, Compile( pattern, new() { MatchWorkLimit = steps.Count,
				WorkObserver = observe ? _ => { } : null } ).Match( subject ), pattern );
			var exhausted = Compile( pattern, new() { MatchWorkLimit = steps.Count - 1,
				WorkObserver = observe ? _ => { } : null } ).Match( subject );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
		}
	}

	[Fact]
	public void MixedAnyMatchingAndCompilationRemainCancellable() {
		const string pattern = @"[\p{Any}\p{L}]";
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { WorkObserver = count => { if ( count == 1 ) { cancellation.Cancel(); } } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( "!"u8.ToArray(), cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), cancellationToken: cancellation.Token ) );
	}

	[Fact]
	public void DeepMixedAnyWrappersAndRepeatCeilingsRemainShared() {
		var wrapped = String.Concat( "(*ANYCRLF)", String.Concat( Enumerable.Repeat( "(?>", 200 ) ), @"(?<x>[^\p{Any}\p{L}])(A)", new string( ')', 200 ) );
		var subject = "\r\nA"u8.ToArray();
		Compare( Reference( wrapped ), subject, 0, Compile( wrapped ).Match( subject ), wrapped );
		var depth = String.Concat( String.Concat( Enumerable.Repeat( "(?>", 257 ) ), @"[\p{Any}\p{L}]{2}A", new string( ')', 257 ) );
		Assert.Equal( PcreByteDiagnosticCode.PatternDepthLimitExceeded, PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( depth ) ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.PatternRepeatLimitExceeded,
			PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( @"[\p{Any}\p{L}]{4096}A" ) ).Diagnostic?.Code );
	}

	private static string[] Subjects() => new[] { "", "00", "0D0A", "0D0A41", "0D0A4241", "0D0A4141", "0D0A4161",
		"FF0D0A4241", "0D0AFF41", "0D0A2341", "0D0A0941", "C3A90D0A4241", "0D0A42410D0A4141",
		"0D0A4242424242424242424241", "0D0A424242424242424242424241" };

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}: native={( expected.Success ? expected.Index : -1 )}, managed={actual.Match?.ByteIndex ?? -1}" );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool ucp = false, bool utf = false ) => new(
		Encoding.ASCII.GetBytes( pattern ), Encoding.Latin1, new PcreRegexSettings {
			Options = PcreOptions.Compiled | ( ucp ? PcreOptions.Ucp : PcreOptions.None )
				| ( utf ? PcreOptions.Utf | PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions? options = null ) {
		var result = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
