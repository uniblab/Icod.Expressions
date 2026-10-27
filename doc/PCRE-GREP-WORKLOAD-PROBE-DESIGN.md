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

The long-record allocation improvement was substantial, but its 141 ms internal time remained much higher than the repeat reference's 4.22 ms. PR #48 searches for exact leading-byte literals with a span search, charging the same candidate/instruction/comparison work for skipped positions. Observed, cancellable, caseless and UTF-8 paths remain on the ordinary per-candidate path.

The next user-run three-round measurement after literal search (PR #48) again completed all seven full comparisons on the same reported Windows x64/.NET 10.0.14/workstation-GC configuration:

| Workload | Internal ms after #47 → after #48 | PCRE2 ms after #48 | Internal allocated after #48 |
|---|---:|---:|---:|
| literal | 35.54 → 8.02 | 5.66 | 5,165,792 B |
| lookbehind | 64.98 → 38.05 | 7.10 | 5,165,792 B |
| backreference | 14.75 → 16.40 | 6.83 | 5,296,864 B |
| unicode-property | 211.68 → 161.81 | 3.19 | 14,734,048 B |
| dense-only-matching | 8.70 → 1.86 | 1.85 | 2,785,280 B |
| multi-pattern | 286.23 → 23.95 | 26.41 | 40,907,960 B |
| long-record | 141.35 → 0.40 | 3.69 | 10,432 B |

The repeat shows large gains on literal-leading workloads, especially long records and multiple independent patterns. Lookbehind and Unicode property paths remain much slower. Unchanged allocations showed that each no-match call still created VM state before discovering that the exact leading byte was absent; PR #49 added an early no-prefix result while preserving the work-limit result. No speed threshold or consumer migration decision follows from these individual runs.

The three-round measurement after the no-prefix result (PR #49) completed all seven full comparisons, again on Windows x64/.NET 10.0.14 with workstation GC:

| Workload | Internal ms after #48 → after #49 | Internal allocated after #48 → after #49 | PCRE2 ms after #49 |
|---|---:|---:|---:|
| literal | 8.02 → 4.53 | 5,165,792 → 848,192 B | 6.17 |
| lookbehind | 38.05 → 50.64 | 5,165,792 → 5,165,792 B | 6.69 |
| backreference | 16.40 → 14.97 | 5,296,864 → 5,296,864 B | 4.07 |
| unicode-property | 161.81 → 255.24 | 14,734,048 → 14,734,048 B | 4.78 |
| dense-only-matching | 1.86 → 2.80 | 2,785,280 → 1,638,400 B | 3.00 |
| multi-pattern | 23.95 → 15.65 | 40,907,960 → 4,275,280 B | 44.80 |
| long-record | 0.40 → 0.47 | 10,432 → 3,712 B | 3.75 |

No-prefix allocation and time improved on literal-heavy workloads; timing changes in unrelated modes varied between runs. A positive fixed lookbehind followed immediately by a required literal can also return no match before VM allocation when that literal is absent, provided a conservative bound on all candidate steps fits the configured work limit. Small budgets, observers and cancellation retain the ordinary path. Unicode-property matching still needs a separate investigation; these component readings are not Grep command thresholds.

The three-round measurement after the fixed-lookbehind shortcut (PR #50) again completed all seven full span comparisons on the reported Windows x64/.NET 10.0.14/workstation-GC setup:

| Workload | Internal ms after #49 → after #50 | Internal allocated after #50 | PCRE2 ms after #50 |
|---|---:|---:|---:|
| literal | 4.53 → 3.93 | 848,192 B | 4.88 |
| lookbehind | 50.64 → 8.14 | 848,192 B | 5.83 |
| backreference | 14.97 → 57.21 | 5,296,864 B | 4.99 |
| unicode-property | 255.24 → 186.87 | 14,734,048 B | 4.00 |
| dense-only-matching | 2.80 → 2.29 | 1,638,400 B | 2.38 |
| multi-pattern | 15.65 → 15.09 | 4,275,280 B | 41.00 |
| long-record | 0.47 → 1.31 | 3,712 B | 4.62 |

The lookbehind allocation reduction matches the shortcut's scope. Unrelated time variations, especially the backreference result, should not be attributed to this change without repeated controlled measurements. Unicode property remains the largest persistent component gap. ASCII bytes in this UTF-8 workload still pass through general scalar decoding and Unicode-category lookup; an ASCII decoder and a direct ASCII `Letter` check are narrowly applicable without changing Unicode categories, byte offsets or work accounting. Their effect remains to be measured.
