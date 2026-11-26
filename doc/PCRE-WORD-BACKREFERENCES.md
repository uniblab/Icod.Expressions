# Native 16-bit backreferences

The bounded word compiler now admits numbered, relative and named
backreferences through the shared parser. This includes decimal references,
`\g{number}`, relative `\g{-number}`/`\g{+number}`, named `\k<name>`,
`\k'name'`, `\k{name}`, `\g{name}` and `(?P=name)` spellings. Numeric
versus octal interpretation remains the shared parser's responsibility.
Subroutine spellings such as `\g<name>` remain outside word execution.

## Matching and repeats

A reference reads the completed capture span from the current match's
capture state and compares directly against the original subject view.
Failed comparisons do not advance the matching position. References can
observe a group's earlier completed iteration while its next iteration is
open. Existing capture-journal rollback restores the correct spans when
alternatives, counts or match-policy restrictions force a retry.

Unset captures fail under the currently admitted default policy. Participating
empty captures succeed without consuming input. Duplicate names select the
first participating slot in native declaration order, including an empty
slot. A comparison failure cannot fall through to a later duplicate.

Raw matching compares original 16-bit units, preserving isolated surrogates.
Strict UTF-16 matching compares whole scalars while retaining code-unit
coordinates. Reference-local case-insensitive mode uses pinned simple-case
equivalence under UTF or UCP and ASCII folding otherwise. Restricted casing
continues to exclude the native ASCII/non-ASCII cross-boundary folds.

Greedy/lazy `?`, `*`, `+`, exact, finite and open-ended counts are admitted
on references. A repeated reference uses an internal capture-free group body
and the existing iteration/return frames. This preserves whole captured
strings on retries, avoids scalar-atom rewinds, and stops an unbounded loop
when its reference consumes nothing. The internal body adds no capture slot.

## Resources and scope

Each reference admission charges a work step. Every duplicate-name slot
inspected and every attempted captured scalar/raw-unit comparison charges
another step, with cancellation checked before and after the observer.
Long captured strings cannot bypass the match work quota. Failed or
cancelled calls publish no partial capture result.

Repeated references retain the existing finite maximum/minimum of 64,
64 retained unbounded-repeat bound, 64 static-path bound, 4,096 weighted-node
budget and 1,024 retry-frame ceiling. Their bodies count once at compilation;
runtime count decisions share the existing work/frame budgets. Pattern,
capture and source-group depth limits remain unchanged.

No subject copy or transcoding is introduced. Match results retain their
owned spans and compiled name metadata. The optional unset-matches-empty
compile policy is not exposed by the word entry point. Lookarounds,
subroutine calls, atomic/possessive forms and verbs remain unsupported.

## Evidence and next work

107 fixtures across eight compiled defaults and 64 call masks add 54,784
PCRE2-16 comparisons of status and every capture slot. Cases include
reference-driven branch retry, previous-iteration references, unset/empty
captures, duplicate-name selection, branch reset, Unicode names, caseless
restrictions, supplementary scalars, raw surrogate units and CRLF strings.
Standalone pinned native preflight produced 38,168 matches and 16,616
misses, with no compile/match errors. Its LF-terminated output SHA-256 is
`9616fd3c4142f8d20aa1cf0ac41baaef4a8d828681ee04d19a3e80f65a49e89f`.
Build/provenance is retained in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Additional tests compare all cross-width capture spans and values, check
exact lookup/comparison quotas, cancellation inside a long reference,
frame exhaustion, admission limits, original diagnostic offsets, source
ownership and durable results across later calls. Full six-host managed
and package qualification is recorded on the pull request.

Next execution work is lookarounds, followed by calls and verbs. Broader
newline search, graphemes, word package consumers, cross-width qualification
and public API/resource closure remain open.
