# Native 16-bit nested noncapturing alternatives

The word-sequence compiler admits nested noncapturing groups from the shared
PCRE grammar, including ordered alternatives, empty branches and scoped
options. Supported atoms, assertions and atom repeats can appear inside or
outside a group. The group itself cannot yet be quantified. Capturing,
atomic, lookaround, conditional and control-verb nodes remain outside this
bounded entry point.

Compilation visits group bodies in iterative postorder. Each compiled body
retains its own ordered branches. Groups separate finite-choice segments
just as unbounded repeats do: a suffix choice retries before the enclosing
group's next alternative, while a preceding choice retries after the group.
For example, `(?:a|ab)c?` first matches only `a` in `abc`; an end-anchored
policy rejects that completion and allows the later `ab` branch plus `c`.
Expanding the optional `c` into root alternatives would change this order.

The existing execution loop now keeps immutable return continuations for
group entry. Every retry frame records its return context. A failed suffix
can therefore resume an earlier group's repeat or next branch, then return
to the enclosing suffix again. Greedy repeats at the end of a group retain
rewind frames when an enclosing continuation exists. Supplementary scalars
and `\R` CRLF pairs still rewind as whole atoms. Execution and compilation
do not recurse through nested groups, and compiled patterns retain no
per-match state.

## Bounds and work

Group nesting is capped at 64. Group alternatives and finite atom counts
share the existing 64-path admission limit: alternatives add paths and
sequential choices multiply them, including choices across group boundaries.
At most 64 unbounded atoms can lie on one complete path, counting enclosing,
inner and sibling groups. The 4,096-node expanded-path budget includes group
entries and the largest admitted body path. Existing pattern length, exact
count/minimum and parser repeat-state limits also remain in force.

Each group entry and completed branch return costs one work step. A group
with multiple compiled branches charges another step for each branch
admission. Existing comparison, assertion, repeat, finite-choice and root
completion charges are unchanged. Cancellation is checked at these steps;
group entry/return is not invisible bookkeeping. A raw single-literal path
wrapped in `d` single-branch groups takes exactly `2*d + 3` steps.

Nested word boundaries and absolute-start assertions contribute to the
whole pattern's native one-character validation lookbehind. Unrelated
malformed prefixes still remain outside the strict UTF-16 validation window.
Subject views and all result/diagnostic coordinates remain native code units.

## Evidence and remaining work

Sixty fixtures across eight compiled defaults and 64 call masks compare
30,720 outcomes with pinned PCRE2-16. They cover alternative/finite-choice
ordering, retries through group returns, scoped modes, empty completions,
supplementary and raw-surrogate units, line/word assertions, newline pairs,
and depths 8, 32 and 64. Native preflight returned 27,588 matches and 3,132
misses with no errors. Its LF-terminated output SHA-256 is
`1e947e73cd5492a0839aa0f3251eee994d628a488b95920690fc8e19eea1ddb3`.
The oracle revisions are pinned in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Additional tests compare managed widths, check combined admission limits,
exact quota exhaustion and cancellation during group return, and exercise
state reset, source ownership and strict validation around nested assertions.
Repeated groups and their empty-progress rules are the next execution work,
followed by capture rollback and durable capture results. Broader newline
search, graphemes, cross-width qualification and the public PCRE API remain
open.
