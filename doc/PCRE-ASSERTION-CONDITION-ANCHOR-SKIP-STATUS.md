# Assertion-condition source anchors and unmatched named SKIP

[PR #306](https://github.com/uniblab/Icod.Expressions/pull/306) recognizes
source start anchors through assertion conditionals with an atomic positive
forward guard and an explicit else branch. Native interpreted matching stops
these anchored attempts before replaying an unmatched named SKIP.

Native source analysis checks the guard and else branch. The then branch may
be empty or unanchored without defeating this proof. Conversely, two anchored
bodies do not establish this evidence if the guard is a literal, negative
assertion, lookbehind, multiline caret, or has an empty/unanchored alternative.
An absent or empty else branch must decline. Optional conditions and mixed
unanchored root alternatives must also decline.

Retain the linear emitted-prefix check. The bounded compiler frontier uses
branch ranges so it can schedule the guard's entire body and only the else
branch. Ordinary groups and earlier conditional kinds retain their complete
branch ranges. This adds no synthetic AST nodes. The existing compile-visit
ceiling bounds visits and pending entries; the guard and both bodies already
compile within shared budgets. Cancellation remains checked at every body
and branch. Compiled NotBol retains lexical caret evidence.

The result reuses private immutable metadata in the existing unmatched-name
guard. It changes no candidate-search anchoring, matching-time walk, admission
error, emitted program, or consumer declaration. Matching continues to select
the actual conditional body normally.

The pinned native corpus contains 54 definitions across 64 call masks,
for 3,456 outcomes. It covers true/false conditions, empty/unanchored then
bodies, captures, nested assertions/conditions, atomic and ordinary conditional
composition, SKIP in both bodies or after the condition, NotBol, supplementary
values, a nonzero search start, and eleven controls. Two focused tests cover
exact observed/unobserved work exhaustion and cancellation. The suite reaches
107,205 tests. A twenty-third package record checks the same protocols through
the freshly packed library.

See the [oracle record](../tests/Pcre/Native/assertion-condition-anchor-skip-oracle.md).
Observe native-first failures and freeze the catalog, assertions, and test
project before implementation. Final six-host .NET 10 tests, configured
.NET 8/9/10 builds, frozen cases, both API snapshots, package checks, and
compatibility/support results are recorded on the pull request.

Other prefix/repeat anchor inference, wider P09 conformance, resource and
performance closure, native UTF-16, later roadmap stages, and consumer
migrations remain open. Grep last.

## Regression checkpoint

At 3e4d252372d3c8ca89f7858c23c65421abac1b83, tree
5b35419be3e748e6a47e96ad361d0406da4ffe3f, Linux x64 reported 45 failures,
107,160 passes, and zero skips in
[run 44950109822](https://github.com/uniblab/Icod.Expressions/actions/runs/44950109822).
The forty-three affected theories completed their native comparisons before
failing in the managed consumer. Two resource cases failed at their no-match
assertions. All 107,149 prior tests and eleven new controls passed. The log
contains no coded compiler/analyzer warnings or errors.

Freeze the catalog at Git blob bb1d32b8dda32c0db6b6afccc51e5b780267bc19,
the assertions at 96cd61b9d580adfd56bbba859b9fb1875b35f6bc, and the test
project at c68560306dfc4d6fa4ea1b05fefc2d74b1ddb394 before implementation.
