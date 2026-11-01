# UTF Noncapture Group Repeat Recovery Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native leading class PLUS recovery through four noncapture groups.
**Architecture:** Bounded iterative descent in the existing selector; reuse existing
AST identity markers, VM comparisons and integer repeat endpoint.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-GROUP-REPEAT-RECOVERY-DESIGN.md.

## Global constraints and review focus

All design constraints apply; all6022 baseline expectations/frozen35 unchanged.
1. Four versus five group descent: marking depth5 can lose native LF matches.
2. Single-branch noncapture only; alternatives/atomic/capture/quantified groups
   do not inherit an optimizer they cannot prove.
3. First CR versus failed member run, explicit caller offsets and malformed gaps.
4. Local case/source-newline/full captures and existing wide/root cases preserved.
5. All newly visited/skipped candidates retain observed work/exhaustion/cancellation.

## Task1: Native-first regressions and observed RED

Files: tests/Pcre/PcreUtfGroupRepeatRecoveryReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and PCRE.NET PcreRegex8Bit Compiled/JIT.
- [ ] Pin literal spans for depth1..4 and depth5/6 boundaries, ASCII/wide and
  greedy/lazy/possessive/count forms; shape controls, six-newline/UCP/all-offset
  and strict/tolerant/fragment protocols, shared work/cancellation/no-LF controls.
- [ ] Publish tests-only draft; successful-build expected RED with all6022 baseline
  passing. Keep test blobs fixed afterRED.

## Task2: Extend bounded group selection and handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical PCRE inventories.
Interface: existing PcreByteScalarSet? FindUtfCrLfSkipRepeatSet(root); unchanged.
- [ ] Iteratively follow up to4 PcreByteNoncapturingGroup.Body first nodes, each
  single nonempty branch; keep all existing terminal repeat/atomic/set guards.
  Check cancellation per descent; no recursive compiler walk or runtime change.
- [ ] Six native Staging platforms/net8/net9/net10 full/frozen214/configured gates
  green; failures/skips/compiler warnings/errors0 and seven equal workloads.
- [ ] Whole-branch author review; exact head/tree/log/frozen/whitespace/clean and
  tests-afterRED audit; update102/PR and mark ready; user owns merge.

Pre-flight: tests consume unchanged compile/match results; leaf selection reuses
the existing instruction marker and recovery without a new runtime interface.
