# Positive Any in mixed UTF star classes implementation plan

> Inline execution under standing direction; no subagents/approval pauses.

**Goal:** Preserve native ALLANY star anchoring when a positive Any member
dominates an ordinary positive UTF class with other predicates or ranges.
**Architecture:** Extend IsUtfAllAny in PcreByteProgram.cs, retaining old fast
paths and the existing shared4096 predicate-visit proof budget.
**Tech:** C#13; build net8/net9/net10, execute net10 only.
**Spec:** doc/PCRE-UTF-ANY-MIXED-STAR-ANCHOR-DESIGN.md.

## Global constraints

Base b595a96ba1bc842fcbdadab4f2b45efdaa5bd46e, 98,546 tests. Preserve317 prior
files/312 C# sources/frozen35; no public limit/API/runtime state/work changes.
#7/#102 stay open; Grep last; user owns merge/tag/publication.

## Review focus

Positive versus negative Any and outer negation; Any at first/middle/last with
late mixed/range operands; native source versus semantic universality; shared
budget/cancellation/boundary fallback without admission changes; inherited
capture31/32/global cuts/targets/NoDot/COMMIT/UTF-validation/resource contracts.
Each is pinned in Task1 and checked in Task2's inline author review.

## Task1: native-first fixtures and freeze

- [ ] Reproduce393 merged268 groups/46,776 native protocols, all JIT0.
- [ ] Create PcreUtfAnyMixedStarAnchorReferenceTests.cs and protocol partials:
  literal native spans/marks, matching/diagnostic/resource/source/budget cases
  from the design, exact/aggregate4096/4097 and old-path/source counterexamples.
- [ ] Publish tests-only draft; observe exact expected RED with all prior
  98,546 tests passing, then freeze every new C# Git blob before production.

## Task2: dominance proof and source review

- [ ] Modify only src/Pcre/PcreByteProgram.cs IsUtfAllAny(node, ref remaining,
  CancellationToken): allow positive Any dominance for mixed pairs and bounded
  longer/range classes. Preserve all old fast paths and outer-negative rules.
- [ ] Retain precheck, per-visit cancellation/charge, complete positive scan,
  shared budget and exhaustion-as-declined-inference semantics.
- [ ] Update all seven inventories; local frozen/corpus/A05/classifier/main-job/
  whitespace/identity gates; inline whole-branch review against Review focus.
- [ ] Publish exact reviewed source using a branch-head lease.

## Task3: qualify and deliver

- [ ] Six fresh net10 98,546+new full results, frozen214/120, all3 library
  targets and support gates, zero failures/skips/compiler/analyzer warnings.
- [ ] Verify source/base/tree/proposed-merge/frozen identities; final evidence
  external on PR, concise issue7 footer/prefix, ready/readback, #7/#102 open.
- [ ] Identity-checked scratch cleanup; preserve native oracle/review patches.

Self-review: scoped to one proof; tasks cover every design requirement and all
five review conditions. No source edits before native-first RED/freeze.

## Native-first fixture inventory

Reproduced all393 merged268 groups/46,776 protocols. New551 groups/65,782
matching protocols,15 diagnostic and54 supplementary cases:620 new cases,
99,166 total. Dynamic boundary preflights:20 groups/28 literal checks, JIT0.
All matching profiles compiled with JIT_COMPLETE0. Before native execution,
exclude16,317 strict-validation research rows (8,806 malformed whole subjects,
7,511 split offsets). They are not matching expectations. Native surrogate/
out-of-range research is rejected at compile time and stays separate.

Expected tests-only RED:215 failures (203 matching,6 work,1 owned-source,
2 list boundary,1 aggregate boundary,1 constant-time-pair-after-budget and
1 range-member boundary);98,951 passes/99,166 total. All98,546 prior tests
and405 new controls must pass. The negative/over-budget admission and work
controls must remain green. Freeze all five C# files only after exact RED.

The CI-only269 merge changes no PCRE/C# source. Refresh the317-file baseline
to actual base b595a96ba1bc842fcbdadab4f2b45efdaa5bd46e before freezing.
Main now requires eight successful jobs; the shell guard has13 rejection cases.
Issue7 footer publication remains blocked without explicit approval; leave it
untouched and keep qualification evidence on the PR.

## Pre-freeze diagnostic correction

Initial tests-only head a9f20cc7fc0b5a497794a5da2df73f79285d38b8,
run44091436736/Linux134067259346:218 failures/98,948 passes/99,166 total,
zero skips/compiler or analyzer warnings/errors. All203 matching failure
identities matched the native-first expectation; all98,546 prior tests pass.
Three extra failures are fixture errors in diagnostic5/10/15: I selected the
preceding B atom instead of the opening quantifier brace. The frozen prior
fixtures independently pin the quantifier opener. Correct only those three
managed expected offsets (13→14,27→28,27→28); native codes/offsets and all
matching/resource expectations stay unchanged. Production remains unchanged.
Observe corrected RED215/98,951/99,166 before freezing all five C# blobs.
