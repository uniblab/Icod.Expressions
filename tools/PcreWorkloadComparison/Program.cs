using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Icod.Expressions.Pcre;
using PCRE;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

try {
	var measure = args.Length > 0 && args[0] == "--measure";
	var steps = args.Length > 0 && args[0] == "--steps";
	const string usage = "Usage: --smoke | --measure [--iterations 1..20] | --steps <unicode-property|backreference> [--smoke]";
	var boundedSteps = steps && args.Length == 3 && args[2] == "--smoke";
	var iterations = 3;
	if ( measure ) {
		if ( args.Length == 3 && args[1] == "--iterations"
			&& int.TryParse( args[2], NumberStyles.None, CultureInfo.InvariantCulture, out var parsed )
			&& parsed is >= 1 and <= 20 ) { iterations = parsed; }
		else if ( args.Length != 1 ) { throw new ArgumentException( usage ); }
	} else if ( steps ) {
		if ( args.Length is not 2 and not 3 || args[1] is not ( "unicode-property" or "backreference" )
			|| args.Length == 3 && !boundedSteps ) {
			throw new ArgumentException( usage );
		}
	} else if ( args.Length != 1 || args[0] != "--smoke" ) {
		throw new ArgumentException( usage );
	}

	Console.WriteLine( JsonSerializer.Serialize( new {
		kind = "host", mode = measure ? "measure" : boundedSteps ? "steps-smoke" : steps ? "steps" : "smoke",
		framework = RuntimeInformation.FrameworkDescription,
		os = RuntimeInformation.OSDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
		serverGc = GCSettings.IsServerGC, reference = "PCRE.NET 1.6.0 compiled", internalLimit = PcreByteOptions.DefaultMatchWorkLimit,
		iterations = measure ? iterations : 0
	} ) );
	foreach ( var scenario in Workloads.All ) {
		if ( steps && scenario.Name != args[1] ) { continue; }
		long workSteps = 0;
		Console.Error.WriteLine( $"{scenario.Name}: preparing and comparing {( measure || steps && !boundedSteps ? "full" : "smoke" )} records" );
		var workload = PreparedWorkload.Create( scenario, !measure && ( !steps || boundedSteps ),
			steps ? _ => workSteps++ : null );
		var verified = workload.Run( Engine.Compare );
		var expected = ( workload.Records.Length - 1 ) / scenario.MatchEvery + 1;
		if ( verified.Selected != expected || verified.Matches != expected ) {
			throw new InvalidOperationException( $"{scenario.Name}: expected {expected} selected records and matches; "
				+ $"found {verified.Selected} and {verified.Matches}." );
		}
		if ( steps ) {
			workSteps = 0; // Exclude the prior reference-comparison pass.
			long selectedSteps = 0;
			long unselectedSteps = 0;
			long previous = 0;
			long maximumRecordSteps = 0;
			var observed = workload.Run( Engine.Managed, selected => {
				var recordSteps = workSteps - previous;
				previous = workSteps;
				if ( selected ) { selectedSteps += recordSteps; }
				else { unselectedSteps += recordSteps; }
				maximumRecordSteps = Math.Max( maximumRecordSteps, recordSteps );
			} );
			workload.CheckTotals( observed, verified );
			if ( selectedSteps + unselectedSteps != workSteps || maximumRecordSteps == 0 ) {
				throw new InvalidOperationException( $"{scenario.Name}: inconsistent match-work accounting." );
			}
			Console.WriteLine( JsonSerializer.Serialize( new {
				kind = "workload-steps", scenario.Name, records = workload.Records.Length,
				selected = verified.Selected, matches = verified.Matches,
				workSteps, selectedSteps, unselectedSteps, maximumRecordSteps,
				averageSelectedSteps = selectedSteps / (double)verified.Selected,
				averageUnselectedSteps = unselectedSteps / (double)( workload.Records.Length - verified.Selected )
			} ) );
			continue;
		}
		if ( !measure ) {
			Console.WriteLine( JsonSerializer.Serialize( new {
				kind = "workload", scenario.Name, records = workload.Records.Length,
				bytes = workload.RecordBytes, patterns = scenario.Patterns.Length,
				selected = verified.Selected, matches = verified.Matches, status = "equal"
			} ) );
			continue;
		}

		// Warm each component on the exact inputs before collecting alternating rounds.
		workload.CheckTotals( workload.Run( Engine.Reference ), verified );
		workload.CheckTotals( workload.Run( Engine.Managed ), verified );
		var referenceMs = 0.0;
		var managedMs = 0.0;
		long referenceBytes = 0;
		long managedBytes = 0;
		for ( var round = 0; round < iterations; round++ ) {
			if ( round % 2 == 0 ) {
				Collect( Engine.Reference, ref referenceMs, ref referenceBytes );
				Collect( Engine.Managed, ref managedMs, ref managedBytes );
			} else {
				Collect( Engine.Managed, ref managedMs, ref managedBytes );
				Collect( Engine.Reference, ref referenceMs, ref referenceBytes );
			}
		}
		Console.WriteLine( JsonSerializer.Serialize( new {
			kind = "workload", scenario.Name, records = workload.Records.Length,
			bytes = workload.RecordBytes, patterns = scenario.Patterns.Length,
			selected = verified.Selected, matches = verified.Matches,
			referenceMeanMs = referenceMs / iterations, managedMeanMs = managedMs / iterations,
			referenceMeanAllocatedBytes = referenceBytes / iterations,
			managedMeanAllocatedBytes = managedBytes / iterations
		} ) );

		void Collect( Engine engine, ref double elapsed, ref long allocated ) {
			var beforeBytes = GC.GetAllocatedBytesForCurrentThread();
			var beforeTicks = Stopwatch.GetTimestamp();
			var total = workload.Run( engine );
			var afterTicks = Stopwatch.GetTimestamp();
			var afterBytes = GC.GetAllocatedBytesForCurrentThread();
			workload.CheckTotals( total, verified );
			elapsed += Stopwatch.GetElapsedTime( beforeTicks, afterTicks ).TotalMilliseconds;
			allocated += afterBytes - beforeBytes;
		}
	}
} catch ( Exception error ) {
	Console.Error.WriteLine( error );
	Environment.ExitCode = 1;
}

internal enum Engine { Reference, Managed, Compare }

internal readonly record struct SpanResult( int Index, int Length ) {
	internal bool Success => Index >= 0;
}

internal readonly record struct Totals( int Selected, int Matches );

internal enum CorpusKind { Catalog, Backreference, UnicodeProperty }

internal sealed record Workload( string Name, int Records, int Length, int MatchEvery,
	string[] Patterns, CorpusKind Kind = CorpusKind.Catalog, bool OnlyMatching = false );

internal static class Workloads {
	internal static readonly Workload[] All = [
		new( "literal", 16_384, 80, 17, [ "TARGET" ] ),
		new( "lookbehind", 16_384, 80, 17, [ "(?<=prefix-)TARGET" ] ),
		new( "backreference", 16_384, 0, 17, [ @"(TARGET)\1" ], CorpusKind.Backreference ),
		new( "unicode-property", 16_384, 0, 17, [ @"\p{L}+ 世界" ], CorpusKind.UnicodeProperty ),
		new( "dense-only-matching", 4_096, 96, 1, [ "TARGET" ], OnlyMatching: true ),
		new( "multi-pattern", 4_096, 96, 17,
			Enumerable.Range( 0, 31 ).Select( index => $"NO_MATCH_{index:D2}" ).Append( "TARGET" ).ToArray() ),
		new( "long-record", 32, 262_144, 4, [ "TARGET" ] )
	];
}

internal sealed class PreparedWorkload {
	private static readonly string PaddingAlphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
	private readonly PcreRegex8Bit[] reference;
	private readonly PcreCompiledBytePattern[] managed;
	private readonly Workload scenario;

	private PreparedWorkload( Workload scenario, byte[][] records,
		PcreRegex8Bit[] reference, PcreCompiledBytePattern[] managed ) {
		this.scenario = scenario;
		this.Records = records;
		this.reference = reference;
		this.managed = managed;
		this.RecordBytes = records.Sum( record => (long)record.Length + 1 ); // Grep's LF framing.
	}

	internal byte[][] Records { get; }
	internal long RecordBytes { get; }

	internal static PreparedWorkload Create( Workload scenario, bool smoke, Action<int>? workObserver = null ) {
		var count = smoke ? Math.Min( 64, scenario.Records ) : scenario.Records;
		var length = smoke ? Math.Min( 256, scenario.Length ) : scenario.Length;
		var records = new byte[count][];
		for ( var index = 0; index < count; index++ ) {
			var selected = index % scenario.MatchEvery == 0;
			var line = scenario.Kind switch {
				CorpusKind.Backreference => selected ? "record TARGETTARGET payload" : "record ordinary payload",
				CorpusKind.UnicodeProperty => selected ? "Καλημέρα 世界 TARGET payload" : "Καλημέρα ordinary payload",
				_ => CreateCatalogLine( index, selected, length )
			};
			records[index] = Encoding.UTF8.GetBytes( line );
		}
		var utf8 = scenario.Kind == CorpusKind.UnicodeProperty;
		var encoding = utf8 ? Encoding.UTF8 : Encoding.Latin1;
		var reference = new PcreRegex8Bit[scenario.Patterns.Length];
		var managed = new PcreCompiledBytePattern[scenario.Patterns.Length];
		for ( var index = 0; index < scenario.Patterns.Length; index++ ) {
			var bytes = encoding.GetBytes( scenario.Patterns[index] );
			var options = PcreOptions.Compiled;
			if ( utf8 ) { options |= PcreOptions.Utf | PcreOptions.Ucp | PcreOptions.MatchInvalidUtf; }
			reference[index] = new PcreRegex8Bit( bytes, encoding, new PcreRegexSettings {
				Options = options,
				ExtraCompileOptions = utf8 ? PcreExtraCompileOptions.AsciiBsD : PcreExtraCompileOptions.None
			} );
			var result = PcreByteCompiler.Compile( bytes, new PcreByteOptions {
				Utf8 = utf8, Ucp = utf8, AsciiBsD = utf8, MatchInvalidUtf = utf8,
				WorkObserver = workObserver
			} );
			managed[index] = result.Pattern ?? throw new InvalidOperationException(
				$"{scenario.Name}: internal compilation failed for pattern {index}: {result.Diagnostic?.Message}" );
		}
		return new( scenario, records, reference, managed );
	}

	private static string CreateCatalogLine( int index, bool selected, int targetLength ) {
		var prefix = $"record-{index:D8} ";
		var marker = selected ? "prefix-TARGET " : "ordinary-data ";
		var remaining = Math.Max( 0, targetLength - prefix.Length - marker.Length );
		var padding = new char[remaining];
		for ( var offset = 0; offset < remaining; offset++ ) {
			padding[offset] = PaddingAlphabet[offset % PaddingAlphabet.Length];
		}
		return prefix + marker + new string( padding );
	}

	internal Totals Run( Engine engine, Action<bool>? afterRecord = null ) {
		var selectedRecords = 0;
		var matches = 0;
		for ( var recordIndex = 0; recordIndex < Records.Length; recordIndex++ ) {
			var record = Records[recordIndex];
			var selected = false;
			for ( var patternIndex = 0; patternIndex < scenario.Patterns.Length; patternIndex++ ) {
				var offset = 0;
				while ( offset <= record.Length ) {
					SpanResult found;
					if ( engine == Engine.Compare ) {
						var native = FindReference( patternIndex, record, offset );
						var internalMatch = FindManaged( patternIndex, record, offset );
						if ( native != internalMatch ) {
							throw new InvalidOperationException( $"{scenario.Name}: record {recordIndex}, pattern {patternIndex}, "
								+ $"start {offset}: PCRE2 {native}, internal {internalMatch}." );
						}
						found = native;
					} else {
						found = engine == Engine.Reference
							? FindReference( patternIndex, record, offset )
							: FindManaged( patternIndex, record, offset );
					}
					if ( !found.Success ) { break; }
					selected = true;
					matches++;
					if ( !scenario.OnlyMatching ) { break; }
					if ( found.Length == 0 ) {
						throw new InvalidOperationException( $"{scenario.Name}: unexpected empty -o result." );
					}
					offset = checked( found.Index + found.Length );
				}
			}
			if ( selected ) { selectedRecords++; }
			afterRecord?.Invoke( selected );
		}
		return new( selectedRecords, matches );
	}

	internal void CheckTotals( Totals result, Totals verified ) {
		if ( result != verified ) {
			throw new InvalidOperationException( $"{scenario.Name}: changed totals {result} versus verified {verified}." );
		}
	}

	private SpanResult FindReference( int index, byte[] subject, int start ) {
		var result = reference[index].Match( subject, start );
		return result.Success ? new( result.Index, result.Length ) : new( -1, 0 );
	}

	private SpanResult FindManaged( int index, byte[] subject, int start ) {
		var result = managed[index].Match( subject, start );
		if ( !result.IsSuccess ) {
			throw new InvalidOperationException( $"{scenario.Name}: internal match diagnostic {result.Diagnostic?.Code} "
				+ $"at start {start}: {result.Diagnostic?.Message}" );
		}
		return result.Match is null ? new( -1, 0 ) : new( result.Match.ByteIndex, result.Match.ByteLength );
	}
}
