# Internal PCRE counted repetition

**Status:** next internal syntax tranche under issue #7. Grep and CommandFramework production code remain untouched.

## Intent and scope

Accept the ordinary PCRE2 counted forms `{n}`, `{n,m}`, `{n,}`, and `{,m}` for already supported consuming atoms and groups, in byte and UTF-8 modes. Permit PCRE2's horizontal whitespace just inside the braces and around the comma. Values are decimal and at most 65535; a finite maximum must be at least the minimum. A following `?` selects lazy order; a possessive `+` stays explicitly unsupported. A quantifier on a nonrepeatable anchor is invalid and on a currently restricted fixed literal assertion stays unsupported. Unrecognized brace constructs remain explicit unsupported diagnostics, never silent literals.

Compile mandatory copies and bounded optional copies into the existing ordered bytecode, and reuse the progress guarded loop for an unbounded tail. This preserves capture rollback, source byte spans, invalid UTF barriers, cancellation, work limits, and empty-body termination. `{0}` compiles to no body; exact copies retain the final capture. The compiled instruction count and compile-node visits are each limited to 4096, and repeat marks to 256. This also bounds nested repetitions of bodies that emit no bytecode. Oversize expansion reports `PatternRepeatLimitExceeded` at the counted quantifier rather than allocating unbounded code. This provisional engine resource cap is stricter than PCRE2's semantic count limit and must be revisited at the Grep conformance gate.

The parser recognizes bounded numeric syntax only. It does not enable scoped options, possessive repetitions, general lookarounds, or other pending PCRE features. No public PCRE package claim is made.

## Approach

| Approach | Tradeoff |
|---|---|
| Add repetition counters to VM frames | Compact compiled programs, but changes backtrack frame snapshots and capture rollback for every match. Defer until measured pressure warrants it. |
| Expand mandatory/optional copies in bytecode | Reuses proven split ordering and capture snapshots; compile size needs a strict bound. Choose for this tranche. |

## Verification

Test exact/range/open bounds, zero count, lazy and greedy fallback, alternatives and captures, UTF scalar byte spans and invalid barriers, empty groups, diagnostics and offsets, large counts/expansion, cancellation and work exhaustion. Observe red CI for tests before implementation, then all six Staging jobs with net8/net9/net10, frozen 214-case GNU corpus and package smoke. PCRE2 10.48 reference conformance, especially broad patterns and performance, remains a separate migration gate.
