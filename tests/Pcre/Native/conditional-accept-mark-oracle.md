# Marks after interpreted conditional assertion ACCEPT

On `a`, `(*MARK:M)(?(?=a(*ACCEPT:N))a|b)` returns a one-byte match
with mark `M` when per-call Anchored or EndAnchored selects the
interpreter. An assertion condition that returns MATCH_ACCEPT supplies
its captures to the caller. The caller resumes with its prior mark.
[Pinned PCRE2 interpreter](https://github.com/PCRE2Project/pcre2/blob/7978954dbd2efc6f2196869290553cf1871b4ce6/src/pcre2_match.c#L6109-L6121)

The [match-policy probe](match-policy-oracle.c) ran against static 8-bit
PCRE2 10.48 at `7978954dbd2efc6f2196869290553cf1871b4ce6` with SLJIT
`de0259c7aaf36aa40cba8014f3fad3edde9307f9`. The
[policy protocol](match-policy-oracle.md) describes the seven input fields.

The 40 [definitions](../../../samples/PackageSmoke/PcreConditionalAcceptMarkCases.cs)
each exercise call masks 0 through 63. All 2,560 outcomes are matches.
Captures and successful marks are recorded for each mask. Each JIT
compilation returned zero and reported positive code size. The
LF-terminated raw output has SHA-256
`923fe79b1a80cf2e43267933fefeaa20b1ca96f4a70d61589f879d699f3c70cc`.

Definitions 1–36 cover positive and negative lookahead and lookbehind,
captures, a missing entry mark, nested assertions and calls, compiled
anchoring, supplementary scalars, and malformed UTF-8 fragments.
The remaining controls check normal assertion completion, standalone
assertion ACCEPT, a branch that sets a new mark, and unnamed ACCEPT.
Two resource cases use
`(*MARK:before)(?(?=(?<seen>a)(*ACCEPT:inside))a|b)` on `a`. The result
retains capture `seen` and mark `before` while sharing the work budget
and cancellation token across assertion completion.
