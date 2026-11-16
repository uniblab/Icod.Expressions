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
New265 matching groups/31,988 successful-or-no-match native protocols;
7,497 validation research rows isolated from matching fixtures:4,046 malformed
whole-subject rows and3,451 split offsets. Native itself reports5,117 errors. Add15 diagnostic
and34 supplementary rows:314 new cases,97,737 expected total.
All native patterns JIT_COMPLETE0. Byte controls use0..FF; Unicode forms
use0..10FFFF. Include NUL/U+10FFFF/near-full/negative/range/property
boundaries; existing Any/complement controls; exact31/32 capture dependencies.
Tests-only source will establish exact RED before freezing both new C# files.

## Pre-freeze fixture correction

Initial tests-only source ac672e37be972dad9c58e0223c848c4b71314fa8,
[run 43950586072](https://github.com/uniblab/Icod.Expressions/actions/runs/43950586072),
Linux x64 job133591678536:185 failures/97,552 passes/97,737 total, zero skips
or compiler/analyzer warnings/errors. All failures belong to this new fixture.
Exactly119 groups included malformed strict-UTF prefixes before a valid start.
Native validates its relevant suffix; our documented strict API validates the
whole subject. Keep this contract unchanged and isolate these rows alongside
strict split-offset research. Add an explicit whole-subject diagnostic assertion.
The remaining66 failures are the predicted59 matching/6 work/1 ownership
witnesses. Neither new C# file was frozen; production remained byte-identical.
Requalify this corrected tests-only fixture before freezing or implementation.


## PR266: UTF full scalar-range star anchoring

Continue merged #265 at 410447d1c0de322ec5d6ab56760716abf9233a16,
tree 17a90db0123408d1ea38c441e6b920a1630dc394; 97,423 baseline tests.
PCRE2 10.48 fills its byte bitmap and marks all higher characters for the
positive literal range 0..10FFFF, then emits ALLANY. A leading star over
[\x{0}-\x{10FFFF}] therefore anchors. On C3A942 at offset 1 in tolerant UTF,
native misses; the previous managed proof could retry and return 2:1.

Only IsUtfAllAny in PcreByteProgram.cs changes in production (5 additions,
2 deletions). Require ordinary Class source, positive outer class, no predicates,
and exactly one stored range with Start 0 and End 0x10FFFF. Retain the nonempty
range rejection before all existing singleton Any negation-parity and two-predicate
complement rules. Standalone positive Any, byte Exact bitmap inference, and every
bounded group/alternative/dependency 31/32 bitmap/global cut/target/NoDot/COMMIT/
start/possessification gate remain unchanged. Existing caseless closure can merge
ranges into this exact stored interval; no new normalization or range algebra.
Negated/near-full/partial ranges and property mixtures do not satisfy this guard.
Multiple ranges/additional members remain outside this rule unless existing
case closure already produces the exact one-range shape. Matching instructions,
work metadata and parser/AST/emitter/matcher/VM/API/dependencies/workflows are fixed.
No new allocation, borrowed lifetime, quota or admission limit.

Native-first fixtures reproduce all 248 merged265 groups/24,787 protocols using
PCRE2 tag 7978954dbd2efc6f2196869290553cf1871b4ce6 and SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9; every JIT_COMPLETE result is 0.
New fixtures pin 265 matching groups/31,988 native protocols, 15 diagnostic and
34 supplementary cases: 314 new cases, 97,737 expected total. Cover three full
range spellings, NUL/U+10FFFF endpoints, missing endpoints/negative/property/
range-mixture controls, byte 0..FF controls, casing/UCP/restrictions, groups,
alternatives, dependencies/cuts/marks, caller/source policies, malformed/split UTF,
empty-match policies, signed captures, exact observed/unobserved work budgets,
owned slices/options, validation/cancellation, full instruction/capture/depth
capacities and proof-exhaustion fallback.

### Fixture correction, observed RED and freeze

Initial tests-only source ac672e37be972dad9c58e0223c848c4b71314fa8,
[run 43950586072](https://github.com/uniblab/Icod.Expressions/actions/runs/43950586072),
Linux x64 job 133591678536: 185 failures/97,552 passes/97,737 total. Exactly
119 new groups contained malformed strict-UTF prefixes before a valid start:
native validates the relevant suffix, while our documented strict API validates
the whole subject. Correct fixture selection before freezing; retain explicit
whole-subject validation assertions and unchanged production. Keep 7,497 research
rows separate: 4,046 malformed-whole-subject rows and 3,451 split offsets; native
itself reports 5,117 errors. All 10,234 retained strict comparisons independently
pass whole-subject UTF validation and scalar-aligned starts.

Corrected tests-only source b6495a0a98ab005920bfc07212f6a0bca2a9aac9,
tree 6cab833dd9159b08f7b42435c142cb657e704214, checkout
f1984db953063c0173eea5e9c6053cff1205ded3 (identical tree; ordered base/head
parents), [run 43951573405](https://github.com/uniblab/Icod.Expressions/actions/runs/43951573405),
Linux x64 job 133595405101: exactly 66 expected managed failures, 97,671 passes,
97,737 total, zero skips/compiler or analyzer warnings/errors. The 59 matching
IDs are 7,8,13,14,19,20,25,26,34,35,40,41,46,47,52,53,61,62,67,68,73,74,79,80,
83,85,87,89,91,93,95,101,107,117,118,123,124,125,129,130,138,145,146,152,153,
154,155,160,161,163,167,237,239,241,243,245,247,249,251; add six disabled=false
UTF work cases and one tolerant ownership case. All native preflights, all 97,423
prior tests and 248 new controls pass. The only fixture correction was the
pre-freeze strict-validation separation described above.

Freeze before production: reference fb741b7492915b336be0d3f9361d3203e9e9be17;
protocols 9f438c3b89315114b12c7593c7078e6128e6ab84. Preserve both new blobs,
all 308 prior test files/303 C# sources and frozen35 byte-identically.

Merged265 closure: main 43948453014/scripts 43948452986 green at attempt 1.
Six native 97,423 net10 results/frozen214 outcomes/120 methods, all three library
targets, zero compiler or analyzer warnings/errors; Intel aggregate 133592837315
verifies the same single native result. Script jobs 133584240263/133584240822/
133584240882 each pass 31 checks; Ubuntu passes eight main-job rejection cases.
The actual merge tree matches reviewed9a9eb9d11569d48b7dff3b5e0fb54ed577e8846f
and tested proposed merge260b7b456db363e865b130d0b36fb7ebd07c1aa7. No retry.

### Review and final qualification

Whole-branch author review checks source/kind/count/range/outer-negation/property
boundaries and existing case closure; all prior Any/complement eligibility;
bounded traversal/dependency31-32/global cuts/targets; NoDot/COMMIT/start/
possessification; fixed instructions/work/exact budgets/ownership/signed captures/
marks/diagnostics/validation/cancellation/full capacities/proof exhaustion; source,
tree and frozen preservation. No introduced unresolved critical/important/minor
findings. Execute inline under standing direction; no subagents/approval pauses.

All seven inventories precede final source qualification: these five shared
inventories plus [design](PCRE-UTF-FULLRANGE-STAR-ANCHOR-DESIGN.md) and
[plan](PCRE-UTF-FULLRANGE-STAR-ANCHOR-PLAN.md). Require six fresh 97,737/97,737
net10 results/frozen214 outcomes/120 methods, net8/net9/net10 library outputs,
zero failures/skips/compiler or analyzer warnings/errors and every support gate.
Final GREEN/review/exact source/base/tree/merge identities remain external on
PR266 and issue7's footer, preserving the original 2,600-character prefix.
No documentation-only commit or repeat after final GREEN. #7/#102 stay open;
Grep last; user owns merging/tagging/publication. Additional members/duplicates/
range unions/property mixtures/algebra and wider #102/API/consumer/release work
remain separately scoped.
