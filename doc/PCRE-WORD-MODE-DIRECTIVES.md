# Native 16-bit UTF and UCP directives

Word regex compilation accepts the initial global directives `(*UTF)`,
`(*UTF16)` and `(*UCP)`. They compose with the existing newline and BSR
prefixes in any admitted order. Repeated selections are idempotent.

| Selection | Effective word mode |
|---|---|
| Caller raw, no mode directive | Original 16-bit units, including isolated surrogates. |
| `(*UTF)` or `(*UTF16)` | Strict UTF-16 source and subject decoding. Supplementary scalars consume two original units. |
| `(*UCP)` | Unicode classes and raw-mode Unicode casing; UTF remains independently selected. |
| Caller UTF/UCP already enabled | The corresponding source directive preserves that selection. |

`(*UTF8)` and `(*UTF32)` are inappropriate for width 16 and remain rejected.
Directives must form an uninterrupted prefix before ordinary options, comments,
whitespace or body syntax. Prefix-like spelling in quotes, ordinary classes and
comments remains source data. The explicit literal compiler treats the entire
pattern as data and does not activate a mode directive.

## Resolution before validation

The compiler first checks cancellation, a positive work limit and the original
source-length bounds. It owns the visible pattern by copying that word view.
Its one-token-per-unit ASCII grammar projection preserves every original index;
non-ASCII syntax values continue to come from the owned word source.

The shared prefix resolver now has an explicit native width. The byte profile
retains its existing directive set and UTF8 alias. The word profile admits the
three mode directives alongside newline and BSR, retaining the other global
policy gates. The resolver keeps the complete original source and a body offset.

The selected mode validates the complete owned source before grammar parsing.
Malformed UTF inside comments, quotes, erased repeats, uncalled definitions and
accepted tails remains an InvalidUtf16Pattern error at its original code-unit
offset. An earlier cancellation, invalid work limit or source-length limit keeps
its established priority. Syntax and admission errors retain original positions
after the prefix, including supplementary source characters.

Parsing uses the resolved UTF/UCP values. If the effective flags differ from
the caller's options, the compiler creates a new immutable options value for
matching and source analysis. The caller's object remains unchanged. Scalar
limits, classes, folding, boundaries, references, graphemes, script runs,
lookbehind navigation, validation and search therefore use the same mode.

## Execution and qualification

The matcher reads the original subject view and returns original-unit captures.
Source-selected UTF uses the existing strict validation window and rejects a
low-surrogate caller start. UCP alone retains the raw unit domain, including
isolated surrogate source and subjects. ASCII caller restrictions remain in
force until a lexical option overrides them.

`PcreWordModeDirectiveTests` compares complete captures and successful labels
with PCRE2-16 across four caller UTF/UCP combinations, prefix orderings, every
valid caller start, eight compiled policies and all 64 call masks. Cases cover
supplementary input, UCP classes and casing, lexical restrictions, references,
lookbehind, graphemes, script runs, extended classes and newline behavior.
Thirteen former rejection shapes now have explicit native-result cases. Their
old rejection fixtures retain the deferred NO_JIT policy.

Additional checks cover validation/error priority, source and subject views,
owned flags/names/results, concurrent reuse, exact and one-less work limits,
and cancellation at every observed match transition followed by reuse.
Shared source-prefix cases compare byte and word captures through the independent
coordinate mapper. The existing byte prefix suites and executable four-mode
inventory remain qualification gates for the shared resolver.

Eight new package protocols bring the word gate to 82: five native-result
cases and three controlled diagnostics. All earlier package records and controls
remain required. PCRE.NET 1.6.0 with PCRE2 10.48 remains the reference; see the
[oracle provenance](../tests/Pcre/Native/README.md).

CASELESS_RESTRICT, NOTEMPTY/ATSTART, NO_JIT, optimizer policies and Turkish casing
remain separate word global-policy increments. Native LIMIT directives, tolerant
UTF16, NO_UTF_CHECK, UTF code-unit execution and callouts also remain deferred.
The word admission/resource caps and public delivery gates are unchanged.
See the [inventory](PCRE-FOUR-MODE-ADMISSION.md) and
[roadmap](PCRE-ROADMAP-STATUS.md).
