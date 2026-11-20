# Unmatched named SKIP and anchored interpreter replay

The independent preflight uses static native8 PCRE2 10.48 at
7978954dbd2efc6f2196869290553cf1871b4ce6 with SLJIT at
de0259c7aaf36aa40cba8014f3fad3edde9307f9 and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreUnmatchedSkipCases.cs](../../../samples/PackageSmoke/PcreUnmatchedSkipCases.cs)
and call mask 0 through 63, supply a probe input line
`id|mode|defaults|call|pattern-hex|subject-hex|start` followed by LF.
Mode and policy bits are recorded in the
[original policy record](match-policy-oracle.md). Compiled anchor defaults
enter compilation. Remaining defaults are combined with match-time flags.

All 2,048 rows completed, containing 596 matches and 1,452 misses.
Every JIT compilation returned zero and positive code size. No rows were
discarded and there were no compile/match errors. SHA-256 of the raw LF output:
`f0462346e8844b1cedfd01100733142ff833fa17377f652dbb8e99a2ce28c6cd`.

Call-time ANCHORED or ENDANCHORED selects native interpreted matching.
In pcre2_match.c, MATCH_SKIP_ARG prepares a same-position replay, but the
driver's anchored termination check stops the attempt before that replay.
Default/JIT ignores unmatched names during lookup. Forward suffix success
never triggers the backtracking effect. Literal protocols include captures,
labels, byte coordinates, empty-match restrictions, and control cases.

Qualification compares every protocol with native8 through PCRE.NET before
running the managed consumer. Preserve literals and assertions at the failure
checkpoint. Final source identities and platform results are recorded on PR #298.
