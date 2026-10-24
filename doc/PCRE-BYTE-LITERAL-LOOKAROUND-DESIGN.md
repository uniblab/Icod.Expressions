# PCRE byte-mode fixed literal lookaround

**Status:** internal syntax slice under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7). No public API, package claim, or Grep migration.

## Intent and boundary

Support positive and negative zero-width assertions over a fixed byte literal: `(?=...)`, `(?!...)`, `(?<=...)`, and `(?<!...)`. In particular, `(?<=foo)bar` must find `bar` at byte 3 in `foobar`. The body may contain raw bytes (including NUL and high bytes) and existing literal escapes such as `\xFF` and `\n`. Empty bodies are valid. The assertion does not allocate a capture number, consume bytes, or alter an existing capture. Lookbehind examines the exact preceding bytes; lookahead examines the exact following bytes. A negative assertion succeeds when the positive comparison fails, including when insufficient bytes remain.

This is a literal-only subset. Classes, anchors, alternation, nested groups, repeats, backreferences, options, and other nonliteral bodies remain `UnsupportedSyntax` at the opening parenthesis. Unclosed assertions are `InvalidSyntax` at that parenthesis. Repetition of an assertion is diagnosed as unsupported. No assertion silently becomes a literal or invokes a runtime regex engine.

## Approach

The parser recognizes the four prefixes before allocating captures. It decodes literal escapes into an owned byte array and rejects every nonliteral node. One bytecode instruction records the bytes, direction, and polarity. The matcher compares them at the current byte position, charges each attempted byte against the existing work budget, observes cancellation, and then either advances the program counter at the same position or backtracks. Existing branch snapshots remain responsible for state rollback.

An unrestricted assertion subprogram would require capture propagation, atomic assertion boundaries, nested backtracking, and shared resource accounting. Keeping the first slice literal gives the consumer's fixed-prefix fixture an exact byte contract while leaving those semantics for a separate reviewed milestone. PCRE2's [assertion specification](https://www.pcre.org/current/doc/html/pcre2pattern.html) defines zero-width lookaround and its positive/negative directions.

## Verification

Test the Grep fixture, both polarities and directions, boundary and empty-body behavior, ordered alternative fallback, unchanged capture numbering, literal escapes, raw NUL/high bytes, unsupported constructs, compile diagnostics, work limits, and cancellation. The PR must pass net8/net9/net10 on all six CI runners, the frozen GNU corpus, package smoke, and the source-pinned consumer probes as applicable. Do not switch Grep or change CommandFramework production code.
