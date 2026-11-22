# Named control verbs, source anchoring, and named SKIP

The independent preflight uses static native8 PCRE2 10.48 at
`7978954dbd2efc6f2196869290553cf1871b4ce6` with SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9` and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
`9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee`, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreVerbPrefixAnchorSkipCases.cs](../../../samples/PackageSmoke/PcreVerbPrefixAnchorSkipCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`.
The [policy protocol](match-policy-oracle.md) records option bits and dispatch.
Compiled anchor defaults enter compilation. Match-time `ANCHORED` and
`ENDANCHORED` select interpreted matching.

All 3,072 rows completed with 912 matches and 2,160 misses. Every JIT
compilation returned zero with positive code size. There were zero native
errors and zero discarded rows. The raw LF output has SHA-256
`ea7056b12af9650fb1a1e69933a2c1761a856c891b00e1df0c52d93f195f782d`.

`first_significant_code` advances over `OP_MARK`, `OP_COMMIT_ARG`,
`OP_PRUNE_ARG`, `OP_SKIP_ARG`, and `OP_THEN_ARG` using their stored argument
length. The corresponding unnamed cut opcodes are significant. The parser
treats an immediate closing parenthesis after the colon as an empty argument
and selects the unnamed form for verbs that allow it. A raw name beginning
with NUL is still a named form. The source anchor after these prefixes
makes a failed unresolved SKIP stop the anchored interpreted attempt.
See the [pinned compiler source](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_compile.c).

Definitions 1–32 cover eight shapes in byte, UTF, UTF/UCP, and tolerant UTF
modes. Definitions 33–41 exercise named verbs in positive assertions, a
mixed prefix chain with nonzero `\G`, compiled NotBol, supplementary UTF-8,
a NUL-leading raw name, and compiled EndAnchored. Definitions 42–48 cover
successful continuation, empty arguments, and incomplete source anchoring.

Each theory checks the 64 native comparisons before running the managed
package consumer. The focused resource tests use the first definition.
See the [status record](../../../doc/PCRE-VERB-PREFIX-ANCHOR-SKIP-STATUS.md)
for the tests-only result and the fixture identities.
