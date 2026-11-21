# Positive lookaheads, source anchoring, and named SKIP

The independent preflight uses static native8 PCRE2 10.48 at
7978954dbd2efc6f2196869290553cf1871b4ce6 with SLJIT at
de0259c7aaf36aa40cba8014f3fad3edde9307f9 and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreLookaheadAnchorSkipCases.cs](../../../samples/PackageSmoke/PcreLookaheadAnchorSkipCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`.
Mode and policy bits are recorded in the
[original policy record](match-policy-oracle.md). Compiled anchor defaults
enter compilation; remaining defaults combine with match-time restrictions.

All 2,304 rows completed, with 656 matches and 1,648 misses.
Every JIT compilation returned zero and positive code size. No outcomes were
discarded and there were no compile/match errors. Raw LF output SHA-256:
`a668e54b2153b28036b72913c4ee0de223fd1d7a5d2dff9b71d36e0ab7c5b0f2`.

Call-time ANCHORED or ENDANCHORED selects interpreted matching. Native
is_anchored in pcre2_compile.c descends both OP_ASSERT and OP_ASSERT_NA and
checks every alternative. first_significant_code with skipassert false retains
negative assertions and lookbehinds, which do not establish this evidence.
The anchored driver stops before unmatched-name replay. Compiled NotBol keeps
lexical caret anchoring. The controls distinguish these source profiles.

Each theory completes native8 comparisons before the managed consumer runs.
Preserve literal protocols and assertions at the regression checkpoint.
Final source identities and qualification are recorded on PR #303.
