# UTF Capture Dependencies Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native LF entry and repeat recovery according to capture identity.
**Architecture:** Prefix-number set plus Eligible/Skip result from existing bounded
compiler worklist; unchanged marker/proof/matcher interfaces.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-CAPTURE-DEPENDENCIES-DESIGN.md.

## Global constraints and review focus

Spec constraints apply; all6309 baseline expectations/frozen35 unchanged.
1. Prefix-slot dependencies clear Skip; later-slot dependencies retain Skip.
2. All duplicate targets and condition-body references are observed globally.
3. Grouped zero-count dependency bodies remain; direct zero-count reference and
   calls/DEFINE/other conditions stay conservative without changing baseline.
4. Shared wrapper depth4/5, full byte captures, UTF/newline/fragment/offsets stay.
5. Compiler1000 branch/node/queue bounds,256 prefix slots, cancellation and all
   runtime resource ceilings stay; work is exact/observed/cancellable.

## Task1: Native-first regressions and observed RED

Files: tests/Pcre/PcreUtfCaptureDependencyReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match/reference PcreRegex8Bit Compiled/JIT.
- [ ] Literal full spans for related/later numeric/named/duplicate slot references/
  conditions, group depth/quantifiers/assertion/zero-group and conservative controls.
- [ ] Every offset/six newlines/UCP/strict-tolerant fragments; work/cancellation/
  no-LF budget/parser depth. Publish tests-only draft and inspect successful-build
  expected RED with6309 baseline passing; freeze test blobs afterRED.

## Task2: Compiler dependency classification and handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interfaces: existing FindUtfCrLfLeadingRepeat; new private
(bool Eligible, bool Skip) ClassifyUtfCaptureRepeat(PcreByteAlternatives root,
IReadOnlySet<int> captures).
- [ ] Collect traversed prefix slot numbers locally; replace bool helper with
  independent eligibility/skip result and identity intersection scan.
- [ ] Observe resolved reference/condition targets and condition bodies, reject
  direct zero-count reference repeats and prior unknowns, preserve bounded queues/
  cancellation/no-runtime-change. Final marker requires depth<=4 AND helper.Skip.
- [ ] Six native Staging platforms/net8/9/10 full/frozen214/configured gates green,
  failed/skipped tests/compiler warnings/errors0,7 equal workloads.
- [ ] Whole-branch author review/exact head-tree/log/tests-afterRED/frozen/clean/
  whitespace audit; update102/PR, mark ready, preserve evidence; user owns merge.

Self-check: all signatures/result fields consistent; set lifetime compilation-only;
existing root/noncapture selection unchanged; unsupported pruning is conservative.
