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

- [x] Reproduce393 merged268 groups/46,776 native protocols, all JIT0.
- [x] Create PcreUtfAnyMixedStarAnchorReferenceTests.cs and protocol partials:
  literal native spans/marks, matching/diagnostic/resource/source/budget cases
  from the design, exact/aggregate4096/4097 and old-path/source counterexamples.
- [x] Publish tests-only draft; observe exact expected RED with all prior
  98,546 tests passing, then freeze every new C# Git blob before production.

## Task2: dominance proof and source review

- [x] Modify only src/Pcre/PcreByteProgram.cs IsUtfAllAny(node, ref remaining,
  CancellationToken): allow positive Any dominance for mixed pairs and bounded
  longer/range classes. Preserve all old fast paths and outer-negative rules.
- [x] Retain precheck, per-visit cancellation/charge, complete positive scan,
  shared budget and exhaustion-as-declined-inference semantics.
- [x] Update all seven inventories; local frozen/corpus/A05/classifier/main-job/
  whitespace/identity gates; inline whole-branch review against Review focus.
- [x] Publish exact reviewed source using a branch-head lease.

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

## Frozen corrected RED and implementation review

Corrected tests-only head b6026422c7d480219e1712240576f1aead7107bb, tree
c6f39abd87c9b223c6678816d0c70882ff788717, proposed merge
0c3c2446a2b49cc755952c0015828dc0da998d72. Run44092761677, Linux x64 job
134072156398: exactly215 failures/98,951 passes/99,166 total, zero skips
and compiler/analyzer warnings/errors. All203 matching identities and12
resource/ownership/boundary failures match the native-first expectation. All
98,546 prior tests and405 new controls pass. Freeze before production:

- tests/Pcre/PcreUtfAnyMixedStarAnchorProtocols1.cs: 0edd639719e0bb42e6398f469a55d101711dab3c
- tests/Pcre/PcreUtfAnyMixedStarAnchorProtocols2.cs: b6318d33aa467d28ca300d8765a853e6aaf1c5ae
- tests/Pcre/PcreUtfAnyMixedStarAnchorProtocols3.cs: 48e27f897e3ec7c9107349335b4c0518f2a36c54
- tests/Pcre/PcreUtfAnyMixedStarAnchorProtocols4.cs: 061245f8b88a76874b0389136f7dafd77e7ef712
- tests/Pcre/PcreUtfAnyMixedStarAnchorReferenceTests.cs: 6c14f10f6cec04eb62068ed2c52bb44c46ad93fa

Only IsUtfAllAny in PcreByteProgram.cs changes. Preserve the exact direct,
full-range and singleton paths. Add constant-time positive Any dominance for
mixed pairs before the unchanged pair/complement logic. For longer lists or
range-bearing classes, precheck the existing shared remaining-predicate budget
and charge/check cancellation per member. In positive outer unions ignore
unrelated member kinds while still scanning every predicate; one exact
positive Any suffices. Negative outer classes reject ranges and still require
only exact negative Any members. Exhaustion declines proof, not admission.

Inline author review checks first/middle/last Any, late mixed/range members,
outer-negation/source/metadata boundaries, uncharged existing fast paths,
4096/4097 shared limits and range/pair boundary work controls. Entry/global
AST bounds, capture31/32 maps, cuts/calls/targets and NoDot/COMMIT/start/
possessification guards remain intact. No parser/AST/matcher/VM/API/dependency/
workflow, runtime state/allocation/work charge, instruction or public-limit
change. Preserve317 prior files/312 C# sources/frozen35. No outstanding defect.

Main269 closure: runs44090328005/44090328118 are green at attempt1. Actual
merge b595a96ba1bc842fcbdadab4f2b45efdaa5bd46e has reviewed tree
81e7ef00f4d42c53a8d3d052560af01c0b326747 and ordered268/269-head parents.
Six native98,546 net10 results/frozen214 outcomes/120 methods/all3 library
targets, zero compiler/analyzer warnings/errors; Intel aggregate134072170331
verifies the same artifact. Dedicated Gnulib134063473029 passes on Ubuntu24.04;
.NET Linux134063473087 remains on Ubuntu26.04. All eight main jobs pass the
completion guard. Script jobs134063472402/134063472876/134063473331 each31
checks; Ubuntu also passes13 main-job rejection cases.

All seven inventories are updated before final source qualification; record
final results and exact identities externally. Issue7 remains untouched under
the existing publication block pending explicit approval.

Local prequalification gates pass: frozen35 source/corpus/A05, classifier,
eight main jobs/13 rejection cases, whitespace and frozen5/prior317 identities.
Inline whole-branch author review found no outstanding defects. Final source
qualification remains pending; retain its outcome externally.
