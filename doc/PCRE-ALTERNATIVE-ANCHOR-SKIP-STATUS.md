# Source anchors across alternatives and unmatched named SKIP

[PR #302](https://github.com/uniblab/Icod.Expressions/pull/302) recognizes
source anchoring when every root or nested capturing/noncapturing group
alternative begins with an absolute, ordinary-caret, or search-start anchor.
Native interpreted matching stops these anchored attempts before replaying
an unmatched named SKIP. A linear emitted-prefix check misses a leading choice.

The existing linear evidence remains the first check. For named-SKIP programs
where it declines, a bounded iterative AST frontier checks every leading
branch. A group schedules its body. An empty branch, consuming prefix,
optional path, multiline caret, reset, call, assertion, atomic group, or
conditional declines this additional proof. Source caret remains lexical
anchor evidence when compiled NotBol disables that branch's execution.

Frontier work and pending entries use the existing compile-visit ceiling.
Cancellation is checked at every body and branch. The result reuses private
immutable program metadata and only the existing unmatched-name stop guard.
It introduces no new admission error or matching-time walk.

The pinned native corpus contains 32 definitions across 64 call masks,
for 2,048 outcomes. It covers root/nested alternatives, mixed anchor kinds,
captures, supplementary values, compiled NotBol, and mixed, empty, optional,
consuming-prefix, multiline, and direct-anchor controls. Two focused tests
check exact work exhaustion and cancellation. The suite reaches 107,014 tests.
A nineteenth package record checks the same protocols through the consumer.

See the [oracle record](../tests/Pcre/Native/alternative-anchor-skip-oracle.md).
Observe native-first failures and freeze the catalog, assertions, and test
project before implementation. Final six-host .NET 10 tests, .NET 8/9/10
builds, frozen cases, both API snapshots, package checks, and configured
compatibility/support results are recorded on the pull request.

Broader repeat/assertion/atomic/conditional anchor inference, other driver
replay boundaries, wider P09 conformance/resource/performance work, native
UTF-16, later roadmap stages, and consumer migrations remain open. Grep last.

## Regression checkpoint

At 2f9ecc58a8652908b534578a999d2023e5d523ed, tree
e4efba326455189dc71b6e65b9bad698a24a31e5, Linux x64 reported 28 failures,
106,986 passes, and zero skips in
[run 44842892566](https://github.com/uniblab/Icod.Expressions/actions/runs/44842892566).
The twenty-six affected theories completed their native comparisons before
failing in the managed consumer. The two resource cases failed at their new
no-match assertions. All 106,980 prior tests and six new controls passed.

Freeze the catalog at Git blob a251a529d9194b2bcaacf8ffb28424d99b4ac30f,
the assertions at 8d24a6cbc34acced59877de1424310bf85b91b9e, and the test
project at 8f08d9e8fb2aae1dd2e7f18b42802fe673dcd654 before implementation.
