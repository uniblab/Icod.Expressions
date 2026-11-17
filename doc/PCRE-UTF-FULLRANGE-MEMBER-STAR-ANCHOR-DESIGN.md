# Full-range members in UTF star classes

Continue merged #271 at 1de910aba76e9d236f7e29b148528cbcac1f6402,
tree51861f58eabbe598c72dc6b00f57ae46ab1e6933; floor99,815 tests.
Execute inline under standing direction: no subagents, SDK installation or
routine approval pauses. User owns merge/tag/publication; #7/#102 stay open;
Grep last. Preserve327 prior test files/322 C# sources and frozen35.

## Native cause and scoped proof

PCRE2 10.48 merges class ranges before its XCLASS_HIGH_ANY/full-bitmap lowering
to ALLANY. An explicit0..0x10FFFF range dominates a positive union even when
other retained literal ranges and predicates surround it. Reproduced579
merged271 groups/69,072 protocols, JIT_COMPLETE0. Native preflight84 groups/
252 rows confirms first/middle/last range placement, properties, casing/UCP/
NoStart and NoDot controls. Tolerant UTF C3A942/start1 misses with native star
anchoring and matches2:1 when disabled. Negative outer and nearly-full controls
match2:1 either way. Strict invalid subjects/offsets remain research, not
matching expectations.

Extend only the leading-star proof in PcreByteProgram.cs. Keep the existing
single retained full-range constant-time path unchanged. For positive ordinary
Class nodes with more than one retained range, scan for an exact0..0x10FFFF
member, irrespective of surrounding ranges/predicates. Bound the total range
visits across leading paths by MaximumInstructions4096, with a separate local
remaining-range counter. Precheck the whole range count, check cancellation
and charge each visited member, and scan all admitted members even if the
full range occurs early. No sorting, range merging or general coverage proof.

The range counter is independent of the existing predicate counter. A failed
or exhausted range proof must fall through to the established Any/predicate
proof rather than reject that proof. This preserves old inference when an
over-budget range list contains positive Any. Existing exact single-range,
direct Any, singleton/pair and bounded predicate-list paths keep their charges.
Both counters are compile-local, allocation-free and independently bounded;
neither is a public admission limit. Exhaustion declines the new inference.

Keep Source:Class, positive outer polarity and exact endpoints; exclude
negative outer classes, nearly-full ranges, general unions/algebra. Existing
case closure can merge ranges to a single full-range shape and continues to
use the old uncharged path. Preserve entry/global-AST bounds, capture31/32
maps, cuts/calls/targets and NoDot/COMMIT/start/possessification gates. No
parser/AST/VM/API/dependency/workflow/syntax admission, instruction/runtime
allocation/state/work charge or public-limit changes.

## Verification and delivery

Pin native members in first/middle/last positions, property-free and mixed
classes, overlapping/duplicate/literal ranges, categories/scripts/binary/Bidi/
shorthands/POSIX, casing/UCP/scopes, greedy/lazy/possessive stars and inherited
groups/captures/cuts/targets/policies/marks. Strict matching rows require valid
whole subjects and scalar-aligned offsets before native execution. Include
malformed fragments, NUL/max scalar, all offsets, byte controls, empty policies,
owned source/captures/marks, diagnostics, instruction4090/4091, capture/depth256,
AST-proof fallback, validation/cancellation priority and exact work/limits.

Exercise range counts2/3/4/64/4096/4097, shared totals4096/4097, precheck
without partial charge, full-range proof after an exhausted predicate budget,
old constant paths and positive-Any fallback after range-budget exhaustion.
Beyond the range bound, pin valid-subject admission/work fallback without
claiming inferred malformed-UTF parity. Native-first tests-only draft and exact
RED precede freezing every new C# blob and production. Build net8/net9/net10;
execute net10 only on six native hosts, frozen214 outcomes/120 methods and
support gates. Update five shared inventories plus this design/plan before
final qualification. Final evidence/identities external on PR; no source/docs
edits after final GREEN. Issue7 footer remains untouched under its existing
publication block. Delete only identity-checked task scratch after ready.

## Native-first inventory and expected RED

New455 matching groups/55,384 protocols,15 diagnostics and55 supplementary
cases add525 tests (100,340 total). Native boundary preflight21 groups/29
literal checks, JIT_COMPLETE0. Exclude12,411 strict-validation research rows
before execution (6,698 malformed whole subjects/5,713 split offsets).
Expected RED152:137 matching identities and15 supplementary failures
(6 work,1 ownership,1 range boundary,1 shared boundary,1 shared-work boundary,
1 independent-predicate budget,3 established paths after range exhaustion,
1 oversized-list Any fallback). Expect100,188 passes: all99,815 prior tests
and373 new controls. Freeze all four new C# blobs after exact RED.
