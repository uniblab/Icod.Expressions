# PCRE UTF-8 invalid-subject design

## Intent and constraints

Grep compiles `-P` with PCRE2 `UTF`, `UCP`, `MATCH_INVALID_UTF` and `ASCII_BSD` in its UTF-8 locale. Preserve matches and captures as offsets into the original bytes, while allowing a valid UTF-8 island of a binary record to match. This remains an internal matcher milestone, not a Grep migration or a public package API.

The strict `Utf8` option continues to reject the first malformed subject sequence. Patterns must remain valid UTF-8 even when the new match option is enabled. The byte matcher and its resource/cancellation policy remain intact. No replacement character may match a malformed sequence.

## Approaches

- **Chosen:** Scan once for maximal valid UTF-8 fragments, retain their source offsets, and run the same bounded bytecode candidate search within each fragment. Empty fragments and their edge assertions are valid. This directly expresses the PCRE2 barrier rule and prevents literal, dot, negated class, repetition and backreference from crossing an invalid run.
- Decode with replacement characters: rejected because it permits false positive negated matches and changes byte spans.
- Search the original buffer and patch every consumer separately: rejected because repeated constructs and backreferences could still cross a malformed sequence.

## Match rules

`PcreByteOptions.MatchInvalidUtf` implies UTF-8, as PCRE2's option does. Valid pattern checking always runs. Fragments include one zero-length edge at a malformed prefix and an edge at the subject end after trailing invalid bytes. After valid text the preceding fragment's end is the edge of the next invalid run. Search candidates consume only valid scalar boundaries within fragments; an empty pattern can instead match at the requested byte offset, even inside a valid scalar or malformed run. Zero-width assertions can observe the start of an invalid run, but later malformed bytes in that run are skipped. Strict UTF retains its existing `InvalidStartOffset` diagnostic.

Consuming instructions cannot move beyond a fragment end. `\b`/`\B` classify outside each fragment as nonword. `\z` and `\Z` require the whole-subject end (or the final LF position for `\Z`), while `\A`, `^`, and `$` keep their source or line meanings. Captures remain absolute source-byte spans. The work budget includes the subject scan and search; cancellation is checked during both. The original fragment-edge `\z`/`\Z` rule came from the older local PCRE2 10.42 probe; the pinned PCRE.NET 1.6.0 / PCRE2 10.48 offset matrix corrects it.

This milestone covers supported syntax only. Unicode-version/case-fold conformance, arbitrary lookaround and the full malformed-sequence offset matrix against Grep's pinned PCRE2 10.48 remain release gates. The local reference probe used PCRE2 10.42; fixtures assert behaviors described in PCRE2's Unicode manual and observed in that probe, without claiming version-wide parity.

Reference: https://www.pcre.org/current/doc/html/pcre2unicode.html#SEC8
