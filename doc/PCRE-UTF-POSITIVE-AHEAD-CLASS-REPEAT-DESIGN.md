# Positive lookahead before mandatory range-class repeats

Continue issue #102 after merged PR168 (30,631 tests; frozen 214). The
optional-bracket bitmap proof crosses positive lookahead before a direct
CR-excluding class, supported type, literal or mandatory literal repeat. A
mandatory repeat of a direct range class is still rejected by the bounded
positive-ahead continuation and leaf proof. Pinned PCRE2 10.48 retains LF in
its first bitmap after CR under ANYCRLF for these patterns.

Recognize a positive-minimum, positive-maximum repeat whose child is directly
a `PcreByteScalarSet`, including the parser's possessive atomic wrapper. Reuse
`HasUtfCrLfClassBitmap` to reject CR-containing, negated, predicate-bearing,
empty, malformed or out-of-range sets and to charge every range at the proof
leaf. Pre-scan against a copy of the visit count so the speculative check does
not double-charge it. Preserve all-branch mandatory heads, iterative bounds,
cancellation, skipped assertion/boundary prefixes, success-only checkpoints,
and the existing runtime matcher. Zero-minimum repeats, nested operands and
unproven classes remain on their former paths.

Native PCRE2 10.48 with JIT enabled reports LF present, CR absent and positive
JIT sizes for plus, exact, bounded lazy, possessive and mixed-width class
repeats in optional capture, noncapture and atomic brackets. Pin full native
capture spans on CRLF and ordinary subjects, in strict and tolerant UTF.
Commit the tests, observe a successful-build RED confined to this class, then
make the bounded proof change. Require six native Staging OS/architecture jobs
on net8/9/10, the full suite and frozen214 with no failed/skipped tests or
compiler warnings/errors and audit all configured gates. Keep issue #102
open. Publication, consumer migration, CommandFramework removal and Grep
remain separate.
