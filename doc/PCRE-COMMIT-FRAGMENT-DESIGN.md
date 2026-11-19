# COMMIT fragment restart and physical end anchors

Base merged PR288 e46472fbcb17683a4c9afd5d584c7f32cf7da548,
treed0e2a29afd5e602540b4ba15ef93cc266d7e3e2d;106,446 prior tests.
Continue P09's hostile/malformed per-call contract, preserving the experimental
boundary. P09/native16/P10-P18/P00/P01/Stage C and actual migrations remain open.
No subagents/Python/local SDK installation. Old C# tests/A05/#7/#102/footer fixed;
one test-project Compile link is the sole old-test-file exception. Grep last;
user owns merge/tags/publication.

## Native contract and bounded correction

In native10.48 pcre2_match.c, a reached root MATCH_COMMIT becomes MATCH_NOMATCH
before ENDLOOP. Under match-time-anchor interpreter dispatch with tolerant UTF,
a nonterminal valid fragment can then continue to the next valid fragment.
The existing managed root cut returns no-match for the entire input, losing
later results. Introduce only a call-local stop-current-fragment flag at that
existing root guard and leave scoped/call-local cuts, delayed guard reachability,
shared rollback and the VM unchanged. Abandon that fragment after failure; use
the existing outer fragment traversal without resetting the shared work budget.
The default/compiled JIT-profile root cut still returns immediately.

Separately, native OP_EOD/OP_EODN compare true_end_subject. Absolute z/Z anchors
retain the physical subject end even when call EndAnchored checks the current
tolerant fragment end. Restore the original physical absoluteEnd default while
retaining existing scoped lookbehind limits and the distinct matchEnd acceptance
checks. Keep caret/dollar provenance and implicit invalid-fragment line flags.
No grammar/program/opcode/frame/journal/metadata/API/limit/proof/dependency change,
no new subject/program clone or transcoding, no new instruction or Step charge.

## Independent corpus and tests-first qualification

Reuse the unchanged original C oracle at blob6fbaee449941be0b462604d3d1c4101d79241a37
and its pinned PCRE2/SLJIT inputs. [Native contract](../tests/Pcre/Native/commit-fragment-oracle.md)
records52x64=3,328 complete outcomes,1,744 matches/1,584 misses, all JIT compile0/
positive size, raw SHA256
`f7c4a5b23dabe17e98d2fee8b9b5cc2038d41d49a29be5a561aadd238f8b4a5b`.
No native row is discarded. Every expected capture/mark/empty/no-match comes
from this standalone probe, not the managed implementation.

52 theories check all64 native literals before exercising the existing unchanged
typed consumer. It compiles once per definition, overwrites caller pattern bytes,
uses padded subject views and interleaves the original default entry after every
call. Four facts cover one shared exact/exhausted work budget, deterministic
cancellation in the later fragment, discarded-capture/result lifetime and observed
original subject views.56 additions give106,502 cases. Existing mutable progress
hooks are test-only; expected resource numbers are library controls, not native
threshold equivalence.

Publish a successful-build tests-only checkpoint with production unchanged.
Reconcile exact semantic failure identities/causes after all native/default
assertions; require all106,446 old tests to pass and no coded warnings/errors or
skips. Record the observed witness before freezing new catalog/assertion/XML
blobs and implementing the two corrections. The one XML link source-links only
PcreCommitFragmentCases; all old C# tests, prior sample data/helpers, existing
PcreMatchPolicyCase/Consumer and the reused C probe remain byte-identical.

## Package and final review

Append a seventh structured package result requiring52 definitions/3,328 protocols.
Reuse the existing candidate-only helper and all six old records, unique feed/cache,
all target builds and unchanged workflow call sites. Build net8/net9/net10 library/
consumer targets; execute real managed suites/consumers net10 only on six native
hosts. All actual compatibility/Gnulib/support/script gates remain required.

Review the whole branch and five baseline inventories plus this design/plan/small
status addendum before final qualification. Keep the large syntax/resource/Grep
inventories base-identical, with small linked roadmap/recent progress entries.
Require106,502 full/frozen214 cases/120 methods per host, seven package records,
all targets and zero failures/skips/coded warnings/errors. Record source/tree/base/
merge identities, raw allocations/results/plan completion externally on PR289;
no source/docs/test edits after final GREEN. Retain the original roadmap/repository;
clean only exact disposable pcre289-research after evidence readback and observe
ready/open/unmerged state. This is bounded conformance, not whole P09 closure.

## Observed RED and immutable sources

Tests-only source289c0eac0085a37540846808d9a27aac040f35d4,
tree9c68198a3defa61df97190e5085a5b7e8e45a46e, proposed
merged82beb7706c6508d81ab72ed1ef2a866c8b95df8, run44555775526.
Linux ARM64135671286336 and Linux x64135671286475 build net8/9/10 without
coded warnings/errors, then report32 failed/106,470 passed/zero skips/106,502
total. Exact failure identities match: definition IDs1-13,26,31-33,35-39,44-46,
48,51-52 and all four facts. Every failed theory reaches the managed protocol
comparison only after its complete native batch. All24 new controls and106,446
old tests pass, including raw/strict/default paths, overridden/unreached cuts
and existing physical-end successes. No native mismatch or fixture correction.
Windows x64135671286263 and Windows ARM64135671286318 corroborate the same set.
The Linux witness and following freeze precede production implementation; the
remaining hosts corroborate before final qualification.

Frozen sources:

- samples/PackageSmoke/PcreCommitFragmentCases.cs:3883bf052683ffadc89e717bb560d31152be5d19.
- tests/Pcre/PcreCommitFragmentTests.cs:7262ee787cc9faa23ca3c63d4c8c57751895e32f.
- tests/Icod.Expressions.Tests.csproj:58a78b8d41277fb58dcf81cab20cc90df96729ff.
- reused samples/PackageSmoke/PcreMatchPolicyConsumer.cs:07ba6226a117768d36a0aab119cb5e4093c81fb7.
- reused samples/PackageSmoke/PcreMatchPolicyCases.cs:34b87a6fc455468bdabcb8ad90e6a0ff779fa554.
- reused tests/Pcre/Native/match-policy-oracle.c:6fbaee449941be0b462604d3d1c4101d79241a37.

## Reviewed source before final qualification

Matcher blob0758dd9315326372ba66480361795cc8dc06a558; only10 additions/two
deletions in production. A per-candidate committedFragment bool becomes true
only at the reached existing root guard under interpretedUtfFragments. Unwinding
still pops the original frames/charges before leaving the candidate. The outer
fragment loop resumes with its existing state reset; work/cancellation never
reset. A false flag preserves every prior path, including discarded cuts. The
absoluteEnd change restores physical input end only when no scoped limit applies;
the separate matchEnd end-anchoring checks remain untouched.

Whole13-file author review covers primary native COMMIT/true-end provenance,
all unfiltered protocols, exact RED causes, root versus discarded/scoped cuts,
fragment/capture/mark resets, physical/scoped end boundaries, original offsets,
one shared budget/exhaustion/cancellation, retained result/metadata lifetime,
observed byte views and package source/model reuse. No introduced unresolved
findings or independent review claimed. No opcode/program/parser/metadata/frame/
journal/Step site/proof/API/dependency/workflow call-site changes.

All382 prior C# tests among402 old test files stay fixed; one Compile link is
the sole old-test-file change. All13 prior sample helper/data sources, original
56-policy catalog and large syntax/resource/Grep inventories retain base blobs.
A05 source35/corpus identities, classifier, complete eight-main-job/thirteen-
rejection and whitespace gates pass locally. The baseline five inventories and
this design/plan/small status addendum precede final source qualification.

Merged288 closure: main44553241211/scripts44553241374 green at attempt1.
Six106,446 net10/frozen214/120/all-target/six-package results have zero failures/
skips/coded warnings/errors. Main's eight jobs including Gnulib/Intel aggregate
pass; all script hosts report31 checks, Ubuntu also eight main jobs/thirteen
rejection cases. The merged base tree equals the reviewed/tested PR288 tree.
Final current-source results/identities/plan completion stay externally on PR289.
