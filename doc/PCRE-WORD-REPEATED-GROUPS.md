# Native 16-bit repeated noncapturing groups

The word executor admits `?`, `*`, `+`, exact counts, finite ranges and
open-ended counts on noncapturing groups, with greedy or lazy preference.
Repeated bodies can contain nested alternatives, assertions, atom repeats
and other repeated noncapturing groups. Scoped options remain resolved by
the shared parser. Captures, atomic/possessive forms, lookarounds and control
verbs remain explicitly unsupported by this entry point.

Each group body is compiled once. An iteration continuation records its
enclosing sequence, completed count, starting subject position and outer
return context. Greedy decisions retain an exit retry before entering the
next iteration; lazy decisions try the suffix first and retain a growth
retry. Backtracking inside an earlier body restores that iteration's count
and return context before rebuilding later iterations. All execution remains
iterative and reads the original word subject.

## Empty progress and resource limits

An optional unbounded iteration that consumes nothing exits the loop while
retaining the body's alternative retries. Required counted copies still run
even when empty, and finite repeats can finish their admitted counts. The
shorthand `+` also checks its first mandatory copy for progress, matching the
existing byte emitter's distinction from a counted `{1,}` spelling. Root
empty-match or end-anchored rejection can retry a consuming body alternative.

The word matcher now has a 1,024-backtrack-frame ceiling, matching the byte
matcher's fixed ceiling. Exhaustion returns `MatchDepthLimitExceeded`;
`MatchWorkLimitExceeded` remains independent. All retry-frame pushes use the
same check. There is no new user-specified resource option. Return chains
remain bounded by the admitted group nesting, and frames/continuations are
local to each match.

Repeated-group admission, iteration return/progress decisions and exit/growth
retries each charge a work step. Ordinary group entry/return and body
comparisons retain their existing charges. Cancellation is checked at these
steps. Exact work tests distinguish empty `*`, `+`, counted and finite forms;
an ambiguous repeated alternative must stop at its work quota.

## Compilation bounds

Finite maxima and required minima remain at most 64, and group nesting is at
most 64. The static 64-path choice bound and 64-unbounded-node bound include
each retained repeated body once; subject-driven iterations do not multiply
these compile-time bounds. Runtime retry growth is governed by the work and
frame ceilings. Existing atom finite-choice expansion keeps its prior bounds.

The 4,096-node budget includes each retained body once plus its group and
repeat entries. Group counts are executed at runtime rather than expanded,
so `(?:a|b){0,64}` is admitted without duplicating the body. Pattern length
and the shared parser's repeat-state ceiling still apply. Zero-count bodies
are validated and retain native assertion lookbehind metadata, even though
the matcher does not enter them.

## Evidence and remaining work

Eighty fixtures across eight compiled defaults and 64 call masks add 40,960
PCRE2-16 comparisons. Native preflight returned 37,232 matches and 3,728
misses with no errors. Its LF-terminated output SHA-256 is
`32d4287b8ed1305f43d98956088e65ef9510f838dc820ea3d17b8a2aa577bb9c`.
The oracle revisions are pinned in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).
Additional tests cover managed cross-width spans, exact work boundaries,
frame exhaustion, cancellation, source ownership, state reset and strict
UTF-16 validation around assertions in zero-count bodies.

The initial standalone probe of 64 nested greedy stars over `aab` exhausted
the native default JIT stack in 128 policy combinations (`-46`). The parity
fixture at that depth uses `b` to test nested empty progress within the
native resource limit; deeper resource qualification remains separate from
successful match parity.

Capture rollback and durable capture results are the next execution work.
Broader newline search, graphemes, cross-width qualification, independently
configurable resource budgets and the public PCRE contract remain open.
