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
