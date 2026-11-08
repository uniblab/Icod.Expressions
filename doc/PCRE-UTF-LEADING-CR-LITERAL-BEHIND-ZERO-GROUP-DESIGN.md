# CR-containing leading literal lookbehind before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged #193. The bounded UTF start-bitmap proof
currently rejects CR in the leading positive literal lookbehind. Pinned
PCRE2 10.48 compile/JIT probes retained LF and excluded CR for all 135
patterns across nine leads, three middle prefixes and five group shapes,
with positive JIT sizes. All three native subject matches per pattern
matched at the expected offsets. Six leads contain CR; three LF-only leads
are controls.

## Scope and finding

The test-only CI passed 34,571 cases per framework before any production
change. The CR guard is necessary: a CR-containing lookbehind can succeed
at the LF inside CRLF, whereas admitting it to this specialized start
skip would bypass that candidate. Native first-bitmap membership alone
does not prove that the skip is safe. Retain the guard and pin this
boundary in the reference suite. The existing eight-scalar limit,
parser, matcher, public API, limits and consumers remain unchanged.

The final matrix has 390 distinct native fixtures / 780 strict and
tolerant checks across nine leads, three prefixes, five repeated-group
shapes and CRLF/ordinary/positive subjects. The single-CR positive
subject is identical to its CRLF case and is enumerated only once.
