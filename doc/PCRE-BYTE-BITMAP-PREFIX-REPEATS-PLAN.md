# Broad Byte Bitmap Repeat Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve bounded native broad byte bitmap exact-repeat prefix scanning.
**Architecture:** Source-tag ordinary bracket bitmaps, keep atom admission false,
and validate broad CR/LF-containing direct repeats in the existing compiler proof.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 JIT.
**Spec:** PCRE-BYTE-BITMAP-PREFIX-REPEATS-DESIGN.md.

## Constraints and review focus

No agents/Python/SDK install, public/package/workflow/release/consumer/fallback or
raised-limit changes. Frozen35 intact; source flags/membership/state unchanged.
1. Bitmap atoms/count1/group copies remain excluded; direct H/V provenance stays.
2. Native broad six-byte threshold and CR+LF membership; sparse cases stay bounded.
3. UCP/ASCII/case/POSIX distinction and #121 mixed-property membership intact.
4. Count2/11/12, actual byte offsets, sole literal and conditional miss controls.
5. Original captures, exact/exhausted budgets, cancellation, UTF and compile limits.

## Task1: Native contracts and observed RED

Files: tests/Pcre/PcreByteBitmapPrefixRepeatReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and pinned reference APIs only.
- [ ] Native-first capture/span rows with positive/miss and shape/policy controls;
  every offset over six newlines/two UCP choices; supported UTF controls.
- [ ] Observed/exact/exhausted work, selector/cancellation, sparse/single-atom prior
  behavior and depth/repeat ceilings. Native successful spans bounded.
- [ ] Tests-only draft, observed successful-build expected RED; old5662 baseline pass.

## Task2: Source-tagged proof and handoff

Files: src/Pcre/PcreByteSyntax.cs, PcreByteParser.cs, PcreByteProgram.cs;
four canonical inventories.
Interfaces: new PcreByteClassPrefix.Bitmap; PrefixAny explicitly excludes it;
local static bool IsBroadBitmap(PcreByteSet set) checks CR/LF and popcount>=6.
- [ ] Otherwise-None ordinary brackets receive Bitmap. Direct None untouched.
- [ ] Exact direct Bitmap repeats require min>=2 plus IsBroadBitmap; existing
  width/proof/selector/group/source-kind rules supply remaining behavior.
- [ ] Six native Staging/net8/9/10 full/frozen214/configured gates green, zero
  failed/skipped/build warnings/errors; seven Linux workload equalities.
- [ ] Whole-branch author review and exact head/tree/log/frozen/whitespace/clean
  audit; update #99/PR with boundaries, mark ready; user owns merge.

Pre-flight: parser producer and both atom/repeat consumers change together. Old
property and direct type consumers retain Exact/Atom/AnyProperty; no runtime/API
interface change. Sparse/metadata/general optimizer gates remain explicit.
