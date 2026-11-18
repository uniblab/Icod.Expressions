# UTF finite ASCII union star anchoring

Continue merged PR278 at `a64396a03836b21a01dc173ce961a60692664c80`,
tree `1b2aa06f81dd7ef0f81363bb9519285f72f73423`; 103,351 existing tests.
Work inline without subagents, Python or a local SDK installation. User owns
merge, tags and publication. Keep #7/#102 open and Grep last; preserve issue
#7's publication-blocked footer.

## Native provenance and scope

Pinned PCRE2 10.48 `7978954dbd2efc6f2196869290553cf1871b4ce6`, SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`; unchanged PCRE.NET 1.6.0 CI
oracle. An unmodified out-of-tree static JIT build reproduces 428 merged PR278
groups / 22,936 protocols. Every matching probe requires JIT_COMPLETE zero,
positive JIT size and no native execution error. Reproduce with configure
--enable-jit --disable-shared --enable-static, make -j4 libpcre2-8.la, then
link an 8-bit C oracle against .libs/libpcre2-8.a.

Native parse_class combines a negative finite ASCII class's high-character
inclusion with byte bitmap coverage. Ordinary positive case-sensitive classes
such as [\D0-9], [[:^alpha:][:word:]] and [[:^digit:][:^space:]] lower to
ALLANY. Their lazy stars before B miss on tolerant UTF C3A942 from byte 1;
NoDotStarAnchor selects 2:1. A missing ASCII digit ([[:^digit:]0-8]) retains
XCLASS and selects 2:1 under both policies.

Admit only exact null-category/zero-property finite kinds: AsciiAlnum,
AsciiAlpha, AsciiBlank, AsciiControl, AsciiDigit, AsciiGraph, AsciiLower,
AsciiPrintable, AsciiPunctuation, AsciiSpace, AsciiUpper, AsciiWord,
AsciiHexDigit, plus positive Ascii. Require a negative finite non-Ascii
predicate; all literal ranges must lie within 0..127. Decline caseless sets,
negative Ascii, unsupported metadata/kinds, wide ranges and exact same-kind
positive/negative pairs. Earlier proofs retain ownership even after exhaustion.
Unicode/binary/Any/horizontal/vertical predicates and unrestricted UCP stay
outside the new proof. Scoped (?aP) can preserve the finite POSIX kinds.

## Bounded compiler proof

Only PcreByteProgram.cs changes in production. An eight-counter overload
calls the unchanged seven-counter proof first, returning every old success
without a new charge. Add one independent compile-local 4,096-item counter,
shared across leading paths. Precheck the combined predicate/range lengths;
oversized lists consume no partial charge. Visit every admitted predicate and
range with cancellation and one decrement, even after full coverage, an
unsupported member or an existing owned pair appears.

Use existing immutable static byte bitmaps, complement negative members,
and union low literal ranges via constant-time ulong masks. Guard 63/64/127
boundaries explicitly; no shift by 64, FromPredicate/Range allocation,
sorting, AST mutation or enum-ordinal inference. Explicit positive/negative
kind bits identify existing pair ownership. Complete 256-bit coverage plus
wide inclusion proves native ALLANY only inside the conservative source scope.
No parser/matcher/VM/runtime work/API/dependency/workflow/public-limit change.
Preserve all earlier entry, capture, cuts/calls, COMMIT and whole-AST guards,
UTF validation, source ownership and PR278 fragment-recovery behavior.

## Native-first qualification

122 class shapes produce 426 groups / 22,214 native protocols. Exclude 364
strict malformed whole subjects and 208 strict split starts before matching.
Cover every finite kind, exact/missing coverage, order/duplicate/range/positive
containment/disjoint-negation combinations, UCP/scopes/casing, wrappers,
captures/marks, cuts/references, caller/empty policies and malformed fragments.
Unproved shapes use valid aligned controls without claiming raw parity.

Fifteen diagnostic cases and 56 supplementary cases add 497 tests, reaching
103,848. Supplementary native preflight independently verifies 13 combined
boundary/ownership shapes / 66 literal outcomes, all JIT zero/positive size.
Pin work/cancellation, source/options ownership, instruction/capture/depth and
global-AST capacities, 4,096/4,097 combined items, early/late coverage, shared
budgets, old-success bypass and new-budget exhaustion with old success.
Expected RED: 149 matching identities plus 16 supplementary cases, or 165
failures / 103,683 passes / 103,848 total / zero skips. All 103,351 old tests
and 332 new controls must pass; reconcile exact identities, then freeze all
five new C# blobs before production. Do not alter old tests.

Preserve 362 prior test files / 357 C# sources and A05 source 35/corpus.
Review the whole branch and update both documents and all five shared
inventories before qualification. Build net8/net9/net10; run full net10 and
frozen 214 cases / 120 methods on six native hosts, zero failures/skips/coded
warnings/errors and all configured support/package/script gates. Final source,
base/tree/merge identities and plan completion remain external on PR279.
No source, tests or docs edits follow final GREEN.

## Observed tests-only RED and freeze

Head `d64e0afe171f5217113be48ea2722203e13200a2`, tree
`ffc5a4506ad9b39fb318d891c7f91bbbef70c024`, proposed merge
`7cabc5b04bd78d847bfc58375342f6ef32ea02ba`. Run 44305698841 / Linux x64
job 134806571618 builds all three targets with zero coded compiler/analyzer
warnings/errors, then reports exactly 165 failures / 103,683 passes / 103,848
total / zero skips. All 149 matching IDs equal the native shadow prediction,
including both COMMIT policies. The 16 supplementary failures are eight work,
two ownership, three admitted combined-list boundaries, one shared boundary,
one old-success bypass and one new-exhaustion/old-success. There are no native
assertion frames or unrelated failures; all 103,351 prior tests and 332 new
controls pass. All five new C# blobs were frozen before production:

- `tests/Pcre/PcreUtfFiniteAsciiStarAnchorProtocols1.cs`: `310b9d5abcea56a7f824226d671822926c025c90`
- `tests/Pcre/PcreUtfFiniteAsciiStarAnchorProtocols2.cs`: `b993c67467edf1b67d55324004bba154569f23fa`
- `tests/Pcre/PcreUtfFiniteAsciiStarAnchorProtocols3.cs`: `14d6ffc0b8554a756ded48bbb525acbe10eb9cb3`
- `tests/Pcre/PcreUtfFiniteAsciiStarAnchorProtocols4.cs`: `3f4550a62be8cf02eb746aaccb9937ce63440b9e`
- `tests/Pcre/PcreUtfFiniteAsciiStarAnchorReferenceTests.cs`: `90a6f36cd034f69018811a1c0be05131dbbb055f`

## Whole-branch author review

The eight-counter wrapper preserves every existing success before new work.
All exact finite kind/bitmap mappings, negative high-character inclusion,
63/64/127 mask boundaries, unsupported metadata and owner exclusions are
reviewed. Both complete-list scans retain charges and cancellation after
coverage/pair/unknown members; oversized combined lists do not partially
charge. Existing pair/negative-Ascii/high-range owners are not rescued.
Removing only the new wrapper/helper/local/argument restores the entire old
production file byte-for-byte. Matcher/VM/Step handling and the PR278 fragment
barrier remain unchanged. All 362 prior test files / 357 C# sources, new frozen
fixtures, A05 35 sources/corpus/candidate identities are retained. No unresolved
introduced finding; no independent agent review was performed.

Local A05/corpus/candidate, CI classifier, eight main jobs/thirteen rejection
controls and diff whitespace gates pass. Both documents and all five shared
inventories precede final qualification. Final exact identities, six-platform
results and plan completion remain external on PR279 without subsequent
source/docs/test edits.

## Revision 4 alignment

The user-requested 2026-11-18 full roadmap review is recorded in
[roadmap status](PCRE-ROADMAP-STATUS.md#revision-4-stocktake-2026-11-18).
This bounded 8-bit correctness/analysis increment does not close P09, P10-P18,
consumer migration or the still-open Stage C normalization. Native UTF-16
without transcoding or a copied parser/HIR remains mandatory before stable
PCRE publication. All seven inventories, including the stocktake, precede
final source qualification.
