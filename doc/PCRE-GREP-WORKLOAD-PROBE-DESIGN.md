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

All seven agreed on the expected selected-record and match counts. The large internal allocations prompted candidate-state reuse in PR #47; the remaining CPU gap and broader conformance gate are still open.

The repeat three-round run after candidate-state reuse (PR #47) reported the same Windows x64, .NET 10.0.14 and workstation GC metadata. All seven full byte-span comparisons still completed. Internal mean times and allocations changed as follows:

| Workload | Internal ms before → after | Internal allocated before → after | PCRE2 ms in repeat |
|---|---:|---:|---:|
| literal | 109.57 → 35.54 | 246,274,016 → 5,165,792 B | 4.84 |
| lookbehind | 101.11 → 64.98 | 246,274,016 → 5,165,792 B | 5.60 |
| backreference | 27.78 → 14.75 | 77,578,464 → 5,296,864 B | 4.98 |
| unicode-property | 249.64 → 211.68 | 219,141,568 → 14,734,048 B | 4.41 |
| dense-only-matching | 18.99 → 8.70 | 73,564,160 → 2,785,280 B | 1.93 |
| multi-pattern | 569.31 → 286.23 | 2,453,449,208 → 40,907,960 B | 35.65 |
| long-record | 235.82 → 141.35 | 1,208,005,312 → 10,432 B | 4.22 |

The long-record allocation improvement is substantial, but its 141 ms internal time remains much higher than the repeat reference's 4.22 ms. An exact leading-byte literal can skip failed candidate positions using a span search while charging the same candidate/instruction/comparison work. Observed, cancellable, caseless and UTF-8 paths remain on the ordinary per-candidate path. Measure this narrower optimization on the same host before deciding whether to extend search acceleration to more syntax.
