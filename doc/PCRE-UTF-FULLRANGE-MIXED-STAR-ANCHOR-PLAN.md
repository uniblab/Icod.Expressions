# Full-range mixed UTF star implementation plan

Inline execution under standing direction, no subagents or approval pauses.
Base3eb569260d13ed737dbb6bb69071bc9ee68a3f09; floor99,166 tests.
Spec: [design](PCRE-UTF-FULLRANGE-MIXED-STAR-ANCHOR-DESIGN.md).

## Task1: native fixtures and freeze

- [x] Reproduce551 merged270 groups/65,782 native protocols, JIT0.
- [ ] Add native-pinned matching, diagnostic, ownership, work, capacity,
  source-boundary and predicate-count/budget fixtures from the design.
- [ ] Publish tests-only draft; verify exact expected RED and every prior
  test passing; freeze every new C# Git blob before production.

## Task2: constant-time proof and review

- [ ] Remove only the empty-predicate condition in the existing exact positive
  full-range Class fast path in IsUtfAllAny, PcreByteProgram.cs.
- [ ] Review retained/source range shape, negation, folding, uncharged proof
  before/after4096 visits, unchanged budgets/cancellation/global-entry guards.
- [ ] Preserve322 prior test files/317 C# sources/frozen35; run frozen/corpus/
  A05/classifier/main-job/whitespace gates, update all seven inventories.
- [ ] Publish exact reviewed source using a branch-head lease.

## Task3: qualify and deliver

- [ ] Six fresh full net10 results, all3 library targets, frozen214/120,
  zero failures/skips/compiler/analyzer warnings, all support gates.
- [ ] Verify final head/base/tree/proposed merge and frozen identities,
  record evidence externally, ready/readback, #7/#102 open, own scratch cleanup.

User owns merge/tag/publication; Grep last. Issue7 footer remains deferred
under its existing publication block. No source/docs edits after final GREEN.

## Native-first inventory and expected RED

New579 matching groups/69,072 protocols,15 diagnostics and55 supplementary
cases:649 new cases,99,815 total. Native dynamic preflight21 groups/35
literal checks, JIT_COMPLETE0 throughout. Exclude17,199 strict-validation
research rows before native execution (9,282 malformed whole subjects and
7,917 split offsets). They are not matching expectations.

Expected tests-only RED228:211 matching identities,6 work,1 owned-source,
6 predicate-count,2 after-budget,1 before-budget and1 work-boundary failure.
Expect99,587 passes; all99,166 prior tests and421 new controls stay green.
Freeze all five new C# blobs only after observing that exact RED.
