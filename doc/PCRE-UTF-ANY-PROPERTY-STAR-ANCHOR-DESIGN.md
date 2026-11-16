# UTF Any-property star anchoring

Continue merged263 ata7ea3fc91a609bfaa1dd4e14bfdc19dda4acd6ff,
tree48ec2a54b56f6ccf00f014e94abadb874638d5da;96,823 baseline tests.
Execute inline under standing user direction; no subagents/approval pauses.
User owns merge/tag/publication; #7/#102 open; Grep last.

## Cause and selected approach

PCRE2 10.48 compile.c explicitly lowers positive PT_ANY to OP_ALLANY to benefit
auto-anchoring; negative PT_ANY becomes an empty class. compile_class likewise
lowers an ordinary singleton positive Any property to ALLANY. Both direct
`\p{Any}` and `[\p{Any}]`, including positive `\P{^Any}` and loose-name aliases,
therefore anchor a leading star with UCP/casing/restrictions. Managed UTF nodes
retain ScalarClass/ScalarSet provenance and currently miss this bounded proof.
On C3A942 at1 tolerant native misses; managed can retry to2:1.

Rename IsComplementAllAny to IsUtfAllAny and extend only its constant-time
eligibility in PcreByteProgram.cs. Admit a positive AnyScalar ScalarClass with
null category/zero property value, or an ordinary positive Class scalar set
with no ranges and exactly one such predicate. Keep the existing two-predicate
ASCII/horizontal/vertical/mixed-decimal rules unchanged. Retain scalar matching
instructions/work and every surrounding bounded group/alternative/capture/
31-32 bitmap/global-cut/target/NoDot/COMMIT and start/possessification policy.

Normalizing property instructions to Any would alter work/provenance; a semantic
universal-set proof would misclassify native property pairs. Parser-wide opcode
provenance could cover outer negations/additional members/ranges/algebra but
requires a broader design. Those remain separately scoped, as do byte-mode
AnyProperty work inference and other anchor inference. No parser/AST/emitter/
matcher/VM/API/dependency/workflow changes or new admission/resource limits.

## Validation and delivery

Native PCRE2 tag7978954dbd2efc6f2196869290553cf1871b4ce6,
SLJITde0259c7aaf36aa40cba8014f3fad3edde9307f9; require JIT_COMPLETE return0.
Reproduce all246 merged263 matching groups/23,704 protocols before new fixtures.
Cover direct/singleton/positive aliases/loose names, negative/property/full-set
counterexamples, byte controls, UCP/casing/restrictions, groups/alternatives/
dependencies/cuts/marks/NoDot/COMMIT, split/malformed UTF, work/budgets/ownership/
cancellation/diagnostics/full capacities/proof exhaustion.

Tests-only draft/exact expected RED precede production. Freeze new C#; preserve
all304 prior test files/299 C# sources/frozen35 byte-identically. C#13 builds
net8/net9/net10; tests net10 only on six native hosts; frozen214 outcomes/120
methods. Seven inventories precede final qualification. Final GREEN/review/
source/base/tree/merge evidence external on PR264/issue7 footer, preserving its
original2600-character prefix; no docs-only CI repeat after GREEN.


Native-first inventory:254 matching groups/22,492 PCRE2 10.48/JIT0 protocols,15 diagnostics and34 supplementary cases (303 new/97,126 total). Keep2,300 strict split-offset(-36) research rows separate. Reproduce all246 merged263 groups/23,704 protocols independently. Direct/singleton Any aliases lower to ALLANY; negative Any and ordinary Unicode property-complement controls retain their separate native behavior. Byte AnyProperty work inference, additional members/ranges/outer negations/algebra/call-target inference remain separately scoped; their native retry controls use NoDotStarAnchor where needed. Preserve304 prior test files/299 C# sources/frozen35. No production changes before exact tests-only RED and fixture freeze.
