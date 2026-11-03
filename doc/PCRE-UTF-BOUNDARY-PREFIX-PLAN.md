# UTF word-boundary prefix implementation plan

> Execute inline; no subagents.

**Goal:** Cross word-boundary assertions in existing bounded UTF PLUS and
nullable-group proofs without changing recovery ownership.
**Architecture:** Reuse the two compiler analysis walkers; one visit per boundary.
**Tech:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-BOUNDARY-PREFIX-DESIGN.md.

## Constraints and review focus
Spec invariants apply. Zero-width boundaries do not consume early-fail count;
primitive stars remain owners, returned groups still block following PLUS,
ASCII/UCP restrictions retain parser semantics, all proof visits stay bounded,
and malformed input/caller offsets retain original byte capture coordinates.

## Task1: Native regression contract and observed RED
Files: tests/Pcre/PcreUtfBoundaryPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and PcreRegex8Bit JIT reference.
- [ ] Pin full-capture literals for boundaries and owner/group/guard/exclusion
  cases; six newline/UCP/every-offset malformed fragments and strict validation.
- [ ] Pin observed/exact/exhausted work, cancellation, default budget and bounded
  analysis controls. Expectations derive from native reference, never managed.
- [ ] Publish tests-only draft; observe successful build/new-class failures with
  all21932 baseline pass; record test blob and preserve it thereafter.

## Task2: Reuse boundary-aware proof traversal
Files: src/Pcre/PcreByteProgram.cs; four canonical inventory appendices.
Interfaces: FindUtfCrLfLeadingNode tuple and FindUtfCrLfRootStarAtom tuple unchanged.
- [ ] Shared leading-node walker recognizes PcreByteWordBoundary with cost1,
  retainedfalse. Root first-star finder crosses it with cost1 before existing
  atomic/erased/group/depth/capture checks. No other admission changes.
- [ ] Existing cancellation/budget guards cover each visit; do not modify emitters
  or runtime. Document bounded scope and remaining #102 gates.
- [ ] Preserve immutable tests and frozen35; audit six native Staging jobs,
  full/frozen214/net8/9/10, configured gates/seven equal workloads and zero
  failures/skips/compiler warnings/errors. Review whole branch, match remote
  tree/eight blobs, clean local workspace; ready PR remains unmerged.
