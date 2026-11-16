# UTF Any-predicate pair star anchoring

Continue merged #266 at b0c56119dcdd0b384846e3768714a9dafbbad278,
tree 4176165f5e158ab3f0775dc15ce7d5c4571aaefd; 97,737 baseline tests.
Execute inline under standing direction: no subagents or approval pauses.
User owns merging/tagging/publication; #7/#102 stay open; Grep last.

## Native cause and selected approach

PCRE2 10.48 class compilation treats positive Any as the whole character range
and negative Any as empty. Two Any predicates lower to ALLANY exactly when
an ordinary positive class has at least one positive Any, or an ordinary
outer-negative class contains two negative Any predicates. Native misses on
C3A942 at offset 1 in tolerant UTF; the current proof may retry and return 2:1.

Extend only IsUtfAllAny in PcreByteProgram.cs. Preserve its direct Any,
one-full-range and singleton negation-parity branches. For no ranges/exactly
two AnyScalar predicates, both null categories and zero property values,
apply the stated truth table before rejecting outer negation in the unchanged
complement branch. All other two-predicate rules remain byte-identical.
No new iteration, allocation, metadata, range normalization, API or admission.

Rewriting class nodes would alter instruction/work provenance. General semantic
universality would incorrectly broaden property/range/algebra inference.
Choose the exact constant-time pair proof; additional members, triples,
property mixtures and range unions remain separately scoped.

## Validation and delivery

Pin PCRE2 tag 7978954dbd2efc6f2196869290553cf1871b4ce6 and SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9; require every JIT_COMPLETE result 0.
Reproduce all 265 merged266 groups/31,988 protocols before generating new data.
Cover every predicate/outer negation combination, both orders, aliases/casing/
UCP/restrictions, singletons/full ranges/complements and mixed/triple controls,
byte 0..FF controls, groups/alternatives/capture31-32 dependencies/global cuts/
targets, NoDot/COMMIT/start/possessification, NUL/U+10FFFF and malformed UTF,
empty policies/signed captures/marks, exact work/budgets, owned source/options,
diagnostics/whole-subject validation/cancellation/full capacities/proof fallback.
Exclude every malformed strict whole subject and split start from match fixtures;
retain their explicit API diagnostics separately.

Tests-only draft and exact expected RED precede freezing every new C# file and
production. Preserve all 310 prior files/305 C# sources and frozen35. Build
net8/net9/net10, execute net10 only on six native hosts, retain frozen214 outcomes/
120 methods, zero failures/skips/compiler or analyzer warnings/errors and every
support gate. Keep parser/AST/emitter/matcher/VM/work metadata/dependencies/
workflows and all traversal/policy/resource boundaries unchanged.
Seven inventories precede final qualification; final GREEN/review/source/base/
tree/merge evidence lives on PR267 and issue7 footer, with the original prefix
preserved. No documentation-only commit or repeat after final GREEN.

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
