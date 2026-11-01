# UTF Deep Noncapture Repeat Entry Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native bitmap LF entry for known deep wide class PLUS prefixes.
**Architecture:** Compile-time (Set, Skip) classification; recognize transparent
noncapture chains independently of native depth4 repeat acceleration.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-DEEP-REPEAT-ENTRY-DESIGN.md.

## Global constraints and review focus

Design constraints apply; all6108 baseline expectations/frozen35 unchanged.
1. Depth4 still skips LF after a failed run; depth5+ must retain LF.
2. Recognition cannot accidentally enable a marker for an unaccelerated repeat.
3. Empty/multiple/unsupported prefix nodes and no-LF/CR-including sets stay guarded.
4. Captures, UTF caller offsets/fragments and local/source-newline policies shared.
5. Proof1000/parser256/work/storage limits unchanged; observed work/cancellation paid.

## Task1: Native-first regressions and observed RED

Files: tests/Pcre/PcreUtfDeepRepeatEntryReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and PCRE.NET PcreRegex8Bit Compiled/JIT.
- [ ] Pin literal spans across depth4/5/6/34/200, ASCII/wide/quantifiers and shape
  controls; six-newline/UCP/all-offset and strict/tolerant/fragment protocols.
- [ ] Shared work/exhaustion/cancellation, no-LF budget and depth cap controls.
- [ ] Tests-only draft successful-build expected RED; all6108 baseline pass and
  test blobs fixed afterRED.

## Task2: Split class recognition from skip marker and handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interfaces: private (PcreByteScalarSet? Set, bool Skip) FindUtfCrLfLeadingRepeat(root),
compile-only selected set and Skip bool; runtime interfaces remain unchanged.
- [ ] Recognize bounded noncapture chains with terminal guards; return Skip only
  for depth<=4. Existing start-class proof checks leaf identity independent of Skip.
- [ ] Emit existing scalar instruction marker only when classification.Skip and
  leaf identity both hold. No matcher, record layout, saved frame/state or cap change.
- [ ] Six native Staging platforms/net8/9/10 full/frozen214/configured gates green;
  failures/skips/compiler warnings/errors0; seven equal workloads.
- [ ] Whole-branch author review and exact head/tree/log/frozen/whitespace/clean/
  tests-afterRED audit; update102/PR; mark ready; user owns merge.

Pre-flight: tests consume unchanged compile/match outputs; classifier Set feeds
the existing proof, Skip feeds the existing marker; the decisions must stay separate.
