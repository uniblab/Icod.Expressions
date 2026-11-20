# Direct source start anchors and unmatched named SKIP

[PR #300](https://github.com/uniblab/Icod.Expressions/pull/300) extends the
interpreted unmatched-name stop check from a directly emitted search-start
anchor to direct absolute-start and non-multiline caret anchors. Native
interpreted matching stops an anchored attempt before replaying with an
unmatched named SKIP ignored. Eagerly ignoring that name permits an earlier
alternative to match.

The existing emitted Start opcode covers absolute start and ordinary caret.
StartLine remains separate for multiline caret. Only the direct first
instruction qualifies here. The matcher reuses its existing delayed cut,
rollback, and candidate/fragment termination paths.

The pinned native corpus contains 32 definitions across 64 call masks,
for 2,048 outcomes. Coverage includes absolute and caret start, alternatives,
captures, propagated assertion failures, supplementary subject characters,
successful suffixes, known marks, empty ACCEPT restrictions, and multiline,
unanchored, and search-start controls. Two focused tests check exact work
exhaustion and cancellation. The suite reaches 106,946 tests. A seventeenth
package record checks all 2,048 protocols through the existing consumer.

See the [oracle record](../tests/Pcre/Native/start-anchor-skip-oracle.md).
Observe native-first failures and freeze the catalog, assertions, and test
project before changing the guard. Final six-host .NET 10 tests, .NET 8/9/10
builds, frozen cases, both API snapshots, package records, and configured
compatibility/support results are recorded on the pull request.

Broader grouped/alternative source-anchor inference, other driver replay
boundaries, wider P09 conformance/resource/performance work, native UTF-16,
later roadmap stages, and consumer migrations remain open. Grep migrates last.

## Regression checkpoint

At d49d936c0e4510fe850539caf79130987361742b, tree
72d37a4da44333eebd091c0e9b4d2a3a4b43ff78, Linux x64 reported 19 failures,
106,927 passes, and zero skips in
[run 44820757136](https://github.com/uniblab/Icod.Expressions/actions/runs/44820757136).
The seventeen affected theories completed their native comparisons before
failing in the managed consumer. The two resource cases failed at their new
no-match assertions. All 106,912 prior tests and fifteen new controls passed.

Freeze the catalog at Git blob 901be63cef2616980ba8909bb57228c81e7336bc,
the assertions at 5578921176e744439850e511c2f317c2af20d39e, and the test
project at 3d917cd8e01c0415d9d00d040b991828814b656c before the correction.
