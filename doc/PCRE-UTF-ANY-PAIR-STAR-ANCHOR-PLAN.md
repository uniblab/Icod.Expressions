# UTF Any-predicate pair star anchoring implementation plan

> Execute inline; no subagents or approval pauses. User merges/tags/publishes.

**Goal:** Recognize exact universal two-Any classes in native UTF star anchoring.
**Architecture:** Constant-time pair truth table inside IsUtfAllAny only.
**Tech:** C#13, net8/net9/net10 builds and net10-only tests.
**Spec:** doc/PCRE-UTF-ANY-PAIR-STAR-ANCHOR-DESIGN.md.
**Base:** b0c56119dcdd0b384846e3768714a9dafbbad278; 97,737 tests.

## Task 1: native-first fixtures and freeze

- [ ] Reproduce 265 merged266 groups/31,988 protocols with pinned native JIT0.
- [ ] Create PcreUtfAnyPairStarAnchorReferenceTests and protocol partials.
- [ ] Pin all eight negation combinations/orders/aliases and all design controls.
- [ ] Tests-only draft: confirm the exact expected RED and all prior tests passing.
- [ ] Freeze every new C# blob; keep 310 prior files/305 C# sources/frozen35 fixed.

## Task 2: exact pair proof

- [ ] In PcreByteProgram.cs retain direct Any/full-range/singleton branches.
- [ ] Require exactly two AnyScalar/null-category/zero-value predicates/no ranges.
- [ ] Positive outer: either predicate positive. Negative outer: both negative.
- [ ] Retain outer-negation rejection before unchanged other complement rules.
- [ ] Update seven inventories; run local guards and whole-branch author review.

Review source/count/kind/property/range/negation boundaries; existing singleton,
full-range, ASCII/horizontal/vertical/mixed-decimal eligibility; bounded group/
alternative/dependency31-32/global cuts/targets; source/caller NoDot/COMMIT/
start/possessification; fixed instructions/work/budgets/ownership/signed captures/
marks/diagnostics/validation/cancellation/full capacities/proof exhaustion.

## Task 3: final qualification and delivery

- [ ] Six fresh full net10 records/frozen214/120, all three library targets,
  zero failures/skips/compiler or analyzer warnings/errors and every support gate.
- [ ] Exact clean source/base/tree/proposed merge parent and frozen identities.
- [ ] PR267/issue7 footer/ready/readback; remove only identity-checked task scratch.

Final evidence external; no documentation-only repeat after GREEN. #7/#102 stay
open; Grep last. Inline author review follows standing no-subagent direction.

## Native-first inventory

Reproduced 265 merged266 groups/31,988 protocols with JIT_COMPLETE0.
New 305 groups/36,436 native protocols, 15 diagnostics and 34 supplementary
cases:354 new cases/98,091 total. Isolate 9,009 strict-validation research
rows (4,862 malformed whole subjects/4,147 split offsets). All retained
strict comparisons validate their complete subjects and scalar-aligned starts.
Three new C# files: reference plus two protocol partials.
Expected 86 RED failures (79 matching/6 work/1 ownership); all 97,737 prior
tests and 268 new controls must pass before freeze or production.
