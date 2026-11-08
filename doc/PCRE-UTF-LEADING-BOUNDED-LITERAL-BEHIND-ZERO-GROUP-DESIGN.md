# Bounded leading literal lookbehind before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged192. The optional-bracket UTF start-bitmap
proof accepts one or two CR-excluding literal lookbehind scalars in its
leading assertion. Pinned PCRE2 10.48 compile/JIT probes retained LF and
excluded CR with positive JIT sizes for 90 patterns having three, four
or eight scalars, across three middle prefixes and five group shapes.
Three native subject matches per pattern had the expected spans.

## Scope and choice

Raise only this leading positive caseful literal lookbehind scan to eight
valid UTF scalars, each excluding CR. The loop remains bounded and keeps
the existing optional bracket, middle prefix checks, mandatory repeated
group head, cancellation and 1,000-visit ceiling. Longer, caseless and
CR-containing lookbehinds remain separate. Generalizing the bitmap walker
would affect unrelated placements; this local extension leaves parser,
matcher, public API, limits and consumers unchanged.

Pin full match and capture spans for 270 native fixtures / 540 strict and
tolerant checks across six leads, three prefixes, five repeated-group
shapes and three subjects. Observe test-only RED first.
