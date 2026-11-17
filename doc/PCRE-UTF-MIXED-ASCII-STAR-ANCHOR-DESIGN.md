# UTF mixed ASCII star anchoring

Continue merged275 at4104ae84422b588f845dfac5b3f3cd494dfe0808,
tree0f2289c4a78e3db3245d468d1f5da64a33534028; floor101,959 tests.
Inline execution; no subagents or local .NET SDK installation. Development
branch/PR updates authorized; user owns merge/tags/package publication.
Preserve347 prior test files/342 C# sources/frozen35. #7/#102 stay open,
Grep last; issue7's publication-blocked footer remains unchanged.

## Native behavior

PCRE2 10.48 pin7978954dbd2efc6f2196869290553cf1871b4ce6 with SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9, independently built with JIT,
reproduces all506 merged275 groups/30,390 protocols, including signed reset
lengths. PCRE.NET1.6.0 remains the CI oracle. All matching JIT_COMPLETE calls
return0 with positive JIT sizes.

`[\p{ASCII}[:^ascii:]]*?B` lowers to ALLANY and misses on tolerant UTF
C3A942 fromoffset1 and FF42 fromoffset0. NoDotStarAnchor matches2:1 and1:1.
Member reordering, aliases, duplicate members, casing/UCP/restrictions and
additional union predicates/literals retain this provenance. Native negative
POSIX ASCII supplies wide inclusion; the positive binary ASCII property fills
the low-byte holes. Extra union members cannot remove full coverage.

The opposite-polarity `[[:ascii:]\P{ASCII}]*?B` and binary-only
`[\p{ASCII}\P{ASCII}]*?B` keep XCLASS provenance, matching2:1 with either
policy. Wrong polarity, wrong binary property and negative outer classes
remain controls. Do not infer native lowering from semantic universality.

## Compiler contract

Modify only PcreByteProgram.cs. A six-counter IsUtfAllAny overload first
invokes the unchanged five-counter overload, which retains the unchanged
four-counter method and all helpers. Established successes return before
new work; established failures retain their exact charges and fallbacks.
This preserves prior predicate/member/coverage/binary/POSIX resource contracts.

Only positive ordinary Source:Class scalar sets attempt the new proof after
old failure. Add an independent4096 predicate-visit budget shared across
leading paths. Precheck the complete predicate list with no partial new
charge when oversized. Resolve the ASCII binary-property ID through the
pinned catalogue, then scan every admitted predicate with cancellation and
one charge, even after both required markers occur.

Require exact positive BinaryProperty/Category:null with that ASCII ID and
exact negative Ascii/Category:null/PropertyValue:0. Return the conjunction
only after the complete scan. Literal ranges are irrelevant to this pair's
universality: do not visit, precheck or charge ranges in the new helper.
Both markers independently prove native full coverage, even when literal
budgets are exhausted. A positive binary ASCII property alone cannot rescue
an exhausted established binary-ASCII range proof; the negative POSIX marker
is mandatory. Pure POSIX pairs keep their own existing budget.

No allocation/sorting/node mutation or runtime state/work change. Preserve
all entry/global-AST/capture31/32/cuts/calls/targets/NoDot/COMMIT, source
ownership, validation, cancellation and resource/admission priorities.
No parser/AST/VM/API/dependency/workflow/public-limit change. Both established
IsUtfAllAny overload bodies and all existing helper bodies remain unchanged.

## Native-first fixtures and RED prediction

324 matching groups/21,456 protocols,15 diagnostics and93 supplementary
cases add432 tests, reaching102,391. Native preflight verifies58 dynamic
groups/174 literal outcomes, plus20 exploration groups/60 outcomes. Vary
source/polarity/property identity, aliases/casing/scopes/UCP, literal ranges,
captures/marks/references/cuts, caller policies and every admitted byte offset.
Exclude strict malformed-whole-subject/split rows before matching.

Pin4096/4097 predicates with0/4097 literal ranges; a shared4096/4097 total;
complete scans after early markers; independence from all five old budgets;
and established paths after new-budget exhaustion. Existing mixed low-range
proofs must bypass new work. Existing general/binary/POSIX scan charges remain
observable. Include exact observed/unobserved work limits/cancellation,
source ownership, diagnostics, instruction4090/4091, capture/depth256 and
erased/global-AST fallback. Budget-declined rows pin valid-subject admission
and work fallback without claiming raw split parity beyond the new bound.

Predict tests-only RED109:79 matching identities and30 supplementary cases
(10 work,3 ownership,2 predicate-boundary,1 shared-boundary,1 range-independent,
5 old-budget-independent,7 old-path-preservation,1 established-mixed bypass).
All101,959 old tests and323 new controls must pass. Reconcile exact identities
and freeze all five new C# blobs before production; native assertions precede
managed comparison. Never edit existing test expectations.

## Observed RED and pre-production freeze

Tests-only head3c2c0705e957d1294cc986d2e9181b9fa8c605b5,
tree297ae31dd9f27c096585bb97a47eeba55ecf75cc, proposed merge
fce9b125fa5f5efda651f1186c7831ca379d9383 has ordered base/source parents
and an identical tree. Run44202431085/Linux x64 job134459671708 confirms
109 failures/102,282 passes/102,391 total, zero skips/compiler warnings/errors.
All79 matching and30 supplementary identities match. Every failure is in the
new reference class; none has an AssertNative frame. All101,959 old tests
and323 new controls pass.

Matching IDs:
3/5/7/11/13/17/19/21/25/27/31/33/35/39/41/45/47/49/53/55
59/61/63/67/69/73/75/77/81/83/87/89/91/95/97/101/103/105/109/111
115/117/119/123/125/129/131/133/137/139/143/145/147/151/153/157/159/161/165/167
265/269/275/277/279/281/282/291/299/301/303/305/307/309/311/313/315/317/319

Freeze all five Git blobs before production:

- PcreUtfMixedAsciiStarAnchorProtocols1.cs 65cd6dec5dccdd07d5fda591eac7e039081e4a0a
- PcreUtfMixedAsciiStarAnchorProtocols2.cs 9a26aaa746745de10a31f091424408d5109baeb8
- PcreUtfMixedAsciiStarAnchorProtocols3.cs 7d5a83cfd210669ba8c0af5ec9fda543f384fc26
- PcreUtfMixedAsciiStarAnchorProtocols4.cs 0d49f56a79e67c433ac17aa56406d8ac824be4fe
- PcreUtfMixedAsciiStarAnchorReferenceTests.cs 20e74f1d0f56ce20ddd2b15c34e932cabe9677e4

All347 prior test files/342 C# sources/frozen35 remain unchanged. No test
edits follow this checkpoint. Expected GREEN total remains102,391.

Strict research exclusions before matching:1008 malformed-whole-subject rows
and864 split offsets. Whole-branch author review verifies exact polarity and
catalogue identity, unchanged established overload/helper bodies and charges,
complete cancellation-aware scans and literal-range independence. No
unresolved introduced finding remains. All seven inventories and local
support gates precede the final qualification.

## Delivery gate

Whole-branch author review, old/frozen identity checks, local support gates and
all seven inventories precede final qualification. Build net8/net9/net10;
execute full102,391 net10 tests and frozen214 cases/120 methods on six native
OS/architecture hosts, with zero failures/skips/compiler/analyzer warnings.
Require configured support/package gates. Final source/base/tree/merge and
qualification evidence remain external on PR276; no source/docs/test edits
after final GREEN. Identity-checked scratch cleanup and ready readback finish
delivery. Broader optimizer/release/Grep readiness remain separate.
