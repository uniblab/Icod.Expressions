# UTF Captured Star Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve first-star bitmap entry and capture-dependent endpoint recovery.
**Architecture:** Extend bounded ancestor discovery and reuse existing capture classifier.
**Tech Stack:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-CAPTURED-STAR-PREFIX-DESIGN.md.

## Constraints and review focus

Spec constraints apply; all11718 baseline expectations/frozen35 unchanged.
1. Related ancestor reference/test disables Skip, not bitmap identity.
2. Direct reference0 erased; retained zero-group references remain dependencies.
3. Only star ancestors count: unrelated/following captures must not disable Skip.
4. Mixed group depth4 vs5; explicit atomic ancestors remain excluded.
5. Existing proof/capture/parser/work/depth/cancellation ceilings unchanged.

## Task1: Native-first behavioral fixtures and observed RED

Files: tests/Pcre/PcreUtfCapturedStarPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and PcreRegex8Bit JIT reference.
- [ ] Pin full captures for ancestry/dependency/source/modifier/casing/UCP,
  erasures, mixed/deep wrappers, named/duplicate numbers, unsupported controls.
- [ ] Pin six newline/every offset/strict-tolerant fragments/work/cancellation/
  default budget/parser/capture/backtrack limits.
- [ ] Tests-only commit/draft; successful-build new-class RED, all11718baseline pass.
  Freeze original tests/expectations after RED.

## Task2: Bounded capturing descent and dependency-aware Skip

Files: src/Pcre/PcreByteProgram.cs; four canonical PCRE inventories.
Interfaces: existing FindUtfCrLfRootStarAtom(PcreByteAlternatives)->(Atom,Skip)
and ClassifyUtfCaptureRepeat(root,IReadOnlySet<int>)->(Eligible,Skip).
- [ ] Descend Group/NoncapturingGroup single nonempty bodies with existing group1
  cost/depth. Collect ancestor capture numbers only; classify at first star.
  Reject ineligible proof; Skip=depth<=4 && classifier.Skip, identity preserved.
- [ ] Keep all other guards/markers/runtime paths; update inventories.
- [ ] Six native Staging OS/arch/net8/9/10 full/frozen214/configured gates/
  7equal workloads pass;0failed/skipped tests/buildwarnings/errors.
- [ ] Wholebranch author review; strict logs/head/tree/blob/tests-afterRED/frozen/
  clean/whitespace audit; update issue102/PR ready/unmerged. User review/merge.
