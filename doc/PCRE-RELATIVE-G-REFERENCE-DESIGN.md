# Relative `\g` backreference slice

**Status:** issue #7 internal syntax tranche, after the absolute numeric
`\g` slice. No public PCRE API or Grep consumer switch follows from this work.

## Contract

PCRE2's [backreference rules](https://www.pcre.org/current/doc/html/pcre2pattern.html)
define `\g{-N}` and `\g-N` relative to the number of the *next* capture to
open: a negative operand resolves to `groupCount + 1 - N`. A positive
`\g{+N}` or `\g+N` references the Nth capture yet to open, so it resolves to
`groupCount + N`. An enclosing capture counts because its opening parenthesis
has already been numbered. The relative value is resolved during parsing; the
existing post-parse reference validation checks that the target group exists.

Only positive magnitudes yielding a target in the internal 1–256 capture
range are accepted. Zero, missing digits and out-of-range targets produce
`InvalidSyntax` at the escape. Named `\g{name}`, `\g<...>` subroutine calls,
bare multi-digit/octal ambiguity and bracket-class uses remain unsupported.
No runtime PCRE dependency or new VM opcode is needed: relative forms lower
to the existing absolute-number backreference node after resolving the group.

## Reference boundary

Pin negative references after two captures and inside an enclosing capture,
positive forward references before a group and after a previous iteration,
UTF-8 caseless spans, and invalid/pending operands. Use the pinned PCRE.NET
1.6.0 / PCRE2 10.48 test-only wrapper. The existing capture journal, byte
spans, resource limits and cancellation path must remain intact. All six
Staging jobs and package-only consumer must pass before review.
