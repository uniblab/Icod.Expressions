# Native 16-bit direct assertion repeats

Raw 16-bit and strict UTF-16 sequences now admit direct quantifiers on
positive/negative atomic lookahead and lookbehind, and non-atomic positive
assertions. Short and long assertion aliases use the same path. Greedy,
lazy and possessive forms retain the shared parser's count and option rules.

## Finite assertion semantics

PCRE2 normalizes an unlimited direct assertion maximum to minimum plus one:
`*` becomes `{0,1}`, `+` becomes `{1,2}` and `{3,}` becomes `{3,4}`.
Every required finite iteration executes, although the subject position does
not advance. Captures may change between iterations: `^(?=(\1a|a)){3,}\1$`
matches four `a` units with a four-unit capture.

An ordinary group wrapping an assertion retains its existing unbounded
no-progress behavior. Direct quantifiers must not be lowered through that
loop. The word compiler uses the already-normalized finite bounds instead.
Greedy/lazy optional exits retain their usual capture checkpoints, and
non-atomic bodies can reopen their own choices when a later suffix fails.

Possessive direct repeats commit both optional count choices and retained
body alternatives. Because the entire repeated expression is zero-width,
the existing atomic positive assertion guard supplies the required commit
boundary. This does not admit general atomic groups or consuming possessive
repetition. For example, `(?*(a|ab)){1}\1c` can match `abc`, whereas the
possessive `{1}+` spelling commits the first capture and fails.

## Integration and resource bounds

Only the word compiler's traversal and lowering change. Repeated lookbehind
retains its original branch width metadata and validation contribution.
Possessive wrappers use synthetic compiled nodes; they do not create parser
captures or consume additional source nesting levels. Existing iterative
returns, capture undo, endpoint visibility, work accounting and cancellation
remain in the word matcher. The shared byte parser and executor are unchanged.

Normalized finite counts must remain within the word compiler's ceiling of
64. Thus `{63,}` is admitted and `{64,}` is not; source counts above 65,535
remain syntax errors. Bodies with zero executions still retain capture
numbering, validation windows and compile-time admission checks.

The 64-level source nesting, 64-static-path, 64-retained-unbounded-repeat,
4,096-weighted-node and 1,024-saved-frame ceilings continue to apply. A direct
assertion's normalized finite count consumes no unbounded repeat slot, but
unbounded loops inside its body still count. Repeated non-atomic assertions
retain guards; possessive completion discards those guards. Failure or
cancellation publishes no partial captures, and compiled patterns remain
reusable.

## Evidence and remaining scope

243 fixtures across eight compiled defaults and all 64 call masks add
124,416 PCRE2-16 comparisons of status and every capture slot. Cases cover
all assertion polarities, count spellings, aliases, greedy/lazy/U/possessive
behavior, capture growth and rollback, nested and repeated groups,
fixed/variable lookbehind, endpoint restoration, Unicode and raw surrogate
units, newline selectors, and call-time versus compiled anchoring.

Standalone pinned native preflight produced 83,116 matches and 41,300 misses
with no native errors. Its LF-terminated output SHA-256 is
`8b65f93a3a9e801e3045e5795e35b2a029d80d149056dfb644cd49cc4ff9e6f5`.
Build/provenance is retained in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Additional regressions pin explicit capture growth/commit outcomes,
cross-width spans and values, exact work limits, cancellation, the shared
frame ceiling, source ownership, durable results, admission limits and
original diagnostic/UTF validation offsets. Full six-host build, managed
test and package qualification is recorded on the pull request.

General atomic groups and consuming possessive repetition are next, followed
by calls, conditions and verbs. Broader newline search, graphemes,
word package consumers and public API/resource qualification remain open.
