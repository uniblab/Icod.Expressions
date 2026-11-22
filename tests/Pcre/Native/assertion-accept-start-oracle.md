# Reported match start after assertion ACCEPT

With lookaround `\K` enabled, `(?=a\K(*ACCEPT))a` on `a` reports a
one-byte match when per-call EndAnchored selects the interpreter. The
assertion's early acceptance returns its completed captures and its mark.
The caller resumes with the reported start that was in effect when it
entered the assertion, including any earlier caller-side `\K`.
[Pinned PCRE2 interpreter](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_match.c#L5783-L5814)

The [match-policy probe](match-policy-oracle.c) ran against static 8-bit
PCRE2 10.48 at `7978954dbd2efc6f2196869290553cf1871b4ce6` with SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`. The
[policy protocol](match-policy-oracle.md) describes the seven input
fields. Mode bit 8 enables `PCRE2_EXTRA_ALLOW_LOOKAROUND_BSK`.

The 40 [definitions](../../../samples/PackageSmoke/PcreAssertionAcceptStartCases.cs)
each exercise call masks 0 through 63. The 2,560 rows contain 2,160
matches and 400 misses. Each JIT compilation returned zero and reported
positive code size. The LF-terminated raw output has SHA-256
`b0c903baef659a4d234eeb610d8a83956f790fae80a838832170143a7fc7d547`.

The cases cover positive lookahead, lookbehind, conditional assertions,
captures, marks, nested assertions, UTF-8 offsets, and malformed fragments.
The resource tests use `x\Kz(?=(a\K(*ACCEPT:m)))a` on `xza`. The
interpreter reports byte 1 with length 2 and captures the final byte at
byte 2 with mark `m`. The intervening `z` makes the earlier reset
distinct from the assertion's entry position. The tests check the work
budget and cancellation on that path.
