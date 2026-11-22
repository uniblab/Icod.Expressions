# Subject bounds in interpreted variable lookbehind

The preflight uses static 8-bit PCRE2 10.48 at
`7978954dbd2efc6f2196869290553cf1871b4ce6` with JIT enabled and SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`.
Compile the [policy probe](match-policy-oracle.c), Git blob
`9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee`, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static 8-bit library.

For each definition in
[PcreInterpretedLookbehindEndCases.cs](../../../samples/PackageSmoke/PcreInterpretedLookbehindEndCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`. The
[policy protocol](match-policy-oracle.md) records option bits and dispatch.
Per-call Anchored and EndAnchored select interpreted matching.

The 3,072 rows contain 992 matches and 2,080 misses. Every JIT compilation
returned zero with a positive code size. There were zero discarded rows
and zero native errors. The raw LF output has SHA-256
`3755926dbc739cbc360a1219379836098ede28afe08fa51bd7c2a4e0790f483e`.

The interpreter tries the longest available reverse distance, then tries
shorter distances on failure. At the end of a variable branch it compares
the final position to the position saved on entry to the lookbehind. A
nested lookahead uses the real subject or fragment bounds. A possessive
repeat can run past the endpoint and cause the branch to fail there.
The JIT uses the saved position as the lookbehind's visible subject end.
See the [pinned interpreter source](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_match.c#L6233-L6454).

Definitions 1–28 run seven shapes in byte, UTF, UTF/UCP, and tolerant UTF
modes. Definitions 29–43 extend the cases to end and word assertions,
referenced widths, nested lookbehind, mixed branch lengths, UTF byte
offsets, malformed fragments, and resumable assertions. Definitions 44–48
check fixed lookbehind, endpoint backtracking, malformed fragment bounds,
the physical end of the subject, and backreferences. The resource cases
use a nested lookahead that captures a byte and a marker after the saved
endpoint.
