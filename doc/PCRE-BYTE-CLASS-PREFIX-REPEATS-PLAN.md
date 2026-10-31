# Repeated Byte Class Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native byte exact-repeat prefix scanning with source provenance.
**Architecture:** PcreByteClass None/Atom/Exact enum; existing compiler proof and #117
runtime selector; no matcher changes.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0/PCRE2 10.48 JIT.
**Spec:** PCRE-BYTE-CLASS-PREFIX-REPEATS-DESIGN.md.

## Global constraints and review focus

No agents/Python/SDK install, public/package/workflow/release/consumer/fallback or
raised-limit changes. Frozen35 intact; original offsets/captures/budgets retained.
1. Equal byte memberships with different native class/property encodings differ.
2. Singleton/equal range source shortcuts versus duplicates; preserve newline flags.
3. UCP/local ASCII policies and direct type versus bracket-property aggregation.
4. Exact1/2/11/12, atomic/capture wrappers; no repeated group/bounds inference.
5. Existing selector work/observer/cancellation, UTF and compile ceilings stay shared.

## Task1: Native contracts and observed RED

Files: tests/Pcre/PcreByteClassPrefixRepeatReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and pinned reference APIs only.
- [ ] Pin native positive/miss full captures, source-property/bitmap controls,
  direct shorthand/dot, fixed1/2/11/12 and local case/ASCII/UCP policies.
- [ ] All-offset six-newline/two-UCP and supported UTF controls, shared work/exact/
  exhausted/cancellation/deep/resource cases; native spans bounded.
- [ ] Publish tests-only draft; observe expected RED/build success, old5336 pass.

## Task2: Source provenance and existing proof

Files: src/Pcre/PcreByteSyntax.cs, src/Pcre/PcreByteParser.cs,
src/Pcre/PcreByteProgram.cs; four canonical inventories.
Interfaces: `PcreByteClassPrefix { None, Atom, Exact }`; class PrefixKind defaultNone,
computed PrefixAny. Existing FindByteCrLfPrefix bool/optional literal return.
- [ ] Set Exact for single-source negation, property-free all-byte bitmap, direct
  property/type lowerings; Atom for property brackets; retain prior atom admission.
- [ ] Compiler direct exact class repeat consumes Exact at positive counts, Atom
  only at count1; bounded width and restrictive selector unchanged. Matcher untouched.
- [ ] Six native Staging jobs/net8/9/10 full/frozen214/configured gates green,
  zero failed/skipped/build warnings/errors and seven Linux workload equalities.
- [ ] Whole-branch author review, exact head/tree/log/frozen/whitespace/clean audit;
  update #99/PR, mark ready, user owns merge.

Pre-flight: new enum changes only private class constructor; existing parser
aggregation/AST rewrites use computed PrefixAny/preserved record fields. Task1 APIs
stable, task2 passes provenance into existing proof/runtime metadata. No conflict.
