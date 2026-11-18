# UTF built-in class complement star anchoring

Continue merged PR276 at `4ae09332139d5d4e20c2abe1c00653d9b9c3fd2b`,
with 102,391 existing tests. Work inline without subagents, Python or a local
.NET SDK installation. User owns merge, tags and publication. Keep #7/#102
open and Grep last; preserve issue #7's publication-blocked footer.

## Native provenance

Pinned PCRE2 10.48 `7978954dbd2efc6f2196869290553cf1871b4ce6` and SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9` reproduce all 324 merged PR276
groups / 21,456 protocols. Matching requires JIT_COMPLETE return zero and a
positive JIT size. PCRE.NET 1.6.0 remains the CI oracle.

`[[:alpha:][:^alpha:]]*?B` misses on tolerant UTF C3A942 from byte 1;
NoDotStarAnchor selects 2:1. Ordinary positive classes with complementary
finite ASCII POSIX members lower to native ALLANY. Negative ASCII specials
contribute wide inclusion; their positive partners fill the byte bitmap.
Horizontal/vertical whitespace lists also have complementary native ranges.
Under UCP, POSIX blank is a list-based exception; general Unicode
property pairs retain XCLASS even when their membership is universal.

Admit exact same-kind complementary pairs for AsciiAlnum, AsciiAlpha,
AsciiBlank, AsciiControl, AsciiDigit, AsciiGraph, AsciiLower, AsciiPrintable,
AsciiPunctuation, AsciiSpace, AsciiUpper, AsciiWord, AsciiHexDigit,
HorizontalSpace, VerticalSpace, UnicodeBlank. Require null
category and zero property value. Do not include Ascii: its earlier independent
proof owns its resource boundary. No general containment, property complement,
cross-kind, range-union or class-algebra inference is introduced.

Member order, duplicate members, extra literals/properties and caller/scoped
casing do not remove a proven pair. UCP substitutions and ASCII restrictions
must be classified by their retained typed nodes, not their source spelling.
Repeated outer groups, DEFINE-leading and COMMIT-leading raw split entry stay
research controls on valid aligned subjects; their wider frontier proofs are
outside this increment. Their raw native results are preserved during research.

## Bounded compiler proof

Only PcreByteProgram.cs changes in production. Add a seven-counter overload
which first invokes the unchanged six-counter proof and returns every earlier
success before new work. Keep all established overload/helper bodies unchanged.
Only failed positive ordinary Source:Class sets attempt the new proof.

Use one independent compile-local 4,096-predicate budget shared across leading
paths. Precheck the entire array without partial charge when oversized. Scan
every admitted predicate with cancellation and one decrement, including after
finding a pair. Map the explicitly listed exact kinds to fixed mask bits;
aggregate positive and negative masks and test their intersection after the
complete scan. Extra unknown predicates are ignored, never mapped by enum
ordinal. Literal ranges consume no new budget. Exhaustion declines inference
without rejecting syntax, changing public quotas or rescuing older Ascii proofs.

No allocation, sorting, AST mutation, instruction, matcher, runtime state/work,
parser, API, dependency or workflow change. Preserve entry/global-AST/capture
31/32/cuts/calls/targets/NoDotStarAnchor/COMMIT guards, source ownership,
UTF validation, work/cancellation and admission priorities.

## Native-first qualification

402 matching groups / 21,952 native protocols, 15 diagnostics and 58 supplementary
cases add 475 tests, reaching 102,866. Strict malformed whole subjects and
unaligned strict starts are excluded before matching. Dynamic native preflight
pins 22 budget groups / 66 literal outcomes, all JIT zero with positive sizes.
Cover all admitted kinds, UCP property controls/list exception, scopes,
extra union members, signed captures/marks, wrappers/references/cuts, caller
anchoring/empty policies, malformed fragments, NUL and U+10FFFF.

Supplementary cases pin exact observed/unobserved budgets and cancellation,
owned sliced source/options, 4,096/4,097 predicate lists with early/late markers
and 0/4,097 literal ranges, aggregate 4,096/4,097 visits, old-success bypass,
instruction 4,090/4,091 boundaries, capture/depth 256 and global-AST fallback.
Declined proofs assert valid-subject behavior/work without claiming native
raw split parity beyond the proof boundary.

Publish tests-only, reconcile predicted RED identities, then freeze all five
new C# blobs before production. Preserve all 352 prior test files / 347 C#
sources and frozen A05 source 35. Review the whole branch and update these two
documents and all five shared inventories before final source qualification.
Build net8/net9/net10; run full net10 and frozen 214 cases / 120 methods on six
native hosts with zero failures/skips/compiler warnings/errors and all configured
support gates. Final identities/evidence and plan completion stay on PR277;
no source, tests or docs edits after final GREEN.

Expected RED is 123 matching identities plus 19 supplementary cases: ten
anchored-work, three ownership, four admitted single-list boundaries, one
admitted shared boundary and one established-success bypass. Expect 142
failures / 102,724 passes / 102,866 total, zero skips/compiler warnings/errors,
with all 102,391 prior tests and 333 new controls passing. Reconcile the exact
matching IDs against the independently saved native enabled/disabled pairs.
Strict research exclusions are 504 malformed-whole-subject rows and 288 split
starts. A further 132 raw protocols for the three wider frontier shapes above
remain research; their valid aligned controls stay in the fixture.

## Refined tests-only scope

Initial source `6fa677154839899583f921deafdcf306fa025708`, tree
`136f4d87701da60a1ab7779279acb5477a8c370e`, proposed merge
`175f01e93f05339883b71c5bac6bfcbf34eac6d2`, run 44212867287 / Linux x64
job 134495333672 built all three targets with zero compiler warnings/errors:
146 failures / 102,720 passes / 102,866 total / zero skips. All old tests pass.
Four matching failures (207/208/319/320) expose a separate UCP xdigit start-study
boundary, including with NoDotStarAnchor: retain their original raw protocols
on that immutable source and restrict these unfrozen controls to aligned valid
subjects. Exclude UnicodeHexDigit from the new proof until its provenance and
independent optimizer behavior have a separate native contract. This removes
another 132 raw research rows; no native expected span is changed.

Fourteen predicted matching failures already pass through the established
digit/word/space/horizontal/vertical pair proofs, including reversed order and
POSIX spellings. The remaining 123 matching identities are exact. All 19
supplementary failures are predicted, but the UCP ownership case reaches a
native assertion because unrestricted alpha becomes a Unicode property pair.
Use explicit (?aP) for UCP work/ownership source so those cases exercise the
intended finite ASCII pair; 38 independent supplementary native preflights
verify the corrected sources, work subjects and literal captures/marks.
Production and all old test blobs stay unchanged. Freeze only after refined
RED confirms 142 managed failures / 102,724 passes and zero native failures.

## Observed refined RED and fixture freeze

Tests-only head `b79d073df0388c4ca9fadaf1e72f56e69c488e28`, tree
`0fc269eb721d5deb46e7d2b602f6f1f10695d3eb`, proposed merge
`3259544805c7778094671a36d5db32ae240cf79e` have the exact source tree and
ordered base/source parents. Run 44213868327 / Linux x64 job 134498872591:
exactly 142 managed failures / 102,724 passes / 102,866 total / zero skips
and compiler/analyzer warnings/errors. All 123 matching identities and all
19 supplementary identities match the refined prediction. No native assertion
frame or unrelated failure; all 102,391 old tests and 333 new controls pass.

Freeze before production:

- `tests/Pcre/PcreUtfClassComplementStarAnchorProtocols1.cs`: `ebf102b1f47edf6e03d0a78a71a3a5a5349d9d52`
- `tests/Pcre/PcreUtfClassComplementStarAnchorProtocols2.cs`: `020f7da70a35e3283f99636b52b6db9c4bfe022b`
- `tests/Pcre/PcreUtfClassComplementStarAnchorProtocols3.cs`: `a90a52987a49369593b87f733242b6da28f63e17`
- `tests/Pcre/PcreUtfClassComplementStarAnchorProtocols4.cs`: `49dda95d5f1751a0a9cbcba86e66951921844523`
- `tests/Pcre/PcreUtfClassComplementStarAnchorReferenceTests.cs`: `f635df01fa88a21fc9e7153664796f655972090b`

Whole-branch author review: the new seven-counter wrapper returns established
successes before new work. The complete exact-metadata mask scan retains all
16 native-proven kinds and excludes Ascii and UnicodeHexDigit. No early return
after finding a pair; full-list precheck, shared counter and per-visit
cancellation preserve bounded compile work. Literal ranges are not visited.
Removing only the new wrapper/helper, local and argument restores the entire
previous production file byte-for-byte, proving every old body is unchanged.
All old/frozen tests, A05 sources/corpus and local support gates remain fixed.
No unresolved introduced finding; independent agent review was not performed.
All seven inventories precede final qualification. Final completion is recorded
externally on PR277 without edits to the qualified tree.
