# UTF Pruned Reference Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native candidate entry after leading direct zero refs disappear.
**Architecture:** Shared bounded first-node selection; preserve existing leading
repeat Set/Skip and restrict start-proof skipping to a recognized repeat chain.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-PRUNED-REFERENCE-PREFIX-DESIGN.md.

## Global constraints and review focus

Spec constraints apply; all6567 baseline expectations/frozen35 unchanged.
1. Root/interior omitted refs are transparent; retained groups/other starts are not.
2. Possessive zero AtomicRepeat vanishes; explicit atomic/group shapes remain.
3. Live trailing dependencies and depth4/5 retain distinct native recovery.
4. All-omitted bodies return no proof; no-LF/CR/G/UTF/newline/offset guards remain.
5. Charge omitted2/3 source nodes per1000 budget with CT; all runtime caps stay.

## Task1: Native-first regressions and observed RED

Files: tests/Pcre/PcreUtfPrunedPrefixReferenceTests.cs; design/plan.
Interfaces: unchanged Compile/Match/reference PcreRegex8Bit Compiled/JIT.
- [ ] Pin literal full spans for roots/interiors/aliases/quantifiers/depth/later
  live refs and all-omitted/retained/unknown/no-LF/CR/G controls.
- [ ] Six newline/UCP/every-offset/strict-tolerant, work/exhaustion/cancellation/
  default budget/parser-depth tests. Publish tests-only draft and inspect successful
  build expected RED with6567 baseline passing; freeze test blobs afterRED.

## Task2: Shared compiler first-node helper and handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interfaces: new private PcreByteNode? FindUtfCrLfLeadingNode(
IReadOnlyList<PcreByteNode> branch, ref int visits); existing Set/Skip outputs.
- [ ] Skip only direct max0 Backreference (cost2) or AtomicRepeat0Ref (cost3), CT
  and1000 guard; null for exhausted/all-omitted branch; no other node elision.
- [ ] Selector uses helper/root and group bodies with budget/group charging.
  Proof uses helper only when selected repeat leaf exists; keep old first-node
  path otherwise. Preserve depth/identity/slot/terminal guards.
- [ ] Six native Staging platforms/net8/9/10 full/frozen214/configured gates green,
  failed/skipped tests/compiler warnings/errors0,7 equal workloads.
- [ ] Whole-branch author review/exact head-tree-log/tests-afterRED/frozen/clean/
  whitespace audit; update102/PR, mark ready, preserve artifacts; user owns merge.

Self-check: one helper/interface shared; selected semantic nodes charged by callers;
only pruned nodes charged in helper; proof gating preserves conservative scope.
