# Mandatory repetitions, source anchoring, and named SKIP

The independent preflight uses static native8 PCRE2 10.48 at
7978954dbd2efc6f2196869290553cf1871b4ce6 with SLJIT at
de0259c7aaf36aa40cba8014f3fad3edde9307f9 and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreRepeatAnchorSkipCases.cs](../../../samples/PackageSmoke/PcreRepeatAnchorSkipCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`.
Mode and policy bits are recorded in the
[original policy record](match-policy-oracle.md). Compiled anchor defaults
enter compilation; remaining defaults combine with match-time restrictions.

All 3,840 rows completed, with 1,072 matches and 2,768 misses.
Every JIT compilation returned zero and positive code size. No outcomes were
discarded and there were no compile/match errors. Raw LF output SHA-256:
`3eba5bd451e3ddeeb27c7c67ad49fe0feee22f80f03976cf52aa92b1a602ef99`.

Call-time ANCHORED or ENDANCHORED selects interpreted matching. Native
is_anchored in pcre2_compile.c recognizes mandatory group headers, including
capturing, possessive, and potentially empty variants. An optional group prefix
does not establish this evidence. The anchored driver stops before unmatched-name
replay. Source controls distinguish positive minimum from nullable execution
and from zero-minimum forms. Compiled NotBol retains lexical caret anchoring.

The managed compiler retains repeat nodes and may emit progress marks before
the child. Possessive repeats have an atomic wrapper. A positive minimum permits
inspection of the original child without changing the emitted program or its
budgeted execution. Each theory completes native8 comparisons before the
managed consumer runs. Preserve literal protocols and assertions at the
regression checkpoint. Final source identities and qualification are recorded
on PR #307.
