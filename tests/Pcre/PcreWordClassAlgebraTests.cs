using System.Globalization;
using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordClassAlgebraTests {
	public static IEnumerable<object[]> Cases {
		get {
			var perl = new[] {
				@"(?[[a-z] - [aeiou]])", @"(?[[a] | [b] & [a]])", @"(?[[a] - [b] ^ [c]])", @"(?[!(![a] | [b])])",
				@"(?[\p{L} & !\p{sc=Greek}])", @"(?[\p{Any} ^ \p{Nd}])", @"(?[[\x{0100}-\x{02ff}] - [\x{0200}-\x{02ff}]])",
				@"(?[[k] | [s]])", @"(?i:(?[[k] - [K]]))", @"(?ir:(?[[k] | [s]]))",
				@"(?[[\p{Cs}] | [\x{0000}-\x{00ff}]])", @"(?[[:alpha:] - [aeiou]])"
			};
			var uts = new[] {
				@"[a-z--[aeiou]]", @"[a&&b]", @"[a||b]", @"[a~~b]", @"[[^a]&&[^b]]",
				@"[\p{L}&&\P{sc=Greek}]", @"[\p{Any}~~\p{Nd}]", @"[\x{0100}-\x{02ff}--[\x{0200}-\x{02ff}]]",
				@"[k||s]", @"(?i:[k--K])", @"(?ir:[k||s])", @"[\p{Cs}||[\x{0000}-\x{00ff}]]"
			};
			var subjects = new[] { "", "abcX", "bβX", "ĀāX", "AKſX", "😀X", "\r\nX", "\0X" };
			foreach ( var utf in new[] { false, true } ) {
				foreach ( var alt in new[] { false, true } ) {
					foreach ( var atom in alt ? uts : perl ) {
						foreach ( var source in new[] { String.Concat( "(", atom, ")" ), String.Concat( @"\G(", atom, "+)" ),
							String.Concat( @"\A(", atom, @"{0,3})X\z" ), String.Concat( @"\A(", atom, "+?)(", atom, @")\z" ),
							String.Concat( "(?=(", atom, @"))\1" ) } ) {
							foreach ( var subject in subjects ) { yield return new object[] { source, subject, utf, alt }; }
						}
					}
				}
			}
			var scoped = new (string Source, string Subject, bool Alt)[] {
				(@"\A((?[[a-z] - [aeiou]])+)\1\z", "bcbc", false),
				(@"\A(?<x>(?[[a-z]&[^aeiou]]))(?1)\z", "bc", false),
				(@"\A(?:(?[[a] | [b]])Z|((?[[a] ^ [b]])))\z", "b", false),
				(@"(?<=(?[[a-z] - [b-y]]))b", "ab", false), (@"(?<!(?[[a-z] - [b-y]]))b", "cb", false),
				(@"(?*(((?[[a-z]]))+))\1a", "bba", false),
				(@"\A(*sr:((?[[\p{L}] | [0-9]])+))\z", "aβ", false),
				(@"(?[[a]])(*MARK:M)(*SKIP:M)X|(?[[a] | [b]])", "ab", false),
				(@"(?[[a]])(*SKIP:missing)X|(?[[a] | [b]])", "ab", false),
				(@"(*CRLF)(?[[\r] | [\n]])X", "\r\nX", false),
				(@"(*ANYCRLF)(?[[\x09-\x0c] & \s])X", "\r\nX", false),
				(@"(*ANYCRLF)(?i)(?[[z\x09-\x0c] & \s])X", "\r\nX", false),
				(@"\A([a-z--[aeiou]]+)\1\z", "bcbc", true),
				(@"\A(*sr:([\p{L}||[0-9]]+))\z", "ab1", true),
				(@"\A[a||[^a]]\z", "😀", true), (@"(?xx)\A[ a - z && [ ^ b - y ] ]\z", "a", true),
				(@"(*CRLF)[\r||\n]X", "\r\nX", true),
				(@"(*ANYCRLF)[\x09-\x0c&&\s]X", "\r\nX", true),
				(@"(*ANYCRLF)(?i)[z\x09-\x0c&&\s]X", "\r\nX", true),
				(@"\A(?[[a&&b] & [b]])\z", "b", true),
				(@"\A(?[[a||b] & [|]])\z", "|", true)
			};
			foreach ( var utf in new[] { false, true } ) {
				foreach ( var row in scoped ) { yield return new object[] { row.Source, row.Subject, utf, row.Alt }; }
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void OperatorsCapturesAndScopedRetriesAgreeWithNativePolicies( string source, string subject, bool utf, bool alt ) =>
		CompareNative( source, subject, utf, alt: alt );

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void ExtendedClassesConsumeOriginalWordUnits( bool utf ) {
		CompareNative( @"\G((?[[a-z] - [aeiou]]))", "bc", utf );
		CompareNative( @"((?[[Ā-Ă] | [😀]]))", "😀Ā", utf );
		CompareNative( @"([Ā-Ă||[😀]])", "😀Ā", utf, alt: true );
		CompareNative( @"([Ā-Ă])", "😀Ā", utf, alt: true );
	}

	[Theory]
	[InlineData( false, 0 )]
	[InlineData( false, 1 )]
	[InlineData( false, 2 )]
	[InlineData( false, 3 )]
	[InlineData( true, 0 )]
	[InlineData( true, 1 )]
	[InlineData( true, 2 )]
	[InlineData( true, 3 )]
	public void Utf16KeepsIndependentFrozenScalarMembership( bool alt, int profile ) {
		var expressions = alt ? PcreUtsClassAlgebraFixtureData.UnicodeExpressions : PcrePerlClassAlgebraFixtureData.UnicodeExpressions;
		var rows = alt ? PcreUtsClassAlgebraFixtureData.UnicodeMasks : PcrePerlClassAlgebraFixtureData.UnicodeMasks;
		foreach ( var row in rows ) {
			var fields = row.Split( ';' );
			if ( int.Parse( fields[0], CultureInfo.InvariantCulture ) != profile ) { continue; }
			var expression = expressions[int.Parse( fields[1], CultureInfo.InvariantCulture )];
			var pattern = String.Concat( @"\A(", alt ? WrapUtsExpression( expression ) : String.Concat( "(?[", expression, "])" ), @")\z" );
			var compiled = Compile( pattern, new() { Utf16 = true, Ucp = ( profile & 1 ) != 0,
				Caseless = ( profile & 2 ) != 0, AltExtendedClass = alt } );
			var mask = Convert.FromHexString( fields[2] );
			for ( var index = 0; index < PcrePerlClassAlgebraFixtureData.Scalars.Length; index++ ) {
				var scalar = PcrePerlClassAlgebraFixtureData.Scalars[index];
				var text = Char.ConvertFromUtf32( scalar );
				var result = compiled.Match( text.AsMemory() );
				Assert.True( result.IsSuccess, result.Diagnostic?.ToString() );
				var expected = ( mask[index >> 3] & ( 1 << ( index & 7 ) ) ) != 0;
				Assert.True( expected == ( result.Match is not null ), $"{pattern} {profile}: U+{scalar:X}" );
				if ( expected ) { Assert.Equal( $"0:{text.Length},0:{text.Length}", Spans( result.Match ) ); }
			}
		}
	}

	[Theory]
	[InlineData( false, 1 )]
	[InlineData( false, 3 )]
	[InlineData( false, 5 )]
	[InlineData( false, 7 )]
	[InlineData( true, 1 )]
	[InlineData( true, 3 )]
	[InlineData( true, 5 )]
	[InlineData( true, 7 )]
	public void EveryFrozenExpressionKeepsUtfScalarByteRangeMembership( bool alt, int profile ) {
		var expressions = alt ? PcreUtsClassAlgebraFixtureData.Expressions : PcrePerlClassAlgebraFixtureData.Expressions;
		var rows = alt ? PcreUtsClassAlgebraFixtureData.ByteMasks : PcrePerlClassAlgebraFixtureData.ByteMasks;
		foreach ( var row in rows ) {
			var fields = row.Split( ';' );
			if ( int.Parse( fields[0], CultureInfo.InvariantCulture ) != profile ) { continue; }
			var expression = expressions[int.Parse( fields[1], CultureInfo.InvariantCulture )];
			var pattern = String.Concat( @"\A(", alt ? WrapUtsExpression( expression ) : String.Concat( "(?[", expression, "])" ), @")\z" );
			var compiled = Compile( pattern, new() { Utf16 = true, Ucp = ( profile & 2 ) != 0,
				Caseless = ( profile & 4 ) != 0, AltExtendedClass = alt } );
			var mask = Convert.FromHexString( fields[2] );
			for ( var scalar = 0; scalar < 256; scalar++ ) {
				var result = compiled.Match( new string( (char)scalar, 1 ).AsMemory() );
				Assert.True( result.IsSuccess );
				var expected = ( mask[scalar >> 3] & ( 1 << ( scalar & 7 ) ) ) != 0;
				Assert.True( expected == ( result.Match is not null ), $"{pattern} {profile}: U+{scalar:X}" );
				if ( expected ) { Assert.Equal( "0:1,0:1", Spans( result.Match ) ); }
			}
		}
	}

	[Theory]
	[InlineData( false, false )]
	[InlineData( false, true )]
	[InlineData( true, false )]
	[InlineData( true, true )]
	public void HighUnitsAndOperandFoldingAgreeWithNative16( bool utf, bool ucp ) {
		var values = new[] { 0, 9, 10, 13, 65, 97, 75, 107, 83, 115, 127, 128, 255, 256, 257,
			0x3a3, 0x3c2, 0x3c3, 0x17f, 0x212a, 0x300, 0xd7ff, 0xd800, 0xdbff, 0xdc00, 0xdfff, 0xe000,
			0xffff, 0x10000, 0x10400, 0x10428, 0x1f600, 0x10ffff };
		foreach ( var alt in new[] { false, true } ) {
			foreach ( var atom in alt ? new[] { @"[\p{Any}--a]", @"[\p{Cs}||[\x{100}-\x{2ff}]]", @"[k||s]", @"[\x{100}-\x{2ff}--[\x{200}-\x{2ff}]]", utf ? @"[\N{U+004B}||\x{53}]" : @"[\x{4b}||\x{53}]", @"[\p{Lu}~~\p{Ll}]" }
				: new[] { @"(?[\p{Any} - [a]])", @"(?[\p{Cs} | [\x{100}-\x{2ff}]])", @"(?[[k] | [s]])", @"(?[[\x{100}-\x{2ff}] - [\x{200}-\x{2ff}]])", utf ? @"(?[\N{U+004B} | \x{53}])" : @"(?[\x{4b} | \x{53}])", @"(?[\p{Lu} ^ \p{Ll}])" } ) {
				foreach ( var casing in new[] { "", "(?i)", "(?ir)" } ) {
					var pattern = String.Concat( casing, @"\A(", atom, @")\z" );
					var compiled = Compile( pattern, new() { Utf16 = utf, Ucp = ucp, AltExtendedClass = alt } );
					foreach ( var value in values ) {
						if ( utf && value is >= 0xd800 and <= 0xdfff ) { continue; }
						var subject = value <= 0xffff ? new string( (char)value, 1 ) : Char.ConvertFromUtf32( value );
						var native = PcreNative16Oracle.ExecuteSequence( Words( pattern ), Words( subject ), utf, 0, ucp: ucp, altExtendedClass: alt );
						var result = compiled.Match( subject.AsMemory() );
						Assert.True( native.Kind is "M" or "N", native.ToString() ); Assert.True( result.IsSuccess );
						Assert.True( native.Captures == Spans( result.Match ) && ( native.Kind == "M" ) == ( result.Match is not null ),
							$"{pattern} {utf}/{ucp}: U+{value:X}: {native.Captures} != {Spans( result.Match )}" );
					}
				}
			}
		}
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void EveryExpressionStepSharesBudgetsCancellationAndFreshScratch( bool utf ) {
		foreach ( var (pattern, subject, alt) in new[] {
			(String.Concat( @"\A(?[", String.Join( " ^ ", Enumerable.Repeat( "[a]", 101 ) ), @"])\z" ), "a", false),
			(@"\A(?:(?[[a] | [b]])Z|((?[[a] ^ [b]])))\z", "b", false),
			(@"\A([a-z--[aeiou]]+?)[a-z--[b-y]]\z", "bca", true),
			(@"\A(?(DEFINE)(?<x>(?[[a] | [b]])))(?&x){2}\z", "ab", false)
		} ) {
			var work = 0;
			var options = new PcreWordOptions { Utf16 = utf, AltExtendedClass = alt, WorkObserver = step => work = step };
			var observed = Compile( pattern, options ).Match( subject.AsMemory() );
			Assert.True( observed.IsSuccess ); Assert.NotNull( observed.Match );
			var exact = Compile( pattern, options with { WorkObserver = null, MatchWorkLimit = work } ).Match( subject.AsMemory() );
			Assert.True( exact.IsSuccess ); Assert.Equal( Spans( observed.Match ), Spans( exact.Match ) );
			Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded,
				Compile( pattern, options with { WorkObserver = null, MatchWorkLimit = work - 1 } ).Match( subject.AsMemory() ).Diagnostic?.Code );
			for ( var stop = 1; stop <= work; stop++ ) {
				using var cancellation = new CancellationTokenSource();
				var cancelAt = stop;
				var compiled = Compile( pattern, options with { WorkObserver = step => { if ( step == cancelAt ) { cancellation.Cancel(); } } } );
				Assert.Throws<OperationCanceledException>( () => compiled.Match( subject.AsMemory(), cancellationToken: cancellation.Token ) );
				Assert.Equal( Spans( observed.Match ), Spans( compiled.Match( subject.AsMemory() ).Match ) );
			}
		}
	}

	[Fact]
	public async Task OwnedSourceAndResultsSurviveViewsAndConcurrentEvaluation() {
		var source = @"\G(?<x>(?[\p{L} - [a-z]]))\z(*MARK:done)".ToCharArray();
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } ).Pattern!;
		Array.Fill( source, 'z' );
		var subject = "\ud800_𐐀_\udfff".ToCharArray();
		var saved = compiled.Match( subject.AsMemory( 2, 2 ) ).Match;
		Assert.NotNull( saved ); Assert.Equal( "0:2,0:2", Spans( saved ) ); Assert.Equal( "done", saved.MarkName );
		Array.Fill( subject, 'z' );
		var results = await Task.WhenAll( Enumerable.Range( 0, 32 ).Select( index => Task.Run( () =>
			compiled.Match( ( index % 2 == 0 ? "𐐀" : "a" ).AsMemory() ) ) ) );
		for ( var index = 0; index < results.Length; index++ ) {
			Assert.True( results[index].IsSuccess ); Assert.Equal( index % 2 == 0, results[index].Match is not null );
		}
		Assert.Equal( "0:2,0:2", Spans( saved ) ); Assert.Equal( "done", saved.MarkName );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Subject, compiled.Match( "a\udfff".AsMemory() ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidStartOffset, compiled.Match( "😀".AsMemory(), 1 ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void GrammarErrorsAndLeafLimitsKeepOriginalWordOffsets( bool utf ) {
		foreach ( var prefix in new[] { "", "é", "😀" } ) {
			foreach ( var suffix in new[] { "(?[a])", "(?[[a] |])", "(?[&[a]])", "(?[[a][b]])", @"(?[\X])" } ) {
				var result = PcreWordSequenceCompiler.CompileSequence( String.Concat( prefix, suffix ).AsMemory(), new() { Utf16 = utf } );
				Assert.Equal( PcreWordDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
				Assert.Equal( prefix.Length, result.Diagnostic?.CodeUnitOffset );
			}
		}
		foreach ( var alt in new[] { false, true } ) {
			var leaf = String.Concat( "[", new string( 'a', 65 ), "]" );
			var expression = alt ? String.Concat( "[", leaf, "&&a]" ) : String.Concat( "(?[", leaf, "])" );
			foreach ( var pattern in new[] { expression, String.Concat( expression, "{0}" ), String.Concat( "(?(DEFINE)(?<x>", expression, "))a" ) } ) {
				Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
					PcreWordSequenceCompiler.CompileSequence( pattern.AsMemory(), new() { Utf16 = utf, AltExtendedClass = alt } ).Diagnostic?.Code );
			}
		}
		const string literal = "[a&&b]";
		var literalResult = PcreWordLiteralCompiler.CompileLiteral( literal.AsMemory(), new() { Utf16 = utf, AltExtendedClass = true } );
		Assert.NotNull( literalResult.Pattern!.Match( literal.AsMemory() ).Match );
		CompareNative( @"\A[a&&b]\z", "&", utf, allDefaults: false );
	}

	[Fact]
	public void FoldedMembersAndSourceNestingHaveAggregateBoundsEvenWhenErased() {
		const string leaf = @"[\x{100}-\x{2ff}]";
		var options = new PcreWordOptions { Utf16 = true, Caseless = true };
		Assert.NotNull( Compile( String.Concat( "(?[", leaf, "])" ), options ).Match( "ā".AsMemory() ).Match );
		var large = String.Concat( "(?[", String.Join( "|", Enumerable.Repeat( leaf, 100 ) ), "])" );
		Assert.True( large.Length < 4096 );
		foreach ( var source in new[] { large, String.Concat( large, "{0}a" ), String.Concat( "(?(DEFINE)(?<x>", large, "))a" ) } ) {
			Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
				PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), options ).Diagnostic?.Code );
		}
		var valid = String.Concat( "(?[", new string( '(', 13 ), "[a]", new string( ')', 13 ), "])" );
		Assert.NotNull( Compile( valid, options ).Match( "a".AsMemory() ).Match );
		var deep = String.Concat( "é(?[", new string( '(', 14 ), "[a]", new string( ')', 14 ), "])" );
		var result = PcreWordSequenceCompiler.CompileSequence( deep.AsMemory(), options );
		Assert.Equal( PcreWordDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( 1, result.Diagnostic?.CodeUnitOffset );
	}

	[Theory]
	[InlineData( @"\A((?[\p{L} | \p{Emoji}])+)\z", "é😀" )]
	[InlineData( @"\A((?[[a-z] - [aeiou]])+)\1\z", "bcbc" )]
	public void SharedScalarsKeepIndependentByteAndWordCaptureCoordinates( string pattern, string subject ) {
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

	private static string WrapUtsExpression( string expression ) {
		// The frozen UTS corpus includes leading x/xx scopes. They remain
		// grammar outside the nested class used to exercise expression lowering.
		var prefix = expression.StartsWith( "(?", StringComparison.Ordinal ) ? expression.IndexOf( ')' ) + 1 : 0;
		return String.Concat( expression[..prefix], "[", expression[prefix..], "]" );
	}

	private static PcreCompiledWordPattern Compile( string source, PcreWordOptions options ) {
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), options );
		Assert.True( result.IsSuccess, String.Concat( source, ": ", result.Diagnostic ) ); return result.Pattern!;
	}

	private static void CompareNative( string source, string subject, bool utf, bool ucp = false, bool allDefaults = true, bool alt = false ) {
		foreach ( var defaults in allDefaults ? new[] { 0, 1, 2, 4, 8, 16, 32, 63 } : new[] { 0 } ) {
			var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
				Utf16 = utf, Ucp = ucp, AltExtendedClass = alt, Policy = PcreMatchPolicyConsumer.Policy( defaults )
			} );
			Assert.True( compiled.IsSuccess, String.Concat( source, ": ", compiled.Diagnostic ) );
			for ( var start = 0; start <= subject.Length; start++ ) {
				if ( utf && start < subject.Length && Char.IsLowSurrogate( subject[start] ) ) { continue; }
				var native = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start, defaults, ucp, altExtendedClass: alt );
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
