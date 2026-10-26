# UTF-8 literal bracket design

**Status:** internal PCRE syntax slice under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7).

## Intent and boundary

Match ordinary case-sensitive UTF-8 bracket sets and scalar ranges over the original subject bytes. A class consumes exactly one valid UTF-8 scalar. Preserve the already implemented standalone POSIX digit, word, and space brackets. This milestone does not expose a public PCRE API, switch Grep, or modify CommandFramework production code.

Support literal members, leading `^` negation, raw UTF-8 members, single-scalar escapes already recognized outside a class (`\xFF`, controls, escaped punctuation), `\b` as backspace, inclusive ranges, first-position `]`, and literal hyphens at the first or last position or following a range. A hyphen can itself be a range endpoint, as in `[--0]`. `[` is literal unless it starts a complete POSIX, collating, or equivalence token; an incomplete prefix such as `[[.]` is an ordinary literal set. Treat a complete mixed POSIX class, shorthand/property escape, or unsupported escape in a UTF bracket as `UnsupportedSyntax` at the opening bracket. Reject malformed or descending ranges and missing closing brackets diagnostically. UTF caseless remains wholly unsupported.

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Decode the entire subject to `string` and use .NET regex classes | Loses authoritative byte spans and malformed-subject barriers. | Reject. |
| Expand scalar sets into alternations of literals | Minimal matcher change, but ranges could explode in code size and work accounting. | Reject. |
| Store inclusive scalar ranges in an immutable instruction and evaluate the next decoded scalar | Bounded parser and matcher change with exact source-byte spans, and leaves room for future union predicates. | Choose. |

## Semantics and verification

Parse only valid scalar values; compile entry already validates the pattern. The matcher checks the current valid fragment before decoding, charges each attempted interval inspection to the work budget, and advances by the scalar's UTF-8 width. Negation complements the scalar set within valid fragments, never matching malformed bytes or end of input. The byte-mode set implementation is unchanged. Cases include BMP and supplementary characters, `\xFF` as U+00FF, range edges, reversed ranges, metacharacters, first `]`, hyphen placement, quantifiers, capture rollback, invalid UTF barriers, nonzero start offsets, work limits, and cancellation. Compare selected fixtures with PCRE2. The syntax decisions follow the [PCRE2 square-bracket reference](https://www.pcre.org/current/doc/html/pcre2pattern.html); pinned PCRE2 10.48 conformance remains a separate release gate.

UTF bracket unions with POSIX, `\d`/`\w`/`\s`, `\p{...}`, caseless folding, and nested/extended classes remain separate milestones. No unsupported construct is silently interpreted bytewise.
