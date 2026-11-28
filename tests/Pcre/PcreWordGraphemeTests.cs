using System.Globalization;
using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordGraphemeTests {
	public static IEnumerable<object[]> Cases {
		get {
			var patterns = new[] {
				@"(\X)", @"\G(\X)", @"\A(\X)\z", @"\X\x{301}", @"\X\n", @"(\X?)(\X)",
				@"(\X*)\X\z", @"(\X+)\X\z", @"(\X+?)\X\z", @"(\X*+)\X\z",
				@"(\X{2})", @"(\X{0,3})\X\z", @"(\X{1,3})\X", @"(\X{2,4})\X", @"(\X{2,})\X", @"(\X{1,3}?)\X", @"(\X){2}", @"(?:(\X)Z|(\X))",
				@"(?=(\X))\1", @"(?=(\X)(*ACCEPT))\1", @"(?:\X(*ACCEPT)|a)", @"(\X)\1", @"(?(DEFINE)(?<x>\X))(?&x)(?&x)",
				@"(?(?=\X)(\X)|a)", @"(?*(\X+))\1a", @"(?<=a(?=(\X)))",
				@"a\K(\X)", @"(?>\X+)\X", @"(?:\X|a)+Z", @"(?:(\X)?)*Z",
				@"\X(*SKIP)(*F)|(\X)", @"(*MARK:M)\X(*SKIP:M)(*F)|(\X)",
				@"\X(*SKIP:missing)Z|(\X)", @"\X(*PRUNE)Z|(\X)", @"(?:\X(*THEN)Z|(\X))",
				@"a\X(*COMMIT)Z|(\X)", @"(?s)(?i)(\X)", @"(*CRLF)\X?\nx",
				@"(*ANYCRLF)(\X)\Xx", @"(*ANY)(\X)x", @"\A\X{0}a\z"
			};
			var subjects = new[] { "", "a", "a\u0301", "\u0300\u0301", "a\u0301b\u0308c", "\r\nx", "a\r\nZ",
				"👩🏽‍💻", "👩\u0301‍💻", "👩‍\u0301💻", "🇦🇧🇨🇩", "🇦\u0301🇧", "\u0600a", "\u0600😀", "각", "क्ष", "‍©", "©́‍©", "🇦🇧🇨🇩🇪" };
			foreach ( var utf in new[] { false, true } ) {
				foreach ( var pattern in patterns ) {
					foreach ( var subject in subjects ) { yield return new object[] { pattern, subject, utf }; }
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void CapturesRestartsAndRetriesAgreeWithNativePolicies( string source, string subject, bool utf ) =>
		CompareNative( source, subject, utf );

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void GraphemesKeepAtomicEndpoints( bool utf ) {
		CompareNative( @"\G(\X)", "a\u0301", utf );
		CompareNative( @"\A(\X)\x{301}\z", "a\u0301", utf );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void BreakRepresentativesAndUnicodeBoundariesKeepNativeWordSpans( bool utf, bool ucp ) {
		var representatives = new[] { 0xD, 0xA, 0, 0x300, 0x600, 0x903, 0x1100, 0x1160, 0x11A8,
			0xAC00, 0xAC01, 0x1F1E6, 0x20, 0x200D, 0xA9 };
		foreach ( var left in representatives ) {
			foreach ( var right in representatives ) {
				CompareNative( @"\G(\X)", String.Concat( Char.ConvertFromUtf32( left ), Char.ConvertFromUtf32( right ) ), utf, ucp, false );
			}
		}
		foreach ( var line in PcreGraphemeFixtureData.Boundaries.Split( '\n', StringSplitOptions.RemoveEmptyEntries ) ) {
			var scalar = Int32.Parse( line.Split( ';' )[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture );
			var text = Char.ConvertFromUtf32( scalar );
			// A supplementary scalar projects into two independent raw units.
			foreach ( var subject in new[] { String.Concat( text, "\u0301" ), String.Concat( "\u0600", text ) } ) {
				CompareOne( @"\G(\X)", subject, utf, ucp );
			}
		}
	}

	[Theory]
	[InlineData( "🇦🇧🇨🇩🇪" )]
	[InlineData( "🇦🇧\u0301🇨🇩🇪" )]
	[InlineData( "👩\u0301‍💻\u0308" )]
	[InlineData( "👩‍\u0301💻" )]
	[InlineData( "\ud800a\u0301\udfff\u0308" )]
	public void OriginalPrefixAndRawSurrogatesKeepNativeContext( string subject ) {
		foreach ( var pattern in new[] { @"\G(\X)", @"\G(\X+)\X", @"(?<=.(?=(\X)))", @"(?=(\X))\1" } ) {
			CompareNative( pattern, subject, false );
			if ( !subject.Contains( '\ud800' ) ) { CompareNative( pattern, subject, true ); }
		}
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void ClusterScansAndWholeClusterRetriesShareWorkAndCancellation( bool utf, bool interpreted ) {
		var policy = new PcreMatchPolicy { Anchored = interpreted };
		foreach ( var (pattern, subject) in new[] {
			(@"\A(\X+)\X\z", String.Concat( "a", new string( '\u0301', 32 ), "b\u0308c" )),
			(@"\G(\X+?)\X\z", "🇦🇧🇨🇩🇪"),
			(@"\G(\X*)Z", "🇦🇧🇨🇩🇪"),
			(@"(?*(\X+))\1a", "b\u0308b\u0308a")
		} ) {
			var work = 0;
			var options = new PcreWordOptions { Utf16 = utf, WorkObserver = step => work = step };
			var observed = Compile( pattern, options ).Match( subject.AsMemory(), policy );
			Assert.True( observed.IsSuccess );
			var exact = Compile( pattern, options with { WorkObserver = null, MatchWorkLimit = work } ).Match( subject.AsMemory(), policy );
			Assert.True( exact.IsSuccess ); Assert.Equal( Spans( observed.Match ), Spans( exact.Match ) );
			Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded,
				Compile( pattern, options with { WorkObserver = null, MatchWorkLimit = work - 1 } ).Match( subject.AsMemory(), policy ).Diagnostic?.Code );
			for ( var stop = 1; stop <= work; stop++ ) {
				using var cancellation = new CancellationTokenSource();
				var cancelAt = stop;
				var compiled = Compile( pattern, options with { WorkObserver = step => { if ( step == cancelAt ) { cancellation.Cancel(); } } } );
				Assert.Throws<OperationCanceledException>( () => compiled.Match( subject.AsMemory(), policy, cancellationToken: cancellation.Token ) );
				Assert.Equal( Spans( observed.Match ), Spans( compiled.Match( subject.AsMemory(), policy ).Match ) );
			}
		}
	}

	[Fact]
	public void LongRunsAndGreedyRetryUseLinearWorkAndOneRepeatFrame() {
		foreach ( var utf in new[] { false, true } ) {
			var previous = 0;
			foreach ( var size in new[] { 512, 1024, 2048 } ) {
				var work = 0;
				var text = String.Concat( Enumerable.Repeat( "🇦", size ) );
				var result = Compile( @"\A(\X+)\X\z", new() { Utf16 = utf, MatchWorkLimit = 100_000, WorkObserver = step => work = step } ).Match( text.AsMemory() );
				Assert.True( result.IsSuccess, result.Diagnostic?.ToString() ); Assert.NotNull( result.Match );
				Assert.Equal( text.Length, result.Match.Captures[0]?.CodeUnitLength );
				if ( previous > 0 ) { Assert.True( work < previous * 3, $"{previous} -> {work}" ); }
				previous = work;
			}
		}
	}

	[Theory]
	[InlineData( @"\G(\X)", "a\u0301👩🏽‍💻" )]
	[InlineData( @"\G(\X+)\X\z", "🇦🇧🇨🇩" )]
	[InlineData( @"\A(?:(\X)Z|(\X))\z", "👩🏽‍💻" )]
	[InlineData( @"\A(\X){2}\z", "각a\u0301" )]
	public void SharedUnicodeMatchesRetainTheirNativeCaptureCoordinates( string pattern, string subject ) {
		var bytes = Encoding.UTF8.GetBytes( subject );
		var bytePattern = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.True( bytePattern.IsSuccess );
		var byteResult = bytePattern.Pattern!.Match( bytes );
		var wordResult = Compile( pattern, new() { Utf16 = true } ).Match( subject.AsMemory() );
		Assert.True( byteResult.IsSuccess ); Assert.True( wordResult.IsSuccess );
		Assert.NotNull( byteResult.Match ); Assert.NotNull( wordResult.Match );
		var byteSpans = String.Join( ",", byteResult.Match.Captures.Select( capture => capture is { } value
			? FormattableString.Invariant( $"{value.ByteIndex}:{value.ByteLength}" ) : "-" ) );
		var map = new PcreDualWidthCoordinates( bytes, subject );
		Assert.Equal( map.ByteCaptures( byteSpans ), map.WordCaptures( Spans( wordResult.Match ) ) );
	}

	[Fact]
	public void NativeValidationWindowAndSourceAdmissionRemainExplicit() {
		var subject = "\ud800🇦🇧🇨";
		foreach ( var start in new[] { 1, 3 } ) {
			var expected = PcreNative16Oracle.ExecuteSequencePolicies( Words( @"\G(\X)" ), Words( subject ), true, (uint)start );
			var compiled = Compile( @"\G(\X)", new() { Utf16 = true } );
			for ( var mask = 0; mask < expected.Length; mask++ ) {
				var actual = compiled.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
				Assert.True( actual.IsSuccess, actual.Diagnostic?.ToString() ); Assert.True( expected[mask].Kind is "M" or "N" );
				Assert.Equal( expected[mask].Kind == "M", actual.Match is not null );
				Assert.Equal( expected[mask].Captures, Spans( actual.Match ) );
			}
		}
		foreach ( var source in new[] { @"\X{65}", @"\X{0,65}", @"\X{65,}", @"(*UTF)\X" } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
		}
		foreach ( var source in new[] { @"(*ACCEPT)\X", @"\X{0}a", @"(*FAIL)\X", @"a(*THEN)\X|a" } ) {
			Assert.True( PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).IsSuccess, source );
		}
	}

	[Fact]
	public async Task SlicedViewsAndOwnedCapturesSurviveReuseAndMalformedSentinels() {
		var source = @"\G(?<x>\X+)(*MARK:done)".ToCharArray();
		var compiled = Compile( source.AsMemory(), new() { Utf16 = true } );
		Array.Fill( source, 'z' );
		var subject = "\ud800_a\u0301👩🏽‍💻_\udfff".ToCharArray();
		var view = subject.AsMemory( 2, subject.Length - 4 );
		var saved = compiled.Match( view ).Match;
		Assert.NotNull( saved ); Assert.Equal( view.Length, saved.Captures[1]?.CodeUnitLength ); Assert.Equal( "done", saved.MarkName );
		var spans = Spans( saved );
		Array.Fill( subject, 'z' );
		var results = await Task.WhenAll( Enumerable.Range( 0, 32 ).Select( index => Task.Run( () =>
			compiled.Match( ( index % 2 == 0 ? "a\u0301👩🏽‍💻" : "" ).AsMemory() ) ) ) );
		for ( var index = 0; index < results.Length; index++ ) {
			Assert.True( results[index].IsSuccess ); Assert.Equal( index % 2 == 0, results[index].Match is not null );
		}
		Assert.Equal( spans, Spans( saved ) ); Assert.Equal( "done", saved.MarkName );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Subject, compiled.Match( "a\u0301\udfff".AsMemory() ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidStartOffset, compiled.Match( "😀".AsMemory(), 1 ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Pattern,
			PcreWordSequenceCompiler.CompileSequence( "\\X\ud800".AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
		foreach ( var rejected in new[] { @"[\X]", @"(?<=\X)a" } ) {
			Assert.False( PcreWordSequenceCompiler.CompileSequence( rejected.AsMemory() ).IsSuccess );
			Assert.Equal( "E", PcreNative16Oracle.ExecuteSequence( Words( rejected ), Words( "a" ), false, 0 ).Kind );
		}
	}

	private static void CompareOne( string source, string subject, bool utf, bool ucp ) {
		var expected = PcreNative16Oracle.ExecuteSequence( Words( source ), Words( subject ), utf, 0, ucp: ucp );
		var actual = Compile( source, new() { Utf16 = utf, Ucp = ucp } ).Match( subject.AsMemory() );
		Assert.Equal( "M", expected.Kind ); Assert.True( actual.IsSuccess ); Assert.Equal( expected.Captures, Spans( actual.Match ) );
	}

	private static PcreCompiledWordPattern Compile( string source, PcreWordOptions options ) => Compile( source.AsMemory(), options );
	private static PcreCompiledWordPattern Compile( ReadOnlyMemory<char> source, PcreWordOptions options ) {
		var result = PcreWordSequenceCompiler.CompileSequence( source, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.ToString() );
		return result.Pattern!;
	}

	private static void CompareNative( string source, string subject, bool utf, bool ucp = false, bool allDefaults = true ) {
		foreach ( var defaults in allDefaults ? new[] { 0, 1, 2, 4, 8, 16, 32, 63 } : new[] { 0 } ) {
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
			} );
			Assert.True( compiled.IsSuccess, String.Concat( source, ": ", compiled.Diagnostic ) );
			for ( var start = 0; start <= subject.Length; start++ ) {
				if ( utf && start < subject.Length && Char.IsLowSurrogate( subject[start] ) ) { continue; }
				var native = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start, defaults, ucp );
				for ( var mask = 0; mask < native.Length; mask++ ) {
					var expected = native[mask];
					Assert.True( expected.Kind is "M" or "N", $"Native {source} {start}/{defaults}/{mask}: {expected}" );
					var actual = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
					Assert.True( actual.IsSuccess, actual.Diagnostic?.ToString() );
					var spans = Spans( actual.Match );
					var mark = actual.Match is null ? "" : String.Concat( actual.Match.MarkName.Select( unit => FormattableString.Invariant( $"{(int)unit:X4}" ) ) );
					Assert.True( expected.Captures == spans && expected.Mark == mark && ( expected.Kind == "M" ) == ( actual.Match is not null ),
						$"{source} [{String.Join( " ", Words( subject ).Select( value => value.ToString( "X4", CultureInfo.InvariantCulture ) ) )}] {start}/{defaults}/{mask}: expected {expected.Kind} {expected.Captures}|{expected.Mark}, actual {spans}|{mark}" );
				}
			}
		}
	}

	private static ushort[] Words( string text ) => text.Select( unit => (ushort)unit ).ToArray();
	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( capture => capture is { } value ? FormattableString.Invariant( $"{value.CodeUnitIndex}:{value.CodeUnitLength}" ) : "-" ) );
}
