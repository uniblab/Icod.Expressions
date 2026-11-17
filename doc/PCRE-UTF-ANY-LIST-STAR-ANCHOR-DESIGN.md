# UTF Any-only list star anchoring

Continue merged267 at bfbd74760f4d48e502a98adecfaaa634e57704ad,
tree590f92837ed3acf452ac276aab6faa98c345610f; 98,091 baseline tests.
Execute inline with no subagents or approval pauses under standing direction.
User owns merge/tag/publication; #7/#102 stay open; Grep last.

## Native cause and chosen approach

PCRE2 10.48 treats positive Any as the full scalar domain and negative Any as
empty. The existing singleton/pair proof is exact but longer Any-only classes
still miss native ALLANY anchoring. For positive outer classes, any positive
member makes the union universal; for negative outer classes, every member
must be negative. Probe lengths3/4/64/4096/4097, JIT_COMPLETE0, confirms the rule.

Preserve the existing direct Any, full-range, singleton and pair/complement
fast paths unchanged. Extend only ordinary Class/no-ranges/more-than-two
predicates when every member is AnyScalar/null-category/zero-property-value.
Scan all members, aggregate whether one is positive, and apply the native rule.
Add one compiler-local remaining-predicate budget initialized from the existing
MaximumInstructions4096. Share it across all newly extended leading-class
paths; check cancellation and consume one visit per scanned predicate. Reject
proof if the array exceeds the remaining budget. Existing constant-time paths
consume no new predicate budget. Proof exhaustion declines inference, never
rejects admitted syntax or changes public quotas. This adds bounded compile
work, no allocation/runtime state/VM instruction/work charging change.

Normalizing classes to Any would change instruction/work provenance. General
semantic universality would broaden mixed-property/range/algebra inference.
Choose the bounded pure-Any list proof; keep mixed operands outside it.
Keep entry/global-AST/capture31-32/global-cut/target/NoDot/COMMIT/start/
possessification and all existing resource/ownership/validation policies.

## Validation and delivery

Reproduce305 merged267 groups/36,436 protocols before generating new fixtures.
Pin PCRE2 tag7978954dbd2efc6f2196869290553cf1871b4ce6 and SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9; require JIT_COMPLETE0.
Cover every three-member sign/outer-negation combination; positive member at
first/middle/last positions; lengths3/4/8/64 and exact4096 predicate visits;
late non-Any/negative-category/range counterexamples, aliases/casing/UCP/
restrictions, existing singleton/pair/full-range/complement controls, groups/
alternatives/capture31-32 dependencies/cuts/targets, NoDot/COMMIT/start/
possessification, malformed UTF/NUL/U+10FFFF/empty policies, exact work/budgets,
owned source/options/signed captures/marks/diagnostics/cancellation/capacities.
Pin proof exhaustion and aggregate-budget fallback on ordinary valid-boundary
subjects, without claiming inference or native malformed-UTF parity outside
the bounded proof. Strict matching fixtures require a valid whole subject and
scalar-aligned start; retain explicit strict diagnostics separately.

Tests-only draft/exact RED precede freezing every new C# blob and production.
Preserve313 prior files/308 C# sources/frozen35. Build net8/net9/net10; execute
net10 only on six native hosts/frozen214 outcomes/120 methods. All seven
inventories before final qualification; final GREEN/review/source/base/tree/
merge evidence on PR268/issue7 footer, original prefix preserved. No docs-only
commit/repeat after final GREEN. No parser/AST/matcher/VM/API/dependency/workflow
or admission change. Publication uses only owned technical additions and links
to canonical design/plan evidence; old public inventory baselines stay intact.

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
