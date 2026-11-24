# Native 16-bit binary and bidi properties

The internal word sequence binding admits all 57 pinned Unicode 17 binary
properties and all 23 bidi classes recognized by the shared PCRE parser.
For example, `\p{Emoji}`, `\P{White_Space}`, `\p{BC=R}` and
`\p{Bidi_Class=NSM}` work as standalone atoms and as members of bounded
bracket sets. The parser supplies the property identifier to the existing
shared scalar membership table. Inner predicate complementation and outer
bracket negation retain their separate meanings.

Raw matching tests individual 16-bit code units, including surrogates.
Strict UTF-16 matching reads scalars, including supplementary pairs, and
reports group-zero captures in original code units. The word compiler's
64-member bracket bound and one-step predicate work accounting still apply.
Unrecognized or nonbinary special properties, including `\p{XUC}`, remain
outside this word sequence profile.

## Evidence

Fifty-one source/subject/start fixtures cover binary aliases, bidi short and
long syntax, positive and negative properties, unions and nested negations,
raw surrogate starts, strict supplementary scalars and straight sequences.
Six compiled defaults and 64 call masks yield 19,584 live PCRE2-16 outcomes.
An additional sweep compares a representative member of each of the 57
binary and 23 bidi properties in both modes (160 native outcomes). Four
valid-Unicode cases also compare managed UTF-8 and UTF-16 coordinates.
Exact work quota and 64/65 bracket controls verify bounded execution.

The independent preflight used the ordinary-regex oracle and pinned PCRE2
10.48/SLJIT revisions recorded in the [word sequence
record](PCRE-WORD-SEQUENCES.md). All matrix rows JIT compiled with positive
code size. The policy matrix produced 14,608 matches and 4,976 misses,
without native errors. Its LF-terminated output SHA-256 was:

`13dcdc3c03d3b02c9d11c3e7bd950b9431d0d95aa6f31426ccf8e7b3dab7e978`

The 80-name sweep produced 158 matches and two misses, without native
errors. Its LF-terminated output SHA-256 was:

`4d28cd67e9553326e9d63826cdfa625aab46e02bc6744723e8c6f9d50556a656`

The following [UCP class increment](PCRE-WORD-UCP-CLASSES.md) admits
Unicode-aware shorthand/POSIX predicates. Caseless word matching, ordered
branches, repetition, captures and the broader P10–P12 gates remain open.
