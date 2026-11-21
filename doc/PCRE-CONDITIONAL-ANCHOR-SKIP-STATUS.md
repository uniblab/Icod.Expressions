# Conditional source anchors and unmatched named SKIP

[PR #305](https://github.com/uniblab/Icod.Expressions/pull/305) recognizes
source start anchors through capture, recursion, and version conditionals.
Native interpreted matching stops these anchored attempts before replaying an
unmatched named SKIP. The previous compiler frontier declines these headers
even when both branches begin with source anchor evidence.

Retain the linear emitted-prefix check. The existing bounded compiler frontier
schedules the bodies of capture and recursion conditionals, and non-definition
static conditionals, only when there are exactly two explicit branches. Check
both branches, including a version branch that is never selected. Compilation
already visits both bodies within shared budgets; the existing compile-visit
ceiling and cancellation checks at every body and branch remain applicable.

An absent or empty branch, an unanchored inactive branch, or multiline caret
declines the additional proof. Definition and assertion conditions do not enter
the new cases. Compiled NotBol preserves lexical caret evidence. This reuses
private immutable metadata in the existing unmatched-name guard; it changes no
candidate-search anchoring, matching-time walk, admission error, or consumer
declaration.

The pinned native corpus contains 53 definitions across 64 call masks,
for 3,392 outcomes. It covers named/numeric targets, both version outcomes,
nested groups and lookaheads, SKIP inside and after conditionals, NotBol,
supplementary values, a nonzero search start, and ten controls. Two focused
tests cover exact observed/unobserved work exhaustion and cancellation. The
suite reaches 107,149 tests. A twenty-second package record checks the same
protocols against the freshly packed library.

See the [oracle record](../tests/Pcre/Native/conditional-anchor-skip-oracle.md).
Observe native-first failures and freeze the catalog, assertions, and test
project before implementation. Final six-host .NET 10 tests, configured
.NET 8/9/10 builds, frozen cases, both API snapshots, package checks, and
compatibility/support results are recorded on the pull request.

Assertion-condition and other prefix/repeat anchor inference, wider P09
conformance, resource and performance closure, native UTF-16, later roadmap
stages, and consumer migrations remain open. Grep last.

## Regression checkpoint

At a6fd4eefacdb6948f7e4565c507e10ab5f122116, tree
cdffdf466cb560484d4bcaba0023787fc9870d49, Linux x64 reported 45 failures,
107,104 passes, and zero skips in
[run 44925853021](https://github.com/uniblab/Icod.Expressions/actions/runs/44925853021).
The forty-three affected theories completed their native comparisons before
failing in the managed consumer. Two resource cases failed at their no-match
assertions. All 107,094 prior tests and ten new controls passed. The log
contains no coded compiler/analyzer warnings or errors.

Freeze the catalog at Git blob aaae3978e063b150481efd89eafe16d19fc83843,
the assertions at 7eea8006e0aa8e626c49bee62b6fbc6e4da7188d, and the test
project at 9cc9a692dad0e125f399cf954bbdef79d4ee0a71 before implementation.
