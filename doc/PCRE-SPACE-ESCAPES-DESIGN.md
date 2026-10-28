# Horizontal and vertical whitespace escapes

**Scope:** issue #7 internal PCRE conformance; no public API or consumer migration.

## Intent and approach

Implement `\h`, `\H`, `\v`, and `\V` as consuming character classes in ordinary expressions and brackets. PCRE2 defines fixed scalar lists for these escapes, independently of UTF/UCP, caseless options, locale tables, and newline convention. Byte mode uses only values below 256. Each uppercase form complements its corresponding lowercase form; no form consumes end-of-subject or invalid UTF bytes.

Use the existing byte-set and UTF scalar-class nodes and VM opcodes. Share the fixed membership predicates between byte-set construction and scalar evaluation, including the existing UCP POSIX `blank` predicate, whose list equals `\h`. Reusing `\s` or .NET whitespace/category classification was rejected because it would introduce different membership and UCP/version dependencies. A new matcher or instruction is unnecessary.

## Fixed membership

- Horizontal: U+0009, U+0020, U+00A0, U+1680, U+180E, U+2000–U+200A, U+202F, U+205F, U+3000.
- Vertical: U+000A–U+000D, U+0085, U+2028, U+2029.

These are character classes: `\v` consumes CR and LF separately, rather than treating CRLF as the indivisible newline sequence matched by the pending `\R` family. The unanchored CRLF candidate policy must still agree with the pinned reference.

## Boundaries and evidence

Preserve original byte spans, numbered captures, bracket union/complement rules, work/cancellation accounting and malformed-UTF fragment barriers. Class escapes are not valid range endpoints. `\N`, `\R`, `\X`, `\C` and broader assertions remain pending. Compare all 256 byte values and every listed Unicode member plus boundary/nonmember probes with PCRE.NET 1.6.0 / PCRE2 10.48; include UCP on/off, caseless, ASCII-digit policy, CRLF start offsets, repetition/captures and invalid-UTF cases. The PR records test-first and final six-platform CI evidence.
