# Native 16-bit Unicode script properties

The internal word sequence compiler admits pinned Unicode 17 script
properties. `\p{sc=Greek}` requests primary Greek script; `\p{scx=Greek}`
requests Greek script extensions; the bare `\p{Greek}` form follows the
shared parser's extension semantics. `\P` complements and negated bracket
members retain their separate meanings. The matcher passes the parsed
script identifier and the primary/extension flag to the shared table.

Raw mode compares individual 16-bit units, so a supplementary script
character occupies two raw units. Strict UTF-16 mode reads it as one scalar
and reports its original two-unit group-zero span. Subject starts,
diagnostics, policy, validation and cancellation retain the existing word
contract. Bracket sets admit at most 64 combined ranges and predicates;
one standalone or bracket script comparison consumes one match work step
with a bounded table lookup.

Binary properties, Unicode-aware shorthand/POSIX classes, caseless word
matching, ordered branches and captures remain follow-on work.

## Evidence

Forty-one source/subject/start fixtures cover primary and extended Greek,
Hiragana, Latin and Adlam membership, complements, bracket unions and
negations, shared punctuation, combining marks, supplementary strict
scalars and raw surrogate starts. Six compiled defaults and 64 call masks
yield 15,744 live PCRE2-16 comparisons. A separate sweep compares 175
pinned script names in primary and extended forms over both raw and strict
subjects (700 further native outcomes). Four valid-Unicode fixtures compare
managed UTF-8 and UTF-16 captures. Work quota and 64/65 bracket controls
verify bounded admission.

The independent preflight used the ordinary-regex oracle and pinned
PCRE2 10.48/SLJIT revisions recorded in the [word sequence
record](PCRE-WORD-SEQUENCES.md). Every row JIT compiled with positive code
size. The policy matrix produced 11,856 matches and 3,888 misses, with no
native errors. Its LF-terminated output SHA-256 was:

`dcd97edb7a4e340ed5c36c1f522bf55870467f28192d2f7da78c917c193702fe`
