# Capturing groups and source-anchor named SKIP

The independent preflight uses static native8 PCRE2 10.48 at
7978954dbd2efc6f2196869290553cf1871b4ce6 with SLJIT at
de0259c7aaf36aa40cba8014f3fad3edde9307f9 and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreCaptureAnchorSkipCases.cs](../../../samples/PackageSmoke/PcreCaptureAnchorSkipCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`.
Mode and policy bits are recorded in the
[original policy record](match-policy-oracle.md). Compiled anchor defaults
enter compilation. Remaining defaults combine with match-time restrictions.

All 2,048 rows completed, with 576 matches and 1,472 misses.
Every JIT compilation returned zero and positive code size. No rows were
discarded and there were no compile/match errors. Raw LF output SHA-256:
`de4756adfce4f87737880f074c091ce7ee425d485505fc1a9ba032983199eea7`.

Call-time ANCHORED or ENDANCHORED selects interpreted matching. Native
is_anchored in pcre2_compile.c descends capturing groups when establishing
source anchoring. The anchored driver then stops unmatched-name replay.
Optional, mixed-alternative, consuming-prefix, and multiline controls
separate those cases from the bounded mandatory capture-prefix proof.

Every theory completes native8 comparisons before running the managed
consumer. Preserve literal protocols and assertions at the failure checkpoint.
Final source identities and qualification are recorded on PR #301.
