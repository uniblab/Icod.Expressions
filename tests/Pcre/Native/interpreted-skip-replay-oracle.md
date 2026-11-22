# Ordered named SKIP replay in interpreted matching

The independent preflight uses static native8 PCRE2 10.48 at
`7978954dbd2efc6f2196869290553cf1871b4ce6` with SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9` and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
`9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee`, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreInterpretedSkipReplayCases.cs](../../../samples/PackageSmoke/PcreInterpretedSkipReplayCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`.
The [policy protocol](match-policy-oracle.md) records option bits and dispatch.
Compiled anchor defaults enter compilation. Match-time `ANCHORED` and
`ENDANCHORED` select interpreted matching.

All 3,072 rows completed with 816 matches and 2,256 misses. Every JIT
compilation returned zero with positive code size. There were zero native
errors and zero discarded rows. The raw LF output has SHA-256
`3a27fcd8ab65ffe3854570981ca4c6b4df124de6d47e3936e4ad595b3660ee91`.

The native interpreter counts each execution of `OP_SKIP_ARG` and ignores
the first `ignore_skip_arg` visits of an attempt. When `MATCH_SKIP_ARG`
reaches the driver, it sets that threshold to `skip_arg_count` and selects
the same candidate. This can suppress a resolved named SKIP encountered
before an unresolved one. Forward `MATCH_SKIP` keeps the threshold and
ordinary bumpalong from `MATCH_NOMATCH`, `MATCH_PRUNE`, or `MATCH_THEN`
clears it. The driver applies anchoring and newline rules before repeating.
See the [pinned interpreter source](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_match.c).

Definitions 1–32 exercise eight shapes across byte, UTF, UTF/UCP, and
tolerant UTF modes. Definitions 33–41 cover forward assertion failure, a
nonzero search offset, compiled EndAnchored, multiple resolved prefixes,
forward SKIP carry, a fragment-local end, supplementary UTF-8, and LF and
CRLF first-line boundaries. Definitions 42–48 cover ordinary bumpalong,
explicit and source anchoring, an active call, successful continuation,
resolved names, and retry before the first newline.

Each theory checks the 64 native comparisons before running the managed
package consumer. The focused resource tests use the two-retry shape.
See the [status record](../../../doc/PCRE-INTERPRETED-SKIP-REPLAY-STATUS.md)
for the tests-only result and fixture identities.
