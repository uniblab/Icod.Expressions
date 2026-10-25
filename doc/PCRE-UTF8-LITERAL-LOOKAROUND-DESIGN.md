# UTF-8 fixed literal lookaround design

## Intent and boundary

Enable the already supported fixed literal `(?=...)`, `(?!...)`, `(?<=...)`, and `(?<!...)` syntax in internal UTF-8 mode. This includes Grep's `(?<=foo)bar` fixture, UTF-8 literals, and `\xFF` as Unicode U+00FF. Matches and captures keep authoritative source-byte spans. A lookaround cannot inspect across a malformed UTF-8 barrier when `MatchInvalidUtf` is enabled.

No public PCRE API, package claim or Grep migration is part of this slice. Byte-mode behavior and strict invalid-subject diagnostics remain unchanged. Unicode caseless and nonliteral assertion bodies remain separate gates.

## Approaches

- **Chosen:** Reuse the parser's literal-only AST and bytecode instruction. Validate the UTF-8 pattern at compile entry; preserve raw literal UTF-8 bytes and encode scalar escapes into UTF-8. Compare the resulting exact bytes only inside the match's valid fragment, with the existing work and cancellation accounting. A valid literal's encoded bytes cannot match an interior continuation-byte position.
- Run a nested matcher for every assertion: unnecessary for a fixed literal and adds backtracking/capture semantics outside this tranche.
- Decode the subject into a replacement string: changes byte offsets and could let assertions see through invalid bytes.

## Semantics and checks

Positive and negative lookahead compare the literal at the current position; lookbehind compares the exact preceding byte length. Empty literal assertions retain existing zero-width behavior. Positive assertions fail at a short fragment edge; negative assertions succeed there. The assertion itself consumes no bytes and allocates no capture. In UTF mode, `\xFF` denotes U+00FF and becomes the UTF-8 sequence C3 BF inside the assertion, just as it does as a consuming literal.

Retain the existing literal-only syntax diagnostics for classes, properties, nested groups, alternatives, repetition, backreferences and unsupported escapes. Test both directions and polarities, multibyte and escaped code points, capture rollback, start offsets, strict/invalid subject policies, work limits, and cancellation. Compare fixtures with a PCRE2 oracle; local reference is 10.42, while pinned 10.48 conformance remains a release gate.

Reference: https://www.pcre.org/current/doc/html/pcre2pattern.html#SEC28
