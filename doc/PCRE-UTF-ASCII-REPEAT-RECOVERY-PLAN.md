# ASCII UTF Root-Repeat Recovery Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native CRLF recovery for direct root ASCII class PLUS.
**Architecture:** Remove the width-only filter from #125's existing marker selector;
reuse the immutable instruction marker and per-attempt integer endpoint.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-ASCII-REPEAT-RECOVERY-DESIGN.md.

## Global constraints and review focus

Design constraints apply; no SDK/Python/agents, all5941 baseline/frozen35 unchanged.
1. First CR with no consumed run retains native LF entry; member-run CR skips LF.
2. Nonmember/multibyte/invalid-fragment prefixes cannot supply an unrelated end.
3. Source explicit LF disables recovery; finite/min2/group/atomic/alternative
   shapes do not accidentally acquire the direct root PLUS marker.
4. Exact/exhausted observed work/cancellation stay shared; no waived comparison.
5. UTF original byte offsets, capture spans and scalar boundaries remain intact.

## Task1: Native regressions and RED

Files: tests/Pcre/PcreUtfAsciiRepeatRecoveryReferenceTests.cs; this design/plan.
Interfaces: existing Compile/Match, PCRE.NET8-bit Compiled/JIT.
- [ ] Native-first literal expectations across PLUS/count{1,} variants, offsets,
  later winners/captures and excluded shapes; six-newline/UCP/all-offset and
  strict/tolerant/fragment protocols; observed work/cancellation/no-LF controls.
- [ ] Tests-only draft, successful-build expected RED, all5941 baseline expectations
  pass; keep test blobs fixed afterRED.

## Task2: Extend existing marker and handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical PCRE inventories.
Interface: PcreByteScalarSet? FindUtfCrLfSkipRepeatSet(PcreByteAlternatives root).
- [ ] Remove only wide aggregation/filter; retain LF/CR/root/count/predicate/
  positivity/atomic and cancellation guards. No matcher, descriptor or frame change.
- [ ] Six native Staging OS/architecture jobs/net8/9/10 full/frozen214/configured
  gates green, failures/skips/compiler warnings/errors0; seven equal workloads.
- [ ] Whole-branch author review; exact head/tree/log/frozen/whitespace/clean and
  tests-afterRED audit; update #102/PR, mark ready. User owns merge.

Pre-flight: Task1 consumes unchanged compile/match results. Task2 reuses #125
instruction metadata/recovery without changing any runtime interfaces.
