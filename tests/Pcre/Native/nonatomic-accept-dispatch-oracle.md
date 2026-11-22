# Native dispatch for non-atomic assertion ACCEPT

PCRE2 10.48 cannot JIT an ACCEPT lexically inside a non-atomic assertion.
Its opcode scan retains the outer non-atomic assertion's endpoint while
visiting nested groups and rejects an OP_ASSERT_ACCEPT before that endpoint.
[Endpoint tracking](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_jit_compile.c#L1221-L1228)
and [ACCEPT rejection](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_jit_compile.c#L1321-L1326).

This selects interpreted semantics for the complete pattern. For example,
`(*MARK:M)(?(?=a(*ACCEPT:N))a|b)(?*(*ACCEPT)){0}` on `a` returns
mark `M` without call-time anchoring. The zero-repeat assertion still
prevents native JIT compilation. Unused DEFINE and false version branches
have the same effect. An external subroutine containing ACCEPT does not
trigger this lexical restriction when called from a non-atomic assertion.

The [match-policy probe](match-policy-oracle.c) ran against static 8-bit
PCRE2 10.48 at `7978954dbd2efc6f2196869290553cf1871b4ce6` with SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`. The
[policy protocol](match-policy-oracle.md) describes the seven input fields.

The 40 [definitions](../../../samples/PackageSmoke/PcreNonAtomicAcceptDispatchCases.cs)
each exercise call masks 0 through 63. All 2,560 outcomes are matches.
For definitions 1–36, every JIT compilation returned -68 and reported zero
code size. Definitions 37–40 returned zero and positive code size. The
LF-terminated raw output has SHA-256
`683c01d3d1230b8ff431f292e3f219ffe43fc8d29ae2f2b559f6c20b8571b55b`.

The corpus covers lookahead and lookbehind, nested atomic assertions,
zero-repeat ACCEPT, unused definitions, false static branches, assertion
aliases, captures, match-start resets, compiled anchors, UTF/UCP,
supplementary scalars, malformed fragments, and nonzero start offsets.
Controls cover non-atomic assertions without ACCEPT, an external callee,
ACCEPT after a closed assertion, and quoted syntax. Two resource cases
retain a completed capture and the entry mark while exercising the exact
work budget and cancellation without call-time anchor flags.
