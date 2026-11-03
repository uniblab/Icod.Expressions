# UTF Wrapped Star Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve bitmap entry through noncapturing wrappers with correct star-cache depth.
**Architecture:** Separate compile-time star identity from endpoint-recovery eligibility;
iteratively traverse bounded single-branch noncapturing ancestors and erased primitives.
**Tech Stack:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-WRAPPED-STAR-PREFIX-DESIGN.md.

## Constraints and review focus

Spec constraints apply; all10797 baseline expectations/frozen35 unchanged.
1. Wrapper4 consumes-to-CR recovery; wrapper5 retains bitmap without recovery.
2. Count star ancestors only, not following capture/atomic/deep group levels.
3. Preserve erasure/possessive behavior; reject explicit atomic/capturing ancestors.
4. Recognition and cache fields clear on failed final set proof; guards/budgets unchanged.
5. No matcher/Step/runtime layout or capture-state change; use existing marker.

## Task1: Native-first fixtures and observed RED

Files: tests/Pcre/PcreUtfWrappedStarPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and PcreRegex8Bit JIT reference.
- [ ] Full captures for depths/source/star modifiers/UCP, consumed vs empty runs,
  erasure combinations, suffix captures/dependencies/atomic groups and controls.
- [ ] Six newlines/every offset/strict-tolerant UTF/work/cancellation/default budget/
  parser/depth ceilings. Tests-only draft -> successful-build new-class RED with
  all10797 baseline passing; preserve original tests/expectations afterRED.

## Task2: Bounded discovery and separate cache eligibility

Files: src/Pcre/PcreByteProgram.cs; four canonical PCRE inventories.
Interfaces: FindUtfCrLfRootStarAtom(PcreByteAlternatives) -> (PcreByteNode? Atom,bool Skip),
compile-time skipUtfCrLfLeadingStar bool; existing opcode marker unchanged.
- [ ] Iterative branch/index descent through nonempty single-branch noncapturing
  groups (cost1) and existing erasures (cost2+wrapper1), CT/shared1000 bound.
  Unwrap atomic only for Repeat child; star Skip depends on ancestor depth<=4.
- [ ] Store/clear both fields; gate only star endpoint marker with Skip while
  retaining star admission identity, original leading-PLUS marker and all guards.
- [ ] Six native Staging OS/arch/net8/9/10 full/frozen214/configured gates/
  7equal workloads pass;0failed/skipped tests/build warnings/errors.
- [ ] Wholebranch author review; strict logs/head/tree/blob/tests-afterRED/frozen/
  clean/whitespace audit; issue102/PR ready/unmerged; user review/merge next.
