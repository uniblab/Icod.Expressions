# UTF full scalar-range star anchoring

Continue merged265 at410447d1c0de322ec5d6ab56760716abf9233a16,
tree17a90db0123408d1ea38c441e6b920a1630dc394;97,423 baseline tests.
Execute inline under standing direction; no subagents/approval pauses.
User merges/tags/publishes; #7/#102 open; Grep last.

## Native cause and selected approach

PCRE2 10.48 compile_class fills the byte bitmap and marks all higher characters
for a full literal range0..10FFFF, then emits ALLANY. A leading star over
`[\x{0}-\x{10FFFF}]` therefore anchors, including UCP/casing/restrictions.
Current managed UTF proof excludes every nonempty range array. On C3A942 at1
tolerant native misses; managed can retry to2:1.

Extend only IsUtfAllAny in PcreByteProgram.cs: ordinary Class source, positive
outer class, no predicates, exactly one stored range with Start0/End0x10FFFF.
Then retain the existing nonempty-range rejection before all predicate rules.
Keep direct Any, singleton Any negation parity and every two-predicate complement
condition unchanged. Existing case closure already preserves/merges a canonical
full interval; do not add normalization, range algebra or source metadata.

Rewriting matching nodes to Any would change instruction/work provenance.
General semantic universality would misclassify property unions. Multiple-range
unions/additional members/property mixtures/negated ranges/algebra remain separate
unless existing case closure already yields this exact stored full interval.
Keep all bounded group/alternative/capture31-32 bitmap/global-cut/target/
NoDot/COMMIT/start/possessification gates. No parser/AST/emitter/matcher/VM/API/
dependency/workflow or quota/admission changes.

## Validation and delivery

Pin PCRE2 tag7978954dbd2efc6f2196869290553cf1871b4ce6,
SLJITde0259c7aaf36aa40cba8014f3fad3edde9307f9; require JIT_COMPLETE return0.
Reproduce248 merged265 groups/24,787 protocols. Cover full-range spellings,
NUL/U+10FFFF boundaries, near-full/negative/property/range-mixture controls,
byte0..FF controls, casing/UCP/source restrictions, groups/alternatives/
dependencies/cuts/marks/policies/COMMIT, split/malformed UTF, exact work/budgets/
owned source/diagnostics/cancellation/full capacities/proof exhaustion.

Tests-only draft/exact RED precede production. Freeze new C#; preserve308 prior
files/303 C# sources/frozen35 byte-identically. C#13 builds net8/net9/net10;
tests net10 only on six native hosts/frozen214 outcomes/120 methods. Seven
inventories before final qualification; final GREEN/review/source/base/tree/
merge evidence external on PR266/issue7 footer, original2600-character prefix
preserved; no docs-only CI repeat.

## Native-first fixture record

Reproduced248 merged265 groups/24,787 protocols with JIT_COMPLETE0.
New265 matching groups/34,368 successful-or-no-match native protocols;
5,117 strict UTF errors isolated from matching fixtures. Add15 diagnostic
and34 supplementary rows:314 new cases,97,737 expected total.
All native patterns JIT_COMPLETE0. Byte controls use0..FF; Unicode forms
use0..10FFFF. Include NUL/U+10FFFF/near-full/negative/range/property
boundaries; existing Any/complement controls; exact31/32 capture dependencies.
Tests-only source will establish exact RED before freezing both new C# files.
