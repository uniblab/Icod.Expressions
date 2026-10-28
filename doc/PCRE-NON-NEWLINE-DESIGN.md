# PCRE non-newline and Unicode codepoint escapes

**Scope:** issue #7 internal conformance. No public API, release or consumer migration.

## Semantics

Bare `\N` consumes one character that is not a newline under the current LF or AnyCRLF convention. DotAll never changes it; UCP, caseless and BSR settings do not change membership. Byte mode consumes one byte and UTF mode one complete valid scalar. A malformed UTF byte is a fragment barrier, and end-of-subject cannot be consumed.

In UTF mode, `\N{U+hex}` is a single literal codepoint, equivalent to the existing braced hex literal. It can occur in a bracket/range or fixed literal assertion, and can explicitly represent a newline. Spaces/tabs may follow the opening brace or precede the closing brace, but may not occur between `U+` and the hex digits. Reject non-UTF codepoint forms, surrogates, out-of-range values and character names.

PCRE2 also accepts counted repetition such as `\N{2,3}`. After an optional space/tab prefix following `{`, only `U+` selects the codepoint form; other braced content must be a valid counted quantifier. Bare `\N`, including counted forms, is invalid in brackets. Bare `\N` in a general lookaround body remains outside the fixed literal assertion subset.

The semantic sources are the official [PCRE2 pattern specification](https://www.pcre.org/current/doc/html/pcre2pattern.html) and its [escape disambiguation](https://github.com/PCRE2Project/pcre2/blob/be940c4eaec0bc4117e06e8f14e56bf82089be6b/src/pcre2_compile.c#L1707-L1770). Executable tests pin PCRE.NET 1.6.0 / PCRE2 10.48.

## Approach

Reuse the existing dot nodes with DotAll forced off for bare `\N`. Reuse the bounded braced-hex value reader and literal node for `\N{U+...}`. Validate an apparent counted form with the existing count reader, then leave its bounds for the normal quantifier path. A separate VM opcode or a textual pattern rewrite would duplicate existing work and make byte diagnostics harder to preserve.

Preserve source-byte spans, capture journaling, candidate-search metadata, compiler limits and matching work/cancellation checks. Keep `\X`, `\C`, `\G`, `\K`, general assertions and remaining option/directive families pending. The PR records test-first and final six-platform evidence.
