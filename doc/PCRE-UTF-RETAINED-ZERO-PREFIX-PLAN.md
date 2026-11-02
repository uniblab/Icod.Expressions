# UTF Retained Zero Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Keep native bitmap entry but block repeat skipping after retained brackets.
**Architecture:** Shared first-node helper returns Node/BlocksSkip; compiler
accumulates barrier across prefixes, proof ignores barrier and uses Node only.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-RETAINED-ZERO-PREFIX-DESIGN.md.

## Global constraints and review focus

Spec constraints apply; all6668 baseline expectations/frozen35 unchanged.
1. Retained prefix brackets block Skip even when empty or their inner ref is pruned.
2. Omitted prefixes keep prior acceleration; trailing retained brackets do not block.
3. Unset prefix captures/dependencies/atomic and assertion source forms remain shared.
4. Root/interior barriers accumulate with depth4/5; all-skipped sequences no proof.
5. Source proof1000/CT and all runtime storage/work ceilings unchanged.

## Task1: Native-first regressions and observed RED

Files: tests/Pcre/PcreUtfRetainedZeroPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match/reference PcreRegex8Bit Compiled/JIT.
- [ ] Literal full spans for retained kinds/quantifiers/unset captures/depth/
  combinations/live refs/trailing and conservative controls.
- [ ] Six newline/UCP/all offsets/strict-tolerant, work/exhaustion/cancellation/
  default budget/parser tests. Publish tests-only draft; inspect successful-build
  expected RED with6668 baseline passing; freeze test blobs afterRED.

## Task2: Compiler barrier classification and handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interfaces: (PcreByteNode? Node, bool BlocksSkip) FindUtfCrLfLeadingNode; existing
Set/Skip classifier outputs.
- [ ] Classify direct0ref versus retained bracket types and optional possessive
  wrapper, charge2/3 plus wrapper1, CT/1000 guard; preserve unknown boundaries.
- [ ] Accumulate !BlocksSkip across root/group bodies and AND final depth/capture
  eligibility. Proof uses .Node under existing selected-leaf guard; global
  dependency scan still traverses retained bodies.
- [ ] Six native Staging platforms/net8/9/10 full/frozen214/configured gates green,
  failed/skipped tests/compiler warnings/errors0,7 equal workloads.
- [ ] Whole-branch author review/exact head-tree-log/tests-afterRED/frozen/clean/
  whitespace audit; update102/PR, mark ready, preserve artifacts; user owns merge.

Self-check: tuple/field names consistent; barrier compilation-only and monotonic;
first-node selection skips bracket execution only, not global dependencies.
