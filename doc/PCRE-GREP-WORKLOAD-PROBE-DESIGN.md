# Grep PCRE component workload probe

**Status:** test-only measurement aid for issue #7. It does not approve a PCRE release or consumer migration.

## Intent and provenance

Reproduce the seven deterministic inputs in `Icod.Grep` commit `2ef1c11f11b77eb876242eb0c92a88d20857db64`, `benchmarks/Grep.Benchmarks/PcreCommandBenchmarks.cs` and `CorpusFactory.cs`. Measure the pinned PCRE.NET 1.6.0 / PCRE2 JIT matcher and the current internal byte matcher against the same record bytes and pattern order. Include the literal, fixed literal lookbehind, backreference, UTF-8 property, dense `-o`, 32 independent patterns, and 262 KiB long-record cases.

## Boundaries

The probe is a separate, unpackaged .NET 10 console tool. The pinned native reference is used only here and in tests. It compiles patterns once and times matching over already framed record bytes, so its times **cannot** be compared to Grep's published command-level 1.6.0 timings. It also does not measure Grep's record framing, pattern-file ingestion, output, or packaging. Keep `Icod.Grep` and `Icod.CommandFramework` unchanged.

`--smoke` reproduces Grep's bounded smoke dimensions (at most 64 records, 256-byte/character record target length) while retaining all 32 patterns. It compares every pattern's source-byte result, start offset and `-o` iteration before checking selected-record counts; no timing threshold. Run this on one CI runner without slowing every target. `--measure` uses the original seven full workloads, verifies results first, warms both engines, then runs alternating measurement rounds and prints per-workload elapsed time and current-thread allocation with host/runtime metadata. A mismatch or internal resource diagnostic fails with the scenario and record number. The match-work limit is the existing internal default; a long-record limit failure is a finding, not something to hide by raising the limit silently.

Measured thresholds require an agreed physical reference host, repeated results and a separate conformance decision. No CI timing assertion or speedup claim follows from a single smoke run.

## First full component measurement

The first user-run `--measure --iterations 3` completed all seven span comparisons on Windows 10.0.26200 x64, .NET 10.0.14, workstation GC. These are mean **component** times and current-thread allocations; they are not Grep command timings or migration thresholds.

| Workload | PCRE2 ms | Internal ms | PCRE2 allocated | Internal allocated |
|---|---:|---:|---:|---:|
| literal | 5.47 | 109.57 | 38,560 B | 246,274,016 B |
| lookbehind | 3.70 | 101.11 | 38,560 B | 246,274,016 B |
| backreference | 4.00 | 27.78 | 53,984 B | 77,578,464 B |
| unicode-property | 3.95 | 249.64 | 38,560 B | 219,141,568 B |
| dense-only-matching | 2.13 | 18.99 | 163,840 B | 73,564,160 B |
| multi-pattern | 27.04 | 569.31 | 9,640 B | 2,453,449,208 B |
| long-record | 3.39 | 235.82 | 320 B | 1,208,005,312 B |

All seven agreed on the expected selected-record and match counts. The large internal allocations prompted candidate-state reuse in the next matcher change. A fresh full measurement on the same host is needed to quantify its effect; the remaining CPU gap and broader conformance gate are still open.
