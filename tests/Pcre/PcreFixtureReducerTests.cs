using System.Text;
using PCRE;
using Xunit;

namespace Icod.Expressions.Tests.Pcre;

public sealed class PcreFixtureReducerTests {
	private static readonly UTF8Encoding StrictUtf8 = new( false, true );
	private static byte[] Bytes( string value ) => Encoding.ASCII.GetBytes( value );
	private static string Text( ReadOnlyMemory<byte> value ) => Encoding.ASCII.GetString( value.Span );

	[Theory]
	[InlineData( "xxAyy", "zzBqq" )]
	[InlineData( "A", "B" )]
	[InlineData( "1234A5678", "12B3456" )]
	[InlineData( "AA", "BB" )]
	public void BothFieldsReachAOneDeletionMinimum( string pattern, string subject ) {
		var calls = 0;
		var result = PcreFixtureReducer.Reduce<byte>( Bytes( pattern ), Bytes( subject ), 0, ( p, s, _ ) => {
			calls++;
			return p.Span.Contains( (byte)'A' ) && s.Span.Contains( (byte)'B' );
		} );
		Assert.Equal( PcreReductionStatus.Completed, result.Status );
		Assert.Equal( "A", Text( result.Pattern ) );
		Assert.Equal( "B", Text( result.Subject ) );
		Assert.Equal( calls, result.Evaluations );
		Assert.InRange( calls, 1, 1024 );
	}

	[Fact]
	public void CharacterUnitsPreserveIsolatedSurrogates() {
		var result = PcreFixtureReducer.Reduce<char>( new[] { 'x', '\uD800', 'y' }, new[] { 'z', '\uDC00' }, 0,
			( p, s, _ ) => p.Span.Contains( '\uD800' ) && s.Span.Contains( '\uDC00' ) );
		Assert.Equal( PcreReductionStatus.Completed, result.Status );
		Assert.Equal( new[] { '\uD800' }, result.Pattern.ToArray() );
		Assert.Equal( new[] { '\uDC00' }, result.Subject.ToArray() );
	}

	[Fact]
	public void UnsignedNativeWordsRemainValuesWithoutTranscoding() {
		var result = PcreFixtureReducer.Reduce<ushort>( new ushort[] { 1, 65535, 2 }, new ushort[] { 0, 256 }, 0,
			( p, s, _ ) => p.Span.Contains( (ushort)65535 ) && s.Span.Contains( (ushort)256 ) );
		Assert.Equal( PcreReductionStatus.Completed, result.Status );
		Assert.Equal( new ushort[] { 65535 }, result.Pattern.ToArray() );
		Assert.Equal( new ushort[] { 256 }, result.Subject.ToArray() );
	}

	[Fact]
	public void AlwaysTrueCanReduceBothFieldsToEmpty() {
		var result = PcreFixtureReducer.Reduce<byte>( Bytes( "ABC" ), Bytes( "DEF" ), 0, ( _, _, _ ) => true );
		Assert.Equal( PcreReductionStatus.Completed, result.Status );
		Assert.Empty( result.Pattern.ToArray() );
		Assert.Empty( result.Subject.ToArray() );
	}

	[Fact]
	public void OriginalThatDoesNotReproduceStopsAfterOneEvaluation() {
		var calls = 0;
		var result = PcreFixtureReducer.Reduce<byte>( Bytes( "AB" ), Bytes( "CD" ), 0, ( _, _, _ ) => { calls++; return false; } );
		Assert.Equal( PcreReductionStatus.NotReproduced, result.Status );
		Assert.Equal( 1, calls );
		Assert.Equal( 1, result.Evaluations );
		Assert.Equal( "AB", Text( result.Pattern ) );
		Assert.Equal( "CD", Text( result.Subject ) );
	}

	[Fact]
	public void StartOffsetRemainsFixedAndValidForEveryCandidate() {
		var result = PcreFixtureReducer.Reduce<byte>( Bytes( "xAy" ), Bytes( "zzBqq" ), 2, ( p, s, offset ) => {
			Assert.Equal( 2, offset );
			Assert.True( s.Length >= offset );
			return p.Span.Contains( (byte)'A' ) && s.Span[offset..].Contains( (byte)'B' );
		} );
		Assert.Equal( PcreReductionStatus.Completed, result.Status );
		Assert.Equal( 2, result.StartOffset );
		Assert.Equal( "A", Text( result.Pattern ) );
		Assert.Equal( "zzB", Text( result.Subject ) );
	}

	[Theory]
	[InlineData( 0 )]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	[InlineData( 4 )]
	[InlineData( 5 )]
	[InlineData( 6 )]
	[InlineData( 7 )]
	public void InvalidArgumentsDoNotCallTheOracle( int selection ) {
		var calls = 0;
		Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, int, bool> oracle = ( _, _, _ ) => { calls++; return true; };
		if ( selection == 0 ) { Assert.Throws<ArgumentNullException>( () => PcreFixtureReducer.Reduce<byte>( Array.Empty<byte>(), Array.Empty<byte>(), 0, null! ) ); }
		else if ( selection == 1 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreFixtureReducer.Reduce<byte>( Array.Empty<byte>(), Array.Empty<byte>(), -1, oracle ) ); }
		else if ( selection == 2 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreFixtureReducer.Reduce<byte>( Array.Empty<byte>(), Array.Empty<byte>(), 1, oracle ) ); }
		else if ( selection == 3 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreFixtureReducer.Reduce<byte>( Array.Empty<byte>(), Array.Empty<byte>(), 0, oracle, 0 ) ); }
		else if ( selection == 4 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreFixtureReducer.Reduce<byte>( Array.Empty<byte>(), Array.Empty<byte>(), 0, oracle, -1 ) ); }
		else if ( selection == 5 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreFixtureReducer.Reduce<byte>( Array.Empty<byte>(), Array.Empty<byte>(), 0, oracle, 65537 ) ); }
		else if ( selection == 6 ) { Assert.Throws<ArgumentOutOfRangeException>( () => PcreFixtureReducer.Reduce<byte>( new byte[4097], Array.Empty<byte>(), 0, oracle ) ); }
		else { Assert.Throws<ArgumentOutOfRangeException>( () => PcreFixtureReducer.Reduce<byte>( Array.Empty<byte>(), new byte[4097], 0, oracle ) ); }
		Assert.Equal( 0, calls );
	}

	[Fact]
	public void PrecancelledWorkDoesNotCallTheOracle() {
		using var source = new CancellationTokenSource();
		source.Cancel();
		var calls = 0;
		Assert.Throws<OperationCanceledException>( () => PcreFixtureReducer.Reduce<byte>( Bytes( "AB" ), Bytes( "CD" ), 0,
			( _, _, _ ) => { calls++; return true; }, cancellationToken: source.Token ) );
		Assert.Equal( 0, calls );
	}

	[Fact]
	public void CancellationAfterAnOracleCallPropagates() {
		using var source = new CancellationTokenSource();
		var calls = 0;
		Assert.Throws<OperationCanceledException>( () => PcreFixtureReducer.Reduce<byte>( Bytes( "AB" ), Bytes( "CD" ), 0,
			( _, _, _ ) => { calls++; source.Cancel(); return true; }, cancellationToken: source.Token ) );
		Assert.Equal( 1, calls );
	}

	[Fact]
	public void OracleExceptionsAreNotTreatedAsNonMismatches() {
		var failure = new InvalidOperationException( "oracle failed" );
		var actual = Assert.Throws<InvalidOperationException>( () => PcreFixtureReducer.Reduce<byte>( Bytes( "AB" ), Bytes( "CD" ), 0,
			( _, _, _ ) => throw failure ) );
		Assert.Same( failure, actual );
	}

	[Theory]
	[InlineData( 1 )]
	[InlineData( 2 )]
	[InlineData( 3 )]
	[InlineData( 4 )]
	[InlineData( 5 )]
	public void EvaluationLimitReturnsTheLastAcceptedWitness( int budget ) {
		var calls = 0;
		var result = PcreFixtureReducer.Reduce<byte>( new byte[20], new byte[20], 0, ( p, s, _ ) => {
			calls++;
			return p.Length == 20 && s.Length == 20;
		}, budget );
		Assert.Equal( PcreReductionStatus.EvaluationLimit, result.Status );
		Assert.Equal( budget, result.Evaluations );
		Assert.Equal( budget, calls );
		Assert.Equal( 20, result.Pattern.Length );
		Assert.Equal( 20, result.Subject.Length );
	}

	[Fact]
	public void ResultsOwnCopiesOfOriginalBuffers() {
		var pattern = Bytes( "AB" );
		var subject = Bytes( "CD" );
		var result = PcreFixtureReducer.Reduce<byte>( pattern, subject, 0, ( _, _, _ ) => false );
		Array.Fill( pattern, (byte)'X' );
		Array.Fill( subject, (byte)'Y' );
		Assert.Equal( "AB", Text( result.Pattern ) );
		Assert.Equal( "CD", Text( result.Subject ) );
	}

	[Fact]
	public void DeterministicPredicatesProduceIdenticalTracesAndResults() {
		var first = new List<string>();
		var second = new List<string>();
		PcreReduction<byte> Run( List<string> trace ) => PcreFixtureReducer.Reduce<byte>( Bytes( "xxAyy" ), Bytes( "zzBqq" ), 0, ( p, s, _ ) => {
			trace.Add( String.Concat( Text( p ), "|", Text( s ) ) );
			return p.Span.Contains( (byte)'A' ) && s.Span.Contains( (byte)'B' );
		} );
		var a = Run( first );
		var b = Run( second );
		Assert.Equal( first, second );
		Assert.Equal( Text( a.Pattern ), Text( b.Pattern ) );
		Assert.Equal( Text( a.Subject ), Text( b.Subject ) );
	}

	[Fact]
	public void EmptyWitnessCompletesWithinOneEvaluation() {
		var result = PcreFixtureReducer.Reduce<byte>( Array.Empty<byte>(), Array.Empty<byte>(), 0, ( _, _, _ ) => true, 1 );
		Assert.Equal( PcreReductionStatus.Completed, result.Status );
		Assert.Equal( 1, result.Evaluations );
	}

	[Fact]
	public void BothFieldsAreRevisitedAfterTheOtherChanges() {
		var result = PcreFixtureReducer.Reduce<byte>( Bytes( "AB" ), Bytes( "CD" ), 0, ( p, s, _ ) =>
			( Text( p ) == "AB" && ( Text( s ) == "CD" || Text( s ) == "D" ) ) || ( Text( p ) == "B" && Text( s ) == "D" ) );
		Assert.Equal( PcreReductionStatus.Completed, result.Status );
		Assert.Equal( "B", Text( result.Pattern ) );
		Assert.Equal( "D", Text( result.Subject ) );
	}

	[Fact]
	public void OneDeletionMinimalityDoesNotClaimTheGlobalSmallestPair() {
		var result = PcreFixtureReducer.Reduce<byte>( Bytes( "AB" ), Bytes( "CD" ), 0, ( p, s, _ ) =>
			( Text( p ) == "AB" && Text( s ) == "CD" ) || ( Text( p ) == "A" && Text( s ) == "D" ) );
		Assert.Equal( PcreReductionStatus.Completed, result.Status );
		Assert.Equal( "AB", Text( result.Pattern ) );
		Assert.Equal( "CD", Text( result.Subject ) );
	}

	[Fact]
	public void AConsumerOffsetComparisonFaultShrinksToOneNonAsciiScalar() {
		var result = PcreFixtureReducer.Reduce<byte>( StrictUtf8.GetBytes( "é" ), StrictUtf8.GetBytes( "zzéyy" ), 0, Disagrees );
		Assert.Equal( PcreReductionStatus.Completed, result.Status );
		Assert.Equal( "é", StrictUtf8.GetString( result.Pattern.Span ) );
		Assert.Equal( "é", StrictUtf8.GetString( result.Subject.Span ) );
		Assert.True( Disagrees( result.Pattern, result.Subject, 0 ) );
		var coordinates = new PcreDualWidthCoordinates( result.Subject.ToArray(), "é" );
		Assert.Equal( coordinates.ByteCaptures( "0:2" ), coordinates.WordCaptures( "0:1" ) );

		bool Disagrees( ReadOnlyMemory<byte> pattern, ReadOnlyMemory<byte> subject, int offset ) {
			string p, s;
			try { p = StrictUtf8.GetString( pattern.Span ); s = StrictUtf8.GetString( subject.Span ); }
			catch ( DecoderFallbackException ) { return false; }
			var regex = new PcreRegex8Bit( pattern.ToArray(), StrictUtf8, new PcreRegexSettings { Options = PcreOptions.Utf | PcreOptions.Compiled } );
			var match = regex.Match( subject.ToArray(), offset );
			if ( !match.Success ) { return false; }
			var words = PcreNative16Oracle.Execute( p.Select( c => (ushort)c ).ToArray(), s.Select( c => (ushort)c ).ToArray(), 1, 0 );
			return words.Kind == "M" && words.Captures != $"{match.Index}:{match.EndIndex - match.Index}";
		}
	}
}
