# Homogeneous Sparse Byte Head Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native sparse bitmap matches for one homogeneous repeated head.
**Architecture:** Extend existing exact Bitmap admission at head, then seal further
consuming prefix widths for the non-broad subset; reuse sole literal/selector.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 JIT.
**Spec:** PCRE-BYTE-SPARSE-HEAD-PREFIX-DESIGN.md.

## Constraints and review focus

No agents/Python/SDK install, public/package/workflow/release/consumer/fallback or
raised-limit changes. Frozen35/214; membership/source flags/state unchanged.
1. Sparse must be first and only consuming prefix repeat; mixed shapes reject.
2. CR/LF membership and exact min>=2; native small2/3/4/5-set/table/pair selection.
3. Existing broad/FF mixed prefixes unaffected; atom/count1/group-copy boundaries.
4. Literal overlap, captures/local case/UCP, width/window and compile proof limits.
5. Shared budgets/cancellation/UTF; only two old eligible-work budgets16->22 change.

## Task1: Native contracts and observed RED

Files: new tests/Pcre/PcreByteSparseHeadPrefixReferenceTests.cs; design/plan;
tests/Pcre/PcreByteBitmapPrefixRepeatReferenceTests.cs and
tests/Pcre/PcreByteFfBitmapPrefixReferenceTests.cs.
Interfaces: existing Compile/Match and pinned reference APIs.
- [ ] New native-first capture/shape/count/membership protocols and shared work/
  cancellation/compile controls. Native successful spans bounded.
- [ ] BeforeRED update exactly two prior sparse budgets16->22 and rename their
  current-eligibility theory methods; other expectations unchanged.
- [ ] Tests-only draft, successful-build expected RED including those two controls;
  all5799 unchanged baseline expectations pass. No tests change afterRED.

## Task2: Compiler-only sealed sparse head and handoff

Files: src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interfaces: existing proof result/selector; compile-local bool sparseBitmapPrefix.
- [ ] Exact Bitmap min>=2 qualifies if broad OR head+CR/LF. Compute non-broad
  classification; AddWidth first, then set sparseBitmapPrefix for that repeat.
- [ ] AddWidth rejects later prefix atoms when sparseBitmapPrefix; single final
  literal and wrappers remain shared. No runtime/membership/flags/limits changes.
- [ ] Six native Staging/net8/9/10 full/frozen214/configured gates green, zero
  failed/skipped/build warnings/errors; seven Linux workload equalities.
- [ ] Whole-branch author review, exact head/tree/log/frozen/whitespace/clean audit;
  update #99/PR with work transition/scope, mark ready; user owns merge.

Pre-flight: proof bool is set only after admitted width; broad paths remain false.
All added width paths pass AddWidth, while literal does not. No runtime interface
change. Two baseline control updates are intentional/explicit and precede source.
