# Native 16-bit CRLF, ANYCRLF and ANY newlines

The word compiler accepts the shared `(*CRLF)`, `(*ANYCRLF)` and `(*ANY)`
line conventions on raw 16-bit and strict UTF-16 input. Prefix ordering and
original-unit diagnostics follow the existing shared parser. These conventions
control dot, `\N`, multiline anchors and the final-newline behavior of `$` and
`\Z`. `ANY` includes VT, FF, NEL, line separator and paragraph separator.
`\R` keeps its independently selected BSR convention and consumes CRLF atomically.

## Original input and search entry

The matcher reads the original char view. CRLF-aware predicates inspect at most
two units, and all returned spans remain in code units. A caller can request an
initial LF position inside a pair. After a failed CR candidate, ordinary search
advances past LF unless source newline metadata or a qualified native search
proof permits entry there. The compiler reuses the existing source metadata
analysis used by the byte backend.

The word search proofs are bounded to 1,000 visits. Postorder first-set analysis
handles literals, positive range classes, alternatives and nullable repetition
or groups. It permits LF entry when the complete initial set excludes CR and
includes LF. A separate JIT prefix proof covers fixed-width atoms and group
copies followed by a literal, including a bounded final alternative of case-sensitive
literals. It checks fewer than twelve prefix units. Wider restrictive prefixes
and end-anchored matches compare the original subject's suffix unit before
choosing the next candidate. Call-time anchoring selects the native interpreter
and retains its own search advancement.

Forward `SKIP` enters its saved target directly under JIT dispatch. The
interpreter applies the CRLF-tail rule to that target and to unresolved named
`SKIP` replay. Explicitly mentioned CR/LF and the original requested offset keep
their established exceptions. Candidate scans, suffix comparisons, replay and
skipped units share work limits and cancellation with normal matching.

The shared parser's extended-mode `ANY` comments now read original UTF-16 units
for Unicode line terminators. A supplementary character inside a comment retains
its native width while the source cursor advances. Byte parsing continues to use
its native decoder.

## Qualification

`PcreWordPairNewlineTests` compares complete capture vectors and successful labels
with PCRE2-16 at every valid caller start, under eight compiled defaults and all
64 call masks. Cases cover all three new conventions in raw and UTF-16 modes,
line assertions, dot and dotall, classes, repeats, calls, lookarounds, cuts,
source comments, nullable starts, prefix limits and named-SKIP replay. Additional
UCP cases check shorthand and POSIX whitespace under Unicode classification.

Supplemental checks compare selected valid Unicode cases across managed widths,
exercise exact work budgets and cancellation at every observed step, and retain
owned captures and labels across sliced input and concurrent reuse. Diagnostics
cover malformed visible UTF-16, starts inside surrogate pairs and original
pattern offsets. The package consumer adds six frozen newline cases and requires
one complete 50-case word result on every native CI host.

The test reference remains PCRE.NET 1.6.0 with PCRE2 10.48. Detailed native ABI and
version provenance are recorded in [the oracle documentation](../tests/Pcre/Native/README.md).
The full syntax, optimizer, resource and cross-width release gates remain in the
[roadmap](PCRE-ROADMAP-STATUS.md). Grapheme matching is the next word increment.
