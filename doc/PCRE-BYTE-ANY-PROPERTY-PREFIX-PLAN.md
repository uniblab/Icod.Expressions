# Byte Any-property Bracket Prefix Implementation Plan

> REQUIRED SUB-SKILL: superpowers:executing-plans inline. No subagents.

**Goal:** Preserve native exact full-byte bitmap repeats containing Any properties.
**Architecture:** Carry source-property kind into existing bracket aggregation;
reuse the bounded compiler proof and conditional selector.
**Tech Stack:** C#13/net8/net9/net10, xUnit, PCRE.NET1.6.0 / PCRE2 10.48 JIT.
**Spec:** PCRE-BYTE-ANY-PROPERTY-PREFIX-DESIGN.md.

## Constraints and review focus

No agents/Python/SDK install, public/package/workflow/release/consumer/fallback or
raised-limit changes. Frozen35 intact; original spans/captures/work preserved.
1. Any-property bitmap versus real XCLASS provenance, including both complements.
2. Empty/non-full classes preserve prior atom/work behavior; no membership change.
3. UCP/ASCII POSIX/shorthand projection and local i/r retain existing paths.
4. Direct/group repeat, count1/2/11/12 and sole-literal/proof bounds remain shared.
5. UTF/fragment, exact/exhausted budgets, cancellation and capture bounds remain.

## Task1: Native contracts and observed RED

Files: tests/Pcre/PcreByteAnyPropertyPrefixReferenceTests.cs; design/plan.
Interfaces: existing Compile/Match and pinned reference APIs.
- [ ] Native-first all-capture rows and every-offset protocol, boundary and mode
  controls; all native successful spans inside the original subject.
- [ ] Shared work/exhaustion/cancellation, empty class prior behavior and compile
  ceiling tests; no unsafe native positive UTF Xuc or UTF C.
- [ ] Tests-only draft, successful-build expected RED; old5488 baseline passes.

## Task2: Provenance and verified handoff

Files: src/Pcre/PcreByteSyntax.cs, PcreByteParser.cs, PcreByteProgram.cs;
four canonical inventories.
Interfaces: new PcreByteClassPrefix.AnyProperty; bracket-item tuple adds bool
XclassProperty; existing proof result and runtime selector unchanged.
- [ ] CreatePropertyClass tags byte AnyScalar; aggregate actual XCLASS separately
  from old atom admission. Full members and no XCLASS permit Exact bracket kind.
- [ ] Direct AnyProperty exact-repeat admission retains prior behavior.
- [ ] Six native Staging jobs/net8/9/10 full/frozen214/configured gates green,
  zero failed/skipped/build warnings/errors, seven Linux workload equalities.
- [ ] Whole-branch author review and exact head/tree/log/frozen/whitespace/clean
  audit; update #99/PR with scope and mixed-property discrepancy; mark ready.

Pre-flight: the parser produces the enum consumed by both bracket aggregation and
compiler; both consumers must preserve old direct-property admission. Tuple fifth
field must be supplied by every literal/escape/POSIX return. No runtime/state
interface conflict. Standing no-subagent/scope authority applies.
