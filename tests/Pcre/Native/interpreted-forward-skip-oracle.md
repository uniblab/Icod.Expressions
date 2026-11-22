# Forward SKIP and interpreted newline boundaries

The independent preflight uses static native8 PCRE2 10.48 at
`7978954dbd2efc6f2196869290553cf1871b4ce6` with SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9` and JIT enabled.
Compile the [policy probe](match-policy-oracle.c), Git blob
`9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee`, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static native8 library.

For each definition in
[PcreInterpretedForwardSkipCases.cs](../../../samples/PackageSmoke/PcreInterpretedForwardSkipCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`.
The [policy protocol](match-policy-oracle.md) records option bits and dispatch.
Compiled anchor defaults enter compilation. Match-time `ANCHORED` and
`ENDANCHORED` select interpreted matching.

All 3,072 rows completed with 976 matches and 2,096 misses. Every JIT
compilation returned zero with positive code size. There were zero native
errors and zero discarded rows. The raw LF output has SHA-256
`c6806a7721b90970065de08086bfbe8255ecca029249428dd84d76403adebc57`.

On a forward `MATCH_SKIP`, the interpreter records `verb_skip_ptr` as the
next candidate. It checks `FIRSTLINE` at the failed candidate before changing
the start. At the new candidate it applies the CRLF bumpalong rule, which
requires a complete pair, a target past the requested start, a compatible
newline convention, and no explicit CR or LF match in the pattern. The JIT
can enter the raw target.
See the [pinned interpreter source](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_match.c).

Definitions 1–32 cover eight shapes in byte, UTF, UTF/UCP, and tolerant UTF
modes. Definitions 33–41 include a nonzero offset, compiled EndAnchored,
malformed fragments, capture rollback, a supplementary scalar, `FIRSTLINE`
with additional newline forms, and an atomic group. Definitions 42–48
exercise LF and CR conventions, an explicit newline, a permitted forward
skip across an earlier newline, compiled Anchored, EOF, and a non-pair target.

Each theory checks the 64 native comparisons before running the managed
package consumer. The resource tests use the one-byte fallback after CRLF.
See the [status record](../../../doc/PCRE-INTERPRETED-FORWARD-SKIP-STATUS.md)
for the tests-only result and fixture identities.
