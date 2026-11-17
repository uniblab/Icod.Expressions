# Positive Any in mixed UTF star classes

Continue merged #269, base b595a96ba1bc842fcbdadab4f2b45efdaa5bd46e,
tree 81e7ef00f4d42c53a8d3d052560af01c0b326747; 98,546 tests.
Execute inline under standing direction: no subagents or routine approval
pauses. User owns merge/tag/publication; #7/#102 stay open; Grep last.

## Cause and approach

Native PCRE2 10.48 parse_class adds the complete scalar range for a positive
PT_ANY. Its high-character optimization suppresses other property payloads,
and a full bitmap with all high characters lowers to ALLANY. A positive Any
member therefore dominates an ordinary positive class even when it contains
other predicates or literal ranges. Native preflight: 140 profiles/420 rows,
all JIT_COMPLETE0, including reversed member order, categories/scripts/ranges,
UCP/casing/NoStart and disabled-anchoring controls. On tolerant UTF subject
C3A942/start1, the default inferred star misses while NoDotStarAnchor matches
2:1. Negative outer/property-complement controls retain their native entry.

Choose a source-aware extension in IsUtfAllAny only. Keep direct/full-range/
singleton behavior and existing two-member complement behavior. For pairs,
also accept a positive outer class with either exact positive Any predicate.
For longer lists or classes with ranges, preserve the existing shared 4096
predicate-visit budget, precheck remaining capacity, check cancellation and
charge every visited predicate. A positive outer class qualifies if any member
is AnyScalar/non-negated/null-category/zero-property-value; unrelated operands
do not disqualify that fact. Still scan all members to preserve visit bounds.
Outer-negated classes with ranges stay outside the proof; without ranges they
still require every member to be exact negative Any. Negative Any alone never
establishes dominance. Existing constant-time paths remain uncharged.

Normalizing classes to Any would change instructions and matching work. General
semantic universality would include property complements, full-range mixtures
and class algebra with different native provenance. Keep those separate.
This is my scoped implementation choice under the existing roadmap.

No parser/AST/matcher/VM/API/dependency/workflow, syntax admission, runtime
allocation/state/work-charge or public-limit change. Preserve traversal/AST
bounds, capture31/32 dependencies, cuts/calls/targets and NoDot/COMMIT/start/
possessification gates. Proof exhaustion declines inference, never admission.

## Verification and delivery

Reproduce all 393 merged268 groups/46,776 protocols using native tag
7978954dbd2efc6f2196869290553cf1871b4ce6 and SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9, JIT_COMPLETE0. Pin positive Any
in pairs, first/middle/last long-list positions, categories/scripts/binary/Bidi/
shorthands/POSIX/ranges, casing/UCP/aliases and source boundaries. Keep negative
outer/negative Any/non-Any/property-complement/full-range-mixture/algebra
controls explicit. Exercise inherited groups/captures31/32/cuts/calls/targets,
strict/tolerant UTF, malformed fragments/NUL/U+10FFFF/all offsets/empty policies,
exact work/budgets/cancellation, owned source/captures/marks, diagnostics,
capacities and exact/aggregate4096/4097 proof fallback. Beyond proof capacity,
pin admission and ordinary valid-subject fallback without claiming inferred
malformed-UTF parity. Strict matching rows require valid whole subjects and
scalar-aligned offsets before native execution; diagnostics stay separate.

Tests-only draft and exact RED precede freezing every new C# blob and production.
Preserve all317 prior test files/312 C# sources/frozen35. Build net8/net9/net10,
execute net10 only on six native hosts, frozen214 outcomes/120 methods. Update
five shared inventories plus this design/plan before final qualification.
Keep shared additions minimal with canonical links, public baselines intact.
Final qualification/source identities live on the PR; issue7 gets a concise
link/status footer preserving its exact2607-character prefix. No source/docs
edits after final GREEN. Delete only identity-checked task scratch after ready.

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
