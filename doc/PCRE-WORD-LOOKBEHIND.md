# Native 16-bit atomic lookbehind

The bounded word compiler admits positive `(?<=...)` and negative
`(?<!...)` lookbehind, including `(*plb:...)` /
`(*positive_lookbehind:...)` and `(*nlb:...)` /
`(*negative_lookbehind:...)` aliases. The shared parser supplies
per-branch maximum widths and the native common minimum for variable bodies.
Both fixed and bounded variable-length assertions reuse the assertion
guards introduced for lookahead.

## Original-width execution

Each original assertion branch begins with its own rewind node. Raw mode
rewinds original 16-bit units; strict UTF-16 mode rewinds whole scalars,
preserving code-unit offsets for every capture and result. Different branch
lengths restart from the assertion's original position.

A variable branch first tries its longest available rewind, provided that
the common minimum is satisfied. Body alternatives and counts retry before
a shorter rewind is tried. A separate retry frame advances the rewind start
by one complete character, restores the capture checkpoint and reruns the
body. Rewind boundaries keep finite body choices inside this ordering.

A completed body must end at the assertion's original position. Positive
success retains captures and discards internal alternatives and rewind
choices. A later suffix failure cannot reopen a committed assertion.
Negative assertions roll back attempted captures on either body outcome.
Enclosing alternatives can still undo successful positive captures.
Captures may precede group zero or extend beyond it through nested lookahead.

The execution path stays iterative and reads the caller's original subject
view. No subject copy, transcoding or recursive assertion runner is added.
The existing byte parser's width analysis and byte execution remain unchanged.

## Validation and resources

Strict subject validation extends left from the requested start by the
largest retained lookbehind width, or at least one character when a retained
word boundary or absolute-start assertion requires it. Rewinding that window
charges every inspected code unit; forward validation retains its existing
scalar charges. Bodies inside zero-count groups still contribute metadata.
Invalid surrogates outside the native validation window remain outside the
validation pass. Malformed units inside it report their original offset.

Rewind admission, every attempted reverse scalar/unit, every shorter-start
retry, assertion return/end checks, capture restoration and atomic pruning
share the cancellation-aware work budget. Rewind and assertion frames share
the 1,024-frame ceiling. Resource failures never become successful negative
assertions and never publish partial results.

The shared parser retains the default 255-character variable-lookbehind
ceiling and 65,535-character fixed-width ceiling. Existing word admission
limits still apply: 64 static paths, 64 source nesting levels, finite repeat
counts at most 64, 64 retained unbounded repeats and 4,096 weighted nodes.
A rewind adds one retained node per original assertion branch. Native width
restrictions on references and unbounded consumed bodies remain in force.

## Evidence and remaining scope

133 fixtures across eight compiled defaults and 64 call masks add 68,096
PCRE2-16 comparisons of status and every capture slot. They cover distinct
branch lengths, greedy/lazy finite bodies, longest-first rewind ordering,
atomic commitment, nested polarities, references, captures before group
zero, Unicode casing/classes, supplementary scalars, raw surrogates, newline
sequences, aliases, nonzero starts and the variable-width ceiling.

Standalone pinned native preflight produced 29,676 matches and 38,420
misses, with no compile/match errors. Its LF-terminated output SHA-256 is
`b23457a93585c20a74d0bece4b54a0ce5255ec5905a940add9de760f36651bf8`.
Build/provenance is retained in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Additional tests compare cross-width capture spans and values, exact work
quotas, cancellation during long rewinds, extended validation and shorter
rewind retries, capture/result reuse, shared frame exhaustion, admission
bounds and native malformed-prefix validation windows. Full six-host managed
and package qualification is recorded on the pull request.

Non-atomic and directly quantified assertions remain unsupported. Calls,
conditions, atomic/possessive groups and verbs follow the remaining assertion
work. Broader newline search, graphemes, word package consumers, cross-width
qualification and public API/resource closure remain open.
