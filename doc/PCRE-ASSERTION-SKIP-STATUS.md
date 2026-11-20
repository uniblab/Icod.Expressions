# Interpreted named SKIP across assertion scopes

[PR #299](https://github.com/uniblab/Icod.Expressions/pull/299) resolves
interpreted named SKIP through negative and conditional assertions before
selecting the catch boundary. An unresolved name propagates past an assertion.
A matching MARK inside that assertion allows its ordinary local SKIP catch.
A matching MARK outside it can propagate failure farther outward. An active
callee remains the outer boundary for verbs originating in that invocation.

The correction searches the retained path-mark history above the call boundary
and chooses an assertion catch from the matched MARK's enclosing scopes.
Unresolved root names use the existing anchored-stop or ignored-name handling.
Lookup and scope traversal spend the shared work budget and observe cancellation.
The existing frames, journal, and candidate/fragment termination paths handle
failure. Default/JIT uses its current local-quit and history policy.

The pinned native corpus contains 32 definitions by all 64 call masks, for
2,048 outcomes. Coverage includes negative/conditional assertions, lookbehind,
missing names, interior and exterior marks, nested assertions, active callees,
unanchored replay, successful suffixes, UTF/UCP and tolerant modes.
Two focused tests cover exact work exhaustion and cancellation. The suite
reaches 106,912 tests. A sixteenth package record repeats the 2,048 protocols
through the existing match-policy consumer.

See the [oracle record](../tests/Pcre/Native/assertion-skip-oracle.md).
Observe native-first regression failures and freeze the new catalog,
assertions, and test project before changing production scope resolution.
Final six-host .NET 10 tests, .NET 8/9/10 builds, frozen cases, both API
snapshots, package checks, and configured compatibility/support results are
recorded on the pull request.

Broader source-anchor inference, other driver replay boundaries, wider P09
conformance/resource/performance work, native UTF-16, later roadmap stages,
and consumer migrations remain open. Grep migrates last.

## Regression checkpoint

At 13fbededef651d26e448d0252c03f62e11e0a30f, tree
6899a70b3b2082b23faee15acbaacee0cffb5368, Linux x64 reported 30 failures,
106,882 passes, and zero skips in
[run 44813996795](https://github.com/uniblab/Icod.Expressions/actions/runs/44813996795).
The 28 affected theories completed their native comparisons before failing
in the managed consumer. The two resource cases failed at their new no-match
assertions. All 106,878 prior tests and four new controls passed.

Freeze the catalog at Git blob 055870c1bc787365134adf19d2c5ca53851651dc,
the assertions at 36bc30dd7babc17a2048404da43d07817a4e04c7, and the test
project at 6f9b8a2a561a6aed2633288e309e1805c66f524d before the correction.
