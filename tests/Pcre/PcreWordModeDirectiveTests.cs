using System.Globalization;
using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordModeDirectiveTests {
	public static IEnumerable<object[]> Cases() {
		var prefixes = new (string Source, bool Utf)[] {
			("(*UTF)", true), ("(*UTF16)", true), ("(*UCP)", false), ("(*UTF)(*UCP)", true),
			("(*UCP)(*UTF16)", true), ("(*UTF16)(*UTF)(*UTF16)", true), ("(*UCP)(*UCP)", false),
			("(*CRLF)(*UTF)", true), ("(*UTF)(*ANYCRLF)(*BSR_ANYCRLF)(*UCP)", true),
			("(*UCP)(*NUL)(*UTF16)", true), ("(*CR)(*UCP)", false),
			("(*BSR_UNICODE)(*UTF16)", true), ("(*LF)(*UTF)(*UCP)(*LF)", true)
		};
		var bodies = new (string Source, string Subject)[] {
			("(.)", "😀a"), (@"\G(\w+)", "éΩ0"), ("(?i)(k)", "K"), (@"\A(\d+)\z", "٥٨"),
			(@"(?aD)\A(\d+)\z", "٥٨"), (@"(?<x>a)\k<x>", "aa"), ("(?<=(.))(a)", "😀a"),
			(@"\G(\X)", "a\u0301"), (@"\A(*sr:(.+))\z", "漢あ"), (@"((?[[\pL] | [a]]))", "😀é"),
			(@"(*MARK:M)(a\K)b", "ab"), (@"\G(a+)a", "aaa"), ("(?m)^(.)$", "x\r\n😀\r\n"),
			(@"(\R)", "\r\n"), (@"[\x09-\x0c]X", "\r\nX")
		};
		foreach ( var callerUtf in new[] { false, true } ) {
			foreach ( var callerUcp in new[] { false, true } ) {
				foreach ( var prefix in prefixes ) {
					foreach ( var body in bodies ) {
						yield return new object[] { String.Concat( prefix.Source, body.Source ), body.Subject,
							callerUtf, callerUcp, callerUtf || prefix.Utf };
					}
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void EffectiveModesScopesAndCapturesAgreeWithNativePolicies( string source, string subject,
		bool callerUtf, bool callerUcp, bool strict ) => CompareNative( source, subject, callerUtf, callerUcp, effectiveUtf: strict );

	[Theory]
	[InlineData( "(*UTF)", true )]
	[InlineData( "(*UTF16)", true )]
	[InlineData( "(*UCP)", false )]
	public void PrefixSelectsEffectiveModeBeforeExecution( string prefix, bool strict ) {
		CompareNative( String.Concat( prefix, "(.)" ), "😀a", false, effectiveUtf: strict );
	}

	[Theory]
	[InlineData( "(*UTF)" )]
	[InlineData( "(*UTF16)" )]
	[InlineData( "(*CRLF)(*UCP)(*UTF16)" )]
	public void EffectiveUtfValidatesTheCompleteOwnedSourceBeforeSyntax( string prefix ) {
		foreach ( var suffix in new[] { "\ud800", "a\udfff", "\ud800(", "(?#\ud800)", "\\Q\ud800\\E",
			"(?:\ud800){0}", "(?(DEFINE)(?<x>\ud800))a", "(*ACCEPT)\ud800" } ) {
			var source = String.Concat( prefix, suffix );
			var offset = source.IndexOfAny( ['\ud800', '\udfff'] );
			var native = PcreNative16Oracle.ExecuteSequence( Words( source ), [], false, 0 );
			Assert.Equal( "E", native.Kind ); Assert.InRange( native.ErrorCode, -26, -24 );
			Assert.Equal( offset, (int)native.ErrorOffset );
			var actual = PcreWordSequenceCompiler.CompileSequence( source.AsMemory() );
			Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Pattern, actual.Diagnostic?.Code );
			Assert.Equal( offset, actual.Diagnostic?.CodeUnitOffset );
		}
		var invalid = String.Concat( prefix, "\ud800" ).AsMemory();
		Assert.Equal( PcreWordDiagnosticCode.InvalidWorkLimit,
			PcreWordSequenceCompiler.CompileSequence( invalid, new() { MatchWorkLimit = 0, MaxPatternLength = 0 } ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.PatternLengthLimitExceeded,
			PcreWordSequenceCompiler.CompileSequence( invalid, new() { MaxPatternLength = 0 } ).Diagnostic?.Code );
		using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreWordSequenceCompiler.CompileSequence( invalid, cancellationToken: cancellation.Token ) );
	}

	[Theory]
	[InlineData( "(*UTF)" )]
	[InlineData( "(*UTF16)" )]
	public void EffectiveUtfOwnsSubjectValidationAndCallerStartUnits( string prefix ) {
		var compiled = Compile( String.Concat( prefix, "(.)" ), new() );
		foreach ( var subject in new[] { "\ud800", "a\udfff", "\ud800a" } ) {
			var native = PcreNative16Oracle.ExecuteSequence( Words( String.Concat( prefix, "(.)" ) ), Words( subject ), false, 0 );
			Assert.Equal( "X", native.Kind ); Assert.InRange( native.ErrorCode, -26, -24 );
			Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Subject, compiled.Match( subject.AsMemory() ).Diagnostic?.Code );
		}
		var start = PcreNative16Oracle.ExecuteSequence( Words( String.Concat( prefix, "(.)" ) ), Words( "😀" ), false, 1 );
		Assert.Equal( -36, start.ErrorCode );
		Assert.Equal( PcreWordDiagnosticCode.InvalidStartOffset, compiled.Match( "😀".AsMemory(), 1 ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidStartOffset, compiled.Match( "a".AsMemory(), -1 ).Diagnostic?.Code );
		var padded = "\ud800😀\udfff".ToCharArray();
		Assert.Equal( "0:2,0:2", Spans( compiled.Match( padded.AsMemory( 1, 2 ) ).Match ) );
	}

	[Theory]
	[InlineData( "(*UTF8)a", 0 )]
	[InlineData( "(*UTF32)a", 0 )]
	[InlineData( "a(*UTF)", 1 )]
	[InlineData( "(?i)(*UCP)a", 4 )]
	[InlineData( "(*UTF)(*NO_JIT)a", 6 )]
	[InlineData( "(*UCP)(*NOTEMPTY)a", 6 )]
	public void OtherWidthAliasesMisplacedAndDeferredPoliciesStayExplicit( string source, int offset ) {
		foreach ( var utf in new[] { false, true } ) {
			var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf } );
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax, result.Diagnostic?.Code );
			Assert.Equal( offset, result.Diagnostic?.CodeUnitOffset );
		}
	}

	[Fact]
	public void RawUcpAndLiteralEntryKeepTheirSeparateDomains() {
		var options = new PcreWordOptions();
		CompareNative( "(*UCP)(\ud800)", "\ud800", false );
		Assert.Equal( "0:1,0:1", Spans( Compile( "(*UCP)(.)", options ).Match( "\ud800".AsMemory() ).Match ) );
		Assert.Equal( "0:2,0:2", Spans( Compile( "(*UTF)(.)", options ).Match( "😀".AsMemory() ).Match ) );
		Assert.Equal( "0:1,0:1", Spans( Compile( "(.)", options ).Match( "😀".AsMemory() ).Match ) );
		Assert.False( options.Utf16 ); Assert.False( options.Ucp );
		foreach ( var source in new[] { "(*UTF)", "(*UTF16)(*UCP)", "(*UTF)\ud800", @"\Q(*UTF)\E" } ) {
			var compiled = PcreWordLiteralCompiler.CompileLiteral( source.AsMemory(), options );
			Assert.True( compiled.IsSuccess ); Assert.NotNull( compiled.Pattern!.Match( source.AsMemory() ).Match );
		}
		CompareNative( @"\Q(*UTF)\E", "(*UTF)", false );
		CompareNative( "(?#(*UTF)a", "a", false );
		CompareNative( "[(*UTF)]", "U", false );
	}

	[Theory]
	[InlineData( @"\d", "٥", "AsciiBsD", "aD" )]
	[InlineData( @"\s", "\u00a0", "AsciiBsS", "aS" )]
	[InlineData( @"\w", "Ω", "AsciiBsW", "aW" )]
	[InlineData( "[[:alpha:]]", "Ω", "AsciiPosix", "aP" )]
	[InlineData( "[[:digit:]]", "٥", "AsciiDigit", "aT" )]
	public void SourceUcpRetainsCallerRestrictionsAndLexicalOverrides( string body, string subject, string member, string scope ) {
		var options = member switch {
			"AsciiBsD" => new PcreWordOptions { AsciiBsD = true },
			"AsciiBsS" => new PcreWordOptions { AsciiBsS = true },
			"AsciiBsW" => new PcreWordOptions { AsciiBsW = true },
			"AsciiPosix" => new PcreWordOptions { AsciiPosix = true },
			_ => new PcreWordOptions { AsciiDigit = true }
		};
		foreach ( var prefix in new[] { "(*UCP)", "(*UTF16)(*UCP)" } ) {
			foreach ( var local in new[] { "", String.Concat( "(?-", scope, ")" ) } ) {
				var source = String.Concat( prefix, local, body );
				var reference = String.Concat( prefix, "(?", scope, ")", local, body );
				var native = PcreNative16Oracle.ExecuteSequence( Words( reference ), Words( subject ), false, 0 );
				var actual = Compile( source, options ).Match( subject.AsMemory() );
				Assert.True( native.Kind is "M" or "N" ); Assert.True( actual.IsSuccess );
				Assert.Equal( native.Captures, Spans( actual.Match ) );
			}
		}
	}

	[Theory]
	[InlineData( "(*UTF)😀\\", 8 )]
	[InlineData( "(*UTF16)(*UCP)😀\\", 16 )]
	[InlineData( "(*UCP)😀\\", 8 )]
	public void ModePrefixesRetainOriginalSyntaxOffsets( string source, int offset ) {
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory() );
		Assert.Equal( PcreWordDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.CodeUnitOffset );
	}

	[Theory]
	[InlineData( "(*UTF16)(*UCP)", "\\G(\\w+)\\z", "éΩ" )]
	[InlineData( "(*UTF)", "\\G(\\X)\\z", "a\u0301" )]
	[InlineData( "(*UTF16)", "\\G(?:(a)\\KX|(ab))\\z", "ab" )]
	public void EffectiveModesShareExactWorkCancellationAndReuse( string prefix, string body, string subject ) {
		var source = String.Concat( prefix, body );
		var work = 0;
		var options = new PcreWordOptions { WorkObserver = step => work = step };
		var observed = Compile( source, options ).Match( subject.AsMemory() );
		Assert.True( observed.IsSuccess ); Assert.NotNull( observed.Match );
		var exact = Compile( source, options with { WorkObserver = null, MatchWorkLimit = work } ).Match( subject.AsMemory() );
		Assert.True( exact.IsSuccess ); Assert.Equal( Spans( observed.Match ), Spans( exact.Match ) );
		Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded,
			Compile( source, options with { WorkObserver = null, MatchWorkLimit = work - 1 } ).Match( subject.AsMemory() ).Diagnostic?.Code );
		for ( var stop = 1; stop <= work; stop++ ) {
			using var cancellation = new CancellationTokenSource(); var cancelAt = stop;
			var compiled = Compile( source, options with { WorkObserver = step => { if ( step == cancelAt ) { cancellation.Cancel(); } } } );
			Assert.Throws<OperationCanceledException>( () => compiled.Match( subject.AsMemory(), cancellationToken: cancellation.Token ) );
			Assert.Equal( Spans( observed.Match ), Spans( compiled.Match( subject.AsMemory() ).Match ) );
		}
	}

	[Fact]
	public async Task SourceFlagsNamesViewsAndResultsSurviveConcurrentReuse() {
		const string pattern = @"(*UTF16)(*UCP)\G(?<x>\w+)\z(*MARK:done)";
		var source = String.Concat( "\ud800", pattern, "\udfff" ).ToCharArray();
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory( 1, pattern.Length ) );
		Assert.True( result.IsSuccess ); var compiled = result.Pattern!;
		Array.Fill( source, 'z' );
		var subject = "\ud800_𐐀𐐁_\udfff".ToCharArray();
		var saved = compiled.Match( subject.AsMemory( 2, 4 ) ).Match;
		Assert.NotNull( saved ); Assert.Equal( "0:4,0:4", Spans( saved ) ); Assert.Equal( "done", saved.MarkName );
		Array.Fill( subject, 'z' );
		var results = await Task.WhenAll( Enumerable.Range( 0, 32 ).Select( index => Task.Run( () =>
			compiled.Match( ( index % 2 == 0 ? "𐐀𐐁" : "a😀" ).AsMemory() ) ) ) );
		for ( var index = 0; index < results.Length; index++ ) {
			Assert.True( results[index].IsSuccess ); Assert.Equal( index % 2 == 0, results[index].Match is not null );
		}
		Assert.Equal( "0:4,0:4", Spans( saved ) ); Assert.Equal( "done", saved.MarkName );
		Assert.Equal( new PcreWordCapture( 0, 4 ), saved.GetCapture( "x" ) );
	}

	public static IEnumerable<object[]> FormerAdmissions() {
		var sources = new (string Source, string Subject)[] {
			("(*CRLF)(*UTF)^a$", "a"),
			("(*UTF)a", "a"),
			("(*ANYCRLF)(*UTF)\\R", "\r\n"),
			("(*ANY)(*UTF)\\R", "\r\n"),
			("(*ANYCRLF)(*UTF)^a$", "a"),
			("(*ANY)(*UTF)^a$", "a"),
			("(*CR)(*UTF)a", "a"),
			("(*UTF)(*CR)a", "a"),
			("(*NUL)(*UCP)a", "a"),
			("(*CRLF)(*UTF)a|^b", "a"),
			("(*ANY)(*UTF)(?(DEFINE)(?<g>.))(?&g)", "a"),
			("(*ANY)(*UTF)(?(R)a|b)", "b"),
			("(*UTF)\\X", "a"),
		};
		foreach ( var utf in new[] { false, true } ) {
			foreach ( var ucp in new[] { false, true } ) {
				foreach ( var row in sources ) { yield return new object[] { row.Source, row.Subject, utf, ucp }; }
			}
		}
	}

	[Theory]
	[MemberData( nameof( FormerAdmissions ) )]
	public void FormerDirectiveRejectionsNowHaveNativeOutcomes( string source, string subject, bool utf, bool ucp ) =>
		CompareNative( source, subject, utf, ucp, effectiveUtf: utf || source.Contains( "(*UTF)", StringComparison.Ordinal ) );

	private static PcreCompiledWordPattern Compile( string source, PcreWordOptions options ) {
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), options );
		Assert.True( result.IsSuccess, String.Concat( source, ": ", result.Diagnostic ) ); return result.Pattern!;
	}

	[Theory]
	[InlineData( @"(*UTF)(*UCP)\G(?<x>\w+)\z", "é𐐀" )]
	[InlineData( @"(*UCP)(*UTF)\A(\X)\z", "👩🏽‍💻" )]
	[InlineData( @"(*UTF)(*UCP)\A((?[[\pL] | [0-9]]))\z", "Ω" )]
	[InlineData( @"(*ANYCRLF)(*UTF)(\R)(.)", "\r\n😀" )]
	public void SharedModePrefixesKeepIndependentCaptureCoordinates( string source, string subject ) {
		var bytes = Encoding.UTF8.GetBytes( subject );
		var bytePattern = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ) );
		Assert.True( bytePattern.IsSuccess );
		var byteMatch = bytePattern.Pattern!.Match( bytes );
		var wordMatch = Compile( source, new() ).Match( subject.AsMemory() );
		Assert.True( byteMatch.IsSuccess ); Assert.True( wordMatch.IsSuccess );
		Assert.NotNull( byteMatch.Match ); Assert.NotNull( wordMatch.Match );
		var byteSpans = String.Join( ",", byteMatch.Match.Captures.Select( capture => capture is { } value
			? FormattableString.Invariant( $"{value.ByteIndex}:{value.ByteLength}" ) : "-" ) );
		var positions = new PcreDualWidthCoordinates( bytes, subject );
		Assert.Equal( positions.ByteCaptures( byteSpans ), positions.WordCaptures( Spans( wordMatch.Match ) ) );
	}

	private static void CompareNative( string source, string subject, bool utf, bool ucp = false, bool allDefaults = true, bool? effectiveUtf = null ) {
		foreach ( var defaults in allDefaults ? new[] { 0, 1, 2, 4, 8, 16, 32, 63 } : new[] { 0 } ) {
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
			} );
			Assert.True( compiled.IsSuccess, String.Concat( source, ": ", compiled.Diagnostic ) );
			for ( var start = 0; start <= subject.Length; start++ ) {
				if ( ( effectiveUtf ?? utf ) && start < subject.Length && Char.IsLowSurrogate( subject[start] ) ) { continue; }
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
