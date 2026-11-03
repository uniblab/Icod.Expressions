# UTF nullable-alternative continuation implementation plan

> Execute inline with superpowers:executing-plans; no subagents.

**Goal:** Resume the bounded UTF bitmap proof after all branches of a nullable group.
**Architecture:** Extend the existing compile-time continuation tuple with body/
next-branch metadata; reuse shared atom/set/capture proofs and all runtime code.
**Tech:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-NULLABLE-ALTERNATIVE-ENTRY-DESIGN.md.

## Global constraints and review focus

Spec constraints apply; all17256 baseline expectations/frozen35 unchanged.
1. Late failing branches invalidate the whole proof, regardless of branch order.
2. Nested returns cannot accept a mandatory set while an alternative is pending.
3. Empty/branch-reset captures retain native participation and byte spans.
4. Branch stars gain bitmap proof without taking an earlier root star's recovery.
5. Empty/additional branches consume bounded visits and preserve resource limits.

## Task1: Native fixtures and observed RED

Files: tests/Pcre/PcreUtfNullableAlternativeEntryReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match/PcreRegex8Bit JIT reference.
- [ ] Native full-capture literals for group/branch/depth/order/source/modifier/
  capture/recovery/exclusion cases, plus bounded six-newline/UCP/every-offset probes.
- [ ] Strict/tolerant fragments, work exact/exhausted/observed, cancellation,
  default budget/parser257/capture257/backtrack limits and empty-branch breadth.
- [ ] Tests-only draft; successful-build failures confined to the new class and
  all17256baseline pass. Keep tests/expectations unchanged afterward.

## Task2: Bounded all-branch continuation

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interface: FindUtfCrLfJoinedRepeat(root)->PcreByteScalarSet? remains unchanged.
- [ ] Continuation tuple gains PcreByteAlternatives Body/int NextBranch. A local
  pending-alternative count blocks final-set selection until every branch closes.
- [ ] Enter nonzero branch counts; alternatives set BlocksSkip. Nullable completion
  schedules/charges the next branch or returns to parent after the last branch.
- [ ] Preserve old successful paths, root/first-star policy, shared classifiers,
  final-set/capture eligibility/actual-return/CT1000 guards and PLUS Skip=false.
- [ ] Verify unchanged tests/frozen35/whitespace; six native Staging full/frozen214
  ×net8/9/10/configured gates/7equal workloads,0failed/skipped tests/buildwarnings/
  errors. Author wholebranch review/strict logs/exacthead/tree/blob/clean audit;
  issue102 stays open, ready PR stays unmerged for user review.
