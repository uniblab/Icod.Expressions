# Caseless negative assertions before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged #196. The specialized UTF start proof
accepts at most eight non-newline scalars in a caseful leading negative
literal assertion, ahead or behind. Pinned PCRE2 10.48 compile/JIT
probes retained LF and excluded CR for 210 caseless patterns with one,
two, three or eight scalars across three middle prefixes and five
nullable group shapes, including special S/K/Kelvin case folds. All had
positive JIT sizes and expected CRLF and ordinary subject offsets.

## Scope and choice

Allow the same bounded negative literal scan when caseless. The initial
source revision passed all 70 no-middle CRLF cases but retained 140
failures with caseless middle `(?=B)` or `(?!Z)`. Permit those middle
literal assertions only after the bounded caseless negative lead has
been established. Every scalar in the leading negative assertion
must still be a valid UTF scalar other than CR/LF; case folding does not
turn a non-newline scalar into CR or LF. Keep the optional group, final
mandatory CR-excluding head, supported middle prefixes, cancellation
and 1,000-visit ceiling. Longer and newline-containing negatives stay
outside this proof. Parser, matcher, public API, limits and consumers
are unchanged.

Pin full match and capture spans for 420 native fixtures / 840 strict
and tolerant checks across fourteen leads, three prefixes, five group
shapes and two subjects. Test-only RED preceded the source changes.
