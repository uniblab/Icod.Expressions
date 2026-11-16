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


## Observed RED, freeze and implementation review

Tests-only source9709708516817332be7e07f8ba1736ed08c56948/tree420766b319e21d168362ee43e2afe675a5e68678, checkout813c6143362bf66a4a6bf82b52aabbc2d6ed9c34 (identical tree/ordered basea7ea3fc91a609bfaa1dd4e14bfdc19dda4acd6ff/head parents), [run43895809155](https://github.com/uniblab/Icod.Expressions/actions/runs/43895809155), Linux x64 job133398921378:78 exact expected managed failures/97,048 passes/97,126 total/zero skips/compiler or analyzer warnings/errors.71 matching IDs4,5,10,11,16,17,22,23,28,29,34,35,40,41,46,47,52,53,58,59,64,65,70,71,76,77,82,83,88,89,94,95,100,101,106,107,110,112,114,116,118,120,122,128,134,144,145,150,151,152,156,157,165,172,173,179,180,181,182,187,188,190,194,235,236,237,238,239,240,241,242 (68 ordinary anchor witnesses plus3 existing zero-width COMMIT entry witnesses128/152/165),6 disabled=false UTF work cases and1 tolerant ownership case. Every native preflight, all96,823 prior tests and225 new controls pass; no fixture corrections.

Freeze before production: referenceca1d0a8e081eb41420ceecf29e8e1f8486bdd087; protocolsdcb716417f128ef009384a7c30bfb450683a30da. Both new blobs and304 prior test files/299 C# sources/frozen35 remain byte-identical. Merged263 main43894601891/scripts43894602469 fully green attempt1:six96,823 net10 results/frozen214/120/all3 library targets/no compiler or analyzer warnings/errors; Intel aggregate133401222539 verifies the same single result; all3 scripts31 checks/Ubuntu8 guards. No retry/code change.

Implementation/whole-branch author review before qualification: PcreByteProgram.cs only renames IsUtfAllAny and admits an exact positive AnyScalar ScalarClass or one-predicate ordinary positive Class with no ranges (null categories/zero property values). Length0 or>2 declines; non-Any singletons decline; length2 retains every prior complement rule. Native compile.c PT_ANY handling and compile_class singleton behavior support both parser forms/positive-caret/loose-name aliases; parser emits negative Any separately and algebra as an expression. No arbitrary semantic universal-set inference, source metadata change or instruction normalization. UTF callsite preserves byte AnyProperty behavior.

Reviewed all plan boundaries: source/provenance/alias/negation/property counterexamples; groups/alternatives/dependency31-32 bits/global cuts/targets; source/caller NoDot/zero-width COMMIT/start/possessification; unchanged instructions/work/exact observed-unobserved budgets/owned slices/signed captures/marks/validation/cancellation/diagnostics/full capacities/proof exhaustion; frozen/source/tree preservation. No new allocation, borrowed lifetime or quota/admission limit. Parser/AST/emitter/matcher/VM/API/dependencies/workflows fixed. No introduced unresolved critical/important/minor findings. Local frozen-source/corpus/A05/classifier/main-job/diff gates pass. Seven inventories precede final source qualification; final GREEN/review/exact source identities remain external.


## Publication gate

Automatic approval review rejected uploading the shared inventory documentation to public GitHub, citing potentially sensitive inventory material. The existing Grep inventory was independently verified public and unchanged (blob1b0e0b9ec0db3204ee9d3fb1f30757e646eefcea/342,135 characters); its20 added lines contain this PR scope and public test/commit evidence. No workaround upload. Publish only the authorized production change and these current design/plan documents for provisional source validation. Five shared inventories remain prepared locally pending explicit publication approval. Do not mark ready or call this final source qualification until all seven inventories are published. Final evidence remains external.
