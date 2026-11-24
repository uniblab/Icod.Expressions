# Native 16-bit ASCII shorthand and POSIX classes

The internal word sequence binding now admits `\d`, `\D`, `\w`, `\W`, `\s`
and `\S` as standalone atoms and bracket members. It also admits the
default ASCII POSIX predicates (`[:digit:]`, `[:alpha:]`, `[:word:]` and the
other ASCII names recognized by the shared parser). A negated POSIX member
and a negated outer bracket retain their separate meanings. Mixed literal
ranges and predicates retain the shared parser's source order and matching
semantics.

The native word binding preserves ASCII class membership in both raw and
strict UTF-16 modes. Raw mode consumes one 16-bit unit, including isolated
surrogates; strict mode consumes a Unicode scalar. A non-ASCII scalar does
not become an ASCII class member. The existing word matcher reports original
16-bit offsets and group zero.

Standalone predicates consume one match step. Bracket classes admit at most
64 combined ranges and predicates, and one class attempt consumes one step
after candidate admission. This bounds comparison work within a step.
The following [UCP class increment](PCRE-WORD-UCP-CLASSES.md) selects
Unicode membership and ASCII restrictions. Caseless word classes remain
unsupported in this binding. Following increments admit [fixed whitespace](PCRE-WORD-FIXED-SPACE.md)
and [Unicode general categories](PCRE-WORD-GENERAL-CATEGORIES.md). A further
[script increment](PCRE-WORD-SCRIPTS.md) covers primary and extended scripts;
the [binary and bidi increment](PCRE-WORD-BINARY-BIDI.md) admits its pinned
properties. The UCP class increment adds Unicode-aware shorthand/POSIX classes.

## Evidence

Thirty-six source/subject/start fixtures include all six shorthand forms,
positive and negated bracket predicates, mixed ranges, default ASCII POSIX
classes, BMP values, supplementary scalars, raw surrogate starts, and line
whitespace. Six compiled defaults and 64 call masks yield 13,824 live
PCRE2-16 comparisons. Four valid-Unicode fixtures also compare managed
UTF-8 and UTF-16 captures. Work quota and 64/65 member boundary checks
exercise the bounded contract.

The independent preflight used the regular-expression oracle from the
[word sequence record](PCRE-WORD-SEQUENCES.md), which requires successful
JIT compilation and positive code size for each row. It produced 5,472
matches and 8,352 misses, with no native errors. The LF-terminated output
SHA-256 was:

`7e70c252b9c232bb8d39dd2d2b835d439a9d595538fbe5168c1738c60da8f76b`

Word casing, ordered execution,
captures and cross-width release qualification remain roadmap work.
