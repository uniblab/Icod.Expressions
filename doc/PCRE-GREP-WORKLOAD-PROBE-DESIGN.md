# Grep PCRE component workload probe

**Status:** test-only measurement aid for issue #7. It does not approve a PCRE release or consumer migration.

## Intent and provenance

Reproduce the seven deterministic inputs in `Icod.Grep` commit `2ef1c11f11b77eb876242eb0c92a88d20857db64`, `benchmarks/Grep.Benchmarks/PcreCommandBenchmarks.cs` and `CorpusFactory.cs`. Measure the pinned PCRE.NET 1.6.0 / PCRE2 JIT matcher and the current internal byte matcher against the same record bytes and pattern order. Include the literal, fixed literal lookbehind, backreference, UTF-8 property, dense `-o`, 32 independent patterns, and 262 KiB long-record cases.

## Boundaries

The probe is a separate, unpackaged .NET 10 console tool. The pinned native reference is used only here and in tests. It compiles patterns once and times matching over already framed record bytes, so its times **cannot** be compared to Grep's published command-level 1.6.0 timings. It also does not measure Grep's record framing, pattern-file ingestion, output, or packaging. Keep `Icod.Grep` and `Icod.CommandFramework` unchanged.

`--smoke` reproduces Grep's bounded smoke dimensions (at most 64 records, 256-byte/character record target length) while retaining all 32 patterns. It compares every pattern's source-byte result, start offset and `-o` iteration before checking selected-record counts; no timing threshold. Run this on one CI runner without slowing every target. `--measure` uses the original seven full workloads, verifies results first, warms both engines, then runs alternating measurement rounds and prints per-workload elapsed time and current-thread allocation with host/runtime metadata. A mismatch or internal resource diagnostic fails with the scenario and record number. The match-work limit is the existing internal default; a long-record limit failure is a finding, not something to hide by raising the limit silently.

Measured thresholds require an agreed physical reference host, repeated results and a separate conformance decision. No CI timing assertion or speedup claim follows from a single smoke run.
