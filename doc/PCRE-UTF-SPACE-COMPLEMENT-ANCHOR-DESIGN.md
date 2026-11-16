# UTF horizontal/vertical complement star anchoring

Continue merged262 at3568507574eeab3f18e4d5ef2b4a805923e76157,
treec8c9ecdd3cc185fd7d9ede2b274e5a450a8b804e;96,528 baseline tests.
Execute inline under standing user direction; no subagents/approval pauses.
User owns merge/tag/publication; #7/#102 remain open; Grep last.

## Native cause and scope

PCRE2 10.48 compile_class adds explicit horizontal/vertical lists and their
complements, disabling caseless expansion for these lists. Both operand orders
of `[\h\H]` and `[\v\V]` lower to ALLANY, including with UCP. The existing
managed proof excludes these UTF predicate kinds, so tolerant UTF can retry
beyond a requested split byte where native implicit anchoring suppresses retry.

Extend the constant-time helper in PcreByteProgram.cs to same-kind opposite
HorizontalSpace/VerticalSpace pairs. Rename IsAsciiComplementAllAny to
IsComplementAllAny. Keep ordinary positive Class source, empty ranges, exactly
two predicates, null categories/zero property values. Existing ASCII and mixed
positive DecimalDigit/negative AsciiDigit recognition remains unchanged.

Retain scalar-set matching instructions and work provenance. No parser/AST/
emitter/matcher/VM/API/dependency/workflow changes. Keep bounded alternatives,
capture dependency/32+ bits/global cuts/targets, caller/source NoDotStarAnchor,
disabled-anchor COMMIT entry and independent start/possessification settings.
Additional members/ranges/Any properties, mixed families, algebra and other
anchor inference remain separately scoped; semantic universality alone is
insufficient to predict native opcode provenance.

## Validation and delivery

Pin native PCRE2 10.48 tag7978954dbd2efc6f2196869290553cf1871b4ce6,
SLJITde0259c7aaf36aa40cba8014f3fad3edde9307f9; require JIT_COMPLETE return0.
Independently reproduce merged262 protocols before new fixtures. Cover both
families/orders, UCP/casing/restrictions, byte controls, negative/same-sign/mixed
counterexamples, groups/alternatives/captures/cuts/marks/NoDot/COMMIT, malformed
and split UTF, exact budgets/ownership/diagnostics/full capacities.

Tests-only draft and exact expected RED precede production. Freeze new C#;
preserve all302 prior test files/297 C# sources and frozen35 byte-for-byte.
Build C#13 net8/net9/net10; execute net10 only on six native hosts. Preserve214
frozen outcomes/120 methods. Seven inventories precede final qualification;
final GREEN/review/source/base/tree/merge evidence belongs externally on PR263
and issue7 footer, preserving its original2600-character prefix. No docs-only
CI repeat after GREEN.


Native-first inventory:246 matching groups/23,704 PCRE2 10.48/JIT0 protocols,15 diagnostics and34 supplementary cases (295 new/96,823 total). Keep2,712 strict split-offset(-36) research rows separate. Independently reproduce196 merged262 groups/15,938 protocols. Both horizontal/vertical orders remain native anchored with UCP/casing/restrictions. Preserve302 prior test files/297 C# sources/frozen35. No production changes before exact tests-only RED and fixture freeze.


## Observed RED, freeze and implementation review

Tests-only sourceae3ab2e40ca6162aad0326ed5967747c64e0ae24/tree07515eefece2b6d72fc152c3d3549c3d9ddbca99, checkoutec2835b77caaf1b9949040a46db6e8a1512b5f84 (identical tree; ordered base3568507574eeab3f18e4d5ef2b4a805923e76157/head parents), [run43890422396](https://github.com/uniblab/Icod.Expressions/actions/runs/43890422396), Linux x64 job133379801191:76 expected managed failures/96,747 passes/96,823 total/zero skips/compiler or analyzer warnings/errors.69 matching IDs7,8,13,14,19,20,25,26,34,35,40,41,46,47,52,53,61,62,67,68,73,74,79,80,88,89,94,95,100,101,106,107,110,112,114,116,118,120,122,128,134,144,145,150,151,152,156,157,165,172,173,179,180,181,182,187,188,190,194,197,201,239,240,241,242,243,244,245,246; remaining failures6 disabled=false UTF work cases and1 tolerant ownership case. All native preflights, all96,528 prior tests and219 new controls pass. Three COMMIT entry witnesses128/152/165 supplement the66 ordinary inferred-vs-disabled witnesses: native can execute zero-width COMMIT at a split byte before class consumption; the existing bounded entry proof needs the new class families. No fixture corrections.

Freeze before production: reference8360c699d30a5756b897da28e8c283b864c46a3f; protocols7466e135e4ebbd2a648698555876e29c5fc6db40. Keep both blobs and all302 prior files/297 C# sources/frozen35 byte-identical. Merged262 main43888718709/scripts43888718494 are fully green: six96,528 net10 results/frozen214/120/all3 library outputs/no compiler or analyzer warnings/errors; Intel aggregate133380441980 verifies the same single native result. No retry/code change needed.

Implementation and whole-branch author review before qualification: PcreByteProgram.cs only renames IsComplementAllAny and adds HorizontalSpace/VerticalSpace to same-kind opposite predicate eligibility. Existing mixed DecimalDigit-positive/AsciiDigit-negative exception remains exact. Ordinary positive Class/no ranges/two opposite predicates/null categories/zero property values unchanged; parser emits h/H/v/V independently from UCP/ASCII restrictions and property aliases. Native explicit list union/complement normalization supports both operand orders and ignores casing. No semantic-wide universality inference or class instruction normalization.

Reviewed provenance/counterexamples; groups/alternatives/enclosing dependency31-32 bits/lexical cuts; NoDot/COMMIT/start/possessification policies; unchanged instructions/work/exact observed-unobserved budgets/cancellation/ownership/signed captures/marks/diagnostics/full capacities/proof exhaustion; source/tree/frozen preservation. Surrounding proof traversal, admission limits and all parser/AST/emitter/matcher/VM/API/dependency/workflow boundaries remain fixed. No introduced unresolved critical/important/minor findings. Local frozen-source/corpus/A05/classifier/main-job/diff gates pass. Seven inventories precede final source qualification; final GREEN/author-review/exact source evidence remains external.
