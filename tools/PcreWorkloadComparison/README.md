# PCRE workload comparison probe

This unpackaged tool reproduces the seven PCRE inputs from `Icod.Grep` 1.6.0 at commit `2ef1c11f11b77eb876242eb0c92a88d20857db64`. It compiles the pinned PCRE.NET 1.6.0 native matcher and the internal `Icod.Expressions` byte matcher, compares every pattern and record's source-byte match span, and verifies the selected-record and match counts before it measures anything.

Run a quick correctness check locally in Debug:

```sh
dotnet run --project tools/PcreWorkloadComparison/PcreWorkloadComparison.csproj -c Debug -- --smoke
```

Run the complete matcher-component inputs on a controlled physical reference host:

```sh
dotnet run --project tools/PcreWorkloadComparison/PcreWorkloadComparison.csproj -c Release -- --measure --iterations 3 > pcre-workloads.jsonl
```

Diagnose the internal work budget for a full Unicode-property or backreference workload:

```sh
dotnet run --project tools/PcreWorkloadComparison/PcreWorkloadComparison.csproj -c Release -- --steps unicode-property > pcre-unicode-steps.jsonl
dotnet run --project tools/PcreWorkloadComparison/PcreWorkloadComparison.csproj -c Release -- --steps backreference > pcre-backreference-steps.jsonl
```

`--steps` verifies the same full source-byte results first, then counts the internal match-work steps separately for selected and unselected records. It reports no timings: the observer runs on every step and changes execution cost. The mode is restricted to these two workloads, which do not use the unobserved byte-search shortcuts. The counts describe this fixed corpus and matcher version, not a performance threshold.

Add `--smoke` after the workload name to check the diagnostic accounting on 64 records; Linux x64 Staging CI runs this bounded form. Omit it when collecting the full diagnostic.

Progress and failures go to stderr; JSON Lines output goes to stdout. The host line records the runtime, OS, architecture and reference options. Each workload line records count, byte volume, pattern count, selection totals and, in measure mode, per-engine mean elapsed milliseconds and thread allocations. Re-run on the same host under a controlled load before deciding measured thresholds. The probe uses already framed records and compiled patterns, so its timings are **matcher-component timings**, not Grep command timings and not directly comparable to the published Grep 1.6.0 command benchmark numbers.

CI runs `--smoke` on Linux x64 for correctness only. The full 16,384-record, 32-pattern and 262 KiB-record scenarios are intentionally excluded from PR CI. PCRE.NET is a tool dependency, not a library package dependency.
