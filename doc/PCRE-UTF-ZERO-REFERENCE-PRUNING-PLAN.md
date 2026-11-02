# UTF Zero-Count Reference Pruning Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native dependency effects after direct zero-count references vanish.
**Architecture:** One compiler classifier case ignores its direct child; existing
Set/Skip proof/markers/VM zero-repeat handling remain shared.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-ZERO-REFERENCE-PRUNING-DESIGN.md.

## Global constraints and review focus

Spec constraints apply; all6464 baseline expectations/frozen35 unchanged.
1. Direct zero backreferences restore depth<=4 Skip; live/grouped refs still clear it.
2. Nested group0/reference0 and named/duplicate zero targets follow native spans.
3. Retained zero assertion/condition bodies still contribute dependencies globally.
4. Depth4/5/34/200, UTF fragments/offsets/newline and full byte captures remain shared.
5. Existing compile1000/slots256/CT and runtime caps/work accounting unchanged.

## Task1: Native-first regressions and observed RED

Files: tests/Pcre/PcreUtfZeroReferencePruningReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match/reference PcreRegex8Bit Compiled/JIT.
- [ ] Pin literal full spans for direct zero versus retained bodies/nested pruning/
  later live refs, quantifiers/depth/aliases and conservative call/DEFINE controls.
- [ ] Six newline/UCP/every-offset/strict-tolerant and work/cancellation/no-LF/
  parser-cap tests. Publish tests-only draft; inspect successful-build expected RED
  with6464 baseline passing; freeze tests afterRED.

## Task2: Minimal classifier change and handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interfaces: unchanged (bool Eligible, bool Skip) ClassifyUtfCaptureRepeat.
- [ ] Change direct Backreference repeat max0 handler from return default to break
  and explain native omission; retain generic zero-group traversal and all guards.
- [ ] Verify six native Staging platforms/net8/9/10 full/frozen214/configured gates,
  zero failed/skipped tests/compiler warnings/errors and7 equal workloads.
- [ ] Whole-branch author review/exact head-tree-log/tests-afterRED/frozen/clean/
  whitespace audit; update102/PR, mark ready, preserve artifacts; user owns merge.

Self-check: one classifier case, no changed signatures/layouts; all focus inputs
covered by literal/protocol/resource cases; no scope or permission questions needed.
