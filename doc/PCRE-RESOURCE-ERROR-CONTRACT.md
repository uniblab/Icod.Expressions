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
| More than 1,024 live saved paths or assertion guards | `MatchDepthLimitExceeded`, no match | `PcreResourceContractTests` pins 1,024/1,025 lazy optional paths on empty input. General lookahead guards share this ceiling, pinned at 1,023 choices plus one guard and nested-guard variants in `PcreGeneralLookaheadReferenceTests`. General fixed-length lookbehind uses the same guards and ceiling (`PcreFixedLookbehindReferenceTests`). This is an explicit-frame ceiling, not PCRE2 recursion depth. |
| Cancellation before/during compilation or matching | `OperationCanceledException` | Foundation and resource tests; distinct from all diagnostics. |

PCRE2 exposes match, heap and depth limits with different accounting; its
[API documentation](https://www.pcre.org/current/doc/html/pcre2api.html) states
that the depth limit is ignored by JIT matching. Do not equate the internal
saved-path count with PCRE2's depth limit or compare numeric thresholds across
engines. Test the selected Grep workloads and error behavior separately.

General fixed-length lookbehind validates character widths after complete parsing and name resolution with
heap traversal and bounded sums/products (65,535 per new general branch). Byte
and UTF scalar rewind spends match work and observes cancellation within the
current valid fragment. Bounded variable lengths use min/max analysis and cap every branch at255 if
any branch varies. Reverse retry frames share saved-path/journal ceilings, use
a byte-sized remaining count and spend work. Scoped end limits restore through
stable saved assertion-scope IDs. Consumed references resolve bounded capture-body widths through a global heap
map and consumed-only dependency traversal; lexical self/cycles, unbounded bodies
and global branch reset remain controlled unsupported syntax. Erased references
still validate their target. Name/tree reconstruction and width dependency
traversal use explicit heap frames with cancellation. Native's independent 2,000
branch-analysis accounting is not a managed numeric-limit parity claim.

Empty/reset option headers add no runtime state or work units. Bare settings
consume neither a capture slot nor a nesting frame; scoped empty/reset groups
use the existing nesting ceiling. Reset n restores ordinary capture allocation;
reset preserves U, J and ASCII restrictions. Scoped/mixed ASCII restrictions use the same reader
and a five-bit snapshot; immutable class predicates and existing b/B metadata
need no VM state. `PcreAsciiRestrictionReferenceTests` covers mask algebra,
policy scopes and unchanged depth/capture/work/cancellation limits.
`PcreAsciiDigitOptionReferenceTests` and `PcreResetOptionReferenceTests` pin
these boundaries and unchanged match-work/cancellation outcomes.

Local r uses immutable node/instruction comparison metadata, with no option
stack or saved-frame changes. Restricted range keys remain in the existing
scalar-domain bitmap and retain cancellation polling. UCP POSIX Ll/Lu stay
category-specific independently of explicit property case widening.
`PcreCaselessRestrictionReferenceTests` covers scope, captures, optimized/general
assertions, range closure and unchanged depth/capture/work/cancellation limits.

Duplicate named references share immutable declaration-ordered slot lists, each
bounded by capture256. Native ordered registration may let an earlier matching
name preempt a later number conflict; distinct names remain bounded by256, with
aggregate name/slot pairs at most65,536 and candidate integer storage at most256KiB.
Every participation lookup spends match work and polls cancellation.
Selection commits before the existing comparison and allocates no matching state.
Unique names retain the numbered fast path. Actual duplicate consumed names reject
lookbehind width analysis; J-only, numeric and lookahead-only controls remain valid.
`PcreDuplicateNameReferenceTests` pins source-order/empty/mismatch selection, scope,
registration errors and compile/match budgets.

Atomic-group and atomic-lookaround aliases reuse the same heap GroupFrame and
semantic lowering: one nesting level, no capture slot and no added matching work.
Exact byte headers preserve original source offsets and assertion context; all
width, journal, saved-path and work/cancellation gates remain shared.
`PcreGroupAliasReferenceTests` covers the nine prefixes and distinct pending forms.

Non-atomic positive assertions retain body retries and guards under the same1024
saved-frame ceiling. A scope arena stores five integers per live guard (at most
20KiB of integer payload); every guard pop/prune releases its last arena node.
Frames store integer current/parent IDs, never spans or capture snapshots. On
success, scope limits hide behind the parent ID; retry restores the original
context while the journal rolls back captures and marks. All atomic/negative
pruning shares this scope lifetime path. The arena resets with candidate state.
`PcreNonAtomicAssertionReferenceTests` pins retries, scope reuse, width/cap ordering
and unchanged compile, saved-path, match-work and cancellation boundaries.

## Remaining release work

The bounded internal taxonomy is testable now. Before migration, add
command-level Grep fixtures for a managed match-work and saved-path failure,
assert status 2 and useful stderr, and test cancellation on the new adapter.
Measure whether the default budgets admit Grep's seven documented workloads,
large pattern sets and long records without disproportionate memory use.
Choose thresholds from those measurements. Synthetic process-wide memory caps
are not a portable parity oracle, as Grep's T6.8 resource report explains.
Grep and CommandFramework production code remain unchanged here.
