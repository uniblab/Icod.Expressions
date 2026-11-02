# UTF Pruned Atom Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native entry and acceleration after omitted zero-count atoms.
**Architecture:** Extend the existing cost-two/nonretained first-node arm; reuse
Node/BlocksSkip, capture classification, proof and existing runtime markers.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-PRUNED-ATOM-PREFIX-DESIGN.md.

## Global constraints and review focus

Spec constraints apply. All6789 baseline expectations/frozen35 remain unchanged.
1. Omitted literals/types/classes/newline sequences preserve acceleration.
2. Pruned newline atoms retain source newline metadata and LF entry behavior.
3. Retained zero brackets block acceleration, even around pruned atoms.
4. Root/interior/deep groups and related capture dependencies keep existing rules.
5. All-skipped/nonzero/unknown prefixes retain conservative proof/CT1000 bounds.

## Task1: Native-first regressions and observed RED

Files: tests/Pcre/PcreUtfPrunedAtomPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and reference PcreRegex8Bit Compiled/JIT.
- [ ] Pin literal full spans for supported atom kinds/modifiers, source flags,
  root/interior/deep groups, references, retained/trailing/all-skipped controls.
- [ ] Six newline/UCP/all offsets/strict-tolerant protocols, work/exhaustion/
  cancellation/default budget/parser257 tests. Publish tests-only draft; inspect
  successful-build RED with6789 baseline passing; freeze test blobs afterward.

## Task2: Shared omitted-atom classification and handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical PCRE inventories.
Interfaces: existing (PcreByteNode? Node, bool BlocksSkip) FindUtfCrLfLeadingNode.
- [ ] Add Literal/Any/ScalarClass/ScalarSet/NewlineSequence to direct max-zero
  Backreference cost-two/nonretained arm; outer possessive wrapper costs one.
  Preserve retained barriers, source metadata and all admission/boundary guards.
- [ ] Six native Staging platforms/net8/9/10 full/frozen214/configured gates pass,
  zero failed/skipped tests/compiler warnings/errors; seven equal workloads.
- [ ] Whole-branch author review and exact head/tree/log/frozen/tests-after-RED/
  clean/whitespace audit. Update102/PR, mark ready, preserve evidence; user merges.

Self-check: no new classifier result or runtime storage; source metadata is never
recomputed from the selected node. Prefix barriers accumulate monotonically.
