# Native 16-bit caseless literals and classes

The bounded word sequence compiler now accepts caller `Caseless` and
`CaselessRestricted` and the shared parser's inline `(?i)`, `(?-i)` and
`(?r)` switches. Literal comparisons, bracket ranges and scalar predicates
execute on the original word subject. Strict UTF-16 and raw UCP use the
pinned Unicode 17 simple-fold table; raw 16-bit mode without UCP folds only
ASCII letters. Restricted folding prevents a non-ASCII character from
folding to ASCII. The scope excludes Turkish-specific compile settings.

The parser closes Unicode bracket ranges into folded keys. Raw UCP keeps
surrogate units when closing ranges, while strict UTF-16 omits invalid
scalars. Raw mode without UCP keeps original ranges and compares the ASCII
case counterpart at match time. Both the original member count and expanded
range/predicate count must fit the existing 64-member word bound, so one
comparison still costs one work step. Literal-mode compilation retains its
explicit literal contract and does not apply source inline options.

## Evidence

Fifty-two source/subject/start fixtures across six compiled defaults and
64 call masks compare 19,968 live outcomes with PCRE2-16. They cover ASCII,
Latin, Kelvin and long-s folding, restricted folding, brackets and Unicode
properties, inline option changes, raw surrogate units, supplementary
scalars, raw starts and both UCP settings. Five caller-caseless and two
restricted-option cases compare with native inline switches. Four valid
Unicode cases compare managed byte and word capture coordinates. Exact work
quotas and the 64/65 source-member boundary preserve bounded execution.

The independent preflight used the ordinary-regex oracle and pinned PCRE2
10.48/SLJIT revisions recorded in the [word sequence
record](PCRE-WORD-SEQUENCES.md). Every row JIT compiled with positive code
size. It produced 14,336 matches and 5,632 misses without native errors.
The LF-terminated output SHA-256 was:

`7c77a9768360f7fbf32a536e59033f0a08fd4736a3c9cb60551a6743b42b5e46`

Ordered branches and repetition, durable captures, expanded compile
settings and the broader P10–P12 release gates remain open.
