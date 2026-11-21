# Alternatives, source anchoring, and named SKIP

The independent preflight uses static native8 PCRE2 10.48 at
7978954dbd2efc6f2196869290553cf1871b4ce6 with SLJIT at
de0259c7aaf36aa40cba8014f3fad3edde9307f9 and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreAlternativeAnchorSkipCases.cs](../../../samples/PackageSmoke/PcreAlternativeAnchorSkipCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`.
Mode and policy bits are recorded in the
[original policy record](match-policy-oracle.md). Compiled anchor defaults
enter compilation. Remaining defaults combine with match-time restrictions.

All 2,048 rows completed, with 528 matches and 1,520 misses.
Every JIT compilation returned zero and positive code size. No rows were
discarded and there were no compile/match errors. Raw LF output SHA-256:
`3faf254732433510c849a9312dc43778de541bb4b6c4d024540174d5fc6f1099`.

Call-time ANCHORED or ENDANCHORED selects interpreted matching. Native
is_anchored in pcre2_compile.c descends capturing/noncapturing groups and
checks every alternative. Source caret metadata remains anchored despite
match-time NotBol. The anchored driver stops unmatched-name replay.
Mixed/empty/optional/consuming/multiline controls distinguish other profiles.

Every theory completes native8 comparisons before the managed consumer runs.
Preserve literal protocols and assertions at the failure checkpoint.
Final source identities and qualification are recorded on PR #302.
