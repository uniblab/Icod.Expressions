# Marks after backtracking through an accepted call

On `abc`, the pattern
`(*MARK:M)(?:\A(?&x)c|abc)(?(DEFINE)(?<x>a(*ACCEPT:N)|ab))`
returns a three-byte match with mark `M` under per-call Anchored or
EndAnchored. The interpreter retains mark `N` when the call returns,
then restores `M` as a caller failure unwinds the call.
[Pinned PCRE2 interpreter](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_match.c#L940-L973)

The [match-policy probe](match-policy-oracle.c) ran against static 8-bit
PCRE2 10.48 at `7978954dbd2efc6f2196869290553cf1871b4ce6` with SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`. The
[policy protocol](match-policy-oracle.md) describes the seven input fields.

The 40 [definitions](../../../samples/PackageSmoke/PcreInterpretedAcceptedCallMarkCases.cs)
each exercise call masks 0 through 63. All 2,560 outcomes are matches,
with captures and successful marks recorded independently for each mask.
Each JIT compilation returned zero and reported positive code size.
The LF-terminated raw output has SHA-256
`6c14e3998a11b3967c7fa662a40388e1c9ce1d203d9228a4b7d330212fd5d7b6`.

The definitions include named and unnamed ACCEPT, a preceding MARK,
nested calls, positive and negative assertions, assertion conditions,
fallback captures, nonzero offsets, supplementary scalars, malformed
UTF-8, and compiled anchoring. The final controls check successful calls,
lexical PRUNE, and ordinary returns. Two resource cases verify the exact
shared work budget and cancellation on the fallback path that restores `M`.
