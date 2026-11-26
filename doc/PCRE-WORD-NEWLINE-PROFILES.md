# Native 16-bit single-unit newline and BSR profiles

The bounded word-sequence compiler reuses the shared prefix parser for
newline and BSR directives. It admits effective LF, CR and NUL conventions,
selected by `(*LF)`, `(*CR)` and `(*NUL)`. Each convention controls dot,
`\N`, multiline `^`/`$`, and the final-newline behavior of `$` and `\Z`.
Dotall still makes dot include newlines, while `\N` remains restricted.
Ordinary character classes and absolute `\A`/`\z` retain their semantics.

`(*BSR_ANYCRLF)` restricts `\R` to CR, LF and atomic CRLF; `(*BSR_UNICODE)`
restores its fixed Unicode newline list. BSR and line conventions are
independent. In particular, selecting NUL as the line separator does not
make `\R` match NUL. Repeated directives use the last setting for each
family, including when a later supported line convention replaces an
earlier CRLF/ANY choice.

These profiles work in raw-word and strict UTF-16 matching, across the
existing finite choices and four unbounded-repeat levels. They preserve
original code-unit coordinates, the strict validation window, work quotas
and cancellation. Prefix parsing keeps the original pattern and body index;
diagnostics still refer to the original spelling. Literal compilation and
quoted directive text remain literal.

## Evidence and remaining scope

Forty fixtures across eight compiled defaults and 64 call masks compare
20,480 outcomes with pinned PCRE2-16. Standalone native preflight returned
6,920 matches and 13,560 misses with no native errors. Its LF-terminated
output SHA-256 is
`6a4e1567041a536b594e504d0a2bbaceed7b40a38c00eeb1ca3bc97ca6b6ea1c`.
Additional tests cover managed cross-width results, exact work quotas,
cancellation, malformed prefixes, source coordinates and rejected directives.
The oracle revisions are pinned in [word-sequence evidence](PCRE-WORD-SEQUENCES.md).

Effective CRLF, ANYCRLF and ANY conventions remain explicitly unsupported.
Their remaining work includes native candidate advancement past the LF of
CRLF, explicit-newline metadata, and the prefix/search optimizations that
can bypass that advancement. Matching only their dot and anchor predicates
would not establish compatibility. The existing byte engine's corresponding
proofs and source-shape cases provide the starting point for that extension.
Other start directives, graphemes, general nested execution and durable
captures also remain open.
