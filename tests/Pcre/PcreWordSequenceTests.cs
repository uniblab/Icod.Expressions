using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordSequenceTests {
	public static IEnumerable<object[]> Cases {
		get {
			var cases = new[] {
				("", "", 0, false), ("a", "xxay", 0, false), (@"\Aab\z", "ab", 0, false),
				(@"\Aab\z", "xab", 0, true), (@"\Ga", "xay", 1, true), (@"a\G", "aa", 0, false),
				(@"\x{100}", "x\u0100y", 0, false), (@"\x{FFFF}", "\uFFFF", 0, false),
				(@"\x{D800}", "\uD800", 0, false), (@"\o{177777}", "\uFFFF", 0, false),
				(@"\777", "\u01FF", 0, false), (@"\x{1F600}", "x😀y", 0, true),
				(@"\o{200000}", "\U00010000", 0, true), (@"\Q[ab].*\E", "x[ab].*y", 0, true),
				(@"\n\t\e\a\f", "\n\t\u001B\u0007\f", 0, false), (@"\cA", "\u0001", 0, false),
				(@"\N{U+1F600}", "😀", 0, true), (@"\x41b", "Ab", 0, true),
				(@"\.\*\?\+\(\)\[\]\{\}\^\$\\", ".*?+()[]{}^$\\", 0, false), (@"\A\G\z", "", 0, true)
			};
			for ( var index = 0; index < cases.Length; index++ ) {
				var (source, subject, start, utf) = cases[index];
				foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 63 } ) {
					yield return new object[] { index, source, subject, start, utf, defaults };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( Cases ) )]
	public void SharedGrammarSequencesAgreeWithEveryNativeCallPolicy( int id, string source, string subject,
		int start, bool utf, int defaults ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() {
			Utf16 = utf, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"sequence {id}: {compiled.Diagnostic}" );
		var nativePolicies = PcreNative16Oracle.ExecuteSequencePolicies( Words( source ), Words( subject ), utf, (uint)start, defaults );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = nativePolicies[mask];
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			var result = compiled.Pattern!.Match( subject.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			Assert.Equal( native.Captures, result.Match is { } c ? $"{c.CodeUnitIndex}:{c.CodeUnitLength}" : "" );
		}
	}

	[Theory]
	[InlineData( @"\x{10000}", false )]
	[InlineData( @"\o{200000}", false )]
	[InlineData( @"\x{D800}", true )]
	[InlineData( @"\x{110000}", true )]
	public void NumericAdmissionUsesNativeWordDomain( string source, bool utf ) {
		var native = PcreNative16Oracle.ExecuteSequence( Words( source ), Array.Empty<ushort>(), utf, 0 );
		Assert.Equal( "E", native.Kind );
		var result = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = utf } );
		Assert.Equal( PcreWordDiagnosticCode.InvalidSyntax, result.Diagnostic?.Code );
		Assert.Equal( 0, result.Diagnostic?.CodeUnitOffset );
	}

	[Theory]
	[InlineData( "(?>(*sr:(?:a){65}a)a+)b*c*d*e*" )]
	[InlineData( "(*CRLF)(*UTF)^a$" )]
	[InlineData( "(*UTF)a" )]
	public void UnimplementedLoweringIsExplicit( string source ) {
		Assert.Equal( PcreWordDiagnosticCode.UnsupportedSyntax,
			PcreWordSequenceCompiler.CompileSequence( source.AsMemory() ).Diagnostic?.Code );
	}

	[Theory]
	[InlineData( @"\A\x{E9}\x{1F600}\z", "é😀" )]
	[InlineData( @"\Q[ab].*\E", "x[ab].*y" )]
	[InlineData( @"\Ga", "abc" )]
	[InlineData( @"\o{200000}", "x\U00010000y" )]
	public void ValidUnicodeSequencesRetainManagedCrossWidthSemantics( string source, string subject ) {
		var bytes = Encoding.UTF8.GetBytes( subject );
		var bytePattern = PcreByteCompiler.Compile( Encoding.ASCII.GetBytes( source ), new() { Utf8 = true } );
		var wordPattern = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), new() { Utf16 = true } );
		Assert.True( bytePattern.IsSuccess ); Assert.True( wordPattern.IsSuccess );
		var b = bytePattern.Pattern!.Match( bytes ); var w = wordPattern.Pattern!.Match( subject.AsMemory() );
		Assert.True( b.IsSuccess ); Assert.True( w.IsSuccess );
		var coordinates = new PcreDualWidthCoordinates( bytes, subject );
		Assert.Equal( coordinates.ByteCaptures( b.Match is { } bc ? $"{bc.ByteIndex}:{bc.ByteLength}" : "" ),
			coordinates.WordCaptures( w.Match is { } wc ? $"{wc.CodeUnitIndex}:{wc.CodeUnitLength}" : "" ) );
	}

	[Theory]
	[InlineData( false, 5 )]
	[InlineData( true, 6 )]
	public void SequenceAssertionsShareTheBoundedWorkCounter( bool utf, int required ) {
		foreach ( var observed in new[] { false, true } ) {
			foreach ( var budget in new[] { required - 1, required } ) {
				var trace = new List<int>();
				var pattern = PcreWordSequenceCompiler.CompileSequence( @"\Aa\z".AsMemory(), new() {
					Utf16 = utf, MatchWorkLimit = budget, WorkObserver = observed ? trace.Add : null
				} ).Pattern!;
				var result = pattern.Match( "a".AsMemory() );
				if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); }
				else { Assert.Equal( new PcreWordCapture( 0, 1 ), result.Match?.Captures[0] ); }
				if ( observed ) { Assert.Equal( Enumerable.Range( 1, budget ), trace ); }
			}
		}
	}

	[Fact]
	public void SequenceAdmissionOwnershipAndCancellationRemainBounded() {
		var source = @"\x{100}".ToCharArray();
		var compiled = PcreWordSequenceCompiler.CompileSequence( source ).Pattern!;
		Array.Fill( source, 'X' );
		Assert.Equal( new PcreWordCapture( 0, 1 ), compiled.Match( "\u0100".AsMemory() ).Match?.Captures[0] );
		Assert.Equal( PcreWordDiagnosticCode.PatternLengthLimitExceeded,
			PcreWordSequenceCompiler.CompileSequence( new string( 'a', 4097 ).AsMemory() ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Pattern,
			PcreWordSequenceCompiler.CompileSequence( new char[] { '\uD800' }, new() { Utf16 = true } ).Diagnostic?.Code );
		using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreWordSequenceCompiler.CompileSequence( source, cancellationToken: cancelled.Token ) );
	}

	private static ushort[] Words( string source ) => source.Select( unit => (ushort)unit ).ToArray();
}
