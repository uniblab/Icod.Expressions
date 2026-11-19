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
