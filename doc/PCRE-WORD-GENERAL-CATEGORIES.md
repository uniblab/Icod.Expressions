# Native 16-bit Unicode general categories

The internal word sequence compiler admits Unicode general category groups
(`\p{L}`, `\p{N}`, `\p{Z}` and the other shared group names), specific
categories (`\p{Lu}`, `\p{Nd}`, `\p{So}` and their peers), the `L&` cased
letter group, and `\P` complements. They work as standalone atoms or
bounded bracket members. An inner predicate complement and outer bracket
negation retain their separate meanings. The shared parser supplies the
category value, and the matcher passes it to the pinned Unicode 17 table.

In raw mode each 16-bit unit has its category: an isolated surrogate can
match `\p{Cs}`, while a supplementary character is two raw units. Strict
UTF-16 mode decodes supplementary pairs as one scalar and validates the
subject suffix before matching. All starts and group-zero spans remain in
original 16-bit code units. Four valid-Unicode fixtures also compare
managed UTF-8 and UTF-16 capture coordinates.

The existing bracket bound admits up to 64 combined ranges and predicates.
A standalone or bracket predicate consumes one match step after candidate
admission, with bounded table lookup inside that step. The following
[script increment](PCRE-WORD-SCRIPTS.md) adds bounded script predicates.
Binary properties, Unicode-aware shorthand/POSIX classes, caseless folding,
ordered branches and captures remain follow-on work.

## Evidence

Forty-seven source/subject/start fixtures cover category groups, specific
categories, `\p` and `\P`, literal/predicate unions, two levels of
negation, supplementary characters, raw surrogate units and nonzero raw
starts. Six compiled defaults and 64 call masks yield 18,048 live native
PCRE2-16 comparisons. An additional 38 category names compare their
representative members in both raw and strict modes. Exact quota and
64/65 member controls exercise bounded work and admission.

The independent preflight used the ordinary-regex oracle and pinned
PCRE2 10.48/SLJIT revisions recorded in the [word sequence
record](PCRE-WORD-SEQUENCES.md). Every row JIT compiled with positive code
size. It produced 12,752 matches and 5,296 misses, with no native errors.
The LF-terminated policy output SHA-256 was:

`06bce957b1f3c2a1d20d4362e7497fe93d6d73a9148a09a39010f272347a2d3f`
