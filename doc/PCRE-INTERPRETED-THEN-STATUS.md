# Interpreted THEN catches in called positive assertions

[PR #295](https://github.com/uniblab/Icod.Expressions/pull/295) corrects the
scope that catches a failing THEN inside a called positive assertion.
Match-time ANCHORED or ENDANCHORED selects native interpreted execution.
That execution fails the assertion and can retry an earlier choice in the
called group. Default/JIT dispatch can quit the call at an unowned THEN.

For example, the called group first captures a from the choice a|ab. Its
positive lookahead reaches THEN and fails to find c at the following b.
Interpreted matching retries the longer ab choice, then finds c and completes
the match. The existing managed scope rule can miss that match.

## Implementation and regression coverage

The matcher already has assertion guards and the earlier callee alternatives.
Under interpreted dispatch, let the positive assertion catch THEN. Its
ordinary failure path then restores state and retries the earlier choice.

The native corpus covers 48 definitions across all 64 match policies, with
3,072 complete outcomes. Cases include positive lookahead and lookbehind,
named THEN and successful marks, nested and atomic assertion bodies,
selected and unselected return captures, empty captures, casing, UTF/UCP,
supplementary characters, and nonzero offsets. Negative, conditional, and
non-atomic assertions and other control verbs provide independent controls.
See the [oracle record](../tests/Pcre/Native/interpreted-then-oracle.md).

Four additional facts check shared work limits, cancellation after an
assertion retry, compiled-anchor dispatch and result lifetime, and observation
of the original subject view. The 52 additions bring the full suite to
106,762 cases. The twelfth package-consumer record uses the same native
definitions through the existing immutable match-policy consumer.

## Qualification

Confirm the tests-only semantic failures and passing prior cases before
changing the matcher, then freeze the catalog, assertions, and test-project
source link. Final qualification includes all six .NET 10 suites, frozen
results, .NET 8/9/10 library and package-consumer builds, both API snapshots,
and the configured compatibility and support jobs. Final results are recorded
on the pull request.

Broader P09 conformance, resource and performance closure, native UTF-16,
and the remaining roadmap gates retain their current acceptance requirements.

## Semantic failure checkpoint and fixture freeze

At tests-only commit 0890df4eff4095c0475759de541dac609b39b286, tree
8d831388782eb4b7913d14ea394544c80e30f795, Linux x64 and Linux ARM64 each
reported 36 failures, 106,726 passes, and zero skips in
[run 44673572825](https://github.com/uniblab/Icod.Expressions/actions/runs/44673572825).
The failures were the four new facts and native comparison cases 1–32.
All native assertions, sixteen new control cases, and 106,710 prior tests
passed before the matcher correction.

The frozen catalog blob is e36b643c012ba2bd898acf96630c7449f1b5d482.
The assertion blob is e5a48541028537a5d6880f42f4071c70d1d4e4e3.
The test-project link blob is 5045887dffbf2b482803cf9949be0f8bb9480be4.
