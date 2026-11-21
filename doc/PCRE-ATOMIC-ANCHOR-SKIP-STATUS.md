# Explicit atomic group source anchors and unmatched named SKIP

[PR #304](https://github.com/uniblab/Icod.Expressions/pull/304) recognizes
source start anchors through leading explicit atomic groups. Native interpreted
matching stops these anchored attempts before replaying an unmatched named SKIP.
The previous compiler frontier declines an atomic wrapper even when every
leading branch begins with source anchor evidence.

Retain the linear emitted-prefix check. The existing bounded compiler frontier
also schedules the noncapturing body inside an explicit atomic wrapper.
Captures, nested atomic groups, positive lookaheads, and all alternatives use
the same existing start-anchor evidence and compile-visit ceiling. Cancellation
remains checked at every body and branch. Possessive quantifiers have a repeat
child and do not enter this added case.

Mixed/empty alternatives, multiline caret, optional groups, possessive optional
repeats, and negative assertions decline the proof. Mixed root alternatives
must also decline. Compiled NotBol preserves lexical caret evidence. This
reuses private immutable metadata in the existing unmatched-name guard; it
changes no candidate-search anchoring, matching-time walk, admission error,
or consumer declaration.

The pinned native corpus contains 40 definitions across 64 call masks,
for 2,560 outcomes. It covers nested and consuming atomic groups, captures,
alternatives, SKIP inside and after the group, lookahead composition, NotBol,
supplementary values, a nonzero search start, and eight source-shape controls.
Two focused tests cover exact observed/unobserved work exhaustion and
cancellation. The suite reaches 107,094 tests. A twenty-first package record
checks the same protocols against the freshly packed library.

See the [oracle record](../tests/Pcre/Native/atomic-anchor-skip-oracle.md).
Observe native-first failures and freeze the catalog, assertions, and test
project before implementation. Final six-host .NET 10 tests, configured
.NET 8/9/10 builds, frozen cases, both API snapshots, package checks, and
compatibility/support results are recorded on the pull request.

Other prefix/repeat/conditional anchor inference, wider P09 conformance,
resource and performance closure, native UTF-16, later roadmap stages, and
consumer migrations remain open. Grep last.

## Regression checkpoint

At 9c788164bf5acbc1223d4bf4c38bcf40eb553883, tree
6b5d6f56774caf938b692ed79718d5d18c246796, Linux x64 reported 34 failures,
107,060 passes, and zero skips in
[run 44860298356](https://github.com/uniblab/Icod.Expressions/actions/runs/44860298356).
The thirty-two affected theories completed their native comparisons before
failing in the managed consumer. Two resource cases failed at their no-match
assertions. All 107,052 prior tests and eight new controls passed. The log
contains no coded compiler/analyzer warnings or errors.

Freeze the catalog at Git blob b6dcdac567c368032f25043afa735cb8ba14e634,
the assertions at 2b3cbeaae1629c5862c5cc11d9ba88143a1b47a1, and the test
project at 8325d53d2f3df098e474d9c23d5cd15e3a8cbecd before implementation.
