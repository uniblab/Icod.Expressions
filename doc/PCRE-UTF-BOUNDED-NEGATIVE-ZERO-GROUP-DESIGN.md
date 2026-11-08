# Bounded multi-scalar negative assertions before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged #195. The specialized UTF optional-group
start proof currently accepts one caseful negative assertion scalar
excluding CR/LF. Pinned PCRE2 10.48 compile/JIT probes retained LF and
excluded CR for 120 patterns having two, three or eight literal scalars,
across three middle prefixes and five nullable group shapes. All had
positive JIT sizes and the expected CRLF and ordinary subject offsets.

## Scope and choice

Extend only the existing negative literal assertion scan to at most eight
valid non-CR/LF UTF scalars, ahead or behind. The optional group, final
mandatory CR-excluding class, supported middle prefixes, cancellation
and 1,000-visit ceiling retain their guards. Caseless, newline-containing
and longer negative assertions stay outside the proof. No parser,
matcher, public API, limits or consumer change is intended.

Pin full match and capture spans for 240 native fixtures / 480 strict
and tolerant checks across eight leads, three prefixes, five group
shapes and two subjects. Observe test-only RED before changing code.
