# PCRE error and resource contract checkpoint

**Status:** issue #7 internal fixture gate. These limits and diagnostics are
provisional; this document does not approve a public PCRE API or Grep switch.

## Grep-facing boundary

The pinned `Icod.Grep` source at `2ef1c11f11b77eb876242eb0c92a88d20857db64`
uses PCRE.NET 1.6.0 / PCRE2 10.48 with `Compiled` (JIT requested), and in a
UTF-8 locale also requests UTF, UCP, `MatchInvalidUtf`, and ASCII `\d`.
`PcreTests.InvalidPatternReturnsUsageError` asserts status 2 and nonempty
stderr for `-P '('`. Grep catches `PcreException` while compiling patterns;
its command-level `OperationCanceledException` handler returns canceled
status 130. `PcrePattern.Find` checks cancellation immediately before and
after the native match, rather than providing an interrupt inside it. Match
resource failures have no separate command fixture yet.

The future Expressions adapter must map compile and match diagnostics to
Grep's error path (status 2 with stderr), keep successful no-match as status 1
when no record is selected, and let cancellation reach the canceled path.
The wording of native PCRE exceptions is not frozen as an API contract.
No adapter is implemented in this tranche; these are migration requirements.

## Internal outcome matrix

| Condition | Internal outcome | Evidence and consumer relevance |
|---|---|---|
| Malformed or pending pattern | `InvalidSyntax` or `UnsupportedSyntax` with source-pattern byte offset; no compiled pattern | `PcreByteFoundationTests`, `PcreSyntaxAcceptanceTests`; both must become Grep errors when the adapter exists. |
| Bad UTF-8 pattern | `InvalidUtf8Pattern` at offending byte | `PcreUtf8InvalidSubjectTests`; Grep's current string-to-pattern encoding does not directly expose arbitrary raw pattern bytes. |
| Nonpositive work limit; pattern nesting/capture/repeat ceilings | `InvalidWorkLimit`, `PatternDepthLimitExceeded`, `PatternCaptureLimitExceeded`, `PatternRepeatLimitExceeded` | Foundation, counted-repeat and `PcreAssertionRepeatReferenceTests`; finite direct assertion repeats need no progress marks but remain bounded by compiled instructions/node visits, including normalized `{65535,}`. Provisional internal compile policy, not native limit parity. |
| Start outside input, or inside a scalar in strict UTF mode | `InvalidStartOffset` | Foundation and UTF tests; current Grep filters out-of-range offsets before calling PCRE.NET. |
| Invalid subject in strict UTF mode | `InvalidUtf8Subject` with offending byte offset | UTF tests; Grep requests tolerant `MatchInvalidUtf`, so strict-mode diagnostics are an internal API condition. |
| Malformed subject in Grep-like tolerant UTF mode | Match or successful no-match on valid fragments | Invalid UTF offset/capture reference suites; invalid bytes are not an error. |
| Ordinary miss | Successful result with null match | Foundation tests; must not be treated as a resource failure. |
| Exhausted match steps | `MatchWorkLimitExceeded`, no match | Foundation and optimization tests; 1,000,000 default steps is provisional. |
| More than 1,024 live saved paths or assertion guards | `MatchDepthLimitExceeded`, no match | `PcreResourceContractTests` pins 1,024/1,025 lazy optional paths on empty input. General lookahead guards share this ceiling, pinned at 1,023 choices plus one guard and nested-guard variants in `PcreGeneralLookaheadReferenceTests`. This is an explicit-frame ceiling, not PCRE2 recursion depth. |
| Cancellation before/during compilation or matching | `OperationCanceledException` | Foundation and resource tests; distinct from all diagnostics. |

PCRE2 exposes match, heap and depth limits with different accounting; its
[API documentation](https://www.pcre.org/current/doc/html/pcre2api.html) states
that the depth limit is ignored by JIT matching. Do not equate the internal
saved-path count with PCRE2's depth limit or compare numeric thresholds across
engines. Test the selected Grep workloads and error behavior separately.

## Remaining release work

The bounded internal taxonomy is testable now. Before migration, add
command-level Grep fixtures for a managed match-work and saved-path failure,
assert status 2 and useful stderr, and test cancellation on the new adapter.
Measure whether the default budgets admit Grep's seven documented workloads,
large pattern sets and long records without disproportionate memory use.
Choose thresholds from those measurements. Synthetic process-wide memory caps
are not a portable parity oracle, as Grep's T6.8 resource report explains.
Grep and CommandFramework production code remain unchanged here.
