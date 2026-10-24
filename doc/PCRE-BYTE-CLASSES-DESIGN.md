# PCRE byte-mode bracket classes

**Status:** internal syntax slice stacked on [byte atoms](https://github.com/uniblab/Icod.Expressions/pull/9), under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7). No Grep or CommandFramework production changes.

## Intent and contract

Add PCRE2-style ordinary bracket classes to the in-house 8-bit non-UTF matcher. A class consumes exactly one source byte and reuses the immutable 256-bit set and class opcode from the prior slice. This work supports Grep's existing `[[:digit:]]` spelling in byte mode. UTF/UCP POSIX behavior, caseless expansion, extended set algebra and locale options remain separate milestones.

Support literal class items, `^` negation at the start, inclusive `x-y` ranges, escaped punctuation and existing single-byte control/hex escapes, ASCII `\d`/`\s`/`\w` and complements, and ordinary POSIX `[:name:]` / `[:^name:]` items within `[...]`. The supported names are `alnum`, `alpha`, `ascii`, `blank`, `cntrl`, `digit`, `graph`, `lower`, `print`, `punct`, `space`, `upper`, `word`, and `xdigit`, with ASCII non-UCP definitions. A literal `]` may be the first class item or be escaped; `-` is literal first, last, or escaped. A class negated with `^` can match LF and high bytes. The parser must not consume subject bytes during compilation.

An unclosed class or descending range is `InvalidSyntax` at the class/range byte. A hyphen joining a character type or POSIX item to another element is invalid unless it is the last literal item. Unknown POSIX names, collating/equivalence forms, Unicode properties and unsupported escapes fail explicitly; no silent literalization or fallback. A closing `]` outside a class is a literal in PCRE2's default mode.

## Approach

| Approach | Tradeoff | Decision |
|---|---|---|
| Reuse GNU bracket parsing | Shares familiar syntax, but couples POSIX/GNU selection and locale behavior to a separate PCRE engine. | Reject. |
| Compile class items and POSIX sets to one byte set | Fixed matching cost, natural union/complement and reuse of the existing class opcode; parser must distinguish ranges and POSIX tokens carefully. | Choose. |
| Add a predicate interpreter per class item | Easier parser, but more work on every byte and more state in the backtracking VM. | Reject. |

The parser builds one set from class elements; class negation complements the final union. `PcreByteSet` gains immutable singleton, inclusive range and union operations. Existing `ParseEscape` remains authoritative for escapes outside classes; inside classes, `\b` means byte 0x08, while outside it remains pending word-boundary syntax. Each parse step checks cancellation. Matching and work accounting are unchanged because every class uses the existing byte-set instruction.

## Verification and boundaries

Pin ordinary ranges, `]` and `-` placement, negation including LF/NUL/0xFF, escaped endpoints, shorthand union, invalid ranges, and POSIX ASCII sets to PCRE2 10.48 byte-mode behavior. Check captures and greedy backtracking through classes, a small work budget, and pattern snapshot isolation. The frozen GNU 214-case corpus, full .NET 8/9/10 suite on Linux/Windows/macOS, and single-library package-only smoke must pass. The reference engine is a fixture oracle only, never a runtime dependency. See [PCRE2 character-class specification](https://www.pcre.org/current/doc/html/pcre2pattern.html#SEC9) and [syntax summary](https://www.pcre.org/current/doc/html/pcre2syntax.html).

The public package remains the existing GNU BRE/ERE alpha. Do not migrate Grep, publish a PCRE-capable version, or remove CommandFramework regex code from this slice.
