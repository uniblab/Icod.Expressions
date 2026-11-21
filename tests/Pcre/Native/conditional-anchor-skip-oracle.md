# Conditional branches, source anchoring, and named SKIP

The independent preflight uses static native8 PCRE2 10.48 at
7978954dbd2efc6f2196869290553cf1871b4ce6 with SLJIT at
de0259c7aaf36aa40cba8014f3fad3edde9307f9 and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreConditionalAnchorSkipCases.cs](../../../samples/PackageSmoke/PcreConditionalAnchorSkipCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`.
Mode and policy bits are recorded in the
[original policy record](match-policy-oracle.md). Compiled anchor defaults
enter compilation; remaining defaults combine with match-time restrictions.

All 3,392 rows completed, with 960 matches and 2,432 misses.
Every JIT compilation returned zero and positive code size. No outcomes were
discarded and there were no compile/match errors. Raw LF output SHA-256:
`e29d76c2101b9536daaf7f4239f01189f5a50acdab16ea8ad30f8172b1024e48`.

Call-time ANCHORED or ENDANCHORED selects interpreted matching. Native
is_anchored in pcre2_compile.c requires an explicit second branch for OP_COND
and OP_SCOND, then checks every alternative. first_significant_code skips
capture, recursion, true, and false condition headers. An unanchored inactive
branch still defeats the proof. The anchored driver stops before unmatched-name
replay. Compiled NotBol retains lexical caret anchoring.

The managed compiler retains and charges both conditional bodies, including
unselected version branches. Assertion conditions need separate inference and
are outside this increment. Each theory completes native8 comparisons before
the managed consumer runs. Preserve literal protocols and assertions at the
regression checkpoint. Final source identities and qualification are recorded
on PR #305.
