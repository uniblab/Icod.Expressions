# Native 16-bit non-atomic assertions

The word compiler admits non-atomic positive lookahead `(?*...)`
and lookbehind `(?<*...)`, including their `(*napla:...)`,
`(*non_atomic_positive_lookahead:...)`, `(*naplb:...)`
and `(*non_atomic_positive_lookbehind:...)` aliases. Lookbehind
retains the shared parser's fixed and bounded-variable width restrictions.

## Retained choices and captures

A successful non-atomic assertion restores the caller's position while
keeping its body choices and capture checkpoint on the existing retry stack.
If the suffix fails, those frames restore the body's capture state and
resume an alternative, repeat count or shorter lookbehind start. Every body
completion returns to the same outer continuation. Exhausting all choices
fails the positive assertion and restores its entry state.

For example, `(?*(a|ab))\1c` can match `abc` by retrying
the capture as `ab`. The atomic spelling commits to `a`
and cannot reopen that choice after the suffix fails. An enclosing atomic
assertion still discards retained inner choices when it commits. Negative
assertions continue to roll back all attempted captures.

Nested assertions, repeated enclosing groups, references, duplicate names,
branch-reset captures and root match policies reuse the existing iterative
executor and capture journal. Results retain owned code-unit spans and
compiled name metadata without retaining subject memory.

## Variable lookbehind visibility

The pinned native JIT bounds reads inside a variable lookbehind at its
original endpoint, including nested assertions. Call-time anchoring selects
the interpreter, which retains the enclosing end instead. This difference
also applies to atomic variable lookbehind.

The word runner now records the visible end in retry frames and saves the
enclosing end in assertion continuations. Body entry, retry, completion and
failure restore the appropriate limit. Literals, classes, repeats, anchors,
word boundaries and reference comparisons honor that limit. A reference's
captured source span still reads the original subject, even when its capture
lies beyond the current visible end.

This uses bounded views of the caller's original memory, with no copy or
transcoding. Existing left validation windows and call-time interpreted
rewind boundaries remain unchanged.

## Resources and remaining scope

Body retry, capture undo, comparison and assertion return retain their
existing work charges and cancellation checks. Successful non-atomic
assertions keep their guards, so repeated enclosing groups can reach the
shared 1,024-frame ceiling sooner than atomic forms. This is an explicit
resource bound, not a new syntax restriction. Failed or cancelled calls
publish no partial capture result.

The existing 64-level nesting, 64-static-path, 64-retained-unbounded-repeat,
4,096-weighted-node and finite-count limits remain. Shared lookbehind width
limits remain 255 for variable bodies and 65,535 for fixed bodies, subject
to the word compiler's independent admission limits.

Directly quantified assertions remain unsupported. Calls, conditions,
atomic/possessive groups and verbs follow that work. Broader newline search,
graphemes, word package consumers, cross-width qualification and public
API/resource closure remain open.

## Evidence

132 fixtures across eight compiled defaults and 64 call masks add 67,584
PCRE2-16 comparisons of status and every capture slot. They cover nested
atomic/non-atomic boundaries, greedy/lazy body retries, fixed/variable
lookbehind, aliases, captures and references, visible-end restoration,
Unicode casing/classes, supplementary values, raw surrogate units and
call-time versus compiled anchoring.

Standalone pinned native preflight produced 40,500 matches and 27,084 misses
with no compile/match errors. Its LF-terminated output SHA-256 is
`253c1fe7d7382bffe41644a85c2822099e4363590d0e8e1216f1c6a2cf1fe1b3`.
Build/provenance is retained in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Additional tests compare cross-width capture spans and values, exact work
quotas, cancellation during re-entry, guard exhaustion, admission limits,
source ownership, durable results and original diagnostic/validation offsets.
Full six-host managed and package qualification is recorded on the pull request.
