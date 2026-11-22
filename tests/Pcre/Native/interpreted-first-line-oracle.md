# Interpreted FIRSTLINE after fragment and SKIP restarts

The independent preflight uses static 8-bit PCRE2 10.48 at
`7978954dbd2efc6f2196869290553cf1871b4ce6` with JIT enabled and SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`.
Compile the [policy probe](match-policy-oracle.c), Git blob
`9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee`, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static 8-bit library.

For each definition in
[PcreInterpretedFirstLineCases.cs](../../../samples/PackageSmoke/PcreInterpretedFirstLineCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`. The
[policy protocol](match-policy-oracle.md) records option bits and dispatch.
Match-time anchoring options select the interpreter.

All 3,072 rows completed with 832 matches and 2,240 misses. Every JIT
compilation returned zero with positive code size. No rows were discarded
and no native diagnostics occurred. The raw LF output has SHA-256
`e046abf77af0c51ddb8136da27f5f5b7beafd0e8c94451fd693d748264aa25e3`.

In the interpreter, each first-code-unit scan under FIRSTLINE starts at the
current candidate and ends at the next newline or the fragment end. A match
failure at a newline ends that fragment's bumpalong. If tolerant UTF has
more valid text after an invalid byte, the interpreter restarts with that
fragment's bounds. A forward SKIP first checks the failed candidate, then
installs the adjusted target as the next candidate. Any first-code-unit scan
in that attempt starts at the new target.
See the [pinned interpreter source](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_match.c#L7587-L7613).

Definitions 1–20 cover fragment restarts, several newline conventions,
COMMIT, capture rollback, and named SKIP replay. Definitions 21–36 exercise
forward SKIP with byte, UTF, UTF/UCP, and tolerant UTF compilation. The
remaining definitions cover CRLF target adjustment, Unicode offsets and
captures, a nonzero requested offset, compiled EndAnchored, a sequence of
both restart types, and five controls. Each theory checks the native
protocol before comparing the managed matches. The two resource cases
exercise both restart types under one work budget and cancellation token.
