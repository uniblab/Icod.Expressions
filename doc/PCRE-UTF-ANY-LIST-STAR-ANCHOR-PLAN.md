# UTF Any-only list star anchoring implementation plan

> Inline execution; no subagents/approval pauses. User owns merge/tag/publication.

**Goal:** Native ALLANY anchoring for bounded ordinary UTF Any-only lists.
**Architecture:** Preserve constant-time paths; scan longer pure lists under one
compiler-local4096 predicate-visit budget shared across new leading paths.
**Tech:** C#13; build net8/net9/net10, test net10 only.
**Spec:** doc/PCRE-UTF-ANY-LIST-STAR-ANCHOR-DESIGN.md.
**Base:** bfbd74760f4d48e502a98adecfaaa634e57704ad; 98,091 tests.

## Task1: native-first fixtures and freeze

- [ ] Reproduce305 merged267 groups/36,436 protocols with pinned native JIT0.
- [ ] Create reference/protocol partials with all sign/position/length/boundary,
  existing-path and late mixed/range counterexamples from the design.
- [ ] Pin aggregate4096 visit limits/fallback, all traversal/policy/resource gates.
- [ ] Tests-only draft/exact expected RED; all prior tests passing.
- [ ] Freeze every new C# blob; keep313 prior files/308 C# sources/frozen35 fixed.

## Task2: bounded list proof

- [ ] PcreByteProgram.cs only: compiler-local remainingPredicateVisits4096.
- [ ] Pass it by ref and pass cancellation to IsUtfAllAny; old paths unchanged.
- [ ] For >2 predicates/no ranges/ordinary Class, bound before scan; check
  cancellation/charge each visit/require exact AnyScalar metadata; aggregate
  positivity and apply positive-outer-any-positive/negative-outer-all-negative.
- [ ] Exhaustion declines inference, preserving admission and resource contracts.
- [ ] Seven inventories/local gates/whole-branch author review before qualification.

Review guards, first/middle/last positivity, late non-Any/count/range/type/value
controls, global budget arithmetic/cancellation/no allocation, old fast paths,
entry/global-AST/capture31-32/cut/target/NoDot/COMMIT/start/possessification,
unchanged instructions/work/budgets/ownership/signed captures/marks/diagnostics/
whole-subject UTF validation/capacities/proof fallback and frozen identities.

## Task3: final qualification and delivery

- [ ] Six fresh full net10 records/frozen214/120/all3 library targets, zero failures/
  skips/compiler or analyzer warnings/errors; every configured support gate.
- [ ] Exact clean source/base/tree/proposed merge and frozen-file identities.
- [ ] PR268/issue7 footer/ready/readback; identity-checked task-scratch cleanup.

Final evidence external; no docs-only repeat after GREEN; #7/#102 open, Grep last.

## Native-first inventory

Reproduced305 merged267 groups/36,436 protocols with JIT_COMPLETE0.
New393 groups/46,776 native protocols,15 diagnostics/46 supplementary cases:
454 new cases/98,545 total. Exclude11,781 strict-validation research rows
(6,358 malformed whole subjects/5,423 split offsets). Four new C# files:
reference plus three protocol partials. Native boundary preflights cover4096/
4097 member admission and aggregate4096/4097 visits. Exact expected RED is
117 failures (107 matching/6 work/1 owned-source/2 single-list boundary/1
aggregate boundary),98,428 passes/98,545 total; all98,091 prior tests and337
new controls must pass. Freeze all four C# files before production.
