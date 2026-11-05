# Positive lookahead before mandatory literal repeats

Continue issue #102 after merged PR166 (30,560 tests; frozen 214). The
optional bracket bitmap proof accepts a positive lookahead only when its first
significant continuation is a direct CR-excluding literal or a supported
scalar type. PCRE2 10.48 `set_start_bits` also skips the assertion before
mandatory single-character literal repeats (`PLUS`, `MINPLUS`, `POSPLUS`,
`EXACT`, and `CHAR` followed by `UPTO`). A repeated literal currently stops
the optional body proof, which can lose LF entry after CR under ANYCRLF.

Extend the existing iterative mandatory-body proof to recognize only a
positive-minimum repeat of a direct, valid, CR-excluding literal. Look through
the established unquantified assertion/boundary prefix and, for a possessive
repeat, its atomic wrapper. Retain the current all-branches requirement,
success-only visit checkpoint, cancellation and checked proof limit. Repeats
with minimum zero, general classes, properties, nested groups or other
unproven operands stay on their existing paths. The runtime repeat,
assertion and capture behavior is unchanged.

Pinned native PCRE2 10.48 with JIT enabled confirms the candidate patterns
have a first bitmap with LF present and CR absent, JIT size greater than zero,
and expected byte capture spans. Compare native PCRE.NET 1.6.0 matches to
managed matches in strict and tolerant UTF for optional capture, noncapture
and atomic brackets, plus, exact, bounded, lazy and possessive inner repeats,
with CRLF and ordinary subjects. Commit tests and observe a successful-build
RED before production changes. Then require six Staging OS/architecture jobs
on net8/9/10, full suite and frozen 214 with no failed or skipped tests or
compiler warnings/errors, plus configured workflow gates. Review the complete
branch before marking its PR ready; keep issue #102 open.

No consumer migration, release or CommandFramework removal is implied by
the component smoke gates. Grep remains last.
