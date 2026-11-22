# Interpreted forward SKIP newline admission

With ANYCRLF newline handling, subject `\r\nX`, pattern
`\A(?s:.)(*SKIP)(*F)|(?s:..)`, and per-call `EndAnchored`, native
PCRE2 reports no match. The forward SKIP targets the LF, and the interpreter
advances to `X` before trying another candidate. The managed matcher
previously entered the LF directly and accepted the two-byte fallback.

For an interpreted forward SKIP, the matcher applies the `FIRSTLINE`
newline check at the failed candidate before selecting the target. It then
shares the CRLF target rule with same-offset named SKIP replay. The rule
advances a target on the LF of a complete CRLF pair when the target is past
the requested offset, the newline convention recognizes CRLF, and the
pattern has no explicit newline. The JIT path enters the original target.
Forward transfers use the same candidate initialization and shared work
counter, and retain any named SKIP replay suppression.

The [native oracle record](../tests/Pcre/Native/interpreted-forward-skip-oracle.md)
describes 48 definitions and 3,072 outcomes. They cover plain and named
forward SKIP, CRLF conventions, `FIRSTLINE`, replay suppression, captures,
UTF byte offsets, and malformed fragments. Two resource cases check the
exact work budget and cancellation. A twenty-ninth package smoke record
exercises the same outcomes against the packed library.

## Tests-only checkpoint

Commit `c0e67a08a6ed483e63e37b4edaa80930fa8f7fd9` and tree `bcb341da7ee5805ae26ae2ff7095b67a0f70aa79`
produced 43 failures, 107,474 passes, and zero skips on
[Linux ARM64](https://github.com/uniblab/Icod.Expressions/actions/runs/44973320468/job/137057610532),
[Linux x64](https://github.com/uniblab/Icod.Expressions/actions/runs/44973320468/job/137057610570), and
[Windows x64](https://github.com/uniblab/Icod.Expressions/actions/runs/44973320468/job/137057610524).
Definitions 1–41 passed their native assertions before failing the managed
comparison. Both resource cases reproduced the wrong restart offset.
All 107,467 existing tests and seven new controls passed.

- `samples/PackageSmoke/PcreInterpretedForwardSkipCases.cs` at `d736a8e9bcff8dcdfb3689088b0c7aae6837209d`
- `tests/Icod.Expressions.Tests.csproj` at `abf1a07e5bde1b3bdfb5ae384ced680700b002e3`
- `tests/Pcre/PcreInterpretedForwardSkipTests.cs` at `c4b1be143f215b9cf68e74eaad513e254c3ac372`

Final qualification is recorded on [PR #312](https://github.com/uniblab/Icod.Expressions/pull/312).
