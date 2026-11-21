# False static conditions, source anchoring, and named SKIP

The independent preflight uses static native8 PCRE2 10.48 at
`7978954dbd2efc6f2196869290553cf1871b4ce6` with SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9` and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
`9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee`, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreStaticPrefixAnchorSkipCases.cs](../../../samples/PackageSmoke/PcreStaticPrefixAnchorSkipCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`.
The [policy protocol](match-policy-oracle.md) records option bits and dispatch.
Compiled anchor defaults enter compilation. Match-time `ANCHORED` and
`ENDANCHORED` select interpreted matching.

All 3,072 rows completed with 896 matches and 2,176 misses. Every JIT
compilation returned zero with positive code size. There were zero native
errors and zero discarded rows. The raw LF output has SHA-256
`c3ac4edbc0167871e4df0b0226936e267823aeb66792da2e3e0561fcd64a8ad6`.

`first_significant_code` advances past a conditional whose condition opcode
is `OP_FALSE` and whose only branch ends at `OP_KET`. `DEFINE` receives that
opcode after the native compiler validates its branch count. A false version
check receives the same opcode during condition compilation. The native
repeat compiler ignores quantifiers on this shape and retains the body for
subroutine calls. The next significant source item can provide a start anchor
for the interpreted driver's named SKIP handling.
See the [pinned compiler source](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_compile.c).

Definitions 1–32 exercise eight group shapes in byte, UTF, UTF/UCP, and
tolerant UTF modes. Definitions 33–40 cover a callable quantified declaration,
an exact-zero and static prefix chain, compiled NotBol, supplementary UTF-8,
`\G` with a nonzero start, skipped MARK history, compiled EndAnchored, and
`(*NO_START_OPT)`. Definitions 41–48 exercise false conditions with an explicit
else, true conditions, capture and recursion tests, mixed branches, and
anchors inside or after a declaration-only group.

Each theory checks the 64 native comparisons before running the managed
package consumer. The focused resource tests use the first definition.
See the [status record](../../../doc/PCRE-STATIC-PREFIX-ANCHOR-SKIP-STATUS.md)
for the tests-only result and the fixture identities.
