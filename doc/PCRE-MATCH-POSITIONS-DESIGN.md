# PCRE search and reported match positions

**Scope:** issue #7 internal conformance; no public API, release or consumer migration.

## Semantics

`\G` succeeds only when the current input position equals the original `Match` start offset. It is independent of subject-start and multiline anchors and does not move as unanchored search tries later candidates. Alternatives without `\G` may still match later. `\K` resets the reported whole-match start to the current input position without consuming a character or changing anchoring. Numbered captures before the reset retain their original source-byte spans, even when outside the reported group-zero span. Later resets win on the successful path; abandoned paths must restore their prior reported start.

Both forms are zero-width and cannot be directly quantified or used inside brackets. Enclosing groups may repeat, subject to the existing progress/limit rules. PCRE2 forbids `\K` in lookarounds by default; reject it in the existing literal assertion parser. `\G` inside a general assertion body remains unsupported. Quotes continue to treat these escapes literally. UTF matching uses authoritative byte indexes, and strict/tolerant invalid-input and start-offset rules remain in effect.

The official [PCRE2 pattern specification](https://www.pcre.org/current/doc/html/pcre2pattern.html), resetting the match start and simple assertions sections, defines these rules. Tests pin PCRE.NET 1.6.0 / PCRE2 10.48, including empty matches at malformed-input offsets.

## Implementation approach

Add semantic search-start and match-start-reset nodes. Lower `\G` to a search-start instruction that compares the original start offset. Lower `\K` to the existing capture-start instruction for reserved slot zero. Initialize that slot to each candidate start and have Accept use it for both the match properties and capture zero. Existing journal checkpoints then restore resets with other capture-start mutations. Frames still store integers/enums; no spans or interior pointers are retained.

A separate reported-start field in every frame would duplicate the journal and widen saved paths. Cloning capture state for each reset would add unnecessary allocations. Reusing capture-start slot zero keeps rollback in one mechanism. Existing matches without `\K` keep their original spans and work costs. No public capture-name or iteration API is added.

## Verification

Compare search offsets, multiple resets, captures before group zero, branch/atomic/repetition rollback, greedy/lazy/possessive paths, byte/UTF/NUL spans, quoting and every offset around valid/malformed UTF and CRLF. Pin invalid syntax and existing budget/cancellation behavior. The PR records test-first and six-platform Staging results.
