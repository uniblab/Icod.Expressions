# Internal PCRE capture and repetition options

## Goal and semantics

Extend the existing option reader with `n` and `U`, standalone or scoped,
including combinations with i/m/s and explicit unsetting. Follow the
[pinned PCRE2 10.48 specification](https://github.com/PCRE2Project/pcre2/blob/pcre2-10.48/doc/html/pcre2pattern.html)
and PCRE.NET 1.6.0 test oracle. This advances issue #7 internally.

`n` makes ordinary parentheses noncapturing at their opening location. Named
parentheses still allocate numbered captures. Suppressed groups do not consume
capture numbers or the capture-count budget; they still consume nesting depth.
References, branch-reset numbering and aliases use the resulting ordinary
numbered slots. Changes affect following groups and later alternatives in the
current group, and restore at every group boundary. Turning n on inside a group
does not suppress that already-open group's capture.

`U` reverses the default greediness of all existing quantifiers. A question-mark
suffix reverses that default again. A plus suffix is always greedy and
possessive, independently of U. The setting at the quantifier, after any group
body has restored its caller's options, controls repetition. It does not alter
alternative order. Atomic groups preserve their usual pruning semantics.

## Approaches

1. Runtime option instructions would require unnecessary frame state and
   rollback for settings whose effects are determined during compilation.
2. A second parser/matcher for these modes would duplicate semantic behavior.
3. **Choose parser state and existing nodes.** Extend the shared reader and
   group save/restore with two booleans. Decide capture allocation at group
   entry (named groups override n). Compute repeat greediness from U and the
   explicit suffix, then lower possessive repeats through the existing atomic
   node. No syntax-node, instruction, matcher or journal changes are needed.

Retain the current explicit per-flag reader rather than introduce a general
options abstraction for two additional switches. Keep repeated modifiers and
unsupported letters under existing diagnostics. No caller-side n/U properties
are introduced: this tranche covers pattern settings, as the existing m/s do.

## Boundaries and verification

Empty/reset options, x/xx/J and extra flags, scoped/mixed aD, general assertions,
Unicode data-version parity, public API, release and consumer migrations remain
separate gates. Do not edit Grep or CommandFramework production. Keep UTF-8+LF,
C# 13, net8/net9/net10, existing resource limits, work/cancellation, byte spans
and invalid-UTF barriers. No subagents or runtime regex/native fallback.

Pin capture counts and every capture span, named forms, reference/octal
numbering, branch resets and scope propagation. Check each quantifier family
with ordinary, question-mark and plus suffixes, including nested options,
atomic fallback, empty paths and UTF byte spans. Cover quoted syntax, every
selected invalid-UTF/CRLF offset, 256-level nesting and capture limits,
work exhaustion and cancellation. Inspect test-first CI failures, then require
six green Staging jobs and existing frozen/package/workload gates on the final
head before marking the PR ready. No local .NET SDK is installed.
