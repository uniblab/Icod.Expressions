# Mandatory-repeat source anchors and unmatched named SKIP

[PR #307](https://github.com/uniblab/Icod.Expressions/pull/307) recognizes
source start anchors through repetitions whose minimum is positive. Native
interpreted matching stops these anchored attempts before replaying an
unmatched named SKIP. The previous compiler frontier declines repeat nodes;
emitted progress marks can also hide the first source anchor from linear proof.

Retain the linear emitted-prefix check. Before classifying a leading source
node, the existing bounded compiler frontier unwraps a possessive wrapper
around a positive-minimum repeat, then inspects that repeat's child. This is
a fixed two-step normalization without AST allocation or an additional loop.
Earlier group, assertion, conditional, and branch-range checks remain in use.
The existing compile-visit ceiling bounds visits and pending entries, and
cancellation remains checked at every body and branch. Both wrappers and
their child already compile within shared budgets.

Zero-minimum optional/star/counted/possessive forms decline. A mandatory repeat
of a literal, mixed/empty alternatives, or multiline caret also declines the
proof. Compiled NotBol preserves lexical caret evidence. The result reuses
private immutable metadata in the existing unmatched-name guard. It changes
no candidate-search anchoring, matching-time walk, emitted instructions,
admission limit, or consumer declaration.

The pinned native corpus contains 60 definitions across 64 call masks,
for 3,840 outcomes. It covers greedy/lazy/possessive and nested repetitions,
captures, atomic groups, positive assertions, ordinary/assertion conditions,
SKIP inside and after a repeat, NotBol, supplementary values, a nonzero search
start, and nine controls. Two focused tests cover exact observed/unobserved
work exhaustion and cancellation. The suite reaches 107,267 tests. A
twenty-fourth package record checks the same protocols through the freshly
packed library.

See the [oracle record](../tests/Pcre/Native/repeat-anchor-skip-oracle.md).
Observe native-first failures and freeze the catalog, assertions, and test
project before implementation. Final six-host .NET 10 tests, configured
.NET 8/9/10 builds, frozen cases, both API snapshots, package checks, and
compatibility/support results are recorded on the pull request.

Other prefix/zero-repeat source inference, wider P09 conformance, resource and
performance closure, native UTF-16, later roadmap stages, and consumer
migrations remain open. Grep last.

## Regression checkpoint

At 351eb59221802c47619d57a7ed58e702452c1344, tree
2c0b79bdd38fdb715039534b23de52b30fc08c1e, macOS ARM64 reported 53 failures,
107,214 passes, and zero skips in
[run 44952895107](https://github.com/uniblab/Icod.Expressions/actions/runs/44952895107).
The fifty-one affected theories completed their native comparisons before
failing in the managed consumer. Two resource cases failed at their no-match
assertions. All 107,205 prior tests and nine new controls passed. The log
contains no coded compiler/analyzer warnings or errors.

Freeze the catalog at Git blob c0c93ab141db5746daab24dc61695b18953edc37,
the assertions at c7d456d1b6424d9c6cf4acb3544a982a9fb9bf9a, and the test
project at 1299a96d889e9aace6f7669a9ed85476a56956cb before implementation.
