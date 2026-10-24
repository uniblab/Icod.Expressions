# PCRE byte-mode numbered backreferences

**Status:** internal syntax slice stacked on [byte classes](https://github.com/uniblab/Icod.Expressions/pull/10), under [issue #7](https://github.com/uniblab/Icod.Expressions/issues/7). The public GNU package, Grep, and CommandFramework production code remain untouched.

## Intent and contract

Add case-sensitive, numbered single-digit backreferences to the 8-bit non-UTF, non-UCP matcher. The existing Grep fixture `^(\w+)\s+\1$` is the first consumer-driven pattern this enables internally. A backreference consumes the exact bytes most recently captured by its group in the current backtracking path. Unset groups fail by default; an empty but set group matches zero bytes. Captures and backreferences retain byte offsets, NUL and high bytes without decoding.

Support `\1` through `\9` outside bracket classes when they designate an existing capture group. Allow a single-digit reference before its group, with validation after the full pattern is parsed; the initial unset reference fails but may work in later iterations. A reference to a group that does not exist is `InvalidSyntax` at the backslash. Multi-digit sequences such as `\10`, octal `\0`, named/relative forms (`\g`, `\k`), caseless matching, and in-class numeric escapes remain explicit `UnsupportedSyntax` until their ambiguity and option scopes are implemented. This slice does not claim general PCRE2 backreference syntax.

The VM instruction reads an existing capture span, compares its bytes to the subject at the current position, and accounts for each attempted byte through the match work limit and cancellation. It does not use recursive .NET calls or a runtime regex library. The existing frame snapshots restore captures after a failed path.

## Approach

| Approach | Tradeoff | Decision |
|---|---|---|
| Replay the captured group's AST | Changes PCRE2 semantics: backreferences match captured text, not the group's pattern. | Reject. |
| Compare the captured byte span with a dedicated instruction | Fits the existing immutable bytecode and capture snapshots; explicit per-byte resource accounting. | Choose. |
| Convert captures to strings | Loses byte fidelity, adds allocation and Unicode/culture behavior. | Reject. |

## Verification and boundaries

Pin the Grep `\1` fixture, alternate-path capture rollback, an optional group that never matched, an empty capture, repeated group captures, NUL/high-byte spans, forward-reference syntax and multi-digit rejection against PCRE2 8-bit default matching. A small work limit must interrupt a long backreference comparison; cancellation remains observable. Run all .NET 8/9/10 and Linux/Windows/macOS CI jobs, frozen GNU 214-case corpus, package-only consumer and source-pinned consumer bridge. Reference PCRE2 is a fixture oracle only.

The current 1,000,000-step default and 1,024-frame cap are provisional, and no Grep switch or CommandFramework removal follows from this internal milestone. [PCRE2 backreference rules](https://www.pcre.org/current/doc/html/pcre2pattern.html) govern later expansion to multi-digit and named forms.
