# Native 16-bit literal character classes

The internal word sequence compiler now admits a bounded subset of bracket
classes through the shared PCRE parser's scalar-range node. It accepts
case-sensitive literal members, escaped literal members, ordered ranges and
negation. The parser reads direct non-ASCII source from its owned word pattern.
Raw mode treats each 16-bit unit as a separate class member; strict UTF-16
mode reads whole scalars, including supplementary characters. Match offsets
remain in the subject's original 16-bit units.

The word matcher checks at most 64 source ranges per class attempt. A larger
range list is explicitly unsupported at compilation, keeping work per match
step bounded. The following [ASCII predicate increment](PCRE-WORD-ASCII-CLASSES.md)
adds bounded shorthand and POSIX members. The [general category
increment](PCRE-WORD-GENERAL-CATEGORIES.md) admits bounded category
predicates. Caseless classes remain unsupported until word-width folding
uses the pinned Unicode data. Class membership consumes one match work step,
with the existing candidate, validation, policy and cancellation costs.

## Evidence

Twenty-two source/subject/start fixtures exercise positive and negated
classes, ASCII and BMP ranges, numeric escapes, supplementary UTF-16 scalars,
raw surrogate units, line breaks and nonzero starts.
Six compiled defaults and all 64 call masks yield 8,448 live native PCRE2-16
comparisons. Four valid-Unicode patterns also compare the managed byte and
word scalar results. Exact work quota and 64/65-range admission controls
check the bounded execution contract.

An independent preflight used the ordinary-regex oracle and the pinned
PCRE2 10.48/SLJIT revisions recorded in the [word sequence
record](PCRE-WORD-SEQUENCES.md). In fixture/default/mask order, it produced
4,160 matches and 4,288 misses, with no native errors and successful JIT
compilation for each row. The LF-terminated output SHA-256 was:

`e7a7986c55bf1b7dc267e17a6d1c1001b226bc12f5582f5a626a4381f0d3abd7`

Binary properties and Unicode-aware POSIX classes, word-width casing, class algebra,
alternation, repetition, captures and cross-width release qualification
remain separate roadmap work.
