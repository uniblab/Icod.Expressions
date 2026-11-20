# Returned subroutine MARK history and named SKIP

The independent preflight uses static native8 PCRE2 10.48 at
7978954dbd2efc6f2196869290553cf1871b4ce6 with SLJIT at
de0259c7aaf36aa40cba8014f3fad3edde9307f9, built with JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

The literal catalog is
[samples/PackageSmoke/PcreReturnedSkipCases.cs](../../../samples/PackageSmoke/PcreReturnedSkipCases.cs).
For each definition and call mask 0 through 63, supply one probe line with
`id|mode|defaults|call|pattern-hex|subject-hex|start` followed by LF.
The mode and policy bit definitions are recorded in the
[original policy probe record](match-policy-oracle.md). Compiled anchor bits
enter compilation and the remaining defaults are combined with the call flags.

All 2,048 rows completed. Each JIT compilation returned zero and positive code
size. The outcomes contain 608 matches and 1,440 misses, with no compile/match
errors or discarded rows. SHA-256 of the complete raw LF output is
`b3619eb080a0a84a60e16252c0ab3b69228b73c69474c405749002bc931876c4`.

Default calls use native JIT. Call-time ANCHORED or ENDANCHORED selects the
native interpreter. The latter can resolve named SKIP against a MARK retained
inside a returned callee. Default/JIT hides that callee's history from root
lookup. Completed and selected captures, final labels, repeat/nested call
history, and original byte coordinates are part of each literal protocol.

Qualification compares every literal with PCRE.NET native8 first, then invokes
the managed package consumer. Preserve the catalog and assertions after the
regression checkpoint. Final failures, source identities, and platform results
are recorded on PR #297.
