# Full-range dominance in mixed UTF star classes

Continue merged #270 at 3eb569260d13ed737dbb6bb69071bc9ee68a3f09,
tree e5c329ce5656cf8efa4cfc3f9564449dedfc3f14; 99,166 tests.
Execute inline under standing direction: no subagents, SDK installation or
routine approval pauses. User owns merge/tag/publication; #7/#102 stay open;
Grep last. Preserve all322 prior test files/317 C# sources and frozen35.

## Native cause and implementation

PCRE2 10.48 parse_class recognizes a complete scalar range, suppresses
redundant property payloads through XCLASS_HIGH_ANY, and lowers the full
bitmap/high-character union to ALLANY. Additional property predicates cannot
remove members from that positive union. Native preflight80 profiles/240 rows
confirms the three proposed shapes, reversed member order, casing/UCP/NoStart
and NoDotStarAnchor controls, JIT_COMPLETE0. On tolerant UTF C3A942/start1,
the default inferred star misses; disabled inference matches2:1.

Extend only IsUtfAllAny in PcreByteProgram.cs: remove the empty-predicate
requirement from its existing positive ordinary Class fast path when exactly
one retained range has endpoints0 and0x10FFFF. Retain source provenance,
outer-negation and exact retained-range shape. This constant-time proof
inspects no predicates, consumes no predicate budget and still works after
another alternative has consumed the shared4096 visits. Existing folding
can merge source ranges into that retained shape; do not alter parsing or
folding. Keep the existing bounded positive-Any/list inference unchanged.

General universality, nearly-full ranges, negative outer classes, multiple
retained ranges and algebra remain outside this extension. Cover them using
native-compatible controls, explicitly disabling inference where the native
compiler proves a broader shape than this scoped rule. Do not normalize the
runtime class or change its instructions, work charges, states or allocation.
Preserve entry/global-AST bounds, capture31/32 maps, cuts/calls/targets and
NoDot/COMMIT/start/possessification gates. No syntax admission/API/dependency/
workflow/public-limit changes. Proof exhaustion still declines inference.

## Verification and delivery

Reproduced all551 merged270 groups/65,782 protocols using native tag
7978954dbd2efc6f2196869290553cf1871b4ce6 and SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9, JIT_COMPLETE0. Add independent
full-range mixtures with categories/scripts/binary/Bidi/shorthands/POSIX,
member order, aliases/casing/UCP/scopes, greedy/lazy/possessive stars and
inherited groups/captures/cuts/targets/policies/marks. Select valid whole UTF
subjects and scalar-aligned strict offsets before native execution; malformed
research and diagnostics remain separate. Include bytes/NUL/max-scalar,
all offsets, empty policies, owned source/captures/marks, diagnostics,
instruction4090/4091, capture/depth256, AST-proof exhaustion, validation and
cancellation priority. Pin predicate counts0/1/2/3/64/4096/4097, full-range
proof before and after the shared4096 budget, exact work/limits/cancellation.

Tests-only draft, exact expected RED, freeze every new C# blob, then the
one-condition production change. Build net8/net9/net10, execute net10 only
on six native hosts with frozen214 outcomes/120 methods and support gates.
Update all five shared inventories plus this design/plan before final
qualification. Keep final evidence/identities on the PR, no source/docs edit
after final GREEN. Issue7 footer stays untouched under its existing approval
block. Delete only identity-checked own task scratch after ready/readbacks.

## Native-first inventory and expected RED

New579 matching groups/69,072 protocols,15 diagnostics and55 supplementary
cases:649 new cases,99,815 total. Native dynamic preflight21 groups/35
literal checks, JIT_COMPLETE0 throughout. Exclude17,199 strict-validation
research rows before native execution (9,282 malformed whole subjects and
7,917 split offsets). They are not matching expectations.

Expected tests-only RED228:211 matching identities,6 work,1 owned-source,
6 predicate-count,2 after-budget,1 before-budget and1 work-boundary failure.
Expect99,587 passes; all99,166 prior tests and421 new controls stay green.
Freeze all five new C# blobs only after observing that exact RED.

## Pre-freeze fixture correction

Initial tests-only head d7bc98f47de07863eaa8ed18d4217fa8a69bf072,
run44112346787/Linux134141040126:188 failures/99,627 passes/99,815 total,
zero skips or compiler/analyzer warning/error codes. All17 supplementary
failures matched;171 of211 predicted matching cases failed, with no unexpected
failures. The other40 still used an already-supported positive Any form in
one generated member order. Correct those80 profiles (strict and tolerant)
to the planned full-range-plus-property source. Recompute all579 native
groups/69,072 protocols; outcomes remain literal native pins, JIT0. No prior
file, resource expectation or production source changes. Observe corrected
RED228/99,587/99,815 before freezing all five C# blobs.

## Frozen RED and implementation review

Corrected tests-only head8f80fb4a2797b9f3bb717557727e012eee3fa50f,
tree8cf524425a312fe49ddbadfec05aa9d3d0f64d86, run44113511785, Linux x64
134145508682: exactly228 failures/99,587 passes/99,815 total. All211 matching
identities and17 supplementary failures match; all99,166 prior tests and421
new controls pass. All3 library targets build, zero skips/compiler/analyzer
warning/error codes. Freeze before production:

- tests/Pcre/PcreUtfFullRangeMixedStarAnchorProtocols1.cs: e0857b45609ea51b57807d71f9e859138aa3ad90
- tests/Pcre/PcreUtfFullRangeMixedStarAnchorProtocols2.cs: 159639302dc82294a8c1fe112b4207d5205a01f3
- tests/Pcre/PcreUtfFullRangeMixedStarAnchorProtocols3.cs: 312beb79d2f7fc9e492adc3c442ab89fc2124cfb
- tests/Pcre/PcreUtfFullRangeMixedStarAnchorProtocols4.cs: 9a70383d365122cfce8a3b17ea134b5e5d8010c6
- tests/Pcre/PcreUtfFullRangeMixedStarAnchorReferenceTests.cs: 664225a607add9e013c5d1ae8d4e556b0be820ae

Change only IsUtfAllAny in PcreByteProgram.cs: remove the empty-predicate
condition from the existing exact positive full-range Class fast path, with
a comment explaining union dominance. Preserve Class provenance, positive
outer polarity, exactly one retained range and exact endpoints0/0x10FFFF.
No predicate scan/charge or new allocation/state/instruction. Existing
positive-Any/list paths, shared4096 visits, per-visit cancellation and
exhaustion fallback are unchanged. Caller cancellation still guards each
leading branch. Entry/global-AST/capture31/32/cut/call/target and NoDot/COMMIT/
start/possessification gates remain intact. No parser/AST/VM/API/dependency/
workflow/admission/public-limit or runtime-work changes.

Inline whole-branch author review checked positive/negative/source/range
boundaries, retained shape after existing case closure, predicate counts
through4097, uncharged proof both before and after a4096-member list,
work/limits/cancellation/ownership/capacities/validation and all native pins.
No outstanding defect. Preserve322 prior files/317 C# sources and frozen35.
All seven inventories precede final qualification; final results and source/
merge identities remain external on the PR, with no source/docs edits after
final GREEN. Issue7 footer stays untouched under its existing block.
