# UTF Any-only list star anchoring implementation plan

> Inline execution; no subagents/approval pauses. User owns merge/tag/publication.

**Goal:** Native ALLANY anchoring for bounded ordinary UTF Any-only lists.
**Architecture:** Preserve constant-time paths; scan longer pure lists under one
compiler-local4096 predicate-visit budget shared across new leading paths.
**Tech:** C#13; build net8/net9/net10, test net10 only.
**Spec:** doc/PCRE-UTF-ANY-LIST-STAR-ANCHOR-DESIGN.md.
**Base:** bfbd74760f4d48e502a98adecfaaa634e57704ad; 98,091 tests.

## Task1: native-first fixtures and freeze

- [x] Reproduce305 merged267 groups/36,436 protocols with pinned native JIT0.
- [x] Create reference/protocol partials with all sign/position/length/boundary,
  existing-path and late mixed/range counterexamples from the design.
- [x] Pin aggregate4096 visit limits/fallback, all traversal/policy/resource gates.
- [x] Tests-only draft/exact expected RED; all prior tests passing.
- [x] Freeze every new C# blob; keep313 prior files/308 C# sources/frozen35 fixed.

## Task2: bounded list proof

- [x] PcreByteProgram.cs only: compiler-local remainingPredicateVisits4096.
- [x] Pass it by ref and pass cancellation to IsUtfAllAny; old paths unchanged.
- [x] For >2 predicates/no ranges/ordinary Class, bound before scan; check
  cancellation/charge each visit/require exact AnyScalar metadata; aggregate
  positivity and apply positive-outer-any-positive/negative-outer-all-negative.
- [x] Exhaustion declines inference, preserving admission and resource contracts.
- [x] Seven inventories/local gates/whole-branch author review before qualification.

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
New393 groups/46,776 native protocols,15 diagnostics/47 supplementary cases:
455 new cases/98,546 total. Exclude11,781 strict-validation research rows
(6,358 malformed whole subjects/5,423 split offsets). Four new C# files:
reference plus three protocol partials. Native boundary preflights cover4096/
4097 member admission and aggregate4096/4097 visits. Exact expected RED is
117 failures (107 matching/6 work/1 owned-source/2 single-list boundary/1
aggregate boundary),98,429 passes/98,546 total; all98,091 prior tests and338
new controls must pass. Freeze all four C# files before production.

Before freezing, add one aggregate-exhaustion work/budget regression: two
separately admissible lists consume4097 total predicate visits. With start
study disabled, ordinary valid-subject no-match work must equal explicit
NoDotStarAnchor fallback, with exact/exhausted/observed-unobserved budgets.
This catches an erroneous per-list reset of the shared proof budget.
Production remains unchanged; confirm the updated tests-only RED before freeze.

## Frozen RED and implementation review

Tests-only head 7b7ef49672bc90703e21a75d1ac1694571324878, tree
be40721b647b534254e37b0706cc664ab0badacc, proposed merge
a6cf6bac9ec986356fe21d14afc04025650fa3eb. Native run43983371169,
Linux x64 job133705412667: exactly117 failures/98,429 passes/98,546 total,
zero skips/compiler or analyzer warning/error codes. All107 matching profile
identities and10 resource/ownership/boundary failures match the native-first
expectation. All98,091 prior tests and338 new controls pass. Freeze before
production:

- tests/Pcre/PcreUtfAnyListStarAnchorProtocols1.cs: b1c79fa002e866cca325602a116550dfa9e33480
- tests/Pcre/PcreUtfAnyListStarAnchorProtocols2.cs: 86bb2851fd1595a2235f554e272220ef45c2fa15
- tests/Pcre/PcreUtfAnyListStarAnchorProtocols3.cs: 5f1821c33a516c840e3ef024c276ced03bdb8190
- tests/Pcre/PcreUtfAnyListStarAnchorReferenceTests.cs: 68d79ff761a94b2f85fc3277367cac754b051b79

PcreByteProgram.cs is the sole production change. The new local4096 predicate
budget is shared across leading paths and passed by ref with cancellation.
The longer-list branch requires ordinary Class provenance, no ranges, exact
Any metadata for every visited member, and enough remaining visits before
scanning. Every scan checks cancellation/charges one compile visit. The final
positive-member aggregate implements both outer-negation cases. A late mixed
member declines proof even after an earlier positive member. Existing direct,
full-range, singleton, pair/complement paths remain unchanged and uncharged.
Exhaustion returns false without admission error or new public quota.

Inline author review: entry/global-AST bounds, capture31/32 map, all cuts/calls,
target and NoDot/COMMIT/start/possessification gates remain intact. No parser,
AST, matcher, VM, API, dependency, workflow, runtime allocation/state/work
charge or class instruction changes. Prior tests and frozen35 stay identical.
The new aggregate-exhaustion work test detects a per-list budget reset.
Final source qualification remains pending; retain its evidence externally.

Local prequalification gates pass: frozen35 source, frozen corpus, A05 candidate,
CI classifier, complete main jobs/eight rejection cases, whitespace, prior313
test identities and all four new frozen C# blobs. Inline author review found
no outstanding defect. All seven inventories updated before source qualification.

Publication status: automatic approval review blocked the large shared design
inventory payload as potentially non-public disclosure. Preserve the exact
five-inventory addition patch locally; publish code and these own-scope records
for unaffected qualification. Final delivery waits for explicit approval of
that exact patch and fresh qualification of its resulting head.
