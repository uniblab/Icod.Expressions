# Unmatched named SKIP in anchored interpreted attempts

[PR #298](https://github.com/uniblab/Icod.Expressions/pull/298) preserves
native failure when an unmatched named SKIP is reached during backtracking
in an anchored interpreted attempt. The native driver stops that attempt
before replaying with the unmatched SKIP ignored. Eagerly ignoring the name
can let an earlier alternative match instead.

The bounded correction covers call-time or compiled anchoring, the existing
dot-star anchor policy, and a directly emitted root search-start anchor.
It reuses the existing cut frame and candidate/fragment termination path.
Successful suffixes still complete normally. Default/JIT and ordinary
unanchored unmatched-name handling retain their current paths.

The independent native corpus records 32 definitions across all 64 call
policies, for 2,048 outcomes. It covers alternatives, captures, other marks,
positive lookahead, lookbehind and non-atomic assertions, compiled anchors,
supplementary subject characters, successful suffixes, callee-local cuts,
unanchored replay, and empty ACCEPT with per-call empty-match restrictions.
Two focused tests exercise exact work exhaustion and cancellation.
The suite reaches 106,878 tests. A fifteenth package record checks all
2,048 protocols through the existing consumer.

See the [native record](../tests/Pcre/Native/unmatched-skip-oracle.md).
Observe native-first regression failures and freeze the catalog, assertions,
and test project before changing the matcher. Final six-host .NET 10 tests,
.NET 8/9/10 builds, frozen cases, both API snapshots, package checks, and
configured compatibility/support results are recorded on the pull request.

Interpreted assertion-local named-SKIP propagation, broader source-anchor
inference, and other driver replay boundaries require separate conformance
work. Broader P09, native UTF-16, later roadmap stages, and consumer migrations
remain open. Grep migrates last.

## Regression checkpoint

At a11ea8e24ca49cd85adbb116863c97dadda8c4fa, tree
46b6986f4c41608eb214710f0eb43f06f326f980, Linux x64 reported 30 failures,
106,848 passes, and zero skips in
[run 44810195545](https://github.com/uniblab/Icod.Expressions/actions/runs/44810195545).
All 28 affected theory cases reached the managed comparison after completing
their native checks. The two resource cases failed at the new no-match
assertions. All 106,844 prior tests and four new controls passed.

Freeze the catalog at Git blob 4b54f57064ec6a1cb30eedb6aafc463c5b46f4ae,
the assertions at b865e6c4dca757e6130be82af7d7331a2ea2a06e, and the test
project at 849a02af77b0afa19166f83ad5587e23873a8134 before the correction.
