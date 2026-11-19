# Interpreted optional lookbehind at UTF fragment barriers

Base merged290 `7ba96098b155eda2be10b3f389682286db21cfc1`, tree
`f0ee83014d67cb25eb5268814818e883509db9d4`;106,554 prior tests. Bounded
P09 malformed-input work; wider P09, mandatory native16/P10-P18, remaining
P00/P01/Stage C and actual migrations stay open. No agents/Python/local SDK
installation. Preserve old C# tests/A05/#7/#102/footer; one Compile link is the
old-test-file XML exception. Grep last; user owns merge/tags/publication.

## Native contract and proposed correction

Pinned pcre2_match.c OP_VREVERSE stops rewinding at check_subject. If the
minimum has been met it reduces the available maximum and tries forward starts;
otherwise it fails. Match-time ANCHORED/ENDANCHORED disable JIT dispatch. On
FF62, `(?<=a?)b` therefore has default/JIT miss but interpreted anchored1:1;
the corresponding negative assertion reverses success. Fixed lookbehind,
insufficient required context and default/JIT barrier behavior remain distinct.

Restrict only the existing ReverseRange invalid-UTF barrier exception under
interpretedUtfFragments. Keep the unconditional reversedCount<minimumReverse
failure, all existing Step/cancellation calls, scalar rewind and retry order,
scoped ends, captures/marks/journal and frame capacity. No parser/program/opcode/
frame/layout/API/quota/proof/dependency/version change, new Step site, clone,
transcoding, rescan or borrowed lifetime.

## Native-first corpus and semantic RED

Recompile unchanged original C probe6fbaee449941be0b462604d3d1c4101d79241a37
against the existing independently built static native8/JIT library, PCRE2
7978954dbd2efc6f2196869290553cf1871b4ce6 /SLJIT
de0259c7aaf36aa40cba8014f3fad3edde9307f9.48x64=3,072 complete outcomes:
1,848 matches/1,224 misses, every JIT compile0/positive size, no errors or
discarded final-corpus rows. Raw SHA256
`f46d73b9a28bf1e3b46da589341496c9c2b8fc848f29cdb092ab108945059477`.
See [native contract](../tests/Pcre/Native/optional-rewind-oracle.md).

48 theories compare every native literal before the unchanged typed policy
consumer/model. It compiles once, overwrites caller pattern bytes, uses original
padded input/metadata identity and interleaves original defaults after every
policy. Cover positive/negative/non-atomic/conditional assertions, empty/nonzero
minimums, greedy/lazy bodies, captures/marks/K, source/default policies, raw/
strict/tolerant/UCP/supplementary input, malformed runs/repeated fragments,
nonzero/split starts, fixed alternatives, required-context failure and physical
end. Four facts cover exact/exhausted shared observed/unobserved work, later
cancellation, empty/nonempty capture lifetime and observed original subject views.
52 additions give106,606 tests.

Tests-only source5eecaf029c9061ce0af78049c32e56a0a71bbc4b,
tree5dc2bbbb0dc86ad2aadbde8823c6c8bfe973cad8, production unchanged. Observe
successful-build semantic RED and reconcile exact identities/first assertions,
all native/default outcomes and106,554 old tests. Freeze new catalog/assertion/
XML blobs before implementation; no subsequent fixture or old C# test edits.

## Package, review and qualification

Append ninth pcre-optional-rewind-smoke record:48 patterns/3,072 protocols/zero
failed. Keep all eight prior records, consumer/model, unique candidate feed/cache
and workflow call sites. Preserve all384 prior C# sources among406 test files,
15 prior sample helpers/data, A05 source35/corpus and unrelated production.

Review whole12-file branch/five baseline inventories plus this design,
[plan](PCRE-OPTIONAL-REWIND-PLAN.md) and [status](PCRE-OPTIONAL-REWIND-STATUS.md)
before qualification. Large syntax/resource/Grep inventories stay base-identical;
roadmap/recent progress link the small addendum. Require six106,606/frozen214
cases/120 methods, library/consumer builds8/9/10, real execution10 only, nine
package records and actual configured compatibility/Gnulib/support/script gates,
zero failures/skips/coded warnings/errors. Verify merged290 main/scripts and
record earlier unstarted PR script cancellations separately. Final source/base/
tree/merge identities/raw allocations/results/plan completion stay externally
on PR291 without source/docs/test edits after GREEN. Remove only exact disposable
research after evidence readback; retain repo/original roadmap and verify ready.

## Observed RED and immutable fixtures

Run44583709141, Linux x64135770865585 /Linux ARM64135770865651, builds all
targets with zero coded warnings/errors and reports42 failed/106,564 passed/
zero skipped/106,606 total. Exact38 theory IDs1-32,43-48 fail only at managed
projection line43 after all64 native literals and original-default assertions.
Positive/conditional paths lose shorter available context; negative polarity
incorrectly succeeds. Four facts first fail on the missing recovered match,
before their resource/lifetime/view checks. All106,554 old tests and10 new
controls pass. No fixture correction. This witness/freeze precedes production;
remaining hosts corroborate before final qualification.

Frozen catalog a0e3c1bebe254bca3adea2bd336fd714a42b51c2; assertions
6098b1d27cbeaac0f4bc103bfa65470be2e95633; XML
3083fec45dcb37cf055f0b499cfab6cc4ca007cd. Unchanged consumer
07ba6226a117768d36a0aab119cb5e4093c81fb7; old policy model/catalog
34b87a6fc455468bdabcb8ad90e6a0ff779fa554; C probe
6fbaee449941be0b462604d3d1c4101d79241a37 remain fixed.

## Reviewed implementation before final qualification

Matcher a2a2d5a7f4157d163c13fffe7d61bfa51bdc09d8 changes one condition
(one addition/deletion): require !interpretedUtfFragments for the extra JIT-only
malformed-barrier rejection. Required minimum failure remains the first OR arm.
For defaults the old condition is identical; fixed reverse and physical-start
behavior stay fixed. On interpreted success after the existing boundary Step,
reuse available reversedCount, unchanged longest-first ReverseRetry and scoped
end/capture rollback machinery. No new Step/frame/layout/quota or allocation
mechanism is introduced. Windows x64135770865597 /ARM64135770866091 and macOS
ARM64135770865327 corroborate the same42 RED identities after the Linux witness/
freeze; final macOS x64 corroboration belongs to external qualification evidence.

Whole12-file author review checks primary OP_VREVERSE/check_subject provenance,
complete literals/RED causes, profile/minimum/fixed/physical and scalar boundaries,
assertion polarity/conditional/non-atomic retry order, original offsets, empty/
unset/capture/mark lifetime, one budget/cancellation, observed original memory
and candidate-only consumer reuse. No introduced unresolved findings or claimed
independent review. All384 old C# sources/406 test files (one XML-link exception),
15 sample helpers/data and large inventories retain base blobs. All other
production is unchanged. A05/classifier/eight-main-job/thirteen-rejection/diff
checks pass; baseline five inventories/design/plan/status precede qualification.

Merged290 main44582654192/scripts44582654185 are green attempt1 at the identical
reviewed source tree. All six106,554/frozen214/120/all-target/eight-package results
and eight main jobs pass with zero failures/skips/coded warnings/errors. All
three scripts pass31; Ubuntu also eight main jobs/thirteen rejection cases.
Earlier unstarted PR macOS script cancellations remain historical; actual merged
macOS script success closes that source-tree gate. Final new-source results/
identities/remaining plan completion/cleanup/ready stay externally on PR291.
