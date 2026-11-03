# UTF Star Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve supported star bitmap entry and consumed-prefix CRLF recovery.
**Architecture:** Reuse atom/source proofs and the existing instruction endpoint
bit/per-attempt integer; root identity or local retained barrier gates star admission.
**Tech Stack:** C#13/net8/9/10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-UTF-STAR-PREFIX-DESIGN.md.

## Constraints and review focus

Spec constraints apply; all8993 baseline expectations/frozen35 unchanged.
1. Root consumed runs ending at CR must not gain false LF matches; empty runs do.
2. Only first root star records an endpoint; local retained barriers suppress it.
3. Casing/UCP/source/property/CR/fragment/caller guards remain intact.
4. Following capture references/deep groups must not clear root-prefix recovery.
5. Original stepping, budgets/cancellation/depth/ExplicitNewline remain unchanged.

## Task1: Native-first fixtures and observed RED

Files: tests/Pcre/PcreUtfStarPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and PcreRegex8Bit JIT reference.
- [ ] Full captures for star families/modifiers/aliases/source/UCP, consumed vs
  empty runs ending at CR, overlaps/barriers/mixed/root-before-capture/depth cases.
- [ ] Six newline/every offset/strict-tolerant UTF, exact/exhausted/observed work,
  cancellation/default budget/parser257 and depth rejection; conservative controls.
- [ ] Tests-only draft -> successful-build new-class RED/all8993 baseline pass;
  preserve expectations/test blobs afterRED.

## Task2: Bounded selector and existing endpoint recovery

Files: src/Pcre/PcreByteProgram.cs, src/Pcre/PcreByteMatcher.cs; four canonical
PCRE design/fixture/resource-contract/syntax inventories.
Interfaces: compile-time PcreByteNode? root identity; private helpers root-star
selection and CanCrossUtfCrLfOptionalRepeat(PcreByteRepeat,bool); existing
SkipCrLfAfterRepeat bit/utfRepeatEnd integer, no runtime layout change.
- [ ] Record root-first star child (unwrap possessive atom), gate four optional
  cases with max>0 plus finite/local barrier/root identity; clear identity if
  following mandatory set PLUS selection fails. Keep all classifiers/costs/guards.
- [ ] Emit root identity marker on Literal/ScalarClass/ScalarSet. After charged
  successful literal/type consumption, reuse same endpoint update as ScalarSet;
  retain existing reset/recovery/Scope guards/Step/limits and update comments.
- [ ] Six native Staging OS/arch/net8/9/10 full/frozen214/configured gates/
  7equal workloads pass;0failed/skipped tests/build warnings/errors.
- [ ] Wholebranch author review; strict logs/head/tree/blob/tests-afterRED/
  frozen/clean/whitespace audit; issue102/PR ready/unmerged; user review/merge next.

Original-fixture completion: the bounded group walk must descend an atomic
noncapturing wrapper behind !skip/root-star identity. Tests already pin this
source shape before implementation; no expectation changes are permitted.
