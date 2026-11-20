# Named SKIP assertion propagation

The independent preflight uses static native8 PCRE2 10.48 at
7978954dbd2efc6f2196869290553cf1871b4ce6 with SLJIT at
de0259c7aaf36aa40cba8014f3fad3edde9307f9 and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreAssertionSkipCases.cs](../../../samples/PackageSmoke/PcreAssertionSkipCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`.
Mode and policy bits are recorded in the
[original policy record](match-policy-oracle.md). Anchor defaults enter
compilation. Remaining defaults combine with match-time restrictions.

All 2,048 rows completed, with 704 matches and 1,344 misses.
Every JIT compilation returned zero and positive code size. No rows were
discarded and there were no compile/match errors. Raw LF output SHA-256:
`f64891822a702024100dc976b0d701c2dbce3a9ee62bccb020ef6d4312132b7b`.

Call-time ANCHORED or ENDANCHORED selects native interpreted execution.
In pcre2_match.c, negative and conditional assertions catch MATCH_SKIP but
propagate MATCH_SKIP_ARG. A matching MARK converts the latter to MATCH_SKIP.
An originating recursion catches the verb before it can escape that call.
The corpus distinguishes interior and exterior marks, missing names, active
calls, nested assertions, successful forward paths, and unanchored replay.

Native8 comparisons precede managed comparisons in every theory case.
Preserve literal protocols and assertions at the failure checkpoint. Final
source identities and platform evidence are recorded on PR #299.
