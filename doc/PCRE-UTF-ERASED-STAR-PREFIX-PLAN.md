# UTF Erased Star Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve first-star ownership after native-erased root primitives.
**Architecture:** Share the existing omission predicate and boundedly scan past
only those root nodes; reuse all marker/endpoint/remaining proofs unchanged.
**Tech Stack:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-ERASED-STAR-PREFIX-DESIGN.md.

## Constraints and review focus

Spec constraints apply; all9912 baseline expectations/frozen35 unchanged.
1. Only the existing six erased child families qualify; retained groups stay barriers.
2. Root identity survives erasures and still owns consumed-to-CR recovery.
3. Source ExplicitNewline flags survive omission; negated singleton exception intact.
4. CT/shared1000 proof and all existing capture/depth/source/UCP guards remain.
5. No matcher/Step/layout/resource change; no first-star wrapper scope expansion.

## Task1: Native-first fixtures and observed RED

Files: tests/Pcre/PcreUtfErasedStarPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and PcreRegex8Bit native JIT reference.
- [ ] Full captures across erasures/aliases/sequences/star families/modifiers,
  empty/consumed/source/newline/UCP/capture/dependency/deep/atomic/barrier controls.
- [ ] Six newline/every offset/strict-tolerant UTF/work/cancellation/default budget/
  parser/depth cases; tests-only draft -> successful-build new-class RED with all
  9912 baseline passing; preserve original tests/expectations afterRED.

## Task2: Shared erased-repeat proof and root discovery

Files: src/Pcre/PcreByteProgram.cs; four canonical PCRE inventories.
Interfaces: private static bool IsUtfCrLfErasedRepeat(PcreByteNode);
instance FindUtfCrLfRootStarAtom(PcreByteAlternatives), unchanged output type.
- [ ] Extract exactly the old Max0 primitive/reference predicate; use for existing
  cost2/retained-false arm and CT/1000-bounded root scan (root1+2/operand+wrapper1).
  Stop at first other node; preserve current star identity and recovery guards.
- [ ] Six native Staging platforms/net8/9/10 full/frozen214/configured gates/
  7equal workloads pass;0failed/skipped tests/build warnings/errors.
- [ ] Wholebranch author review; strict logs/head/tree/blob/tests-afterRED/frozen/
  clean/whitespace audit; issue102/PR ready/unmerged; user review/merge next.
