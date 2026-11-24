# Native 16-bit fixed whitespace classes

The internal word sequence compiler admits `\h`, `\H`, `\v` and `\V` as
standalone atoms and as bracket predicates. It reuses the shared PCRE2 fixed
whitespace lists through `PcreScalarClassMembership`, independent of UCP and
Unicode categories. In particular, ASCII `\s` keeps its separate default
membership; `\h` includes non-ASCII horizontal spaces such as U+00A0 and
U+3000, and `\v` includes U+0085 and U+2028.

Raw matching compares one 16-bit unit, including isolated surrogates.
Strict UTF-16 matching decodes one scalar, so a negated class can consume
a supplementary pair as one match and still report a two-unit group-zero
span. Original 16-bit start and diagnostic coordinates remain authoritative.

The existing limit admits no more than 64 combined ranges and predicates
per bracket set. A standalone or bracket whitespace comparison consumes one
match work step; this finite set bounds the work within the step. The
following [general category increment](PCRE-WORD-GENERAL-CATEGORIES.md)
adds bounded category predicates. [Script predicates](PCRE-WORD-SCRIPTS.md)
follow; UCP shorthand, binary properties,
caseless word matching, branches and captures remain separate roadmap work.

## Evidence

Forty-one source/subject/start fixtures exercise all four forms, non-ASCII
whitespace, excluded near-neighbors, outer and inner negation, mixed
literal/predicate classes, raw surrogate starts and supplementary strict
UTF-16 subjects. Six compiled defaults and 64 call masks yield 15,744
live PCRE2-16 comparisons. Four valid-Unicode patterns also compare
managed UTF-8 and UTF-16 captures. Exact work quota and 64/65 member
controls check the bounded execution contract.

The independent preflight used the regular-expression oracle and pinned
PCRE2 10.48/SLJIT revisions recorded in the [word sequence
record](PCRE-WORD-SEQUENCES.md). All rows JIT compiled with positive code
size. It produced 9,424 matches and 6,320 misses, with no native errors.
The LF-terminated output SHA-256 was:

`023137a630a089e18566a362f6854fb1bb21cf5277c7e379f0deba9e362fe02d`
