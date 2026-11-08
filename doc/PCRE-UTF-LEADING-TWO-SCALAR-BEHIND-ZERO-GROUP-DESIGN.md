# Positive leading two-scalar lookbehind before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged191. The optional-bracket UTF start-bitmap
proof accepts a positive one-scalar literal lookbehind as the leading
assertion, but rejects a two-scalar literal. Pinned PCRE2 10.48
compile/JIT probes retained LF, excluded CR and had positive JIT sizes
for four two-scalar leads, three middle prefixes and five group shapes.
Three native subjects per pattern had the expected match spans.

## Scope and choice

Extend the leading literal lookbehind proof to at most two valid UTF
scalars, both excluding CR. Decode each scalar once with a bounded scan;
keep the one-scalar behavior, optional bracket, middle prefix checks,
mandatory repeated-group head, cancellation and 1,000-visit ceiling.
Three-scalar and CR-containing literals remain separate. A general
lookbehind bitmap walker would affect unrelated placements; this narrow
extension keeps parser, matcher, public API, limits and consumers unchanged.

Pin full match and capture spans for 180 native fixtures / 360 strict and
tolerant checks across four two-scalar lookbehinds (including wide UTF),
three middle prefixes, five group shapes and three subjects. Observe
test-only RED before modifying production code.
