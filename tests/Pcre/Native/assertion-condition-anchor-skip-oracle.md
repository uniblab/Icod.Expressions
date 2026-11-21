# Assertion guards, else branches, source anchoring, and named SKIP

The independent preflight uses static native8 PCRE2 10.48 at
7978954dbd2efc6f2196869290553cf1871b4ce6 with SLJIT at
de0259c7aaf36aa40cba8014f3fad3edde9307f9 and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreAssertionConditionAnchorSkipCases.cs](../../../samples/PackageSmoke/PcreAssertionConditionAnchorSkipCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`.
Mode and policy bits are recorded in the
[original policy record](match-policy-oracle.md). Compiled anchor defaults
enter compilation; remaining defaults combine with match-time restrictions.

All 3,456 rows completed, with 1,040 matches and 2,416 misses.
Every JIT compilation returned zero and positive code size. No outcomes were
discarded and there were no compile/match errors. Raw LF output SHA-256:
`4072fb967abd97312f70fc416138b0048a6e109a8d5e96fbd081cefb8a2e1cb3`.

Call-time ANCHORED or ENDANCHORED selects interpreted matching. Native
is_anchored in pcre2_compile.c requires an explicit else alternative for
OP_COND/OP_SCOND. Its first significant opcode for the first alternative is
the assertion guard. A positive forward guard can establish anchoring; native
then checks the else alternative. The then body is outside this inference.
Negative/lookbehind guards and non-anchor guard alternatives defeat the proof.
The anchored driver stops before unmatched-name replay. Compiled NotBol retains
lexical caret anchoring.

The managed parser retains a general assertion node for conditional guards.
The compiler charges the guard and both bodies within its shared budgets.
Each theory completes native8 comparisons before the managed consumer runs.
Preserve literal protocols and assertions at the regression checkpoint.
Final source identities and qualification are recorded on PR #306.
