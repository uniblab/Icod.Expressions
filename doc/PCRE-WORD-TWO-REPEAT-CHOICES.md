# Native 16-bit finite choices around two unbounded repeats

The word sequence compiler admits finite variable atom counts between and
after two unbounded atoms in a top-level path. It preserves three ordered
levels: finite choices before the first repeat are outer paths; choices
between repeats run inside each first-repeat count; choices after the
second repeat run inside each second-repeat count. Greedy and lazy counts
retain source order at every level. Exact counts, straight atoms, absolute
assertions and top-level alternatives remain available around the repeats.

The compiler expands at most 64 combinations across all top-level paths,
counting the product of the three finite-choice levels. Each expanded
source path retains the 4,096-node bound and exact counts and unbounded
minima stay capped at 64. Three unbounded repeats, groups, captures and
possessive wrappers remain unsupported. Both unbounded atoms match the
original raw 16-bit units or strict UTF-16 scalars. Group zero retains
original code-unit coordinates; all admissions, comparisons, rewinds and
completions share the cancellation-aware work quota.

## Evidence

Forty-nine source/subject/start fixtures across six compiled defaults and
64 call masks compare 18,816 outcomes with pinned PCRE2-16: 17,816 matches
and 1,000 misses. The matrix covers finite choices between and after the
repeats, overlapping atoms, greedy and lazy combinations, prefix choices,
top-level alternatives, absolute assertions, empty and end-anchored
policies, UCP, caseless classes, raw surrogates and supplementary scalars.
Three valid-Unicode fixtures compare managed UTF-8 byte and UTF-16 word
coordinates. Product-bound and cancellation-aware work tests cover the
new representation. Every native preflight row JIT compiled without error;
their LF-terminated output SHA-256 was:

`32c936785cdbd59bd0042073e4f63d1a4f4f5faa896e8df3d23d065ecfcde377`

The independent native oracle and pinned PCRE2 10.48/SLJIT revisions are
recorded in the [word sequence evidence](PCRE-WORD-SEQUENCES.md). Three or
more unbounded repeats, general nested execution, durable captures and
broader P10–P12 gates remain open.
