# CR-containing leading literal lookbehind before nullable group repeats

## Intent and native evidence

Continue issue #102 after merged #193. The bounded UTF start-bitmap proof
currently rejects CR in the leading positive literal lookbehind. Pinned
PCRE2 10.48 compile/JIT probes retained LF and excluded CR for all 135
patterns across nine leads, three middle prefixes and five group shapes,
with positive JIT sizes. All three native subject matches per pattern
matched at the expected offsets. Six leads contain CR; three LF-only leads
are controls.

## Scope and choice

Allow valid CR scalars in the existing maximum-eight-scalar, caseful
positive literal lookbehind scan. Keep the optional bracket, middle prefix
checks, mandatory repeated-group head, cancellation and 1,000-visit
ceiling. Other CR-consuming heads, negative and caseless assertions, and
longer lookbehinds remain outside this proof. Parser, matcher, public API,
limits and consumers remain unchanged.

Pin full match and capture spans for 405 native fixtures / 810 strict and
tolerant checks across the nine leads, three prefixes, five repeated-group
shapes and three subjects. Observe test-only RED before changing code.
