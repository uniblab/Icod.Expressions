# Grep PCRE component workload probe plan

**Goal:** produce reproducible component-level correctness and timing evidence for Grep's seven current PCRE benchmark inputs.

**Architecture:** add a separate test-only console tool that constructs Grep's pinned workload records and patterns, compiles both matchers with Grep's C/POSIX or UTF-8 options, compares exact spans, and times alternate execution rounds only in explicit measure mode. CI runs short correctness smoke on Linux x64; full measurements stay manual.

**Tech Stack:** C# 13, net10.0, PCRE.NET 1.6.0 test-only, Staging CI.

**Spec:** `doc/PCRE-GREP-WORKLOAD-PROBE-DESIGN.md` and pinned Grep benchmark sources identified there.

## Global constraints

- Source and solution/project files remain in repo root; tool code lives in `/tools`; no production dependency change.
- Preserve Grep's seven workload names, record counts, target lengths, periodicity, patterns and mode options.
- Measure component matching only; never compare these numbers directly to full-command times.
- No subagents; no consumer or CommandFramework edits.

## Task 1: deterministic workload probe

**Files:** add `tools/PcreWorkloadComparison/PcreWorkloadComparison.csproj`, `tools/PcreWorkloadComparison/Program.cs`; update `src/Properties/AssemblyInfo.cs` for internal test-tool access.

- [ ] Generate the seven pinned workloads and both sets of compiled patterns.
- [ ] Compare exact per-pattern byte results and record selection; model dense `-o` repeated-start behavior.
- [ ] Fail promptly with case and record on mismatch or diagnostic.
- [ ] Add `--smoke` and `--measure [--iterations N]` with machine-readable measured output.

## Task 2: CI and documentation

**Files:** update `.github/workflows/pull-request.yml`, `doc/PCRE-DESIGN.md`; add `doc/PCRE-GREP-WORKLOAD-PROBE-DESIGN.md`.

- [ ] Run `--smoke` on Linux x64 in Staging after the full test suite.
- [ ] Document how to run full measurements on the physical reference host and why the output is component-only.
- [ ] Verify all six CI jobs, frozen 214-case corpus, package smoke and exact PR head; self-review the diff.
