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
