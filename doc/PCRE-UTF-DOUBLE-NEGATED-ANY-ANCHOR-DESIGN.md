# UTF double-negated Any singleton star anchoring

Continue merged264 at96aad43cbd40c74f6586ea0e82e747606f5df58f,
treeebf777b616fed2059061555d00ae516d0024252e;97,126 baseline tests.
Execute inline under standing user direction; no subagents/approval pauses.
User owns merge/tag/publication; #7/#102 stay open; Grep last.

## Native cause and selected approach

Native PCRE2 10.48 compile_class handles class negation and Any properties
specially. Exact singleton
`[^\P{Any}]` and `[^\p{^Any}]` classes lower to ALLANY, including loose-name
aliases/UCP/casing/restrictions. Negative Any is empty; its outer complement is
universal. Current managed singleton recognition excludes outer negation.
On C3A942 at1 tolerant native misses, while managed can retry to2:1.

Extend only IsUtfAllAny in PcreByteProgram.cs. For an ordinary Class source,
no ranges/exactly one AnyScalar predicate/null category/zero property value,
require equal outer and predicate negation flags. This preserves positive
Any singletons and admits the exact double-negated forms. Keep standalone
positive AnyScalar recognition unchanged. Move the outer-negation rejection
to the two-predicate complement branch, retaining every prior complement guard.
Empty or other-sized sets still decline. No general negation algebra inference.

Parser/AST metadata already preserves both flags. Normalizing matching nodes
would alter instruction/work provenance; semantic universal-set analysis would
misclassify native property unions. Additional members/ranges/duplicates and
class algebra remain separate. Keep all existing bounded group/alternative/
capture31-32 bitmap/global-cut/target/NoDot/COMMIT/start/possessification policy,
admission limits and parser/AST/emitter/matcher/VM/API/dependencies/workflows.

## Validation and delivery

Native tag7978954dbd2efc6f2196869290553cf1871b4ce6,
SLJITde0259c7aaf36aa40cba8014f3fad3edde9307f9; JIT_COMPLETE must return0.
Reproduce all254 merged264 groups/22,492 protocols independently. Pin both
double-negated forms/loose aliases, negative Any and negated complement/mixed
counterexamples, byte controls, UCP/casing/restrictions, groups/alternatives/
dependencies/cuts/marks/policies/COMMIT, malformed/split UTF, exact work/budgets/
owned source/diagnostics/cancellation/full capacities/proof exhaustion.

Tests-only draft/exact expected RED precede production. Freeze new C#; preserve
all306 prior test files/301 C# sources/frozen35 byte-identically. C#13 builds
net8/net9/net10; execute tests net10 only on six native hosts; retain214 frozen
outcomes/120 methods. Seven inventories before final source qualification;
final GREEN/review/source/base/tree/merge evidence external on PR265/issue7
footer, preserving its original2600-character prefix; no docs-only CI repeat.


Native-first inventory:248 matching groups/24,787 PCRE2 10.48/JIT0 protocols,15 diagnostics and34 supplementary cases:297 new/97,423 total. Keep2,507 strict split-offset(-36) research rows separate. Reproduce all254 merged264 groups/22,492 protocols independently. Both double-negated singleton forms/loose aliases remain native anchored under UCP/casing/restrictions; negative Any and outer-negated complementary pairs stay unanchored. Byte full-class controls already use the existing Exact bitmap proof; preserve that behavior. All306 prior test files/301 C# sources/frozen35 remain fixed. Tests-only exact expected RED and new C# freeze precede production.


## Observed RED, freeze and implementation review

Tests-only source316cac8f885b3e5cc223edd398fd8cba8b0f89f5/treeb1a675f65ab2f10dded3aac7880d0f54ce7a8a38, checkout8935ca2d833ee52340ff8215d23f7169f71016b4 (identical tree/ordered base96aad43cbd40c74f6586ea0e82e747606f5df58f/head parents), [run43937691525](https://github.com/uniblab/Icod.Expressions/actions/runs/43937691525), Linux x64 job133546523537:74 exact expected managed failures/97,349 passes/97,423 total/zero skips/compiler or analyzer warnings/errors.67 matching IDs7,8,13,14,19,20,25,26,34,35,40,41,46,47,52,53,61,62,67,68,73,74,79,80,88,89,94,95,100,101,106,107,110,112,114,116,118,120,122,128,134,144,145,150,151,152,156,157,165,172,173,179,180,181,182,187,188,190,194,237,238,239,240,241,242,243,244; remaining failures6 disabled=false UTF work cases/1 tolerant ownership case. Every native preflight, all97,126 prior tests and223 new controls pass. No fixture correction.

Freeze before production:reference6edf26795d16f40a050604359a2fc18289c85d97;protocolsb7432881d1708ba6e99f4ebaaecf867b9fb07e71. Both new blobs and306 prior files/301 C# sources/frozen35 remain byte-identical. Merged264 main43935934306/scripts43935934247 fully green attempt1:six97,126 net10 results/frozen214/120/all3 library targets/no compiler or analyzer warnings/errors; Intel aggregate133547334580 verifies one native result;3 scripts31 checks/Ubuntu8 guards.

Implementation and whole-branch author review before qualification: PcreByteProgram.cs only keeps exact ordinary Class/no ranges/one AnyScalar/null category/zero property value and compares predicate.Negated==set.Negated. This preserves positive singleton Any and recognizes the double negation. Move outer-negation rejection to the two-predicate branch; keep direct positive Any and every prior complement condition unchanged. Zero or>2 predicates decline; non-Any singletons decline; every negated two-predicate set declines. Native PT_ANY class lowering, not arbitrary semantic universality, supports this exact rule. No node normalization/new allocation/borrowed lifetime/quota or admission change. Byte Exact full-class inference and all instruction/work metadata remain fixed.

Reviewed all plan boundaries: source/kind/count/range/negation parity and property/loose-alias controls; existing direct/positive Any and ASCII/horizontal/vertical/mixed-decimal complements; grouped alternatives/dependency31-32 bits/global cuts/targets; source/caller NoDot/COMMIT/start/possessification; exact observed-unobserved budgets/owned slices/signed captures/marks/diagnostics/validation/cancellation/full capacities/proof exhaustion; frozen/source/tree preservation. Parser/AST/emitter/matcher/VM/API/dependencies/workflows fixed. No introduced unresolved critical/important/minor findings. Local frozen-source/corpus/A05/classifier/main-job/diff gates pass. All seven inventories precede final qualification; final GREEN/review/source/base/tree/merge evidence remains external.
