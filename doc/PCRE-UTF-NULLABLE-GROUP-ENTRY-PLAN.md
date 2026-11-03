# UTF Nullable Group Continuation Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Continue a proven first-star bitmap proof after nullable prefix groups.
**Architecture:** Keep old successful selector; add an iterative bounded fallback
using shared nullable-atom classification and final set validation. Consume only
its completed proof in start-class discovery; preserve runtime code.
**Tech Stack:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-NULLABLE-GROUP-ENTRY-DESIGN.md.

## Constraints and review focus

Spec constraints apply; all14898 baseline expectations/frozen35 unchanged.
1. Resume only fully checked nullable branches; required/CR-admitting operands stop.
2. First-star cache depth/capture/atomic ownership survives group closure.
3. Empty participating capture spans remain correct; no VM instructions removed.
4. Unsupported/budget failures differ from nullable success, no repeated scans.
5. Fallback requires proven first-star identity and at least one actual group return.

## Task1: Native-first fixtures and observed RED

Files: tests/Pcre/PcreUtfNullableGroupEntryReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match/PcreRegex8Bit JIT reference.
- [ ] Full literal captures for group types/depths/mixed/sequential/nested closures,
  modifiers/source/UCP/casing/prefixes/dependencies and required/unsupported controls.
- [ ] Six newline/offset/strict-tolerant fragments/work/cancellation/default budget/
  parser/capture/backtrack ceilings.
- [ ] Tests-only draft; successful-build new-class RED/all14898baseline pass.
  Keep original tests/expectations unchanged afterward.

## Task2: Bounded continuation proof

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interfaces: FindUtfCrLfLeadingNode(branch,ref visits,startIndex=0,blocksSkip=false)
returns (Node,BlocksSkip,NextIndex,Nullable); shared final-set validator;
FindUtfCrLfJoinedRepeat(root)->Set?; compile-only joined-proof bool.
- [ ] Extend helper outputs/default inputs; distinguish full nullable consumption
  from budget failure. Existing consumers retain old behavior.
- [ ] Extract guarded mandatory-set validation for old and new selectors.
- [ ] Fallback walks groups and continuations once under CT/1000, carries retained
  state, classifies captures/eligibility, requires a group return; no PLUS Skip.
- [ ] Compile invokes only after old proof fails/proven star; HasUtfCrLfStartClass
  consumes completed fallback proof, otherwise retains generic traversal.
- [ ] Six native Staging/net8/9/10 full/frozen214/configured gates/7equal workloads,
  0failed/skipped tests/buildwarnings/errors; author wholebranch review and strict
  logs/head/tree/blob/testsafterRED/frozen/clean audit, issue102 open/PR ready.
