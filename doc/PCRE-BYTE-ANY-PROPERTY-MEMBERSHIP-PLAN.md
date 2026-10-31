# Byte Mixed-Any Property Membership Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Reproduce pinned native byte mixed-Any property membership.
**Architecture:** Aggregate property-only membership and ordinary bitmap-source
presence during existing byte bracket parsing; select before outer negation.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 JIT.
**Spec:** PCRE-BYTE-ANY-PROPERTY-MEMBERSHIP-DESIGN.md.

## Constraints and review focus

No agents/Python/SDK install, public/package/workflow/release/consumer/fallback or
raised-limit changes. Frozen35 intact; original spans/captures/work preserved.
1. Both property orders, caret/outer/P complements and positive versus empty Any.
2. Literal/range/nonempty quote/bitmap keeps the full set; empty quote/xx does not.
3. UCP/ASCII/caseless/POSIX lowering uses existing source kinds, not semantic size.
4. UTF scalar union/fragment barriers and existing byte prefix proof unchanged.
5. Shared captures/rollback/work/exhaustion/cancellation and compile ceilings.

## Task1: Native contracts and observed RED

Files: tests/Pcre/PcreByteAnyPropertyMembershipReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and pinned reference APIs only.
- [ ] Every-byte anchored membership and native literal full-capture fixtures;
  normal/negated/caret/mixed-source/UCP/ASCII/case/xx controls.
- [ ] Every caller offset over six newlines, supported UTF controls, shared
  observed/exact/exhausted budgets/cancellation and deep/compile ceilings.
- [ ] Native spans bounded, membership JIT/interpreter agree. Publish tests-only
  draft, observe successful-build expected RED; old5566 baseline passes.

## Task2: Compile-time membership and verified handoff

Files: src/Pcre/PcreByteParser.cs; four canonical inventories.
Interfaces: existing ParseBracket item PrefixAny/XclassProperty fields; existing
PcreByteClass.Set result. No new field/API/runtime contract.
- [ ] Aggregate property-only set and bitmap-source presence. Select property set
  iff actual property data exists without ordinary bitmap source, then negate.
- [ ] Keep source flags/provenance/grammar/case/UTF/state/limits unchanged.
- [ ] Six native Staging jobs/net8/9/10 full/frozen214/configured gates green,
  zero failed/skipped/build warnings/errors, seven Linux workload equalities.
- [ ] Whole-branch author review, exact head/tree/log/frozen/whitespace/clean audit;
  update #99/PR with pinned-native ruling and remaining gaps; mark ready.

Pre-flight: item tuple flags from #120 identify actual properties and bitmap
sources; literal range endpoints cannot be properties. Existing class consumers
use the selected Set without new interfaces. No task/spec interface conflict.
