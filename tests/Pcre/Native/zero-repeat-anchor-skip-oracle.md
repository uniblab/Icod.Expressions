# Exact-zero repeats, source anchoring, and named SKIP

The independent preflight uses static native8 PCRE2 10.48 at
`7978954dbd2efc6f2196869290553cf1871b4ce6` with SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9` and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
`9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee`, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreZeroRepeatAnchorSkipCases.cs](../../../samples/PackageSmoke/PcreZeroRepeatAnchorSkipCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`.
The [policy protocol](match-policy-oracle.md) records option bits and dispatch.
Compiled anchor defaults enter compilation. Match-time `ANCHORED` and
`ENDANCHORED` select interpreted matching.

All 3,072 rows completed with 880 matches and 2,192 misses. Every JIT
compilation returned zero with positive code size. There were zero native
errors and zero discarded rows. The raw LF output has SHA-256
`08295e830b180b126e42d42ac09ba00eeeeca812da399030ee76e8142e80a5ea`.

`first_significant_code` advances over `OP_SKIPZERO` in native anchoring
analysis. The compiler emits this opcode for a skipped group so that a
subroutine can still call a capture inside it. It erases an exact-zero
literal or character class directly. The lexical analysis reaches a later
anchor in the same branch in either case. This evidence makes the
interpreted anchored attempt stop before unmatched-name replay.
See the [pinned compiler source](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_compile.c).

Definitions 1–32 cover eight branch and group shapes in byte, UTF, UTF/UCP,
and tolerant UTF modes. Definitions 33–38 cover compiled NotBol, a skipped
MARK and retained capture slots, a callable zero-count capture, a nonzero
search offset with `\G`, supplementary UTF-8 values, and `(*NO_START_OPT)`.
Definitions 39–47 exercise optional, star, counted optional, and possessive
optional prefixes, a mixed branch, a branch with only a zero-count prefix,
an omitted anchor, multiline caret, and the existing emitted-prefix proof.
Definition 48 combines the first definition with compiled EndAnchored.

Each theory checks all 64 native comparisons before running the managed
package consumer. The resource tests use the first definition. The
[status record](../../../doc/PCRE-ZERO-REPEAT-ANCHOR-SKIP-STATUS.md) records
the tests-only result and the fixture identities.
