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
