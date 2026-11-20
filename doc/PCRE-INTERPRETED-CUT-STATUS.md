# Interpreted capture rollback after callee cuts

[PR #294](https://github.com/uniblab/Icod.Expressions/pull/294) corrects the
capture journal used when a subroutine reaches a control verb and then fails.
Match-time ANCHORED or ENDANCHORED selects interpreted native execution. In
that profile, selected return captures revert to their values at call entry.
Default/JIT execution can retain selected captures completed before the cut.
Compile-time anchoring alone keeps the default dispatch profile.

For example, a called group captures a in v, reaches PRUNE, and fails to
consume b. The caller's alternative then matches acd. A match-time anchored
call must report v as unset. If an earlier successful invocation supplied v,
the failed call restores that earlier value.

## Implementation and evidence

The existing journal already contains the caller values. Restrict the
selected-capture journal filtering on forced callee failure to calls using
the default dispatch profile. The ordinary rollback then restores interpreted
calls. Successful returns continue through their existing return path.

The native corpus covers 48 definitions across all 64 match policies, with
3,072 complete outcomes. It includes THEN, PRUNE, COMMIT, SKIP, capture
conditions and references after failure, prior successful values, marks,
UTF/UCP, supplementary characters, nonzero offsets, lexical THEN, and calls
that succeed or fail before reaching a cut. All native outcomes are checked
before managed comparisons. See the [oracle record](../tests/Pcre/Native/interpreted-cut-oracle.md).

Four additional facts check shared work limits, cancellation, compiled-anchor
dispatch and independent result lifetime, and observation of the original
subject view. The 52 additions bring the full suite to 106,710 cases. The
package consumer adds an eleventh record using the same 48 definitions and
the existing immutable match-policy consumer.

## Qualification

The tests-only checkpoint precedes the matcher correction. Freeze the case
catalog, assertions, and test-project source link after confirming semantic
failures and passing prior cases. Final qualification includes all six
.NET 10 suites, frozen results, .NET 8/9/10 library and package-consumer
builds, both API snapshots, and configured compatibility and support checks.
Final results are recorded on the pull request.

Broader P09 conformance, resource and performance closure, native UTF-16,
remaining roadmap gates, and downstream migration work retain their current
acceptance requirements.

## Semantic failure checkpoint and fixture freeze

At tests-only commit a2e5797004ad5389faad5044f246ae3afc56e587, tree
212d066b80ca0b7fb7c277bc9927cba92576992d, Linux x64, Linux ARM64, and Windows
x64 each reported 42 failures, 106,668 passes, and zero skips in
[run 44665454614](https://github.com/uniblab/Icod.Expressions/actions/runs/44665454614).
All failures were the four new facts and native comparison cases 1–32 and
43–48. All native assertions, ten new control cases, and 106,658 prior tests
passed before the matcher correction.

The frozen catalog blob is 0007cc5b2f5b7c60128cc3de738ff0d131a5812e.
The assertion blob is 63d9ccb18617814ef62585d43f94199088e6e80f.
The test-project link blob is 8544c4120e8ddb23042b080b4399de7830aab544.
