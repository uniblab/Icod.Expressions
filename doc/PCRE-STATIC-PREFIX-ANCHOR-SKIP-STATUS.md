# Skipped static condition prefixes and unmatched named SKIP

With subject `ac`, pattern `(?(DEFINE)(?<x>z))\A(?:a(*SKIP:missing)b|ac)`,
and per-call `EndAnchored`, native interpreted matching reports no match.
The managed source-anchor check previously stopped at the declaration,
and the unmatched named SKIP allowed the fallback branch to match.

The compiler now advances past a false static conditional with exactly
one branch. This covers `DEFINE`, false version checks, and repeats whose
child has that shape. The parser represents ignored possessive quantifiers
on these groups as bare repeats. Declaration compilation resolves captures
and subroutine targets, and repeat compilation charges the original repeat.

The existing branch scan charges each inspected source node and checks
cancellation. It requires a start anchor for every leading path. The proof
feeds the interpreted unmatched-name check during match-time anchoring.

The [native oracle record](../tests/Pcre/Native/static-prefix-anchor-skip-oracle.md)
describes 48 definitions and 3,072 outcomes. It covers callable declarations,
mixed prefixes, assertion guards, compiled match defaults, labels, and
original byte offsets. Controls exercise true static conditions, explicit
else branches, runtime tests, and incomplete anchor paths. Two focused
resource tests cover exact work limits and cancellation. A twenty-sixth
package smoke record verifies the same outcomes through the packed library.

## Tests-only checkpoint

Commit `ed4be91cf5e48a64eca148eced6b0abf5f5a3f9f` and tree `7b1671201655adf1ddee7b730a533162c6cdf7d4`
produced 42 failures, 107,325 passes, and zero skips on
[Linux x64](https://github.com/uniblab/Icod.Expressions/actions/runs/44961390349/job/137020745034).
Definitions 1–40 passed all native assertions before the managed comparison
failed. Both resource tests reproduced the unexpected fallback match.
All 107,317 existing tests and eight new controls passed.

- `samples/PackageSmoke/PcreStaticPrefixAnchorSkipCases.cs` at `c8352f1a2936126bc5b731ac243d8610c0687e1a`
- `tests/Icod.Expressions.Tests.csproj` at `26d1e95f112f5692f2938a5a522e479e48677dd5`
- `tests/Pcre/PcreStaticPrefixAnchorSkipTests.cs` at `c19a009de54cafbe28c6644283c9167be79b7947`

Final qualification is recorded on [PR #309](https://github.com/uniblab/Icod.Expressions/pull/309).
