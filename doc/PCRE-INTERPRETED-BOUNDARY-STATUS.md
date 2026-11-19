# Interpreted word boundary status

[PR #292](https://github.com/uniblab/Icod.Expressions/pull/292) adds 52 tests
for a total of 106,658. On the
[tests-only run](https://github.com/uniblab/Icod.Expressions/actions/runs/44597424647),
each .NET 10 runner reported 106,622 passes, 36 failures, and zero skips.
The failures were the four new work, cancellation, capture, and view facts
and native comparison cases 1–14, 17–22, 31–34, 39–42, and 45–48. All
106,606 prior tests passed.

See the [design](PCRE-INTERPRETED-BOUNDARY-DESIGN.md),
[plan](PCRE-INTERPRETED-BOUNDARY-PLAN.md), and
[native oracle record](../tests/Pcre/Native/interpreted-boundary-oracle.md).
