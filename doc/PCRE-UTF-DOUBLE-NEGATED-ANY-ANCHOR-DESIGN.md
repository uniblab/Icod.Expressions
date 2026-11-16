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
