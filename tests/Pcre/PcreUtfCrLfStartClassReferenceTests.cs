using System.Text;
using Icod.Expressions.Pcre;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

/// <summary>Pins UTF JIT start-class entry independently of explicit CR/LF source metadata.</summary>
public sealed class PcreUtfCrLfStartClassReferenceTests {
	[Theory]
	[InlineData( @"[\x09-\x0c]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0b]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x07-\x0c]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0cA]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c]", "0D0A", 0, "1:1" )]
	[InlineData( @"[\x09-\x0c]AA", "0D0A4141", 0, "1:3" )]
	[InlineData( @"(?i)[\x09-\x0c]a", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?xx)[ \x09 - \x0c ] A", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?<x>[\x09-\x0c])A", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"(?<x>[\x09-\x0c]A)", "0D0A41", 0, "1:2,1:2" )]
	[InlineData( @"(?:[\x09-\x0c])(?:A)", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?>[\x09-\x0c]A)", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:[\x09-\x0c]A|[\x09-\x0b]B)", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?:[A-C]X|[\x09-\x0c]A)", "0D0A41", 0, "1:2" )]
	[InlineData( @"(?|([\x09-\x0c])A|([\x09-\x0b])B)", "0D0A41", 0, "1:2,1:1" )]
	[InlineData( @"[\x09-\x0c]+A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c]+?A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c]++A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c]{1,2}A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c]{2,3}A", "0D0A0B41", 0, "1:3" )]
	[InlineData( @"[\x09-\x0cA]{2,3}A", "0D0A4141", 0, "1:3" )]
	[InlineData( @"(?:[\x09-\x0c]A)+", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0c]{1}A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0e]A", "0D0A41", 0, "" )]
	[InlineData( @"[\x00-\x10ffff]A", "0D0A41", 0, "" )]
	[InlineData( @"[^\r]A", "0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0c\p{L}]A", "0D0A41", 0, "" )]
	[InlineData( @"(?s).A", "0D0A41", 0, "" )]
	[InlineData( @"(?:[\x09-\x0c]A|[\x09-\x0e]B)", "0D0A41", 0, "" )]
	[InlineData( @"[\x09-\x0cA]{2}A", "0D0A4141", 0, "" )]
	[InlineData( @"\G[\x09-\x0c]A", "0D0A41", 0, "" )]
	[InlineData( @"\G[\x09-\x0c]A", "0D0A41", 1, "1:2" )]
	[InlineData( @"(?m)^[\x09-\x0c]A", "0D0A41", 0, "" )]
	[InlineData( @"(?(DEFINE)(?<x>Z))[\x09-\x0c]A", "0D0A41", 0, "" )]
	[InlineData( @"[^\r\r]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\n-\r]A", "0D0A41", 0, "1:2" )]
	[InlineData( @"[\x09-\x0e]A", "0D0A41", 1, "1:2" )]
	public void LeadingClassNativeSpansRemainIndependentOfSourceFlags( string pattern, string hex, int offset, string spans ) {
		var subject = Convert.FromHexString( hex );
		foreach ( var tolerant in new[] { false, true } ) {
			var reference = Reference( pattern, true, tolerant );
			AssertLiteral( reference, subject, offset, spans );
			Compare( reference, subject, offset, Compile( pattern, new() { Utf8 = true,
				MatchInvalidUtf = tolerant, AnyCrLfNewline = true } ).Match( subject, offset ), pattern );
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void CallerOffsetsKeepFragmentsCapturesAndNativeCrLfControls( bool anyCrLf ) {
		foreach ( var pattern in new[] { @"[\x09-\x0c]A", @"[\x09-\x0e]A", @"[^\r]A",
			@"[\x09-\x0c\p{L}]A", @"(?<x>[\x09-\x0c])A", @"(?>[\x09-\x0c]A)",
			@"(?:[\x09-\x0c]A|[\x09-\x0b]B)", @"(?:[\x09-\x0c]A|[\x09-\x0e]B)",
			@"[\x09-\x0c]+A", @"[\x09-\x0c]{1,2}A", @"(?:[\x09-\x0c]A)+",
			@"[\x09-\x0cA]{2}A", @"[\x09-\x0cA]{2,3}A", @"\G[\x09-\x0c]A",
			@"(?<x>[\x09-\x0c]A)\k<x>", @"(?<x>[\x09-\x0c])A(?&x)",
			@"(?<x>[\x09-\x0c])A(?<y>(?(R&y)(?&x)|(?&y)))" } ) {
			foreach ( var hex in new[] { "0D0A41", "FF0D0A41", "0D0AFF41", "C3A90D0A41", "0D0A410D0A41", "0D0A410A", "0D0A4141" } ) {
				var subject = Convert.FromHexString( hex );
				var reference = Reference( pattern, anyCrLf, true );
				var compiled = Compile( pattern, new() { Utf8 = true, MatchInvalidUtf = true, AnyCrLfNewline = anyCrLf } );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					Compare( reference, subject, offset, compiled.Match( subject, offset ), $"{pattern}, {hex}, {offset}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void StrictUtfAndByteControlsRetainTheirOwnCandidateProtocols( bool utf ) {
		var patterns = utf
			? new[] { @"[\x09-\x0c]A", @"[\x09-\x0e]A", @"[^\r]A", @"(?:[\x09-\x0c]A|[\x09-\x0e]B)" }
			: new[] { @"[\x09-\x0c]A", @"[^\r]A", @"[\s\S]A" };
		foreach ( var pattern in patterns ) {
			foreach ( var anyCrLf in new[] { false, true } ) {
				var subject = "é\r\nA\r\nAA"u8.ToArray();
				var reference = Reference( pattern, anyCrLf, false, utf );
				var compiled = Compile( pattern, new() { Utf8 = utf, AnyCrLfNewline = anyCrLf } );
				for ( var offset = 0; offset <= subject.Length; offset++ ) {
					if ( utf && offset == 1 ) { continue; } // Strict caller-boundary error is pinned separately.
					Compare( reference, subject, offset, compiled.Match( subject, offset ), pattern );
				}
			}
		}
		var invalid = Compile( @"[\x09-\x0c]A", new() { Utf8 = true, AnyCrLfNewline = true } );
		Assert.Equal( PcreByteDiagnosticCode.InvalidStartOffset, invalid.Match( "é\r\nA"u8.ToArray(), 1 ).Diagnostic?.Code );
		Assert.Equal( PcreByteDiagnosticCode.InvalidUtf8Subject, invalid.Match( Convert.FromHexString( "FF0D0A41" ) ).Diagnostic?.Code );
	}

	[Fact]
	public void ClassesWithoutLfKeepTheDefaultBudgetForCrlfOnlyInput() {
		const string pattern = @"[A-C]A";
		var subject = Encoding.ASCII.GetBytes( String.Concat( Enumerable.Repeat( "\r\n", 120_000 ) ) );
		Assert.False( Reference( pattern, true, false ).Match( subject ).Success );
		foreach ( var observe in new[] { false, true } ) {
			using var cancellation = new CancellationTokenSource();
			var work = 0;
			var compiled = Compile( pattern, new() { Utf8 = true, AnyCrLfNewline = true,
				WorkObserver = observe ? count => { work = count; } : null } );
			var result = compiled.Match( subject, cancellationToken: observe ? cancellation.Token : default );
			Assert.True( result.IsSuccess, result.Diagnostic?.Message );
			Assert.Null( result.Match );
			if ( observe ) { Assert.InRange( work, 1, PcreByteOptions.DefaultMatchWorkLimit ); }
		}
	}

	[Fact]
	public void ExtraLfCandidateSharesWorkObserverAndCancellation() {
		const string pattern = @"(?<x>[\x09-\x0c])A";
		var subject = "\r\nA"u8.ToArray();
		var steps = new List<int>();
		var observed = Compile( pattern, new() { Utf8 = true, AnyCrLfNewline = true, WorkObserver = steps.Add } );
		var result = observed.Match( subject );
		Assert.Equal( 1, result.Match?.ByteIndex );
		Assert.Equal( Enumerable.Range( 1, steps.Count ), steps );
		foreach ( var observe in new[] { false, true } ) {
			using var token = new CancellationTokenSource();
			var options = new PcreByteOptions { Utf8 = true, AnyCrLfNewline = true,
				MatchWorkLimit = steps.Count - 1, WorkObserver = observe ? _ => { } : null };
			var exhausted = Compile( pattern, options ).Match( subject, cancellationToken: token.Token );
			Assert.Null( exhausted.Match );
			Assert.Equal( PcreByteDiagnosticCode.MatchWorkLimitExceeded, exhausted.Diagnostic?.Code );
			Compare( Reference( pattern, true, false ), subject, 0,
				Compile( pattern, options with { MatchWorkLimit = steps.Count } ).Match( subject, cancellationToken: token.Token ), pattern );
		}
		using var cancellation = new CancellationTokenSource();
		var cancellable = Compile( pattern, new() { Utf8 = true, AnyCrLfNewline = true,
			WorkObserver = count => { if ( count == 4 ) cancellation.Cancel(); } } );
		Assert.Throws<OperationCanceledException>( () => cancellable.Match( subject, cancellationToken: cancellation.Token ) );
		Assert.Throws<OperationCanceledException>( () => observed.Match( subject, cancellationToken: cancellation.Token ) );
	}

	private static void AssertLiteral( PcreRegex8Bit reference, byte[] subject, int offset, string spans ) {
		var match = reference.Match( subject, offset );
		Assert.Equal( spans.Length > 0, match.Success );
		if ( !match.Success ) { return; }
		var groups = spans.Split( ',' );
		Assert.Equal( groups.Length, match.Groups.Count );
		for ( var group = 0; group < groups.Length; group++ ) {
			var capture = match.Groups[group];
			Assert.Equal( groups[group], capture.Success ? $"{capture.Index}:{capture.Length}" : "-" );
		}
	}

	private static void Compare( PcreRegex8Bit reference, byte[] subject, int offset, PcreByteMatchResult actual, string context ) {
		var expected = reference.Match( subject, offset );
		Assert.True( actual.IsSuccess, $"{context}: {actual.Diagnostic?.Message}" );
		Assert.True( ( expected.Success ? expected.Index : -1 ) == ( actual.Match?.ByteIndex ?? -1 ),
			$"{context}: native={ ( expected.Success ? expected.Index : -1 ) }, managed={actual.Match?.ByteIndex ?? -1}" );
		if ( !expected.Success ) { return; }
		Assert.Equal( expected.Length, actual.Match!.ByteLength );
		Assert.Equal( expected.Groups.Count, actual.Match.Captures.Count );
		for ( var group = 0; group < expected.Groups.Count; group++ ) {
			var capture = expected.Groups[group];
			Assert.Equal( capture.Success ? new PcreByteCapture( capture.Index, capture.Length ) : null, actual.Match.Captures[group] );
		}
	}

	private static PcreRegex8Bit Reference( string pattern, bool anyCrLf, bool tolerant, bool utf = true ) => new(
		Encoding.UTF8.GetBytes( ( anyCrLf ? "(*ANYCRLF)" : "(*LF)" ) + pattern ), Encoding.UTF8,
		new PcreRegexSettings { Options = PcreOptions.Compiled | ( utf ? PcreOptions.Utf : PcreOptions.None )
			| ( tolerant ? PcreOptions.MatchInvalidUtf : PcreOptions.None ), ParensLimit = 512 } );

	private static PcreCompiledBytePattern Compile( string pattern, PcreByteOptions options ) {
		var result = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), options );
		Assert.True( result.IsSuccess, $"{pattern}: {result.Diagnostic?.Message}" );
		return Assert.IsType<PcreCompiledBytePattern>( result.Pattern );
	}
}
