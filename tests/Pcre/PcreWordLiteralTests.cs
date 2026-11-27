using System.Buffers;
using System.Globalization;
using System.Text;
using Icod.Expressions.PackageSmoke;
using Icod.Expressions.Pcre;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreWordLiteralTests {
	public static IEnumerable<object[]> NativeCases {
		get {
			var cases = new[] {
				("", "", 0, false), ("", "00410042", 0, false), ("", "D800DC00", 0, true),
				("0041", "005800410059", 0, false), ("0041", "005800410059", 2, true),
				("002E002A", "0058002E002A0059", 0, true),
				("0028003F006900290041", "0028003F006900290041", 0, true),
				("0028002A0055005400460029", "0028002A0055005400460029", 0, false),
				("0000", "004100000042", 0, true), ("FFFF", "0041FFFF", 0, false),
				("D800", "0041D800DC00", 0, false), ("DC00", "D800DC00", 1, false),
				("D800DC00", "0041D800DC000042", 0, true), ("DBFFDFFF", "DBFFDFFF", 0, true),
				("00E9", "D800DC0000E9", 0, true), ("00E9", "00C9", 0, true),
				("0041", "D8000041", 1, true), ("", "D800", 1, true),
				("0041", "004100410041", 0, true), ("00410042", "004100410042", 0, true)
			};
			for ( var index = 0; index < cases.Length; index++ ) {
				var (pattern, subject, start, utf) = cases[index];
				foreach ( var defaults in new[] { 0, 1, 2, 4, 8, 63 } ) {
					yield return new object[] { index, pattern, subject, start, utf, defaults };
				}
			}
		}
	}

	[Theory]
	[MemberData( nameof( NativeCases ) )]
	public void LiteralCompilationAndEveryCallPolicyMatchNative16( int id, string patternHex, string subjectHex,
		int start, bool utf, int defaults ) {
		var pattern = Units( patternHex ); var subject = Units( subjectHex );
		var source = new string( pattern.Select( unit => (char)unit ).ToArray() );
		var text = new string( subject.Select( unit => (char)unit ).ToArray() );
		var compiled = PcreWordLiteralCompiler.CompileLiteral( source.AsMemory(), new() {
			Utf16 = utf, Policy = PcreMatchPolicyConsumer.Policy( defaults )
		} );
		Assert.True( compiled.IsSuccess, $"literal {id}: {compiled.Diagnostic}" );
		var nativePolicies = PcreNative16Oracle.ExecuteLiteralPolicies( pattern, subject, utf, (uint)start, defaults );
		for ( var mask = 0; mask < 64; mask++ ) {
			var native = nativePolicies[mask];
			Assert.True( native.Kind is "M" or "N", $"native {id}/{defaults}/{mask}: {native}" );
			Assert.NotEqual( 0u, native.Options & 0x02000000u );
			var result = compiled.Pattern!.Match( text.AsMemory(), PcreMatchPolicyConsumer.Policy( mask ), start );
			Assert.True( result.IsSuccess, $"managed {id}/{defaults}/{mask}: {result.Diagnostic}" );
			var projection = result.Match is { } capture ? $"{capture.CodeUnitIndex}:{capture.CodeUnitLength}" : "";
			Assert.Equal( native.Captures, projection );
		}
	}

	[Theory]
	[InlineData( "😀", "x😀y" )]
	[InlineData( "é", "😀é" )]
	[InlineData( "a.b", "xa.by" )]
	[InlineData( "", "é😀" )]
	[InlineData( "(*UTF)", "x(*UTF)y" )]
	[InlineData( "\0", "x\0y" )]
	[InlineData( "z", "😀é" )]
	[InlineData( "é😀", "xé😀y" )]
	public void ValidUnicodeLiteralsAgreeWithTheExistingByteBackend( string pattern, string subject ) {
		var bytes = Encoding.UTF8.GetBytes( subject );
		var bytePattern = PcreByteCompiler.Compile( Encoding.UTF8.GetBytes( pattern ), new() { Literal = true, Utf8 = true } );
		var wordPattern = PcreWordLiteralCompiler.CompileLiteral( pattern.AsMemory(), new() { Utf16 = true } );
		Assert.True( bytePattern.IsSuccess ); Assert.True( wordPattern.IsSuccess );
		var byteResult = bytePattern.Pattern!.Match( bytes );
		var wordResult = wordPattern.Pattern!.Match( subject.AsMemory() );
		Assert.True( byteResult.IsSuccess ); Assert.True( wordResult.IsSuccess );
		var byteSpan = byteResult.Match is { } b ? $"{b.ByteIndex}:{b.ByteLength}" : "";
		var wordSpan = wordResult.Match is { } w ? $"{w.CodeUnitIndex}:{w.CodeUnitLength}" : "";
		var coordinates = new PcreDualWidthCoordinates( bytes, subject );
		Assert.Equal( coordinates.ByteCaptures( byteSpan ), coordinates.WordCaptures( wordSpan ) );
	}

	[Theory]
	[InlineData( false )]
	[InlineData( true )]
	public void MaximumLiteralLengthMatchesWithoutHostRecursion( bool utf ) {
		var source = new string( 'a', 4096 );
		var compiled = PcreWordLiteralCompiler.CompileLiteral( source.AsMemory(), new() { Utf16 = utf } );
		Assert.True( compiled.IsSuccess );
		Assert.Equal( new PcreWordCapture( 0, 4096 ), compiled.Pattern!.Match( source.AsMemory() ).Match?.Captures[0] );
	}

	[Theory]
	[InlineData( "D800", 0 )]
	[InlineData( "0041DC00", 1 )]
	[InlineData( "D8000041", 0 )]
	public void InvalidUtfPatternKeepsNativeErrorOffset( string hex, int offset ) {
		var words = Units( hex );
		var native = PcreNative16Oracle.ExecuteLiteral( words, Array.Empty<ushort>(), true, 0 );
		Assert.Equal( "E", native.Kind ); Assert.Equal( (uint)offset, native.ErrorOffset );
		var text = new string( words.Select( unit => (char)unit ).ToArray() );
		var result = PcreWordLiteralCompiler.CompileLiteral( text.AsMemory(), new() { Utf16 = true } );
		Assert.Equal( PcreWordDiagnosticCode.InvalidUtf16Pattern, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.CodeUnitOffset );
	}

	[Theory]
	[InlineData( "D800", 0, false )]
	[InlineData( "DC00", 0, false )]
	[InlineData( "0041D800", 0, false )]
	[InlineData( "D800DC00", 1, true )]
	[InlineData( "0041DC00", 1, true )]
	public void StrictSubjectAndStartErrorsStaySeparate( string hex, int start, bool badOffset ) {
		var subject = Units( hex );
		var native = PcreNative16Oracle.ExecuteLiteral( Array.Empty<ushort>(), subject, true, (uint)start );
		Assert.Equal( "X", native.Kind );
		if ( badOffset ) { Assert.Equal( -36, native.ErrorCode ); }
		else { Assert.InRange( native.ErrorCode, -26, -24 ); }
		var compiled = PcreWordLiteralCompiler.CompileLiteral( ReadOnlyMemory<char>.Empty, new() { Utf16 = true } ).Pattern!;
		var text = new string( subject.Select( unit => (char)unit ).ToArray() );
		var result = compiled.Match( text.AsMemory(), start );
		Assert.Equal( badOffset ? PcreWordDiagnosticCode.InvalidStartOffset : PcreWordDiagnosticCode.InvalidUtf16Subject,
			result.Diagnostic?.Code );
	}

	[Fact]
	public void CompilationOwnsSlicedLiteralValuesAndMatchingKeepsNativeViews() {
		var source = new[] { 'X', '\uD800', '\uDC00', 'Y' };
		var compiled = PcreWordLiteralCompiler.CompileLiteral( source.AsMemory( 1, 2 ), new() { Utf16 = true } ).Pattern!;
		Array.Fill( source, 'Z' );
		var subject = new[] { '\uDC00', 'a', '\uD800', '\uDC00', 'b', '\uD800' };
		Assert.Equal( new PcreWordCapture( 1, 2 ), compiled.Match( subject.AsMemory( 1, 4 ) ).Match?.Captures[0] );
		Assert.Equal( new[] { '\uDC00', 'a', '\uD800', '\uDC00', 'b', '\uD800' }, subject );
	}

	[Theory]
	[InlineData( false, 6 )]
	[InlineData( true, 8 )]
	public void ExactWorkBudgetsAgreeWithAndWithoutObservation( bool utf, int required ) {
		var trace = new List<int>();
		foreach ( var observed in new[] { false, true } ) {
			foreach ( var budget in new[] { required - 1, required } ) {
				trace.Clear();
				var compiled = PcreWordLiteralCompiler.CompileLiteral( "b".AsMemory(), new() {
					Utf16 = utf, MatchWorkLimit = budget, WorkObserver = observed ? trace.Add : null
				} ).Pattern!;
				var result = compiled.Match( "ab".AsMemory() );
				if ( budget < required ) { Assert.Equal( PcreWordDiagnosticCode.MatchWorkLimitExceeded, result.Diagnostic?.Code ); }
				else { Assert.Equal( new PcreWordCapture( 1, 1 ), result.Match?.Captures[0] ); }
				if ( observed ) { Assert.Equal( Enumerable.Range( 1, budget ), trace ); }
			}
		}
	}

	[Fact]
	public void PreCancellationAndObservedCancellationPropagate() {
		using var source = new CancellationTokenSource(); source.Cancel();
		Assert.Throws<OperationCanceledException>( () => PcreWordLiteralCompiler.CompileLiteral( "a".AsMemory(), cancellationToken: source.Token ) );
		var ordinary = PcreWordLiteralCompiler.CompileLiteral( "a".AsMemory() ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => ordinary.Match( "a".AsMemory(), cancellationToken: source.Token ) );
		using var during = new CancellationTokenSource();
		var observed = PcreWordLiteralCompiler.CompileLiteral( "z".AsMemory(), new() {
			Utf16 = true, WorkObserver = work => { if ( work == 2 ) { during.Cancel(); } }
		} ).Pattern!;
		Assert.Throws<OperationCanceledException>( () => observed.Match( "abcd".AsMemory(), cancellationToken: during.Token ) );
	}

	[Fact]
	public void AdmissionGuardsPrecedeReadingPatternStorage() {
		using var source = new UnreadableMemory();
		Assert.Equal( PcreWordDiagnosticCode.PatternLengthLimitExceeded,
			PcreWordLiteralCompiler.CompileLiteral( source.Memory ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.InvalidWorkLimit,
			PcreWordLiteralCompiler.CompileLiteral( source.Memory, new() { MatchWorkLimit = 0 } ).Diagnostic?.Code );
		Assert.Equal( PcreWordDiagnosticCode.PatternLengthLimitExceeded,
			PcreWordLiteralCompiler.CompileLiteral( "ab".AsMemory(), new() { MaxPatternLength = 1 } ).Diagnostic?.Code );
		Assert.True( PcreWordLiteralCompiler.CompileLiteral( ReadOnlyMemory<char>.Empty, new() { MaxPatternLength = 0 } ).IsSuccess );
	}

	[Theory]
	[InlineData( -1 )]
	[InlineData( 2 )]
	public void OutOfRangeOffsetsAreDiagnostics( int offset ) {
		var compiled = PcreWordLiteralCompiler.CompileLiteral( "a".AsMemory() ).Pattern!;
		var result = compiled.Match( "a".AsMemory(), offset );
		Assert.Equal( PcreWordDiagnosticCode.InvalidStartOffset, result.Diagnostic?.Code );
		Assert.Equal( offset, result.Diagnostic?.CodeUnitOffset );
	}

	private static ushort[] Units( string hex ) => Enumerable.Range( 0, hex.Length / 4 ).Select( index =>
		UInt16.Parse( hex.AsSpan( index * 4, 4 ), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture ) ).ToArray();

	private sealed class UnreadableMemory : MemoryManager<char> {
		internal new Memory<char> Memory => CreateMemory( 4097 );
		public override Span<char> GetSpan() => throw new InvalidOperationException( "Storage must not be read." );
		public override MemoryHandle Pin( int elementIndex = 0 ) => throw new NotSupportedException();
		public override void Unpin() { }
		protected override void Dispose( bool disposing ) { }
	}
}
