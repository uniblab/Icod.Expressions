using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordPairNewlineTests {
	public static IEnumerable<object[]> Cases {
		get {
			var patterns = new[] {
				".", "(?s).", @"\N", @"(?s)\N", "..", "(?s)..", ".X", "(?s).X",
				"^", "$", @"\Z", @"\z", "(?m)^", "(?m)$", "(?m)^X$", "X$", @"X\Z",
				"(?m)\\r^", "(?m)\\r$", "(?m)\\n^", @"\r\Z", @"\n\Z",
				@"[^a]X", @"[^\r]X", @"[\x09-\x0c]X", @"[\x09-\x0d]X", @"\sX", @"\RX", @"\R*X",
				"(?s:.)X", "(?s:.){2}X", "(?s:.)*X", "(?s:.)+X", "[^a]+X", "(?m).*$",
				@"(?:\r\KX|\nX)", "(?s:(.))X", @"(?=\r).X", "(?!X).", @"(?<=\r)\nX",
				@"\r(*SKIP)(*F)|(?s:.)X", @"\r(*MARK:M)(*SKIP:M)(*F)|(?s:.)X",
				@"(?s:.)(*SKIP:missing)X|(?s:.)", "(?s:.)(*PRUNE)X|X", "(?s:.)(*THEN)X|X",
				@"(?(DEFINE)(?<x>\r))(?&x)\nX", @"(?(?=\r)(?s:.)|.)X", @"(?*\r|\r\n)X",
				@"(?m)(?<=\r)^(X)", @"(?m)(?<=\r)$", @"(?s)(a|.)*X", @"(?i:[\x09-\x0c])X",
				@"(?x) a # comment" + "\r\n b", @"(?x) a # comment" + "\u2028 b"
			};
			var subjects = new[] { "", "X", "\r", "\n", "\r\n", "\r\nX", "X\r\n", "a\r\nX", "a\u2028X", "\v\f\u0085X", "😀\r\nX", "ab" };
			foreach ( var newline in new[] { "CRLF", "ANYCRLF", "ANY" } ) {
				foreach ( var utf in new[] { false, true } ) {
					foreach ( var pattern in patterns ) {
						foreach ( var subject in subjects ) {
							yield return new object[] { String.Concat( "(*", newline, ")", pattern ), subject, utf };
						}
					}
				}
			}
			var targeted = new (string Pattern, string Subject)[] {
				(@"[^\r]", "\r\n"),
				(@"(?s:.{2})X", "\r\nXX"), (@".{2}X", "\r\nXX"),
				(@"(?s:.{2})(?i:X|Y)", "\r\nbX"),
				(@"(?s:.{11})X", "\r\n" + new string( 'b', 10 ) + "X"),
				(@"(?s:.{12})X", "\r\n" + new string( 'b', 11 ) + "X"), (@"[\x09-\x0c]", "\r\n"),
				(@"[\x09-\x0c]+X", "\r\n\nX"), (@"[\x09-\x0c]*X", "\r\nX"),
				(@"a?[\x09-\x0c]X", "\r\nX"), (@"(?:|a)[\x09-\x0c]X", "\r\nX"),
				(@"[\x09-\x0c]X|Y", "\r\nX"), (@"[\x09-\x0c]X|\sY", "\r\nX"),
				(@"[\x09-\x0c]{2}X", "\r\n\nX"), (@"[\x09-\x0c]+?X", "\r\n\nX"),
				(@"(?s:.{2})X", "\r\nbX"), (@".{2}X", "\r\nbX"),
				(@"[^a]{2}X", "\r\nbX"), (@"\s{2}X", "\r\n X"),
				(@"(?s:.{3})X", "\r\nbcX"), (@"[^a]{3}X", "\r\nbcX"),
				(@"(?=a)(?s:.{2})X", "\r\nbX"), (@"(?s:.{2})(X|Y)", "\r\nbX"),
				(@"(?m)\r^X", "\r\nX"), (@"(?m)\r$\nX", "\r\nX"),
				(@"(?m)(?=\r)\r$", "\r\nX"), (@"\r(*SKIP)(*F)|(\nX)", "\r\nX"),
				(@"(?s:.)(*SKIP)(*F)|.X", "\r\nX"),
				(@"[\x09-\x0c](*SKIP:missing)Y|[\x09-\x0c]X", "\r\nX"),
				(@"(?(?=[\x09-\x0c])Y|[\x09-\x0c]X)", "\r\nX"),
				(@"(?x) a # 😀 ignored" + "\u2028 b", "ab"),
				(@"(?x) a # ignored" + "\u0085 b", "ab"),
				(@"(?x) a # ignored" + "\r b", "ab")
			};
			foreach ( var newline in new[] { "CRLF", "ANYCRLF", "ANY" } ) {
				foreach ( var utf in new[] { false, true } ) {
					foreach ( var (pattern, subject) in targeted ) {
						yield return new object[] { String.Concat( "(*", newline, ")", pattern ), subject, utf };
					}
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void SearchStartsAndFullCapturesAgreeWithNative( string source, string subject, bool utf ) =>
		CompareNative( source, subject, utf, false );

	private static void CompareNative( string source, string subject, bool utf, bool ucp ) {
		foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 16, 32, 63 } ) {
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				Utf16 = utf, Ucp = ucp, Policy = PcreMatchPolicyConsumer.Policy( defaults )
			} );
			for ( var start = 0; start <= subject.Length; start++ ) {
				if ( utf && start < subject.Length && Char.IsLowSurrogate( subject[start] ) ) { continue; }
				var native = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start, defaults, ucp );
				Assert.True( compiled.IsSuccess, compiled.Diagnostic?.ToString() );
				for ( var mask = 0; mask < native.Length; mask++ ) {
					var expected = native[mask];
					Assert.True( expected.Kind is "M" or "N", $"Native {start}/{defaults}/{mask}: {expected}" );
					var actual = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
					Assert.True( actual.IsSuccess, actual.Diagnostic?.ToString() );
					var spans = Spans( actual.Match );
					var mark = actual.Match is null ? "" : Hex( actual.Match.MarkName );
					Assert.True( expected.Captures == spans && expected.Mark == mark && ( expected.Kind == "M" ) == ( actual.Match is not null ),
						$"{start}/{defaults}/{mask}: expected {expected.Kind} {expected.Captures}|{expected.Mark}, actual {spans}|{mark}" );
				}
			}
		}
	}

	[Theory]
	[InlineData( @"\sX", "\r\nX" )]
	[InlineData( @"\vX", "\r\nX" )]
	[InlineData( @"\hX", "\r\nX" )]
	[InlineData( @"[[:space:]]X", "\r\nX" )]
	[InlineData( @"\p{Z}X", "\r\nX" )]
	[InlineData( @"\sX", "\u2028X" )]
	[InlineData( @"\p{Z}X", "\u2028X" )]
	public void UcpClassesPreserveLineEntryAndUnicodeSeparators( string pattern, string subject ) {
		foreach ( var newline in new[] { "CRLF", "ANYCRLF", "ANY" } ) {
			foreach ( var utf in new[] { false, true } ) { CompareNative( String.Concat( "(*", newline, ")", pattern ), subject, utf, true ); }
		}
	}

	[Theory]
	[InlineData( "(*CRLF)(?m)^😀+$", "x\r\n😀😀\r\n" )]
	[InlineData( @"(*ANY)\R$", "😀\u2028" )]
	[InlineData( @"(*ANYCRLF)\A\R*\r\n\z", "\r\n" )]
	public void SharedUnicodeCasesRetainTheirNativeCoordinates( string source, string subject ) {
		var bytes = Encoding.UTF8.GetBytes( subject );
		var bytePattern = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( source ), new() { Utf8 = true } );
		var wordPattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.True( bytePattern.IsSuccess ); Assert.True( wordPattern.IsSuccess );
		var byteResult = bytePattern.Pattern!.Match( bytes );
		var wordResult = wordPattern.Pattern!.Match( subject.AsMemory() );
		Assert.True( byteResult.IsSuccess ); Assert.True( wordResult.IsSuccess );
		var map = new PcreDualWidthCoordinates( bytes, subject );
		var byteSpans = byteResult.Match is { } match ? String.Join( ",", match.Captures.Select( capture => capture is { } value
			? FormattableString.Invariant( $"{value.ByteIndex}:{value.ByteLength}" ) : "-" ) ) : "";
		Assert.Equal( map.ByteCaptures( byteSpans ), map.WordCaptures( Spans( wordResult.Match ) ) );
	}

	[Theory]
	[InlineData( "(*CRLF)(?m)^a$", "a\r\n" )]
	[InlineData( @"(*ANYCRLF)[\x09-\x0c](*SKIP:missing)Y|[\x09-\x0c]X", "\r\nX" )]
	[InlineData( "(*ANY)(?s:.{2})X", "\r\nbX" )]
	[InlineData( @"(*ANYCRLF)\r(*SKIP)(*F)|(?s:.)X", "\r\nX" )]
	public void NewlineSearchAndReplayShareWorkAndCancellation( string source, string subject ) {
		foreach ( var utf in new[] { false, true } ) {
			var work = 0;
			var options = new PcreWordOptions { Utf16 = utf, WorkObserver = step => work = step };
			var observed = Compile( source, options ).Match( subject.AsMemory(), new PcreMatchPolicy { EndAnchored = true } );
			Assert.True( observed.IsSuccess );
			var exact = Compile( source, options with { WorkObserver = null, MatchWorkLimit = work } )
				.Match( subject.AsMemory(), new PcreMatchPolicy { EndAnchored = true } );
			Assert.True( exact.IsSuccess ); Assert.Equal( Spans( observed.Match ), Spans( exact.Match ) );
			Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded,
				Compile( source, options with { WorkObserver = null, MatchWorkLimit = work - 1 } )
					.Match( subject.AsMemory(), new PcreMatchPolicy { EndAnchored = true } ).Diagnostic?.Code );
			for ( var cancelAt = 1; cancelAt <= work; cancelAt++ ) {
				using var cancellation = new CancellationTokenSource();
				var stop = cancelAt;
				var compiled = Compile( source, options with { WorkObserver = step => { if ( step == stop ) { cancellation.Cancel(); } } } );
				Assert.Throws<OperationCanceledException>( () => compiled.Match( subject.AsMemory(),
					new PcreMatchPolicy { EndAnchored = true }, cancellationToken: cancellation.Token ) );
				Assert.Equal( Spans( observed.Match ), Spans( compiled.Match( subject.AsMemory(), new PcreMatchPolicy { EndAnchored = true } ).Match ) );
			}
		}
	}

	[Fact]
	public async Task NewlineProfilesOwnSourcesAndResultsAndValidateVisibleViews() {
		var source = "(*ANYCRLF)(?m)^(?<x>😀+)$(?:\\R|\\z)(*MARK:done)".ToCharArray();
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.True( result.IsSuccess );
		var compiled = result.Pattern!;
		Array.Fill( source, 'z' );
		var subject = "\ud800_😀😀\r\n_\udfff".ToCharArray();
		var saved = compiled.Match( subject.AsMemory( 2, 6 ) ).Match;
		Assert.Equal( "0:6,0:4", Spans( saved ) );
		Assert.Equal( "done", saved?.MarkName );
		Array.Fill( subject, 'z' );
		var results = await Task.WhenAll( Enumerable.Range( 0, 32 ).Select( index => Task.Run( () =>
			compiled.Match( ( index % 2 == 0 ? "😀😀\r\n" : "xx" ).AsMemory() ) ) ) );
		for ( var index = 0; index < results.Length; index++ ) {
			Assert.True( results[index].IsSuccess );
			Assert.Equal( index % 2 == 0, results[index].Match is not null );
		}
		Assert.Equal( "0:6,0:4", Spans( saved ) ); Assert.Equal( "done", saved?.MarkName );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Subject, compiled.Match( "😀\udfff".AsMemory() ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidStartOffset, compiled.Match( "😀\r\n".AsMemory(), 1 ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Pattern,
			PcreWordSequenceCompiler.CompileSequence( "(*ANY)\ud800".AsMemory(), new() { Utf16 = true } ).Diagnostic?.Code );
		var malformed = PcreWordSequenceCompiler.CompileSequence( "(*ANYCRLF)\\".AsMemory() );
		Assert.Equal( new( PcreWordDiagnosticCode.InvalidSyntax, 10 ), malformed.Diagnostic );
	}

	private static PcreCompiledWordPattern Compile( string source, PcreWordOptions options ) {
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), options );
		Assert.True( result.IsSuccess, result.Diagnostic?.ToString() );
		return result.Pattern!;
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
	private static string Spans( PcreWordMatch? match ) => match is null ? "" : String.Join( ",",
		match.Captures.Select( span => span is { } value ? FormattableString.Invariant( $"{value.CodeUnitIndex}:{value.CodeUnitLength}" ) : "-" ) );
	private static string Hex( string source ) => String.Concat( source.Select( unit => FormattableString.Invariant( $"{(int)unit:X4}" ) ) );
}
