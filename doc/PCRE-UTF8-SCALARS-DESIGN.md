# Internal PCRE UTF-8 scalar foundation

## Intent

Add an opt-in internal UTF-8 mode that consumes whole Unicode scalar values for ordinary literals and dot while retaining byte offsets and captures. This is a step toward Grep's C.UTF-8 `-P` path; it is not a consumer migration or a public PCRE API.

## Approaches

| Approach | Tradeoff | Decision |
|---|---|---|
| Decode the entire subject into a character array | Simpler instruction execution but requires a second offset map and a potentially large allocation per record. | Defer. |
| Keep source bytes and decode one scalar at each instruction | Preserves byte spans and uses bounded temporary space; requires validation and boundary-aware candidate starts. | Choose. |
| Treat UTF-8 as several independent bytes | Small change but makes dot, character starts and captures disagree with PCRE2. | Reject. |

## Scope and constraints

- `PcreByteOptions.Utf8` opts in. Default 8-bit byte mode, including high bytes, remains unchanged.
- Validate the complete pattern at compile time and the complete subject at match time, reporting the first invalid byte offset. Reject a start offset inside a UTF-8 sequence. No replacement-character decoding or match-invalid-UTF policy in this step.
- Raw UTF-8 literals, ASCII/control/one-or-two-digit `\x` escapes, dot, anchors, groups, alternatives, repetition, backreferences, and ASCII word boundaries may use the existing ordered bytecode. Dot consumes one scalar and excludes LF unless dot-all. Capture and match offsets remain byte offsets.
- Keep classes, POSIX classes, shorthands, literal lookarounds and caseless matching explicitly unsupported in UTF mode until Unicode and option semantics are designed. An unsupported construct must not silently behave as a byte operation.
- Pattern validation is cancellable; subject validation and matching share the internal per-match work budget and cancellation checks. No native or .NET regex engine at runtime. Continue the six Staging CI jobs and leave Grep and CommandFramework untouched.

## Follow-on

Unicode properties, UCP POSIX/shorthand behavior (including Grep's ASCII `\d` policy), case folding, lookaround and `PCRE2_MATCH_INVALID_UTF` need separate conformance work before Grep migration.
