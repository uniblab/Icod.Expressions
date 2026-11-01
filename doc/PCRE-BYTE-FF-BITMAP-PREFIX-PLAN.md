# Byte-FF Bitmap Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native broad scanning for FF-containing exact byte bitmap repeats.
**Architecture:** Extend #122 IsBroadBitmap's source-tagged guard with the native
effective FF-bit rule; existing proof/selector supplies runtime behavior.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 JIT.
**Spec:** PCRE-BYTE-FF-BITMAP-PREFIX-DESIGN.md.

## Constraints and review focus

No agents/Python/SDK install, public/package/workflow/release/consumer/fallback or
raised-limit changes. Frozen35; membership/source flags/state unchanged.
1. Effective FF membership, not a lexical FF operand; FE-only sparse controls.
2. Ordinary/count1/group-copy exclusion, direct/wrapped exact repeats and captures.
3. Both CR/LF still required; old popcount>=6 path retained and proof bounds shared.
4. Conditional literal miss/pass, local i/r/UCP, offsets and budget/cancellation.
5. UTF valid scalar/fragments, deep/compile limits and prior ordinary work.

## Task1: Native contracts and observed RED

Files: tests/Pcre/PcreByteFfBitmapPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and pinned reference APIs only.
- [ ] Native-first full-capture fixtures, direct/group/count/case/source-bit controls;
  every offset over six newlines/two UCP choices; supported tolerant UTF controls.
- [ ] Work/exhaustion/selector charge/cancellation, sparse/atom prior budgets,
  depth/repeat ceilings. Native successful spans bounded.
- [ ] Tests-only draft, successful-build expected RED; all5734 baseline cases pass.

## Task2: Existing broad predicate and verified handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interface: existing local static bool IsBroadBitmap(PcreByteSet set).
- [ ] Keep CR/LF requirement; accept set.Contains(0xFF) OR existing popcount>=6.
  No parser/AST/source metadata/runtime/limit change.
- [ ] Six native Staging/net8/9/10 full/frozen214/configured gates green, zero
  failed/skipped/build warnings/errors; seven Linux workload equalities.
- [ ] Whole-branch author review and exact head/tree/log/frozen/whitespace/clean
  audit; update #99/PR with boundaries, mark ready; user owns merge.

Pre-flight: helper consumes the same immutable set and returns bool; Bitmap source
tag/min>=2 and all downstream proof/selector bounds remain. No interface conflict.
