# Empty-match restrictions and interpreted ACCEPT

The preflight uses static 8-bit PCRE2 10.48 at
`7978954dbd2efc6f2196869290553cf1871b4ce6` with JIT enabled and SLJIT at
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`.
Compile the [policy probe](match-policy-oracle.c), Git blob
`9e4a61ee74b61111b8153deef7d0e2aa90c3e9ee`, with
`gcc -O2 -std=c11 -Wall -Wextra -Werror` against the static 8-bit library.

For each definition in
[PcreInterpretedAcceptPolicyCases.cs](../../../samples/PackageSmoke/PcreInterpretedAcceptPolicyCases.cs)
and call mask 0 through 63, supply one LF-terminated line with
`id|mode|defaults|call|pattern-hex|subject-hex|start`. The
[policy protocol](match-policy-oracle.md) records option bits and dispatch.
Per-call Anchored and EndAnchored select the interpreter.

The 3,072 rows contain 2,708 matches and 364 misses. Every JIT compilation
returned zero with positive code size. There were zero discarded rows
and zero native errors. The raw LF output has SHA-256
`71789d61f99ce94f05bb4212070595ddcea2d39da6404fb7b7f6e1af81d9973d`.

At root ACCEPT, interpreted matching treats a disallowed empty result as a
backtrack before it applies the end-anchored check. A result that passes
the empty-match check and ends short under EndAnchored abandons the current
candidate. The compiled EndAnchored cases show the JIT's end-first behavior.
See the [pinned interpreter source](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_match.c#L974-L1014).
The compiled path is in the [pinned JIT source](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_jit_compile.c#L11552-L11605).

Definitions 1–32 run eight shapes in byte, UTF, UTF/UCP, and tolerant UTF
modes. Definitions 33–43 extend the cases to malformed fragments,
supplementary scalars, nonzero starts, compiled policies, and capture and
marker rollback after lookbehind. Definitions 44–48 check premature
nonempty ACCEPT, assertion and callee ACCEPT, and a nonempty match at the
physical end. Both resource cases use a root acceptance made empty by `\K`.
