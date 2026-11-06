# Positive lookahead before mandatory supported-type repeats

Continue issue #102 after merged PR169 (30,673 tests; frozen 214). The bounded
optional-bracket UTF bitmap proof accepts direct scalar types after positive
lookahead but admits only literal and range-class repeats. Mandatory repeats
of those same supported scalar types still stop the proof, losing LF entry
after CR in ANYCRLF. Pinned native PCRE2 10.48 retains LF.

Recognize a positive-minimum and positive-maximum repeat whose child is a
`PcreByteScalarClass` accepted by the existing `HasUtfCrLfTypeBitmap` predicate,
including the possessive atomic wrapper. The predicate permits only ASCII
digit/word, horizontal space and negated ASCII space; parser UCP distinctions
remain in force. Preserve the all-branch mandatory proof, iterative visit
limit, cancellation, skipped prefix behavior, success-only checkpoints and
the current runtime matcher. Reject zero-minimum repeats, arbitrary Unicode
properties, negated horizontal space, general classes and nested operands.

Native PCRE2 10.48 with JIT enabled reports LF present and CR absent in the
first bitmap and positive JIT size for mandatory plus, exact, lazy, possessive
and finite repeat shapes across these four supported types. Pin complete
capture spans for CRLF and ordinary subjects in strict and tolerant UTF.
Commit test-only fixtures and observe a successful-build RED confined to the
new class before changing production. Require six native Staging OS/arch jobs
on net8/9/10, full suite and frozen214 with zero failed/skipped tests or
compiler warnings/errors, and audit configured gates. Keep issue #102 open;
publication and migrations are separate, with Grep last.
