# Internal PCRE UTF-8 whitespace and standalone POSIX word/space classes

## Intent

Complete the internal UTF-8 matcher’s `\s`/`\S` support and the standalone POSIX `digit`, `word`, and `space` bracket families. Grep’s `^(\w+)\s+\1$` fixture then has all its consuming atoms implemented, while Grep continues to use PCRE2 in production.

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Reuse byte whitespace sets in UTF mode | Splits non-ASCII scalars and misses UCP whitespace. | Reject. |
| Add scalar whitespace predicates and extend the existing standalone POSIX parser | Uses the current scalar opcode and preserves strict byte spans without introducing a general UTF bracket parser. | Choose. |
| Implement arbitrary UTF bracket unions/ranges now | Useful later, but needs distinct parser and set semantics well beyond these named classes. | Defer. |

## Scope

- In UTF mode without UCP, `\s` matches HT, LF, VT, FF, CR and ASCII space; `\S` matches any other valid scalar. Under UCP, `\s` additionally matches Unicode separators (Zs, Zl, Zp), U+0085 and U+180E. Negation consumes one entire scalar and never matches end-of-subject.
- The standalone bracket forms `[[:space:]]`, `[[:^space:]]`, `[[:word:]]`, `[[:^word:]]` and outer-negated variants use the corresponding scalar predicates. `[[:digit:]]` continues its independent UCP digit behavior. `AsciiBsD` restricts `\d` only; it does not change POSIX word/space or `\s`.
- Mixed UTF brackets, ranges, other POSIX names, scoped flags, `AsciiBsS`, and Unicode caseless remain explicit unsupported syntax. Existing 8-bit byte-mode behavior is unchanged.
- Preserve original byte offsets/captures, ordered backtracking, cancellation, resource limits, strict UTF validation, and runtime Unicode-version gate.

## Follow-on

PCRE2 `MatchInvalidUtf` has fragment, anchor, and zero-width rules; a separate design and oracle matrix must cover it before it is enabled. Full UTF bracket syntax and Grep migration remain later gates.
