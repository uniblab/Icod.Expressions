# Positive lookahead source anchors and unmatched named SKIP

[PR #303](https://github.com/uniblab/Icod.Expressions/pull/303) recognizes
source start anchors through leading positive lookaheads, including non-atomic
forms. Native interpreted matching stops these anchored attempts before
replaying an unmatched named SKIP. The previous group-only frontier declines
a leading assertion even when all its alternatives begin with source anchors.

Retain the linear emitted-prefix check. The existing bounded compiler frontier
also schedules a positive lookahead's body, retaining every-branch checks and
the existing absolute, ordinary-caret, and search-start evidence. Both atomic
and non-atomic lookaheads qualify. Nested assertions and capturing/noncapturing
groups use the same pending bodies and compile-visit ceiling. Cancellation
remains checked at each body and branch.

Negative assertions, lookbehinds, mixed or empty alternatives, multiline
caret, optional and literal prefixes decline the additional proof. A mixed
root alternative must also decline. Compiled NotBol preserves lexical caret
evidence. This reuses private immutable metadata in the existing unmatched-name
guard; it changes no candidate-search anchoring, runtime walk, admission error,
or consumer declaration.

The pinned native corpus contains 36 definitions across 64 call masks,
for 2,304 outcomes. It covers atomic/non-atomic and nested lookaheads, captures,
root alternatives, NotBol, supplementary values, a nonzero search start, and
nine controls. Two focused tests cover exact observed/unobserved work exhaustion
and cancellation. The suite reaches 107,052 tests. A twentieth package record
checks the same protocols through the freshly packed library.

See the [oracle record](../tests/Pcre/Native/lookahead-anchor-skip-oracle.md).
Observe native-first failures and freeze the catalog, assertions, and test
project before implementation. Final six-host .NET 10 tests, configured
.NET 8/9/10 builds, frozen cases, both API snapshots, package checks, and
compatibility/support results are recorded on the pull request.

Other prefix/atomic/repeat/conditional anchor inference, wider P09 conformance,
resource and performance closure, native UTF-16, later roadmap stages, and
consumer migrations remain open. Grep last.

## Regression checkpoint

At 93ebb4cabf2ae5f7b73b9d54c668829429b92241, tree
857ad0cb8c5de252ea7d00d392a8ee5a4fb4b0d1, Linux x64 reported 29 failures,
107,023 passes, and zero skips in
[run 44848616090](https://github.com/uniblab/Icod.Expressions/actions/runs/44848616090).
The twenty-seven affected theories completed their native comparisons before
failing in the managed consumer. Two resource cases failed at their no-match
assertions. All 107,014 prior tests and nine new controls passed. The log
contains no coded compiler/analyzer warnings or errors.

Freeze the catalog at Git blob 5873c404af5b92db18228f1517b09031a2921c6c,
the assertions at e47870f697f8f8a230fa41bc54c1558bbe9162a7, and the test
project at 748767ede7dc766a3ab7b5c3a71d0afb70a7d8cb before implementation.
