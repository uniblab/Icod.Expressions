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

## Observed RED and frozen tests

Tests-only head956c9f129a5d482c97a1cf7497014ddca7d0b2c6,
tree dae67e696f68d4800cf30cce6cf2dee7d1a6fd5a, proposed merge
69183a85ee67f44a726b37d7a13a916c0b5fb954. Run44118987690/Linux x64
job134164806556 reports exactly152 failures/100,188 passes/100,340 total,
zero skips and zero compiler/analyzer warnings/errors. All137 matching
identities and15 supplementary failures match the expected categories;
every failure belongs to the new reference class. All99,815 prior tests and
373 new controls pass. No fixture correction is needed.

Freeze before production; paths are under tests/Pcre/:

| File | Git blob |
| --- | --- |
| PcreUtfFullRangeMemberStarAnchorProtocols1.cs | ea3da1af8d8731f65785faf121b90f8f9e2a0c43 |
| PcreUtfFullRangeMemberStarAnchorProtocols2.cs | 28d25956458bd091076bb1cb7efbcb166e65d571 |
| PcreUtfFullRangeMemberStarAnchorProtocols3.cs | 5b112712b48fb832b9b8214e893c00e2c4745868 |
| PcreUtfFullRangeMemberStarAnchorReferenceTests.cs | 9f0aa7ed94301a0d3739a18af4d2fe500e76d2fc |

All327 prior test files/322 C# sources and frozen35 match the base. Local
frozen-source/corpus/A05/classifier/main-job checks pass before production.
No local SDK is present; final compilation and execution use the configured
native CI hosts without installing an SDK in this workspace.

The137 matching failure profile IDs are:
7,8,19,20,34,35,46,47,61,62,73,74,83,85,87,89,91,93,95,101,107,117,118,123,124,125,129,130,138,145,146,152,153,154,155,160,161,163,167,169,171,173,175,177,179,181,183,185,187,189,191,193,195,197,199,201,203,205,207,209,211,213,215,217,219,221,223,225,227,229,231,233,235,237,239,241,243,245,247,249,251,253,255,257,259,261,263,265,267,269,271,273,275,277,279,281,283,285,287,289,291,293,295,297,299,301,303,305,307,309,311,312,314,316,318,320,322,324,326,328,330,332,334,336,338,340,342,344,346,348,350,353,355,357,359,365,367.

## Implementation review before final qualification

Only PcreByteProgram.cs changes production behavior. HasLeadingDotStarAnchor
creates a second compile-local counter initialized to MaximumInstructions and
passes it by reference alongside the unchanged predicate counter. IsUtfAllAny
retains its direct Any and uncharged exact-single-full-range paths first.
Only positive ordinary Class sets with more than one range and an affordable
complete list enter the new scan. Each visit checks cancellation and decrements
the range counter; an exact full-range member sets a flag without ending the
scan. A successful complete scan returns true; every declined or unsuccessful
scan continues through the existing predicate rules without changing their
charges. No negative-outer list is scanned. Both counters stay nonnegative;
oversized lists consume no partial range budget.

Whole-branch author review covers first/middle/last members, duplicate and
overlapping ranges, near-full/negative/source/algebra boundaries and casing;
single/shared4096/4097 visits, independent predicate exhaustion, complete scans
and positive-Any fallback; capture31/32 dependency maps, global cuts/calls/
targets and entry/AST limits; NoDot/COMMIT/start/possessification policies;
exact work/budgets, owned slices/captures/marks, validation/cancellation,
diagnostics, instruction4090/4091 and capture/depth256 capacities. Existing
parser/AST/emitter/matcher/VM/API/dependencies/workflows and every prior test
remain unchanged. No introduced unresolved review findings.

All seven inventories are updated before final qualification. Final native
GREEN, exact reviewed source/base/tree/proposed-merge identities, support-gate
results and ready readback belong on PR272; Task3 completion is recorded there
without a documentation-only CI repeat. #7/#102 remain open and issue7's footer
stays untouched. The six full suites, frozen214 outcomes/120 methods and all
three library targets remain mandatory before ready status.
