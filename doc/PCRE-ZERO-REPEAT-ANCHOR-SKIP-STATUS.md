# Exact-zero repeat prefixes and unmatched named SKIP

With subject `ac`, pattern `(?:(?:z){0}\Aa(*SKIP:missing)b|\Aac)`, and
per-call `EndAnchored`, native interpreted matching reports no match. The
managed compiler previously stopped its source-anchor check at the `{0}`
node, so the unmatched named SKIP allowed the fallback branch to match.

The compiler checks each branch for an anchor after consecutive exact-zero
repeats, including possessive forms such as `{0}+`. This corresponds to
native bytecode erasing exact-zero atoms or skipping the retained group body.
Each inspected node counts toward the existing compiler visit ceiling and
checks cancellation. An exhausted branch fails the proof. The first applicable
node follows the existing rules for mandatory repeats, groups, assertions,
conditions, and source anchors. The resulting flag feeds the interpreter's
match-time anchoring check for unresolved named SKIP.

The [native oracle record](../tests/Pcre/Native/zero-repeat-anchor-skip-oracle.md)
describes 48 definitions and 3,072 outcomes. The corpus exercises repeated
zero-count prefixes, nested branch and guard checks, captures retained for
subroutine calls, MARK labels, and byte offsets. Controls include optional
and star prefixes, mixed and exhausted alternatives, an anchor inside an
omitted group, and multiline caret. Two resource tests exercise exact work
exhaustion and cancellation. A twenty-fifth package smoke record exercises
the same 3,072 outcomes through the isolated package consumer.

## Tests-only checkpoint

Commit `f37d4caf9cc5611ecadc70b077c16b77b150044b` and tree `39ae0bfc3535b798c0009b48b2940211683ef03f`
produced 41 expected failures, 107,276 passes, and zero skips on
[Linux x64](https://github.com/uniblab/Icod.Expressions/actions/runs/44958353570/job/137011385068),
[Linux ARM64](https://github.com/uniblab/Icod.Expressions/actions/runs/44958353570/job/137011385076), and
[Windows x64](https://github.com/uniblab/Icod.Expressions/actions/runs/44958353570/job/137011385072).
All native comparisons completed before the managed failures in definitions
1–38 and 48. Both resource tests encountered a match at the no-match assertion.
All 107,267 existing tests and nine new controls passed.

- `samples/PackageSmoke/PcreZeroRepeatAnchorSkipCases.cs` at `2081408ac6078dee06febcded12d73acc26b4f67`
- `tests/Icod.Expressions.Tests.csproj` at `6e19110abefb6c196d3fee20ee95fcbfb87685dd`
- `tests/Pcre/PcreZeroRepeatAnchorSkipTests.cs` at `7c25c3aab85a5c60d7b00d9e321a88f2194adae3`

Final qualification is recorded on [PR #308](https://github.com/uniblab/Icod.Expressions/pull/308).
