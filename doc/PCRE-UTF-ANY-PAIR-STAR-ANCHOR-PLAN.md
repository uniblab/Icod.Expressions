# UTF Any-predicate pair star anchoring implementation plan

> Execute inline; no subagents or approval pauses. User merges/tags/publishes.

**Goal:** Recognize exact universal two-Any classes in native UTF star anchoring.
**Architecture:** Constant-time pair truth table inside IsUtfAllAny only.
**Tech:** C#13, net8/net9/net10 builds and net10-only tests.
**Spec:** doc/PCRE-UTF-ANY-PAIR-STAR-ANCHOR-DESIGN.md.
**Base:** b0c56119dcdd0b384846e3768714a9dafbbad278; 97,737 tests.

## Task 1: native-first fixtures and freeze

- [x] Reproduce 265 merged266 groups/31,988 protocols with pinned native JIT0.
- [x] Create PcreUtfAnyPairStarAnchorReferenceTests and protocol partials.
- [x] Pin all eight negation combinations/orders/aliases and all design controls.
- [x] Tests-only draft: confirm the exact expected RED and all prior tests passing.
- [x] Freeze every new C# blob; keep 310 prior files/305 C# sources/frozen35 fixed.

## Task 2: exact pair proof

- [x] In PcreByteProgram.cs retain direct Any/full-range/singleton branches.
- [x] Require exactly two AnyScalar/null-category/zero-value predicates/no ranges.
- [x] Positive outer: either predicate positive. Negative outer: both negative.
- [x] Retain outer-negation rejection before unchanged other complement rules.
- [x] Prepare seven inventories locally; run local guards and whole-branch author review.
- [ ] Publish the five-file shared-document patch after explicit user authorization.

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


## Exact RED, freeze and whole-branch author review

Tests-only source ad9d821124c75537af5c5c8a609ef919bf5fcd51,
tree f75019734cf6f84d40b5490a0a6eb87d2eacd4fc, checkout
ac5af6e232458ea960afc195229861fe749d5c1f has the identical tree and ordered
base b0c56119dcdd0b384846e3768714a9dafbbad278/head parents.
[Run43960485627](https://github.com/uniblab/Icod.Expressions/actions/runs/43960485627),
Linux x64 job133626248918: exactly86 expected managed failures/98,005 passes/
98,091 total/zero skips/compiler or analyzer warnings/errors. The79 matching
IDs are 7,8,13,14,19,20,25,26,34,35,40,41,46,47,52,53,61,62,67,68,73,74,79,80,
83,85,87,89,91,93,95,101,107,117,118,123,124,125,129,130,138,145,146,152,153,
154,155,160,161,163,167,169,171,173,175,177,179,197,199,201,203,205,207,209,
211,213,215,217,219,221,223,277,279,281,283,285,287,289,291; add six UTF
work cases and one tolerant owned-source case. Every native preflight, all
97,737 prior tests and268 new controls pass. No fixture correction.

Freeze before production:

|New C# file|Git blob|
|---|---|
|PcreUtfAnyPairStarAnchorReferenceTests.cs|193eea4653eaff6202667a3b785cee8f437f1797|
|PcreUtfAnyPairStarAnchorProtocols1.cs|e96da7e5b3fe89ba2df3ca00675da1fc9c7c5f90|
|PcreUtfAnyPairStarAnchorProtocols2.cs|2fb2f3be81e470dc0599eed9b360c42b51797597|

All three blobs/310 prior files/305 C# sources/frozen35 stay byte-identical.

### Native pair truth table

|Outer class negated|First Any negative|Second Any negative|Native ALLANY|
|---|---|---|---|
|No|No|No|Yes|
|No|No|Yes|Yes|
|No|Yes|No|Yes|
|No|Yes|Yes|No|
|Yes|No|No|No|
|Yes|No|Yes|No|
|Yes|Yes|No|No|
|Yes|Yes|Yes|Yes|

Only PcreByteProgram.cs changes in production (7 additions/2 deletions).
Keep the no-ranges gate and require exactly two AnyScalar/null-category/
zero-property predicates before applying this table. Move outer-negation
rejection to the unchanged other-complement branch. The direct Any, full-range
and singleton paths are untouched. Zero/>2 predicates and mixtures remain
outside this exact pair rule. Native compile_class PT_ANY handling supplies
whole/empty operands, not arbitrary semantic universality.

Author review independently checks all eight rows, both predicate orders,
aliases/casing/UCP/restrictions, singleton/full-range/other-complement eligibility,
property/range/member count boundaries, grouped alternatives and31/32 dependency
bits/global cuts/targets; source/caller NoDot/COMMIT/start/possessification;
unchanged instructions/work/exact budgets/ownership/signed captures/marks/
diagnostics/whole-subject validation/cancellation/full capacities/proof fallback.
No allocation, iteration, borrowed lifetime, quota or admission change. Parser/
AST/emitter/matcher/VM/API/dependencies/workflows remain fixed. No introduced
unresolved critical/important/minor findings. Review is inline under standing
no-subagent direction. Local frozen-source/corpus/A05/classifier/main-job/diff
gates pass before qualification.

Merged266 closure: actual b0c56119dcdd0b384846e3768714a9dafbbad278,
tree4176165f5e158ab3f0775dc15ce7d5c4571aaefd and ordered base410447d1c0de322ec5d6ab56760716abf9233a16/
reviewed24a2ea08b9110af48b6f30ec6285e80f690611f4 parents match tested proposed
mergeae84704129f07295548babb707f283dcd566ea47. Main43959009957/scripts43959010032
green at attempt1: six97,737 net10 results/frozen214 outcomes/120 methods/all
three library targets/zero compiler or analyzer warnings/errors; Intel aggregate
133628347267 verifies the same single native result. Script jobs133621084623/
133621085014/133621085283 each31 checks; Ubuntu8 main-job rejection cases.
No source change or retry.

All seven inventories precede final source qualification. Require six fresh
98,091/98,091 net10 records/frozen214/120/all three library targets, zero failures/
skips/compiler or analyzer warnings/errors and every support gate. Final GREEN,
review, exact source/base/tree/merge identities and delivery readback remain
external on PR267/issue7 footer. Preserve the original issue prefix; no docs-only
commit or repeat after GREEN. User merges/tags/publishes; #7/#102 stay open;
Grep last; additional predicates/ranges/unions/algebra stay separately scoped.

## Shared-document publication gate

The five shared inventory updates are prepared locally in the separate
PR267-documentation.patch review artifact. Automatic approval review blocked
their public publication as potentially private Icod.Grep-derived inventory
material, and requires explicit user authorization. Keep all five updates out
of this draft branch while that authorization is pending. Only the owned
implementation, frozen tests and this design/plan record are published here.

Six-host CI on this source is preliminary implementation qualification. After
user approval, apply exactly that five-file patch and perform final source
qualification with all seven inventories present. Final GREEN/ready/issue7
evidence remains external; no documentation edits after that final GREEN.
