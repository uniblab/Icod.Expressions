using System.Globalization;
using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordScriptRunTests {
	public static IEnumerable<object[]> AdmissionCases => new[] {
		"(*ACCEPT)(*sr:a)",
		"(*ACCEPT)\\X(*sr:a)",
		"(*F)(*asr:a)",
		"(*F)(*sr:a)",
		"(*asr:a(*ACCEPT:N))X",
		"(*asr:a)",
		"(*sr:a)",
		"(?(DEFINE)(*MARK:x)(*asr:a))",
		"(?(DEFINE)(?<g>a(*sr:a)))(?&g){0}",
		"(?(VERSION=10.48)a|(*sr:a))",
		"(?*(?>(*sr:a)a))",
		"(?*(?>(*sr:a)a)){0}",
		"(?*a(*sr:a))",
		"(?:a(*sr:a))*",
		"(?:a(*sr:a)|b)",
		"(?<!a(*asr:a))",
		"(?<=(?>(*sr:a)a)){0}",
		"(?=(*sr:a)){0}",
		"(?=(?>(*sr:a)a)){0}",
		"(?=a(*asr:a))",
		"(?=a(*sr:a)){0}",
		"(?>(*sr:a)a)",
		"(?>(*sr:a)a)*",
		"(?>(*sr:a)a){2}",
		"(?>(*sr:a)a*)b*",
		"(?>(*sr:a)a*)b*c",
		"(?>(*sr:a)a*)b*c*d",
		"(?>(*sr:a)a*)b*c*d*",
		"(?>(*sr:a)a*)b?",
		"(?>(*sr:a)a*)b?c*",
		"(?>(*sr:a)a*)b?c*d*",
		"(?>(*sr:a)a+)b*c*d*e*",
		"(?>(*sr:a)a?)b?",
		"(?>(*sr:a)a|b)",
		"(?>a(*sr:a)){0}",
		"(a)(*sr:a)",
		"(a)(?(1)b(*sr:a)|c)",
		"(a)(?1)(*sr:a)",
		"(a)(?<=a)(?1)(*sr:a)",
		"(a)(?>(?1)(*sr:a))",
		"\\X(*sr:a)",
	}.SelectMany( source => new[] { new object[] { source, false }, new object[] { source, true } } );

	[Theory]
	[MemberData( nameof( AdmissionCases ) )]
	public void FormerlyUnimplementedShapesKeepNativeCapturesAfterAdmission( string source, bool utf ) {
		CompareNative( source, "aaaβ", utf );
	}

	public static IEnumerable<object[]> Cases {
		get {
			var patterns = new[] {
				@"(*sr:([\s\S]*))", @"\G(*sr:(.+))", @"\A(*asr:(.*))\z",
				@"\A(*sr:.*)a\z", @"\A(*asr:.*)a\z", @"\A(*sr:.*)β\z", @"\A(?>(*sr:.*))β\z",
				@"\A(*script_run:(aβ)|(a))β\z", @"\A(*atomic_script_run:(aβ)|(a))β\z",
				@"\A(?:(*sr:(aβ))|(aβ))\z", @"\A(?<r>(*sr:a(?&r)?|β))\z",
				@"\A(?(DEFINE)(?<r>(*sr:.*)))(?&r)\z", @"\A(*sr:(?<x>a))(?1)\z",
				@"\A(*sr:(?=(aβ)))(aβ)\z", @"(?<=(*sr:(a)))x", @"(?<=(*sr:(aβ)))x",
				@"(?<!(*sr:(aβ)))x", @"(?<=(*sr:(a{1,2})))x", @"\A(*sr:(*sr:a)β)\z",
				@"\A(*sr:.*)\Kβ\z", @"\A(*sr:a\Kβ)\z", @"\Aa\K(*sr:β)\z",
				@"\A(*sr:a){2,4}?\z", @"\A(*asr:a|β)+\z", @"\A(*sr:a|β)+\z", @"(*sr:)*a",
				@"\A(*sr:(aβ)(*ACCEPT))\z", @"\A(*asr:(aβ)(*ACCEPT))\z",
				@"(?=(*sr:(aβ)(*ACCEPT)))\1", @"(?(DEFINE)(?<r>(*sr:aβ(*ACCEPT))))(?&r)",
				@"(*sr:a(*MARK:M)(*SKIP:M)X)|(a)", @"(*sr:a(*PRUNE)X)|(a)",
				@"(*sr:a(*THEN)X|(a))", @"(*sr:a(*COMMIT)X)|(aβ)",
				@"(*sr:\A.(*SKIP:missing)X|\A.)", @"(*asr:\A.(*SKIP:missing)X|\A.)",
				@"(*CRLF)(*sr:[\r\n])x", @"(*ANYCRLF)(*sr:[\x09-\x0c]X)",
				@"(*ANY)(*sr:(?s:.{2})X)", @"(?i)\A(*sr:µm)\z", @"\A(*sr:\X+)\z"
			};
			var subjects = new[] { "", "a", "aa", "ab", "aβ", "βa", "aβx", "aX", "aа", "\r\nx", "\r\nbX",
				"0０", "٠۰", "019", "漢あ가", "あ漢가", "漢ㄅあ", "漢가ㄅ", "😀", "😀a", "a\u0301", "a\u0301β",
				"𐐀𐐁", "a\0b", "\ue000", "a\ue000", "🇦🇧", "µm", "μm" };
			foreach ( var utf in new[] { false, true } ) {
				foreach ( var pattern in patterns ) {
					foreach ( var subject in subjects ) { yield return new object[] { pattern, subject, utf }; }
				}
				foreach ( var alias in new[] { "sr", "script_run", "asr", "atomic_script_run" } ) {
					foreach ( var subject in subjects ) { yield return new object[] { String.Concat( @"\A(*", alias, @":([\s\S]*))\z" ), subject, utf }; }
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void CapturesNestedScopesAndSearchAgreeWithNativePolicies( string source, string subject, bool utf ) =>
		CompareNative( source, subject, utf );

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void RunsValidateConsumedWordSpans( bool utf ) {
		CompareNative( @"\A(*sr:(.+))\z", "ab", utf );
		CompareNative( @"\A(*sr:(.+))\z", "aβ", utf );
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void ScriptDescriptorsAndBoundariesKeepNativeUnitClassification( bool utf, bool ucp ) {
		const string pattern = @"\A(*sr:([\s\S]*))\z";
		var compiled = Compile( pattern, new() { Utf16 = utf, Ucp = ucp } );
		var source = Words( pattern );
		foreach ( var scalar in PcreScriptRunFixtureData.Boundaries.Concat( PcreScriptRunFixtureData.Representatives ).Distinct() ) {
			var text = Char.ConvertFromUtf32( scalar );
			CompareOne( source, compiled, text, utf, ucp );
			foreach ( var context in new[] { "a", "漢", "\u0300" } ) {
				CompareOne( source, compiled, String.Concat( text, context ), utf, ucp );
				CompareOne( source, compiled, String.Concat( context, text ), utf, ucp );
			}
		}
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void AllDecimalSetPairsKeepNativeZeroAndNineIdentities( bool utf, bool ucp ) {
		const string pattern = @"\A(*sr:([\s\S]*))\z";
		var compiled = Compile( pattern, new() { Utf16 = utf, Ucp = ucp } );
		var source = Words( pattern );
		foreach ( var first in PcreScriptRunFixtureData.DigitZeros ) {
			foreach ( var second in PcreScriptRunFixtureData.DigitZeros ) {
				for ( var variant = 0; variant < 4; variant++ ) {
					var subject = String.Concat( Char.ConvertFromUtf32( first + ( variant & 1 ) * 9 ),
						Char.ConvertFromUtf32( second + ( ( variant >> 1 ) & 1 ) * 9 ) );
					CompareOne( source, compiled, subject, utf, ucp );
				}
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ContextTriplesKeepDirectionalHanTransitions( bool utf ) {
		const string pattern = @"\A(*sr:([\s\S]*))\z";
		var compiled = Compile( pattern, new() { Utf16 = utf } );
		var source = Words( pattern );
		foreach ( var first in PcreScriptRunFixtureData.Contexts ) {
			foreach ( var second in PcreScriptRunFixtureData.Contexts ) {
				foreach ( var third in PcreScriptRunFixtureData.Contexts ) {
					CompareOne( source, compiled, String.Concat( Char.ConvertFromUtf32( first ), Char.ConvertFromUtf32( second ),
						Char.ConvertFromUtf32( third ) ), utf, false );
				}
			}
		}
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void ValidationAndBodyRetriesShareWorkCancellationAndReuse( bool utf, bool interpreted ) {
		var policy = new PcreMatchPolicy { Anchored = interpreted };
		foreach ( var (pattern, subject) in new[] {
			(@"\A(*sr:([\s\S]+))\z", new string( 'a', 96 )),
			(@"\A(*sr:(aβ)|(a))β\z", "aβ"),
			(@"\A(*asr:(aβ)|(a))β\z", "aβ"),
			(@"\A(?(DEFINE)(?<r>(*sr:(aβ|a))))(?&r)β\z", "aβ")
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
	public async Task OwnedPatternsAndResultsKeepVisibleViewsAcrossConcurrentReuse() {
		var source = @"\G(*sr:(?<x>[\s\S]+))\z(*MARK:done)".ToCharArray();
		var compiled = Compile( source.AsMemory(), new() { Utf16 = true } );
		Array.Fill( source, 'z' );
		var subject = "\ud800_𐐀𐐁_\udfff".ToCharArray();
		var saved = compiled.Match( subject.AsMemory( 2, 4 ) ).Match;
		Assert.NotNull( saved ); Assert.Equal( "0:4,0:4", Spans( saved ) ); Assert.Equal( "done", saved.MarkName );
		Array.Fill( subject, 'z' );
		var results = await Task.WhenAll( Enumerable.Range( 0, 32 ).Select( index => Task.Run( () =>
			compiled.Match( ( index % 2 == 0 ? "𐐀𐐁" : "aβ" ).AsMemory() ) ) ) );
		for ( var index = 0; index < results.Length; index++ ) {
			Assert.True( results[index].IsSuccess ); Assert.Equal( index % 2 == 0, results[index].Match is not null );
		}
		Assert.Equal( "0:4,0:4", Spans( saved ) ); Assert.Equal( "done", saved.MarkName );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Subject, compiled.Match( "a\udfff".AsMemory() ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidStartOffset, compiled.Match( "😀".AsMemory(), 1 ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Pattern,
			PcreWordSequenceCompiler.CompileSequence( "(*sr:\ud800)".AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
		foreach ( var text in new[] { "\ud800", "\ud800\udfff", "a\udfff", "\ud800a" } ) { CompareNative( @"\G(*sr:([\s\S]+))", text, false ); }
	}

	[Theory]
	[InlineData( @"\A(*sr:([\s\S]*))\z", "𐐀𐐁" )]
	[InlineData( @"\A(*sr:([\s\S]*))\z", "漢あが" )]
	[InlineData( @"\A(*sr:([\s\S]*))\z", "漢ㄅあ" )]
	[InlineData( @"\A(*sr:(aβ)|(a))β\z", "aβ" )]
	public void SharedUnicodeCasesRetainIndependentCaptureCoordinates( string pattern, string subject ) {
		var bytes = Encoding.UTF8.GetBytes( subject );
		var bytePattern = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Utf8 = true } );
		Assert.True( bytePattern.IsSuccess );
		var byteResult = bytePattern.Pattern!.Match( bytes );
		var wordResult = Compile( pattern, new() { Utf16 = true } ).Match( subject.AsMemory() );
		Assert.True( byteResult.IsSuccess ); Assert.True( wordResult.IsSuccess );
		Assert.Equal( byteResult.Match is not null, wordResult.Match is not null );
		var byteSpans = byteResult.Match is { } match ? String.Join( ",", match.Captures.Select( capture => capture is { } value
			? FormattableString.Invariant( $"{value.ByteIndex}:{value.ByteLength}" ) : "-" ) ) : "";
		var map = new PcreDualWidthCoordinates( bytes, subject );
		Assert.Equal( map.ByteCaptures( byteSpans ), map.WordCaptures( Spans( wordResult.Match ) ) );
	}

	private static void CompareOne( ushort[] source, PcreCompiledWordPattern compiled, string subject, bool utf, bool ucp ) {
		var expected = PcreNative16Oracle.ExecuteSequence( source, Words( subject ), utf, 0, ucp: ucp );
		var actual = compiled.Match( subject.AsMemory() );
		Assert.True( expected.Kind is "M" or "N", expected.ToString() ); Assert.True( actual.IsSuccess, actual.Diagnostic?.ToString() );
		Assert.Equal( expected.Kind == "M", actual.Match is not null ); Assert.Equal( expected.Captures, Spans( actual.Match ) );
	}

	private static PcreCompiledWordPattern Compile( string source, PcreWordOptions options ) => Compile( source.AsMemory(), options );
	private static PcreCompiledWordPattern Compile( ReadOnlyMemory<char> source, PcreWordOptions options ) {
		var result = PcreWordSequenceCompiler.CompileSequence( source, options );
		Assert.True( result.IsSuccess, result.Diagnostic?.ToString() ); return result.Pattern!;
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
