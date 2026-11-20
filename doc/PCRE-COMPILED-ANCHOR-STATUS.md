# Compiled anchors and native call-policy routing

[PR #296](https://github.com/uniblab/Icod.Expressions/pull/296) corrects the
native reference path for compiled anchor defaults. Passing those defaults
back as match-time flags can select interpreted matching during a comparison
that requires the default/JIT profile.

| Setting | Native destination |
|---|---|
| Compiled ANCHORED and ENDANCHORED defaults | Compile options |
| ANCHORED and ENDANCHORED supplied for a call | Match options |
| Compiled empty-match and line-boundary restrictions | Match options combined with the call's restrictions |

The C probe and shared test adapter use the same separation. Seven existing
policy test families use the shared adapter and verify their literal catalogs.

## Independent native evidence

A corrected out-of-tree copy of the pinned native8/JIT probe establishes
48 definitions across all 64 call policies. The 3,072 outcomes include
2,732 matches and 340 misses. Comparing them with the original routing finds
448 differing outcomes across thirty definitions.

The definitions combine all three compiled-anchor combinations with control
verbs and return captures, positive assertions, named marks, malformed UTF,
empty results, literals, supplementary values, and successful calls.
Additional defaults exercise empty-match and line-boundary restrictions.
Replaying the original 56 definitions reproduces all 3,584 recorded outcomes.

See the [oracle record](../tests/Pcre/Native/compiled-anchor-oracle.md).
The 48 new tests bring the full suite to 106,810 cases. A thirteenth
package-consumer record checks the same definitions using the existing
managed match-policy consumer.

## Qualification

The first checkpoint extracts the existing native option mapping and adds
the independent catalog. Confirm its native comparison failures before
correcting the shared adapter and C probe. Freeze the catalogs, assertions,
adapter call sites, and project source link before the correction.

Final qualification includes the six .NET 10 suites, frozen results,
.NET 8/9/10 library and package-consumer builds, both API snapshots, and the
configured compatibility and support jobs. Results are recorded on the pull
request. Broader conformance, resource, performance, native UTF-16, and
remaining roadmap requirements retain their current acceptance gates.

## Failure checkpoint and fixture freeze

At checkpoint 95eea1edbde5592cb983f89f51ee147d09762785, tree
9b1d72531210772fe496c1c62b4502fc60569b37, Linux x64 reported thirty failures,
106,780 passes, and zero skips in
[run 44680693502](https://github.com/uniblab/Icod.Expressions/actions/runs/44680693502).
The new native comparisons for definitions 1–30 failed before their managed
comparisons. All 106,762 prior tests and eighteen new controls passed.

The new catalog blob is a40288ddbba580ef2f22e0d3b19dabafc5f45150.
The new assertion blob is aebc6e590198acc261e0503ce066e32e8ce50947.
The source-link project blob is ecd6adb444d0a32937e6a49c34cab3142d32068f.
These files and the seven adapter call sites freeze at that checkpoint.
The correction changes the shared mapper and C probe.
