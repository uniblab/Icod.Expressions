# Multiline caret at interpreted UTF fragment ends

Base merged289 `48779884944a8676cb1f008580556dd92d276f7f`, tree
`3bae145da3c5b19426a4f21a1f6a76e99d9d3cc7`; 106,502 prior tests.
Bounded P09 malformed-input work; broader P09, mandatory native16/P10-P18,
remaining P00/P01/Stage C and actual migrations remain open. No subagents/Python/
local SDK installation. Old C# tests/A05/#7/#102/footer fixed; one Compile link
is the old-test-file XML exception. Grep last; user owns merge/tags/publication.

## Native rule and bounded correction

Pinned pcre2_match.c OP_CIRCM rejects Feptr==end_subject without
ALT_CIRCUMFLEX. In interpreted tolerant matching, end_subject is the current
valid fragment end. Separate true_end_subject controls z/Z. Match-time ANCHORED/
ENDANCHORED disable JIT dispatch. Default JIT-profile caret can accept after a
newline at a nonterminal fragment end. For `(?m)a\n^` on610AFF62, default gives
0:2 while call anchors miss; alternate-caret permits0:2 in both profiles.

Restrict the existing StartLine physical-end allowance under interpretedUtfFragments.
Retain alternate-caret, preceding-newline checks and scoped limits. No change to
physical z/Z, ordinary caret/SearchStart, default/compiled profile, parser/program/
opcodes/frames/journal/metadata/API/quotas/proofs, Step sites, subject/program
ownership or transcoding. Rejection uses existing backtracking/fragment traversal.

## Native-first corpus and freeze

Reuse original C probe `6fbaee449941be0b462604d3d1c4101d79241a37`, PCRE2
`7978954dbd2efc6f2196869290553cf1871b4ce6`, SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`. Independent static native8/JIT
preflight records all48x64=3,072 outcomes:1,680 matches/1,392 misses, JIT0/
positive size throughout, no errors/discarded final-corpus rows. Raw SHA256
`9f6d32a45c6e0f0731e41d6302b1bcfffcdd253c872a127342a67a8843426ad5`.
See [native contract](../tests/Pcre/Native/fragment-caret-oracle.md).

48 theories compare every native literal before the unchanged typed consumer/
model. It compiles once, overwrites caller pattern bytes, retains original padded
input/metadata identity and interleaves original defaults after every policy.
Cover captures/marks/K, positive/negative ahead/behind, alternatives/COMMIT/
atomic/repeated scopes, source/default policies, raw/strict/tolerant/UCP/
supplementary input, nonzero starts, LF/CR/CRLF/NEL/LS, physical/interior ends,
alternate-caret and absolute/search/word/end controls. Four facts cover exact/
exhausted observed/unobserved shared work, later cancellation, discarded/retained
result lifetime and observed original views.52 additions give106,554 tests.

Tests-only source `0d691d4a47d6675de8b9292ba9cb796fdeedbbe7`, tree
`98f6cabbbc8734e6cd701fecd2a0b48c6ae3e9a5`, production unchanged. Observe
successful-build semantic RED; reconcile identities/first assertions/native/default
and old-test results. Freeze new catalog/assertion/XML before implementation;
no later fixture changes or old C# assertion/helper edits.

## Package, review and final gates

Append eighth pcre-fragment-caret-smoke record:48 patterns/3,072 protocols/zero
failed. Preserve seven earlier records, consumer/model, unique candidate feed/
cache and workflow call sites. Build library/consumer8/9/10; execute10 only on
six native hosts. Preserve all383 old C# sources among404 test files,14 prior
sample helper/data sources, A05 source35/corpus and unrelated production.

Review complete branch/five baseline inventories plus design/[plan](PCRE-FRAGMENT-CARET-PLAN.md)/
[status](PCRE-FRAGMENT-CARET-STATUS.md) before qualification. Large syntax/
resource/Grep inventories stay base-identical; link the addendum from roadmap/
recent progress. Require six106,554/frozen214 cases/120 methods/all targets/
eight package records and actual configured compatibility/Gnulib/support/script
gates, zero failures/skips/coded warnings/errors; verify merged289 closure.
Final identities/raw metrics/results/plan completion stay on PR290 without
source/docs/test edits after GREEN. Remove only exact pcre290-research after
evidence publication/readback; retain repository/original PDF and verify ready.

## Observed RED, freeze and implementation review

Run44568393428 builds all three targets without coded warnings/errors. Linux
x64135716367993, Linux ARM64135716368441 and Windows x64135716368594 each
report35 failed/106,519 passed/zero skipped/106,554 total. Exact31 theory IDs
are1-27,38,45,46,48; all fail at managed projection line43 after all64 native
and original-default assertions pass. Positive caret accepts the wrong fragment
end; negative caret rejects it; alternatives lose the later result. Four facts
first expose the earlier0:2 capture instead of later3:1 (or an expected unset
early slot), before their resource/lifetime/view checks. All106,502 old tests
and17 new controls pass. No fixture correction.

This three-host witness and following freeze precede production implementation.
Windows ARM64135716368660/macOS ARM64135716368718 subsequently corroborate the
same35 identities and causes; macOS x64 corroboration remains final evidence.

Frozen catalog a58c6bdcac509dc716352e07bfd990a3b6af6f27; new assertions
088ee7132002d33ceab5f84e75af4efad60a0a64; XML
c0ce9c5a059f73e8058e2b6598eb54e7899615c0. Reused consumer
07ba6226a117768d36a0aab119cb5e4093c81fb7; old policy catalog/model
34b87a6fc455468bdabcb8ad90e6a0ff779fa554; C probe
6fbaee449941be0b462604d3d1c4101d79241a37 stay fixed.

Reviewed matcher6322130d6d2044d66052e14214452300e161d18c changes one condition:
the physical-end allowance now requires !interpretedUtfFragments. When false,
the default/JIT condition is identical; alternate-caret remains a separate OR;
interior positions use the prior first arm; existing scoped absoluteEnd limits
already decline the physical-end allowance. No other anchor, state/layout or
Step site changes. Normal failure uses existing unwind/COMMIT/fragment handling.

Complete12-file author review covers primary OP_CIRCM/fragment versus true-end
provenance, native literal completeness/RED causes, guard precedence and default/
alternate/scoped controls, nested assertion polarity, original offsets and
discarded/retained captures/marks, shared work/cancellation, observed memory and
candidate-only package reuse. No introduced unresolved findings or independent
review claimed. All383 prior C# sources/404 test files (sole XML-link exception),
14 old helpers and large inventories retain base blobs. Local A05/classifier/
eight-main-job/thirteen-rejection/whitespace checks pass. Five baseline inventories
and design/plan/status review precede final source qualification; final results/
identities/remaining plan completion remain external.
