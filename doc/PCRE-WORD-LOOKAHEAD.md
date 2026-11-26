# Native 16-bit atomic lookahead

The bounded word compiler admits positive `(?=...)` and negative
`(?!...)` lookahead, including the shared parser's `(*pla:...)` /
`(*positive_lookahead:...)` and `(*nla:...)` /
`(*negative_lookahead:...)` aliases. Bodies may contain admitted
alternatives, groups, repetition, captures, backreferences and nested
atomic lookahead. Groups containing assertions may themselves repeat.

## Execution and capture lifetime

A lookahead pushes a guarded return into the existing iterative executor.
Its body starts at the current original code-unit position. A completed
positive body restores that position, retains its captures and discards
only the body's internal retry frames. Later suffix failure cannot reopen
those choices. An earlier enclosing choice can still undo those captures
through the existing journal. Captures may extend beyond group zero,
including a nonempty capture inside a zero-length outer match.

A negative body that matches fails the assertion after restoring its
capture checkpoint. Exhausting the body's choices succeeds without consuming
input and without publishing attempted captures. Nested assertions use
the same frame stack and immutable continuations. Resource failures and
cancellation propagate out of either polarity.

Finite choices on either side of an assertion remain separate ordered
continuations. Root match policies apply to the outer completion; explicit
anchors and boundaries inside a body still observe call-time options and
the original search start.

Word parsing preserves original scalar/unit nodes inside literal assertions.
The byte-only literal assertion optimization remains unchanged. No subject
copy or UTF transcoding is introduced. Strict validation still starts at
the requested offset, with the existing previous-scalar window when any
retained body contains a word boundary or absolute-start anchor.

## Resources and remaining scope

Assertion entry, return, exhausted-body resolution, each discarded retry
frame and each restored capture charge the shared work quota and check
cancellation. Guards share the 1,024-frame ceiling. Assertion bodies
participate in the existing 64-level nesting, 64-static-path, 64-unbounded-
repeat and 4,096-weighted-node admission limits. These limits count retained
bodies once, independently of runtime iteration.

Directly quantified assertions and non-atomic assertions remain unsupported.
Lookbehind, calls, conditional execution, atomic groups, possessive forms
and verbs remain outside word execution. Broader newline search, graphemes,
word package consumers, cross-width qualification and public API/resource
closure are still open.

## Evidence and next work

111 fixtures across eight compiled defaults and 64 call masks add 56,832
PCRE2-16 comparisons of status and every capture slot. They cover atomic
choice commitment, capture rollback, nested polarity, repeated groups,
references, aliases, Unicode casing/classes, supplementary scalars, raw
surrogates, newline/BSR behavior and original start coordinates.

Standalone pinned native preflight produced 38,436 matches and 18,396
misses, with no compile/match errors. Its LF-terminated output SHA-256 is
`426e37aa7a7469c0057641563649d20cb89f19c45899e057bce03fa5ddea7988`.
Build/provenance is retained in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Additional tests compare every cross-width capture span and value, check
exact work quotas, cancel during pruning and rollback, exhaust guards and
nested bodies, exercise admission limits, and verify source ownership and
durable results. Full managed six-host and package qualification is recorded
on the pull request.

Next work extends lookarounds with lookbehind and the remaining assertion
forms, then calls and verbs.
