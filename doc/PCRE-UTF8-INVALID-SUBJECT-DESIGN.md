# PCRE UTF-8 invalid-subject design

## Intent and constraints

Grep compiles `-P` with PCRE2 `UTF`, `UCP`, `MATCH_INVALID_UTF` and `ASCII_BSD` in its UTF-8 locale. Preserve matches and captures as offsets into the original bytes, while allowing a valid UTF-8 island of a binary record to match. This remains an internal matcher milestone, not a Grep migration or a public package API.

The strict `Utf8` option continues to reject the first malformed subject sequence. Patterns must remain valid UTF-8 even when the new match option is enabled. The byte matcher and its resource/cancellation policy remain intact. No replacement character may match a malformed sequence.

## Approaches

- **Chosen:** Scan once for maximal valid UTF-8 fragments, retain their source offsets, and run the same bounded bytecode candidate search within each fragment. Empty fragments and their edge assertions are valid. This directly expresses the PCRE2 barrier rule and prevents literal, dot, negated class, repetition and backreference from crossing an invalid run.
- Decode with replacement characters: rejected because it permits false positive negated matches and changes byte spans.
- Search the original buffer and patch every consumer separately: rejected because repeated constructs and backreferences could still cross a malformed sequence.

## Match rules

`PcreByteOptions.MatchInvalidUtf` implies UTF-8, as PCRE2's option does. Valid pattern checking always runs. Fragments include zero-length regions at malformed leading bytes and the end of the subject. A stray continuation byte cannot itself start a fragment; after valid text its preceding edge remains visible to a search begun earlier. Search candidates are scalar boundaries within a fragment, including its end, and the first eligible candidate is chosen in source order. An offset inside a valid multibyte scalar advances to the next boundary under this option; strict UTF retains its existing `InvalidStartOffset` diagnostic. A search starting on a stray continuation skips that byte, whereas a search starting on a malformed leading byte can observe the zero-width edge there.

Consuming instructions cannot move beyond a fragment end. `\b`/`\B` classify outside each fragment as nonword. `\z` and `\Z` see its end, while `\A`, `^`, and `$` keep their source or line meanings. Captures remain absolute source-byte spans. The work budget includes the subject scan and search; cancellation is checked during both.

This milestone covers supported syntax only. Unicode-version/case-fold conformance, arbitrary lookaround and the full malformed-sequence offset matrix against Grep's pinned PCRE2 10.48 remain release gates. The local reference probe used PCRE2 10.42; fixtures assert behaviors described in PCRE2's Unicode manual and observed in that probe, without claiming version-wide parity.

Reference: https://www.pcre.org/current/doc/html/pcre2unicode.html#SEC8
