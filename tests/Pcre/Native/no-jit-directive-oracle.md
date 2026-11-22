# NO_JIT start directive

PCRE2 accepts `(*NO_JIT)` in the uninterrupted start-directive prefix and
disables native JIT compilation for the complete pattern. It can repeat
and interleave with other start directives. Quoted text and literal mode
retain the source spelling as ordinary text.
[Directive parsing](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_compile.c#L748)
and [JIT suppression](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_jit_compile.c#L14380).

The directive selects interpreted semantics. On `a`,
`(*NO_JIT)(*MARK:M)(?(?=a(*ACCEPT:N))a|b)` returns mark `M` without
call-time anchoring. Assertion match-start resets, accepted-call mark
rollback, empty ACCEPT rejection, named SKIP replay, and lookbehind subject
visibility also follow the existing interpreted paths.

The [match-policy probe](match-policy-oracle.c) ran against static 8-bit
PCRE2 10.48 at `7978954dbd2efc6f2196869290553cf1871b4ce6` with SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`. The
[policy protocol](match-policy-oracle.md) describes the seven input fields.

The 40 [definitions](../../../samples/PackageSmoke/PcreNoJitDirectiveCases.cs)
each exercise call masks 0 through 63: 2,232 matches and 328 misses.
Every JIT compilation returned zero. Definitions 1–36 reported zero code
size; the four controls reported positive code size. The LF-terminated raw
output has SHA-256
`c7ad1febeff51a45b21334e76affb47056a2b9b1b6f4284d9328c2e0579b97b8`.

The corpus covers byte, UTF and UCP modes, supplementary scalars,
malformed fragments, nonzero offsets, captures and marks, compiled anchors,
repeated directives, and prefix ordering. Resource cases retain a completed
capture and the entry mark while checking the exact work budget and
cancellation without call-time anchors. The shared consumer checks owned
source and input views, policy reuse, metadata identity, and sentinels.

Five earlier pending-syntax rows retain coverage with still-pending limit
or callout headers. Independent native probes confirm the replacement
headers: LIMIT_HEAP with FAIL has no match; LIMIT_DEPTH with `a` matches;
CASELESS_RESTRICT and UTF TURKISH_CASING with LIMIT_MATCH accept the empty
body. A callout following NO_JIT retains its original opening byte offset.
