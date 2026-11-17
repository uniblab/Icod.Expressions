# Full-range mixed UTF star implementation plan

Inline execution under standing direction, no subagents or approval pauses.
Base3eb569260d13ed737dbb6bb69071bc9ee68a3f09; floor99,166 tests.
Spec: [design](PCRE-UTF-FULLRANGE-MIXED-STAR-ANCHOR-DESIGN.md).

## Task1: native fixtures and freeze

- [x] Reproduce551 merged270 groups/65,782 native protocols, JIT0.
- [x] Add native-pinned matching, diagnostic, ownership, work, capacity,
  source-boundary and predicate-count/budget fixtures from the design.
- [x] Publish tests-only draft; verify exact expected RED and every prior
  test passing; freeze every new C# Git blob before production.

## Task2: constant-time proof and review

- [x] Remove only the empty-predicate condition in the existing exact positive
  full-range Class fast path in IsUtfAllAny, PcreByteProgram.cs.
- [x] Review retained/source range shape, negation, folding, uncharged proof
  before/after4096 visits, unchanged budgets/cancellation/global-entry guards.
- [x] Preserve322 prior test files/317 C# sources/frozen35; run frozen/corpus/
  A05/classifier/main-job/whitespace gates, update all seven inventories.
- [x] Publish exact reviewed source using a branch-head lease.

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

## Pre-freeze fixture correction

Initial tests-only head d7bc98f47de07863eaa8ed18d4217fa8a69bf072,
run44112346787/Linux134141040126:188 failures/99,627 passes/99,815 total,
zero skips or compiler/analyzer warning/error codes. All17 supplementary
failures matched;171 of211 predicted matching cases failed, with no unexpected
failures. The other40 still used an already-supported positive Any form in
one generated member order. Correct those80 profiles (strict and tolerant)
to the planned full-range-plus-property source. Recompute all579 native
groups/69,072 protocols; outcomes remain literal native pins, JIT0. No prior
file, resource expectation or production source changes. Observe corrected
RED228/99,587/99,815 before freezing all five C# blobs.

## Frozen RED and implementation review

Corrected tests-only head8f80fb4a2797b9f3bb717557727e012eee3fa50f,
tree8cf524425a312fe49ddbadfec05aa9d3d0f64d86, run44113511785, Linux x64
134145508682: exactly228 failures/99,587 passes/99,815 total. All211 matching
identities and17 supplementary failures match; all99,166 prior tests and421
new controls pass. All3 library targets build, zero skips/compiler/analyzer
warning/error codes. Freeze before production:

- tests/Pcre/PcreUtfFullRangeMixedStarAnchorProtocols1.cs: e0857b45609ea51b57807d71f9e859138aa3ad90
- tests/Pcre/PcreUtfFullRangeMixedStarAnchorProtocols2.cs: 159639302dc82294a8c1fe112b4207d5205a01f3
- tests/Pcre/PcreUtfFullRangeMixedStarAnchorProtocols3.cs: 312beb79d2f7fc9e492adc3c442ab89fc2124cfb
- tests/Pcre/PcreUtfFullRangeMixedStarAnchorProtocols4.cs: 9a70383d365122cfce8a3b17ea134b5e5d8010c6
- tests/Pcre/PcreUtfFullRangeMixedStarAnchorReferenceTests.cs: 664225a607add9e013c5d1ae8d4e556b0be820ae

Change only IsUtfAllAny in PcreByteProgram.cs: remove the empty-predicate
condition from the existing exact positive full-range Class fast path, with
a comment explaining union dominance. Preserve Class provenance, positive
outer polarity, exactly one retained range and exact endpoints0/0x10FFFF.
No predicate scan/charge or new allocation/state/instruction. Existing
positive-Any/list paths, shared4096 visits, per-visit cancellation and
exhaustion fallback are unchanged. Caller cancellation still guards each
leading branch. Entry/global-AST/capture31/32/cut/call/target and NoDot/COMMIT/
start/possessification gates remain intact. No parser/AST/VM/API/dependency/
workflow/admission/public-limit or runtime-work changes.

Inline whole-branch author review checked positive/negative/source/range
boundaries, retained shape after existing case closure, predicate counts
through4097, uncharged proof both before and after a4096-member list,
work/limits/cancellation/ownership/capacities/validation and all native pins.
No outstanding defect. Preserve322 prior files/317 C# sources and frozen35.
All seven inventories precede final qualification; final results and source/
merge identities remain external on the PR, with no source/docs edits after
final GREEN. Issue7 footer stays untouched under its existing block.
