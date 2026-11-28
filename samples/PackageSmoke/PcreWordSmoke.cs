using System.Globalization;
using Icod.Expressions.Pcre;

namespace Icod.Expressions.PackageSmoke;

// Shared with tests. The executable consumes only the candidate NuGet assembly.
internal static class PcreWordSmoke {
	internal static PcreSmokeProtocol Run( PcreWordSmokeCase item ) {
		var source = Padded( item.Pattern );
		var subject = Padded( item.Subject );
		var pattern = source.AsMemory( 1, item.Pattern.Length );
		var compiled = item.Literal
			? PcreWordLiteralCompiler.CompileLiteral( pattern, item.Options )
			: PcreWordSequenceCompiler.CompileSequence( pattern, item.Options );
		if ( !compiled.IsSuccess ) {
			var diagnostic = compiled.Diagnostic ?? throw new InvalidOperationException( "Missing word compile diagnostic." );
			return new( "C", Diagnostic: diagnostic.Code.ToString(), DiagnosticOffset: diagnostic.CodeUnitOffset );
		}
		Array.Fill( source, 'X' );
		var result = compiled.Pattern!.Match( subject.AsMemory( 1, item.Subject.Length ), item.Start );
		if ( !subject.AsSpan().SequenceEqual( Padded( item.Subject ) ) ) {
			throw new InvalidOperationException( "Word matching modified the subject view." );
		}
		if ( !result.IsSuccess ) {
			var diagnostic = result.Diagnostic ?? throw new InvalidOperationException( "Missing word match diagnostic." );
			return new( "X", Diagnostic: diagnostic.Code.ToString(), DiagnosticOffset: diagnostic.CodeUnitOffset );
		}
		if ( result.Match is not { } match ) { return new( "N" ); }
		if ( !ReferenceEquals( match.CaptureInfo, compiled.Pattern.CaptureInfo )
			|| match.Captures[0] != new PcreWordCapture( match.CodeUnitIndex, match.CodeUnitLength ) ) {
			throw new InvalidOperationException( "Word match lost capture metadata or reported endpoints." );
		}
		return Project( match );
	}

	internal static void VerifyObservedView( bool utf ) {
		var subject = Padded( "A" );
		var compiled = PcreWordSequenceCompiler.CompileSequence( "B".AsMemory(), new() {
			Utf16 = utf, WorkObserver = step => { if ( step == 1 ) { subject[1] = 'B'; } }
		} );
		if ( !compiled.IsSuccess || compiled.Pattern!.Match( subject.AsMemory( 1, 1 ) ).Match
			is not { CodeUnitIndex: 0, CodeUnitLength: 1 } || subject[0] != '\udfff' || subject[^1] != '\ud800' ) {
			throw new InvalidOperationException( "Word matching did not observe the authoritative subject view." );
		}
	}

	internal static void VerifyCancellation( bool utf, bool compile ) {
		using var cancelled = new CancellationTokenSource();
		cancelled.Cancel();
		if ( compile ) {
			_ = PcreWordSequenceCompiler.CompileSequence( "a".AsMemory(), new() { Utf16 = utf }, cancelled.Token );
			return;
		}
		_ = Compile( "a", new() { Utf16 = utf } ).Match( "a".AsMemory(), cancellationToken: cancelled.Token );
	}

	internal static void VerifyInterruptedMatch( bool utf ) {
		using var cancelled = new CancellationTokenSource();
		var compiled = Compile( @"a\Kb", new() {
			Utf16 = utf, WorkObserver = step => { if ( step == 3 ) { cancelled.Cancel(); } }
		} );
		try {
			_ = compiled.Match( "ab".AsMemory(), cancellationToken: cancelled.Token );
			throw new InvalidOperationException( "Word match cancellation did not propagate." );
		} catch ( OperationCanceledException ) {
			if ( Project( compiled.Match( "ab".AsMemory() ).Match ) != new PcreSmokeProtocol( "M", "1:1" ) ) {
				throw new InvalidOperationException( "Word match cancellation leaked state into reuse." );
			}
		}
	}

	internal static void VerifyReuse( bool utf ) {
		var source = Padded( @"(?<x>a)\K(b)(*MARK:saved)" );
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory( 1, source.Length - 2 ),
			new() { Utf16 = utf } );
		if ( !compiled.IsSuccess ) { throw new InvalidOperationException( "Word reuse pattern failed." ); }
		Array.Fill( source, 'X' );
		var subject = Padded( "ab" );
		var saved = compiled.Pattern!.Match( subject.AsMemory( 1, 2 ) ).Match;
		Array.Fill( subject, 'z' );
		var missed = compiled.Pattern.Match( "ax".AsMemory() );
		var again = compiled.Pattern.Match( "ab".AsMemory() );
		if ( Project( saved ) != new PcreSmokeProtocol( "M", "1:1,0:1,1:1", "00730061007600650064" )
			|| !missed.IsSuccess || missed.Match is not null || Project( again.Match ) != Project( saved )
			|| saved?.GetCapture( "x" ) != new PcreWordCapture( 0, 1 ) ) {
			throw new InvalidOperationException( "Word results or metadata changed across subject mutation and reuse." );
		}
	}

	private static PcreCompiledWordPattern Compile( string source, PcreWordOptions options ) {
		var compiled = PcreWordSequenceCompiler.CompileSequence( source.AsMemory(), options );
		return compiled.Pattern ?? throw new InvalidOperationException( $"Word compile failed: {compiled.Diagnostic}." );
	}

	private static PcreSmokeProtocol Project( PcreWordMatch? match ) => match is null ? new( "N" )
		: new( "M", String.Join( ",", match.Captures.Select( capture => capture is { } value
			? String.Concat( value.CodeUnitIndex.ToString( CultureInfo.InvariantCulture ), ":",
				value.CodeUnitLength.ToString( CultureInfo.InvariantCulture ) ) : "-" ) ),
			String.Concat( match.MarkName.Select( unit => ( (int)unit ).ToString( "X4", CultureInfo.InvariantCulture ) ) ) );

	private static char[] Padded( string text ) {
		var result = new char[checked( text.Length + 2 )];
		result[0] = '\udfff';
		result[^1] = '\ud800';
		text.AsSpan().CopyTo( result.AsSpan( 1 ) );
		return result;
	}
}
