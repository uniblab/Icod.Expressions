# Positive leading literal lookbehind before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged190. The optional-bracket UTF start-bitmap
proof admits a positive leading literal lookahead and direct class
lookbehind, but rejects a positive leading literal lookbehind. Pinned
PCRE2 10.48 compile/JIT probes over 60 lead/prefix/group patterns retained
LF and excluded CR with positive JIT sizes. Three native subject matches
per pattern had the expected spans, including a wide one-scalar lookbehind.

## Scope and choice

Permit a leading positive caseful literal lookbehind only when it contains
exactly one UTF scalar that excludes CR. Reuse the literal membership proof
and existing optional bracket, middle zero-width scan, mandatory repeated
group head, cancellation and 1,000-visit ceiling. Multi-scalar or
CR-admitting lookbehind literals remain outside this narrow proof. The
general literal-ahead path and middle assertion handling remain unchanged;
parser, matcher, API, limits and consumers are unchanged.

Pin full match and capture spans for 180 native fixtures / 360 strict and
tolerant checks across four leading literals, three middle prefixes, five
repeated-group shapes and three subjects. Observe test-only RED first.
