# UTF-8 bracket predicate unions

**Status:** internal PCRE syntax slice under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7).

## Intent

Combine the implemented UTF-8 literal scalar ranges with existing `\d`/`\D`, `\w`/`\W`, `\s`/`\S`, `\p{L}`/`\P{L}`, `\p{Nd}`/`\P{Nd}`, and POSIX digit/word/space predicates inside ordinary brackets. Each member is an OR term; leading `^` negates the complete union. Preserve Grep's `AsciiBsD` restriction on `\d`, not on POSIX `[[:digit:]]` or explicit `\p{Nd}`. Byte mode and the public package API are unchanged; Grep and CommandFramework production code remain untouched.

Support inner POSIX negation (`[:^digit:]`, `[:^word:]`, `[:^space:]`) as a predicate-term complement, independently of outer negation. Standalone POSIX brackets use the same parser, retain scalar spans, and preserve current UCP behavior. A shorthand, property, or POSIX predicate cannot be an endpoint of a range. Unsupported POSIX names, collating/equivalence tokens, property names, escape kinds, and option scopes remain explicit compile diagnostics.

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Expand Unicode predicates to code-point intervals at compile time | Large, version-sensitive tables and code size. | Reject. |
| Add one alternation for every bracket member | Changes backtracking shape and capture work, and makes negation awkward. | Reject. |
| Extend the scalar-set operand with predicate terms | Reuses the matcher and its byte spans, supports union/complement cleanly, and charges each inspected term to the work budget. | Choose. |

## Parsing and matching

Read each UTF bracket item as either one literal scalar or one predicate with a kind and local negation. A literal followed by `-` and another literal forms an inclusive scalar range. Preserve the existing first `]`, hyphen range and incomplete-special-token handling. A complete `[:name:]` token becomes a predicate item; a complete unsupported special token is rejected. At match time, test literal intervals first, then predicate terms until one succeeds, count every attempted term, apply outer negation once, and advance exactly one valid UTF scalar inside the current fragment. An invalid byte or EOF never matches, even through double negation.

Test UCP on/off, Grep's ASCII `\d` policy and `(?-aD)` override, inner/outer negation, POSIX mixed with ranges, Unicode properties, malformed barriers, start offsets, captures, failed alternatives, invalid range endpoints, work and cancellation. Compare selected cases against [PCRE2 character-class semantics](https://www.pcre.org/current/doc/html/pcre2pattern.html). Pinned PCRE2 10.48 version-wide conformance, Unicode caseless, and additional POSIX names remain separate gates.
